using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// A control's colour, set once by its colour class, for its template to paint with: MudBlazor's
/// <c>Color</c> and <c>UncheckedColor</c> on a checkbox or a radio. The colour class sets the glyph
/// colour, its hover tint and its ripple; <c>unchecked-{colour}</c> sets the same three for while
/// the control is not checked. This works out what the template shows now: the glyph colour off,
/// and the halo and ripple for the current state. A switch takes its colour only while on and its
/// unchecked colour only while off, each falling back to its own thumb, track and halo at rest.
/// </summary>
/// <remarks>
/// <para>
/// Each colour was seven styles - the three glyphs, the halo on hover, focus and press, and the
/// ripple - and each unchecked colour five more, so a checkbox carried 176 nested styles, every
/// one an instance on every checkbox in the window. Now a colour is one style setting these
/// values, and the template binds them.
/// </para>
/// <para>
/// <see cref="ColourProperty"/> and <see cref="HoverProperty"/> have the control's own defaults
/// from its theme; <see cref="RippleProperty"/> is null for the ripple's default, the control's
/// text colour. An unchecked value left null follows the checked one, as MudBlazor's
/// <c>UncheckedColor</c> does when it is not set.
/// </para>
/// <para>
/// A tab bar's colour class sets <see cref="BarProperty"/>, <see cref="GroundProperty"/> and the
/// contrast text and lighten tint as <see cref="ColourProperty"/> and <see cref="HoverProperty"/>;
/// one set of styles reads them for every colour, where each colour had its own styles on every
/// tab. A list's selected row is <see cref="RowColourProperty"/> over <see cref="RowHoverProperty"/>,
/// inherited from the list: its theme sets primary and a colour class its colour, and the row's
/// own theme reads them, so no style reaches from the list to its rows for a colour. A
/// multi-selection list's colour class sets the colour and hover of its rows' checkboxes
/// the same way, and a filled button's the line between it and the button before it in a group
/// (<see cref="SeamProperty"/>).
/// </para>
/// <para>
/// A button uses <see cref="HoverProperty"/> alone: its colour class sets the tint its state layer,
/// or a filled button's fill, takes on hover, focus and press.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The classes are the
/// interface.
/// </para>
/// </remarks>
internal sealed class Accent
{
    private Accent()
    {
    }

    /// <summary>The glyph's colour: action-default, or the colour class's.</summary>
    public static readonly AttachedProperty<IBrush?> ColourProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Colour");

    /// <summary>The halo on hover, focus and press: action-default-hover, or the colour's hover tint.</summary>
    public static readonly AttachedProperty<IBrush?> HoverProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Hover");

    /// <summary>The ripple: null for the control's text colour, or the colour class's.</summary>
    public static readonly AttachedProperty<IBrush?> RippleProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Ripple");

    /// <summary>UncheckedColor's glyph colour, while not checked.</summary>
    public static readonly AttachedProperty<IBrush?> UncheckedColourProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("UncheckedColour");

    /// <summary>UncheckedColor's halo, while not checked.</summary>
    public static readonly AttachedProperty<IBrush?> UncheckedHoverProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("UncheckedHover");

    /// <summary>UncheckedColor's ripple, while not checked.</summary>
    public static readonly AttachedProperty<IBrush?> UncheckedRippleProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("UncheckedRipple");

    /// <summary>A tab bar with a colour class: the styles that read its colours apply.</summary>
    public static readonly AttachedProperty<bool> BarProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, bool>("Bar");

    /// <summary>A list's selected row's text: primary, or the colour class's. Inherited.</summary>
    public static readonly AttachedProperty<IBrush?> RowColourProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("RowColour", inherits: true);

    /// <summary>A list's selected row's tint: primary's hover, or the colour's. Inherited.</summary>
    public static readonly AttachedProperty<IBrush?> RowHoverProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("RowHover", inherits: true);

    /// <summary>A coloured tab bar's ground.</summary>
    public static readonly AttachedProperty<IBrush?> GroundProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Ground");

    /// <summary>A filled button's line against the button before it in a group.</summary>
    public static readonly AttachedProperty<IBrush?> SeamProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Seam");

    /// <summary>A switch's thumb with no colour for its state: switch-thumb.</summary>
    public static readonly AttachedProperty<IBrush?> RestThumbProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("RestThumb");

    /// <summary>A switch's track with no colour for its state: action-default.</summary>
    public static readonly AttachedProperty<IBrush?> RestTrackProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("RestTrack");

    /// <summary>A switch's halo with no colour for its state: action-default-hover.</summary>
    public static readonly AttachedProperty<IBrush?> RestHoverProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("RestHover");

    /// <summary>Worked out: a switch's thumb for the current state.</summary>
    public static readonly AttachedProperty<IBrush?> ThumbProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Thumb");

    /// <summary>Worked out: a switch's track for the current state.</summary>
    public static readonly AttachedProperty<IBrush?> TrackProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Track");

    /// <summary>Worked out: the unchecked glyphs' colour.</summary>
    public static readonly AttachedProperty<IBrush?> OffColourProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("OffColour");

    /// <summary>
    /// Worked out: a checkbox's glyph colour for the current state, the colour while checked and
    /// the unchecked colour otherwise. Its one glyph Path binds it, where three Paths bound the two.
    /// </summary>
    public static readonly AttachedProperty<IBrush?> GlyphProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Glyph");

    /// <summary>Worked out: the halo for the current state.</summary>
    public static readonly AttachedProperty<IBrush?> HaloProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("Halo");

    /// <summary>Worked out: the ripple for the current state.</summary>
    public static readonly AttachedProperty<IBrush?> RippleNowProperty =
        AvaloniaProperty.RegisterAttached<Accent, Control, IBrush?>("RippleNow");

    static Accent()
    {
        foreach (var input in new[] { ColourProperty, HoverProperty, RippleProperty, UncheckedColourProperty, UncheckedHoverProperty, UncheckedRippleProperty, RestThumbProperty, RestTrackProperty, RestHoverProperty })
        {
            input.Changed.AddClassHandler<Control>((control, _) => Update(control));
        }

        ToggleButton.IsCheckedProperty.Changed.AddClassHandler<ToggleButton>((toggle, _) => Update(toggle));
    }

    public static IBrush? GetColour(Control c) => c.GetValue(ColourProperty);

    public static void SetColour(Control c, IBrush? value) => c.SetValue(ColourProperty, value);

    public static IBrush? GetHover(Control c) => c.GetValue(HoverProperty);

    public static void SetHover(Control c, IBrush? value) => c.SetValue(HoverProperty, value);

    public static IBrush? GetRipple(Control c) => c.GetValue(RippleProperty);

    public static void SetRipple(Control c, IBrush? value) => c.SetValue(RippleProperty, value);

    public static IBrush? GetUncheckedColour(Control c) => c.GetValue(UncheckedColourProperty);

    public static void SetUncheckedColour(Control c, IBrush? value) => c.SetValue(UncheckedColourProperty, value);

    public static IBrush? GetUncheckedHover(Control c) => c.GetValue(UncheckedHoverProperty);

    public static void SetUncheckedHover(Control c, IBrush? value) => c.SetValue(UncheckedHoverProperty, value);

    public static IBrush? GetUncheckedRipple(Control c) => c.GetValue(UncheckedRippleProperty);

    public static void SetUncheckedRipple(Control c, IBrush? value) => c.SetValue(UncheckedRippleProperty, value);

    public static bool GetBar(Control c) => c.GetValue(BarProperty);

    public static void SetBar(Control c, bool value) => c.SetValue(BarProperty, value);

    public static IBrush? GetRowColour(Control c) => c.GetValue(RowColourProperty);

    public static void SetRowColour(Control c, IBrush? value) => c.SetValue(RowColourProperty, value);

    public static IBrush? GetRowHover(Control c) => c.GetValue(RowHoverProperty);

    public static void SetRowHover(Control c, IBrush? value) => c.SetValue(RowHoverProperty, value);

    public static IBrush? GetGround(Control c) => c.GetValue(GroundProperty);

    public static void SetGround(Control c, IBrush? value) => c.SetValue(GroundProperty, value);

    public static IBrush? GetSeam(Control c) => c.GetValue(SeamProperty);

    public static void SetSeam(Control c, IBrush? value) => c.SetValue(SeamProperty, value);

    public static IBrush? GetRestThumb(Control c) => c.GetValue(RestThumbProperty);

    public static void SetRestThumb(Control c, IBrush? value) => c.SetValue(RestThumbProperty, value);

    public static IBrush? GetRestTrack(Control c) => c.GetValue(RestTrackProperty);

    public static void SetRestTrack(Control c, IBrush? value) => c.SetValue(RestTrackProperty, value);

    public static IBrush? GetRestHover(Control c) => c.GetValue(RestHoverProperty);

    public static void SetRestHover(Control c, IBrush? value) => c.SetValue(RestHoverProperty, value);

    public static IBrush? GetThumb(Control c) => c.GetValue(ThumbProperty);

    public static IBrush? GetTrack(Control c) => c.GetValue(TrackProperty);

    public static IBrush? GetOffColour(Control c) => c.GetValue(OffColourProperty);

    public static IBrush? GetGlyph(Control c) => c.GetValue(GlyphProperty);

    public static IBrush? GetHalo(Control c) => c.GetValue(HaloProperty);

    public static IBrush? GetRippleNow(Control c) => c.GetValue(RippleNowProperty);

    /// <summary>
    /// What the template shows now. Checked is IsChecked true; unchecked and indeterminate are
    /// both "not checked", as MudCheckBox's UncheckedColor covers false and null.
    /// </summary>
    private static void Update(Control control)
    {
        // A button reads Colour and Hover straight from its template; only these work out a state.
        if (control is not (CheckBox or RadioButton or ToggleSwitch))
        {
            return;
        }

        var on = ((ToggleButton)control).IsChecked == true;
        if (control is ToggleSwitch)
        {
            // MudSwitch: `mud-{color}` only while checked, UncheckedColor only while not.
            var now = control.GetValue(on ? ColourProperty : UncheckedColourProperty);
            Write(control, ThumbProperty, now ?? control.GetValue(RestThumbProperty));
            Write(control, TrackProperty, now ?? control.GetValue(RestTrackProperty));
            Write(control, HaloProperty, control.GetValue(on ? HoverProperty : UncheckedHoverProperty) ?? control.GetValue(RestHoverProperty));
            Write(control, RippleNowProperty, control.GetValue(on ? RippleProperty : UncheckedRippleProperty));
            return;
        }

        var colour = control.GetValue(ColourProperty);
        var hover = control.GetValue(HoverProperty);
        var ripple = control.GetValue(RippleProperty);
        var off = control.GetValue(UncheckedColourProperty) ?? colour;
        Write(control, OffColourProperty, off);
        if (control is CheckBox)
        {
            Write(control, GlyphProperty, on ? colour : off);
        }

        Write(control, HaloProperty, on ? hover : control.GetValue(UncheckedHoverProperty) ?? hover);
        Write(control, RippleNowProperty, on ? ripple : control.GetValue(UncheckedRippleProperty) ?? ripple);
    }

    /// <summary>Only a change is written, so a toggle with no colour of this kind stays untouched.</summary>
    private static void Write(Control control, AvaloniaProperty<IBrush?> property, IBrush? value)
    {
        if (!ReferenceEquals(control.GetValue(property), value))
        {
            control.SetValue(property, value);
        }
    }
}
