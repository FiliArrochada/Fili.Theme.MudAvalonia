using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// CSS's <c>text-transform</c> for a button's label: MudBlazor's
/// <c>Typography.Button.TextTransform</c>, <c>uppercase</c> by default, which <c>.mud-button</c>,
/// <c>.mud-fab</c> and <c>.mud-tab</c> apply to their text.
/// </summary>
/// <remarks>
/// <para>
/// Avalonia has no text transform, so the label's string is transformed instead. A template's
/// presenter sets <see cref="CaseProperty"/> to the <c>FiliButtonTextTransform</c> resource and
/// binds its <c>Content</c> through <see cref="Converter"/>, with the control's content and that
/// case as the two values. The resource is the theme-wide switch MudBlazor's typography is: an
/// app that sets it to <c>none</c> keeps its labels as written, everywhere or in one region.
/// </para>
/// <para>
/// Only a string is transformed. CSS reaches every piece of text inside a button; here content
/// that is already a control - a <c>TextBlock</c>, a panel holding one - is the app's own and is
/// shown as it is. An access key survives: "_Save" becomes "_SAVE", still Alt+S.
/// </para>
/// <para>
/// Internal, like the converters: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class TextTransform
{
    private TextTransform()
    {
    }

    /// <summary>The case a presenter's string content is shown in: CSS's <c>none</c>, <c>uppercase</c>, <c>lowercase</c> or <c>capitalize</c>.</summary>
    public static readonly AttachedProperty<string?> CaseProperty =
        AvaloniaProperty.RegisterAttached<TextTransform, ContentPresenter, string?>("Case");

    public static string? GetCase(ContentPresenter presenter) => presenter.GetValue(CaseProperty);

    public static void SetCase(ContentPresenter presenter, string? value) => presenter.SetValue(CaseProperty, value);

    /// <summary>The content, then the case: a string in that case, anything else unchanged.</summary>
    public static readonly IMultiValueConverter Converter = new CaseConverter();

    internal static object? Apply(object? content, string? textCase, CultureInfo culture) =>
        content is not string text ? content : textCase switch
        {
            "uppercase" => text.ToUpper(culture),
            "lowercase" => text.ToLower(culture),
            "capitalize" => Capitalize(text, culture),
            _ => text,
        };

    /// <summary>CSS's <c>capitalize</c>: the first letter of each word up, the rest left as written.</summary>
    private static string Capitalize(string text, CultureInfo culture)
    {
        var result = new StringBuilder(text.Length);
        var startOfWord = true;

        foreach (var c in text)
        {
            result.Append(startOfWord && char.IsLetter(c) ? char.ToUpper(c, culture) : c);
            if (char.IsLetter(c) || char.IsDigit(c))
            {
                startOfWord = false;
            }
            else if (char.IsWhiteSpace(c))
            {
                startOfWord = true;
            }
        }

        return result.ToString();
    }

    private sealed class CaseConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
            values.Count < 2 ? values.Count == 1 ? values[0] : null : Apply(values[0], values[1] as string, culture);
    }
}
