using Shared.Input;

namespace ActivityChecker.IO;

public class PathUserInput(bool request = true) : BaseUserInput(request)
{
    [InputParameter("Путь к файлу с ссылками:")]
    public string Path { get; set; } = null!;
}