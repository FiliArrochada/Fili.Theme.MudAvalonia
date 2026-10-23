using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Calendar = Avalonia.Controls.Calendar;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// The date picker's toolbar buttons: MudBaseDatePicker's year button opens the year view, and its
/// date button goes back to the selected date's month. And the toolbar's corners: the paper's top
/// two, as `.mud-picker-container .mud-toolbar` inherits them.
/// </summary>
/// <remarks>
/// <para>
/// A button in the picker's template sets <see cref="TargetProperty"/> to the template's
/// <c>Calendar</c> and <see cref="ShowsProperty"/> to the view it opens; a click sets the calendar's
/// <c>DisplayMode</c>, and for the month view moves its <c>DisplayDate</c> to the selected date.
/// A template has no way to set another control's property on a click, so this does it.
/// </para>
/// <para>
/// Internal, like the other helpers: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class CalendarView
{
    private CalendarView()
    {
    }

    /// <summary>The calendar the button changes.</summary>
    public static readonly AttachedProperty<Calendar?> TargetProperty =
        AvaloniaProperty.RegisterAttached<CalendarView, Button, Calendar?>("Target");

    /// <summary>The view a click opens: <c>Decade</c> for the years, <c>Month</c> for the selected date's days.</summary>
    public static readonly AttachedProperty<CalendarMode?> ShowsProperty =
        AvaloniaProperty.RegisterAttached<CalendarView, Button, CalendarMode?>("Shows");

    static CalendarView()
    {
        ShowsProperty.Changed.AddClassHandler<Button>((button, e) =>
        {
            button.Click -= OnClick;
            if (e.GetNewValue<CalendarMode?>() is not null)
            {
                button.Click += OnClick;
            }
        });
    }

    /// <summary>A CornerRadius's top two corners, the bottom two square.</summary>
    public static readonly IValueConverter TopCorners = new TopCornersConverter();

    public static Calendar? GetTarget(Button button) => button.GetValue(TargetProperty);

    public static void SetTarget(Button button, Calendar? value) => button.SetValue(TargetProperty, value);

    public static CalendarMode? GetShows(Button button) => button.GetValue(ShowsProperty);

    public static void SetShows(Button button, CalendarMode? value) => button.SetValue(ShowsProperty, value);

    private static void OnClick(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;
        if (GetTarget(button) is not { } calendar || GetShows(button) is not { } mode)
        {
            return;
        }

        if (mode == CalendarMode.Month && calendar.SelectedDate is { } selected)
        {
            calendar.DisplayDate = selected;
        }

        calendar.DisplayMode = mode;
    }

    private sealed class TopCornersConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is CornerRadius r ? new CornerRadius(r.TopLeft, r.TopRight, 0, 0) : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
