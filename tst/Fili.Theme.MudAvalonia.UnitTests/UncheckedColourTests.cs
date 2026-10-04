using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Path = Avalonia.Controls.Shapes.Path;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudCheckBox's and MudRadio's UncheckedColor: while NOT checked (for a checkbox, false or
/// null), the glyph and its halo take this colour instead of Color. It is a class,
/// `unchecked-{colour}`, and it wins where both are set - `primary unchecked-error` is
/// Color.Primary with UncheckedColor.Error.
/// </summary>
public class UncheckedColourTests
{
    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AnUncheckedCheckBoxTakesTheUncheckedColour(string colour) => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new CheckBox { IsChecked = false, Content = "Box", Classes = { "primary", $"unchecked-{colour.ToLowerInvariant()}" } });

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Matrix.Part<Path>(box, "PART_Unchecked").Fill));
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Matrix.Part<Path>(box, "PART_Indeterminate").Fill));

        Matrix.Hover(box);
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(box, "PART_StateLayer").Fill));
    });

    /// <summary>Checked, the colour class is back in charge, glyph and halo alike.</summary>
    [Fact]
    public Task ACheckedCheckBoxKeepsItsColour() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new CheckBox { IsChecked = true, Content = "Box", Classes = { "primary", "unchecked-error" } });

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Matrix.Part<Path>(box, "PART_Checked").Fill));
        Matrix.Hover(box);
        Assert.Equal(Matrix.Token("FiliPrimaryHoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(box, "PART_StateLayer").Fill));
    });

    /// <summary>MudCheckBox counts null as unchecked too: the indeterminate halo takes it.</summary>
    [Fact]
    public Task AnIndeterminateCheckBoxIsUnchecked() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new CheckBox { IsThreeState = true, IsChecked = null, Content = "Box", Classes = { "primary", "unchecked-warning" } });

        Matrix.Hover(box);
        Assert.Equal(Matrix.Token("FiliWarningHoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(box, "PART_StateLayer").Fill));
    });

    [Fact]
    public Task ARadioTakesTheUncheckedColourUntilChecked() => UiThread.RunAsync(() =>
    {
        var off = Matrix.Show(new RadioButton { IsChecked = false, Content = "Off", GroupName = "u1", Classes = { "secondary", "unchecked-success" } });
        Assert.Equal(Matrix.Token("FiliSuccessColor"), Matrix.Colour(Matrix.Part<Path>(off, "PART_Unchecked").Fill));
        Matrix.Hover(off);
        Assert.Equal(Matrix.Token("FiliSuccessHoverColor"), Matrix.Colour(Matrix.Part<Ellipse>(off, "PART_StateLayer").Fill));

        var on = Matrix.Show(new RadioButton { IsChecked = true, Content = "On", GroupName = "u2", Classes = { "secondary", "unchecked-success" } });
        Assert.Equal(Matrix.Token("FiliSecondaryColor"), Matrix.Colour(Matrix.Part<Path>(on, "PART_Checked").Fill));
    });
}
