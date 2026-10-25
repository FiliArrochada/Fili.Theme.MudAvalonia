using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>How close one MudBlazor parameter or behaviour comes here.</summary>
public enum Parity
{
    /// <summary>The same, transcribed from MudBlazor's source.</summary>
    Same,

    /// <summary>Close: the same result another way, or nearly the same.</summary>
    Close,

    /// <summary>Not here.</summary>
    Missing,

    /// <summary>Not MudBlazor's: an option of Material's, named so it cannot be taken for one.</summary>
    Extra,
}

/// <summary>One row of a component's parity table.</summary>
public sealed class ParityRow
{
    /// <summary>The MudBlazor parameter or behaviour, as MudBlazor names it.</summary>
    public string? MudBlazor { get; set; }

    /// <summary>What it is here: a class, a property, a key.</summary>
    public string? Here { get; set; }

    public Parity Status { get; set; }

    public string? Note { get; set; }
}

/// <summary>
/// A component's MudBlazor parameters and what each is here, with how close it comes. The rows
/// are its items; DocsTheme.axaml draws them.
/// </summary>
public sealed class ParityTable : ItemsControl
{
}

/// <summary>Which status chip a parity row shows.</summary>
public static class ParityConverters
{
    public static readonly IValueConverter IsSame = new FuncValueConverter<Parity, bool>(p => p == Parity.Same);
    public static readonly IValueConverter IsClose = new FuncValueConverter<Parity, bool>(p => p == Parity.Close);
    public static readonly IValueConverter IsMissing = new FuncValueConverter<Parity, bool>(p => p == Parity.Missing);
    public static readonly IValueConverter IsExtra = new FuncValueConverter<Parity, bool>(p => p == Parity.Extra);
}
