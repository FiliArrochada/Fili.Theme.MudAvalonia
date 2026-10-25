using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// What an icon marked <c>start-icon</c> or <c>end-icon</c> takes from the control it sits in: a
/// button's, a chip's or a FAB's icon size and gaps, a nav link's size, gap and colour, a link's,
/// a tab's or a text field adornment's size and gap. And what an <c>avatar</c> takes from a chip:
/// its size, gaps and text size. Set by the host - a button's theme, a nav row's style - and
/// inherited by everything in its content; when a value reaches a control with the class, this
/// sets it on that control.
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
    private const string Avatar = "avatar";

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

    /// <summary>An avatar's width and height in a chip: `.mud-chip .mud-avatar`.</summary>
    public static readonly AttachedProperty<double?> AvatarSizeProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, double?>("AvatarSize", inherits: true);

    public static readonly AttachedProperty<Thickness?> AvatarMarginProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, Thickness?>("AvatarMargin", inherits: true);

    /// <summary>An avatar's text size in a chip.</summary>
    public static readonly AttachedProperty<double?> AvatarFontSizeProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, double?>("AvatarFontSize", inherits: true);

    /// <summary>A start icon's colour: a nav link's drawer-icon, or its colour while active.</summary>
    public static readonly AttachedProperty<IBrush?> BrushProperty =
        AvaloniaProperty.RegisterAttached<IconSlot, Control, IBrush?>("Brush", inherits: true);

    static IconSlot()
    {
        StartSizeProperty.Changed.AddClassHandler<Control>((c, e) =>
        {
            Apply(c, StartIcon, e.GetNewValue<double?>(), Layoutable.WidthProperty, Layoutable.HeightProperty);
            Centre(c, StartIcon, e.GetNewValue<double?>() is not null);
        });
        EndSizeProperty.Changed.AddClassHandler<Control>((c, e) =>
        {
            Apply(c, EndIcon, e.GetNewValue<double?>(), Layoutable.WidthProperty, Layoutable.HeightProperty);
            Centre(c, EndIcon, e.GetNewValue<double?>() is not null);
        });
        StartMarginProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, StartIcon, e.GetNewValue<Thickness?>(), Layoutable.MarginProperty));
        EndMarginProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, EndIcon, e.GetNewValue<Thickness?>(), Layoutable.MarginProperty));
        BrushProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, StartIcon, e.GetNewValue<IBrush?>(), TextElement.ForegroundProperty));
        AvatarSizeProperty.Changed.AddClassHandler<Control>((c, e) =>
        {
            Apply(c, Avatar, e.GetNewValue<double?>(), Layoutable.WidthProperty, Layoutable.HeightProperty);
            Centre(c, Avatar, e.GetNewValue<double?>() is not null);
        });
        AvatarMarginProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, Avatar, e.GetNewValue<Thickness?>(), Layoutable.MarginProperty));
        AvatarFontSizeProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, Avatar, e.GetNewValue<double?>(), TextElement.FontSizeProperty));
    }

    public static double? GetStartSize(Control c) => c.GetValue(StartSizeProperty);

    public static void SetStartSize(Control c, double? value) => c.SetValue(StartSizeProperty, value);

    public static Thickness? GetStartMargin(Control c) => c.GetValue(StartMarginProperty);

    public static void SetStartMargin(Control c, Thickness? value) => c.SetValue(StartMarginProperty, value);

    public static double? GetEndSize(Control c) => c.GetValue(EndSizeProperty);

    public static void SetEndSize(Control c, double? value) => c.SetValue(EndSizeProperty, value);

    public static Thickness? GetEndMargin(Control c) => c.GetValue(EndMarginProperty);

    public static void SetEndMargin(Control c, Thickness? value) => c.SetValue(EndMarginProperty, value);

    public static double? GetAvatarSize(Control c) => c.GetValue(AvatarSizeProperty);

    public static void SetAvatarSize(Control c, double? value) => c.SetValue(AvatarSizeProperty, value);

    public static Thickness? GetAvatarMargin(Control c) => c.GetValue(AvatarMarginProperty);

    public static void SetAvatarMargin(Control c, Thickness? value) => c.SetValue(AvatarMarginProperty, value);

    public static double? GetAvatarFontSize(Control c) => c.GetValue(AvatarFontSizeProperty);

    public static void SetAvatarFontSize(Control c, double? value) => c.SetValue(AvatarFontSizeProperty, value);

    public static IBrush? GetBrush(Control c) => c.GetValue(BrushProperty);

    public static void SetBrush(Control c, IBrush? value) => c.SetValue(BrushProperty, value);

    /// <summary>
    /// The row an icon or a chip's avatar sits in, centred: `.mud-button-label`, `.mud-chip-content`
    /// and their kin are flex rows with
    /// `align-items: center`, so the icon and the label beside it share a centre line. A TextBlock
    /// left to stretch to the icon's 20px draws its text at the top of that box, 2px above the
    /// icon's middle; centred, its own line box is centred on the row, as CSS's is. The icon and
    /// every TextBlock beside it in its panel, at the same priority as the rest, so an app's own
    /// alignment still wins; cleared again when the icon leaves its host.
    /// </summary>
    private static void Centre(Control control, string icon, bool hosted)
    {
        if (!control.Classes.Contains(icon))
        {
            return;
        }

        var value = hosted ? (object)VerticalAlignment.Center : null;
        Write(control, Layoutable.VerticalAlignmentProperty, value);
        if (control.Parent is Panel row)
        {
            foreach (var label in row.Children.OfType<TextBlock>())
            {
                Write(label, Layoutable.VerticalAlignmentProperty, value);
            }
        }
    }

    /// <summary>
    /// What this set on each control, by property: a value set below LocalValue priority is an
    /// entry of its own, which a second SetValue - an UnsetValue included - adds to rather than
    /// replaces, so the earlier one is disposed first. Without that, an icon that once took a list
    /// row's colour kept it after a button round it cleared the colour.
    /// </summary>
    private static readonly ConditionalWeakTable<Control, Dictionary<AvaloniaProperty, IDisposable>> Set = new();

    /// <summary>A value that has reached a control: set on it if it is that kind of icon, cleared if empty.</summary>
    private static void Apply(Control control, string icon, object? value, params AvaloniaProperty[] targets)
    {
        if (!control.Classes.Contains(icon))
        {
            return;
        }

        foreach (var target in targets)
        {
            Write(control, target, value);
        }
    }

    private static void Write(Control control, AvaloniaProperty target, object? value)
    {
        var set = Set.GetOrCreateValue(control);
        if (set.Remove(target, out var previous))
        {
            previous.Dispose();
        }

        if (value is not null && control.SetValue(target, value, BindingPriority.StyleTrigger) is { } entry)
        {
            set[target] = entry;
        }
    }
}
