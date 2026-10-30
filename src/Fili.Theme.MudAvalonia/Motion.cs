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
/// MudBlazor has no such switch. With it on: no ripple, no popup fade, no skeleton pulse or wave,
/// no snackbar slide, an expansion panel and a scrim appear at once, a tab's indicator jumps and
/// a striped bar's bands stand still. A progress indicator keeps moving, as it is the only sign
/// that work is going on.
/// </para>
/// <para>
/// Not read yet, so still moving: the drawer pane's slide (SplitView), the switch thumb's travel,
/// a field's floating label, the expander's and tree row's chevron, an expansion panel's margin
/// and a determinate circular progress's sweep.
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
