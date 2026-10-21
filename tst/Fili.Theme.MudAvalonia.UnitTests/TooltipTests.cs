using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudTooltip's Color and Arrow, on a ToolTip the app builds itself: `ToolTip.Tip` takes a
/// ToolTip as it is, so its classes survive. The arrow's side comes from the popup's Placement.
/// </summary>
public class TooltipTests
{
    private static ToolTip Open(PlacementMode placement, params string[] classes)
    {
        var tip = new ToolTip { Content = "Delete" };
        tip.Classes.AddRange(classes);
        var target = new Button { Content = "Target" };
        ToolTip.SetTip(target, tip);
        ToolTip.SetPlacement(target, placement);
        Matrix.Show(target);

        ToolTip.SetIsOpen(target, true);
        Dispatcher.UIThread.RunJobs();
        Assert.IsType<Popup>(tip.GetLogicalParent());
        return tip;
    }

    private static Path Arrow(ToolTip tip) => Matrix.Part<Path>(tip, "PART_Arrow");

    /// <summary>`.mud-tooltip` has no width of its own: a long tooltip wraps only where the screen stops it.</summary>
    [Fact]
    public Task ATooltipHasNoWidthOfItsOwn() => UiThread.RunAsync(() =>
    {
        var tip = new ToolTip { Content = "Save" };
        Matrix.Show(new Border { Child = tip });

        Assert.Equal(double.PositiveInfinity, tip.MaxWidth);
    });

    [Fact]
    public Task NoArrowByDefault() => UiThread.RunAsync(() =>
    {
        var tip = Open(PlacementMode.Bottom);

        Assert.False(Arrow(tip).IsVisible);
        Assert.Equal(Matrix.Token("FiliTooltipBackgroundColor"), Matrix.Colour(tip.Background));
    });

    /// <summary>`.mud-tooltip.mud-theme-{color}`: the colour and its contrast text.</summary>
    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColourIsTheChipAndItsContrastText(string colour) => UiThread.RunAsync(() =>
    {
        var tip = Open(PlacementMode.Bottom, colour.ToLowerInvariant());

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(tip.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(tip.Foreground));
    });

    /// <summary>A 12x6 triangle on the side facing the target, 4px short of it, in the chip's colour.</summary>
    [Theory]
    [InlineData(PlacementMode.Top, Dock.Bottom, 12, 6)]
    [InlineData(PlacementMode.Bottom, Dock.Top, 12, 6)]
    [InlineData(PlacementMode.Left, Dock.Right, 6, 12)]
    [InlineData(PlacementMode.Right, Dock.Left, 6, 12)]
    public Task TheArrowPointsAtTheTarget(PlacementMode placement, Dock side, double width, double height) => UiThread.RunAsync(() =>
    {
        var tip = Open(placement, "arrow", "secondary");
        var arrow = Arrow(tip);

        Assert.True(arrow.IsVisible);
        Assert.Equal(side, DockPanel.GetDock(arrow));
        Assert.Equal(width, arrow.Width);
        Assert.Equal(height, arrow.Height);
        Assert.Equal(Matrix.Token("FiliSecondaryColor"), Matrix.Colour(arrow.Fill));
    });

    [Fact]
    public Task APointerPlacedTipHasNoArrow() => UiThread.RunAsync(() =>
    {
        Assert.False(Arrow(Open(PlacementMode.Pointer, "arrow")).IsVisible);
    });
}
