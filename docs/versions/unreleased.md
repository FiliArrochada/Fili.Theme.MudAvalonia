# Unreleased

Work since 0.28.0, kept here as it lands; at the bump this file is renamed to its version, and this
line goes.

[All versions](README.md)

`FiliFontSize` now does what its page says, and the published keys and classes are frozen in a file
the tests hold the theme to. Four things Avalonia's own controls do, and this theme's templates had
silently dropped, work again: an editable select takes typing, a scroll bar's trough pages, a
calendar takes the keyboard, and its keyboard position shows. A standard field's resting label
moves down onto its text line ([Upgrading](#upgrading-from-0280-unreleased)). A screen reader
now hears a name on every icon button the theme draws, in words an app can translate, and an
upper-case label as it was written. The docs gain an Accessibility page, say which Avalonia
versions the package needs, and correct what they said about a DataGrid out of range, reduced
motion and a window's own variant.

## What changed

### The interface and releases

- **`FiliFontSize` is the app-wide text size.** Every window and popup takes its size from it,
  where they read `FiliBody2FontSize`, so an app that sets it in the application's `Resources`
  scales all unclassed text. Its default is body2's 14, as before. `FiliLineHeight` and
  `FiliLetterSpacing` are documented for what they are: the baseline's line and tracking for an
  app's own styles, which the theme puts on no unclassed text.
- **The published interface is frozen in `published-baseline.txt`.** Every published key with the
  type it resolves to in each variant, and every class, in a test file that only grows. A key or a
  class removed from the source and its documentation together now fails the build, as does a key
  that changes type.

### Controls

- **An editable select takes typing.** `IsEditable="True"` on a ComboBox took focus and ignored
  every key: the template had no `PART_EditableTextBox`, which Avalonia looks up without
  complaint. The input now sits in the selected item's place inside the same field; typed text
  floats the label as a value does, and the input is the one Tab stop.
- **A scroll bar keeps Avalonia's behaviour.** A click on the trough pages again, the content
  waits for the release under `ScrollViewer.IsDeferredScrollingEnabled`, and a right-click opens
  Avalonia's Scroll here / Top / Page up menu. The rail looks as it did: the page buttons are
  transparent, and there are still no line buttons.
- **A calendar takes the keyboard, and shows where it is.** The calendar is now a Tab stop of its
  own, where nothing in it could take focus, and the day, month or year the arrow keys are on has
  the focus ring's 2px of text-primary round it. Before, in the year and decade views and in a
  range selection, nothing showed it.
- **A standard field's label rests on its text line.** Empty and unfocused, a standard text field's
  or select's label sat centred in the field, 7px above the line its value is typed on (6.5 dense);
  it now rests where `_inputlabel.scss` puts it. Filled and outlined labels stay centred in their
  box, as MudBlazor's are. Fields built on the text field (number, date, time, autocomplete) follow.
- **The time picker's popover has a height limit, Avalonia's 398px,** so the offset Avalonia works
  out from it for its own, empty, popup is finite. The popover is 372px and is not cut.

### Accessibility

- **Every icon button in the theme's own templates has a name.** The time picker's OK and Cancel,
  a number field's spinners, a calendar's and a carousel's arrows, a snackbar's close and a text
  field's clear button were read by their content's type ("Avalonia.Controls.PathIcon"), and the
  clear button's "Clear" was fixed English. Each now reads a new `FiliString*` key
  ([Published keys](../published-keys.md#strings)), English by default, which an app sets in its
  `Resources` to translate. The time picker's icon button is hidden from automation, as the date
  picker's is: the field is the picker's one name.
- **An upper-case label reads as written.** A button's, FAB's, split button's or tab's label is
  drawn upper case, and a screen reader read that drawn text, "SAVE", which some readers spell out.
  The control is now named with its label as written, below any name the app sets.
- **High contrast:** the disabled-text figure on the page is 13.6:1, not "about 12:1".

### Fixes

- **A menu opened from an icon button keeps its own icon size.** `Button.icon.small` and `.large`
  sized every `PathIcon` they reached, and a selector reaches into the menus, flyouts and tooltips
  a control opens, so a kebab menu on a small icon button drew its rows' icons at 20px (36 on a
  large one). They size the button's own icon only, as does `Button.fab`. A DataGrid's dense
  checkbox likewise stops at the cell's own boxes and no longer shrinks one in a flyout opened
  from a cell or header.
- **A press no longer keeps its window alive.** The ripple remembered the last press until the
  next one anywhere, and with it the pressed control's window after it closed; an app's leak
  test found the last-clicked window still reachable. It lets go once the press has routed.

### Documentation

- **An Accessibility page** on the docs site, and a section in the README: what the theme does
  for keyboard, screen-reader and low-vision users, what it leaves to the app (its own icon-only
  buttons' names, following the system's contrast and motion settings, announcing validation
  errors), and the contrast of the default light and dark pairs that fall below WCAG AA. No
  conformance is claimed.
- **The Avalonia requirement is stated:** 12.1.4 or later, below 13, with every `Avalonia*`
  package on one version, and what NU1605 on restore means. A control type a later Avalonia 12
  adds stays unthemed until a theme release covers it.
- **A DataGrid outside its supported range shows nothing**, not its own package's look as two
  pages said: the theme skips its templates, and Avalonia has no default theme.
- **Troubleshooting** covers FluentTheme left in, an include built in C# and trimmed away, the
  browser's missing scripts, `x:Key="HighContrast"` and NU1605, and links the published keys.
- **Theming** shows the font, the corner radius and density: a replacement font needs the Light,
  Regular and Medium faces, `FiliCornerRadius` sets every control's corners, and `dense` is set
  per control, with no app-wide switch.
- **Reduced motion** no longer says it stops everything: the field's focus underline, a data
  grid's sort arrow and a slider's value label join the list it does not reach yet, and the page
  says plainly that Avalonia does not report the system's setting.
- **A window's own variant** is described as it behaves: it does not restyle the window, where
  the docs said half of it followed.
- **A plain style outranks an app's own `ControlTheme`** for a few `SplitView`, `DrawerPage` and
  `DatePicker` values, now in [Compatibility](../compatibility.md).
- A bug-report issue template, and a "report a bug" row in the README.

## Upgrading from 0.28.0 (unreleased)

- **An app that set `FiliFontSize`** sees it take effect: its windows' and popups' unclassed text
  takes that size. Remove the setting to keep 14, or set it to the size wanted.
- **A standard text field or select with no value** shows its label 7px lower (5px dense), on the
  text line. Nothing to change; a pixel baseline of such a field moves.
- **A Calendar** is now a Tab stop, so Tab order through a page with one gains a stop. Set
  `Focusable="False"` on it to keep it out.
- **An icon button whose icon sits inside a panel or another control** (not the button's direct
  content) no longer has the icon sized for it: put the `PathIcon` directly in the button, or size
  it. An icon directly in the button, as the docs show, is unchanged.
- **A DataGrid template column** whose checkbox is nested deeper than the cell's root panel keeps
  a standard checkbox's size. A box directly in the cell or in its root panel is unchanged.
- **A scroll bar** pages on a trough click and has a context menu. An app that relied on the
  trough doing nothing can set `ContextFlyout="{x:Null}"`; the paging is Avalonia's own behaviour.
