using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The date and time pickers keep their forked look, but their spinning columns are ListBoxItems,
/// and the forked item theme is BasedOn the ListBoxItem theme. The themed list item is 48px high;
/// the picker lays its items out 40px apart, so an item that inherits the list's row would overlap
/// the next one and hide the highlight band's centre.
/// </summary>
public class PickerItemTests
{
    private static ListBoxItem[] Items(Control presenter)
    {
        Matrix.Show(presenter);
        Dispatcher.UIThread.RunJobs();
        var items = presenter.GetVisualDescendants().OfType<DateTimePickerPanel>()
            .SelectMany(panel => panel.Children.OfType<ListBoxItem>())
            .ToArray();
        Assert.NotEmpty(items);
        return items;
    }

    [Fact]
    public Task DatePickerItemsFitTheirSlots() => UiThread.RunAsync(() =>
    {
        foreach (var item in Items(new DatePickerPresenter()))
        {
            Assert.True(item.Bounds.Height <= 40, $"A date item is {item.Bounds.Height}px high in a 40px slot.");
        }
    });

    [Fact]
    public Task TimePickerItemsFitTheirSlots() => UiThread.RunAsync(() =>
    {
        foreach (var item in Items(new TimePickerPresenter()))
        {
            Assert.True(item.Bounds.Height <= 40, $"A time item is {item.Bounds.Height}px high in a 40px slot.");
        }
    });
}
