using Avalonia;
using Avalonia.Controls;
using Path = Avalonia.Controls.Shapes.Path;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A label longer than its field ends in an ellipsis inside the text's own column. Untrimmed, it
/// ran past the field's edge and under the clear button, an end adornment or a select's arrow.
/// </summary>
public class LongLabelTests
{
    private const string Long = "The full legal name of the account holder, as on the statement";

    private static double Right(Control part, Control field) =>
        part.TranslatePoint(new Point(part.Bounds.Width, 0), field)!.Value.X;

    private static double Left(Control part, Control field) =>
        part.TranslatePoint(default, field)!.Value.X;

    [Theory]
    [InlineData("", false)]
    [InlineData("", true)]
    [InlineData("filled", false)]
    [InlineData("outlined", false)]
    [InlineData("outlined", true)]
    public Task AFieldsLabelStopsAtItsTextColumn(string variant, bool focused) => UiThread.RunAsync(() =>
    {
        var box = new TextBox
        {
            Width = 200,
            PlaceholderText = Long,
            InnerRightContent = new Path { Width = 24, Height = 24, Classes = { "end-icon" } },
        };
        box.Classes.AddRange(variant.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(box);
        if (focused)
        {
            box.Focus();
            Dispatcher.UIThread.RunJobs();
        }

        var label = Matrix.Part<TextBlock>(box, "PART_FloatingLabel");
        var column = Matrix.Part<ScrollViewer>(box, "PART_ScrollViewer");

        Assert.Equal(TextTrimming.CharacterEllipsis, label.TextTrimming);
        Assert.True(Right(label, box) <= Right(column, box) + 0.5, $"The label ends at {Right(label, box)}, past the text column's {Right(column, box)}.");
        Assert.True(label.TextLayout.TextLines[0].HasCollapsed, "The label was not trimmed.");
    });

    /// <summary>A short label is left whole.</summary>
    [Fact]
    public Task AShortLabelIsNotTrimmed() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 200, PlaceholderText = "Name" });

        Assert.False(Matrix.Part<TextBlock>(box, "PART_FloatingLabel").TextLayout.TextLines[0].HasCollapsed);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ASelectsLabelStopsBeforeItsArrow(bool selected) => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 200, PlaceholderText = Long, ItemsSource = new[] { "Gamma" } };
        Matrix.Show(combo);
        if (selected)
        {
            combo.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();
        }

        var label = Matrix.Part<TextBlock>(combo, selected ? "PART_SelectedLabel" : "PART_FloatingLabel");
        var arrow = Matrix.Part<Path>(combo, "PART_Chevron");

        Assert.True(Right(label, combo) <= Left(arrow, combo) + 0.5, $"The label ends at {Right(label, combo)}, under the arrow at {Left(arrow, combo)}.");
        Assert.True(label.TextLayout.TextLines[0].HasCollapsed, "The label was not trimmed.");
    });
}
