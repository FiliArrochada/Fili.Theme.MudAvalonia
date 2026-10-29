using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudMenuItem, from _menu.scss: a row is 8px 12px with a 4px margin round its body1 text, so
/// 48px high; an icon is 24px with 12px after it, and a list that has any icon gives its other
/// rows the same 36px gutter, while a list with none has no column at all.
/// </summary>
public class MenuTests
{
    /// <summary>A menu bar whose one item is open, so its rows are laid out in the dropdown.</summary>
    private static MenuItem[] Open(params MenuItem[] rows)
    {
        var top = new MenuItem { Header = "File" };
        foreach (var row in rows)
        {
            top.Items.Add(row);
        }

        Matrix.Show(new Menu { Items = { top } });
        top.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        return rows;
    }

    private static MenuItem Row(string header, bool icon = false) =>
        new() { Header = header, Icon = icon ? new PathIcon { Data = Avalonia.Media.Geometry.Parse("M0,0 L24,24") } : null };

    private static ContentPresenter IconColumn(MenuItem row) => Matrix.Part<ContentPresenter>(row, "PART_IconPresenter");

    /// <summary>
    /// `.mud-popover` scrolls: a menu longer than the screen scrolls in its sheet, as the forked
    /// base's did, rather than being measured in full and running off the edge.
    /// </summary>
    [Fact]
    public Task ALongMenuScrollsInItsSheet() => UiThread.RunAsync(() =>
    {
        var rows = Open(Enumerable.Range(0, 100).Select(i => Row("Row " + i)).ToArray());
        var scroller = rows[0].GetVisualAncestors().OfType<ScrollViewer>().First();

        Assert.True(scroller.Extent.Height > scroller.Viewport.Height,
            $"100 rows of 48px should overflow the sheet: extent {scroller.Extent.Height}, viewport {scroller.Viewport.Height}");
    });

    [Fact]
    public Task ARowIsMudMenuItemsBody1At48px() => UiThread.RunAsync(() =>
    {
        var row = Open(Row("Open"))[0];

        Assert.Equal(new Thickness(12, 8), row.Padding);
        Assert.Equal(48, row.MinHeight);
        Assert.Equal(16, row.FontSize);
        Assert.Equal(new Thickness(0, 4), Matrix.Part<ContentPresenter>(row, "PART_HeaderPresenter").Margin);
        Assert.True(row.Bounds.Height >= 48);
    });

    [Fact]
    public Task AListWithNoIconsHasNoIconColumn() => UiThread.RunAsync(() =>
    {
        foreach (var row in Open(Row("Open"), Row("Save")))
        {
            Assert.False(IconColumn(row).IsVisible);
        }
    });

    /// <summary>`.mud-menu-list:has(.mud-menu-item-icon)`: every row keeps the 36px, so the labels line up.</summary>
    [Fact]
    public Task OneIconOpensTheColumnOnEveryRow() => UiThread.RunAsync(() =>
    {
        var rows = Open(Row("Open", icon: true), Row("Save"));

        foreach (var row in rows)
        {
            Assert.True(IconColumn(row).IsVisible);
            Assert.Equal(24, IconColumn(row).Width);
            Assert.Equal(new Thickness(0, 0, 12, 0), IconColumn(row).Margin);
        }

        Assert.Equal(
            Matrix.Part<ContentPresenter>(rows[0], "PART_HeaderPresenter").Bounds.X,
            Matrix.Part<ContentPresenter>(rows[1], "PART_HeaderPresenter").Bounds.X);
    });

    [Fact]
    public Task AnIconSetLaterOpensTheColumnAndRemovingItClosesIt() => UiThread.RunAsync(() =>
    {
        var rows = Open(Row("Open"), Row("Save"));

        rows[0].Icon = new PathIcon();
        Dispatcher.UIThread.RunJobs();
        Assert.True(IconColumn(rows[1]).IsVisible);

        rows[0].Icon = null;
        Dispatcher.UIThread.RunJobs();
        Assert.False(IconColumn(rows[1]).IsVisible);
    });

    [Fact]
    public Task TheIconIsActionDefault() => UiThread.RunAsync(() =>
    {
        var row = Open(Row("Open", icon: true))[0];

        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(IconColumn(row).Foreground));
    });

    /// <summary>
    /// Every row is MudMenuItem, however the menu was opened: a <c>MenuFlyout</c>'s rows and a
    /// submenu's submenu too. Neither has a container theme set by a menu of the theme's: a
    /// <c>MenuFlyout</c> binds its own, null, onto its presenter, which beats the presenter's
    /// theme, and a dropdown row named none for its own rows. Both fell through to the base's
    /// Simple row, 24px high with no state layer, until <c>MenuItem</c> was themed by type.
    /// </summary>
    [Fact]
    public Task AFlyoutsRowsAndANestedSubmenusAreMudMenuItems() => UiThread.RunAsync(() =>
    {
        var flyoutRow = Row("Journey");
        var button = Matrix.Show(new Button { Content = "Menu", Flyout = new MenuFlyout { Items = { flyoutRow } } });
        button.Flyout!.ShowAt(button);
        Dispatcher.UIThread.RunJobs();

        var nested = Row("Recent");
        var deepest = Row("Report.pdf");
        nested.Items.Add(deepest);
        Open(nested);
        nested.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();

        foreach (var row in new[] { flyoutRow, deepest })
        {
            Assert.Equal(new Thickness(12, 8), row.Padding);
            Assert.Equal(48, row.MinHeight);
            Assert.NotNull(Matrix.Part<Avalonia.Controls.Border>(row, "PART_StateLayer"));
        }

        // MudMenu.Dense through FlyoutPresenterClasses: the same row, 2px 12px.
        var denseRow = Row("Near me");
        var dense = new MenuFlyout { Items = { denseRow } };
        dense.FlyoutPresenterClasses.Add("dense");
        var denseButton = Matrix.Show(new Button { Content = "Menu", Flyout = dense });
        dense.ShowAt(denseButton);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new Thickness(12, 2), denseRow.Padding);
        Assert.NotNull(Matrix.Part<Avalonia.Controls.Border>(denseRow, "PART_StateLayer"));
    });
}
