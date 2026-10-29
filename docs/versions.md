# Versions

What changed in each release of `Fili.Theme.MudAvalonia`, newest first. The package is on
[nuget.org](https://www.nuget.org/packages/Fili.MudAvalonia.Theme) under its original id,
`Fili.MudAvalonia.Theme`; install steps are in the [README](../README.md#install).

| Version | |
|---|---|
| *(unreleased)* | MudInput's `Clearable`, which was a button the app wrote and wired: `clearable` on a text field, an autocomplete or a number field shows MudBlazor's small clear button while the field holds text that is not blank and is neither disabled nor read-only, before an end adornment and out of the Tab order. Nothing changes without the class. And MudDataGrid's `Loading` and `LoadingProgressColor`, which the app laid over the grid itself: `loading` shows an indeterminate 4px bar in its own row under the header, info, or in the grid's colour class. And MudListSubheader, which was a TextBlock the app styled: `subheader` on a `TextBlock` is its action-default, medium 14px text, 8px above and 20px below, with `inset` and `no-gutters` for Inset and Gutters="false". And MudChipSet's and MudToggleGroup's `CheckMark`, which were an icon the app showed and hid: `check-mark` on a selectable chip shows MudBlazor's check while it is selected, in the place of its start icon, and on a toggle item before its content. And MudAutocomplete's drop-down arrow, which the app put in itself: an `AutoCompleteBox` now shows ArrowDropDown in an edge-end icon button, ArrowDropUp while open, right against the text as `.mud-select-input` has it, and clicking it opens and closes the list; `MinimumPrefixLength` is 0, MudAutocomplete's `MinCharacters`, so an empty field lists everything. The date picker's button and the arrow are raw to automation, the picker's own peer expanding. A checkbox in a `DataGrid` column header is the table's compact one, as one in a cell is: `.mud-table-cell .mud-checkbox` covers header cells too. And the docs site has a Custom components page: MudAlert's default icon per severity, MudAvatarGroup's Max and its "+n" avatar, closable chips, MudDynamicTabs and MudDataGrid's SelectColumn, built in the app as worked examples with their C#, and its Control states page is gone, every sample on it now on its component's page, with new examples for Color.Inherit buttons, a binding's validation error and toggles outside a group. The Avatar page's group example is MudAvatarGroup's: a -12px overlap, 44px and 60px ringed avatars, the first on top; its styles, like the Max example's, keep a small, rounded or square avatar's size and shape, and the Max slider reaches 0, as MudBlazor's does. And every docs page is checked against MudBlazor 9.11: a parity row for every parameter that changes a component's look or behaviour, Missing with a reason where nothing here does it, the component index's overall parity to match, and these looks transcribed where they had drifted - scroll bars as MudThemeProvider's 8px grey rail (new `FiliScrollbarThumb*` tokens), nav links 14px on a 24.5px line with an ArrowDropDown group arrow, small icon buttons 26px round a 20px icon, the alert's 24px icon, the switch thumb at elevation 1, a filled snackbar's message at 400, tooltips centred on a 1.4em line, a dense numeric field's 20px spin column, the grey halo on an `inherit` checkbox and radio, a selected chip's ripple, `TextBlock.button`'s 24.5px line, and tabs always uppercase. The published class list grows to 183. See [Upgrading from 0.24.0 to 0.25.0](#upgrading-from-0240-to-0250-unreleased). |
| **0.24.0** | MudPopover's `Elevation`, `Square`, `DropShadow` and `Paper` on a flyout, a menu flyout and a context menu, which were missing: `elevation0` to `elevation25`, `square`, `flat` and the new `no-paper`, through `FlyoutPresenterClasses` on a flyout and on the `ContextMenu` itself. `material-elevation-overlay` follows the level, and leaves a `flat` or `no-paper` sheet alone. Nothing changes without a class. And MudCarousel's `ArrowsPosition` and `BulletsPosition`: `arrows-top` and `arrows-bottom` move the arrows from the middle, `bullets-top` and `bullets-center` the bullets from the bottom. The bullets sit in a 48px band, as MudCarousel's bullet buttons do, so they are on the arrows' line when both share an edge, and a carousel's bullets move 12px up from the bottom. The published class list grows to 179. See [Upgrading from 0.23.0 to 0.24.0](#upgrading-from-0230-to-0240). |
| 0.23.0 | Three of MudBlazor's options that were missing, as classes. `bordered` on a `TableView` is MudTable's `Bordered`: table-lines between a row's cells. `always-show-scroll-buttons` on a `TabControl` or `TabStrip` is MudTabs' `AlwaysShowScrollButtons`: the scroll buttons stay while the tabs fit, both disabled. `default` on a toggle is MudToggleGroup's `Color.Default`, MudButton's grey, since a toggle with no class is primary. And the tab bar's scroll arrows are `ScrollIconColor`'s default, `Color.Inherit`: the bar's text, text-primary or a coloured bar's contrast text, where they were action-default grey on every bar. The published class list grows to 174. See [Upgrading from 0.22.0 to 0.23.0](#upgrading-from-0220-to-0230). |
| 0.22.0 | Fixes the docs site turned up, building a page for every component, and the docs themselves moved into that site. The label beside a `start-icon` or `end-icon` is centred on the icon, as `.mud-button-label`'s flex row centres them: left to stretch to the icon's 20px, a `TextBlock` drew its text at the top of that box, 2px above the icon's middle, on buttons, chips, extended FABs and every other host of an icon slot; and so is the label beside a chip's `avatar`. `button-group` on a StackPanel of ToggleButtons is MudToggleGroup's shape: the radius on the outer corners and outlined items sharing one line, where each item kept its own corners and two met in a 2px line. A link is underlined: `underline` and the default hover underline set nothing, as the style looked in the template for a TextBlock that a string's AccessText and the app's own text never were. Selecting a tab scrolls only its bar, where it scrolled the page to the bar; a scroll button at its end has an action-disabled arrow; and a tab bar's elevation shadow is no longer clipped. A carousel's bullets are its text colour, as MudCarousel's `Color.Inherit` bullets are, where the current one was primary and vanished on a primary slide. A list row and a `list-item` take a `start-icon` as MudListItem's Icon: 24px in a 56px column, action-default, the text centred on it; and an icon helper's earlier value no longer outlives a later one, so a button inside a list row keeps its own icon colour. A snackbar's elevation shadow is no longer clipped by the wrapper its close animation scales, and an alert's icon rules reach only the alert's own icon, not the X in a close button inside it. A tooltip placed `Top`, `Bottom`, `Left` or `Right` sits centred on that side, 10px from its target, as MudTooltip's does, arrow or not: Avalonia's 20px `ToolTip.VerticalOffset`, meant for the pointer, put a `Top` one over its target and `Left` and `Right` ones 20px low, and only an `arrow` tooltip had a gap. The time picker's popover has its elevation 8 shadow, which the presenter clipped to its own edge. The usage docs are now the [docs site](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/), one page per component with live examples and a parity table, each at its own address; `docs/controls.md`, `docs/mudblazor-parity.md`, `docs/known-gaps.md` and `docs/razor-vs-axaml.md` are gone, and `docs/` keeps what a contributor needs. See [Upgrading from 0.21.0 to 0.22.0](#upgrading-from-0210-to-0220). |
| 0.21.0 | Two gaps the apps found moving onto 0.20.0. `placeholder` on a text field makes `PlaceholderText` MudTextField's `Placeholder` rather than its `Label`: the hint in the empty field where the text goes, in the text's colour at .42, with no label and no room for one. And `FiliReducedMotion`, which MudBlazor does not have: `True` in the application's resources stops everything the theme moves or fades on its own, in every window and popup - the ripple, popup fades, skeletons, snackbar slides, expansion panels, overlays and the tab indicator - while a progress indicator keeps moving. The published class list grows to 172. See [Upgrading from 0.20.0 to 0.21.0](#upgrading-from-0200-to-0210). |
| 0.20.0 | Six gaps the apps found moving onto 0.19.0. A `PathIcon` draws its glyph as MudIcon does, in a 24px viewBox scaled to its size, where Avalonia's stretched the glyph's bounds to the box: icons are smaller and sit where Material put them, and an app no longer pads its glyphs. An alert holds `.mud-alert-message`'s 9px, so a one-line alert is MudAlert's 50px, and its icon is 22px, 12px before the message, on the first line. The newest snackbar is at the bottom of the stack wherever it is anchored, as MudBlazor's, where a bottom stack put it at the top; `newest-on-top` is `NewestOnTop`. A field with no `PlaceholderText` takes no room for a label: a standard field is MudInput's 32px. `list-item` on a Button is MudListItem with `OnClick`, a row that acts. And `caption` and `overline` have no colour of their own, as in MudBlazor: they were grey, which also turned a caption link grey. The published class list grows to 171. See [Upgrading from 0.19.0 to 0.20.0](#upgrading-from-0190-to-0200). |
| 0.19.0 | A nav label with a type class (`caption`, `body2`...) keeps its class's line height, as `.mud-typography-*` overrides the line it would inherit from `.mud-nav-link`; the nav's 28px line was forced on it, so a caption under a title grew the row. Labels still wrap. The docs add `FiliActionDefaultHoverColor` to what overriding `FiliActionDefaultColor` needs, and warn that `FiliButtonTextTransform` set on a button reaches its flyout: a single label keeps its case as a run. A wrapped nav group keeps its arrow on the first line, as `.mud-nav-link`'s `align-items: flex-start` does. DrawerPage's pane toggles are MudIconButton's 48px circle, where the fork's bar squeezed them to a 48x38 pill. And the spinner `DatePicker`'s highlight follows `FiliPrimaryColor`, where it stayed MudBlazor's purple. `Color.Inherit` reaches three more controls: `inherit` on a `TextBlock` is the surrounding colour over a type class's own, and on a checkbox or radio the glyph's, with `unchecked-inherit` for `UncheckedColor`. MudSelect's `AdornmentColor` is `adornment-{colour}` and `adornment-inherit` on a `ComboBox`. A chip with nothing to click, MudChip with neither `OnClick` nor `Href`, is `chip` on a `ContentControl`: no hover, ripple or focus. The docs show how to change one control's hover and ripple through tokens in its `Resources`, rather than by reaching into its template. The published class list grows to 169. See [Upgrading from 0.18.0 to 0.19.0](#upgrading-from-0180-to-0190). |
| 0.18.0 | A nav label in a `DockPanel` or `Grid` written straight into a row's `Header` wraps, as 0.17.0 said it would; it ran on at one line, cut off at the drawer's edge. Only string headers and templated ones wrapped. The date picker's toolbar fits its 100px again: 0.17.0 gave its 34px date a button's 1.75 line, 72px, where `.mud-button-date` is on a 1.17 line, 52px, and the year and date overflowed the toolbar. A command bar button with its label on the right is a button's 36.5px, where it stayed 29. And the toggle icon buttons carry no label line box, which DrawerPage's pane toggles, wearing them without the `icon` class, had kept. A `ToggleButton`, MudToggleItem, is a MudButton's 36.5px too, where it was 32. The docs now say that overriding a colour means its hover, darken, lighten and contrast-text tokens as well. See [Upgrading from 0.17.0 to 0.18.0](#upgrading-from-0170-to-0180). |
| 0.17.0 | An open drawer's nav menu never scrolls sideways, as MudDrawer does not: a label too long for it wraps, where it ran past the edge and the menu scrolled. A nav link is the drawer's text colour, as `.mud-nav-link` inherits it: on a coloured drawer the links were text-primary, dark on the colour; and every nav row is 44px, where one whose header was a control was 40. A button is MudButton's height, 36.5px (30.75 small, 42.25 large), where it was 29, as are a split and a drop-down button. A `TimePicker` stretches as the other fields do. See [Upgrading from 0.16.0 to 0.17.0](#upgrading-from-0160-to-0170). |
| 0.16.0 | Seven of Avalonia's controls in MudBlazor's look, where they wore the forked templates. `TimePicker` is MudTimePicker: the standard field with the `AccessTime` adornment, and the toolbar's popover over Avalonia's columns. `CommandBar` is MudToolBar of MudIconButtons with a `MoreVert` MudMenu. `TableView` is MudTable, as the data grid is. A `NavigationPage`'s bar is MudAppBar, a `TabbedPage` is MudTabs and a `ContentPage` sits on the background. `Carousel` and `CarouselPage` are MudCarousel, with `no-arrows` and `no-bullets`; a time picker with `editable` takes a typed time. A filled or outlined field's icon adornment is centred on the box, as `.mud-input` centres it, where it sat on the text line below the box's middle; a text adornment, such as a unit, stays on the text line beside the value it is read with. `FiliMoreVertGeometry`, `FiliAccessTimeGeometry`, `FiliCheckGeometry` and `FiliArrowBackGeometry` are new, and the published class list grows to 159. See [Upgrading from 0.15.0 to 0.16.0](#upgrading-from-0150-to-0160). |
| 0.15.0 | MudDatePicker on `CalendarDatePicker`: the standard field with the `Event` adornment, taking the field's classes, and MudPicker's popover - the primary toolbar with the year and the date over the calendar, a 36px day filled primary when selected and ringed when today. `FiliEventGeometry` is new. A button's label beside an icon is uppercase, as MudButton's is with a `StartIcon`: the case is inherited, as CSS's is, so a `TextBlock` in a button's content reads in capitals, with its `Text` left as the app wrote or bound it; `TextBlock.button` is uppercase too. See [Upgrading from 0.14.0](#upgrading-from-0140-to-0150). A select's closed box draws the selection with its `SelectionBoxItemTemplate`, as Avalonia's own select does, and with its `ItemTemplate` only when it has none. It always used the `ItemTemplate`, so a closed-box template was ignored and anything the list's template set, such as a colour meant for the popup, showed in the closed box too. An outlined or filled `NumericUpDown` or `AutoCompleteBox` draws its stroke or its fill, which a value inside their templates had kept them from drawing, and an outlined label on either, or on the date picker, is no longer cut off along its top edge. A select, a number field, an autocomplete and a date picker are each exactly a text field's height in every variant, so a row of them lines up: a standard select was 3px taller, and a number field was 56px in every variant, `dense` included. |
| 0.14.0 | MudButtonGroup of any length is `button-group` on a `StackPanel` of buttons, across or down: one shape with the radius on its outer corners, shared lines and MudBlazor's separators. A disabled slider is MudSlider's: rail and thumb at .38 in their colour, the thumb ringed in white, where it was grey. A flyout has no padding of its own, as MudPopover has none. The published class list grows to 156. See [Upgrading from 0.13.0 to 0.14.0](#upgrading-from-0130-to-0140). |
| 0.13.0 | A `paper` casts MudPaper's default elevation 1, as a `card` does; `elevation0` takes it away. Icons find their place in four more hosts: an icon marked `start-icon` or `end-icon` in a link is the link's font size and 4px from the text, in a tab header 24px and 8px from the text, and in a text field's adornment 24px; an adornment keeps 8px from the text; and a chip's `avatar` is MudChip's 24px (18 small, 32 large) with its margins. A `PathIcon` with no size class is 24px, MudIcon's default `Size.Medium`, where it was 20. MudOverlay is `overlay` on a Border, with `dark-background` and `light-background`: its content centred, its container's corners, and MudBlazor's .15s fade each time it shows. The published class list grows to 155. See [Upgrading from 0.12.0 to 0.13.0](#upgrading-from-0120-to-0130). |
| 0.12.0 | The last names to settle before 1.0. MudPaper is `paper`, MudBlazor's word, where it was `surface`; MudTabs' `SliderColor` is `indicator-{colour}` and `HideSlider` is `hide-indicator`, so that no tab class reads as a `Slider`'s. A number field's spin buttons draw MudNumericField's chevrons, `FiliKeyboardArrowUpGeometry` and `FiliKeyboardArrowDownGeometry`, not the select's triangles. The spacing scale is MudBlazor's whole one, 0 to 20 steps of 4px, each as a number (`FiliSpacing4`) and a `Thickness` (`FiliSpacing4Thickness`); `FiliBlackBrush` joins `FiliBlackColor`. See [Upgrading from 0.11.0 to 0.12.0](#upgrading-from-0110-to-0120). |
| 0.11.0 | Screens build about three times faster: a Border inside a control's template is no longer matched against every Border class style, `material-elevation-overlay` no longer searches each Border's ancestors, and an icon's `start-icon` or `end-icon` size comes from its button rather than from a style every control was matched against. A number field's spin buttons sit inside its box, as MudNumericField's do; an expansion panel grows open over .3s instead of cross-fading, as MudCollapse does; a tooltip wraps only at the screen's edge; and `drawer-header` is MudDrawerHeader. The published class list grows to 152. See [Upgrading from 0.10.0](#upgrading-from-0100-to-0110). |
| 0.10.0 | Seven known gaps closed, and two of Material's own options. An option that is one fixed property of its control is that property: `Square` on a paper, an alert, a data grid or one panel is `CornerRadius="0"`, a toolbar's or an app bar's `Gutters="false"` is `Padding="0"`, and a data grid's `Bordered` is `GridLinesVisibility="All"`. A disabled field's line is dotted, `no-animation` on tabs is `SliderAnimation="false"`, a drawer casts MudDrawer's elevation 1, and `expansion-panels` follows a virtualising panel by item index. A press ripples, as in MudBlazor: a circle from the pointer in the control's text colour on buttons, chips, FABs, toggles, tabs and rows, and in the halo of a checkbox, radio or switch; `FiliRippleOpacity` and `FiliRippleOpacitySecondary` at 0 turn it off. A button's label is uppercase, as MudBlazor's button typography makes it, on buttons, split and drop-down buttons, toggle items, extended FABs and tabs; `FiliButtonTextTransform` keeps labels as written. Every popup fades in over 251ms, as a MudPopover does. `expansion-panels` on a `StackPanel` is MudExpansionPanels: the panels joined into one shape, ruled between, with an expanded one standing 16px apart and the others closing, unless the group is `multi-expansion`, and `no-outline` drops its rules; the group also reads an `ItemsControl`'s generated panels. `material-fixed-tabs` shares the tab bar in equal parts, and `material-elevation-overlay` lightens raised surfaces by their level in dark mode; both are Material's, not MudBlazor's, and named so. In high contrast the elevation rings are the application's line colour rather than a fixed `#E8E8EE`. The published class list grows to 151. See [Upgrading from 0.9.4](#upgrading-from-094-to-0100). |
| 0.9.4 | Eleven fixes: eight found by moving four more apps onto 0.9.3, three by the gallery's new samples of each control as an app uses it. A `MenuFlyout`'s rows and a submenu's own rows are MudMenuItem's 48px rows, not the base's 24px ones. Keyboard focus draws no dashed rectangle over the state layer, and a slider is one Tab stop rather than three. "_Save" is Save with Alt+S on buttons, toggles, checkboxes, radios, switches, tabs, expanders and menus. An app's own `FiliFontFamily` reaches popups. A start adornment floats a text field's label; a multi-line field's label sits above its text, which starts at the top. `ShowProgressText` on a thin bar shows the whole value. A data grid's checkbox column keeps its colour in every row, and a switch stretched to its row centres its label. See [Upgrading from 0.9.3 to 0.9.4](#upgrading-from-093-to-094). |
| 0.9.3 | Two fixes found by moving an app onto 0.9.2. A `TextBox` whose `Text` is null no longer logs a binding error from its hidden counter, which every such field did. A checkbox in a `DataGrid` cell is MudBlazor's table checkbox, a 32px circle pulled in 4px, so a selection column no longer makes its rows 81px tall; they are 57. |
| 0.9.2 | Nothing changes on screen: this release fixes the interface 1.0 will promise. [Compatibility](compatibility.md) says what a version may and may not change, and the resource keys an app may name are listed in [Published resource keys](published-keys.md) and pinned by a test, beside the classes. The README's links work on nuget.org, each pointing at the release's own commit, and the package has an icon and a description that says what it is. |
| 0.9.1 | Trimmed and AOT apps that reference `Avalonia.Controls.DataGrid` start again: the theme's optional DataGrid styles are kept through trimming. From 0.7.0 to 0.9.0 such an app threw "No precompiled XAML found" for `DataGridStyles.axaml` at startup, and so did the browser gallery. Untrimmed apps were never affected. Nothing to change in markup. |
| 0.9.0 | Every MudBlazor icon parameter as a resource key: the theme's glyphs are `DynamicResource`s, so a key such as `FiliCheckBoxCheckedGeometry` set in a control's `Resources` swaps that control's glyph. MudToggleItem as MudBlazor 9.11 draws it, filled once checked, and MudToggleIconButton as `icon` on a `ToggleButton`. MudNavMenu as `nav` on a `TreeView`, with the mini drawer and drawer colours; MudToolBar as `toolbar`; MudIcon's sizes and colours on a `PathIcon`. MudSnackbar's severity icon, close button and `text`, `outlined` and `normal` variants; MudTooltip's colours and `arrow`. Tab bars that scroll. `DrawerPage`'s pane toggles as icon buttons and its closed compact rail as a mini drawer. `multi-selection` on lists and trees, 48px list, select and menu rows, `dense-items`. `required`, `shrink-label` and `no-underline` on fields, and a field's classes reaching the field inside an `AutoCompleteBox` or `NumericUpDown`. `LabelPlacement`, the 48px hit circle and `dense` on checkboxes and radios; `ThumbIcon` on switches. Expansion panel options and every elevation level from 0 to 25. The DataGrid is themed from 12.1.0. The published class list grows to 146. See [Upgrading from 0.8 to 0.9](#upgrading-from-08-to-09). |
| 0.8.0 | MudTabs' sliding indicator: selecting a tab slides the indicator over from the old one, on a `TabControl` or a `TabStrip`, on any side. MudDataGrid's nested groups: a nested group's header steps in 48px per level and its rows stay put. A `dense` grid's last visible column has 16px on its right, as MudDataGrid's does. A group row's 1px stray edge is gone. No new classes. See [Upgrading from 0.7 to 0.8](#upgrading-from-07-to-08). |
| 0.7.0 | MudDataGrid on Avalonia's `DataGrid`, themed by type when the app references `Avalonia.Controls.DataGrid` 12.1.2 or later - the package does not depend on it: `dense`, `striped`, `bordered`, `hover`, `outlined`, `square` and elevation, sorting and group rows. MudFab as `fab` on a `Button`, MudAvatar as `avatar` and MudCard as `card` on a `Border`, MudBadge as `badge` on a `HeaderedContentControl`. Filled buttons, split and drop-down buttons, the expander and snackbars now draw the shadows Avalonia had been clipping. The published class list grows to 106. See [Upgrading from 0.6 to 0.7](#upgrading-from-06-to-07). |
| 0.6.0 | MudProgressCircular as `circular` on a `ProgressBar`: sizes, colours, `rounded`, `ShowProgressText` and MudBlazor's indeterminate spin. MudSkeleton's `Animation.Wave` as `wave`. The published class list grows to 81. |
| 0.5.1 | Avalonia 12.1.3, which the package now requires. The forked base templates are re-synced to that tag; the only upstream change is the drawn window title bar's decoration role. |
| 0.5.0 | MudTabs' `Position` through `TabStripPlacement`, `Elevation` as `elevation*` and `SliderColor` as `slider-{colour}`; MudProgressLinear's `Striped` as `striped`; MudButton's `StartIcon` / `EndIcon` and MudChip's `Icon` spacing as `start-icon` / `end-icon` on the icon in the content. |
| 0.4.0 | MudSlider's `Variant`, `TickMarks` and `ValueLabel`: `filled`, tick dots through `TickPlacement`, and `value-label`. `UncheckedColor` on checkboxes and radios as `unchecked-{colour}`. The `GrayDefault` … `GrayDarker` tokens. An outlined text field's or select's floated label is no longer cut off along its top edge. See [Upgrading from 0.3 to 0.4](#upgrading-from-03-to-04). |
| 0.3.0 | The project is renamed `Fili.Theme.MudAvalonia`: the assembly, namespaces, `avares://` paths and repository. The NuGet id stays `Fili.MudAvalonia.Theme`. See [Upgrading from 0.2 to 0.3](#upgrading-from-02-to-03). |
| 0.2.0 | MudBlazor parity. A colour class is now only a colour, as `Color` is in MudBlazor - on buttons, split and drop-down buttons, checkboxes, radios, switches, sliders, progress bars, text, tabs and the app bar - with Button's full Variant × Color × Size matrix. Icon buttons, chips, alerts and skeletons as classes; helper text, a counter and dense fields; select variants; MudTabs and MudAppBar options. See [Upgrading from 0.1 to 0.2](#upgrading-from-01-to-02). |
| 0.1.1 | The app bar's text colour is inherited rather than set on every `TextBlock`, which had turned an app-bar select's drop-down white on white. The gallery loses its Fluent comparison switch. |
| 0.1.0 | First release: the palette, elevation, typography and geometry tokens, the hand-written control themes over a forked Avalonia Simple base, light, dark and high contrast. |

Every version and its release notes are on the
[releases page](https://github.com/FiliArrochada/Fili.Theme.MudAvalonia/releases); a version on
nuget.org can be unlisted but never replaced.

## Upgrading from 0.24.0 to 0.25.0 *(unreleased)*

No class or key is renamed or removed. `clearable`, `loading`, `subheader` and `check-mark` are
new words in the published class list: grep your app for them before upgrading, as an app's own
style of that name now meets the theme's. `FiliScrollbarThumbColor`, `FiliScrollbarThumbHoverColor`
and their brushes are new keys. These look or behave differently, as MudBlazor's do:

- **Every `AutoCompleteBox` shows a drop-down arrow** at the end of its field, a 48px icon button
  that opens and closes the list, and the text runs 8px closer to any end adornment. A search box
  that should not read as a drop-down sets `InnerRightContent="{x:Null}"`, or every field at once
  with `<Style Selector="AutoCompleteBox"><Setter Property="InnerRightContent" Value="{x:Null}" /></Style>`.
  Content of the app's own in `InnerRightContent` replaces the arrow as before; to bring the
  arrow back after replacing it, clear the value rather than set null. A theme `BasedOn` the
  autocomplete's inherits the arrow.
- **An empty `AutoCompleteBox` lists everything** when opened, by the arrow, Down or F4:
  `MinimumPrefixLength` is now 0, where Avalonia's default is 1. Set it back to 1 on a field to
  list nothing until something is typed; the arrow then does nothing over an empty field.
- **A `CheckBox` in a `DataGrid` column header is the compact table checkbox**: the 32px circle
  (28 small, 44 large) with -4px margins, as one in a cell has been, where it was the 48px one.
- **Scroll bars are 8px**, MudThemeProvider's injected rail, where they were 12: content that
  scrolls gains 4px of width. The thumb is a fixed grey, `FiliScrollbarThumb*`, darker on hover
  and while dragged, in every variant but high contrast.
- **A nav menu row is 40.5px**: its link is the default 14px type on a 24.5px line, where it was
  body1 on 28 (44px). A group's arrow is ArrowDropDown, MudNavGroup's default; an app that
  swapped it through `FiliExpandMoreGeometry` in a group's resources sets
  `FiliArrowDropDownGeometry` instead.
- **Tab labels are always uppercase**, as `.mud-tab` is: `FiliButtonTextTransform` no longer
  reaches them.
- **Small controls grow by 2px**: a small icon button is 26px round a 20px icon (the clear button,
  a small toggle icon button and the snackbar's close button with it), and a tooltip's line is
  1.4em, centred, so its chip is 25px. An alert's icon is 24px, its height unchanged.
- **Quieter details**: the switch thumb's shadow is elevation 1, a filled snackbar's message is
  Regular weight, a dense numeric field's spin column is 20px, an `inherit` checkbox or radio
  keeps the grey hover halo, and `TextBlock.button` is a 24.5px line.

## Upgrading from 0.23.0 to 0.24.0

No class or key is renamed or removed. `no-paper`, `arrows-top`, `arrows-bottom`, `bullets-top`
and `bullets-center` are new words in the published class list, and `elevation0` to
`elevation25`, `square` and `flat` now reach a flyout's presenter and a context menu: grep your
app for them before upgrading, as an app's own style of that name now meets the theme's. One
thing looks different, as MudBlazor's does:

- **A carousel's bullets** sit 12px higher, centred in a 48px band as MudCarousel's bullet
  buttons are, so they share the arrows' line when both are on the same edge.

## Upgrading from 0.22.0 to 0.23.0

No class or key is renamed or removed. `bordered`, `always-show-scroll-buttons` and `default` are
new words in the published class list: grep your app for them before upgrading, as an app's own
style of that name now meets the theme's. One thing looks different, as MudBlazor's does:

- **A tab bar's scroll arrows** are the bar's text colour, text-primary or a coloured bar's
  contrast text, where they were action-default grey. A disabled one is still action-disabled.

## Upgrading from 0.21.0 to 0.22.0

No class or key is renamed or removed. Some things look different, as MudBlazor's do:

- **A tooltip placed `Top`, `Bottom`, `Left` or `Right`** now sits centred on that side, 10px off.
  If your app set `ToolTip.VerticalOffset` to pull one back into place, remove it: an offset the
  app sets still applies, now from MudBlazor's position.
- **A label beside a `start-icon`, `end-icon` or a chip's `avatar`** is centred on it. A margin
  or `VerticalAlignment` your app added to nudge it can go.
- **An icon in a list row or a `list-item`** is 24px in a 56px column, in action-default. Size,
  colour or margin your app set on it by hand can go; its own values still win.
- **A carousel's bullets** are its text colour; set `Foreground` on the carousel for another.
- **Links to the old docs**: `docs/controls.md`, `docs/mudblazor-parity.md`, `docs/known-gaps.md`
  and `docs/razor-vs-axaml.md` are gone. Their pages are on the docs site, at
  `https://filiarrochada.github.io/Fili.Theme.MudAvalonia/#<page>`.

## Upgrading from 0.20.0 to 0.21.0

Nothing changes unless you use the new pieces.

- **One new class name**, `placeholder`: grep your app for it, as for any published class.
- **A text field that should show a hint and no label** takes `Classes="placeholder"`; its
  `PlaceholderText` is then the hint.
- **If your app has a reduced-motion setting**, set `FiliReducedMotion` from it in the
  application's resources: the theme's own animations then stop with the app's.

## Upgrading from 0.19.0 to 0.20.0

- **Icons are smaller.** A `PathIcon` keeps a Material glyph's padding, as MudIcon does. Padding
  you added to make a glyph fill 24×24 can go: the glyph draws in a true 24px box now. A curved
  glyph you padded with move commands comes out a little smaller than in 0.19.0, because a
  curve's control points widened its bounds and 0.19.0 drew it about 10% too large. A glyph drawn
  in another box needs scaling into 24.
- **Captions and overlines are the colour round them**, not grey. Where you want grey supporting
  text, set `Foreground="{DynamicResource FiliTextSecondaryBrush}"`.
- **An alert is taller**: 50px for one line, where it was 32.
- **A text field with no `PlaceholderText` is shorter**: a standard one is 32px, where it was 48.
  Give it a label, or a `MinHeight`, to keep the old height.
- **Snackbars at the bottom stack the other way**: the newest at the bottom. `newest-on-top` on
  the manager puts it back on top.
- **Two new class names**, `list-item` and `newest-on-top`: grep your app for them.

## Upgrading from 0.18.0 to 0.19.0

- **Ten new class names**: `unchecked-inherit`, and `adornment-primary` … `adornment-dark` with
  `adornment-inherit`. Grep your app for them before upgrading, as for any published class.
- **A `ContentControl` with the class `chip` is now a chip.** If your app styles its own
  `ContentControl.chip`, the theme's chip look now applies to every property your style does not set.
- **A nav label with a type class keeps that class's line height**, so a `caption` under a title
  in a nav row is shorter than in 0.18.0.
- **`DrawerPage`'s pane toggle is a 48px circle**, and its top and bottom bars lose their 5px
  above and below.
- **If a style of yours reaches a template part** to change a control's hover or ripple, it can
  now be tokens in that control's `Resources` instead (see [Theming](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/#theming) on the docs site).

## Upgrading from 0.17.0 to 0.18.0

- **A `ToggleButton` is 36.5px** (30.75 small, 42.25 large), where it was 32: MudToggleItem is a
  MudButton, on its 1.75 line. A toggle group sized round the old height grows by 4 or 5px.
- **A command bar button with its label on the right is 36.5px**, where it was 29.
- **A nav label in a `DockPanel` or `Grid` written straight into the `Header` wraps**, on 28px
  lines, as 0.17.0 said it would. A menu that relied on it being cut off now has taller rows.
- **Overriding a colour is five tokens.** If your app sets `Fili{Color}Color` alone, its hover,
  press and filled-hover states are still MudBlazor's: add `…HoverColor`, `…DarkenColor`,
  `…LightenColor` and `…ContrastTextColor` (see [Theming](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/#theming) on the docs site).

## Upgrading from 0.16.0 to 0.17.0

- **A nav menu never scrolls sideways**, open or closed. A label too long for the drawer wraps
  onto a second line when its header lets it: a string `Header`, or a `Grid` or `DockPanel`
  with the text in the star column. In a horizontal `StackPanel`, the layout the mini drawer
  needs, the text cannot wrap and is cut off at the edge; widen the drawer (`OpenPaneLength`)
  or shorten the label.
- **A nav link takes the drawer's text colour** instead of setting text-primary. In a plain
  drawer that is drawer-text: the same colour in light, and dimmer in dark (50% white, where
  text-primary is 70%), as MudBlazor's links are; in a coloured drawer (`SplitView.primary`
  and the rest) the links are the contrast text, where they were dark. Outside a drawer a link
  is still text-primary.
- **Every button is taller**: 36.5px, MudButton's, where it was 29 (32 with an icon beside the
  label); 30.75 small and 42.25 large. `.mud-button` gives its label a 1.75 line, which the text
  alone did not. A split button and a drop-down button match it. Icon buttons, chips and FABs keep
  their heights. A row or bar sized round the old 29px grows by 7 or 8.
- **A nav row is 44px whatever its header holds**: one written with a control in its `Header`
  (an icon and a `TextBlock`) was 40.
- **A `TimePicker` stretches** across its column, as the date picker and every field do, where it
  stayed at 120px. One that should not sets `HorizontalAlignment` or a `Width`.

## Upgrading from 0.15.0 to 0.16.0

Nothing needs a markup change; several things look different, each now in MudBlazor's shape.

- **A `TimePicker` is MudTimePicker**: the standard text field with the clock icon button, where
  it was a button of grey blocks. It has no `PlaceholderText`, so give it a label with
  `AutomationProperties.Name`. Field classes (`outlined`, `filled`, `dense`...) apply when set on
  the picker, and `editable` lets the time be typed.
- **A `CommandBar` is a toolbar of icon buttons.** Its `DefaultLabelPosition` is `Collapsed` by
  default here, where Avalonia's is `Bottom`: each command is an icon with its label as the
  tooltip. Set `DefaultLabelPosition="Right"` for labelled buttons, or `Bottom` for Avalonia's
  layout.
- **A `TableView` is a paper at elevation 1** with MudTable's header, cells and rules.
- **A `NavigationPage`'s bar is an app bar**: primary, 64px, with a shadow; `HasShadow="False"`
  takes the shadow away. A `TabbedPage` has the theme's tabs.
- **A filled or outlined field's icon adornment is centred on the box**, 6px higher in a filled
  field's end and 2px in an outlined field's start or end, where it sat on the text line. A text
  adornment, a string or a `TextBlock` such as a unit, does not move: it stays on the value's
  line, where MudBlazor would lift it with the icons.
- **A `Carousel` shows arrows and bullets**, as MudCarousel does by default. Add `no-arrows` or
  `no-bullets` to keep one without them.

## Upgrading from 0.14.0 to 0.15.0

Nothing needs a markup change; five things look different.

- **A `CalendarDatePicker` is MudDatePicker**, where it wore the forked Simple template: the
  standard text field with the calendar icon button at its end, and a popover with the primary
  toolbar over the calendar. Classes the field takes (`outlined`, `filled`, `dense`, `error`...)
  now apply when set on the picker. Today is ringed, as MudDatePicker marks it; set
  `IsTodayHighlighted="False"` to leave it plain. A `Calendar` on its own takes the same days,
  months and arrows.
- **The picker and its field are one Tab stop**, where they were two.
- **An outlined or filled `NumericUpDown` or `AutoCompleteBox` is outlined or filled.** The
  class reached the field inside, but the field drew neither the stroke nor the fill; a screen
  that set one sees the box appear.
- **A number field is a text field's height**, where it was 56px whatever its variant: 48px
  standard, 45 dense, 40 outlined dense. A standard select is 48px, where it was 51. A form whose
  rows held one of them closes up by those pixels.
- **A label in a button's content is uppercase**, where only a string label was: the text beside
  a `start-icon` or `end-icon` in a panel, on a button, a split or drop-down button, a toggle item,
  an extended FAB and a tab header, and any `TextBlock` with the `button` class. A chip's label
  keeps its case. To keep one region's labels as written, put
  `<x:String x:Key="FiliButtonTextTransform">none</x:String>` in its `Resources`; a `TextBlock`
  whose `Inlines` the app wrote is left as it is.

## Upgrading from 0.13.0 to 0.14.0

One thing needs a markup change, one looks different, and one class is new.

- **A flyout no longer pads its content.** A `Flyout` whose content relied on the 16px gives the
  content a `Margin` or `Padding` of 16, as MudBlazor's popover content does with `pa-4`. Menu
  flyouts are unchanged.
- **A disabled slider keeps its colour**, faded to .38 with a white ring round the thumb, where
  it was action-disabled grey.
- **`button-group` is new**, on a `StackPanel` of buttons. An app that joined buttons by hand -
  `Spacing="-1"`, corner radii per button - can drop that for the class; an app with its own
  `button-group` class now gets the theme's look on a StackPanel.

## Upgrading from 0.12.0 to 0.13.0

Nothing needs a markup change; five things look different, and three classes are new.

- **A bare `paper` casts a shadow**, MudPaper's default elevation 1. A paper meant flat, as
  `Elevation="0"` is in MudBlazor, takes `elevation0`; an `outlined` paper has none.
- **A text field's adornment sits 8px from the text**, on either side, where it touched it.
- **An avatar in a chip shrinks to the chip's**: 24px, 18 on a small chip and 32 on a large one,
  pulled out to the chip's edge, where it was 40px.
- **An icon marked `start-icon` or `end-icon`** in a link, a tab header or a text field's
  adornment takes that host's size and gap. One not marked keeps its own.
- **A `PathIcon` with no size class is 24px**, MudIcon's default `Size.Medium`, where it was 20.
  One meant at 20 takes `small`, MudIcon's `Size.Small`; an icon a host sizes is unchanged.
- **`overlay`, `dark-background` and `light-background` are new** and MudOverlay's. An app that
  draws a scrim as a `Panel` with `FiliOverlayDarkBrush` can make it a `Border` with
  `overlay dark-background`; an app with its own class of one of those names now gets the
  theme's look on a Border too.

## Upgrading from 0.11.0 to 0.12.0

Three classes are renamed, the last renames before 1.0 freezes the class list; the old names
match nothing now, silently. Search the markup and styles for each:

| Was | Now |
|---|---|
| `surface` (a `Border`, `ListBox` or `TreeView`) | `paper` |
| `slider-primary` … `slider-dark` (a `TabControl` or `TabStrip`) | `indicator-primary` … `indicator-dark` |
| `hide-slider` | `hide-indicator` |

And one thing looks different:

- **A number field's spin buttons are chevrons**, MudNumericField's `KeyboardArrowUp` and
  `KeyboardArrowDown`, where they were the select's filled triangles. They have keys of their own
  now: an app that set `FiliArrowDropUpGeometry` or `FiliArrowDropDownGeometry` in a number
  field's `Resources` sets `FiliKeyboardArrowUpGeometry` and `FiliKeyboardArrowDownGeometry`.

## Upgrading from 0.10.0 to 0.11.0

Nothing needs a markup change; three things look different, one app style may stop matching, and
one icon class set late no longer takes.

- **A number field's box runs under its spin buttons**: the underline, the filled ground and the
  outline reach the field's edge, and the arrows sit inside the box, as tall as it.
- **An expansion panel grows open** over .3s, and shrinks closed, where its content cross-faded.
- **A long tooltip is wider**: it wraps at the screen's edge, where it wrapped at 320px.
- **A style of the app's that reaches into a theme template's Border**, such as
  `Button /template/ Border#PART_Root`, matches nothing now: those parts are a Border of the
  theme's own type, which keeps them out of every Border class style. A template's inside is not
  part of the published interface ([Compatibility](compatibility.md)); set the control's own
  properties or resource keys instead.
- **`start-icon` and `end-icon` are read when the icon joins its button**, chip, FAB or nav link.
  One written in markup, as the docs show, is unchanged; one added to an icon already in its
  button, from code, keeps the icon's own size until the button's size class changes.

## Upgrading from 0.9.4 to 0.10.0

Three classes no longer apply to some controls, where the control's own property says the same,
and need a markup change there; five things look different, and a sixth in an app that repaints
high contrast's line colour.

- **One option, one property.** Each of these classes did nothing but set one value the control
  already has a property for, so it is gone from that control; write the property:

  | Was | Now |
  |---|---|
  | `square` on a `Border.surface`, a `Border.alert`, a `DataGrid` or one `Expander` | `CornerRadius="0"` |
  | `no-gutters` on a `Border.toolbar` or `Border.appbar` | `Padding="0"` |
  | `bordered` on a `DataGrid` | `GridLinesVisibility="All"` |

  The class left behind does nothing, and fails no build: grep for these pairs. `square` on a
  card, an avatar or an `expansion-panels` group, `no-gutters` on an `Expander`, a `ListBox` or
  a row, and `bordered` on a badge or a nav menu are unchanged.

- **Button and tab labels are uppercase**: `Content="Save"` reads SAVE on a `Button`, a
  `SplitButton`, a `DropDownButton`, a `ToggleButton`, an extended FAB and a tab, as on every
  MudButton. Chips, links, checkboxes and menu rows keep their case. A label written in capitals
  already is unchanged, and so is content that is a control rather than a string. To keep every
  label as written, put `<x:String x:Key="FiliButtonTextTransform">none</x:String>` in the
  application's resources, or in one control's to keep one region's.
- **Popups fade in** over 251ms, where they appeared at once: a select's list, a menu, an
  autocomplete's list, a flyout, a context menu and a tooltip.
- **A press ripples** on buttons, chips, FABs, split and drop-down buttons, toggle items, tabs,
  list, select, menu, tree and nav rows, and in a checkbox's, radio's or switch's halo. To keep
  the old still press, put `<x:Double x:Key="FiliRippleOpacity">0</x:Double>` and the same for
  `FiliRippleOpacitySecondary` in the application's resources, or one control's.
- **A disabled field's line is dotted**, as MudBlazor's is, in lines-inputs, where it was a
  solid action-disabled line.
- **A drawer casts elevation 1** past its edge, as MudDrawer does; `elevation0` on the
  `SplitView` keeps it flat. High contrast is unchanged.
- **High contrast's rings follow the line colour.** An app that sets `FiliLinesDefaultColor` in
  its high-contrast resources now sees that colour round every raised surface too, where the
  rings stayed `#E8E8EE`.

## Upgrading from 0.9.3 to 0.9.4

Nothing needs a markup change; seven things look or behave differently.

- **An underscore in a button's, toggle's, checkbox's, radio's, switch's, tab's, expander's or
  menu item's text is an access key**, as in Avalonia's own themes: "_Save" shows Save and answers
  Alt+S. Text meant to show an underscore, "file_name", now shows "filename"; double it,
  "file__name".
- **No dashed focus rectangle.** Keyboard focus is the state layer's tint alone, as the docs had
  said. An app that relied on the rectangle sets a control's `FocusAdorner`.
- **A `MenuFlyout`'s rows, a submenu's rows and an app's `ItemContainerTheme` based on
  `{x:Type MenuItem}` are 48px MudMenuItem rows**, where they were 24px. Menus opened from a
  button get taller.
- **A multi-line text field (`AcceptsReturn`) starts its text at the top**, with its label above
  it, where both were centred. A single-line field is unchanged.
- **A text field with `InnerLeftContent` floats its label** even while empty.
- **A data grid's checkbox column is action-default in every row**, where every row but the
  current one, and every row of a read-only grid, was drawn disabled.
- **A switch stretched to its row centres its content**, where its 40px sat at the top: beside a
  checkbox, its label moves 4px down to line up.

## Upgrading from 0.8 to 0.9

One thing needs a markup change; most screens look different without one.

**Needs a change**

- **`flat` on an `Expander` is gone.** It dropped both the surface and the shadow. `elevation0` is
  MudExpansionPanel's `Elevation="0"`, which drops the shadow and keeps the surface; add
  `Background="Transparent"` as well for the old look.

**Looks different with no markup change**

- **Rows are MudBlazor's 48px**, with body1 text, where they were 36px with body2: `ListBox` rows
  (and an `AutoCompleteBox`'s drop-down), `ComboBox` options and menu rows. A screen with a list
  gets taller. `dense` brings a row back to 36px; on a select, that is `dense-items`.
- **A select's and an auto-complete's drop-down is 300px high at most**, where it was 200.
- **Menus:** 4px of padding round the list rather than 8, 112px wide at least rather than 160 or
  180, and 4px either side of a separator rather than 8. A menu with no icons no longer reserves
  an icon column, so its labels move left. A submenu's arrow is MudBlazor's filled ArrowRight.
- **A plain `ToggleButton` is MudToggleItem**: primary text, and filled primary with contrast
  text once checked, rather than a 6% tint. `outlined` draws its line in the colour in every state.
  A toggle with its own `Background` keeps it when checked, so its checked text, the contrast
  colour, can disappear against it. Check every toggle that sets a background: an icon toggle
  wants `icon`, which never fills. `DrawerPage`'s own pane toggles are icon buttons already.
- **A closed compact `DrawerPage`** is a mini drawer: a 56px rail rather than Avalonia's 48, and
  a `nav` menu in it shows only its icons.
- **A closed mini drawer's nav no longer scrolls sideways.** A scrollbar under the icons is gone,
  in a `SplitView` and a `DrawerPage` alike.
- **Checkboxes and radios** have MudBlazor's 48px hit circle (44 small, 60 large), 8px larger than
  before, and a disabled one is text-disabled.
- **A disabled switch** has a grey thumb with its shadow over its own track colour at 12%.
- **Text fields and selects:** a label floated by a value is text-primary rather than
  text-secondary; disabled text, labels and helper text are text-disabled. A select's arrow
  turns into ArrowDropUp while it is open.
- **`NumericUpDown`** shows a validation error on the field's helper line, as a text field does,
  rather than in Avalonia's box under it. The field classes (`filled`, `outlined`, `dense`, …) and
  `AutomationProperties.HelpText` set on it now take effect, as they do on an `AutoCompleteBox`.
- **A `TreeView`** no longer tints a row under the pointer unless it has `hover`, as MudTreeView's
  `Hover` defaults to off. A leaf keeps the empty arrow slot, so its text moves 40px right to line
  up with a branch's.
- **Snackbars** show the severity icon and a close button on every card, including a card made
  with `Show(INotification)`; `hide-icon` and `hide-close` remove them.
- **Tabs that do not fit scroll**, with chevron buttons at the ends, instead of being squeezed. A
  custom `ItemsPanel` on the tab bar is now measured inside a horizontal scroller.
- **Tooltip text is weight 500.**
- **`appbar elevationN` now shows elevation N.** The app bar always drew elevation 4 before.
- **A DataGrid on `Avalonia.Controls.DataGrid` 12.1.0 or 12.1.1 is now themed**; 0.8 needed
  12.1.2.

**Can affect an app's own styles**

- **Glyphs are `DynamicResource`s.** An app that defines a resource named `Fili…Geometry` for its
  own use now sees it drawn inside that region in place of the theme's glyph. Rename it.
- **Template parts moved** on `CheckBox`, `RadioButton`, `ToggleSwitch`, `ListBoxItem`,
  `ComboBoxItem`, `TreeViewItem`, `NumericUpDown`, `MenuItem`, `NotificationCard`, `ToolTip`,
  `TabControl` and `TabStrip`. An app's own `/template/` selector on one of them may no longer
  match. Two to look for: a `ListBoxItem`'s padding is on a panel round the content rather than
  on `PART_ContentPresenter`, and a `MenuItem`'s `PART_Chevron` is filled, so `Stroke` does
  nothing.
- **40 new classes can collide with an app's own**, and some old ones now do something on more
  controls (`icon` on a `ToggleButton`, `dense` on a `ListBox`, `TreeView` or menu, colour classes
  on a `TreeView`, `SplitView`, `ToolTip` or `PathIcon`). The new ones:
  - `button`, `nav`, `toolbar`, `normal`;
  - `hide-close`, `hide-icon`, `no-gutters`, `required`, `shrink-label`, `arrow`, `dense-items`,
    `multi-selection`;
  - `label-start`, `label-top`, `label-bottom`;
  - `thumb-primary` … `thumb-dark`;
  - every elevation level the 0.8 list skipped, up to `elevation25`.

  The README's class list is the one to grep against.

## Upgrading from 0.7 to 0.8

Nothing needs to change in markup; three things look different.

- **The tab indicator slides** between tabs instead of fading, over 0.3s. There is no class to
  turn it off.
- **A `dense` `DataGrid` gives its last column 8px less** on the right, so an auto-sized last
  column is 8px narrower and a star-sized column takes the difference.
- **Grouped `DataGrid`s:** with more than one group level, the nested group headers move in 48px
  per level. Every group row's title moves 1px left, where a stray row-header edge had pushed it.

## Upgrading from 0.6 to 0.7

- **Raised controls now show their shadow.** A filled button, a filled split or drop-down button,
  an expander and a snackbar had their elevation set but never drawn, because Avalonia 12 clips
  every templated control to its bounds. They now look as MudBlazor's do, which is a visible
  change on every screen that has one. Nothing needs to change in markup.
- **A `DataGrid` is now themed**, if the app references `Avalonia.Controls.DataGrid`. It becomes
  MudDataGrid: elevation 1 on surface, no hover highlight unless the grid has `hover`. Remove any
  `StyleInclude` of the package's own theme (`avares://Avalonia.Controls.DataGrid/Themes/...`):
  included after this theme it wins, and before it, it is dead weight. A grid on a version older
  than 12.1.2 is left alone, with a warning in Avalonia's log: keep the package's own theme
  included there, or the grid has no theme at all.
- **New classes can collide with an app's own:** `fab`, `avatar`, `card` and its parts, `badge`
  and its options, and `hover` on a `DataGrid`. The README's class list is the one to grep against.

## Upgrading from 0.3 to 0.4

- **A slider with no variant class is MudSlider's `Variant.Text`**: the whole rail is the colour
  at 30%, with no solid part. 0.3 drew the part up to the thumb solid on every slider; add
  `filled` for that.

## Upgrading from 0.2 to 0.3

0.3.0 renames the project but not the package. The package id does not change, so `dotnet add package Fili.MudAvalonia.Theme` and every
`PackageReference` keep working. What changes is the assembly name inside it, which is part of
every resource URI and namespace:

- **The two includes:** `avares://Fili.MudAvalonia.Theme/...` becomes
  `avares://Fili.Theme.MudAvalonia/...` - in `Themes/Base/FiliBaseTheme.axaml` and
  `FiliTheme.axaml` alike. The old URI does not fail to compile; it fails at startup.
- **`FiliThemeVariants`:** `xmlns:theme="using:Fili.MudAvalonia.Theme"` becomes
  `using:Fili.Theme.MudAvalonia`, and `using Fili.MudAvalonia.Theme;` in C# likewise.

## Upgrading from 0.1 to 0.2

0.2.0 changes what some existing markup looks like:

- **`Classes="primary"` on a button is a primary *text* button.** It used to mean filled; add
  `filled` for that. The same holds on `SplitButton`, and an unclassed `DropDownButton` is no
  longer primary.
- **`secondary` on a `TextBlock` is the pink secondary colour**, MudText's `Color`. Grey
  supporting text is `Foreground="{DynamicResource FiliTextSecondaryBrush}"`.
- **Tabs follow MudTabs' defaults:** a surface bar with no rule under it (add `border` for one),
  tabs at least 160px wide, labels in text-primary.
- **An app bar has 24px side padding**, MudAppBar's `Gutters`; `Padding="0"` removes it.
- **Filled and outlined fields gain their 12px / 14px side padding**, which 0.1 never applied.
