using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// A <see cref="Border"/> inside one of this theme's templates. It is a Border in every way but
/// its type, which is what keeps it out of the theme's own class styles.
/// </summary>
/// <remarks>
/// <para>
/// The classes an app puts on a Border - <c>surface</c>, <c>card</c>, <c>alert</c>, the elevation
/// and colour classes - are styles whose selector ends in <c>Border</c>, and Avalonia matches such a
/// style against every Border in the window: it cannot know a class will never come. Nineteen in
/// twenty of those Borders are parts of a control's template, which never carry an app's class,
/// and the 127 styles cost each of them a watcher - about two thirds of the theme's whole styling
/// cost on a screen of ordinary controls. A selector's type matches that type exactly, not a
/// subclass, so a template part of this type is passed over once per type rather than once per
/// style and instance.
/// </para>
/// <para>
/// So every Border in the hand-written templates is one of these, and every selector reaching one
/// through <c>/template/</c> names it <c>theme|PartBorder</c>. Code that walks a template for a
/// Border still finds it. Internal: the template's structure is not part of the published
/// interface (docs/compatibility.md).
/// </para>
/// </remarks>
internal sealed class PartBorder : Border
{
}
