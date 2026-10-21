using Avalonia;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether <c>material-elevation-overlay</c> is on for a control: set on the control that carries
/// the class, and inherited by everything inside it, popups included. The overlay's styles read it
/// on the surface itself.
/// </summary>
/// <remarks>
/// <para>
/// The overlay was a descendant selector, <c>.material-elevation-overlay Border.card</c> and one
/// per level, and that shape made it the theme's costliest feature: a style whose last part is a
/// <c>Border</c> is matched against every Border in the window - those in every control's
/// template included - and a descendant one watches the classes of every ancestor of each. The 27
/// of them cost more than the theme's other top-level styles together, in every app, overlay or
/// not. A property on the Border itself is watched there and nowhere else.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The class is the
/// interface.
/// </para>
/// </remarks>
internal sealed class ElevationOverlay
{
    private ElevationOverlay()
    {
    }

    public static readonly AttachedProperty<bool> IsOnProperty =
        AvaloniaProperty.RegisterAttached<ElevationOverlay, Control, bool>("IsOn", inherits: true);

    public static bool GetIsOn(Control control) => control.GetValue(IsOnProperty);

    public static void SetIsOn(Control control, bool value) => control.SetValue(IsOnProperty, value);
}
