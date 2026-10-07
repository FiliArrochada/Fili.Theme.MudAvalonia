using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.VisualTree;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudProgressCircular, as the <c>circular</c> class on a ProgressBar. Sizes, stroke and colours
/// are from _progresscircular.scss and MudProgressCircular.razor.cs; the indeterminate key frames
/// are its circular-rotate and circular-dash, converted from SVG dash lengths to arc angles.
/// </summary>
public class ProgressCircularTests
{
    private static ProgressBar Show(params string[] classes)
    {
        var bar = new ProgressBar { Value = 25 };
        bar.Classes.Add("circular");
        bar.Classes.AddRange(classes);
        return Matrix.Show(bar);
    }

    [Fact]
    public Task NoClassIsAMediumTextSecondaryRing() => UiThread.RunAsync(() =>
    {
        var bar = Show();
        var circle = Matrix.Part<Arc>(bar, "PART_Circle");

        Assert.Equal(40, bar.Bounds.Width);
        Assert.Equal(40, bar.Bounds.Height);
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(bar.Foreground));
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(circle.Stroke));
        Assert.Equal(3 * 40 / 43.0, circle.StrokeThickness, 3);
        Assert.Equal(PenLineCap.Flat, circle.StrokeLineCap);
    });

    [Theory]
    [InlineData("small", 24)]
    [InlineData("large", 56)]
    public Task SizesScaleTheStrokeWithTheBox(string size, double side) => UiThread.RunAsync(() =>
    {
        var bar = Show(size);
        var circle = Matrix.Part<Arc>(bar, "PART_Circle");

        Assert.Equal(side, bar.Bounds.Width);
        Assert.Equal(side, bar.Bounds.Height);
        Assert.Equal(3 * side / 43.0, circle.StrokeThickness, 3);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColourPaintsTheRing(string colour) => UiThread.RunAsync(() =>
    {
        var bar = Show(colour.ToLowerInvariant());

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Matrix.Part<Arc>(bar, "PART_Circle").Stroke));
    });

    [Fact]
    public Task TheSweepStartsAtTwelveAndFollowsTheValue() => UiThread.RunAsync(() =>
    {
        var bar = Show();
        var circle = Matrix.Part<Arc>(bar, "PART_Circle");

        // The sweep is transitioned, so the base value is what the binding asked for.
        Assert.Equal(-90, circle.StartAngle);
        Assert.Equal(90, circle.GetBaseValue(Arc.SweepAngleProperty).GetValueOrDefault(), 6);

        bar.Value = 100;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(360, circle.GetBaseValue(Arc.SweepAngleProperty).GetValueOrDefault(), 6);
    });

    [Fact]
    public Task RoundedRoundsTheEnds() => UiThread.RunAsync(() =>
    {
        Assert.Equal(PenLineCap.Round, Matrix.Part<Arc>(Show("rounded"), "PART_Circle").StrokeLineCap);
    });

    [Fact]
    public Task IndeterminateSwapsTheRing() => UiThread.RunAsync(() =>
    {
        var bar = Show();
        bar.IsIndeterminate = true;
        Dispatcher.UIThread.RunJobs();

        Assert.False(Matrix.Part<Arc>(bar, "PART_Circle").IsVisible);
        Assert.True(Matrix.Part<Arc>(bar, "PART_IndeterminateCircle").IsVisible);
    });

    [Fact]
    public Task TheIndeterminateDashIsMudBlazorsCircularDash() => UiThread.RunAsync(() =>
    {
        var dash = KeyFrames.InTheme("FiliProgressCircular", "PART_IndeterminateCircle");

        Assert.Equal(TimeSpan.FromSeconds(1.4), dash.Duration);
        Assert.Equal(IterationCount.Infinite, dash.IterationCount);

        // dasharray 1px -> 100px -> 100px and offset 0 -> -15px -> -125px on a circumference of
        // 2 pi 20, as degrees, starting from 12 o'clock.
        var circumference = 2 * Math.PI * 20;
        double Degrees(double px) => px / circumference * 360;

        Assert.Equal(-90, KeyFrames.Value(dash, 0, Arc.StartAngleProperty), 2);
        Assert.Equal(Degrees(1), KeyFrames.Value(dash, 0, Arc.SweepAngleProperty), 2);
        Assert.Equal(-90 + Degrees(15), KeyFrames.Value(dash, 0.5, Arc.StartAngleProperty), 2);
        Assert.Equal(Degrees(100), KeyFrames.Value(dash, 0.5, Arc.SweepAngleProperty), 2);
        Assert.Equal(-90 + Degrees(125), KeyFrames.Value(dash, 1, Arc.StartAngleProperty), 2);
        Assert.Equal(Degrees(100), KeyFrames.Value(dash, 1, Arc.SweepAngleProperty), 2);

        // ease-in-out on EACH segment, as CSS applies it: a KeySpline on the frame that ends the
        // segment. An Animation.Easing would ease the whole cycle once instead.
        Assert.IsType<LinearEasing>(dash.Easing);
        foreach (var cue in new[] { 0.5, 1 })
        {
            var spline = KeyFrames.Frame(dash, cue).KeySpline;
            Assert.NotNull(spline);
            Assert.Equal((0.42, 0.0, 0.58, 1.0), (spline.ControlPointX1, spline.ControlPointY1, spline.ControlPointX2, spline.ControlPointY2));
        }
    });

    [Fact]
    public Task TheIndeterminateRingTurnsOnceIn1400ms() => UiThread.RunAsync(() =>
    {
        var turn = KeyFrames.InTheme("FiliProgressCircular", "PART_Spinner");

        Assert.Equal(TimeSpan.FromSeconds(1.4), turn.Duration);
        Assert.Equal(IterationCount.Infinite, turn.IterationCount);
        Assert.Equal(0, KeyFrames.Value(turn, 0, RotateTransform.AngleProperty));
        Assert.Equal(360, KeyFrames.Value(turn, 1, RotateTransform.AngleProperty));
    });

    [Fact]
    public Task ShowProgressTextCentresTheValue() => UiThread.RunAsync(() =>
    {
        var bar = Show();
        bar.ShowProgressText = true;
        Dispatcher.UIThread.RunJobs();

        var text = Assert.Single(bar.GetVisualDescendants().OfType<TextBlock>());
        Assert.True(text.IsVisible);
        Assert.Equal("25%", text.Text);
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(text.Foreground));
    });
}

/// <summary>MudSkeleton's Animation.Wave, as the <c>wave</c> class.</summary>
public class SkeletonWaveTests
{
    [Fact]
    public Task TheBandCrossesTheSkeletonInTheFirst60PercentAfterTheDelay() => UiThread.RunAsync(() =>
    {
        var wave = KeyFrames.InStyles("skeleton.wave");

        Assert.Equal(TimeSpan.FromSeconds(1.6), wave.Duration);
        Assert.Equal(TimeSpan.FromSeconds(0.5), wave.Delay);
        Assert.Equal(IterationCount.Infinite, wave.IterationCount);
        Assert.Equal([0, 0.6, 1], wave.Children.Select(k => k.Cue.CueValue));
        Assert.All(wave.Children, k => Assert.Equal(Border.BackgroundProperty, Assert.IsType<Setter>(Assert.Single(k.Setters)).Property));
    });

    [Fact]
    public Task TheBandIsOneSkeletonWideAndStartsAndEndsOffTheEdges() => UiThread.RunAsync(() =>
    {
        var start = Brush("FiliSkeletonWaveStartBrush");
        var end = Brush("FiliSkeletonWaveEndBrush");

        // translateX(-100%) and translateX(100%) of a layer as wide as the skeleton.
        Assert.Equal((-1.0, 0.0), (start.StartPoint.Point.X, start.EndPoint.Point.X));
        Assert.Equal((1.0, 2.0), (end.StartPoint.Point.X, end.EndPoint.Point.X));
        Assert.Equal(RelativeUnit.Relative, start.StartPoint.Unit);

        foreach (var band in new[] { start, end })
        {
            Assert.Equal([0, 0.5, 1], band.GradientStops.Select(g => g.Offset));
            Assert.Equal(Matrix.Token("FiliSkeletonColor"), band.GradientStops[0].Color);
            Assert.Equal(Matrix.Token("FiliSkeletonWaveColor"), band.GradientStops[1].Color);
            Assert.Equal(Matrix.Token("FiliSkeletonColor"), band.GradientStops[2].Color);
        }
    });

    private static LinearGradientBrush Brush(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value), $"{key} did not resolve.");
        return Assert.IsType<LinearGradientBrush>(value);
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public void TheBandIsFourPercentBlackOverTheSkeleton(string variant)
    {
        var theme = variant == "HighContrast" ? FiliThemeVariants.HighContrast : new ThemeVariant(variant, null);
        Assert.True(Application.Current!.TryFindResource("FiliSkeletonColor", theme, out var s));
        Assert.True(Application.Current!.TryFindResource("FiliSkeletonWaveColor", theme, out var w));

        // Source-over of rgba(0,0,0,0.04) onto the skeleton colour, in straight alpha.
        var under = (Color)s!;
        var a = (under.A / 255.0) + (0.04 * (1 - (under.A / 255.0)));
        double Channel(byte c) => c * (under.A / 255.0) * 0.96 / a;
        var expected = Color.FromArgb(
            (byte)Math.Round(a * 255), (byte)Math.Round(Channel(under.R)), (byte)Math.Round(Channel(under.G)), (byte)Math.Round(Channel(under.B)));

        var actual = (Color)w!;
        Assert.InRange(Math.Abs(actual.A - expected.A), 0, 1);
        Assert.InRange(Math.Abs(actual.R - expected.R), 0, 1);
        Assert.InRange(Math.Abs(actual.G - expected.G), 0, 1);
        Assert.InRange(Math.Abs(actual.B - expected.B), 0, 1);
    }

    [Fact]
    public Task AWaveSkeletonDoesNotAlsoPulse() => UiThread.RunAsync(() =>
    {
        // Two skeleton animations, and each names `wave`: the wave itself, and a pulse that
        // excludes it. Running both would fade the band in and out as it travels.
        var animated = KeyFrames.AnimatedSelectors().Where(s => s.Contains("skeleton")).ToList();
        Assert.Equal(2, animated.Count);
        Assert.Single(animated, s => s.Contains(":not(.wave)"));
    });
}

/// <summary>
/// Reads a theme's own <see cref="Animation"/>s as data. Avalonia 12 keeps its animation clock
/// internal, so a test cannot step time to a key frame; what it can check is that the key frames
/// say what MudBlazor's do, which is the part that was transcribed.
/// </summary>
internal static class KeyFrames
{
    public static Animation InTheme(string themeKey, string selectorPart)
    {
        Assert.True(Application.Current!.TryFindResource(themeKey, ThemeVariant.Light, out var value));
        var theme = Assert.IsType<ControlTheme>(value);
        return Single(Flatten(theme.Children).Where(s => s.Selector?.ToString()?.Contains(selectorPart) == true));
    }

    public static Animation InStyles(string selectorPart) =>
        Single(Flatten(Application.Current!.Styles).Where(s => s.Selector?.ToString()?.Contains(selectorPart) == true));

    /// <summary>The selector text of every application Style that carries an animation.</summary>
    public static IEnumerable<string> AnimatedSelectors() =>
        Flatten(Application.Current!.Styles)
            .Where(s => s.Animations.Count > 0)
            .Select(s => s.Selector?.ToString() ?? "");

    public static KeyFrame Frame(Animation animation, double cue) =>
        Assert.Single(animation.Children, k => Math.Abs(k.Cue.CueValue - cue) < 1e-9);

    public static double Value(Animation animation, double cue, AvaloniaProperty property) =>
        Convert.ToDouble(Assert.Single(Frame(animation, cue).Setters.OfType<Setter>(), s => s.Property == property).Value);

    private static Animation Single(IEnumerable<Style> styles)
    {
        var style = Assert.Single(styles, s => s.Animations.Count > 0);
        return Assert.IsType<Animation>(Assert.Single(style.Animations));
    }

    private static IEnumerable<Style> Flatten(IEnumerable<IStyle> styles)
    {
        foreach (var item in styles)
        {
            switch (item)
            {
                case Style style:
                    yield return style;
                    foreach (var child in Flatten(style.Children))
                    {
                        yield return child;
                    }

                    break;
                case Styles group:
                    foreach (var child in Flatten(group))
                    {
                        yield return child;
                    }

                    break;
                case Avalonia.Markup.Xaml.Styling.StyleInclude include when include.Loaded is { } loaded:
                    foreach (var child in Flatten([loaded]))
                    {
                        yield return child;
                    }

                    break;
            }
        }
    }
}
