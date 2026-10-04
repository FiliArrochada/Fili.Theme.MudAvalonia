using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery.Views;
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
    private static (ControlStatesView View, ScrollViewer Page) Open()
    {
        var view = new ControlStatesView();
        var window = new Window { Content = view, Width = 1180, Height = 800 };
        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        return (view, view.GetVisualDescendants().OfType<ScrollViewer>().First());
    }

    /// <summary>
    /// The controls page opened scrolled almost to the bottom: every sample with a selection asks
    /// to bring it into view, the ones with no scroll area of their own pass that up to the page,
    /// and the page obeyed the last - a tab strip near the end.
    /// </summary>
    [Fact]
    public Task TheControlsPageOpensAtTheTop() => UiThread.RunAsync(() =>
    {
        var (_, page) = Open();

        Assert.Equal(0, page.Offset.Y);
        Assert.True(page.Extent.Height > page.Viewport.Height, "The page should be long enough to scroll.");
    });

    /// <summary>Keyboard focus still scrolls the page to what it lands on.</summary>
    [Fact]
    public Task KeyboardFocusStillScrollsThePage() => UiThread.RunAsync(() =>
    {
        var (view, page) = Open();
        var last = view.GetVisualDescendants().OfType<TextBox>().Last();

        last.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();

        Assert.True(page.Offset.Y > 0, "Tabbing to a field at the bottom did not scroll the page.");
    });
}
