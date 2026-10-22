using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Where each button of a <c>button-group</c> sits, for MudButtonGroup's one shape: a button with
/// a visible button after it gets <c>:joined-next</c>, one with a visible button before it
/// <c>:joined-previous</c>, each of two or more <c>:joined</c>, and in a vertical group
/// <c>:joined-vertical</c>. Set on a StackPanel by the class's style; the styles beside it square
/// the inner corners, overlap the lines and draw the separators.
/// </summary>
/// <remarks>
/// <para>
/// _buttongroup.scss does it with <c>:first-child</c> and <c>:last-child</c>. Avalonia's
/// <c>:nth-child</c> counts hidden children too, and a selector reaching a group's buttons from
/// the StackPanel would end in a type matched against every button in the window; so this marks
/// the few buttons in a group, and a style on Button asks only for a pseudo-class.
/// </para>
/// <para>
/// Internal, like the other helpers: a style needs it, a consumer does not. The class is the
/// interface.
/// </para>
/// </remarks>
internal sealed class ButtonGroup
{
    private const string Joined = ":joined";
    private const string JoinedNext = ":joined-next";
    private const string JoinedPrevious = ":joined-previous";
    private const string JoinedVertical = ":joined-vertical";

    private ButtonGroup()
    {
    }

    public static readonly AttachedProperty<bool> IsOnProperty =
        AvaloniaProperty.RegisterAttached<ButtonGroup, StackPanel, bool>("IsOn");

    /// <summary>The handler watching the group's children, while it is on.</summary>
    private static readonly AttachedProperty<NotifyCollectionChangedEventHandler?> WatcherProperty =
        AvaloniaProperty.RegisterAttached<ButtonGroup, StackPanel, NotifyCollectionChangedEventHandler?>("Watcher");

    static ButtonGroup()
    {
        IsOnProperty.Changed.AddClassHandler<StackPanel>((panel, e) =>
        {
            if (panel.GetValue(WatcherProperty) is { } old)
            {
                panel.Children.CollectionChanged -= old;
                panel.ClearValue(WatcherProperty);
            }

            if (e.GetNewValue<bool>())
            {
                NotifyCollectionChangedEventHandler watcher = (_, args) =>
                {
                    foreach (var gone in args.OldItems?.OfType<Button>() ?? [])
                    {
                        Mark(gone, false, false, false, false);
                    }

                    Refresh(panel);
                };
                panel.Children.CollectionChanged += watcher;
                panel.SetValue(WatcherProperty, watcher);
            }

            Refresh(panel);
        });
        StackPanel.OrientationProperty.Changed.AddClassHandler<StackPanel>((panel, _) =>
        {
            if (GetIsOn(panel))
            {
                Refresh(panel);
            }
        });
        Visual.IsVisibleProperty.Changed.AddClassHandler<Button>((button, _) =>
        {
            if (button.Parent is StackPanel panel && GetIsOn(panel))
            {
                Refresh(panel);
            }
        });
    }

    public static bool GetIsOn(StackPanel panel) => panel.GetValue(IsOnProperty);

    public static void SetIsOn(StackPanel panel, bool value) => panel.SetValue(IsOnProperty, value);

    private static void Refresh(StackPanel panel)
    {
        var members = GetIsOn(panel) ? panel.Children.OfType<Button>().Where(b => b.IsVisible).ToList() : [];
        var vertical = panel.Orientation == Orientation.Vertical;
        foreach (var button in panel.Children.OfType<Button>())
        {
            var at = members.IndexOf(button);
            var joined = members.Count > 1 && at >= 0;
            Mark(button, joined, joined && at < members.Count - 1, joined && at > 0, joined && vertical);
        }
    }

    private static void Mark(Button button, bool joined, bool next, bool previous, bool vertical)
    {
        var pseudo = (IPseudoClasses)button.Classes;
        pseudo.Set(Joined, joined);
        pseudo.Set(JoinedNext, next);
        pseudo.Set(JoinedPrevious, previous);
        pseudo.Set(JoinedVertical, vertical);
    }
}
