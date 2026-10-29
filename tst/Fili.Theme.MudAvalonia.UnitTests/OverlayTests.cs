using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudOverlay as <c>overlay</c> on a Border, from _overlay.scss and MudOverlay.razor.cs: the
/// scrim's colours, the content centred, the container's corners, and the .15s fade in.
/// </summary>
public class OverlayTests
{
    private static (Border Container, Border Overlay, Control Content) Covering(params string[] classes)
    {
        var content = new TextBlock { Text = "Loading" };
        var overlay = new Border { Child = content };
        overlay.Classes.Add("overlay");
        overlay.Classes.AddRange(classes);
        var container = new Border
        {
            Classes = { "paper" },
            Width = 300,
            Height = 200,
            Child = new Panel { Children = { new TextBlock { Text = "Behind" }, overlay } },
        };
        Matrix.Show(container);
        return (container, overlay, content);
    }

    [Fact]
    public Task WithNoBackgroundItIsTransparentAndTakesThePointer() => UiThread.RunAsync(() =>
    {
        var (container, overlay, _) = Covering();

        Assert.Equal(Colors.Transparent, Matrix.Colour(overlay.Background));
        Assert.True(overlay.IsHitTestVisible);
        Assert.Equal(container.Bounds.Size, overlay.Bounds.Size);
    });

    [Theory]
    [InlineData("dark-background", "FiliOverlayDarkColor")]
    [InlineData("light-background", "FiliOverlayLightColor")]
    public Task TheBackgroundsAreThePalettesOverlays(string cls, string token) => UiThread.RunAsync(() =>
    {
        var (_, overlay, _) = Covering(cls);

        Assert.Equal(Matrix.Token(token), Matrix.Colour(overlay.Background));
    });

    /// <summary>`.mud-overlay`'s flex centring, and an app's own alignment winning over it.</summary>
    [Fact]
    public Task TheContentIsCentred() => UiThread.RunAsync(() =>
    {
        var (_, overlay, content) = Covering("dark-background");
        var mid = overlay.Bounds.Center;
        var at = content.TranslatePoint(new Point(content.Bounds.Width / 2, content.Bounds.Height / 2), overlay)!.Value;

        Assert.Equal(mid.X, at.X, 0.5);
        Assert.Equal(mid.Y, at.Y, 0.5);

        content.HorizontalAlignment = HorizontalAlignment.Left;
        Assert.Equal(HorizontalAlignment.Left, content.HorizontalAlignment);
    });

    /// <summary>`border-radius: inherit`: the corners of the container, past the Panel between.</summary>
    [Fact]
    public Task TheCornersAreTheContainers() => UiThread.RunAsync(() =>
    {
        var (container, overlay, _) = Covering("dark-background");

        Assert.Equal(new CornerRadius(4), overlay.CornerRadius);
        container.CornerRadius = new CornerRadius(12);
        Assert.Equal(new CornerRadius(12), overlay.CornerRadius);
        overlay.CornerRadius = new CornerRadius(0);
        Assert.Equal(new CornerRadius(0), overlay.CornerRadius);
    });

    /// <summary>
    /// Losing the class gives the content its own alignment back, and toggling it leaves nothing
    /// behind: each value the helper sets is an entry in the content's value store, and writing
    /// UnsetValue over them added a third rather than removing them, so the content stayed centred
    /// and grew by an entry with every toggle.
    /// </summary>
    [Fact]
    public Task TogglingTheClassLeavesNothingBehind() => UiThread.RunAsync(() =>
    {
        var (_, overlay, content) = Covering("dark-background");
        overlay.Classes.Remove("overlay");
        Settle();
        Settle();
        Assert.Equal(HorizontalAlignment.Stretch, content.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Stretch, content.VerticalAlignment);

        var (overlayEntries, contentEntries) = (Entries(overlay), Entries(content));
        for (var i = 0; i < 5; i++)
        {
            // Each showing fades in, and a running animation is an entry until it ends.
            overlay.Classes.Add("overlay");
            Settle();
            overlay.Classes.Remove("overlay");
            Dispatcher.UIThread.RunJobs();
        }

        Assert.Equal(overlayEntries, Entries(overlay));
        Assert.Equal(contentEntries, Entries(content));
        Assert.Equal(HorizontalAlignment.Stretch, content.HorizontalAlignment);
    });

    /// <summary>
    /// How many entries a control's value store holds: Avalonia keeps them in an internal list,
    /// so this reads it, as nothing public counts them.
    /// </summary>
    private static int Entries(AvaloniaObject control)
    {
        const System.Reflection.BindingFlags Hidden = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var store = typeof(AvaloniaObject).GetMethod("GetValueStore", Hidden)!.Invoke(control, null)!;
        return ((System.Collections.IList)store.GetType().GetField("_frames", Hidden)!.GetValue(store)!).Count;
    }

    /// <summary>A Border without the class is untouched: no centring, no corners taken.</summary>
    [Fact]
    public Task APlainBorderIsLeftAlone() => UiThread.RunAsync(() =>
    {
        var content = new TextBlock { Text = "Inside" };
        var plain = new Border { Child = content };
        Matrix.Show(new Border { Classes = { "paper" }, Child = new Panel { Children = { plain } } });

        Assert.Equal(HorizontalAlignment.Stretch, content.HorizontalAlignment);
        Assert.Equal(default, plain.CornerRadius);
    });

    /// <summary>`mud-animation-fadein ease 0.15s`: opacity 0 to 1 at CSS's default ease.</summary>
    [Fact]
    public Task TheFadeIsMudBlazors() => UiThread.RunAsync(() =>
    {
        // Internal to the theme, as every helper is: read where it is kept.
        var fade = Assert.IsType<Animation>(typeof(FiliThemeVariants).Assembly
            .GetType("Fili.Theme.MudAvalonia.OverlayScrim")!
            .GetField("Fade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null));

        Assert.Equal(TimeSpan.FromMilliseconds(150), fade.Duration);
        var ease = Assert.IsType<SplineEasing>(fade.Easing);
        Assert.Equal((0.25, 0.1, 0.25, 1.0), (ease.X1, ease.Y1, ease.X2, ease.Y2));
        var frames = fade.Children.Select(k => (k.Cue.CueValue, ((Setter)k.Setters.Single()).Value)).ToList();
        Assert.Equal([(0d, (object?)0d), (1d, 1d)], frames);
    });

    /// <summary>It fades in when shown again, not only the first time.</summary>
    [Fact]
    public Task ItFadesInEachTimeItShows() => UiThread.RunAsync(() =>
    {
        var (_, overlay, _) = Covering("dark-background");
        Settle();

        for (var show = 1; show <= 2; show++)
        {
            overlay.IsVisible = false;
            Dispatcher.UIThread.RunJobs();
            overlay.IsVisible = true;

            var seen = new List<double> { overlay.Opacity };
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < 80)
            {
                Thread.Sleep(10);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
                seen.Add(overlay.Opacity);
            }

            Assert.True(seen.Any(o => o < 0.9), $"Show {show} never faded: {string.Join(", ", seen.Distinct())}.");
            Settle();
        }
    });

    /// <summary>Lets a fade run out: headless, the animation clock moves only on a render tick.</summary>
    private static void Settle()
    {
        for (var i = 0; i < 30; i++)
        {
            Thread.Sleep(10);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
