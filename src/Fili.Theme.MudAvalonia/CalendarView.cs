using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Calendar = Avalonia.Controls.Calendar;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// The date picker's toolbar buttons: MudBaseDatePicker's year button opens the year view, and its
/// date button goes back to the selected date's month. And the toolbar's corners: the paper's top
/// two, as `.mud-picker-container .mud-toolbar` inherits them. And `clearable`: the field's clear
/// button clears the picker's date, as MudPicker's Clear sets the value to null at once.
/// </summary>
/// <remarks>
/// <para>
/// A button in the picker's template sets <see cref="TargetProperty"/> to the template's
/// <c>Calendar</c> and <see cref="ShowsProperty"/> to the view it opens; a click sets the calendar's
/// <c>DisplayMode</c>, and for the month view moves its <c>DisplayDate</c> to the selected date.
/// A template has no way to set another control's property on a click, so this does it.
/// </para>
/// <para>
/// The picker's field sets <see cref="ClearsDateProperty"/>. Its clear button empties the text, and
/// a CalendarDatePicker reads typed text only when the field loses focus, so the old date would
/// stay until then; the button's Click, raised before its command runs, clears the date with it.
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

    /// <summary>On a date picker's field: its clear button clears the picker's date.</summary>
    public static readonly AttachedProperty<bool> ClearsDateProperty =
        AvaloniaProperty.RegisterAttached<CalendarView, TextBox, bool>("ClearsDate");

    /// <summary>
    /// The keyboard is in use in this calendar, or the picker it drops down from: the day and
    /// month the arrow keys are on draw their ring. Inherited, so the days read it.
    /// </summary>
    public static readonly AttachedProperty<bool> KeyboardProperty =
        AvaloniaProperty.RegisterAttached<CalendarView, Control, bool>("Keyboard", inherits: true);

    static CalendarView()
    {
        // A picker's popup gives its calendar focus when it opens (Avalonia 12.1.5), so the arrow
        // keys work; :dayfocused is set then, opened by the pointer too, and MudDatePicker draws no
        // ring for a click. So the ring waits for the keyboard: a key in the calendar, or in the
        // picker that opened it, which the calendar inherits; a press of the pointer drops it.
        InputElement.KeyDownEvent.AddClassHandler<CalendarDatePicker>(
            (picker, _) => picker.SetValue(KeyboardProperty, true), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerPressedEvent.AddClassHandler<CalendarDatePicker>(
            (picker, _) => picker.SetValue(KeyboardProperty, false), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.KeyDownEvent.AddClassHandler<Calendar>(
            (calendar, _) => calendar.SetValue(KeyboardProperty, true), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerPressedEvent.AddClassHandler<Calendar>(
            (calendar, _) => calendar.SetValue(KeyboardProperty, false), RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.GotFocusEvent.AddClassHandler<Calendar>((calendar, e) =>
        {
            if (e.Source != calendar)
            {
                return;
            }

            // Tabbed to: the keyboard. Otherwise whatever the picker above it knows, or nothing.
            if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional)
            {
                calendar.SetValue(KeyboardProperty, true);
            }
            else
            {
                calendar.ClearValue(KeyboardProperty);
            }
        });

        ClearsDateProperty.Changed.AddClassHandler<TextBox>((field, e) =>
        {
            field.RemoveHandler(Button.ClickEvent, OnFieldClick);
            if (e.GetNewValue<bool>())
            {
                field.AddHandler(Button.ClickEvent, OnFieldClick);
            }
        });

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

    public static bool GetClearsDate(TextBox field) => field.GetValue(ClearsDateProperty);

    public static void SetClearsDate(TextBox field, bool value) => field.SetValue(ClearsDateProperty, value);

    private static void OnFieldClick(object? sender, RoutedEventArgs e)
    {
        var field = (TextBox)sender!;
        // The clear button is built into the field's PART_ClearHost by `clearable`'s style, so
        // it is the content of the field's template rather than a part of it: its own field is
        // the nearest one above it.
        if (e.Source is Button { Name: "PART_ClearButton" } button && button.FindAncestorOfType<TextBox>() == field
            && field.TemplatedParent is CalendarDatePicker picker)
        {
            picker.SelectedDate = null;
        }
    }

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
