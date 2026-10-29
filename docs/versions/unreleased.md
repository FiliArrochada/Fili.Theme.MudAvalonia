# Unreleased

Work since 0.25.0, kept here as it lands; at the bump this file is renamed to its version.

[All versions](README.md)

Lighter templates, with no change to how anything looks: a list row draws its padding on its
content presenter rather than on a panel round it, and a checkbox or radio scales its glyph for
`small` and `large` with a render transform rather than a `LayoutTransformControl`, its box being
sized on its own. One visual less on each; building 200 of them takes about 15% less time for list
rows and 9% less for checkboxes and radios. A template part named `PART_Scale` is now a `Panel`,
and a list row's template has no `DockPanel` unless it is a `multi-selection` row.

A checkbox draws its unchecked, checked and indeterminate glyphs with one Path, which swaps its
glyph and colour with the state, instead of three with two of them hidden; the size classes scale
that Path itself. Three visuals less on each checkbox: building 200 takes about a quarter less
time and allocates a quarter less memory. The template parts `PART_Unchecked`, `PART_Checked`,
`PART_Indeterminate` and `PART_Scale` are one `PART_Glyph` on a checkbox; a `/template/` selector
of an app's that named them names it instead. A radio keeps its two glyphs.

An indeterminate progress bar no longer re-lays out the window as it moves. Its two bars grow and
shrink as MudBlazor's do, and a bar longer than the rail made the rail, the progress bar and every
control round it measure again on each frame; now only the bar does. In a desktop window, 60
indeterminate bars take about a quarter less CPU. They look and move exactly as before.

And the docs site shows its examples again: the browser build is trimmed, and trimming dropped the
compiled XAML every example is loaded from by its path, so each one failed with "No precompiled
XAML found".
