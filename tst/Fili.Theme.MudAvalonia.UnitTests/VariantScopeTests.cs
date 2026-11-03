using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Where an app may set a variant and override a colour, as <c>docs/compatibility.md</c> promises
/// it. Every brush is declared in each variant's dictionary and resolves its colour for that
/// variant, so a window or a <c>ThemeVariantScope</c> with a variant of its own paints with that
/// variant's brushes, while a colour an app sets in its application's resources still reaches
/// them. A lookup that names no variant finds <c>FallbackBrushes.axaml</c>, which follows the
/// application's variant as every brush did before 0.32.0.
/// </summary>
public class VariantScopeTests
{
    private static readonly Color LightSurface = Color.Parse("#FFFFFF");
    private static readonly Color DarkSurface = Color.Parse("#373740");
    private static readonly Color LightPrimary = Color.Parse("#594AE2");

    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    private static Color ColourOf(string key, ThemeVariant variant)
    {
        Assert.True(Application.Current!.TryFindResource(key, variant, out var value), $"{key} did not resolve in {variant}.");
        return Assert.IsType<Color>(value);
    }

    private static Border Painted(string key) => new() { Width = 50, Height = 50, [!Border.BackgroundProperty] = new DynamicResourceExtension(key) };

    private static void InApplication(ThemeVariant variant, Action check)
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;
        application.RequestedThemeVariant = variant;
        try
        {
            check();
        }
        finally
        {
            application.RequestedThemeVariant = previous;
        }
    }

    /// <summary>
    /// A window's or a scope's own variant restyles what is inside it: its palette brushes, the
    /// forked templates' bridge brushes and the brushes the forked pages declare themselves are
    /// that variant's, in a light application and a dark one.
    /// </summary>
    [Theory]
    [InlineData("window", "Light", "Dark")]
    [InlineData("scope", "Light", "Dark")]
    [InlineData("scope", "Light", "HighContrast")]
    [InlineData("scope", "Dark", "Light")]
    [InlineData("window", "HighContrast", "Light")]
    public Task AVariantBelowTheApplicationIsApplied(string host, string applicationVariant, string ownVariant) => UiThread.RunAsync(() =>
        InApplication(Variant(applicationVariant), () =>
        {
            var own = Variant(ownVariant);
            var surface = Painted("FiliSurfaceBrush");
            var bridge = Painted("ThemeForegroundBrush");
            var navigationBar = Painted("NavigationBarBackground");
            var selectedTab = Painted("TabbedPageTabItemHeaderForegroundSelected");
            var content = new StackPanel { Children = { surface, bridge, navigationBar, selectedTab } };
            var window = host == "window"
                ? new Window { RequestedThemeVariant = own, Content = content }
                : new Window { Content = new ThemeVariantScope { RequestedThemeVariant = own, Child = content } };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();

                Assert.Equal(own, surface.ActualThemeVariant);
                Assert.Equal(ColourOf("FiliSurfaceColor", own), ((ISolidColorBrush)surface.Background!).Color);
                Assert.Equal(ColourOf("FiliTextPrimaryColor", own), ((ISolidColorBrush)bridge.Background!).Color);
                Assert.Equal(ColourOf("ThemeControlMidColor", own), ((ISolidColorBrush)navigationBar.Background!).Color);
                Assert.Equal(ColourOf("ThemeAccentColor", own), ((ISolidColorBrush)selectedTab.Background!).Color);
            }
            finally
            {
                window.Close();
            }
        }));

    /// <summary>A scope changed while shown repaints at once, as the application's variant does.</summary>
    [Fact]
    public Task AScopesVariantChangedWhileShownRepaints() => UiThread.RunAsync(() => InApplication(ThemeVariant.Light, () =>
    {
        var surface = Painted("FiliSurfaceBrush");
        var scope = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light, Child = surface };
        var window = new Window { Content = scope };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(LightSurface, ((ISolidColorBrush)surface.Background!).Color);

            scope.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(DarkSurface, ((ISolidColorBrush)surface.Background!).Color);
        }
        finally
        {
            window.Close();
        }
    }));

    /// <summary>
    /// A colour an app sets in its application's resources, per variant, reaches the brushes of
    /// every variant, inside a scope as outside one; and a change at runtime follows.
    /// </summary>
    [Fact]
    public Task AnAppsColourReachesEveryVariantsBrushes() => UiThread.RunAsync(() => InApplication(ThemeVariant.Light, () =>
    {
        var application = Application.Current!;
        var light = new ResourceDictionary { ["FiliPrimaryColor"] = Colors.Orange };
        var dark = new ResourceDictionary { ["FiliPrimaryColor"] = Colors.Teal };
        application.Resources.ThemeDictionaries[ThemeVariant.Light] = light;
        application.Resources.ThemeDictionaries[ThemeVariant.Dark] = dark;
        var outside = Painted("FiliPrimaryBrush");
        var inside = Painted("FiliPrimaryBrush");
        var window = new Window
        {
            Content = new StackPanel { Children = { outside, new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = inside } } },
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Colors.Orange, ((ISolidColorBrush)outside.Background!).Color);
            Assert.Equal(Colors.Teal, ((ISolidColorBrush)inside.Background!).Color);

            dark["FiliPrimaryColor"] = Colors.Gold;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Colors.Gold, ((ISolidColorBrush)inside.Background!).Color);
        }
        finally
        {
            window.Close();
            application.Resources.ThemeDictionaries.Remove(ThemeVariant.Light);
            application.Resources.ThemeDictionaries.Remove(ThemeVariant.Dark);
        }
    }));

    /// <summary>
    /// A lookup that names no variant - FindResource(key) in an app's code - still finds every
    /// brush, following the application's variant: a variant dictionary is searched only for a
    /// lookup that names one, and FallbackBrushes.axaml answers the rest.
    /// </summary>
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public Task ALookupWithNoVariantFindsTheApplicationsBrush(string name) => UiThread.RunAsync(() => InApplication(Variant(name), () =>
    {
        var control = new Border();
        var window = new Window { Content = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = control } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var expected = ColourOf("FiliSurfaceColor", Variant(name));

            Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(Application.Current!.FindResource("FiliSurfaceBrush")).Color);
            Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(control.FindResource("FiliSurfaceBrush")).Color);
            Assert.IsAssignableFrom<ISolidColorBrush>(control.FindResource("ThemeForegroundBrush"));
        }
        finally
        {
            window.Close();
        }
    }));

    /// <summary>
    /// A <c>*Color</c> key in a control's Resources changes nothing, as the brush built from it
    /// reads the application's; the <c>*Brush</c> key there recolours that control alone.
    /// </summary>
    [Fact]
    public Task AControlTakesABrushKeyNotAColourKey() => UiThread.RunAsync(() => InApplication(ThemeVariant.Light, () =>
    {
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
        }
    }));
}
