# The base

Why the theme ships its own fork of Avalonia's Simple templates instead of layering over Fluent or
Simple, and what that fork has to be kept honest about. The procedure for re-syncing it to a new
Avalonia version is in
[`FORK.md`](../src/Fili.Theme.MudAvalonia/Themes/Base/FORK.md).

## The base

**Avalonia ships no implicit default theme.** A `Button` with no `ControlTheme` in scope has no
template, so it measures to nothing and renders nothing — not an unstyled button, *no* button. Some
theme has to supply a template for every control type used.

This package supplies its own. `Themes/Base/` holds **79 templates forked verbatim from Avalonia's
Simple theme at 12.1.3**, rebased onto this package and repaletted by a single hand-written file,
`Themes/Base/Accents.axaml`, which redefines the ~96 resource keys those templates paint from in
terms of Fili tokens. One file repalettes all 79.

On top of that sit **hand-written Material control themes for 39 of the 89 templated control
types**, which win over their forked counterparts because `FiliTheme.axaml` is included second:

| | |
|---|---|
| Buttons | `Button`, `ToggleButton`, `RepeatButton`, `HyperlinkButton`, `SplitButton`, `DropDownButton` |
| Fields | `TextBox`, `ComboBox`, `ComboBoxItem`, `NumericUpDown`, `ButtonSpinner`, `AutoCompleteBox`, `Label`, `DataValidationErrors` |
| Selection | `CheckBox`, `RadioButton`, `ToggleSwitch` |
| Collections | `ListBox`, `ListBoxItem`, `TreeView`, `TreeViewItem`, `TabControl`, `TabItem`, `TabStrip`, `TabStripItem`, `Expander` |
| Indicators | `Slider`, `ProgressBar`, `PipsPager`, `Separator`, `ScrollBar` |
| Menus and overlays | `Menu`, `MenuItem`, `ContextMenu`, `ToolTip`, `FlyoutPresenter`, `MenuFlyoutPresenter` |
| Notifications | `NotificationCard`, `WindowNotificationManager` |

`StandaloneReadinessTests` pins that list — and the count — so neither can drift without a failing
build, and so an Avalonia version that adds control types shows up as a failure rather than as a
gap nobody noticed.

The other 50 keep the forked templates. Most of them are Avalonia plumbing MudBlazor has no
counterpart for — `PopupRoot`, `AdornerLayer`, `TextSelectionHandle`, `ManagedFileChooser`,
`CommandBar`, the `*Page` shell types — and keeping those byte-faithful is what makes an Avalonia
upgrade a re-download and a diff.

So the distinction is no longer themed-versus-invisible, it is **hand-written Material versus
forked-and-repaletted**. Nothing is missing, and nothing external is required.

`Themes/Base/FORK.md` records provenance, the two mechanical edits applied, and the upgrade
procedure. See also *How the fork earns its keep* below.

<details>
<summary>Historical: when this package layered over a substrate</summary>

This package used to be tokens plus control themes for **17** control types, with the other **72**
templated types (`ComboBox`, `ListBox`, `TreeView`, `DataGrid`, `DatePicker`, `Flyout`, `Window`
chrome, …) coming from `FluentTheme`. Everything below describes that arrangement and is kept for
the reasoning, not as instructions — the package has been standalone since the Simple fork.

Precisely what "invisible" means, because it is narrower than it sounds: **only *templated*
controls need a theme.** `TextBlock`, `Border`, `Image`, `Panel`, `Canvas` and the shapes draw
themselves and are unaffected. So a theme-less app is not a blank window — it is text and
rectangles laid out correctly with every interactive control missing, which is a worse failure
than blank because it looks half-working.

**The gallery let you switch substrate at runtime**, Fluent or Simple, from the app bar. The
themed controls did not change at all while it flipped, because they carry full templates;
everything else did. That difference was the honest measure of how much of the look was still
borrowed — and it was how the choice of substrate was settled, rather than by arguing it. (The
selector was removed once the base became a fork; see [Gallery](../README.md#gallery).)

**What Fluent is actually worth**, beyond "templates exist":

- **It is complete, first-party and versioned in lockstep with Avalonia.** It is regression-tested
  by the Avalonia team on every platform Avalonia targets. No community theme can promise that.
- **It covers the work nobody wants.** Not just "controls" — `DataGrid` (thousands of lines),
  `DatePicker`'s calendar flyout, `ColorPicker`, `NumericUpDown`'s spinner, IME/composition
  rendering inside `TextBox`, popup placement and flipping, window decorations. These are
  functional templates, not styling.
- **It carries accessibility and platform behaviour.** Focus adorners, keyboard affordances,
  hit-target sizing, high-contrast handling. Every `ControlTheme` written here re-earns all of
  that by hand, and it is easy to ship a beautiful theme with no visible focus ring.
- **It absorbs Avalonia's churn.** When a template part is renamed or a pseudo-class added in
  Avalonia 13, Fluent is updated with it. Every ControlTheme owned here is one that must be
  updated by hand — which is the argument that cuts hardest against going standalone in a small
  project.
- **It fails safe.** Forget a control while layered and it looks Fluent; forget one while
  standalone and it disappears.
- **One accent colour retints everything it templates** — a checked `CheckBox`, a `Slider` thumb,
  the selected `TabItem` underline — without this package knowing those controls exist.

The cost is equally real: shapes you do not control, a `:not()` guard on every blanket style, and
internal brush keys that move between versions. And the discomfort is not constant — **layering is
most comfortable at 0% coverage and at 100%, and worst in the middle**, because every control that
becomes properly Material increases the contrast with the ones that have not. At 13 of 89, that is
exactly where this repo sits.

That last point is the highest-leverage lever in the repo: one accent colour retints every
substrate-templated control at once — a checked `CheckBox`, a `Slider` thumb, the selected
`TabItem` underline — without this package knowing those controls exist.

**Set it through `FluentTheme.Palettes`, not through a `SystemAccentColor` resource.** Fluent
derives its accent ramp from its own `Palettes` collection, seeded from platform settings, and
never consults a resource of that name:

```csharp
var theme = new FluentTheme();
theme.Palettes[ThemeVariant.Light] = new ColorPaletteResources { Accent = Color.Parse("#594AE2") };
theme.Palettes[ThemeVariant.Dark]  = new ColorPaletteResources { Accent = Color.Parse("#776BE7") };
```

Doing it the other way fails **silently** — every unthemed control just stays the OS accent blue.
That is exactly what the first rendered capture of this gallery showed, after the wrong approach
had been confidently written down here. A test pinned it for as long as the gallery used Fluent.

`SimpleTheme` has no equivalent and ignores `Palettes` entirely; retinting it means overriding its
own `ThemeAccentBrush` family. That is not done, so the gallery's Simple substrate still renders
the OS blue — deliberately left visible rather than papered over.

**Order is load-bearing.** `FluentTheme` first, then `FiliTheme`. Styles are evaluated in order and
later ones win for overlapping setters, so reversing them leaves Fluent's colours on top.

What this arrangement costs: the app reads as **Fluent shapes wearing Material colours**, except
for the two controls with real templates. That seam is visible — Fluent separates surfaces with a
1px border where Material floats them on a shadow, and its controls are geometrically tighter. Each
`ControlTheme` added shrinks Fluent's role, and Fluent only disappears entirely if every control
gets one, which is the multi-year project this repo exists to avoid.

**Fluent versus Simple was settled by looking**, with `--capture`, and Fluent won clearly:

| | Fluent | Simple |
|---|---|---|
| Accent lever | `Palettes` / `ColorPaletteResources` — one colour retints everything | none; needs its own `ThemeAccentBrush` overrides |
| Metrics next to the themed controls | close in weight and spacing; mild seam | visibly tighter and smaller, so the seam is *worse* |
| Tab strip | a coloured underline — near-identical to Material's indicator | a filled box behind the active tab |
| Scrollbar | thin, unobtrusive | thick, with arrow buttons |
| ComboBox / NumericUpDown | clean chevrons | small triangles, cramped spinner |

The theory for Simple was that its plainness would be *neutral*, so retinted plain controls would
read as minimal Material while Fluent's leftovers would read as Windows 11 in purple. **Rendering
both killed that theory.** Simple does not read as neutral, it reads as Win32 circa 2003 — the tab
strip and scrollbar give it away instantly — and because its metrics are tighter than Material's,
the boundary between themed and unthemed controls is *more* obvious, not less.

Fluent also turned out to have an accidental advantage: its `TabItem` indicator is a coloured
underline, which is very close to what Material does anyway.

The evidence came from `--capture`, which rendered both substrates at the time. It renders the
shipping base only now.

`Material.Avalonia` would be a visually closer substrate, but consuming it means binding to *its*
slot names (`PrimaryHueMidBrush`, `MaterialCardBackgroundBrush`) — the coupling that ties an
app's Avalonia upgrades to Material.Avalonia shipping a matching build.

</details>

## How the fork earns its keep

Going standalone was expected to mean writing 72 templates. It did not. It meant **downloading 79
and rewriting one file.**

The whole adaptation was:

1. Fetch `src/Avalonia.Themes.Simple/Controls/*.xaml` at tag 12.1.3.
2. `.xaml` → `.axaml`, and rebase `avares://Avalonia.Themes.Simple/...` onto this package. Both by
   script; no template was touched by hand.
3. Write `Accents.axaml`, mapping the ~96 keys those templates paint from onto Fili tokens.

Step 3 is the whole trick, and it is why **Simple** rather than Fluent: its key surface is small,
flat and stable. Fluent's equivalent is hundreds of layered keys that move between versions — the
same churn that made retinting it a documented caveat earlier in this file.

What this bought, beyond the palette: every `:not()` guard in this repo is now a transitional
artifact rather than a permanent tax, the accent needs no theme-specific API, and there is no
external theme package to track across Avalonia releases.

What it costs: the forked templates are this repo's problem now. `FORK.md` documents the upgrade
as re-download, re-apply the two edits, diff. `SimpleBridgeTests` asserts every contract key still
resolves in both variants, which is what turns "Avalonia 13 added a key" from a silent rendering
bug into a failing build.

**One bug found exactly that way, immediately.** The first standalone render looked correct until
you noticed the substrate-default `ToggleSwitch`es were labels with no switch. Eighteen `*Color`
keys had been missed while their `*Brush` twins were mapped — and nothing reported it. That is the
same silent-key failure this repo has now hit four separate times, and the reason the contract test
exists rather than a comment saying to be careful.

## Two different "use Simple" arguments — do not conflate them

Community guidance for writing a full Avalonia theme is to **copy the Simple theme's `.axaml`
files out of the Avalonia repo and edit them**, because there is less to strip than in Fluent.
That advice is real and good — and it is about a *different decision* from the Fluent-vs-Simple
comparison above.

| | Fork Simple's files | Reference Simple as substrate |
|---|---|---|
| What it is | copy ~89 templates into this repo and restyle them | `<SimpleTheme />` in `Application.Styles` |
| Runtime dependency | **none** — fully standalone | Simple, at runtime |
| The comparison above applies? | no | yes, and Fluent measured better |

The second is what the gallery switch compares, and Fluent won it. The first dissolves the
question entirely: fork the files and you depend on no substrate at all.

It also makes standalone **much cheaper than "write 89 templates"**, which is how the sequencing
below was originally framed. Forking Simple's templates and restyling them reuses all the part
names, states and keyboard handling — the tested structure — and changes only the visuals. That
is a far smaller job than Semi's several hundred files from scratch, and it is the route to take
if this repo ever goes standalone.

Note what the official docs do *not* say: they describe Simple as "a minimal and lightweight theme
with limited built-in styling" whose "low visual and structural complexity makes it a good choice
for applications running on embedded devices", and designate **neither** theme as the recommended
base. The fork-Simple advice is community practice, not documentation.

## Flowery.NET layers over Fluent too

Worth knowing, since it is the closest project to this one: [Flowery.NET](https://github.com/tobitege/Flowery.NET)
— 95 controls, a DaisyUI port — has exactly this shape in its gallery's `App.axaml`:

```xml
<Application.Styles>
    <FluentTheme />
    <daisy:DaisyUITheme />
</Application.Styles>
```

Its `DaisyUITheme.axaml` merges themes only for its own `Daisy*` types. Nothing for Avalonia's
built-ins — those stay Fluent's.

So there are **three** architectures, not two:

1. **Replace the substrate** — Semi.Avalonia, Material.Avalonia, Classic.Avalonia. Complete
   `ControlTheme` sets, no Fluent.
2. **Add new control types over a substrate** — Flowery.NET. New `Daisy*` controls with their own
   templates; Fluent still renders every built-in.
3. **Replace the substrate by forking one** — this repo, now. Avalonia's Simple templates taken
   wholesale and repaletted, with Material themes written over the controls that need them.

The second is the cheapest, and explains how one person shipped 95 controls in nine months: a
brand-new control type has no existing template to match, no states to preserve and nothing to
stay compatible with. Retemplating `TextBox` is harder than inventing `DaisyInput`.

The third — forking — turned out far cheaper than the first. Semi wrote several hundred files;
this repo downloaded 79 and wrote one. The difference is whether you author templates or inherit
them and change only what they paint from.

`Material.Avalonia` remains the closest *visual* match, but consuming it would mean binding to
*its* slot names (`PrimaryHueMidBrush`, `MaterialCardBackgroundBrush`) — the coupling that ties
an app's Avalonia upgrades to Material.Avalonia shipping a matching build. Forking Simple avoids
taking on anyone else's vocabulary.

## In the browser

The same gallery runs in a browser, from `src/Fili.Theme.MudAvalonia.Gallery.Browser`, and
`.github/workflows/pages.yml` publishes it to
[GitHub Pages](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/) on every push to `master`.
Both heads share one `MainView`; the desktop's `MainWindow` only hosts it.

```powershell
dotnet workload install wasm-tools     # once: Avalonia.Browser links Skia and HarfBuzz natively
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Browser
```

Building the solution needs the workload too, because the browser head is part of it.

Every URL the page loads is relative, so the published output works unchanged under the Pages
sub-path and at the root of a local server. The first time, Pages has to be switched to
**Settings → Pages → Source: GitHub Actions**; until then the deploy job fails.

The theme is loaded only by the two `StyleInclude`s in App.axaml, which compile to code. Keep it
that way: a `StyleInclude` constructed in C# loads its source by reflection at runtime, which the
browser build's trimming can strip, and the page would then render with no base theme at all.
