using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// High contrast is the variant meant to be read by anyone, so its colours are held to WCAG AA,
/// computed from the tokens: 4.5:1 for text, 3:1 for a line or a rail. Light and dark keep
/// MudBlazor's own palette, contrast and all; high contrast has no MudBlazor original, and each
/// of these once fell short - the <c>dark</c> class black on black, alert text at 3.4:1, white on
/// a hovered chip at 1.5:1, a slider rail at 1.9:1.
/// </summary>
public class HighContrastLegibilityTests
{
    private const double Text = 4.5;
    private const double Line = 3.0;

    private static readonly string[] Colours = ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    /// <summary>The two grounds a control sits on: the page and a paper surface.</summary>
    private static readonly string[] Grounds = ["FiliBackgroundColor", "FiliSurfaceColor"];

    /// <summary>A colour class on text, a link or an outline: the colour on the ground, and its contrast text on it.</summary>
    [Fact]
    public Task EveryColourReadsOnTheGroundAndUnderItsContrastText() => UiThread.RunAsync(() =>
    {
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            var fill = Token($"Fili{colour}Color");
            foreach (var ground in Grounds)
            {
                Check(failures, $"{colour} on {ground}", fill, Token(ground), Text);
            }

            Check(failures, $"{colour} contrast text on {colour}", Token($"Fili{colour}ContrastTextColor"), fill, Text);
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>A text or outlined alert: its text on the colour's 35% tint, laid on each ground.</summary>
    [Fact]
    public Task AlertTextReadsOnItsTint() => UiThread.RunAsync(() =>
    {
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            foreach (var ground in Grounds)
            {
                var tint = Over(Token($"Fili{colour}HoverColor"), Token(ground));
                Check(failures, $"{colour} alert text on its tint over {ground}", Token($"Fili{colour}AlertTextColor"), tint, Text);
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>A default chip hovered, focused or pressed: its text-primary label on the chip-hover ground.</summary>
    [Fact]
    public Task AHoveredDefaultChipKeepsItsText() => UiThread.RunAsync(() =>
    {
        var failures = new List<string>();
        var hover = Token("FiliChipHoverColor");
        foreach (var ground in Grounds)
        {
            Check(failures, $"chip text on chip hover over {ground}", Token("FiliTextPrimaryColor"), Over(hover, Token(ground)), Text);
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>A slider's rail, every colour at the rail opacity, against each ground and against its own track.</summary>
    [Fact]
    public Task EverySliderRailIsALine() => UiThread.RunAsync(() =>
    {
        Assert.True(Application.Current!.TryFindResource("FiliSliderRailOpacity", FiliThemeVariants.HighContrast, out var value));
        var opacity = Assert.IsType<double>(value);
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            var track = Token($"Fili{colour}Color");
            foreach (var ground in Grounds)
            {
                var rail = Over(Color.FromArgb((byte)Math.Round(opacity * 255), track.R, track.G, track.B), Token(ground));
                Check(failures, $"{colour} rail on {ground}", rail, Token(ground), Line);
                Check(failures, $"{colour} track on its rail over {ground}", track, rail, Line);
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    private static Color Token(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, FiliThemeVariants.HighContrast, out var value), $"{key} did not resolve in high contrast.");
        return Assert.IsType<Color>(value);
    }

    private static void Check(List<string> failures, string what, Color foreground, Color background, double minimum)
    {
        var ratio = Ratio(Over(foreground, background), background);
        if (ratio < minimum)
        {
            failures.Add($"{what}: {foreground} on {background} is {ratio:0.00}:1, under {minimum}:1.");
        }
    }

    /// <summary>A colour composited over an opaque ground.</summary>
    private static Color Over(Color top, Color ground)
    {
        var a = top.A / 255d;
        byte Mix(byte t, byte g) => (byte)Math.Round((a * t) + ((1 - a) * g));
        return Color.FromRgb(Mix(top.R, ground.R), Mix(top.G, ground.G), Mix(top.B, ground.B));
    }

    /// <summary>WCAG 2's contrast ratio between two opaque colours.</summary>
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
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Linear(c.R)) + (0.7152 * Linear(c.G)) + (0.0722 * Linear(c.B));
    }
}
