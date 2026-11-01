using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A page the theme has worked on is collected once the app lets go of it: nothing the theme
/// keeps - a helper's table, a static, an animation, an event it subscribed to - holds a control.
///
/// <para>
/// The page is taken out of a window that stays open, as an app navigating between pages does,
/// rather than the window being closed: in Avalonia 12.1.4's headless session a closed
/// <c>Window</c> is not collected even with no theme loaded at all (measured: alive after three
/// full collections, inside the UI thread and outside it, with a memory dump afterwards showing no
/// root left to release), so a test on a closed window would measure the headless platform, not
/// this theme. A never-shown window is collected only once the dispatch that made it has
/// returned. Controls taken out of a live tree are collected within the same dispatch, which is
/// what this relies on (docs/traps.md, "A closed headless window is not collectable").
/// </para>
/// </summary>
public class LeakTests
{
    [Fact]
    public Task APageTheThemeWorkedOnIsCollected() => UiThread.RunAsync(() =>
    {
        var window = new Window { Width = 800, Height = 600 };
        window.Resources["FiliFocusRing"] = true;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var page = ShowAndTakeOut(window);

        // The pointer last over the page and the last press are the platform's to forget: move
        // the mouse over the empty window, as a user would, and let pending jobs run.
        window.MouseMove(new Point(790, 590));
        Dispatcher.UIThread.RunJobs();
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        var alive = page.Where(entry => entry.Reference.IsAlive).Select(entry => entry.Name).ToArray();
        window.Close();

        Assert.True(alive.Length == 0, "Still reachable after the page was taken out: " + string.Join(", ", alive));
    });

    /// <summary>
    /// Builds a page of the controls the theme's helpers attach to, works each one as a user
    /// would, then takes the page out of the window. Kept out of line so no local of the test
    /// itself holds a control.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (string Name, WeakReference Reference)[] ShowAndTakeOut(Window window)
    {
        // TextTransform names it in its own case; Ripple answers the press.
        var button = new Button { Content = "Save draft", Classes = { "filled", "primary" } };
        // FieldClasses and Adornments mirror its classes and label; the focus ring adorns it.
        var field = new TextBox { PlaceholderText = "Name" };
        // A popup opened and closed.
        var select = new ComboBox { ItemsSource = new[] { "One", "Two" } };
        // OverlayScrim centres its child and fades it in.
        var scrim = new Border { Classes = { "overlay" }, Child = new TextBlock { Text = "Loading" } };
        // TooltipPlacement places it.
        var tipped = new Button { Content = "Info" };
        ToolTip.SetTip(tipped, "More");

        var page = new StackPanel
        {
            Children = { button, field, select, new Panel { Width = 200, Height = 100, Children = { scrim } }, tipped },
        };
        window.Content = page;
        Dispatcher.UIThread.RunJobs();

        var centre = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        // Avalonia's own gesture recogniser holds the pressed control in a timer until its hold
        // delay passes, with or without a theme; headless timers fire on the first RunJobs after
        // they are due. The ripple's fade is still under way when the page is taken out, which
        // is the case this test is for.
        Thread.Sleep(TimeSpan.FromSeconds(1));
        Dispatcher.UIThread.RunJobs();

        field.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();

        select.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        select.IsDropDownOpen = false;
        Dispatcher.UIThread.RunJobs();

        ToolTip.SetIsOpen(tipped, true);
        Dispatcher.UIThread.RunJobs();
        ToolTip.SetIsOpen(tipped, false);
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        Dispatcher.UIThread.RunJobs();

        return
        [
            ("the page", new WeakReference(page)),
            ("the filled button", new WeakReference(button)),
            ("the text field", new WeakReference(field)),
            ("the select", new WeakReference(select)),
            ("the overlay", new WeakReference(scrim)),
            ("the tooltip's button", new WeakReference(tipped)),
        ];
    }
}
