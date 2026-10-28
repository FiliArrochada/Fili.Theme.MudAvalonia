using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia.Gallery.Docs.Custom;

/// <summary>
/// MudDataGrid's SelectColumn on a DataGrid: a CheckBox marked <c>IsRow</c> in a template
/// column's cell is its row's selection, and one marked <c>IsAll</c> in the column's header is
/// every row's: checked when all are selected, indeterminate when some are, and a click selects
/// all, or none once all are. The grid's own selection is the state, so a row clicked, Ctrl+A
/// and the boxes all agree. Give the grid <c>SelectionMode="Extended"</c>, as MudDataGrid's
/// MultiSelection.
/// </summary>
public sealed class RowSelect
{
    private RowSelect()
    {
    }

    public static readonly AttachedProperty<bool> IsRowProperty =
        AvaloniaProperty.RegisterAttached<RowSelect, CheckBox, bool>("IsRow");

    public static readonly AttachedProperty<bool> IsAllProperty =
        AvaloniaProperty.RegisterAttached<RowSelect, CheckBox, bool>("IsAll");

    static RowSelect()
    {
        IsRowProperty.Changed.AddClassHandler<CheckBox>((box, e) => Follow(box, e.GetNewValue<bool>(), all: false));
        IsAllProperty.Changed.AddClassHandler<CheckBox>((box, e) => Follow(box, e.GetNewValue<bool>(), all: true));
    }

    public static bool GetIsRow(CheckBox box) => box.GetValue(IsRowProperty);

    public static void SetIsRow(CheckBox box, bool value) => box.SetValue(IsRowProperty, value);

    public static bool GetIsAll(CheckBox box) => box.GetValue(IsAllProperty);

    public static void SetIsAll(CheckBox box, bool value) => box.SetValue(IsAllProperty, value);

    /// <summary>Keeps <paramref name="box"/> in step with the grid it sits in, and the grid with its clicks.</summary>
    private static void Follow(CheckBox box, bool on, bool all)
    {
        if (!on)
        {
            return;
        }

        DataGrid? grid = null;

        void Sync()
        {
            if (grid is not null)
            {
                box.IsChecked = all ? AllState(grid) : grid.SelectedItems.Contains(box.DataContext);
            }
        }

        void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => Sync();

        void Attach()
        {
            grid = box.FindAncestorOfType<DataGrid>();
            if (grid is not null)
            {
                grid.SelectionChanged += OnSelectionChanged;
                Sync();
            }
        }

        box.AttachedToVisualTree += (_, _) => Attach();
        box.DetachedFromVisualTree += (_, _) =>
        {
            if (grid is not null)
            {
                grid.SelectionChanged -= OnSelectionChanged;
                grid = null;
            }
        };

        // A recycled row's box shows its new item's state.
        box.DataContextChanged += (_, _) => Sync();

        box.Click += (_, _) =>
        {
            if (grid is null)
            {
                return;
            }

            if (all)
            {
                if (AllState(grid) == true)
                {
                    grid.SelectedItems.Clear();
                }
                else
                {
                    grid.SelectAll();
                }
            }
            else if (box.IsChecked == true)
            {
                grid.SelectedItems.Add(box.DataContext);
            }
            else
            {
                grid.SelectedItems.Remove(box.DataContext);
            }

            Sync();
        };

        if (box.IsAttachedToVisualTree())
        {
            Attach();
        }
    }

    /// <summary>MudDataGrid's IsAllSelected: true for all, false for none, null for some.</summary>
    private static bool? AllState(DataGrid grid)
    {
        var selected = grid.SelectedItems.Count;
        var total = grid.ItemsSource?.Cast<object>().Count() ?? 0;
        return selected == 0 ? false : selected >= total ? true : null;
    }
}
