using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A text or outlined alert's text reaches WCAG 2.2 AA, 4.5:1, on the ground the theme composes
/// for it, in every variant: the alert's own background (the colour's hover tint, or nothing for
/// an outlined one) laid over the page and over a paper surface. MudBlazor's darken shade, which
/// the alert text was until 1.0.1, fell short in light and dark for most colours (found by an
/// adopting app's accessibility review); the colours, grounds and text are read from real alerts,
/// so a change to the style as well as to a token is caught.
/// </summary>
public class AlertTextContrastTests
{
    private const double Text = 4.5;

    private static readonly string[] Colours = ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    /// <summary>The two grounds an alert sits on: the page and a paper surface.</summary>
    private static readonly string[] Grounds = ["FiliBackgroundBrush", "FiliSurfaceBrush"];

    public static TheoryData<string> Variants => ["Light", "Dark", "HighContrast"];

    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryAlertsTextReadsOnItsGround(string variant) => UiThread.RunAsync(() =>
    {
        // Every colour as a text and an outlined alert, and the normal alert with no colour, both ways.
        var classes = Colours.Select(c => new[] { c.ToLowerInvariant() })
            .Concat(Colours.Select(c => new[] { "outlined", c.ToLowerInvariant() }))
            .Append(Array.Empty<string>())
            .Append(["outlined"])
            .ToList();

        var cases = new List<(string What, Border Ground, Border Alert, TextBlock Text)>();
        var rows = new StackPanel();
        foreach (var groundKey in Grounds)
        {
            var column = new StackPanel();
            var ground = new Border { Child = column, [!Border.BackgroundProperty] = new DynamicResourceExtension(groundKey) };
            foreach (var extra in classes)
            {
                var text = new TextBlock { Text = "Message" };
                var alert = new Border { Child = text };
                alert.Classes.Add("alert");
                alert.Classes.AddRange(extra);
                column.Children.Add(alert);
                cases.Add(($"{string.Join(' ', extra.DefaultIfEmpty("normal"))} alert on {groundKey}", ground, alert, text));
            }

            rows.Children.Add(ground);
        }

        var window = new Window { RequestedThemeVariant = Variant(variant), Content = rows, Width = 400, Height = 2000 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var failures = new List<string>();
            foreach (var (what, ground, alert, text) in cases)
            {
                var groundColour = Opaque(ground.Background);
                var alertGround = alert.Background is null ? groundColour : Over(Solid(alert.Background), groundColour);
                var foreground = Over(Solid(text.Foreground), alertGround);
                var ratio = Ratio(foreground, alertGround);
                if (ratio < Text)
                {
                    failures.Add($"{variant}, {what}: {Solid(text.Foreground)} on {alertGround} is {ratio:0.00}:1, under {Text}:1.");
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
    });

    /// <summary>The computation itself, against the published WCAG examples' end points.</summary>
    [Fact]
    public Task TheRatioIsWcags() => UiThread.RunAsync(() =>
    {
        Assert.Equal(21, Ratio(Colors.Black, Colors.White), 3);
        Assert.Equal(1, Ratio(Color.Parse("#777777"), Color.Parse("#777777")), 3);
        Assert.Equal(4.48, Ratio(Color.Parse("#777777"), Colors.White), 2);
    });

    private static Color Solid(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

    private static Color Opaque(IBrush? brush)
    {
        var colour = Solid(brush);
        Assert.True(colour.A == 255, $"The ground {colour} is not opaque.");
        return colour;
    }

    /// <summary>A colour composited over an opaque ground.</summary>
    private static Color Over(Color top, Color ground)
    {
        var a = top.A / 255d;
        byte Mix(byte t, byte g) => (byte)Math.Round((a * t) + ((1 - a) * g));
        return Color.FromRgb(Mix(top.R, ground.R), Mix(top.G, ground.G), Mix(top.B, ground.B));
    }

    /// <summary>WCAG 2's contrast ratio between two opaque colours, from their relative luminance.</summary>
    private static double Ratio(Color a, Color b)
    {
        var (la, lb) = (Luminance(a), Luminance(b));
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(Color c)
    {
        static double Linear(byte channel)
        {
            var v = channel / 255d;
            return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Linear(c.R)) + (0.7152 * Linear(c.G)) + (0.0722 * Linear(c.B));
    }
}
