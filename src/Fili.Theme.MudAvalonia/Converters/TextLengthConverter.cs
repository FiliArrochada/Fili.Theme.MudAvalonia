using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// The text field counter's number: the length of a string, 0 for none.
/// </summary>
///
/// <remarks>
/// <para>
/// <c>MudTextField.GetCounterText</c> counts <c>Text?.Length ?? 0</c>. A binding to
/// <c>Text.Length</c> shows the same 0 through its fallback, but logs a binding error first
/// whenever <c>Text</c> is null - which is every <c>TextBox</c> an app has not filled yet, counter
/// or not, because the hidden counter is still bound. Binding <c>Text</c> and taking the length
/// here reads null as the empty string it means, and logs nothing.
/// </para>
/// <para>
/// Internal, like <see cref="FactorConverter"/>: it exists because a template needs it, not
/// because a consumer does.
/// </para>
/// </remarks>
internal sealed class TextLengthConverter : IValueConverter
{
    public static readonly TextLengthConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as string)?.Length ?? 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("TextLengthConverter is one-way: a counter is never edited.");
}
