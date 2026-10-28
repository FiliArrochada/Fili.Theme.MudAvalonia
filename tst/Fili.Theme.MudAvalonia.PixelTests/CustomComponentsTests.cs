using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery.Docs;
using Fili.Theme.MudAvalonia.Gallery.Docs.Custom;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// The examples built in the app with C# of their own, the Custom components page's and the text
/// field's validation, do what MudBlazor's do: clicked with the headless mouse, as a reader would.
/// The Avatar page's groups, the half of MudAvatarGroup that needs no C#, are measured beside Max.
/// </summary>
public class CustomComponentsTests
{
    private static Window Show(string source)
    {
        var window = new Window { Content = new ExampleCard { Source = source }, Width = 900, Height = 700 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Click(Window window, Control control)
    {
        var centre = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>MudAlert's default Icon for each Severity (MudAlert.razor.cs), from the theme's keys.</summary>
    [Fact]
    public Task EveryAlertShowsItsSeveritysIcon() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/AlertIcons");
        var expected = new Dictionary<string, string>
        {
            [""] = "FiliEventNoteGeometry",
            ["info"] = "FiliInfoOutlinedGeometry",
            ["success"] = "FiliAlertSuccessGeometry",
            ["warning"] = "FiliReportProblemGeometry",
            ["error"] = "FiliErrorOutlineGeometry",
        };

        var alerts = window.GetVisualDescendants().OfType<Border>().Where(b => b.Classes.Contains("alert")).ToList();
        Assert.Equal(expected.Count, alerts.Count);
        foreach (var alert in alerts)
        {
            var severity = alert.Classes.FirstOrDefault(expected.ContainsKey) ?? "";
            var icon = alert.GetVisualDescendants().OfType<PathIcon>().Single();
            Assert.Same(window.FindResource(expected[severity]), icon.Data);
        }

        window.Close();
    });

    /// <summary>
    /// MudAvatarGroup's Max: the first Max avatars and a "+n" for the rest, hidden when none are,
    /// each avatar over the next (MudAvatarGroup.GetAvatarZindex), the 2px ring on 44px.
    /// </summary>
    [Fact]
    public Task AnAvatarGroupShowsMaxAndCountsTheRest() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/AvatarGroupMax");
        var group = window.GetVisualDescendants().OfType<StackPanel>().Single(p => p.Name == "group");
        var avatars = group.Children.OfType<Border>().Where(b => !AvatarGroup.GetIsMore(b)).ToList();
        var more = group.Children.OfType<Border>().Single(AvatarGroup.GetIsMore);
        Assert.Equal(7, avatars.Count);
        Assert.Equal(4, AvatarGroup.GetMax(group));

        Assert.Equal([true, true, true, true, false, false, false], avatars.Select(a => a.IsVisible));
        Assert.True(more.IsVisible);
        Assert.Equal("+3", ((TextBlock)more.Child!).Text);
        Assert.Equal([7, 6, 5, 4, 3, 2, 1], avatars.Select(a => a.ZIndex));
        Assert.Equal(0, more.ZIndex);
        Assert.Equal(new Size(44, 44), avatars[0].Bounds.Size);
        Assert.Equal(new Thickness(2), avatars[0].BorderThickness);

        // Spacing 3: each avatar 12px into the one before it.
        Assert.Equal(avatars[0].Bounds.Right - 12, avatars[1].Bounds.Left);
        Assert.Equal(avatars[3].Bounds.Right - 12, more.Bounds.Left);

        // The slider is Max: all seven need no "+n".
        window.GetVisualDescendants().OfType<Slider>().Single().Value = 7;
        Dispatcher.UIThread.RunJobs();
        Assert.All(avatars, a => Assert.True(a.IsVisible));
        Assert.False(more.IsVisible);

        window.GetVisualDescendants().OfType<Slider>().Single().Value = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, avatars.Count(a => a.IsVisible));
        Assert.Equal("+6", ((TextBlock)more.Child!).Text);

        // Max 0, the docs slider's minimum: the "+n" alone.
        window.GetVisualDescendants().OfType<Slider>().Single().Value = 0;
        Dispatcher.UIThread.RunJobs();
        Assert.All(avatars, a => Assert.False(a.IsVisible));
        Assert.True(more.IsVisible);
        Assert.Equal("+7", ((TextBlock)more.Child!).Text);
        window.Close();
    });

    /// <summary>
    /// The Avatar page's groups, MudAvatarGroup without Max: Spacing 3 a 12px overlap, the
    /// Outlined ring, surface and error, on a 44px avatar and a large one of 60 (_avatar.scss), and
    /// each avatar drawn over the next.
    /// </summary>
    [Fact]
    public Task AnAvatarGroupOverlapsAsMudBlazors() => UiThread.RunAsync(() =>
    {
        var window = Show("Avatar/Groups");
        var groups = window.GetVisualDescendants().OfType<StackPanel>()
            .Where(p => p.Children.Count > 0 && p.Children.All(c => c.Classes.Contains("avatar")))
            .Select(p => p.Children.OfType<Border>().ToList()).ToList();
        Assert.Equal([4, 3], groups.Select(g => g.Count));

        // Hit testing reads a rendered frame: without one, after another class's tests, it finds nothing.
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        foreach (var ((avatars, size), ring) in groups.Zip([44.0, 60.0]).Zip(["FiliSurfaceBrush", "FiliErrorBrush"]))
        {
            Assert.All(avatars, a => Assert.Equal(new Size(size, size), a.Bounds.Size));
            Assert.All(avatars, a => Assert.Equal(new Thickness(2), a.BorderThickness));
            Assert.All(avatars, a => Assert.Same(window.FindResource(ring), a.BorderBrush));
            Assert.Equal(Enumerable.Range(1, avatars.Count).Reverse(), avatars.Select(a => a.ZIndex));
            for (var i = 1; i < avatars.Count; i++)
            {
                Assert.Equal(avatars[i - 1].Bounds.Right - 12, avatars[i].Bounds.Left);

                // The middle of the overlap is the earlier avatar's: it is drawn on top.
                var overlap = avatars[i - 1].TranslatePoint(new Point(size - 6, size / 2), window)!.Value;
                var hit = window.InputHitTest(overlap) as Visual;
                Assert.Same(avatars[i - 1], hit?.GetSelfAndVisualAncestors().OfType<Border>().First(b => b.Classes.Contains("avatar")));
            }
        }

        // Any avatar may join, its size ringed and its shape kept: the group sets no shape.
        var row = (StackPanel)groups[0][0].Parent!;
        var small = new Border { Classes = { "avatar", "small", "rounded" } };
        var square = new Border { Classes = { "avatar", "large", "square" } };
        row.Children.Add(small);
        row.Children.Add(square);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new Size(28, 28), small.Bounds.Size);
        Assert.Equal(window.FindResource("FiliCornerRadius"), small.CornerRadius);
        Assert.Equal(new Size(60, 60), square.Bounds.Size);
        Assert.Equal(new CornerRadius(0), square.CornerRadius);
        Assert.Equal(new Thickness(2), square.BorderThickness);

        window.Close();
    });

    /// <summary>
    /// The text field page's validation example, whose C# is a view model: the binding's error
    /// paints the field and shows its message, follows the text, and clears.
    /// </summary>
    [Fact]
    public Task AFieldShowsItsBindingsValidationError() => UiThread.RunAsync(() =>
    {
        var window = Show("TextField/Validation");
        var field = window.GetVisualDescendants().OfType<TextBox>().First();
        string[] Shown() => field.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && t.Text is { Length: > 0 })
            .Select(t => t.Text!).ToArray();

        Assert.True(DataValidationErrors.GetHasErrors(field));
        Assert.Contains("Must be a valid email address", Shown());
        Assert.DoesNotContain("Hidden in error", Shown());

        field.Text = "";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("Email is required", Shown());

        field.Text = "someone@example.com";
        Dispatcher.UIThread.RunJobs();
        Assert.False(DataValidationErrors.GetHasErrors(field));
        Assert.Contains("Hidden in error", Shown());
        window.Close();
    });

    /// <summary>MudChip's OnClose: the close button removes its chip, and only its own.</summary>
    [Fact]
    public Task AChipsCloseButtonRemovesIt() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/ClosableChips");
        var list = window.GetVisualDescendants().OfType<ItemsControl>().First().ItemsSource as ClosableList;
        Assert.NotNull(list);
        Assert.Equal(5, list.Count);

        var banana = window.GetVisualDescendants().OfType<ContentControl>()
            .First(c => c.Classes.Contains("chip") && (c.DataContext as ClosableItem)?.Header == "Banana");
        var close = banana.GetVisualDescendants().OfType<Button>().Single();

        // .mud-chip-close-button: 18px, out of the Tab order.
        Assert.Equal(new Size(18, 18), close.Bounds.Size);
        Assert.False(close.Focusable);

        Click(window, close);
        Assert.Equal(["Apple", "Cherry", "Date", "Elderberry"], list.Select(i => i.Header));

        list.Reset();
        Assert.Equal(5, list.Count);
        window.Close();
    });

    /// <summary>MudDynamicTabs' CloseTab and AddTab: a tab's close button removes it, the add button adds one.</summary>
    [Fact]
    public Task TabsCloseAndAdd() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/ClosableTabs");
        var tabs = window.GetVisualDescendants().OfType<TabControl>().Single();
        var list = (ClosableList)tabs.ItemsSource!;
        Assert.Equal(0, tabs.SelectedIndex);

        var first = window.GetVisualDescendants().OfType<TabItem>().First();
        var close = first.GetVisualDescendants().OfType<Button>().Single();
        Assert.False(close.Focusable);
        Click(window, close);

        Assert.Equal(["Tab Two", "Tab Three"], list.Select(i => i.Header));
        Assert.Equal("Tab Two", (tabs.SelectedItem as ClosableItem)?.Header);

        var add = window.GetVisualDescendants().OfType<Button>().First(b => ToolTip.GetTip(b) as string == "Add tab");
        Click(window, add);
        Assert.Equal(["Tab Two", "Tab Three", "New tab 1"], list.Select(i => i.Header));
        window.Close();
    });

    /// <summary>
    /// MudDataGrid's SelectColumn: a row's box is its selection, rows add up, and the header's box
    /// is indeterminate for some, checked for all, and a click selects all, then none.
    /// </summary>
    [Fact]
    public Task TheSelectColumnIsTheGridsSelection() => UiThread.RunAsync(() =>
    {
        var window = Show("Custom/SelectColumn");
        var grid = window.GetVisualDescendants().OfType<DataGrid>().Single();
        var boxes = window.GetVisualDescendants().OfType<CheckBox>().ToList();
        var header = boxes.Single(RowSelect.GetIsAll);
        var rows = boxes.Where(RowSelect.GetIsRow).ToList();
        var total = grid.ItemsSource!.Cast<object>().Count();
        Assert.Equal(total, rows.Count);
        Assert.False(header.IsChecked);

        Click(window, rows[1]);
        Click(window, rows[2]);
        Assert.Equal([rows[1].DataContext, rows[2].DataContext], grid.SelectedItems.Cast<object>());
        Assert.True(rows[1].IsChecked);
        Assert.False(rows[0].IsChecked);
        Assert.Null(header.IsChecked);

        Click(window, rows[1]);
        Assert.Equal([rows[2].DataContext], grid.SelectedItems.Cast<object>());
        Assert.False(rows[1].IsChecked);

        Click(window, header);
        Assert.Equal(total, grid.SelectedItems.Count);
        Assert.True(header.IsChecked);
        Assert.All(rows, r => Assert.True(r.IsChecked));

        Click(window, header);
        Assert.Empty(grid.SelectedItems);
        Assert.False(header.IsChecked);
        Assert.All(rows, r => Assert.False(r.IsChecked));

        // A row selected the grid's own way checks its box.
        grid.SelectedItem = rows[0].DataContext;
        Dispatcher.UIThread.RunJobs();
        Assert.True(rows[0].IsChecked);
        window.Close();
    });
}
