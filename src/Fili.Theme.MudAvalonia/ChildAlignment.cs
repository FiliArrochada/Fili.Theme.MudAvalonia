using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

namespace Fili.Theme.MudAvalonia;

/// <summary>How a host centres what it holds.</summary>
internal enum Centring
{
    None,

    /// <summary>`align-items: center` in a row: centred across it.</summary>
    Vertical,

    /// <summary>Centred both ways, as an avatar's content is.</summary>
    Both,
}

/// <summary>
/// The children a host centres: an <c>avatar</c>'s content both ways, a <c>toolbar</c>'s row and
/// that row's items, a <c>card-actions</c> row's items. Set by the host's style; this sets the
/// alignment on the children, and keeps it as they come and go.
/// </summary>
/// <remarks>
/// <para>
/// These were child selectors, <c>Border.avatar &gt; :is(Control)</c> and two more. A selector
/// ending in <c>:is(Control)</c> is matched against every control in the window, every template
/// part included, and each match watches the control's parent for the host's class: three such
/// styles were an instance on every control, to centre the children of a few. Here only a host
/// pays.
/// </para>
/// <para>
/// The values are set at <see cref="BindingPriority.StyleTrigger"/>, where the selectors set them,
/// so an app's own alignment on a child still wins; a child that leaves the host, or a host that
/// loses its class, gets its own back.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The classes are the
/// interface.
/// </para>
/// </remarks>
internal sealed class ChildAlignment
{
    private ChildAlignment()
    {
    }

    /// <summary>On a Border: how its child is centred.</summary>
    public static readonly AttachedProperty<Centring> ChildProperty =
        AvaloniaProperty.RegisterAttached<ChildAlignment, Control, Centring>("Child");

    /// <summary>
    /// On a Panel, its children centred across the row; on a Border, the children of the panel
    /// it holds.
    /// </summary>
    public static readonly AttachedProperty<bool> ItemsProperty =
        AvaloniaProperty.RegisterAttached<ChildAlignment, Control, bool>("Items");

    static ChildAlignment()
    {
        ChildProperty.Changed.AddClassHandler<Control>((host, _) => Refresh(host));
        ItemsProperty.Changed.AddClassHandler<Control>((host, _) => Refresh(host));
        Decorator.ChildProperty.Changed.AddClassHandler<Decorator>((host, _) =>
        {
            if (Hosts.TryGetValue(host, out var _))
            {
                Refresh(host);
            }
        });
    }

    public static Centring GetChild(Control host) => host.GetValue(ChildProperty);

    public static void SetChild(Control host, Centring value) => host.SetValue(ChildProperty, value);

    public static bool GetItems(Control host) => host.GetValue(ItemsProperty);

    public static void SetItems(Control host, bool value) => host.SetValue(ItemsProperty, value);

    /// <summary>What a host has centred, and the panel whose children it follows.</summary>
    private sealed class Host
    {
        public readonly List<Control> Centred = [];
        public Panel? Watched;
        public NotifyCollectionChangedEventHandler? OnChildren;
    }

    private static readonly ConditionalWeakTable<Control, Host> Hosts = new();

    /// <summary>Clears what the host centred, then centres what it holds now.</summary>
    private static void Refresh(Control host)
    {
        var state = Hosts.GetOrCreateValue(host);
        foreach (var child in state.Centred)
        {
            Write(child, Layoutable.VerticalAlignmentProperty, null);
            Write(child, Layoutable.HorizontalAlignmentProperty, null);
        }

        state.Centred.Clear();
        if (state.Watched is { } watched)
        {
            watched.Children.CollectionChanged -= state.OnChildren;
            state.Watched = null;
        }

        var centring = GetChild(host);
        var items = GetItems(host);
        if (centring == Centring.None && !items)
        {
            Hosts.Remove(host);
            return;
        }

        if (centring != Centring.None && host is Decorator { Child: { } only })
        {
            Write(only, Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center);
            if (centring == Centring.Both)
            {
                Write(only, Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            }

            state.Centred.Add(only);
        }

        if (items && (host as Panel ?? (host as Decorator)?.Child as Panel) is { } row)
        {
            foreach (var child in row.Children)
            {
                Write(child, Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center);
                state.Centred.Add(child);
            }

            state.OnChildren ??= (_, _) => Refresh(host);
            row.Children.CollectionChanged += state.OnChildren;
            state.Watched = row;
        }
    }

    /// <summary>
    /// What this set on each child, by property: a value set below LocalValue priority is an entry
    /// of its own, which a second SetValue adds to rather than replaces, so the earlier one is
    /// disposed first.
    /// </summary>
    private static readonly ConditionalWeakTable<Control, Dictionary<AvaloniaProperty, IDisposable>> Set = new();

    private static void Write(Control control, AvaloniaProperty target, object? value)
    {
        var set = Set.GetOrCreateValue(control);
        if (set.Remove(target, out var previous))
        {
            previous.Dispose();
        }

        if (value is not null && control.SetValue(target, value, BindingPriority.StyleTrigger) is { } entry)
        {
            set[target] = entry;
        }
    }
}
