using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudTimePicker on Avalonia's TimePicker: the standard text field holding the time as
/// MudTimePicker writes it, with the AccessTime adornment, and MudPicker's popover with
/// MudTimePicker's toolbar over Avalonia's columns. From MudTimePicker.razor(.cs) and
/// _pickertime.scss.
/// </summary>
public class TimePickerTests
{
    private static TimePicker Show(TimeSpan? time, string clock = "24HourClock", params string[] classes)
    {
        var picker = new TimePicker { Width = 260, SelectedTime = time, ClockIdentifier = clock };
        AutomationProperties.SetName(picker, "Start");
        picker.Classes.AddRange(classes);
        return Matrix.Show(picker);
    }

    private static Control Open(TimePicker picker)
    {
        Matrix.Part<Button>(picker, "PART_FlyoutButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        return (Control)picker.GetVisualDescendants().OfType<Popup>().Single(p => p.Name == "PART_Popover").Child!;
    }

    private static string Text(Control popover, string name) =>
        popover.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Name == name).Text!;

    /// <summary>
    /// The field is the standard text field, its own theme untouched: the picker's classes reach
    /// it, AutomationProperties.Name is its label, and it holds the time as HH:mm.
    /// </summary>
    [Fact]
    public Task TheFieldIsAStandardTextField() => UiThread.RunAsync(() =>
    {
        var picker = Show(new TimeSpan(14, 30, 0), "24HourClock", "outlined", "dense");
        var field = Matrix.Part<TextBox>(picker, "PART_TextBox");

        Assert.Contains("outlined", field.Classes);
        Assert.Contains("dense", field.Classes);
        Assert.Equal("Start", field.PlaceholderText);
        Assert.Equal("14:30", field.Text);
        Assert.True(field.IsReadOnly);
    });

    /// <summary>
    /// Full width, as MudTimePicker is a MudTextField: in a column it stretches as the date
    /// picker does, where it stayed at its 120px minimum.
    /// </summary>
    [Fact]
    public Task ItStretchesAsAField() => UiThread.RunAsync(() =>
    {
        var picker = new TimePicker();
        var date = new CalendarDatePicker();
        Matrix.Show(new StackPanel { Width = 300, Children = { date, picker } });

        Assert.Equal(300, picker.Bounds.Width);
        Assert.Equal(date.Bounds.Width, picker.Bounds.Width);
    });

    /// <summary>
    /// A 12-hour clock writes hh:mm and the period, "AM" or "PM" where the culture has none, as
    /// MudTimePicker's toolbar does; no time is an empty field, the label resting in it.
    /// </summary>
    [Fact]
    public Task ATwelveHourClockWritesThePeriod() => UiThread.RunAsync(() =>
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        try
        {
            Assert.Equal("02:30 PM", Matrix.Part<TextBox>(Show(new TimeSpan(14, 30, 0), "12HourClock"), "PART_TextBox").Text);
            Assert.Equal(string.Empty, Matrix.Part<TextBox>(Show(null), "PART_TextBox").Text);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
    });

    /// <summary>
    /// MudPicker's AdornmentIcon for a time, Icons.Material.Filled.AccessTime, in an icon button at
    /// the end: Avalonia's own flyout button, so it opens the picker, with MudIconButton's hover.
    /// </summary>
    [Fact]
    public Task TheAdornmentIsAccessTimeWithAHover() => UiThread.RunAsync(() =>
    {
        var picker = Show(null);
        var button = Matrix.Part<Button>(picker, "PART_FlyoutButton");

        Assert.Contains("icon", button.Classes);
        Assert.Same(Application.Current!.FindResource("FiliAccessTimeGeometry"), button.GetVisualDescendants().OfType<PathIcon>().Single().Data);

        Matrix.Hover(button);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Matrix.Part<Border>(button, "PART_StateLayer").Background));
    });

    /// <summary>Read-only, as MudTimePicker is by default: a click on the field opens the picker, and so does Enter.</summary>
    [Fact]
    public Task AClickOrEnterOnTheFieldOpensThePicker() => UiThread.RunAsync(() =>
    {
        var picker = Show(new TimeSpan(14, 30, 0));
        var field = Matrix.Part<TextBox>(picker, "PART_TextBox");
        var popover = picker.GetVisualDescendants().OfType<Popup>().Single(p => p.Name == "PART_Popover");
        var window = (Window)TopLevel.GetTopLevel(picker)!;
        var point = field.TranslatePoint(new Point(40, field.Bounds.Height / 2), window)!.Value;

        Assert.True(field.IsReadOnly);
        window.MouseDown(point, Avalonia.Input.MouseButton.Left);
        window.MouseUp(point, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.True(popover.IsOpen);

        popover.IsOpen = false;
        Dispatcher.UIThread.RunJobs();
        field.RaiseEvent(new Avalonia.Input.KeyEventArgs { RoutedEvent = Avalonia.Input.InputElement.KeyDownEvent, Key = Avalonia.Input.Key.Enter });
        Dispatcher.UIThread.RunJobs();
        Assert.True(popover.IsOpen);
    });

    /// <summary>
    /// `editable` is MudTimePicker's Editable: the time is typed, and Enter makes it SelectedTime;
    /// text that is not a time goes back to the time held, and empty text is no time.
    /// </summary>
    [Fact]
    public Task AnEditablePickerTakesATypedTime() => UiThread.RunAsync(() =>
    {
        var picker = Show(new TimeSpan(14, 30, 0), "24HourClock", "editable");
        var field = Matrix.Part<TextBox>(picker, "PART_TextBox");
        void Type(string text)
        {
            field.SetCurrentValue(TextBox.TextProperty, text);
            field.RaiseEvent(new Avalonia.Input.KeyEventArgs { RoutedEvent = Avalonia.Input.InputElement.KeyDownEvent, Key = Avalonia.Input.Key.Enter });
            Dispatcher.UIThread.RunJobs();
        }

        Assert.False(field.IsReadOnly);

        Type("9:05");
        Assert.Equal(new TimeSpan(9, 5, 0), picker.SelectedTime);
        Assert.Equal("09:05", field.Text);

        Type("2:15 PM");
        Assert.Equal(new TimeSpan(14, 15, 0), picker.SelectedTime);

        Type("not a time");
        Assert.Equal(new TimeSpan(14, 15, 0), picker.SelectedTime);
        Assert.Equal("14:15", field.Text);

        Type("");
        Assert.Null(picker.SelectedTime);
    });

    /// <summary>
    /// A time picker is a text field's height in every variant, so it lines up in a row of fields,
    /// and its icon is centred on the box of a filled or outlined one.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("dense")]
    [InlineData("outlined")]
    [InlineData("outlined dense")]
    [InlineData("filled")]
    [InlineData("filled dense")]
    public Task ThePickerIsATextFieldsHeight(string variant) => UiThread.RunAsync(() =>
    {
        var classes = variant.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = new TextBox { Width = 260, PlaceholderText = "Start", Text = "14:30" };
        field.Classes.AddRange(classes);
        Matrix.Show(field);

        var picker = Show(new TimeSpan(14, 30, 0), "24HourClock", classes);
        var glyph = Matrix.Part<Button>(picker, "PART_FlyoutButton").GetVisualDescendants().OfType<PathIcon>().Single();
        var box = Matrix.Part<Border>(Matrix.Part<TextBox>(picker, "PART_TextBox"), "PART_Root");
        var text = Matrix.Part<Avalonia.Controls.Presenters.TextPresenter>(Matrix.Part<TextBox>(picker, "PART_TextBox"), "PART_TextPresenter");
        double Middle(Control c) => c.TranslatePoint(new Point(0, c.Bounds.Height / 2), picker)!.Value.Y;

        Assert.Equal(field.Bounds.Height, picker.Bounds.Height);
        // On the box in a filled or outlined field, as `.mud-input` centres its adornment; on the
        // text line in the standard one.
        Assert.InRange(Middle(glyph) - (classes.Contains("filled") || classes.Contains("outlined") ? Middle(box) : Middle(text)), -1.5, 1.5);
    });

    /// <summary>
    /// The popover: a paper at elevation 8 round the 100px primary toolbar, the hour and the
    /// minute at h2's 60px light, following the columns before the time is accepted.
    /// </summary>
    [Fact]
    public Task TheToolbarFollowsTheColumns() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show(new TimeSpan(14, 30, 0)));
        var toolbar = popover.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Toolbar");
        var hour = popover.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Name == "PART_ToolbarHour");

        Assert.True(Application.Current!.TryGetResource("FiliElevation8", Application.Current.ActualThemeVariant, out var elevation8));
        Assert.Equal(elevation8, Matrix.Part<Border>((TimePickerPresenter)popover, "PART_PopupBorder").BoxShadow);
        Assert.Equal(100, toolbar.Height);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(toolbar.Background));
        Assert.Equal(60, hour.FontSize);
        Assert.Equal("14", Text(popover, "PART_ToolbarHour"));
        Assert.Equal("30", Text(popover, "PART_ToolbarMinute"));

        popover.GetVisualDescendants().OfType<DateTimePickerPanel>().Single(p => p.Name == "PART_MinuteSelector").SelectedValue = 45;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("45", Text(popover, "PART_ToolbarMinute"));
    });

    /// <summary>
    /// The paper's elevation 8 shadow falls in the presenter's margin: neither the presenter nor
    /// anything between it and the paper clips it.
    /// </summary>
    [Fact]
    public Task ThePapersShadowIsNotClipped() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show(new TimeSpan(14, 30, 0)));

        Assert.False(popover.ClipToBounds, "The presenter clips the paper's shadow.");
        Assert.All(popover.GetVisualDescendants().OfType<Control>().TakeWhile(c => c.Name != "PART_PopupBorder"),
            c => Assert.False(c.ClipToBounds, $"{c.GetType().Name}#{c.Name} clips the paper's shadow."));
    });

    /// <summary>On a 12-hour clock the toolbar shows AM and PM, the one not chosen at .54.</summary>
    [Fact]
    public Task TheToolbarGreysThePeriodNotChosen() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show(new TimeSpan(14, 30, 0), "12HourClock"));
        var blocks = popover.GetVisualDescendants().OfType<TextBlock>().ToList();

        Assert.Equal("02", Text(popover, "PART_ToolbarHour"));
        Assert.Equal(0.54, blocks.Single(t => t.Name == "PART_ToolbarAm").Opacity);
        Assert.Equal(1, blocks.Single(t => t.Name == "PART_ToolbarPm").Opacity);
    });

    /// <summary>
    /// The popover hangs from the field's bottom-left, as MudPicker's does. Avalonia's code moves
    /// PART_Popup to centre the chosen row on the field, from the presenter's height, which put it
    /// at minus infinity: so PART_Popup stays empty and the presenter is in a popover it cannot
    /// move, open while PART_Popup is. Accepting still commits the time and closes both.
    /// </summary>
    [Fact]
    public Task ThePopoverHangsFromTheField() => UiThread.RunAsync(() =>
    {
        var picker = Show(new TimeSpan(14, 30, 0));
        var presenter = Open(picker);
        var popups = picker.GetVisualDescendants().OfType<Popup>().ToList();
        var popover = popups.Single(p => p.Name == "PART_Popover");
        var avalonias = popups.Single(p => p.Name == "PART_Popup");

        Assert.True(popover.IsOpen);
        Assert.Equal(PlacementMode.BottomEdgeAlignedLeft, popover.Placement);
        Assert.Equal(0, popover.VerticalOffset);
        Assert.Same(picker, popover.PlacementTarget);
        Assert.Null(avalonias.Child);

        presenter.GetVisualDescendants().OfType<DateTimePickerPanel>().Single(p => p.Name == "PART_MinuteSelector").SelectedValue = 45;
        presenter.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_AcceptButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new TimeSpan(14, 45, 0), picker.SelectedTime);
        Assert.False(popover.IsOpen);
        Assert.False(avalonias.IsOpen);
    });

    /// <summary>
    /// The paper meets the field's bottom-left and is 310px wide, as the date picker's is. A popup
    /// lets its child's margin hang outside the anchor, so the shadow's 8px is on the presenter:
    /// on the paper inside it, the paper sat 8px right of and below the field, and 16px narrow.
    /// </summary>
    [Fact]
    public Task ThePaperMeetsTheFieldLikeTheDatePickers() => UiThread.RunAsync(() =>
    {
        // Tall enough that neither popup is slid up to fit.
        var time = new TimePicker { Width = 260, SelectedTime = new TimeSpan(14, 30, 0) };
        var date = new CalendarDatePicker { Width = 260 };
        var row = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 400, Margin = new Thickness(20), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Children = { time, date } };
        var window = new Window { Width = 1200, Height = 900, Content = row };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Open(time);
        date.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        try
        {
            foreach (var picker in new Control[] { time, date })
            {
                var paper = window.GetVisualDescendants().OfType<Border>()
                    .Single(b => b.Name == "PART_PopupBorder" && b.TemplatedParent is TimePickerPresenter == picker is TimePicker && b.IsEffectivelyVisible);
                Assert.Equal(picker.TranslatePoint(new Point(0, picker.Bounds.Height), window), paper.TranslatePoint(default, window));
                Assert.Equal(310, paper.Bounds.Width);
            }
        }
        finally
        {
            window.Close();
        }
    });

    /// <summary>A 24-hour clock has no period in the toolbar.</summary>
    [Fact]
    public Task ATwentyFourHourClockHasNoPeriod() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show(new TimeSpan(14, 30, 0)));

        Assert.False(popover.GetVisualDescendants().OfType<StackPanel>().Single(p => p.Name == "PART_ToolbarPeriod").IsVisible);
    });

    /// <summary>The chosen row: on the primary band, in its contrast text; the others body1 in text-secondary.</summary>
    [Fact]
    public Task TheChosenRowIsOnThePrimaryBand() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show(new TimeSpan(14, 30, 0)));
        var rows = popover.GetVisualDescendants().OfType<DateTimePickerPanel>().Single(p => p.Name == "PART_HourSelector")
            .GetVisualChildren().OfType<ListBoxItem>().ToList();
        var chosen = rows.Single(r => r.IsSelected);
        var other = rows.First(r => !r.IsSelected);

        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(chosen.Foreground));
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(other.Foreground));
        Assert.Equal(16, other.FontSize);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"),
            Matrix.Colour(popover.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "HighlightRect").Background));
    });

    /// <summary>The actions: Close and Check icon buttons, the accept in primary.</summary>
    [Fact]
    public Task TheActionsAreIconButtons() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show(new TimeSpan(14, 30, 0)));
        var buttons = popover.GetVisualDescendants().OfType<Button>().ToList();
        var accept = buttons.Single(b => b.Name == "PART_AcceptButton");

        Assert.Contains("icon", accept.Classes);
        Assert.Contains("primary", accept.Classes);
        Assert.Same(Application.Current!.FindResource("FiliCheckGeometry"), accept.GetVisualDescendants().OfType<PathIcon>().Single().Data);
        Assert.Contains("icon", buttons.Single(b => b.Name == "PART_DismissButton").Classes);
    });
}
