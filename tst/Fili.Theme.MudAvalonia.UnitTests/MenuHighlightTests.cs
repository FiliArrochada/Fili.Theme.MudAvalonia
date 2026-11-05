using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A menu row is tinted on hover and on keyboard focus, as MudBlazor's are, never just for being
/// Avalonia's selection. A menu flyout opened by the pointer focuses and selects its first row, and
/// the theme had painted <c>:selected</c> with the hover tint: a language menu opened by a click
/// showed "English" highlighted beside the ticked "Português", which reads as the wrong choice
/// (found by an adopting app).
/// </summary>
public class MenuHighlightTests
{
    private static (Window Window, Button Button, MenuItem First, MenuItem Second) Menu()
    {
        var first = new MenuItem { Header = "English" };
        var second = new MenuItem { Header = "Português", Icon = new CheckBox { IsChecked = true } };
        var button = new Button { Content = "Language", Flyout = new MenuFlyout { Items = { first, second } } };
        var other = new Button { Content = "Other" };
        var window = new Window { Width = 400, Height = 300, Content = new StackPanel { Children = { other, button } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, button, first, second);
    }

    private static bool Tinted(MenuItem item)
    {
        var layer = Matrix.Part<Border>(item, "PART_StateLayer");
        return layer.Background is ISolidColorBrush { Color.A: > 0 };
    }

    private static void Click(Window window, Control target)
    {
        var point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Key(Control focused, PhysicalKey key)
    {
        // A real key on the top level the focused control is in: the window, or the popup's root.
        var root = TopLevel.GetTopLevel(focused)!;
        root.KeyPressQwerty(key, RawInputModifiers.None);
        root.KeyReleaseQwerty(key, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// A menu opened by a click shows no row highlighted until the pointer reaches one, or until
    /// an arrow key, which then shows where the keyboard is.
    /// </summary>
    [Fact]
    public Task APointerOpenedMenuHighlightsNoRow() => UiThread.RunAsync(() =>
    {
        var (window, button, first, second) = Menu();
        try
        {
            Click(window, button);
            Assert.True(first.IsAttachedToVisualTree());

            Assert.False(Tinted(first), $"The first row is tinted (selected {first.IsSelected}, focused {first.IsFocused}).");
            Assert.False(Tinted(second));

            Key(first, PhysicalKey.ArrowDown);
            Assert.True(second.IsFocused);
            Assert.True(Tinted(second), "The row the arrow moved to is not tinted.");
            Assert.False(Tinted(first));
        }
        finally
        {
            button.Flyout!.Hide();
            Dispatcher.UIThread.RunJobs();
            window.MouseMove(new Point(399, 299), RawInputModifiers.None);
            window.Close();
        }
    });

    /// <summary>A menu shown from code highlights no row either.</summary>
    [Fact]
    public Task AMenuShownFromCodeHighlightsNoRow() => UiThread.RunAsync(() =>
    {
        var (window, button, first, second) = Menu();
        try
        {
            button.Flyout!.ShowAt(button);
            Dispatcher.UIThread.RunJobs();

            Assert.False(Tinted(first), $"The first row is tinted (selected {first.IsSelected}, focused {first.IsFocused}).");
            Assert.False(Tinted(second));
        }
        finally
        {
            button.Flyout!.Hide();
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    });

    /// <summary>A context menu opened by a right click highlights no row.</summary>
    [Fact]
    public Task ARightClickedContextMenuHighlightsNoRow() => UiThread.RunAsync(() =>
    {
        var first = new MenuItem { Header = "Copy" };
        var second = new MenuItem { Header = "Paste" };
        var target = new Border { Width = 100, Height = 40, Background = Brushes.Transparent, ContextMenu = new ContextMenu { Items = { first, second } } };
        var window = new Window { Width = 400, Height = 300, Content = new StackPanel { Children = { target } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var point = target.TranslatePoint(new Point(50, 20), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();
            Assert.True(first.IsAttachedToVisualTree(), "The right click did not open the menu.");

            Assert.False(Tinted(first), $"The first row is tinted (selected {first.IsSelected}, focused {first.IsFocused}).");
            Assert.False(Tinted(second));
        }
        finally
        {
            target.ContextMenu!.Close();
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    });

    /// <summary>
    /// A menu opened from the keyboard shows where the keyboard is: the focused row is tinted, and
    /// the tint follows the arrow keys.
    /// </summary>
    [Fact]
    public Task AKeyboardOpenedMenuHighlightsTheFocusedRow() => UiThread.RunAsync(() =>
    {
        var (window, button, first, second) = Menu();
        try
        {
            button.Focus(NavigationMethod.Tab);
            Dispatcher.UIThread.RunJobs();
            Key(button, PhysicalKey.Enter);
            Assert.True(first.IsAttachedToVisualTree(), "Enter did not open the menu.");

            Assert.True(first.IsFocused);
            Assert.True(Tinted(first), "The first row, where the keyboard is, is not tinted.");
            Assert.False(Tinted(second));

            Key(first, PhysicalKey.ArrowDown);
            Assert.True(second.IsFocused);
            Assert.True(Tinted(second), "The row the arrow moved to is not tinted.");
            Assert.False(Tinted(first));
        }
        finally
        {
            button.Flyout!.Hide();
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    });
}
