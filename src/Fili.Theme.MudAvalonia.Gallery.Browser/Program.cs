using Avalonia;
using Avalonia.Browser;
using Fili.Theme.MudAvalonia.Gallery.Docs;

namespace Fili.Theme.MudAvalonia.Gallery.Browser;

internal static class Program
{
    // "out" is the id of the element in wwwroot/index.html that the app renders into. The page on
    // show lives in the URL's fragment, set up before the first view reads it.
    private static Task Main(string[] args)
    {
        DocAddress.Current = new BrowserDocAddress();
        return BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .WithGalleryFonts();
}
