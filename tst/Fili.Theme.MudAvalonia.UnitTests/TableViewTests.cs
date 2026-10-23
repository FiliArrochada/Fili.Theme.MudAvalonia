using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// TableView is MudTable, from _table.scss: the look the DataGrid wears, on Avalonia's own table.
/// </summary>
public class TableViewTests
{
    public sealed record Element(string Name, int Position);

    private static TableView Show(params string[] classes)
    {
        var table = new TableView
        {
            Width = 400,
            Height = 300,
            ItemsSource = new[] { new Element("Hydrogen", 1), new Element("Helium", 2), new Element("Lithium", 3) },
        };
        table.Columns.Add(new TableViewColumn { Header = "Name", Width = new GridLength(1, GridUnitType.Star), CellTemplate = new FuncDataTemplate<Element>((e, _) => new TextBlock { Text = e.Name }) });
        table.Columns.Add(new TableViewColumn { Header = "Position", Width = new GridLength(100), CellTemplate = new FuncDataTemplate<Element>((e, _) => new TextBlock { Text = e.Position.ToString(System.Globalization.CultureInfo.InvariantCulture) }) });
        table.Classes.AddRange(classes);
        return Matrix.Show(table);
    }

    private static List<TableViewRow> Rows(TableView table) => table.GetVisualDescendants().OfType<TableViewRow>().ToList();

    /// <summary>`.mud-table`: text-primary body2 on surface, the default radius, Elevation 1.</summary>
    [Fact]
    public Task TheTableIsAPaperAtElevationOne() => UiThread.RunAsync(() =>
    {
        var table = Show();

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(table.Background));
        Assert.Equal(14, table.FontSize);
        Assert.True(Application.Current!.TryGetResource("FiliElevation1", Application.Current.ActualThemeVariant, out var elevation1));
        Assert.Equal(elevation1, Matrix.Part<Border>(table, "PART_Border").BoxShadow);
    });

    /// <summary>Outlined: a 1px lines-default border and no elevation, as MudPaper adds none when outlined.</summary>
    [Fact]
    public Task OutlinedIsABorderAndNoShadow() => UiThread.RunAsync(() =>
    {
        var table = Show("outlined");

        Assert.Equal(new Thickness(1), table.BorderThickness);
        Assert.Equal(Matrix.Token("FiliLinesDefaultColor"), Matrix.Colour(table.BorderBrush));
        Assert.True(Application.Current!.TryGetResource("FiliElevation0", Application.Current.ActualThemeVariant, out var elevation0));
        Assert.Equal(elevation0, Matrix.Part<Border>(table, "PART_Border").BoxShadow);
    });

    /// <summary>The header cell: subtitle2 at 500, 16px round it, over 1px of table-lines.</summary>
    [Fact]
    public Task TheHeaderIsSubtitle2() => UiThread.RunAsync(() =>
    {
        var table = Show();
        var header = table.GetVisualDescendants().OfType<TableViewColumnHeader>().First();
        var line = table.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_HeaderRow");

        Assert.Equal(14, header.FontSize);
        Assert.Equal(FontWeight.Medium, header.FontWeight);
        Assert.Equal(new Thickness(16), header.Padding);
        Assert.Equal(new Thickness(0, 0, 0, 1), line.BorderThickness);
        Assert.Equal(Matrix.Token("FiliTableLinesColor"), Matrix.Colour(line.BorderBrush));
    });

    /// <summary>A cell is 16px; each row has 1px of table-lines under it, but the last.</summary>
    [Fact]
    public Task ARowIsRuledButTheLast() => UiThread.RunAsync(() =>
    {
        var rows = Rows(Show());

        Assert.Equal(new Thickness(16), rows[0].GetVisualDescendants().OfType<TableViewCell>().First().Padding);
        Assert.Equal(new Thickness(0, 0, 0, 1), rows[0].BorderThickness);
        Assert.Equal(new Thickness(0), rows[^1].BorderThickness);
    });

    /// <summary>`.mud-table-dense`: 6px 24px 6px 16px a cell, 16px on the right of the last.</summary>
    [Fact]
    public Task DenseCellsAreSixPixels() => UiThread.RunAsync(() =>
    {
        var cells = Rows(Show("dense"))[0].GetVisualDescendants().OfType<TableViewCell>().ToList();

        Assert.Equal(new Thickness(16, 6, 24, 6), cells[0].Padding);
        Assert.Equal(new Thickness(16, 6, 16, 6), cells[^1].Padding);
    });

    /// <summary>Striped: table-striped on odd rows; a selected row is primary-hover.</summary>
    [Fact]
    public Task StripesAndSelection() => UiThread.RunAsync(() =>
    {
        var table = Show("striped");
        table.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        var rows = Rows(table);

        Assert.Equal(Matrix.Token("FiliTableStripedColor"), Matrix.Colour(rows[0].Background));
        Assert.Equal(Matrix.Token("FiliPrimaryHoverColor"), Matrix.Colour(rows[1].Background));
        Assert.Equal(Matrix.Token("FiliTableStripedColor"), Matrix.Colour(rows[2].Background));
    });
}
