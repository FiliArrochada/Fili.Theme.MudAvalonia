using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudAutocomplete's adornment (MudAutocomplete.razor.cs): Adornment.End by default, OpenIcon
/// ArrowDropDown and CloseIcon ArrowDropUp, in the MudIconButton MudInputAdornment draws for a
/// clickable adornment - Edge.End, Color.Default, tabindex -1 - whose click toggles the list, the
/// whole of it from an empty field, MinCharacters being 0.
/// </summary>
public class AutocompleteArrowTests
{
    private static readonly string[] States = ["Alabama", "Alaska", "Arizona"];

    private static AutoCompleteBox Show(string? text = null, params string[] classes)
    {
        var box = new AutoCompleteBox { Width = 220, PlaceholderText = "US state", Text = text, ItemsSource = States };
        box.Classes.AddRange(classes);
        return Matrix.Show(box);
    }

    private static Button Arrow(AutoCompleteBox box) => Assert.IsType<Button>(box.InnerRightContent);

    private static TextBox Field(AutoCompleteBox box) => Matrix.Part<TextBox>(box, "PART_TextBox");

    /// <summary>The glyph on show: ArrowDropDown's key or ArrowDropUp's.</summary>
    private static PathIcon Glyph(AutoCompleteBox box) =>
        Assert.Single(((Panel)Arrow(box).Content!).Children.OfType<PathIcon>(), i => i.IsVisible);

    private static void Click(Control target)
    {
        var window = (Window)TopLevel.GetTopLevel(target)!;
        var centre = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public Task EveryFieldHasItsOwnArrowInItsField() => UiThread.RunAsync(() =>
    {
        var box = Show();
        var arrow = Arrow(box);

        Assert.NotSame(arrow, Arrow(Show()));
        Assert.Equal(["icon"], arrow.Classes.Where(c => !c.StartsWith(':')));
        Assert.Same(arrow, Field(box).InnerRightContent);
        Assert.Same(box.FindResource("FiliArrowDropDownGeometry"), Glyph(box).Data);
    });

    /// <summary>
    /// Edge.End: the 48px circle reaches 12px past the field's end. Color.Default: action-default.
    /// tabindex -1: not a Tab stop. And no name to give: raw to automation, the AutoCompleteBox's
    /// own peer being the one that expands and collapses.
    /// </summary>
    [Fact]
    public Task ItIsAnActionDefaultEdgeEndIconButton() => UiThread.RunAsync(() =>
    {
        var box = Show();
        var arrow = Arrow(box);
        var field = Field(box);
        var right = arrow.TranslatePoint(new Point(arrow.Bounds.Width, 0), field)!.Value.X;

        Assert.Equal(new Size(48, 48), arrow.Bounds.Size);
        Assert.Equal(field.Bounds.Width + 12, right);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(Glyph(box).Foreground));
        Assert.False(arrow.Focusable);
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(arrow));
    });

    /// <summary>
    /// `.mud-select-input .mud-input-adornment-end { margin-left: 0 }`: in every variant an
    /// autocomplete's end adornment has no 8px before it, where a text field's keeps it.
    /// </summary>
    [Theory]
    [InlineData("standard")]
    [InlineData("filled")]
    [InlineData("outlined")]
    public Task NoGapBeforeTheAdornment(string variant) => UiThread.RunAsync(() =>
    {
        var classes = variant == "standard" ? [] : new[] { variant };
        var presenter = Matrix.Part<ContentPresenter>(Field(Show(null, classes)), "PART_InnerRightContent");

        var text = new TextBox { Width = 220, InnerRightContent = new Border { Width = 24 } };
        text.Classes.AddRange(classes);
        Matrix.Show(text);
        var textPresenter = Matrix.Part<ContentPresenter>(text, "PART_InnerRightContent");

        Assert.Equal(0, presenter.Margin.Left);
        Assert.Equal(8, textPresenter.Margin.Left);
        Assert.Equal(textPresenter.Margin.Top, presenter.Margin.Top);
    });

    /// <summary>
    /// A click on an empty field opens the whole list, MinCharacters being 0, and the arrow turns;
    /// a click on the open list's arrow closes it.
    /// </summary>
    [Fact]
    public Task AClickOnAnEmptyFieldOpensTheWholeList() => UiThread.RunAsync(() =>
    {
        var box = Show();
        Assert.Equal(0, box.MinimumPrefixLength);

        Click(Arrow(box));
        Assert.True(box.IsDropDownOpen);
        var list = Matrix.Part<Popup>(box, "PART_Popup").Child!.GetLogicalDescendants().OfType<ListBox>().Single();
        Assert.Equal(States.Length, list.ItemCount);
        Assert.Same(box.FindResource("FiliArrowDropUpGeometry"), Glyph(box).Data);

        Click(Arrow(box));
        Assert.False(box.IsDropDownOpen);
        Assert.Same(box.FindResource("FiliArrowDropDownGeometry"), Glyph(box).Data);
    });

    /// <summary>
    /// An app's MinimumPrefixLength of 1 refuses to open an empty field: the click does nothing,
    /// and the arrow still says closed, never ArrowDropUp over no list.
    /// </summary>
    [Fact]
    public Task TheArrowNeverSaysOpenOverAClosedList() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new AutoCompleteBox { Width = 220, ItemsSource = States, MinimumPrefixLength = 1 });

        Click(Arrow(box));

        Assert.False(box.IsDropDownOpen);
        Assert.Same(box.FindResource("FiliArrowDropDownGeometry"), Glyph(box).Data);
    });

    /// <summary>`.mud-input.mud-disabled > .mud-input-adornment`: text-disabled, as the select's arrow.</summary>
    [Fact]
    public Task DisabledItIsTextDisabled() => UiThread.RunAsync(() =>
    {
        var box = Show();
        box.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Glyph(box).Foreground));
    });

    /// <summary>AdornmentIcon with the toggle kept: the glyph's key in the field's Resources.</summary>
    [Fact]
    public Task TheGlyphsKeyChangesTheIconAndKeepsTheToggle() => UiThread.RunAsync(() =>
    {
        var heart = Geometry.Parse("M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z");
        var box = new AutoCompleteBox { Width = 220, ItemsSource = States };
        box.Resources["FiliArrowDropDownGeometry"] = heart;
        Matrix.Show(box);

        Assert.Same(heart, Glyph(box).Data);
        Click(Arrow(box));
        Assert.True(box.IsDropDownOpen);
    });

    /// <summary>
    /// The app's own InnerRightContent replaces the arrow; null is Adornment.None, with no
    /// adornment gap; and clearing the value, not setting null, brings the arrow back.
    /// </summary>
    [Fact]
    public Task AnAppsOwnAdornmentReplacesIt() => UiThread.RunAsync(() =>
    {
        var own = new PathIcon { Classes = { "end-icon" } };
        var replaced = Matrix.Show(new AutoCompleteBox { Width = 220, InnerRightContent = own });
        var none = Matrix.Show(new AutoCompleteBox { Width = 220, InnerRightContent = null });

        Assert.Same(own, Field(replaced).InnerRightContent);
        Assert.Null(Field(none).InnerRightContent);
        Assert.DoesNotContain(":end-adornment", Field(none).Classes);

        replaced.ClearValue(AutoCompleteBox.InnerRightContentProperty);
        Dispatcher.UIThread.RunJobs();
        Assert.IsType<Button>(Field(replaced).InnerRightContent);
    });

    /// <summary>
    /// Clearable and the arrow together, as MudAutocomplete draws them: the clear button left of
    /// the arrow, `mud-icon-button-edge-end` pulling it 3px under the adornment, which has no
    /// margin of its own to keep them apart.
    /// </summary>
    [Fact]
    public Task TheClearButtonSitsLeftOfTheArrow() => UiThread.RunAsync(() =>
    {
        var box = Show("Ala", "clearable");
        var clear = Matrix.Part<Button>(Field(box), "PART_ClearButton");
        var arrow = Arrow(box);

        Assert.True(clear.IsEffectivelyVisible);
        Assert.Equal(arrow.TranslatePoint(default, box)!.Value.X + 3,
                     clear.TranslatePoint(new Point(clear.Bounds.Width, 0), box)!.Value.X);
    });

    /// <summary>The date picker's button is the same icon: raw too, its picker's peer expanding.</summary>
    [Fact]
    public Task TheDatePickersButtonIsRawToo() => UiThread.RunAsync(() =>
    {
        var picker = Matrix.Show(new CalendarDatePicker { Width = 220 });
        var button = Matrix.Part<Button>(picker, "PART_Button");

        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(button));
    });
}
