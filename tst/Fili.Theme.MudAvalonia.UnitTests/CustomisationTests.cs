using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The last of MudBlazor's parameters: custom glyphs as resource keys, LabelPlacement, MudSwitch's
/// ThumbIcon, MudMenu's HideSubMenuArrow, MudSelect's Dense, `.mud-list-item-text`'s margin and a
/// mini drawer's nav.
/// </summary>
public class CustomisationTests
{
    // Parsed on first use, inside a test: a static initialiser runs whenever xunit first touches the
    // class, which can be before any test has started Avalonia, and a geometry needs its renderer.
    private static Geometry? _heart;

    private static Geometry Heart => _heart ??= Geometry.Parse("M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z");

    /// <summary>CheckedIcon and friends: every glyph is a DynamicResource, so a key set on the control wins.</summary>
    [Fact]
    public Task AGlyphKeyOnTheControlReplacesTheTheme() => UiThread.RunAsync(() =>
    {
        var box = new CheckBox { IsChecked = true, Content = "Favourite" };
        box.Resources["FiliCheckBoxCheckedGeometry"] = Heart;
        var plain = new CheckBox { IsChecked = true };
        Matrix.Show(new StackPanel { Children = { box, plain } });

        Assert.Same(Heart, Matrix.Part<Path>(box, "PART_Glyph").Data);
        Assert.NotSame(Heart, Matrix.Part<Path>(plain, "PART_Glyph").Data);
    });

    /// <summary>MudSelect's OpenIcon / CloseIcon: the two arrow keys, set on the select.</summary>
    [Fact]
    public Task ASelectsArrowsAreKeysToo() => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 200, ItemsSource = new[] { "PC" } };
        combo.Resources["FiliArrowDropDownGeometry"] = Heart;
        Matrix.Show(combo);

        Assert.Same(Heart, Matrix.Part<Path>(combo, "PART_Chevron").Data);
    });

    /// <summary>`.mud-menu-submenu-icon`: Filled.ArrowRight, 24px, action-default; HideSubMenuArrow is `hide-icon`.</summary>
    [Fact]
    public Task ASubmenusArrowIsArrowRightAndHideIconTakesItAway() => UiThread.RunAsync(() =>
    {
        var parent = new MenuItem { Header = "Export", Items = { new MenuItem { Header = "PDF" } } };
        var hidden = new MenuItem { Header = "Share", Classes = { "hide-icon" }, Items = { new MenuItem { Header = "Mail" } } };
        var top = new MenuItem { Header = "File", Items = { parent, hidden } };
        Matrix.Show(new Menu { Items = { top } });
        top.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();

        var arrow = Matrix.Part<Path>(parent, "PART_Chevron");
        Application.Current!.TryFindResource("FiliArrowRightGeometry", out var glyph);
        Assert.True(arrow.IsVisible);
        Assert.Same(glyph, arrow.Data);
        Assert.Equal(24, arrow.Width);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(arrow.Fill));
        Assert.False(Matrix.Part<Path>(hidden, "PART_Chevron").IsVisible);
    });

    /// <summary>
    /// MudTabs' scroll icons are ChevronLeft and ChevronRight drawn in a 24px box, not stretched to
    /// fill it as a PathIcon would; and as keys, PrevIcon and NextIcon.
    /// </summary>
    [Fact]
    public Task TheTabScrollChevronsAreDrawnAtTheirOwnSize() => UiThread.RunAsync(() =>
    {
        // Enough tabs to overflow 200px, so the buttons show and their content is laid out.
        var strip = new TabStrip { Width = 200 };
        for (var i = 0; i < 6; i++)
        {
            strip.Items.Add(new TabStripItem { Content = $"Tab {i}" });
        }

        strip.Resources["FiliChevronRightGeometry"] = Heart;
        Matrix.Show(strip);
        Dispatcher.UIThread.RunJobs();

        foreach (var name in new[] { "PART_PrevIcon", "PART_NextIcon" })
        {
            var icon = Matrix.Part<Path>(strip, name);
            Assert.Equal(Stretch.None, icon.Stretch);
            Assert.Equal(24, icon.Width);
        }

        Assert.Same(Heart, Matrix.Part<Path>(strip, "PART_NextIcon").Data);
    });

    /// <summary>`.mud-list-item-text`: 4px above and below the text, so a row is 48px, and 36 dense.</summary>
    [Fact]
    public Task AListRowIs48pxAnd36Dense() => UiThread.RunAsync(() =>
    {
        var list = Matrix.Show(new ListBox { Width = 200, ItemsSource = new[] { "One" } });
        var dense = Matrix.Show(new ListBox { Width = 200, ItemsSource = new[] { "One" }, Classes = { "dense" } });

        Assert.Equal(48, list.ContainerFromIndex(0)!.Bounds.Height);
        Assert.Equal(36, dense.ContainerFromIndex(0)!.Bounds.Height);
    });

    /// <summary>MudSelect's Dense on the options: `dense-items` on the select, or `dense` on one option.</summary>
    [Fact]
    public Task DenseItemsMakesEveryOptionDense() => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 200, ItemsSource = new[] { "PC", "Mac" }, Classes = { "dense-items" } };
        var plain = new ComboBox { Width = 200, ItemsSource = new[] { "PC" } };
        Matrix.Show(new StackPanel { Children = { combo, plain } });
        combo.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var option = Assert.IsType<ComboBoxItem>(combo.ContainerFromIndex(0));

        Assert.Equal(new Thickness(16, 4), option.Padding);
        Assert.Equal(14, option.FontSize);
        Assert.Equal(36, option.Bounds.Height);
        combo.IsDropDownOpen = false;

        plain.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var row = Assert.IsType<ComboBoxItem>(plain.ContainerFromIndex(0));
        Assert.Equal(48, row.Bounds.Height);
        row.Classes.Add("dense");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Thickness(16, 4), row.Padding);
        plain.IsDropDownOpen = false;
    });

    public static readonly TheoryData<string> Toggles = ["CheckBox", "RadioButton", "ToggleSwitch"];

    private static (Control Control, Control Box, Control Label) Placed(string type, string placement)
    {
        Control control = type switch
        {
            "CheckBox" => new CheckBox { Content = "Label" },
            "RadioButton" => new RadioButton { Content = "Label" },
            _ => new ToggleSwitch { Content = "Label" },
        };
        control.Classes.Add(placement);
        Matrix.Show(control);
        var box = Matrix.Part<Panel>(control, type == "ToggleSwitch" ? "PART_SwitchArea" : "PART_Box");
        return (control, box, Matrix.Part<ContentPresenter>(control, "PART_ContentPresenter"));
    }

    private static Rect Where(Control part, Control root) =>
        new(part.TranslatePoint(new Point(0, 0), root)!.Value, part.Bounds.Size);

    /// <summary>`.mud-input-content-placement-start`: row-reverse, the label first.</summary>
    [Theory]
    [MemberData(nameof(Toggles))]
    public Task LabelStartPutsTheLabelBeforeTheGlyph(string type) => UiThread.RunAsync(() =>
    {
        var (control, box, label) = Placed(type, "label-start");

        Assert.True(Where(label, control).Right <= Where(box, control).Left);
    });

    /// <summary>`-top` is column-reverse and `-bottom` column, the two centred on each other.</summary>
    [Theory]
    [MemberData(nameof(Toggles))]
    public Task LabelTopAndBottomStackThem(string type) => UiThread.RunAsync(() =>
    {
        var (top, topBox, topLabel) = Placed(type, "label-top");
        var (bottom, bottomBox, bottomLabel) = Placed(type, "label-bottom");

        Assert.True(Where(topLabel, top).Bottom <= Where(topBox, top).Top);
        Assert.True(Where(bottomBox, bottom).Bottom <= Where(bottomLabel, bottom).Top);
        Assert.Equal(Where(topBox, top).Center.X, Where(topLabel, top).Center.X, 1);
    });

    /// <summary>MudSwitch's ThumbIcon: a key on the switch, drawn 16px in the 20px thumb, text-secondary.</summary>
    [Fact]
    public Task AThumbIconIsAKeyOnTheSwitch() => UiThread.RunAsync(() =>
    {
        var toggle = new ToggleSwitch();
        toggle.Resources["FiliSwitchThumbIconGeometry"] = Heart;
        var plain = new ToggleSwitch();
        Matrix.Show(new StackPanel { Children = { toggle, plain } });
        var icon = Matrix.Part<Path>(toggle, "PART_ThumbIcon");

        Assert.Same(Heart, icon.Data);
        Assert.Equal(16, Assert.IsType<Viewbox>(icon.Parent).Bounds.Width);
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(icon.Fill));
        Assert.True(Matrix.Part<Path>(plain, "PART_ThumbIcon").Data!.Bounds.Width == 0);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task ThumbColourIsThumbIconColor(string colour) => UiThread.RunAsync(() =>
    {
        var toggle = Matrix.Show(new ToggleSwitch { Classes = { $"thumb-{colour.ToLowerInvariant()}" } });

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Matrix.Part<Path>(toggle, "PART_ThumbIcon").Fill));
    });

    /// <summary>`.mud-switch-disabled`: a gray-default thumb, the track's colour at 12%, a text-disabled label.</summary>
    [Fact]
    public Task ADisabledSwitchKeepsItsTrackAt12Percent() => UiThread.RunAsync(() =>
    {
        var toggle = Matrix.Show(new ToggleSwitch { IsChecked = true, IsEnabled = false, Classes = { "primary" }, Content = "Wi-Fi" });

        Assert.Equal(Matrix.Token("FiliGrayDefaultColor"), Matrix.Colour(Matrix.Part<Border>(toggle, "PART_Thumb").Background));
        Assert.Equal(0.12, Matrix.Part<Border>(toggle, "PART_Track").Opacity, 3);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Matrix.Part<Border>(toggle, "PART_Track").Background));
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(toggle.Foreground));
    });

    /// <summary>A closed mini drawer: the text beside a link's icon goes, and no row is indented.</summary>
    [Fact]
    public Task AMiniDrawerShowsOnlyTheIcons() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Inbox" };
        var nestedText = new TextBlock { Text = "Starred" };
        var nested = new TreeViewItem { Header = new StackPanel { Children = { new PathIcon { Classes = { "start-icon" } }, nestedText } } };
        var link = new TreeViewItem { Header = new StackPanel { Children = { new PathIcon { Classes = { "start-icon" } }, text } }, IsExpanded = true, Items = { nested } };
        var drawer = new SplitView
        {
            DisplayMode = SplitViewDisplayMode.CompactInline,
            IsPaneOpen = false,
            Width = 500,
            Height = 300,
            Pane = new TreeView { Classes = { "nav" }, Items = { link } },
        };
        Matrix.Show(drawer);

        Assert.False(text.IsVisible);
        Assert.Equal(0, Matrix.Part<Border>(nested, "PART_Indent").Width);

        // The icon's 12px goes with the text, so a row fits the 56px rail and nothing scrolls sideways.
        var scroller = ((TreeView)drawer.Pane!).GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.True(scroller.Extent.Width <= scroller.Viewport.Width, $"The rail scrolls: {scroller.Extent.Width} in {scroller.Viewport.Width}.");

        drawer.IsPaneOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(text.IsVisible);
        Assert.Equal(20, Matrix.Part<Border>(nested, "PART_Indent").Width);
    });

    /// <summary>
    /// CommonSnackbarOptions' icons: keys set on the WindowNotificationManager reach the cards it
    /// builds, because a template's root is the templated control's logical child.
    /// </summary>
    [Fact]
    public Task SnackbarGlyphKeysOnTheManagerReachItsCards() => UiThread.RunAsync(() =>
    {
        var host = Matrix.Show(new Border { Width = 400, Height = 200 });
        var manager = new WindowNotificationManager(TopLevel.GetTopLevel(host)!);
        manager.Resources["FiliInfoOutlinedGeometry"] = Heart;
        manager.Resources["FiliCloseGeometry"] = Heart;
        Dispatcher.UIThread.RunJobs();

        manager.Show(new Notification(null, "Saved", NotificationType.Information));
        Dispatcher.UIThread.RunJobs();

        var card = manager.GetVisualDescendants().OfType<NotificationCard>().Single();
        Assert.Same(Heart, Matrix.Part<Path>(card, "PART_Icon").Data);
        var close = Matrix.Part<Button>(card, "PART_CloseButton");
        Assert.Same(Heart, close.GetVisualDescendants().OfType<Path>().Single().Data);
    });

    /// <summary>
    /// One control's hover and ripple, through tokens rather than its template: the state layer
    /// and ripple read DynamicResources, so keys in the button's own Resources reach only it.
    /// </summary>
    [Fact]
    public Task AControlsHoverIsAKeyInItsOwnResources() => UiThread.RunAsync(() =>
    {
        var quiet = new Button { Content = "Poster" };
        quiet.Resources["FiliActionDefaultHoverBrush"] = Brushes.Transparent;
        quiet.Resources["FiliRippleOpacity"] = 0d;
        var plain = new Button { Content = "Plain" };
        Matrix.Show(new StackPanel { Children = { quiet, plain } });

        Matrix.Hover(quiet);
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Border>(quiet, "PART_StateLayer").Background));
        var rippleOpacity = AvaloniaPropertyRegistry.Instance.GetRegisteredAttached(typeof(Button)).Single(p => p.OwnerType.Name == "Ripple" && p.Name == "Opacity");
        Assert.Equal(0d, quiet.GetValue(rippleOpacity));

        Matrix.Hover(plain);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Matrix.Part<Border>(plain, "PART_StateLayer").Background));
    });
}
