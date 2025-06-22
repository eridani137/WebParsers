namespace Shared;

public record MenuItem(string Title, Func<Task> Task);