using System.Collections.Generic;

namespace Fili.Theme.MudAvalonia.Gallery.Views;

/// <summary>
/// One row of the data grid's last sample: a selection column, and titles longer than their column,
/// which is how an app's grid looks and the periodic table's never does.
/// </summary>
public sealed record GalleryTask(bool Done, string Title, string Owner, string Due)
{
    public static IReadOnlyList<GalleryTask> Some { get; } =
    [
        new(true, "Reconcile the March statement against the bank's export", "Accounts", "2 Apr"),
        new(false, "Review the recurring transfers before the quarter closes", "Budgets", "14 Apr"),
        new(false, "Rename the duplicated categories", "Settings", "30 Apr"),
        new(true, "Archive last year's attachments", "Files", "—"),
    ];
}
