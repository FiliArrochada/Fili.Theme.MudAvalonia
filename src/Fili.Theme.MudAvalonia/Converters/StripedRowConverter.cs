using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// Whether a data grid row is one MudDataGrid's <c>Striped</c> paints: true for row index 0, 2, 4…
/// </summary>
///
/// <remarks>
/// <para>
/// `.mud-table-striped` paints <c>.mud-table-row:nth-of-type(odd)</c>: the first, third and fifth
/// rows, counting from one - so the even ones counting from zero, which is what
/// <c>DataGridRow.Index</c> does. Avalonia 12's DataGrid no longer has an alternating row
/// background, its rows sit in a recycling panel whose child order is not their display order, so
/// <c>:nth-child</c> cannot find them either, and a binding cannot take a remainder. Hence this.
/// </para>
/// <para>
/// Internal, like <see cref="FactorConverter"/>: it exists because a template needs it, not
/// because a consumer does.
/// </para>
/// </remarks>
internal sealed class StripedRowConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int index && index >= 0 && index % 2 == 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("StripedRowConverter is one-way: a stripe never moves a row.");
}
