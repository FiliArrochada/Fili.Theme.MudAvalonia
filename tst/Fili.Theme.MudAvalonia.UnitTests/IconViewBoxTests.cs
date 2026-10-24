using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudIcon draws its glyph in an svg with ViewBox="0 0 24 24" and sizes the svg, so a Material
/// glyph keeps the padding round it. A PathIcon here does the same: the path at its own
/// coordinates on a 24px canvas, scaled to the icon's size - not its bounds stretched to the box.
/// </summary>
public class IconViewBoxTests
{
    // Material's PlayArrow: 11px wide and 14px high, from (8,5), in its 24px box.
    private static Geometry Play => Geometry.Parse("M8 5v14l11-7z");

    private static Point Where(PathIcon icon, Point glyph) =>
        icon.GetVisualDescendants().OfType<Path>().Single().TranslatePoint(glyph, icon)!.Value;

    [Fact]
    public Task AGlyphKeepsItsPlaceInTheBox() => UiThread.RunAsync(() =>
    {
        var icon = Matrix.Show(new PathIcon { Data = Play });

        Assert.Equal(24, icon.Bounds.Width);
        Assert.Equal(new Point(8, 5), Where(icon, new Point(8, 5)));
        Assert.Equal(new Point(19, 12), Where(icon, new Point(19, 12)));
    });

    /// <summary>A size class scales the whole box, padding and all: `large` is 36px, 1.5 times.</summary>
    [Fact]
    public Task ASizeScalesTheBox() => UiThread.RunAsync(() =>
    {
        var icon = Matrix.Show(new PathIcon { Data = Play, Classes = { "large" } });

        Assert.Equal(36, icon.Bounds.Width);
        Assert.Equal(new Point(12, 7.5), Where(icon, new Point(8, 5)));
    });

    /// <summary>In an icon button, as everywhere: the button sizes the icon, not the glyph's bounds.</summary>
    [Fact]
    public Task AnIconButtonsGlyphKeepsItsPlace() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Data = Play };
        Matrix.Show(new Button { Classes = { "icon" }, Content = icon });

        Assert.Equal(new Point(8, 5), Where(icon, new Point(8, 5)));
    });
}
