using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Path = Avalonia.Controls.Shapes.Path;
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
        var tabs = Tabs(Dock.Top, "primary", $"indicator-{colour.ToLowerInvariant()}");

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Named<Border>(Item(tabs, 0), "PART_Indicator").Background));
        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(Item(tabs, 0).Foreground));
    });

    [Fact]
    public Task ATabStripTakesTheSliderColour() => UiThread.RunAsync(() =>
    {
        var strip = new TabStrip { Width = 560, Classes = { "indicator-success" } };
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

    /// <summary>
    /// ShowProgressText draws the whole value over the bar, whatever its thickness, as
    /// .mud-progress-linear-content does. Arranged in the bar's own 4px, and clipped there by the
    /// bar, the text was a 4px slice of "40%".
    /// </summary>
    [Theory]
    [InlineData(null, Orientation.Horizontal)]
    [InlineData("large", Orientation.Horizontal)]
    [InlineData(null, Orientation.Vertical)]
    public Task TheProgressTextIsWholeOverAThinBar(string? size, Orientation orientation) => UiThread.RunAsync(() =>
    {
        var bar = new ProgressBar { Value = 40, ShowProgressText = true, Orientation = orientation };
        if (orientation == Orientation.Horizontal) bar.Width = 300; else bar.Height = 200;
        if (size is not null) bar.Classes.Add(size);
        Matrix.Show(bar);

        var text = bar.GetVisualDescendants().OfType<TextBlock>().Single();
        var host = Named<LayoutTransformControl>(bar, "PART_LayoutTransformControl");

        Assert.False(bar.ClipToBounds);
        Assert.Equal(text.DesiredSize.Width, text.Bounds.Width, 1);
        Assert.Equal(text.DesiredSize.Height, text.Bounds.Height, 1);
        Assert.True(host.Bounds.Height >= text.Bounds.Height || host.Bounds.Width >= text.Bounds.Height,
            $"The text's host is {host.Bounds.Size}, the text {text.Bounds.Size}.");

        var centre = host.TranslatePoint(new Point(host.Bounds.Width / 2, host.Bounds.Height / 2), bar)!.Value;
        Assert.Equal(bar.Bounds.Width / 2, centre.X, 0.5); // layout rounding to whole pixels
        Assert.Equal(bar.Bounds.Height / 2, centre.Y, 0.5);
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
    /// <summary>
    /// A split or drop-down button's content sits inside plain Buttons in its template, which set
    /// no icon values of their own, so the outer button's size reaches its icons; a toggle item
    /// sizes them as a button does.
    /// </summary>
    [Theory]
    [InlineData("SplitButton", null, 20, -4)]
    [InlineData("SplitButton", "small", 18, -2)]
    [InlineData("DropDownButton", "large", 22, -4)]
    [InlineData("ToggleButton", "small", 18, -2)]
    public Task EveryButtonSizesItsIconsAsMudButton(string type, string? size, double px, double outset) => UiThread.RunAsync(() =>
    {
        ContentControl host = type switch
        {
            "SplitButton" => new SplitButton(),
            "DropDownButton" => new DropDownButton(),
            _ => new ToggleButton(),
        };
        var (start, end) = Icons(host, size is null ? [] : [size]);

        Assert.Equal(px, start.Width);
        Assert.Equal(px, end.Height);
        Assert.Equal(new Thickness(outset, 0, 8, 0), start.Margin);
        Assert.Equal(new Thickness(8, 0, outset, 0), end.Margin);
    });

    /// <summary>A FAB's icons are 24px at any size, 8px from the label, 4px on a small one; a chip's end icon stays 18px.</summary>
    [Fact]
    public Task FabAndChipIconsKeepTheirOwnSizes() => UiThread.RunAsync(() =>
    {
        var (start, end) = Icons(new Button(), "fab", "extended", "small");
        Assert.Equal(24, start.Width);
        Assert.Equal(new Thickness(0, 0, 4, 0), start.Margin);
        Assert.Equal(new Thickness(4, 0, 0, 0), end.Margin);

        var (_, chipEnd) = Icons(new Button(), "chip", "large");
        Assert.Equal(18, chipEnd.Width);
        Assert.Equal(new Thickness(6, 0, -4, 0), chipEnd.Margin);
    });

    /// <summary>MudLink's icons: `font-size: inherit`, the link's own size, and 4px from the text.</summary>
    [Fact]
    public Task LinkIconsAreTheLinksSizeAndSpacedAsMudLinks() => UiThread.RunAsync(() =>
    {
        var link = new HyperlinkButton { FontSize = 18 };
        var (start, end) = Icons(link);

        Assert.Equal(18, start.Width);
        Assert.Equal(18, end.Height);
        Assert.Equal(new Thickness(0, 0, 4, 0), start.Margin);
        Assert.Equal(new Thickness(4, 0, 0, 0), end.Margin);
    });

    /// <summary>MudTabPanel's Icon beside its Text: 24px, `.mud-tab-icon-text` 8px from the text.</summary>
    [Fact]
    public Task ATabsIconIsSpacedAsMudTabs() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        var header = new StackPanel { Orientation = Orientation.Horizontal, Children = { icon, new TextBlock { Text = "Tab" } } };
        var tabs = new TabControl();
        tabs.Items.Add(new TabItem { Header = header, Content = "Content" });
        Matrix.Show(tabs);

        var stripIcon = new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        var strip = new TabStrip();
        strip.Items.Add(new TabStripItem { Content = new StackPanel { Orientation = Orientation.Horizontal, Children = { stripIcon, new TextBlock { Text = "Tab" } } } });
        Matrix.Show(strip);

        foreach (var i in new[] { icon, stripIcon })
        {
            Assert.Equal(24, i.Width);
            Assert.Equal(new Thickness(0, 0, 8, 0), i.Margin);
        }
    });

    /// <summary>MudInput's adornments: a 24px icon, 8px from the text on either side.</summary>
    [Fact]
    public Task AnAdornmentIsSpacedAsMudInputs() => UiThread.RunAsync(() =>
    {
        var start = new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        var end = new PathIcon { Classes = { "end-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        var box = Matrix.Show(new TextBox { Width = 240, Text = "Text", InnerLeftContent = start, InnerRightContent = end });
        var text = Matrix.Part<Control>(box, "PART_TextPresenter");
        double Left(Visual v) => v.TranslatePoint(default, box)!.Value.X;

        Assert.Equal(24, start.Width);
        Assert.Equal(24, end.Height);
        Assert.Equal(Left(start) + 24 + 8, Left(text), 0.5);
        Assert.Equal(Left(text) + text.Bounds.Width + 8, Left(end), 0.5);
    });

    /// <summary>
    /// A text adornment, a unit such as "kg", is centred on the text line it is read with, in
    /// every variant; an icon is centred on a filled or outlined box, as `.mud-input` centres it.
    /// Centred on the box, the unit sat above the value.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("dense")]
    [InlineData("filled")]
    [InlineData("filled dense")]
    [InlineData("outlined")]
    [InlineData("outlined dense")]
    public Task ATextAdornmentStaysOnTheTextLine(string? classes) => UiThread.RunAsync(() =>
    {
        var start = new TextBlock { Text = "$", VerticalAlignment = VerticalAlignment.Center };
        var end = new TextBlock { Text = "kg", VerticalAlignment = VerticalAlignment.Center };
        var box = new TextBox { Width = 240, Text = "72", InnerLeftContent = start, InnerRightContent = end };
        if (classes is not null) box.Classes.AddRange(classes.Split(' '));
        Matrix.Show(box);
        var text = Matrix.Part<Control>(box, "PART_TextPresenter");
        double Middle(Visual v) => v.TranslatePoint(new Point(0, v.Bounds.Height / 2), box)!.Value.Y;

        Assert.Contains(":start-text-adornment", box.Classes);
        Assert.Contains(":end-text-adornment", box.Classes);
        Assert.Equal(Middle(text), Middle(start), 1);
        Assert.Equal(Middle(text), Middle(end), 1);

        box.InnerRightContent = new PathIcon { Classes = { "end-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") };
        Dispatcher.UIThread.RunJobs();
        Assert.DoesNotContain(":end-text-adornment", box.Classes);
    });

    /// <summary>With no adornment the presenters take no gap: the text runs to the padding.</summary>
    [Fact]
    public Task NoAdornmentTakesNoGap() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 240, Text = "Text" });

        Assert.Equal(default, Matrix.Part<ContentPresenter>(box, "PART_InnerLeftContent").Margin);
        Assert.Equal(default, Matrix.Part<ContentPresenter>(box, "PART_InnerRightContent").Margin);
    });

    /// <summary>A chip's avatar, _chip.scss's `.mud-avatar` at each chip size.</summary>
    [Theory]
    [InlineData("small", 18, -4, 4, 10)]
    [InlineData(null, 24, -8, 8, 12)]
    [InlineData("large", 32, -12, 8, 16)]
    public Task AChipsAvatarIsSizedAsMudChips(string? size, double px, double outset, double gap, double text) => UiThread.RunAsync(() =>
    {
        var avatar = new Border { Classes = { "avatar" }, Child = new TextBlock { Text = "A" } };
        var chip = new Button { Classes = { "chip" }, Content = new StackPanel { Orientation = Orientation.Horizontal, Children = { avatar, new TextBlock { Text = "Chip" } } } };
        if (size is not null)
        {
            chip.Classes.Add(size);
        }

        Matrix.Show(chip);

        Assert.Equal(px, avatar.Width);
        Assert.Equal(px, avatar.Height);
        Assert.Equal(new Thickness(outset, 0, gap, 0), avatar.Margin);
        Assert.Equal(text, avatar.GetValue(TextElement.FontSizeProperty));
    });

    /// <summary>An avatar outside a chip keeps MudAvatar's 40px.</summary>
    [Fact]
    public Task AnAvatarOutsideAChipKeepsItsSize() => UiThread.RunAsync(() =>
    {
        var avatar = Matrix.Show(new Border { Classes = { "avatar" } });

        Assert.Equal(40, avatar.Width);
        Assert.Equal(default, avatar.Margin);
    });

    /// <summary>MudIcon's Size defaults to Medium: a bare PathIcon is 24px, as `medium` is.</summary>
    [Fact]
    public Task ABareIconIsMudIconsMedium() => UiThread.RunAsync(() =>
    {
        var bare = Matrix.Show(new PathIcon { Data = Geometry.Parse("M0 0h2v2H0z") });
        var medium = Matrix.Show(new PathIcon { Classes = { "medium" }, Data = Geometry.Parse("M0 0h2v2H0z") });

        Assert.Equal(24, bare.Width);
        Assert.Equal(24, bare.Height);
        Assert.Equal(medium.Bounds.Size, bare.Bounds.Size);
    });

    /// <summary>An icon marked start-icon outside any host keeps its own size and gap.</summary>
    [Fact]
    public Task AnIconOutsideAHostKeepsItsOwnSize() => UiThread.RunAsync(() =>
    {
        var plain = Matrix.Show(new PathIcon { Data = Geometry.Parse("M0 0h2v2H0z") });
        var marked = Matrix.Show(new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") });

        Assert.Equal(plain.Width, marked.Width);
        Assert.Equal(plain.Height, marked.Height);
        Assert.Equal(default, marked.Margin);
    });

    /// <summary>
    /// MudChipSet's CheckMark (MudChip.razor): a selected chip shows Icons.Material.Filled.Check
    /// as a MudIcon at Size.Small with `.mud-chip-icon`'s gaps, the start icon's size and margin.
    /// </summary>
    [Theory]
    [InlineData("", 20, -4)]
    [InlineData("small", 18, -4)]
    [InlineData("large", 24, -6)]
    public Task ASelectedCheckMarkChipShowsItsCheck(string size, double side, double lead) => UiThread.RunAsync(() =>
    {
        var chip = new ToggleButton { Content = "Indie", Classes = { "chip", "check-mark" } };
        if (size.Length > 0)
        {
            chip.Classes.Add(size);
        }

        Matrix.Show(chip);
        var check = Named<Viewbox>(chip, "PART_CheckMark");
        Assert.False(check.IsEffectivelyVisible);

        chip.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(check.IsEffectivelyVisible);
        Assert.Equal(new Size(side, side), check.Bounds.Size);
        Assert.Equal(new Thickness(lead, 0, -lead, 0), check.Margin);

        var label = Named<ContentPresenter>(chip, "PART_ContentPresenter");
        Assert.True(check.TranslatePoint(default, chip)!.Value.X < label.TranslatePoint(default, chip)!.Value.X);

        var plain = new ToggleButton { Content = "Indie", IsChecked = true, Classes = { "chip" } };
        Matrix.Show(plain);
        Assert.False(Named<Viewbox>(plain, "PART_CheckMark").IsEffectivelyVisible);
    });

    /// <summary>`!string.IsNullOrEmpty(Icon) &amp;&amp; !ShowCheckMark`: the check takes the icon's place.</summary>
    [Fact]
    public Task TheCheckTakesTheStartIconsPlace() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" } };
        var chip = new ToggleButton
        {
            Classes = { "chip", "check-mark" },
            Content = new StackPanel { Orientation = Orientation.Horizontal, Children = { icon, new TextBlock { Text = "Indie" } } },
        };
        Matrix.Show(chip);
        Assert.True(icon.IsEffectivelyVisible);

        chip.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.False(icon.IsVisible);

        chip.IsChecked = false;
        Dispatcher.UIThread.RunJobs();
        Assert.True(icon.IsVisible);
    });

    /// <summary>
    /// MudToggleGroup's CheckMark (MudToggleItem.razor): a selected item shows its SelectedIcon,
    /// Icons.Material.Filled.Check, before its content, as `.mud-toggle-item-check-icon`: 20px
    /// with 6px each side, 18px with 4px small, 22px with 8px large (_togglegroup.scss).
    /// </summary>
    [Theory]
    [InlineData("", 20, 6)]
    [InlineData("small", 18, 4)]
    [InlineData("large", 22, 8)]
    public Task ASelectedCheckMarkToggleShowsItsCheck(string size, double side, double gap) => UiThread.RunAsync(() =>
    {
        var item = new ToggleButton { Content = "Bold", Classes = { "check-mark" } };
        var plain = new ToggleButton { Content = "Bold", IsChecked = true };
        if (size.Length > 0)
        {
            item.Classes.Add(size);
            plain.Classes.Add(size);
        }

        Matrix.Show(item);
        var check = Named<Viewbox>(item, "PART_CheckMark");
        Assert.False(check.IsEffectivelyVisible);

        item.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(check.IsEffectivelyVisible);
        Assert.Equal(new Size(side, side), check.Bounds.Size);
        Assert.Equal(new Thickness(gap, 0, gap, 0), check.Margin);

        var label = Named<ContentPresenter>(item, "PART_ContentPresenter");
        Assert.True(check.TranslatePoint(default, item)!.Value.X < label.TranslatePoint(default, item)!.Value.X);

        // The check sits on the item's line: it adds width, never height.
        Matrix.Show(plain);
        Assert.False(Named<Viewbox>(plain, "PART_CheckMark").IsEffectivelyVisible);
        Assert.Equal(plain.Bounds.Height, item.Bounds.Height);
        Assert.Equal(plain.Bounds.Width + side + 2 * gap, item.Bounds.Width, 1);
    });

    /// <summary>The check is MudIcon in the item's text colour, and MudIcon's Disabled when disabled.</summary>
    [Fact]
    public Task TheToggleCheckIsInTheItemsTextColour() => UiThread.RunAsync(() =>
    {
        var item = new ToggleButton { Content = "Bold", IsChecked = true, Classes = { "check-mark", "secondary" } };
        Matrix.Show(item);
        var glyph = Named<Path>(item, "PART_CheckGlyph");
        Assert.Same(item.Foreground, glyph.Fill);
        Assert.Same(item.FindResource("FiliCheckGeometry"), glyph.Data);

        item.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(item.FindResource("FiliActionDisabledBrush"), glyph.Fill);
    });
}
