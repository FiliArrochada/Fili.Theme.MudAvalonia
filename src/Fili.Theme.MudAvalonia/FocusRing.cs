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
/// leaving MudBlazor's tint alone. The ring uses adjacent text-primary and white strokes so at
/// least one remains visible against light, dark and coloured surfaces.
/// </para>
/// <para>
/// A property on the layer rather than a lookup, because a style can only test a property, and a
/// token rather than a class for the reason <see cref="Motion"/> gives: a popup's layer is in a
/// window of its own.
/// </para>
/// </remarks>
internal sealed class FocusRing
{
    /// <summary>
    /// The inner stroke's width, against the control: 2px of white where a single ring had a
    /// 2px gap. The outer stroke, <see cref="WidthProperty"/>, sits outside it.
    /// </summary>
    private const double Gap = 2;

    private FocusRing()
    {
    }

    static FocusRing()
    {
        AdornerLayer.AdornedElementProperty.Changed.AddClassHandler<Border>((ring, _) => Fit(ring));
        WidthProperty.Changed.AddClassHandler<Border>((ring, _) => Fit(ring));
    }

    public static readonly AttachedProperty<bool> ShownProperty =
        AvaloniaProperty.RegisterAttached<FocusRing, AdornerLayer, bool>("Shown");

    /// <summary>
    /// Set on the ring: its corners follow the control it frames, each the control's own radius
    /// plus the ring's outset (<see cref="Gap"/> plus <see cref="WidthProperty"/>), so the ring
    /// runs parallel to a pill, a circle or a square. XAML cannot say this: the adorned element
    /// is a <c>Visual</c>, and only some visuals have a corner radius. The ring is built afresh
    /// each time a control takes keyboard focus.
    /// </summary>
    public static readonly AttachedProperty<bool> FollowsProperty =
        AvaloniaProperty.RegisterAttached<FocusRing, Border, bool>("Follows");

    /// <summary>
    /// The outer stroke's width, <c>FiliFocusRingThickness</c>: it sets the ring's
    /// <c>BorderThickness</c> and moves the ring out by as much, so the inner stroke keeps its
    /// 2px. XAML cannot add the two: a margin is not a sum of resources.
    /// </summary>
    public static readonly AttachedProperty<double> WidthProperty =
        AvaloniaProperty.RegisterAttached<FocusRing, Border, double>("Width", Gap);

    public static bool GetShown(AdornerLayer layer) => layer.GetValue(ShownProperty);

    public static void SetShown(AdornerLayer layer, bool value) => layer.SetValue(ShownProperty, value);

    public static bool GetFollows(Border ring) => ring.GetValue(FollowsProperty);

    public static void SetFollows(Border ring, bool value) => ring.SetValue(FollowsProperty, value);

    public static double GetWidth(Border ring) => ring.GetValue(WidthProperty);

    public static void SetWidth(Border ring, double value) => ring.SetValue(WidthProperty, value);

    private static void Fit(Border ring)
    {
        if (!GetFollows(ring))
        {
            return;
        }

        var width = Math.Max(0, GetWidth(ring));
        var outset = Gap + width;
        ring.BorderThickness = new Thickness(width);
        ring.Margin = new Thickness(-outset);

        var radius = AdornerLayer.GetAdornedElement(ring) switch
        {
            TemplatedControl control => control.CornerRadius,
            Border border => border.CornerRadius,
            _ => default,
        };
        ring.CornerRadius = new CornerRadius(
            radius.TopLeft + outset, radius.TopRight + outset, radius.BottomRight + outset, radius.BottomLeft + outset);

        // The inner stroke sits inside the outer one's width, so its corners are that much
        // tighter; the outer's own radius would leave a sliver of ground at each corner.
        if (ring.Child is Border inner)
        {
            var outer = ring.CornerRadius;
            inner.CornerRadius = new CornerRadius(
                Math.Max(0, outer.TopLeft - width),
                Math.Max(0, outer.TopRight - width),
                Math.Max(0, outer.BottomRight - width),
                Math.Max(0, outer.BottomLeft - width));
        }
    }
}
