using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Optional;

/// <summary>
/// [a group header's group, its grid's ItemsSource] to the space before a nested group header's
/// expander: <c>.mud-row-group-indented-{level}</c>.
/// </summary>
/// <remarks>
/// <para>
/// MudDataGrid sets a nested group header's left padding to <c>level * 48 - 48</c>px, for levels 2
/// to 5 (a deeper one stays at 5's), and leaves the data rows alone. The top level keeps the
/// cell's 16px. The theme already pads 16px before the expander, so this is the rest: 32px at
/// level 2, 80 at 3, 128 at 4, 176 from 5 on. Dense changes none of it - the rule is
/// <c>!important</c>.
/// </para>
/// <para>
/// Avalonia's own <c>SublevelIndent</c> cannot do this: it moves every row under the group as
/// well. And a header's level is internal, so it is worked out from the public group tree: a
/// group's depth is how many levels the top groups go below it, less how many it does.
/// </para>
/// <para>
/// Names the DataGrid's types, so it lives in <c>Themes/Optional/DataGrid/</c> (see
/// <c>DataGridLastColumn</c>).
/// </para>
/// </remarks>
internal sealed class GroupIndentConverter : IMultiValueConverter
{
    private const double Step = 48;
    private const double CellPadding = 16;
    private const int DeepestLevel = 5;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2
            || values[0] is not DataGridCollectionViewGroup group
            || values[1] is not DataGridCollectionView { Groups: [DataGridCollectionViewGroup top, ..] })
        {
            return 0d;
        }

        var level = Math.Min(Depth(top) - Depth(group) + 1, DeepestLevel);
        return level < 2 ? 0d : level * Step - Step - CellPadding;
    }

    /// <summary>How many group levels lie below this one: 0 for a group of rows.</summary>
    private static int Depth(DataGridCollectionViewGroup group)
    {
        var depth = 0;
        while (!group.IsBottomLevel && group.Items is [DataGridCollectionViewGroup child, ..])
        {
            group = child;
            depth++;
        }

        return depth;
    }
}
