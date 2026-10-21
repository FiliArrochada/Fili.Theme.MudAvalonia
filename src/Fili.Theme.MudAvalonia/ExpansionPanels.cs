using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// What MudExpansionPanels knows about its panels and a selector cannot: where each one stands in
/// the group, which are next to an expanded one, and - with <c>MultiExpansion</c> off, as it is by
/// default - that opening one closes the others. Set on every panel of an <c>expansion-panels</c>
/// group by its style.
/// </summary>
/// <remarks>
/// <para>
/// _expansionpanel.scss rounds and rules a panel by what is next to it. A panel before an expanded
/// one is <c>.mud-panel-next-expanded</c> - no divider under it, its bottom corners rounded - and
/// the one after it takes rounded top corners (<c>.mud-panel-expanded + .mud-expand-panel</c>). A
/// selector cannot ask about a sibling, so this keeps <c>:next-expanded</c> and
/// <c>:previous-expanded</c> on each panel, and <c>:first-panel</c> and <c>:last-panel</c> for
/// <c>:first-child</c> and <c>:last-child</c>, which <c>:nth-child</c> cannot see through an
/// <c>ItemsControl</c>'s presenters. <c>:joined</c> marks a panel in a group. FiliTheme.axaml
/// styles them.
/// </para>
/// <para>
/// A group's panels are the children of a panel - the <c>StackPanel</c> they are written in, or an
/// <c>ItemsControl</c>'s items panel - each either an Expander or the presenter one was generated
/// into. Every child counts as a neighbour, as in CSS. Internal, like the converters: a theme
/// needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class ExpansionPanels
{
    private const string Joined = ":joined";
    private const string FirstPanel = ":first-panel";
    private const string LastPanel = ":last-panel";
    private const string NextExpanded = ":next-expanded";
    private const string PreviousExpanded = ":previous-expanded";

    private ExpansionPanels()
    {
    }

    public static readonly AttachedProperty<bool> JoinsProperty =
        AvaloniaProperty.RegisterAttached<ExpansionPanels, Expander, bool>("Joins");

    /// <summary>MudExpansionPanels' <c>MultiExpansion</c>: off, opening a panel closes the rest.</summary>
    public static readonly AttachedProperty<bool> MultiExpansionProperty =
        AvaloniaProperty.RegisterAttached<ExpansionPanels, Expander, bool>("MultiExpansion");

    public static bool GetJoins(Expander panel) => panel.GetValue(JoinsProperty);

    public static void SetJoins(Expander panel, bool value) => panel.SetValue(JoinsProperty, value);

    public static bool GetMultiExpansion(Expander panel) => panel.GetValue(MultiExpansionProperty);

    public static void SetMultiExpansion(Expander panel, bool value) => panel.SetValue(MultiExpansionProperty, value);

    static ExpansionPanels()
    {
        JoinsProperty.Changed.AddClassHandler<Expander>((panel, e) =>
        {
            var joins = e.GetNewValue<bool>();
            panel.AttachedToVisualTree -= OnAttached;
            panel.DetachedFromVisualTree -= OnDetached;
            if (joins)
            {
                panel.AttachedToVisualTree += OnAttached;
                panel.DetachedFromVisualTree += OnDetached;
            }
            else
            {
                var pseudo = (IPseudoClasses)panel.Classes;
                pseudo.Set(FirstPanel, false);
                pseudo.Set(LastPanel, false);
                pseudo.Set(NextExpanded, false);
                pseudo.Set(PreviousExpanded, false);
            }

            ((IPseudoClasses)panel.Classes).Set(Joined, joins);
            RefreshLater(GroupOf(panel));
        });

        Expander.IsExpandedProperty.Changed.AddClassHandler<Expander>((panel, e) =>
        {
            if (!GetJoins(panel))
            {
                return;
            }

            if (e.GetNewValue<bool>() && !GetMultiExpansion(panel))
            {
                // MudExpansionPanels.CollapseAllExceptAsync: the one just opened stays open.
                foreach (var other in PanelsOf(GroupOf(panel)).Where(p => p != panel && p.IsExpanded && GetJoins(p)))
                {
                    other.SetCurrentValue(Expander.IsExpandedProperty, false);
                }
            }

            Refresh(GroupOf(panel));
        });
    }

    private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var panel = (Expander)sender!;
        var group = GroupOf(panel);

        // MudExpansionPanels.AddPanelAsync: a panel that arrives open while another is open closes.
        // Only panels already there count: a group attaches its children in order, so of panels
        // written open the first stays open, as the first one added does in MudBlazor.
        if (panel.IsExpanded && !GetMultiExpansion(panel) &&
            PanelsOf(group).Any(p => p != panel && p.IsExpanded && GetJoins(p) && p.IsAttachedToVisualTree()))
        {
            panel.SetCurrentValue(Expander.IsExpandedProperty, false);
        }

        Refresh(group);
    }

    private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // The panel has already left its parent's visual children; its group is where it was.
        var from = e.AttachmentPoint as Visual;
        RefreshLater(from is ContentPresenter presenter && presenter.GetVisualParent() is Panel ? presenter.GetVisualParent() : from);
    }

    /// <summary>The panel's place in its group: the panel, or the presenter it was generated into.</summary>
    private static Visual SlotOf(Expander panel) =>
        panel.GetVisualParent() is ContentPresenter presenter && presenter.Child == panel && presenter.GetVisualParent() is Panel
            ? presenter
            : panel;

    private static Visual? GroupOf(Expander panel) => SlotOf(panel).GetVisualParent();

    private static Expander? PanelIn(Visual child) =>
        child as Expander ?? (child as ContentPresenter)?.Child as Expander;

    private static IEnumerable<Expander> PanelsOf(Visual? group) =>
        group is null ? [] : group.GetVisualChildren().Select(PanelIn).OfType<Expander>();

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

        var children = group.GetVisualChildren().ToList();
        for (var i = 0; i < children.Count; i++)
        {
            if (PanelIn(children[i]) is not { } panel || !GetJoins(panel))
            {
                continue;
            }

            var pseudo = (IPseudoClasses)panel.Classes;
            pseudo.Set(FirstPanel, i == 0);
            pseudo.Set(LastPanel, i == children.Count - 1);
            pseudo.Set(NextExpanded, i + 1 < children.Count && PanelIn(children[i + 1]) is { IsExpanded: true });
            pseudo.Set(PreviousExpanded, i > 0 && PanelIn(children[i - 1]) is { IsExpanded: true });
        }
    }
}
