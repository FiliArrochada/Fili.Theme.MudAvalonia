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

    /// <summary>
    /// The panel's root draws the corners and the shadow, and PART_Rule over it the rule. The root
    /// never takes the rule: a Border with square corners and a one-sided BorderThickness draws no
    /// BoxShadow, and a closed group's middle panels lost theirs - a notch in the group's edge,
    /// and no ring at all in high contrast.
    /// </summary>
    [Fact]
    public Task ThePanelsRootDrawsTheShapeAndItsRuleTheLine() => UiThread.RunAsync(() =>
    {
        var (_, p) = Group(3);
        var root = Matrix.Part<Border>(p[1], "PART_Root");
        var rule = Matrix.Part<Border>(p[1], "PART_Rule");

        Assert.Equal(None, root.CornerRadius);
        Assert.Equal(NoRule, root.BorderThickness);
        Assert.Equal(FabTests.Shadow(1), root.BoxShadow);
        Assert.Equal(Rule, rule.BorderThickness);
        Assert.Equal(Matrix.Token("FiliLinesDefaultColor"), Matrix.Colour(rule.BorderBrush));
        Assert.Equal(Top, Matrix.Part<Border>(p[0], "PART_Rule").CornerRadius);
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
    /// <summary>A panel's item. A virtualising panel rebuilds and reuses its panels, so whether one is open belongs to the item.</summary>
    private sealed class PanelItem(string header)
    {
        public string Header { get; } = header;

        public bool IsExpanded { get; set; }
    }

    /// <summary>
    /// Forty generated panels in a virtualising panel, 480px of them on screen, with the
    /// ScrollViewer that the panel virtualises against.
    /// </summary>
    private static (ItemsControl Group, ScrollViewer Scroller) VirtualisedGroup()
    {
        var group = new ItemsControl
        {
            Classes = { "expansion-panels" },
            ItemsSource = Enumerable.Range(0, 40).Select(i => new PanelItem($"Panel {i}")).ToArray(),
            ItemsPanel = new FuncTemplate<Panel?>(() => new VirtualizingStackPanel()),
            ItemTemplate = new FuncDataTemplate<PanelItem>((_, _) => new Expander
            {
                Content = "Body",
                [!Avalonia.Controls.Primitives.HeaderedContentControl.HeaderProperty] = new Avalonia.Data.Binding(nameof(PanelItem.Header)),
                [!Expander.IsExpandedProperty] = new Avalonia.Data.Binding(nameof(PanelItem.IsExpanded)) { Mode = Avalonia.Data.BindingMode.TwoWay },
            }),
        };
        // A window of its own: a virtualising panel realises only what the window shows.
        var scroller = new ScrollViewer { Width = 400, Height = 480, Content = group };
        new Window { Width = 600, Height = 600, Content = scroller }.Show();
        Dispatcher.UIThread.RunJobs();
        return (group, scroller);
    }

    private static Dictionary<int, Expander> OnScreen(ItemsControl group)
    {
        Dispatcher.UIThread.RunJobs();
        return group.GetRealizedContainers()
            .Where(c => c.IsVisible && group.IndexFromContainer(c) >= 0)
            .ToDictionary(group.IndexFromContainer, c => (Expander)((Avalonia.Controls.Presenters.ContentPresenter)c).Child!);
    }

    /// <summary>
    /// Scrolls and lets the panel settle: it realises against an estimated row height first, and
    /// corrects once the rows it made have measured, so one layout pass can stop short.
    /// </summary>
    private static void ScrollTo(ScrollViewer scroller, double y)
    {
        scroller.Offset = new Vector(0, y);
        for (var pass = 0; pass < 4; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            scroller.UpdateLayout();
        }

        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// A virtualising panel holds only the panels on screen. The first and last of those are
    /// not the group's ends unless their items are: scrolled to the middle, every panel is a
    /// middle one, and scrolled to the end the last item's panel closes the group.
    /// </summary>
    [Fact]
    public Task AVirtualisedGroupsEndsAreItsItemsEnds() => UiThread.RunAsync(() =>
    {
        var (group, scroller) = VirtualisedGroup();
        var top = OnScreen(group);
        Assert.Equal(Top, top[0].CornerRadius);
        Assert.All(top.Where(p => p.Key > 0), p => Assert.Equal((None, Rule), (p.Value.CornerRadius, p.Value.BorderThickness)));

        ScrollTo(scroller, 1000);
        var middle = OnScreen(group);
        Assert.DoesNotContain(0, middle.Keys);
        Assert.All(middle.Values, p => Assert.Equal((None, Rule), (p.CornerRadius, p.BorderThickness)));

        ScrollTo(scroller, double.PositiveInfinity);
        var bottom = OnScreen(group);
        Assert.Equal((Bottom, NoRule), (bottom[39].CornerRadius, bottom[39].BorderThickness));
        Assert.All(bottom.Where(p => p.Key < 39), p => Assert.Equal(None, p.Value.CornerRadius));
    });

    /// <summary>
    /// A container recycled for another item stays attached, so the group hears it from its
    /// ItemsControl: an expanded panel rounds the panels on either side of it however often the
    /// panel rebuilds them, and still does after a scroll away and back.
    /// </summary>
    [Fact]
    public Task AVirtualisedGroupFollowsItsRecycledPanels() => UiThread.RunAsync(() =>
    {
        var (group, scroller) = VirtualisedGroup();
        ScrollTo(scroller, 1000);
        var shown = OnScreen(group);
        var open = shown.Keys.OrderBy(i => i).ElementAt(2);
        Expand(shown[open]);

        shown = OnScreen(group);
        Assert.Equal((Bottom, NoRule), (shown[open - 1].CornerRadius, shown[open - 1].BorderThickness));
        Assert.Equal(All, shown[open].CornerRadius);
        Assert.Equal(Top, shown[open + 1].CornerRadius);
        Assert.Equal(None, shown[open + 2].CornerRadius);

        ScrollTo(scroller, 0);
        Assert.All(OnScreen(group).Values, p => Assert.False(p.IsExpanded));
        ScrollTo(scroller, 1000);
        shown = OnScreen(group);
        Assert.True(shown[open].IsExpanded);
        Assert.Equal((Bottom, NoRule), (shown[open - 1].CornerRadius, shown[open - 1].BorderThickness));
        Assert.Equal(Top, shown[open + 1].CornerRadius);
    });
}
