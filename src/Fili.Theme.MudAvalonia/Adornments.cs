using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Which of a text field's adornments are there: a text field with <c>InnerLeftContent</c> gets
/// the <c>:start-adornment</c> pseudo-class, one with <c>InnerRightContent</c>
/// <c>:end-adornment</c>. Set on every text field by its theme, which floats the label on the
/// first and keeps each adornment 8px from the text. And whether it has a label at all: one with
/// no <c>PlaceholderText</c>, which doubles as the label, gets <c>:no-label</c>; and whether it has
/// helper text, <c>:helper-text</c>, for which its theme builds the helper line.
/// </summary>
/// <remarks>
/// <para>
/// <c>MudInput.razor.cs</c> shrinks the label on <c>Adornment == Adornment.Start</c> as it does on
/// a value or focus, so a resting label never lies over the adornment; and _input.scss's
/// <c>.mud-input-adornment-start</c> and <c>-end</c> hold the adornment 8px from the text. A
/// selector cannot ask whether a property is null, so this does, and keeps the answer as a
/// pseudo-class. An empty presenter takes no gap.
/// </para>
/// <para>
/// A text adornment, a string or a <c>TextBlock</c> such as a unit, also gets
/// <c>:start-text-adornment</c> or <c>:end-text-adornment</c>. <c>.mud-input</c> centres every
/// adornment on a filled or outlined box, which lifts a unit above the value it is read with;
/// the theme keeps text on the text line instead and centres only icons on the box, a deliberate
/// departure from MudBlazor.
/// </para>
/// <para>
/// <c>MudInputCssHelper</c> adds <c>mud-input-{variant}-with-label</c> only when there is a
/// Label, and only then does a standard field take the label's 16px above the text, or a filled
/// one push its text down to 27px; with none it is MudInput, the bare input, 32px tall, and a
/// filled field centres its text (<c>.mud-input-root-hidden-label</c>).
/// </para>
/// <para>
/// Internal, like <see cref="MenuIconGutter"/>: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class Adornments
{
    private const string Start = ":start-adornment";
    private const string End = ":end-adornment";
    private const string StartText = ":start-text-adornment";
    private const string EndText = ":end-text-adornment";
    private const string NoLabel = ":no-label";
    private const string HelperText = ":helper-text";

    private Adornments()
    {
    }

    public static readonly AttachedProperty<bool> TracksProperty =
        AvaloniaProperty.RegisterAttached<Adornments, TextBox, bool>("Tracks");

    public static bool GetTracks(TextBox box) => box.GetValue(TracksProperty);

    public static void SetTracks(TextBox box, bool value) => box.SetValue(TracksProperty, value);

    static Adornments()
    {
        TracksProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
        TextBox.InnerLeftContentProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
        TextBox.InnerRightContentProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
        TextBox.PlaceholderTextProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
        AutomationProperties.HelpTextProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
    }

    private static void Refresh(TextBox box)
    {
        var tracks = GetTracks(box);
        ((IPseudoClasses)box.Classes).Set(Start, tracks && box.InnerLeftContent is not null);
        ((IPseudoClasses)box.Classes).Set(End, tracks && box.InnerRightContent is not null);
        ((IPseudoClasses)box.Classes).Set(StartText, tracks && IsText(box.InnerLeftContent));
        ((IPseudoClasses)box.Classes).Set(EndText, tracks && IsText(box.InnerRightContent));
        ((IPseudoClasses)box.Classes).Set(NoLabel, tracks && string.IsNullOrEmpty(box.PlaceholderText));
        ((IPseudoClasses)box.Classes).Set(HelperText, tracks && !string.IsNullOrEmpty(AutomationProperties.GetHelpText(box)));
    }

    private static bool IsText(object? content) => content is string or TextBlock;
}
