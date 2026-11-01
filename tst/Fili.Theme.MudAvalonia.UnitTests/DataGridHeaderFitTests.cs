using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A narrow data grid column: the sort button takes room only on a column that can sort, as
/// MudDataGrid renders it only then; a title wider than its column ends in an ellipsis; and the
/// scroll bars take their 8px rail's width, not the base's 12px.
/// </summary>
public class DataGridHeaderFitTests
{
    public sealed record Row(string Name, string Description);

    private static readonly Row[] Rows = [.. Enumerable.Range(0, 40).Select(i => new Row($"Item {i}", "A description"))];

    private static DataGrid Show(bool gridSorts = true, bool nameSorts = true, double nameWidth = 200, double height = double.NaN)
    {
        var grid = new DataGrid
        {
            ItemsSource = Rows,
            Width = 400,
            Height = height,
            CanUserSortColumns = gridSorts,
            Columns =
            {
                new DataGridTextColumn { Header = "Name of the item", Binding = new Binding(nameof(Row.Name)), CanUserSort = nameSorts, Width = new DataGridLength(nameWidth) },
                new DataGridTextColumn { Header = "Description", Binding = new Binding(nameof(Row.Description)) },
            },
        };
        Matrix.Show(grid);
        Dispatcher.UIThread.RunJobs();
        return grid;
    }

    private static DataGridColumnHeader Header(DataGrid grid, string text) =>
        grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First(h => Equals(h.Content, text));

    [Fact]
    public Task ASortableColumnKeepsItsSortButton() => UiThread.RunAsync(() =>
    {
        var grid = Show();

        Assert.True(Matrix.Part<Border>(Header(grid, "Name of the item"), "PART_SortButton").IsVisible);
    });

    [Fact]
    public Task AColumnThatCannotSortHasNoSortButton() => UiThread.RunAsync(() =>
    {
        var grid = Show(nameSorts: false);

        Assert.False(Matrix.Part<Border>(Header(grid, "Name of the item"), "PART_SortButton").IsVisible);
        Assert.True(Matrix.Part<Border>(Header(grid, "Description"), "PART_SortButton").IsVisible);
    });

    [Fact]
    public Task AGridThatCannotSortHasNoSortButtons() => UiThread.RunAsync(() =>
    {
        var grid = Show(gridSorts: false);

        Assert.All(
            grid.GetVisualDescendants().OfType<DataGridColumnHeader>().Where(h => h.Content is string),
            h => Assert.False(Matrix.Part<Border>(h, "PART_SortButton").IsVisible));
    });

    /// <summary>Without the button, the title has the whole column less its 16px padding a side.</summary>
    [Fact]
    public Task WithoutTheButtonTheTitleHasTheColumn() => UiThread.RunAsync(() =>
    {
        var grid = Show(nameSorts: false, nameWidth: 120);
        var header = Header(grid, "Name of the item");
        var title = Matrix.Part<Avalonia.Controls.Presenters.ContentPresenter>(header, "PART_ContentPresenter");

        Assert.Equal(header.Bounds.Width - 32, title.Bounds.Width, 1);
    });

    [Fact]
    public Task ANarrowTitleEndsInAnEllipsis() => UiThread.RunAsync(() =>
    {
        var grid = Show(nameWidth: 90);
        var header = Header(grid, "Name of the item");
        var text = header.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "Name of the item");

        Assert.Equal(TextTrimming.CharacterEllipsis, text.TextTrimming);
        Assert.True(text.TextLayout.TextLines[0].HasCollapsed, "The title was not trimmed.");
        var right = text.TranslatePoint(new Point(text.Bounds.Width, 0), header)!.Value.X;
        var button = Matrix.Part<Border>(header, "PART_SortButton");
        Assert.True(right <= button.TranslatePoint(default, header)!.Value.X + 0.5, "The title runs under the sort button.");
    });

    [Fact]
    public Task TheScrollBarsTakeTheirRailsWidth() => UiThread.RunAsync(() =>
    {
        var grid = Show(nameWidth: 500, height: 200);
        var vertical = grid.GetVisualDescendants().OfType<ScrollBar>().Single(b => b.Name == "PART_VerticalScrollbar");
        var horizontal = grid.GetVisualDescendants().OfType<ScrollBar>().Single(b => b.Name == "PART_HorizontalScrollbar");

        Assert.True(vertical.IsVisible, $"vertical hidden; grid {grid.Bounds}");
        Assert.True(horizontal.IsVisible, $"horizontal hidden; grid {grid.Bounds}");
        Assert.Equal(8, vertical.Bounds.Width);
        Assert.Equal(8, horizontal.Bounds.Height);
    });
}
