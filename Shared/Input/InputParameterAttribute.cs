namespace Shared.Input;

[AttributeUsage(AttributeTargets.Property)]
public class InputParameterAttribute(string description, bool isSecret = false) : Attribute
{
    public string Description { get; } = description;
    public bool IsSecret { get; } = isSecret;
}