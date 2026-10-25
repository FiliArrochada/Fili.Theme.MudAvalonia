using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Fili.Theme.MudAvalonia;
using Fili.Theme.MudAvalonia.Gallery.Docs;

namespace Fili.Theme.MudAvalonia.Gallery;

public partial class MainView : UserControl
{
    /// <summary>Below this width the drawer overlays the page and closes once a page is chosen.</summary>
    private const double NarrowWidth = 960;

    private readonly Dictionary<string, Control> _built = [];
    private readonly Dictionary<string, TreeViewItem> _links = [];
    private readonly SplitView _drawer;
    private readonly TreeView _nav;
    private readonly ContentControl _host;
    private readonly bool _ready;

    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
        this.FindControl<TextBlock>("ThemeVersion")!.Text = ThemeVersionText();
        _drawer = this.FindControl<SplitView>("Drawer")!;
        _nav = this.FindControl<TreeView>("Nav")!;
        _host = this.FindControl<ContentControl>("PageHost")!;

        // The selector starts on the variant the app is in, rather than setting Light over it.
        var variant = Application.Current?.RequestedThemeVariant;
        this.FindControl<ComboBox>("VariantSelector")!.SelectedIndex =
            variant == ThemeVariant.Dark ? 1 : variant == FiliThemeVariants.HighContrast ? 2 : 0;
        _ready = true;

        BuildNav(string.Empty);
        Navigate("overview");
    }

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
            var content = page.Create();
            view = page.Scrolls ? content : new ScrollViewer { Content = content };
            _built[slug] = view;
        }

        Current = slug;
        _host.Content = view;
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

    private void OnRightToLeft(object? sender, RoutedEventArgs e) =>
        _host.FlowDirection = (sender as ToggleButton)?.IsChecked == true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
}
