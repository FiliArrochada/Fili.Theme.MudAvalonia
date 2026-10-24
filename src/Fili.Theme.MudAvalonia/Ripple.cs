using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudBlazor's ripple: a circle that grows from the pointer when a control is pressed and fades
/// once it is released. A template's <c>PART_Ripple</c> border is the layer it draws in.
/// </summary>
/// <remarks>
/// <para>
/// Transcribed from mudRipple.js and _ripple.scss. On a primary-button press inside the layer, a
/// circle twice the layer's longer side is centred on the pointer, then grows from nothing over
/// .5s at cubic-bezier(.4,0,.2,1) while its opacity rises to <see cref="OpacityProperty"/> over
/// .1s. On release, capture loss or the pointer leaving, it fades over .4s - but not before .5s
/// from the press. The layer is clipped to its own corners.
/// </para>
/// <para>
/// The colour is MudBlazor's <c>--mud-ripple-color</c>, which is the control's text colour in
/// every case but the icon-button ones round a checkbox, radio or switch glyph: the layer's
/// inherited <c>Foreground</c>, unless <see cref="BrushProperty"/> names the glyph's colour. The
/// opacity is <c>FiliRippleOpacity</c>, or <c>FiliRippleOpacitySecondary</c> on filled surfaces,
/// set by the control's theme; zero turns the ripple off. Only the innermost layer under the pointer ripples, as
/// <c>closest('.mud-ripple')</c> picks one. Internal, like the converters: a theme needs it, a
/// consumer does not.
/// </para>
/// </remarks>
internal sealed class Ripple
{
    /// <summary>mudRipple.js MIN_RIPPLE_DURATION: no fade before this long after the press.</summary>
    internal static readonly TimeSpan MinimumDuration = TimeSpan.FromMilliseconds(500);

    /// <summary>.mud-ripple-effect-expanding: transform .5s cubic-bezier(.4,0,.2,1).</summary>
    internal static readonly TimeSpan GrowDuration = TimeSpan.FromMilliseconds(500);

    /// <summary>.mud-ripple-effect-expanding: opacity .1s linear.</summary>
    internal static readonly TimeSpan RiseDuration = TimeSpan.FromMilliseconds(100);

    /// <summary>.mud-ripple-effect-fading: opacity .4s linear.</summary>
    internal static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(400);

    private static readonly TransformOperations Collapsed = TransformOperations.Parse("scale(0)");
    private static readonly TransformOperations Grown = TransformOperations.Parse("scale(1)");

    /// <summary>The press the innermost layer already answered, so no layer round it does.</summary>
    private static PointerPressedEventArgs? s_answered;

    private Ripple()
    {
    }

    /// <summary>
    /// <c>--mud-ripple-opacity</c>: set on the control by its theme, and inherited by its layer, so
    /// a filled variant's style can raise it and a region's resources can turn it off.
    /// </summary>
    public static readonly AttachedProperty<double> OpacityProperty =
        AvaloniaProperty.RegisterAttached<Ripple, Control, double>("Opacity", inherits: true);

    /// <summary>The ripple's colour where it is not the text's: a checkbox, radio or switch glyph's.</summary>
    public static readonly AttachedProperty<IBrush?> BrushProperty =
        AvaloniaProperty.RegisterAttached<Ripple, Border, IBrush?>("Brush");

    /// <summary>Marks a template's border as the layer the ripple draws in.</summary>
    public static readonly AttachedProperty<bool> IsLayerProperty =
        AvaloniaProperty.RegisterAttached<Ripple, Border, bool>("IsLayer");

    private static readonly AttachedProperty<Layer?> LayerProperty =
        AvaloniaProperty.RegisterAttached<Ripple, Border, Layer?>("Layer");

    public static double GetOpacity(Control control) => control.GetValue(OpacityProperty);

    public static void SetOpacity(Control control, double value) => control.SetValue(OpacityProperty, value);

    public static IBrush? GetBrush(Border layer) => layer.GetValue(BrushProperty);

    public static void SetBrush(Border layer, IBrush? value) => layer.SetValue(BrushProperty, value);

    public static bool GetIsLayer(Border layer) => layer.GetValue(IsLayerProperty);

    public static void SetIsLayer(Border layer, bool value) => layer.SetValue(IsLayerProperty, value);

    static Ripple()
    {
        IsLayerProperty.Changed.AddClassHandler<Border>((border, e) =>
        {
            if (e.GetNewValue<bool>() && border.GetValue(LayerProperty) is null)
            {
                border.SetValue(LayerProperty, new Layer(border));
            }
        });
    }

    private sealed class Layer
    {
        private readonly Border _border;
        private readonly Canvas _canvas = new();
        private readonly List<(Panel Wave, long Started)> _held = [];
        private InputElement? _host;

        public Layer(Border border)
        {
            _border = border;
            border.IsHitTestVisible = false;
            border.Child = _canvas;
            border.AttachedToVisualTree += (_, _) => Attach();
            border.DetachedFromVisualTree += (_, _) => Detach();
            if (border.IsAttachedToVisualTree())
            {
                Attach();
            }
        }

        private void Attach()
        {
            Detach();
            _host = _border.TemplatedParent as InputElement;
            if (_host is null)
            {
                return;
            }

            _host.AddHandler(InputElement.PointerPressedEvent, OnPressed, RoutingStrategies.Bubble, handledEventsToo: true);
            _host.AddHandler(InputElement.PointerReleasedEvent, OnReleased, RoutingStrategies.Bubble, handledEventsToo: true);
            _host.AddHandler(InputElement.PointerCaptureLostEvent, OnReleased, handledEventsToo: true);
            _host.AddHandler(InputElement.PointerExitedEvent, OnReleased, handledEventsToo: true);
        }

        private void Detach()
        {
            if (_host is not null)
            {
                _host.RemoveHandler(InputElement.PointerPressedEvent, OnPressed);
                _host.RemoveHandler(InputElement.PointerReleasedEvent, OnReleased);
                _host.RemoveHandler(InputElement.PointerCaptureLostEvent, OnReleased);
                _host.RemoveHandler(InputElement.PointerExitedEvent, OnReleased);
                _host = null;
            }

            _held.Clear();
            _canvas.Children.Clear();
        }

        private void OnPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(_border);
            var size = _border.Bounds.Size;
            var opacity = GetOpacity(_border);
            if (ReferenceEquals(e, s_answered) || !point.Properties.IsLeftButtonPressed || opacity <= 0 ||
                Motion.GetReduced(_border) ||
                !_border.IsEffectivelyVisible || !new Rect(size).Contains(point.Position))
            {
                return;
            }

            s_answered = e;
            _border.Clip = RoundedRect(size, _border.CornerRadius);

            // createRipple: Math.max(rect.width, rect.height) * 2, centred on the pointer.
            var diameter = Math.Max(size.Width, size.Height) * 2;
            var dot = new Ellipse
            {
                Fill = GetBrush(_border) ?? _border.GetValue(TextElement.ForegroundProperty),
                Opacity = 0,
                RenderTransform = Collapsed,
                Transitions =
                [
                    new TransformOperationsTransition
                    {
                        Property = Visual.RenderTransformProperty,
                        Duration = GrowDuration,
                        Easing = new SplineEasing(0.4, 0, 0.2, 1),
                    },
                    new DoubleTransition { Property = Visual.OpacityProperty, Duration = RiseDuration },
                ],
            };

            // The wave holds the fade, so the dot's own opacity transition is never swapped mid-rise.
            var wave = new Panel
            {
                Width = diameter,
                Height = diameter,
                Transitions = [new DoubleTransition { Property = Visual.OpacityProperty, Duration = FadeDuration }],
                Children = { dot },
            };
            Canvas.SetLeft(wave, point.Position.X - diameter / 2);
            Canvas.SetTop(wave, point.Position.Y - diameter / 2);
            _canvas.Children.Add(wave);

            dot.Opacity = opacity;
            dot.RenderTransform = Grown;
            _held.Add((wave, Stopwatch.GetTimestamp()));
        }

        private void OnReleased(object? sender, RoutedEventArgs e)
        {
            foreach (var (wave, started) in _held)
            {
                var remaining = MinimumDuration - Stopwatch.GetElapsedTime(started);
                if (remaining > TimeSpan.Zero)
                {
                    DispatcherTimer.RunOnce(() => Fade(wave), remaining);
                }
                else
                {
                    Fade(wave);
                }
            }

            _held.Clear();
        }

        private void Fade(Panel wave)
        {
            wave.Opacity = 0;
            DispatcherTimer.RunOnce(() => _canvas.Children.Remove(wave), FadeDuration);
        }
    }

    /// <summary>The layer's own shape: Avalonia's compositor clips a visual to a rectangle only.</summary>
    internal static Geometry RoundedRect(Size size, CornerRadius radius)
    {
        var (w, h) = (size.Width, size.Height);
        var limit = Math.Min(w, h) / 2;
        double Clamp(double r) => Math.Max(0, Math.Min(r, limit));
        var (tl, tr, br, bl) = (Clamp(radius.TopLeft), Clamp(radius.TopRight), Clamp(radius.BottomRight), Clamp(radius.BottomLeft));

        var geometry = new StreamGeometry();
        using var context = geometry.Open();

        void Corner(Point to, double r)
        {
            if (r > 0)
            {
                context.ArcTo(to, new Size(r, r), 0, false, SweepDirection.Clockwise);
            }
            else
            {
                context.LineTo(to);
            }
        }

        context.BeginFigure(new Point(tl, 0), true);
        context.LineTo(new Point(w - tr, 0));
        Corner(new Point(w, tr), tr);
        context.LineTo(new Point(w, h - br));
        Corner(new Point(w - br, h), br);
        context.LineTo(new Point(bl, h));
        Corner(new Point(0, h - bl), bl);
        context.LineTo(new Point(0, tl));
        Corner(new Point(tl, 0), tl);
        context.EndFigure(true);
        return geometry;
    }
}
