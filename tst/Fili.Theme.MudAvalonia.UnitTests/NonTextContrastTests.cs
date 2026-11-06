using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;
using Shapes = Avalonia.Controls.Shapes;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A palette colour as a line or glyph reaches WCAG 2.2's 3:1 for non-text contrast (1.4.11) in
/// every variant: each colour's <c>Fili{Colour}NonTextColor</c> on the page, a surface, a filled
/// field's fill over either (a focused or invalid field's underline sits on it) and the colour's
/// hover tint over either (an alert's icon, an active nav link's icon). MudBlazor paints the colour
/// itself, which fell to 1.74:1 for a light tertiary checkbox and 2.14:1 for dark primary on a
/// filled field (found by an accessibility review); since 1.2.0 the non-text token is that hue
/// moved to 3:1 where the colour falls short. The real controls that show a state or a value in a
/// colour are read too, so a style that paints the colour itself again is caught as well as a token.
/// </summary>
public class NonTextContrastTests
{
    private const double NonText = 3;

    private static readonly string[] Colours = ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    public static TheoryData<string> Variants => ["Light", "Dark", "HighContrast"];

    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    /// <summary>Every colour's non-text token on each ground it is drawn on.</summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryColoursLineShowsOnItsGrounds(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            var line = TextContrastTests.Token($"Fili{colour}NonTextColor", variant);
            foreach (var groundKey in new[] { "FiliBackgroundColor", "FiliSurfaceColor" })
            {
                var ground = TextContrastTests.Token(groundKey, variant);
                Check(failures, $"{name}, {colour} on {groundKey}", line, ground);
                Check(failures, $"{name}, {colour} on a filled field over {groundKey}", line, TextContrastTests.Over(TextContrastTests.Token("FiliInputFilledColor", variant), ground));
                Check(failures, $"{name}, {colour} on its hover tint over {groundKey}", line, TextContrastTests.Over(TextContrastTests.Token($"Fili{colour}HoverColor", variant), ground));
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>
    /// The non-text token is the colour itself, or the colour's hue moved away from the ground:
    /// darker in light, lighter in dark. High contrast's colours all clear 3:1 as they are.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryLineKeepsItsColoursHue(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        foreach (var colour in Colours)
        {
            var fill = TextContrastTests.Token($"Fili{colour}Color", variant);
            var line = TextContrastTests.Token($"Fili{colour}NonTextColor", variant);
            if (name == "HighContrast" || line == fill)
            {
                Assert.Equal(fill, line);
                continue;
            }

            var (fillHsl, lineHsl) = (fill.ToHsl(), line.ToHsl());
            var hueDistance = Math.Abs(fillHsl.H - lineHsl.H) % 360;
            Assert.True(Math.Min(hueDistance, 360 - hueDistance) < 1.5, $"{name}, {line} does not keep the hue of {colour}, {fill}.");
            Assert.True(name == "Light" ? lineHsl.L < fillHsl.L : lineHsl.L > fillHsl.L, $"{name}, {line} does not move {(name == "Light" ? "darker" : "lighter")} than {colour}, {fill}.");
        }
    });

    /// <summary>
    /// The controls that show a state or a value in a colour, on the page and on a surface: a
    /// checked checkbox and radio, an on switch's thumb, a slider's thumb and filled track, a
    /// progress bar and a circular one, a coloured icon, a tab indicator, an alert's icon on its
    /// tint, and a field's focused and invalid underline on its fill.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryColouredControlShows(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Func<IBrush?> Line, Func<Color> Under)>();
        var rows = new StackPanel();
        foreach (var groundKey in new[] { "FiliBackgroundBrush", "FiliSurfaceBrush" })
        {
            var column = new StackPanel { Spacing = 4 };
            var ground = new Border { Child = column, Padding = new Thickness(8), [!Border.BackgroundProperty] = new DynamicResourceExtension(groundKey) };
            rows.Children.Add(ground);
            Color Page() => TextContrastTests.Solid(ground.Background);

            void Add(string what, Control control, Func<IBrush?> line, Func<Color>? under = null)
            {
                if (control.Parent is null)
                {
                    column.Children.Add(control);
                }

                cases.Add(($"{what} on {groundKey}", line, under ?? Page));
            }

            foreach (var colour in Colours)
            {
                var cls = colour.ToLowerInvariant();
                var box = new CheckBox { IsChecked = true, Content = "Box", Classes = { cls } };
                Add($"a checked {cls} checkbox", box, () => Glyph(box, "PART_Glyph").Fill);
                var radio = new RadioButton { IsChecked = true, Content = "Radio", GroupName = $"{groundKey}{cls}", Classes = { cls } };
                Add($"a checked {cls} radio", radio, () => Glyph(radio, "PART_Checked").Fill);
                var toggle = new ToggleSwitch { IsChecked = true, Classes = { cls } };
                Add($"an on {cls} switch's thumb", toggle, () => TextContrastTests.Part(toggle, "PART_Thumb").Background);
                var slider = new Slider { Width = 200, Value = 40, Classes = { "filled", cls } };
                Add($"a {cls} slider's thumb", slider, () => Find<Thumb>(slider, "PART_Thumb").Background);
                Add($"a {cls} slider's filled track", slider, () => Find<RepeatButton>(slider, "PART_DecreaseButton").Background);
                var bar = new ProgressBar { Value = 40, Width = 200, Classes = { cls } };
                Add($"a {cls} progress bar", bar, () => bar.Foreground);
                var circular = new ProgressBar { Value = 40, Classes = { "circular", cls } };
                Add($"a {cls} circular progress", circular, () => circular.Foreground);
                var icon = new PathIcon { Data = Geometry.Parse("M3 18h18v-2H3v2z"), Classes = { cls } };
                Add($"a {cls} icon", icon, () => icon.Foreground);
                var tabs = new TabControl { Classes = { $"indicator-{cls}" }, ItemsSource = new[] { new TabItem { Header = "One" }, new TabItem { Header = "Two" } } };
                Add($"a {cls} tab indicator", tabs, () => TextContrastTests.Part((TabItem)tabs.Items[0]!, "PART_Indicator").Background);
                var alertIcon = new PathIcon { Data = Geometry.Parse("M3 18h18v-2H3v2z") };
                var alert = new Border { Classes = { "alert", cls }, Child = new StackPanel { Children = { alertIcon, new TextBlock { Text = "Alert" } } } };
                Add($"a {cls} alert's icon", alert, () => alertIcon.Foreground, () => TextContrastTests.Over(TextContrastTests.Solid(alert.Background), Page()));
            }

            // A field's underlines lie on its fill: none for a standard field, the 9% fill for a filled one.
            foreach (var variant in new[] { "standard", "filled" })
            {
                var field = new TextBox { Width = 200, PlaceholderText = "Label" };
                var invalid = new TextBox { Width = 200, PlaceholderText = "Label", Classes = { "error" } };
                if (variant != "standard")
                {
                    field.Classes.Add(variant);
                    invalid.Classes.Add(variant);
                }

                Color OnFill(TextBox box) => box.Background is ISolidColorBrush fill ? TextContrastTests.Over(fill.Color, Page()) : Page();
                Add($"a {variant} field's focused underline", field, () => TextContrastTests.Part(field, "PART_UnderlineActive").Background, () => OnFill(field));
                Add($"an invalid {variant} field's underline", invalid, () => TextContrastTests.Part(invalid, "PART_Underline").Background, () => OnFill(invalid));
            }
        }

        var window = new Window { RequestedThemeVariant = Variant(name), Content = rows, Width = 400, Height = 12000 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var failures = new List<string>();
            foreach (var (what, line, under) in cases)
            {
                Check(failures, $"{name}, {what}", TextContrastTests.Solid(line()), under());
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
    });

    private static Shapes.Path Glyph(Control control, string name) => Find<Shapes.Path>(control, name);

    private static T Find<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(c => c.Name == name);

    private static void Check(List<string> failures, string what, Color line, Color ground)
    {
        var ratio = Wcag.Ratio(TextContrastTests.Over(line, ground), ground);
        if (ratio < NonText)
        {
            failures.Add($"{what}: {line} on {ground} is {ratio:0.00}:1, under {NonText}:1.");
        }
    }
}
