using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery;
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
        // Shorter than the page's first screen of samples, as a browser tab is: the focused "Tabbed
        // to" sample then sits below the fold, which is what made the page open scrolled.
        var window = new Window { Content = view, Width = 1180, Height = 560 };
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

    /// <summary>
    /// Every section heading has a jump link at the top, and the link brings its heading to the
    /// top of the page - by setting the offset itself, since the page ignores scroll requests
    /// that do not come from keyboard focus.
    /// </summary>
    [Fact]
    public Task AJumpLinkScrollsToItsSection() => UiThread.RunAsync(() =>
    {
        var (view, page) = Open();
        var headings = view.GetVisualDescendants().OfType<TextBlock>().Where(t => Equals(t.Tag, "section")).ToList();
        var links = view.GetVisualDescendants().OfType<WrapPanel>().Single(p => p.Name == "SectionIndex").Children.OfType<Button>().ToList();

        Assert.Equal(15, headings.Count);
        Assert.Equal(headings.Select(h => h.Text), links.Select(l => l.Content as string));

        var sliders = links.Single(l => (string?)l.Content == "Sliders and progress");
        sliders.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var heading = headings.Single(h => h.Text == "Sliders and progress");
        var top = heading.TranslatePoint(default, page)!.Value.Y;
        Assert.InRange(top, 0, 48);
    });

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
