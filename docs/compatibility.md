# Compatibility

What an app can rely on from one version of `Fili.MudAvalonia.Theme` to the next. From 1.0 the
package follows semantic versioning over the interface below; before 1.0, a minor version could
break it, and each one that did has an "Upgrading" section in [Versions](versions.md).

A theme's interface is mostly not C#. It is the names an app writes in its own XAML: the two
include paths, the classes, the resource keys. Those are what this page promises, and each is
pinned by a test, so a change that breaks one fails the build rather than an app.

## Stable within a major version

| What | Where it is listed | Pinned by |
|---|---|---|
| The NuGet id `Fili.MudAvalonia.Theme` | [README](../README.md#install) | - |
| The two includes, `avares://Fili.Theme.MudAvalonia/Themes/Base/FiliBaseTheme.axaml` and `avares://Fili.Theme.MudAvalonia/FiliTheme.axaml`, in that order | [README](../README.md#using-it) | the gallery and every test session load them |
| The style classes, and the control each applies to | [README](../README.md#every-control) and [MudBlazor parity](mudblazor-parity.md) | `ClassVocabularyTests` |
| The resource keys: every palette, elevation, geometry and typography token, and every glyph key | [Published resource keys](published-keys.md) | `PublishedInterfaceTests` |
| The public C# API: `FiliThemeVariants` and its `HighContrast` variant | [README](../README.md) | `PublishedInterfaceTests` |
| Themed by type: a control the theme styles is styled with no class, and an optional package's controls with nothing to opt into | [Controls](controls.md) | `StandaloneReadinessTests`, `OptionalBoundaryTests` |
| No dependency but `Avalonia` | [Releasing](releasing.md#supported-avalonia-versions) | - |

So a 1.x release does not remove or rename a class or a key, does not change what a class means,
and does not add a dependency. A key that has to go is kept as an alias of its replacement
until the next major version.

## May change in a minor version

- **Additions.** A new class, key or optional package. A new class is not free - class names are
  MudBlazor's ordinary words, and one an app already uses picks up the theme's setters - so each
  is named in [Versions](versions.md).
- **A look moving closer to MudBlazor.** A value transcribed wrongly is corrected, and a pixel
  baseline moves with it. A change an app would notice on screen gets a line in Versions.
- **The MudBlazor version tracked.** The theme transcribes MudBlazor 9.11; following a newer one
  is a minor version, with the visible differences listed.
- **The Avalonia minimum, within Avalonia 12.** `Themes/Base/` is a fork of Avalonia Simple at
  the declared minimum, and the two move together ([Releasing](releasing.md#supported-avalonia-versions)).
  Moving to Avalonia 13 is a major version.

## Not covered

These are implementation, and may change in any version:

- **Template structure**: the parts inside a template, their names (`PART_*`, `PART_StateLayer`),
  their order and nesting. A `/template/` selector in an app's own styles reaches into it at its
  own risk.
- **The keyed control themes** (`FiliFilledButton`, `FiliOutlinedTextBox`...): the theme selects
  them itself from the classes. Set the class, not `Theme`.
- **The forked base's keys** (`ThemeAccentBrush`, `ThemeControlMidBrush`...), Avalonia Simple's
  names for what the base paints from. Use the `Fili*` tokens instead.
- **Internal types**: the converters, `OptionalStyleInclude`, `SlidingIndicator`, `FieldClasses`,
  `MenuIconGutter`, `TabBarScroll` and the DataGrid helpers.
- **Exact pixels.** Rendering follows Avalonia and Skia, and anti-aliased edges can differ by a
  level between machines; the pixel tests allow for that ([Testing](testing.md)).
