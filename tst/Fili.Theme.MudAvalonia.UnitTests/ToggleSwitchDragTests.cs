using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The switch's thumb rides where Avalonia puts it: Canvas.Left on PART_MovingKnobs, 0 when off,
/// the travel when on, and under the pointer while dragged. A template that moved the thumb by a
/// transform kept the click and lost the drag, with nothing to say so.
/// </summary>
public class ToggleSwitchDragTests
{
    private static (Window Window, ToggleSwitch Toggle) Show(bool on = false, string? size = null)
    {
        var toggle = new ToggleSwitch { IsChecked = on, Content = "Wi-Fi" };
        if (size is not null)
        {
            toggle.Classes.Add(size);
        }

        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = toggle,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Top,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, toggle);
    }

    private static Point Centre(Visual part, Visual to) =>
        part.TranslatePoint(new Point(part.Bounds.Width / 2, part.Bounds.Height / 2), to)!.Value;

    /// <summary>
    /// At rest the thumb sits flush with the track's left end when off and its right end when on,
    /// in every size: the travel is the track less the thumb, as `.mud-switch-base` translates it.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData("small", false)]
    [InlineData("small", true)]
    [InlineData("large", false)]
    [InlineData("large", true)]
    public Task TheThumbRestsAtTheTracksEnds(string? size, bool on) => UiThread.RunAsync(() =>
    {
        var (window, toggle) = Show(on, size);
        var track = Matrix.Part<Border>(toggle, "PART_Track");
        var thumb = Matrix.Part<Border>(toggle, "PART_Thumb");
        var left = thumb.TranslatePoint(default, track)!.Value.X;

        Assert.Equal(on ? track.Bounds.Width - thumb.Bounds.Width : 0, left, 1);
        Assert.Equal(track.Bounds.Height / 2, Centre(thumb, track).Y, 1);
        window.Close();
    });

    /// <summary>The halo and the ripple stay centred on the thumb, wherever it is.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task TheHaloTravelsWithTheThumb(bool on) => UiThread.RunAsync(() =>
    {
        var (window, toggle) = Show(on);
        var thumb = Centre(Matrix.Part<Border>(toggle, "PART_Thumb"), toggle);

        Assert.Equal(thumb, Centre(Matrix.Part<Avalonia.Controls.Shapes.Ellipse>(toggle, "PART_ThumbStateLayer"), toggle));
        Assert.Equal(thumb, Centre(Matrix.Part<Border>(toggle, "PART_ThumbRipple"), toggle));
        window.Close();
    });

    [Fact]
    public Task TheThumbFollowsADragAndAFullDragTurnsItOn() => UiThread.RunAsync(() =>
    {
        var (window, toggle) = Show();
        var thumb = Matrix.Part<Border>(toggle, "PART_Thumb");
        var track = Matrix.Part<Border>(toggle, "PART_Track");
        var start = Centre(thumb, window);

        window.MouseDown(start, MouseButton.Left);
        window.MouseMove(start + new Point(8, 0), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(8, thumb.TranslatePoint(default, track)!.Value.X, 1);

        window.MouseMove(start + new Point(30, 0), RawInputModifiers.LeftMouseButton);
        window.MouseUp(start + new Point(30, 0), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(toggle.IsChecked);
        window.Close();
    });

    [Fact]
    public Task AShortDragLeavesItOff() => UiThread.RunAsync(() =>
    {
        var (window, toggle) = Show();
        var thumb = Matrix.Part<Border>(toggle, "PART_Thumb");
        var start = Centre(thumb, window);

        window.MouseDown(start, MouseButton.Left);
        window.MouseMove(start + new Point(5, 0), RawInputModifiers.LeftMouseButton);
        window.MouseUp(start + new Point(5, 0), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.False(toggle.IsChecked);
        Assert.Equal(0, Canvas.GetLeft(Matrix.Part<Panel>(toggle, "PART_MovingKnobs")), 1);
        window.Close();
    });

    [Fact]
    public Task AClickStillToggles() => UiThread.RunAsync(() =>
    {
        var (window, toggle) = Show();
        var point = Centre(Matrix.Part<Border>(toggle, "PART_Track"), window);

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(toggle.IsChecked);
        window.Close();
    });

    /// <summary>
    /// The focus ring frames the switch itself, the track with the thumb's overhang, not the
    /// whole control with its label: PART_FocusTarget is the template's focus target.
    /// </summary>
    [Theory]
    [InlineData(null, 34, 20)]
    [InlineData("small", 30, 14)]
    [InlineData("large", 38, 26)]
    public Task TheFocusTargetIsTheSwitch(string? size, double width, double height) => UiThread.RunAsync(() =>
    {
        var (window, toggle) = Show(size: size);
        var target = Matrix.Part<Border>(toggle, "PART_FocusTarget");

        Assert.True(TemplatedControl.GetIsTemplateFocusTarget(target));
        Assert.Equal(new Size(width, height), target.Bounds.Size);
        Assert.Equal(new CornerRadius(height / 2), target.CornerRadius);
        Assert.Equal(Centre(Matrix.Part<Border>(toggle, "PART_Track"), toggle), Centre(target, toggle));
        window.Close();
    });
}
