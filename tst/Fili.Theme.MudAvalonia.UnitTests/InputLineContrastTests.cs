using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// An unfocused field's edge reaches WCAG 2.2's 3:1 for non-text contrast (1.4.11) in every
/// variant: lines-inputs on the page, on a surface, and on a filled field's own fill over either,
/// as a filled field's underline sits on its fill. MudBlazor's lines-inputs was 1.88:1 in light and
/// 2.50:1 in dark (found by an accessibility review); since 1.1.0 it is moved to
/// 3:1. The real standard, filled and outlined text fields are read too, so a template that paints
/// another colour for the edge is caught as well as the token.
/// </summary>
public class InputLineContrastTests
{
    private const double NonText = 3;

    public static TheoryData<string> Variants => ["Light", "Dark", "HighContrast"];

    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public Task TheInputLineShowsOnEveryGround(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        var line = TextContrastTests.Token("FiliLinesInputsColor", variant);
        var failures = new List<string>();
        foreach (var groundKey in new[] { "FiliBackgroundColor", "FiliSurfaceColor" })
        {
            var ground = TextContrastTests.Token(groundKey, variant);
            Check(failures, $"{name}, on {groundKey}", line, ground);
            Check(failures, $"{name}, on a filled field over {groundKey}", line, TextContrastTests.Over(TextContrastTests.Token("FiliInputFilledColor", variant), ground));
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>A standard and a filled field's underline, and an outlined field's outline, at rest.</summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryFieldsEdgeShows(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Border Ground, TextBox Field)>();
        var rows = new StackPanel();
        foreach (var groundKey in new[] { "FiliBackgroundBrush", "FiliSurfaceBrush" })
        {
            var column = new StackPanel();
            var ground = new Border { Child = column, Padding = new Avalonia.Thickness(8), [!Border.BackgroundProperty] = new DynamicResourceExtension(groundKey) };
            rows.Children.Add(ground);
            foreach (var variant in new[] { "standard", "filled", "outlined" })
            {
                var field = new TextBox { Width = 200, PlaceholderText = "Label" };
                if (variant != "standard")
                {
                    field.Classes.Add(variant);
                }

                column.Children.Add(field);
                cases.Add(($"a {variant} field on {groundKey}", ground, field));
            }
        }

        var window = new Window { RequestedThemeVariant = Variant(name), Content = rows, Width = 400, Height = 800 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var failures = new List<string>();
            foreach (var (what, ground, field) in cases)
            {
                var groundColour = Solid(ground.Background);
                if (field.Classes.Contains("outlined"))
                {
                    var root = Part(field, "PART_Root");
                    Check(failures, $"{name}, {what}", Solid(root.BorderBrush), groundColour);
                }
                else
                {
                    // The fill, if any, is the field's Background, under the underline.
                    var under = field.Background is ISolidColorBrush fill ? TextContrastTests.Over(fill.Color, groundColour) : groundColour;
                    Check(failures, $"{name}, {what}", Solid(Part(field, "PART_Underline").Background), under);
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
    });

    private static Border Part(Control control, string name) =>
        control.GetVisualDescendants().OfType<Border>().First(b => b.Name == name);

    private static Color Solid(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

    private static void Check(List<string> failures, string what, Color line, Color ground)
    {
        var ratio = Wcag.Ratio(TextContrastTests.Over(line, ground), ground);
        if (ratio < NonText)
        {
            failures.Add($"{what}: the edge {line} on {ground} is {ratio:0.00}:1, under {NonText}:1.");
        }
    }
}
