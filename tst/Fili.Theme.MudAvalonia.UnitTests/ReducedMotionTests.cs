using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// FiliReducedMotion: True in an app's resources, and nothing the theme draws moves or fades on
/// its own. Set here on one window, so it reaches only that window and the popups it opens.
/// </summary>
public class ReducedMotionTests
{
    private static T Calm<T>(T control) where T : Control
    {
        Matrix.Show(control);
        TopLevel.GetTopLevel(control)!.Resources["FiliReducedMotion"] = true;
        Dispatcher.UIThread.RunJobs();
        return control;
    }

    /// <summary>A popup is at full opacity from its first frame: no fade.</summary>
    [Fact]
    public Task APopupDoesNotFade() => UiThread.RunAsync(() =>
    {
        var row = new MenuItem { Header = "Open" };
        var file = new MenuItem { Header = "File", Items = { row } };
        Calm(new Menu { Items = { file } });

        file.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        var host = row.GetVisualAncestors().OfType<TemplatedControl>().Last(a => a is PopupRoot or OverlayPopupHost);
        var presenter = Matrix.Part<ContentPresenter>(host, "PART_ContentPresenter");

        for (var frame = 0; frame < 5; frame++)
        {
            Thread.Sleep(10);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, presenter.Opacity);
        }
    });

    /// <summary>A press draws no ripple.</summary>
    [Fact]
    public Task APressDoesNotRipple() => UiThread.RunAsync(() =>
    {
        var button = Calm(new Button { Content = "Save" });
        var window = (Window)TopLevel.GetTopLevel(button)!;
        var centre = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;

        window.MouseMove(centre, RawInputModifiers.None);
        window.MouseDown(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        // The layer makes its canvas on its first ripple, so none is made at all.
        Assert.Null(Matrix.Part<Border>(button, "PART_Ripple").Child);
        window.MouseUp(centre, MouseButton.Left);
    });

    /// <summary>An expansion panel is open the moment it is told to be, with no grow to watch.</summary>
    [Fact]
    public Task APanelOpensAtOnce() => UiThread.RunAsync(() =>
    {
        var expander = Calm(new Expander { Header = "Panel", Content = "Content", Width = 320 });
        var collapse = Matrix.Part<Decorator>(expander, "PART_Collapse");
        var openness = AvaloniaPropertyRegistry.Instance.FindRegistered(collapse, "Openness")!;

        expander.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1d, collapse.GetValue(openness));
    });

    /// <summary>
    /// A striped bar's bands stand still, still drawn: the bar's length shows the work going on.
    /// An indeterminate bar keeps moving, as its movement is the only sign of it. Read from the
    /// styles, as a key-frame animation ticked headless stalls wherever it happens to be.
    /// </summary>
    [Fact]
    public Task StripesStandStill() => UiThread.RunAsync(() =>
    {
        var bar = Calm(new ProgressBar { Width = 200, Value = 60, Classes = { "large", "striped" } });
        Assert.True(bar.GetVisualDescendants().OfType<Panel>().Single(p => p.Name == "PART_Stripes").IsVisible);

        var theme = Assert.IsType<ControlTheme>(Application.Current!.FindResource(typeof(ProgressBar)));
        var animated = Styles(Chain(theme).SelectMany(t => t.Children)).Where(s => s.Animations.Count > 0).Select(s => s.Selector!.ToString()).ToList();
        Assert.Contains(animated, s => s.Contains("PART_Stripes"));
        Assert.All(animated.Where(s => s.Contains("PART_Stripes")), s => Assert.Contains("Motion.Reduced", s));
        Assert.All(animated.Where(s => s.Contains("IndeterminateIndicator")), s => Assert.DoesNotContain("Reduced", s));

        // The type's theme is an alias, BasedOn the one that holds the styles.
        static IEnumerable<ControlTheme> Chain(ControlTheme? t) => t is null ? [] : Chain(t.BasedOn).Prepend(t);

        static IEnumerable<Style> Styles(IEnumerable<IStyle> styles) =>
            styles.OfType<Style>().SelectMany(s => new[] { s }.Concat(Styles(s.Children)));
    });

    /// <summary>
    /// One of every control whose template or styles declare a transition, the states that set
    /// one included: an open inline drawer, joined expansion panels, a sortable grid.
    /// </summary>
    private static StackPanel EveryTransitionedControl()
    {
        var tree = new TreeView { Items = { new TreeViewItem { Header = "Library", IsExpanded = true, Items = { new TreeViewItem { Header = "Books" } } } } };
        var nav = new TreeView { Classes = { "nav" }, Items = { new TreeViewItem { Header = "Home" } } };
        var tabs = new TabControl { Items = { new TabItem { Header = "One" }, new TabItem { Header = "Two" } }, SelectedIndex = 0 };
        var strip = new TabStrip { Items = { new TabStripItem { Content = "One" }, new TabStripItem { Content = "Two" } }, SelectedIndex = 0 };
        var panels = new StackPanel
        {
            Classes = { "expansion-panels" },
            Children = { new Expander { Header = "First", Content = "One" }, new Expander { Header = "Second", Content = "Two" } },
        };
        return new StackPanel
        {
            Width = 560,
            Children =
            {
                new TextBox { PlaceholderText = "Name" },
                new ComboBox { PlaceholderText = "Pick", ItemsSource = new[] { "A", "B" } },
                new Slider { Value = 40 },
                new ToggleSwitch { Content = "Wi-Fi" },
                new Expander { Header = "Panel", Content = "Content" },
                panels,
                tree,
                new ListBox { ItemsSource = new[] { "One", "Two" } },
                tabs,
                strip,
                new PipsPager { NumberOfPages = 3 },
                new ProgressBar { Value = 25, Classes = { "circular" } },
                new SplitView { DisplayMode = SplitViewDisplayMode.Inline, IsPaneOpen = true, Height = 120, Pane = nav },
                new DataGrid
                {
                    Height = 120,
                    ItemsSource = new[] { new Row("A") },
                    Columns = { new DataGridTextColumn { Header = "Name", Binding = new Avalonia.Data.Binding(nameof(Row.Name)) } },
                },
            },
        };
    }

    public sealed record Row(string Name);

    /// <summary>
    /// The transitions running in a tree: every visual's, and every switch's knob's. A collapse
    /// panel keeps its own, as it opens at once by itself (<see cref="APanelOpensAtOnce"/>).
    /// </summary>
    private static List<string> Moving(Visual root) =>
        root.GetVisualDescendants()
            .Where(v => v.Transitions is { Count: > 0 } && v.GetType().Name != "CollapsePanel")
            .Select(v => $"{v.GetType().Name}#{(v as Control)?.Name} in {v.FindAncestorOfType<TemplatedControl>()?.GetType().Name}")
            .Concat(root.GetVisualDescendants().OfType<ToggleSwitch>().Where(s => s.KnobTransitions is { Count: > 0 }).Select(_ => "ToggleSwitch.KnobTransitions"))
            .ToList();

    /// <summary>
    /// No transition the theme declares runs: a field's label and focus underline, the switch's
    /// thumb, a chevron, the grid's sort arrow, the slider's value label, the drawer's pane, an
    /// expansion panel's margin, a determinate circle's sweep and every hover and selection fade.
    /// </summary>
    [Fact]
    public Task NoTransitionTheThemeDeclaresRuns() => UiThread.RunAsync(() =>
    {
        var all = Matrix.Show(EveryTransitionedControl());
        Assert.NotEmpty(Moving(all));

        TopLevel.GetTopLevel(all)!.Resources["FiliReducedMotion"] = true;
        Dispatcher.UIThread.RunJobs();

        var moving = Moving(all);
        Assert.True(moving.Count == 0, "Still transitioned with reduced motion: " + string.Join("; ", moving));
    });

    /// <summary>Calm from the start, as an app that sets the token before its first window.</summary>
    [Fact]
    public Task NoTransitionRunsInAWindowShownCalm() => UiThread.RunAsync(() =>
    {
        var all = EveryTransitionedControl();
        var window = new Window { Width = 600, Height = 300, Content = all };
        window.Resources["FiliReducedMotion"] = true;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var moving = Moving(all);
        Assert.True(moving.Count == 0, "Transitioned in a calm window: " + string.Join("; ", moving));
    });

    /// <summary>Motion back on, and every transition the theme declared is back with it.</summary>
    [Fact]
    public Task TransitionsComeBackWithMotion() => UiThread.RunAsync(() =>
    {
        var all = Matrix.Show(EveryTransitionedControl());
        var before = Moving(all).OrderBy(s => s).ToList();
        var resources = TopLevel.GetTopLevel(all)!.Resources;

        resources["FiliReducedMotion"] = true;
        Dispatcher.UIThread.RunJobs();
        resources["FiliReducedMotion"] = false;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(before, Moving(all).OrderBy(s => s).ToList());
        var label = Matrix.Part<TextBlock>(all.Children.OfType<TextBox>().Single(), "PART_FloatingLabel");
        Assert.Equal(2, label.Transitions!.Count);
    });

    /// <summary>An app's own transition is the app's: reduced motion leaves it alone.</summary>
    [Fact]
    public Task AnAppsOwnTransitionStays() => UiThread.RunAsync(() =>
    {
        var own = new Border { Width = 20, Height = 20, Transitions = [new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromSeconds(1) }] };
        Calm(own);

        Assert.NotNull(own.Transitions);
    });

    /// <summary>
    /// Every transition a hand-written template declares sits on an element marked
    /// <c>Motion.Reducible</c>, so a new one is caught here rather than by a user who asked for
    /// less motion.
    /// </summary>
    [Fact]
    public Task EveryTemplateTransitionIsReducible() => UiThread.RunAsync(() =>
    {
        var theme = Generation.GeneratedThemeTests.ThemeDirectory();
        var files = Directory.GetFiles(Path.Combine(theme, "Themes", "Controls"), "*.axaml")
            .Concat(Directory.GetFiles(Path.Combine(theme, "Themes", "Optional"), "*.axaml", SearchOption.AllDirectories));
        var unmarked = new List<string>();
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var owner = System.Text.RegularExpressions.Regex.Match(lines[i], @"^(\s*)<([\w:]+)\.Transitions>\s*$");
                if (!owner.Success)
                {
                    continue;
                }

                var tag = Enumerable.Range(0, i).Reverse().Select(j => lines[j])
                    .First(l => System.Text.RegularExpressions.Regex.IsMatch(l, $@"^\s{{0,{owner.Groups[1].Length - 1}}}<{owner.Groups[2].Value}(\s|>|$)"));
                if (!tag.Contains("Motion.Reducible"))
                {
                    unmarked.Add($"{Path.GetFileName(file)}:{i + 1}");
                }
            }
        }

        Assert.True(unmarked.Count == 0, "A transition reduced motion does not stop: " + string.Join(", ", unmarked));
    });

    /// <summary>A newly selected tab's indicator is under it from the first frame: no slide.</summary>
    [Fact]
    public Task ATabsIndicatorJumps() => UiThread.RunAsync(() =>
    {
        var tabs = Calm(new TabControl { Width = 560, Items = { new TabItem { Header = "One" }, new TabItem { Header = "Two" } }, SelectedIndex = 0 });
        var second = (TabItem)tabs.Items[1]!;
        var indicator = Matrix.Part<Border>(second, "PART_Indicator");
        var home = second.TranslatePoint(indicator.Bounds.Position, tabs);

        tabs.SelectedIndex = 1;

        // Read before the dispatcher runs, where a slide would still be at the old tab.
        Assert.Equal(home, indicator.TranslatePoint(default, tabs));
        Assert.True(indicator.RenderTransform is null || indicator.RenderTransform.Value.IsIdentity);
    });

    /// <summary>An overlay is at full opacity the moment it shows: no fade in.</summary>
    [Fact]
    public Task AScrimAppearsAtOnce() => UiThread.RunAsync(() =>
    {
        var overlay = new Border { Width = 100, Height = 100, IsVisible = false, Classes = { "overlay", "dark-background" } };
        Calm(new Panel { Width = 100, Height = 100, Children = { overlay } });

        overlay.IsVisible = true;
        for (var frame = 0; frame < 3; frame++)
        {
            Thread.Sleep(10);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, overlay.Opacity);
        }
    });

    /// <summary>
    /// A skeleton neither pulses nor waves, and a snackbar neither slides in nor out: each of
    /// their animations sits under a style that asks for motion. Read from the styles, as a
    /// key-frame animation ticked headless stalls wherever it happens to be.
    /// </summary>
    [Fact]
    public Task SkeletonsAndSnackbarsKeepStill() => UiThread.RunAsync(() =>
    {
        var skeleton = Styles(Application.Current!.Styles)
            .Where(s => s.Animations.Count > 0 && s.Selector!.ToString().Contains("skeleton"))
            .Select(s => s.Selector!.ToString())
            .ToList();
        Assert.Equal(2, skeleton.Count);
        Assert.All(skeleton, s => Assert.Contains("Motion.Reduced", s));

        var card = Assert.IsType<ControlTheme>(Application.Current!.FindResource(typeof(Avalonia.Controls.Notifications.NotificationCard)));
        var snackbar = Styles(Chain(card).SelectMany(t => t.Children))
            .Where(s => s.Animations.Count > 0)
            .Select(s => s.Selector!.ToString())
            .ToList();
        Assert.NotEmpty(snackbar);
        Assert.All(snackbar, s => Assert.Contains("Motion.Reduced", s));

        static IEnumerable<ControlTheme> Chain(ControlTheme? t) => t is null ? [] : Chain(t.BasedOn).Prepend(t);

        static IEnumerable<Style> Styles(IEnumerable<IStyle> styles) =>
            styles.SelectMany(s => s switch
            {
                Style style => new[] { style }.Concat(Styles(style.Children)),
                Avalonia.Styling.Styles group => Styles(group),
                Avalonia.Markup.Xaml.Styling.StyleInclude include when include.Loaded is { } loaded => Styles([loaded]),
                _ => [],
            });
    });

    /// <summary>Without the token the theme moves as before: a press ripples.</summary>
    [Fact]
    public Task MotionIsOnByDefault() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "Save" });
        var window = (Window)TopLevel.GetTopLevel(button)!;
        var centre = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;

        window.MouseMove(centre, RawInputModifiers.None);
        window.MouseDown(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        var waves = Assert.IsType<Canvas>(Matrix.Part<Border>(button, "PART_Ripple").Child);
        Assert.NotEmpty(waves.Children);
        window.MouseUp(centre, MouseButton.Left);
    });
}
