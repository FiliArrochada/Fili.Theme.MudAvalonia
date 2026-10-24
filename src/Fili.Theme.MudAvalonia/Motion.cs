using Avalonia;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether motion is reduced: the <c>FiliReducedMotion</c> token, set True in an app's resources
/// for people whose system or settings ask for less motion. Every window and popup takes the token
/// (a style on <c>TopLevel</c>) and passes it down as this inherited value, which the theme's
/// animations read: the styles through a property selector, the helpers in code.
/// </summary>
/// <remarks>
/// <para>
/// MudBlazor has no such switch. With it on, nothing the theme draws moves or fades on its own:
/// no ripple, no popup fade, no skeleton pulse or wave, no snackbar slide, an expansion panel and
/// a scrim appear at once, and a tab's indicator jumps. A progress indicator keeps moving, as it
/// is the only sign that work is going on.
/// </para>
/// <para>
/// A token rather than a class, because a popup is a window of its own that a class on the
/// app's window would not reach; a resource reaches it through the popup's parent. And an
/// inherited value rather than a lookup in each animation, because a style can only test a
/// property.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The token is the
/// interface.
/// </para>
/// </remarks>
internal sealed class Motion
{
    private Motion()
    {
    }

    public static readonly AttachedProperty<bool> ReducedProperty =
        AvaloniaProperty.RegisterAttached<Motion, Visual, bool>("Reduced", inherits: true);

    public static bool GetReduced(Visual visual) => visual.GetValue(ReducedProperty);

    public static void SetReduced(Visual visual, bool value) => visual.SetValue(ReducedProperty, value);
}
