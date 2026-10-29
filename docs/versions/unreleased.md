# Unreleased

Work since 0.25.0, kept here as it lands; at the bump this file is renamed to its version.

[All versions](README.md)

Lighter templates, with no change to how anything looks: a list row draws its padding on its
content presenter rather than on a panel round it, and a checkbox or radio scales its glyph for
`small` and `large` with a render transform rather than a `LayoutTransformControl`, its box being
sized on its own. One visual less on each; building 200 of them takes about 15% less time for list
rows and 9% less for checkboxes and radios. A template part named `PART_Scale` is now a `Panel`,
and a list row's template has no `DockPanel` unless it is a `multi-selection` row.
