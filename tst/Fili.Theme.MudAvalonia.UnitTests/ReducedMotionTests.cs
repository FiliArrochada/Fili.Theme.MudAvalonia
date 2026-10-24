using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
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

        var waves = Assert.IsType<Canvas>(Matrix.Part<Border>(button, "PART_Ripple").Child);
        Assert.Empty(waves.Children);
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
