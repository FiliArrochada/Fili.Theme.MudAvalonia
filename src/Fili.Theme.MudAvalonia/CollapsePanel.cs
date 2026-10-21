using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudCollapse: an expansion panel's content revealed by growing its height, not faded in. Holds
/// the content in an Expander's template.
/// </summary>
/// <remarks>
/// <para>
/// _collapse.scss animates <c>grid-template-rows</c> from <c>0fr</c> to <c>1fr</c> over
/// <c>300ms ease-in-out</c>, with the overflow hidden: the content keeps its own size, and the box
/// round it grows from nothing to the content's height, pushing what is below down as it goes.
/// Avalonia cannot animate a height to <c>Auto</c>, so this lays its child out at full size and
/// reports only <see cref="Openness"/> of that size along <see cref="Orientation"/>, clipping the
/// rest. Openness eases between 0 and 1 by a transition, so a panel closed half-way through
/// opening turns back from where it is.
/// </para>
/// <para>
/// The transition is set once the panel is loaded, so a panel that starts open is open on its
/// first frame. Closed, the content is not just clipped to nothing but hidden, so Tab does not
/// find it. <see cref="FromEnd"/> reveals from the far edge, for a panel that opens upwards or to
/// the left.
/// </para>
/// <para>
/// Internal, like the other helpers: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class CollapsePanel : Decorator
{
    /// <summary>`ease-in-out`: CSS's cubic-bezier(.42,0,.58,1).</summary>
    private static readonly Easing EaseInOut = new SplineEasing(0.42, 0, 0.58, 1);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<CollapsePanel, bool>(nameof(IsOpen), true);

    public static readonly StyledProperty<double> OpennessProperty =
        AvaloniaProperty.Register<CollapsePanel, double>(nameof(Openness), 1.0);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CollapsePanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<bool> FromEndProperty =
        AvaloniaProperty.Register<CollapsePanel, bool>(nameof(FromEnd));

    static CollapsePanel()
    {
        AffectsMeasure<CollapsePanel>(OpennessProperty, OrientationProperty);
        AffectsArrange<CollapsePanel>(FromEndProperty);
        ClipToBoundsProperty.OverrideDefaultValue<CollapsePanel>(true);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>How much of the content shows: 0 closed, 1 open, and between while it moves.</summary>
    public double Openness
    {
        get => GetValue(OpennessProperty);
        set => SetValue(OpennessProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool FromEnd
    {
        get => GetValue(FromEndProperty);
        set => SetValue(FromEndProperty, value);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Transitions ??=
        [
            new DoubleTransition { Property = OpennessProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = EaseInOut },
        ];
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty)
        {
            Openness = IsOpen ? 1 : 0;
            ShowChild();
        }
        else if (change.Property == OpennessProperty || change.Property == ChildProperty)
        {
            ShowChild();
        }
    }

    /// <summary>Shown while open or still closing; hidden once closed, so Tab passes it by.</summary>
    private void ShowChild()
    {
        if (Child is { } child)
        {
            child.IsVisible = IsOpen || Openness > 0;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is not { IsVisible: true } child)
        {
            return default;
        }

        var vertical = Orientation == Orientation.Vertical;
        child.Measure(vertical
            ? availableSize.WithHeight(double.PositiveInfinity)
            : availableSize.WithWidth(double.PositiveInfinity));

        var open = Math.Clamp(Openness, 0, 1);
        var size = child.DesiredSize;
        return vertical ? size.WithHeight(size.Height * open) : size.WithWidth(size.Width * open);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is not { } child)
        {
            return finalSize;
        }

        // The content at its own size along the axis, or the room given once fully open, so a
        // stretched panel still stretches its content; the overflow is clipped.
        var vertical = Orientation == Orientation.Vertical;
        var open = Openness >= 1;
        var length = vertical
            ? (open ? Math.Max(finalSize.Height, child.DesiredSize.Height) : child.DesiredSize.Height)
            : (open ? Math.Max(finalSize.Width, child.DesiredSize.Width) : child.DesiredSize.Width);
        var start = FromEnd ? (vertical ? finalSize.Height : finalSize.Width) - length : 0;

        child.Arrange(vertical
            ? new Rect(0, start, finalSize.Width, length)
            : new Rect(start, 0, length, finalSize.Height));
        return finalSize;
    }
}
