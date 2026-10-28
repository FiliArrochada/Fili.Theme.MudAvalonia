using System.Collections.ObjectModel;

namespace Fili.Theme.MudAvalonia.Gallery.Docs.Custom;

/// <summary>
/// Items the user can close: MudChip's OnClose and MudDynamicTabs' CloseTab and AddTab, which a
/// theme cannot give, as closing is the app's. Bind a list's ItemsSource to it, and each item's
/// close button's Command to the item's <see cref="ClosableItem.Close"/>.
/// </summary>
public sealed class ClosableList : ObservableCollection<ClosableItem>
{
    private readonly string[] _headers;
    private int _added;

    public ClosableList(params string[] headers)
    {
        _headers = headers;
        Reset();
    }

    /// <summary>A chip set's sample. Each read is a new list, so two examples never share one.</summary>
    public static ClosableList Chips => new("Apple", "Banana", "Cherry", "Date", "Elderberry");

    /// <summary>A tab bar's sample.</summary>
    public static ClosableList Tabs => new("Tab One", "Tab Two", "Tab Three");

    /// <summary>MudDynamicTabs' AddTab: a new item at the end.</summary>
    public void AddNew() => Add(new ClosableItem(this, $"New tab {++_added}"));

    /// <summary>Every item back, as the list began.</summary>
    public void Reset()
    {
        Clear();
        _added = 0;
        foreach (var header in _headers)
        {
            Add(new ClosableItem(this, header));
        }
    }
}

/// <summary>One item of a <see cref="ClosableList"/>, which removes itself when closed.</summary>
public sealed class ClosableItem(ClosableList owner, string header)
{
    public string Header { get; } = header;

    public string Content => $"The content of {Header}.";

    public void Close() => owner.Remove(this);
}
