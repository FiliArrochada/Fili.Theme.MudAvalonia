using Avalonia;
using Avalonia.Controls.Primitives;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether the focus ring is drawn: the <c>FiliFocusRing</c> token, read by each window's and
/// popup's <c>AdornerLayer</c>, which then draws the ring round whichever control the keyboard
/// focuses (its <c>DefaultFocusAdorner</c>). A control's own <c>FocusAdorner</c> still wins.
/// </summary>
/// <remarks>
/// <para>
/// Off in light and dark, as MudBlazor draws no ring: keyboard focus is the state layer's tint.
/// On in high contrast, where the tint is no indicator. True or False in an app's resources sets
/// it in every variant.
/// </para>
/// <para>
/// A property on the layer rather than a lookup, because a style can only test a property, and a
/// token rather than a class for the reason <see cref="Motion"/> gives: a popup's layer is in a
/// window of its own.
/// </para>
/// </remarks>
internal sealed class FocusRing
{
    private FocusRing()
    {
    }

    public static readonly AttachedProperty<bool> ShownProperty =
        AvaloniaProperty.RegisterAttached<FocusRing, AdornerLayer, bool>("Shown");

    public static bool GetShown(AdornerLayer layer) => layer.GetValue(ShownProperty);

    public static void SetShown(AdornerLayer layer, bool value) => layer.SetValue(ShownProperty, value);
}
