using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// The time picker's text: the field's value as MudTimePicker writes it, and the toolbar's hour,
/// minute and period following the columns as they turn.
/// </summary>
/// <remarks>
/// <para>
/// Avalonia's <c>TimePicker</c> shows its time in separate hour, minute and period blocks, which
/// its code fills in. MudTimePicker is a text field holding one string, <c>HH:mm</c>, or
/// <c>hh:mm tt</c> on a 12-hour clock (TimeSpanConverter.Format24Hours and Format12Hours), so the
/// template shows that string in the theme's text field and keeps the blocks out of sight.
/// </para>
/// <para>
/// The toolbar shows the time being picked, before it is accepted. The columns are
/// <c>DateTimePickerPanel</c>s, whose <c>SelectedValue</c> is not a property a binding can watch,
/// so a toolbar <c>TextBlock</c> names its column in <see cref="FollowsProperty"/> and this keeps
/// it in step through the column's <c>SelectionChanged</c>: the value in two digits, or, with
/// <see cref="PeriodProperty"/> set, the <c>chosen</c> class while that period is selected.
/// </para>
/// <para>
/// And the field itself, with <see cref="FieldProperty"/> on the template's TextBox. Read-only,
/// as MudTimePicker is by default, a click on it or Enter, Space or Alt+Down opens the picker, as
/// clicking MudPicker's input does; the clock icon button at its end is Avalonia's
/// <c>PART_FlyoutButton</c>, so it opens the picker with a hover of its own. With the picker's
/// <c>editable</c> class, MudTimePicker's Editable, the time is typed instead: on Enter or when
/// the field loses focus the text becomes SelectedTime, empty text no time, and text that is not
/// a time goes back to the time the picker holds.
/// </para>
/// <para>
/// Internal, like the other helpers: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class TimeView
{
    private const string Chosen = "chosen";

    private TimeView()
    {
    }

    /// <summary>The column a toolbar TextBlock follows.</summary>
    public static readonly AttachedProperty<DateTimePickerPanel?> FollowsProperty =
        AvaloniaProperty.RegisterAttached<TimeView, TextBlock, DateTimePickerPanel?>("Follows");

    /// <summary>For a period column: the period this TextBlock names, 0 for AM or 1 for PM.</summary>
    public static readonly AttachedProperty<int?> PeriodProperty =
        AvaloniaProperty.RegisterAttached<TimeView, TextBlock, int?>("Period");

    private static readonly AttachedProperty<EventHandler?> HandlerProperty =
        AvaloniaProperty.RegisterAttached<TimeView, TextBlock, EventHandler?>("Handler");

    /// <summary>The time picker's field: opens the picker when read-only, commits typed text when not.</summary>
    public static readonly AttachedProperty<bool> FieldProperty =
        AvaloniaProperty.RegisterAttached<TimeView, TextBox, bool>("Field");

    static TimeView()
    {
        FollowsProperty.Changed.AddClassHandler<TextBlock>((block, e) =>
        {
            if (e.GetOldValue<DateTimePickerPanel?>() is { } old && block.GetValue(HandlerProperty) is { } handler)
            {
                old.SelectionChanged -= handler;
            }

            if (e.GetNewValue<DateTimePickerPanel?>() is { } column)
            {
                EventHandler follow = (_, _) => Show(block, column);
                block.SetValue(HandlerProperty, follow);
                column.SelectionChanged += follow;
                Show(block, column);
            }
        });
        FieldProperty.Changed.AddClassHandler<TextBox>((field, e) =>
        {
            field.RemoveHandler(InputElement.PointerReleasedEvent, OnFieldReleased);
            field.RemoveHandler(InputElement.KeyDownEvent, OnFieldKeyDown);
            field.LostFocus -= OnFieldLostFocus;
            if (e.GetNewValue<bool>())
            {
                field.AddHandler(InputElement.PointerReleasedEvent, OnFieldReleased, RoutingStrategies.Bubble, handledEventsToo: true);
                field.AddHandler(InputElement.KeyDownEvent, OnFieldKeyDown, RoutingStrategies.Tunnel);
                field.LostFocus += OnFieldLostFocus;
            }
        });
        PeriodProperty.Changed.AddClassHandler<TextBlock>((block, _) =>
        {
            if (block.GetValue(FollowsProperty) is { } column)
            {
                Show(block, column);
            }
        });
    }

    public static DateTimePickerPanel? GetFollows(TextBlock block) => block.GetValue(FollowsProperty);

    public static void SetFollows(TextBlock block, DateTimePickerPanel? value) => block.SetValue(FollowsProperty, value);

    public static int? GetPeriod(TextBlock block) => block.GetValue(PeriodProperty);

    public static bool GetField(TextBox field) => field.GetValue(FieldProperty);

    public static void SetField(TextBox field, bool value) => field.SetValue(FieldProperty, value);

    private static void OnFieldReleased(object? sender, PointerReleasedEventArgs e)
    {
        var field = (TextBox)sender!;
        if (field.IsReadOnly && e.InitialPressMouseButton == MouseButton.Left && field.TemplatedParent is TimePicker picker)
        {
            Open(picker);
        }
    }

    private static void OnFieldKeyDown(object? sender, KeyEventArgs e)
    {
        var field = (TextBox)sender!;
        if (field.TemplatedParent is not TimePicker picker)
        {
            return;
        }

        if (field.IsReadOnly)
        {
            if (e.Key is Key.Enter or Key.Space || (e.Key == Key.Down && e.KeyModifiers.HasFlag(KeyModifiers.Alt)))
            {
                Open(picker);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Enter)
        {
            Commit(field, picker);
            e.Handled = true;
        }
    }

    private static void OnFieldLostFocus(object? sender, RoutedEventArgs e)
    {
        var field = (TextBox)sender!;
        if (!field.IsReadOnly && field.TemplatedParent is TimePicker picker)
        {
            Commit(field, picker);
        }
    }

    /// <summary>Opens the picker as its own button does: Avalonia keeps opening private to it.</summary>
    private static void Open(TimePicker picker)
    {
        // The button is the field's content, so it has no TemplatedParent of its own to match on.
        var button = picker.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "PART_FlyoutButton");
        if (button is { IsEffectivelyEnabled: true })
        {
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }

    /// <summary>The typed text to SelectedTime; text that is not a time goes back to the one held.</summary>
    private static void Commit(TextBox field, TimePicker picker)
    {
        var text = field.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            picker.SetCurrentValue(TimePicker.SelectedTimeProperty, null);
            return;
        }

        if (Parse(text) is { } time)
        {
            picker.SetCurrentValue(TimePicker.SelectedTimeProperty, time);
        }

        field.SetCurrentValue(TextBox.TextProperty,
            FieldText.Convert([picker.SelectedTime, picker.ClockIdentifier, picker.UseSeconds], typeof(string), null, CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// A typed time: 14:30, 14:30:15, 2:30 PM, 02:30 pm, 2:30PM, in the current culture's
    /// designators or "AM" and "PM".
    /// </summary>
    internal static TimeSpan? Parse(string text)
    {
        var culture = CultureInfo.CurrentCulture;
        var normal = text.Replace(AmDesignator, "AM", StringComparison.OrdinalIgnoreCase)
            .Replace(PmDesignator, "PM", StringComparison.OrdinalIgnoreCase);
        string[] formats =
        [
            "H:mm", "HH:mm", "H:mm:ss", "HH:mm:ss",
            "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt", "h:mm:ss tt", "hh:mm:ss tt", "h tt", "htt",
        ];
        return DateTime.TryParseExact(normal, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var at)
            || DateTime.TryParseExact(text, formats, culture, DateTimeStyles.AllowWhiteSpaces, out at)
            ? at.TimeOfDay
            : null;
    }

    public static void SetPeriod(TextBlock block, int? value) => block.SetValue(PeriodProperty, value);

    /// <summary>
    /// SelectedTime, ClockIdentifier and UseSeconds to the field's text: <c>14:30</c>,
    /// <c>02:30 PM</c>, with <c>:ss</c> after the minutes when seconds are shown; empty for no time.
    /// </summary>
    public static readonly IMultiValueConverter FieldText = new FieldTextConverter();

    /// <summary>True for a 12-hour ClockIdentifier, which shows AM and PM.</summary>
    public static readonly IValueConverter IsTwelveHour =
        new FuncValueConverter<string?, bool>(clock => clock == "12HourClock");

    /// <summary>
    /// The current culture's AM, or "AM" where it has none, as MudTimePicker's toolbar writes it:
    /// several cultures, pt-PT among them, leave the designators empty.
    /// </summary>
    public static string AmDesignator => Designator(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator, "AM");

    /// <summary>The current culture's PM, or "PM" where it has none.</summary>
    public static string PmDesignator => Designator(CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator, "PM");

    private static string Designator(string culture, string fallback) =>
        string.IsNullOrEmpty(culture) ? fallback : culture;

    private static void Show(TextBlock block, DateTimePickerPanel column)
    {
        if (block.GetValue(PeriodProperty) is { } period)
        {
            block.Classes.Set(Chosen, column.SelectedValue == period);
        }
        else
        {
            block.Text = column.SelectedValue.ToString("D2", CultureInfo.CurrentCulture);
        }
    }

    private sealed class FieldTextConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 3 || values[0] is not TimeSpan time)
            {
                return string.Empty;
            }

            var twelve = values[1] as string == "12HourClock";
            var seconds = values[2] is true;
            var at = DateTime.Today.Add(time);
            var text = at.ToString((twelve ? "hh:mm" : "HH:mm") + (seconds ? ":ss" : string.Empty), CultureInfo.CurrentCulture);
            return twelve ? text + " " + (time.Hours < 12 ? AmDesignator : PmDesignator) : text;
        }
    }
}
