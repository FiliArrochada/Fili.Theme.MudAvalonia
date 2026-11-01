using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether motion is reduced: the <c>FiliReducedMotion</c> token, set True in an app's resources
/// for people whose system or settings ask for less motion. Every window and popup takes the token
/// (a style on <c>TopLevel</c>) and passes it down as this inherited value, which the theme's
/// animations read: the styles through a property selector, the helpers in code, and every
/// transition the theme declares through <see cref="ReducibleProperty"/>.
/// </summary>
/// <remarks>
/// <para>
/// MudBlazor has no such switch. With it on: no ripple, no popup fade, no skeleton pulse or wave,
/// no snackbar slide, an expansion panel and a scrim appear at once, a tab's indicator jumps and
/// a striped bar's bands stand still. And no transition the theme declares runs: a field's label
/// floats and its focus underline draws at once, the switch's thumb jumps, a chevron or a data
/// grid's sort arrow turns at once, a slider's value label shows without fading, the drawer pane
/// opens without sliding, a determinate circular progress jumps to its value, and a hover or
/// selection colour changes without a fade. An indeterminate progress indicator keeps moving, as
/// it is the only sign that work is going on.
/// </para>
/// <para>
/// A transition is reduced by marking the element that holds it <c>Motion.Reducible</c>: while
/// that element's <c>Motion.Reduced</c> is True its <c>Transitions</c> (and a switch's
/// <c>KnobTransitions</c>) are held back, set to null at their own priority, and put back when
/// it turns False. A style that sets new transitions meanwhile has them held back too. Only marked
/// elements: an app's own transitions are the app's to keep or drop.
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
    private sealed class Held
    {
        public Transitions? Transitions;
        public Transitions? KnobTransitions;
    }

    private static readonly ConditionalWeakTable<Visual, Held> HeldBack = new();

    private Motion()
    {
    }

    public static readonly AttachedProperty<bool> ReducedProperty =
        AvaloniaProperty.RegisterAttached<Motion, Visual, bool>("Reduced", inherits: true);

    /// <summary>Set by the theme on an element whose transitions stop with reduced motion.</summary>
    public static readonly AttachedProperty<bool> ReducibleProperty =
        AvaloniaProperty.RegisterAttached<Motion, Visual, bool>("Reducible");

    static Motion()
    {
        ReducedProperty.Changed.AddClassHandler<Visual>((visual, _) => Apply(visual));
        ReducibleProperty.Changed.AddClassHandler<Visual>((visual, _) => Apply(visual));
        Animatable.TransitionsProperty.Changed.AddClassHandler<Visual>((visual, e) =>
        {
            if (e.NewValue is not null)
            {
                Apply(visual);
            }
        });
        ToggleSwitch.KnobTransitionsProperty.Changed.AddClassHandler<ToggleSwitch>((toggle, e) =>
        {
            if (e.NewValue is not null)
            {
                Apply(toggle);
            }
        });
    }

    public static bool GetReduced(Visual visual) => visual.GetValue(ReducedProperty);

    public static void SetReduced(Visual visual, bool value) => visual.SetValue(ReducedProperty, value);

    public static bool GetReducible(Visual visual) => visual.GetValue(ReducibleProperty);

    public static void SetReducible(Visual visual, bool value) => visual.SetValue(ReducibleProperty, value);

    private static void Apply(Visual visual)
    {
        if (!GetReducible(visual))
        {
            Restore(visual);
            return;
        }

        if (!GetReduced(visual))
        {
            Restore(visual);
            return;
        }

        var held = HeldBack.GetOrCreateValue(visual);
        if (visual.Transitions is { } transitions)
        {
            held.Transitions = transitions;
            visual.SetCurrentValue(Animatable.TransitionsProperty, null);
        }

        if (visual is ToggleSwitch toggle && toggle.KnobTransitions is { } knob)
        {
            held.KnobTransitions = knob;
            // Declared non-null, but ToggleSwitch hands it to its knob panel as is, and a null
            // there is no transition.
            toggle.SetCurrentValue(ToggleSwitch.KnobTransitionsProperty, null!);
        }
    }

    private static void Restore(Visual visual)
    {
        if (!HeldBack.TryGetValue(visual, out var held))
        {
            return;
        }

        HeldBack.Remove(visual);
        if (held.Transitions is { } transitions && visual.Transitions is null)
        {
            visual.SetCurrentValue(Animatable.TransitionsProperty, transitions);
        }

        if (visual is ToggleSwitch toggle && held.KnobTransitions is { } knob && toggle.KnobTransitions is null)
        {
            toggle.SetCurrentValue(ToggleSwitch.KnobTransitionsProperty, knob);
        }
    }
}
