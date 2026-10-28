using Avalonia;
using Avalonia.Controls;

namespace Fili.Theme.MudAvalonia.Gallery.Docs.Custom;

/// <summary>
/// MudAvatarGroup's Max on a StackPanel of avatars: <c>Max</c> on the panel shows its first Max
/// avatars and hides the rest, and the avatar marked <c>IsMore</c> shows how many are hidden as
/// "+n", hidden itself when none are. Each avatar sits over the next, as MudAvatarGroup's z-index
/// counts down; the "+n" avatar, which has none, under the last. The overlap is the panel's
/// negative Spacing and the ring each avatar's border, both the app's.
/// </summary>
public sealed class AvatarGroup
{
    private AvatarGroup()
    {
    }

    public static readonly AttachedProperty<int> MaxProperty =
        AvaloniaProperty.RegisterAttached<AvatarGroup, StackPanel, int>("Max", int.MaxValue);

    public static readonly AttachedProperty<bool> IsMoreProperty =
        AvaloniaProperty.RegisterAttached<AvatarGroup, Border, bool>("IsMore");

    static AvatarGroup()
    {
        MaxProperty.Changed.AddClassHandler<StackPanel>((panel, e) =>
        {
            // Children may arrive after Max is set, as XAML sets attributes before content.
            if (e.OldValue is int.MaxValue)
            {
                panel.Children.CollectionChanged += (_, _) => Update(panel);
            }

            Update(panel);
        });
    }

    public static int GetMax(StackPanel panel) => panel.GetValue(MaxProperty);

    public static void SetMax(StackPanel panel, int value) => panel.SetValue(MaxProperty, value);

    public static bool GetIsMore(Border avatar) => avatar.GetValue(IsMoreProperty);

    public static void SetIsMore(Border avatar, bool value) => avatar.SetValue(IsMoreProperty, value);

    private static void Update(StackPanel panel)
    {
        var max = Math.Max(0, GetMax(panel));
        var avatars = panel.Children.OfType<Border>().Where(b => !GetIsMore(b)).ToList();
        for (var i = 0; i < avatars.Count; i++)
        {
            avatars[i].IsVisible = i < max;
            avatars[i].ZIndex = avatars.Count - i;
        }

        var hidden = avatars.Count - max;
        foreach (var more in panel.Children.OfType<Border>().Where(GetIsMore))
        {
            more.IsVisible = hidden > 0;
            var text = $"+{hidden}";
            if (more.Child is TextBlock label)
            {
                label.Text = text;
            }
            else
            {
                more.Child = new TextBlock { Text = text };
            }
        }
    }
}
