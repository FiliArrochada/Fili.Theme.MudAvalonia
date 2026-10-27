using System.Runtime.InteropServices.JavaScript;
using Fili.Theme.MudAvalonia.Gallery.Docs;

namespace Fili.Theme.MudAvalonia.Gallery.Browser;

/// <summary>
/// The page's address in the browser: the slug after the URL's <c>#</c>, so
/// <c>…/Fili.Theme.MudAvalonia/#tooltip</c> opens the Tooltip page. GitHub Pages serves one
/// file, and a fragment never reaches the server, so this needs no routing there. The functions
/// are main.js's, registered under the module name "main.js" before the app starts.
/// </summary>
internal sealed partial class BrowserDocAddress : IDocAddress
{
    public BrowserDocAddress() => Listen(slug => Changed?.Invoke(slug));

    public string? Slug => Read();

    public event Action<string>? Changed;

    public void Show(string slug, string title) => Write(slug, title);

    [JSImport("docAddress.read", "main.js")]
    private static partial string Read();

    [JSImport("docAddress.write", "main.js")]
    private static partial void Write(string slug, string title);

    [JSImport("docAddress.listen", "main.js")]
    private static partial void Listen([JSMarshalAs<JSType.Function<JSType.String>>] Action<string> changed);
}
