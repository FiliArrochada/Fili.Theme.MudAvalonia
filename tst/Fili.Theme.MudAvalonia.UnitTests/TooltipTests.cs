using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

    /// <summary>
    /// `.mud-tooltip`: `line-height: 1.4em` and `text-align: center` (_tooltip.scss). A one-line
    /// chip is 4 + 16.8 + 4px; the text had its font's own 15px line, start-aligned.
    /// </summary>
    [Fact]
    public Task TheTextIsCentredOnA1Point4EmLine() => UiThread.RunAsync(() =>
    {
        var tip = Open(PlacementMode.Bottom);
        var text = tip.GetVisualDescendants().OfType<TextBlock>().First();

        Assert.Equal(12 * 1.4, text.LineHeight, 6);
        Assert.Equal(Avalonia.Media.TextAlignment.Center, text.TextAlignment);
        Assert.Equal(Math.Ceiling(4 + 12 * 1.4 + 4), tip.GetVisualDescendants().OfType<Border>().First().Bounds.Height);
    });

    /// <summary>Without `arrow` the arrow's place is kept, as the gap, but nothing is drawn in it.</summary>
    [Fact]
    public Task NoArrowByDefault() => UiThread.RunAsync(() =>
    {
        var tip = Open(PlacementMode.Bottom);

        Assert.Equal(0, Arrow(tip).Opacity);
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
        Assert.Equal(1, arrow.Opacity);
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

    /// <summary>
    /// `.mud-tooltip-{placement}`: centred on that side of the target, 10px off, with or without
    /// the arrow - not Avalonia's ToolTip.VerticalOffset of 20 down, which put a Top tooltip over
    /// its target and Left and Right ones 20px low.
    /// </summary>
    [Theory]
    [InlineData(PlacementMode.Top, false)]
    [InlineData(PlacementMode.Bottom, false)]
    [InlineData(PlacementMode.Left, false)]
    [InlineData(PlacementMode.Right, false)]
    [InlineData(PlacementMode.Top, true)]
    [InlineData(PlacementMode.Bottom, true)]
    [InlineData(PlacementMode.Left, true)]
    [InlineData(PlacementMode.Right, true)]
    public Task ATooltipSitsCentredTenPixelsOffItsSide(PlacementMode placement, bool arrow) => UiThread.RunAsync(() =>
    {
        var (target, tip) = OpenInRoom(placement, arrow ? ["arrow"] : []);
        var t = new Rect(target.PointToScreen(default).ToPoint(1), target.Bounds.Size);
        var chip = Chip(tip);

        switch (placement)
        {
            case PlacementMode.Top:
                Assert.Equal(t.Top - 10, chip.Bottom, 0.5);
                break;
            case PlacementMode.Bottom:
                Assert.Equal(t.Bottom + 10, chip.Top, 0.5);
                break;
            case PlacementMode.Left:
                Assert.Equal(t.Left - 10, chip.Right, 0.5);
                break;
            default:
                Assert.Equal(t.Right + 10, chip.Left, 0.5);
                break;
        }

        if (placement is PlacementMode.Top or PlacementMode.Bottom)
        {
            Assert.Equal(t.Center.X, chip.Center.X, 1.0);
        }
        else
        {
            Assert.Equal(t.Center.Y, chip.Center.Y, 1.0);
        }
    });

    /// <summary>Pointer, Avalonia's default and not MudBlazor's, keeps Avalonia's 20px under the pointer.</summary>
    [Fact]
    public Task APointerPlacedTipKeepsAvaloniasOffset() => UiThread.RunAsync(() =>
    {
        var (target, _) = OpenInRoom(PlacementMode.Pointer, []);

        Assert.Equal(20, ToolTip.GetVerticalOffset(target));
    });

    /// <summary>The placement's offset is set where a style would set it: an app's own wins.</summary>
    [Fact]
    public Task AnAppsOwnOffsetWins() => UiThread.RunAsync(() =>
    {
        var target = new Button { Content = "Target" };
        ToolTip.SetVerticalOffset(target, 6);
        var (_, tip) = OpenInRoom(PlacementMode.Bottom, [], target);

        Assert.Equal(6, ToolTip.GetVerticalOffset(target));
        Assert.Equal(target.PointToScreen(new Point(0, target.Bounds.Height)).Y + 16, Chip(tip).Top, 0.5);
    });

    /// <summary>A target moved back to Pointer gets Avalonia's offset back the next time it opens.</summary>
    [Fact]
    public Task BackOnPointerTheOffsetReturns() => UiThread.RunAsync(() =>
    {
        var (target, _) = OpenInRoom(PlacementMode.Top, []);
        Assert.Equal(0, ToolTip.GetVerticalOffset(target));

        ToolTip.SetIsOpen(target, false);
        ToolTip.SetPlacement(target, PlacementMode.Pointer);
        ToolTip.SetIsOpen(target, true);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(20, ToolTip.GetVerticalOffset(target));
    });

    /// <summary>A target in the middle of a window, so no placement has to flip at its edge.</summary>
    private static (Button Target, ToolTip Tip) OpenInRoom(PlacementMode placement, string[] classes, Button? target = null)
    {
        target ??= new Button { Content = "Target" };
        var tip = new ToolTip { Content = "Delete" };
        tip.Classes.AddRange(classes);
        ToolTip.SetTip(target, tip);
        ToolTip.SetPlacement(target, placement);
        var window = new Window
        {
            Content = new Border { Padding = new Thickness(200), Child = target },
            Width = 600,
            Height = 600,
            Position = new PixelPoint(100, 100),
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ToolTip.SetIsOpen(target, true);
        Dispatcher.UIThread.RunJobs();
        return (target, tip);
    }

    /// <summary>The chip itself, on screen: the tooltip less the arrow's side.</summary>
    private static Rect Chip(ToolTip tip)
    {
        var chip = tip.GetVisualDescendants().OfType<Border>().Single();
        return new Rect(chip.PointToScreen(default).ToPoint(1), chip.Bounds.Size);
    }
}
