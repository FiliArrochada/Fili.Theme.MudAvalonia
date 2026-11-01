using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The scroll bar is MudBlazor's bare 8px rail, but keeps what Avalonia's scroll bar does: a
/// click on the trough pages, deferred scrolling waits for the release, and the context menu is
/// there. Each was lost silently when the template dropped the part behind it.
/// </summary>
public class ScrollBarBehaviourTests
{
    private static (Window Window, ScrollViewer Viewer, ScrollBar Bar) Show(bool deferred = false)
    {
        var viewer = new ScrollViewer
        {
            Height = 300,
            Width = 200,
            IsDeferredScrollingEnabled = deferred,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            Content = new Border { Height = 3000 },
        };
        var window = new Window { Width = 400, Height = 400, Content = viewer, VerticalContentAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Left };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var bar = viewer.GetVisualDescendants().OfType<ScrollBar>().Single(b => b.Orientation == Orientation.Vertical);
        return (window, viewer, bar);
    }

    private static Point At(ScrollBar bar, Window window, double fromBottom) =>
        bar.TranslatePoint(new Point(bar.Bounds.Width / 2, bar.Bounds.Height - fromBottom), window)!.Value;

    [Fact]
    public Task AClickOnTheTroughPages() => UiThread.RunAsync(() =>
    {
        var (window, viewer, bar) = Show();
        var point = At(bar, window, 20);

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(viewer.Viewport.Height, viewer.Offset.Y, 1);
        window.Close();
    });

    [Fact]
    public Task DeferredScrollingWaitsForTheRelease() => UiThread.RunAsync(() =>
    {
        var (window, viewer, bar) = Show(deferred: true);
        var thumb = Matrix.Part<Thumb>(bar, "PART_Thumb");
        var start = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;
        var end = start + new Point(0, 40);

        window.MouseDown(start, MouseButton.Left);
        window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, viewer.Offset.Y);

        window.MouseUp(end, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.True(viewer.Offset.Y > 0, "The deferred drag did not apply on release.");
        window.Close();
    });

    [Fact]
    public Task WithoutDeferringTheContentFollowsTheDrag() => UiThread.RunAsync(() =>
    {
        var (window, viewer, bar) = Show();
        var thumb = Matrix.Part<Thumb>(bar, "PART_Thumb");
        var start = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;
        var end = start + new Point(0, 40);

        window.MouseDown(start, MouseButton.Left);
        window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.True(viewer.Offset.Y > 0);

        window.MouseUp(end, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.Close();
    });

    /// <summary>The page buttons draw nothing: the rail is still MudBlazor's transparent track.</summary>
    [Fact]
    public Task ThePageButtonsAreInvisibleAndOutOfTheTabOrder() => UiThread.RunAsync(() =>
    {
        var (window, _, bar) = Show();

        foreach (var name in new[] { "PART_PageUpButton", "PART_PageDownButton" })
        {
            var button = Matrix.Part<RepeatButton>(bar, name);
            Assert.False(button.Focusable);
            Assert.Equal(Avalonia.Media.Colors.Transparent, Matrix.Colour(button.Background));
        }

        window.Close();
    });

    [Fact]
    public Task BothOrientationsHaveTheContextMenu() => UiThread.RunAsync(() =>
    {
        var (window, viewer, bar) = Show();
        viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        Dispatcher.UIThread.RunJobs();
        var horizontal = viewer.GetVisualDescendants().OfType<ScrollBar>().Single(b => b.Orientation == Orientation.Horizontal);

        Assert.IsType<MenuFlyout>(bar.ContextFlyout);
        Assert.IsType<MenuFlyout>(horizontal.ContextFlyout);
        Assert.NotSame(bar.ContextFlyout, horizontal.ContextFlyout);
        window.Close();
    });
}
