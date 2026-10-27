namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>
/// An address outside the app that names the page on show: in a browser, the part of the URL after
/// the <c>#</c>, so a page can be linked to, reloaded and gone back from. The head that has one sets
/// <see cref="DocAddress.Current"/>; the desktop has none, and the site opens on the overview.
/// </summary>
public interface IDocAddress
{
    /// <summary>The slug the address names, or null or empty when it names none.</summary>
    string? Slug { get; }

    /// <summary>Writes the page on show into the address, with its title for the window or tab.</summary>
    void Show(string slug, string title);

    /// <summary>Raised when the address changes from outside: Back, Forward, or a link followed.</summary>
    event Action<string>? Changed;
}

/// <summary>Where the head puts its <see cref="IDocAddress"/>, before the first view is built.</summary>
public static class DocAddress
{
    public static IDocAddress? Current { get; set; }
}
