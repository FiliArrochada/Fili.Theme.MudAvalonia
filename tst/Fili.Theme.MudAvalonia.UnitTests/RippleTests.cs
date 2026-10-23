using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudBlazor's ripple (mudRipple.js, _ripple.scss): a press inside a control centres a circle
/// twice the control's longer side on the pointer, growing from nothing over .5s at
/// cubic-bezier(.4,0,.2,1) while its opacity rises over .1s to --mud-ripple-opacity - 0.1, or 0.2
/// on filled buttons, filled chips and coloured FABs - in --mud-ripple-color, the control's text
/// colour. Released, it fades over .4s, not before .5s from the press. Only the innermost
/// control under the pointer ripples.
/// </summary>
/// <remarks>
/// Every press is released when the test is done with it: the headless mouse is one device for
/// every window, and a press left down keeps its control captured into the next test. Headless
/// dispatcher timers run on the next pass of jobs rather than when due, so a release fades at
/// once here; the .5s hold is mudRipple.js's MIN_RIPPLE_DURATION, read in Ripple.cs.
/// </remarks>
public class RippleTests
{
    /// <summary>
    /// A press at <paramref name="at"/> in <paramref name="target"/>'s coordinates, or its centre,
    /// held until the result is disposed.
    /// </summary>
    private static Held Press(Control target, Point? at = null, MouseButton button = MouseButton.Left)
    {
        var window = (Window)TopLevel.GetTopLevel(target)!;
        var point = target.TranslatePoint(at ?? new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, button);
        Dispatcher.UIThread.RunJobs();
        return new Held(window, point, button);
    }

    private sealed record Held(Window Window, Point Point, MouseButton Button) : IDisposable
    {
        public void Dispose()
        {
            Window.MouseUp(Point, Button);
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Canvas Waves(Control control, string layer = "PART_Ripple") =>
        Assert.IsType<Canvas>(Matrix.Part<Border>(control, layer).Child);

    /// <summary>The one ripple a press drew in <paramref name="control"/>: its wave and its circle.</summary>
    private static (Panel Wave, Ellipse Dot) Single(Control control, string layer = "PART_Ripple")
    {
        var wave = Assert.IsAssignableFrom<Panel>(Assert.Single(Waves(control, layer).Children));
        return (wave, Assert.IsType<Ellipse>(Assert.Single(wave.Children)));
    }

    /// <summary>The opacity the ripple rises to: its transition's end, which the live value trails.</summary>
    private static double Strength(Ellipse dot) => dot.GetBaseValue(Visual.OpacityProperty).GetValueOrDefault();

    /// <summary>The opacity a press on <paramref name="control"/> ripples to, the press released after.</summary>
    private static double Pressed(Control control, string layer = "PART_Ripple")
    {
        using var _ = Press(control);
        return Strength(Single(control, layer).Dot);
    }

    private static T WithClasses<T>(T control, string classes) where T : Control
    {
        control.Classes.AddRange(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return control;
    }

    [Fact]
    public Task APressGrowsACircleFromThePointer() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save", Width = 120, Height = 36 });
        var layer = Matrix.Part<Border>(button, "PART_Ripple");
        using var _ = Press(button, new Point(30, 10));

        var (wave, dot) = Single(button);
        var at = button.TranslatePoint(new Point(30, 10), layer)!.Value;

        // createRipple: Math.max(rect.width, rect.height) * 2, centred on the pointer.
        Assert.Equal(240, wave.Width);
        Assert.Equal(240, wave.Height);
        Assert.Equal(at.X - 120, Canvas.GetLeft(wave), 3);
        Assert.Equal(at.Y - 120, Canvas.GetTop(wave), 3);

        // From scale(0) and transparent, to scale(1) at the ripple opacity, in the text's colour.
        Assert.InRange(dot.Opacity, 0, 0.05);
        Assert.Equal(0.1, Strength(dot), 3);
        var grown = (TransformOperations)dot.GetBaseValue(Visual.RenderTransformProperty).GetValueOrDefault()!;
        Assert.Equal(TransformOperations.Parse("scale(1)").Value, grown.Value);
        Assert.Equal(Matrix.Colour(button.Foreground), Matrix.Colour(dot.Fill));
    });

    [Fact]
    public Task TheTimingIsMudBlazors() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save" });
        using var _ = Press(button);
        var (wave, dot) = Single(button);

        var grow = Assert.Single(dot.Transitions!.OfType<TransformOperationsTransition>());
        Assert.Equal(Visual.RenderTransformProperty, grow.Property);
        Assert.Equal(TimeSpan.FromMilliseconds(500), grow.Duration);
        var easing = Assert.IsType<SplineEasing>(grow.Easing);
        Assert.Equal((0.4, 0.0, 0.2, 1.0), (easing.X1, easing.Y1, easing.X2, easing.Y2));

        var rise = Assert.Single(dot.Transitions!.OfType<DoubleTransition>());
        Assert.Equal(Visual.OpacityProperty, rise.Property);
        Assert.Equal(TimeSpan.FromMilliseconds(100), rise.Duration);

        var fade = Assert.Single(wave.Transitions!.OfType<DoubleTransition>());
        Assert.Equal(Visual.OpacityProperty, fade.Property);
        Assert.Equal(TimeSpan.FromMilliseconds(400), fade.Duration);
    });

    /// <summary>
    /// The circle is seen part way up its opacity. Only that it runs: a transition sampled frame by
    /// frame headless stalls before its end, which <see cref="APressGrowsACircleFromThePointer"/> reads.
    /// </summary>
    [Fact]
    public Task TheRippleAnimates() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save" });
        using var _ = Press(button);
        var (_, dot) = Single(button);

        var seen = new List<double> { dot.Opacity };
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (elapsed.ElapsedMilliseconds < 120)
        {
            Thread.Sleep(10);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            seen.Add(dot.Opacity);
        }

        Assert.Contains(seen, o => o is > 0 and < 0.1);
    });

    /// <summary>
    /// The layer is clipped to a geometry the size of the control: the compositor clips a visual to
    /// a rectangle only, so the corners are the geometry's. Its shape is drawn by the pixel suite.
    /// </summary>
    [Fact]
    public Task TheRippleKeepsToTheControlsShape() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save", Width = 120, Height = 36 });
        using var _ = Press(button);

        var clip = Assert.IsAssignableFrom<Geometry>(Matrix.Part<Border>(button, "PART_Ripple").Clip);
        Assert.Equal(new Rect(0, 0, 120, 36), clip.Bounds);
    });

    /// <summary>Released, the ripple fades to nothing and is taken out of the layer.</summary>
    [Fact]
    public Task ReleasedItFadesAndIsGone() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save" });
        Panel wave;
        using (Press(button))
        {
            wave = Single(button).Wave;
            Assert.Equal(1, wave.Opacity);
        }

        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (Waves(button).Children.Count > 0 && elapsed.ElapsedMilliseconds < 3000)
        {
            Thread.Sleep(20);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
        }

        Assert.Equal(0, wave.GetBaseValue(Visual.OpacityProperty).GetValueOrDefault());
        Assert.Empty(Waves(button).Children);
    });

    [Fact]
    public Task OnlyThePrimaryButtonRipples() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save" });
        using var _ = Press(button, button: MouseButton.Right);

        Assert.Empty(Waves(button).Children);
    });

    /// <summary>closest('.mud-ripple'): a button in a list row ripples, and the row does not.</summary>
    [Fact]
    public Task OnlyTheInnermostControlRipples() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Edit" };
        var row = new ListBoxItem
        {
            Content = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children = { new TextBlock { Text = "Row", Width = 80 }, button },
            },
        };
        Matrix.Show(new ListBox { Width = 300, Items = { row } });

        using (Press(button))
        {
            Assert.Single(Waves(button).Children);
            Assert.Empty(Waves(row).Children);
        }

        using (Press(row, new Point(20, row.Bounds.Height / 2)))
        {
            Assert.Single(Waves(row).Children);
        }
    });

    /// <summary>A checkbox ripples in its halo only, as MudBlazor's icon button round the glyph does.</summary>
    [Fact]
    public Task ACheckboxRipplesInItsHaloNotItsLabel() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new CheckBox { Content = "Remember me" });
        using (Press(Matrix.Part<Control>(box, "PART_ContentPresenter")))
        {
            Assert.Empty(Waves(box).Children);
        }

        var halo = Matrix.Part<Border>(box, "PART_Ripple");
        using (Press(halo))
        {
            Assert.Equal(96, Single(box).Wave.Width);
            Assert.Equal(new Rect(0, 0, 48, 48), halo.Clip!.Bounds);
        }
    });

    [Theory]
    [InlineData("", 0.1)]
    [InlineData("outlined", 0.1)]
    [InlineData("filled", 0.2)]
    [InlineData("filled primary", 0.2)]
    [InlineData("icon", 0.1)]
    [InlineData("icon filled", 0.2)]
    [InlineData("chip", 0.2)]
    [InlineData("chip primary", 0.2)]
    [InlineData("chip outlined", 0.1)]
    [InlineData("chip text", 0.1)]
    [InlineData("fab", 0.1)]
    [InlineData("fab primary", 0.2)]
    [InlineData("fab text primary", 0.1)]
    [InlineData("fab outlined primary", 0.1)]
    public Task AButtonsRippleIsItsVariants(string classes, double opacity) => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(WithClasses(new Button { Content = "Go" }, classes));
        using var _ = Press(button);
        var dot = Single(button).Dot;

        Assert.Equal(opacity, Strength(dot), 3);
        Assert.Equal(Matrix.Colour(button.Foreground), Matrix.Colour(dot.Fill));
    });

    /// <summary>The halves inherit the split button's opacity, so `filled` raises both.</summary>
    [Theory]
    [InlineData("", 0.1)]
    [InlineData("filled", 0.2)]
    public Task ASplitButtonsHalvesRippleAsTheButtonIs(string classes, double opacity) => UiThread.RunAsync(() =>
    {
        var split = Matrix.Show(WithClasses(new SplitButton { Content = "Merge", Flyout = new MenuFlyout() }, classes));

        Assert.Equal(opacity, Pressed(Matrix.Part<Button>(split, "PART_PrimaryButton")), 3);
    });

    [Theory]
    [InlineData("", 0.1)]
    [InlineData("filled", 0.2)]
    public Task ADropDownButtonRipples(string classes, double opacity) => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(WithClasses(new DropDownButton { Content = "Sort", Flyout = new MenuFlyout() }, classes));

        Assert.Equal(opacity, Pressed(button), 3);
    });

    /// <summary>The rows and tabs MudBlazor ripples: a list row, a tab, a tree row, a toggle item, a menu.</summary>
    [Fact]
    public Task RowsTabsAndTogglesRipple() => UiThread.RunAsync(() =>
    {
        var listRow = new ListBoxItem { Content = "Row" };
        var tab = new TabItem { Header = "One" };
        var stripTab = new TabStripItem { Content = "Two" };
        var treeRow = new TreeViewItem { Header = "Node" };
        var toggle = new ToggleButton { Content = "Bold" };
        var menuItem = new MenuItem { Header = "File" };
        Matrix.Show(new StackPanel
        {
            Width = 400,
            Children =
            {
                new ListBox { Items = { listRow } },
                new TabControl { Items = { tab } },
                new TabStrip { Items = { stripTab } },
                new TreeView { Items = { treeRow } },
                toggle,
                new Menu { Items = { menuItem } },
            },
        });

        foreach (var control in new TemplatedControl[] { listRow, tab, stripTab, treeRow, toggle, menuItem })
        {
            using var _ = Press(control);
            var dot = Single(control).Dot;
            Assert.Equal(0.1, Strength(dot), 3);
            Assert.Equal(Matrix.Colour(control.Foreground), Matrix.Colour(dot.Fill));
        }
    });

    /// <summary>A select's option is a MudListItem, and ripples as one.</summary>
    [Fact]
    public Task ASelectsOptionRipples() => UiThread.RunAsync(() =>
    {
        var select = Matrix.Show(new ComboBox { Width = 200, Items = { "One", "Two" } });
        select.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0.1, Pressed((ComboBoxItem)select.ContainerFromIndex(1)!), 3);
    });

    /// <summary>A nav link and a nav group's header, MudNavLink and MudNavGroup.</summary>
    [Fact]
    public Task NavRowsRipple() => UiThread.RunAsync(() =>
    {
        var link = new TreeViewItem { Header = "Home" };
        var group = new TreeViewItem { Header = "Settings", Items = { new TreeViewItem { Header = "Account" } } };
        Matrix.Show(new TreeView { Classes = { "nav" }, Width = 240, Items = { link, group } });

        Assert.Equal(0.1, Pressed(link), 3);

        var toggle = group.GetVisualDescendants().OfType<ToggleButton>()
            .First(t => t.GetVisualDescendants().Any(v => v is Border { Name: "PART_Ripple" }));
        Assert.Equal(0.1, Pressed(toggle), 3);
    });

    /// <summary>`.mud-{color}-text` on the icon button: the glyph's colour, checked or not; text-primary with none.</summary>
    [Theory]
    [InlineData("", false, "FiliTextPrimaryColor")]
    [InlineData("primary", true, "FiliPrimaryColor")]
    [InlineData("primary", false, "FiliPrimaryColor")]
    [InlineData("primary unchecked-error", false, "FiliErrorColor")]
    [InlineData("primary unchecked-error", true, "FiliPrimaryColor")]
    public Task ACheckboxAndARadioRippleInTheGlyphsColour(string classes, bool isChecked, string token) => UiThread.RunAsync(() =>
    {
        var box = WithClasses(new CheckBox { IsChecked = isChecked }, classes);
        var radio = WithClasses(new RadioButton { IsChecked = isChecked }, classes);
        Matrix.Show(new StackPanel { Children = { box, radio } });

        foreach (var control in new Control[] { box, radio })
        {
            using var _ = Press(Matrix.Part<Border>(control, "PART_Ripple"));
            var dot = Single(control).Dot;
            Assert.Equal(0.1, Strength(dot), 3);
            Assert.Equal(Matrix.Token(token), Matrix.Colour(dot.Fill));
        }
    });

    /// <summary>`.mud-ripple-switch`: the ripple is the thumb's halo, and moves with it.</summary>
    [Theory]
    [InlineData("", true, "FiliTextPrimaryColor")]
    [InlineData("primary", true, "FiliPrimaryColor")]
    [InlineData("primary", false, "FiliTextPrimaryColor")]
    [InlineData("unchecked-error", false, "FiliErrorColor")]
    public Task ASwitchRipplesInItsThumb(string classes, bool isChecked, string token) => UiThread.RunAsync(() =>
    {
        var toggle = Matrix.Show(WithClasses(new ToggleSwitch { IsChecked = isChecked }, classes));

        var halo = Matrix.Part<Ellipse>(toggle, "PART_ThumbStateLayer");
        var layer = Matrix.Part<Border>(toggle, "PART_ThumbRipple");
        Assert.Equal(halo.Bounds, layer.Bounds);
        Assert.Equal(halo.GetBaseValue(Visual.RenderTransformProperty).GetValueOrDefault()?.Value,
            layer.GetBaseValue(Visual.RenderTransformProperty).GetValueOrDefault()?.Value);

        using var _ = Press(layer);
        Assert.Equal(Matrix.Token(token), Matrix.Colour(Single(toggle, "PART_ThumbRipple").Dot.Fill));
    });

    /// <summary>MudBlazor has no ripple on a link, an expansion panel's header or a spinner.</summary>
    [Fact]
    public Task ControlsMudBlazorDoesNotRippleHaveNoLayer() => UiThread.RunAsync(() =>
    {
        var link = new HyperlinkButton { Content = "Docs" };
        var panel = new Expander { Header = "Details" };
        var repeat = new RepeatButton { Content = "+" };
        Matrix.Show(new StackPanel { Width = 300, Children = { link, panel, repeat } });

        foreach (var control in new Control[] { link, panel, repeat })
        {
            Assert.DoesNotContain(control.GetVisualDescendants(), v => v is Border { Name: "PART_Ripple" });
        }
    });

    /// <summary>Both opacities at zero, in a region's resources, turn the ripple off there and nowhere else.</summary>
    [Fact]
    public Task ZeroOpacityTurnsTheRippleOff() => UiThread.RunAsync(() =>
    {
        var quiet = new Button { Content = "Quiet", Classes = { "filled" } };
        var loud = new Button { Content = "Loud" };
        var region = new StackPanel { Children = { quiet } };
        region.Resources["FiliRippleOpacity"] = 0.0;
        region.Resources["FiliRippleOpacitySecondary"] = 0.0;
        Matrix.Show(new StackPanel { Children = { region, loud } });

        using (Press(quiet))
        {
            Assert.Empty(Waves(quiet).Children);
        }

        Assert.Equal(0.1, Pressed(loud), 3);
    });

    /// <summary>Palette.cs RippleOpacity and RippleOpacitySecondary, the same in every variant.</summary>
    [Fact]
    public Task TheOpacitiesArePalettes() => UiThread.RunAsync(() =>
    {
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark, FiliThemeVariants.HighContrast })
        {
            Assert.True(Application.Current!.TryFindResource("FiliRippleOpacity", variant, out var primary));
            Assert.True(Application.Current!.TryFindResource("FiliRippleOpacitySecondary", variant, out var secondary));
            Assert.Equal(0.1, primary);
            Assert.Equal(0.2, secondary);
        }
    });

    /// <summary>A layer's ripple ends with its control: detached mid-press, nothing is left behind.</summary>
    [Fact]
    public Task ADetachedControlLeavesNoRipple() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save" };
        var host = new StackPanel { Children = { button } };
        Matrix.Show(host);
        using var _ = Press(button);
        host.Children.Remove(button);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(Waves(button).Children);
    });

    /// <summary>A ripple is the text's colour at the moment of the press: an app's own foreground too.</summary>
    [Fact]
    public Task AnAppsForegroundIsTheRipplesColour() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save", Foreground = Brushes.Teal });
        using var _ = Press(button);

        Assert.Equal(Colors.Teal, Matrix.Colour(Single(button).Dot.Fill));
        Assert.Equal(Colors.Teal, Matrix.Colour(Matrix.Part<Border>(button, "PART_Ripple").GetValue(TextElement.ForegroundProperty)));
    });
}
