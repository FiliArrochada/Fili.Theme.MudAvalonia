using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudDataGrid on Avalonia's DataGrid, from _table.scss, _datagrid.scss and MudDataGrid.razor.cs.
/// Nothing is opted into: a DataGrid is themed by type.
/// </summary>
public class DataGridTests
{
    public sealed record Element(string Name, int Number, string Group);

    private static readonly Element[] Elements =
    [
        new("Hydrogen", 1, "Nonmetal"), new("Helium", 2, "Noble gas"), new("Lithium", 3, "Alkali metal"),
    ];

    private static DataGrid Show(object? items = null, params string[] classes)
    {
        var grid = new DataGrid
        {
            ItemsSource = (System.Collections.IEnumerable)(items ?? Elements),
            Width = 400,
            Columns =
            {
                new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Element.Name)) },
                new DataGridTextColumn { Header = "Nr", Binding = new Binding(nameof(Element.Number)) },
            },
        };
        grid.Classes.AddRange(classes);
        Matrix.Show(grid);
        Dispatcher.UIThread.RunJobs();
        return grid;
    }

    private static List<DataGridRow> Rows(DataGrid grid) =>
        grid.GetVisualDescendants().OfType<DataGridRow>().Where(r => r.IsVisible).OrderBy(r => r.Index).ToList();

    private static DataGridColumnHeader Header(DataGrid grid, string text) =>
        grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First(h => Equals(h.Content, text));

    private static IBrush? Fill(DataGridRow row) => Matrix.Part<Rectangle>(row, "BackgroundRectangle").Fill;

    [Fact]
    public Task ADataGridIsPaperAtElevation1() => UiThread.RunAsync(() =>
    {
        var grid = Show();
        var border = Matrix.Part<Border>(grid, "DataGridBorder");

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(border.Background));
        Assert.Equal(new CornerRadius(4), border.CornerRadius);
        Assert.Equal(FabTests.Shadow(1), border.BoxShadow);
        Assert.False(grid.ClipToBounds);
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(grid.Foreground));
        Assert.Equal(DataGridHeadersVisibility.Column, grid.HeadersVisibility);
    });

    /// <summary>$header-height 59px; subtitle2; a table-lines rule under the headers.</summary>
    [Fact]
    public Task TheHeaderRowIs59PixelsOfSubtitle2() => UiThread.RunAsync(() =>
    {
        var grid = Show();
        var header = Header(grid, "Name");

        Assert.Equal(59, header.Bounds.Height);
        Assert.Equal(FontWeight.Medium, header.FontWeight);
        Assert.Equal(14, header.FontSize);
        Assert.Equal(22, Matrix.Part<Avalonia.Controls.Presenters.ContentPresenter>(header, "PART_ContentPresenter").LineHeight);
        Assert.Equal(Matrix.Token("FiliTableLinesColor"), Matrix.Colour(Matrix.Part<Rectangle>(grid, "PART_ColumnHeadersAndRowsSeparator").Fill));
    });

    /// <summary>16px round a 20px body2 line, and the 1px rule: 53px.</summary>
    [Fact]
    public Task ABodyRowIs53PixelsOfBody2() => UiThread.RunAsync(() =>
    {
        var grid = Show();
        var row = Rows(grid)[0];
        var text = row.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == "Hydrogen");

        Assert.Equal(53, row.Bounds.Height);
        Assert.Equal(20, text.LineHeight);
        Assert.Equal(16, text.TranslatePoint(default, row)!.Value.X);
        Assert.Equal(Matrix.Token("FiliTableLinesColor"), Matrix.Colour(grid.HorizontalGridLinesBrush));
        Assert.Equal(DataGridGridLinesVisibility.Horizontal, grid.GridLinesVisibility);
    });

    /// <summary>`.mud-table-root` takes the last row's rule away: the grid ends 1px short of it.</summary>
    [Fact]
    public Task TheLastRowHasNoRule() => UiThread.RunAsync(() =>
    {
        var grid = Show();

        Assert.Equal(59 + (3 * 53) - 1, grid.Bounds.Height);
    });

    [Fact]
    public Task DenseIs6By16And24WithA39PixelHeader() => UiThread.RunAsync(() =>
    {
        var grid = Show(null, "dense");
        var cell = Rows(grid)[0].GetVisualDescendants().OfType<DataGridCell>().First();

        Assert.Equal(new Thickness(16, 6, 24, 6), cell.Padding);
        Assert.Equal(33, Rows(grid)[0].Bounds.Height);
        Assert.Equal(39, Header(grid, "Name").Bounds.Height);
    });

    /// <summary>
    /// `.mud-table-dense .mud-table-cell:last-child`: 16px on the right of the last column, header
    /// included - and the last column is the last VISIBLE one in display order, so hiding or moving
    /// a column moves the padding with it.
    /// </summary>
    [Fact]
    public Task DensePadsTheLastColumn16OnTheRight() => UiThread.RunAsync(() =>
    {
        var grid = Show(null, "dense");
        Thickness Right(string header) => new(16, 6, Equals(header, "Nr") ? 16 : 24, 6);
        DataGridCell Cell(int column) => Rows(grid)[0].GetVisualDescendants().OfType<DataGridCell>()
            .Single(c => DataGridColumn.GetColumnContainingElement(c) == grid.Columns[column]);

        Assert.Equal(Right("Name"), Cell(0).Padding);
        Assert.Equal(Right("Nr"), Cell(1).Padding);
        Assert.Equal(new Thickness(16, 0, 24, 0), Header(grid, "Name").Padding);
        Assert.Equal(new Thickness(16, 0, 16, 0), Header(grid, "Nr").Padding);

        grid.Columns[1].DisplayIndex = 0;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Thickness(16, 6, 16, 6), Cell(0).Padding);
        Assert.Equal(new Thickness(16, 6, 24, 6), Cell(1).Padding);

        grid.Columns[0].IsVisible = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Thickness(16, 6, 16, 6), Cell(1).Padding);
    });

    /// <summary>Without `dense` every cell keeps the 16px all round.</summary>
    [Fact]
    public Task TheLastColumnIsPaddedLikeTheRestWhenNotDense() => UiThread.RunAsync(() =>
    {
        var grid = Show();
        Assert.All(Rows(grid)[0].GetVisualDescendants().OfType<DataGridCell>(), cell => Assert.Equal(new Thickness(16), cell.Padding));
    });

    /// <summary>`:nth-of-type(odd)` counts from one: the rows at index 0 and 2.</summary>
    [Fact]
    public Task StripedPaintsTheFirstThirdAndFifthRows() => UiThread.RunAsync(() =>
    {
        var rows = Rows(Show(null, "striped"));

        Assert.Equal([true, false, true], rows.Select(r => Matrix.Part<Border>(r, "PART_Stripe").IsVisible));
        Assert.Equal(Matrix.Token("FiliTableStripedColor"), Matrix.Colour(Matrix.Part<Border>(rows[0], "PART_Stripe").Background));
    });

    [Fact]
    public Task NoStripesWithoutTheClass() => UiThread.RunAsync(() =>
    {
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Border>(Rows(Show())[0], "PART_Stripe").Background));
    });

    /// <summary>Hover is off by default, as MudDataGrid.Hover is.</summary>
    [Fact]
    public Task HoverTintsTheRowOnlyWithTheClass() => UiThread.RunAsync(() =>
    {
        var plain = Rows(Show())[1];
        Matrix.Hover(plain);
        Assert.Equal(Colors.Transparent, Matrix.Colour(Fill(plain)));

        var hovered = Rows(Show(null, "hover"))[1];
        Matrix.Hover(hovered);
        Assert.Equal(Matrix.Token("FiliTableHoverColor"), Matrix.Colour(Fill(hovered)));
    });

    /// <summary>`.mud-table-hover.mud-table-striped`: the hover replaces the stripe.</summary>
    [Fact]
    public Task HoverReplacesTheStripe() => UiThread.RunAsync(() =>
    {
        var row = Rows(Show(null, "hover", "striped"))[0];
        Matrix.Hover(row);

        Assert.Equal(Matrix.Token("FiliTableHoverColor"), Matrix.Colour(Fill(row)));
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Border>(row, "PART_Stripe").Background));
    });

    /// <summary>The one departure: MudDataGrid draws no selection, so a selected list row's tint.</summary>
    [Fact]
    public Task ASelectedRowIsPrimaryHover() => UiThread.RunAsync(() =>
    {
        var grid = Show(null, "striped");
        grid.SelectedIndex = 0;
        Dispatcher.UIThread.RunJobs();
        var row = Rows(grid)[0];

        Assert.Equal(Matrix.Token("FiliPrimaryHoverColor"), Matrix.Colour(Fill(row)));
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Border>(row, "PART_Stripe").Background));
    });

    /// <summary>Lines between body cells; never between headers, which MudDataGrid does not border.</summary>
    [Fact]
    public Task BorderedRulesTheBodyAndNotTheHeaders() => UiThread.RunAsync(() =>
    {
        var grid = Show(null, "bordered");

        Assert.Equal(DataGridGridLinesVisibility.All, grid.GridLinesVisibility);
        Assert.Equal(Matrix.Token("FiliTableLinesColor"), Matrix.Colour(grid.VerticalGridLinesBrush));
        // The column headers; the corner headers are hidden and never templated.
        Assert.All(grid.GetVisualDescendants().OfType<DataGridColumnHeader>().Where(h => h.Content is string),
            h => Assert.False(Matrix.Part<Rectangle>(h, "VerticalSeparator").IsVisible));
        Assert.Contains(Rows(grid)[0].GetVisualDescendants().OfType<Rectangle>(), r => r.Name == "PART_RightGridLine" && r.IsVisible);
    });

    [Fact]
    public Task OutlinedIsALineAndNoShadowWhateverTheElevation() => UiThread.RunAsync(() =>
    {
        var grid = Show(null, "outlined", "elevation8");
        var border = Matrix.Part<Border>(grid, "DataGridBorder");

        Assert.Equal(0, border.BoxShadow.Count);
        Assert.Equal(new Thickness(1), border.BorderThickness);
        Assert.Equal(Matrix.Token("FiliLinesDefaultColor"), Matrix.Colour(border.BorderBrush));
    });

    [Fact]
    public Task SquareAndElevation() => UiThread.RunAsync(() =>
    {
        var grid = Show(null, "square", "elevation8");
        var border = Matrix.Part<Border>(grid, "DataGridBorder");

        Assert.Equal(new CornerRadius(0), border.CornerRadius);
        Assert.Equal(FabTests.Shadow(8), border.BoxShadow);
    });

    /// <summary>MudIconButton's ArrowUpward: hidden until sorted, turned for descending.</summary>
    [Fact]
    public Task TheSortArrowShowsOnlyOnTheSortedColumn() => UiThread.RunAsync(() =>
    {
        var view = new DataGridCollectionView(Elements);
        view.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(Element.Name), System.ComponentModel.ListSortDirection.Descending));
        var grid = Show(view);
        var sorted = Header(grid, "Name");
        var unsorted = Header(grid, "Nr");

        Assert.Equal(1, Matrix.Part<Border>(sorted, "PART_SortButton").GetBaseValue(Visual.OpacityProperty).GetValueOrDefault());
        Assert.Equal(0, Matrix.Part<Border>(unsorted, "PART_SortButton").GetBaseValue(Visual.OpacityProperty).GetValueOrDefault());
        var turn = Assert.IsType<TransformOperations>(Matrix.Part<Viewbox>(sorted, "PART_SortIcon").GetBaseValue(Visual.RenderTransformProperty).GetValueOrDefault());
        Assert.Equal(TransformOperations.Parse("rotate(180deg)").Value, turn.Value);

        var icon = Matrix.Part<Viewbox>(sorted, "PART_SortIcon");
        Assert.Equal(18, icon.Width);
        Assert.Equal(new Thickness(5), Matrix.Part<Border>(sorted, "PART_SortButton").Padding);
    });

    /// <summary>`.mud-datagrid-group`: background-gray, "Title: Key", no count, no row indent.</summary>
    [Fact]
    public Task AGroupRowIsBackgroundGrayAndNamesItsProperty() => UiThread.RunAsync(() =>
    {
        var view = new DataGridCollectionView(Elements);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(Element.Group)));
        var grid = Show(view);
        var group = grid.GetVisualDescendants().OfType<DataGridRowGroupHeader>().First();

        Assert.Equal(Matrix.Token("FiliBackgroundGrayColor"), Matrix.Colour(group.Background));
        Assert.True(group.IsPropertyNameVisible);
        Assert.False(group.IsItemCountVisible);
        Assert.Equal(0, group.SublevelIndent);

        var text = Rows(grid)[0].GetVisualDescendants().OfType<TextBlock>().First(t => t.Text is not null);
        Assert.Equal(16, text.TranslatePoint(default, Rows(grid)[0])!.Value.X);
    });

    /// <summary>
    /// `.mud-row-group-indented-{level}`: a nested group's header starts (level - 1) * 48px in, the
    /// top level keeps the cell's 16, and the data rows under either are not moved.
    /// </summary>
    [Fact]
    public Task ANestedGroupHeaderIsIndented48PxPerLevel() => UiThread.RunAsync(() =>
    {
        var view = new DataGridCollectionView(Elements);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(Element.Group)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(Element.Number)));
        var grid = Show(view, "dense");
        grid.Height = 600;
        Dispatcher.UIThread.RunJobs();

        var headers = grid.GetVisualDescendants().OfType<DataGridRowGroupHeader>().Where(h => h.IsVisible)
            .OrderBy(h => h.TranslatePoint(default, grid)!.Value.Y).ToList();
        double ExpanderX(DataGridRowGroupHeader header) =>
            Matrix.Part<ToggleButton>(header, "PART_ExpanderButton").TranslatePoint(default, header)!.Value.X;

        Assert.Equal(16, ExpanderX(headers[0]));
        Assert.Equal(48, ExpanderX(headers[1]));

        var text = Rows(grid)[0].GetVisualDescendants().OfType<TextBlock>().First(t => t.Text is not null);
        Assert.Equal(16, text.TranslatePoint(default, Rows(grid)[0])!.Value.X);
    });

    /// <summary>Levels 3, 4 and 5 go on in 48px steps, and a deeper one stays at level 5's.</summary>
    [Fact]
    public Task GroupIndentStepsTo192AndStops() => UiThread.RunAsync(() =>
    {
        var levels = Enumerable.Range(0, 6).Select(_ => nameof(Element.Group)).ToArray();
        var view = new DataGridCollectionView(Elements);
        foreach (var level in levels)
        {
            view.GroupDescriptions.Add(new DataGridPathGroupDescription(level));
        }

        var grid = Show(view);
        grid.Height = 900;
        Dispatcher.UIThread.RunJobs();

        var expanders = grid.GetVisualDescendants().OfType<DataGridRowGroupHeader>().Where(h => h.IsVisible)
            .OrderBy(h => h.TranslatePoint(default, grid)!.Value.Y).Take(6)
            .Select(h => Matrix.Part<ToggleButton>(h, "PART_ExpanderButton").TranslatePoint(default, h)!.Value.X);

        Assert.Equal([16d, 48, 96, 144, 192, 192], expanders);
    });

    /// <summary>`.edit-mode-cell`: no resting line, the cell's font size.</summary>
    [Fact]
    public Task TheEditBoxHasNoRestingLine() => UiThread.RunAsync(() =>
    {
        Assert.True(Application.Current!.TryFindResource("DataGridCellTextBoxTheme", ThemeVariant.Light, out var value));
        var box = new TextBox { Theme = Assert.IsType<ControlTheme>(value), Text = "Hydrogen" };
        Matrix.Show(box);

        Assert.False(Matrix.Part<Border>(box, "PART_Underline").IsVisible);
        Assert.Equal(14, box.FontSize);
        Assert.Equal(0, box.MinHeight);
    });

    /// <summary>
    /// The one StaticResource that leaves Themes/Optional/DataGrid: the editor is MudBlazor's
    /// standard field. It resolves only because OptionalStyleInclude passes the theme above it to
    /// the loader; lose that and the editor is a bare TextBox theme with nothing to say so.
    /// </summary>
    [Fact]
    public Task TheEditBoxIsTheStandardField() => UiThread.RunAsync(() =>
    {
        Assert.True(Application.Current!.TryFindResource("DataGridCellTextBoxTheme", ThemeVariant.Light, out var editor));
        Assert.True(Application.Current!.TryFindResource("FiliStandardTextBox", ThemeVariant.Light, out var standard));
        Assert.Same(standard, Assert.IsType<ControlTheme>(editor).BasedOn);
    });

    /// <summary>The group row's ExpandMore and ChevronRight, dynamic references to Icons.axaml.</summary>
    [Fact]
    public Task AGroupRowsGlyphFollowsItsState() => UiThread.RunAsync(() =>
    {
        var view = new DataGridCollectionView(Elements);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(Element.Group)));
        var grid = Show(view);
        var expander = Matrix.Part<ToggleButton>(
            grid.GetVisualDescendants().OfType<DataGridRowGroupHeader>().First(), "PART_ExpanderButton");
        var glyph = Matrix.Part<Avalonia.Controls.Shapes.Path>(expander, "PART_Glyph");

        Assert.True(Application.Current!.TryFindResource("FiliExpandMoreGeometry", ThemeVariant.Light, out var more));
        Assert.True(Application.Current!.TryFindResource("FiliChevronRightGeometry", ThemeVariant.Light, out var right));

        expander.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(more, glyph.Data);

        expander.IsChecked = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(right, glyph.Data);
    });

    /// <summary>
    /// `.mud-table-cell .mud-checkbox`: margin -4px, and the icon button padded 4px rather than 12,
    /// so a selection column's 32px circle takes 24px of the row: 16 + 24 + 16 and the rule, 57px,
    /// where the 48px circle a checkbox has elsewhere made it 81.
    /// </summary>
    [Theory]
    [InlineData(null, 32)]
    [InlineData("small", 28)]
    [InlineData("large", 44)]
    public Task ACheckBoxInACellIsTheTablesCompactOne(string? size, double circle) => UiThread.RunAsync(() =>
    {
        var grid = new DataGrid
        {
            ItemsSource = Elements,
            Width = 400,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = "Pick",
                    CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<Element>((_, _) =>
                    {
                        var box = new CheckBox();
                        if (size is not null)
                        {
                            box.Classes.Add(size);
                        }

                        return box;
                    }),
                },
                new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Element.Name)) },
            },
        };
        Matrix.Show(grid);
        Dispatcher.UIThread.RunJobs();

        var box = grid.GetVisualDescendants().OfType<CheckBox>().First();
        var part = Matrix.Part<Panel>(box, "PART_Box");

        Assert.Equal(circle, part.Bounds.Width);
        Assert.Equal(circle, part.Bounds.Height);
        Assert.Equal(new Thickness(-4), box.Margin);
        Assert.Equal(16 + circle - 8 + 16 + 1, Rows(grid)[0].Bounds.Height);

        // Outside a cell, the same checkbox keeps its 48px circle.
        var loose = Matrix.Show(new CheckBox());
        Assert.Equal(48, Matrix.Part<Panel>(loose, "PART_Box").Bounds.Width);
    });
}
