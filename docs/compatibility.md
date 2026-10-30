# Compatibility

What an app can rely on from one version of `Fili.MudAvalonia.Theme` to the next. From 1.0 the
package follows semantic versioning over the interface below; before 1.0, a minor version could
break it, and each one that did has an "Upgrading" section in [Versions](versions/README.md).

A theme's interface is mostly not C#. It is the names an app writes in its own XAML: the two
include paths, the classes, the resource keys. Those are what this page promises, and each is
pinned by a test, so a change that breaks one fails the build rather than an app.

## Stable within a major version

| What | Where it is listed | Pinned by |
|---|---|---|
| The NuGet id `Fili.MudAvalonia.Theme` | [README](../README.md#install) | - |
| The two includes, `avares://Fili.Theme.MudAvalonia/Themes/Base/FiliBaseTheme.axaml` and `avares://Fili.Theme.MudAvalonia/FiliTheme.axaml`, in that order | [README](../README.md#quick-start) | the gallery and every test session load them |
| The style classes, and the control each applies to | [README](../README.md#classes-are-variants) and the docs site's [component pages](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/#parity) | `ClassVocabularyTests` |
| The resource keys: every palette, elevation, geometry and typography token, and every glyph key, with what each promises ([below](#what-a-resource-key-promises)) | [Published resource keys](published-keys.md) | `PublishedInterfaceTests`, `VariantScopeTests` |
| The public C# API: `FiliThemeVariants` and its `HighContrast` variant, whose key is the string `"HighContrast"` | [README](../README.md) | `PublishedInterfaceTests` |
| Themed by type: a control the theme styles is styled with no class, and an optional package's controls with nothing to opt into | [Installation](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/#installation) and [The base](the-base.md) | `StandaloneReadinessTests`, `OptionalBoundaryTests` |
| No dependency but `Avalonia`, from 12.1.3 up to but not including 13.0.0 | [Releasing](releasing.md#supported-avalonia-versions) | `PackageContentTests` |

So a 1.x release does not remove or rename a class or a key, does not change what a class means,
and does not add a dependency. A key that has to go is kept as an alias of its replacement
until the next major version.

The `"HighContrast"` string is stable because an app may save a variant's key in its settings and
read it back. Avalonia has no built-in variant of that name; if a future one adds it, that is
Avalonia 13, a major version here too.

### What a resource key promises

- **Its name and its type.** A colour stays a `Color`, a brush a `SolidColorBrush`, a size a
  `Double` or a `Thickness`, a glyph a `Geometry`.
- **Where an app may set it.** A `*Color` key is set in the application's `Resources` (per
  variant, in its `ThemeDictionaries`) and changes every control; in a control's `Resources` it
  does nothing, because each brush is one application-level object built from it. A `*Brush`
  key or a glyph key may also be set in a control's `Resources`, for that control and what is
  inside it.
- **The variant is the application's.** Set `RequestedThemeVariant` on the `Application`. A
  window's or a `ThemeVariantScope`'s own variant reaches what looks a per-variant key up inside
  it but not the brushes, so half of what is inside follows it. Per-scope brushes would be a
  major version.

## May change in a minor version

- **Additions.** A new class, key or optional package. A new class is not free - class names are
  MudBlazor's ordinary words, and one an app already uses picks up the theme's setters - so each
  is named in [Versions](versions/README.md). A class style matches its control's style key,
  which is its exact type: an app's own `Button` subclass is not restyled by a new `Button`
  class unless it declares `Button` as its `StyleKeyOverride`.
- **A look moving closer to MudBlazor.** A value transcribed wrongly is corrected, and a pixel
  baseline moves with it. A change an app would notice on screen gets a line in Versions.
- **Accessibility and high-contrast corrections.** A colour, contrast or focus cue that falls
  short of WCAG AA, or high contrast's own values, which have no MudBlazor original to move
  closer to, may change a look. Each is named in Versions.
- **What `FiliReducedMotion` covers.** It stops the animations and transitions the theme starts
  itself; one it does not reach yet ([Published resource keys](published-keys.md) lists them) may
  join it in a minor version.
- **The MudBlazor version tracked.** The theme transcribes MudBlazor 9.11; following a newer one
  is a minor version, with the visible differences listed.
- **The Avalonia minimum, within Avalonia 12.** `Themes/Base/` is a fork of Avalonia Simple at
  the declared minimum, and the two move together ([Releasing](releasing.md#supported-avalonia-versions)).
  Moving to Avalonia 13 is a major version, and the dependency stops below it. An optional
  package's gate does the same: the DataGrid is themed from 12.1.0 up to but not including 13.0.0,
  and skipped with a warning outside that.

## Not covered

These are implementation, and may change in any version:

- **Template structure**: the parts inside a template, their names (`PART_*`, `PART_StateLayer`),
  their order and nesting. A `/template/` selector in an app's own styles reaches into it at its
  own risk. An older version's Upgrading section that names a part described that version.
- **The pseudo-classes the theme sets** on controls for its own templates: `:joined`,
  `:joined-next`, `:joined-previous`, `:joined-vertical`, `:first-panel`, `:last-panel`,
  `:next-expanded`, `:previous-expanded`, `:start-adornment`, `:end-adornment`,
  `:start-text-adornment`, `:end-text-adornment`, `:no-label`, `:helper-text`, `:select-input`,
  `:spin-start`, `:spin-end` and `:icon-gutter`. Avalonia's own (`:pointerover`, `:checked`...)
  are Avalonia's.
- **Every `Fili*` key not on [Published resource keys](published-keys.md)**: the keyed control
  themes (`FiliFilledButton`, `FiliOutlinedTextBox`...), which the theme selects itself from the
  classes, so set the class, not `Theme`; the data templates (`FiliTextFieldHelperLine`...); the
  converters (`FiliRailFactor`, `FiliCircularFactor`...); and `FiliTreeViewItemIndent`.
  `PublishedInterfaceTests` holds every key the theme declares to one side or the other.
- **The forked base's keys** (`ThemeAccentBrush`, `ThemeControlMidBrush`...), Avalonia Simple's
  names for what the base paints from. Use the `Fili*` tokens instead.
- **Internal types**: the converters, `OptionalStyleInclude`, `SlidingIndicator`, `FieldClasses`,
  `MenuIconGutter`, `TabBarScroll`, `SpinColumn`, `CollapsePanel`, `ElevationOverlay`, `PartBorder`, `IconSlot`, `ChildAlignment`, `Accent`, `MultiSelection`, `Adornments`, `AutoCompleteField`, `OverlayScrim`, `ButtonGroup`, `TooltipPlacement`,
  `Ripple`, `TextTransform`, `ExpansionPanels`, `HighContrastRings`, `CalendarView`, `TimeView`,
  `InheritedColour`, `Motion` and the DataGrid helpers. A template's Borders are `PartBorder`s,
  so a `/template/ Border` selector of the app's own finds none of them.
- **Exact pixels.** Rendering follows Avalonia and Skia, and anti-aliased edges can differ by a
  level between machines; the pixel tests allow for that ([Testing](testing.md)).
