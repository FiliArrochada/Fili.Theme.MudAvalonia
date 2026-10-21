using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudTabs' sliding indicator: when the selection moves, the new tab's indicator starts where the
/// old one was and slides into place. Set on a TabControl or TabStrip by its theme.
/// </summary>
/// <remarks>
/// <para>
/// MudBlazor draws one slider under the tab row and transitions its <c>left</c> (or <c>top</c>,
/// for a vertical bar) by .3s cubic-bezier(.64,.09,.08,1); its width jumps. Here every tab keeps
/// its own <c>PART_Indicator</c>, so the colour, placement and <c>hide-indicator</c> styles still
/// apply to it, and this moves the newly selected one: it is put, untransitioned, at the old
/// indicator's offset, and then let go, so the <c>RenderTransform</c> transition its template
/// declares slides it home. Its width is its own tab's from the start, as MudBlazor's is.
/// </para>
/// <para>
/// The offset comes from the two indicators themselves, so it works for a bar on any side and
/// in either direction, and from wherever an interrupted slide had got to. Nothing slides when
/// either tab has no laid-out container, as on first show.
/// </para>
/// <para>
/// Internal, like the converters: a template needs it, a consumer does not. The tab items opt
/// out of clipping (<c>ClipToBounds="False"</c>), or the indicator would vanish outside its own
/// tab.
/// </para>
/// </remarks>
internal sealed class SlidingIndicator
{
    private const string IndicatorName = "PART_Indicator";

    private SlidingIndicator()
    {
    }

    public static readonly AttachedProperty<bool> SlidesProperty =
        AvaloniaProperty.RegisterAttached<SlidingIndicator, SelectingItemsControl, bool>("Slides");

    public static bool GetSlides(SelectingItemsControl control) => control.GetValue(SlidesProperty);

    public static void SetSlides(SelectingItemsControl control, bool value) => control.SetValue(SlidesProperty, value);

    static SlidingIndicator()
    {
        SlidesProperty.Changed.AddClassHandler<SelectingItemsControl>((control, e) =>
        {
            control.PropertyChanged -= OnPropertyChanged;
            if (e.GetNewValue<bool>())
            {
                control.PropertyChanged += OnPropertyChanged;
            }
        });
    }

    private static void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is SelectingItemsControl control && e.Property == SelectingItemsControl.SelectedIndexProperty)
        {
            Slide(control, e.GetOldValue<int>(), e.GetNewValue<int>());
        }
    }

    private static void Slide(SelectingItemsControl control, int from, int to)
    {
        if (from < 0 || to < 0 || from == to
            || Indicator(control.ContainerFromIndex(from)) is not { } old
            || Indicator(control.ContainerFromIndex(to)) is not { } next
            || old.TranslatePoint(default, next) is not { } offset)
        {
            return;
        }

        var transitions = next.Transitions;
        next.Transitions = null;
        var start = TransformOperations.CreateBuilder(1);
        start.AppendTranslate(offset.X, offset.Y);
        next.RenderTransform = start.Build();
        next.Transitions = transitions;
        next.RenderTransform = TransformOperations.Identity;
    }

    private static Border? Indicator(Control? container) =>
        container is { IsArrangeValid: true, Bounds.Width: > 0 }
            ? container.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == IndicatorName)
            : null;
}
