using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using System.Linq;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// MudNumericField's spin column inside its input box, rather than beside it. Set by the
/// NumericUpDown's template on the TextBox inside it.
/// </summary>
/// <remarks>
/// <para>
/// _inputcontrol.scss puts <c>.mud-input-numeric-spin</c> inside the input,
/// <c>position: absolute; right: 0; top: 0; bottom: 0</c>, and gives the input
/// <c>padding-right: 24px</c> to clear it: the box's underline, its filled ground or its outline
/// runs on under the arrows. Here the arrows are parts of the ButtonSpinner's template and the
/// box is the TextBox's, so the box cannot hold them. Instead the NumericUpDown's theme lets the
/// TextBox run 24px under the column, which is drawn over it, and this does the two things a
/// style cannot reach:
/// </para>
/// <list type="bullet">
/// <item>it marks the TextBox <c>:spin-end</c> or <c>:spin-start</c> while the column shows, so the
/// TextBox's own theme keeps the text 24px clear of it - two templates down from the
/// NumericUpDown, where its selectors cannot go;</item>
/// <item>it sizes the column to the TextBox's input box, top to bottom, and no further: the helper
/// line below the box runs the field's full width, its counter at the far end.</item>
/// </list>
/// <para>
/// Internal, like the other helpers: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class SpinColumn
{
    private const string SpinEnd = ":spin-end";
    private const string SpinStart = ":spin-start";

    private static readonly ConditionalWeakTable<TextBox, Link> Links = new();

    private SpinColumn()
    {
    }

    public static readonly AttachedProperty<bool> FollowsProperty =
        AvaloniaProperty.RegisterAttached<SpinColumn, TextBox, bool>("Follows");

    public static bool GetFollows(TextBox box) => box.GetValue(FollowsProperty);

    public static void SetFollows(TextBox box, bool value) => box.SetValue(FollowsProperty, value);

    static SpinColumn()
    {
        FollowsProperty.Changed.AddClassHandler<TextBox>((box, e) =>
        {
            box.AttachedToLogicalTree -= OnAttached;
            box.DetachedFromLogicalTree -= OnDetached;
            box.TemplateApplied -= OnTemplateApplied;
            Release(box);

            if (e.GetNewValue<bool>())
            {
                box.AttachedToLogicalTree += OnAttached;
                box.DetachedFromLogicalTree += OnDetached;
                box.TemplateApplied += OnTemplateApplied;
                if (((Avalonia.LogicalTree.ILogical)box).IsAttachedToLogicalTree)
                {
                    Follow(box);
                }
            }
        });
    }

    /// <summary>What one TextBox follows: its NumericUpDown, the spin column, and its own input box.</summary>
    private sealed class Link
    {
        public required NumericUpDown Field { get; init; }

        public Control? Box { get; set; }

        public Control? Column { get; set; }
    }

    private static void OnAttached(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) => Follow((TextBox)sender!);

    private static void OnDetached(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) => Release((TextBox)sender!);

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        var box = (TextBox)sender!;
        if (Links.TryGetValue(box, out var link))
        {
            Watch(box, link, e.NameScope.Find<Control>("PART_Root"));
        }
    }

    private static void Follow(TextBox box)
    {
        Release(box);
        if (box.TemplatedParent is not NumericUpDown field)
        {
            return;
        }

        var link = new Link { Field = field };
        Links.Add(box, link);
        field.PropertyChanged += OnFieldChanged;
        Mark(box, field);
        Watch(box, link, Part<Control>(box, "PART_Root"));
    }

    private static void Release(TextBox box)
    {
        if (!Links.TryGetValue(box, out var link))
        {
            return;
        }

        link.Field.PropertyChanged -= OnFieldChanged;
        if (link.Box is not null)
        {
            link.Box.PropertyChanged -= OnBoxChanged;
        }

        Links.Remove(box);
        ((IPseudoClasses)box.Classes).Set(SpinEnd, false);
        ((IPseudoClasses)box.Classes).Set(SpinStart, false);
    }

    /// <summary>Follows the TextBox's input box, the part the column spans.</summary>
    private static void Watch(TextBox box, Link link, Control? inputBox)
    {
        if (link.Box is not null)
        {
            link.Box.PropertyChanged -= OnBoxChanged;
        }

        link.Box = inputBox;
        if (inputBox is not null)
        {
            inputBox.PropertyChanged += OnBoxChanged;
            Size(link);
        }
    }

    private static void OnFieldChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ButtonSpinner.ShowButtonSpinnerProperty && e.Property != ButtonSpinner.ButtonSpinnerLocationProperty
            && e.Property != NumericUpDown.ShowButtonSpinnerProperty && e.Property != NumericUpDown.ButtonSpinnerLocationProperty)
        {
            return;
        }

        var field = (NumericUpDown)sender!;
        if (Part<ButtonSpinner>(field, "PART_Spinner")?.Content is TextBox box)
        {
            Mark(box, field);
        }
    }

    private static void OnBoxChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Visual.BoundsProperty)
        {
            return;
        }

        var inputBox = (Control)sender!;
        if (inputBox.TemplatedParent is TextBox box && Links.TryGetValue(box, out var link))
        {
            Size(link);
        }
    }

    private static void Mark(TextBox box, NumericUpDown field)
    {
        var shown = field.ShowButtonSpinner;
        var start = field.ButtonSpinnerLocation == Location.Left;
        ((IPseudoClasses)box.Classes).Set(SpinEnd, shown && !start);
        ((IPseudoClasses)box.Classes).Set(SpinStart, shown && start);
    }

    /// <summary>A named part of a control's own template, not of a template inside it.</summary>
    private static T? Part<T>(TemplatedControl owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name && c.TemplatedParent == owner);

    /// <summary>The column from the top of the input box to its bottom, over it.</summary>
    private static void Size(Link link)
    {
        link.Column ??= Part<ButtonSpinner>(link.Field, "PART_Spinner") is { } spinner
            ? Part<Control>(spinner, "PART_SpinnerPanel")
            : null;
        if (link.Column is null || link.Box is null || link.Box.Bounds.Height <= 0)
        {
            return;
        }

        link.Column.VerticalAlignment = VerticalAlignment.Top;
        link.Column.Height = link.Box.Bounds.Height;
    }
}
