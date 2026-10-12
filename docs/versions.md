# Versions

What changed in each release of `Fili.Theme.MudAvalonia`, newest first. The package is on
[nuget.org](https://www.nuget.org/packages/Fili.MudAvalonia.Theme) under its original id,
`Fili.MudAvalonia.Theme`; install steps are in the [README](../README.md#install).

| Version | |
|---|---|
| **0.7.0** | MudDataGrid on Avalonia's `DataGrid`, themed by type when the app references `Avalonia.Controls.DataGrid` 12.1.2 or later - the package does not depend on it: `dense`, `striped`, `bordered`, `hover`, `outlined`, `square` and elevation, sorting and group rows. MudFab as `fab` on a `Button`, MudAvatar as `avatar` and MudCard as `card` on a `Border`, MudBadge as `badge` on a `HeaderedContentControl`. Filled buttons, split and drop-down buttons, the expander and snackbars now draw the shadows Avalonia had been clipping. The published class list grows to 106. See [Upgrading from 0.6 to 0.7](#upgrading-from-06-to-07). |
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
