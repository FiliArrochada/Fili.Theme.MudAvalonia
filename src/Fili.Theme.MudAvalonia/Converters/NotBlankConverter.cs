using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// True when a text is neither null, empty nor only white space: when a clearable field shows its
/// clear button.
/// </summary>
///
/// <remarks>
/// <para>
/// <c>MudInput.ShowClearButton</c> asks <c>!string.IsNullOrWhiteSpace</c>. A field's
/// <c>:empty</c> pseudo-class is only null or empty, so a field holding spaces would show a button
/// MudBlazor does not; no selector reads white space, and Avalonia's <c>StringConverters</c> stop
/// at empty.
/// </para>
/// <para>
/// Internal, like <see cref="FactorConverter"/>: it exists because a template needs it, not
/// because a consumer does.
/// </para>
/// </remarks>
internal sealed class NotBlankConverter : IValueConverter
{
    public static readonly NotBlankConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("NotBlankConverter is one-way: a button's visibility is never edited.");
}
