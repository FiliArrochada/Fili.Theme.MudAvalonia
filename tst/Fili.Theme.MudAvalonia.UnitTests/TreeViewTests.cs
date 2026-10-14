using Avalonia.Controls;
using Avalonia.Media;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudTreeView's hover, from _treeview.scss: `.mud-treeview-hover .mud-treeview-item-content:hover`
/// is action-default-hover, and only when <c>Hover</c> is set, which it is not by default.
/// </summary>
public class TreeViewTests
{
    private static (TreeView Tree, TreeViewItem Branch, TreeViewItem Leaf) Show(params string[] classes)
    {
        var leaf = new TreeViewItem { Header = "Installed" };
        var branch = new TreeViewItem { Header = "Library", IsExpanded = true, Items = { leaf } };
        var tree = new TreeView { Width = 220, Height = 140, Items = { branch } };
        tree.Classes.AddRange(classes);
        Matrix.Show(tree);
        return (tree, branch, leaf);
    }

    private static Border Row(TreeViewItem item) => Matrix.Part<Border>(item, "SelectionBorder");

    /// <summary>The row's resting value, past its 150ms background transition.</summary>
    private static IBrush? Ground(TreeViewItem item) => Row(item).GetBaseValue(Border.BackgroundProperty).GetValueOrDefault();

    private static bool Tinted(TreeViewItem item) =>
        Ground(item) is ISolidColorBrush brush && brush.Color == Matrix.Token("FiliOverlayHoverColor");

    [Fact]
    public Task WithoutHoverTheRowUnderThePointerIsNotTinted() => UiThread.RunAsync(() =>
    {
        var (_, _, leaf) = Show();

        Matrix.Hover(Row(leaf));

        Assert.False(Tinted(leaf));
    });

    /// <summary>The row, not the item: hovering a child leaves its branch's row alone.</summary>
    [Fact]
    public Task HoverTintsOnlyTheRowUnderThePointer() => UiThread.RunAsync(() =>
    {
        var (_, branch, leaf) = Show("hover");

        Matrix.Hover(Row(leaf));

        Assert.True(Tinted(leaf));
        Assert.True(branch.IsPointerOver);
        Assert.False(Tinted(branch));
    });

    [Fact]
    public Task ASelectedRowKeepsItsOwnTintUnderThePointer() => UiThread.RunAsync(() =>
    {
        var (tree, _, leaf) = Show("hover");
        tree.SelectedItem = leaf;

        Matrix.Hover(Row(leaf));

        Assert.Equal(Matrix.Token("FiliPrimaryHoverColor"), Matrix.Colour(Ground(leaf)));
    });
}
