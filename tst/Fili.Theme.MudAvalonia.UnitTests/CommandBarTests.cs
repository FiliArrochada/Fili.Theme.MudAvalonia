using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// CommandBar is MudToolBar holding MudIconButtons, with a MoreVert MudMenu for the rest: from
/// _toolbar.scss, _button.scss and _menu.scss.
/// </summary>
public class CommandBarTests
{
    private static PathIcon Icon() => new() { Data = (Geometry)Application.Current!.FindResource("FiliEventGeometry")! };

    private static (CommandBar Bar, CommandBarButton Add) Show(CommandBarDefaultLabelPosition? position = null, params string[] classes)
    {
        var bar = new CommandBar { Width = 600 };
        if (position is { } p)
        {
            bar.DefaultLabelPosition = p;
        }

        bar.Classes.AddRange(classes);
        var add = new CommandBarButton { Label = "Add", Icon = Icon() };
        bar.PrimaryCommands.Add(add);
        bar.SecondaryCommands.Add(new CommandBarButton { Label = "Settings", Icon = Icon() });
        bar.SecondaryCommands.Add(new CommandBarButton { Label = "Help" });
        return (Matrix.Show(bar), add);
    }

    /// <summary>`.mud-toolbar`: 64px, 24px gutters, no ground of its own; dense is 48px.</summary>
    [Fact]
    public Task TheBarIsAToolbar() => UiThread.RunAsync(() =>
    {
        var (bar, _) = Show();
        var (dense, _) = Show(null, "dense");

        Assert.Equal(64, bar.Bounds.Height);
        Assert.Equal(new Thickness(24, 0), bar.Padding);
        Assert.Equal(Colors.Transparent, Matrix.Colour(bar.Background));
        Assert.Equal(48, dense.Bounds.Height);
    });

    /// <summary>
    /// A MudToolBar holds MudIconButtons, so the theme's default is Collapsed: a 48px circle round
    /// a 24px icon in action-default, the label its tooltip.
    /// </summary>
    [Fact]
    public Task ACommandIsAnIconButton() => UiThread.RunAsync(() =>
    {
        var (bar, add) = Show();

        Assert.Equal(CommandBarDefaultLabelPosition.Collapsed, bar.DefaultLabelPosition);
        Assert.Equal(new Size(48, 48), add.Bounds.Size);
        Assert.Equal(new CornerRadius(999), add.CornerRadius);
        Assert.Equal("Add", ToolTip.GetTip(add));
        Assert.Equal(24, Matrix.Part<Viewbox>(add, "PART_IconBox").Bounds.Width);
        Assert.False(Matrix.Part<TextBlock>(add, "PART_Label").IsVisible);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(add.Foreground));
    });

    /// <summary>
    /// LabelPosition Right is a text MudButton with a StartIcon: a 20px icon, the label in button
    /// type, and a button's 36.5px, the label on its 1.75 line (it was 29).
    /// </summary>
    [Fact]
    public Task ALabelOnTheRightIsATextButton() => UiThread.RunAsync(() =>
    {
        var (_, add) = Show(CommandBarDefaultLabelPosition.Right);
        var label = Matrix.Part<TextBlock>(add, "PART_Label");

        Assert.Equal(20, Matrix.Part<Viewbox>(add, "PART_IconBox").Bounds.Width);
        Assert.True(label.IsVisible);
        Assert.Equal(14, label.FontSize);
        Assert.Equal(37, add.Bounds.Height);
        Assert.Equal("ADD", string.Concat(label.Inlines!.OfType<Avalonia.Controls.Documents.Run>().Select(r => r.Text)));
    });

    /// <summary>The overflow button is a MudIconButton with Icons.Material.Filled.MoreVert.</summary>
    [Fact]
    public Task TheOverflowButtonIsMoreVert() => UiThread.RunAsync(() =>
    {
        var (bar, _) = Show();
        var more = Matrix.Part<Button>(bar, "PART_OverflowButton");

        Assert.Contains("icon", more.Classes);
        Assert.Same(Application.Current!.FindResource("FiliMoreVertGeometry"), more.GetVisualDescendants().OfType<Path>().Single().Data);
    });

    /// <summary>
    /// The overflow is MudMenu's popover at elevation 8, each command a 48px MudMenuItem row with
    /// its label body1 in text-primary, as written; a row with no icon has no icon column.
    /// </summary>
    [Fact]
    public Task TheOverflowIsAMenu() => UiThread.RunAsync(() =>
    {
        var (bar, _) = Show();
        bar.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        var popover = (Border)bar.GetVisualDescendants().OfType<Popup>().Single().Child!;
        var rows = popover.GetVisualDescendants().OfType<CommandBarButton>().ToList();
        var settings = rows.Single(r => r.Label == "Settings");
        var help = rows.Single(r => r.Label == "Help");
        var label = Matrix.Part<TextBlock>(settings, "PART_Label");

        Assert.True(Application.Current!.TryGetResource("FiliElevation8", Application.Current.ActualThemeVariant, out var elevation8));
        Assert.Equal(elevation8, popover.BoxShadow);
        Assert.Equal(48, settings.Bounds.Height);
        Assert.Equal(16, label.FontSize);
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(label.Foreground));
        Assert.Equal("Settings", label.Text);
        Assert.True(label.Inlines is null || label.Inlines.Count == 0);
        Assert.False(Matrix.Part<Viewbox>(help, "PART_IconBox").IsVisible);
    });

    /// <summary>On an app bar the commands take the bar's text colour, as Color.Inherit does.</summary>
    [Fact]
    public Task OnAnAppBarTheCommandsInherit() => UiThread.RunAsync(() =>
    {
        var bar = new CommandBar { Width = 600 };
        var add = new CommandBarButton { Label = "Add", Icon = Icon() };
        var pin = new CommandBarToggleButton { Label = "Pin", Icon = Icon(), IsChecked = true };
        bar.PrimaryCommands.Add(add);
        bar.PrimaryCommands.Add(pin);
        Matrix.Show(new Border { Classes = { "appbar" }, Width = 600, Child = bar });
        var text = Matrix.Token("FiliAppbarTextColor");

        Assert.Equal(text, Matrix.Colour(add.Foreground));
        Assert.Equal(text, Matrix.Colour(pin.Foreground));
        Assert.Equal(text, Matrix.Colour(Matrix.Part<Button>(bar, "PART_OverflowButton").Foreground));
    });

    /// <summary>A checked toggle is MudToggleIconButton with ToggledColor primary.</summary>
    [Fact]
    public Task ACheckedToggleIsPrimary() => UiThread.RunAsync(() =>
    {
        var bar = new CommandBar { Width = 600 };
        var pin = new CommandBarToggleButton { Label = "Pin", Icon = Icon(), IsChecked = true };
        bar.PrimaryCommands.Add(pin);
        Matrix.Show(bar);

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(pin.Foreground));
    });
}
