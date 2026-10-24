using Avalonia;
using Avalonia.Media;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// The text colour a control sits in, for its template parts to paint with: MudBlazor's
/// <c>Color.Inherit</c>, <c>.mud-inherit-text { color: inherit }</c>, on a part that is not text.
/// </summary>
/// <remarks>
/// <para>
/// A checkbox's glyph is a Path filled from a style, and the control's own Foreground is its
/// label's text-primary, not the colour round it. The <c>inherit</c> class binds this on the
/// control to its parent's inherited text colour, as Button's <c>inherit</c> binds Foreground, and
/// the part styles read it back through <c>$parent[CheckBox]</c>: a style on a template part
/// cannot name the control's parent itself.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The classes are the
/// interface.
/// </para>
/// </remarks>
internal sealed class InheritedColour
{
    private InheritedColour()
    {
    }

    public static readonly AttachedProperty<IBrush?> BrushProperty =
        AvaloniaProperty.RegisterAttached<InheritedColour, AvaloniaObject, IBrush?>("Brush");

    public static IBrush? GetBrush(AvaloniaObject element) => element.GetValue(BrushProperty);

    public static void SetBrush(AvaloniaObject element, IBrush? value) => element.SetValue(BrushProperty, value);
}
