using Avalonia;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether a row is in a <c>multi-selection</c> list or tree: set on the ListBox or TreeView by
/// the class's style, and inherited by its rows, whose theme then swaps in the template that
/// draws MudBlazor's checkbox.
/// </summary>
/// <remarks>
/// <para>
/// Every row of every list carried the checkbox hidden, a Panel, a halo and two glyphs, four of
/// its dozen visuals, for a look few lists use. Only a row of the theme's own reads this, so an
/// app's own row template is left as it is.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The classes are the
/// interface.
/// </para>
/// </remarks>
internal sealed class MultiSelection
{
    private MultiSelection()
    {
    }

    public static readonly AttachedProperty<bool> IsOnProperty =
        AvaloniaProperty.RegisterAttached<MultiSelection, Control, bool>("IsOn", inherits: true);

    public static bool GetIsOn(Control control) => control.GetValue(IsOnProperty);

    public static void SetIsOn(Control control, bool value) => control.SetValue(IsOnProperty, value);
}
