using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudExpansionPanels' view of its panels' neighbours: which panel comes just before an expanded
/// one, and which just after. Set on every panel of an <c>expansion-panels</c> group by its style.
/// </summary>
/// <remarks>
/// <para>
/// _expansionpanel.scss rounds and rules a panel by what is next to it. A panel before an expanded
/// one is <c>.mud-panel-next-expanded</c> - no divider under it, its bottom corners rounded - and
/// the one after it takes rounded top corners (<c>.mud-panel-expanded + .mud-expand-panel</c>). A
/// selector cannot ask about a sibling, so this does: it keeps <c>:next-expanded</c> and
/// <c>:previous-expanded</c> on each panel, and FiliTheme.axaml styles them. First and last are
/// <c>:nth-child</c>, which a selector can already see.
/// </para>
/// <para>
/// The panels are the visual children of the group's panel, so a group is a panel of Expanders
/// written in place: a <c>StackPanel</c>, not an <c>ItemsControl</c>, whose items each sit in a
/// presenter of their own. Internal, like the converters: a theme needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class ExpansionPanels
{
    private const string NextExpanded = ":next-expanded";
    private const string PreviousExpanded = ":previous-expanded";

    private ExpansionPanels()
    {
    }

    public static readonly AttachedProperty<bool> JoinsProperty =
        AvaloniaProperty.RegisterAttached<ExpansionPanels, Expander, bool>("Joins");

    public static bool GetJoins(Expander panel) => panel.GetValue(JoinsProperty);

    public static void SetJoins(Expander panel, bool value) => panel.SetValue(JoinsProperty, value);

    static ExpansionPanels()
    {
        JoinsProperty.Changed.AddClassHandler<Expander>((panel, e) =>
        {
            panel.AttachedToVisualTree -= OnAttached;
            panel.DetachedFromVisualTree -= OnDetached;
            if (e.GetNewValue<bool>())
            {
                panel.AttachedToVisualTree += OnAttached;
                panel.DetachedFromVisualTree += OnDetached;
            }
            else
            {
                ((IPseudoClasses)panel.Classes).Set(NextExpanded, false);
                ((IPseudoClasses)panel.Classes).Set(PreviousExpanded, false);
            }

            RefreshLater(panel.GetVisualParent());
        });

        Expander.IsExpandedProperty.Changed.AddClassHandler<Expander>((panel, _) =>
        {
            if (GetJoins(panel))
            {
                Refresh(panel.GetVisualParent());
            }
        });
    }

    private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) =>
        Refresh(((Visual)sender!).GetVisualParent());

    private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) =>
        RefreshLater(e.AttachmentPoint as Visual);

    /// <summary>
    /// After the change in hand: a panel being removed is still among its group's children while
    /// it detaches, and loses its style - and with it <see cref="JoinsProperty"/> - first.
    /// </summary>
    private static void RefreshLater(Visual? group)
    {
        if (group is not null)
        {
            Dispatcher.UIThread.Post(() => Refresh(group));
        }
    }

    private static void Refresh(Visual? group)
    {
        if (group is null)
        {
            return;
        }

        // Every child counts as a neighbour, as in CSS: a panel's sibling is whatever is next to it.
        var children = group.GetVisualChildren().ToList();
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is not Expander panel || !GetJoins(panel))
            {
                continue;
            }

            var pseudo = (IPseudoClasses)panel.Classes;
            pseudo.Set(NextExpanded, i + 1 < children.Count && children[i + 1] is Expander { IsExpanded: true });
            pseudo.Set(PreviousExpanded, i > 0 && children[i - 1] is Expander { IsExpanded: true });
        }
    }
}
