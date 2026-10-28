using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;
using Ellipse = Avalonia.Controls.Shapes.Ellipse;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The MudBlazor options added for 1.0: MudList's Dense, Gutters, Color and body1 text,
/// MudTreeView's Dense and Color, MudLink's every Color, MudAlert's Square, MudExpansionPanel's
/// HideIcon, MudSwitch's UncheckedColor, MudCheckBox's and MudRadio's Dense, MudMenu's Dense.
/// </summary>
public class ParityOptionsTests
{
    private static ListBox List(params string[] classes)
    {
        var list = new ListBox { Width = 200, ItemsSource = new[] { "One", "Two" } };
        list.Classes.AddRange(classes);
        Matrix.Show(list);
        return list;
    }

    private static ListBoxItem Row(ListBox list, int index) => Assert.IsType<ListBoxItem>(list.ContainerFromIndex(index));

    /// <summary>
    /// MudListSubheader (_list.scss): action-default, 14px at 500 on a 20px line, 8px above and
    /// 20px below, 16px gutters unless Gutters="false", 72px in front when Inset.
    /// </summary>
    [Theory]
    [InlineData("", 16, 16)]
    [InlineData("no-gutters", 0, 0)]
    [InlineData("inset", 72, 16)]
    [InlineData("inset no-gutters", 72, 0)]
    public Task ASubheaderIsMudListSubheader(string classes, double left, double right) => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Fruit", Classes = { "subheader" } };
        text.Classes.AddRange(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(text);

        Assert.Equal(new Thickness(left, 8, right, 20), text.Padding);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(text.Foreground));
        Assert.Equal(14, text.FontSize);
        Assert.Equal(20, text.LineHeight);
        Assert.Equal(FontWeight.Medium, text.FontWeight);
        Assert.Equal(48, text.Bounds.Height);
    });

    /// <summary>MudListItem's text is body1, and body2 when dense; so is a select's option.</summary>
    [Fact]
    public Task ARowIsBody1AndBody2WhenDense() => UiThread.RunAsync(() =>
    {
        Assert.Equal(16, Row(List(), 0).FontSize);
        Assert.Equal(14, Row(List("dense"), 0).FontSize);

        var combo = new ComboBox { Width = 200, ItemsSource = new[] { "PC" } };
        Matrix.Show(combo);
        combo.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(16, Assert.IsType<ComboBoxItem>(combo.ContainerFromIndex(0)).FontSize);
        combo.IsDropDownOpen = false;
    });

    [Fact]
    public Task DenseAndNoGuttersOnTheListOrTheRow() => UiThread.RunAsync(() =>
    {
        Assert.Equal(new Thickness(16, 8), Row(List(), 0).Padding);
        Assert.Equal(new Thickness(16, 4), Row(List("dense"), 0).Padding);
        Assert.Equal(new Thickness(0, 8), Row(List("no-gutters"), 0).Padding);
        Assert.Equal(new Thickness(0, 4), Row(List("dense", "no-gutters"), 0).Padding);

        var list = List("dense");
        Row(list, 1).Classes.Add("no-gutters");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Thickness(0, 4), Row(list, 1).Padding);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AListsColourIsTheSelectedRows(string colour) => UiThread.RunAsync(() =>
    {
        var list = List(colour.ToLowerInvariant());
        list.SelectedIndex = 0;
        Dispatcher.UIThread.RunJobs();
        var row = Row(list, 0);
        var ground = Matrix.Part<Border>(row, "PART_StateLayer").GetBaseValue(Border.BackgroundProperty).GetValueOrDefault();

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(row.Foreground));
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(ground));
    });

    [Theory]
    [InlineData("Primary")]
    [InlineData("Warning")]
    public Task ATreesColourIsTheSelectedRows(string colour) => UiThread.RunAsync(() =>
    {
        var leaf = new TreeViewItem { Header = "Leaf" };
        var tree = new TreeView { Width = 200, Classes = { colour.ToLowerInvariant() }, Items = { leaf } };
        Matrix.Show(tree);
        tree.SelectedItem = leaf;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Matrix.Part<ContentPresenter>(leaf, "PART_HeaderPresenter").Foreground));
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"),
            Matrix.Colour(Matrix.Part<Border>(leaf, "SelectionBorder").GetBaseValue(Border.BackgroundProperty).GetValueOrDefault()));
    });

    /// <summary>`.mud-treeview-dense`: no 32px minimum, rows padded 1px 4px.</summary>
    [Fact]
    public Task ADenseTreeHasNoMinimumHeight() => UiThread.RunAsync(() =>
    {
        var leaf = new TreeViewItem { Header = "Leaf" };
        Matrix.Show(new TreeView { Width = 200, Classes = { "dense" }, Items = { leaf } });

        Assert.Equal(0, leaf.MinHeight);
        Assert.Equal(new Thickness(4, 1), leaf.Padding);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task EveryColourIsALinkColour(string colour) => UiThread.RunAsync(() =>
    {
        var link = Matrix.Show(new HyperlinkButton { Content = "Docs", Classes = { colour.ToLowerInvariant() } });

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(link.Foreground));
    });

    [Fact]
    public Task AnAlertIsRoundedUntilItsCornerRadiusSaysOtherwise() => UiThread.RunAsync(() =>
    {
        Assert.Equal(new CornerRadius(4), Matrix.Show(new Border { Classes = { "alert" } }).CornerRadius);
        Assert.Equal(new CornerRadius(0), Matrix.Show(new Border { Classes = { "alert" }, CornerRadius = new CornerRadius(0) }).CornerRadius);
    });

    [Fact]
    public Task HideIconTakesThePanelsChevronAway() => UiThread.RunAsync(() =>
    {
        var shown = Matrix.Show(new Expander { Header = "Panel", Width = 300 });
        var hidden = Matrix.Show(new Expander { Header = "Panel", Width = 300, Classes = { "hide-icon" } });

        Assert.True(Matrix.Part<Path>(shown, "PART_Chevron").IsVisible);
        Assert.False(Matrix.Part<Path>(hidden, "PART_Chevron").IsVisible);
    });

    /// <summary>MudSwitch's UncheckedColor: the thumb and the track in the colour while off.</summary>
    [Fact]
    public Task AnUncheckedColourPaintsTheSwitchWhileOff() => UiThread.RunAsync(() =>
    {
        var toggle = Matrix.Show(new ToggleSwitch { Classes = { "unchecked-error" } });

        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(Matrix.Part<Border>(toggle, "PART_Thumb").Background));
        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(Matrix.Part<Border>(toggle, "PART_Track").Background));

        toggle.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.NotEqual(Matrix.Token("FiliErrorColor"), Matrix.Colour(Matrix.Part<Border>(toggle, "PART_Track").Background));
    });

    /// <summary>`.mud-checkbox-dense` / `.mud-radio-dense`: 4px round the glyph, so 28 / 32 / 44px.</summary>
    [Theory]
    [InlineData(null, 32)]
    [InlineData("small", 28)]
    [InlineData("large", 44)]
    public Task DenseCheckBoxesAndRadiosArePadded4(string? size, double halo) => UiThread.RunAsync(() =>
    {
        foreach (var control in new ToggleButton[] { new CheckBox(), new RadioButton() })
        {
            control.Classes.Add("dense");
            if (size is not null)
            {
                control.Classes.Add(size);
            }

            Matrix.Show(control);
            Assert.Equal(halo, Matrix.Part<Panel>(control, "PART_Box").Width);
            Assert.Equal(halo, Matrix.Part<Ellipse>(control, "PART_StateLayer").Bounds.Height);
        }
    });

    /// <summary>`.mud-checkbox.mud-disabled *`: the glyph and the label in text-disabled.</summary>
    [Fact]
    public Task ADisabledCheckBoxIsTextDisabled() => UiThread.RunAsync(() =>
    {
        foreach (var control in new ToggleButton[] { new CheckBox { Content = "Agree" }, new RadioButton { Content = "One" } })
        {
            control.IsEnabled = false;
            Matrix.Show(control);
            Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Matrix.Part<Path>(control, "PART_Unchecked").Fill));
            Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(control.Foreground));
        }
    });

    private static TextBlock Label(Control field, string name = "PART_FloatingLabel") => Matrix.Part<TextBlock>(field, name);

    [Theory]
    [InlineData(null)]
    [InlineData("filled")]
    public Task NoUnderlineDrawsNoRule(string? variant) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, Classes = { "no-underline" } };
        var combo = new ComboBox { Width = 200, Classes = { "no-underline" } };
        if (variant is not null)
        {
            box.Classes.Add(variant);
            combo.Classes.Add(variant);
        }

        foreach (var field in new Control[] { box, combo })
        {
            Matrix.Show(field);
            Assert.False(Matrix.Part<Border>(field, "PART_Underline").IsVisible);
            Assert.False(Matrix.Part<Border>(field, "PART_UnderlineActive").IsVisible);
        }

        Assert.True(Matrix.Part<Border>(Matrix.Show(new TextBox { Width = 200 }), "PART_Underline").IsVisible);
    });

    /// <summary>`.mud-input-required … .mud-input-label::after { content: "*" }`.</summary>
    [Fact]
    public Task RequiredAddsTheAsterisk() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 200, PlaceholderText = "Name", Classes = { "required" } });
        var combo = Matrix.Show(new ComboBox { Width = 200, PlaceholderText = "Platform", Classes = { "required" }, ItemsSource = new[] { "PC" } });
        var plain = Matrix.Show(new TextBox { Width = 200, PlaceholderText = "Name" });

        Assert.Equal("Name*", Label(box).Text);
        Assert.Equal("Platform*", Label(combo).Text);
        Assert.Equal("Platform*", Label(combo, "PART_SelectedLabel").Text);
        Assert.Equal("Name", Label(plain).Text);
    });

    /// <summary>ShrinkLabel: the label floats with no text and no focus, in text-primary.</summary>
    [Theory]
    [InlineData(null, -14)]
    [InlineData("filled", -10)]
    [InlineData("outlined", -26)]
    public Task ShrinkLabelFloatsAnEmptyField(string? variant, double rise) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, PlaceholderText = "Name", Classes = { "shrink-label" } };
        var combo = new ComboBox { Width = 200, PlaceholderText = "Platform", Classes = { "shrink-label" } };
        if (variant is not null)
        {
            box.Classes.Add(variant);
            combo.Classes.Add(variant);
        }

        foreach (var field in new Control[] { box, combo })
        {
            Matrix.Show(field);
            var label = Label(field);
            Assert.Equal(12, label.GetBaseValue(TextBlock.FontSizeProperty).GetValueOrDefault());
            var transform = label.GetBaseValue(Visual.RenderTransformProperty).GetValueOrDefault();
            Assert.Equal(rise, Assert.IsAssignableFrom<ITransform>(transform).Value.M32, 1);
            Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(label.Foreground));
        }
    });

    /// <summary>`.mud-shrink ~ label`: a label floated by its text is text-primary, not secondary.</summary>
    [Fact]
    public Task ALabelOverTextIsTextPrimary() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 200, PlaceholderText = "Name", Text = "Ada" });
        var combo = Matrix.Show(new ComboBox { Width = 200, PlaceholderText = "Platform", ItemsSource = new[] { "PC" }, SelectedIndex = 0 });
        var empty = Matrix.Show(new TextBox { Width = 200, PlaceholderText = "Name" });

        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(Label(box).Foreground));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(Label(combo, "PART_SelectedLabel").Foreground));
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(Label(empty).Foreground));
    });

    [Fact]
    public Task TheWrappedFieldsPassTheNewClassesOn() => UiThread.RunAsync(() =>
    {
        var auto = Matrix.Show(new AutoCompleteBox { Width = 200, Classes = { "required", "no-underline", "shrink-label" } });
        var inner = Matrix.Part<TextBox>(auto, "PART_TextBox");

        Assert.Contains("required", inner.Classes);
        Assert.Contains("no-underline", inner.Classes);
        Assert.Contains("shrink-label", inner.Classes);
    });

    /// <summary>MudSelect's OpenIcon: ArrowDropUp while the list is open.</summary>
    [Fact]
    public Task TheSelectsArrowFlipsWhileOpen() => UiThread.RunAsync(() =>
    {
        var combo = Matrix.Show(new ComboBox { Width = 200, ItemsSource = new[] { "PC" } });
        var arrow = Matrix.Part<Path>(combo, "PART_Chevron");
        Application.Current!.TryFindResource("FiliArrowDropDownGeometry", out var down);
        Application.Current!.TryFindResource("FiliArrowDropUpGeometry", out var up);

        Assert.Same(down, arrow.Data);
        combo.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(up, arrow.Data);
        combo.IsDropDownOpen = false;
    });

    /// <summary>`.mud-toolbar`: 64px with 24px gutters; dense 48; Gutters="false" is Padding="0".</summary>
    [Fact]
    public Task AToolbarIsMudToolBarsBox() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save" };
        var bar = Matrix.Show(new Border { Classes = { "toolbar" }, Child = button });

        Assert.Equal(64, bar.Height);
        Assert.Equal(new Thickness(24, 0), bar.Padding);
        Assert.Equal(Avalonia.Layout.VerticalAlignment.Center, button.VerticalAlignment);
        Assert.Equal(48, Matrix.Show(new Border { Classes = { "toolbar", "dense" } }).Height);
        Assert.Equal(new Thickness(0), Matrix.Show(new Border { Classes = { "toolbar" }, Padding = new Thickness(0) }).Padding);
        Assert.Equal(new Thickness(0), Matrix.Show(new Border { Classes = { "appbar" }, Padding = new Thickness(0) }).Padding);
    });

    /// <summary>MudIcon's Size, 20 / 24 / 36px, and Color, on a PathIcon.</summary>
    [Theory]
    [InlineData("small", 20)]
    [InlineData("medium", 24)]
    [InlineData("large", 36)]
    public Task AnIconsSizeIsAClass(string size, double pixels) => UiThread.RunAsync(() =>
    {
        var icon = Matrix.Show(new PathIcon { Classes = { size } });

        Assert.Equal(pixels, icon.Width);
        Assert.Equal(pixels, icon.Height);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AnIconsColourIsAClass(string colour) => UiThread.RunAsync(() =>
    {
        var icon = Matrix.Show(new PathIcon { Classes = { colour.ToLowerInvariant() } });

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(icon.Foreground));
    });

    /// <summary>An icon's own colour beats the one its alert gives it, as MudIcon's IconColor does.</summary>
    [Fact]
    public Task AnIconsOwnColourBeatsItsAlerts() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "error" } };
        Matrix.Show(new Border { Classes = { "alert", "info" }, Child = icon });

        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(icon.Foreground));
    });

    /// <summary>MudDrawer's Color: `.mud-theme-{color}`, the pane in the colour and its contrast text.</summary>
    [Fact]
    public Task ADrawersColourIsThePanes() => UiThread.RunAsync(() =>
    {
        var drawer = Matrix.Show(new SplitView
        {
            IsPaneOpen = true,
            DisplayMode = SplitViewDisplayMode.Inline,
            Classes = { "primary" },
            Pane = new TextBlock { Text = "Library" },
            Width = 600,
            Height = 300,
        });
        var pane = Matrix.Part<ContentPresenter>(drawer, "PART_PanePresenter");

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(drawer.PaneBackground));
        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(pane.GetValue(TextElement.ForegroundProperty)));
    });

    /// <summary>
    /// MudDrawer.Elevation, 1 by default: a shadow beside the pane, on every side the pane can
    /// stand. The pane's content still clips to the pane, one level in, and the shadow is
    /// outside that clip. `elevation0` drops it, and a drawer closed to nothing casts none.
    /// </summary>
    [Theory]
    [InlineData(SplitViewPanePlacement.Left)]
    [InlineData(SplitViewPanePlacement.Right)]
    [InlineData(SplitViewPanePlacement.Top)]
    [InlineData(SplitViewPanePlacement.Bottom)]
    public Task ADrawerCastsElevationOne(SplitViewPanePlacement placement) => UiThread.RunAsync(() =>
    {
        var drawer = Matrix.Show(new SplitView
        {
            IsPaneOpen = true,
            DisplayMode = SplitViewDisplayMode.Inline,
            PanePlacement = placement,
            Pane = new TextBlock { Text = "Library" },
            Width = 600,
            Height = 300,
        });
        var shadow = Matrix.Part<Border>(drawer, "PART_PaneShadow");

        Assert.Equal(FabTests.Shadow(1), shadow.BoxShadow);
        Assert.True(shadow.IsEffectivelyVisible);
        Assert.False(Matrix.Part<Panel>(drawer, "PART_PaneRoot").ClipToBounds);
        Assert.True(Matrix.Part<Panel>(drawer, "PART_PaneClip").ClipToBounds);
        Assert.Same(Matrix.Part<Panel>(drawer, "PART_PaneClip"), Matrix.Part<ContentPresenter>(drawer, "PART_PanePresenter").GetVisualParent());

        drawer.Classes.Add("elevation0");
        Assert.Equal(FabTests.Shadow(0), shadow.BoxShadow);
        drawer.Classes.Remove("elevation0");
        drawer.Classes.Add("elevation8");
        Assert.Equal(FabTests.Shadow(8), shadow.BoxShadow);

        drawer.IsPaneOpen = false;
        Dispatcher.UIThread.RunJobs();
        Assert.False(shadow.IsVisible);
        drawer.DisplayMode = SplitViewDisplayMode.CompactInline;
        Dispatcher.UIThread.RunJobs();
        Assert.True(shadow.IsVisible);
    });

    /// <summary>
    /// MudDrawerHeader: `.mud-drawer-header`, at least the app bar's 64px with 12px 24px round it,
    /// and dense 48px with 8px 24px. A floor, so a taller header grows.
    /// </summary>
    [Fact]
    public Task ADrawerHeaderIsAnAppBarTallAndDenseThreeQuarters() => UiThread.RunAsync(() =>
    {
        var header = Matrix.Show(new Border { Classes = { "drawer-header" }, Child = new TextBlock { Text = "Library" } });
        Assert.Equal(64, header.MinHeight);
        Assert.Equal(new Thickness(24, 12), header.Padding);
        Assert.Equal(64, header.Bounds.Height);

        header.Classes.Add("dense");
        Assert.Equal(48, header.MinHeight);
        Assert.Equal(new Thickness(24, 8), header.Padding);

        var tall = Matrix.Show(new Border { Classes = { "drawer-header" }, Child = new Border { Height = 100 } });
        Assert.Equal(124, tall.Bounds.Height);
    });

    /// <summary>MudMenu.Dense: 2px 12px rows in body2.</summary>
    [Fact]
    public Task ADenseMenusRowsAre2By12InBody2() => UiThread.RunAsync(() =>
    {
        var row = new MenuItem { Header = "Open" };
        var top = new MenuItem { Header = "File", Classes = { "dense" }, Items = { row } };
        Matrix.Show(new Menu { Items = { top } });
        top.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new Thickness(12, 2), row.Padding);
        Assert.Equal(14, row.FontSize);
        Assert.Equal(32, row.MinHeight);
    });
}
