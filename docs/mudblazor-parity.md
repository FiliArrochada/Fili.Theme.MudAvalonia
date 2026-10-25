# MudBlazor parity

What each MudBlazor component parameter, palette property and token becomes in this theme, and
where the two differ. For side-by-side markup, see [Razor and AXAML](razor-vs-axaml.md).

The rule behind every row: **a class only ever names a MudBlazor variant.** Every control the
theme templates is keyed to its type, so an unclassed control is already the MudBlazor default.
Class names are MudBlazor's own words (`primary`, `outlined`, `small`), deliberately not
namespaced. `ClassVocabularyTests` pins the full list of 159. Two are not MudBlazor's, and their
names say so: `material-fixed-tabs`, Material's fixed tabs, and `material-elevation-overlay`,
Material's dark-theme elevation overlay. MudBlazor offers neither.

| Mark | Meaning |
|---|---|
| ✅ | Same values, transcribed from MudBlazor's source |
| ≈ | Same look, reached differently, or close with a stated difference |
| ❌ | Not available, with the reason |
| ➕ | Material's own, beyond MudBlazor, under a `material-` name: no MudBlazor parameter to transcribe |

### Properties, not classes

**An option that is one property of its control is that property.** A class that set
`CornerRadius="0"` would say what the property already says, and be one more ordinary word to
collide with an app's own styles. These are written as the property, and each is ✅ in its row
below:

| MudBlazor | Here |
|---|---|
| `MudPaper Square`, `MudAlert Square`, `MudDataGrid Square`, one `MudExpansionPanel`'s `Square` | `CornerRadius="0"` |
| `MudToolBar`'s and `MudAppBar`'s `Gutters="false"` | `Padding="0"` |
| `MudDataGrid Bordered` | `GridLinesVisibility="All"` |

A word keeps its class where it does more than that: `square` on a card squares its media too, on
an avatar it lets the image fill the corners, and on an `expansion-panels` group it reaches every
panel; `no-gutters` on an `Expander` or a `ListBox` reaches the header or every row, and on one
row it halves with `dense`; `inset` and `middle` move to the top of a `vertical` divider; and
`rounded` on a linear bar is `FiliCornerRadius`, so it follows an app's radius. The Size and
Variant words (`small`, `dense`, `outlined`, …) are classes everywhere, so that one word means one
thing on every control. `ClassVocabularyTests.NoClassIsOnePropertyOfItsOwnControl` fails a new
class that is only one fixed value on its own control.

**A MudBlazor icon parameter is a resource key.** Every glyph the theme draws is a
`DynamicResource`, so the app sets the key in the control's `Resources` (or a parent's, for a
whole region) and that control draws its own glyph in the theme's place:

```xml
<CheckBox Content="Favourite">
  <CheckBox.Resources>
    <StreamGeometry x:Key="FiliCheckBoxCheckedGeometry">M12 21.35l-1.45-1.32C5.4 15.36 …</StreamGeometry>
  </CheckBox.Resources>
</CheckBox>
```

| MudBlazor | Key |
|---|---|
| MudCheckBox `CheckedIcon`, `UncheckedIcon`, `IndeterminateIcon`; MudList's and MudTreeView's the same | `FiliCheckBoxCheckedGeometry`, `FiliCheckBoxUncheckedGeometry`, `FiliCheckBoxIndeterminateGeometry` |
| MudRadio `CheckedIcon`, `UncheckedIcon` | `FiliRadioCheckedGeometry`, `FiliRadioUncheckedGeometry` |
| MudSwitch `ThumbIcon` (none by default) | `FiliSwitchThumbIconGeometry` |
| MudSelect `OpenIcon` (shown while closed), `CloseIcon` (shown while open) | `FiliArrowDropDownGeometry`, `FiliArrowDropUpGeometry` |
| MudNumericField `NumericUpIcon`, `NumericDownIcon` | `FiliKeyboardArrowUpGeometry`, `FiliKeyboardArrowDownGeometry` |
| MudExpansionPanel `Icon`, MudNavGroup `ExpandIcon` | `FiliExpandMoreGeometry` |
| MudTreeView `ExpandButtonIcon` | `FiliChevronRightGeometry` |
| MudTabs `PrevIcon`, `NextIcon` | `FiliNavigateBeforeGeometry`, `FiliChevronRightGeometry` |
| MudSnackbar `CloseIcon`; CommonSnackbarOptions' `InfoIcon`, `SuccessIcon`, `WarningIcon`, `ErrorIcon`, `NormalIcon` | on the `WindowNotificationManager`: `FiliCloseGeometry`; `FiliInfoOutlinedGeometry`, `FiliAlertSuccessGeometry`, `FiliReportProblemGeometry`, `FiliErrorOutlineGeometry`, `FiliEventNoteGeometry` |

A glyph is drawn in a 24px box at its own size, as MudIcon draws its 24px viewBox, so a Material
path from MudBlazor's `Icons` class fits as it is. A key reaches everything under the control it
is set on: on an `Expander`, the expanders inside it too.

Every parameter of a MudBlazor 9.11 component that changes how it looks has a row, and every
component is in a section or under [Not here](#not-here). Parameters that only change behaviour
(events, binding, validation logic, `Class`, `Style`, `UserAttributes`) do not.

## Components

### MudButton → `Button`

Variant, colour and size are one class each and combine freely:
`<Button Classes="outlined error small" />`.

| MudBlazor | Here | |
|---|---|---|
| `Variant.Text` (default) | no class, or `text` | ✅ |
| `Variant.Outlined` | `outlined` | ✅ |
| `Variant.Filled` | `filled` | ✅ |
| `Color.Default` (default) | no class | ✅ text-primary; grey fill when filled |
| `Color.Primary` … `Color.Dark` | `primary` `secondary` `tertiary` `info` `success` `warning` `error` `dark` | ✅ |
| `Color.Inherit` | `inherit` | ✅ text and line follow the surrounding text colour |
| `Size.Small` / `Medium` / `Large` | `small` / no class / `large` | ✅ per-variant padding, 13 / 14 / 15px, on a 1.75 line: 30.75 / 36.5 / 42.25px in every variant |
| `Disabled` | `IsEnabled="False"` | ✅ |
| hover, `:focus-visible`, `:active` | the same state, as in `_button.scss` | ✅ `{color}-hover` for text and outlined, `{color}-darken` for filled |
| elevation 2 → 4 → 8 | the same | ✅ |
| `DropShadow="false"` | `flat` | ✅ elevation 0 at rest, hover, focus and press |
| `FullWidth` | `HorizontalAlignment="Stretch"` | ≈ |
| `StartIcon` / `EndIcon` | an icon in `Content` with `start-icon` / `end-icon` | ✅ 20px (18 small, 22 large), 4px out and 8px from the label (2px out when small) |
| `Ripple` (default true) | on | ✅ mudRipple.js: from the pointer, a circle twice the button's longer side grows over .5s while it rises to `RippleOpacity` 0.1 (0.2 filled) in the text colour, then fades over .4s once released and at least .5s old; clipped to the corners. ≈ `Ripple="false"` is `FiliRippleOpacity` and `FiliRippleOpacitySecondary` at 0 in the button's `Resources` |
| uppercase label (`Typography.Button.TextTransform`) | the same; `FiliButtonTextTransform` is the switch | ✅ a string label is drawn uppercase, and so is a plain `TextBlock` in content that is a control, such as the label beside a `start-icon`; `none`, `lowercase` or `capitalize` in the key change it as CSS would. ≈ a `TextBlock` with runs the app wrote keeps them as written |
| `IconColor`, `IconSize`, `IconClass` | the icon's own `Foreground`, `Width` / `Height`, `Classes` | ≈ ordinary Avalonia properties |
| `Href`, `Target` | — | ≈ no look of their own; a link is `HyperlinkButton` |

### MudIconButton → `Button.icon`

Put a `PathIcon` in `Content`: `<Button Classes="icon primary"><PathIcon Data="…" /></Button>`.

| MudBlazor | Here | |
|---|---|---|
| `Variant.Text` (default) | `icon` | ✅ round, 12px round a 24px icon |
| `Variant.Outlined` / `Filled` | `icon outlined` / `icon filled` | ✅ `default-borderradius`, 5px padding; filled lifts like a filled button |
| `Color.Default` (default) | no colour class | ✅ `action-default` grey, which beats a text button's text-primary |
| `Color.Primary` … `Color.Dark` | the colour classes | ✅ |
| `Size.Small` / `Large` | `small` / `large` | ✅ 3px round 18px; 12px round 36px (32px when outlined or filled) |
| `Edge` | `Margin` | ≈ an ordinary Avalonia property |
| `Icon` | a `PathIcon` in `Content` | ≈ the gallery carries a few Material glyphs; the theme ships no icon set |
| `DropShadow="false"` | `icon filled flat` | ✅ |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled |
| `Ripple` (default true) | on | ✅ as on a button: 0.1, 0.2 when `filled` |

### MudToggleIconButton → `ToggleButton.icon`

| MudBlazor | Here | |
|---|---|---|
| the component | `icon` on a `ToggleButton` | ✅ MudIconButton's look in all three variants, from the same generator as `Button.icon` |
| `Toggled` | `IsChecked` | ✅ |
| `Color` (default `Default`) | `unchecked-primary` … `unchecked-dark` | ✅ the colour while not checked; action-default with none |
| `ToggledColor` (default: `Color`) | `primary` … `dark` | ✅ the colour while checked; for one colour in both states, put both classes on |
| `Icon`, `ToggledIcon` | a `PathIcon`; for two, a `Panel` holding both, `Icon` first | ✅ the second shows only while checked; a lone icon stays |
| `Variant`, `Size`, `DropShadow="false"` | `outlined` / `filled`, `small` / `large`, `flat` | ✅ as on `Button.icon` |
| `ToggledSize`, `ToggledVariant` | the classes, bound to `IsChecked` | ≈ one size and one variant unless the app swaps them |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled |

### MudChip → `Button.chip`, `ToggleButton.chip`, `ContentControl.chip`

| MudBlazor | Here | |
|---|---|---|
| `Variant.Filled` (default) | `chip` | ✅ 32px pill, `action-disabled-background` grey, `action-disabled` on hover |
| neither `OnClick` nor `Href` | `chip` on a `ContentControl` | ✅ no `mud-clickable`: each variant's look at rest, no hover, ripple, hand cursor or focus |
| `Color.Default` (default) | no colour class | ✅ text-primary on that grey |
| `Variant.Outlined` / `Text` | `chip outlined` / `chip text` | ✅ |
| `Color.Primary` … `Color.Dark` | the colour classes | ✅ filled: colour, `{color}-darken` on hover; text: `{color}-hover` ground, 12% on hover |
| `Size.Small` / `Large` | `small` / `large` | ✅ 24 / 40px high, 12 / 16px text |
| selected (in a `MudChipSet`) | `IsChecked` on a `ToggleButton.chip` | ✅ `MudChip.GetVariant`'s swap: a selected filled chip draws as text, a selected text chip as filled |
| `Disabled` | `IsEnabled="False"` | ✅ |
| `Icon` | an icon in `Content` with `start-icon` | ✅ 20px (18 small, 24 large), 4px out and 4px in (6px on a large chip) |
| close icon | an icon or button in `Content` with `end-icon` | ≈ the close button's 18px and spacing; closing is the app's |
| `Avatar` | a `Border.avatar` in `Content` | ✅ `.mud-chip .mud-avatar`: 24px, 8px out and 8px in, 12px text; 18px, 4px and 10px on a small chip; 32px, 12px out and 8px in, 16px on a large one |
| `MudChipSet` selection rules | — | ❌ single/multi selection is the app's, through `IsChecked` bindings |
| `Label` | `CornerRadius="4"` | ≈ `.mud-chip-label`'s default radius in place of the pill |
| `SelectedColor` | a colour class the app sets while `IsChecked` | ≈ |
| `IconColor` | the icon's `Foreground` | ≈ |
| `CheckedIcon`, `MudChipSet.CheckMark` | an icon in `Content` | ≈ shown by the app while `IsChecked` |
| `MudChipSet`'s `Variant`, `Color`, `Size`, `Label`, `CloseIcon`, `AllClosable`, `Disabled` | the classes on each chip, or one `Style` over the container | ≈ a chip set is a panel of chips; nothing cascades |
| `Ripple` (default true) | on | ✅ 0.2 on a filled chip, the default, and 0.1 on `outlined` and `text`; a selected chip keeps its variant's |

### MudFab → `Button.fab`

| MudBlazor | Here | |
|---|---|---|
| the component | `fab` on a `Button` | ✅ round, at elevation 6, lifting to 8 on hover and focus and 12 on press |
| `Variant.Filled` (default) / `Outlined` / `Text` | no class / `outlined` / `text` | ✅ filled is grey `action-default-hover`, `action-disabled-background` on hover; outlined and text never cast a shadow |
| `Color.Default` (default) | no colour class | ✅ text-primary |
| `Color.Primary` … `Color.Dark` | the colour classes | ✅ filled: the colour, `{color}-darken` on hover; outlined and text: the colour as content, `{color}-hover` on hover |
| `Color.Inherit` | — | ✅ draws as `Color.Default`, because `_fab.scss` has no rule for the classes it emits |
| `Size.Large` (default) / `Medium` / `Small` | no class / `medium` / `small` | ✅ 56 / 48 / 40px |
| `Label` | `extended`, the label in `Content` | ✅ a pill 48 / 40 / 34px high and 16 / 16 / 12px a side |
| `StartIcon`, `EndIcon` | an icon in `Content` with `start-icon` / `end-icon` | ✅ 24px, `IconSize`'s default; 8px from the label (4px small) |
| `IconSize`, `IconColor` | the icon's own `Width`, `Height`, `Foreground` | ≈ ordinary Avalonia properties |
| `DropShadow="false"` | `flat` | ✅ |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled on action-disabled-background, flat; outlined and text keep no ground |
| uppercase label | the same | ✅ as on every button: a string `Content` |
| `Ripple` (default true) | on | ✅ 0.2 on a coloured FAB, 0.1 on the default, text and outlined ones |

### MudAvatar → `Border.avatar`

Put initials, a `PathIcon` or an `Image` inside: `<Border Classes="avatar primary"><TextBlock Text="M" /></Border>`.

| MudBlazor | Here | |
|---|---|---|
| the component | `avatar` on a `Border` | ✅ its content centred |
| `Variant.Filled` (default) / `Outlined` / `Text` | no class / `outlined` / `text` | ✅ filled is text-primary on lines-inputs; outlined a 1px line on no ground; text the bare `.mud-avatar`, white on gray-light whatever the colour, since `_avatar.scss` has no text-variant rule |
| `Color.Primary` … `Color.Dark` | the colour classes | ✅ filled: the colour with its contrast text; outlined: the colour as text and line |
| `Size.Medium` (default) / `Small` / `Large` | no class / `small` / `large` | ✅ 40 / 24 / 56px, with 20 / 12 / 24px text |
| `Rounded` / `Square` | `rounded` / `square` | ✅ 4px / no radius |
| `Elevation` | the elevation classes | ✅ the avatar never clips itself, so its shadow survives |
| a `MudImage` child | an `Image` inside, or an `ImageBrush` as `Background` | ✅ covers the avatar and is clipped to its shape (at a size of your own, an `ImageBrush` needs nothing more) |
| a `MudIcon` child | a `PathIcon` inside | ✅ 24px, `MudIcon`'s default size |
| `MudAvatarGroup` `Spacing` | a `StackPanel` with a negative `Spacing` | ≈ |
| `MudAvatarGroup` `Outlined`, `OutlineColor` | `BorderThickness="2"` and a `BorderBrush` on each avatar | ≈ the 2px ring is two ordinary properties |
| `MudAvatarGroup` `Max`, the "+n" avatar | — | ❌ counting the avatars is code |

### MudBadge → `HeaderedContentControl.badge`

`Content` is what MudBadge wraps and `Header` is the badge:
`<HeaderedContentControl Classes="badge error" Header="{Binding Unread}"><PathIcon Data="…" /></HeaderedContentControl>`.

| MudBlazor | Here | |
|---|---|---|
| the component | `badge` on a `HeaderedContentControl` | ✅ the root shrinks to its content, like the inline-block `.mud-badge-root` |
| `Content` | `Header` | ✅ a 20px pill, at least 20px wide, 12px text |
| `Max` | — | ≈ fixed at its default: an `int` over 99 reads `99+` |
| `Color.Default` (default) | no colour class | ✅ text-primary on gray-light |
| `Color.Primary` … `Color.Dark` | the colour classes | ✅ `.mud-theme-{color}`, on the badge alone and not on what it decorates |
| `Origin.TopRight` (default) | no class | ✅ `calc(100% - 4px)` from the opposite edges: 4px into the corner |
| the other `Origin`s | `top-left`, `top-center`, `center-left`, `center-center`, `center-right`, `bottom-left`, `bottom-center`, `bottom-right` | ✅ |
| `Overlap` | `overlap` | ✅ 12px into the corner |
| `Dot` | `dot` | ✅ a 9px circle with nothing in it |
| `Bordered` | `bordered` | ✅ a 2px surface ring |
| `Icon` | `icon`, with a `PathIcon` as the `Header` | ✅ 12px in a 20px circle |
| `Elevation` | the elevation classes | ✅ on the badge |
| `Visible` | a `Header` that is not null | ≈ `null` hides the badge, a dot's too |
| `OnClick` | — | ≈ the badge takes the pointer; put a control that handles the click in `Header` |

### MudText → `TextBlock`

| MudBlazor | Here | |
|---|---|---|
| `Typo.h1` … `Typo.h6` | `h1` … `h6` | ✅ size, weight, line height and letter spacing from `Typography.cs` |
| `Typo.subtitle1` / `subtitle2` | `subtitle1` / `subtitle2` | ✅ |
| `Typo.body1` / `body2` | `body1` / `body2` | ✅ body1 is also the default text |
| `Typo.caption` / `overline` | `caption` / `overline` | ✅ no colour of their own, as `.mud-typography-*` sets none: the colour round them |
| `Typo.button` | `button` | ✅ 14px, 500, a 24px line, 0.4px tracking ≈ not uppercase: a `TextBlock`'s own `Text` cannot be transformed by a style, so write it in capitals |
| `Color.Primary` … `Color.Dark` | `primary` `secondary` `tertiary` `info` `success` `warning` `error` `dark` | ✅ the palette colour; beats a type style's own colour |
| `Color.Inherit` | `inherit` | ✅ the surrounding text colour, over a colour class too |
| `mud-text-secondary` (grey) | `Foreground="{DynamicResource FiliTextSecondaryBrush}"` | ≈ a utility class in MudBlazor, not a `Color` |
| `Align`, `GutterBottom`, `Inline` | `TextAlignment`, `Margin` | ≈ ordinary Avalonia properties |

### MudPaper and MudCard → `Border`

| MudBlazor | Here | |
|---|---|---|
| `MudPaper` | `Border.paper` | ✅ the surface, the default radius and text-primary, at MudPaper's default `Elevation` 1; an elevation class replaces it |
| `Outlined` | `outlined` | ✅ a 1px line in lines-default and no shadow, whatever the elevation, as MudPaper adds `mud-elevation-{n}` only when not outlined |
| `Elevation="0…25"` | `elevation0` … `elevation25` | ✅ every level `_elevation.scss` has; three shadow layers each, from `Shadow.cs` |
| (none: Material's dark-theme overlay) | `material-elevation-overlay` on a window or container | ➕ in dark, each raised surface inside lightened by white at its level's opacity, 5% at 1 to 16% at 24: papers, cards, panels, grids, menus, selects and flyouts. Light and high contrast unchanged |
| `Square` | `CornerRadius="0"` on a paper; `square` on a card | ✅ no radius; a card's media squares with it |
| `MudCard` | `Border.card` | ✅ a paper at elevation 1 in text-primary; `outlined` drops the shadow whatever elevation class is on it, as MudPaper does |
| `MudCardHeader` | `card-header`, on a `DockPanel` | ✅ 16px round |
| `CardHeaderAvatar` / `CardHeaderContent` / `CardHeaderActions` | `card-header-avatar` / `card-header-content` / `card-header-actions` | ✅ docked at the start 16px from the content, filling the rest (so it goes last), docked at the end pulled 8px up and out |
| `MudCardMedia` | `card-media`, on a `Border` | ✅ 300px high; an `Image` inside covers it; it takes the card's top corners when it comes first |
| `MudCardContent` | `card-content` | ✅ 16px round |
| `MudCardActions` | `card-actions`, on a `StackPanel` | ✅ a row, 8px round, its items centred |
| `ContentPadding="false"` | `Margin="0"` on the part | ≈ the paddings are margins here, since a panel has none |
| `Width`, `Height`, `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight` | the same | ≈ ordinary Avalonia properties |
| `MudCardMedia` `Height`, `Image` | `Height`; an `Image` inside, or an `ImageBrush` as `Background` | ≈ |

### MudAppBar → `Border.appbar`

| MudBlazor | Here | |
|---|---|---|
| background, text colour | `appbar` | ✅ text colour is inherited, so a control's own colour wins |
| height | `FiliAppbarHeight` | ✅ 64px |
| `Elevation` (default 4) | no class, or the elevation classes | ✅ |
| `Dense` | `dense` | ✅ 48px |
| `Color` | the colour classes | ✅ `.mud-theme-{color}`: the colour as ground, its contrast text inherited |
| `Gutters` (default true) | `Padding="0"` for false | ✅ 24px a side, or none |
| `Bottom`, `Fixed` | — | ≈ placement, which is the app's layout |
| `WrapContent` | a `WrapPanel` inside | ≈ |
| `Contextual` | — | ❌ swapping the bar's content on selection is code |

### MudTextField → `TextBox`

| MudBlazor | Here | |
|---|---|---|
| `Variant.Text` (default) | no class | ✅ |
| `Variant.Filled` / `Outlined` | `filled` / `outlined` | ✅ |
| `Label` | `PlaceholderText` | ≈ **the placeholder is the floating label**, so there is no separate placeholder |
| `Placeholder` | `placeholder`, and the text in `PlaceholderText` | ✅ the hint where the text goes, in its colour at .42, never floating; with no label, as a field has one or the other |
| no `Label`, and `MudInput` | no `PlaceholderText` | ✅ no room for a label: a standard field is MudInput's 32px (29 dense), a filled one centres its text (`.mud-input-root-hidden-label`) |
| `Error="true"` | `error` | ✅ |
| validation | a binding's own validation error | ✅ message under the field at 12px, in the helper text's place |
| `HelperText` | `AutomationProperties.HelpText` | ✅ caption, text-secondary; inset 4px under filled, 8px under outlined; read by a screen reader too |
| `HelperTextOnFocus` | `helper-on-focus` | ✅ the help keeps its room and shows while the field has focus |
| `Counter` | `counter` with `MaxLength` | ✅ `12 / 50` on the right; the length alone when `MaxLength` is 0, as `Counter="0"` |
| `Margin.Dense` | `dense` | ✅ standard 3px shorter, filled 8px, outlined 16px, with the label resting and floating to match |
| `Margin.Normal` | `Margin` | ≈ only an outer margin in MudBlazor; an ordinary Avalonia property |
| `Adornment`, `AdornmentIcon`, `AdornmentText` | `InnerLeftContent` / `InnerRightContent` | ✅ 8px from the text, as `.mud-input-adornment-start` and `-end` are, beside a spin column too; an icon there marked `start-icon` or `end-icon` is MudIcon's 24px, in the field's text colour. A start adornment floats the label, as `Adornment.Start` does. On a filled or outlined box an icon is centred on the box, as `.mud-input` centres it; a text adornment, unlike MudBlazor's, stays on the text line, so a unit is not lifted above the value it is read with |
| `AdornmentColor`, `IconSize` | a colour class, or `Width` / `Height`, on the icon | ≈ ordinary Avalonia properties |
| `Disabled` | `IsEnabled="False"` | ✅ text-disabled text, label and helper text; the line stays lines-inputs and turns dotted, 1px on and 1px off, on a text field and a select alike |
| `ReadOnly` | `IsReadOnly` | ✅ looks the same, as in MudBlazor |
| `Underline="false"` | `no-underline` | ✅ neither the resting nor the focus rule; an outlined field has none anyway |
| `Required` | `required` | ✅ `*` straight after the label, as `.mud-input-label::after` |
| `ShrinkLabel` | `shrink-label` | ✅ the label floated with nothing typed, in text-primary |
| the label over text | the same | ✅ text-primary once floated by its text (`.mud-shrink ~ label`), primary while focused |
| `Lines`, `AutoGrow`, `MaxLines` | `AcceptsReturn`, `TextWrapping`, `MinLines`, `MaxLines` | ≈ ordinary Avalonia properties; with `AcceptsReturn` the text starts at the top and the label sits above it, as a textarea's do |
| `Clearable` | a button in `InnerRightContent` | ≈ the app clears the text |
| `Typo` | `FontSize`, `LineHeight` from the `Fili{Style}…` tokens | ≈ |
| `FullWidth` | `HorizontalAlignment="Stretch"` | ≈ |
| `InputType.Password` | `PasswordChar` | ≈ |
| `MudMask` | `MaskedTextBox` | ✅ it is a `TextBox` to the theme (its `StyleKeyOverride`), so every row above applies |
| `MudField` | a `Label` over any content | ≈ there is no input box without an input |

### MudSelect, MudAutocomplete, MudNumericField

| MudBlazor | Here | |
|---|---|---|
| `MudSelect` | `ComboBox` | ✅ a standard text field with a drop-down adornment, as `_select.scss` makes it |
| `Label` | `PlaceholderText` | ≈ the floating label |
| `Error` | `error` | ✅ |
| `Variant` on a select | `filled` / `outlined` | ✅ the text field's own variants |
| `HelperText`, `Margin.Dense` on a select | `AutomationProperties.HelpText`, `dense` | ✅ as on a text field; a failing binding shows its message too |
| `MudAutocomplete` | `AutoCompleteBox` | ✅ the popup on a surface at elevation 8 |
| `MudNumericField` | `NumericUpDown` | ✅ a 24px spin column inside the input box, top to bottom, as `.mud-input-numeric-spin` is: the underline, the filled ground and the outline run on under it, and the text keeps 24px clear |
| `Variant`, `Margin.Dense`, `Error`, `Counter`, `HelperText`, `HelperTextOnFocus` on either | the text field's classes and `AutomationProperties.HelpText`, on the outer control | ✅ passed to the `TextBox` inside; a `NumericUpDown`'s validation error shows in its helper line |
| `MaxHeight` (default 300) on a select or an autocomplete | `MaxDropDownHeight` | ✅ 300, where Avalonia's default is 200 |
| `Disabled` on a select | `IsEnabled="False"` | ≈ as a disabled text field, its arrow in text-disabled |
| `OpenIcon` / `CloseIcon` (defaults) | the same | ✅ `ArrowDropDown`, turned to `ArrowDropUp` while the list is open |
| `AdornmentColor` on a select | `adornment-primary` … `adornment-dark`, `adornment-inherit` | ✅ the arrow in the colour, or the colour round the select; text-disabled when disabled |
| `OpenIcon` / `CloseIcon` set to other glyphs | `FiliArrowDropDownGeometry` / `FiliArrowDropUpGeometry` in the select's `Resources` | ✅ |
| `Underline`, `Required`, `ShrinkLabel` on a select, an autocomplete or a numeric field | `no-underline`, `required`, `shrink-label` | ✅ as on a text field |
| `Dense` on the items | `dense-items` on the select, or `dense` on one `ComboBoxItem` | ✅ 4px top and bottom, body2: 36px rows; `dense` on the select is its field's `Margin.Dense` |
| an option's height | the same | ✅ 48px, MudListItem's 8px and `.mud-list-item-text`'s 4px round a body1 line |
| `MultiSelection`, `SelectAll` | — | ❌ a `ComboBox` holds one value |
| `Clearable` | a button in `InnerRightContent` on an `AutoCompleteBox` or `NumericUpDown` | ≈ the app clears the value; a `ComboBox` has no `InnerRightContent` |
| MudAutocomplete's drop-down arrow | an arrow in `InnerRightContent` | ≈ an `AutoCompleteBox` shows none of its own |
| `ShowProgressIndicator`, `ProgressIndicatorColor` | a `ProgressBar.circular.small` in `InnerRightContent` | ≈ |
| `HideSpinButtons` | `ShowButtonSpinner="False"` | ≈ |

### MudCheckBox, MudRadio, MudSwitch

| MudBlazor | Here | |
|---|---|---|
| `MudCheckBox` | `CheckBox` | ✅ the real Material glyphs, swapped per state |
| `MudRadio` | `RadioButton` | ✅ checked grows an inner disc |
| `MudSwitch` | `ToggleSwitch` | ✅ 20px thumb over a 14px track |
| `TriState` | `IsThreeState` | ✅ the indeterminate glyph |
| `Color.Default` (default) | no class | ✅ grey `action-default` glyphs, checked or not; the switch's `#fafafa` thumb on and off |
| `Color.Primary` … `Color.Dark` | `primary` … `dark` | ✅ every glyph and the halo; on a switch, thumb and track only when on |
| `Size.Small` / `Large` | `small` / `large` | ✅ glyph 20 / 36px; switch 14 / 26px thumb on its own span |
| `UncheckedColor` | `unchecked-primary` … `unchecked-dark` | ✅ on a checkbox or radio: the glyph and its halo while not checked (a checkbox's null too), in place of the colour class |
| `Color.Inherit`, `UncheckedColor.Inherit` | `inherit`, `unchecked-inherit` | ✅ on a checkbox or radio: the glyph in the surrounding text colour, with no halo, as `hover:mud-inherit-hover` names no rule |
| `MudSwitch.UncheckedColor` | `unchecked-primary` … `unchecked-dark` | ✅ the thumb and the track while off |
| `Disabled` | `IsEnabled="False"` | ✅ the glyph and the label in text-disabled, `.mud-checkbox.mud-disabled *` |
| `ReadOnly` | `IsHitTestVisible="False"` | ≈ looks the same, as in MudBlazor |
| `Dense` | `dense` on a checkbox or radio | ✅ 4px round the glyph: 32px; 28 small, 44 large |
| the hit circle | the same | ✅ 48px, `.mud-icon-button`'s 12px round the 24px glyph; 44 small, 60 large |
| `LabelPlacement` `End` (default) | no class | ✅ |
| `LabelPlacement` `Start`, `Top`, `Bottom` | `label-start`, `label-top`, `label-bottom` | ✅ row-reverse, column-reverse and column, the two centred when stacked; on a checkbox, radio or switch |
| `CheckedIcon`, `UncheckedIcon`, `IndeterminateIcon` | the glyph keys in the control's `Resources` | ✅ see [the keys](#mudblazor-parity) |
| `MudSwitch.ThumbIcon` | `FiliSwitchThumbIconGeometry` in the switch's `Resources` | ✅ MudIcon filling the thumb with 2px padding: 16px in a 20px thumb |
| `MudSwitch.ThumbIconColor` (default `Default`) | no class, or `thumb-primary` … `thumb-dark` | ✅ text-secondary, `.mud-icon-default`, or the colour |
| `MudSwitch.Disabled` | `IsEnabled="False"` | ✅ the thumb gray-default, the track keeping its colour at 12%, the label text-disabled |
| `Ripple` (default true) | on | ✅ in the halo only, as `.mud-ripple-checkbox` / `-radio` / `-switch` is the icon button round the glyph: in the glyph's colour (`UncheckedColor` while off), text-primary with no colour; a switch's rides with its thumb |
| `MudRadioGroup` | a panel of `RadioButton`s with one `GroupName` | ≈ no look of its own |

### MudSlider → `Slider`

| MudBlazor | Here | |
|---|---|---|
| `Size.Small` (default) | no class | ✅ 2px rail, 12px thumb |
| `Size.Medium` / `Large` | `medium` / `large` | ✅ 4px / 20px and 6px / 24px |
| `Variant.Text` (default) | no class | ✅ the whole rail the colour at 30%, no solid part |
| `Variant.Filled` | `filled` | ✅ the colour, solid, from the start to the thumb |
| `Color.Primary` (default) … `Color.Dark` | no class, or `secondary` … `dark` | ✅ thumb, rail, filled part, ticks and value label |
| hover, focus and press | the same | ✅ a 1px / 2px ring of the colour at 24%; no growth |
| vertical | `Orientation="Vertical"` | ✅ |
| `TickMarks` | `TickPlacement` other than `None`, at `TickFrequency` or `Ticks` | ✅ dots on the rail, its thickness and the colour |
| `TickMarkLabels` | — | ❌ a `Slider` has nowhere to put the strings; see [Known gaps](known-gaps.md) |
| `ValueLabel` | `value-label` | ✅ a 12px chip of the colour above the thumb while it is held |
| `ValueLabelFormat`, `Culture` | — | ≈ at most two decimals, in the app's culture rather than MudBlazor's invariant default |
| `ValueLabelContent` | — | ❌ the label shows the value |
| `Disabled` | `IsEnabled="False"` | ✅ the rail and thumb at .38 in their colour, the thumb ringed 1px in white; the filled part and ticks keep their strength, as elements outside the native input. The rule's black, ×4 thumb loses the cascade to the colour and size rules |
| `ChildContent` | a `TextBlock` above it | ≈ |

### MudProgressLinear → `ProgressBar`

| MudBlazor | Here | |
|---|---|---|
| `Size.Small` (default) | no class | ✅ a 4px square hairline |
| `Size.Medium` / `Large` | `medium` / `large` | ✅ 8 / 12px |
| `Rounded` | `rounded` | ✅ |
| `Indeterminate` | `IsIndeterminate` | ✅ MudBlazor's two-bar animation |
| `Color` | `primary` … `dark` | ✅ every palette colour, the track at 20% of it |
| `Striped` | `striped` | ✅ 25% white bands at 135°, 40px apart, sliding as MudBlazor's do |
| `Buffer`, `BufferValue` | — | ❌ it needs a second value `ProgressBar` does not have |
| `Vertical` | `Orientation="Vertical"` | ✅ every size |
| `Min`, `Max`, `Value` | `Minimum`, `Maximum`, `Value` | ✅ |
| `ShowBackground="false"` | `Background="Transparent"` | ≈ |
| `ChildContent` | `ShowProgressText` | ≈ the formatted value centred over the bar, whole however thin the bar; not arbitrary content |

### MudProgressCircular → `ProgressBar.circular`

| MudBlazor | Here | |
|---|---|---|
| the component | `circular` on a `ProgressBar` | ✅ a ring drawn with an `Arc`, from 12 o'clock clockwise |
| `Size.Medium` (default) / `Small` / `Large` | no class / `small` / `large` | ✅ 40 / 24 / 56px |
| `StrokeWidth` | — | ≈ fixed at MudBlazor's default 3, scaled with the ring as the SVG viewBox scales it |
| `Color` | no class, `primary` … `dark` | ✅ `text-secondary` by default, no track |
| `Rounded` | `rounded` | ✅ round line caps |
| `Indeterminate` | `IsIndeterminate` | ✅ circular-rotate and circular-dash, key frame for key frame, eased per segment |
| `Min`, `Max`, `Value` | `Minimum`, `Maximum`, `Value` | ✅ the 300ms ease on a value change too |
| `ChildContent` | `ShowProgressText` | ≈ the formatted value in the middle, in the ring's colour; not arbitrary content |

### MudDivider → `Separator`

| MudBlazor | Here | |
|---|---|---|
| default | no class | ✅ 1px, no margin |
| `DividerType.Inset` / `Middle` | `inset` / `middle` | ✅ 72px / 16px |
| `Vertical` | `vertical` | ✅ |
| `Light` | `light` | ✅ `divider-light` |
| `Absolute`, `FlexItem` | a `Panel` overlay; alignment | ≈ layout |

### MudLink → `HyperlinkButton`

| MudBlazor | Here | |
|---|---|---|
| `Underline.Hover` (default) | no class | ✅ |
| `Underline.Always` / `None` | `underline` / `no-underline` | ✅ |
| `Color.Primary` (default) | no class | ✅ |
| `Color.Secondary` / `Inherit` | `secondary` / `inherit` | ✅ `inherit` is the surrounding text colour |
| `Color.Tertiary` … `Color.Dark` | `tertiary` … `dark` | ✅ every palette colour as the text |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled |
| `Typo` | a type class on a `TextBlock` inside | ≈ |
| `StartIcon`, `EndIcon` | an icon in the content marked `start-icon` / `end-icon` | ✅ `.mud-link-icon-start` / `-end`: the link's own font size and 4px from the text |

### MudTabs → `TabControl`, `TabStrip`

| MudBlazor | Here | |
|---|---|---|
| 48px strip, 2px indicator | the same | ✅ |
| tab bar | the same | ✅ the surface colour, with no rule under it until `Border` |
| tab text | the same | ✅ text-primary, primary when active, text-disabled when disabled |
| hover | the same | ✅ `action-default-hover`; `primary-hover` on the active tab |
| `MinimumTabWidth` (160px) | the same | ✅ |
| `Color` | a colour class on the `TabControl` or `TabStrip` | ✅ the bar in the colour; tabs and indicator in its contrast text; `{color}-lighten` on the active tab's hover |
| `Border` / `Outlined` | `border` / `outlined` | ✅ a `lines-default` rule under the bar, or round it |
| `Rounded` / `Centered` | `rounded` / `centered` | ✅ |
| (none: Material's fixed tabs) | `material-fixed-tabs` | ➕ the tabs share the bar in equal parts, under `MinimumTabWidth`, and never scroll |
| `HideSlider` | `hide-indicator` | ✅ |
| `Position` | `TabStripPlacement` on a `TabControl` | ✅ Top, Bottom, Left, Right: the bar docks there, `border` draws its rule on the content side, and the indicator follows |
| `Elevation` | `elevation0` … `elevation25` | ✅ on the bar, outside `rounded`'s clip |
| `SliderColor` | `indicator-primary` … `indicator-dark` | ✅ the indicator alone, over any bar colour |
| sliding indicator, `SliderAnimation` (default true) | the indicator slides; `no-animation` for false | ✅ .3s, cubic-bezier(.64,.09,.08,1), the width jumping; `no-animation` puts the new tab's indicator under it at once |
| uppercase tab text (`.mud-tab`) | the same | ✅ a string `Header` is uppercase; `FiliButtonTextTransform` is the switch, as for buttons |
| `Disabled` on the tabs | `IsEnabled="False"` | ✅ text-disabled |
| `MudTabPanel.Icon`, `IconColor` | an icon in the `Header` marked `start-icon` | ✅ MudIcon's 24px, `.mud-tab-icon-text` 8px from the text, in the tab's colour unless set |
| `BadgeData`, `BadgeDot`, `BadgeColor`, `BadgeMax` | a `HeaderedContentControl.badge` as the `Header` | ≈ |
| `ToolTip` | `ToolTip.Tip` on the `TabItem` | ≈ |
| scroll buttons | the same | ✅ ChevronLeft and ChevronRight icon buttons at the bar's ends while the tabs overflow it, each disabled at its end, a bar's length per click; the selected tab scrolls into view; on a side bar, 32px high and turned |
| `PrevIcon`, `NextIcon` | `FiliNavigateBeforeGeometry`, `FiliChevronRightGeometry` in the tabs' `Resources` | ✅ drawn at their own size in the 24px box |
| `AlwaysShowScrollButtons`, `ScrollIconColor` | — | ❌ the buttons are action-default, and only shown on overflow |
| `ShowCloseIcon` | a button in the `Header` | ≈ closing is the app's |
| `Header`, `HeaderPosition`, `TabPanelHeader` | `HeaderTemplate`, or content beside the bar | ≈ |
| `MaxHeight` | `MaxHeight` | ≈ |
| `ApplyEffectsToContainer` | a `Border.paper` with an elevation class round the control | ≈ |
| `Ripple` (default true) | on | ✅ in the tab's text colour |

### MudList → `ListBox`

| MudBlazor | Here | |
|---|---|---|
| a MudListItem with `OnClick` or `Href`, outside a selecting list | `Button.list-item` | ✅ the row's look and ripple, its label in its own case; `dense` and `no-gutters` |
| selected item | the same | ✅ primary text over primary at 6%, never a filled bar |
| `Dense` | `dense` on the `ListBox` or an item | ✅ 4px top and bottom, body2 |
| the text | the same | ✅ body1, MudListItem's own; body2 when dense |
| a row's height | the same | ✅ 48px, 36 dense: the row's 8px (4px) and `.mud-list-item-text`'s 4px round the line |
| on a paper | `ListBox.paper` | ✅ |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled |
| `Color` (the selection, default `Primary`) | `primary` … `dark` on the `ListBox` | ✅ the colour as the text over its hover tint |
| `Gutters` (default true) | `no-gutters` on the `ListBox` or an item | ✅ no 16px sides |
| `Padding` (default true) | `Padding="0"` | ≈ |
| `Icon` | a `PathIcon` in the content marked `start-icon` | ✅ `.mud-list-item-icon`: 24px in a 56px column, action-default (action-disabled on a disabled row), the text centred beside it; on a list row and a `list-item` |
| `IconColor`, `AvatarContent`, `SecondaryText`, `Inset` | content in the item | ≈ the app lays out the row |
| `NestedList`, `Expanded`, `ExpandMoreIcon`, `ExpandLessIcon` | `TreeView` | ≈ a `ListBox` is flat; a tree nests |
| `SelectionMode.MultiSelection` | `multi-selection` and `SelectionMode="Multiple, Toggle"` | ✅ a 48px checkbox on each row, 8px into the gutter, checked while the row is selected; no row is marked selected. ≈ two settings for one parameter: a XAML list pins `SelectionMode` before any style can set it |
| `CheckBoxColor` (default `Default`) | no class, or `primary` … `dark` on the list while `multi-selection` is on | ✅ both glyphs and the halo; ≈ the list's `Color` class doubles as it, the selection colour being unused while selecting many |
| `MudListSubheader` | a `TextBlock` with `Margin` | ≈ |
| `Ripple` (default true) | on | ✅ on every row, and on a select's options, which are MudListItems |

### MudExpansionPanels → `Expander`

| MudBlazor | Here | |
|---|---|---|
| `MudExpansionPanels`, the group | `expansion-panels` on the `StackPanel` that holds the Expanders, or on an `ItemsControl` that generates them | ✅ one shape: the group's radius on the first panel's top and the last one's bottom, square between |
| panel on a surface at elevation 1 | the same | ✅ |
| 15px header | the same | ✅ `.9375rem`, sized on its own |
| `Elevation` (default 1) | no class, or `elevation0` … `elevation25` | ✅ on the panel; `elevation0` keeps the surface |
| `Square` | `CornerRadius="0"` | ✅ no radius |
| `Dense` | `dense` | ✅ the content loses its 16px top and bottom |
| `Gutters` (default true) | `no-gutters` for false | ✅ the header and the content lose their 24px sides |
| `Dense`, `Gutters` on MudExpansionPanels | `dense`, `no-gutters` on each panel | ≈ the group's values do not cascade: the classes go on every panel |
| `Outlined` (default true) | in a group, on; `no-outline` on the group for false | ✅ a 1px lines-default rule under each panel but the last, and under none that is expanded or comes before one; `no-outline` rules none |
| an expanded panel | in a group | ✅ 16px from its neighbours (none above the first or below the last) over .3s, every corner rounded; the panel before it takes the bottom corners and the one after it the top |
| `Square` on the group | `square` on the group | ✅ no radius on any panel |
| `MultiExpansion` (default false) | no class, or `multi-expansion` for true | ✅ opening a panel closes the others, and of panels written open the first stays open; `multi-expansion` lets them open independently |
| `Disabled` | `IsEnabled="False"` | ✅ text-disabled header, action-disabled chevron |
| `HideIcon` | `hide-icon` | ✅ no chevron |
| `Icon` | `FiliExpandMoreGeometry` in the panel's `Resources` | ✅ |
| `MaxHeight` | `MaxHeight` on the content | ≈ |
| opening | the same | ✅ MudCollapse: the content grows into view over .3s `ease-in-out` and shrinks away closing, from where it is if turned back half-way |

### MudCollapse

| MudBlazor | Here | |
|---|---|---|
| the component | an `Expander`'s content, or `IsVisible` | ≈ Avalonia has no headerless collapse |

### MudToggleGroup, MudButtonGroup, MudMenu

| MudBlazor | Here | |
|---|---|---|
| `MudToggleGroup` item | `ToggleButton` | ✅ a MudButton with no border, shadow or minimum width, on its 1.75 line, 36.5px: the colour as text, `{color}-hover` under the pointer; filled once checked, `{color}-darken` under the pointer |
| `Color` (default `Primary`) | no class, or `primary` … `dark` | ✅ on each item |
| `Outlined` (default true) | `outlined` | ≈ opt-in: an Avalonia toggle is one item, not a group, so the group's 1px line in the colour is a class on each |
| `Size.Small` / `Large` | `small` / `large` | ✅ 4px / 8px at 13 / 15px; 3px / 7px when outlined |
| `Color.Default` | — | ❌ no class names it: a toggle's default is `Primary` |
| `Vertical` | a vertical `StackPanel` of toggles | ≈ |
| `Delimiters` (default true) | — | ≈ each outlined item draws its own line, so two side by side meet in a 2px one; `Spacing="-1"` shares it |
| `CheckMark`, `SelectedIcon`, `UnselectedIcon`, `FixedContent`, `CheckMarkClass` | an icon in `Content` | ≈ shown by the app while `IsChecked` |
| `SelectedClass` | a `:checked` style | ≈ |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled text, and the line or ground in action-disabled-background |
| `Ripple` (default true) | on | ✅ on toggle items, both halves of a split button (0.2 when `filled`), drop-down buttons and menu rows |
| a two-part `MudButtonGroup` | `SplitButton` | ✅ Button's classes: `outlined` / `filled`, any colour, `small` / `large` |
| group separator | the same | ✅ text-primary or the colour; `divider` when filled; `{color}-lighten` between filled coloured segments |
| `DropShadow="false"` | `filled flat` | ✅ on `SplitButton` and `DropDownButton` alike |
| `Vertical` | — | ≈ not applicable: `SplitButton` is two halves side by side; a vertical group is a `StackPanel` of buttons |
| a `MudButtonGroup` of three or more | `button-group` on a `StackPanel` of buttons | ✅ the radius on the outer corners only, each button after the first 1px over the one before so outlined lines are shared, a 1px line before each text button in its colour and before each filled one in `{color}-lighten` or `divider`; a hidden button leaves the group |
| `MudButtonGroup` `Vertical` | `Orientation="Vertical"` on the `StackPanel` | ✅ the same, top to bottom |
| `MudButtonGroup` `Variant.Filled`'s elevation | — | ≈ each filled button rests flat and lifts to 4, as in the group; the group's own elevation 2 is the app's, a `StackPanel` having no shadow |
| `MudButtonGroup` `FullWidth` | `HorizontalAlignment="Stretch"` | ≈ |
| `MudButtonGroup` `OverrideStyles` | — | ❌ nothing cascades; put the classes on each button |
| `MudMenu` with a button activator | `DropDownButton` | ✅ Button's classes; an unclassed one is `Color.Default`, text-primary |
| menu items | `MenuItem` | ✅ 48px rows, 8px 12px round a body1 label with 4px above and below; strip items and dropdown rows are separate themes |
| `MudMenuItem.Icon` | `MenuItem.Icon` | ✅ 24px in action-default, 12px from the label; every row in a list with an icon keeps the gutter, a list without has none |
| the list | the dropdown | ✅ 4px above and below, at least 112px wide, 4px either side of a separator |
| `Dense` | `dense` on the `ContextMenu`, on a `MenuFlyout`'s `FlyoutPresenterClasses`, or on the item whose submenu it is | ✅ 2px 12px rows in body2 |
| `MaxHeight` | `MaxHeight` on the `ContextMenu` or flyout | ≈ |
| an icon-button activator (`Icon`) | a `Button.icon` with a `Flyout` | ≈ |
| `StartIcon`, `EndIcon`, `IconColor`, `FullWidth` on the activator | the `DropDownButton`'s content and alignment | ≈ |
| `MudMenuItem.IconColor` | the icon's `Foreground` | ≈ action-default unless set |
| `MudMenuItem.Disabled` | `IsEnabled="False"` | ✅ action-disabled, its icon too |
| a submenu's arrow | the same | ✅ Filled.ArrowRight at 24px in action-default, mirrored under RTL as MudBlazor swaps in ArrowLeft; action-disabled when disabled |
| `HideSubMenuArrow` | `hide-icon` on the item whose submenu it is | ✅ |

### MudTooltip, MudSnackbar, MudDrawer, MudTreeView

| MudBlazor | Here | |
|---|---|---|
| `MudTooltip` | `ToolTip.Tip` | ✅ a solid `gray-darker` chip in light, dark and high contrast, 12px at weight 500, 4px 8px; no width of its own, so it wraps only at the screen's edge, or the page's in a browser |
| `Placement`, `Delay` | `ToolTip.Placement`, `ToolTip.ShowDelay` | ≈ |
| `Duration` (251ms) | the same | ✅ the tooltip fades in as every popup does (see MudPopover) |
| `Color` (default `Default`) | a `ToolTip` as the `Tip`, with `primary` … `dark` | ✅ `.mud-theme-{color}`: the colour and its contrast text. `ToolTip.Tip` uses a `ToolTip` as it is, so its classes survive |
| `Arrow` | `arrow` on that `ToolTip` | ✅ a 6px triangle in the chip's colour on the side facing the target, for `Placement` `Top`, `Bottom`, `Left` and `Right`; ≈ none for `Pointer`, Avalonia's default and no MudBlazor placement; the offset from the target is `ToolTip.VerticalOffset`'s |
| `ISnackbar.Add(msg, Severity.X)` | `WindowNotificationManager.Show(…)` with `NotificationType.X` | ✅ a filled alert at elevation 6, 288 to 500px wide, 6px 16px |
| a card's classes | `WindowNotificationManager.Show(content, type, …, classes)` | ≈ the variants below are classes on the card; `Show(INotification)` takes none |
| `Severity.Normal` | `normal` | ✅ filled `dark` with its contrast text; text-primary on a text or outlined card |
| `SnackbarVariant.Filled` (default) | no class | ✅ |
| `SnackbarVariant.Text` / `Outlined` | `text` / `outlined` | ✅ the surface, the text in the colour's darken shade at 400; outlined adds a 1px line in the colour |
| the severity icon | the same | ✅ CommonSnackbarOptions' InfoIcon, SuccessIcon, WarningIcon, ErrorIcon and NormalIcon, 24px at 0.9 opacity, 12px before the message |
| `HideIcon` | `hide-icon` | ✅ |
| CommonSnackbarOptions' severity icons | the icon keys on the `WindowNotificationManager` | ✅ see [the keys](#mudblazor-parity) |
| `Icon`, `IconColor`, `IconSize` on one message | — | ❌ the icon is the severity's: `Show` has no slot for them |
| `ShowCloseIcon` (default true) | the same; `hide-close` for false | ✅ Filled.Close in a small icon button at the card's end, the card's colour on a filled card and action-default otherwise; it closes the card |
| `CloseIcon` | `FiliCloseGeometry` on the `WindowNotificationManager` | ✅ |
| `Action`, `ActionColor`, `ActionVariant` | a control as the notification's content | ≈ |
| `PositionClass` | `WindowNotificationManager.Position` | ✅ every position, 24px from its edges |
| `NewestOnTop` (default false) | no class; `newest-on-top` on the manager | ✅ the newest at the bottom of the stack wherever it is anchored, or at the top |
| `MaxDisplayedSnackbars` | `MaxItems` | ≈ |
| `MaximumOpacity`, `BackgroundBlurred`, the show and hide durations | — | ≈ fully opaque, unblurred, and Avalonia's own enter and exit animations |
| `MudDrawer` | `SplitView` | ✅ 240px open, 56px mini, drawer-background and drawer-text, elevation 1 past the pane's edge |
| `Elevation` (default 1) | no class, or `elevation0` … `elevation25` on the `SplitView` | ✅ none from a drawer closed to nothing, and none in high contrast, where the drawer's edge is its drawer-border line |
| `Anchor` | `PanePlacement` | ≈ |
| `Width`, `MiniWidth`, `Height` | `OpenPaneLength`, `CompactPaneLength`, `Height` | ≈ |
| `Variant` (Temporary, Persistent, Mini, Responsive) | `DisplayMode` (`Overlay`, `Inline`, `CompactInline`, `CompactOverlay`) | ≈; `Responsive`'s breakpoints are the app's |
| `Overlay` | the scrim behind an overlay pane | ✅ `overlay-dark` |
| `ClipMode`, `Fixed` | — | ≈ layout |
| `Color` | `primary` … `dark` on the `SplitView` | ✅ the pane in the colour, its content in the contrast text |
| `OpenMiniOnHover` | — | ❌ behaviour |
| `MudDrawerHeader` | `drawer-header` on a `Border` at the pane's top | ✅ at least the app bar's 64px, 12px 24px round it; `dense` 48px and 8px 24px |
| `MudTreeView` | `TreeView` | ✅ 32px rows, 17px per level; a leaf keeps the arrow's empty 40px slot, so its text lines up with a branch's |
| `Hover` (default false) | `hover` | ✅ action-default-hover on the row under the pointer, not on its branch; a selected row keeps its tint |
| `Disabled` | `IsEnabled="False"` | ✅ action-disabled |
| `Color` (the selection, default `Primary`) | `primary` … `dark` on the `TreeView` | ✅ the colour as the text over its hover tint |
| `Dense` | `dense` | ✅ no 32px minimum, rows padded 1px 4px |
| `SelectionMode.MultiSelection` | `multi-selection` and `SelectionMode="Multiple, Toggle"` | ✅ a 32px checkbox after the arrow (24 dense), checked while the row is selected; no row is marked. ≈ a branch's box is its own selection, never indeterminate from its children |
| `CheckBoxColor` (default `Default`) | no class, or `primary` … `dark` on the tree while `multi-selection` is on | ✅ ≈ as on a list |
| `Icon`, `IconExpanded`, `EndIcon`, `EndText`, `TextTypo` | the `Header`'s content | ≈ |
| `ExpandButtonIcon` | `FiliChevronRightGeometry` in the tree's `Resources` | ✅ |
| `ExpandButtonIconColor`, `LoadingIcon` | — | ❌ the arrow is action-default; nothing loads lazily |
| `Width`, `Height`, `MaxHeight` | the same | ≈ |
| MudTreeView `Ripple` (default true) | on | ✅ on each row, and on its expand arrow |

### MudAlert → `Border.alert`

The message goes in the `Border`; an icon is a `PathIcon` inside it, and takes the colour.

| MudBlazor | Here | |
|---|---|---|
| `Severity.Normal` (default) | `alert` | ✅ text-primary on `dark-hover` |
| `Severity.Info` / `Success` / `Warning` / `Error` | `info` / `success` / `warning` / `error` | ✅ any of the eight colour classes works |
| `Variant.Text` (default) | no class | ✅ `{color}-hover` ground, `{color}-darken` text, the colour on the icon |
| `Variant.Outlined` | `outlined` | ✅ a 1px line in the colour |
| `Variant.Filled` | `filled` | ✅ the colour as ground, its contrast text at Medium weight |
| `Dense` | `dense` | ✅ |
| `.mud-alert-message`, `.mud-alert-icon` | the alert's padding, a `PathIcon` inside | ✅ 9px round the message inside the 6px, so one line is 50px (38 dense); the icon 22px, 12px before the message, on its first line at .9 opacity |
| `ShowCloseIcon`, `CloseIcon`, `ContentAlignment` | — | ≈ put a button in the content; align with ordinary layout |
| `Icon`, `NoIcon` | a `PathIcon` in the content, or none | ≈ there is no default icon per severity |
| `Elevation` | the elevation classes | ✅ any `Border` takes them |
| `Square` | `CornerRadius="0"` | ✅ no radius |

### MudSkeleton → `Border.skeleton`

| MudBlazor | Here | |
|---|---|---|
| `SkeletonType.Text` (default) | `skeleton` | ✅ `skeleton` colour, 20px scaled to 60% as `_skeleton.scss` does |
| `SkeletonType.Circle` / `Rectangle` | `circle` / `rectangle` | ✅ |
| `Animation.Pulse` (default) | the same | ✅ 1.5s ease-in-out, 0.5s delay, opacity 1 → 0.4 → 1 |
| `Animation.False` | `no-animation` | ✅ |
| `Animation.Wave` | `wave` | ✅ 1.6s linear after 0.5s, the 4% band crossing in the first 60%; drawn into the background, since a `Border` has no `::after` layer |
| `Width`, `Height` | `Width`, `Height` | ✅ |

### MudDataGrid → `DataGrid`

Avalonia's own `DataGrid`, from the `Avalonia.Controls.DataGrid` package, themed by type when
the app references it; this package does not depend on it.

| MudBlazor | Here | |
|---|---|---|
| the component | any `DataGrid` | ✅ surface, the default radius, text-primary |
| `Elevation` (default 1) | no class, or the elevation classes | ✅ |
| `Outlined` / `Square` | `outlined` / `CornerRadius="0"` | ✅ outlined drops the elevation, as MudPaper does |
| header row | the column headers | ✅ subtitle2, 16px round, 59px with the rule under it; no rule between headers |
| body row | the rows | ✅ body2, 16px round, a table-lines rule under every row but the last |
| `Dense` | `dense` | ✅ 6px 24px 6px 16px, 16px on the right of the last visible column, a 39px header row |
| `Hover` | `hover` | ✅ table-hover, off by default |
| `Striped` | `striped` | ✅ the 1st, 3rd, 5th… row; hover replaces it |
| `Bordered` | `GridLinesVisibility="All"` | ✅ table-lines between body cells, none after the last, none in the header |
| `SortIcon`, sorting | the column's sort | ✅ ArrowUpward in a small icon button at the end of the header, action-default, hidden until sorted, 0.8 on hover, turned for descending |
| a selected row | `SelectedItem` | ≈ MudDataGrid draws none; here a selected list row's primary-hover, so selection is visible |
| `Groupable`, a group row | a `DataGridCollectionView` with `GroupDescriptions` | ✅ background-gray, "Title: Key" in bold, ExpandMore / ChevronRight, a nested group's header 48px further in per level and its rows not at all |
| edit mode | `IsReadOnly="False"` | ✅ the field loses its resting line and keeps its focused one, as `_datagrid.scss` (with its typo) leaves it |
| `ShowColumnOptions`, `Filterable`, `MudDataGridPager` | — | ❌ no column menu, filter row or pager: each is a control of its own, not a look |
| `ColumnResizeMode` | `CanUserResizeColumns` | ≈ Avalonia's resizing works and shows its cursor; MudBlazor's 2px primary line down the grid does not appear, because the grid keeps its resize state to itself |
| `FixedHeader`, `Virtualize` | built in | ≈ Avalonia's header never scrolls and its rows always virtualize |
| `Height`, `HorizontalScrollbar` | `Height`, `HorizontalScrollBarVisibility` | ≈ |
| `DragDropColumnReordering`, `Hideable` | `CanUserReorderColumns`, a column's `IsVisible` | ≈ |
| `MultiSelection` | `SelectionMode="Extended"` | ≈ no checkbox column |
| a checkbox in a cell (`SelectColumn`, a template column's `MudCheckBox`) | a `CheckBox` in a cell template | ✅ `.mud-table-cell .mud-checkbox`: the 32 / 28 / 44px circle of a dense checkbox, pulled in 4px, so a row with one is 57px rather than 81; a `DataGridCheckBoxColumn`'s box in action-default in every row, as a read-only `MudCheckBox` is, though the column disables all but the current row's |
| `FixedFooter`, footers | — | ❌ Avalonia's DataGrid has no footer row |
| `Loading`, `LoadingProgressColor` | a `ProgressBar` over the grid | ≈ |
| `Breakpoint` (the stacked mobile layout) | — | ❌ re-laying rows as cards is a layout the DataGrid does not have, not a look |

### MudIcon → `PathIcon`

| MudBlazor | Here | |
|---|---|---|
| `Icon` | `Data` | ≈ the gallery carries a few Material glyphs; the theme ships no icon set |
| `Size.Small` / `Medium` (default) / `Large` | `small` / `medium` (or no class) / `large` | ✅ 20 / 24 / 36px; a bare `PathIcon` is 24px, as MudIcon is with no `Size` |
| `Color.Inherit` (default) | no class | ✅ the surrounding text colour |
| `Color.Primary` … `Color.Dark` | `primary` … `dark` | ✅ and the icon's own class beats an alert's or a button's colour, as MudIcon's does |
| `Color.Default` | — | ≈ no class names it: set `Foreground` to `FiliTextSecondaryBrush` |
| `Disabled` | `IsEnabled="False"` | ≈ no look of its own |
| `ViewBox` (default `0 0 24 24`) | the same | ✅ the glyph at its own coordinates in a 24px box, scaled to the icon's size; another box is scaled into 24 by the app |

### MudImage → `Image`

| MudBlazor | Here | |
|---|---|---|
| `Src`, `Width`, `Height` | `Source`, `Width`, `Height` | ≈ |
| `ObjectFit`, `ObjectPosition`, `Fluid` | `Stretch`, alignment, `MaxWidth` | ≈ |
| `Elevation` | a `Border` with an elevation class and the image inside | ≈ an `Image` has no shadow |
| `FallbackSrc` | — | ❌ behaviour |

### MudPopover → `Flyout`, `ContextMenu`

| MudBlazor | Here | |
|---|---|---|
| the paper | `FlyoutPresenter`, `MenuFlyoutPresenter`, `ContextMenu` | ✅ surface, the default radius, elevation 8 |
| padding | none | ✅ the content brings its own, as `<MudPaper Class="pa-4">` does in a popover |
| `Elevation`, `Square`, `Paper="false"`, `DropShadow="false"` | — | ❌ a presenter takes no elevation or shape class |
| `AnchorOrigin`, `TransformOrigin` | `Placement` | ≈ |
| `MaxHeight` | `MaxHeight` | ≈ |
| the opening fade, `Duration` (251ms), `Delay` (0) | the same, on every popup | ✅ opacity 0 to 1 over 251ms at CSS's `ease`, closing at once; a select's list, a menu, an autocomplete's list, a flyout and a tooltip alike. ≈ a dense MudMenu opens without it, and here fades too; ❌ the two are not settable per popup |
| `RelativeWidth` | — | ❌ its width relative to the anchor is the app's to bind |

### MudOverlay → `Border.overlay`

A Border over the content it covers, in a `Panel` with it; the app shows and hides it.

```xml
<Panel>
  <StackPanel>…</StackPanel>
  <!-- <MudOverlay Visible="true" DarkBackground="true" Absolute="true"> -->
  <Border Classes="overlay dark-background" IsVisible="{Binding Loading}">
    <ProgressBar Classes="circular primary" IsIndeterminate="True" />
  </Border>
</Panel>
```

| MudBlazor | Here | |
|---|---|---|
| the component | `overlay` | ✅ transparent, and it still takes the pointer, as a modal MudOverlay does |
| `DarkBackground`, `LightBackground` | `dark-background`, `light-background` | ✅ the palette's `OverlayDark` and `OverlayLight` |
| the content, centred (`align-items` and `justify-content` center) | the Border's child | ✅ centred; an alignment the app sets on it wins |
| `border-radius: inherit` | the same | ✅ the corners of the nearest Border, ContentPresenter or templated control above, past a `Panel`, which has none; a `CornerRadius` the app sets wins |
| the scrim's 0.15s fade-in (`mud-animation-fadein ease 0.15s`) | the same, each time it is shown | ≈ the whole overlay fades, content included, where MudBlazor fades the scrim alone and shows the content at once |
| `Modal="false"` | `IsHitTestVisible="False"` | ≈ an ordinary Avalonia property |
| `Absolute` (over its container) / fixed (over the page) | the overlay in a `Panel` with the container's content, or with the window's | ≈ layout |
| `ZIndex` (5) | `ZIndex` | ≈ an ordinary Avalonia property |
| `DarkBackground` on a temporary drawer's scrim | the light-dismiss layer behind an overlay `SplitView` pane | ✅ `FiliOverlayDarkBrush` |
| `AutoClose`, `OnClick`, `LockScroll` | — | ❌ behaviour: closing on a click and locking scroll are the app's |

### MudNavMenu, MudNavLink, MudNavGroup → `TreeView.nav`

A link is a `TreeViewItem` without children and the active link is the selected one; a group is
one with children:
`<TreeView Classes="nav"><TreeViewItem Header="Home" /><TreeViewItem Header="Settings"> … </TreeViewItem></TreeView>`.

| MudBlazor | Here | |
|---|---|---|
| `MudNavLink` | a row without children | ✅ 8px 16px, body1 on a 28px line, the drawer's text colour (`color: inherit`), the text 12px in and wrapping when too long, as the drawer never scrolls sideways; action-default-hover under the pointer and keyboard focus |
| the active link (`.active`) | the selected row | ✅ weight 500, primary over action-default-hover |
| `MudNavMenu.Color` | `primary` … `dark` | ✅ the active link's text over the colour's hover tint, the colour at 12% under the pointer; its icon, its line and an open group's arrow in the colour |
| `Bordered` | `bordered` | ✅ a 2px line at the active link's end, in its colour |
| `Rounded` | `rounded` | ✅ the default radius on every row |
| `Dense` | `dense` | ✅ 4px 16px |
| `Margin` | `Margin` on the rows, through a `Style` | ≈ |
| `MudNavLink.Icon`, `IconColor` | a `PathIcon` with `start-icon` in the `Header` | ✅ 24px in drawer-icon, 12px before the text, the colour when active; ≈ `IconColor` is its `Foreground` |
| `MudNavLink.Disabled` | `IsEnabled="False"` | ✅ text-disabled |
| `Href`, `Match`, `Target` | `SelectedItem` | ≈ which link is active is the app's to set |
| `MudNavGroup` | a row with children | ✅ opens on a click anywhere on its row; ExpandMore in drawer-icon at its end, turned 180 degrees and in the colour once open; a top-level title at 500, a nested one at 400 until it opens |
| nesting | rows inside a group | ✅ 36px in, then 48, 60 and 72 |
| `MudNavGroup.HideExpandIcon` | `hide-icon` on the group's row | ✅ |
| `MudNavGroup.ExpandIcon` | `FiliExpandMoreGeometry` in the group's `Resources` | ✅ |
| `MudNavGroup.MaxHeight` | — | ❌ a tree item's children have no scroller of their own, so a cap would clip them rather than scroll |
| a mini drawer's nav | a nav tree in a closed `CompactInline` or `CompactOverlay` `SplitView`, or the drawer of a closed compact `DrawerPage` | ✅ a link's text beside its icon hidden, so the 56px rail shows the icons, and every row 8px 16px whatever its depth; no sideways scrolling, a group's arrow running off the edge as in MudBlazor |
| `Ripple` (default true) on MudNavLink and MudNavGroup | on | ✅ on a link's row and a group's header |

### MudToolBar → `Border.toolbar`

| MudBlazor | Here | |
|---|---|---|
| the component | `toolbar` on a `Border` | ✅ 64px, the children centred; ≈ 56px below the sm breakpoint is the app's to set |
| `Gutters` (default true) | `Padding="0"` for false | ✅ 24px a side, or none |
| `Dense` | `dense` | ✅ 48px |
| `WrapContent` | a `WrapPanel` inside and `Height="NaN"` | ≈ |

### MudDatePicker → `CalendarDatePicker`

| MudBlazor | Here | |
|---|---|---|
| the component | `CalendarDatePicker` | ✅ the standard field with the `Event` adornment, and MudPicker's popover |
| `Variant`, `Margin`, `Error`, `HelperText` | the field's classes on the picker | ✅ |
| `Label` | `PlaceholderText` | ✅ the floating label |
| `TitleDateFormat`, `Color` | `ddd, dd MMM`, primary | ✅ |
| `PickerVariant.Static` | a `Calendar` on its own | ✅ |
| the day names, the year list | Avalonia's | ≈ shortest names, a decade grid; see [Known gaps](known-gaps.md) |

### MudTimePicker → `TimePicker`

| MudBlazor | Here | |
|---|---|---|
| the component | `TimePicker` | ✅ the standard field with the `AccessTime` adornment, full width as a field is, and the toolbar's popover |
| `AmPm`, `TimeFormat` | `ClockIdentifier`, `UseSeconds` | ✅ `HH:mm` or `hh:mm tt` |
| `Label` | `AutomationProperties.Name` | ✅ the floating label |
| `Editable` | `editable` | ✅ taken on Enter or when the field loses focus |
| the clock face | Avalonia's hour and minute columns | ≈ see [Known gaps](known-gaps.md) |

### MudToolBar with MudIconButtons → `CommandBar`

| MudBlazor | Here | |
|---|---|---|
| the bar | `CommandBar` | ✅ 64px, 24px gutters; `dense` 48px |
| MudIconButton | `CommandBarButton` | ✅ a 48px circle, the label its tooltip (`DefaultLabelPosition="Collapsed"`, the default here) |
| MudButton with a `StartIcon` | `DefaultLabelPosition="Right"` | ✅ |
| MudToggleIconButton, `ToggledColor` | `CommandBarToggleButton` | ✅ primary when checked |
| MudMenu with `MoreVert` | the overflow | ✅ 48px rows on the popover |
| `Color.Inherit` on an app bar | inside an `appbar` | ✅ |

### MudTable, MudSimpleTable → `TableView`

| MudBlazor | Here | |
|---|---|---|
| the table | `TableView` | ✅ the data grid's `.mud-table` look |
| `Dense`, `Striped`, `Hover`, `Outlined`, `Elevation` | `dense`, `striped`, `hover`, `outlined`, `elevation*` | ✅ |

### MudLayout's app bar and tabs → the page types

| MudBlazor | Here | |
|---|---|---|
| MudMainContent | `ContentPage` | ✅ text-primary on the background |
| MudAppBar with a back MudIconButton and an h6 title | `NavigationPage` | ✅ `HasShadow` is Elevation 4 or 0 |
| MudTabs | `TabbedPage` | ✅ the theme's tabs |

### MudCarousel → `Carousel`, `CarouselPage`

| MudBlazor | Here | |
|---|---|---|
| the arrows, `NavigateBefore` / `NavigateNext` | at the sides, centred, .75 | ✅ |
| the bullets | a `PipsPager` along the bottom | ✅ `Color.Inherit`, the carousel's text colour, the current one told apart by its glyph |
| `ArrowsColor`, `BulletsColor` | `Foreground` on the carousel | ≈ one colour for the arrows and the bullets together |
| `ShowArrows="false"`, `ShowBullets="false"` | `no-arrows`, `no-bullets` | ✅ |
| `ArrowsPosition`, `BulletsPosition`, `AutoCycle` | — | ❌ the defaults, Center and Bottom; cycling is the app's |

### MudSplitPanel → `GridSplitter`

| MudBlazor | Here | |
|---|---|---|
| the divider | `GridSplitter` | ≈ left on the forked template on purpose; see [Known gaps](known-gaps.md) |
| `Horizontal`, `MinPanelSize` | `ResizeDirection`, the columns' or rows' `MinWidth` / `MinHeight` | ≈ |
| `Rounded`, `Elevation`, `PanelGap`, `Padding`, `Transparent` | a `Border.paper` round each panel | ≈ |

### Not here

Each needs a whole new control, is behaviour rather than a look, or is layout an Avalonia panel
already does:

| MudBlazor | Why not |
|---|---|
| MudColorPicker, MudChart, MudRating, MudPagination, MudStepper, MudTimeline, MudBreadcrumbs, MudFileUpload, MudDropZone, MudFabMenu, MudHighlighter, MudPageContentNavigation, MudScrollToTop | a control of its own, with behaviour beyond a look |
| MudDialog, MudMessageBox | a dialog service, not a look |
| MudLayout, MudMainContent, MudContainer, MudGrid, MudItem, MudStack, MudSpacer | layout, which Avalonia's panels do |
| MudHidden, MudBreakpointProvider, MudFocusTrap, MudHotkey, MudForm, MudSwipeArea, MudVirtualize, MudElement, MudRender, MudExitPrompt | not visual |
| MudThemeProvider, MudRTLProvider | the theme itself, and `FlowDirection`; see [Theme variants](#theme-variants) and [Right to left](controls.md#right-to-left) |

## Theme

### Palette → `Fili…Color` / `Fili…Brush`

Every colour token is a `Color` with a paired `Brush`, declared for light, dark and high contrast.
Light and dark are MudBlazor's `Palette.cs` and `PaletteDark.cs`; high contrast is derived.

| `Palette.cs` | Token | |
|---|---|---|
| `Primary` … `Dark` | `FiliPrimaryColor` … `FiliDarkColor` | ✅ |
| `PrimaryContrastText` … `DarkContrastText` | `Fili…ContrastTextColor` | ✅ |
| `PrimaryDarken` … `DarkDarken` | `Fili…DarkenColor` | ✅ MudBlazor's own algorithm, checked on every build by `PaletteDerivationTests` |
| `{color}-hover` (theme provider) | `Fili…HoverColor` | ✅ the colour at `HoverOpacity` |
| `PrimaryLighten` … `DarkLighten` | `Fili…LightenColor` | ✅ the same algorithm; the 14 darken and lighten values of the default palette match mudblazor.com |
| `Black`, `White` | `FiliBlackColor`, `FiliWhiteColor` | ✅ |
| `TextPrimary` / `TextSecondary` / `TextDisabled` | `FiliText…Color` | ✅ |
| `ActionDefault` / `ActionDisabled` / `ActionDisabledBackground` | `FiliAction…Color` | ✅ |
| `action-default-hover` (theme provider) | `FiliActionDefaultHoverColor` | ✅ |
| `Background` / `BackgroundGray` / `Surface` | `FiliBackground…Color`, `FiliSurfaceColor` | ✅ |
| `DrawerBackground` / `DrawerText` / `DrawerIcon` | `FiliDrawer…Color` | ✅ |
| `AppbarBackground` / `AppbarText` | `FiliAppbar…Color` | ✅ |
| `LinesDefault` / `LinesInputs` / `Divider` | `FiliLines…Color`, `FiliDividerColor` | ✅ |
| `TableLines` / `TableStriped` / `TableHover` | `FiliTable…Color` | ✅ |
| `Skeleton` | `FiliSkeletonColor` | ✅ |
| `OverlayDark` / `OverlayLight` | `FiliOverlayDark/LightColor` | ✅ |
| `DividerLight` | `FiliDividerLightColor` | ✅ high contrast keeps a visible line |
| `GrayDefault` … `GrayDarker` | `FiliGrayDefaultColor` … `FiliGrayDarkerColor` | ✅ the same in light and dark, as PaletteDark.cs leaves them; a step lighter in high contrast |
| `HoverOpacity` / `BorderOpacity` | — | ≈ folded into the hover and line tokens rather than exposed |
| `RippleOpacity` / `RippleOpacitySecondary` | `FiliRippleOpacity` / `FiliRippleOpacitySecondary` | ✅ 0.1 and 0.2, a `Double` the same in every variant, as MudThemeProvider writes both from the light palette |

### Typography, shadows, layout

| MudBlazor | Here | |
|---|---|---|
| `Typography.cs` | `Fili{Style}FontSize`, `…LineHeight`, `…LetterSpacing` per style | ✅ line height and letter spacing converted to absolute pixels |
| `Typography.Button.TextTransform` (`uppercase`) | `FiliButtonTextTransform` | ✅ CSS's words: `none`, `uppercase`, `lowercase`, `capitalize`, on a string label |
| `Shadow.cs` | `FiliElevation0` … `FiliElevation25` | ✅ three layers each; a 1px ring in high contrast |
| `DefaultBorderRadius` | `FiliCornerRadius` | ✅ 4px |
| `AppbarHeight`, `DrawerWidthLeft`, `DrawerMiniWidthLeft` | `FiliAppbarHeight`, `FiliDrawerWidth`, `FiliDrawerMiniWidth` | ✅ |
| spacing scale (`pa-4` and friends) | `Padding`, `Margin`, `Spacing`; `FiliSpacing0` … `FiliSpacing20` and `FiliSpacing0Thickness` … `FiliSpacing20Thickness` | ≈ properties, not classes: MudBlazor's 0 to 20 steps of 4px, a number for a `Spacing` and a `Thickness` on every side for `pa-N` and `ma-N`; a one-sided step is written out |
| font | Roboto, embedded | ✅ Light, Regular and Medium as static faces |

### Theme variants

| MudBlazor | Here |
|---|---|
| `IsDarkMode` | `Application.RequestedThemeVariant = ThemeVariant.Dark` |
| — | `FiliThemeVariants.HighContrast`, this package's own third variant |

## Known divergences

- **The `dark` colour barely shows in dark mode, and not at all in high contrast** when used as text
  or a line: `#27272F` on a `#32333D` page, and black on black. MudBlazor's dark theme has the same
  property; a filled dark button is still visible.
