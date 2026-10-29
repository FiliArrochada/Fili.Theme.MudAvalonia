using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// FiliReducedMotion: True in an app's resources, and nothing the theme draws moves or fades on
/// its own. Set here on one window, so it reaches only that window and the popups it opens.
/// </summary>
public class ReducedMotionTests
{
    private static T Calm<T>(T control) where T : Control
    {
        Matrix.Show(control);
        TopLevel.GetTopLevel(control)!.Resources["FiliReducedMotion"] = true;
        Dispatcher.UIThread.RunJobs();
        return control;
    }

    /// <summary>A popup is at full opacity from its first frame: no fade.</summary>
    [Fact]
    public Task APopupDoesNotFade() => UiThread.RunAsync(() =>
    {
        var row = new MenuItem { Header = "Open" };
        var file = new MenuItem { Header = "File", Items = { row } };
        Calm(new Menu { Items = { file } });

        file.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        var host = row.GetVisualAncestors().OfType<TemplatedControl>().Last(a => a is PopupRoot or OverlayPopupHost);
        var presenter = Matrix.Part<ContentPresenter>(host, "PART_ContentPresenter");

        for (var frame = 0; frame < 5; frame++)
        {
            Thread.Sleep(10);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, presenter.Opacity);
        }
    });

    /// <summary>A press draws no ripple.</summary>
    [Fact]
    public Task APressDoesNotRipple() => UiThread.RunAsync(() =>
    {
        var button = Calm(new Button { Content = "Save" });
        var window = (Window)TopLevel.GetTopLevel(button)!;
        var centre = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;

        window.MouseMove(centre, RawInputModifiers.None);
        window.MouseDown(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        // The layer makes its canvas on its first ripple, so none is made at all.
        Assert.Null(Matrix.Part<Border>(button, "PART_Ripple").Child);
        window.MouseUp(centre, MouseButton.Left);
    });

    /// <summary>An expansion panel is open the moment it is told to be, with no grow to watch.</summary>
    [Fact]
    public Task APanelOpensAtOnce() => UiThread.RunAsync(() =>
    {
        var expander = Calm(new Expander { Header = "Panel", Content = "Content", Width = 320 });
        var collapse = Matrix.Part<Decorator>(expander, "PART_Collapse");
        var openness = AvaloniaPropertyRegistry.Instance.FindRegistered(collapse, "Openness")!;

        expander.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1d, collapse.GetValue(openness));
    });

    /// <summary>
    /// A striped bar's bands stand still, still drawn: the bar's length shows the work going on.
    /// An indeterminate bar keeps moving, as its movement is the only sign of it. Read from the
    /// styles, as a key-frame animation ticked headless stalls wherever it happens to be.
    /// </summary>
    [Fact]
    public Task StripesStandStill() => UiThread.RunAsync(() =>
    {
        var bar = Calm(new ProgressBar { Width = 200, Value = 60, Classes = { "large", "striped" } });
        Assert.True(bar.GetVisualDescendants().OfType<Panel>().Single(p => p.Name == "PART_Stripes").IsVisible);

        var theme = Assert.IsType<ControlTheme>(Application.Current!.FindResource(typeof(ProgressBar)));
        var animated = Styles(Chain(theme).SelectMany(t => t.Children)).Where(s => s.Animations.Count > 0).Select(s => s.Selector!.ToString()).ToList();
        Assert.Contains(animated, s => s.Contains("PART_Stripes"));
        Assert.All(animated.Where(s => s.Contains("PART_Stripes")), s => Assert.Contains("Motion.Reduced", s));
        Assert.All(animated.Where(s => s.Contains("IndeterminateIndicator")), s => Assert.DoesNotContain("Reduced", s));

        // The type's theme is an alias, BasedOn the one that holds the styles.
        static IEnumerable<ControlTheme> Chain(ControlTheme? t) => t is null ? [] : Chain(t.BasedOn).Prepend(t);

        static IEnumerable<Style> Styles(IEnumerable<IStyle> styles) =>
            styles.OfType<Style>().SelectMany(s => new[] { s }.Concat(Styles(s.Children)));
    });

    /// <summary>Without the token the theme moves as before: a press ripples.</summary>
    [Fact]
    public Task MotionIsOnByDefault() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save" });
        var window = (Window)TopLevel.GetTopLevel(button)!;
        var centre = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;

        window.MouseMove(centre, RawInputModifiers.None);
        window.MouseDown(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        var waves = Assert.IsType<Canvas>(Matrix.Part<Border>(button, "PART_Ripple").Child);
        Assert.NotEmpty(waves.Children);
        window.MouseUp(centre, MouseButton.Left);
    });
}
