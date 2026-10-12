# MudBlazor parity

What each MudBlazor component parameter, palette property and token becomes in this theme, and
where the two differ. For side-by-side markup, see [Razor and AXAML](razor-vs-axaml.md).

The rule behind every row: **a class only ever names a MudBlazor variant.** Every control the
theme templates is keyed to its type, so an unclassed control is already the MudBlazor default.
Class names are MudBlazor's own words (`primary`, `outlined`, `small`), deliberately not
namespaced. `ClassVocabularyTests` pins the full list of 106.

| Mark | Meaning |
|---|---|
| ✅ | Same values, transcribed from MudBlazor's source |
| ≈ | Same look, reached differently, or close with a stated difference |
| ❌ | Not available |

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
| `Size.Small` / `Medium` / `Large` | `small` / no class / `large` | ✅ per-variant padding, 13 / 14 / 15px |
| `Disabled` | `IsEnabled="False"` | ✅ |
| hover, `:focus-visible`, `:active` | the same state, as in `_button.scss` | ✅ `{color}-hover` for text and outlined, `{color}-darken` for filled |
| elevation 2 → 4 → 8 | the same | ✅ |
| `DropShadow="false"` | `flat` | ✅ elevation 0 at rest, hover, focus and press |
| `FullWidth` | `HorizontalAlignment="Stretch"` | ≈ |
| `StartIcon` / `EndIcon` | an icon in `Content` with `start-icon` / `end-icon` | ✅ 20px (18 small, 22 large), 4px out and 8px from the label (2px out when small) |
| ripple | — | ❌ no Avalonia primitive; the state tint is its static half |
| uppercase label | — | ❌ Avalonia has no text-transform |

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

### MudChip → `Button.chip`, `ToggleButton.chip`

| MudBlazor | Here | |
|---|---|---|
| `Variant.Filled` (default) | `chip` | ✅ 32px pill, `action-disabled-background` grey, `action-disabled` on hover |
| `Variant.Outlined` / `Text` | `chip outlined` / `chip text` | ✅ |
| `Color.Primary` … `Color.Dark` | the colour classes | ✅ filled: colour, `{color}-darken` on hover; text: `{color}-hover` ground, 12% on hover |
| `Size.Small` / `Large` | `small` / `large` | ✅ 24 / 40px high, 12 / 16px text |
| selected (in a `MudChipSet`) | `IsChecked` on a `ToggleButton.chip` | ✅ `MudChip.GetVariant`'s swap: a selected filled chip draws as text, a selected text chip as filled |
| `Disabled` | `IsEnabled="False"` | ✅ |
| `Icon` | an icon in `Content` with `start-icon` | ✅ 20px (18 small, 24 large), 4px out and 4px in (6px on a large chip) |
| close icon | an icon or button in `Content` with `end-icon` | ≈ the close button's 18px and spacing; closing is the app's |
| `Avatar` | put it in `Content` | ≈ no avatar margins applied |
| `MudChipSet` selection rules | — | ❌ single/multi selection is the app's, through `IsChecked` bindings |

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
| ripple, uppercase label | — | ❌ as on every button |

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
| `MudAvatarGroup` | — | ❌ its `Max` and its "+n" avatar count the avatars, which is code |

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
| `Typo.caption` / `overline` | `caption` / `overline` | ✅ |
| `Typo.button` | `FiliButton*` tokens | ≈ no class; a button applies them itself |
| `Color.Primary` … `Color.Dark` | `primary` `secondary` `tertiary` `info` `success` `warning` `error` `dark` | ✅ the palette colour; beats a type style's own colour |
| `mud-text-secondary` (grey) | `Foreground="{DynamicResource FiliTextSecondaryBrush}"` | ≈ a utility class in MudBlazor, not a `Color` |
| `Align`, `GutterBottom`, `Inline` | `TextAlignment`, `Margin` | ≈ ordinary Avalonia properties |

### MudPaper and MudCard → `Border`

| MudBlazor | Here | |
|---|---|---|
| `MudPaper` | `Border.surface` | ≈ surface fill and the default radius, but no shadow: MudPaper's `Elevation` defaults to 1, so it is `surface elevation1` (a `card` carries it) |
| `Elevation="0…24"` | `elevation0` `1` `2` `4` `6` `8` `12` `16` `24` | ✅ the ladder levels apps use; three shadow layers each, from `Shadow.cs` |
| `Outlined` | `outlined` | ✅ 1px `lines-default` |
| `Square` | `square` | ✅ no radius, on a paper or a card |
| `MudCard` | `Border.card` | ✅ a paper at elevation 1 in text-primary; `outlined` drops the shadow whatever elevation class is on it, as MudPaper does |
| `MudCardHeader` | `card-header`, on a `DockPanel` | ✅ 16px round |
| `CardHeaderAvatar` / `CardHeaderContent` / `CardHeaderActions` | `card-header-avatar` / `card-header-content` / `card-header-actions` | ✅ docked at the start 16px from the content, filling the rest (so it goes last), docked at the end pulled 8px up and out |
| `MudCardMedia` | `card-media`, on a `Border` | ✅ 300px high; an `Image` inside covers it; it takes the card's top corners when it comes first |
| `MudCardContent` | `card-content` | ✅ 16px round |
| `MudCardActions` | `card-actions`, on a `StackPanel` | ✅ a row, 8px round, its items centred |
| `ContentPadding="false"` | `Margin="0"` on the part | ≈ the paddings are margins here, since a panel has none |

### MudAppBar → `Border.appbar`

| MudBlazor | Here | |
|---|---|---|
| background, text colour | `appbar` | ✅ text colour is inherited, so a control's own colour wins |
| height | `FiliAppbarHeight` | ✅ 64px |
| elevation | the same | ✅ 4 |
| `Dense` | `dense` | ✅ 48px |
| `Color` | the colour classes | ✅ `.mud-theme-{color}`: the colour as ground, its contrast text inherited |
| `Gutters` (default) | the same | ✅ 24px a side; `Padding="0"` for none |
| `Bottom`, `Fixed` | — | ≈ placement, which is the app's layout |

### MudTextField → `TextBox`

| MudBlazor | Here | |
|---|---|---|
| `Variant.Text` (default) | no class | ✅ |
| `Variant.Filled` / `Outlined` | `filled` / `outlined` | ✅ |
| `Label` | `PlaceholderText` | ≈ **the placeholder is the floating label**, so there is no separate placeholder |
| `Error="true"` | `error` | ✅ |
| validation | a binding's own validation error | ✅ message under the field at 12px, in the helper text's place |
| `HelperText` | `AutomationProperties.HelpText` | ✅ caption, text-secondary; inset 4px under filled, 8px under outlined; read by a screen reader too |
| `HelperTextOnFocus` | `helper-on-focus` | ✅ the help keeps its room and shows while the field has focus |
| `Counter` | `counter` with `MaxLength` | ✅ `12 / 50` on the right; the length alone when `MaxLength` is 0, as `Counter="0"` |
| `Margin.Dense` | `dense` | ✅ standard 3px shorter, filled 8px, outlined 16px, with the label resting and floating to match |
| `Margin.Normal` | `Margin` | ≈ only an outer margin in MudBlazor; an ordinary Avalonia property |
| `Adornment` | `InnerLeftContent` / `InnerRightContent` | ≈ ordinary Avalonia properties |

### MudSelect, MudAutocomplete, MudNumericField

| MudBlazor | Here | |
|---|---|---|
| `MudSelect` | `ComboBox` | ✅ a standard text field with a drop-down adornment, as `_select.scss` makes it |
| `Label` | `PlaceholderText` | ≈ the floating label |
| `Error` | `error` | ✅ |
| `Variant` on a select | `filled` / `outlined` | ✅ the text field's own variants |
| `HelperText`, `Margin.Dense` on a select | `AutomationProperties.HelpText`, `dense` | ✅ as on a text field; a failing binding shows its message too |
| `MudAutocomplete` | `AutoCompleteBox` | ✅ |
| `MudNumericField` | `NumericUpDown` | ✅ 24px spin column, as `_inputcontrol.scss` reserves |

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
| `TickMarkLabels` | — | ❌ |
| `ValueLabel` | `value-label` | ✅ a 12px chip of the colour above the thumb while it is held |
| `ValueLabelFormat`, `Culture` | — | ≈ at most two decimals, in the app's culture rather than MudBlazor's invariant default |

### MudProgressLinear → `ProgressBar`

| MudBlazor | Here | |
|---|---|---|
| `Size.Small` (default) | no class | ✅ a 4px square hairline |
| `Size.Medium` / `Large` | `medium` / `large` | ✅ 8 / 12px |
| `Rounded` | `rounded` | ✅ |
| `Indeterminate` | `IsIndeterminate` | ✅ MudBlazor's two-bar animation |
| `Color` | `primary` … `dark` | ✅ every palette colour, the track at 20% of it |
| `Striped` | `striped` | ✅ 25% white bands at 135°, 40px apart, sliding as MudBlazor's do |
| `Buffer` | — | ❌ it needs a second value `ProgressBar` does not have |

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

### MudLink → `HyperlinkButton`

| MudBlazor | Here | |
|---|---|---|
| `Underline.Hover` (default) | no class | ✅ |
| `Underline.Always` / `None` | `underline` / `no-underline` | ✅ |
| `Color.Primary` (default) | no class | ✅ |
| `Color.Secondary` / `Inherit` | `secondary` / `inherit` | ✅ `inherit` is the surrounding text colour |

### MudTabs → `TabControl`, `TabStrip`

| MudBlazor | Here | |
|---|---|---|
| 48px strip, 2px indicator | the same | ✅ |
| tab bar | the same | ✅ `surface`, with no rule under it until `Border` |
| tab text | the same | ✅ text-primary, primary when active, text-disabled when disabled |
| hover | the same | ✅ `action-default-hover`; `primary-hover` on the active tab |
| `MinimumTabWidth` (160px) | the same | ✅ |
| `Color` | a colour class on the `TabControl` or `TabStrip` | ✅ the bar in the colour; tabs and indicator in its contrast text; `{color}-lighten` on the active tab's hover |
| `Border` / `Outlined` | `border` / `outlined` | ✅ a `lines-default` rule under the bar, or round it |
| `Rounded` / `Centered` | `rounded` / `centered` | ✅ |
| `HideSlider` | `hide-slider` | ✅ |
| `Position` | `TabStripPlacement` on a `TabControl` | ✅ Top, Bottom, Left, Right: the bar docks there, `border` draws its rule on the content side, and the indicator follows |
| `Elevation` | `elevation0` … `elevation24` | ✅ on the bar, outside `rounded`'s clip |
| `SliderColor` | `slider-primary` … `slider-dark` | ✅ the indicator alone, over any bar colour |
| sliding indicator, `SliderAnimation` | the indicator slides | ✅ .3s, cubic-bezier(.64,.09,.08,1), the width jumping ≈ no switch to turn it off |

### MudList → `ListBox`

| MudBlazor | Here | |
|---|---|---|
| selected item | the same | ✅ primary text over primary at 6%, never a filled bar |
| `Dense` | `dense` on the item | ✅ |
| on a surface | `ListBox.surface` | ✅ |

### MudExpansionPanels → `Expander`

| MudBlazor | Here | |
|---|---|---|
| panel on a surface at elevation 1 | the same | ✅ |
| 15px header | the same | ✅ `.9375rem`, sized on its own |
| `Elevation="0"`, sitting on a card | `flat` | ≈ drops both surface and shadow |

### MudToggleGroup, MudButtonGroup, MudMenu

| MudBlazor | Here | |
|---|---|---|
| `MudToggleGroup` item | `ToggleButton` | ✅ |
| `Outlined`, `Size.Small` / `Large` | `outlined`, `small` / `large` | ✅ |
| a two-part `MudButtonGroup` | `SplitButton` | ✅ Button's classes: `outlined` / `filled`, any colour, `small` / `large` |
| group separator | the same | ✅ text-primary or the colour; `divider` when filled; `{color}-lighten` between filled coloured segments |
| `DropShadow="false"` | `filled flat` | ✅ on `SplitButton` and `DropDownButton` alike |
| `Vertical` | — | ≈ not applicable: `SplitButton` is two halves side by side; a vertical group is a `StackPanel` of buttons |
| `MudMenu` with a button activator | `DropDownButton` | ✅ Button's classes; an unclassed one is `Color.Default`, text-primary |
| menu items | `Menu`, `MenuItem` | ✅ strip items and dropdown rows are separate themes |

### MudTooltip, MudSnackbar, MudDrawer, MudTreeView

| MudBlazor | Here | |
|---|---|---|
| `MudTooltip` | `ToolTip.Tip` | ✅ solid `gray-darker` chip in every variant |
| `ISnackbar.Add(msg, Severity.X)` | `WindowNotificationManager.Show(…)` with `NotificationType.X` | ✅ a filled alert at elevation 6 |
| `MudDrawer` | `SplitView` | ✅ 240px open, 56px mini |
| `MudTreeView` | `TreeView` | ✅ 32px rows, 17px per level |

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
| `ShowCloseIcon`, `ContentAlignment` | — | ≈ put a button in the content; align with ordinary layout |

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
| `Outlined` / `Square` | `outlined` / `square` | ✅ outlined drops the elevation, as MudPaper does |
| header row | the column headers | ✅ subtitle2, 16px round, 59px with the rule under it; no rule between headers |
| body row | the rows | ✅ body2, 16px round, a table-lines rule under every row but the last |
| `Dense` | `dense` | ✅ 6px 24px 6px 16px, a 39px header row ≈ the last column keeps 24px on its right, not 16 |
| `Hover` | `hover` | ✅ table-hover, off by default |
| `Striped` | `striped` | ✅ the 1st, 3rd, 5th… row; hover replaces it |
| `Bordered` | `bordered` | ✅ table-lines between body cells, none after the last, none in the header |
| `SortIcon`, sorting | the column's sort | ✅ ArrowUpward in a small icon button at the end of the header, action-default, hidden until sorted, 0.8 on hover, turned for descending |
| a selected row | `SelectedItem` | ≈ MudDataGrid draws none; here a selected list row's primary-hover, so selection is visible |
| `Groupable`, a group row | a `DataGridCollectionView` with `GroupDescriptions` | ✅ background-gray, "Title: Key" in bold, ExpandMore / ChevronRight ≈ a nested group's header is not indented |
| edit mode | `IsReadOnly="False"` | ✅ the field loses its resting line and keeps its focused one, as `_datagrid.scss` (with its typo) leaves it |
| `ShowColumnOptions`, `Filterable`, `MudDataGridPager`, column resize handle | — | ❌ no column menu, filter row, pager or resize indicator |
| `FixedHeader`, `Virtualize` | built in | ≈ Avalonia's header never scrolls and its rows always virtualize |

### Not here

These need a whole new control, or are not visual: MudTable (MudDataGrid's look, above, is the same `.mud-table`), MudDatePicker,
MudTimePicker, MudColorPicker, MudChart, MudRating, MudPagination, MudStepper, MudTimeline,
MudBreadcrumbs, MudCarousel (left on the forked template), MudFileUpload, MudDialog (no dialog
service), MudOverlay, MudHidden, MudFocusTrap, MudHotkey, MudForm, MudGrid,
MudStack.

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
| `RippleOpacity` / `RippleOpacitySecondary` | — | ❌ no ripple |

### Typography, shadows, layout

| MudBlazor | Here | |
|---|---|---|
| `Typography.cs` | `Fili{Style}FontSize`, `…LineHeight`, `…LetterSpacing` per style | ✅ line height and letter spacing converted to absolute pixels |
| `Shadow.cs` | `FiliElevation0` … `FiliElevation24` | ✅ three layers each; a 1px ring in high contrast |
| `DefaultBorderRadius` | `FiliCornerRadius` | ✅ 4px |
| `AppbarHeight`, `DrawerWidthLeft`, `DrawerMiniWidthLeft` | `FiliAppbarHeight`, `FiliDrawerWidth`, `FiliDrawerMiniWidth` | ✅ |
| spacing scale (`pa-4` and friends) | `FiliSpacing1` … `FiliSpacing8` | ≈ values only, no utility classes |
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
- **No ripple and no uppercase button text.** Neither has an Avalonia primitive.
