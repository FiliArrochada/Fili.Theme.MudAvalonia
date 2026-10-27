using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// The text field's classes, passed from a control that wraps one to the <c>TextBox</c> inside it:
/// MudAutocomplete and MudNumericField are MudInputs, and take its Variant, Margin, Error,
/// Counter and HelperTextOnFocus as MudTextField does.
/// </summary>
/// <remarks>
/// <para>
/// An <c>AutoCompleteBox</c> or <c>NumericUpDown</c> draws its field with a <c>TextBox</c> in its
/// template, which wears Controls/TextBox.axaml; but a class on the outer control never reaches
/// it, and those options are classes the TextBox's own theme reads (<c>^.dense</c>,
/// <c>^.error</c>, ...), deep inside its template. A style cannot add a class, and a selector
/// cannot reach two templates down, so the inner TextBox mirrors them instead: set on it by its
/// template, this copies each of <see cref="Mirrored"/> from its templated parent, and keeps
/// copying as they change, since an app toggles <c>error</c> as validation runs.
/// </para>
/// <para>
/// Internal, like the converters: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class FieldClasses
{
    /// <summary>Every class the text field's theme reads; the variants pick its ControlTheme.</summary>
    internal static readonly string[] Mirrored =
        ["filled", "outlined", "dense", "error", "counter", "clearable", "helper-on-focus", "no-underline", "placeholder", "required", "shrink-label"];

    private static readonly ConditionalWeakTable<TextBox, StyledElement> Sources = new();

    private FieldClasses()
    {
    }

    public static readonly AttachedProperty<bool> MirrorsProperty =
        AvaloniaProperty.RegisterAttached<FieldClasses, TextBox, bool>("Mirrors");

    public static bool GetMirrors(TextBox box) => box.GetValue(MirrorsProperty);

    public static void SetMirrors(TextBox box, bool value) => box.SetValue(MirrorsProperty, value);

    static FieldClasses()
    {
        MirrorsProperty.Changed.AddClassHandler<TextBox>((box, e) =>
        {
            box.AttachedToLogicalTree -= OnAttached;
            box.DetachedFromLogicalTree -= OnDetached;
            Release(box);

            if (e.GetNewValue<bool>())
            {
                box.AttachedToLogicalTree += OnAttached;
                box.DetachedFromLogicalTree += OnDetached;
                if (((Avalonia.LogicalTree.ILogical)box).IsAttachedToLogicalTree)
                {
                    Follow(box);
                }
            }
        });
    }

    private static void OnAttached(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) => Follow((TextBox)sender!);

    private static void OnDetached(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) => Release((TextBox)sender!);

    private static void Follow(TextBox box)
    {
        Release(box);
        if (box.TemplatedParent is not StyledElement source)
        {
            return;
        }

        Sources.Add(box, source);
        source.Classes.CollectionChanged += Handler(box);
        Copy(source, box);
    }

    private static void Release(TextBox box)
    {
        if (Sources.TryGetValue(box, out var source))
        {
            source.Classes.CollectionChanged -= Handler(box);
            Sources.Remove(box);
        }
    }

    // One handler per box, so it can be removed again.
    private static readonly ConditionalWeakTable<TextBox, NotifyCollectionChangedEventHandler> Handlers = new();

    private static NotifyCollectionChangedEventHandler Handler(TextBox box) =>
        Handlers.GetValue(box, b => (_, _) =>
        {
            if (Sources.TryGetValue(b, out var source))
            {
                Copy(source, b);
            }
        });

    private static void Copy(StyledElement source, TextBox box)
    {
        foreach (var name in Mirrored)
        {
            box.Classes.Set(name, source.Classes.Contains(name));
        }
    }
}
