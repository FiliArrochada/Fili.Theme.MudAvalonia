using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;
using Ellipse = Avalonia.Controls.Shapes.Ellipse;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// SelectionMode.MultiSelection on MudList and MudTreeView: `multi-selection` for the look, a
/// checkbox on each row in CheckBoxColor and no row marked selected, with the app's
/// SelectionMode="Multiple, Toggle" for the behaviour.
/// </summary>
public class MultiSelectionTests
{
    private static ListBox List(params string[] classes)
    {
        var list = new ListBox { Width = 240, ItemsSource = new[] { "One", "Two" } };
        list.Classes.AddRange(classes);
        if (list.Classes.Contains("multi-selection"))
        {
            list.SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle;
        }

        Matrix.Show(list);
        return list;
    }

    private static ListBoxItem Row(ListBox list, int index) => Assert.IsType<ListBoxItem>(list.ContainerFromIndex(index));

    private static Panel Box(Control row) => Matrix.Part<Panel>(row, "PART_CheckBox");

    private static Path Glyph(Control row, string name) => Matrix.Part<Path>(row, name);

    [Fact]
    public Task APlainListHasNoCheckboxes() => UiThread.RunAsync(() =>
    {
        var list = List();

        Assert.Equal(SelectionMode.Single, list.SelectionMode);
        Assert.False(Box(Row(list, 0)).IsVisible);
    });

    [Fact]
    public Task TheClassShowsTheBoxes() => UiThread.RunAsync(() =>
    {
        Assert.True(Box(Row(List("multi-selection"), 0)).IsVisible);
    });

    /// <summary>
    /// Why the class leaves SelectionMode to the app: a list's selection model, once created,
    /// writes SelectionMode as a local value, and a XAML-declared list creates it at EndInit,
    /// before styles. Pinned so that if Avalonia ever stops doing that, the class can take it on.
    /// </summary>
    [Fact]
    public Task ACreatedSelectionModelPinsSelectionModeLocally() => UiThread.RunAsync(() =>
    {
        var list = new ListBox { ItemsSource = new[] { "One" } };
        list.BeginInit();
        list.EndInit();

        Assert.Equal(Avalonia.Data.BindingPriority.LocalValue, Avalonia.Diagnostics.AvaloniaObjectExtensions.GetDiagnostic(list, ListBox.SelectionModeProperty).Priority);
    });

    /// <summary>The 48px box, 8px into the gutter, the row 48px as MudListItem's is.</summary>
    [Fact]
    public Task TheCheckboxShowsTheRowsSelection() => UiThread.RunAsync(() =>
    {
        var list = List("multi-selection");
        list.SelectedItems!.Add("Two");
        Dispatcher.UIThread.RunJobs();
        var row = Row(list, 1);

        Assert.Equal(48, Box(row).Width);
        Assert.Equal(new Avalonia.Thickness(-8, -8, 0, -8), Box(row).Margin);
        Assert.True(Glyph(row, "PART_CheckChecked").IsVisible);
        Assert.False(Glyph(row, "PART_CheckUnchecked").IsVisible);
        Assert.True(Glyph(Row(list, 0), "PART_CheckUnchecked").IsVisible);
        Assert.Equal(48, row.Bounds.Height);
    });

    [Fact]
    public Task SeveralRowsStaySelected() => UiThread.RunAsync(() =>
    {
        var list = new ListBox
        {
            Width = 240,
            Classes = { "multi-selection" },
            SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle,
            Items = { new ListBoxItem { Content = "A" }, new ListBoxItem { Content = "B" }, new ListBoxItem { Content = "C" } },
        };
        Matrix.Show(list);
        list.SelectedItems!.Add(list.Items[0]);
        list.SelectedItems.Add(list.Items[2]);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, list.SelectedItems.Count);
        Assert.True(Glyph(Row(list, 0), "PART_CheckChecked").IsVisible);
        Assert.True(Glyph(Row(list, 2), "PART_CheckChecked").IsVisible);
    });

    /// <summary>`mud-selected-item` is single selection's: a selected row keeps the text colour and no tint.</summary>
    [Fact]
    public Task ASelectedRowIsNotMarked() => UiThread.RunAsync(() =>
    {
        var list = List("multi-selection", "secondary");
        list.SelectedItems!.Add("One");
        Dispatcher.UIThread.RunJobs();
        var row = Row(list, 0);

        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(row.Foreground));
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Border>(row, "PART_StateLayer").Background));
    });

    [Fact]
    public Task NoColourIsActionDefault() => UiThread.RunAsync(() =>
    {
        var row = Row(List("multi-selection"), 0);

        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(Glyph(row, "PART_CheckUnchecked").Fill));
    });

    /// <summary>CheckBoxColor is both Color and UncheckedColor: either glyph, and the halo's tint.</summary>
    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColourIsTheCheckboxes(string colour) => UiThread.RunAsync(() =>
    {
        var list = List("multi-selection", colour.ToLowerInvariant());
        var row = Row(list, 0);

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Glyph(row, "PART_CheckUnchecked").Fill));
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Glyph(row, "PART_CheckChecked").Fill));

        Matrix.Hover(Box(row));
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(row, "PART_CheckHalo").Fill));
    });

    [Fact]
    public Task ADisabledRowsCheckboxIsTextDisabled() => UiThread.RunAsync(() =>
    {
        var list = List("multi-selection", "primary");
        var row = Row(list, 0);
        row.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Glyph(row, "PART_CheckUnchecked").Fill));
    });

    /// <summary>`.mud-treeview-item-checkbox`: a 32px icon button after the arrow, 24px when dense.</summary>
    [Fact]
    public Task ATreeTakesTheSameCheckbox() => UiThread.RunAsync(() =>
    {
        var leaf = new TreeViewItem { Header = "Leaf" };
        var tree = new TreeView { Width = 240, Classes = { "multi-selection", "success" }, SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle, Items = { leaf } };
        Matrix.Show(tree);
        tree.SelectedItems.Add(leaf);
        Dispatcher.UIThread.RunJobs();

        Assert.True(Box(leaf).IsVisible);
        Assert.Equal(32, Box(leaf).Width);
        Assert.True(Glyph(leaf, "PART_CheckChecked").IsVisible);
        Assert.Equal(Matrix.Token("FiliSuccessColor"), Matrix.Colour(Glyph(leaf, "PART_CheckChecked").Fill));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(Matrix.Part<Avalonia.Controls.Presenters.ContentPresenter>(leaf, "PART_HeaderPresenter").Foreground));

        tree.Classes.Add("dense");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(24, Box(leaf).Width);
    });
}
