using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The children a host centres, which were child selectors ending in <c>:is(Control)</c>: an
/// avatar's content both ways (`.mud-avatar` is a centred flex box), a toolbar's row and its
/// items and a card-actions row's items (`align-items: center`).
/// </summary>
public class ChildAlignmentTests
{
    [Fact]
    public Task AnAvatarCentresItsContentBothWays() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "A" };
        Matrix.Show(new Border { Classes = { "avatar" }, Child = text });

        Assert.Equal(HorizontalAlignment.Center, text.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Center, text.VerticalAlignment);
    });

    [Fact]
    public Task AToolbarCentresItsRowAndTheRowsItems() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save" };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Children = { button } };
        Matrix.Show(new Border { Classes = { "toolbar" }, Child = row });

        Assert.Equal(VerticalAlignment.Center, row.VerticalAlignment);
        Assert.Equal(VerticalAlignment.Center, button.VerticalAlignment);
        Assert.Equal(HorizontalAlignment.Stretch, row.HorizontalAlignment);
    });

    /// <summary>The items as they come and go: one added later is centred, one taken out is its own again.</summary>
    [Fact]
    public Task ACardActionsRowFollowsItsItems() => UiThread.RunAsync(() =>
    {
        var first = new Button { Content = "Share" };
        var row = new StackPanel { Classes = { "card-actions" }, Children = { first } };
        Matrix.Show(row);
        Assert.Equal(VerticalAlignment.Center, first.VerticalAlignment);

        var later = new Button { Content = "Learn more" };
        row.Children.Add(later);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(VerticalAlignment.Center, later.VerticalAlignment);

        row.Children.Remove(first);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(VerticalAlignment.Stretch, first.VerticalAlignment);
    });

    /// <summary>At StyleTrigger, where the selectors set it: an app's own value wins, and a host that loses its class lets go.</summary>
    [Fact]
    public Task AnAppsValueWinsAndTheClassLetsGo() => UiThread.RunAsync(() =>
    {
        var own = new Button { Content = "Top", VerticalAlignment = VerticalAlignment.Top };
        var other = new Button { Content = "Other" };
        var row = new StackPanel { Classes = { "card-actions" }, Children = { own, other } };
        Matrix.Show(row);
        Assert.Equal(VerticalAlignment.Top, own.VerticalAlignment);
        Assert.Equal(VerticalAlignment.Center, other.VerticalAlignment);

        row.Classes.Remove("card-actions");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(VerticalAlignment.Stretch, other.VerticalAlignment);
    });

    /// <summary>A card part goes on the controls the docs put it on; any other control is left alone.</summary>
    [Fact]
    public Task ACardPartIsALayoutContainerATextOrAButton() => UiThread.RunAsync(() =>
    {
        foreach (Control part in new Control[] { new StackPanel(), new DockPanel(), new Grid(), new WrapPanel(), new Border(), new TextBlock(), new Button(), new Image() })
        {
            part.Classes.Add("card-content");
            Matrix.Show(part);
            Assert.True(part.Margin == new Avalonia.Thickness(16), part.GetType().Name);
        }

        var other = new ContentControl { Classes = { "card-content" } };
        Matrix.Show(other);
        Assert.Equal(default, other.Margin);
    });
}
