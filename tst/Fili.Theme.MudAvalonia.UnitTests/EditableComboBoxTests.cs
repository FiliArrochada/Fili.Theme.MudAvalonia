using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// ComboBox.IsEditable. Avalonia types into the template's PART_EditableTextBox and looks it up
/// without complaint when it is missing, so a theme without the part turned an editable select
/// into one that took focus and ignored every key. The part is a bare input in the selected
/// item's place, inside the one field.
/// </summary>
public class EditableComboBoxTests
{
    private static ComboBox Show(bool editable, params string[] classes)
    {
        var combo = new ComboBox { Width = 260, PlaceholderText = "Game", IsEditable = editable, ItemsSource = new[] { "Gamma", "Delta" } };
        combo.Classes.AddRange(classes);
        return Matrix.Show(combo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("filled")]
    [InlineData("outlined")]
    public Task TypingReachesTheText(string variant) => UiThread.RunAsync(() =>
    {
        var combo = Show(true, variant.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var input = Matrix.Part<TextBox>(combo, "PART_EditableTextBox");

        input.Focus();
        Dispatcher.UIThread.RunJobs();
        ((Window)TopLevel.GetTopLevel(combo)!).KeyTextInput("Gam");
        Dispatcher.UIThread.RunJobs();

        Assert.True(input.IsFocused);
        Assert.Equal("Gam", combo.Text);
        Assert.Equal("Gam", input.Text);
    });

    /// <summary>
    /// The field round the input is the select's: typed text floats the label, as a value does,
    /// and focus on the input is the select's focus, its accent rule out.
    /// </summary>
    [Fact]
    public Task TypedTextFloatsTheLabel() => UiThread.RunAsync(() =>
    {
        var combo = Show(true);
        var input = Matrix.Part<TextBox>(combo, "PART_EditableTextBox");

        Assert.True(Matrix.Part<TextBlock>(combo, "PART_FloatingLabel").IsVisible);

        input.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Matrix.Part<TextBlock>(combo, "PART_FloatingLabel").Foreground));

        input.Text = "Zeta";
        Dispatcher.UIThread.RunJobs();
        Assert.Null(combo.SelectionBoxItem);
        Assert.False(Matrix.Part<TextBlock>(combo, "PART_FloatingLabel").IsVisible);
        Assert.True(Matrix.Part<TextBlock>(combo, "PART_SelectedLabel").IsVisible);
    });

    /// <summary>One Tab stop either way: the input when editable, the select when not.</summary>
    [Fact]
    public Task OnlyTheEditableSelectShowsTheInput() => UiThread.RunAsync(() =>
    {
        var plain = Show(false);
        var editable = Show(true);

        Assert.False(Matrix.Part<TextBox>(plain, "PART_EditableTextBox").IsVisible);
        Assert.True(Matrix.Part<Avalonia.Controls.Presenters.ContentPresenter>(plain, "PART_ContentPresenter").IsVisible);
        Assert.True(plain.IsTabStop);

        Assert.True(Matrix.Part<TextBox>(editable, "PART_EditableTextBox").IsVisible);
        Assert.False(Matrix.Part<Avalonia.Controls.Presenters.ContentPresenter>(editable, "PART_ContentPresenter").IsVisible);
        Assert.False(editable.IsTabStop);
    });

    /// <summary>The input has no chrome of its own: no label, no rule, no second field.</summary>
    [Fact]
    public Task TheInputIsBare() => UiThread.RunAsync(() =>
    {
        var input = Matrix.Part<TextBox>(Show(true), "PART_EditableTextBox");

        Assert.DoesNotContain(input.GetVisualDescendants(), v => v is Control { Name: "PART_FloatingLabel" or "PART_Underline" });
    });
}
