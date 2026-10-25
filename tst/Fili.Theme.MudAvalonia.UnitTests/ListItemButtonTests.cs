using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudListItem with OnClick or Href, outside a selecting list: `list-item` on a Button. The list
/// row's look - 48px of body1, 36 of body2 dense, the row's hover tint - with the label in its own
/// case, since a list item is not a MudButton.
/// </summary>
public class ListItemButtonTests
{
    private static Button Row(params string[] classes)
    {
        var row = new Button { Content = "Open settings" };
        row.Classes.Add("list-item");
        foreach (var cls in classes)
        {
            row.Classes.Add(cls);
        }

        Matrix.Show(new StackPanel { Width = 300, Children = { row } });
        return row;
    }

    [Theory]
    [InlineData(new string[0], 48, 16)]
    [InlineData(new[] { "dense" }, 36, 14)]
    public Task ARowIsAListRowsHeight(string[] classes, double height, double fontSize) => UiThread.RunAsync(() =>
    {
        var row = Row(classes);

        Assert.Equal(height, row.Bounds.Height);
        Assert.Equal(fontSize, row.FontSize);
        Assert.Equal(300, row.Bounds.Width);
    });

    [Fact]
    public Task ItKeepsItsLabelsCase() => UiThread.RunAsync(() =>
    {
        var row = Row();
        var text = row.GetVisualDescendants().OfType<TextBlock>().Single();

        Assert.Equal("Open settings", text.Text);
    });

    [Fact]
    public Task TheTextStartsAfterTheGutter() => UiThread.RunAsync(() =>
    {
        double Start(Button row) => Matrix.Part<ContentPresenter>(row, "PART_ContentPresenter").TranslatePoint(default, row)!.Value.X;

        Assert.Equal(16, Start(Row()));
        Assert.Equal(0, Start(Row("no-gutters")));
    });

    [Fact]
    public Task HoverIsTheRowsTint() => UiThread.RunAsync(() =>
    {
        var row = Row();
        // The tint fades in over 150ms, which the headless clock stalls part way: the end value
        // is what this pins, so the fade is taken off the layer under test.
        Matrix.Part<Border>(row, "PART_StateLayer").Transitions = null;
        Matrix.Hover(row);

        Assert.Equal(Matrix.Token("FiliOverlayHoverColor"), Matrix.Colour(Matrix.Part<Border>(row, "PART_StateLayer").Background));
    });

    /// <summary>
    /// `.mud-list-item-icon`, MudListItem's Icon: a start-icon is 24px in a 56px column, in
    /// action-default, and the text beside it is centred on it - in a list row and a list-item.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ARowsIconIsMudListItemsIcon(bool clickable) => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" }, Data = Avalonia.Media.Geometry.Parse("M2 2h20v20H2z") };
        var label = new TextBlock { Text = "Inbox" };
        var content = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { icon, label } };
        Control row = clickable ? new Button { Classes = { "list-item" }, Content = content } : new ListBoxItem { Content = content };
        Matrix.Show(new StackPanel { Width = 300, Children = { clickable ? row : new ListBox { Items = { row } } } });

        double Middle(Control c) => c.TranslatePoint(new Point(0, c.Bounds.Height / 2), row)!.Value.Y;

        Assert.Equal(24, icon.Bounds.Width);
        Assert.Equal(56, label.TranslatePoint(default, icon)!.Value.X);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(icon.Foreground));
        Assert.Equal(Middle(icon), Middle(label), 0.5);
    });

    [Fact]
    public Task ADisabledRowsIconIsActionDisabled() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" }, Data = Avalonia.Media.Geometry.Parse("M2 2h20v20H2z") };
        var row = new Button { Classes = { "list-item" }, IsEnabled = false, Content = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { icon, new TextBlock { Text = "Off" } } } };
        Matrix.Show(row);

        Assert.Equal(Matrix.Token("FiliActionDisabledColor"), Matrix.Colour(icon.Foreground));
    });

    /// <summary>A button in a row keeps its own icon colour: the row's grey stops at the button.</summary>
    [Fact]
    public Task AButtonInARowKeepsItsIconColour() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" }, Data = Avalonia.Media.Geometry.Parse("M2 2h20v20H2z") };
        var button = new Button { Classes = { "filled", "primary" }, Content = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { icon, new TextBlock { Text = "Save" } } } };
        Matrix.Show(new ListBox { Width = 300, Items = { new ListBoxItem { Content = button } } });

        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(icon.Foreground));
    });
}
