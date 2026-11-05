using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Whether a menu's selected row is where the keyboard is, so the row is tinted for the keyboard
/// and not for being selected by the menu opening.
/// </summary>
/// <remarks>
/// <para>
/// A menu flyout or context menu focuses and selects its first row when it opens, however it was
/// opened, and the arrow keys move both the selection and the focus without <c>:focus-visible</c>.
/// MudBlazor tints a row on hover and on keyboard focus only, and a selector can test a property,
/// not which input came last. So a menu (a <c>MenuBase</c>: a flyout's presenter, a context menu)
/// carries the inherited <see cref="KeyboardProperty"/>: on opening, whether the last input in the
/// window it opened from was a key; then True on a key in the menu and False on a press of the
/// pointer. A row tints <c>:selected</c> only while it is True.
/// </para>
/// <para>
/// Internal, like the other helpers: a theme needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class MenuHighlight
{
    private MenuHighlight()
    {
    }

    /// <summary>The keyboard is in use in this menu. Inherited, so its rows and submenus read it.</summary>
    public static readonly AttachedProperty<bool> KeyboardProperty =
        AvaloniaProperty.RegisterAttached<MenuHighlight, Control, bool>("Keyboard", inherits: true);

    /// <summary>On a window: the last key or pointer press in it was a key.</summary>
    private static readonly AttachedProperty<bool> LastInputWasKeyProperty =
        AvaloniaProperty.RegisterAttached<MenuHighlight, TopLevel, bool>("LastInputWasKey");

    static MenuHighlight()
    {
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(
            (root, _) => root.SetValue(LastInputWasKeyProperty, true), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(
            (root, _) => root.SetValue(LastInputWasKeyProperty, false), RoutingStrategies.Tunnel, handledEventsToo: true);

        InputElement.KeyDownEvent.AddClassHandler<MenuBase>(
            (menu, _) => menu.SetValue(KeyboardProperty, true), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerPressedEvent.AddClassHandler<MenuBase>(
            (menu, _) => menu.SetValue(KeyboardProperty, false), RoutingStrategies.Tunnel, handledEventsToo: true);

        // A popup's content is loaded each time it opens. Its window is the one it opened from,
        // reached through the logical tree, as the popup's own root is a top level of its own.
        Control.LoadedEvent.AddClassHandler<MenuBase>((menu, e) =>
        {
            if (e.Source == menu)
            {
                menu.SetValue(KeyboardProperty, menu.FindLogicalAncestorOfType<TopLevel>()?.GetValue(LastInputWasKeyProperty) == true);
            }
        });
    }

    public static bool GetKeyboard(Control control) => control.GetValue(KeyboardProperty);

    public static void SetKeyboard(Control control, bool value) => control.SetValue(KeyboardProperty, value);
}
