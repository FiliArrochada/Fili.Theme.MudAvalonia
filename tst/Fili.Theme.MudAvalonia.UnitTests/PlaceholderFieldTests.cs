using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudTextField's Placeholder: with `placeholder`, PlaceholderText is the hint in the empty field
/// rather than the floating label. No label and no room for one, the hint where the text goes, in
/// the text's colour at .42 (`.mud-input-root::placeholder`), gone once there is text.
/// </summary>
public class PlaceholderFieldTests
{
    private static TextBox Field(params string[] classes)
    {
        var box = new TextBox { Width = 240, PlaceholderText = "Add a note" };
        box.Classes.Add("placeholder");
        foreach (var cls in classes)
        {
            box.Classes.Add(cls);
        }

        return Matrix.Show(box);
    }

    [Fact]
    public Task TheHintShowsWhileEmptyAndTheLabelNever() => UiThread.RunAsync(() =>
    {
        var box = Field();
        var hint = Matrix.Part<TextBlock>(box, "PART_Placeholder");

        Assert.True(hint.IsVisible);
        Assert.Equal("Add a note", hint.Text);
        Assert.Equal(0.42, hint.Opacity);
        Assert.False(Matrix.Part<TextBlock>(box, "PART_FloatingLabel").IsVisible);

        box.Text = "Read again";
        Dispatcher.UIThread.RunJobs();
        Assert.False(hint.IsVisible);
    });

    /// <summary>No label, so no room for one: the bare field's 32px, 29 dense.</summary>
    [Theory]
    [InlineData(new string[0], 32)]
    [InlineData(new[] { "dense" }, 29)]
    [InlineData(new[] { "filled" }, 56)]
    [InlineData(new[] { "outlined" }, 56)]
    public Task AFieldWithAPlaceholderHasNoLabelsRoom(string[] classes, double height) => UiThread.RunAsync(() =>
    {
        Assert.Equal(height, Field(classes).Bounds.Height);
    });

    /// <summary>The hint starts where the text does, after a start adornment too.</summary>
    [Fact]
    public Task TheHintSitsWhereTheTextGoes() => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 240, PlaceholderText = "Search", InnerLeftContent = new Border { Width = 24, Height = 24 } };
        box.Classes.Add("placeholder");
        Matrix.Show(box);

        var hint = Matrix.Part<TextBlock>(box, "PART_Placeholder").TranslatePoint(default, box)!.Value;
        var text = Matrix.Part<TextPresenter>(box, "PART_TextPresenter").TranslatePoint(default, box)!.Value;

        Assert.Equal(text.X, hint.X);
        Assert.Equal(text.Y, hint.Y, 1);
    });

    /// <summary>A field without the class keeps PlaceholderText as its floating label.</summary>
    [Fact]
    public Task WithoutTheClassItIsTheLabel() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 240, PlaceholderText = "Name" });

        Assert.False(Matrix.Part<TextBlock>(box, "PART_Placeholder").IsVisible);
        Assert.True(Matrix.Part<TextBlock>(box, "PART_FloatingLabel").IsVisible);
        Assert.Equal(48, box.Bounds.Height);
    });
}
