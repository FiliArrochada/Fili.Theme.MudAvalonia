using Avalonia;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Which of a text field's adornments are there: a text field with <c>InnerLeftContent</c> gets
/// the <c>:start-adornment</c> pseudo-class, one with <c>InnerRightContent</c>
/// <c>:end-adornment</c>. Set on every text field by its theme, which floats the label on the
/// first and keeps each adornment 8px from the text.
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
/// Internal, like <see cref="MenuIconGutter"/>: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class Adornments
{
    private const string Start = ":start-adornment";
    private const string End = ":end-adornment";

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
    }

    private static void Refresh(TextBox box)
    {
        var tracks = GetTracks(box);
        ((IPseudoClasses)box.Classes).Set(Start, tracks && box.InnerLeftContent is not null);
        ((IPseudoClasses)box.Classes).Set(End, tracks && box.InnerRightContent is not null);
    }
}
