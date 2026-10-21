using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// What an icon marked <c>start-icon</c> or <c>end-icon</c> takes from the control it sits in: a
/// button's, a chip's or a FAB's icon size and gaps, a nav link's size, gap and colour. Set by the
/// host - a button's theme, a nav row's style - and inherited by everything in its content; when
/// a value reaches a control with one of the two classes, this sets it on the icon.
/// </summary>
/// <remarks>
/// <para>
/// These were descendant selectors, <c>Button.small :is(Control).start-icon</c> and twenty-two
/// more, one per host and variant. A selector ending in <c>:is(Control)</c> is matched against
/// every control in the window, every template part included, and a descendant one walks each
/// one's ancestors looking for the host. A style on the icon reading these values is no cheaper:
/// any style ending in <c>:is(Control)</c> watches every control, and one under a property
/// selector cost more than the descendant styles together. So no style reads them: an inherited
/// value arrives at every control in the host anyway, and the few with the class take it here.
/// </para>
/// <para>
/// The values are set at <see cref="BindingPriority.StyleTrigger"/>, where the selectors set them,
/// so an app's own value on the icon still wins; an empty one clears what this set, so an icon
/// that leaves its host keeps its own. The nearest host wins: a SplitButton's values reach through
/// the plain Buttons its template holds, which set none. The class is read when a value arrives,
/// which is when the icon joins its host: put it on the icon in markup, as the docs show, not
/// later.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The classes are the
/// interface.
/// </para>
/// </remarks>
internal sealed class IconSlot
{
    private const string StartIcon = "start-icon";
    private const string EndIcon = "end-icon";

    private IconSlot()
    {
    }

    public static readonly AttachedProperty<double?> StartSizeProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, double?>("StartSize", inherits: true);

    public static readonly AttachedProperty<Thickness?> StartMarginProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, Thickness?>("StartMargin", inherits: true);

    public static readonly AttachedProperty<double?> EndSizeProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, double?>("EndSize", inherits: true);

    public static readonly AttachedProperty<Thickness?> EndMarginProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, Thickness?>("EndMargin", inherits: true);

    /// <summary>A start icon's colour: a nav link's drawer-icon, or its colour while active.</summary>
    public static readonly AttachedProperty<IBrush?> BrushProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, IBrush?>("Brush", inherits: true);

    static IconSlot()
    {
        StartSizeProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, StartIcon, e.GetNewValue<double?>(), Layoutable.WidthProperty, Layoutable.HeightProperty));
        EndSizeProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, EndIcon, e.GetNewValue<double?>(), Layoutable.WidthProperty, Layoutable.HeightProperty));
        StartMarginProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, StartIcon, e.GetNewValue<Thickness?>(), Layoutable.MarginProperty));
        EndMarginProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, EndIcon, e.GetNewValue<Thickness?>(), Layoutable.MarginProperty));
        BrushProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, StartIcon, e.GetNewValue<IBrush?>(), TextElement.ForegroundProperty));
    }

    public static double? GetStartSize(Control c) => c.GetValue(StartSizeProperty);

    public static void SetStartSize(Control c, double? value) => c.SetValue(StartSizeProperty, value);

    public static Thickness? GetStartMargin(Control c) => c.GetValue(StartMarginProperty);

    public static void SetStartMargin(Control c, Thickness? value) => c.SetValue(StartMarginProperty, value);

    public static double? GetEndSize(Control c) => c.GetValue(EndSizeProperty);

    public static void SetEndSize(Control c, double? value) => c.SetValue(EndSizeProperty, value);

    public static Thickness? GetEndMargin(Control c) => c.GetValue(EndMarginProperty);

    public static void SetEndMargin(Control c, Thickness? value) => c.SetValue(EndMarginProperty, value);

    public static IBrush? GetBrush(Control c) => c.GetValue(BrushProperty);

    public static void SetBrush(Control c, IBrush? value) => c.SetValue(BrushProperty, value);

    /// <summary>A value that has reached a control: set on it if it is that kind of icon, cleared if empty.</summary>
    private static void Apply(Control control, string icon, object? value, params AvaloniaProperty[] targets)
    {
        if (!control.Classes.Contains(icon))
        {
            return;
        }

        foreach (var target in targets)
        {
            control.SetValue(target, value ?? AvaloniaProperty.UnsetValue, BindingPriority.StyleTrigger);
        }
    }
}
