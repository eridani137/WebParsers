namespace Shared.Menu;

public record MenuItem(string Title, Func<Task> Task);