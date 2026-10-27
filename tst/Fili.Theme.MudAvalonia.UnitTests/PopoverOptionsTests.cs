using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudPopover's Paper, Square, DropShadow and Elevation (MudPopover.razor.cs) on the three popover
/// sheets: a flyout's presenter and a menu flyout's, which take them through
/// FlyoutPresenterClasses, and a context menu. `mud-elevation-{n}` only when Paper and DropShadow,
/// `mud-paper-square` only when Paper, and no `mud-paper` at all when Paper is false.
/// </summary>
public class PopoverOptionsTests
{
    public static readonly TheoryData<string> Sheets = ["Flyout", "MenuFlyout", "ContextMenu"];

    /// <summary>The sheet's Border, open, with <paramref name="classes"/> on its presenter or menu.</summary>
    private static Border Sheet(string kind, params string[] classes)
    {
        var target = Matrix.Show(new Border { Width = 50, Height = 20 });
        switch (kind)
        {
            case "Flyout":
                var text = new TextBlock { Text = "Inside" };
                var flyout = new Flyout { Content = text };
                flyout.FlyoutPresenterClasses.AddRange(classes);
                flyout.ShowAt(target);
                Dispatcher.UIThread.RunJobs();
                return Matrix.Part<Border>(text.GetVisualAncestors().OfType<FlyoutPresenter>().First(), "PART_Border");
            case "MenuFlyout":
                var row = new MenuItem { Header = "Rename" };
                var menu = new MenuFlyout { Items = { row } };
                menu.FlyoutPresenterClasses.AddRange(classes);
                menu.ShowAt(target);
                Dispatcher.UIThread.RunJobs();
                return Matrix.Part<Border>(row.GetVisualAncestors().OfType<MenuFlyoutPresenter>().First(), "PART_Border");
            default:
                var context = new ContextMenu { Items = { new MenuItem { Header = "Copy" } } };
                context.Classes.AddRange(classes);
                target.ContextMenu = context;
                context.Open(target);
                Dispatcher.UIThread.RunJobs();
                return Matrix.Part<Border>(context, "PART_Border");
        }
    }

    private static CornerRadius DefaultRadius()
    {
        Assert.True(Application.Current!.TryFindResource("FiliCornerRadius", ThemeVariant.Light, out var value));
        return Assert.IsType<CornerRadius>(value);
    }

    /// <summary>With no class: Paper, at MudPopover's default Elevation 8, with the default radius.</summary>
    [Theory]
    [MemberData(nameof(Sheets))]
    public Task WithNoClassItIsPaperAtElevation8(string kind) => UiThread.RunAsync(() =>
    {
        var sheet = Sheet(kind);

        Assert.Equal(FabTests.Shadow(8), sheet.BoxShadow);
        Assert.Equal(DefaultRadius(), sheet.CornerRadius);
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(sheet.Background));
    });

    /// <summary>Elevation: every level _elevation.scss has, 0 to 25.</summary>
    [Theory]
    [MemberData(nameof(Sheets))]
    public Task EveryElevationClassIsItsLevel(string kind) => UiThread.RunAsync(() =>
    {
        foreach (var level in ThemeColourGenerator.Elevations)
        {
            Assert.Equal(FabTests.Shadow(level), Sheet(kind, $"elevation{level}").BoxShadow);
        }
    });

    /// <summary>
    /// Square is `mud-paper-square`, no radius; DropShadow="false" is `flat`, no shadow whatever the
    /// level; Paper="false" is `no-paper`, no surface, radius or shadow.
    /// </summary>
    [Theory]
    [MemberData(nameof(Sheets))]
    public Task SquareFlatAndNoPaper(string kind) => UiThread.RunAsync(() =>
    {
        var square = Sheet(kind, "square");
        Assert.Equal(default, square.CornerRadius);
        Assert.Equal(FabTests.Shadow(8), square.BoxShadow);

        Assert.Equal(FabTests.Shadow(0), Sheet(kind, "flat").BoxShadow);
        Assert.Equal(FabTests.Shadow(0), Sheet(kind, "flat", "elevation12").BoxShadow);

        var bare = Sheet(kind, "no-paper", "elevation12");
        Assert.Equal(Colors.Transparent, Matrix.Colour(bare.Background));
        Assert.Equal(default, bare.CornerRadius);
        Assert.Equal(FabTests.Shadow(0), bare.BoxShadow);
    });
}
