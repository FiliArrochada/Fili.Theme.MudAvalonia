using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Optional;

/// <summary>
/// Which column of a DataGrid is its last visible one, so a cell or header in it can be styled:
/// <c>.mud-table-dense .mud-table-cell:last-child</c> pads 16px on the right where the others pad 24.
/// </summary>
/// <remarks>
/// <para>
/// A cell cannot tell it is last: the DataGrid knows (it hides the last cell's right grid line
/// from code), but says so through no pseudo-class or public property, and a cell's column is
/// internal. The grid's theme sets <see cref="TracksProperty"/>, which keeps
/// <see cref="LastProperty"/> on the grid current as columns are added, removed, reordered,
/// hidden or shown; each cell and header binds <see cref="IsLastProperty"/> to it through
/// <see cref="IsLastConverter"/>, which finds the element's column with the public
/// <c>DataGridColumn.GetColumnContainingElement</c>. A cell keeps its column for life, so the
/// binding needs to move only when the last column does.
/// </para>
/// <para>
/// Names the DataGrid's types, so it lives in <c>Themes/Optional/DataGrid/</c> and is reached only
/// from the files there (<c>OptionalBoundaryTests</c>). Internal, like the converters: it exists
/// because a template needs it, not because a consumer does.
/// </para>
/// </remarks>
internal sealed class DataGridLastColumn
{
    private DataGridLastColumn()
    {
    }

    /// <summary>Set by the grid's theme: keep <see cref="LastProperty"/> current.</summary>
    public static readonly AttachedProperty<bool> TracksProperty =
        AvaloniaProperty.RegisterAttached<DataGridLastColumn, DataGrid, bool>("Tracks");

    /// <summary>The grid's last visible column, in display order; null when it has none.</summary>
    public static readonly AttachedProperty<DataGridColumn?> LastProperty =
        AvaloniaProperty.RegisterAttached<DataGridLastColumn, DataGrid, DataGridColumn?>("Last");

    /// <summary>On a cell or column header: whether it is in its grid's last visible column.</summary>
    public static readonly AttachedProperty<bool> IsLastProperty =
        AvaloniaProperty.RegisterAttached<DataGridLastColumn, Control, bool>("IsLast");

    public static bool GetTracks(DataGrid grid) => grid.GetValue(TracksProperty);

    public static void SetTracks(DataGrid grid, bool value) => grid.SetValue(TracksProperty, value);

    public static DataGridColumn? GetLast(DataGrid grid) => grid.GetValue(LastProperty);

    public static bool GetIsLast(Control element) => element.GetValue(IsLastProperty);

    public static void SetIsLast(Control element, bool value) => element.SetValue(IsLastProperty, value);

    /// <summary>[the element itself, its grid's <see cref="LastProperty"/>] to whether they match.</summary>
    public static IMultiValueConverter IsLastConverter { get; } = new LastColumnConverter();

    private static readonly AttachedProperty<Tracker?> TrackerProperty =
        AvaloniaProperty.RegisterAttached<DataGridLastColumn, DataGrid, Tracker?>("Tracker");

    static DataGridLastColumn()
    {
        TracksProperty.Changed.AddClassHandler<DataGrid>((grid, e) =>
        {
            grid.GetValue(TrackerProperty)?.Dispose();
            grid.SetValue(TrackerProperty, e.GetNewValue<bool>() ? new Tracker(grid) : null);
        });
    }

    private sealed class Tracker : IDisposable
    {
        private readonly DataGrid _grid;
        private readonly Dictionary<DataGridColumn, IDisposable> _visibility = [];

        public Tracker(DataGrid grid)
        {
            _grid = grid;
            _grid.Columns.CollectionChanged += OnColumnsChanged;
            _grid.ColumnDisplayIndexChanged += OnDisplayIndexChanged;
            foreach (var column in _grid.Columns)
            {
                Watch(column);
            }

            Update();
        }

        public void Dispose()
        {
            _grid.Columns.CollectionChanged -= OnColumnsChanged;
            _grid.ColumnDisplayIndexChanged -= OnDisplayIndexChanged;
            foreach (var subscription in _visibility.Values)
            {
                subscription.Dispose();
            }

            _visibility.Clear();
            _grid.ClearValue(LastProperty);
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var column in _visibility.Keys.Except(_grid.Columns).ToList())
            {
                _visibility[column].Dispose();
                _visibility.Remove(column);
            }

            foreach (var column in _grid.Columns)
            {
                Watch(column);
            }

            Update();
        }

        private void OnDisplayIndexChanged(object? sender, DataGridColumnEventArgs e) => Update();

        private void Watch(DataGridColumn column)
        {
            if (!_visibility.ContainsKey(column))
            {
                _visibility[column] = column.GetObservable(DataGridColumn.IsVisibleProperty)
                    .Subscribe(new Observer(Update));
            }
        }

        private void Update() =>
            _grid.SetValue(LastProperty, _grid.Columns.Where(c => c.IsVisible).MaxBy(c => c.DisplayIndex));
    }

    private sealed class Observer(Action changed) : IObserver<bool>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(bool value) => changed();
    }

    private sealed class LastColumnConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
            values.Count == 2
            && values[0] is Control element
            && values[1] is DataGridColumn last
            && DataGridColumn.GetColumnContainingElement(element) == last;
    }
}
