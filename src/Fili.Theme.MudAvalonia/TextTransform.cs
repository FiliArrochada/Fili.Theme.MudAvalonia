using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
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
/// The case is inherited, as CSS's <c>text-transform</c> is, so it also reaches the text in
/// content that is a control: the label beside a <c>start-icon</c> in a panel, MudButton's
/// <c>StartIcon</c> idiom. A <c>TextBlock</c> there keeps its <c>Text</c>, which may be the app's
/// binding, and draws it through a single <c>Run</c> in that case; a new <c>Text</c> clears the
/// run and a new one is made. A <c>TextBlock</c> with inlines of the app's own is left as written,
/// and an <c>AccessText</c> too: the string path already hands it the text in case, and inlines
/// would lose its access key. An access key survives: "_Save" becomes "_SAVE", still Alt+S.
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

    /// <summary>
    /// The case a presenter's content is shown in: CSS's <c>none</c>, <c>uppercase</c>,
    /// <c>lowercase</c> or <c>capitalize</c>. Inherited, so every <c>TextBlock</c> in the content
    /// takes it.
    /// </summary>
    public static readonly AttachedProperty<string?> CaseProperty =
        AvaloniaProperty.RegisterAttached<TextTransform, Control, string?>("Case", inherits: true);

    /// <summary>Whether a TextBlock's inlines are the run this class drew, rather than the app's.</summary>
    private static readonly AttachedProperty<bool> DrawsRunProperty =
        AvaloniaProperty.RegisterAttached<TextTransform, TextBlock, bool>("DrawsRun");

    static TextTransform()
    {
        CaseProperty.Changed.AddClassHandler<TextBlock>((block, _) => Draw(block));
        TextBlock.TextProperty.Changed.AddClassHandler<TextBlock>((block, _) => Draw(block));
    }

    public static string? GetCase(Control control) => control.GetValue(CaseProperty);

    public static void SetCase(Control control, string? value) => control.SetValue(CaseProperty, value);

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

    /// <summary>
    /// Draws a TextBlock's text in the case it inherited, through one run, leaving <c>Text</c> as
    /// it is. Called when the case reaches it and whenever its text changes; the TextBlock has
    /// cleared its inlines by then, so the run is drawn again.
    /// </summary>
    private static void Draw(TextBlock block)
    {
        if (block is AccessText)
        {
            return;
        }

        var drawn = block.GetValue(DrawsRunProperty);
        if (!drawn && block.Inlines is { Count: > 0 })
        {
            return;
        }

        var text = block.Text;
        var shown = Apply(text, block.GetValue(CaseProperty), CultureInfo.CurrentCulture) as string;
        if (text is null || shown == text)
        {
            if (drawn)
            {
                block.ClearValue(DrawsRunProperty);
                block.Inlines = null;
            }

            return;
        }

        block.Inlines = new InlineCollection { new Run(shown) };
        block.SetValue(DrawsRunProperty, true);
    }

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
