using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery;
using Fili.Theme.MudAvalonia.Gallery.Views;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// How the gallery's pages behave, as opposed to how they look.
/// <para>
/// Here rather than in the unit-test project because the gallery is referenced only from this one.
/// </para>
/// </summary>
public class GalleryPageTests
{
    private static (ControlStatesView View, ScrollViewer Page) Open()
    {
        var view = new ControlStatesView();
        // Shorter than the page's first screen of samples, as a browser tab is: the focused "Tabbed
        // to" sample then sits below the fold, which is what made the page open scrolled.
        var window = new Window { Content = view, Width = 1180, Height = 560 };
        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        return (view, view.GetVisualDescendants().OfType<ScrollViewer>().Single(s => s.Name == "Page"));
    }

    /// <summary>
    /// The controls page opened scrolled almost to the bottom: every sample with a selection asks
    /// to bring it into view, the ones with no scroll area of their own pass that up to the page,
    /// and the page obeyed the last - a tab strip near the end.
    /// </summary>
    [Fact]
    public Task TheControlsPageOpensAtTheTop() => UiThread.RunAsync(() =>
    {
        var (_, page) = Open();

        Assert.Equal(0, page.Offset.Y);
        Assert.True(page.Extent.Height > page.Viewport.Height, "The page should be long enough to scroll.");
    });

    /// <summary>Keyboard focus still scrolls the page to what it lands on.</summary>
    [Fact]
    public Task KeyboardFocusStillScrollsThePage() => UiThread.RunAsync(() =>
    {
        var (view, page) = Open();
        var last = view.GetVisualDescendants().OfType<TextBox>().Last();

        last.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();

        Assert.True(page.Offset.Y > 0, "Tabbing to a field at the bottom did not scroll the page.");
    });

    private static (List<TextBlock> Headings, ListBox Index) Sections(ControlStatesView view) =>
        (view.GetVisualDescendants().OfType<TextBlock>().Where(t => Equals(t.Tag, "section")).ToList(),
         view.GetVisualDescendants().OfType<ListBox>().Single(l => l.Name == "SectionIndex"));

    /// <summary>
    /// Every section heading has a row in the index beside the page, and choosing it brings the
    /// heading to the top - by setting the offset itself, since the page ignores scroll requests
    /// that do not come from keyboard focus.
    /// </summary>
    [Fact]
    public Task AnIndexRowScrollsToItsSection() => UiThread.RunAsync(() =>
    {
        var (view, page) = Open();
        var (headings, index) = Sections(view);

        Assert.Equal(17, headings.Count);
        Assert.Equal(headings.Select(h => h.Text), index.Items.OfType<ListBoxItem>().Select(i => (i.Content as TextBlock)?.Text));

        index.SelectedIndex = headings.FindIndex(h => h.Text == "Sliders and progress");
        Dispatcher.UIThread.RunJobs();

        var top = headings.Single(h => h.Text == "Sliders and progress").TranslatePoint(default, page)!.Value.Y;
        Assert.InRange(top, 0, 48);
        Assert.Equal("Sliders and progress", ((index.SelectedItem as ListBoxItem)?.Content as TextBlock)?.Text);
    });

    /// <summary>
    /// The index follows the page: scrolling selects the row of the section in view - the first at
    /// the top, whose heading is already on screen, and the last at the bottom, whose heading can
    /// never reach the top - and following it does not move the page.
    /// </summary>
    [Fact]
    public Task TheIndexFollowsTheScroll() => UiThread.RunAsync(() =>
    {
        var (view, page) = Open();
        var (headings, index) = Sections(view);

        Assert.Equal(0, index.SelectedIndex);

        var dividers = headings.Single(h => h.Text == "Dividers");
        page.Offset = new Vector(0, dividers.TranslatePoint(default, page)!.Value.Y + page.Offset.Y + 40);
        Dispatcher.UIThread.RunJobs();
        var offset = page.Offset.Y;
        Assert.Equal("Dividers", ((index.SelectedItem as ListBoxItem)?.Content as TextBlock)?.Text);
        Assert.Equal(offset, page.Offset.Y);

        page.Offset = new Vector(0, page.Extent.Height);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(headings.Count - 1, index.SelectedIndex);

        page.Offset = default;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, index.SelectedIndex);
    });

    /// <summary>
    /// Every section fits beside the index, in the desktop window and in the 1180px screenshot
    /// frame: a row wider than the space left does not wrap or scroll, it is cut off at the edge.
    /// </summary>
    [Fact]
    public Task EverySectionFitsBesideTheIndex() => UiThread.RunAsync(() =>
    {
        var window = new MainWindow();
        window.Show();
        var tabs = window.GetVisualDescendants().OfType<TabControl>().First();
        tabs.SelectedIndex = tabs.Items.OfType<TabItem>().ToList().FindIndex(t => (string?)t.Header == "Control states");
        Dispatcher.UIThread.RunJobs();
        AssertSectionsFit(window.GetVisualDescendants().OfType<ControlStatesView>().Single(), "the desktop window");
        window.Close();

        var frame = new Window { Content = new ControlStatesView(), Width = 1180, Height = 800 };
        frame.Show();
        Dispatcher.UIThread.RunJobs();
        AssertSectionsFit((ControlStatesView)frame.Content, "a screenshot frame");
        frame.Close();
    });

    /// <summary>
    /// Every item of every horizontal row ends inside the page. A row's own size is no help: a
    /// panel never reports more than the width it was given, and arranges the rest past its edge.
    /// Rows inside a nested scroller (a data grid's cells, a scrolling sample) may run on.
    /// </summary>
    private static void AssertSectionsFit(ControlStatesView view, string where)
    {
        var content = view.GetVisualDescendants().OfType<StackPanel>().Single(p => p.Name == "PageContent");
        var room = content.Bounds.Width;

        var tooWide = content.GetVisualDescendants()
            .OfType<Panel>()
            .Where(panel => panel is WrapPanel or StackPanel { Orientation: Avalonia.Layout.Orientation.Horizontal })
            .Where(panel => !panel.GetVisualAncestors().TakeWhile(a => a != content).Any(a => a is ScrollViewer))
            .SelectMany(panel => panel.Children)
            .Where(item => item.IsVisible)
            .Select(item => (Item: item, Right: item.TranslatePoint(new Point(item.Bounds.Width, 0), content)?.X ?? 0))
            .Where(found => found.Right > room + 0.5)
            .Select(found => $"{found.Item.GetType().Name} ending at {found.Right:0} under "
                + (Section(found.Item)?.Text ?? "the introduction"))
            .ToList();

        Assert.True(tooWide.Count == 0, $"Too wide for {where} ({room:0}px): {string.Join("; ", tooWide)}");

        TextBlock? Section(Control item) =>
            item.GetVisualAncestors().OfType<StackPanel>().FirstOrDefault(p => p.GetVisualParent() == content)?
                .GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => Equals(t.Tag, "section"));
    }

    private static int SectionsInPage(ControlStatesView view) =>
        view.GetVisualDescendants().OfType<TextBlock>().Count(t => Equals(t.Tag, "section"));

    private static bool LoadingRowShown(ControlStatesView view) =>
        view.GetVisualDescendants().OfType<StackPanel>().Any(p => p.Name == "LoadingMore");

    /// <summary>
    /// The page took two seconds to template and lay out in one go, the window frozen throughout.
    /// It opens with its first section and a loading row, and the rest arrive one dispatcher pass
    /// at a time; with the last, the row goes and the screenshot props are placed.
    /// </summary>
    [Fact]
    public Task TheLaterSectionsArriveAfterThePageOpens() => UiThread.RunAsync(() =>
    {
        var view = new ControlStatesView();
        var window = new Window { Content = view, Width = 1180, Height = 560 };
        window.Show();

        Assert.False(view.AllSectionsLoaded);
        Assert.Equal(1, SectionsInPage(view));
        Assert.True(LoadingRowShown(view));

        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        Assert.True(view.AllSectionsLoaded);
        Assert.Equal(17, SectionsInPage(view));
        Assert.False(LoadingRowShown(view));
        Assert.True(view.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "FocusedSample").IsFocused);
        window.Close();
    });

    /// <summary>A page taken off screen mid-load stops adding sections, and carries on when it is back.</summary>
    [Fact]
    public Task APageOffScreenWaitsForItsTurn() => UiThread.RunAsync(() =>
    {
        var view = new ControlStatesView();
        var window = new Window { Content = view, Width = 1180, Height = 560 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(view.AllSectionsLoaded);
        window.Close();

        var paused = new ControlStatesView();
        var host = new Window { Content = paused, Width = 1180, Height = 560 };
        host.Show();
        host.Content = null;
        Dispatcher.UIThread.RunJobs();
        Assert.False(paused.AllSectionsLoaded);

        host.Content = paused;
        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        Assert.True(paused.AllSectionsLoaded);
        host.Close();
    });

    /// <summary>Choosing an index row whose section has not arrived yet brings the rest in at once and goes there.</summary>
    [Fact]
    public Task AnIndexRowAheadOfTheLoadGoesThereAnyway() => UiThread.RunAsync(() =>
    {
        var view = new ControlStatesView();
        var window = new Window { Content = view, Width = 1180, Height = 560 };
        window.Show();
        Assert.False(view.AllSectionsLoaded);

        var index = view.GetVisualDescendants().OfType<ListBox>().Single(l => l.Name == "SectionIndex");
        index.SelectedIndex = index.Items.OfType<ListBoxItem>().ToList().FindIndex(i => (i.Content as TextBlock)?.Text == "Dividers");

        Assert.True(view.AllSectionsLoaded);
        Dispatcher.UIThread.RunJobs();
        var page = view.GetVisualDescendants().OfType<ScrollViewer>().Single(s => s.Name == "Page");
        var heading = view.GetVisualDescendants().OfType<TextBlock>().Single(t => Equals(t.Tag, "section") && t.Text == "Dividers");
        var top = heading.TranslatePoint(default, page)!.Value.Y;
        Assert.True(top is >= 0 and <= 48, $"Dividers at {top}, offset {page.Offset.Y} of {page.Extent.Height}.");
        window.Close();
    });

    /// <summary>
    /// The app bar names the theme version it is showing, read from the theme's own assembly:
    /// "v0.4.0", and the commit after it whenever the SDK stamped one, as every clone does.
    /// </summary>
    [Fact]
    public Task TheAppBarShowsTheThemeVersion() => UiThread.RunAsync(() =>
    {
        var view = new MainView();
        new Window { Content = view, Width = 1180, Height = 400 }.Show();
        Dispatcher.UIThread.RunJobs();

        var text = view.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Name == "ThemeVersion").Text;
        var expected = typeof(FiliThemeVariants).Assembly.GetName().Version!.ToString(3);

        Assert.Matches(@"^v\d+\.\d+\.\d+( · [0-9a-f]{7})?$", text);
        Assert.StartsWith($"v{expected}", text);
    });
}
