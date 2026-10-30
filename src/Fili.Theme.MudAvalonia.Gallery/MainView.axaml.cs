using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia;
using Fili.Theme.MudAvalonia.Gallery.Docs;

namespace Fili.Theme.MudAvalonia.Gallery;

public partial class MainView : UserControl
{
    /// <summary>Below this width the drawer overlays the page and closes once a page is chosen.</summary>
    private const double NarrowWidth = 960;

    /// <summary>The overlays the performance toggle shows: FPS and the layout and render times.</summary>
    internal const RendererDebugOverlays PerformanceOverlays =
        RendererDebugOverlays.Fps | RendererDebugOverlays.LayoutTimeGraph | RendererDebugOverlays.RenderTimeGraph;

    private readonly Dictionary<string, Control> _built = [];
    private readonly Dictionary<string, TimeSpan> _buildTimes = [];
    private readonly Dictionary<string, TreeViewItem> _links = [];
    private readonly SplitView _drawer;
    private readonly TreeView _nav;
    private readonly ContentControl _host;
    private readonly TextBlock _readout;
    private readonly ToggleButton _performance;
    private readonly bool _ready;

    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
        this.FindControl<TextBlock>("ThemeVersion")!.Text = ThemeVersionText();
        _drawer = this.FindControl<SplitView>("Drawer")!;
        _nav = this.FindControl<TreeView>("Nav")!;
        _host = this.FindControl<ContentControl>("PageHost")!;
        _readout = this.FindControl<TextBlock>("PerformanceReadout")!;
        _performance = this.FindControl<ToggleButton>("Performance")!;

        // The selector starts on the variant the app is in, rather than setting Light over it.
        var variant = Application.Current?.RequestedThemeVariant;
        this.FindControl<ComboBox>("VariantSelector")!.SelectedIndex =
            variant == ThemeVariant.Dark ? 1 : variant == FiliThemeVariants.HighContrast ? 2 : 0;
        _ready = true;

        BuildSwatches();

        BuildNav(string.Empty);
        Navigate(DocAddress.Current?.Slug is { Length: > 0 } slug && DocCatalog.Find(slug) is not null ? slug : "overview");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DocAddress.Current is { } address)
        {
            address.Changed += OnAddressChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DocAddress.Current is { } address)
        {
            address.Changed -= OnAddressChanged;
        }
    }

    /// <summary>Back, Forward or a followed link changed the address: show the page it names.</summary>
    private void OnAddressChanged(string slug) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (slug != Current)
            {
                Navigate(slug.Length > 0 ? slug : "overview");
            }
        });

    /// <summary>The slug of the page on show.</summary>
    public string? Current { get; private set; }

    internal static string ThemeVersionText()
    {
        var assembly = typeof(FiliThemeVariants).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrEmpty(informational))
        {
            return $"v{assembly.GetName().Version?.ToString(3)}";
        }

        var plus = informational.IndexOf('+');
        if (plus < 0)
        {
            return $"v{informational}";
        }

        var commit = informational[(plus + 1)..];
        return $"v{informational[..plus]} · {commit[..System.Math.Min(7, commit.Length)]}";
    }

    /// <summary>Shows the page with this slug, building it the first time.</summary>
    public void Navigate(string slug)
    {
        if (DocCatalog.Find(slug) is not { } page)
        {
            return;
        }

        if (!_built.TryGetValue(slug, out var view))
        {
            var started = Stopwatch.GetTimestamp();
            var content = page.Create();
            view = page.Scrolls ? content : new ScrollViewer { Content = content };
            _built[slug] = view;
            TimeBuild(slug, view, Stopwatch.GetElapsedTime(started));
        }

        Current = slug;
        _host.Content = view;
        DocAddress.Current?.Show(slug, page.Title);
        ShowReadout();
        if (_links.TryGetValue(slug, out var link) && !ReferenceEquals(_nav.SelectedItem, link))
        {
            _nav.SelectedItem = link;
        }

        if (_drawer.DisplayMode == SplitViewDisplayMode.Overlay)
        {
            _drawer.IsPaneOpen = false;
        }
    }

    /// <summary>
    /// The drawer's menu: a group per section, open, and a link per page whose title matches the
    /// search. A section with no match is left out.
    /// </summary>
    private void BuildNav(string filter)
    {
        _links.Clear();
        var groups = new List<TreeViewItem>();
        foreach (var section in DocCatalog.Sections)
        {
            var pages = DocCatalog.Pages
                .Where(p => p.Section == section && p.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (pages.Count == 0)
            {
                continue;
            }

            var group = new TreeViewItem { Header = section, IsExpanded = true };
            foreach (var page in pages)
            {
                var link = new TreeViewItem { Header = page.Title, Tag = page.Slug };
                _links[page.Slug] = link;
                group.Items.Add(link);
            }

            groups.Add(group);
        }

        _nav.ItemsSource = groups;
        if (Current is { } current && _links.TryGetValue(current, out var selected))
        {
            _nav.SelectedItem = selected;
        }
    }

    private void OnNav(object? sender, SelectionChangedEventArgs e)
    {
        if (_nav.SelectedItem is TreeViewItem { Tag: string slug } && slug != Current)
        {
            Navigate(slug);
        }
    }

    private void OnFilter(object? sender, TextChangedEventArgs e) =>
        BuildNav((sender as TextBox)?.Text?.Trim() ?? string.Empty);

    private void OnMenu(object? sender, RoutedEventArgs e) => _drawer.IsPaneOpen = !_drawer.IsPaneOpen;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var narrow = e.NewSize.Width < NarrowWidth;
        var mode = narrow ? SplitViewDisplayMode.Overlay : SplitViewDisplayMode.Inline;
        if (_drawer.DisplayMode != mode)
        {
            _drawer.DisplayMode = mode;
            _drawer.IsPaneOpen = !narrow;
        }
    }

    private void OnVariantChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_ready || Application.Current is null || sender is not ComboBox selector)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = selector.SelectedIndex switch
        {
            1 => ThemeVariant.Dark,
            2 => FiliThemeVariants.HighContrast,
            _ => ThemeVariant.Light,
        };
    }

    /// <summary>
    /// A page's build time: creating it, then from its joining the window to its first layout,
    /// which applies its templates. The wait in between is the window's, not the page's: the first
    /// page is created before there is a window to show it.
    /// </summary>
    private void TimeBuild(string slug, Control view, TimeSpan created)
    {
        var attached = 0L;
        view.AttachedToVisualTree += OnAttached;
        view.Loaded += OnLoaded;

        void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            view.AttachedToVisualTree -= OnAttached;
            attached = Stopwatch.GetTimestamp();
        }

        void OnLoaded(object? sender, RoutedEventArgs e)
        {
            view.Loaded -= OnLoaded;
            _buildTimes[slug] = created + Stopwatch.GetElapsedTime(attached);
            if (slug == Current)
            {
                ShowReadout();
            }
        }
    }

    private void OnPerformance(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            top.RendererDiagnostics.DebugOverlays = _performance.IsChecked == true ? PerformanceOverlays : RendererDebugOverlays.None;
        }

        ShowReadout();
    }

    /// <summary>
    /// The page's visual count and build time, while the toggle is on. Counted when shown, so a
    /// page whose parts open later counts them the next time it is shown.
    /// </summary>
    private void ShowReadout()
    {
        _readout.IsVisible = _performance.IsChecked == true;
        if (!_readout.IsVisible || Current is not { } slug || !_built.TryGetValue(slug, out var view))
        {
            return;
        }

        var visuals = view.GetVisualDescendants().Count() + 1;
        _readout.Text = _buildTimes.TryGetValue(slug, out var time)
            ? $"{visuals:N0} visuals · built in {time.TotalMilliseconds:0} ms"
            : $"{visuals:N0} visuals";
    }

    private void OnRightToLeft(object? sender, RoutedEventArgs e) =>
        _host.FlowDirection = (sender as ToggleButton)?.IsChecked == true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>
    /// One round button per swatch, ticked when it is the primary on show. The first is the
    /// theme's own and takes the overrides away rather than setting the same colour over them.
    /// </summary>
    private void BuildSwatches()
    {
        var panel = this.FindControl<WrapPanel>("Swatches")!;
        foreach (var (name, colour) in PrimaryColour.Swatches)
        {
            var isDefault = panel.Children.Count == 0;
            var tick = new PathIcon
            {
                Width = 16,
                Height = 16,
                Foreground = new SolidColorBrush(PrimaryColour.ContrastText(colour)),
            };
            // Bound rather than looked up: the view is not in the app's tree yet.
            tick[!PathIcon.DataProperty] = new DynamicResourceExtension("GalleryDoneIcon");
            var swatch = new Button
            {
                Classes = { "icon" },
                Tag = isDefault ? null : colour,
                Content = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush(colour),
                    Child = tick,
                },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(swatch, name);
            Avalonia.Automation.AutomationProperties.SetName(swatch, name);
            swatch.Click += (_, _) =>
            {
                if (Application.Current is { } app)
                {
                    PrimaryColour.Apply(app, swatch.Tag as Color?);
                    TickSwatches(panel);
                }
            };
            panel.Children.Add(swatch);
        }

        TickSwatches(panel);
    }

    private static void TickSwatches(WrapPanel panel)
    {
        foreach (var swatch in panel.Children.OfType<Button>())
        {
            if (swatch.Content is Border { Child: PathIcon tick })
            {
                tick.IsVisible = Equals(swatch.Tag as Color?, PrimaryColour.Current);
            }
        }
    }
}
