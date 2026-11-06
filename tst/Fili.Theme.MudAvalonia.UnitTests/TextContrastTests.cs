using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Text reaches WCAG 2.2 AA, 4.5:1, on the page and on a surface, in every variant: each palette
/// colour as text (<c>Fili{Colour}TextColor</c>) on the ground and on the colour's own hover tint
/// over it, which a text button or a selected row paints under the text, and text-primary and
/// text-secondary on the ground. MudBlazor's colours as text fell as low as 1.17:1 (dark `dark`)
/// and text-secondary to 4.26:1 on the dark surface (found by an accessibility
/// review); since 1.1.0 the text tokens are moved to 4.5:1 where the colour fell short. The real
/// controls that paint a colour as text are read too, so a style that paints the colour itself
/// again is caught as well as a token.
/// </summary>
public class TextContrastTests
{
    private const double Text = 4.5;

    private static readonly string[] Colours = ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    private static readonly string[] Grounds = ["FiliBackgroundColor", "FiliSurfaceColor"];

    public static TheoryData<string> Variants => ["Light", "Dark", "HighContrast"];

    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    /// <summary>Every colour's text token, on the page, a surface, and its hover tint over either.</summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryColoursTextReadsOnItsGrounds(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            var text = Token($"Fili{colour}TextColor", variant);
            foreach (var groundKey in Grounds)
            {
                var ground = Token(groundKey, variant);
                Check(failures, $"{name}, {colour} text on {groundKey}", text, ground);
                Check(failures, $"{name}, {colour} text on its hover tint over {groundKey}", text, Over(Token($"Fili{colour}HoverColor", variant), ground));
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>Text-primary and text-secondary on the page and a surface; the drawer's text on the drawer.</summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task ThePlainTextReadsOnItsGrounds(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        var failures = new List<string>();
        foreach (var textKey in new[] { "FiliTextPrimaryColor", "FiliTextSecondaryColor" })
        {
            foreach (var groundKey in Grounds)
            {
                Check(failures, $"{name}, {textKey} on {groundKey}", Token(textKey, variant), Token(groundKey, variant));
            }
        }

        Check(failures, $"{name}, drawer text on the drawer", Token("FiliDrawerTextColor", variant), Token("FiliDrawerBackgroundColor", variant));
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>
    /// The controls that paint a colour as text: a link, a text and an outlined button and MudText
    /// in each colour, and an error label, on the page and on a surface. Each control's own
    /// Foreground is read, so it is the style as well as the token under test.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryColouredControlsTextReads(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Border Ground, Func<IBrush?> Foreground)>();
        var rows = new StackPanel();
        foreach (var groundKey in new[] { "FiliBackgroundBrush", "FiliSurfaceBrush" })
        {
            var column = new StackPanel();
            var ground = new Border { Child = column, [!Border.BackgroundProperty] = new DynamicResourceExtension(groundKey) };
            rows.Children.Add(ground);

            void Add(string what, Control control, Func<IBrush?> foreground)
            {
                column.Children.Add(control);
                cases.Add(($"{what} on {groundKey}", ground, foreground));
            }

            var link = new HyperlinkButton { Content = "Link" };
            Add("a link", link, () => link.Foreground);
            var label = new Label { Content = "Invalid", Classes = { "error" } };
            Add("an error label", label, () => label.Foreground);
            foreach (var colour in Colours)
            {
                var cls = colour.ToLowerInvariant();
                var text = new Button { Content = "Text", Classes = { cls } };
                Add($"a {cls} text button", text, () => text.Foreground);
                var outlined = new Button { Content = "Outlined", Classes = { "outlined", cls } };
                Add($"a {cls} outlined button", outlined, () => outlined.Foreground);
                var block = new TextBlock { Text = "MudText", Classes = { cls } };
                Add($"{cls} MudText", block, () => block.Foreground);
            }
        }

        var window = new Window { RequestedThemeVariant = Variant(name), Content = new ScrollViewer { Content = rows }, Width = 400, Height = 800 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var failures = new List<string>();
            foreach (var (what, ground, foreground) in cases)
            {
                var groundColour = Solid(ground.Background);
                Check(failures, $"{name}, {what}", Solid(foreground()), groundColour);
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
    });

    private static void Check(List<string> failures, string what, Color text, Color ground)
    {
        var painted = Over(text, ground);
        var ratio = Wcag.Ratio(painted, ground);
        if (ratio < Text)
        {
            failures.Add($"{what}: {text} on {ground} is {ratio:0.00}:1, under {Text}:1.");
        }
    }

    internal static Color Token(string key, ThemeVariant variant) =>
        Application.Current!.TryGetResource(key, variant, out var value) && value is Color colour
            ? colour
            : throw new InvalidOperationException($"{key} does not resolve in {variant}.");

    private static Color Solid(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

    /// <summary>A colour composited over an opaque ground.</summary>
    internal static Color Over(Color top, Color ground)
    {
        var a = top.A / 255d;
        byte Mix(byte t, byte g) => (byte)Math.Round((a * t) + ((1 - a) * g));
        return Color.FromRgb(Mix(top.R, ground.R), Mix(top.G, ground.G), Mix(top.B, ground.B));
    }
}

/// <summary>WCAG 2's contrast ratio between two opaque colours, from their relative luminance.</summary>
internal static class Wcag
{
    public static double Ratio(Color a, Color b)
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
