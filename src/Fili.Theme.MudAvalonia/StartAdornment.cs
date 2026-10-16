using Avalonia;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudInput's start adornment floats the label: a text field with <c>InnerLeftContent</c> gets
/// the <c>:start-adornment</c> pseudo-class, which the field's theme floats the label on. Set on
/// every text field by its theme.
/// </summary>
/// <remarks>
/// <para>
/// <c>MudInput.razor.cs</c> shrinks the label on <c>Adornment == Adornment.Start</c> as it does on
/// a value or focus, so a resting label never lies over the adornment. A selector cannot ask
/// whether a property is null, so this does, and keeps the answer as a pseudo-class.
/// </para>
/// <para>
/// Internal, like <see cref="MenuIconGutter"/>: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class StartAdornment
{
    private const string PseudoClass = ":start-adornment";

    private StartAdornment()
    {
    }

    public static readonly AttachedProperty<bool> TracksProperty =
        AvaloniaProperty.RegisterAttached<StartAdornment, TextBox, bool>("Tracks");

    public static bool GetTracks(TextBox box) => box.GetValue(TracksProperty);

    public static void SetTracks(TextBox box, bool value) => box.SetValue(TracksProperty, value);

    static StartAdornment()
    {
        TracksProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
        TextBox.InnerLeftContentProperty.Changed.AddClassHandler<TextBox>((box, _) => Refresh(box));
    }

    private static void Refresh(TextBox box) =>
        ((IPseudoClasses)box.Classes).Set(PseudoClass, GetTracks(box) && box.InnerLeftContent is not null);
}
