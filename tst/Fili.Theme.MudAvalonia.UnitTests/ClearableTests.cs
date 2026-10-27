using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudInput's Clearable (MudInput.razor and .razor.cs): a small default-colour MudIconButton with
/// the Clear icon, out of the Tab order, shown by ShowClearButton - never disabled or read-only,
/// and only over text that is not blank - and placed by ClearButtonClassname.
/// </summary>
public class ClearableTests
{
    private static Button ClearButton(TextBox box) => Matrix.Part<Button>(box, "PART_ClearButton");

    private static TextBox Show(string? text, params string[] classes)
    {
        var box = new TextBox { Width = 220, PlaceholderText = "Search", Text = text };
        box.Classes.AddRange(classes);
        return Matrix.Show(box);
    }

    [Fact]
    public Task WithoutTheClassThereIsNoButton() => UiThread.RunAsync(() =>
    {
        Assert.False(ClearButton(Show("Avalonia")).IsEffectivelyVisible);
    });

    /// <summary>`!string.IsNullOrWhiteSpace(stringValue)`: no button over nothing or spaces.</summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Avalonia", true)]
    public Task TheButtonShowsOnlyOverText(string? text, bool shown) => UiThread.RunAsync(() =>
    {
        Assert.Equal(shown, ClearButton(Show(text, "clearable")).IsEffectivelyVisible);
    });

    [Fact]
    public Task ClickingItClearsTheTextAndHidesIt() => UiThread.RunAsync(() =>
    {
        var box = Show("Avalonia", "clearable");
        var button = ClearButton(box);

        button.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("", box.Text ?? "");
        Assert.False(button.IsEffectivelyVisible);

        box.Text = "Again";
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsEffectivelyVisible);
    });

    /// <summary>GetDisabledState and, on a standalone input, GetReadOnlyState.</summary>
    [Fact]
    public Task NeverDisabledOrReadOnly() => UiThread.RunAsync(() =>
    {
        var disabled = Show("Avalonia", "clearable");
        disabled.IsEnabled = false;
        var readOnly = Show("Avalonia", "clearable");
        readOnly.IsReadOnly = true;
        Dispatcher.UIThread.RunJobs();

        Assert.False(ClearButton(disabled).IsEffectivelyVisible);
        Assert.False(ClearButton(readOnly).IsEffectivelyVisible);
    });

    /// <summary>
    /// Color.Default, Size.Small: the small icon button, 3px round an 18px icon. tabindex -1: not a
    /// Tab stop, so Tab leaves the field for the next one.
    /// </summary>
    [Fact]
    public Task ItIsASmallIconButtonOutOfTheTabOrder() => UiThread.RunAsync(() =>
    {
        var button = ClearButton(Show("Avalonia", "clearable"));

        Assert.Equal(["icon", "small"], button.Classes.Where(c => !c.StartsWith(':')));
        Assert.Equal(new Size(24, 24), button.Bounds.Size);
        Assert.False(button.Focusable);
    });

    /// <summary>
    /// ClearButtonClassname: `mud-icon-button-edge-margin-end`, 8px, with no end adornment;
    /// `mud-icon-button-edge-end`, -3px, before one. Either way, left of the adornment.
    /// </summary>
    [Fact]
    public Task ItSitsBeforeAnEndAdornment() => UiThread.RunAsync(() =>
    {
        Assert.Equal(new Thickness(0, 0, 8, 0), ClearButton(Show("Avalonia", "clearable")).Margin);

        var icon = new Border { Width = 24, Height = 24 };
        var box = new TextBox { Width = 220, Text = "Avalonia", Classes = { "clearable" }, InnerRightContent = icon };
        Matrix.Show(box);
        var button = ClearButton(box);

        Assert.Equal(new Thickness(0, 0, -3, 0), button.Margin);
        Assert.True(button.TranslatePoint(default, box)!.Value.X < icon.TranslatePoint(default, box)!.Value.X);
    });

    /// <summary>MudAutocomplete and MudNumericField are MudInputs, Clearable and all.</summary>
    [Fact]
    public Task AWrappedFieldClearsToo() => UiThread.RunAsync(() =>
    {
        var complete = Matrix.Show(new AutoCompleteBox { Width = 220, Text = "Avalonia", Classes = { "clearable" } });
        var number = Matrix.Show(new NumericUpDown { Width = 220, Value = 5, Classes = { "clearable" } });
        Dispatcher.UIThread.RunJobs();

        foreach (TemplatedControl control in new TemplatedControl[] { complete, number })
        {
            var button = ClearButton(Matrix.Part<TextBox>(control, "PART_TextBox"));
            Assert.True(button.IsEffectivelyVisible, control.GetType().Name);
            button.Command!.Execute(null);
        }
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("", complete.Text ?? "");
        Assert.Null(number.Value);
    });
}
