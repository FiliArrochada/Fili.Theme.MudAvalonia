namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>One MudBlazor component and what it is here.</summary>
/// <param name="Name">MudBlazor's name for it, without the Mud.</param>
/// <param name="Here">The Avalonia control and class that draw it, or why it is not here.</param>
/// <param name="Status">How close it comes, as a whole.</param>
/// <param name="MudBlazorPath">Its page on mudblazor.com, from the @page of MudBlazor's own docs.</param>
/// <param name="Slug">Its page here, once written.</param>
public sealed record ComponentEntry(string Name, string Here, Parity Status, string MudBlazorPath, string? Slug = null)
{
    public Uri MudBlazorUri => new($"https://mudblazor.com/{MudBlazorPath}");

    public bool HasPage => Slug is not null && DocCatalog.Find(Slug) is not null;
}

/// <summary>
/// Every MudBlazor component, in its docs' order, with what it is here. The paths are the @page
/// routes of MudBlazor's own docs (src/MudBlazor.Docs/Pages/Components/*/*Page.razor), not
/// guessed: mudblazor.com answers any path, so a wrong one would only show as a blank page.
/// </summary>
public static class ComponentIndex
{
    public static IReadOnlyList<ComponentEntry> All { get; } =
    [
        new("Button", "Button", Parity.Same, "components/button", "button"),
        new("ButtonGroup", "button-group on a StackPanel of buttons", Parity.Same, "components/buttongroup", "button-group"),
        new("IconButton", "icon on a Button", Parity.Same, "components/iconbutton", "icon-button"),
        new("ToggleIconButton", "icon on a ToggleButton", Parity.Same, "components/toggleiconbutton", "toggle-icon-button"),
        new("Fab", "fab on a Button", Parity.Same, "components/buttonfab", "fab"),
        new("FabMenu", "not here: a control with behaviour of its own", Parity.Missing, "components/buttonfabmenu"),

        new("TextField", "TextBox", Parity.Same, "components/textfield", "text-field"),
        new("Select", "ComboBox", Parity.Same, "components/select", "select"),
        new("Autocomplete", "AutoCompleteBox", Parity.Same, "components/autocomplete", "autocomplete"),
        new("NumericField", "NumericUpDown", Parity.Same, "components/numericfield", "numeric-field"),
        new("CheckBox", "CheckBox", Parity.Same, "components/checkbox", "checkbox"),
        new("Radio", "RadioButton", Parity.Same, "components/radio", "radio"),
        new("Switch", "ToggleSwitch", Parity.Same, "components/switch", "switch"),
        new("Slider", "Slider", Parity.Same, "components/slider", "slider"),
        new("ToggleGroup", "ToggleButton", Parity.Same, "components/togglegroup", "toggle-group"),
        new("Field", "not here: a container for a field's look", Parity.Missing, "components/field"),
        new("FileUpload", "not here: a control with behaviour of its own", Parity.Missing, "components/fileupload"),
        new("Form", "not here: validation, not a look", Parity.Missing, "components/form"),

        new("DatePicker", "CalendarDatePicker", Parity.Close, "components/datepicker", "date-picker"),
        new("DateRangePicker", "not here: a control of its own", Parity.Missing, "components/daterangepicker"),
        new("TimePicker", "TimePicker", Parity.Close, "components/timepicker", "time-picker"),
        new("ColorPicker", "not here: a control of its own", Parity.Missing, "components/colorpicker"),

        new("AppBar", "appbar on a Border", Parity.Same, "components/appbar", "app-bar"),
        new("Drawer", "SplitView", Parity.Same, "components/drawer", "drawer"),
        new("NavMenu", "nav on a TreeView", Parity.Same, "components/navmenu", "nav-menu"),
        new("Tabs", "TabControl, TabStrip", Parity.Same, "components/tabs", "tabs"),
        new("Menu", "Menu, ContextMenu, MenuFlyout", Parity.Same, "components/menu", "menu"),
        new("ToolBar", "toolbar on a Border; CommandBar", Parity.Same, "components/toolbar", "tool-bar"),
        new("Breadcrumbs", "not here: a control of its own", Parity.Missing, "components/breadcrumbs"),
        new("Pagination", "not here: a control of its own", Parity.Missing, "components/pagination"),
        new("Link", "HyperlinkButton", Parity.Same, "components/link", "link"),

        new("Typography", "type classes on a TextBlock", Parity.Same, "components/typography", "typography"),
        new("Paper", "paper on a Border", Parity.Same, "components/paper", "paper"),
        new("Card", "card on a Border", Parity.Same, "components/card", "card"),
        new("Divider", "Separator", Parity.Same, "components/divider", "divider"),
        new("ExpansionPanels", "Expander; expansion-panels on a StackPanel", Parity.Same, "components/expansionpanels", "expansion-panels"),
        new("Collapse", "Expander's grow", Parity.Same, "components/collapse"),
        new("List", "ListBox; list-item on a Button", Parity.Same, "components/list", "list"),
        new("TreeView", "TreeView", Parity.Same, "components/treeview", "tree-view"),
        new("Table", "TableView", Parity.Same, "components/table", "table"),
        new("SimpleTable", "TableView", Parity.Same, "components/simpletable", "table"),
        new("DataGrid", "DataGrid, with Avalonia.Controls.DataGrid", Parity.Same, "components/datagrid", "data-grid"),
        new("Carousel", "Carousel, CarouselPage", Parity.Same, "components/carousel", "carousel"),
        new("SplitPanel", "GridSplitter", Parity.Close, "components/splitpanel"),

        new("Alert", "alert on a Border", Parity.Same, "components/alert", "alert"),
        new("Snackbar", "WindowNotificationManager", Parity.Same, "components/snackbar", "snackbar"),
        new("Tooltip", "ToolTip", Parity.Same, "components/tooltip", "tooltip"),
        new("Progress", "ProgressBar; circular on a ProgressBar", Parity.Same, "components/progress", "progress"),
        new("Skeleton", "skeleton on a Border", Parity.Same, "components/skeleton", "skeleton"),
        new("Overlay", "overlay on a Border", Parity.Same, "components/overlay", "overlay"),
        new("Popover", "Flyout, ContextMenu, popups", Parity.Close, "components/popover", "popover"),
        new("Dialog", "not here: a dialog service, not a look", Parity.Missing, "components/dialog"),
        new("MessageBox", "not here: a dialog service, not a look", Parity.Missing, "components/messagebox"),

        new("Avatar", "avatar on a Border", Parity.Same, "components/avatar", "avatar"),
        new("Badge", "badge on a HeaderedContentControl", Parity.Same, "components/badge", "badge"),
        new("Chips", "chip on a Button, ToggleButton or ContentControl", Parity.Same, "components/chips", "chips"),
        new("ChipSet", "a panel of chips; selection is the app's", Parity.Close, "components/chipset"),
        new("Icons", "PathIcon", Parity.Same, "components/icons", "icons"),
        new("Image", "Image", Parity.Same, "components/image", "image"),
        new("Rating", "not here: a control of its own", Parity.Missing, "components/rating"),
        new("Timeline", "not here: a control of its own", Parity.Missing, "components/timeline"),
        new("Stepper", "not here: a control of its own", Parity.Missing, "components/stepper"),
        new("Highlighter", "not here: text behaviour, not a look", Parity.Missing, "components/highlighter"),
        new("ScrollToTop", "not here: behaviour, not a look", Parity.Missing, "components/scrolltotop"),
        new("DropZone", "not here: a control of its own", Parity.Missing, "components/dropzone"),
        new("Charts", "not here: a control of its own", Parity.Missing, "components/charts"),

        new("Container", "not here: layout, which Avalonia's panels do", Parity.Missing, "components/container"),
        new("Grid", "not here: layout, which Avalonia's Grid does", Parity.Missing, "components/grid"),
        new("Stack", "not here: layout, which StackPanel does", Parity.Missing, "components/stack"),
        new("Spacer", "not here: layout", Parity.Missing, "components/spacer"),
        new("Hidden", "not here: not visual", Parity.Missing, "components/hidden"),
        new("BreakpointProvider", "not here: not visual", Parity.Missing, "components/breakpointprovider"),
        new("FocusTrap", "not here: not visual", Parity.Missing, "components/focustrap"),
        new("Element", "not here: not visual", Parity.Missing, "components/element"),
        new("SwipeArea", "not here: not visual", Parity.Missing, "components/swipearea"),
        new("Hotkey", "not here: not visual", Parity.Missing, "components/hotkey"),
        new("ExitPrompt", "not here: not visual", Parity.Missing, "components/exitprompt"),
    ];
}
