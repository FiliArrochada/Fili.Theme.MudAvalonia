# Unreleased

Work since 0.24.0, kept here as it lands; at the bump this file is renamed to its version.

[All versions](README.md)

MudInput's `Clearable`, which was a button the app wrote and wired: `clearable` on a text field, an
autocomplete or a number field shows MudBlazor's small clear button while the field holds text that
is not blank and is neither disabled nor read-only, before an end adornment and out of the Tab
order; on a date picker it clears the date at once, as MudPicker's Clear does, where the picker
would read the emptied text only on losing focus. Nothing changes without the class. And
MudDataGrid's `Loading` and `LoadingProgressColor`, which the app laid over the grid itself:
`loading` shows an indeterminate 4px bar in its own row under the header, info, or in the grid's
colour class. And MudListSubheader, which was a TextBlock the app styled: `subheader` on a
`TextBlock` is its action-default, medium 14px text, 8px above and 20px below, with `inset` and
`no-gutters` for Inset and Gutters="false". And MudChipSet's and MudToggleGroup's `CheckMark`, which
were an icon the app showed and hid: `check-mark` on a selectable chip shows MudBlazor's check while
it is selected, in the place of its start icon, and on a toggle item before its content. And
MudAutocomplete's drop-down arrow, which the app put in itself: an `AutoCompleteBox` now shows
ArrowDropDown in an edge-end icon button, ArrowDropUp while open, right against the text as
`.mud-select-input` has it, and clicking it opens and closes the list; `MinimumPrefixLength` is 0,
MudAutocomplete's `MinCharacters`, so an empty field lists everything. The date picker's button and
the arrow are raw to automation, the picker's own peer expanding. A checkbox in a `DataGrid` column
header is the table's compact one, as one in a cell is: `.mud-table-cell .mud-checkbox` covers
header cells too. And the docs site has a Custom components page: MudAlert's default icon per
severity, MudAvatarGroup's Max and its "+n" avatar, closable chips, MudDynamicTabs and MudDataGrid's
SelectColumn, built in the app as worked examples with their C#, and its Control states page is
gone, every sample on it now on its component's page, with new examples for Color.Inherit buttons, a
binding's validation error and toggles outside a group. The Avatar page's group example is
MudAvatarGroup's: a -12px overlap, 44px and 60px ringed avatars, the first on top; its styles, like
the Max example's, keep a small, rounded or square avatar's size and shape, and the Max slider
reaches 0, as MudBlazor's does. And every docs page is checked against MudBlazor 9.11: a parity row
for every parameter that changes a component's look or behaviour, Missing with a reason where
nothing here does it, the component index's overall parity to match, and these looks transcribed
where they had drifted - scroll bars as MudThemeProvider's 8px grey rail (new `FiliScrollbarThumb*`
tokens), nav links 14px on a 24.5px line with an ArrowDropDown group arrow, small icon buttons, a
data grid's sort button among them, 26px round a 20px icon, the alert's 24px icon, the switch thumb
at elevation 1, a filled snackbar's message at 400, tooltips centred on a 1.4em line, a dense
numeric field's 20px spin column, the grey halo on an `inherit` checkbox and radio, a selected
chip's ripple, `TextBlock.button`'s 24.5px line, and tabs always uppercase. The published class list
grows to 183. And the theme costs less to apply: on a typical screen it attaches 45% fewer styles,
allocates a fifth less and handles hover in half the time, as a colour class now sets its colour
once for the template to read instead of a style per colour, part and state, and no style is
matched against every control; and the docs say that publishing with ReadyToRun cuts the theme's
first load to a third. The theme itself loads a quarter faster, as the styles that switch a
button, chip, field, badge or nav menu to its variant's control theme no longer build that theme
up front. And an `overlay` whose class is removed gives its content its own alignment back, where
it stayed centred and kept a stale entry for each time the class came and went. A striped
progress bar's bands stand still with `FiliReducedMotion`, the indeterminate bar still moving, and
only a `dense` data grid's cells look for the grid's last column. And a control that ripples
builds its ripple's canvas on its first press rather than when it is shown, a typical screen
building 5% fewer visuals. A list or tree row is built without the multi-selection checkbox
unless its list has `multi-selection`, where every row carried it hidden: 200 list rows allocate
a quarter less and build a fifth faster. A list's colour class sets its selected row's colour
on the list for its rows to inherit, where three styles per colour reached every row: a list
row carries a quarter fewer style entries and allocates 8% less. See [Upgrading from 0.24.0 to 0.25.0](#upgrading-from-0240-to-0250-unreleased).

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
  1.4em, centred, so its chip is 25px. An alert's icon is 24px, its height unchanged. A data grid's
  sort button is the same small icon button: its arrow grows to 20px and the button shrinks to 26px.
- **Card parts and `material-elevation-overlay` name the controls they go on.** A card part
  (`card-header`, `card-header-avatar`, `card-header-content`, `card-header-actions`,
  `card-content`, `card-actions`) takes a `StackPanel`, `DockPanel`, `Grid`, `WrapPanel`,
  `Border`, `TextBlock`, `Button` or `Image`, and `card-actions` centres its items on a panel;
  `material-elevation-overlay` takes a window, a `UserControl`, one of those panels, a `Border`, a
  `ScrollViewer` or a `ContentControl`. On any control they were styles matched against every
  control in the window, 40% of a screen's style instances with the avatar's and the toolbar's
  child styles. A part on another control loses its margin and docking: wrap it in a `Border`, or
  put the class on the panel round it.
- **Quieter details**: the switch thumb's shadow is elevation 1, a filled snackbar's message is
  Regular weight, a dense numeric field's spin column is 20px, an `inherit` checkbox or radio
  keeps the grey hover halo, and `TextBlock.button` is a 24.5px line.
