using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudExpansionPanels' options on one Expander, from _expansionpanel.scss and
/// MudExpansionPanels.razor.cs: Elevation, Square, Dense and Gutters. And the elevation classes
/// everywhere they are published: every level _elevation.scss has, 0 to 25.
/// </summary>
public class ExpansionPanelTests
{
    private static Expander Show(params string[] classes)
    {
        var expander = new Expander { Header = "Panel", Content = "Content", IsExpanded = true, Width = 320 };
        expander.Classes.AddRange(classes);
        return Matrix.Show(expander);
    }

    private static Border Root(Expander expander) => Matrix.Part<Border>(expander, "PART_Root");

    /// <summary>The content's padding: the Expander's own, which its content presenter is bound to.</summary>
    private static Thickness ContentPadding(Expander expander) => expander.Padding;

    private static Thickness HeaderPadding(Expander expander) =>
        Matrix.Part<ToggleButton>(expander, "PART_toggle").Padding;

    /// <summary>MudExpansionPanels' Elevation defaults to 1; the panel is surface, with the default radius.</summary>
    [Fact]
    public Task APanelIsSurfaceAtElevation1() => UiThread.RunAsync(() =>
    {
        var expander = Show();

        Assert.Equal(FabTests.Shadow(1), Root(expander).BoxShadow);
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(Root(expander).Background));
        Assert.Equal(new CornerRadius(4), Root(expander).CornerRadius);
        Assert.Equal(new Thickness(24, 0, 24, 16), ContentPadding(expander));
        Assert.Equal(new Thickness(24, 16), HeaderPadding(expander));
    });

    /// <summary>
    /// `flat` was this theme's own word for a panel on a card, and is gone: `elevation0` is
    /// MudBlazor's, and keeps the surface as MudBlazor's Elevation="0" does.
    /// </summary>
    [Fact]
    public Task ElevationZeroKeepsTheSurfaceAndFlatDoesNothing() => UiThread.RunAsync(() =>
    {
        var level0 = Show("elevation0");
        Assert.Equal(FabTests.Shadow(0), Root(level0).BoxShadow);
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(Root(level0).Background));

        var flat = Show("flat");
        Assert.Equal(FabTests.Shadow(1), Root(flat).BoxShadow);
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(Root(flat).Background));
    });

    /// <summary>Square on one panel is its CornerRadius, which reaches the root that draws it.</summary>
    [Fact]
    public Task SquareIsTheCornerRadius() => UiThread.RunAsync(() =>
    {
        var expander = Show();
        expander.CornerRadius = new CornerRadius(0);

        Assert.Equal(new CornerRadius(0), Root(expander).CornerRadius);
    });

    /// <summary>`.mud-expand-panel-dense`: the content loses its top and bottom padding, the header keeps its own.</summary>
    [Fact]
    public Task DenseDropsTheContentsVerticalPadding() => UiThread.RunAsync(() =>
    {
        var expander = Show("dense");

        Assert.Equal(new Thickness(24, 0, 24, 0), ContentPadding(expander));
        Assert.Equal(new Thickness(24, 16), HeaderPadding(expander));
    });

    /// <summary>Gutters="false": neither the header nor the content has its 24px sides.</summary>
    [Fact]
    public Task NoGuttersDropsTheSides() => UiThread.RunAsync(() =>
    {
        var expander = Show("no-gutters");

        Assert.Equal(new Thickness(0, 0, 0, 16), ContentPadding(expander));
        Assert.Equal(new Thickness(0, 16), HeaderPadding(expander));
        Assert.Equal(new Thickness(0), ContentPadding(Show("dense", "no-gutters")));
    });

    /// <summary>The options keep each ExpandDirection's own sides: an upward panel pads its content's top.</summary>
    [Fact]
    public Task TheOptionsFollowTheExpandDirection() => UiThread.RunAsync(() =>
    {
        var up = Show("no-gutters");
        up.ExpandDirection = ExpandDirection.Up;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Thickness(0, 16, 0, 0), ContentPadding(up));

        var right = Show("dense");
        right.ExpandDirection = ExpandDirection.Right;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Thickness(0, 0, 24, 0), ContentPadding(right));
    });

    /// <summary>
    /// `mud-elevation-0` to `mud-elevation-25`, every one of them, on each control that publishes
    /// the elevation classes - not only the nine steps the theme once had.
    /// </summary>
    [Fact]
    public Task EveryElevationLevelIsAClassEverywhere() => UiThread.RunAsync(() =>
    {
        Assert.Equal(Enumerable.Range(0, 26), ThemeColourGenerator.Elevations);

        foreach (var level in ThemeColourGenerator.Elevations)
        {
            var cls = $"elevation{level}";

            var paper = new Border { Width = 40, Height = 40 };
            paper.Classes.Add(cls);
            Assert.Equal(FabTests.Shadow(level), Matrix.Show(paper).BoxShadow);

            Assert.Equal(FabTests.Shadow(level), Root(Show(cls)).BoxShadow);

            var tabs = new TabControl { Items = { new TabItem { Header = "One" } } };
            tabs.Classes.Add(cls);
            Assert.Equal(FabTests.Shadow(level), Matrix.Part<Border>(Matrix.Show(tabs), "PART_TabBarHost").BoxShadow);
        }
    });

    /// <summary>
    /// Level 25 is MudBlazor's own last entry, not a 25th step up the ladder: a much softer
    /// shadow, "0 5px 5px -3px rgba(0,0,0,.06), 0 8px 10px 1px rgba(0,0,0,.042), 0 3px 14px 2px
    /// rgba(0,0,0,.036)".
    /// </summary>
    [Fact]
    public Task Level25IsMudBlazorsSoftShadow() => UiThread.RunAsync(() =>
    {
        Assert.Equal(BoxShadows.Parse("0 5 5 -3 #0F000000, 0 8 10 1 #0B000000, 0 3 14 2 #09000000"), FabTests.Shadow(25));
    });

    /// <summary>
    /// Typo.button: 14px, 500, a 24.5px line (1.75 of 14, Typography.cs, as a button's own label), 0.4px
    /// tracking. Uppercase is Avalonia's to lack.
    /// </summary>
    [Fact]
    public Task TheButtonTypeClass() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Label" };
        text.Classes.Add("button");
        Matrix.Show(text);

        Assert.Equal(14, text.FontSize);
        Assert.Equal(24.5, text.LineHeight);
        Assert.Equal(0.4, text.LetterSpacing);
        Assert.Equal(FontWeight.Medium, text.FontWeight);
    });
    /// <summary>The panel the content grows in; internal to the theme, so read through its properties.</summary>
    private static Decorator Collapse(Expander expander) => Matrix.Part<Decorator>(expander, "PART_Collapse");

    private static AvaloniaProperty Property(AvaloniaObject owner, string name) =>
        AvaloniaPropertyRegistry.Instance.FindRegistered(owner, name)!;

    private static double Openness(Decorator collapse) => (double)collapse.GetValue(Property(collapse, "Openness"))!;

    /// <summary>
    /// MudCollapse: `grid-template-rows` 0fr to 1fr over `300ms ease-in-out`. The content grows
    /// into view, as a height the panel eases between nothing and all of it, not a cross-fade.
    /// </summary>
    [Fact]
    public Task OpeningGrowsTheContentOver300MsEaseInOut() => UiThread.RunAsync(() =>
    {
        var expander = Matrix.Show(new Expander { Header = "Panel", Content = "Content", Width = 320 });
        var collapse = Collapse(expander);

        Assert.Null(expander.ContentTransition);
        Assert.Equal(0d, Openness(collapse));
        Assert.False(collapse.Child!.IsVisible);

        expander.IsExpanded = true;
        Assert.Equal(1d, collapse.GetBaseValue(Property(collapse, "Openness")));
        Assert.True(collapse.Child!.IsVisible);

        var grow = Assert.IsType<Avalonia.Animation.DoubleTransition>(Assert.Single(collapse.Transitions!));
        Assert.Equal(Property(collapse, "Openness"), grow.Property);
        Assert.Equal(TimeSpan.FromMilliseconds(300), grow.Duration);
        var easing = Assert.IsType<Avalonia.Animation.Easings.SplineEasing>(grow.Easing);
        Assert.Equal((0.42, 0d, 0.58, 1d), (easing.X1, easing.Y1, easing.X2, easing.Y2));
    });

    /// <summary>A panel that starts open is open on its first frame: nothing grows on load.</summary>
    [Fact]
    public Task APanelThatStartsOpenDoesNotGrowOnLoad() => UiThread.RunAsync(() =>
    {
        var expander = Matrix.Show(new Expander { Header = "Open", Content = new Border { Height = 80 }, IsExpanded = true, Width = 300 });
        var collapse = Collapse(expander);

        Assert.Equal(1d, Openness(collapse));
        Assert.Equal(80 + expander.Padding.Top + expander.Padding.Bottom, collapse.Bounds.Height, 0.5);
    });

    /// <summary>
    /// Part way, the panel is that share of the content's height, the content at its own size and
    /// clipped; a panel that opens upwards shows the content's far end first. Closed, the content
    /// is hidden, so Tab does not reach it.
    /// </summary>
    [Fact]
    public Task PartWayThePanelShowsThatShareOfTheContent() => UiThread.RunAsync(() =>
    {
        var content = new Border { Height = 100 };
        var expander = Matrix.Show(new Expander { Header = "Panel", Content = content, IsExpanded = true, Width = 300, Padding = new Thickness(0) });
        var collapse = Collapse(expander);
        collapse.Transitions = null;

        collapse.SetValue(Property(collapse, "Openness"), 0.5);
        expander.UpdateLayout();
        Assert.Equal(50, collapse.Bounds.Height, 0.5);
        Assert.Equal(100, content.Bounds.Height, 0.5);
        Assert.True(collapse.ClipToBounds);
        Assert.Equal(0, content.TranslatePoint(default, collapse)!.Value.Y, 0.5);

        collapse.SetValue(Property(collapse, "FromEnd"), true);
        expander.UpdateLayout();
        Assert.Equal(-50, content.TranslatePoint(default, collapse)!.Value.Y, 0.5);

        expander.IsExpanded = false;
        Assert.Equal(0d, Openness(collapse));
        Assert.False(content.IsEffectivelyVisible);
    });
}
