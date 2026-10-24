using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Tracks how close this theme is to standing on its own.
///
/// <para>
/// The question this answers: can the substrate be dropped? Every mature Avalonia theme —
/// Semi.Avalonia, Material.Avalonia, Classic.Avalonia — ships a complete ControlTheme set and
/// replaces FluentTheme outright, because layering means inheriting another design system's
/// shapes and paying a permanent tax in <c>:not()</c> guards and version churn.
/// </para>
///
/// <para>
/// The catch is the failure mode. Avalonia has NO implicit default theme, so a control with no
/// ControlTheme in scope does not look wrong — it has no template, measures to nothing and
/// renders NOTHING. Going standalone early does not degrade the app, it deletes controls from
/// it. That makes this a coverage threshold to reach, not a switch to flip, which is what these
/// tests measure.
/// </para>
/// </summary>
public class StandaloneReadinessTests
{
    /// <summary>
    /// Every ControlTheme this package ships. Adding one means adding it here, which keeps the
    /// coverage number honest and makes the list double as an inventory.
    /// </summary>
    internal static readonly string[] ThemeKeys =
    [
        "FiliFilledButton", "FiliTextButton", "FiliOutlinedButton",
        "FiliStandardTextBox", "FiliFilledTextBox", "FiliOutlinedTextBox",
        "FiliCheckBox", "FiliRadioButton", "FiliToggleSwitch",
        "FiliToggleButton", "FiliRepeatButton", "FiliHyperlinkButton",
        "FiliSplitButton", "FiliSplitButtonHalf", "FiliDropDownButton", "FiliChip", "FiliStaticChip", "FiliFab",
        "FiliPathIcon",
        "FiliComboBox", "FiliFilledComboBox", "FiliOutlinedComboBox", "FiliComboBoxItem",
        "FiliListBox", "FiliListBoxItem", "FiliListItemButton",
        "FiliTreeView", "FiliTreeViewItem", "FiliTreeViewChevron",
        "FiliNavMenu", "FiliNavItem", "FiliNavGroupToggle",
        "FiliNumericUpDown", "FiliButtonSpinner", "FiliSpinnerButton",
        "FiliAutoCompleteBox",
        "FiliExpander", "FiliExpanderToggle", "FiliExpanderToggleNoIcon",
        "FiliLabel", "FiliDataValidationErrors",
        "FiliSlider", "FiliTabControl", "FiliTabItem",
        "FiliTabStrip", "FiliTabStripItem", "FiliPipsPager",
        "FiliProgressBar", "FiliProgressCircular", "FiliSeparator",
        "FiliScrollBar",
        "FiliMenu", "FiliMenuItem", "FiliTopLevelMenuItem", "FiliContextMenu",
        "FiliNotificationCard", "FiliWindowNotificationManager",
        "FiliToolTip", "FiliFlyoutPresenter", "FiliMenuFlyoutPresenter",

        // MudDrawer. Almost all values, set by styles in FiliTheme.axaml; the theme is the fork's
        // template with the part MudDrawer's elevation is drawn on.
        "FiliSplitView",

        // MudBadge. Opt-in through the `badge` class: an unclassed HeaderedContentControl keeps
        // the forked template, as an unclassed Button keeps the text one. A MudChip with nothing
        // to click is opt-in the same way, through `chip` on a ContentControl.
        "FiliBadge",

        // MudDataGrid, on the Avalonia.Controls.DataGrid package's controls.
        "FiliDataGrid", "FiliDataGridCell", "FiliDataGridColumnHeader", "FiliDataGridRowHeader",
        "FiliDataGridRow", "FiliDataGridRowGroupHeader", "FiliDataGridGroupExpander",

        // MudDatePicker on CalendarDatePicker, and the calendar inside it.
        "FiliCalendarDatePicker", "FiliCalendar", "FiliCalendarItem", "FiliCalendarDayButton",
        "FiliCalendarButton", "FiliCalendarNavButton", "FiliCalendarHeaderButton",

        // MudTimePicker on TimePicker: the field and the popover, with Avalonia's columns.
        "FiliTimePicker", "FiliTimePickerPresenter",
        "FiliDateTimePickerItem", "FiliDateTimePickerStepButton", "FiliDateTimePickerUpButton",
        "FiliDateTimePickerDownButton",

        // MudToolBar of MudIconButtons, with a MudMenu for the overflow.
        "FiliCommandBar", "FiliCommandBarButton", "FiliCommandBarToggleButton", "FiliCommandBarSeparator",

        // MudTable on TableView. Its ScrollViewer theme is left out: ScrollViewer is classified
        // below, and that theme is only the table's own header row round Avalonia's parts.
        "FiliTableView", "FiliTableViewColumnHeader", "FiliTableViewCell", "FiliTableViewRow",

        // MudBlazor's layout components on Avalonia's page types, and MudCarousel.
        "FiliContentPage", "FiliNavigationPage", "FiliTabbedPage", "FiliCarousel", "FiliCarouselPage",
    ];

    /// <summary>
    /// The other twenty-eight, each with the reason it is still wearing a forked Simple template.
    ///
    /// <para>
    /// This list exists because "twenty-eight still forked" reads like a backlog and is not one. Every
    /// entry below was checked against MudBlazor rather than assumed, and the honest finding is
    /// that the control work is CLOSED: what remains is either framework plumbing with no design
    /// opinion to express, a bespoke MudBlazor component that would be a rewrite rather than a
    /// restyle, or a control MudBlazor simply does not have. Leaving them forked is the fork
    /// earning its keep.
    /// </para>
    /// <para>
    /// It is asserted exhaustive and mutually exclusive, so an Avalonia version that adds a
    /// control type fails here asking to be classified, instead of quietly joining an anonymous
    /// pile and making the coverage number mean less every release.
    /// </para>
    /// </summary>
    internal static readonly (string Reason, string[] Types)[] Unthemed =
    [
        // Verified by rendering both: identical desired size and descendant count to the type
        // they derive from, because StyleKeyOverride points their theme lookup at it. A theme of
        // their own would be a second copy of one that already applies.
        ("Inherits a themed type's template through StyleKeyOverride",
            ["MaskedTextBox", "ToggleSplitButton"]),

        // No chrome of their own - a presenter and nothing else. Theming them would mean
        // inventing a look for something that has never had one.
        ("Base and container types with nothing to paint",
            [
                "HeaderedItemsControl",
                "HeaderedSelectingItemsControl", "ItemsControl", "PageNavigationHost",
                "SelectingItemsControl", "TemplatedControl", "Thumb",
                "TransitioningContentControl", "UserControl",
            ]),

        // Built by the framework, never by an app author, and mostly invisible when correct.
        ("Framework-instantiated surfaces",
            [
                "EmbeddableControlRoot", "NativeMenuBar", "OverlayPopupHost", "PopupRoot",
                "TextSelectionHandle", "Window", "WindowBase",
            ]),

        // DrawerPage keeps the fork's top bar and title; its drawer is the SplitView's MudDrawer,
        // by styles in FiliTheme.axaml rather than a theme of its own.
        ("Shell page styled over the fork", ["DrawerPage"]),

        // Structural rather than decorative: get a part name wrong and it lays out perfectly and
        // does not scroll. Written once while layering, deleted when the fork made it redundant.
        ("Deliberately deleted", ["ScrollViewer"]),

        // The spinner date picker. MudDatePicker is CalendarDatePicker; a date in three turning
        // columns has no MudBlazor counterpart to transcribe.
        ("A second date picker, MudDatePicker being CalendarDatePicker",
            ["DatePicker", "DatePickerPresenter"]),

        // Nothing in MudBlazor to transcribe. GroupBox's nearest relatives are MudCard and
        // MudPaper, which are different shapes; pull-to-refresh has no counterpart at all.
        ("No MudBlazor counterpart",
            ["GroupBox", "RefreshContainer", "RefreshVisualizer"]),

        // MudSplitter's divider is TRANSPARENT: _splitpanel.scss gives it a cursor, a 12px
        // invisible hit area and a focus outline, and separation comes from the two panels having
        // different backgrounds. Avalonia guarantees no such contrast, so transcribing it exactly
        // would produce an invisible control - the same failure this package has already shipped
        // twice in other costumes. Simple's visible hairline is kept deliberately.
        ("Counterpart exists but transcribing it would make the control invisible",
            ["GridSplitter"]),
    ];

    [Fact]
    public Task EveryDeclaredThemeResolves() => UiThread.RunAsync(() =>
    {
        foreach (var key in ThemeKeys)
        {
            Assert.True(
                Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value),
                $"{key} is listed in ThemeKeys but does not resolve.");

            Assert.IsType<ControlTheme>(value);
        }
    });

    /// <summary>
    /// Reports the gap to standalone, and fails if it moves without the list being updated.
    /// <para>
    /// The assertion is deliberately on the exact set rather than a count: a theme silently
    /// changing TargetType would keep the count and break the coverage claim.
    /// </para>
    /// </summary>
    [Fact]
    public Task CoverageMatchesTheDeclaredSet() => UiThread.RunAsync(() =>
    {
        var themed = ThemeKeys
            .Select(key =>
            {
                Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value);
                return ((ControlTheme)value!).TargetType!;
            })
            .Distinct()
            .OrderBy(t => t.Name)
            .ToList();

        Assert.Equal(
            [
                "AutoCompleteBox", "Button", "ButtonSpinner", "Calendar", "CalendarButton",
                "CalendarDatePicker", "CalendarDayButton", "CalendarItem", "Carousel",
                "CarouselPage", "CheckBox", "ComboBox", "ComboBoxItem", "CommandBar",
                "CommandBarButton", "CommandBarSeparator", "CommandBarToggleButton", "ContentControl", "ContentPage",
                "ContextMenu", "DataGrid", "DataGridCell", "DataGridColumnHeader",
                "DataGridRow", "DataGridRowGroupHeader", "DataGridRowHeader",
                "DataValidationErrors", "DropDownButton", "Expander",
                "FlyoutPresenter", "HeaderedContentControl", "HyperlinkButton", "Label", "ListBox", "ListBoxItem", "Menu",
                "MenuFlyoutPresenter", "MenuItem", "NavigationPage", "NotificationCard", "NumericUpDown",
                "PathIcon", "PipsPager", "ProgressBar", "RadioButton", "RepeatButton", "ScrollBar",
                "Separator", "Slider", "SplitButton", "SplitView", "TabbedPage", "TabControl", "TabItem",
                "TableView", "TableViewCell", "TableViewColumnHeader", "TableViewRow", "TabStrip",
                "TabStripItem",
                "TextBox", "TimePicker", "TimePickerPresenter", "ToggleButton", "ToggleSwitch",
                "ToolTip", "TreeView", "TreeViewItem",
                "WindowNotificationManager",
            ],
            themed.Select(t => t.Name).ToArray());

        // THE NUMBER THAT SAYS HOW MUCH OF THE FORK IS STILL DOING THE WORK. 69 of 95 templated
        // control types are hand-written here; the other 26 are still wearing the forked Simple
        // templates from Themes/Base. Pinned rather than merely reported, so it cannot drift
        // unnoticed and so an Avalonia version that adds controls surfaces as a failure here.
        var templated = TemplatedControlTypes();

        Assert.Equal(69, themed.Count);
        Assert.Equal(95, templated.Count);
    });

    /// <summary>
    /// The inventory is complete and says each type once.
    ///
    /// <para>
    /// This is what turns the coverage number from a claim into a fact. Without it, "69 of 95"
    /// only says how many themes exist; with it, every one of the other twenty-six has a stated reason
    /// for not being one, and a control type added by a future Avalonia fails here by name rather
    /// than by making 89 into 90.
    /// </para>
    /// </summary>
    [Fact]
    public Task EveryTemplatedTypeIsThemedOrClassified() => UiThread.RunAsync(() =>
    {
        var classified = Unthemed.SelectMany(group => group.Types).ToList();

        Assert.Equal(classified.Count, classified.Distinct().Count());

        var accounted = ThemedTypeNames().Concat(classified).OrderBy(n => n, StringComparer.Ordinal);
        var templated = TemplatedControlTypes()
            .Select(t => t.Name)
            .OrderBy(n => n, StringComparer.Ordinal);

        Assert.Equal(templated, accounted);
    });

    private static IEnumerable<string> ThemedTypeNames() =>
        ThemeKeys
            .Select(key =>
            {
                Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value);
                return ((ControlTheme)value!).TargetType!.Name;
            })
            .Distinct();

    /// <summary>
    /// Every public templated control in Avalonia.Controls and in the DataGrid package, the two
    /// control assemblies this theme depends on.
    /// </summary>
    private static List<Type> TemplatedControlTypes() =>
        new[] { typeof(Button).Assembly, typeof(DataGrid).Assembly }
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsPublic
                && !t.IsAbstract
                && !t.IsGenericTypeDefinition
                && typeof(TemplatedControl).IsAssignableFrom(t))
            .ToList();
}
