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
}
