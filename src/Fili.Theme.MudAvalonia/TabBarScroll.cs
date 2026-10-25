using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudTabs' scroll buttons: when the tabs do not fit the bar, a previous and a next button show at
/// its ends and scroll it a bar's length at a time. Set on a TabControl or TabStrip by its theme.
/// </summary>
/// <remarks>
/// <para>
/// MudTabs shows them when the tabs are wider than the bar (<c>_showScrollButtons</c>), disables
/// each at its end, and scrolls the selected tab into view. The template here puts the tab row in
/// a <c>ScrollViewer</c> with no scroll bars (<c>PART_TabScroller</c>) between two icon buttons
/// (<c>PART_ScrollPrev</c>, <c>PART_ScrollNext</c>); this wires them, since what the buttons
/// show and do depends on the scroller's extent and offset, which no binding can compare.
/// </para>
/// <para>
/// Internal, like the converters: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class TabBarScroll
{
    private const double Slack = 0.5;

    private TabBarScroll()
    {
    }

    public static readonly AttachedProperty<bool> ScrollsProperty =
        AvaloniaProperty.RegisterAttached<TabBarScroll, SelectingItemsControl, bool>("Scrolls");

    /// <summary>The bar's own scroller, found when the template is applied.</summary>
    private static readonly AttachedProperty<ScrollViewer?> ScrollerProperty =
        AvaloniaProperty.RegisterAttached<TabBarScroll, SelectingItemsControl, ScrollViewer?>("Scroller");

    public static bool GetScrolls(SelectingItemsControl control) => control.GetValue(ScrollsProperty);

    public static void SetScrolls(SelectingItemsControl control, bool value) => control.SetValue(ScrollsProperty, value);

    static TabBarScroll()
    {
        ScrollsProperty.Changed.AddClassHandler<SelectingItemsControl>((control, e) =>
        {
            control.TemplateApplied -= OnTemplateApplied;
            control.PropertyChanged -= OnPropertyChanged;
            if (e.GetNewValue<bool>())
            {
                control.TemplateApplied += OnTemplateApplied;
                control.PropertyChanged += OnPropertyChanged;
            }
        });
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<ScrollViewer>("PART_TabScroller") is not { } scroller
            || e.NameScope.Find<Button>("PART_ScrollPrev") is not { } prev
            || e.NameScope.Find<Button>("PART_ScrollNext") is not { } next)
        {
            return;
        }

        var control = (SelectingItemsControl)sender!;
        control.SetValue(ScrollerProperty, scroller);
        Dispatcher.UIThread.Post(() => Reveal(control, scroller, control.SelectedIndex), DispatcherPriority.Loaded);
        prev.Click += (_, _) => Page(scroller, -1);
        next.Click += (_, _) => Page(scroller, 1);
        scroller.ScrollChanged += (_, _) => Update(scroller, prev, next);
        scroller.PropertyChanged += (_, args) =>
        {
            if (args.Property == ScrollViewer.ExtentProperty || args.Property == ScrollViewer.ViewportProperty)
            {
                Update(scroller, prev, next);
            }
        };
        Update(scroller, prev, next);
    }

    /// <summary>
    /// The selected tab is scrolled into view in the bar, as MudTabs' ScrollToItem does. The bar's
    /// own scroller only: BringIntoView asked every scroller round the bar as well, so a tab bar
    /// low on a page scrolled the page to itself as soon as a tab was selected.
    /// </summary>
    private static void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is SelectingItemsControl control
            && e.Property == SelectingItemsControl.SelectedIndexProperty
            && control.GetValue(ScrollerProperty) is { } scroller)
        {
            var index = e.GetNewValue<int>();
            Dispatcher.UIThread.Post(() => Reveal(control, scroller, index), DispatcherPriority.Loaded);
        }
    }

    private static void Reveal(SelectingItemsControl control, ScrollViewer scroller, int index)
    {
        if (control.ContainerFromIndex(index) is not { } tab
            || scroller.Content is not Visual row
            || tab.TranslatePoint(default, row) is not { } start)
        {
            return;
        }

        if (Vertical(scroller))
        {
            var y = Math.Clamp(scroller.Offset.Y, Math.Min(start.Y, start.Y + tab.Bounds.Height - scroller.Viewport.Height), start.Y);
            scroller.Offset = new Vector(scroller.Offset.X, Math.Max(0, y));
        }
        else
        {
            var x = Math.Clamp(scroller.Offset.X, Math.Min(start.X, start.X + tab.Bounds.Width - scroller.Viewport.Width), start.X);
            scroller.Offset = new Vector(Math.Max(0, x), scroller.Offset.Y);
        }
    }

    /// <summary>A bar on the left or right scrolls down; its theme disables the other direction.</summary>
    private static bool Vertical(ScrollViewer scroller) =>
        scroller.HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled;

    private static void Update(ScrollViewer scroller, Button prev, Button next)
    {
        var vertical = Vertical(scroller);
        var extent = vertical ? scroller.Extent.Height : scroller.Extent.Width;
        var viewport = vertical ? scroller.Viewport.Height : scroller.Viewport.Width;
        var offset = vertical ? scroller.Offset.Y : scroller.Offset.X;
        var overflows = extent > viewport + Slack;

        prev.IsVisible = overflows;
        next.IsVisible = overflows;
        prev.IsEnabled = overflows && offset > Slack;
        next.IsEnabled = overflows && offset < extent - viewport - Slack;
    }

    private static void Page(ScrollViewer scroller, int direction)
    {
        if (Vertical(scroller))
        {
            var max = Math.Max(0, scroller.Extent.Height - scroller.Viewport.Height);
            var y = Math.Clamp(scroller.Offset.Y + (direction * scroller.Viewport.Height), 0, max);
            scroller.Offset = new Vector(scroller.Offset.X, y);
        }
        else
        {
            var max = Math.Max(0, scroller.Extent.Width - scroller.Viewport.Width);
            var x = Math.Clamp(scroller.Offset.X + (direction * scroller.Viewport.Width), 0, max);
            scroller.Offset = new Vector(x, scroller.Offset.Y);
        }
    }
}
