using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Rendering;
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

    /// <summary>
    /// The performance toggle is off until pressed: then Avalonia's FPS, layout and render graphs
    /// draw over the window, and the app bar reads the page's visual count and build time,
    /// following the page shown. Pressed again, both go.
    /// </summary>
    [Fact]
    public Task ThePerformanceToggleShowsTheOverlaysAndThePagesCost() => UiThread.RunAsync(() =>
    {
        var view = new MainView();
        var window = new Window { Content = view, Width = 1440, Height = 900 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var toggle = view.GetVisualDescendants().OfType<ToggleButton>().Single(t => t.Name == "Performance");
        var readout = view.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Name == "PerformanceReadout");
        const string reading = @"^[\d,]+ visuals · built in \d+ ms$";

        Assert.Equal(RendererDebugOverlays.None, window.RendererDiagnostics.DebugOverlays);
        Assert.False(readout.IsVisible);

        toggle.IsChecked = true;
        Assert.Equal(RendererDebugOverlays.Fps | RendererDebugOverlays.LayoutTimeGraph | RendererDebugOverlays.RenderTimeGraph, window.RendererDiagnostics.DebugOverlays);
        Assert.True(readout.IsVisible);
        Assert.Matches(reading, readout.Text);
        var overview = readout.Text;

        view.Navigate("badge");
        Dispatcher.UIThread.RunJobs();
        Assert.Matches(reading, readout.Text);
        Assert.NotEqual(overview, readout.Text);

        toggle.IsChecked = false;
        Assert.Equal(RendererDebugOverlays.None, window.RendererDiagnostics.DebugOverlays);
        Assert.False(readout.IsVisible);
        window.Close();
    });
}
