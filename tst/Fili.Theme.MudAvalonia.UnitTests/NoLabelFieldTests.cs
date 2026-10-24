using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A text field with no label: MudInputCssHelper adds `mud-input-{variant}-with-label` only with
/// a Label, so with none a standard field is MudInput, the bare 32px input, and a filled one
/// centres its text (`.mud-input-root-hidden-label`). PlaceholderText doubles as the label here,
/// so a field without one is the label-less field.
/// </summary>
public class NoLabelFieldTests
{
    private static TextBox Field(string? label, params string[] classes)
    {
        var box = new TextBox { Width = 240, Text = "Value", PlaceholderText = label };
        foreach (var cls in classes)
        {
            box.Classes.Add(cls);
        }

        return Matrix.Show(box);
    }

    [Theory]
    [InlineData(new string[0], 48, 32)]
    [InlineData(new[] { "dense" }, 45, 29)]
    [InlineData(new[] { "filled" }, 56, 56)]
    [InlineData(new[] { "filled", "dense" }, 48, 48)]
    [InlineData(new[] { "outlined" }, 56, 56)]
    [InlineData(new[] { "outlined", "dense" }, 40, 40)]
    public Task ALabelOnlyTakesRoomWhenThereIsOne(string[] classes, double labelled, double bare) => UiThread.RunAsync(() =>
    {
        Assert.Equal(labelled, Field("Name", classes).Bounds.Height);
        Assert.Equal(bare, Field(null, classes).Bounds.Height);
    });

    /// <summary>The bare standard field's text: 6px above it, as `.mud-input-root`'s padding is.</summary>
    [Fact]
    public Task TheBareFieldsTextSitsUnderItsOwnPadding() => UiThread.RunAsync(() =>
    {
        var box = Field(null);
        var text = Matrix.Part<TextPresenter>(box, "PART_TextPresenter");

        Assert.InRange(text.TranslatePoint(default, box)!.Value.Y, 4, 6);
    });

    /// <summary>A filled field without a label centres its text: 9px higher than with one.</summary>
    [Fact]
    public Task AFilledFieldsTextRisesWithoutALabel() => UiThread.RunAsync(() =>
    {
        double Top(TextBox box) => Matrix.Part<TextPresenter>(box, "PART_TextPresenter").TranslatePoint(default, box)!.Value.Y;

        Assert.Equal(9, Top(Field("Name", "filled")) - Top(Field(null, "filled")), 0.5);
    });

    /// <summary>Giving a field a label later puts its room back.</summary>
    [Fact]
    public Task ALabelSetLaterTakesItsRoom() => UiThread.RunAsync(() =>
    {
        var box = Field(null);
        box.PlaceholderText = "Name";
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(48, box.Bounds.Height);
    });
}
