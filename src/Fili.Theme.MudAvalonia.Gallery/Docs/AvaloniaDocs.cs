namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>
/// Avalonia's own page for each control a docs page here is drawn on. The paths are from
/// docs.avaloniaui.net's sitemap.xml, not guessed: its control guide where it has one, its API
/// reference where it has only that. A page names its controls in its header's Avalonia.
/// </summary>
public static class AvaloniaDocs
{
    public static IReadOnlyDictionary<string, string> Paths { get; } = new Dictionary<string, string>
    {
        ["AutoCompleteBox"] = "controls/input/text-input/autocompletebox",
        ["Border"] = "controls/layout/containers/border",
        ["Button"] = "controls/input/buttons/button",
        ["Calendar"] = "controls/input/date-and-time/calendar",
        ["CalendarDatePicker"] = "controls/input/date-and-time/calendardatepicker",
        ["Carousel"] = "controls/data-display/collections/carousel",
        ["CarouselPage"] = "controls/navigation/carouselpage",
        ["CheckBox"] = "controls/input/selectors/checkbox",
        ["ComboBox"] = "controls/input/selectors/combobox",
        ["CommandBar"] = "controls/navigation/commandbar",
        ["ContentPage"] = "controls/navigation/contentpage",
        ["ContextMenu"] = "controls/menus/contextmenu",
        ["DataGrid"] = "controls/data-display/structured-data/datagrid",
        ["DatePicker"] = "controls/input/date-and-time/datepicker",
        ["DrawerPage"] = "controls/navigation/drawerpage",
        ["DropDownButton"] = "api/avalonia/controls/dropdownbutton",
        ["Expander"] = "controls/layout/containers/expander",
        ["Flyout"] = "controls/layout/containers/flyout",
        ["Grid"] = "controls/layout/panels/grid",
        ["GridSplitter"] = "controls/layout/panels/gridsplitter",
        ["HeaderedContentControl"] = "api/avalonia/controls/primitives/headeredcontentcontrol",
        ["HyperlinkButton"] = "controls/input/buttons/hyperlinkbutton",
        ["Image"] = "controls/media/image",
        ["ListBox"] = "controls/data-display/collections/listbox",
        ["MaskedTextBox"] = "controls/input/text-input/maskedtextbox",
        ["Menu"] = "controls/menus/menu",
        ["MenuFlyout"] = "controls/menus/menuflyout",
        ["NavigationPage"] = "controls/navigation/navigationpage",
        ["Notification"] = "controls/feedback/notification",
        ["NumericUpDown"] = "controls/input/selectors/numericupdown",
        ["PathIcon"] = "controls/media/pathicon",
        ["PipsPager"] = "controls/layout/containers/pipspager",
        ["ProgressBar"] = "controls/feedback/progressbar",
        ["RadioButton"] = "controls/input/buttons/radiobutton",
        ["ScrollBar"] = "controls/primitives/scrollbar",
        ["ScrollViewer"] = "controls/layout/containers/scrollviewer",
        ["Separator"] = "controls/menus/separator",
        ["Slider"] = "controls/input/selectors/slider",
        ["SplitButton"] = "controls/input/buttons/splitbutton",
        ["SplitView"] = "controls/layout/containers/splitview",
        ["StackPanel"] = "controls/layout/panels/stackpanel",
        ["TabbedPage"] = "controls/navigation/tabbedpage",
        ["TabControl"] = "controls/navigation/tabcontrol",
        ["TableView"] = "controls/data-display/structured-data/tableview",
        ["TabStrip"] = "controls/navigation/tabstrip",
        ["TextBlock"] = "controls/data-display/text-display/textblock",
        ["TextBox"] = "controls/input/text-input/textbox",
        ["TimePicker"] = "controls/input/date-and-time/timepicker",
        ["ToggleButton"] = "controls/input/buttons/togglebutton",
        ["ToggleSwitch"] = "controls/input/selectors/toggleswitch",
        ["ToolTip"] = "controls/feedback/tooltip",
        ["TreeView"] = "controls/data-display/structured-data/treeview",
    };

    public static Uri Uri(string control) => new($"https://docs.avaloniaui.net/{Paths[control]}");

    /// <summary>The names in a header's Avalonia, "TabControl, TabStrip", in order.</summary>
    public static IReadOnlyList<string> Names(string? controls) =>
        string.IsNullOrWhiteSpace(controls)
            ? []
            : controls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
