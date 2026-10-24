using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudChip with neither OnClick nor Href, on a ContentControl: no `mud-clickable`, so the chip's
/// look at rest and nothing on hover, focus or press.
/// </summary>
public class StaticChipTests
{
    [Fact]
    public Task ItIsTheDefaultChipAtRest() => UiThread.RunAsync(() =>
    {
        var chip = Matrix.Show(new ContentControl { Content = "Indie", Classes = { "chip" } });

        Assert.Equal(32, chip.Bounds.Height);
        Assert.Equal(16, chip.CornerRadius.TopLeft);
        Assert.Equal(Matrix.Token("FiliActionDisabledBackgroundColor"), Matrix.Colour(chip.Background));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(chip.Foreground));
    });

    /// <summary>Not a control to reach: no focus, no hand, and the pointer changes nothing.</summary>
    [Fact]
    public Task NothingHappensUnderThePointer() => UiThread.RunAsync(() =>
    {
        var chip = Matrix.Show(new ContentControl { Content = "Indie", Classes = { "chip", "primary" } });
        var surface = chip.GetVisualDescendants().OfType<Border>().First();
        var before = Matrix.Colour(surface.Background);

        Matrix.Hover(chip);

        Assert.False(chip.Focusable);
        Assert.Null(chip.Cursor);
        Assert.Equal(before, Matrix.Colour(surface.Background));
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), before);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task EachVariantTakesTheColour(string colour) => UiThread.RunAsync(() =>
    {
        var cls = colour.ToLowerInvariant();
        var filled = Matrix.Show(new ContentControl { Content = "Filled", Classes = { "chip", cls } });
        var outlined = Matrix.Show(new ContentControl { Content = "Outlined", Classes = { "chip", "outlined", cls } });
        var text = Matrix.Show(new ContentControl { Content = "Text", Classes = { "chip", "text", cls } });

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(filled.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(filled.Foreground));
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(outlined.BorderBrush));
        Assert.Equal(1, outlined.BorderThickness.Top);
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(text.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(text.Foreground));
    });

    [Theory]
    [InlineData("small", 24)]
    [InlineData("large", 40)]
    public Task SizesAreTheChipsSizes(string size, double height) => UiThread.RunAsync(() =>
    {
        var chip = Matrix.Show(new ContentControl { Content = "Indie", Classes = { "chip", size } });

        Assert.Equal(height, chip.Bounds.Height);
        Assert.Equal(height / 2, chip.CornerRadius.TopLeft);
    });

    /// <summary>A type selector matches its type exactly: a clickable chip keeps its own theme.</summary>
    [Fact]
    public Task AButtonChipIsStillClickable() => UiThread.RunAsync(() =>
    {
        var chip = Matrix.Show(new Button { Content = "Action", Classes = { "chip" } });

        Assert.NotNull(chip.Cursor);
    });
}
