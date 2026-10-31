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
/// FiliFocusRing: off in light and dark, where keyboard focus is MudBlazor's state-layer tint and
/// nothing else, on in high contrast, and True or False in an app's resources in any variant. The
/// ring is the AdornerLayer's default focus adorner, so it follows keyboard focus only.
/// </summary>
public class FocusRingTests
{
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

    [Fact]
    public Task NoRingInLight() => In(ThemeVariant.Light, () => Assert.Null(Ring(Focus().Keyboard)));

    [Fact]
    public Task NoRingInDark() => In(ThemeVariant.Dark, () => Assert.Null(Ring(Focus().Keyboard)));

    /// <summary>A 2px ring in text-primary, white, 2px outside a control the keyboard focuses.</summary>
    [Fact]
    public Task HighContrastDrawsTheRing() => In(FiliThemeVariants.HighContrast, () =>
    {
        var ring = Assert.IsAssignableFrom<Border>(Ring(Focus().Keyboard));

        Assert.Equal(new Thickness(2), ring.BorderThickness);
        Assert.Equal(new Thickness(-4), ring.Margin);
        Assert.True(Application.Current!.TryFindResource("FiliTextPrimaryColor", FiliThemeVariants.HighContrast, out var text));
        Assert.Equal((Color)text!, Assert.IsAssignableFrom<ISolidColorBrush>(ring.BorderBrush).Color);
    });

    /// <summary>A click moves focus without a ring: the ring is keyboard focus's alone.</summary>
    [Fact]
    public Task PointerFocusDrawsNoRing() => In(FiliThemeVariants.HighContrast, () =>
    {
        var (keyboard, pointer) = Focus();

        pointer.Focus(NavigationMethod.Pointer);
        Dispatcher.UIThread.RunJobs();

        Assert.Null(Ring(keyboard));
    });

    [Fact]
    public Task TheTokenTurnsItOnInLight() => In(ThemeVariant.Light, () => Assert.NotNull(Ring(Focus(token: true).Keyboard)));

    [Fact]
    public Task TheTokenTurnsItOffInHighContrast() =>
        In(FiliThemeVariants.HighContrast, () => Assert.Null(Ring(Focus(token: false).Keyboard)));

    /// <summary>Declared in every variant: a variant that forgot it would take dark's value.</summary>
    [Fact]
    public Task TheTokenIsDeclaredInEveryVariant() => UiThread.RunAsync(() =>
    {
        foreach (var (variant, expected) in new[] { (ThemeVariant.Light, false), (ThemeVariant.Dark, false), (FiliThemeVariants.HighContrast, true) })
        {
            Assert.True(Application.Current!.TryFindResource("FiliFocusRing", variant, out var value), $"FiliFocusRing did not resolve in {variant}.");
            Assert.Equal(expected, Assert.IsType<bool>(value));
        }
    });
}
