using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudButtonGroup as <c>button-group</c> on a StackPanel, from _buttongroup.scss: one shape, the
/// lines shared, and the separators between the buttons.
/// </summary>
public class ButtonGroupTests
{
    private static (StackPanel Group, Button[] Buttons) Group(Orientation orientation, params string[] classes)
    {
        var buttons = Enumerable.Range(1, 3).Select(i => new Button { Content = $"B{i}" }).ToArray();
        foreach (var button in buttons)
        {
            button.Classes.AddRange(classes);
        }

        var group = new StackPanel { Orientation = orientation, Classes = { "button-group" } };
        group.Children.AddRange(buttons);
        Matrix.Show(group);
        return (group, buttons);
    }

    /// <summary>`not(:last-child)` and `not(:first-child)`: the radius on the outer corners only.</summary>
    [Fact]
    public Task TheRadiusIsOnTheGroupsOuterCorners() => UiThread.RunAsync(() =>
    {
        var (_, b) = Group(Orientation.Horizontal, "outlined");

        Assert.Equal(new CornerRadius(4, 0, 0, 4), b[0].CornerRadius);
        Assert.Equal(new CornerRadius(0), b[1].CornerRadius);
        Assert.Equal(new CornerRadius(0, 4, 4, 0), b[2].CornerRadius);
    });

    /// <summary>
    /// MudToggleGroup: toggle items in a group are one shape, the radius on the outer corners and
    /// outlined items 1px over each other; and no separator line between them, which a toggle
    /// group does not draw.
    /// </summary>
    [Fact]
    public Task ToggleItemsJoinIntoOneShape() => UiThread.RunAsync(() =>
    {
        var items = new[] { new ToggleButton { Content = "A" }, new ToggleButton { Content = "B" }, new ToggleButton { Content = "C" } };
        var group = new StackPanel { Orientation = Orientation.Horizontal, Classes = { "button-group" } };
        foreach (var item in items)
        {
            group.Children.Add(item);
        }

        Matrix.Show(group);

        Assert.Equal(new CornerRadius(4, 0, 0, 4), items[0].CornerRadius);
        Assert.Equal(new CornerRadius(0), items[1].CornerRadius);
        Assert.Equal(new CornerRadius(0, 4, 4, 0), items[2].CornerRadius);
        Assert.Equal(-1, items[1].Margin.Left);
        Assert.Equal(0, items[1].BorderThickness.Left);
    });

    /// <summary>`margin-left: -1px`: two outlined buttons share one line.</summary>
    [Fact]
    public Task EachButtonAfterTheFirstOverlapsTheOneBefore() => UiThread.RunAsync(() =>
    {
        var (_, b) = Group(Orientation.Horizontal, "outlined");

        Assert.Equal(default, b[0].Margin);
        Assert.Equal(new Thickness(-1, 0, 0, 0), b[1].Margin);
        Assert.Equal(new Thickness(1), b[1].BorderThickness);
        Assert.Equal(b[0].Bounds.Right - 1, b[1].Bounds.Left, 0.5);
    });

    [Fact]
    public Task AVerticalGroupJoinsTopToBottom() => UiThread.RunAsync(() =>
    {
        var (_, b) = Group(Orientation.Vertical, "outlined");

        Assert.Equal(new CornerRadius(4, 4, 0, 0), b[0].CornerRadius);
        Assert.Equal(new CornerRadius(0), b[1].CornerRadius);
        Assert.Equal(new CornerRadius(0, 0, 4, 4), b[2].CornerRadius);
        Assert.Equal(new Thickness(0, -1, 0, 0), b[2].Margin);
    });

    /// <summary>`.mud-button-group-text`: a 1px line in the button's colour before each after the first.</summary>
    [Theory]
    [InlineData(null, "FiliTextPrimaryColor")]
    [InlineData("primary", "FiliPrimaryColor")]
    public Task TextButtonsAreSeparatedInTheirColour(string? colour, string token) => UiThread.RunAsync(() =>
    {
        var (_, b) = Group(Orientation.Horizontal, colour is null ? [] : [colour]);

        Assert.Equal(default, b[0].BorderThickness);
        Assert.Equal(new Thickness(1, 0, 0, 0), b[1].BorderThickness);
        Assert.Equal(Matrix.Token(token), Matrix.Colour(b[1].BorderBrush));
    });

    /// <summary>`.mud-button-group-filled`: `{color}-lighten` between, divider with no colour, and no resting shadow.</summary>
    [Theory]
    [InlineData(null, "FiliDividerColor")]
    [InlineData("primary", "FiliPrimaryLightenColor")]
    public Task FilledButtonsAreSeparatedAndRestFlat(string? colour, string token) => UiThread.RunAsync(() =>
    {
        var (_, b) = Group(Orientation.Horizontal, colour is null ? ["filled"] : ["filled", colour]);

        Assert.Equal(new Thickness(1, 0, 0, 0), b[2].BorderThickness);
        Assert.Equal(Matrix.Token(token), Matrix.Colour(b[2].BorderBrush));
        foreach (var button in b)
        {
            Assert.Equal(FabTests.Shadow(0), Matrix.Part<Border>(button, "PART_Root").BoxShadow);
        }
    });

    /// <summary>A hidden button leaves the group, and one button alone is an ordinary button.</summary>
    [Fact]
    public Task TheGroupFollowsItsVisibleButtons() => UiThread.RunAsync(() =>
    {
        var (group, b) = Group(Orientation.Horizontal, "outlined");

        b[2].IsVisible = false;
        Assert.Equal(new CornerRadius(0, 4, 4, 0), b[1].CornerRadius);

        group.Children.Remove(b[1]);
        Assert.Equal(new CornerRadius(4), b[0].CornerRadius);
        Assert.DoesNotContain(b[1].Classes, c => c.StartsWith(":joined"));

        b[2].IsVisible = true;
        Assert.Equal(new CornerRadius(4, 0, 0, 4), b[0].CornerRadius);
        Assert.Equal(new Thickness(-1, 0, 0, 0), b[2].Margin);
    });

    [Fact]
    public Task APlainStackPanelOfButtonsIsLeftAlone() => UiThread.RunAsync(() =>
    {
        var buttons = new[] { new Button { Content = "A" }, new Button { Content = "B" } };
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.AddRange(buttons);
        Matrix.Show(panel);

        Assert.All(buttons, button => Assert.Equal(new CornerRadius(4), button.CornerRadius));
        Assert.Equal(default, buttons[1].Margin);
        Assert.Equal(default, buttons[1].BorderThickness);
    });
}

/// <summary>MudPopover's paper, from _popover.scss: no padding of its own.</summary>
public class FlyoutPaddingTests
{
    [Fact]
    public Task AFlyoutLeavesThePaddingToItsContent() => UiThread.RunAsync(() =>
    {
        var presenter = Matrix.Show(new FlyoutPresenter { Content = new TextBlock { Text = "Inside" } });

        Assert.Equal(default, presenter.Padding);
    });
}
