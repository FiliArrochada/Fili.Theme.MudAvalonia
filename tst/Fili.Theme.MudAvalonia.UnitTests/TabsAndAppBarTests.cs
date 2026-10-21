using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudTabs' Color, Border, Outlined, Rounded and Centered on TabControl and TabStrip, and MudAppBar's
/// Dense, Color and Gutters on <c>Border.appbar</c>. Values from _tabs.scss, _appbar.scss and
/// _toolbar.scss, defaults from MudTabs.razor.cs and MudAppBar.razor.cs.
/// </summary>
public class TabsAndAppBarTests
{
    private static T Named<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(c => c.Name == name);

    private static TabControl Tabs(params string[] classes)
    {
        var tabs = new TabControl { Width = 560 };
        tabs.Classes.AddRange(classes);
        tabs.Items.Add(new TabItem { Header = "One", Content = "First" });
        tabs.Items.Add(new TabItem { Header = "Two", Content = "Second" });
        tabs.Items.Add(new TabItem { Header = "Off", IsEnabled = false });
        tabs.SelectedIndex = 0;
        return Matrix.Show(tabs);
    }

    private static TabStrip Strip(params string[] classes)
    {
        var strip = new TabStrip { Width = 560 };
        strip.Classes.AddRange(classes);
        strip.Items.Add(new TabStripItem { Content = "One" });
        strip.Items.Add(new TabStripItem { Content = "Two" });
        strip.SelectedIndex = 0;
        return Matrix.Show(strip);
    }

    private static TabItem Item(TabControl tabs, int index) => (TabItem)tabs.Items[index]!;

    // --------------------------------------------------------------------------------------
    // The sliding indicator
    // --------------------------------------------------------------------------------------

    /// <summary>
    /// Where an indicator is drawn, its slide included, relative to its bar. Read straight after a
    /// selection, before the dispatcher runs: the slide moves on in real time from there.
    /// </summary>
    private static Point Drawn(Control bar, Control item) =>
        Named<Border>(item, "PART_Indicator").TranslatePoint(default, bar)!.Value;

    /// <summary>
    /// Where the slide ends: the indicator's resting transform, under the transition. The headless
    /// session never advances animation time, so the slide itself is never seen here; that it
    /// starts at the old tab, comes to rest at its own and is declared with MudBlazor's timing is
    /// what can be asserted.
    /// </summary>
    private static void AssertSlidesHome(Control item)
    {
        var indicator = Named<Border>(item, "PART_Indicator");
        var rest = Assert.IsAssignableFrom<ITransform>(indicator.GetBaseValue(Visual.RenderTransformProperty).Value);
        Assert.True(rest.Value.IsIdentity, "The indicator should come to rest under its own tab.");

        var slide = Assert.IsType<TransformOperationsTransition>(Assert.Single(indicator.Transitions!));
        Assert.Equal(Visual.RenderTransformProperty, slide.Property);
        Assert.Equal(TimeSpan.FromMilliseconds(300), slide.Duration);
        var easing = Assert.IsType<SplineEasing>(slide.Easing);
        Assert.Equal((0.64, 0.09, 0.08, 1d), (easing.X1, easing.Y1, easing.X2, easing.Y2));
    }

    /// <summary>
    /// `.mud-tab-slider`: selecting another tab moves the indicator over from the old one, by .3s
    /// cubic-bezier(.64,.09,.08,1), rather than fading one out and the other in. The old tab's
    /// indicator is gone at once, and the new one is its own tab's width from the start.
    /// </summary>
    [Fact]
    public Task TheIndicatorSlidesToTheSelectedTab() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs();
        var start = Drawn(tabs, Item(tabs, 0));

        tabs.SelectedIndex = 1;
        Assert.Equal(start, Drawn(tabs, Item(tabs, 1)));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, Named<Border>(Item(tabs, 0), "PART_Indicator").Opacity);
        Assert.Equal(Item(tabs, 1).Bounds.Width, Named<Border>(Item(tabs, 1), "PART_Indicator").Bounds.Width);
        Assert.False(Item(tabs, 1).ClipToBounds);
        AssertSlidesHome(Item(tabs, 1));
    });

    /// <summary>A strip's indicator slides the same way.</summary>
    [Fact]
    public Task AStripsIndicatorSlides() => UiThread.RunAsync(() =>
    {
        var strip = Strip();
        var first = (TabStripItem)strip.Items[0]!;
        var second = (TabStripItem)strip.Items[1]!;
        var left = Drawn(strip, first);

        strip.SelectedIndex = 1;
        Assert.Equal(left, Drawn(strip, second));
        Dispatcher.UIThread.RunJobs();
        Assert.False(second.ClipToBounds);
        AssertSlidesHome(second);
    });

    /// <summary>
    /// SliderAnimation="false" is `no-animation`: the new tab's indicator is under it from the
    /// first frame, with nothing to slide.
    /// </summary>
    [Fact]
    public Task NoAnimationPutsTheIndicatorStraightUnderTheTab() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs("no-animation");
        var strip = Strip("no-animation");
        var second = (TabStripItem)strip.Items[1]!;
        var tabHome = Named<Border>(Item(tabs, 1), "PART_Indicator").Bounds.Position;
        var stripHome = Named<Border>(second, "PART_Indicator").Bounds.Position;

        tabs.SelectedIndex = 1;
        strip.SelectedIndex = 1;

        // Read before the dispatcher runs, where a slide would still be at the old tab.
        Assert.Equal(Item(tabs, 1).TranslatePoint(tabHome, tabs), Drawn(tabs, Item(tabs, 1)));
        Assert.Equal(second.TranslatePoint(stripHome, strip), Drawn(strip, second));
    });

    /// <summary>
    /// A slide starts wherever the old indicator is drawn, so one interrupted mid-way carries on from
    /// there; and leftwards works like rightwards.
    /// </summary>
    [Fact]
    public Task ASlideStartsFromWhereTheLastOneHadGot() => UiThread.RunAsync(() =>
    {
        var strip = Strip();
        var first = (TabStripItem)strip.Items[0]!;
        var second = (TabStripItem)strip.Items[1]!;
        var left = Drawn(strip, first);

        strip.SelectedIndex = 1;
        var midway = Drawn(strip, second);
        strip.SelectedIndex = 0;
        Assert.Equal(midway, Drawn(strip, first));
        Dispatcher.UIThread.RunJobs();
        AssertSlidesHome(first);

        var settled = new TabStrip { Width = 560 };
        settled.Items.Add(new TabStripItem { Content = "One" });
        settled.Items.Add(new TabStripItem { Content = "Two" });
        settled.SelectedIndex = 1;
        Matrix.Show(settled);
        var from = Drawn(settled, (TabStripItem)settled.Items[1]!);
        Assert.True(from.X > left.X);

        settled.SelectedIndex = 0;
        Assert.Equal(from, Drawn(settled, (TabStripItem)settled.Items[0]!));
    });

    /// <summary>A vertical bar slides on the other axis, from the indicators' own positions.</summary>
    [Fact]
    public Task AVerticalBarSlidesDown() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs();
        tabs.TabStripPlacement = Dock.Left;
        Dispatcher.UIThread.RunJobs();
        var top = Drawn(tabs, Item(tabs, 0));

        tabs.SelectedIndex = 1;
        Assert.Equal(top, Drawn(tabs, Item(tabs, 1)));
        Assert.True(Named<Border>(Item(tabs, 1), "PART_Indicator").TranslatePoint(default, Item(tabs, 1))!.Value.Y < 0,
            "The indicator should start above its own tab, at the one before it.");
        Dispatcher.UIThread.RunJobs();
        AssertSlidesHome(Item(tabs, 1));
    });

    /// <summary>Nothing slides on first show: a selection made before then has no tab to come from.</summary>
    [Fact]
    public Task TheFirstSelectionDoesNotSlide() => UiThread.RunAsync(() =>
    {
        var tabs = new TabControl { Width = 560 };
        tabs.Items.Add(new TabItem { Header = "One" });
        tabs.Items.Add(new TabItem { Header = "Two" });
        tabs.SelectedIndex = 1;
        Matrix.Show(tabs);

        Assert.Equal(Item(tabs, 1).Bounds.X, Drawn(tabs, Item(tabs, 1)).X);
        Assert.Null(Named<Border>(Item(tabs, 1), "PART_Indicator").RenderTransform);
    });

    // --------------------------------------------------------------------------------------
    // MudTabs defaults
    // --------------------------------------------------------------------------------------

    /// <summary>
    /// MudTabs' defaults, each of which this theme used to differ on: a SURFACE bar with no rule
    /// (Border defaults to false), tabs that inherit text-primary rather than text-secondary, and
    /// MinimumTabWidth's 160px.
    /// </summary>
    [Fact]
    public Task AnUnclassedTabBarIsSurfaceWithNoRule() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs();
        var bar = Named<Border>(tabs, "PART_TabBar");

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(bar.Background));
        Assert.Equal(default, bar.BorderThickness);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Item(tabs, 0).Foreground));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(Item(tabs, 1).Foreground));
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Item(tabs, 2).Foreground));
        Assert.True(Item(tabs, 1).Bounds.Width >= 160);
    });

    [Fact]
    public Task TabHoverIsActionDefaultAndPrimaryWhenActive() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs();

        Matrix.Hover(Item(tabs, 1));
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Named<Border>(Item(tabs, 1), "PART_StateLayer").Background));

        Matrix.Hover(Item(tabs, 0));
        Assert.Equal(Matrix.Token("FiliPrimaryHoverColor"), Matrix.Colour(Named<Border>(Item(tabs, 0), "PART_StateLayer").Background));
    });

    [Fact]
    public Task BorderOutlinedAndRoundedShapeTheBar() => UiThread.RunAsync(() =>
    {
        var border = Named<Border>(Tabs("border"), "PART_TabBar");
        Assert.Equal(new Thickness(0, 0, 0, 1), border.BorderThickness);
        Assert.Equal(Matrix.Token("FiliLinesDefaultColor"), Matrix.Colour(border.BorderBrush));

        Assert.Equal(new Thickness(1), Named<Border>(Tabs("outlined"), "PART_TabBar").BorderThickness);

        var rounded = Named<Border>(Strip("rounded"), "PART_TabBar");
        Assert.Equal(new CornerRadius(4), rounded.CornerRadius);
        Assert.True(rounded.ClipToBounds);
    });

    [Fact]
    public Task CenteredCentresTheTabRow() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs("centered");
        var row = Named<ItemsPresenter>(tabs, "PART_ItemsPresenter");

        Assert.Equal(HorizontalAlignment.Center, row.HorizontalAlignment);
        var left = row.TranslatePoint(default, tabs)!.Value.X;
        Assert.Equal(tabs.Bounds.Width - row.Bounds.Width, left * 2, 1);
    });

    [Fact]
    public Task HideSliderHidesTheIndicator() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs("hide-indicator", "primary");
        var strip = Strip("hide-indicator");

        Assert.False(Named<Border>(Item(tabs, 0), "PART_Indicator").IsVisible);
        Assert.False(Named<Border>((TabStripItem)strip.Items[0]!, "PART_Indicator").IsVisible);
        Assert.True(Named<Border>(Item(Tabs(), 0), "PART_Indicator").IsVisible);
    });

    // --------------------------------------------------------------------------------------
    // MudTabs.Color
    // --------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColouredBarCarriesItsContrastText(string colour) => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(colour.ToLowerInvariant());
        var contrast = Matrix.Token($"Fili{colour}ContrastTextColor");

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Named<Border>(tabs, "PART_TabBar").Background));
        Assert.Equal(contrast, Matrix.Colour(Item(tabs, 0).Foreground));
        Assert.Equal(contrast, Matrix.Colour(Item(tabs, 1).Foreground));
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Item(tabs, 2).Foreground));
        Assert.Equal(contrast, Matrix.Colour(Named<Border>(Item(tabs, 0), "PART_Indicator").Background));

        Matrix.Hover(Item(tabs, 0));
        Assert.Equal(Matrix.Token($"Fili{colour}LightenColor"), Matrix.Colour(Named<Border>(Item(tabs, 0), "PART_StateLayer").Background));
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task ATabStripTakesTheSameColours(string colour) => UiThread.RunAsync(() =>
    {
        var strip = Strip(colour.ToLowerInvariant());
        var first = (TabStripItem)strip.Items[0]!;

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Named<Border>(strip, "PART_TabBar").Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(first.Foreground));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(((TabStripItem)strip.Items[1]!).Foreground));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(Named<Border>(first, "PART_Indicator").Background));
    });

    /// <summary>
    /// The colour reaches the bar's OWN tabs only. A child selector, because a descendant one
    /// would also repaint the tabs of a TabControl nested in the content.
    /// </summary>
    [Fact]
    public Task ABarColourDoesNotReachNestedTabs() => UiThread.RunAsync(() =>
    {
        var inner = new TabControl();
        inner.Items.Add(new TabItem { Header = "Inner" });
        inner.Items.Add(new TabItem { Header = "Other" });
        inner.SelectedIndex = 0;

        var outer = new TabControl { Width = 560, Classes = { "primary" } };
        outer.Items.Add(new TabItem { Header = "Outer", Content = inner });
        outer.SelectedIndex = 0;
        Matrix.Show(outer);

        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(((TabItem)inner.Items[1]!).Foreground));
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(Named<Border>(inner, "PART_TabBar").Background));
    });

    // --------------------------------------------------------------------------------------
    // MudAppBar
    // --------------------------------------------------------------------------------------

    [Fact]
    public Task AnAppBarHasGuttersAndADenseHeight() => UiThread.RunAsync(() =>
    {
        var bar = Matrix.Show(new Border { Classes = { "appbar" }, Width = 400, Child = new TextBlock { Text = "Title" } });
        var dense = Matrix.Show(new Border { Classes = { "appbar", "dense" }, Width = 400 });

        Assert.Equal(64, bar.Bounds.Height);
        Assert.Equal(new Thickness(24, 0), bar.Padding);
        Assert.Equal(48, dense.Bounds.Height);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AnAppBarColourIsTheGroundAndItsContrastText(string colour) => UiThread.RunAsync(() =>
    {
        var title = new TextBlock { Text = "Title" };
        var bar = Matrix.Show(new Border { Classes = { "appbar", colour.ToLowerInvariant() }, Width = 400, Child = title });

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(bar.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(title.Foreground));
    });
}
