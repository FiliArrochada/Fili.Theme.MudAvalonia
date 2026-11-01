using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Optional;

/// <summary>
/// Whether a column header can sort, so its sort button takes room only then: MudDataGrid renders
/// <c>.sort-direction-icon</c> only on a sortable column, and a header that kept the button's 30px
/// for a column that can never sort lost that much of its title in a narrow column.
/// </summary>
/// <remarks>
/// A header says whether it sorts through no pseudo-class or public property: its column is
/// internal, and the grid's <c>CanUserSortColumns</c> and the column's <c>CanUserSort</c> both
/// decide. The header's template binds its sort button's visibility to [the header, its grid's
/// <c>CanUserSortColumns</c>] through <see cref="Converter"/>, which finds the column with the
/// public <c>DataGridColumn.GetColumnContainingElement</c>. Names the DataGrid's types, so it
/// lives in <c>Themes/Optional/DataGrid/</c> (<c>OptionalBoundaryTests</c>).
/// </remarks>
internal static class DataGridSortable
{
    /// <summary>[the header, its grid's <c>CanUserSortColumns</c>] to whether it can sort.</summary>
    public static IMultiValueConverter Converter { get; } = new SortableConverter();

    private sealed class SortableConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
            values.Count != 2
            || values[0] is not Control header
            || values[1] is not bool gridSorts
            || (gridSorts && DataGridColumn.GetColumnContainingElement(header)?.CanUserSort != false);
    }
}
