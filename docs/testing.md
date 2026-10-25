# Testing

What the tests check, how CI runs them, and why each one exists. The short version: **almost
every way a theme breaks is silent.** A misspelt resource key resolves to nothing, a token the
high-contrast variant forgets falls back to dark without a word, a style that loses a priority
contest just does not apply. Nothing throws and the app still starts. The tests are there to turn
those silent failures into red builds, because looking at the app cannot be relied on to find them.

## Three test projects

| Project | What it does | Speed |
|---|---|---|
| `tst/Fili.Theme.MudAvalonia.UnitTests` | 1251 tests on a headless Avalonia session. They resolve resources, template controls, move a real pointer over them, and read back the values the theme applied. Nothing is rendered. | seconds |
| `tst/Fili.Theme.MudAvalonia.NoDataGridTests` | 3 tests. The theme in an app that does not reference `Avalonia.Controls.DataGrid`: the assembly is really absent, `FiliTheme.axaml` loads and themes a button, and no DataGrid rule was loaded. | seconds |
| `tst/Fili.Theme.MudAvalonia.PixelTests` | 84 tests. Twelve render one gallery frame each with Skia and compare it pixel by pixel with a PNG committed in `Baselines/`. Thirty-six, in `GalleryAuditTests`, run three of the unit suite's checks over each of those twelve pages instead of a list of bare controls - no binding error, every control one Tab stop at most, no presented text showing an access key's underscore - so every class, state and variant a sample shows is covered. Ten, in `GalleryPageTests`, check how the gallery behaves - that the controls page opens on its introduction and a loading card pinned over the page, fills in a few rows per dispatcher pass with the card in view however far it is scrolled (pausing while off screen, and loading the rest at once for an index row ahead of the load), that it opens at the top in a window shorter than its first screen, that keyboard focus still scrolls it, that each row of the index beside it lands on its section and the index follows the scroll, that every section fits beside the index in the desktop window and in a frame, and that the app bar shows the theme's version. Twenty-six, in `DocsSiteTests`, check the docs site the gallery is: every page in the catalogue opens, every example card shows its own file's XAML as its code, every component links to a page that exists, and no two pages share a slug. | ~45 seconds |

The no-DataGrid project is its own assembly because what it tests is an absence: the main suite
references the package to test the grid, so it can never see a theme that only loads WITH it. A
DataGrid selector left in `FiliTheme.axaml` passes every other test and fails here with the
missing assembly's name.

The unit and pixel suites are separate on purpose. The pixel suite needs Skia with headless drawing turned **off** -
Avalonia's default headless renderer draws nothing at all, and a pixel suite built on it would
compare two blank images and pass forever. Turning real drawing on also changes how text is
measured, and the unit suite should not start measuring differently because the pixel suite
needed a different platform.

## The unit tests

1251 tests in 80 classes, grouped here by the failure each one exists to catch.

### Tokens: does every value exist, in every variant?

| Class | Tests | What it checks | Why it matters |
|---|---|---|---|
| `ResourceResolutionTests` | 6 | Every colour, brush and elevation token resolves in light and dark, every brush really is an `IBrush`, primary differs between the variants, the dark surface sits above the dark background, and the spacing scale is MudBlazor's 0 to 20 steps of 4px as a number and as a `Thickness`. | A missing or misspelt key is silent in Avalonia: the control keeps whatever it had and the colour quietly never changes. A `*Color` bound where a `*Brush` belonged compiles and paints nothing. |
| `ElevationOverlayTests` | 19 | `material-elevation-overlay`: each overlay level is its shadow plus, in Dark only, Material's white at 4.5 ln(level + 1) + 2 percent; papers, cards, panels and grids take their level's, and every popup sheet - a select's, an autocomplete's, a submenu, a context menu, a flyout and a menu flyout - takes level 8; nothing changes without the class, in Light or in high contrast; and an outlined paper, card or grid, a coloured box, an app bar and a filled button are left alone. | An overlay on a coloured surface would wash out the colour, and one that missed a popup would leave the most raised sheet on the page the darkest. |
| `HighContrastTests` | 18 | Every token resolves in high contrast **and differs from dark**, and the variant's own rules (hard rings for elevation, the drawer edge in place of the drawer's shadow) apply only there. Every ring is the line colour, and an app's own high-contrast line colour reaches them, changed at run time too. | High contrast inherits from Dark, so a forgotten token comes back with the dark value - usually a translucent line over black, which is to say no line. Only "differs from dark" proves the key was written. |
| `PaletteDerivationTests` | 23 | Every `*DarkenColor` and `*LightenColor` equals MudBlazor's own derivation (`MudColorPort`, HSL lightness ∓ 0.075 with .NET rounding), and the port matches the 14 values mudblazor.com publishes. | Hover and filled-button shades are derived, not chosen. Changing a colour without its shades, or rounding the port differently, shows up here rather than as a slightly wrong hover. |
| `SimpleBridgeTests` | 6 | The ~96 keys the forked Avalonia Simple templates paint from still resolve in both variants. | The first standalone render had switches with no switch: eighteen `*Color` keys were missing while their brushes were mapped. A new Avalonia version that adds a key fails here instead of on screen. |

### Coverage: is every control themed?

| Class | Tests | What it checks | Why it matters |
|---|---|---|---|
| `OptionalBoundaryTests` | 7 | The packages the theme builds against but does not depend on: only a package's own folder under `Themes/Optional/` names its types (a scan of every `.axaml` and `.cs`, reporting file and line), each `OptionalStyleInclude`'s `MinimumVersion` is the version `Directory.Packages.props` builds against, an absent package loads nothing, an older one loads nothing and logs why, the one here is loaded, and the include keeps its compiled XAML through trimming. | A DataGrid selector anywhere else passes every other test - this suite has the package - and takes the whole theme down at startup in every app that does not. |
| `StandaloneReadinessTests` | 3 | The exact set of hand-written control themes (69 of the 95 templated types, the DataGrid package's included) and that every one resolves; every other type is classified with the reason it stays on its forked template. | Avalonia has no implicit default theme. A control with no theme does not look wrong - it renders **nothing**. An Avalonia upgrade that adds a control type fails here asking to be classified. |
| `StyleCostTests` | 2 | Every Border in a hand-written template is a `PartBorder`, and no top-level selector looks for a plain Border inside one. | A style is matched against every control of the type its selector ends in, template parts included: template Borders paying for every Border class style made a screen of ordinary controls six times slower to build than the base. |
| `PublishedInterfaceTests` | 4 | The resource keys in `docs/published-keys.md` are exactly the `Fili*` keys the token dictionaries declare plus every glyph key, each resolves in light, dark and high contrast, and the public C# API is `FiliThemeVariants` alone. | Renaming a key breaks every app that names it, silently - a missing key resolves to nothing - while every test here passes, because the theme's own references move with it. See [Compatibility](compatibility.md). |
| `BindingErrorTests` | 31 | Thirty-one controls and variants as an app first shows them - empty, unset - log no binding error. | Avalonia logs a failed binding and carries on, so nothing else here fails; an app that fails its own tests on a binding error inherits every one the theme makes, on every page. An adopting app found the first: the counter's `Text.Length` on every TextBox whose `Text` was null. |
| `TabStopTests` | 31 | The same thirty-one controls are one Tab stop at most: the control, or the one part that stands for it. | A focusable part adds a stop nobody sees until they use the keyboard. An adopting app found one: a slider's two 2px rail halves, three stops per slider. |
| `AccessKeyTests` | 14 | "_Save" is Save with an access key on every control the base recognises one on - buttons, toggles, checkboxes, radios, switches, split and drop-down buttons, links, tabs, expanders - and on a menu bar's items and rows. | The hand-written templates had dropped what the forked base does, so the underscore showed and Alt did nothing. It renders fine either way to anyone who never types an underscore. |
| `TextTransformTests` | 32 | A button's string label is uppercase - text, outlined and filled buttons, an extended FAB, a toggle item, split and drop-down buttons, a tab and a tab strip's item - while a chip, a link, a checkbox, a radio, a switch and a panel header keep their case; a label beside a `start-icon` is uppercase with its `Text` untouched, stays so when its text changes and follows the switch, while a chip's keeps its case and a `TextBlock` with runs of its own is left as written; `TextBlock.button` is uppercase; `_Save` keeps its access key as `_SAVE`; and `FiliButtonTextTransform` turns it off in a region, live, and takes CSS's other values. | MudBlazor's buttons are uppercase by its typography, and a label in the wrong case is the first thing a MudBlazor user notices. The switch is what an app that wants sentence case has, as MudBlazor's has `TextTransform`. |
| `PackageReadmeTests` | 1 | Every relative link in the README names a file that exists. | The README is the package's page on nuget.org too. The pack rewrites its links to the commit and fails on a broken one; this catches a renamed doc on every build rather than at release. |
| `ClassVocabularyTests` | 2 | The classes the theme's selectors use are exactly the 172 the README publishes, in both directions; and no class is one fixed value on its own control, which the property would say. | Class names are MudBlazor's ordinary words (`small`, `error`, `vertical`) and they collide with an app's own. An undeclared class is a collision nobody signed off on; a declared one nothing uses is a lie to adopters. |
| `GeneratedThemeTests` | 1 | `Button.axaml` and the marked regions in nineteen other files are exactly what `ThemeColourGenerator` writes from the one list of eight colours. | Hundreds of colour styles are generated so that no colour can differ from the others. Editing one block by hand fails the build. `FILI_REGENERATE=1` rewrites the files. |

### Controls: does each one look and behave as MudBlazor's does?

These template real controls, hover them with a real headless pointer move, focus them, select
them, and assert the brushes, sizes and visibility the theme produced - each value transcribed
from MudBlazor's SCSS or the component's parameter defaults.

| Class | Tests | What it covers |
|---|---|---|
| `ThemeCompositionTests` | 54 | The cross-cutting rules: Roboto loads with a face per weight, an app's own font reaches popups, button content keeps its contrast colour, the filled field keeps its top-only corners, the select floats its label only with a selection, validation shows the message rather than the exception type, keyboard focus lights the state layer and pointer focus does not and draws no focus rectangle, tree indent, drawer metrics. |
| `ButtonMatrixTests` | 45 | `Button`: three variants × eight colours × three sizes, at rest and under a real pointer hover, plus contrast ratios. |
| `ButtonHeightTests` | 23 | MudButton's height, the label on a 1.75 line: 36.5 / 30.75 / 42.25px in every variant and beside an icon, the label centred on a start or end icon on a button, a chip and an extended FAB, split and drop-down buttons the same, and icon buttons, chips and FABs keeping their own. |
| `ComponentClassTests` | 47 | Icon buttons, chips, alerts (MudAlert's 50px and 38 dense, its 22px icon), skeletons, the light divider and `flat`, and that no raised control clips the shadow its template draws past its edge. |
| `BadgeTests` | 34 | `badge`: the pill, every `Origin` and `overlap` to the pixel, a wide badge centring on narrow content, the colours on the badge alone, `99+`, a null `Header`, `dot`, `bordered`, `icon`, elevation, and that only the pill takes the pointer. |
| `FabTests` | 31 | `fab`: 56px grey at elevation 6, lifting to 8 and 12 under a real pointer, sizes, colours, `outlined` and `text`, `extended`, the 24px icon and its gap, `flat` and disabled. |
| `AvatarTests` | 22 | `avatar`: sizes, the three variants and every colour, `rounded` and `square`, an elevation that is not clipped, and an `Image` covering the shape and clipped to it. |
| `ExpansionPanelTests` | 12 | MudExpansionPanels' options on an `Expander` - elevation 1 by default, the content growing open over .3s `ease-in-out` and not on load, revealed part way from either end and hidden once closed, `elevation0` keeping the surface, `CornerRadius` reaching the root, `dense`, `no-gutters`, each ExpandDirection keeping its own sides, and `flat` gone - plus every elevation level 0 to 25 as a class on a `Border`, an `Expander` and a `TabControl`, MudBlazor's soft level 25, and the `button` type class. |
| `ExpansionPanelsTests` | 21 | `expansion-panels` as MudExpansionPanels: closed panels as one shape ruled between, the shape and its shadow on each panel's root and the rule over it, so a middle panel keeps its shadow; an expanded panel 16px from its neighbours and rounded, with the panel before it unruled and both neighbours rounded on the facing side; no margin at the group's edges; closing, adding and removing panels; `square`; `no-outline`, written and generated; and an `Expander` outside a group unchanged. MultiExpansion: opening a panel closes the others, of panels written open the first stays, a binding on `IsExpanded` survives, `multi-expansion` opts out, and a panel outside the group is left open. An `ItemsControl`'s generated panels, and Expanders as its items, join as written ones do; in a virtualising panel the group's ends are its items' ends, and its panels follow their recycled containers. |
| `DataGridTests` | 26 | `DataGrid`: elevation 1, the 59px subtitle2 header row and 53px body2 rows, a checkbox in a cell at the table's compact size in each of the three sizes and in its colour in every row of a checkbox column, no rule under the last row, `dense` and its 16px on the right of the last visible column as columns move and hide, `striped`, `hover` replacing the stripe, the selection tint, `bordered` without header rules, `outlined`, `square`, the sort arrow, group rows, their glyph and a nested group's 48px-per-level indent with the rows left alone, and the edit box - which must still be based on the standard field, the one static link out of `Themes/Optional/DataGrid`. |
| `OverlayTests` | 8 | `overlay` transparent and taking the pointer, `dark-background` and `light-background` in the palette's overlay colours, the content centred, the container's corners past a Panel and an app's own winning, a plain Border untouched, and MudBlazor's .15s fade each time it shows. |
| `PaperTests` | 4 | `paper` at MudPaper's default elevation 1 in text-primary, an elevation class replacing it, and `outlined` a line with no shadow. |
| `CardTests` | 8 | `card` at elevation 1, `outlined` and `square`, and the header, media, content and actions laid out to MudCard's metrics. |
| `GreyGroundTextTests` | 3 | The text on a default avatar and badge is text-primary (white on a `text` avatar) in light and dark, and black in high contrast. |
| `TabsAndAppBarTests` | 37 | MudTabs' defaults, colours, `border` / `outlined` / `rounded` / `centered` / `hide-indicator`, that a bar's colour never reaches a nested `TabControl`, and the sliding indicator - starting at the old tab, on either axis, from wherever an interrupted slide had got to, resting at its own with MudBlazor's timing, not on first show, and not at all with `no-animation`; the app bar's gutters, `dense` and colours. |
| `ButtonGroupTests` | 10 | `button-group` across and down: the radius on the outer corners, the 1px overlap, a text button's line in its colour and a filled one's in lighten or divider with no resting shadow, a hidden or removed button leaving the group, and a plain StackPanel untouched. |
| `FlyoutPaddingTests` | 1 | A flyout leaves the padding to its content, as MudPopover does. |
| `SplitButtonMatrixTests` | 29 | `SplitButton` as MudButtonGroup, including the separator rule and the hit-testing of each half. |
| `FieldDetailTests` | 59 | Helper text, the error in its place, the counter, `helper-on-focus`, `dense`, select variants, that filled and outlined fields pad their content, a start adornment floating the label, a multi-line field's label as far above its first line as a single-line field's is above its text, grown or sized by the app, and a single-line field's height left alone, a disabled field and select in text-disabled with a dotted line, the 300px drop-downs, the tooltip's weight, the template a select's closed box draws its selection with, nothing clipping an outlined label inside a control that wraps a field, such a control taking the outlined and filled variants, and every such control a text field's height in each variant. |
| `SliderMatrixTests` | 13 | `Slider` sizes and colours, the 30% rail, and the rings on hover and focus. |
| `RemainingOptionsTests` | 46 | Tab position, elevation and slider colour, striped progress, the progress text whole and centred over a thin bar, and start and end icon spacing on every host: buttons, split and drop-down buttons through the plain buttons in their templates, toggle items, chips, FABs, links, tabs and a text field's adornments, which keep 8px from the text and none when absent, a text one on the text line in every variant; a chip's avatar at each size; a bare icon at MudIcon's medium 24px; and an icon or avatar outside a host keeping its own size. |
| `UncheckedColourTests` | 11 | `unchecked-{colour}` on a checkbox and a radio: the glyph and halo while not checked, the colour class once checked. |
| `InheritColourTests` | 13 | A type class with no colour of its own, a link's caption in the link's colour, `inherit` on a `TextBlock` over a colour class and following its surroundings; on a checkbox and a radio, the glyph in the surrounding colour with no halo, `unchecked-inherit` only while unchecked, and disabled still grey; `adornment-{colour}` and `adornment-inherit` on a select's arrow, text-disabled when disabled. |
| `IconViewBoxTests` | 3 | A `PathIcon` draws its glyph in MudIcon's 24px viewBox, scaled with a size class and in an icon button, rather than stretching the glyph's bounds. |
| `NoLabelFieldTests` | 9 | A field with no `PlaceholderText` has no label's room: standard 32 / 29 dense, filled and outlined their own heights, a filled field's text 9px higher, and the room back once a label is set. |
| `PlaceholderFieldTests` | 7 | `placeholder`: the hint shows while empty and the label never, at .42; no label's room in every variant; the hint where the text goes, after a start adornment; and without the class `PlaceholderText` is still the label. |
| `ReducedMotionTests` | 4 | `FiliReducedMotion` on a window: its popup at full opacity from the first frame, no ripple on a press, an expansion panel open at once; and motion on by default. |
| `LinkUnderlineTests` | 5 | MudLink's Underline on a string and on the app's own TextBlock alike: `underline` always, the default only under the pointer, `no-underline` never. |
| `ListItemButtonTests` | 9 | `list-item` on a Button: 48 / 36px rows, full width, the label in its own case, the 16px gutter and `no-gutters`, the row's hover tint. |
| `SnackbarOrderTests` | 5 | The newest snackbar at the bottom of the stack, anchored top or bottom, and at the top with `newest-on-top`. |
| `StaticChipTests` | 13 | `chip` on a `ContentControl`: the default chip at rest, nothing under the pointer (no focus, no hand, the same fill), every variant in every colour, the small and large sizes, and a `Button.chip` still clickable. |
| `SliderOptionsTests` | 8 | `filled`, tick dots - their count, size, colour and where the first and last sit - `value-label`, and a disabled slider's rail and thumb at .38 in their colour with a white ring, at each size. |
| `DropDownButtonMatrixTests` | 12 | `DropDownButton` as MudMenu's button activator. |
| `SelectionGlyphMatrixTests` | 13 | Checkbox and radio glyph colours and sizes, and the 48px halo round the glyph (44 small, 60 large). |
| `SwitchMatrixTests` | 12 | Toggle switch thumb and track colours and sizes, and a switch stretched to its row centring its label with a checkbox's beside it. |
| `TextColourTests` | 9 | `TextBlock` colours, MudText's `Color`. |
| `ControlSizingTests` | 8 | The minimum sizes a control needs to exist when nothing stretches it - a number MudBlazor never has to give, because CSS sizes by container. |
| `ProgressColourTests` | 8 | `ProgressBar` colours. |
| `ProgressCircularTests` | 17 | `circular`: sizes, stroke, colours, the sweep, `rounded`, the indeterminate key frames and the centred value. |
| `SkeletonWaveTests` | 6 | `wave`: its timing, its two gradients, the band colour in every variant, and that it does not also pulse. |
| `RightToLeftTests` | 2 | Avalonia mirrors a subtree with one transform, so layout flips on its own; a checkmark must opt out and does. |
| `AppBarTests` | 3 | The app bar's text colour is inherited, so a select on the bar keeps text-primary in its popup instead of white on white; elevation 4 by default, and any elevation class beats it. |
| `ToggleItemTests` | 21 | `ToggleButton` as MudBlazor 9.11's MudToggleItem: the colour as text and filled once checked, in every colour and under a real pointer, `outlined`'s line in the colour, the sizes and their outlined padding, and disabled in either state. |
| `WrappedFieldTests` | 15 | The text field's classes, helper text and validation error reach the `TextBox` inside an `AutoCompleteBox` and a `NumericUpDown`, including a class set after the control is shown; and a `NumericUpDown`'s spin column sits inside the input box in each variant, on either side, the text 24px clear of it, drawing MudNumericField's chevrons rather than the select's triangles. |
| `MenuTests` | 6 | A menu row is MudMenuItem's 48px body1, in a `MenuFlyout` and a nested submenu too; the icon column opens on every row of a list with an icon, on none of a list without, and follows an icon set later. |
| `NavMenuTests` | 24 | `nav` on a `TreeView` as MudNavMenu: the link's padding and text, the active link in primary or a colour, the 12% under the pointer, the indent by depth, group titles, a group opening on a click without becoming active, its arrow and `hide-icon`, `dense` / `rounded` / `bordered`, the link icon, a disabled link, and a long label wrapping rather than the menu scrolling sideways, in a string header or a panel one, a link in the drawer's text colour, every row 44px whatever its header holds, and a type class keeping its own line. |
| `ParityOptionsTests` | 57 | The options added for 1.0: body1 list and select rows, `dense` and `no-gutters` on a list or a row, a list's and a tree's selection colour, a dense tree, every link colour, an alert's radius, `hide-icon`, a switch's unchecked colour, dense checkboxes and radios at every size, disabled ones in text-disabled, a dense menu, `no-underline`, `required` and `shrink-label` on a field and a select and passed through a wrapped field, a label over text in text-primary, the select's arrow turning up, `toolbar`, an icon's size and colour classes, a drawer's colour and its elevation on each side, and `drawer-header`. |
| `ToggleIconButtonTests` | 10 | `icon` on a `ToggleButton` as MudToggleIconButton: the round icon button, the colour class while checked and `unchecked-*` while not, filled and outlined, the second icon as ToggledIcon, a plain `ToggleButton` still a toggle item, and no label line box in the toggle icon themes, class or not. |
| `SnackbarTests` | 10 | The notification card as MudSnackbarElement: each severity's icon, `normal`, the close button closing the card in the card's colour, `hide-icon` and `hide-close`, and the text and outlined variants. |
| `TooltipTests` | 15 | A `ToolTip` has no width of its own, and one built as the `Tip` keeps its classes: the default chip, every colour with its contrast text, and `arrow` docked on the side facing the target for each of the four placements and absent for `Pointer`. |
| `PopupFadeTests` | 5 | Every popup fades in, as MudPopover does: a select's list on its first and second open, a submenu, a flyout and a tooltip are part way in just after opening; and the fade is 0 to 1 over 251ms at CSS's `ease`, read from the style because the headless clock stalls an animation followed frame by frame. |
| `RippleTests` | 43 | MudBlazor's ripple: a press centres a circle twice the control's longer side on the pointer, in the text colour, rising to 0.1 - 0.2 on filled buttons, filled chips and coloured FABs - with mudRipple.js's .5s growth, .1s rise and .4s fade, caught part way and read from its transitions; clipped to the control's size; gone once released; left button only; only the innermost control; a checkbox in its halo and not its label. Every MudBlazor ripple host has one - buttons by variant, split and drop-down buttons, list, select, menu, tree and nav rows, tabs and toggles, checkboxes, radios and switches in the glyph's colour - and a link, a panel header and a repeat button have none. Zero opacity in a region turns it off there; the opacities are the palette's in every variant. | A ripple is the first thing a MudBlazor user sees missing, and the one most easily drawn wrong: too strong, the wrong colour, spilling past a rounded corner, or rippling a list row under a button pressed inside it. |
| `MultiSelectionTests` | 17 | `multi-selection` on a list and a tree: the checkbox on each row, checked with the selection, 8px into the gutter in a 48px row, no row marked selected, grey or the list's colour on both glyphs and the halo, disabled in text-disabled, the tree's 32 / 24px box. And Avalonia pinning `SelectionMode` locally once a selection model exists, the reason the class cannot set it. |
| `CustomisationTests` | 25 | Glyph keys set on one control replace its glyph and no other's (a checkbox, a select's arrow, the tab scroll chevrons drawn at their own size, a snackbar's icons set on its manager), the submenu arrow and `hide-icon`, 48 / 36px list and select rows, `dense-items`, `label-start` / `-top` / `-bottom` on all three toggles, the switch's thumb icon and `thumb-*`, a disabled switch, a closed mini drawer's nav, and one button's hover and ripple turned off by keys in its own `Resources`. |
| `DatePickerTests` | 21 | A `CalendarDatePicker` is MudDatePicker: the standard field with the picker's classes and the `Event` icon button at its end, as tall as a text field of each variant with the icon on the text line; the popover's elevation 8, 100px primary toolbar and 310px calendar, the year and date fitting the toolbar, centred, on their own line heights; the year and the date in the case written, a long date shrunk to the 310px toolbar, each button changing the view; a 36px day, the selected one filled primary, today ringed in primary and the other months' days hidden; caption day names; the arrows' glyphs and metrics; subtitle months with the selected one h5 in primary; and a plain text field left as it was. | The pickers wore the forked Simple templates, which inside this theme's text field drew a label across the border and the calendar button outside the box. |
| `DrawerPageTests` | 10 | A closed compact `DrawerPage` is a mini drawer - text hidden, no indent, a 56px rail that does not scroll sideways - and a `CompactDrawerLength` the app sets wins. Its three pane toggles are MudIconButtons, open or closed: the icon toggle's theme, action-default, round, no fill under its template's `Background="Transparent"`, and the round hover tint. And the fix stays there: a plain checked toggle is still filled, and a `Background` an app sets beats the checked fill. |
| `TimePickerTests` | 19 | A `TimePicker` is MudTimePicker: the standard field with the picker's classes, `AutomationProperties.Name` as its label and the time as `HH:mm` or `hh:mm` with the period, "AM" and "PM" where the culture has none; the `AccessTime` icon button with MudIconButton's hover; a click or Enter on the read-only field opening it; `editable` taking a typed time, reverting text that is not one and clearing on empty; a text field's height in each variant, its icon centred on the box of a filled or outlined one; the 100px primary toolbar's 60px hour and minute following the columns before the time is accepted, the period not chosen at .54 and none on a 24-hour clock; the chosen row on a primary band; Close and Check icon buttons; the field stretching as the date picker's does, the popover hanging from the field's bottom-left, its paper flush with the field and 310px wide as the date picker's is, accepting committing the time and closing it. | The picker wore the forked Simple template: a button of grey blocks, nothing like the fields beside it. Then the popover opened far from the field: Avalonia's code moves its own popup to centre the chosen row, by an offset that came out infinite. |
| `CommandBarTests` | 7 | A `CommandBar` is MudToolBar: 64px with 24px gutters and no ground, 48 dense; each command a 48px MudIconButton with its label as the tooltip, or with `Right` a text button with a 20px icon and the label uppercase; `MoreVert` for the overflow, a MudMenu of 48px body1 rows as written, a row with no icon having no icon column; on an app bar the commands and the overflow button in its text colour; a checked toggle primary. | The bar wore the forked template: 68px tiles with 11px labels, and a menu with no surface of its own. |
| `TableViewTests` | 6 | A `TableView` is MudTable: surface at elevation 1, `outlined` a border and no shadow; subtitle2 headers over a table-lines rule; 16px cells with a rule under every row but the last; `dense` cells; stripes on odd rows and a primary-hover selection. | Avalonia's newer table wore the forked template, unlike the data grid beside it. |
| `PageTests` | 10 | A `ContentPage` on the background; a `NavigationPage`'s bar as MudAppBar - the appbar colours, 24px gutters, 64px, elevation 4 or 0 with `HasShadow`, an h6 header, an edge-start `ArrowBack` icon button in the bar's colour, the page's commands and overflow button in that colour too; a `TabbedPage` as MudTabs; a `Carousel` and a `CarouselPage` with MudCarousel's arrows and bullets, the arrows and bullets moving it, and `no-arrows` and `no-bullets`. | The page types and the carousel wore the forked templates; a navigation bar's overflow button stayed grey on the primary bar because a selector with no class loses to the icon button's own. |
| `TabScrollTests` | 14 | The tab bar's scroll buttons: none when the tabs fit, a bar's length per click and disabled at each end, the selected tab scrolled into view, a side bar scrolling down, a `TabStrip` too, and `centered` inside the scroller - a row that fits in the middle, one that overflows starting at the bar's start and reaching both ends. And `material-fixed-tabs`: fifteen tabs sharing a 600px bar in equal parts under the 160px minimum, with nothing to scroll; two filling it, `centered` or not; a side bar sharing its height; a `TabStrip` too. Selecting a tab scrolls only the bar, never the page round it; a disabled scroll button's arrow is action-disabled; and neither tab control clips its bar's shadow. |
| `PickerItemTests` | 2 | The date and time pickers' spinning items fit their 40px slots: their forked theme is based on Simple's list item, not the 48px themed one. |
| `TreeViewTests` | 3 | `hover` is opt-in, tints only the row under the pointer and not its branch, and leaves a selected row's tint alone. |
| `LinkInheritTests` | 1 | `HyperlinkButton.inherit` really takes the surrounding text colour. |

Several of these exist because the bug they pin actually shipped once: tabs and selects reached
by a descendant selector, a select with no error host, filled fields with no side padding because
a style lost a priority contest to a value written in the template. The test names say what was
wrong, so a failure reads as the sentence that broke.

## The pixel tests

Twelve frames: the gallery's **controls**, **sample screen**, **palette** and **custom styles** views, each in
**light**, **dark** and **high contrast**. Each is rendered by `GalleryFrames.Render`, the same
routine the gallery's `--capture` mode uses, and compared with `tst/.../Baselines/{view}-{variant}.png`.

**Why they matter:** the unit tests check values, and values can all be right while the picture
is wrong - a part laid out on top of another, a mask on the wrong ground, a glyph clipped by
its container. The frames are also the review: a theme change arrives with the PNGs it changed,
so the diff in a pull request is a picture rather than a description.

Four things keep a frame reproducible, and each was learned from a failure:

- **Animations that never end are masked.** An indeterminate `ProgressBar` and a pulsing skeleton
  follow a wall-clock animation, so two runs of the same build differ there. Their rectangles
  are found from the live tree and left out of the comparison; everything else stays strict. A
  `no-animation` skeleton holds still and is compared like everything else.
- **One-shot animations are waited out.** The capture waits until the longest animation that
  starts on load (the snackbar's 0.45s enter) has finished, then measures the page again, since a
  transition that moves layout (an expanded panel's margin) changes its height.
- **The culture is pinned.** The palette view formats contrast ratios with the current culture,
  so baselines recorded on a pt-PT machine (`6,00:1`) failed on an en-US runner (`6.00:1`).
  Frames render under the invariant culture.
- **The frame is drawn in one pass.** The capture used to read the window's own rendered frame,
  which is the sum of every partial redraw since it opened - and how many there were depends on
  timing. On a loaded machine, and on CI's slower runners, the antialiased ends of a large slider's
  rail and knob and a large switch's track came out up to 36 levels off on about one run in three,
  failing a build and then a release. Drawing the whole tree with `RenderTargetBitmap` has no
  history: fifteen runs out of fifteen were identical under full load. To check a fix like this,
  run the pixel suite while every core is busy; an idle machine hides it.

Each channel of each pixel may differ by 1/255, as headroom for a Skia release rounding an edge
differently. That is the only slack, and it should stay that small: a pixel that keeps failing by
a few levels means a capture that is not deterministic, which is how this one was found.

What is deliberately **not** done: there is no budget of differing pixels. A budget wide enough
to absorb an animation is wide enough to hide a redrawn glyph anywhere in the frame. And the
baselines are Windows/Skia only - text rasterises differently on Linux, so Linux does not run
them; a second committed set per platform is the answer if that is ever needed.

When a frame fails, the test prints how many pixels differ, by how much, and where, and writes
the rendered frame and a diff image (the baseline in grey, changed pixels in red, masked regions
in orange) to `FILI_PIXEL_DIFF_DIR`, or the system temp directory by default. A frame whose
*size* changed fails on that alone, before any pixel is compared: it writes the rendered frame
and no diff, deleting any diff an earlier run left there, so nothing in the folder can be
mistaken for this run's.

## In CI

Three workflows, in `.github/workflows/`.

### `build.yml` - every push and pull request to `master`

| Runner | Builds | Runs |
|---|---|---|
| `ubuntu-latest` | the whole solution, the WebAssembly gallery included | the unit and no-DataGrid tests |
| `windows-latest` | the three test projects, which reference every other project | the unit and no-DataGrid tests **and** the pixel tests |

- **Linux** is there to catch a Windows-only assumption in a library other people will build on
  whatever they have, and to build the browser gallery, which needs the `wasm-tools` workload.
  It does not run the pixel tests, for the rasterisation reason above.
- **Windows** runs the pixel tests because the baselines were rendered there.
- Every run uploads the test results (`.trx`) as `test-results-{os}`. A failing pixel run also
  uploads **`pixel-diffs`**, the rendered frames and diff images, kept for 14 days - so a change
  can be judged from the run page instead of reproduced locally first.
- `fail-fast` is off, so one runner failing does not cancel the other's answer.
- Nothing in it packs or publishes; the token is read-only.

### `release.yml` - a `v*` tag

Before anything is published, it **reruns both suites on Windows**. A tag on a commit that does
not pass does not reach nuget.org - and a nuget.org version can never be replaced, only unlisted,
so this is the last check that matters. It then refuses a tag that differs from `<Version>`,
packs, pushes through trusted publishing and creates the GitHub release.

### `pages.yml` - every push to `master`

Publishes the browser gallery to GitHub Pages. It runs no tests, but it is the check that the
WebAssembly build still works *as a page* - a font that only exists on desktop, or a style
include that trimming strips, shows up there and nowhere else.

## Running them

```bash
dotnet test tst/Fili.Theme.MudAvalonia.UnitTests                     # everything, seconds
dotnet test tst/Fili.Theme.MudAvalonia.UnitTests --filter-class "*FieldDetailTests"
dotnet test tst/Fili.Theme.MudAvalonia.PixelTests                    # the twelve frames
dotnet test tst/Fili.Theme.MudAvalonia.NoDataGridTests               # the theme without a DataGrid
```

```bash
# After changing ThemeColourGenerator: rewrite the generated files.
FILI_REGENERATE=1 dotnet test tst/Fili.Theme.MudAvalonia.UnitTests --filter-class "*GeneratedThemeTests"

# After an intended visual change: look at the diff first, then accept the new frames.
FILI_PIXEL_BASELINES=accept dotnet test tst/Fili.Theme.MudAvalonia.PixelTests
git restore $(git status --porcelain | grep png | grep -v "<the frames you meant to change>" | cut -c4-)
```

Accepting rewrites every frame, so restore the ones the change should not have touched, then run
the suite twice more: a frame that differs between two runs of the same build is an animation
that needs masking, not a baseline to accept. PowerShell sets the variables with
`$env:FILI_REGENERATE = "1"` and `$env:FILI_PIXEL_BASELINES = "accept"`.

## When you change the theme

| Change | Test that must change with it |
|---|---|
| a new token | an entry in `ResourceResolutionTests` and in `HighContrastTests`, with a high-contrast value that differs from dark |
| a new colour shade | `PaletteDerivationTests` checks it against `MudColorPort` |
| a new class | `ClassVocabularyTests`, and the README's class table |
| a colour rule on a control | the generator, never a hand edit; then `FILI_REGENERATE=1` |
| a new control theme | its key and target type in `StandaloneReadinessTests` |
| anything visible | the pixel baselines, reviewed and accepted in the same commit |

Every test body runs inside `UiThread.RunAsync`. Constructing almost any Avalonia object from the
test thread throws, and doing it once poisons the shared headless session, so every later test
in the run fails too - a missing wrapper fails the suite, not just its own test.
