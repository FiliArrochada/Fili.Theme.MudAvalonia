using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudBlazor's Color.Inherit beyond Button and MudLink, `.mud-inherit-text { color: inherit }`:
/// MudText's Color, MudCheckBox's and MudRadio's Color and UncheckedColor, and MudSelect's
/// AdornmentColor. Each takes the colour of whatever the control sits in, and follows it.
/// </summary>
public class InheritColourTests
{
    private static T InHost<T>(T control, IBrush colour) where T : Control
    {
        var host = new Border { Child = control };
        host.SetValue(TextElement.ForegroundProperty, colour);
        Matrix.Show(host);
        return control;
    }

    /// <summary>Over a type class's own colour: a caption is text-secondary until told otherwise.</summary>
    [Fact]
    public Task TextTakesTheSurroundingColourOverItsTypeClass() => UiThread.RunAsync(() =>
    {
        var text = InHost(new TextBlock { Text = "Caption", Classes = { "caption", "inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(text.Foreground));
    });

    [Fact]
    public Task TextFollowsTheSurroundingColour() => UiThread.RunAsync(() =>
    {
        var text = InHost(new TextBlock { Text = "Body", Classes = { "body2", "inherit" } }, Brushes.OrangeRed);

        ((Border)text.Parent!).SetValue(TextElement.ForegroundProperty, Brushes.SeaGreen);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(Colors.SeaGreen, Matrix.Colour(text.Foreground));
    });

    /// <summary>
    /// Every glyph in the surrounding colour, the label keeping its own text-primary, and no halo:
    /// `hover:mud-inherit-hover` names no rule.
    /// </summary>
    [Theory]
    [InlineData(true, "PART_Checked")]
    [InlineData(false, "PART_Unchecked")]
    public Task ACheckBoxGlyphTakesTheSurroundingColour(bool isChecked, string glyph) => UiThread.RunAsync(() =>
    {
        var box = InHost(new CheckBox { IsChecked = isChecked, Content = "Box", Classes = { "inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(box, glyph).Fill));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(box.Foreground));

        Matrix.Hover(box);
        Assert.Equal(Colors.Transparent, Matrix.Colour(Matrix.Part<Ellipse>(box, "PART_StateLayer").Fill));
    });

    /// <summary>UncheckedColor.Inherit: while not checked only; checked, the colour class is back.</summary>
    [Fact]
    public Task UncheckedInheritHoldsOnlyWhileUnchecked() => UiThread.RunAsync(() =>
    {
        var off = InHost(new CheckBox { IsChecked = false, Content = "Off", Classes = { "primary", "unchecked-inherit" } }, Brushes.OrangeRed);
        Assert.Equal(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(off, "PART_Unchecked").Fill));

        var on = InHost(new CheckBox { IsChecked = true, Content = "On", Classes = { "primary", "unchecked-inherit" } }, Brushes.OrangeRed);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Matrix.Part<Path>(on, "PART_Checked").Fill));
    });

    [Fact]
    public Task ARadioGlyphTakesTheSurroundingColour() => UiThread.RunAsync(() =>
    {
        var radio = InHost(new RadioButton { IsChecked = true, Content = "On", GroupName = "inherit", Classes = { "inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(radio, "PART_Checked").Fill));
    });

    [Fact]
    public Task ADisabledCheckBoxIsStillGrey() => UiThread.RunAsync(() =>
    {
        var box = InHost(new CheckBox { IsChecked = true, IsEnabled = false, Content = "Box", Classes = { "inherit" } }, Brushes.OrangeRed);

        Assert.NotEqual(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(box, "PART_Checked").Fill));
    });

    [Fact]
    public Task ASelectArrowTakesTheAdornmentColour() => UiThread.RunAsync(() =>
    {
        var select = Matrix.Show(new ComboBox { Width = 200, Classes = { "adornment-error" } });

        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(Matrix.Part<Path>(select, "PART_Chevron").Fill));
    });

    [Fact]
    public Task ASelectArrowTakesTheSurroundingColour() => UiThread.RunAsync(() =>
    {
        var select = InHost(new ComboBox { Width = 200, Classes = { "adornment-inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(select, "PART_Chevron").Fill));
    });

    [Fact]
    public Task ADisabledSelectArrowIsTextDisabled() => UiThread.RunAsync(() =>
    {
        var select = InHost(new ComboBox { Width = 200, IsEnabled = false, Classes = { "adornment-inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Matrix.Part<Path>(select, "PART_Chevron").Fill));
    });
}
