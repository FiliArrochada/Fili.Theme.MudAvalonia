using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A style a container sets on what it holds stops at its own content: a selector walks the
/// LOGICAL tree, and the items of a menu, flyout or tooltip a control opens are its logical
/// descendants. <c>Button.icon.small PathIcon</c> drew a kebab menu's item icons at 20px, and
/// <c>DataGridColumnHeader CheckBox</c> shrank the boxes of a header's filter flyout.
/// </summary>
public class IconButtonPopupTests
{
    private static void Click(Control target)
    {
        var window = (Window)TopLevel.GetTopLevel(target)!;
        var point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static PathIcon Glyph() => new() { Data = Avalonia.Media.Geometry.Parse("M0,0 L24,24") };

    /// <summary>The size a menu row gives its icon when the menu opens from a plain button.</summary>
    private static Size RowIconSize(string classes, bool contextMenu)
    {
        var rowIcon = Glyph();
        var row = new MenuItem { Header = "Edit", Icon = rowIcon };
        var button = new Button { Content = Glyph() };
        if (contextMenu)
        {
            button.ContextMenu = new ContextMenu { Items = { row } };
        }
        else
        {
            button.Flyout = new MenuFlyout { Items = { row } };
        }

        button.Classes.AddRange(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(button);

        if (contextMenu)
        {
            button.ContextMenu!.Open(button);
            Dispatcher.UIThread.RunJobs();
        }
        else
        {
            Click(button);
        }

        Assert.True(rowIcon.IsAttachedToVisualTree());
        var size = rowIcon.Bounds.Size;
        if (contextMenu)
        {
            button.ContextMenu!.Close();
        }
        else
        {
            button.Flyout!.Hide();
        }

        Dispatcher.UIThread.RunJobs();
        return size;
    }

    [Theory]
    [InlineData("icon")]
    [InlineData("icon small")]
    [InlineData("icon large")]
    [InlineData("icon large filled")]
    [InlineData("fab")]
    public Task AMenuOpenedFromAnIconButtonKeepsTheMenusIconSize(string classes) => UiThread.RunAsync(() =>
    {
        var plain = RowIconSize("", contextMenu: false);

        Assert.Equal(plain, RowIconSize(classes, contextMenu: false));
        Assert.Equal(plain, RowIconSize(classes, contextMenu: true));
    });

    [Theory]
    [InlineData("icon", 24)]
    [InlineData("icon small", 20)]
    [InlineData("icon large", 36)]
    [InlineData("icon large filled", 32)]
    [InlineData("fab", 24)]
    public Task TheButtonsOwnIconKeepsItsSize(string classes, double size) => UiThread.RunAsync(() =>
    {
        var icon = Glyph();
        var button = new Button { Content = icon };
        button.Classes.AddRange(classes.Split(' '));
        Matrix.Show(button);

        Assert.Equal(new Size(size, size), icon.Bounds.Size);
    });

    /// <summary>
    /// Two glyphs in a Panel, the toggled-icon idiom (Icon and ToggledIcon) or a swap an app binds
    /// itself, are the button's icon too: each takes the size it would take alone. A glyph in a
    /// panel a flyout opened from the button shows keeps a plain glyph's size.
    /// </summary>
    [Theory]
    [InlineData(false, "icon", 24)]
    [InlineData(false, "icon small", 20)]
    [InlineData(false, "icon large", 36)]
    [InlineData(false, "icon large outlined", 32)]
    [InlineData(true, "icon", 24)]
    [InlineData(true, "icon small", 20)]
    [InlineData(true, "icon large", 36)]
    [InlineData(true, "icon large filled", 32)]
    public Task GlyphsInAPanelAreSizedAsTheButtonsIcon(bool toggle, string classes, double size) => UiThread.RunAsync(() =>
    {
        var standard = Glyph();
        Matrix.Show(standard);
        var standardSize = standard.Bounds.Size;
        var (off, on) = (Glyph(), Glyph());
        var flyoutIcon = Glyph();
        var panel = new Panel { Children = { off, on } };
        Button button = toggle ? new ToggleButton { Content = panel } : new Button { Content = panel };
        button.Flyout = new Flyout { Content = new StackPanel { Children = { flyoutIcon } } };
        button.Classes.AddRange(classes.Split(' '));
        Matrix.Show(button);

        var shown = toggle ? off : on;
        Assert.Equal(new Size(size, size), shown.Bounds.Size);
        Assert.Equal(size, off.Width);
        Assert.Equal(size, on.Width);

        Click(button);
        Assert.True(flyoutIcon.IsAttachedToVisualTree());
        Assert.Equal(standardSize, flyoutIcon.Bounds.Size);
        button.Flyout.Hide();
        Dispatcher.UIThread.RunJobs();
    });

    private sealed record Row(bool Done);

    /// <summary>
    /// A checkbox column's boxes, and a box in a template column's panel, still take the dense
    /// cell's -4px; a box in a flyout opened from a cell does not.
    /// </summary>
    [Fact]
    public Task ACellsCheckBoxIsDenseAndAFlyoutsFromTheCellIsNot() => UiThread.RunAsync(() =>
    {
        var flyoutBox = new CheckBox { Content = "Show archived" };
        var opener = new Button { Content = "Filter", Flyout = new Flyout { Content = new StackPanel { Children = { flyoutBox } } } };
        var grid = new DataGrid
        {
            Width = 500,
            Height = 200,
            AutoGenerateColumns = false,
            ItemsSource = new AvaloniaList<Row> { new(true) },
            Columns =
            {
                new DataGridCheckBoxColumn { Header = "Done", Binding = new Avalonia.Data.Binding(nameof(Row.Done)) },
                new DataGridTemplateColumn
                {
                    Header = "Pick",
                    CellTemplate = new FuncDataTemplate<Row>((_, _) => new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { new CheckBox() } }),
                },
                new DataGridTemplateColumn
                {
                    Header = "Filter",
                    CellTemplate = new FuncDataTemplate<Row>((_, _) => opener),
                },
            },
        };
        Matrix.Show(grid);

        var cellBoxes = grid.GetVisualDescendants().OfType<CheckBox>().Where(b => b.FindAncestorOfType<DataGridCell>() is not null).ToArray();
        Assert.Equal(2, cellBoxes.Length);
        Assert.All(cellBoxes, box => Assert.Equal(new Thickness(-4), box.Margin));

        Click(opener);

        Assert.True(flyoutBox.IsAttachedToVisualTree());
        Assert.Equal(default, flyoutBox.Margin);
        Assert.Equal(48, Matrix.Part<Panel>(flyoutBox, "PART_Box").Bounds.Width);
        opener.Flyout!.Hide();
        Dispatcher.UIThread.RunJobs();
    });
}
