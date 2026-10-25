using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudTooltip's placements: a tooltip placed Top, Bottom, Left or Right sits centred on that side
/// of its target, 10px from it. Set on a ToolTip by its theme.
/// </summary>
/// <remarks>
/// <para>
/// Avalonia moves every tooltip popup by <c>ToolTip.VerticalOffset</c>, 20 by default, whatever its
/// placement: the distance under the pointer for Pointer, its default. For the four sides that
/// puts a Top tooltip 20px over its target, a Bottom one 20px below it, and Left and Right ones
/// 20px low. MudBlazor's are centred, 10px off (<c>_tooltip.scss</c>'s translate). So when a
/// tooltip opens on a side, this sets the target's offset to 0 at StyleTrigger priority, where a
/// style would, so an app's own offset still wins; the template keeps the 10px on the side facing
/// the target, the arrow's 6 and 4 beyond it. Pointer keeps Avalonia's 20.
/// </para>
/// <para>
/// The ToolTip's logical parent is its Popup, whose PlacementTarget is set before the tooltip joins
/// the tree, and the tree is joined before the popup opens, so the offset is right on the first
/// frame. Internal, like the other helpers: a theme needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class TooltipPlacement
{
    private TooltipPlacement()
    {
    }

    public static readonly AttachedProperty<bool> PlacesProperty =
        AvaloniaProperty.RegisterAttached<TooltipPlacement, ToolTip, bool>("Places");

    public static bool GetPlaces(ToolTip tip) => tip.GetValue(PlacesProperty);

    public static void SetPlaces(ToolTip tip, bool value) => tip.SetValue(PlacesProperty, value);

    /// <summary>The offset this set on each target: an entry below LocalValue, disposed before the next.</summary>
    private static readonly ConditionalWeakTable<Control, IDisposable> Set = new();

    static TooltipPlacement()
    {
        PlacesProperty.Changed.AddClassHandler<ToolTip>((tip, e) =>
        {
            tip.AttachedToLogicalTree -= OnAttached;
            if (e.GetNewValue<bool>())
            {
                tip.AttachedToLogicalTree += OnAttached;
                Place(tip);
            }
        });
    }

    private static void OnAttached(object? sender, LogicalTreeAttachmentEventArgs e) => Place((ToolTip)sender!);

    private static void Place(ToolTip tip)
    {
        if (tip.Parent is not Popup { PlacementTarget: { } target })
        {
            return;
        }

        if (Set.TryGetValue(target, out var previous))
        {
            previous.Dispose();
            Set.Remove(target);
        }

        if (ToolTip.GetPlacement(target) is PlacementMode.Top or PlacementMode.Bottom or PlacementMode.Left or PlacementMode.Right
            && target.SetValue(ToolTip.VerticalOffsetProperty, 0d, BindingPriority.StyleTrigger) is { } entry)
        {
            Set.Add(target, entry);
        }
    }
}
