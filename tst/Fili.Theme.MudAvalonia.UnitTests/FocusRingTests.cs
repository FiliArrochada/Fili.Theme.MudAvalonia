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
    private static (Button Keyboard, Button Pointer) Focus(bool? token = null)
    {
        var keyboard = new Button { Content = "Save" };
        var pointer = new Button { Content = "Cancel" };
        var window = new Window { Content = new StackPanel { Children = { keyboard, pointer } } };
        if (token is { } value)
        {
            window.Resources["FiliFocusRing"] = value;
        }

        window.Show();
        Dispatcher.UIThread.RunJobs();
        keyboard.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();
        Assert.True(keyboard.IsKeyboardFocusWithin);
        return (keyboard, pointer);
    }

    private static Control? Ring(Control control) =>
        AdornerLayer.GetAdornerLayer(control)!.Children.OfType<Control>().SingleOrDefault();

    /// <summary>
    /// By default, in every variant, a 2px ring in the variant's text-primary, 2px outside a
    /// control the keyboard focuses.
    /// </summary>
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public Task KeyboardFocusDrawsTheRing(string name) => In(Variant(name), () =>
    {
        var ring = Assert.IsAssignableFrom<Border>(Ring(Focus().Keyboard));

        Assert.Equal(new Thickness(2), ring.BorderThickness);
        Assert.Equal(new Thickness(-4), ring.Margin);
        Assert.True(Application.Current!.TryFindResource("FiliTextPrimaryColor", Variant(name), out var text));
        Assert.Equal((Color)text!, Assert.IsAssignableFrom<ISolidColorBrush>(ring.BorderBrush).Color);
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
    /// across a pill and stood off a square.
    /// </summary>
    [Theory]
    [InlineData(0, 4)]
    [InlineData(4, 8)]
    [InlineData(18, 22)]
    public Task TheRingFollowsTheControlsCorners(double radius, double ring) => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save", CornerRadius = new CornerRadius(radius) };

        Assert.Equal(new CornerRadius(ring), RingRound(button).CornerRadius);
        ((Window)TopLevel.GetTopLevel(button)!).Close();
    });

    /// <summary>Each corner on its own, for a control rounded on one side only.</summary>
    [Fact]
    public Task EachCornerFollowsItsOwn() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save", CornerRadius = new CornerRadius(8, 0, 0, 8) };

        Assert.Equal(new CornerRadius(12, 4, 4, 12), RingRound(button).CornerRadius);
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
