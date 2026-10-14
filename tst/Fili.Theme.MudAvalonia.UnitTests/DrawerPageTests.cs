using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// DrawerPage's pane toggles are MudLayout's drawer button, a MudIconButton, not a MudToggleItem:
/// its forked template sets Background="Transparent" on them, which would beat a checked toggle's
/// fill and leave a contrast-coloured icon on nothing. And the fix stays on DrawerPage: a toggle
/// an app gives its own Background keeps it, checked or not.
/// </summary>
public class DrawerPageTests
{
    private static DrawerPage Page(bool open)
    {
        var page = new DrawerPage
        {
            Width = 600,
            Height = 300,
            IsOpen = open,
            Drawer = new TextBlock { Text = "Menu" },
            Content = new ContentPage { Content = new TextBlock { Text = "Content" } },
        };

        return Matrix.Show(page);
    }

    private static object Resource(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, out var value), $"{key} did not resolve.");
        return value!;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ThePaneButtonIsAnIconButtonOpenOrClosed(bool open) => UiThread.RunAsync(() =>
    {
        var button = Matrix.Part<ToggleButton>(Page(open), "PART_PaneButton");

        Assert.Equal(open, button.IsChecked);
        Assert.Same(Resource("FiliToggleIconButton"), button.Theme);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(button.Foreground));
        Assert.Equal(new CornerRadius(999), button.CornerRadius);
        Assert.Equal(new Thickness(12), button.Padding);
        Assert.Equal(0, button.MinWidth);

        // No fill: the template's Transparent shows, and nothing is drawn in a contrast colour.
        var root = Matrix.Part<Border>(button, "PART_Root");
        Assert.Equal(Colors.Transparent, Matrix.Colour(root.Background));
    });

    [Fact]
    public Task ThePaneButtonTintsRoundOnHover() => UiThread.RunAsync(() =>
    {
        // Inline, so the open pane sits beside the top bar rather than over it.
        var page = Page(open: true);
        page.DisplayMode = SplitViewDisplayMode.Inline;
        Dispatcher.UIThread.RunJobs();

        var button = Matrix.Part<ToggleButton>(page, "PART_PaneButton");
        Assert.True(button.IsChecked);
        Matrix.Hover(button);

        var layer = Matrix.Part<Border>(button, "PART_StateLayer");
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(layer.Background));
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Border>(button, "PART_Root").Background));
    });

    [Fact]
    public Task TheBottomAndCompactTogglesAreIconButtonsToo() => UiThread.RunAsync(() =>
    {
        var page = Page(open: true);

        foreach (var name in new[] { "PART_BottomPaneButton", "PART_CompactPaneToggle" })
        {
            var button = Matrix.Part<ToggleButton>(page, name);
            Assert.Same(Resource("FiliToggleIconButton"), button.Theme);
            Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(button.Foreground));
        }
    });

    /// <summary>
    /// A closed compact DrawerPage is a mini drawer, as a compact SplitView is: a nav link's text
    /// hidden beside its icon, no indent, a 56px rail. Open, the text and indent come back.
    /// </summary>
    [Fact]
    public Task AClosedCompactDrawerPageIsAMiniDrawer() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Inbox" };
        var nested = new TreeViewItem { Header = new StackPanel { Children = { new PathIcon { Classes = { "start-icon" } }, new TextBlock { Text = "Starred" } } } };
        var link = new TreeViewItem { Header = new StackPanel { Children = { new PathIcon { Classes = { "start-icon" } }, text } }, IsExpanded = true, Items = { nested } };
        var page = Matrix.Show(new DrawerPage
        {
            Width = 600,
            Height = 300,
            IsOpen = false,
            DrawerLayoutBehavior = DrawerLayoutBehavior.CompactInline,
            DrawerBreakpointLength = 0,
            Drawer = new TreeView { Classes = { "nav" }, Items = { link } },
            Content = new ContentPage { Content = new TextBlock { Text = "Content" } },
        });

        Assert.Equal(SplitViewDisplayMode.CompactInline, page.DisplayMode);
        Assert.False(text.IsVisible);
        Assert.Equal(0, Matrix.Part<Border>(nested, "PART_Indent").Width);
        Assert.Equal(56, page.CompactDrawerLength);
        Assert.Equal(56, Matrix.Part<SplitView>(page, "PART_SplitView").CompactPaneLength);

        // The icon's 12px goes with the text, so a row fits the rail and nothing scrolls sideways.
        var scroller = ((TreeView)page.Drawer!).GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.True(scroller.Extent.Width <= scroller.Viewport.Width, $"The rail scrolls: {scroller.Extent.Width} in {scroller.Viewport.Width}.");

        page.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(text.IsVisible);
        Assert.Equal(20, Matrix.Part<Border>(nested, "PART_Indent").Width);
    });

    /// <summary>The 56px rail is a style's: a CompactDrawerLength the app sets wins.</summary>
    [Fact]
    public Task AnAppsOwnRailWidthWins() => UiThread.RunAsync(() =>
    {
        var page = Matrix.Show(new DrawerPage
        {
            Width = 600,
            Height = 300,
            CompactDrawerLength = 72,
            DrawerLayoutBehavior = DrawerLayoutBehavior.CompactInline,
            DrawerBreakpointLength = 0,
            Content = new ContentPage(),
        });

        Assert.Equal(72, page.CompactDrawerLength);
    });

    /// <summary>The fix is DrawerPage's alone: a plain checked toggle is still MudToggleItem, filled.</summary>
    [Fact]
    public Task APlainCheckedToggleIsStillFilled() => UiThread.RunAsync(() =>
    {
        var toggle = Matrix.Show(new ToggleButton { Content = "Bold", IsChecked = true });

        Assert.NotSame(Resource("FiliToggleIconButton"), toggle.Theme);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Matrix.Part<Border>(toggle, "PART_Root").Background));
    });

    /// <summary>Avalonia's precedence holds: a Background the app sets beats the checked fill.</summary>
    [Fact]
    public Task AnAppsOwnBackgroundBeatsTheCheckedFill() => UiThread.RunAsync(() =>
    {
        var own = new SolidColorBrush(Color.Parse("#FF00897B"));
        var toggle = Matrix.Show(new ToggleButton { Content = "Bold", Background = own });

        Assert.Same(own, Matrix.Part<Border>(toggle, "PART_Root").Background);

        toggle.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Same(own, toggle.Background);
        Assert.Same(own, Matrix.Part<Border>(toggle, "PART_Root").Background);
    });
}
