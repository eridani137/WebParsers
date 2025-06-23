using Shared.Input;

namespace ActivityChecker;

public class PathUserInput(bool request = true) : BaseUserInput(request)
{
    [InputParameter("Путь к файлу:")]
    public string Path { get; set; } = null!;
}