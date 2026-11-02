using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// FiliFocusRing: on in every variant, and False in an app's resources leaves keyboard focus to
/// MudBlazor's state-layer tint alone. The ring is the AdornerLayer's default focus adorner, so it
/// follows keyboard focus only: a click or a touch draws none.
/// </summary>
public class FocusRingTests
{
    private static readonly string[] RingGrounds = ["FiliBackgroundColor", "FiliSurfaceColor", "FiliAppbarBackgroundColor"];

    /// <summary>The variants by name, as a theory's data must be constant.</summary>
    private static ThemeVariant Variant(string name) => name switch
    {
        "Light" => ThemeVariant.Light,
        "Dark" => ThemeVariant.Dark,
        _ => FiliThemeVariants.HighContrast,
    };

    /// <summary>Runs a check with the application in a variant, as the variant is the application's.</summary>
    private static Task In(ThemeVariant variant, Action check) => UiThread.RunAsync(() =>
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;
        application.RequestedThemeVariant = variant;
        try
        {
            check();
        }
        finally
        {
            application.RequestedThemeVariant = previous;
        }
    });

    /// <summary>Two buttons in a window, the first focused by the keyboard.</summary>
    private static (Button Keyboard, Button Pointer) Focus(bool? token = null, Action<IResourceDictionary>? resources = null)
    {
        var keyboard = new Button { Content = "Save" };
        var pointer = new Button { Content = "Cancel" };
        var window = new Window { Content = new StackPanel { Children = { keyboard, pointer } } };
        if (token is { } value)
        {
            window.Resources["FiliFocusRing"] = value;
        }

        resources?.Invoke(window.Resources);

        window.Show();
        Dispatcher.UIThread.RunJobs();
        keyboard.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();
        Assert.True(keyboard.IsKeyboardFocusWithin);
        return (keyboard, pointer);
    }

    private static Control? Ring(Control control) =>
        AdornerLayer.GetAdornerLayer(control)!.Children.OfType<Control>().SingleOrDefault();

    /// <summary>At least one ring stroke needs 3:1 non-text contrast with the surrounding surface.</summary>
    [Fact]
    public Task TheRingContrastsWithItsHostSurface() => UiThread.RunAsync(() =>
    {
        var application = Application.Current!;
        var failures = new List<string>();
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark, FiliThemeVariants.HighContrast })
        {
            Assert.True(application.TryFindResource("FiliTextPrimaryColor", variant, out var textValue));
            Assert.True(application.TryFindResource("FiliWhiteColor", variant, out var whiteValue));
            var (text, white) = (Assert.IsType<Color>(textValue), Assert.IsType<Color>(whiteValue));
            foreach (var key in RingGrounds)
            {
                Assert.True(application.TryFindResource(key, variant, out var groundValue), $"{key} did not resolve in {variant}.");
                var ground = Assert.IsType<Color>(groundValue);
                var textRatio = Contrast(Over(text, ground), ground);
                var whiteRatio = Contrast(Over(white, ground), ground);
                if (Math.Max(textRatio, whiteRatio) < 3)
                {
                    failures.Add($"{variant}: {key} has only {Math.Max(textRatio, whiteRatio):0.00}:1 contrast with either ring stroke.");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    private static Color Over(Color top, Color ground)
    {
        var alpha = top.A / 255d;
        byte Mix(byte foreground, byte background) =>
            (byte)Math.Round((alpha * foreground) + ((1 - alpha) * background));
        return Color.FromRgb(Mix(top.R, ground.R), Mix(top.G, ground.G), Mix(top.B, ground.B));
    }

    private static double Contrast(Color a, Color b)
    {
        static double Luminance(Color color)
        {
            static double Linear(byte channel)
            {
                var value = channel / 255d;
                return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
            }

            return (0.2126 * Linear(color.R)) + (0.7152 * Linear(color.G)) + (0.0722 * Linear(color.B));
        }

        var (first, second) = (Luminance(a), Luminance(b));
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    /// <summary>
    /// By default, in every variant, a 2px stroke in the variant's text-primary 2px outside a
    /// control the keyboard focuses, where a single ring stood, round a 2px white stroke against it.
    /// </summary>
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public Task KeyboardFocusDrawsTheRing(string name) => In(Variant(name), () =>
    {
        var ring = Assert.IsAssignableFrom<Border>(Ring(Focus().Keyboard));
        var inner = Assert.IsAssignableFrom<Border>(ring.Child);

        Assert.Equal(new Thickness(2), ring.BorderThickness);
        Assert.Equal(new Thickness(-4), ring.Margin);
        Assert.Equal(new Thickness(2), inner.BorderThickness);
        Assert.True(Application.Current!.TryFindResource("FiliTextPrimaryColor", Variant(name), out var text));
        Assert.Equal((Color)text!, Assert.IsAssignableFrom<ISolidColorBrush>(ring.BorderBrush).Color);
        Assert.True(Application.Current!.TryFindResource("FiliWhiteColor", Variant(name), out var white));
        Assert.Equal((Color)white!, Assert.IsAssignableFrom<ISolidColorBrush>(inner.BorderBrush).Color);
    });

    /// <summary>A click moves focus without a ring: the ring is keyboard focus's alone.</summary>
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public Task PointerFocusDrawsNoRing(string name) => In(Variant(name), () =>
    {
        var (keyboard, pointer) = Focus();

        pointer.Focus(NavigationMethod.Pointer);
        Dispatcher.UIThread.RunJobs();

        Assert.Null(Ring(keyboard));
        Assert.Null(Ring(pointer));
    });

    /// <summary>False in the resources leaves keyboard focus to the state layer's tint, in any variant.</summary>
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public Task TheTokenTurnsItOff(string name) => In(Variant(name), () => Assert.Null(Ring(Focus(token: false).Keyboard)));

    /// <summary>
    /// An app's own ring: FiliFocusRingBrush, FiliFocusRingInnerBrush and FiliFocusRingThickness
    /// in a window's resources. A 3px outer stroke sits 5px out, the inner stroke keeps its 2px,
    /// and the corners follow the new outset.
    /// </summary>
    [Fact]
    public Task AnAppSetsTheRingsColoursAndThickness() => UiThread.RunAsync(() =>
    {
        var accent = new SolidColorBrush(Colors.Goldenrod);
        var (keyboard, _) = Focus(resources: r =>
        {
            r["FiliFocusRingBrush"] = accent;
            r["FiliFocusRingInnerBrush"] = Brushes.Transparent;
            r["FiliFocusRingThickness"] = 3d;
        });
        var ring = Assert.IsAssignableFrom<Border>(Ring(keyboard));
        var inner = Assert.IsAssignableFrom<Border>(ring.Child);
        var radius = keyboard.CornerRadius.TopLeft;

        Assert.Same(accent, ring.BorderBrush);
        Assert.Same(Brushes.Transparent, inner.BorderBrush);
        Assert.Equal(new Thickness(3), ring.BorderThickness);
        Assert.Equal(new Thickness(-5), ring.Margin);
        Assert.Equal(new Thickness(2), inner.BorderThickness);
        Assert.Equal(new CornerRadius(radius + 5), ring.CornerRadius);
        Assert.Equal(new CornerRadius(radius + 2), inner.CornerRadius);
    });

    /// <summary>
    /// The thickness is read live: a television mode switched on while a control has focus moves
    /// the ring at once, as the brushes do.
    /// </summary>
    [Fact]
    public Task TheThicknessFollowsAChangedResource() => UiThread.RunAsync(() =>
    {
        var (keyboard, _) = Focus();
        var ring = Assert.IsAssignableFrom<Border>(Ring(keyboard));
        Assert.True(Application.Current!.TryFindResource("FiliFocusRingThickness", out var standard));
        Assert.Equal(2d, Assert.IsType<double>(standard));

        TopLevel.GetTopLevel(keyboard)!.Resources["FiliFocusRingThickness"] = 4d;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new Thickness(4), ring.BorderThickness);
        Assert.Equal(new Thickness(-6), ring.Margin);
    });

    /// <summary>A control in a window with the ring on, focused by the keyboard; returns the ring.</summary>
    private static Border RingRound(Control control)
    {
        var window = new Window { Content = new StackPanel { Children = { control } } };
        window.Resources["FiliFocusRing"] = true;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        control.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();
        return Assert.IsAssignableFrom<Border>(Ring(control));
    }

    /// <summary>
    /// The ring runs parallel to the control's corners, 4px further out: a fixed 4px ring cut
    /// across a pill and stood off a square. The inner stroke runs 2px inside it, 2px tighter, so
    /// no ground shows between the two at a corner.
    /// </summary>
    [Theory]
    [InlineData(0, 4)]
    [InlineData(4, 8)]
    [InlineData(18, 22)]
    public Task TheRingFollowsTheControlsCorners(double radius, double ring) => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save", CornerRadius = new CornerRadius(radius) };
        var outer = RingRound(button);

        Assert.Equal(new CornerRadius(ring), outer.CornerRadius);
        Assert.Equal(new CornerRadius(ring - 2), Assert.IsAssignableFrom<Border>(outer.Child).CornerRadius);
        ((Window)TopLevel.GetTopLevel(button)!).Close();
    });

    /// <summary>Each corner on its own, for a control rounded on one side only.</summary>
    [Fact]
    public Task EachCornerFollowsItsOwn() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save", CornerRadius = new CornerRadius(8, 0, 0, 8) };
        var outer = RingRound(button);

        Assert.Equal(new CornerRadius(12, 4, 4, 12), outer.CornerRadius);
        Assert.Equal(new CornerRadius(10, 2, 2, 10), Assert.IsAssignableFrom<Border>(outer.Child).CornerRadius);
        ((Window)TopLevel.GetTopLevel(button)!).Close();
    });

    /// <summary>
    /// A switch's ring frames the switch, its template's focus target, not the switch with its
    /// label, and is a pill round it.
    /// </summary>
    [Fact]
    public Task ASwitchsRingFramesTheSwitch() => UiThread.RunAsync(() =>
    {
        var toggle = new ToggleSwitch { Content = "Wi-Fi" };
        var ring = RingRound(toggle);

        Assert.Same(Matrix.Part<Border>(toggle, "PART_FocusTarget"), AdornerLayer.GetAdornedElement(ring));
        Assert.Equal(new CornerRadius(14), ring.CornerRadius);
        ((Window)TopLevel.GetTopLevel(toggle)!).Close();
    });

    /// <summary>Declared, and on, in every variant: a variant that forgot it would take dark's value.</summary>
    [Fact]
    public Task TheTokenIsDeclaredInEveryVariant() => UiThread.RunAsync(() =>
    {
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark, FiliThemeVariants.HighContrast })
        {
            Assert.True(Application.Current!.TryFindResource("FiliFocusRing", variant, out var value), $"FiliFocusRing did not resolve in {variant}.");
            Assert.True(Assert.IsType<bool>(value), $"FiliFocusRing is off in {variant}.");
        }
    });
}
