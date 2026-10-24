using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudSnackbarProvider lists its snackbars in the order they were shown, top to bottom, at every
/// position: _snackbar.scss never reverses the column, and SnackbarConfiguration.NewestOnTop
/// defaults to false. So the newest is at the bottom, anchored top or bottom, and `newest-on-top`
/// on the manager is NewestOnTop.
/// </summary>
public class SnackbarOrderTests
{
    private static string[] Shown(NotificationPosition position, params string[] classes)
    {
        var host = Matrix.Show(new Border { Width = 600, Height = 400 });
        var manager = new WindowNotificationManager(TopLevel.GetTopLevel(host)!) { Position = position };
        foreach (var cls in classes)
        {
            manager.Classes.Add(cls);
        }

        Dispatcher.UIThread.RunJobs();
        manager.Show("First");
        manager.Show("Second");
        Dispatcher.UIThread.RunJobs();

        return manager.GetVisualDescendants().OfType<NotificationCard>()
            .OrderBy(c => c.TranslatePoint(default, manager)!.Value.Y)
            .Select(c => (string)c.Content!)
            .ToArray();
    }

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomRight)]
    [InlineData(NotificationPosition.BottomCenter)]
    public Task TheNewestIsAtTheBottom(NotificationPosition position) => UiThread.RunAsync(() =>
    {
        Assert.Equal(["First", "Second"], Shown(position));
    });

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomLeft)]
    public Task NewestOnTopPutsItFirst(NotificationPosition position) => UiThread.RunAsync(() =>
    {
        Assert.Equal(["Second", "First"], Shown(position, "newest-on-top"));
    });
}
