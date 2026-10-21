# Versions

What changed in each release of `Fili.Theme.MudAvalonia`, newest first. The package is on
[nuget.org](https://www.nuget.org/packages/Fili.MudAvalonia.Theme) under its original id,
`Fili.MudAvalonia.Theme`; install steps are in the [README](../README.md#install).

| Version | |
|---|---|
| *(unreleased)* | Seven known gaps closed, and two of Material's own options. An option that is one fixed property of its control is that property: `Square` on a paper, an alert, a data grid or one panel is `CornerRadius="0"`, a toolbar's or an app bar's `Gutters="false"` is `Padding="0"`, and a data grid's `Bordered` is `GridLinesVisibility="All"`. A disabled field's line is dotted, `no-animation` on tabs is `SliderAnimation="false"`, and `expansion-panels` follows a virtualising panel by item index. A press ripples, as in MudBlazor: a circle from the pointer in the control's text colour on buttons, chips, FABs, toggles, tabs and rows, and in the halo of a checkbox, radio or switch; `FiliRippleOpacity` and `FiliRippleOpacitySecondary` at 0 turn it off. A button's label is uppercase, as MudBlazor's button typography makes it, on buttons, split and drop-down buttons, toggle items, extended FABs and tabs; `FiliButtonTextTransform` keeps labels as written. Every popup fades in over 251ms, as a MudPopover does. `expansion-panels` on a `StackPanel` is MudExpansionPanels: the panels joined into one shape, ruled between, with an expanded one standing 16px apart and the others closing, unless the group is `multi-expansion`, and `no-outline` drops its rules; the group also reads an `ItemsControl`'s generated panels. `material-fixed-tabs` shares the tab bar in equal parts, and `material-elevation-overlay` lightens raised surfaces by their level in dark mode; both are Material's, not MudBlazor's, and named so. In high contrast the elevation rings are the application's line colour rather than a fixed `#E8E8EE`. The published class list grows to 151. See [Upgrading from 0.9.4](#upgrading-from-094). |
| **0.9.4** | Eleven fixes: eight found by moving four more apps onto 0.9.3, three by the gallery's new samples of each control as an app uses it. A `MenuFlyout`'s rows and a submenu's own rows are MudMenuItem's 48px rows, not the base's 24px ones. Keyboard focus draws no dashed rectangle over the state layer, and a slider is one Tab stop rather than three. "_Save" is Save with Alt+S on buttons, toggles, checkboxes, radios, switches, tabs, expanders and menus. An app's own `FiliFontFamily` reaches popups. A start adornment floats a text field's label; a multi-line field's label sits above its text, which starts at the top. `ShowProgressText` on a thin bar shows the whole value. A data grid's checkbox column keeps its colour in every row, and a switch stretched to its row centres its label. See [Upgrading from 0.9.3 to 0.9.4](#upgrading-from-093-to-094). |
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

## Upgrading from 0.9.4

Three classes no longer apply to some controls, where the control's own property says the same,
and need a markup change there; four things look different, and a fifth in an app that repaints
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
