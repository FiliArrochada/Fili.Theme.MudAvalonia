using System.ComponentModel;
using Avalonia.Collections;
using Fili.Theme.MudAvalonia.Gallery.Views;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>
/// Views over the gallery's sample rows that XAML cannot build on its own: a data grid's sort and
/// grouping live on a <see cref="DataGridCollectionView"/>, which takes its source in its
/// constructor. Each read is a new view, so two grids never share a selection or a sort.
/// </summary>
public static class SampleData
{
    /// <summary>The first six elements, sorted by molar mass, largest first.</summary>
    public static DataGridCollectionView ElementsByMolarDescending
    {
        get
        {
            var view = new DataGridCollectionView(GalleryElement.FirstSix);
            view.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(GalleryElement.Molar), ListSortDirection.Descending));
            return view;
        }
    }

    /// <summary>The first six elements, grouped by group and then by position.</summary>
    public static DataGridCollectionView ElementsByGroupAndPosition
    {
        get
        {
            var view = new DataGridCollectionView(GalleryElement.FirstSix);
            view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(GalleryElement.Group)));
            view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(GalleryElement.Position)));
            return view;
        }
    }
}
