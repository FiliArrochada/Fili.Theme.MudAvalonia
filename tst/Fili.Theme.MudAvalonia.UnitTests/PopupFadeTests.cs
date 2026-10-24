using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A popup fades in over 251ms, as a MudPopover does: _popover.scss takes an open popover from
/// opacity 0 to 1 over <c>PopoverOptions.Duration</c>, and a tooltip's is the same. The fade is
/// on the popup's host, which Avalonia builds afresh on each open, so it runs every time - not
/// only the first.
/// </summary>
public class PopupFadeTests
{
    /// <summary>The presenter inside the host that roots <paramref name="inside"/>'s tree.</summary>
    private static ContentPresenter Host(Visual inside)
    {
        var root = inside.GetVisualAncestors().OfType<TemplatedControl>().Last(a => a is PopupRoot or OverlayPopupHost);
        return Matrix.Part<ContentPresenter>(root, "PART_ContentPresenter");
    }

    /// <summary>
    /// The host's presenter is seen part way in, sampled a frame at a time until it is, for up to a
    /// second. Only that the fade runs: the headless render timer ticks on its own thread and stalls
    /// an animation sampled this way before its end, so where it ends is
    /// <see cref="TheFadeIsMudBlazorsAndEnds"/>'s. Not a fixed window: on a loaded CI runner the
    /// fade can start later than the first 120ms, and every sample in that window read 0.
    /// </summary>
    private static void AssertFadesIn(ContentPresenter presenter, string what)
    {
        var seen = new List<double> { presenter.Opacity };
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (elapsed.ElapsedMilliseconds < 1000 && !seen.Any(o => o is > 0 and < 0.9))
        {
            Thread.Sleep(10);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            seen.Add(presenter.Opacity);
        }

        Assert.True(seen.Any(o => o is > 0 and < 0.9), $"{what} never showed part way in: {string.Join(", ", seen.Distinct())}.");
    }

    /// <summary>
    /// The fade itself: 0 to 1 over PopoverOptions.Duration, 251ms after no delay, at CSS's default
    /// ease, ending on the presenter's own opacity of 1.
    /// </summary>
    [Fact]
    public Task TheFadeIsMudBlazorsAndEnds() => UiThread.RunAsync(() =>
    {
        var fade = Assert.Single(AllStyles(Application.Current!.Styles)
            .Where(s => s.Selector?.ToString().Contains("PopupRoot") == true)
            .SelectMany(s => s.Animations)
            .OfType<Animation>());

        Assert.Equal(TimeSpan.FromMilliseconds(251), fade.Duration);
        Assert.Equal(TimeSpan.Zero, fade.Delay);
        var ease = Assert.IsType<SplineEasing>(fade.Easing);
        Assert.Equal((0.25, 0.1, 0.25, 1.0), (ease.X1, ease.Y1, ease.X2, ease.Y2));

        var frames = fade.Children.Select(k => (k.Cue.CueValue, ((Setter)k.Setters.Single()).Value)).ToList();
        Assert.Equal([(0d, (object?)0d), (1d, 1d)], frames);

        Assert.Contains("OverlayPopupHost", AllStyles(Application.Current!.Styles).Single(s => s.Animations.Contains(fade)).Selector!.ToString());

        static IEnumerable<Style> AllStyles(IEnumerable<IStyle> styles) =>
            styles.SelectMany(s => s switch
            {
                Style style => new[] { style }.Concat(AllStyles(style.Children)),
                Styles group => AllStyles(group),
                StyleInclude include when include.Loaded is { } loaded => AllStyles([loaded]),
                _ => [],
            });
    });

    [Fact]
    public Task ASelectsListFadesInEachTimeItOpens() => UiThread.RunAsync(() =>
    {
        var combo = Matrix.Show(new ComboBox { ItemsSource = new[] { "One", "Two" }, Width = 200 });

        for (var open = 1; open <= 2; open++)
        {
            combo.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            var sheet = Assert.IsAssignableFrom<Visual>(Matrix.Part<Popup>(combo, "PART_Popup").Child);
            AssertFadesIn(Host(sheet), $"Open {open}");

            combo.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();
        }
    });

    [Fact]
    public Task ASubmenuFadesIn() => UiThread.RunAsync(() =>
    {
        var row = new MenuItem { Header = "Open" };
        var file = new MenuItem { Header = "File", Items = { row } };
        Matrix.Show(new Menu { Items = { file } });

        file.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        AssertFadesIn(Host(row), "A submenu");
    });

    [Fact]
    public Task AFlyoutFadesIn() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Flyout" };
        var flyout = new Flyout { Content = text };
        var target = Matrix.Show(new Button { Content = "Flyout", Flyout = flyout });

        flyout.ShowAt(target);
        Dispatcher.UIThread.RunJobs();
        AssertFadesIn(Host(text), "A flyout");
    });

    [Fact]
    public Task ATooltipFadesIn() => UiThread.RunAsync(() =>
    {
        var target = Matrix.Show(new Button { Content = "Save" });
        var text = new TextBlock { Text = "Saves the file" };
        ToolTip.SetTip(target, text);
        ToolTip.SetIsOpen(target, true);
        Dispatcher.UIThread.RunJobs();

        AssertFadesIn(Host(text), "A tooltip");
    });
}
