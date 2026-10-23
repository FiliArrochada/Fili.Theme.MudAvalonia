using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudToggleItem as MudBlazor 9.11 renders it: a MudButton in the group's colour (Primary by
/// default), Variant.Outlined while unselected and Variant.Filled once selected, with
/// _togglegroup.scss taking away its border, shadow and minimum width. Earlier MudBlazor tinted a
/// selected item at 6% instead; this theme drew that until 0.9.
/// </summary>
public class ToggleItemTests
{
    private static ToggleButton Show(bool isChecked, params string[] classes)
    {
        var toggle = new ToggleButton { Content = "Week", IsChecked = isChecked };
        toggle.Classes.AddRange(classes);
        return Matrix.Show(toggle);
    }

    private static Border Root(ToggleButton toggle) => Matrix.Part<Border>(toggle, "PART_Root");

    private static Border Layer(ToggleButton toggle) => Matrix.Part<Border>(toggle, "PART_StateLayer");

    private static Color? Fill(Border border) => (border.Background as ISolidColorBrush)?.Color;

    [Fact]
    public Task AnUnselectedItemIsThePrimaryAsTextOnNoGround() => UiThread.RunAsync(() =>
    {
        var toggle = Show(false);

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(toggle.Foreground));
        Assert.Equal(Colors.Transparent, Fill(Root(toggle)));
        Assert.Equal(new Thickness(0), toggle.BorderThickness);
        Assert.Equal(new Thickness(6), toggle.Padding);
        Assert.Equal(0, toggle.MinWidth);
        // A MudButton: the label on its 1.75 line, 6 + 24.5 + 6 = 36.5px, drawn as 37.
        Assert.Equal(37, toggle.Bounds.Height);
    });

    [Fact]
    public Task ASelectedItemIsFilledWithThePrimary() => UiThread.RunAsync(() =>
    {
        var toggle = Show(true);

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Fill(Root(toggle)));
        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(toggle.Foreground));
        Assert.Equal(FabTests.Shadow(0), Root(toggle).BoxShadow);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColourIsTheTextUnselectedAndTheGroundSelected(string colour) => UiThread.RunAsync(() =>
    {
        var off = Show(false, colour.ToLowerInvariant());
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(off.Foreground));

        var on = Show(true, colour.ToLowerInvariant());
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Fill(Root(on)));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(on.Foreground));
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task UnderThePointerAnUnselectedItemTintsAndASelectedOneDarkens(string colour) => UiThread.RunAsync(() =>
    {
        var off = Show(false, colour.ToLowerInvariant());
        Matrix.Hover(off);
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Fill(Layer(off)));

        var on = Show(true, colour.ToLowerInvariant());
        Matrix.Hover(on);
        Assert.Equal(Matrix.Token($"Fili{colour}DarkenColor"), Fill(Root(on)));
        Assert.Equal(Colors.Transparent, Fill(Layer(on)));
    });

    /// <summary>The group's line is the colour, selected or not; MudBlazor's border-opacity is 1.</summary>
    [Fact]
    public Task OutlinedIsALineInTheColourWithinThePadding() => UiThread.RunAsync(() =>
    {
        foreach (var isChecked in new[] { false, true })
        {
            var plain = Show(isChecked, "outlined");
            Assert.Equal(new Thickness(1), plain.BorderThickness);
            Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(plain.BorderBrush));
            Assert.Equal(new Thickness(5), plain.Padding);

            var coloured = Show(isChecked, "outlined", "error");
            Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(coloured.BorderBrush));
        }
    });

    [Fact]
    public Task SizesSetThePaddingAndTheType() => UiThread.RunAsync(() =>
    {
        var small = Show(false, "small");
        Assert.Equal(new Thickness(4), small.Padding);
        Assert.Equal(13, small.FontSize);

        var large = Show(false, "large");
        Assert.Equal(new Thickness(8), large.Padding);
        Assert.Equal(15, large.FontSize);

        Assert.Equal(new Thickness(3), Show(false, "outlined", "small").Padding);
        Assert.Equal(new Thickness(7), Show(false, "outlined", "large").Padding);
    });

    [Fact]
    public Task ADisabledItemIsGreyInEitherState() => UiThread.RunAsync(() =>
    {
        var off = Show(false, "outlined");
        off.IsEnabled = false;
        var on = Show(true, "secondary");
        on.IsEnabled = false;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var text = Matrix.Part<ContentPresenter>(off, "PART_ContentPresenter");
        Assert.Equal(Matrix.Token("FiliActionDisabledColor"), Matrix.Colour(text.Foreground));
        Assert.Equal(Matrix.Token("FiliActionDisabledBackgroundColor"), Matrix.Colour(off.BorderBrush));
        Assert.Equal(Matrix.Token("FiliActionDisabledBackgroundColor"), Fill(Root(on)));
    });
}
