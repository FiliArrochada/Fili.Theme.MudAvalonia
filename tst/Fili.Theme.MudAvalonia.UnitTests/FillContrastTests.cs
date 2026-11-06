using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Text on a fill of a palette colour reaches WCAG 2.2 AA, 4.5:1, in every variant: each colour's
/// <c>Fili{Colour}ContrastTextColor</c> on the colour and on its darken shade, which a filled
/// button, chip, FAB or toggle item and a coloured tab bar's selected tab take under the pointer.
/// MudBlazor's white fell to 1.93:1 on dark warning and 2.13:1 on tertiary, and to 4.17:1 on dark
/// primary (found by an accessibility review); since 1.2.0 the contrast text is black at 87% where
/// white falls short, and dark primary is a shade darker. The real filled controls are read too,
/// at rest and under the pointer, so a style that paints another text or ground is caught as well
/// as a token.
/// </summary>
public class FillContrastTests
{
    private const double Text = 4.5;

    private static readonly string[] Colours = ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    public static TheoryData<string> Variants => ["Light", "Dark", "HighContrast"];

    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    /// <summary>Every colour's contrast text on the colour, and on its darken shade under the pointer.</summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryContrastTextReadsOnItsFill(string name) => UiThread.RunAsync(() =>
    {
        var variant = Variant(name);
        var failures = new List<string>();
        foreach (var colour in Colours)
        {
            var text = TextContrastTests.Token($"Fili{colour}ContrastTextColor", variant);
            Check(failures, $"{name}, {colour} contrast text on the colour", text, TextContrastTests.Token($"Fili{colour}Color", variant));
            Check(failures, $"{name}, {colour} contrast text on its darken shade", text, TextContrastTests.Token($"Fili{colour}DarkenColor", variant));
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    /// <summary>
    /// The controls that paint a colour as a fill under text, each colour at rest: a filled button,
    /// a filled chip and a static one, a FAB, a filled alert, an avatar, a badge, a coloured tab bar
    /// and a coloured app bar. Each control's own ground and text are read.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryFilledControlsTextReads(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Func<IBrush?> Text, Func<IBrush?> Fill)>();
        var column = new StackPanel { Spacing = 8 };
        foreach (var colour in Colours)
        {
            var cls = colour.ToLowerInvariant();
            void Add(string what, Control control, Func<IBrush?> text, Func<IBrush?> fill)
            {
                column.Children.Add(control);
                cases.Add(($"a {cls} {what}", text, fill));
            }

            var button = new Button { Content = "Filled", Classes = { "filled", cls } };
            Add("filled button", button, () => button.Foreground, () => TextContrastTests.Part(button, "PART_Root").Background);
            var chip = new Button { Content = "Chip", Classes = { "chip", cls } };
            Add("filled chip", chip, () => chip.Foreground, () => TextContrastTests.Part(chip, "PART_Root").Background);
            var staticChip = new ContentControl { Content = "Static", Classes = { "chip", cls } };
            Add("static chip", staticChip, () => staticChip.Foreground, () => staticChip.Background);
            var fab = new Button { Content = "Fab", Classes = { "fab", cls } };
            Add("FAB", fab, () => fab.Foreground, () => TextContrastTests.Part(fab, "PART_Root").Background);
            var alertText = new TextBlock { Text = "Filled alert" };
            var alert = new Border { Classes = { "alert", "filled", cls }, Child = alertText };
            Add("filled alert", alert, () => alertText.Foreground, () => alert.Background);
            var avatarText = new TextBlock { Text = "AB" };
            var avatar = new Border { Classes = { "avatar", cls }, Child = avatarText };
            Add("avatar", avatar, () => avatarText.Foreground, () => avatar.Background);
            var badge = new HeaderedContentControl { Header = "5", Content = new Border { Width = 24, Height = 24 }, Classes = { "badge", cls }, Margin = new Thickness(12) };
            Add("badge", badge, () => Presenter(badge, "PART_HeaderPresenter").Foreground, () => TextContrastTests.Part(badge, "PART_Badge").Background);
            var tabs = new TabControl { Classes = { cls }, ItemsSource = new[] { new TabItem { Header = "One" }, new TabItem { Header = "Two" } } };
            Add("tab bar's selected tab", tabs, () => ((TabItem)tabs.Items[0]!).Foreground, () => TextContrastTests.Part(tabs, "PART_TabBar").Background);
            cases.Add(($"a {cls} tab bar's other tab", () => ((TabItem)tabs.Items[1]!).Foreground, () => TextContrastTests.Part(tabs, "PART_TabBar").Background));
            var barText = new TextBlock { Text = "App bar" };
            var bar = new Border { Classes = { "appbar", cls }, Child = barText };
            Add("app bar", bar, () => barText.Foreground, () => bar.Background);
        }

        Show(name, column, () =>
        {
            var failures = new List<string>();
            foreach (var (what, text, fill) in cases)
            {
                Check(failures, $"{name}, {what}", TextContrastTests.Solid(text()), TextContrastTests.Solid(fill()));
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        });
    });

    /// <summary>
    /// Under the pointer: a filled button's ground turns to the darken shade, and a coloured tab
    /// bar's selected tab lays the darken shade on its state layer (since 1.2.0; MudBlazor's
    /// lighten left white on primary at 4.17:1). The text must still read on either.
    /// </summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public Task AFilledControlsTextReadsUnderThePointer(string name) => UiThread.RunAsync(() =>
    {
        var cases = new List<(string What, Control Target, Func<IBrush?> Text, Func<Color> Ground)>();
        var column = new StackPanel { Spacing = 8 };
        foreach (var colour in Colours)
        {
            var cls = colour.ToLowerInvariant();
            var button = new Button { Content = "Filled", Classes = { "filled", cls } };
            column.Children.Add(button);
            cases.Add(($"a {cls} filled button", button, () => button.Foreground, () => TextContrastTests.Solid(TextContrastTests.Part(button, "PART_Root").Background)));

            var selected = new TabItem { Header = "One" };
            var tabs = new TabControl { Classes = { cls }, ItemsSource = new[] { selected, new TabItem { Header = "Two" } } };
            column.Children.Add(tabs);
            cases.Add(($"a {cls} tab bar's selected tab", selected, () => selected.Foreground, () =>
            {
                var ground = TextContrastTests.Solid(TextContrastTests.Part(tabs, "PART_TabBar").Background);
                var layer = TextContrastTests.Part(selected, "PART_StateLayer");
                return layer.Background is ISolidColorBrush tint
                    ? TextContrastTests.Over(TextContrastTests.WithOpacity(tint.Color, layer.Opacity), ground)
                    : ground;
            }));
        }

        Show(name, column, window =>
        {
            var failures = new List<string>();
            foreach (var (what, target, text, ground) in cases)
            {
                TextContrastTests.PointAt(window, target);
                Check(failures, $"{name}, {what} under the pointer", TextContrastTests.Solid(text()), ground());
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        });
    });

    private static ContentPresenter Presenter(Control control, string name) =>
        control.GetVisualDescendants().OfType<ContentPresenter>().First(p => p.Name == name);

    private static void Show(string name, Control content, Action assert) => Show(name, content, _ => assert());

    private static void Show(string name, Control content, Action<Window> assert)
    {
        var ground = new Border { Child = content, Padding = new Thickness(16), [!Border.BackgroundProperty] = new DynamicResourceExtension("FiliBackgroundBrush") };
        var window = new Window { RequestedThemeVariant = Variant(name), Content = ground, Width = 400, Height = 4000 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assert(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void Check(List<string> failures, string what, Color text, Color fill)
    {
        var ratio = Wcag.Ratio(TextContrastTests.Over(text, fill), fill);
        if (ratio < Text)
        {
            failures.Add($"{what}: {text} on {fill} is {ratio:0.00}:1, under {Text}:1.");
        }
    }
}
