using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// Caps a numeric badge at MudBadge's default <c>Max</c>: an <see cref="int"/> above 99 reads
/// <c>"99+"</c>.
/// </summary>
///
/// <remarks>
/// <para>
/// This is <c>MudBadge.OnParametersSet</c>: an int Content over <c>Max</c> becomes
/// <c>Max + "+"</c>, anything else is shown as it is. A binding cannot compare, so the badge
/// template needs this to show what MudBlazor shows for the commonest case, an unread count.
/// <c>Max</c> itself is a parameter Avalonia has nowhere to carry, so it is fixed at its default;
/// an app that wants another cap formats the string itself.
/// </para>
/// <para>
/// MudBlazor shows nothing for a Content that is neither a string nor an int. Here any other value
/// passes through, because an Avalonia Header is also how an icon badge gets its PathIcon.
/// </para>
/// <para>
/// Internal, like <see cref="FactorConverter"/>: it exists because a template needs it, not
/// because a consumer does.
/// </para>
/// </remarks>
internal sealed class BadgeContentConverter : IValueConverter
{
    /// <summary>MudBadge.Max's default.</summary>
    public const int Max = 99;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int number && number > Max ? Max + "+" : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("BadgeContentConverter is one-way: a badge is never edited.");
}
