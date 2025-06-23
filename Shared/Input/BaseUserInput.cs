using System.Reflection;
using Spectre.Console;

namespace Shared.Input;

public class BaseUserInput
{
    protected BaseUserInput(bool request)
    {
        if (request) RequestParameters();
    }

    private void RequestParameters()
    {
        var type = GetType();
        var properties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<InputParameterAttribute>();
            if (attr is null || !prop.CanWrite) continue;
            
            var method = typeof(BaseUserInput).GetMethod(nameof(EnterParameter), 
                BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethod = method?.MakeGenericMethod(prop.PropertyType);
            var value = genericMethod?.Invoke(null, [attr.Description, IsNullable(prop), attr.IsSecret]);
            
            prop.SetValue(this, value);
        }
    }
    
    private static bool IsNullable(PropertyInfo property)
    {
        if (!property.PropertyType.IsValueType) return true;
        return Nullable.GetUnderlyingType(property.PropertyType) != null;
    }

    protected static T EnterParameter<T>(string description, bool allowNull = false, bool secret = false)
    {
        var prompt = new TextPrompt<T>(description.MarkupSecondary())
            .PromptStyle(SpectreConfig.Style)
            .ValidationErrorMessage("Неверное значение".MarkupError());
        
        if (allowNull)prompt.AllowEmpty();
        if (secret) prompt.Secret();
        
        return AnsiConsole.Prompt(prompt);
    }
}