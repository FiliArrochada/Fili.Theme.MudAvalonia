using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
/// again is caught as well as a token. Since 1.2.0 the tints a chip or a nav link lays under the
/// text on hover are grounds too, and a hovered chip is read.
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

    /// <summary>
    /// Every colour's text token, on the page, a surface, and the tints a control lays under it over
    /// either: its hover tint (a text button, a selected row), the colour at 12% (a text chip or a
    /// selected outlined chip under the pointer, the state layer at 0.12), and the hover tint
    /// deepened by the colour at 6.4% (a selected nav link under the pointer). The last two are
    /// since 1.2.0, when hovering a text chip dropped its text to 4.2:1.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryColoursTextReadsOnItsGrounds(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            var text = Token($"Fili{colour}TextColor", variant);
            var fill = Token($"Fili{colour}Color", variant);
            var hover = Token($"Fili{colour}HoverColor", variant);
            foreach (var groundKey in Grounds)
            {
                var ground = Token(groundKey, variant);
                Check(failures, $"{name}, {colour} text on {groundKey}", text, ground);
                Check(failures, $"{name}, {colour} text on its hover tint over {groundKey}", text, Over(hover, ground));
                Check(failures, $"{name}, {colour} text on the colour at 12% over {groundKey}", text, Over(WithOpacity(fill, 0.12), ground));
                Check(failures, $"{name}, {colour} text on its hover tint deepened by a nav link's hover over {groundKey}", text, Over(WithOpacity(fill, 0.064), Over(hover, ground)));
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>
    /// A text chip and a selected outlined chip of each colour under the pointer: the chip's own
    /// ground goes and its state layer shows the colour at 12%, under the colour as text. Read from
    /// the real chip, on the page and a surface, so a state that lays a stronger tint is caught.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task AChipsTextReadsUnderThePointer(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Border Ground, Button Chip)>();
        var rows = new StackPanel();
        foreach (var groundKey in new[] { "FiliBackgroundBrush", "FiliSurfaceBrush" })
        {
            var column = new StackPanel { Spacing = 8 };
            var ground = new Border { Child = column, Padding = new Thickness(8), [!Border.BackgroundProperty] = new DynamicResourceExtension(groundKey) };
            rows.Children.Add(ground);
            foreach (var colour in Colours)
            {
                var cls = colour.ToLowerInvariant();
                var text = new Button { Content = "Text chip", Classes = { "chip", "text", cls } };
                var outlined = new Avalonia.Controls.Primitives.ToggleButton { Content = "Outlined chip", IsChecked = true, Classes = { "chip", "outlined", cls } };
                column.Children.Add(text);
                column.Children.Add(outlined);
                cases.Add(($"a {cls} text chip on {groundKey}", ground, text));
                cases.Add(($"a selected {cls} outlined chip on {groundKey}", ground, outlined));
            }
        }

        var window = new Window { RequestedThemeVariant = Variant(name), Content = rows, Width = 400, Height = 3000 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var failures = new List<string>();
            foreach (var (what, ground, chip) in cases)
            {
                PointAt(window, chip);
                var page = Solid(ground.Background);
                var root = Part(chip, "PART_Root");
                var layer = Part(chip, "PART_StateLayer");
                var under = root.Background is ISolidColorBrush rootFill ? Over(rootFill.Color, page) : page;
                if (layer.Background is ISolidColorBrush layerFill)
                {
                    under = Over(WithOpacity(layerFill.Color, layer.Opacity), under);
                }

                Check(failures, $"{name}, {what} under the pointer", Solid(chip.Foreground), under);
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
    });

    /// <summary>
    /// A selected list row of each colour, and a selected select option, under the pointer: the
    /// row's tint deepened by the colour at 6.4%, as a selected nav link's is, read from the real
    /// row on the page and a surface. Until 1.2.1 the row swapped its tint for the pressed overlay, and its coloured text
    /// fell to 2.9:1 in dark.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task ASelectedRowsTextReadsUnderThePointer(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Border Ground, ContentControl Row)>();
        var rows = new StackPanel();
        foreach (var groundKey in new[] { "FiliBackgroundBrush", "FiliSurfaceBrush" })
        {
            var column = new StackPanel { Spacing = 8 };
            var ground = new Border { Child = column, Padding = new Thickness(8), [!Border.BackgroundProperty] = new DynamicResourceExtension(groundKey) };
            rows.Children.Add(ground);
            foreach (var colour in Colours)
            {
                var cls = colour.ToLowerInvariant();
                var item = new ListBoxItem { Content = "Selected row" };
                var list = new ListBox { Classes = { cls }, Items = { item } };
                list.SelectedItem = item;
                column.Children.Add(list);
                cases.Add(($"a selected row of a {cls} list on {groundKey}", ground, item));
            }

            var option = new ComboBoxItem { Content = "Selected option", IsSelected = true };
            column.Children.Add(option);
            cases.Add(($"a selected select option on {groundKey}", ground, option));
        }

        var window = new Window { RequestedThemeVariant = Variant(name), Content = rows, Width = 400, Height = 3000 };
        window.Resources["FiliReducedMotion"] = true;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var failures = new List<string>();
            foreach (var (what, ground, row) in cases)
            {
                Assert.True(row.Classes.Contains(":selected"), $"{what} is not selected.");
                PointAt(window, row);
                var root = row.GetVisualDescendants().OfType<Panel>().First(p => p.Name == "PART_Root");
                var layer = Part(row, "PART_StateLayer");
                Assert.True(root.Background is ISolidColorBrush, $"{what} keeps no tint under the pointer.");
                Assert.True(layer.Opacity < 0.1, $"{what} lays {layer.Background} at {layer.Opacity} over its tint, not the colour at 6.4%.");

                var under = Over(Solid(root.Background), Solid(ground.Background));
                under = Over(WithOpacity(Solid(layer.Background), layer.Opacity), under);
                Check(failures, $"{name}, {what} under the pointer", Solid(row.Foreground), under);
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
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

    /// <summary>
    /// A real pointer move onto <paramref name="target"/>, from the window's far corner, so the
    /// control under the pointer before it is left. The headless mouse is shared, and this leaves
    /// no button down.
    /// </summary>
    internal static void PointAt(Window window, Control target)
    {
        var centre = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseMove(new Point(window.Width - 1, window.Height - 1), RawInputModifiers.None);
        window.MouseMove(centre, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.True(target.IsPointerOver, $"The pointer move did not reach the {target.GetType().Name}.");
    }

    internal static Border Part(Control control, string name) =>
        control.GetVisualDescendants().OfType<Border>().First(b => b.Name == name);

    /// <summary>A colour with its alpha scaled, as an element's Opacity paints it.</summary>
    internal static Color WithOpacity(Color colour, double opacity) =>
        Color.FromArgb((byte)Math.Round(colour.A * opacity), colour.R, colour.G, colour.B);

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

    internal static Color Solid(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

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
