using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Where an app may set a variant and override a colour, as <c>docs/compatibility.md</c> promises
/// it. Every <c>Fili*Brush</c> is one application-level object whose colour is looked up from the
/// application, so the variant is the application's to set and a colour is overridden per control
/// by its brush. These pin today's behaviour, so that changing it is a deliberate decision rather
/// than a side effect: per-scope brushes would be a major version.
/// </summary>
public class VariantScopeTests
{
    private static readonly Color LightSurface = Color.Parse("#FFFFFF");
    private static readonly Color DarkSurface = Color.Parse("#373740");
    private static readonly Color LightPrimary = Color.Parse("#594AE2");

    /// <summary>
    /// A window's or a scope's own variant reaches what looks up a per-variant key inside it, but
    /// not a brush: the brush is the application's, so a Dark window in a Light app paints Light
    /// surfaces under Dark text. Half of the window follows - which is why the docs say to set the
    /// variant on the Application only.
    /// </summary>
    [Theory]
    [InlineData("window")]
    [InlineData("scope")]
    public Task AVariantBelowTheApplicationIsOnlyHalfApplied(string host) => UiThread.RunAsync(() =>
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;
        application.RequestedThemeVariant = ThemeVariant.Light;
        var surface = new Border { Width = 50, Height = 50 };
        surface.Bind(Border.BackgroundProperty, surface.GetResourceObservable("FiliSurfaceBrush"));
        var window = host == "window"
            ? new Window { RequestedThemeVariant = ThemeVariant.Dark, Content = surface }
            : new Window { Content = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = surface } };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(ThemeVariant.Dark, surface.ActualThemeVariant);
            Assert.True(surface.TryFindResource("FiliSurfaceColor", surface.ActualThemeVariant, out var colour));
            Assert.Equal(DarkSurface, colour);
            Assert.Equal(LightSurface, ((ISolidColorBrush)surface.Background!).Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = previous;
        }
    });

    /// <summary>
    /// A <c>*Color</c> key in a control's Resources changes nothing, as the brush built from it
    /// reads the application's; the <c>*Brush</c> key there recolours that control alone.
    /// </summary>
    [Fact]
    public Task AControlTakesABrushKeyNotAColourKey() => UiThread.RunAsync(() =>
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;
        application.RequestedThemeVariant = ThemeVariant.Light;
        var byColour = new Border { Width = 50, Height = 50, Resources = { ["FiliPrimaryColor"] = Colors.Orange } };
        var byBrush = new Border { Width = 50, Height = 50, Resources = { ["FiliPrimaryBrush"] = new SolidColorBrush(Colors.Orange) } };
        byColour.Bind(Border.BackgroundProperty, byColour.GetResourceObservable("FiliPrimaryBrush"));
        byBrush.Bind(Border.BackgroundProperty, byBrush.GetResourceObservable("FiliPrimaryBrush"));
        var window = new Window { Content = new StackPanel { Children = { byColour, byBrush } } };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(LightPrimary, ((ISolidColorBrush)byColour.Background!).Color);
            Assert.Equal(Colors.Orange, ((ISolidColorBrush)byBrush.Background!).Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = previous;
        }
    });
}
