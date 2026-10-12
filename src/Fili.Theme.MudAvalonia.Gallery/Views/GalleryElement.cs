using System.Collections.Generic;

namespace Fili.Theme.MudAvalonia.Gallery.Views;

/// <summary>
/// One row of the data grid samples: the periodic table MudBlazor's own data grid docs use, so
/// the two can be laid side by side.
/// </summary>
public sealed record GalleryElement(int Number, string Sign, string Name, int Position, double Molar, string Group)
{
    public static IReadOnlyList<GalleryElement> FirstSix { get; } =
    [
        new(1, "H", "Hydrogen", 1, 1.00794, "Nonmetal"),
        new(2, "He", "Helium", 18, 4.002602, "Noble gas"),
        new(3, "Li", "Lithium", 1, 6.941, "Alkali metal"),
        new(4, "Be", "Beryllium", 2, 9.012182, "Alkaline earth metal"),
        new(5, "B", "Boron", 13, 10.811, "Metalloid"),
        new(6, "C", "Carbon", 14, 12.0107, "Nonmetal"),
    ];
}
