using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudExpansionPanels: <c>expansion-panels</c> on the StackPanel that holds the Expanders. The
/// group rounds its first panel's top and its last panel's bottom, rules a line under every
/// panel but the last, and opens an expanded panel 16px away from its neighbours, every corner
/// rounded - with the panel before it losing its rule and taking the bottom corners, and the one
/// after it taking the top ones (_expansionpanel.scss). With MultiExpansion off, as it is by
/// default, opening a panel closes the others (MudExpansionPanels.razor.cs).
/// </summary>
public class ExpansionPanelsTests
{
    private static readonly CornerRadius None = new(0);
    private static readonly CornerRadius All = new(4);
    private static readonly CornerRadius Top = new(4, 4, 0, 0);
    private static readonly CornerRadius Bottom = new(0, 0, 4, 4);
    private static readonly Thickness Rule = new(0, 0, 0, 1);
    private static readonly Thickness NoRule = new(0);

    private static (StackPanel Group, Expander[] Panels) Group(int count, string classes = "expansion-panels")
    {
        var panels = Enumerable.Range(1, count).Select(i => new Expander { Header = $"Panel {i}", Content = "Body" }).ToArray();
        var group = new StackPanel { Width = 400, HorizontalAlignment = HorizontalAlignment.Left };
        group.Classes.AddRange(classes.Split(' '));
        group.Children.AddRange(panels);
        Matrix.Show(group);
        return (group, panels);
    }

    /// <summary>The margin the style asked for: it is transitioned, so the live value lags.</summary>
    private static Thickness Margin(Expander panel) => panel.GetBaseValue(Layoutable.MarginProperty).GetValueOrDefault();

    private static void Expand(Expander panel, bool expanded = true)
    {
        panel.IsExpanded = expanded;
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public Task ClosedPanelsAreOneShapeRuledBetween() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3);

        Assert.Equal([Top, None, Bottom], p.Select(x => x.CornerRadius));
        Assert.Equal([Rule, Rule, NoRule], p.Select(x => x.BorderThickness));
        Assert.All(p, x => Assert.Equal(Matrix.Token("FiliLinesDefaultColor"), Matrix.Colour(x.BorderBrush)));
        Assert.All(p, x => Assert.Equal(default, Margin(x)));
    });

    /// <summary>The panel itself draws the rule and the corners: its root Border takes both.</summary>
    [Fact]
    public Task ThePanelsRootDrawsThem() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(2);
        var root = Matrix.Part<Border>(p[0], "PART_Root");

        Assert.Equal(Top, root.CornerRadius);
        Assert.Equal(Rule, root.BorderThickness);
    });

    [Fact]
    public Task AnExpandedPanelStandsApartWithItsNeighboursRounded() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3);
        Expand(p[1]);

        Assert.Equal(new Thickness(0, 16), Margin(p[1]));
        Assert.Equal(All, p[1].CornerRadius);
        Assert.Equal(NoRule, p[1].BorderThickness);

        // Before it, .mud-panel-next-expanded: no rule, and the first panel is now rounded all round.
        Assert.Equal(All, p[0].CornerRadius);
        Assert.Equal(NoRule, p[0].BorderThickness);

        // After it, the top corners - and the last panel's own bottom ones.
        Assert.Equal(All, p[2].CornerRadius);
    });

    [Fact]
    public Task InTheMiddleOfALongerGroupOnlyTheNeighboursChange() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(5);
        Expand(p[2]);

        Assert.Equal([Top, Bottom, All, Top, Bottom], p.Select(x => x.CornerRadius));
        Assert.Equal([Rule, NoRule, NoRule, Rule, NoRule], p.Select(x => x.BorderThickness));
        Assert.Equal([default, default, new Thickness(0, 16), default, default], p.Select(Margin));
    });

    /// <summary>No margin above the first panel or below the last, as :first-child and :last-child drop them.</summary>
    [Fact]
    public Task TheGroupsEdgesKeepNoMargin() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3, "expansion-panels multi-expansion");
        Expand(p[0]);
        Expand(p[2]);

        Assert.Equal(new Thickness(0, 0, 0, 16), Margin(p[0]));
        Assert.Equal(new Thickness(0, 16, 0, 0), Margin(p[2]));

        var (_, single) = Group(1);
        Assert.Equal(All, single[0].CornerRadius);
        Expand(single[0]);
        Assert.Equal(default, Margin(single[0]));
    });

    [Fact]
    public Task ClosingAPanelPutsTheGroupBack() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3);
        Expand(p[1]);
        Expand(p[1], false);

        Assert.Equal([Top, None, Bottom], p.Select(x => x.CornerRadius));
        Assert.Equal([Rule, Rule, NoRule], p.Select(x => x.BorderThickness));
        Assert.Equal(default, Margin(p[1]));
    });

    /// <summary>A panel added next to an expanded one learns its neighbour, as one removed forgets it.</summary>
    [Fact]
    public Task NeighboursFollowTheGroupsChildren() => UiThread.RunAsync(() =>
    {
        var (group, p) = Group(2);
        Expand(p[1]);
        var added = new Expander { Header = "Panel 3", Content = "Body" };
        group.Children.Add(added);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(All, added.CornerRadius);
        Assert.Equal(new Thickness(0, 16, 0, 16), Margin(p[1]));

        group.Children.Remove(p[1]);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(Top, p[0].CornerRadius);
        Assert.Equal(Bottom, added.CornerRadius);
        Assert.Equal(Rule, p[0].BorderThickness);
    });

    /// <summary>MudExpansionPanels' Square: no corner rounded, open or closed.</summary>
    [Fact]
    public Task ASquareGroupHasNoRadius() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3, "expansion-panels square");
        Expand(p[1]);

        Assert.All(p, x => Assert.Equal(None, x.CornerRadius));
    });

    /// <summary>
    /// MudExpansionPanels' Outlined="false": no rule under any panel, closed or open, and the
    /// corners and margins as an outlined group has them.
    /// </summary>
    [Fact]
    public Task ANoOutlineGroupRulesNoPanel() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3, "expansion-panels no-outline");

        Assert.Equal([NoRule, NoRule, NoRule], p.Select(x => x.BorderThickness));
        Assert.Equal([Top, None, Bottom], p.Select(x => x.CornerRadius));

        Expand(p[1]);
        Assert.Equal([NoRule, NoRule, NoRule], p.Select(x => x.BorderThickness));
        Assert.Equal([All, All, All], p.Select(x => x.CornerRadius));
        Assert.Equal(new Thickness(0, 16), Margin(p[1]));
    });

    /// <summary>A generated group takes no-outline as a written one does.</summary>
    [Fact]
    public Task AGeneratedNoOutlineGroupRulesNoPanel() => UiThread.RunAsync(() =>
    {
        var (group, p) = GeneratedGroup(3);
        group.Classes.Add("no-outline");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal([NoRule, NoRule, NoRule], p.Select(x => x.BorderThickness));
    });

    /// <summary>Outside a group an Expander is one whole panel, as it was: rounded, unruled.</summary>
    [Fact]
    public Task APanelOutsideAGroupIsUnchanged() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3, "plain");
        Expand(p[1]);

        Assert.All(p, x => Assert.Equal(All, x.CornerRadius));
        Assert.All(p, x => Assert.Equal(NoRule, x.BorderThickness));
        Assert.All(p, x => Assert.Equal(default, Margin(x)));
    });

    /// <summary>MudExpansionPanels.CollapseAllExceptAsync: the panel just opened is the only one open.</summary>
    [Fact]
    public Task OpeningAPanelClosesTheOthers() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3);
        Expand(p[0]);
        Expand(p[2]);

        Assert.Equal([false, false, true], p.Select(x => x.IsExpanded));
        Assert.Equal([Top, Bottom, All], p.Select(x => x.CornerRadius));
        Assert.Equal(default, Margin(p[0]));
    });

    /// <summary>MudExpansionPanels.AddPanelAsync: a panel that arrives open while another is open closes.</summary>
    [Fact]
    public Task OfPanelsWrittenOpenTheFirstStaysOpen() => UiThread.RunAsync(() =>
    {
        var panels = Enumerable.Range(1, 3).Select(i => new Expander { Header = $"Panel {i}", Content = "Body", IsExpanded = i > 1 }).ToArray();
        var group = new StackPanel { Width = 400, Classes = { "expansion-panels" } };
        group.Children.AddRange(panels);
        Matrix.Show(group);

        Assert.Equal([false, true, false], panels.Select(x => x.IsExpanded));
    });

    /// <summary>Closing the others is a current value: a binding on IsExpanded survives it.</summary>
    [Fact]
    public Task ClosingTheOthersKeepsTheirBindings() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(2);
        var state = new Expander();
        p[0].Bind(Expander.IsExpandedProperty, state.GetObservable(Expander.IsExpandedProperty));
        state.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();
        Expand(p[1]);

        Assert.False(p[0].IsExpanded);
        state.IsExpanded = false;
        state.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(p[0].IsExpanded);
    });

    /// <summary>MultiExpansion: `multi-expansion` on the group lets every panel open on its own.</summary>
    [Fact]
    public Task MultiExpansionLetsPanelsOpenTogether() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3, "expansion-panels multi-expansion");
        Expand(p[0]);
        Expand(p[1]);

        Assert.Equal([true, true, false], p.Select(x => x.IsExpanded));
        Assert.Equal([new Thickness(0, 0, 0, 16), new Thickness(0, 16), default], p.Select(Margin));
    });

    /// <summary>A panel outside a group is never closed by one inside it.</summary>
    [Fact]
    public Task APanelOutsideTheGroupStaysOpen() => UiThread.RunAsync(() =>
    {
        var outside = new Expander { Header = "Outside", Content = "Body", IsExpanded = true };
        var inside = new Expander { Header = "Inside", Content = "Body" };
        var group = new StackPanel { Classes = { "expansion-panels" }, Children = { inside } };
        Matrix.Show(new StackPanel { Width = 400, Children = { outside, group } });
        Expand(inside);

        Assert.True(outside.IsExpanded);
    });

    private static (ItemsControl Group, Expander[] Panels) GeneratedGroup(int count)
    {
        var group = new ItemsControl
        {
            Width = 400,
            Classes = { "expansion-panels" },
            ItemsSource = Enumerable.Range(1, count).Select(i => $"Panel {i}").ToArray(),
            ItemTemplate = new FuncDataTemplate<string>((header, _) => new Expander { Header = header, Content = "Body" }),
        };
        Matrix.Show(group);
        Dispatcher.UIThread.RunJobs();
        return (group, group.GetRealizedContainers().Select(c => (Expander)((Avalonia.Controls.Presenters.ContentPresenter)c).Child!).ToArray());
    }

    /// <summary>An ItemsControl's panels each sit in a presenter; the group sees through it.</summary>
    [Fact]
    public Task AGeneratedGroupIsOneShape() => UiThread.RunAsync(() =>
    {
        var (_, p) = GeneratedGroup(3);

        Assert.Equal([Top, None, Bottom], p.Select(x => x.CornerRadius));
        Assert.Equal([Rule, Rule, NoRule], p.Select(x => x.BorderThickness));

        Expand(p[1]);
        Assert.Equal([All, All, All], p.Select(x => x.CornerRadius));
        Assert.Equal(new Thickness(0, 16), Margin(p[1]));

        Expand(p[0]);
        Assert.Equal([true, false, false], p.Select(x => x.IsExpanded));
    });

    /// <summary>Panels that are an ItemsControl's own items are their own containers, and join too.</summary>
    [Fact]
    public Task ExpandersAsItemsJoinToo() => UiThread.RunAsync(() =>
    {
        var p = Enumerable.Range(1, 3).Select(i => new Expander { Header = $"Panel {i}", Content = "Body" }).ToArray();
        var group = new ItemsControl { Width = 400, Classes = { "expansion-panels" } };
        foreach (var panel in p)
        {
            group.Items.Add(panel);
        }

        Matrix.Show(group);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal([Top, None, Bottom], p.Select(x => x.CornerRadius));
    });

    /// <summary>Items added and removed: the group's ends move with them.</summary>
    [Fact]
    public Task AGeneratedGroupFollowsItsItems() => UiThread.RunAsync(() =>
    {
        var items = new System.Collections.ObjectModel.ObservableCollection<string> { "One", "Two" };
        var group = new ItemsControl
        {
            Width = 400,
            Classes = { "expansion-panels" },
            ItemsSource = items,
            ItemTemplate = new FuncDataTemplate<string>((header, _) => new Expander { Header = header, Content = "Body" }),
        };
        Matrix.Show(group);
        Dispatcher.UIThread.RunJobs();

        items.Add("Three");
        Dispatcher.UIThread.RunJobs();
        Expander PanelAt(int i) => (Expander)((Avalonia.Controls.Presenters.ContentPresenter)group.ContainerFromIndex(i)!).Child!;
        Assert.Equal([Top, None, Bottom], Enumerable.Range(0, 3).Select(i => PanelAt(i).CornerRadius));

        items.RemoveAt(2);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal([Top, Bottom], Enumerable.Range(0, 2).Select(i => PanelAt(i).CornerRadius));
    });
}
