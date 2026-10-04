using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The last MudBlazor options that are looks rather than behaviour: MudTabs' Position, Elevation
/// and SliderColor, MudProgressLinear's Striped, and the spacing MudButton and MudChip give their
/// icons.
/// </summary>
public class RemainingOptionsTests
{
    private static T Named<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(c => c.Name == name);

    private static TabControl Tabs(Dock placement, params string[] classes)
    {
        var tabs = new TabControl
        {
            Width = 560,
            Height = 300,
            TabStripPlacement = placement,
        };
        tabs.Classes.AddRange(classes);
        tabs.Items.Add(new TabItem { Header = "One", Content = "First" });
        tabs.Items.Add(new TabItem { Header = "Two", Content = "Second" });
        tabs.SelectedIndex = 0;
        return Matrix.Show(tabs);
    }

    private static TabItem Item(TabControl tabs, int index) => (TabItem)tabs.Items[index]!;

    // --------------------------------------------------------------------------------------
    // MudTabs.Position
    // --------------------------------------------------------------------------------------

    [Theory]
    [InlineData(Dock.Top, "0,0,0,1")]
    [InlineData(Dock.Bottom, "0,1,0,0")]
    [InlineData(Dock.Left, "0,0,1,0")]
    [InlineData(Dock.Right, "1,0,0,0")]
    public Task TheBarDocksToItsEdgeWithItsRuleFacingTheContent(Dock placement, string rule) => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(placement, "border");

        Assert.Equal(placement, DockPanel.GetDock(Named<Border>(tabs, "PART_TabBarHost")));
        Assert.Equal(Thickness.Parse(rule), Named<Border>(tabs, "PART_TabBar").BorderThickness);
    });

    [Fact]
    public Task BottomTabsCarryTheirIndicatorOnTop() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(Dock.Bottom);

        Assert.Equal(VerticalAlignment.Top, Named<Border>(Item(tabs, 0), "PART_Indicator").VerticalAlignment);
    });

    /// <summary>Left and Right stack the tabs, and the indicator becomes a 2px column on the content side.</summary>
    [Theory]
    [InlineData(Dock.Left, HorizontalAlignment.Right)]
    [InlineData(Dock.Right, HorizontalAlignment.Left)]
    public Task SideTabsStackWithAVerticalIndicator(Dock placement, HorizontalAlignment side) => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(placement);
        var first = Item(tabs, 0);
        var second = Item(tabs, 1);
        var indicator = Named<Border>(first, "PART_Indicator");

        Assert.True(second.TranslatePoint(default, tabs)!.Value.Y > first.TranslatePoint(default, tabs)!.Value.Y);
        Assert.Equal(2, indicator.Bounds.Width);
        Assert.Equal(first.Bounds.Height, indicator.Bounds.Height, 1);
        Assert.Equal(side, indicator.HorizontalAlignment);
    });

    // --------------------------------------------------------------------------------------
    // MudTabs.Elevation and SliderColor
    // --------------------------------------------------------------------------------------

    /// <summary>The shadow is on the host, so `rounded`, which clips the bar, cannot clip it away.</summary>
    [Fact]
    public Task ElevationShadowsTheBarFromOutsideItsClip() => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(Dock.Top, "elevation4", "rounded");
        var host = Named<Border>(tabs, "PART_TabBarHost");

        Assert.True(Application.Current!.TryFindResource("FiliElevation4", Avalonia.Styling.ThemeVariant.Light, out var shadow));
        Assert.Equal((BoxShadows)shadow!, host.BoxShadow);
        Assert.False(host.ClipToBounds);
        Assert.Equal(new CornerRadius(4), host.CornerRadius);
        Assert.True(Named<Border>(tabs, "PART_TabBar").ClipToBounds);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task SliderColourPaintsTheIndicatorOverABarColour(string colour) => UiThread.RunAsync(() =>
    {
        var tabs = Tabs(Dock.Top, "primary", $"slider-{colour.ToLowerInvariant()}");

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Named<Border>(Item(tabs, 0), "PART_Indicator").Background));
        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(Item(tabs, 0).Foreground));
    });

    [Fact]
    public Task ATabStripTakesTheSliderColour() => UiThread.RunAsync(() =>
    {
        var strip = new TabStrip { Width = 560, Classes = { "slider-success" } };
        strip.Items.Add(new TabStripItem { Content = "One" });
        strip.Items.Add(new TabStripItem { Content = "Two" });
        strip.SelectedIndex = 0;
        Matrix.Show(strip);

        Assert.Equal(Matrix.Token("FiliSuccessColor"), Matrix.Colour(Named<Border>((TabStripItem)strip.Items[0]!, "PART_Indicator").Background));
    });

    // --------------------------------------------------------------------------------------
    // MudProgressLinear.Striped
    // --------------------------------------------------------------------------------------

    [Fact]
    public Task StripedLaysBandsOverTheBar() => UiThread.RunAsync(() =>
    {
        var plain = Matrix.Show(new ProgressBar { Width = 200, Value = 60, Classes = { "large" } });
        var striped = Matrix.Show(new ProgressBar { Width = 200, Value = 60, Classes = { "large", "striped" } });
        var stripes = Named<Panel>(striped, "PART_Stripes");

        Assert.False(Named<Panel>(plain, "PART_Stripes").IsVisible);
        Assert.True(stripes.IsVisible);
        Assert.True(Named<Border>(striped, "PART_Indicator").ClipToBounds, "The bands stop where the bar does.");
        Assert.IsType<DrawingBrush>(stripes.Background);
    });

    // --------------------------------------------------------------------------------------
    // StartIcon / EndIcon
    // --------------------------------------------------------------------------------------

    private static (PathIcon Start, PathIcon End) Icons(ContentControl button, params string[] classes)
    {
        var start = new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        var end = new PathIcon { Classes = { "end-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        button.Classes.AddRange(classes);
        button.Content = new StackPanel { Orientation = Orientation.Horizontal, Children = { start, new TextBlock { Text = "Label" }, end } };
        Matrix.Show(button);
        return (start, end);
    }

    [Theory]
    [InlineData(null, 20, -4)]
    [InlineData("small", 18, -2)]
    [InlineData("large", 22, -4)]
    public Task ButtonIconsAreSizedAndSpacedAsMudButtons(string? size, double px, double outset) => UiThread.RunAsync(() =>
    {
        var (start, end) = Icons(new Button(), size is null ? ["filled", "primary"] : ["filled", "primary", size]);

        Assert.Equal(px, start.Width);
        Assert.Equal(new Thickness(outset, 0, 8, 0), start.Margin);
        Assert.Equal(new Thickness(8, 0, outset, 0), end.Margin);
    });

    [Fact]
    public Task ChipIconsAreSpacedAsMudChips() => UiThread.RunAsync(() =>
    {
        var (start, end) = Icons(new Button(), "chip");
        Assert.Equal(20, start.Width);
        Assert.Equal(new Thickness(-4, 0, 4, 0), start.Margin);
        Assert.Equal(18, end.Width);
        Assert.Equal(new Thickness(6, 0, -4, 0), end.Margin);

        var (large, _) = Icons(new ToggleButton(), "chip", "large");
        Assert.Equal(24, large.Width);
        Assert.Equal(new Thickness(-6, 0, 6, 0), large.Margin);
    });
}
