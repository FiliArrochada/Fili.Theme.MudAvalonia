using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery;
using Fili.Theme.MudAvalonia.Gallery.Docs;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// The app bar's primary colour picker: a swatch repaints the site in light and dark, leaves high
/// contrast alone, and the theme's own swatch puts the shipped tokens back.
/// </summary>
public class PrimaryColourTests
{
    private static readonly Color Blue = Color.Parse("#1565C0");

    public static TheoryData<string> PaletteColours() =>
        ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    /// <summary>
    /// The picker's darken and lighten are MudBlazor's: they give the shades the theme ships for
    /// every palette colour, in light and dark, so a picked colour's shades are what MudBlazor's
    /// would be.
    /// </summary>
    [Theory]
    [MemberData(nameof(PaletteColours))]
    public Task TheShadesAreMudBlazors(string name) => UiThread.RunAsync(() =>
    {
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            var colour = Resource($"Fili{name}Color", variant);
            Assert.Equal(Resource($"Fili{name}DarkenColor", variant), PrimaryColour.MudColor.Darken(colour));
            Assert.Equal(Resource($"Fili{name}LightenColor", variant), PrimaryColour.MudColor.Lighten(colour));
        }
    });

    /// <summary>A swatch repaints the primary and the light app bar, and high contrast keeps its own.</summary>
    [Fact]
    public Task ASwatchRepaintsThePrimary() => UiThread.RunAsync(() =>
    {
        var app = Application.Current!;
        var shippedHighContrast = Resource("FiliPrimaryColor", FiliThemeVariants.HighContrast);
        var view = new MainView();
        var variant = app.RequestedThemeVariant;
        app.RequestedThemeVariant = ThemeVariant.Light;
        var window = new Window { Content = view, Width = 1440, Height = 900 };
        window.Show();
        try
        {
            Click(view, "Blue");

            Assert.Equal(Blue, PrimaryColour.Current);
            Assert.Equal(Blue, Resource("FiliPrimaryColor", ThemeVariant.Light));
            Assert.Equal(Blue, Resource("FiliPrimaryColor", ThemeVariant.Dark));
            Assert.Equal(Color.Parse("#0F1565C0"), Resource("FiliPrimaryHoverColor", ThemeVariant.Light));
            Assert.Equal(PrimaryColour.MudColor.Darken(Blue), Resource("FiliPrimaryDarkenColor", ThemeVariant.Light));
            Assert.Equal(shippedHighContrast, Resource("FiliPrimaryColor", FiliThemeVariants.HighContrast));

            var appBar = view.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("appbar"));
            Assert.Equal(Blue, ((ISolidColorBrush)appBar.Background!).Color);
        }
        finally
        {
            PrimaryColour.Apply(app, null);
            app.RequestedThemeVariant = variant;
            window.Close();
        }
    });

    /// <summary>The theme's own swatch takes the overrides away: the shipped primary is back.</summary>
    [Fact]
    public Task TheThemesSwatchPutsItBack() => UiThread.RunAsync(() =>
    {
        var app = Application.Current!;
        var shipped = Resource("FiliPrimaryColor", ThemeVariant.Light);
        var view = new MainView();
        var variant = app.RequestedThemeVariant;
        app.RequestedThemeVariant = ThemeVariant.Light;
        var window = new Window { Content = view, Width = 1440, Height = 900 };
        window.Show();
        try
        {
            Click(view, "Blue");
            Click(view, "MudBlazor");

            Assert.Null(PrimaryColour.Current);
            Assert.Equal(shipped, Resource("FiliPrimaryColor", ThemeVariant.Light));
            var appBar = view.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("appbar"));
            Assert.Equal(shipped, ((ISolidColorBrush)appBar.Background!).Color);
        }
        finally
        {
            PrimaryColour.Apply(app, null);
            app.RequestedThemeVariant = variant;
            window.Close();
        }
    });

    /// <summary>Every swatch carries its text, and the tick on it, at AA.</summary>
    [Fact]
    public void EverySwatchReadsWithItsText()
    {
        foreach (var (name, colour) in PrimaryColour.Swatches)
        {
            Assert.True(Contrast(PrimaryColour.ContrastText(colour), colour) >= 4.5, $"{name}'s text is below AA.");
        }
    }

    private static void Click(MainView view, string name)
    {
        var swatch = view.FindControl<WrapPanel>("Swatches")!.Children.OfType<Button>()
            .Single(b => Equals(ToolTip.GetTip(b), name));
        swatch.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }

    private static Color Resource(string key, ThemeVariant variant) =>
        Application.Current!.TryGetResource(key, variant, out var value) && value is Color colour
            ? colour
            : throw new InvalidOperationException($"{key} does not resolve in {variant}.");

    private static double Contrast(Color a, Color b)
    {
        static double Luminance(Color c)
        {
            static double Channel(byte v)
            {
                var s = v / 255d;
                return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            }

            return (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));
        }

        var (la, lb) = (Luminance(a), Luminance(b));
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }
}
