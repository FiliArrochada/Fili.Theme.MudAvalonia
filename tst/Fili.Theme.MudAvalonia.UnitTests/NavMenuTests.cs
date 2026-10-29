using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Path = Avalonia.Controls.Shapes.Path;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudNavMenu as `nav` on a TreeView, from _navmenu.scss: a link is a row without children and
/// the active link the selected row; a group is a row with children that opens on a click.
/// </summary>
public class NavMenuTests
{
    private sealed record Menu(TreeView Tree, TreeViewItem Home, TreeViewItem Group, TreeViewItem Child, TreeViewItem Nested, TreeViewItem Deep);

    private static Menu Show(params string[] classes)
    {
        var deep = new TreeViewItem { Header = "Deep" };
        var nested = new TreeViewItem { Header = "Nested", Items = { deep } };
        var child = new TreeViewItem { Header = "Child" };
        var group = new TreeViewItem { Header = "Group", IsExpanded = true, Items = { child, nested } };
        var home = new TreeViewItem { Header = "Home" };
        var tree = new TreeView { Width = 240, Height = 400, Items = { home, group } };
        tree.Classes.Add("nav");
        tree.Classes.AddRange(classes);
        Matrix.Show(tree);
        return new Menu(tree, home, group, child, nested, deep);
    }

    private static Border Ground(TreeViewItem item) => Matrix.Part<Border>(item, "SelectionBorder");

    private static IBrush? Rest(Border border) => border.GetBaseValue(Border.BackgroundProperty).GetValueOrDefault();

    private static ContentPresenter Text(TreeViewItem item) => Matrix.Part<ContentPresenter>(item, "PART_HeaderPresenter");

    private static double Indent(TreeViewItem item) => Matrix.Part<Border>(item, "PART_Indent").Width;

    private static void Select(TreeView tree, TreeViewItem item)
    {
        tree.SelectedItem = item;
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// MudDrawer scrolls up and down, never sideways: a label too long for the drawer wraps, as
    /// `.mud-nav-link-text` does. It ran past the edge, and the open drawer scrolled sideways.
    /// </summary>
    [Fact]
    public Task ALongLabelWrapsAndTheMenuNeverScrollsSideways() => UiThread.RunAsync(() =>
    {
        var link = new TreeViewItem { Header = "A label far too long to fit in the drawer's width" };
        var group = new TreeViewItem { Header = "Group", IsExpanded = true, Items = { link } };
        var short_ = new TreeViewItem { Header = "Short" };
        var tree = new TreeView { Width = 240, Height = 400, Items = { short_, group } };
        tree.Classes.Add("nav");
        Matrix.Show(tree);
        var scroller = tree.GetVisualDescendants().OfType<ScrollViewer>().First();

        Assert.Equal(ScrollBarVisibility.Disabled, scroller.HorizontalScrollBarVisibility);
        Assert.True(scroller.Extent.Width <= scroller.Viewport.Width, $"The menu scrolls: {scroller.Extent.Width} in {scroller.Viewport.Width}.");
        Assert.True(Text(link).Bounds.Height > Text(short_).Bounds.Height * 1.5, "The long label stayed on one line.");
    });

    /// <summary>
    /// `.mud-nav-link` is `color: inherit`: a link is the drawer's text colour - drawer-text, or a
    /// coloured drawer's contrast text - and text-primary outside a drawer. It was text-primary
    /// everywhere, so on a primary drawer the links were dark beside the pane's white text.
    /// </summary>
    [Theory]
    [InlineData(null, "FiliDrawerTextColor")]
    [InlineData("primary", "FiliPrimaryContrastTextColor")]
    public Task ALinkIsTheDrawersTextColour(string? colour, string token) => UiThread.RunAsync(() =>
    {
        var link = new TreeViewItem { Header = "Downloads" };
        var tree = new TreeView { Items = { new TreeViewItem { Header = "Library", IsSelected = true }, link } };
        tree.Classes.Add("nav");
        var drawer = new SplitView { DisplayMode = SplitViewDisplayMode.Inline, IsPaneOpen = true, Width = 420, Height = 260, Pane = tree };
        if (colour is not null) drawer.Classes.Add(colour);
        Matrix.Show(drawer);

        Assert.Equal(Matrix.Token(token), Matrix.Colour(Text(link).Foreground));
    });

    [Fact]
    public Task ALinkOutsideADrawerIsTextPrimary() => UiThread.RunAsync(() =>
    {
        var menu = Show();

        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(Text(menu.Child).Foreground));
    });

    /// <summary>
    /// Every row is 40.5px, `.mud-nav-link`'s 8px and its 24.5px line (1.75 of the inherited
    /// 14px), whatever its header holds. A control written in the Header - the icon and text the
    /// mini drawer needs - was shorter than a string header's row: the presenter's line-height
    /// style never reached it.
    /// </summary>
    [Fact]
    public Task ARowIsTheSameHeightWhateverItsHeaderHolds() => UiThread.RunAsync(() =>
    {
        var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") }, new TextBlock { Text = "Inbox" } } };
        var control = new TreeViewItem { Header = panel };
        var text = new TreeViewItem { Header = "Library" };
        var tree = new TreeView { Width = 240, Items = { control, text } };
        tree.Classes.Add("nav");
        Matrix.Show(tree);

        Assert.Equal(Math.Ceiling(8 + 24.5 + 8), control.Bounds.Height);
        Assert.Equal(Math.Ceiling(8 + 24.5 + 8), text.Bounds.Height);
    });

    /// <summary>
    /// A label in a DockPanel or Grid written straight into the Header wraps too. The template's
    /// presenter only reaches text it makes or templates; a panel in the Header is the row's own
    /// child, so its label ran on at one line, cut off at the drawer's edge. Two lines of 24.5px: 65.
    /// </summary>
    [Theory]
    [InlineData("DockPanel")]
    [InlineData("Grid")]
    public Task ALongLabelInAPanelHeaderWraps(string panel) => UiThread.RunAsync(() =>
    {
        TreeViewItem Row(string text)
        {
            var label = new TextBlock { Text = text };
            Panel host = panel == "Grid"
                ? new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Children = { new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") }, label } }
                : new DockPanel { Children = { new PathIcon { Classes = { "start-icon" }, Data = Geometry.Parse("M0 0h2v2H0z") }, label } };
            if (host is Grid) Grid.SetColumn(label, 1);
            else DockPanel.SetDock(host.Children[0], Dock.Left);
            return new TreeViewItem { Header = host };
        }

        var shortRow = Row("Inbox");
        var longRow = Row("A label far too long to fit in the drawer's width");
        var tree = new TreeView { Width = 240, Items = { shortRow, longRow } };
        tree.Classes.Add("nav");
        Matrix.Show(tree);

        Assert.Equal(Math.Ceiling(8 + 24.5 + 8), shortRow.Bounds.Height);
        Assert.Equal(8 + 24.5 + 24.5 + 8, longRow.Bounds.Height);
    });

    /// <summary>
    /// The 24.5px line is `.mud-nav-link`'s, which CSS inherits: a TextBlock with a type class
    /// keeps its class's own line, as `.mud-typography-caption` does. A caption under a title was
    /// given the link's line, in a panel header and in a templated one alike. A plain label keeps
    /// the link's 24.5.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ATypeClassKeepsItsOwnLine(bool templated) => UiThread.RunAsync(() =>
    {
        var title = new TextBlock { Text = "Title" };
        var caption = new TextBlock { Text = "Subtitle", Classes = { "caption" } };
        Grid.SetRow(caption, 1);
        var header = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto"), Children = { title, caption } };
        var row = templated
            ? new TreeViewItem { Header = "x", HeaderTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object>((_, _) => header) }
            : new TreeViewItem { Header = header };
        var tree = new TreeView { Width = 240, Items = { row } };
        tree.Classes.Add("nav");
        Matrix.Show(tree);

        Assert.True(Application.Current!.TryGetResource("FiliCaptionLineHeight", Application.Current.ActualThemeVariant, out var line));
        Assert.Equal((double)line!, caption.LineHeight);
        Assert.Equal(24.5, title.LineHeight);
    });

    /// <summary>
    /// A group whose label wraps keeps its arrow on the first line, its top on the line's, as
    /// `.mud-nav-link`'s `align-items: flex-start` does for the svg; it was centred on the whole
    /// label.
    /// </summary>
    [Fact]
    public Task AWrappedGroupsArrowStaysOnTheFirstLine() => UiThread.RunAsync(() =>
    {
        var group = new TreeViewItem { Header = "A group label far too long to fit in the drawer", IsExpanded = false, Items = { new TreeViewItem { Header = "Child" } } };
        var tree = new TreeView { Width = 240, Items = { group } };
        tree.Classes.Add("nav");
        Matrix.Show(tree);
        var arrow = Matrix.Part<Path>(group, "PART_ExpandIcon");

        Assert.True(group.Bounds.Height > 60, "The label did not wrap.");
        Assert.Equal(8, arrow.TranslatePoint(default, group)!.Value.Y);
    });

    /// <summary>
    /// `.mud-nav-link` sets no font-size, so a link is the body's Default typography, 14px
    /// (_base.scss, Typography.cs), not body1's 16.
    /// </summary>
    [Fact]
    public Task ALinkIsTheDefault14pxAt8By16WithTheText12pxIn() => UiThread.RunAsync(() =>
    {
        var menu = Show();

        Assert.Equal(new Thickness(16, 8), menu.Home.Padding);
        Assert.Equal(14, menu.Home.FontSize);
        Assert.Equal(new Thickness(12, 0, 0, 0), Text(menu.Home).Margin);
        Assert.Equal(FontWeight.Normal, Text(menu.Home).FontWeight);
        Assert.False(Matrix.Part<Path>(menu.Home, "PART_ExpandIcon").IsVisible);
    });

    /// <summary>`.mud-navmenu-default .mud-nav-link.active`: primary at 500 over action-default-hover.</summary>
    [Fact]
    public Task TheActiveLinkIsPrimaryAt500OverTheHoverTint() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        Select(menu.Tree, menu.Home);

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(Text(menu.Home).Foreground));
        Assert.Equal(FontWeight.Medium, Text(menu.Home).FontWeight);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Rest(Ground(menu.Home))));
    });

    [Theory]
    [InlineData("Secondary")]
    [InlineData("Error")]
    public Task AColourIsTheActiveLinksTextOverItsTint(string colour) => UiThread.RunAsync(() =>
    {
        var menu = Show(colour.ToLowerInvariant());
        Select(menu.Tree, menu.Home);

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Text(menu.Home).Foreground));
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(Rest(Ground(menu.Home))));
    });

    [Fact]
    public Task ALinkUnderThePointerIsTinted() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        var tint = Matrix.Part<Border>(menu.Home, "PART_Tint");

        Matrix.Hover(tint);

        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(tint.Background));
    });

    /// <summary>The active link's colour at 12% under the pointer: 6.4% over its 6% tint.</summary>
    [Fact]
    public Task AColouredActiveLinkDeepensUnderThePointer() => UiThread.RunAsync(() =>
    {
        var menu = Show("secondary");
        Select(menu.Tree, menu.Home);
        var tint = Matrix.Part<Border>(menu.Home, "PART_Tint");

        Matrix.Hover(tint);

        Assert.Equal(Matrix.Token("FiliSecondaryColor"), Matrix.Colour(tint.Background));
        Assert.Equal(0.064, tint.Opacity, 3);
    });

    /// <summary>Links in a group start at 36px, then 48: the 16px padding and 20, then 32.</summary>
    [Fact]
    public Task NestedRowsStepIn() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        menu.Nested.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, Indent(menu.Home));
        Assert.Equal(20, Indent(menu.Child));
        Assert.Equal(20, Indent(menu.Nested));
        Assert.Equal(32, Indent(menu.Deep));
    });

    /// <summary>`.mud-nav-group > .mud-nav-link`: 500; a nested group's title is 400 until it opens.</summary>
    [Fact]
    public Task GroupTitlesAre500AndANestedOneOnceOpen() => UiThread.RunAsync(() =>
    {
        var menu = Show();

        Assert.Equal(FontWeight.Medium, Text(menu.Group).FontWeight);
        Assert.Equal(FontWeight.Normal, Text(menu.Nested).FontWeight);

        menu.Nested.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(FontWeight.Medium, Text(menu.Nested).FontWeight);
    });

    /// <summary>A click anywhere on a group's row opens or closes it, and does not make it the active link.</summary>
    [Fact]
    public Task AGroupOpensOnAClickOnItsRow() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        var row = Ground(menu.Nested);
        var window = (Window)TopLevel.GetTopLevel(row)!;
        var point = row.TranslatePoint(new Point(row.Bounds.Width / 3, row.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(menu.Nested.IsExpanded);
        Assert.Null(menu.Tree.SelectedItem);
    });

    /// <summary>
    /// MudNavGroup.ExpandIcon's default, Filled.ArrowDropDown, in drawer-icon; `.mud-transform`
    /// turns it -180 degrees and makes it primary once open.
    /// </summary>
    [Fact]
    public Task AnOpenGroupsArrowTurnsAndTakesTheColour() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        var closed = Matrix.Part<Path>(menu.Nested, "PART_ExpandIcon");
        var open = Matrix.Part<Path>(menu.Group, "PART_ExpandIcon");

        Assert.Equal(Matrix.Token("FiliDrawerIconColor"), Matrix.Colour(closed.Fill));
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(open.Fill));
        Assert.Same(open.FindResource("FiliArrowDropDownGeometry"), closed.Data);
        Assert.Equal(TransformOperations.Parse("rotate(-180deg)").Value, Assert.IsType<TransformOperations>(open.GetBaseValue(Visual.RenderTransformProperty).GetValueOrDefault()).Value);
    });

    /// <summary>MudNavGroup.HideExpandIcon: `hide-icon` on the group's row.</summary>
    [Fact]
    public Task HideIconTakesAGroupsArrowAway() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        menu.Group.Classes.Add("hide-icon");
        Dispatcher.UIThread.RunJobs();

        Assert.False(Matrix.Part<Path>(menu.Group, "PART_ExpandIcon").IsVisible);
        Assert.True(Matrix.Part<Path>(menu.Nested, "PART_ExpandIcon").IsVisible);
    });

    [Fact]
    public Task DenseRoundedAndBordered() => UiThread.RunAsync(() =>
    {
        var menu = Show("dense", "rounded", "bordered", "success");
        Select(menu.Tree, menu.Home);

        Assert.Equal(new Thickness(16, 4), menu.Home.Padding);
        Assert.Equal(new CornerRadius(4), Ground(menu.Home).CornerRadius);
        Assert.Equal(new Thickness(0, 0, 2, 0), Ground(menu.Home).BorderThickness);
        Assert.Equal(Matrix.Token("FiliSuccessColor"), Matrix.Colour(Ground(menu.Home).BorderBrush));
        Assert.Equal(new Thickness(0), Ground(menu.Child).BorderThickness);
    });

    /// <summary>`.mud-nav-link-icon`: drawer-icon, 24px, 12px before the text, the colour when active.</summary>
    [Fact]
    public Task ALinkIconIsDrawerIconAndTheColourWhenActive() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" } };
        var link = new TreeViewItem { Header = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { icon, new TextBlock { Text = "Inbox" } } } };
        var tree = new TreeView { Width = 240, Classes = { "nav", "info" }, Items = { link } };
        Matrix.Show(tree);

        Assert.Equal(24, icon.Width);
        Assert.Equal(new Thickness(-12, 0, 12, 0), icon.Margin);
        Assert.Equal(Matrix.Token("FiliDrawerIconColor"), Matrix.Colour(icon.Foreground));

        Select(tree, link);
        Assert.Equal(Matrix.Token("FiliInfoColor"), Matrix.Colour(icon.Foreground));
    });

    [Fact]
    public Task ADisabledLinkIsTextDisabled() => UiThread.RunAsync(() =>
    {
        var menu = Show();
        menu.Home.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Text(menu.Home).Foreground));
    });
}
