using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Where the arrow keys are in a calendar. Avalonia marks that day :dayfocused, and that month or
/// year :btnfocused; the base theme drew a rectangle for each, and the hand-written day and
/// month themes dropped it, so in the year and decade views and in a range selection a keyboard
/// user could not see where they were. Each is now the focus ring, 2px of text-primary.
/// </summary>
public class CalendarKeyboardTests
{
    private static Calendar Show(CalendarMode mode, CalendarSelectionMode selection = CalendarSelectionMode.SingleRange)
    {
        var calendar = new Calendar { DisplayDate = new DateTime(2026, 3, 10), SelectionMode = selection, DisplayMode = mode };
        Matrix.Show(calendar);
        calendar.Focus();
        Dispatcher.UIThread.RunJobs();
        var window = (Window)TopLevel.GetTopLevel(calendar)!;
        window.KeyPressQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        return calendar;
    }

    [Fact]
    public Task TheKeyboardsDayIsRinged() => UiThread.RunAsync(() =>
    {
        var calendar = Show(CalendarMode.Month);
        var days = calendar.GetVisualDescendants().OfType<CalendarDayButton>().ToList();
        var current = Assert.Single(days, d => d.Classes.Contains(":dayfocused"));
        var other = days.First(d => !d.Classes.Contains(":dayfocused") && !d.Classes.Contains(":today") && !d.Classes.Contains(":inactive"));

        Assert.Equal(new Thickness(2), current.BorderThickness);
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(current.BorderBrush));
        Assert.Equal(new Thickness(0), other.BorderThickness);
    });

    [Theory]
    [InlineData(CalendarMode.Year)]
    [InlineData(CalendarMode.Decade)]
    public Task TheKeyboardsMonthOrYearIsRinged(CalendarMode mode) => UiThread.RunAsync(() =>
    {
        var calendar = Show(mode);
        var buttons = calendar.GetVisualDescendants().OfType<CalendarButton>().Where(b => b.IsEffectivelyVisible).ToList();
        var current = Assert.Single(buttons, b => b.Classes.Contains(":btnfocused"));
        var other = buttons.First(b => !b.Classes.Contains(":btnfocused"));

        Assert.Equal(new Thickness(2), Matrix.Part<Border>(current, "PART_StateLayer").BorderThickness);
        Assert.Equal(new Thickness(0), Matrix.Part<Border>(other, "PART_StateLayer").BorderThickness);
    });

    /// <summary>
    /// The calendar is reachable: one Tab stop of its own, its days none. Avalonia's Calendar is
    /// not focusable by default and leaned on focusable days; these days are not, so nothing
    /// took the arrow keys.
    /// </summary>
    [Fact]
    public Task TheCalendarTakesFocusAndItsDaysDoNot() => UiThread.RunAsync(() =>
    {
        var calendar = Matrix.Show(new Calendar());

        Assert.True(calendar.Focusable);
        Assert.True(calendar.Focus());
        Assert.All(calendar.GetVisualDescendants().OfType<CalendarDayButton>(), d => Assert.False(d.Focusable));
    });

    /// <summary>In the date picker's popover the arrow keys move the day the ring is on.</summary>
    [Fact]
    public Task ThePickersCalendarTakesTheArrowKeys() => UiThread.RunAsync(() =>
    {
        var picker = Matrix.Show(new CalendarDatePicker { Width = 260, SelectedDate = new DateTime(2026, 3, 10) });
        picker.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var calendar = picker.GetVisualDescendants().OfType<Popup>().Single(p => p.Name == "PART_Popup").Child!
            .GetVisualDescendants().OfType<Calendar>().Single();

        calendar.Focus();
        Dispatcher.UIThread.RunJobs();
        var window = (Window)TopLevel.GetTopLevel(picker)!;
        window.KeyPressQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        var ringed = Assert.Single(calendar.GetVisualDescendants().OfType<CalendarDayButton>(), d => d.Classes.Contains(":dayfocused"));
        Assert.Equal("11", ringed.Content?.ToString());
        Assert.Equal(new Thickness(2), ringed.BorderThickness);
        picker.IsDropDownOpen = false;
        Dispatcher.UIThread.RunJobs();
    });
}
