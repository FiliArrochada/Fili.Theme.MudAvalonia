using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Where an empty, unfocused field's label rests. In the standard field it sits ON the text line,
/// where the value will be typed: _inputlabel.scss rests it at translate(0, 24px) over a text line
/// 22px down. Centred in the whole field instead, it hung 7px above the line (6.5 dense). In the
/// filled and outlined fields it is centred in the box, translate(…, 20px) in 56px, by design.
/// A select is the same field, so the same rule holds for it.
/// </summary>
public class RestingLabelTests
{
    private static double Middle(Control part, Control field) =>
        part.TranslatePoint(new Point(0, part.Bounds.Height / 2), field)!.Value.Y;

    [Theory]
    [InlineData("")]
    [InlineData("dense")]
    public Task AStandardFieldsLabelRestsOnTheTextLine(string variant) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, PlaceholderText = "Name" };
        box.Classes.AddRange(variant.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(box);

        var label = Matrix.Part<TextBlock>(box, "PART_FloatingLabel");
        var text = Matrix.Part<TextPresenter>(box, "PART_TextPresenter");

        Assert.InRange(Middle(label, box) - Middle(text, box), -1, 1);
    });

    [Theory]
    [InlineData("filled")]
    [InlineData("filled dense")]
    [InlineData("outlined")]
    [InlineData("outlined dense")]
    public Task AFilledOrOutlinedLabelRestsInTheBoxsCentre(string variant) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, PlaceholderText = "Name" };
        box.Classes.AddRange(variant.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(box);

        var label = Matrix.Part<TextBlock>(box, "PART_FloatingLabel");
        var root = Matrix.Part<Border>(box, "PART_Root");

        // Dense filled rests 1px low, as it always has (Controls/TextBox.axaml's table).
        Assert.InRange(Middle(label, box) - Middle(root, box), -1, 1.5);
    });

    [Theory]
    [InlineData("")]
    [InlineData("dense")]
    public Task AStandardSelectsLabelRestsOnTheTextLine(string variant) => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 200, PlaceholderText = "Game", ItemsSource = new[] { "Gamma" } };
        combo.Classes.AddRange(variant.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(combo);

        var label = Matrix.Part<TextBlock>(combo, "PART_FloatingLabel");
        var area = Matrix.Part<Grid>(combo, "PART_ContentArea");

        Assert.InRange(Middle(label, combo) - Middle(area, combo), -1, 1);
    });
}
