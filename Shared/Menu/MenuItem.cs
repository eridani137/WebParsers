namespace Shared.Menu;

public record BaseMenuItem(string Title);

public record MenuItem(string Title, Func<Task> Task) : BaseMenuItem(Title);

public record SubMenuItem(string Title, MenuItem[] SubMenuItems) : BaseMenuItem(Title);