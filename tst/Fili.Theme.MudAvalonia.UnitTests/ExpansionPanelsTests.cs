using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudExpansionPanels: <c>expansion-panels</c> on the StackPanel that holds the Expanders. The
/// group rounds its first panel's top and its last panel's bottom, rules a line under every
/// panel but the last, and opens an expanded panel 16px away from its neighbours, every corner
/// rounded - with the panel before it losing its rule and taking the bottom corners, and the one
/// after it taking the top ones (_expansionpanel.scss).
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
        var (_, p) = Group(3);
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
}
