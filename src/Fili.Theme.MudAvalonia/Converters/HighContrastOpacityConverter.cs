using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Styling;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// An opacity from a bound <see cref="ThemeVariant"/>: 0 in high contrast, or a variant that
/// inherits from it, and 1 in every other.
/// </summary>
/// <remarks>
/// <para>
/// A style cannot select on the theme variant, and a resource varies a value, not whether a part
/// is drawn. The drawer needs the second: in high contrast its edge is the drawer-border line the
/// fork's <c>HCPaneBorder</c> draws, and MudDrawer's elevation there, which this theme draws as a
/// ring like every other, would double it into a 2px line.
/// </para>
/// <para>
/// An opacity, not a visibility: a binding in a template outranks a style with a pseudo-class,
/// and the drawer's styles hide its shadow when it closes to nothing.
/// </para>
/// <para>
/// Internal, like the other converters: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class HighContrastOpacityConverter : IValueConverter
{
    public static HighContrastOpacityConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        for (var variant = value as ThemeVariant; variant is not null; variant = variant.InheritVariant)
        {
            if (variant == FiliThemeVariants.HighContrast)
            {
                return 0d;
            }
        }

        return 1d;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("HighContrastOpacityConverter is one-way: it decides whether a part is seen.");
}
