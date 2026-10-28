using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// The two things MudAutocomplete adds to the field inside an <c>AutoCompleteBox</c> that a style
/// cannot: the <c>:select-input</c> pseudo-class on its <c>TextBox</c>, and a drop-down arrow that
/// opens and closes the list.
/// </summary>
/// <remarks>
/// <para>
/// MudAutocomplete's input carries <c>mud-select-input</c>, and _select.scss takes the 8px off
/// <c>.mud-input-adornment-end</c> under it. The rule belongs to the TextBox's theme, two
/// templates down from the AutoCompleteBox, where the AutoCompleteBox's selectors cannot go; the
/// AutoCompleteBox's template sets <see cref="IsSelectInputProperty"/> on its TextBox instead, and
/// this keeps it as a pseudo-class.
/// </para>
/// <para>
/// The arrow is MudAutocomplete's <c>AdornmentClickHandlerAsync</c>: a click toggles the list and,
/// opening it, focuses the field. A button rather than a toggle bound two ways to
/// <c>IsDropDownOpen</c>: the AutoCompleteBox can refuse to open - with fewer characters than
/// <c>MinimumPrefixLength</c> it closes again inside the same change - and a two-way binding never
/// hears the refusal, so the toggle stayed checked over a closed list. The glyph reads
/// <c>IsDropDownOpen</c> instead, which is always the truth.
/// </para>
/// <para>
/// Internal, like <see cref="Adornments"/>: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class AutoCompleteField
{
    private const string SelectInput = ":select-input";

    private AutoCompleteField()
    {
    }

    public static readonly AttachedProperty<bool> IsSelectInputProperty =
        AvaloniaProperty.RegisterAttached<AutoCompleteField, TextBox, bool>("IsSelectInput");

    public static bool GetIsSelectInput(TextBox box) => box.GetValue(IsSelectInputProperty);

    public static void SetIsSelectInput(TextBox box, bool value) => box.SetValue(IsSelectInputProperty, value);

    public static readonly AttachedProperty<bool> TogglesDropDownProperty =
        AvaloniaProperty.RegisterAttached<AutoCompleteField, Button, bool>("TogglesDropDown");

    public static bool GetTogglesDropDown(Button button) => button.GetValue(TogglesDropDownProperty);

    public static void SetTogglesDropDown(Button button, bool value) => button.SetValue(TogglesDropDownProperty, value);

    static AutoCompleteField()
    {
        IsSelectInputProperty.Changed.AddClassHandler<TextBox>((box, e) =>
            ((IPseudoClasses)box.Classes).Set(SelectInput, e.GetNewValue<bool>()));

        TogglesDropDownProperty.Changed.AddClassHandler<Button>((button, e) =>
        {
            button.Click -= Toggle;
            if (e.GetNewValue<bool>())
                button.Click += Toggle;
        });
    }

    private static void Toggle(object? sender, RoutedEventArgs e)
    {
        if ((sender as Visual)?.FindAncestorOfType<AutoCompleteBox>() is not { } box)
            return;

        // The field first: focus moving into it from the AutoCompleteBox is a LostFocus on the
        // box, which closes a list opened before it.
        if (!box.IsDropDownOpen)
            (sender as Visual)?.FindAncestorOfType<TextBox>()?.Focus();
        box.SetCurrentValue(AutoCompleteBox.IsDropDownOpenProperty, !box.IsDropDownOpen);
    }
}
