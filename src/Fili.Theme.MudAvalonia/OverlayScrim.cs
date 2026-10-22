using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// What <c>overlay</c> on a Border does beyond its colours, as _overlay.scss's
/// <c>.mud-overlay</c>: its content centred (<c>align-items</c> and <c>justify-content</c>
/// center), its corners those of the container it covers (<c>border-radius: inherit</c>), and a
/// fade in over .15s each time it is shown (the scrim's <c>mud-animation-fadein ease 0.15s</c>).
/// Set by the class's style.
/// </summary>
/// <remarks>
/// <para>
/// None of the three is a style. Centring the child would be a selector ending in
/// <c>:is(Control)</c>, matched against every control in the window; a fade on
/// <c>Border.overlay[IsVisible=True]</c> would watch every Border's visibility; and no selector
/// can read a value off an ancestor. So the class sets one attached property, and this does the
/// rest on the few Borders that carry it.
/// </para>
/// <para>
/// The child's alignment is set at <see cref="BindingPriority.StyleTrigger"/> and the corners at
/// <see cref="BindingPriority.Style"/>, so an app's own value on either still wins. The corners
/// come from the nearest Border, ContentPresenter or templated control above: a Panel, which has
/// none, is passed over, so an overlay in a Panel inside a card takes the card's. The whole
/// overlay fades, content included, where MudBlazor fades the scrim alone: a Border has no
/// separate scrim to fade.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The classes are the
/// interface.
/// </para>
/// </remarks>
internal sealed class OverlayScrim
{
    private OverlayScrim()
    {
    }

    public static readonly AttachedProperty<bool> IsOnProperty =
        AvaloniaProperty.RegisterAttached<OverlayScrim, Border, bool>("IsOn");

    /// <summary>The binding that carries the container's corners, while attached.</summary>
    private static readonly AttachedProperty<IDisposable?> CornersProperty =
        AvaloniaProperty.RegisterAttached<OverlayScrim, Border, IDisposable?>("Corners");

    /// <summary>`mud-animation-fadein ease 0.15s`: opacity 0 to 1, at CSS's default ease.</summary>
    internal static readonly Animation Fade = new()
    {
        Duration = TimeSpan.FromMilliseconds(150),
        Easing = new SplineEasing(0.25, 0.1, 0.25, 1),
        Children =
        {
            new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
            new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.OpacityProperty, 1d) } },
        },
    };

    static OverlayScrim()
    {
        IsOnProperty.Changed.AddClassHandler<Border>((border, e) =>
        {
            if (e.GetNewValue<bool>())
            {
                border.AttachedToVisualTree += OnAttached;
                border.DetachedFromVisualTree += OnDetached;
                Centre(border.Child);
                if (border.IsAttachedToVisualTree())
                {
                    Show(border);
                }
            }
            else
            {
                border.AttachedToVisualTree -= OnAttached;
                border.DetachedFromVisualTree -= OnDetached;
                Release(border.Child);
                ReleaseCorners(border);
            }
        });
        Border.ChildProperty.Changed.AddClassHandler<Border>((border, e) =>
        {
            if (GetIsOn(border))
            {
                Release(e.GetOldValue<Control?>());
                Centre(e.GetNewValue<Control?>());
            }
        });
        Visual.IsVisibleProperty.Changed.AddClassHandler<Border>((border, e) =>
        {
            if (GetIsOn(border) && e.GetNewValue<bool>() && border.IsAttachedToVisualTree())
            {
                _ = Fade.RunAsync(border);
            }
        });
    }

    public static bool GetIsOn(Border border) => border.GetValue(IsOnProperty);

    public static void SetIsOn(Border border, bool value) => border.SetValue(IsOnProperty, value);

    private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) => Show((Border)sender!);

    private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) => ReleaseCorners((Border)sender!);

    private static void Show(Border border)
    {
        TakeCorners(border);
        if (border.IsVisible)
        {
            _ = Fade.RunAsync(border);
        }
    }

    private static void Centre(Control? child)
    {
        child?.SetValue(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Center, BindingPriority.StyleTrigger);
        child?.SetValue(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center, BindingPriority.StyleTrigger);
    }

    private static void Release(Control? child)
    {
        child?.SetValue(Layoutable.HorizontalAlignmentProperty, AvaloniaProperty.UnsetValue, BindingPriority.StyleTrigger);
        child?.SetValue(Layoutable.VerticalAlignmentProperty, AvaloniaProperty.UnsetValue, BindingPriority.StyleTrigger);
    }

    private static void TakeCorners(Border border)
    {
        ReleaseCorners(border);
        IObservable<CornerRadius>? corners = border.GetVisualAncestors().FirstOrDefault(a => a is Border or ContentPresenter or TemplatedControl) switch
        {
            Border b => b.GetObservable(Border.CornerRadiusProperty),
            ContentPresenter p => p.GetObservable(ContentPresenter.CornerRadiusProperty),
            TemplatedControl t => t.GetObservable(TemplatedControl.CornerRadiusProperty),
            _ => null,
        };
        if (corners is not null)
        {
            border.SetValue(CornersProperty, border.Bind(Border.CornerRadiusProperty, corners, BindingPriority.Style));
        }
    }

    private static void ReleaseCorners(Border border)
    {
        border.GetValue(CornersProperty)?.Dispose();
        border.ClearValue(CornersProperty);
    }
}
