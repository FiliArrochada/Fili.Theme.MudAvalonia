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

    /// <summary>`.mud-typography-*` sets no colour: a caption or an overline is the colour round it.</summary>
    [Theory]
    [InlineData("caption")]
    [InlineData("overline")]
    public Task ATypeClassHasNoColourOfItsOwn(string type) => UiThread.RunAsync(() =>
    {
        var text = InHost(new TextBlock { Text = "Caption", Classes = { type } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(text.Foreground));
    });

    /// <summary>Over a colour class too: `inherit` is the last word on a TextBlock's colour.</summary>
    [Fact]
    public Task TextTakesTheSurroundingColourOverAColourClass() => UiThread.RunAsync(() =>
    {
        var text = InHost(new TextBlock { Text = "Caption", Classes = { "caption", "error", "inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(text.Foreground));
    });

    /// <summary>A link's caption is the link's colour, as `mud-typography-caption` on a MudLink is.</summary>
    [Fact]
    public Task ALinksCaptionIsTheLinksColour() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Terms", Classes = { "caption" } };
        var link = Matrix.Show(new HyperlinkButton { Content = text });

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(text.Foreground));
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
    /// Every glyph in the surrounding colour, the label keeping its own text-primary, and the grey
    /// action-default-hover halo: `hover:mud-inherit-hover` names no rule, so the glyph's
    /// `.mud-icon-button` hover (_iconbutton.scss) stays.
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
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(box, "PART_StateLayer").Fill));
    });

    /// <summary>
    /// UncheckedColor.Inherit: while not checked only; checked, the colour class is back. Unchecked,
    /// the halo is the grey one, not the colour class's: MudCheckBox drops `hover:mud-{Color}-hover`
    /// when UncheckedColor is set and the box is off (MudCheckBox.razor.cs).
    /// </summary>
    [Fact]
    public Task UncheckedInheritHoldsOnlyWhileUnchecked() => UiThread.RunAsync(() =>
    {
        var off = InHost(new CheckBox { IsChecked = false, Content = "Off", Classes = { "primary", "unchecked-inherit" } }, Brushes.OrangeRed);
        Assert.Equal(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(off, "PART_Unchecked").Fill));
        Matrix.Hover(off);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(off, "PART_StateLayer").Fill));

        var on = InHost(new CheckBox { IsChecked = true, Content = "On", Classes = { "primary", "unchecked-inherit" } }, Brushes.OrangeRed);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Matrix.Part<Path>(on, "PART_Checked").Fill));
    });

    [Fact]
    public Task ARadioGlyphTakesTheSurroundingColour() => UiThread.RunAsync(() =>
    {
        var radio = InHost(new RadioButton { IsChecked = true, Content = "On", GroupName = "inherit", Classes = { "inherit" } }, Brushes.OrangeRed);

        Assert.Equal(Colors.OrangeRed, Matrix.Colour(Matrix.Part<Path>(radio, "PART_Checked").Fill));

        Matrix.Hover(radio);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(radio, "PART_StateLayer").Fill));
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
