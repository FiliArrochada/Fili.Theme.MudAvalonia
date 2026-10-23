using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A button is MudButton's height: `.mud-button` sets the button typography's line-height, 1.75
/// (Typography.cs), so the label is 1.75 of its font size tall whatever it holds, and every
/// variant of a size comes out the same - 6 + 24.5 + 6 = 36.5px at 14px, 4 + 22.75 + 4 = 30.75
/// small, 8 + 26.25 + 8 = 42.25 large; an outlined button's 1px line is inside its smaller padding.
/// The text alone is 17px, and a button was 29px: 7.5px short, and 32 with an icon beside the
/// label. Layout rounding draws each up to the next pixel: 37, 31 and 43.
/// </summary>
public class ButtonHeightTests
{
    private static T Show<T>(T control) where T : Control
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        control.VerticalAlignment = VerticalAlignment.Top;
        Matrix.Show(new StackPanel { Children = { control } });
        return control;
    }

    private static PathIcon Icon(string slot) => new() { Classes = { slot }, Data = Geometry.Parse("M0 0h2v2H0z") };

    [Theory]
    [InlineData("", 36.5)]
    [InlineData("small", 30.75)]
    [InlineData("large", 42.25)]
    [InlineData("outlined", 36.5)]
    [InlineData("outlined small", 30.75)]
    [InlineData("outlined large", 42.25)]
    [InlineData("filled primary", 36.5)]
    [InlineData("filled primary small", 30.75)]
    [InlineData("filled primary large", 42.25)]
    public Task AButtonIsMudButtonsHeight(string classes, double height) => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save" };
        if (classes.Length > 0) button.Classes.AddRange(classes.Split(' '));
        Show(button);

        Assert.Equal(Math.Ceiling(height), button.Bounds.Height);
    });

    /// <summary>An icon beside the label does not change it: the line box is the label's.</summary>
    [Fact]
    public Task AnIconBesideTheLabelKeepsTheHeight() => UiThread.RunAsync(() =>
    {
        var button = Show(new Button
        {
            Classes = { "filled", "primary" },
            Content = new StackPanel { Orientation = Orientation.Horizontal, Children = { Icon("start-icon"), new TextBlock { Text = "Save" } } },
        });

        Assert.Equal(37, button.Bounds.Height);
    });

    /// <summary>
    /// `.mud-icon-button` is its padding round the icon, with no label line: 48px, 24 small, and
    /// a chip and a FAB keep their own fixed heights.
    /// </summary>
    [Theory]
    [InlineData("icon", 48)]
    [InlineData("icon small", 24)]
    [InlineData("outlined icon", 36)]
    [InlineData("filled icon", 36)]
    [InlineData("chip", 32)]
    [InlineData("chip small", 24)]
    [InlineData("fab", 56)]
    public Task AShapeOfItsOwnKeepsItsHeight(string classes, double height) => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = classes.StartsWith("chip", StringComparison.Ordinal) ? "Chip" : new PathIcon { Data = Geometry.Parse("M0 0h2v2H0z") } };
        button.Classes.AddRange(classes.Split(' '));
        Show(button);

        Assert.Equal(Math.Ceiling(height), button.Bounds.Height);
    });

    /// <summary>A split button and a drop-down button are MudButtons too: 36.5px, where they were 32 and 36.</summary>
    [Fact]
    public Task SplitAndDropDownButtonsAreAButtonsHeight() => UiThread.RunAsync(() =>
    {
        var split = Show(new SplitButton { Content = "Split" });
        var drop = Show(new DropDownButton { Content = "Drop" });

        Assert.Equal(37, split.Bounds.Height);
        Assert.Equal(37, drop.Bounds.Height);
    });
}
