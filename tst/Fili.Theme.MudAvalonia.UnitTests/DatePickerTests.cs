using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Path = Avalonia.Controls.Shapes.Path;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudDatePicker on Avalonia's CalendarDatePicker: the closed picker is the standard text field
/// with MudPicker's Event adornment, and the popover is MudPickerToolbar over the calendar, from
/// MudPicker.razor, MudBaseDatePicker.razor, MudDatePicker.GetDayClasses and _pickerdate.scss.
/// </summary>
public class DatePickerTests
{
    private static readonly DateTime Selected = new(2026, 10, 11);

    private static CalendarDatePicker Show(params string[] classes)
    {
        var picker = new CalendarDatePicker { Width = 260, PlaceholderText = "Date", SelectedDate = Selected };
        picker.Classes.AddRange(classes);
        return Matrix.Show(picker);
    }

    /// <summary>Opens the picker and returns what its popover draws.</summary>
    private static Control Open(CalendarDatePicker picker)
    {
        picker.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var popup = picker.GetVisualDescendants().OfType<Popup>().Single();
        return Assert.IsAssignableFrom<Control>(popup.Child);
    }

    private static CalendarDayButton Day(Control popover, int day) =>
        popover.GetVisualDescendants().OfType<CalendarDayButton>()
            .Single(d => d.IsEffectivelyVisible && d.Content?.ToString() == day.ToString(System.Globalization.CultureInfo.CurrentCulture)
                         && !d.Classes.Contains(":inactive"));

    /// <summary>
    /// The field is the standard text field, its look untouched: a class on the picker reaches the
    /// TextBox inside it as a number field's does, so `outlined` is MudTextField's outlined variant.
    /// </summary>
    [Fact]
    public Task TheFieldIsTheStandardTextFieldWithThePickersClasses() => UiThread.RunAsync(() =>
    {
        var picker = Show("outlined", "dense");
        var field = Matrix.Part<TextBox>(picker, "PART_TextBox");

        Assert.Contains("outlined", field.Classes);
        Assert.Contains("dense", field.Classes);
        Assert.Equal("Date", field.PlaceholderText);
        Assert.Equal(new Thickness(1), Matrix.Part<Border>(field, "PART_Root").BorderThickness);
    });

    /// <summary>
    /// The icon button sits inside the text line, so a picker is exactly as tall as a text field of
    /// the same variant and lines up beside one; its 48px once made the standard field 3px taller.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("dense")]
    [InlineData("outlined")]
    [InlineData("outlined dense")]
    [InlineData("filled")]
    [InlineData("filled dense")]
    public Task ThePickerIsAsTallAsATextField(string classes) => UiThread.RunAsync(() =>
    {
        var names = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var picker = Show(names);
        var field = new TextBox { Width = 260, PlaceholderText = "Date", Text = "11/10/2026" };
        field.Classes.AddRange(names);
        Matrix.Show(field);
        var glyph = Matrix.Part<Button>(picker, "PART_Button").GetVisualDescendants().OfType<PathIcon>().Single();
        var text = Matrix.Part<TextPresenter>(Matrix.Part<TextBox>(picker, "PART_TextBox"), "PART_TextPresenter");
        var glyphMiddle = glyph.TranslatePoint(new Point(0, glyph.Bounds.Height / 2), picker)!.Value.Y;
        var textMiddle = text.TranslatePoint(new Point(0, text.Bounds.Height / 2), picker)!.Value.Y;

        Assert.Equal(field.Bounds.Height, picker.Bounds.Height);
        Assert.InRange(glyphMiddle - textMiddle, -1.5, 1.5);
    });

    /// <summary>MudPicker's AdornmentIcon, Icons.Material.Filled.Event, in a MudIconButton at the field's end.</summary>
    [Fact]
    public Task TheAdornmentIsTheEventIconButton() => UiThread.RunAsync(() =>
    {
        var picker = Show();
        var button = Matrix.Part<Button>(picker, "PART_Button");
        var glyph = button.GetVisualDescendants().OfType<PathIcon>().Single();

        Assert.Contains("icon", button.Classes);
        Assert.False(button.Focusable);
        Assert.Same(Application.Current!.FindResource("FiliEventGeometry"), glyph.Data);
        Assert.Contains(":end-adornment", Matrix.Part<TextBox>(picker, "PART_TextBox").Classes);
    });

    /// <summary>The popover: MudPaper at elevation 8 round the 100px primary toolbar and a 310px calendar.</summary>
    [Fact]
    public Task ThePopoverIsTheToolbarOverTheCalendar() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        var toolbar = popover.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Toolbar");

        Assert.True(Application.Current!.TryGetResource("FiliElevation8", Application.Current.ActualThemeVariant, out var elevation8));
        Assert.Equal(elevation8, ((Border)popover).BoxShadow);
        Assert.Equal(100, toolbar.Height);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(toolbar.Background));
        Assert.Equal(310, popover.GetVisualDescendants().OfType<CalendarItem>().Single().Bounds.Width);
    });

    /// <summary>
    /// The year, and the date in TitleDateFormat "ddd, dd MMM" in the case written: the date's
    /// `text-transform: none`, where every other button label is uppercase.
    /// </summary>
    [Fact]
    public Task TheToolbarShowsTheYearAndTheDateAsWritten() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        var buttons = popover.GetVisualDescendants().OfType<Button>().ToList();
        var year = buttons.Single(b => b.Name == "PART_YearButton");
        var date = buttons.Single(b => b.Name == "PART_DateButton");

        Assert.Equal("2026", Matrix.Part<ContentPresenter>(year, "PART_ContentPresenter").Content?.ToString());
        Assert.Equal(Selected.ToString("ddd, dd MMM", System.Globalization.CultureInfo.CurrentCulture),
            date.GetVisualDescendants().OfType<TextBlock>().Single().Text);
    });

    /// <summary>
    /// A culture whose day names are long ("domingo, 11 out.") shrinks the date to the toolbar's
    /// width, where at h4 it ran past the toolbar's padding and was cut off; the popover keeps the
    /// calendar's 310px.
    /// </summary>
    [Fact]
    public Task ALongDateFitsTheToolbar() => UiThread.RunAsync(() =>
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("pt-PT");
        try
        {
            var popover = Open(Show());
            var toolbar = popover.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Toolbar");
            var text = popover.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_DateButton")
                .GetVisualDescendants().OfType<Viewbox>().Single();
            var right = text.TranslatePoint(new Point(text.Bounds.Width, 0), toolbar)!.Value.X;

            Assert.Equal(310, toolbar.Bounds.Width);
            Assert.True(right <= toolbar.Bounds.Width - 24, $"the date ends at {right}, past the toolbar's padding");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
    });

    /// <summary>The year button opens the years, and the date button goes back to the selected date's month.</summary>
    [Fact]
    public Task TheToolbarButtonsChangeTheView() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        var calendar = popover.GetVisualDescendants().OfType<Calendar>().Single();
        var buttons = popover.GetVisualDescendants().OfType<Button>().ToList();

        buttons.Single(b => b.Name == "PART_YearButton").RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(CalendarMode.Decade, calendar.DisplayMode);

        calendar.DisplayDate = new DateTime(2024, 3, 1);
        buttons.Single(b => b.Name == "PART_DateButton").RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(CalendarMode.Month, calendar.DisplayMode);
        Assert.Equal(new DateTime(2026, 10, 1), new DateTime(calendar.DisplayDate.Year, calendar.DisplayDate.Month, 1));
    });

    /// <summary>`.mud-day`: 36px round with 2px either side, its number body2.</summary>
    [Fact]
    public Task ADayIsA36pxCircle() => UiThread.RunAsync(() =>
    {
        var day = Day(Open(Show()), 12);

        Assert.Equal(36, day.Bounds.Width);
        Assert.Equal(36, day.Bounds.Height);
        Assert.Equal(new Thickness(2, 0), day.Margin);
        Assert.Equal(new CornerRadius(18), day.CornerRadius);
        Assert.Equal(14, day.FontSize);
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(day.Foreground));
    });

    /// <summary>The selected day is `mud-selected mud-theme-primary`: primary under its contrast text.</summary>
    [Fact]
    public Task TheSelectedDayIsFilledPrimary() => UiThread.RunAsync(() =>
    {
        var day = Day(Open(Show()), 11);

        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(day.Background));
        Assert.Equal(Matrix.Token("FiliPrimaryContrastTextColor"), Matrix.Colour(day.Foreground));
    });

    /// <summary>
    /// Today is `mud-current mud-button-outlined-primary mud-primary-text`, which MudDatePicker
    /// always draws; Avalonia's picker does not mark today unless asked, so the theme asks.
    /// </summary>
    [Fact]
    public Task TodayIsRingedInPrimary() => UiThread.RunAsync(() =>
    {
        var picker = new CalendarDatePicker { Width = 260, PlaceholderText = "Date" };
        Matrix.Show(picker);
        Assert.True(picker.IsTodayHighlighted);

        var today = Day(Open(picker), DateTime.Today.Day);

        Assert.Equal(new Thickness(1), today.BorderThickness);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(today.BorderBrush));
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(today.Foreground));
    });

    /// <summary>A day of the month before or after is `mud-hidden`: not drawn, and not clickable.</summary>
    [Fact]
    public Task TheOtherMonthsDaysAreHidden() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        var other = popover.GetVisualDescendants().OfType<CalendarDayButton>().First(d => d.Classes.Contains(":inactive"));
        var root = Matrix.Part<Border>(other, "PART_Root");

        Assert.Equal(0, root.Opacity);
        Assert.False(root.IsHitTestVisible);
    });

    /// <summary>The day names: caption in text-secondary, 36px wide, 12px above the first week.</summary>
    [Fact]
    public Task TheDayNamesAreCaptionsInTextSecondary() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        var monthView = popover.GetVisualDescendants().OfType<Grid>().Single(g => g.Name == "PART_MonthView");
        // The DayTitleTemplate's TextBlocks, the only 16px-high text in the grid; a day's number is inside its button.
        var name = monthView.GetVisualDescendants().OfType<TextBlock>().First(t => t.Height == 16);

        Assert.Equal(12, name.FontSize);
        Assert.Equal(36, name.Width);
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(name.Foreground));
    });

    /// <summary>The header: ChevronLeft and ChevronRight icon buttons, 8px round the glyph and 6px out, either side of the month in body1.</summary>
    [Fact]
    public Task TheHeaderIsTheMonthBetweenTwoArrows() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        var buttons = popover.GetVisualDescendants().OfType<Button>().ToList();
        var previous = buttons.Single(b => b.Name == "PART_PreviousButton");
        var next = buttons.Single(b => b.Name == "PART_NextButton");
        var header = buttons.Single(b => b.Name == "PART_HeaderButton");

        Assert.Same(Application.Current!.FindResource("FiliNavigateBeforeGeometry"), previous.GetVisualDescendants().OfType<Path>().Single().Data);
        Assert.Same(Application.Current!.FindResource("FiliChevronRightGeometry"), next.GetVisualDescendants().OfType<Path>().Single().Data);
        Assert.Equal(new Thickness(8), previous.Padding);
        Assert.Equal(new Thickness(6), previous.Margin);
        Assert.Equal(16, header.FontSize);
    });

    /// <summary>A month is `.mud-picker-month`, 60px high in subtitle1; the selected one h5 in primary.</summary>
    [Fact]
    public Task TheMonthsAreSubtitlesAndTheSelectedOneIsH5Primary() => UiThread.RunAsync(() =>
    {
        var popover = Open(Show());
        popover.GetVisualDescendants().OfType<Calendar>().Single().DisplayMode = CalendarMode.Year;
        Dispatcher.UIThread.RunJobs();
        var months = popover.GetVisualDescendants().OfType<CalendarButton>().Where(b => b.IsEffectivelyVisible).ToList();
        var selected = months.Single(m => m.Classes.Contains(":selected"));
        var other = months.First(m => !m.Classes.Contains(":selected"));

        Assert.Equal(60, other.Bounds.Height);
        Assert.Equal(16, other.FontSize);
        Assert.Equal(24, selected.FontSize);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(selected.Foreground));
    });

    /// <summary>The picker's own control theme leaves every other text field as it was.</summary>
    [Fact]
    public Task APlainTextFieldIsUnchanged() => UiThread.RunAsync(() =>
    {
        var field = Matrix.Show(new TextBox { Width = 260, PlaceholderText = "Name" });

        Assert.Null(field.InnerRightContent);
        Assert.DoesNotContain(":end-adornment", field.Classes);
    });
}
