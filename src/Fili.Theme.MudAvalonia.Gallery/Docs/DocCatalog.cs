using Avalonia.Controls;
using Fili.Theme.MudAvalonia.Gallery.Docs.Pages;
using Fili.Theme.MudAvalonia.Gallery.Views;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>One page of the docs: where it sits in the drawer, and how to build it.</summary>
/// <param name="Section">The drawer group it is listed under.</param>
/// <param name="Title">Its link text, and the page's heading.</param>
/// <param name="Slug">A stable name for it, used to open a page by name.</param>
/// <param name="Create">Builds the page; called the first time it is opened.</param>
/// <param name="Scrolls">Whether the page scrolls itself; the shell scrolls the others.</param>
public sealed record DocPage(string Section, string Title, string Slug, Func<Control> Create, bool Scrolls = false);

/// <summary>
/// Every page of the docs, in drawer order, grouped by section. A component page is listed here
/// once it exists; the Parity page lists every MudBlazor component, written or not.
/// </summary>
public static class DocCatalog
{
    public const string GettingStarted = "Getting started";
    public const string Customization = "Customization";
    public const string Buttons = "Buttons";
    public const string Inputs = "Form & inputs";
    public const string Navigation = "Navigation";
    public const string Surfaces = "Surfaces & lists";
    public const string Feedback = "Feedback";
    public const string Showcase = "Showcase";

    public static IReadOnlyList<DocPage> Pages { get; } =
    [
        new(GettingStarted, "Overview", "overview", () => new OverviewPage()),
        new(GettingStarted, "Installation", "installation", () => new InstallationPage()),
        new(GettingStarted, "MudBlazor parity", "parity", () => new ParityPage()),

        new(Customization, "Theming", "theming", () => new ThemingPage()),
        new(Customization, "Palette", "palette", () => new PaletteView(), Scrolls: true),
        new(Customization, "Typography", "typography", () => new TypographyView(), Scrolls: true),
        new(Customization, "Elevation", "elevation", () => new ElevationView(), Scrolls: true),
        new(Customization, "Reduced motion", "reduced-motion", () => new ReducedMotionPage()),

        new(Buttons, "Button", "button", () => new ButtonPage()),
        new(Buttons, "Button group", "button-group", () => new ButtonGroupPage()),
        new(Buttons, "Icon button", "icon-button", () => new IconButtonPage()),
        new(Buttons, "Toggle icon button", "toggle-icon-button", () => new ToggleIconButtonPage()),
        new(Buttons, "FAB", "fab", () => new FabPage()),
        new(Buttons, "Chips", "chips", () => new ChipsPage()),

        new(Inputs, "Text field", "text-field", () => new TextFieldPage()),
        new(Inputs, "Select", "select", () => new SelectPage()),
        new(Inputs, "Autocomplete", "autocomplete", () => new AutocompletePage()),
        new(Inputs, "Numeric field", "numeric-field", () => new NumericFieldPage()),
        new(Inputs, "Check box", "checkbox", () => new CheckBoxPage()),
        new(Inputs, "Radio", "radio", () => new RadioPage()),
        new(Inputs, "Switch", "switch", () => new SwitchPage()),
        new(Inputs, "Slider", "slider", () => new SliderPage()),
        new(Inputs, "Toggle group", "toggle-group", () => new ToggleGroupPage()),

        new(Navigation, "App bar", "app-bar", () => new AppBarPage()),
        new(Navigation, "Drawer", "drawer", () => new DrawerDocPage()),
        new(Navigation, "Nav menu", "nav-menu", () => new NavMenuPage()),
        new(Navigation, "Tabs", "tabs", () => new TabsPage()),
        new(Navigation, "Menu", "menu", () => new MenuPage()),
        new(Navigation, "Tool bar", "tool-bar", () => new ToolBarPage()),
        new(Navigation, "Link", "link", () => new LinkPage()),

        new(Surfaces, "Paper", "paper", () => new PaperPage()),
        new(Surfaces, "Card", "card", () => new CardPage()),
        new(Surfaces, "Divider", "divider", () => new DividerPage()),
        new(Surfaces, "Expansion panels", "expansion-panels", () => new ExpansionPanelsPage()),
        new(Surfaces, "List", "list", () => new ListPage()),
        new(Surfaces, "Tree view", "tree-view", () => new TreeViewPage()),
        new(Surfaces, "Carousel", "carousel", () => new CarouselDocPage()),

        new(Feedback, "Alert", "alert", () => new AlertPage()),
        new(Feedback, "Snackbar", "snackbar", () => new SnackbarPage()),
        new(Feedback, "Tooltip", "tooltip", () => new TooltipPage()),
        new(Feedback, "Popover", "popover", () => new PopoverPage()),
        new(Feedback, "Progress", "progress", () => new ProgressPage()),
        new(Feedback, "Skeleton", "skeleton", () => new SkeletonPage()),
        new(Feedback, "Overlay", "overlay", () => new OverlayPage()),

        new(Showcase, "Control states", "control-states", () => new ControlStatesView(), Scrolls: true),
        new(Showcase, "Custom styles", "custom-styles", () => new CustomStylesView(), Scrolls: true),
        new(Showcase, "Sample screen", "sample-screen", () => new SampleScreenView(), Scrolls: true),
    ];

    public static IEnumerable<string> Sections => Pages.Select(p => p.Section).Distinct();

    public static DocPage? Find(string slug) => Pages.FirstOrDefault(p => p.Slug == slug);
}
