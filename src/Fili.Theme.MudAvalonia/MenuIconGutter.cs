using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudMenu's icon gutter: a menu item with no icon lines its text up with the items that have
/// one, but only when the list has any. Set on every dropdown row by its theme.
/// </summary>
/// <remarks>
/// <para>
/// _menu.scss: <c>.mud-menu-list:has(.mud-menu-item-icon) .mud-menu-item:not(:has(.mud-menu-item-icon))
/// .mud-menu-item-text { margin-inline-start: 36px }</c>, the icon's 24px and its 12px. Avalonia
/// marks an item that has an icon with <c>:icon</c>, but a selector cannot ask about the item's
/// siblings, so this does: it gives every row in a list with an icon, and none in a list
/// without, the <c>:icon-gutter</c> pseudo-class that keeps the icon column open.
/// </para>
/// <para>
/// The rows are the visual children of the menu's items panel, which holds them whether the menu
/// was written as MenuItems or generated from ItemsSource. Internal, like the converters: a
/// template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class MenuIconGutter
{
    private const string Gutter = ":icon-gutter";

    private MenuIconGutter()
    {
    }

    public static readonly AttachedProperty<bool> AlignsProperty =
        AvaloniaProperty.RegisterAttached<MenuIconGutter, MenuItem, bool>("Aligns");

    public static bool GetAligns(MenuItem item) => item.GetValue(AlignsProperty);

    public static void SetAligns(MenuItem item, bool value) => item.SetValue(AlignsProperty, value);

    static MenuIconGutter()
    {
        AlignsProperty.Changed.AddClassHandler<MenuItem>((item, e) =>
        {
            item.AttachedToVisualTree -= OnAttached;
            item.DetachedFromVisualTree -= OnDetached;
            if (e.GetNewValue<bool>())
            {
                item.AttachedToVisualTree += OnAttached;
                item.DetachedFromVisualTree += OnDetached;
            }

            Refresh(item.GetVisualParent());
        });

        MenuItem.IconProperty.Changed.AddClassHandler<MenuItem>((item, _) =>
        {
            if (GetAligns(item))
            {
                Refresh(item.GetVisualParent());
            }
        });
    }

    private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) =>
        Refresh(((Visual)sender!).GetVisualParent());

    private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) =>
        Refresh(e.AttachmentPoint as Visual);

    private static void Refresh(Visual? panel)
    {
        if (panel is null)
        {
            return;
        }

        var rows = panel.GetVisualChildren().OfType<MenuItem>().Where(GetAligns).ToList();
        var any = rows.Any(row => row.Icon is not null);
        foreach (var row in rows)
        {
            ((IPseudoClasses)row.Classes).Set(Gutter, any);
        }
    }
}
