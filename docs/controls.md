# Controls

How each control this theme templates is used, which MudBlazor parameter each class stands for,
and where the values came from. Installing the theme and the full class list are in the
[README](../README.md); every parameter mapped row by row is in [MudBlazor parity](mudblazor-parity.md).

## Buttons

MudButton's whole Variant × Color × Size matrix, one class for each parameter:

```xml
<Button Content="Learn more" />                         text, default colour — MudButton's defaults
<Button Classes="primary" Content="Learn more" />       text, primary
<Button Classes="outlined error" Content="Delete" />    outlined, error
<Button Classes="filled primary" Content="Save" />      filled, primary, elevation 2/4/8
<Button Classes="filled success small" Content="OK" />  filled, success, small
<Button Classes="inherit" Content="Sign in" />          the surrounding text colour
```

- **Variant:** `text` (the default), `outlined`, `filled`. Each is a `ControlTheme`.
- **Colour:** `primary`, `secondary`, `tertiary`, `info`, `success`, `warning`, `error`, `dark`,
  `inherit`, or none for `Color.Default`.
- **Size:** `small`, `large`, or none for `Size.Medium`.

**The label is uppercase.** MudBlazor's `ButtonTypography.TextTransform` is `uppercase`, and
`.mud-button`, `.mud-fab` and `.mud-tab` apply it, so `Content="Save"` reads SAVE on a button, a
split or drop-down button, a toggle item, an extended FAB and a tab header. A chip, a link, a
checkbox, a menu row and an expansion panel's header keep their case, as they do in MudBlazor.
Avalonia has no text-transform, so a string is transformed on its way into the template's
presenter, and the case is inherited, as CSS's is: a `TextBlock` in content that is a control - the
label beside a `start-icon` - keeps its `Text`, binding and all, and draws it in the case through a
run of its own. A `TextBlock` with runs you wrote keeps them as written. `TextBlock.button`
(`Typo.button`) takes the case too. An access key survives: `_Save` is SAVE with Alt+S. `FiliButtonTextTransform` is the switch, in
CSS's words, as MudBlazor's typography setting is:

```xml
<Application.Resources>
  <x:String x:Key="FiliButtonTextTransform">none</x:String>     labels as written, everywhere
</Application.Resources>
```

`lowercase` and `capitalize` work too, and the key can go in any control's `Resources` to change
one region. That region is often needed: in Avalonia a `Button` is also the usual clickable
container - a poster card, a list row, a button showing a value such as the current sort - and
its text reads in capitals like any button label. Set `none` on the card rail or the list, as
MudBlazor would never have put that text in a MudButton.

Not on a button with a `Flyout`: the flyout's content finds its resources through the button,
so its action buttons would lose their capitals too, where MudBlazor's popover, rendered at the
layout root, keeps them. For one button's own label, write the label as a run, which keeps the
case written while everything else stays a button:

```xml
<Button>
  <TextBlock><Run Text="{Binding PeriodLabel}" /></TextBlock>
  <Button.Flyout>…</Button.Flyout>
</Button>
```

**A press ripples.** As in MudBlazor, a circle grows from the pointer over .5s and fades once the
button is released, in the label's colour at 10% - 20% on a filled button, a filled chip or a
coloured FAB. The same ripple is on icon buttons, chips, FABs, split and drop-down buttons, toggle
items, tabs, list, select, menu, tree and nav rows, and in the halo of a checkbox, radio or
switch, where it takes the glyph's colour. A link and an expansion panel's header do not ripple,
as they do not in MudBlazor, and nor does a key press. `Ripple="false"` is both opacities at zero
in a control's `Resources`, or the application's to turn it off everywhere:

```xml
<Button Content="Quiet">
  <Button.Resources>
    <x:Double x:Key="FiliRippleOpacity">0</x:Double>
    <x:Double x:Key="FiliRippleOpacitySecondary">0</x:Double>
  </Button.Resources>
</Button>
```

**A colour class is only a colour**, as `Color` is in MudBlazor: `primary` on its own is a primary
*text* button, and the fill comes from `filled`. With no colour, a button is `text-primary` with a
grey hover, which is MudBlazor's default and not Material's primary one.

Every value is from `_button.scss`. Text and outlined buttons tint with the colour at 6%
(`{color}-hover`) on hover, focus and press. Filled ones switch to `{color}-darken` and lift 2 → 4
→ 8. Outlined's line is the colour itself, because `BorderOpacity` is 1.0. The darker and lighter
shades are MudBlazor's own derivation, HSL lightness ∓ 0.075 with .NET's rounding, ported to C# in
`MudColorPort`; `PaletteDerivationTests` checks every shade token against it in all three variants,
and the port against the 14 values mudblazor.com publishes. `Button.axaml` is written by
`ThemeColourGenerator` from one list of colours, so no colour can differ from the others, and
`GeneratedThemeTests` fails if the file is edited by hand. `ButtonMatrixTests` checks every cell of
the matrix, at rest and on a real pointer hover. Both helpers live in the unit-test project, under
`Generation/`.

**`icon` on a `ToggleButton` is MudToggleIconButton**, in the same three variants, generated from
the same rules retargeted at `ToggleButton`. Its two colours are two classes: a colour class is
`ToggledColor`, painted while checked, and `unchecked-{color}` is `Color`, painted while not;
with neither it is `action-default`, both parameters' default. For `ToggledIcon`, put two icons
in a `Panel`, `Icon` first: the second shows only while checked.

```xml
<ToggleButton Classes="icon error"><PathIcon Data="{StaticResource Heart}" /></ToggleButton>
<ToggleButton Classes="icon primary unchecked-secondary">
  <Panel>
    <PathIcon Data="{StaticResource Bell}" />   Icon
    <PathIcon Data="{StaticResource Mail}" />   ToggledIcon
  </Panel>
</ToggleButton>
```

A `ToggleButton` without `icon` is MudToggleItem; see [Lists, selects and the things built out of
them](#lists-selects-and-the-things-built-out-of-them).

## Split and drop-down buttons

`SplitButton` is a two-segment `MudButtonGroup`; `DropDownButton` is a `MudMenu` whose activator
is a button.

```xml
<SplitButton Content="Save" />                         text group, text-primary
<SplitButton Classes="outlined primary" Content="Export" />
<SplitButton Classes="filled primary" Content="Deploy" />
```

**The split button takes Button's classes, meaning the same things**: `text` by default,
`outlined` and `filled`, a colour class that is only a colour, `small` and `large`. That is
MudButtonGroup's own Variant × Color × Size, and the same generator writes it
(`ThemeColourGenerator`, into a marked region of `SplitButton.axaml`). The line between the halves
follows the group's rule: text-primary or the colour for a text group, the colour's own border
for an outlined one, `divider` for a filled one with no colour, and the colour's lighten shade
between filled coloured segments.

`_buttongroup.scss` is almost entirely about joining segments into one shape: a non-last child
loses its right-hand corner radii, a non-first child loses its left-hand ones and pulls back by
`-1px` so two borders collapse into one, and a text group draws `border-left: 1px solid
rgba(text-primary, border-opacity)` between segments.

**`BorderOpacity` is 1.0 in `Palette.cs`**, so that divider is text-primary at full strength, not
a hairline. It looks like a mistake in a screenshot and is not one.

The `-1px` does not appear here: Avalonia's `SplitButton` already puts a dedicated separator
element between its halves, so there are no two borders to collapse. Same result, different
route — which is the usual shape of transcribing CSS onto a templated control.

**A group of any length is `button-group` on a `StackPanel` of buttons**, across or, with
`Orientation="Vertical"`, down:

```xml
<StackPanel Classes="button-group" Orientation="Horizontal">
  <Button Classes="outlined primary" Content="One" />
  <Button Classes="outlined primary" Content="Two" />
  <Button Classes="outlined primary" Content="Three" />
</StackPanel>
```

The rules are the same: the radius on the outer corners only, each button after the first pulled
back 1px so two outlined lines are one, a 1px line in its colour before each text button after
the first, and `{color}-lighten` (`divider` with no colour) before each filled one. A filled
button rests flat in a group and lifts to elevation 4. Each button keeps its own classes:
MudButtonGroup's `OverrideStyles` hands the group's Variant, Color and Size down, and nothing
cascades here, so put them on each button. A hidden button leaves the group.

## Text fields

```xml
<TextBox PlaceholderText="Email" />                              standard — the default
<TextBox Classes="filled"   PlaceholderText="Email" />
<TextBox Classes="outlined" PlaceholderText="Email" />
<TextBox Classes="outlined error" PlaceholderText="Email" Text="nope" />
```

**Standard is the base and the other two derive from it**, mirroring `_input.scss` where
`.mud-input` carries the box model — and matching `MudTextField.Variant`, which defaults to
`Variant.Text`. The three differ only in padding and how far the label travels:

| variant | content padding | label resting | label floated |
|---|---|---|---|
| standard | `6px 0 7px` | `translate(0, 24)` | `translate(0, 1.5) scale(.75)` |
| filled | `12px` horizontal | `translate(12, 20)` | `translate(12, 10) scale(.75)` |
| outlined | `14px` horizontal | `translate(14, 20)` | `translate(0, 1.5) scale(.75)` |

**`PlaceholderText` doubles as the floating label.** Avalonia's `TextBox` has no `Label` property,
and adding an attached one would be the first real API in a package whose point is not having any.
The cost: these variants have no separate placeholder — the label occupies that slot until it
floats. If both are ever needed, a `FiliTextField.Label` attached property is the follow-up, in a
package that depends on this one.

**A field with no `PlaceholderText` has no label, and no room for one**, as a MudTextField with
no `Label` takes none (`mud-input-{variant}-with-label` is only added with a label). A standard
field is then MudInput, the bare input: 32px, 29 dense. A filled one centres its text in the same
56px (`.mud-input-root-hidden-label`). An outlined one is the same box. Set `PlaceholderText`
later and the label's room comes back.

The label floats when the field is **focused or non-empty**, which is expressible as a selector
only because Avalonia gives `TextBox` an `:empty` pseudo-class.

**Two ways to be invalid, and both paint the same.** `Classes="error"` is the one you set by hand
(`MudTextField.Error`); the other fires on its own when a *binding* fails validation and Avalonia
sets `DataValidationErrors.HasErrors`. The template hosts a `DataValidationErrors`, so the message
renders **under the field** at 12px the way MudBlazor's helper text does — not as the forked
template's red circle with a tooltip off to the right. Before this was wired, a failing binding
recoloured nothing and showed nothing.

**Under the field is MudInputControl's helper line**, one row for three things:

```xml
<TextBox Classes="filled" PlaceholderText="Email" AutomationProperties.HelpText="We never share it" />
<TextBox Classes="outlined counter" PlaceholderText="Title" MaxLength="40" />     17 / 40
<TextBox Classes="dense" PlaceholderText="Email" />                              Margin.Dense
```

- **Helper text is `AutomationProperties.HelpText`**, Avalonia's own attached property, so it
  costs no API and a screen reader announces it, as MudBlazor wires `HelperText` into
  `aria-describedby`. An error takes its place, as MudBlazor's does.
- **`counter` is `MudTextField.Counter` over `MaxLength`**: `17 / 40` on the right, or the length
  alone when `MaxLength` is 0, which is `Counter="0"`. It turns red with the field.
- **`dense` is `Margin.Dense`**: 3px off a standard field, 8px off a filled one, 16px off an
  outlined one, from `.mud-input-root-margin-dense`, with the label resting and floating to match.

Three more of MudTextField's parameters are classes:

- **`required` is `Required`**: `*` straight after the label, as `.mud-input-label::after` adds it.
  The label's text is `PlaceholderText` then its `Tag`, which the class sets.
- **`shrink-label` is `ShrinkLabel`**: the label floated with nothing typed.
- **`no-underline` is `Underline="false"`**: neither the resting rule nor the focus one.

A floated label is text-primary once its text holds it up, and primary while the field has focus,
as `.mud-shrink ~ label` and `:focus-within ~ label` colour it.

A `ComboBox` takes the same: `filled`, `outlined`, `dense`, `error`, `required`, `shrink-label`,
`no-underline` and helper text. A select used to have no error host at all, so a failing binding
on one showed nothing. Its `ArrowDropDown` turns to `ArrowDropUp` while the list is open, as
MudSelect's `OpenIcon`. The arrow is `action-default`; `adornment-{colour}` is MudSelect's
`AdornmentColor`, and `adornment-inherit` gives it the colour round the select. Disabled, it is
text-disabled whatever its class.

**Filled and outlined fields had no side padding until 0.2.0**, though the themes declared 12px
and 14px. The template wrote the content margin inline, which binds at Template priority, and a
variant's plain `^ /template/` style cannot beat that; see [the trap](traps.md#styles-priority-and-selectors).
`VariantsPadTheirContentFromTheSide` now pins it.

The outlined variant masks the border behind the floated label with an opaque patch rather than
cutting a real notch in the stroke — a notch needs a generated `Geometry`, and the mask is
indistinguishable when the field sits on `FiliSurface`.

## Date picker

**`CalendarDatePicker` is MudDatePicker**, with nothing to opt into. Closed, it is the standard text
field - the field's own theme, untouched - with MudPicker's adornment, the `Event` icon in an icon
button at the end; `PlaceholderText` is the floating label. A field's classes go on the picker and
reach the field inside it, as a number field's do: `outlined`, `filled`, `dense`, `error`,
`required` and helper text through `AutomationProperties.HelpText`.

```xml
<CalendarDatePicker PlaceholderText="Due date" SelectedDate="{Binding Due}" />
<CalendarDatePicker Classes="outlined dense" PlaceholderText="Start" />
```

Open, it is MudPicker's popover: a paper at elevation 8, the primary toolbar with the year and the
date ("Sat, 11 Oct", in the case written), and the calendar. A day is a 36px circle; the selected
one is filled primary and today is ringed in primary, as MudDatePicker always marks it, where
Avalonia's picker would not (`IsTodayHighlighted="False"` still turns it off). Days of the months
either side are hidden. The year in the toolbar opens the years and the date goes back to the
selected month; the month's name between the arrows steps up to the months, as MudBlazor's does.
A `Calendar` on its own is the static picker's calendar, without the toolbar or the paper.

| MudBlazor | Here |
|---|---|
| `AdornmentIcon` (`Event`) | `FiliEventGeometry`, in the picker's `Resources` to swap it |
| `PreviousIcon`, `NextIcon` | `FiliNavigateBeforeGeometry`, `FiliChevronRightGeometry` |
| `TitleDateFormat` (`ddd, dd MMM`) | the same, in the current culture |
| `Color` (Primary) | primary, the toolbar and the selected day |
| `Editable` | on: the date can be typed, as Avalonia's picker allows |

Two parts are Avalonia's own: the day names are its shortest (S M T), and the years are a decade
in a grid. See [Known gaps](known-gaps.md).

## Time picker

**`TimePicker` is MudTimePicker**, with nothing to opt into. Closed, it is the standard text field
holding the time as MudTimePicker writes it - `14:30`, or `02:30 PM` on a 12-hour clock - with
MudPicker's adornment, `AccessTime` in an icon button at the end. `TimePicker` has no
`PlaceholderText`, so the label is `AutomationProperties.Name`, Avalonia's own property, which a
screen reader announces too. A field's classes go on the picker, as a date picker's do. Read-only,
as MudTimePicker is by default, a click on the field, or Enter, opens the picker; the icon button
opens it too, with MudIconButton's hover. `editable` is MudTimePicker's `Editable`: the time is
typed - `14:30`, `2:30 PM` - and taken on Enter or when the field loses focus, empty text clearing
it and anything that is not a time going back to the time held.

```xml
<TimePicker AutomationProperties.Name="Starts at" SelectedTime="{Binding Start}" />
<TimePicker Classes="editable" AutomationProperties.Name="Alarm" SelectedTime="{Binding Alarm}" />
<TimePicker Classes="outlined dense" ClockIdentifier="12HourClock" AutomationProperties.Name="Alarm" />
```

Open, it is MudPicker's popover: a paper at elevation 8 and MudTimePicker's toolbar on primary -
the hour and the minute at h2's 60px light, with AM and PM beside them on a 12-hour clock, the one
not chosen at .54 - showing the time as it is picked. Under the toolbar MudBlazor draws a clock
face, a control of its own; here are Avalonia's hour and minute columns, body1 rows with the
chosen one on a primary band, as the clock's chosen number is. Avalonia's picker commits on
accept, so the popover ends in Close and Check icon buttons. Where the culture has no AM and PM
(pt-PT among them), the picker writes "AM" and "PM", as MudBlazor's toolbar does.

| MudBlazor | Here |
|---|---|
| `AdornmentIcon` (`AccessTime`) | `FiliAccessTimeGeometry`, in the picker's `Resources` to swap it |
| `AmPm` | `ClockIdentifier="12HourClock"` |
| `TimeFormat` (`HH:mm`, `hh:mm tt`) | the same; `UseSeconds` adds `:ss` |
| `Label` | `AutomationProperties.Name` |
| `Editable` | `editable` |
| the clock face | Avalonia's columns; see [Known gaps](known-gaps.md) |

## Selection controls

```xml
<CheckBox     Content="Enable sync" />
<RadioButton  Content="Weekly" GroupName="cadence" />
<ToggleSwitch Content="Dark mode" />
```

With no class they are MudBlazor's `Color.Default`, which is **grey, not primary**: the icon
button a checkbox or radio glyph sits in is `action-default`, checked or not, and a switch keeps
`_switch.scss`'s literal `#fafafa` thumb on and off. A colour class is `Color` — on a checkbox or
radio it paints the glyph in every state, on a switch the thumb and track only when on — and
`small` / `large` are MudIcon's 20 and 36px and the switch's own spans:

```xml
<CheckBox Classes="primary" Content="Enable sync" />
<ToggleSwitch Classes="success large" Content="Online" />
```

**MudBlazor does not draw them — it renders Material icons.** `MudCheckBox` picks between
`Icons.Material.Filled.CheckBox`, `CheckBoxOutlineBlank` and `IndeterminateCheckBox`; `MudRadio`
between `RadioButtonChecked` and `RadioButtonUnchecked`. So a hand-drawn box with a stroked tick
is not a close approximation, it is a *different artefact* — it cannot match the glyph's corner
geometry or the indeterminate bar. `Themes/Icons.axaml` carries those glyphs as `StreamGeometry`,
transcribed from `Icons/Material/Filled.cs`, and the templates swap between them.

- **CheckBox / RadioButton** — the real glyphs at 24px, swapped per state. Note the radio: checked
  grows an inner disc inside the ring rather than filling it; filling it is the usual mistake and
  the result reads as a round checkbox.
- **ToggleSwitch** — a 20px thumb that is *larger* than its 14px track, overhanging it above and
  below, with elevation under the thumb. The track is `action-default` at 48%, lifting to 50% when
  on, and only a colour class changes its colour.

A checkbox and a radio centre a circular 48px state layer on the glyph rather than tinting the
whole row: `.mud-icon-button`'s 12px round the 24px glyph, 44px small and 60px large, and 4px
round it when `dense`. Disabled, the glyph and the label are text-disabled, as
`.mud-checkbox.mud-disabled *` colours every part. A disabled switch keeps its track's colour at
12% under a gray-default thumb.

**`inherit` is `Color.Inherit`**, on a checkbox or radio: the glyph in the colour the control
sits in, as `.mud-inherit-text` is `color: inherit`, and no halo, since MudBlazor has no
inherit-hover rule for it to light. The label keeps its text-primary. `unchecked-inherit` is
`UncheckedColor.Inherit`, the same while not checked only:

```xml
<CheckBox Classes="inherit" />                        in an app bar, the bar's text colour
<CheckBox Classes="primary unchecked-inherit" />      primary when checked, the surroundings when not
```

**`label-start`, `label-top` and `label-bottom` are `LabelPlacement`**, on a checkbox, radio or
switch: the label before the glyph, above it or below it, the two centred when stacked. End, the
default, needs no class.

**An icon parameter is a resource key.** The glyphs are `DynamicResource`s, so `CheckedIcon`,
`UncheckedIcon` and `IndeterminateIcon` are the checkbox's three keys set in its `Resources`, and
MudSwitch's `ThumbIcon` is `FiliSwitchThumbIconGeometry`, empty until set. The thumb draws it at
16px in text-secondary; `thumb-primary` … `thumb-dark` is `ThumbIconColor`.

```xml
<ToggleSwitch Classes="thumb-success" Content="Wi-Fi">
  <ToggleSwitch.Resources>
    <StreamGeometry x:Key="FiliSwitchThumbIconGeometry">M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z</StreamGeometry>
  </ToggleSwitch.Resources>
</ToggleSwitch>
```

Every other icon parameter works the same way: the table at the top of
[MudBlazor parity](mudblazor-parity.md) names each key.

**One trap in transcribing a Material glyph.** Each one carries an `M0 0h24v24H0z` viewbox spacer,
which is a transparent bounding rect in SVG and a *painted square* in a filled `StreamGeometry`.
It is dropped; bounds are preserved by drawing at `Width`/`Height` 24 with `Stretch="None"`, since
the data is already in 0–24 space.

## Slider and tabs

```xml
<Slider Value="40" />
<TabControl> <TabItem Header="One" /> </TabControl>
```

- **Slider** — `MudSlider` defaults to `Size.Small`, `Color.Primary` and `Variant.Text`: a 2px
  rail and a 12px thumb, the whole rail the colour at 30%. `medium` and `large` are 4px / 20px and
  6px / 24px, and a colour class repaints it. The thumb does not grow: MudBlazor rings it, 1px of
  the colour at 24% on hover and 2px on focus and press.

  ```xml
  <Slider Classes="filled" Value="40" />                                   Variant.Filled
  <Slider TickPlacement="BottomRight" TickFrequency="10" Value="50" />     TickMarks
  <Slider Classes="value-label" Value="75" />                              ValueLabel
  ```

  `filled` draws the colour solid up to the thumb. Tick marks are Avalonia's own `TickPlacement`
  and `TickFrequency` (or `Ticks`), drawn as MudBlazor draws them: dots on the rail, its thickness
  and the colour. `value-label` shows the value in a chip of the colour above the thumb while it is
  held, in the app's number format. Structure is dictated by Avalonia, not Material: `Slider`
  requires a `Track` named `PART_Track` with two `RepeatButton`s, which here are the hit areas a
  click pages through and, for the decrease half, the `filled` part; the 30% rail and the dots sit
  behind them. Both orientations need their own `Template`; a horizontal one applied to a
  vertical slider renders sideways rather than degrading.
- **TabControl** — MudTabs' 48px bar with a 2px primary indicator. MudTabs' defaults decide the
  rest, and this theme used to differ on each: the bar is in the surface colour with **no** rule under it
  (`Border` defaults to false), tabs are text-primary rather than text-secondary, and
  `MinimumTabWidth` is 160px. `ItemContainerTheme` is what carries the header theme down to a bare
  `<TabItem>`; without it the control is themed and its headers are not, which looks like the
  theme half-applied.

```xml
<TabControl Classes="primary">…</TabControl>              MudTabs Color: the bar in the colour
<TabStrip Classes="border centered">…</TabStrip>          Border, Centered
<TabControl Classes="outlined rounded">…</TabControl>     Outlined, Rounded
<TabControl Classes="material-fixed-tabs">…</TabControl>  Material's fixed tabs: equal shares, no scrolling
```

A colour class is MudTabs' `Color`, the colour of the **bar** - the tabs and the indicator take
its contrast text, and the active tab's hover is `{color}-lighten`. A tab's theme cannot see its
parent's class, so those are `TabControl.primary > TabItem` child selectors in `FiliTheme.axaml`;
a descendant selector would also repaint a `TabControl` nested in the content.

The indicator slides between tabs, as MudTabs' does: over .3s on cubic-bezier(.64,.09,.08,1),
moving its position while its width jumps to the new tab's. Each tab still has its own indicator,
so every class above reaches it; `SlidingIndicator` starts the newly selected one where the old
one was drawn and lets its transition bring it home, from wherever an interrupted slide had got
to. The tabs do not clip, or it would vanish outside its own tab. `no-animation` on the bar is
MudTabs' `SliderAnimation="false"`, as it is a skeleton's `Animation.False`: the new tab's
indicator is under it at once.

**Tabs that do not fit the bar scroll**, as MudTabs' do: the tab row sits in a `ScrollViewer`
with no scroll bars, between a ChevronLeft and a ChevronRight icon button that show only while
the row overflows. Each is disabled at its end and scrolls a bar's length per click, and the
selected tab is scrolled into view. On a bar at the left or right they sit above and below, the
bar's width and 32px high, the chevrons turned a quarter. `TabBarScroll` does the comparing,
since whether the row overflows is the scroller's extent against its viewport.

**`TabStrip` is the same thing without a content area.** Avalonia splits the header row and the
tab control into two types; MudBlazor does not, so both carry the same metrics. They are written
out twice rather than shared, because a `ControlTheme` cannot be `BasedOn` one whose `TargetType`
differs — if you change one, change the other, which is why the gallery shows them together.

**`PipsPager` is MudCarousel's bullets, not `MudPagination`.** Avalonia has no numbered pager, and
`MudCarousel.razor.cs` names the parts exactly: `CheckedIcon` is `RadioButtonChecked`,
`UncheckedIcon` is `RadioButtonUnchecked`, and the arrows are `NavigateBefore`/`NavigateNext`. So
the pips are **the same two glyphs the radio button uses** — selecting one swaps the glyph, a ring
gaining a disc, rather than a dot changing colour. The 0.75 opacity on the row is transcribed from
the inline style in that razor file. (`NavigateNext` turns out to be byte-identical to
`ChevronRight`, so `Icons.axaml` carries one path under the name it was first needed for.)

**`Carousel` is MudCarousel**, with `ShowArrows` and `ShowBullets` on, as MudBlazor's are by
default: `NavigateBefore` and `NavigateNext` icon buttons at the sides, centred, and the bullets
along the bottom - a `PipsPager`, drawn as above - all in the carousel's text colour at .75.
`no-arrows` is `ShowArrows="false"` and `no-bullets` is `ShowBullets="false"`. The arrows are for
the pointer; the carousel itself is the Tab stop. A `CarouselPage` is a carousel of pages, so it
has the same arrows and bullets.

```xml
<Carousel>
  <Image Source="/Assets/one.jpg" />
  <Image Source="/Assets/two.jpg" />
</Carousel>
<Carousel Classes="no-arrows" ItemsSource="{Binding Slides}" />
```

## Progress, dividers, panels and links

```xml
<ProgressBar Value="65" />                                4px, square — the MudBlazor defaults
<ProgressBar Classes="primary large rounded" Value="65" />
<ProgressBar Classes="circular" Value="65" />            MudProgressCircular, 40px
<ProgressBar Classes="circular primary small" IsIndeterminate="True" />
<Separator />                                             1px, no margin
<Separator Classes="inset" />                             72px indent, to clear an avatar column
<Expander Header="Details"> … </Expander>
<HyperlinkButton Content="Learn more" />
```

Five places where the MudBlazor default is not the one you would guess:

- **A bare `ProgressBar` is a 4px square hairline.** `MudProgressLinear.Size` defaults to
  `Size.Small` and `Rounded` to `false`; `medium` and `large` are 8px and 12px, `rounded` opts
  into the 4px radius. The track is always the bar's own colour at 20% — only `Color.Default`
  splits them (track `action-disabled`, bar `action-default`), which is why `Background` and
  `Foreground` are the two knobs.
- **`circular` is MudProgressCircular, and it is grey with no colour.** Its default is
  `text-secondary`, not primary, and there is no track. It is a theme the same `ProgressBar` takes,
  as `chip` is one a `Button` takes, so `Value`, `Minimum`, `Maximum` and `IsIndeterminate` all
  carry over: 40px (`small` 24, `large` 56), starting at twelve o'clock and filling clockwise.
  `rounded` rounds the ends and `ShowProgressText` puts the value in the middle, which is where
  MudBlazor puts its `ChildContent`.
- **A `Separator` has `margin: 0`.** The forked Simple template ships `Margin="29,1,0,1"`, a
  menu-shaped indent baked into every divider, which used to survive because the theme only
  recoloured it. `inset` (72px) and `middle` (16px) are MudBlazor's real variants.
- **An `Expander` header is 15px**, which is neither body1 (16) nor body2 (14) — MudBlazor sizes
  `.mud-expand-panel-header` on its own at `.9375rem`. The panel carries a surface and elevation 1,
  as each MudExpansionPanel does. MudExpansionPanels' options are on the panel: the elevation
  classes (`elevation0` when it already sits on a card), `CornerRadius="0"` for `Square`, `dense`
  (no padding above and below the content) and `no-gutters` (`Gutters="false"`: no 24px sides on
  the header or the content). Opening grows the content into view over .3s `ease-in-out`, as
  MudCollapse does, rather than fading it in; a panel that starts open is open on its first frame.
- **`expansion-panels` on a `StackPanel` is MudExpansionPanels**, the group, as it is on an
  `ItemsControl` whose items are Expanders or whose `ItemTemplate` makes them:

  ```xml
  <StackPanel Classes="expansion-panels">
    <Expander Header="Details">…</Expander>
    <Expander Header="Shipping" IsExpanded="True">…</Expander>
    <Expander Header="Payment">…</Expander>
  </StackPanel>
  ```

  The panels become one shape: the group's radius on the first panel's top corners and the last
  one's bottom, a lines-default rule under each panel but the last (`Outlined`, on by default),
  and an expanded panel moved 16px from its neighbours over .3s with every corner rounded. The
  panel before it drops its rule and takes the bottom corners, the one after it the top ones.
  `square` on the group squares every panel, and `no-outline` drops every rule, as
  `Outlined="false"` does. Each panel keeps its own elevation, as each MudExpansionPanel takes
  the group's, so where two closed panels meet the lower one's shadow falls faintly on the upper,
  in MudBlazor too; in high contrast that edge is the panel's ring. A virtualising items panel
  is a group too: a generated panel's place is its item's index, so the panels on screen are
  rounded and ruled as their items stand, and a neighbour not yet realised is off screen and
  counts as closed. Bind `IsExpanded` to the item there, as a virtualising panel rebuilds and
  reuses its Expanders. A `ListBox` is not a group: its rows are list rows, with their own
  padding and selection. As in MudBlazor, where `MultiExpansion` is off by
  default, opening a panel closes the others, and of panels written open only the first stays
  open; `multi-expansion` on the group lets them open independently. Closing is a current value,
  so a binding on `IsExpanded` survives it and hears the change.
- **A `HyperlinkButton` is not underlined at rest.** `MudLink.Underline` defaults to
  `Underline.Hover`. `Classes="underline"` is always-on, `no-underline` is never.

## FABs, avatars, badges and cards

Four more MudBlazor components that are a look on a control Avalonia already has:

```xml
<Button Classes="fab primary"><PathIcon Data="…" /></Button>               MudFab, 56px
<Button Classes="fab extended secondary medium">                           with a Label
  <StackPanel Orientation="Horizontal">
    <PathIcon Classes="start-icon" Data="…" /><TextBlock Text="Share" />
  </StackPanel>
</Button>
<Border Classes="avatar primary"><TextBlock Text="M" /></Border>           MudAvatar, 40px
<HeaderedContentControl Classes="badge error" Header="{Binding Unread}">   MudBadge
  <PathIcon Data="…" />
</HeaderedContentControl>
<Border Classes="card" Width="345">                                        MudCard
  <StackPanel>
    <DockPanel Classes="card-header">
      <Border Classes="avatar secondary card-header-avatar"><TextBlock Text="I" /></Border>
      <Button Classes="icon card-header-actions"><PathIcon Data="…" /></Button>
      <TextBlock Classes="card-header-content body1" Text="Istra Croatia" />
    </DockPanel>
    <Border Classes="card-media" Height="140"><Image Source="…" /></Border>
    <TextBlock Classes="card-content body2" TextWrapping="Wrap" Text="…" />
    <StackPanel Classes="card-actions"><Button Classes="primary" Content="Share" /></StackPanel>
  </StackPanel>
</Border>
```

- **A FAB is filled, grey and large by default.** `MudFab.Variant` defaults to `Filled`, unlike
  MudButton's `Text`, and `Size` to `Large`: 56px of `action-default-hover` at elevation 6, lifting
  to 8 on hover and 12 on press. `medium` and `small` are 48 and 40px, `outlined` and `text` never
  cast a shadow, and `extended` is the pill MudFab draws when it has a `Label`. Its icons are 24px
  at every size, as `IconSize` defaults to `Medium`.
- **An avatar is a `Border`, and it does not clip itself.** Clipping a `Border` would clip its own
  elevation shadow too, because Avalonia pushes a visual's clip before it draws itself. So an
  `Image` inside is scaled to cover and clipped to the shape by the theme, at the three sizes; an
  `ImageBrush` as the `Background` needs no clip at any size. `text` is the bare `.mud-avatar`,
  white on gray-light whatever the colour, because `_avatar.scss` has no rule for the variant.
- **A badge is a `HeaderedContentControl`: `Content` is what it decorates, `Header` the badge.**
  It hangs 4px into the corner (`overlap`: 12px), an `int` over 99 reads `99+` as `MudBadge.Max`
  makes it, and a `null` `Header` hides it, which is how `Visible="false"` reads here. The colour
  classes paint the badge, never the content. The badge sits outside the control's bounds, so the
  theme turns off the clip every templated control has, and the badge still takes the pointer.
- **A card is a `Border` with MudCard's parts as classes on what lays it out.** `card` is a paper
  at elevation 1; `outlined` drops the shadow whatever elevation class it also carries, because
  MudPaper only adds one when it is not outlined. `card-header` belongs on a `DockPanel`: the avatar
  and actions dock to the sides and the content goes LAST so it fills. A `card-media` that comes
  first takes the card's top corners.
- **`material-elevation-overlay` makes elevation read in dark.** A black shadow barely shows on a
  dark ground, in MudBlazor as here. On a window or any container, this class opts into Material's
  dark-theme overlay, which MudBlazor does not offer: each raised surface inside is lightened by
  white at its level's opacity, 5% at 1, 9% at 4, 12% at 8 and 16% at 24. Light and high contrast
  are unchanged. It is an inset shadow over the surface's own background, so it follows a custom
  surface colour and the corner radius. Papers, cards, panels, grids and the menus, selects and
  flyouts take it; coloured app bars, buttons and badges keep their colour.

```xml
<Window Classes="material-elevation-overlay">…</Window>
```

## Data grid

Avalonia's own `DataGrid`, from the `Avalonia.Controls.DataGrid` package, is MudDataGrid with
nothing to opt into. This package does not pull it in: reference it in the app (12.1.0 or later,
the version the theme is built against) and the theme picks it up. An app without it never
downloads it and never loads the DataGrid rules. On an older version the theme leaves the grid
alone rather than apply templates that name parts it lacks, and Avalonia's log says why; Avalonia
has no default theme, so the grid then needs the package's own theme included to show at all.

```xml
<DataGrid ItemsSource="{Binding Elements}">                    elevation 1, no hover
  <DataGrid.Columns>
    <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
  </DataGrid.Columns>
</DataGrid>
<DataGrid Classes="dense striped hover" GridLinesVisibility="All" ItemsSource="…" />   Bordered
<DataGrid Classes="outlined" CornerRadius="0" ItemsSource="…" />                       Square
```

- **Hover is off, and so are the stripes.** `MudDataGrid.Hover` and `Striped` default to false;
  `hover` and `striped` turn them on, and under the pointer the hover replaces the stripe.
- **A selected row is tinted, which MudDataGrid never does.** Its selection is a checkbox column's
  job, so transcribed exactly, an Avalonia grid's selection would be invisible. It takes what
  MudBlazor draws for a selected list row instead: primary-hover.
- **The sort arrow sits at the far end of the header**, where `.sortable-column-header` taking
  the width puts it, not next to the title: ArrowUpward, 18px in a 5px icon button, action-default,
  hidden until the column is sorted and turned for descending. The header has no column menu,
  filter or resize line: see [Known gaps](known-gaps.md).
- **`dense` pads the last column 16px on the right**, header included, where every other column
  has 24 - the last *visible* column in display order, so hiding or moving one moves it.
- **The last row has no rule under it**, as `.mud-table-root` takes it away. Rows cannot tell they
  are last, so the rows reach 1px past the grid's clip instead.
- **Grouping** is a `DataGridCollectionView` with `GroupDescriptions`: background-gray rows,
  "Title: Key" in bold, no count. A nested group's header steps in 48px per level, up to a fifth
  level, and the data rows stay where they are. The grid's own `SublevelIndent` stays at 0,
  because it would move the rows as well.

**`TableView` wears the same MudTable look**, on Avalonia's own table, with no package to add:
surface at elevation 1, subtitle2 headers over a table-lines rule, body2 cells 16px round and a
rule under every row but the last. `dense`, `striped`, `hover`, `outlined` and `elevation0` to
`elevation25` are the data grid's classes, and a selected row is primary-hover, as on the grid.

```xml
<TableView Classes="striped hover" ItemsSource="{Binding Elements}">
  <TableView.Columns>
    <TableViewColumn Header="Name" Width="*" CellTemplate="{StaticResource NameCell}" />
  </TableView.Columns>
</TableView>
```

## The drawer, and a third way to theme a control

`SplitView` is MudDrawer, and nearly all of it is **styles rather than its `ControlTheme`**.

Nearly everything MudDrawer contributes is a *value*: `layout/_drawer.scss` (in `Styles/layout/`, not
`components/`, which is why it takes some finding) paints from the `drawer-background` and
`drawer-text` tokens, `LayoutProperties.cs` gives `DrawerWidthLeft = "240px"` and
`DrawerMiniWidthLeft = "56px"` — already `FiliDrawerWidth` and `FiliDrawerMiniWidth`. What the
template carries is *structure*: pane sliding, the four display modes, the light-dismiss layer.
Get a part name wrong there and the drawer lays out perfectly and never opens, which is the same
reason `ScrollViewer` and `Window` are left alone.

The exception is `MudDrawer.Elevation`, 1 by default, which needs a part: the pane root is a
`Panel`, which cannot carry a `BoxShadow`, and it clips. So `Controls/SplitView.axaml` owns the
template - the fork's, part for part, with `PART_PaneShadow` added under the pane's content and
the content clipping one level in, in `PART_PaneClip` - while the fork itself stays byte-faithful.
The elevation classes on the `SplitView` change the shadow (`elevation0` drops it). A drawer
closed to nothing casts none, and high contrast draws none: the drawer's edge there is its
drawer-border line, and a ring beside it would make it 2px.

So there are three ways to change how a control looks here, and the third is new:

| | when |
|---|---|
| Hand-written `ControlTheme` | the shape is wrong — a floating label, a raised button |
| `Accents.axaml` | the control paints from one of Simple's ~96 shared keys |
| A `Style` in `FiliTheme.axaml` | the shape is right and only values are wrong |

The third works because **a `Style` outranks a `ControlTheme` setter** — normally the trap this
file warns about twice, relied on deliberately here. `DrawerTakesMudDrawerMetricsOverTheForkedTemplate`
pins it, because if that precedence ever reversed every drawer would quietly revert to Simple's
320px grey pane.

One caveat found while doing this: `Accents.axaml` **cannot** override a key a forked control
file declares for itself, such as `SplitViewOpenPaneThemeLength`. Accents is merged *before* the
control dictionaries, and in a merged `ResourceDictionary` the later entry wins.

**Avalonia's `DrawerPage`** builds its own `SplitView` and puts a hamburger toggle in its top bar.
That toggle is MudLayout's drawer button, a MudIconButton: an action-default icon with a round
hover tint, the same whether the pane is open or not. It is not a MudToggleItem, so it never
fills when checked. Closed with `DrawerLayoutBehavior="CompactInline"` or `CompactOverlay`, its
drawer is a mini drawer as a compact `SplitView` is, below, on a 56px rail unless the app sets
`CompactDrawerLength`. Its top bar and title keep the fork's look; the gallery's **Custom styles**
tab shows both states. Set `DrawerLayoutBehavior` rather than `DisplayMode`, which `DrawerPage`
works out for itself, and `DrawerBreakpointLength` if a narrow page should not fall back to an
overlay.

Not carried over: the pane's elevation-1 shadow. The pane root is a `Panel`, and only a `Border`
can carry a `BoxShadow` — the same limitation that made `Button` need a template in the first
place. Adding it means owning the template.

**The nav menu in the pane is `nav` on a `TreeView`**: MudNavMenu, MudNavLink and MudNavGroup.

```xml
<TreeView Classes="nav">
  <TreeViewItem Header="Library" IsSelected="True" />
  <TreeViewItem Header="Downloads">
    <TreeViewItem Header="Queue" />
  </TreeViewItem>
</TreeView>
```

A row without children is a link and the selected one is the active link: weight 500, primary
over action-default-hover, or with a colour class the colour over its hover tint and at 12% under
the pointer. A row with children is a group and opens on a click anywhere on it, its ExpandMore
arrow turning and taking the colour; a top-level group's title is 500. Rows step in to 36, 48, 60
and 72px by depth. `dense`, `rounded` and `bordered` are MudNavMenu's; a `PathIcon` with
`start-icon` in a header is the link icon, in drawer-icon. Which link is active is the app's to
set, where MudBlazor matches the URL. `hide-icon` on a group's row is `HideExpandIcon`. The menu
scrolls up and down, never sideways, as MudDrawer does: a label too long for the drawer wraps
when its header gives the text a width (a string, or a `Grid` or `DockPanel`); in a horizontal
`StackPanel` it is cut off at the edge.

**A colour class on the `SplitView` is MudDrawer's `Color`**, `.mud-theme-{color}`: the pane in the
colour and its content in the contrast text.

**`drawer-header` on a `Border` at the top of the pane is MudDrawerHeader**: at least the app bar's
64px tall, 12px 24px round its content; `dense` makes it 48px and 8px 24px. A floor, so a taller
header grows.

**A mini drawer is a compact `SplitView`.** `CompactInline` or `CompactOverlay` with the pane
closed is MudBlazor's closed mini drawer: a nav link's text beside its `start-icon` is hidden, so
the 56px rail shows the icons, and every row drops its indent. The 12px between an icon and its
text goes with the text, and the menu does not scroll sideways: a group's arrow runs off the
rail's edge, as it does in MudBlazor's. The icon and text go in the
item's `Header`; a panel written straight inside a `TreeViewItem` is a child *item*, so the rail
would show a group's arrow instead of the icon.

```xml
<SplitView DisplayMode="CompactInline" IsPaneOpen="{Binding Open}">
  <SplitView.Pane>
    <TreeView Classes="nav">
      <TreeViewItem>
        <TreeViewItem.Header>
          <StackPanel Orientation="Horizontal">
            <PathIcon Classes="start-icon" Data="{StaticResource InboxIcon}" />
            <TextBlock Text="Inbox" />
          </StackPanel>
        </TreeViewItem.Header>
      </TreeViewItem>
    </TreeView>
  </SplitView.Pane>
</SplitView>
```

## Toolbars and icons

**`toolbar` on a `Border` is MudToolBar**: 64px high with 24px gutters, its children centred.
`dense` is 48px, and `Gutters="false"` is `Padding="0"`, on an app bar too. The row inside
is the app's own panel.

```xml
<Border Classes="toolbar">
  <StackPanel Orientation="Horizontal" Spacing="8">
    <Button Classes="icon"><PathIcon Data="{StaticResource Menu}" /></Button>
    <TextBlock Classes="subtitle1" Text="Library" />
  </StackPanel>
</Border>
```

**A `PathIcon` takes MudIcon's `Size` and `Color` as classes**: `small`, `medium` and `large` are
20, 24 and 36px, and a colour class is the colour. An icon's own class beats one that colours it
by where it sits, an alert's or a button's, as MudIcon's `Color` does. A `PathIcon` draws its
glyph as MudIcon does, in a 24px viewBox scaled to its size: a Material glyph keeps the padding
round it, so a small one such as Play stays small and every glyph sits where Material put it.
Avalonia's own `PathIcon` stretches the glyph's bounds to the box instead. A glyph drawn in
another box is scaled into 24 by the app before it is given here.

**The glyphs the theme draws are replaceable, one control or one region at a time.** Each is a
`DynamicResource` key in `Themes/Icons.axaml`; set the same key in a control's `Resources` and
that control, and everything inside it, draws your glyph instead. This is how every MudBlazor icon
parameter maps across (`CheckedIcon`, `OpenIcon`, `ExpandIcon`, `PrevIcon`, `CloseIcon`, …);
[MudBlazor parity](mudblazor-parity.md) has the table of keys. Paste a path from MudBlazor's
`Icons.Material` as it is: it is drawn in a 24px box at its own size.

## Command bars and pages

**`CommandBar` is MudToolBar holding MudIconButtons**, with a MudMenu for the rest. The bar is
the toolbar's 64px with 24px gutters and no ground of its own (`dense` is 48px, `Padding="0"`
Gutters="false"). The theme's `DefaultLabelPosition` is `Collapsed`, where Avalonia's is `Bottom`,
because a MudToolBar holds icon buttons: each command is a 48px circle round a 24px icon in
action-default, its `Label` the tooltip. `Right` makes each a text MudButton with a start icon,
the label uppercase. A checked `CommandBarToggleButton` is primary, MudToggleIconButton's
`ToggledColor`. The overflow button is `MoreVert`, and the overflow MudMenu's popover, each
command a 48px row with its label as written. On an `appbar` the commands take the bar's text
colour, as MudBlazor's app bar examples give theirs `Color.Inherit`.

```xml
<Border Classes="appbar" Padding="0">
  <CommandBar>
    <CommandBar.Content><TextBlock Classes="h6" Text="Inbox" /></CommandBar.Content>
    <CommandBarButton Label="Search"><CommandBarButton.Icon><PathIcon Data="{StaticResource Search}" /></CommandBarButton.Icon></CommandBarButton>
    <CommandBar.SecondaryCommands>
      <CommandBarButton Label="Settings" />
    </CommandBar.SecondaryCommands>
  </CommandBar>
</Border>
```

**Avalonia's page types are MudBlazor's layout components.** A `ContentPage` is text-primary on
the app's background. A `NavigationPage`'s bar is MudAppBar: the appbar colours, 64px, 24px
gutters and elevation 4 (`HasShadow`, on by default here; `False` is 0), the page's `Header` in
h6, an edge-start `ArrowBack` MudIconButton once there is a page to go back to, and the page's
`NavigationPage.TopCommandBar` at the end, its commands in the bar's colour. A `TabbedPage` is
MudTabs, the theme's own tabs. How pages slide, push and pop stays Avalonia's.

## Snackbars

`WindowNotificationManager` and the `NotificationCard`s it builds are MudSnackbar: 6px/16px
padding, 288px minimum and 500px maximum width, 16px between stacked cards, and the manager 24px
from whichever edges it is anchored to. The newest is at the bottom of the stack wherever it is
anchored, as MudSnackbarProvider lists its snackbars in the order shown; `newest-on-top` on the
manager is `NewestOnTop`.

Two things were read rather than chosen. The **elevation is 6** — `_snackbar.scss` spells out the
same three shadow layers `FiliElevation6` carries, character for character, so the level is
transcribed rather than picked. And the **severity colours come from `_alert.scss`**, because a
snackbar is a filled `MudAlert`: the palette colour as ground, its contrast text as foreground,
at **Medium** weight — the filled alert's 500 overrides the snackbar's own 400. Each severity has
its own contrast token (`FiliInfoContrastTextBrush` and so on). In light and dark all four are
MudBlazor's white; in high contrast they are black, because that variant's status colours are
pastels and white on them would be about 1.6:1.

**The card is MudSnackbarElement's**: the severity icon (CommonSnackbarOptions' InfoIcon,
SuccessIcon, WarningIcon, ErrorIcon) at 24px and 0.9 opacity, the message, and a small close
button with Filled.Close that closes the card. The variants are classes on the card, passed as
the last argument of `WindowNotificationManager.Show`:

```csharp
manager.Show("Saved", NotificationType.Success, classes: ["outlined"]);
manager.Show("Settings saved", NotificationType.Information, classes: ["normal", "hide-close"]);
```

`text` and `outlined` are `SnackbarVariant`: the surface, the text in the colour's darken shade at
400, and for outlined a 1px line in the colour. `normal` is `Severity.Normal`, which
`NotificationType` lacks: filled dark, with the EventNote icon. `hide-icon` is `HideIcon` and
`hide-close` is `ShowCloseIcon="false"`. `Show(INotification)` takes no classes, so a card made
that way is filled with both.

The icons are CommonSnackbarOptions', set once for every card: put `FiliCloseGeometry`,
`FiliInfoOutlinedGeometry`, `FiliAlertSuccessGeometry`, `FiliReportProblemGeometry`,
`FiliErrorOutlineGeometry` or `FiliEventNoteGeometry` in the `WindowNotificationManager`'s
`Resources`. One message cannot have an icon of its own, as MudBlazor's `Icon` gives it: `Show`
has nowhere to pass one.

**The enter and exit animations are copied from the forked template deliberately**, and one of
them is load-bearing: the key frame that sets `IsClosed` at 100% is what actually removes a card.
A theme that drops it renders beautifully and leaves notifications on screen forever.

## Right to left

Set `FlowDirection="RightToLeft"` and everything mirrors. No template in this package does
anything to make that happen, and that is the finding rather than an omission:

> **Avalonia mirrors an entire subtree with a single transform, applied where the flow direction
> CHANGES** — not per control.

So grid columns, dock sides and `Left`/`Right` alignment all flip on their own. The numeric
field's spin column moves to the left, the tree indents from the right, the split button's
rounded corners swap ends, and the inset divider indents from the right, none of which is coded
for anywhere. MudBlazor reaches the same place with logical CSS properties (`margin-inline-end`,
`padding-inline-start`), which is exactly why those have no counterpart here.

**The exception is a glyph that must not mirror.** A checkmark is not a directional symbol, so
Material's bidirectionality guidance leaves it alone — a reversed tick just looks broken. The
three checkbox glyphs therefore opt out with `FlowDirection="LeftToRight"`, which is the same
thing Avalonia's own Simple theme does to its check path and is how the case was spotted at all.

Arrows are deliberately **not** opted out. A tree's disclosure arrow *should* point left in RTL,
and the mirror gives that for free; pinning it left-to-right would leave it pointing away from
the content it opens. `RightToLeftTests` asserts both halves of that split.

Two things to know if you go looking:

- Measuring the mirror requires **crossing the boundary**. Translate a point to the mirrored
  panel and the coordinates are still left-to-right; translate it to the window and the flip
  appears. The first version of that test failed for exactly this reason.
- `HasMirrorTransform` is false on the controls *inside* an RTL subtree. It is true only on the
  element where the direction changed.
- **In a browser, RTL text needs a font for its script.** The mirroring works anywhere; the
  glyphs do not, because there are no system fonts to fall back on — see [Fonts](../README.md#fonts).

## High contrast

A third variant, and **not a MudBlazor one** — MudBlazor ships light and dark and nothing else, so
this one is derived rather than transcribed. Select it like any other variant:

```csharp
Application.Current.RequestedThemeVariant = FiliThemeVariants.HighContrast;
```

```xml
<Application xmlns:theme="using:Fili.Theme.MudAvalonia"
             RequestedThemeVariant="{x:Static theme:FiliThemeVariants.HighContrast}">
```

**`x:Key="HighContrast"` does not work**, and it does not fail quietly: Avalonia's `ThemeVariant`
type converter accepts the built-in variants only and throws `NotSupportedException` while the
merged dictionary is being built, which takes the rest of the theme down with it. A custom variant
is reachable from XAML only through `x:Static`, so `FiliThemeVariants.HighContrast` is the one
object every dictionary here — and every adopting app — has to name.

Three rules produced every value in it:

1. **Separation comes from lines, not fills.** `FiliSurfaceColor` sits one step off the page
   ground and is not doing the work; `#0D0D0D` against `#000000` is a difference nobody can see.
   What divides a card, a menu or a dialog from the page is its outline.
2. **Nothing is conveyed by alpha.** Light and dark lean on translucent blacks and whites — a
   divider is `#1FFFFFFF` — which composite against whatever is behind them. On a black ground a
   12% white line is not a faint line, it is no line. Alpha survives in six tokens, all of them
   overlays rather than content: the two state layers, the selection tint, the input fill and the
   two scrims.
3. **Disabled still has to be legible.** `#CFCFDA` on black is about 12:1, far too strong to read
   as disabled in a normal palette, which is the point. Disabled is signalled by being dimmer than
   enabled, not by being nearly gone.

**The shadow ladder becomes a hard 1px ring.** That is the whole reason this variant needed no
control template edits: every surface that already asked for a `FiliElevation*` gets its outline
for free. A surface painted with `FiliElevation0` has no outline here either. **The ring is
`FiliLinesDefaultColor`**, as the application resolves it: an app that repaints the line for high
contrast in its own resources repaints every ring with it, at run time too. A `BoxShadows` string
cannot carry a `DynamicResource`, so the theme builds each ring when it is looked up
(`HighContrastRings`). A line colour repainted only in a window's or a control's resources does
not reach the rings, which are one application-level value, like every brush here.

### What an adopting app has to do

**Declare the variant, not just the tokens.** `FiliThemeVariants.HighContrast` inherits from
`ThemeVariant.Dark`, which is load-bearing — it is what lets the eighty-one forked control themes,
which declare Light and Dark only, work here untouched. It is also the trap: an app that overrides
palette tokens in its `Light` and `Dark` dictionaries and stops there does **not** get a
high-contrast version of its own colours. It gets its *dark* ones, silently, because that is what
the variant falls back to.

```xml
<ResourceDictionary.ThemeDictionaries>
  <ResourceDictionary x:Key="Light">   <!-- … --> </ResourceDictionary>
  <ResourceDictionary x:Key="Dark">    <!-- … --> </ResourceDictionary>
  <ResourceDictionary x:Key="{x:Static theme:FiliThemeVariants.HighContrast}">
    <!-- the same Fili* keys again, in this variant's colours -->
  </ResourceDictionary>
</ResourceDictionary.ThemeDictionaries>
```

An app that swaps whole palette dictionaries at runtime rather than using `RequestedThemeVariant`
needs nothing new: its high-contrast palette just has to carry the same
`Fili*` overrides as its other palettes.

**A colour is five tokens, not one.** Overriding `FiliPrimaryColor` recolours what rests in the
primary; hover, focus and press paint `FiliPrimaryHoverColor` (the colour at 6%) on text and
outlined controls and `FiliPrimaryDarkenColor` on filled ones, `FiliPrimaryLightenColor` lights a
few, and `FiliPrimaryContrastTextColor` is the text on a fill. Leave one out and that state stays
MudBlazor's own colour - a gold button that turns purple under the pointer. Override all five, in
every variant you declare, for each colour you change; MudBlazor derives the darken and lighten
shades from the colour (`ColorRgbDarken` and `ColorRgbLighten` in its `Palette.cs`), and this
theme's own are that derivation's output. The same goes for a button with no colour class:
overriding `FiliActionDefaultColor` means `FiliActionDefaultHoverColor` too, the colour at 6%,
which its hover, focus and press paint. And a near-black colour darkens to black, as it does in
MudBlazor, so a filled button in it barely changes under the pointer on a near-black page.

**One control's states are tokens too, not its template.** A style that reaches a theme template
part (`/template/ … #PART_StateLayer`) is reaching something that is not published, and a release
may rename or redraw it (see [Compatibility](compatibility.md)). The parts paint `DynamicResource`
tokens, and a resource lookup starts at the control, so a key in a control's own `Resources`
changes that control alone. A poster card that shows its own hover rather than the grey state
layer:

```xml
<Button Classes="poster">
  <Button.Resources>
    <SolidColorBrush x:Key="FiliActionDefaultHoverBrush" Color="Transparent" />   no state layer
    <x:Double x:Key="FiliRippleOpacity">0</x:Double>                                no ripple
  </Button.Resources>
  …
</Button>
```

The keys reach the control's content too, so a default button inside the card loses its hover as
well. And hover is also MudBlazor's focus cue: a control that drops it needs its own for the
keyboard.

## Keyboard focus

Every interactive control lights its state layer on `:focus-visible` — the same tint hovering
gives it. That is MudBlazor's own affordance rather than a choice made here: `_reset.scss` sets
`outline: none` and `outline: 0`, deliberately removing the browser's focus ring, and each
component maps `:focus-visible` to the hover colour:

```
_button.scss          &:focus-visible, &:active { background-color: action-default-hover }
_expansionpanel.scss  &:focus-visible           { background-color: action-default-hover }
_link.scss            &:focus-visible, &:active { text-decoration: underline }
_list.scss            &:focus:not(.mud-selected-item) { background-color: action-default-hover }
```

Three details worth keeping:

- **`:focus-visible`, not `:focus`.** Clicking a button should not leave it tinted once the
  pointer has gone; only keyboard navigation should mark it.
- **A selected list row is excluded**, per `_list.scss`. Arrow keys move focus and selection
  together, so tinting both would just be the selection tint twice.
- **Text fields keep `:focus`**, not `:focus-visible` — a field shows its accent rule however it
  was focused, because that rule says "this is where typing goes" rather than "this is where the
  keyboard is".

The affordance is a 4% tint, which is what MudBlazor ships and is on the subtle side. An app with
a stricter accessibility bar should add a ring of its own; the theme deliberately does not invent
one. Nor does it keep the base's: Avalonia Simple's dashed focus rectangle, which the forked
`AdornerLayer` still drew over the tint until 0.9.4, is off. A ring an app sets through a
control's `FocusAdorner` still draws.

Each control is one Tab stop: the slider is one, not its rail halves, and a text field inside an
`AutoCompleteBox` or `NumericUpDown` stands for its control.

## The converters

Besides `FiliThemeVariants`, the package's only code is a handful of `internal` helpers, each
for something XAML cannot express on its own:

- **`FactorConverter`** multiplies a bound pixel dimension by a constant. The indeterminate
  progress bar needs it: MudBlazor's keyframes animate `left`/`right` percentages, so each bar
  changes *width* as it travels (35% → 90%, and 200% → 1% for the second), and Avalonia's
  transform parser rejects a `%` unit outright — `FormatException: Invalid unit: %`, thrown at
  **runtime**, because a transform string in a key frame is parsed lazily and compiles either way.
- **`ErrorMessageConverter`** turns one `DataValidationErrors.Errors` entry into the sentence to
  show. An entry is usually an `Exception`, and binding straight to it renders
  `"System.InvalidOperationException: Must be a valid email address"`.
- **`TickPositionsConverter`** places a slider's tick dots along its track, because `TickBar`
  only draws lines.
- **`BadgeContentConverter`** caps a numeric badge at `MudBadge.Max`'s default: an `int` over 99
  reads `99+`. A binding cannot compare.
- **`StripedRowConverter`** tells `striped` which rows to paint from `DataGridRow.Index`. Avalonia
  12's DataGrid has no alternating row brush, and a binding cannot take a remainder.
- **`SlidingIndicator`** slides a tab's indicator over from the one it replaces. Where to start
  is the old indicator's position, which only code can read at the moment the selection moves.
- **`FieldClasses`** passes the text field's classes (`filled`, `outlined`, `dense`, `error`,
  `counter`, `helper-on-focus`) from an `AutoCompleteBox` or `NumericUpDown` to the `TextBox` in
  its template, and keeps passing them as they change. A style cannot add a class, and the
  TextBox's own theme reads them two templates down from the outer control.
- **`MenuIconGutter`** opens a menu row's icon column when another row in the same list has an
  icon, MudMenu's `:has(.mud-menu-item-icon)` rule. A selector cannot ask about siblings.
- **`SpinColumn`** puts a number field's spin column inside its input box: it keeps the text
  clear of the column and sizes the column to the box, two templates down from the NumericUpDown.
- **`CollapsePanel`** grows an expansion panel's content into view. Avalonia cannot animate a
  height to `Auto`, so it lays the content out whole and shows a share of it that a transition
  eases from nothing to all.
- **`ElevationOverlay`** is the inherited property `material-elevation-overlay` sets, which each
  surface reads on itself: a descendant selector made every Border watch all its ancestors.
- **`IconSlot`** gives a `start-icon` or `end-icon` its host's size, gap and colour, and an
  `avatar` in a chip the chip's avatar size: the host's theme sets inherited values, and the
  control that carries the class takes them as they arrive. The hosts are buttons, split and
  drop-down buttons, toggle items, chips, FABs, nav links, links, tabs and text fields. A
  style ending in the icon would be matched against every control in the window; the class is
  read when the icon joins its host, so put it on the icon in markup.
- **`ButtonGroup`** marks each button of a `button-group` with where it sits: before another,
  after one, in a vertical group. A selector from the panel to its buttons would end in Button
  and be matched against every button in the window, and `:nth-child` counts hidden ones.
- **`OverlayScrim`** centres an `overlay`'s child, gives it its container's corners and fades it
  in when shown. A style could do none of it without being matched against every control or
  every Border.
- **`PartBorder`** is every Border in a hand-written template. It keeps them out of the app's
  Border class styles, which every Border in the window was matched against
  ([Traps](traps.md)).
- **`HighContrastOpacityConverter`** hides the drawer's shadow in high contrast, where the
  drawer's edge is its own line. A style cannot select on the theme variant.
- **`HighContrastRings`** builds high contrast's elevation rings in the application's
  `FiliLinesDefaultColor`. A `BoxShadows` string cannot carry a `DynamicResource`.
- **`ExpansionPanels`** keeps `:joined`, `:first-panel`, `:last-panel`, `:next-expanded` and
  `:previous-expanded` on the panels of an `expansion-panels` group, for the rule and corners
  MudBlazor gives each panel by its neighbours, and closes the others when one opens. A selector
  cannot ask about siblings, and `:nth-child` cannot see past an `ItemsControl`'s presenters.
- **`Ripple`** draws MudBlazor's ripple in a template's `PART_Ripple` border: a circle from the
  pointer, its opacity inherited from the control's theme. Avalonia has no ripple, and a style
  cannot follow a pointer.
- **`TextTransform`** draws a button's label in `FiliButtonTextTransform`'s case: a string on
  its way into the presenter, and any plain `TextBlock` in the content, which the inherited case
  reaches, through a run that leaves its `Text` alone. Avalonia has no text-transform, and a
  binding cannot read a resource as a converter parameter.
- **`Adornments`** marks a text field that has `InnerLeftContent` with `:start-adornment`, which
  floats its label, as MudInput's start adornment does, and one with `InnerRightContent` with
  `:end-adornment`; each adornment then keeps 8px from the text. A string or `TextBlock` there
  also gets `:start-text-adornment` or `:end-text-adornment`, which keeps it on the text line where
  an icon is centred on a filled or outlined box; and a field with no `PlaceholderText` gets
  `:no-label`, which drops the label's room. A selector cannot ask whether a property is null, or
  what type its value is.
- **`CalendarView`** makes the date picker's toolbar buttons change the calendar's view - the
  year to the years, the date back to the selected month - and gives the toolbar the paper's top
  corners. A template cannot set another control's property on a click.
- **`TimeView`** keeps the time picker's text: the field's value as MudTimePicker writes it, and
  the toolbar's hour, minute and period following Avalonia's columns as they turn, before the time
  is accepted. A column's selection is not a property a binding can watch.
- **`TabBarScroll`** shows a tab bar's scroll buttons while its tabs overflow, disables each at
  its end, scrolls a bar's length per click, and brings the selected tab into view. Overflow is
  the scroller's extent against its viewport, which no binding compares.
- **`DataGridLastColumn`** marks the cells and header of a grid's last visible column, for
  `dense`'s 16px on the right. The grid knows which column is last, but tells no one.
- **`GroupIndentConverter`** indents a nested group's header 48px per level. A header's level is
  internal to the DataGrid, so it is counted from the public group tree.
- **`OptionalStyleInclude`** loads the DataGrid's files only when the app has the package; see
  [Data grid](#data-grid).

The two DataGrid helpers name the package's types, so they live in `Themes/Optional/DataGrid/`
with the files that use them.

Compiled XAML in the same assembly can construct an `internal` type, so being internal costs
nothing and keeps the public surface at that one type. Reach for one only after establishing
that the XAML route does not exist.

## Lists, selects and the things built out of them

`MudSelect`, `MudAutocomplete` and `MudList` share their parts, and so do these:

```xml
<ComboBox PlaceholderText="Platform"> <ComboBoxItem>PC</ComboBoxItem> </ComboBox>
<ListBox Classes="paper"> <ListBoxItem>First</ListBoxItem> </ListBox>
<AutoCompleteBox PlaceholderText="Search" />
<NumericUpDown Value="42" PlaceholderText="Quantity" />
```

- **`list-item` on a `Button`** is MudListItem with `OnClick` or `Href`: a row that does
  something rather than one that is chosen, outside a selecting list. The list row's look - body1
  on 48px, 16px sides, the row's hover tint and the ripple - with the label in its own case, as a
  list item is not a MudButton. `dense` and `no-gutters` are MudListItem's.

  ```xml
  <Button Classes="list-item" Command="{Binding OpenSettings}">Settings</Button>
  ```
- **ComboBox** — the finding in `_select.scss` is a *negative* one: `.mud-select` has no box model
  at all, and every visual comes from `.mud-select-input`, which is a `MudInput`. So a select
  **is** a text field with a drop-down adornment, and it wears the standard field's underline,
  accent rule and floating label. Anything else would make selects and fields disagree in the
  same form.
- **A selected row is a tint, never a filled bar.** `MudList` marks it with
  `mud-selected-item mud-primary-text mud-primary-hover` — primary text over primary at 6% — so it
  stays as readable as the rows around it. `ListBox` and `ComboBox` both use that one rule. A
  colour class on the `ListBox` is MudList's `Color`, the selected row's colour; `dense` and
  `no-gutters`, on the list or a row, are its `Dense` and `Gutters`. The text is body1, as
  MudListItem's is, and body2 when dense. A row is 48px, 36 dense: `.mud-list-item-text` adds 4px
  above and below whatever the row holds. A select's options are the same rows; `dense-items` on
  the `ComboBox`, or `dense` on one option, is MudSelect's `Dense`, since `dense` on the select is
  its field's `Margin.Dense`.
- **`multi-selection` is `SelectionMode.MultiSelection`**, on a `ListBox` or a `TreeView`, with
  `SelectionMode="Multiple, Toggle"` for the behaviour: each row gets a checkbox, checked while it
  is selected, and no row is marked selected. The checkbox is grey, `CheckBoxColor`'s default, or
  the list's colour class. The class cannot set `SelectionMode` itself: a list declared in XAML
  pins it as a local value before any style applies ([the trap](traps.md)).

  ```xml
  <ListBox Classes="multi-selection primary" SelectionMode="Multiple, Toggle"> … </ListBox>
  ```
- **ToggleButton** — MudToggleItem, which in MudBlazor 9.11 is a `MudButton` in the group's colour,
  primary by default: the colour as text while off, `Variant.Filled` once on, with the button's
  border, shadow and minimum width taken away. A colour class recolours it; `small` and `large`
  are the group's sizes, and `outlined` is the group's 1px line in the colour, absorbed into the
  padding. An Avalonia toggle is one item rather than a group, so those are classes on each.
- **NumericUpDown** — a `MudInput` with a 24px spin column, which is not a guess:
  `_inputcontrol.scss` reserves exactly `padding-right: 24px` for it, with the comment *"This must
  be the same width of the spinners"*. The column sits inside the input box, top to bottom, as
  `.mud-input-numeric-spin` does: the box runs on under it, so its underline, filled ground or
  outline reaches the field's edge, and the text keeps 24px clear. The helper line below runs the
  field's full width.
- **AutoCompleteBox** — both halves were already themed (a `TextBox` and a `ListBox`), so its
  theme exists for the popup surface.
- **Both take the text field's classes.** `filled`, `outlined`, `dense`, `error`, `counter` and
  `helper-on-focus` on an `AutoCompleteBox` or `NumericUpDown` reach the `TextBox` inside, as do
  its helper text and, on a `NumericUpDown`, its validation error, which shows in the field's
  helper line with the red rule.
- **TreeView** — rows are 32px with 4px/8px padding, indented **17px per level**, and the arrow is
  `ChevronRight` turning **90°** rather than the expansion panel's `ExpandMore` turning 180°. A
  leaf keeps the arrow's 40px slot, as MudBlazor's empty `.mud-treeview-item-arrow` does, so its
  text lines up with a branch's. The indent is Avalonia's
  mechanism rather than MudBlazor's: a `TreeViewItem` multiplies its own `Level` by an indent
  resource through `TreeViewItemIndentConverter` instead of nesting margins, so the 17px is
  declared once and every level derives from it. A row under the pointer is tinted only on a
  `hover` tree, MudTreeView's `Hover`, which is off by default; a selected row keeps its own tint.
  A colour class is its `Color`, and `dense` its `Dense`: no 32px minimum, 1px 4px.

**The select's floating label needed two TextBlocks**, and the reason generalises. A select must
float its label when it holds a *value*, not only when focused, or the resting label sits on top
of the selected text — and Avalonia has no pseudo-class for "has a selection". `:empty` on an
`ItemsControl` means **no items**, which is why the first attempt floated every label permanently.
A `Style` selector is the only thing that can drive an animation, so the animated label exists
only while nothing is selected and a static copy takes over once something is. They share
position, size and colour, so the handover is invisible.

## Scrolling and menus

```xml
<ScrollViewer> … </ScrollViewer>
<Menu> <MenuItem Header="Library"> … </MenuItem> </Menu>
```

- **ScrollBar** — a thin buttonless rail with a rounded thumb that darkens on hover and press.
  Dropping the line buttons is safe (their lookups are null-safe, and wheel/drag/keyboard/touch
  are `ScrollViewer`'s job) but a `Track` named `PART_Track` is required or the thumb never moves.
- **ScrollViewer keeps its forked template**, deliberately. There was a hand-written one; it was
  deleted once the fork landed, because it did nothing the fork does not and it is the control
  where a mistake costs most: `PART_ContentPresenter` must be a `ScrollContentPresenter`
  specifically, alongside `PART_HorizontalScrollBar` and `PART_VerticalScrollBar`. Rename any of
  the three and you get a control that lays out perfectly and does not scroll. The themed
  `ScrollBar` is reached *through* it.
- **Menu** — **two** item themes, not one. A top-level strip item and a dropdown row look alike in
  markup and are different controls on screen: the strip item has no chevron, no icon column, and
  drops its popup downward. The first render of this menu showed `Library >  View >` across the
  top, because the dropdown template hides its chevron only for *leaf* items and a top-level item
  always has children. `Menu.ItemContainerTheme` is the strip item; that item's own
  `ItemContainerTheme` is the row.
- **A dropdown row is MudMenuItem**: 8px 12px round a body1 label with 4px above and below it,
  so 48px, and a 24px icon in action-default with 12px after it. The icon column opens for a row
  with an icon and, so the labels line up, for every row in a list that has one; a list with no
  icons has no column. The list has 4px above and below, and 4px either side of a separator. A row
  with a submenu ends in Filled.ArrowRight at 24px; `hide-icon` on it is `HideSubMenuArrow`.

## Overlays

`ToolTip`, `FlyoutPresenter` and `MenuFlyoutPresenter` are created by the framework rather than
placed in markup — `ToolTip.Tip="text"` builds the ToolTip internally, `<Flyout>` builds its
presenter — which is why they were keyed by type even back when everything else was keyed by
class. They now look like every other theme in the package.

A flyout is MudPopover's paper - surface, the default radius, elevation 8 - and, like it, has no
padding: the content brings its own, as `<MudPaper Class="pa-4">` does in a popover. Set
`Padding` on the content, or on the presenter through `FlyoutPresenterClasses`.

The tooltip is deliberately **not theme-varying**: a Material tooltip is a dark chip in light and
dark alike, so it reads as an annotation floating above the UI rather than as another surface of
it. Its fill is transcribed rather than chosen — `_tooltip.scss` paints
`var(--mud-palette-gray-darker)`, which is `Palette.GrayDarker` = `Colors.Gray.Darken2` = `#616161`,
**solid**. An earlier version used 90% alpha, which is Material's spec and not MudBlazor's. It has
no width of its own, as `.mud-tooltip` sets none, so a long tooltip wraps only where the screen,
or the page in a browser, stops it.

**A tooltip can carry a class after all.** `ToolTip.Tip` uses a `ToolTip` as it is rather than
wrapping it, so one built in markup keeps its classes: a colour class is MudTooltip's `Color`, and
`arrow` its `Arrow`, a 6px triangle in the chip's colour on the side that faces the target for
`Placement` `Top`, `Bottom`, `Left` and `Right`.

```xml
<Button Content="Delete" ToolTip.Placement="Bottom">
  <ToolTip.Tip>
    <ToolTip Classes="error arrow" Content="Deletes for good" />
  </ToolTip.Tip>
</Button>
```

**MudOverlay is `overlay` on a Border**, over the content it covers in a `Panel` with it, and
shown and hidden by the app. With no other class it is transparent and still takes the pointer, as
a modal MudOverlay does; `dark-background` and `light-background` are `DarkBackground` and
`LightBackground`. Its child is centred, its corners are those of the container above it, as CSS's
`border-radius: inherit` makes them, and it fades in over .15s each time it is shown - the whole
overlay, where MudBlazor fades the scrim and shows the content at once.

```xml
<Border Classes="paper" Height="200">
  <Panel>
    <StackPanel Margin="16">…</StackPanel>
    <Border Classes="overlay dark-background" IsVisible="{Binding Loading}">
      <ProgressBar Classes="circular primary" IsIndeterminate="True" />
    </Border>
  </Panel>
</Border>
```

**Every popup fades in**, as a MudPopover does: a select's list, a menu, an autocomplete's list,
a flyout, a context menu and a tooltip go from opacity 0 to 1 over 251ms, `PopoverOptions.Duration`
and MudTooltip's `Duration`, at CSS's default `ease`, and close at once. The fade is on the popup's
host, which Avalonia builds every time a popup opens, so it runs on every open. A dense MudMenu
opens without it; here a dense menu fades too, because the host cannot see the menu's class.

Flyout templates carry an 8px outer margin because `BoxShadow` draws *outside* the border and a
popup window is sized to its content — with no room to spill into, the shadow is clipped and the
flyout reads flat.

**`Window` is not themed**, and that is a deletion rather than an omission. There was a hand-written
`Window` theme, written while layering; it went when the fork landed, because two of its parts are
load-bearing and invisible until missing — `VisualLayerManager` (leave it out and every tooltip
and flyout in the app silently fails to appear) and `PART_TransparencyFallback` (without it, a
window asking for acrylic on a compositor that cannot provide it renders with no background at
all). The forked template already gets both right, and Material has no opinion about window
chrome.
