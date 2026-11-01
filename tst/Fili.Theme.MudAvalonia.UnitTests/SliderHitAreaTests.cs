using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A click anywhere in the slider's row moves the thumb to it, as a range input's does: the rail
/// halves are their thickness to look at, but their hit band reaches across the row. At 2px, a
/// click had to land on the line itself.
/// </summary>
public class SliderHitAreaTests
{
    private static (Window Window, Slider Slider) Show(Orientation orientation, string? size = null)
    {
        var slider = new Slider { Minimum = 0, Maximum = 100, Value = 0, Orientation = orientation };
        if (orientation == Orientation.Horizontal)
        {
            slider.Width = 300;
        }
        else
        {
            slider.Height = 300;
        }

        if (size is not null)
        {
            slider.Classes.Add(size);
        }

        var window = new Window
        {
            Width = 400,
            Height = 400,
            Content = slider,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Top,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, slider);
    }

    /// <summary>15px off the rail, in every size: the band covers most of the 40px row.</summary>
    [Theory]
    [InlineData(null, -15)]
    [InlineData(null, 15)]
    [InlineData("medium", 15)]
    [InlineData("large", -15)]
    public Task AClickBesideTheRailMovesTheThumbThere(string? size, double off) => UiThread.RunAsync(() =>
    {
        var (window, slider) = Show(Orientation.Horizontal, size);
        var point = slider.TranslatePoint(new Point(slider.Bounds.Width / 2, slider.Bounds.Height / 2 + off), window)!.Value;

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.InRange(slider.Value, 45, 55);
        window.Close();
    });

    [Fact]
    public Task AVerticalSliderTakesAClickBesideItsRail() => UiThread.RunAsync(() =>
    {
        var (window, slider) = Show(Orientation.Vertical);
        var point = slider.TranslatePoint(new Point(slider.Bounds.Width / 2 + 15, slider.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.InRange(slider.Value, 45, 55);
        window.Close();
    });

    /// <summary>The band is invisible: the rail still draws at its own thickness.</summary>
    [Fact]
    public Task TheBandDrawsNothing() => UiThread.RunAsync(() =>
    {
        var (window, slider) = Show(Orientation.Horizontal);

        foreach (var name in new[] { "PART_DecreaseButton", "PART_IncreaseButton" })
        {
            var half = Matrix.Part<RepeatButton>(slider, name);
            var band = Matrix.Part<Border>(half, "PART_HitArea");
            Assert.Equal(2, half.Bounds.Height);
            Assert.Equal(Avalonia.Media.Colors.Transparent, Matrix.Colour(band.Background));
            Assert.Equal(36, band.Bounds.Height);
        }

        window.Close();
    });
}
