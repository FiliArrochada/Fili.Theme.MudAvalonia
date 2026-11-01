using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether the focus ring is drawn: the <c>FiliFocusRing</c> token, read by each window's and
/// popup's <c>AdornerLayer</c>, which then draws the ring round whichever control the keyboard
/// focuses (its <c>DefaultFocusAdorner</c>). A control's own <c>FocusAdorner</c> still wins.
/// </summary>
/// <remarks>
/// <para>
/// On in every variant: the state layer's tint, MudBlazor's only focus indicator, is about 1.1:1
/// and a keyboard user loses it. The layer adorns only a control focused by the keyboard, so a
/// click or a touch draws no ring. False in an app's resources turns it off in every variant,
/// leaving MudBlazor's tint alone.
/// </para>
/// <para>
/// A property on the layer rather than a lookup, because a style can only test a property, and a
/// token rather than a class for the reason <see cref="Motion"/> gives: a popup's layer is in a
/// window of its own.
/// </para>
/// </remarks>
internal sealed class FocusRing
{
    /// <summary>How far outside the control the ring's outer edge is: a 2px gap, then 2px of ring.</summary>
    private const double Outset = 4;

    private FocusRing()
    {
    }

    static FocusRing()
    {
        AdornerLayer.AdornedElementProperty.Changed.AddClassHandler<Border>(OnAdornedElementChanged);
    }

    public static readonly AttachedProperty<bool> ShownProperty =
        AvaloniaProperty.RegisterAttached<FocusRing, AdornerLayer, bool>("Shown");

    /// <summary>
    /// Set on the ring: its corners follow the control it frames, each the control's own radius
    /// plus <see cref="Outset"/>, so the ring runs parallel to a pill, a circle or a square. XAML
    /// cannot say this: the adorned element is a <c>Visual</c>, and only some visuals have a
    /// corner radius. The ring is built afresh each time a control takes keyboard focus.
    /// </summary>
    public static readonly AttachedProperty<bool> FollowsProperty =
        AvaloniaProperty.RegisterAttached<FocusRing, Border, bool>("Follows");

    public static bool GetShown(AdornerLayer layer) => layer.GetValue(ShownProperty);

    public static void SetShown(AdornerLayer layer, bool value) => layer.SetValue(ShownProperty, value);

    public static bool GetFollows(Border ring) => ring.GetValue(FollowsProperty);

    public static void SetFollows(Border ring, bool value) => ring.SetValue(FollowsProperty, value);

    private static void OnAdornedElementChanged(Border ring, AvaloniaPropertyChangedEventArgs e)
    {
        if (!GetFollows(ring))
        {
            return;
        }

        var radius = e.NewValue switch
        {
            TemplatedControl control => control.CornerRadius,
            Border border => border.CornerRadius,
            _ => default,
        };
        ring.CornerRadius = new CornerRadius(
            radius.TopLeft + Outset, radius.TopRight + Outset, radius.BottomRight + Outset, radius.BottomLeft + Outset);
    }
}
