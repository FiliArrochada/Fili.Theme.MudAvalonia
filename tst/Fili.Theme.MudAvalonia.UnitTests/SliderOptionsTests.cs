using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudSlider's Variant, TickMarks and ValueLabel, from _slider.scss and MudSlider.razor.
/// </summary>
public class SliderOptionsTests
{
    private static Slider Show(Slider slider) => Matrix.Show(slider);

    private static List<Ellipse> Dots(Slider slider) =>
        Matrix.Part<ItemsControl>(slider, "PART_TickMarks").GetVisualDescendants().OfType<Ellipse>().ToList();

    /// <summary>Variant.Filled lays the colour, solid, from the start to the thumb.</summary>
    [Fact]
    public Task FilledShowsTheSolidPart() => UiThread.RunAsync(() =>
    {
        var slider = Show(new Slider { Width = 240, Value = 40, Classes = { "filled", "secondary" } });
        var solid = Matrix.Part<RepeatButton>(slider, "PART_DecreaseButton");

        Assert.Equal(1, solid.Opacity);
        Assert.Equal(Matrix.Token("FiliSecondaryColor"), Matrix.Colour(solid.Background));
        Assert.Equal(0.30, Matrix.Part<Border>(slider, "PART_Rail").Opacity, 2);
    });

    [Fact]
    public Task NoTicksUnlessAskedFor() => UiThread.RunAsync(() =>
    {
        var slider = Show(new Slider { Width = 240, Value = 40, TickFrequency = 25 });

        Assert.False(Matrix.Part<ItemsControl>(slider, "PART_TickMarks").IsVisible);
    });

    /// <summary>
    /// A dot per tick, the rail's thickness, the slider's colour - and the first and last under
    /// the thumb's centre at the minimum and the maximum, which is 20px in from each end.
    /// </summary>
    [Theory]
    [InlineData(null, 2)]
    [InlineData("large", 6)]
    public Task TicksAreDotsOnTheRail(string? size, double dot) => UiThread.RunAsync(() =>
    {
        var slider = new Slider
        {
            Width = 240,
            Value = 40,
            TickPlacement = TickPlacement.BottomRight,
            TickFrequency = 25,
            Classes = { "success" },
        };
        if (size is not null) slider.Classes.Add(size);
        Show(slider);

        var dots = Dots(slider);
        Assert.Equal(5, dots.Count);
        Assert.All(dots, d => Assert.Equal(dot, d.Bounds.Width));
        Assert.All(dots, d => Assert.Equal(Matrix.Token("FiliSuccessColor"), Matrix.Colour(d.Fill)));

        var track = Matrix.Part<Track>(slider, "PART_Track");
        var centres = dots
            .Select(d => d.TranslatePoint(new Point(dot / 2, dot / 2), track)!.Value.X)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(20, centres[0], 1);
        Assert.Equal(track.Bounds.Width - 20, centres[^1], 1);
        Assert.Equal((track.Bounds.Width - 40) / 4, centres[1] - centres[0], 1);
    });

    /// <summary>
    /// ValueLabel: off unless asked for; when asked, a chip of the colour with its contrast text,
    /// hidden until the thumb is held (opacity, so the chip keeps its place).
    /// </summary>
    [Fact]
    public Task TheValueLabelShowsTheValueInTheColour() => UiThread.RunAsync(() =>
    {
        var plain = Show(new Slider { Width = 240, Value = 40 });
        Assert.False(Matrix.Part<Panel>(plain, "PART_ValueLabel").IsVisible);

        var slider = Show(new Slider { Width = 240, Value = 40, Classes = { "value-label", "warning" } });
        var label = Matrix.Part<Panel>(slider, "PART_ValueLabel");
        var chip = label.GetVisualDescendants().OfType<Border>().First();
        var text = label.GetVisualDescendants().OfType<TextBlock>().First();

        Assert.True(label.IsVisible);
        Assert.Equal(0, label.Opacity);
        Assert.Equal("40", text.Text);
        Assert.Equal(Matrix.Token("FiliWarningColor"), Matrix.Colour(chip.Background));
        Assert.Equal(Matrix.Token("FiliWarningContrastTextColor"), Matrix.Colour(text.Foreground));
        Assert.False(slider.ClipToBounds);
    });
}
