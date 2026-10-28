using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery.Docs;
using Fili.Theme.MudAvalonia.Gallery.Docs.Custom;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// The Custom components page's examples, the pieces of MudBlazor an app builds here, do what
/// MudBlazor's do: clicked with the headless mouse, as a reader would.
/// </summary>
public class CustomComponentsTests
{
    private static Window Show(string source)
    {
        var window = new Window { Content = new ExampleCard { Source = source }, Width = 900, Height = 700 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Click(Window window, Control control)
    {
        var centre = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>MudAlert's default Icon for each Severity (MudAlert.razor.cs), from the theme's keys.</summary>
    [Fact]
    public Task EveryAlertShowsItsSeveritysIcon() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/AlertIcons");
        var expected = new Dictionary<string, string>
        {
            [""] = "FiliEventNoteGeometry",
            ["info"] = "FiliInfoOutlinedGeometry",
            ["success"] = "FiliAlertSuccessGeometry",
            ["warning"] = "FiliReportProblemGeometry",
            ["error"] = "FiliErrorOutlineGeometry",
        };

        var alerts = window.GetVisualDescendants().OfType<Border>().Where(b => b.Classes.Contains("alert")).ToList();
        Assert.Equal(expected.Count, alerts.Count);
        foreach (var alert in alerts)
        {
            var severity = alert.Classes.FirstOrDefault(expected.ContainsKey) ?? "";
            var icon = alert.GetVisualDescendants().OfType<PathIcon>().Single();
            Assert.Same(window.FindResource(expected[severity]), icon.Data);
        }

        window.Close();
    });

    /// <summary>MudChip's OnClose: the close button removes its chip, and only its own.</summary>
    [Fact]
    public Task AChipsCloseButtonRemovesIt() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/ClosableChips");
        var list = window.GetVisualDescendants().OfType<ItemsControl>().First().ItemsSource as ClosableList;
        Assert.NotNull(list);
        Assert.Equal(5, list.Count);

        var banana = window.GetVisualDescendants().OfType<ContentControl>()
            .First(c => c.Classes.Contains("chip") && (c.DataContext as ClosableItem)?.Header == "Banana");
        var close = banana.GetVisualDescendants().OfType<Button>().Single();

        // .mud-chip-close-button: 18px, out of the Tab order.
        Assert.Equal(new Size(18, 18), close.Bounds.Size);
        Assert.False(close.Focusable);

        Click(window, close);
        Assert.Equal(["Apple", "Cherry", "Date", "Elderberry"], list.Select(i => i.Header));

        list.Reset();
        Assert.Equal(5, list.Count);
        window.Close();
    });

    /// <summary>MudDynamicTabs' CloseTab and AddTab: a tab's close button removes it, the add button adds one.</summary>
    [Fact]
    public Task TabsCloseAndAdd() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/ClosableTabs");
        var tabs = window.GetVisualDescendants().OfType<TabControl>().Single();
        var list = (ClosableList)tabs.ItemsSource!;
        Assert.Equal(0, tabs.SelectedIndex);

        var first = window.GetVisualDescendants().OfType<TabItem>().First();
        var close = first.GetVisualDescendants().OfType<Button>().Single();
        Assert.False(close.Focusable);
        Click(window, close);

        Assert.Equal(["Tab Two", "Tab Three"], list.Select(i => i.Header));
        Assert.Equal("Tab Two", (tabs.SelectedItem as ClosableItem)?.Header);

        var add = window.GetVisualDescendants().OfType<Button>().First(b => ToolTip.GetTip(b) as string == "Add tab");
        Click(window, add);
        Assert.Equal(["Tab Two", "Tab Three", "New tab 1"], list.Select(i => i.Header));
        window.Close();
    });

    /// <summary>
    /// MudDataGrid's SelectColumn: a row's box is its selection, rows add up, and the header's box
    /// is indeterminate for some, checked for all, and a click selects all, then none.
    /// </summary>
    [Fact]
    public Task TheSelectColumnIsTheGridsSelection() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/SelectColumn");
        var grid = window.GetVisualDescendants().OfType<DataGrid>().Single();
        var boxes = window.GetVisualDescendants().OfType<CheckBox>().ToList();
        var header = boxes.Single(RowSelect.GetIsAll);
        var rows = boxes.Where(RowSelect.GetIsRow).ToList();
        var total = grid.ItemsSource!.Cast<object>().Count();
        Assert.Equal(total, rows.Count);
        Assert.False(header.IsChecked);

        Click(window, rows[1]);
        Click(window, rows[2]);
        Assert.Equal([rows[1].DataContext, rows[2].DataContext], grid.SelectedItems.Cast<object>());
        Assert.True(rows[1].IsChecked);
        Assert.False(rows[0].IsChecked);
        Assert.Null(header.IsChecked);

        Click(window, rows[1]);
        Assert.Equal([rows[2].DataContext], grid.SelectedItems.Cast<object>());
        Assert.False(rows[1].IsChecked);

        Click(window, header);
        Assert.Equal(total, grid.SelectedItems.Count);
        Assert.True(header.IsChecked);
        Assert.All(rows, r => Assert.True(r.IsChecked));

        Click(window, header);
        Assert.Empty(grid.SelectedItems);
        Assert.False(header.IsChecked);
        Assert.All(rows, r => Assert.False(r.IsChecked));

        // A row selected the grid's own way checks its box.
        grid.SelectedItem = rows[0].DataContext;
        Dispatcher.UIThread.RunJobs();
        Assert.True(rows[0].IsChecked);
        window.Close();
    });
}
