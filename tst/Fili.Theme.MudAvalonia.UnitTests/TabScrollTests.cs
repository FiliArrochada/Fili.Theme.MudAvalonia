using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudTabs' scroll buttons: shown only while the tabs overflow the bar, each disabled at its
/// end, a bar's length per click, and the selected tab scrolled into view.
/// </summary>
public class TabScrollTests
{
    private static TabControl Tabs(int count, double width, Dock placement = Dock.Top)
    {
        var tabs = new TabControl { Width = width, Height = 300, TabStripPlacement = placement };
        for (var i = 0; i < count; i++)
        {
            tabs.Items.Add(new TabItem { Header = $"Tab {i}" });
        }

        Matrix.Show(tabs);
        Dispatcher.UIThread.RunJobs();
        return tabs;
    }

    private static Button Prev(Control tabs) => Matrix.Part<Button>(tabs, "PART_ScrollPrev");

    private static Button Next(Control tabs) => Matrix.Part<Button>(tabs, "PART_ScrollNext");

    private static ScrollViewer Scroller(Control tabs) => Matrix.Part<ScrollViewer>(tabs, "PART_TabScroller");

    private static void Click(Button button)
    {
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public Task TabsThatFitHaveNoButtons() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(2, 500);

        Assert.False(Prev(tabs).IsVisible);
        Assert.False(Next(tabs).IsVisible);
    });

    [Fact]
    public Task OverflowingTabsScrollABarAtATime() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(8, 500);
        var scroller = Scroller(tabs);

        Assert.True(Prev(tabs).IsVisible);
        Assert.True(Next(tabs).IsVisible);
        Assert.False(Prev(tabs).IsEnabled);
        Assert.True(Next(tabs).IsEnabled);

        Click(Next(tabs));
        Assert.Equal(scroller.Viewport.Width, scroller.Offset.X, 1);
        Assert.True(Prev(tabs).IsEnabled);

        while (Next(tabs).IsEnabled)
        {
            Click(Next(tabs));
        }

        Assert.Equal(scroller.Extent.Width - scroller.Viewport.Width, scroller.Offset.X, 1);
    });

    [Fact]
    public Task TheSelectedTabIsScrolledIntoView() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(8, 500);

        tabs.SelectedIndex = 7;
        Dispatcher.UIThread.RunJobs();

        Assert.True(Scroller(tabs).Offset.X > 0);
        Assert.False(Next(tabs).IsEnabled);
    });

    /// <summary>A bar on the side scrolls down, its buttons the bar's width and 32px high.</summary>
    [Fact]
    public Task ASideBarScrollsDown() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(12, 500, Dock.Left);
        var scroller = Scroller(tabs);

        Assert.Equal(ScrollBarVisibility.Disabled, scroller.HorizontalScrollBarVisibility);
        Assert.True(Next(tabs).IsVisible);
        Assert.Equal(32, Next(tabs).Bounds.Height);
        Assert.Equal(Dock.Bottom, DockPanel.GetDock(Next(tabs)));

        Click(Next(tabs));
        Assert.True(scroller.Offset.Y > 0);
    });

    /// <summary>
    /// <c>centered</c> centres the tab row inside the scroller: a row that fits sits in the middle,
    /// and one that overflows starts at the bar's start, so the first tab is not centred out of
    /// reach and every tab can be scrolled to.
    /// </summary>
    [Fact]
    public Task CenteredTabsThatFitSitInTheMiddle() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(2, 500);
        tabs.Classes.Add("centered");
        Dispatcher.UIThread.RunJobs();

        var presenter = Matrix.Part<ItemsPresenter>(tabs, "PART_ItemsPresenter");
        var scroller = Scroller(tabs);
        var left = presenter.TranslatePoint(default, scroller)!.Value.X;
        var right = scroller.Viewport.Width - left - presenter.Bounds.Width;

        Assert.True(left > 0);
        Assert.Equal(left, right, 1);
    });

    [Fact]
    public Task CenteredTabsThatOverflowStillReachBothEnds() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(8, 500);
        tabs.Classes.Add("centered");
        Dispatcher.UIThread.RunJobs();

        var presenter = Matrix.Part<ItemsPresenter>(tabs, "PART_ItemsPresenter");
        var scroller = Scroller(tabs);

        Assert.Equal(0, presenter.TranslatePoint(default, scroller)!.Value.X, 1);
        Assert.Equal(presenter.Bounds.Width, scroller.Extent.Width, 1);

        while (Next(tabs).IsEnabled)
        {
            Click(Next(tabs));
        }

        var end = presenter.TranslatePoint(new Point(presenter.Bounds.Width, 0), scroller)!.Value.X;
        Assert.Equal(scroller.Viewport.Width, end, 1);
    });

    [Fact]
    public Task ATabStripScrollsToo() => UiThread.RunAsync(() =>
    {
        var strip = new TabStrip { Width = 400 };
        for (var i = 0; i < 8; i++)
        {
            strip.Items.Add(new TabStripItem { Content = $"Tab {i}" });
        }

        Matrix.Show(strip);
        Dispatcher.UIThread.RunJobs();

        Assert.True(Next(strip).IsVisible);
        Click(Next(strip));
        Assert.True(Scroller(strip).Offset.X > 0);
    });
}
