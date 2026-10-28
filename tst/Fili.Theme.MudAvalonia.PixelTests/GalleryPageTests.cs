using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// How the gallery's pages behave, as opposed to how they look.
/// <para>
/// Here rather than in the unit-test project because the gallery is referenced only from this one.
/// </para>
/// </summary>
public class GalleryPageTests
{
    /// <summary>
    /// The app bar names the theme version it is showing, read from the theme's own assembly:
    /// "v0.4.0", and the commit after it whenever the SDK stamped one, as every clone does.
    /// </summary>
    [Fact]
    public Task TheAppBarShowsTheThemeVersion() => UiThread.RunAsync(() =>
    {
        var view = new MainView();
        new Window { Content = view, Width = 1180, Height = 400 }.Show();
        Dispatcher.UIThread.RunJobs();

        var text = view.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Name == "ThemeVersion").Text;
        var expected = typeof(FiliThemeVariants).Assembly.GetName().Version!.ToString(3);

        Assert.Matches(@"^v\d+\.\d+\.\d+( · [0-9a-f]{7})?$", text);
        Assert.StartsWith($"v{expected}", text);
    });
}
