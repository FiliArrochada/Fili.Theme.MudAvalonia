using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudInputControl's helper line - helper text, the error in its place, and the counter - plus
/// Margin.Dense and Variant on a select. Values from _inputcontrol.scss, _input.scss and
/// _inputlabel.scss; the counter's text is MudTextField.GetCounterText.
/// </summary>
public class FieldDetailTests
{
    private static TextBlock Block(Control field, string name) =>
        field.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Name == name);

    private static T Named<T>(Control field, string name) where T : Control =>
        field.GetVisualDescendants().OfType<T>().Single(t => t.Name == name);

    /// <summary>The visible texts under the field, in the helper line.</summary>
    private static List<string?> HelperTexts(Control field) =>
        Named<Control>(field, "PART_HelperLine").GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible)
            .Select(t => t.Text)
            .ToList();

    // --------------------------------------------------------------------------------------
    // Helper text
    // --------------------------------------------------------------------------------------

    [Fact]
    public Task HelperTextIsCaptionTextSecondaryUnderTheField() => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 220, PlaceholderText = "Email" };
        AutomationProperties.SetHelpText(box, "We never share it");
        Matrix.Show(box);

        var helper = Block(box, "PART_HelperText");
        Assert.True(helper.IsEffectivelyVisible);
        Assert.Equal("We never share it", helper.Text);
        Assert.Equal(12, helper.FontSize);
        Assert.Equal(20, helper.LineHeight);
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(helper.Foreground));
        Assert.Equal(3, helper.Margin.Top);

        // Under the field, not beside or over it.
        var root = Named<Border>(box, "PART_Root");
        Assert.True(helper.TranslatePoint(default, box)!.Value.Y >= root.Bounds.Height);
    });

    /// <summary>An empty helper line takes no room: a plain field is exactly as tall as before.</summary>
    [Fact]
    public Task NoHelperTakesNoRoom() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 220, PlaceholderText = "Email" });

        Assert.Equal(Named<Border>(box, "PART_Root").Bounds.Height, box.Bounds.Height);
        Assert.Empty(HelperTexts(box));
    });

    /// <summary>
    /// The line is built only on a field with a use for it, and comes and goes with helper text or
    /// an error set on a field already shown: a plain field has nothing under it at all. Once
    /// gone, the line is hidden and takes no room; a presenter swaps its child when next measured.
    /// </summary>
    [Fact]
    public Task TheLineIsBuiltOnlyWhenNeeded() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 220, PlaceholderText = "Email" });
        var line = Named<Control>(box, "PART_HelperLine");
        Assert.Empty(line.GetVisualDescendants());

        DataValidationErrors.SetError(box, new InvalidOperationException("Must be an email"));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(["Must be an email"], HelperTexts(box));

        DataValidationErrors.ClearErrors(box);
        Dispatcher.UIThread.RunJobs();
        Assert.False(line.IsVisible);
        Assert.Equal(Named<Border>(box, "PART_Root").Bounds.Height, box.Bounds.Height);

        AutomationProperties.SetHelpText(box, "We never share it");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(["We never share it"], HelperTexts(box));

        AutomationProperties.SetHelpText(box, null);
        Dispatcher.UIThread.RunJobs();
        Assert.False(line.IsVisible);
        Assert.Equal(Named<Border>(box, "PART_Root").Bounds.Height, box.Bounds.Height);
    });

    /// <summary>MudInputControl: the error takes the helper text's place, and the row goes red.</summary>
    [Fact]
    public Task AnErrorReplacesTheHelperText() => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 220, PlaceholderText = "Email", Classes = { "counter" }, MaxLength = 50, Text = "abc" };
        AutomationProperties.SetHelpText(box, "We never share it");
        DataValidationErrors.SetError(box, new InvalidOperationException("Must be an email"));
        Matrix.Show(box);

        var texts = HelperTexts(box);
        Assert.Contains("Must be an email", texts);
        Assert.DoesNotContain("We never share it", texts);
        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(Block(box, "PART_CounterLimit").Foreground));
    });

    [Fact]
    public Task TheErrorClassHidesTheHelperTextToo() => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 220, Classes = { "error" } };
        AutomationProperties.SetHelpText(box, "We never share it");
        Matrix.Show(box);

        Assert.False(Block(box, "PART_HelperText").IsEffectivelyVisible);
    });

    /// <summary>
    /// HelperTextOnFocus: the help keeps its room and shows only while the field has focus, so
    /// focusing a field never pushes the layout below it down.
    /// </summary>
    [Fact]
    public Task HelperOnFocusShowsOnlyWhileFocused() => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 220, Classes = { "helper-on-focus" } };
        AutomationProperties.SetHelpText(box, "Shown while focused");
        Matrix.Show(box);
        var helper = Block(box, "PART_HelperText");
        var height = box.Bounds.Height;

        Assert.Equal(0, helper.Opacity);
        Assert.True(helper.IsEffectivelyVisible, "The help keeps its room while hidden.");

        box.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, helper.Opacity);
        Assert.Equal(height, box.Bounds.Height);
    });

    [Theory]
    [InlineData("filled", 4)]
    [InlineData("outlined", 8)]
    public Task TheHelperLineIsInsetUnderFilledAndOutlinedFields(string variant, double inset) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 220, Classes = { variant } };
        AutomationProperties.SetHelpText(box, "Help");
        Matrix.Show(box);

        Assert.Equal(new Thickness(inset, 0), Named<Control>(box, "PART_HelperLine").Margin);
    });

    // --------------------------------------------------------------------------------------
    // Counter
    // --------------------------------------------------------------------------------------

    [Fact]
    public Task TheCounterCountsAgainstMaxLength() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 220, Classes = { "counter" }, MaxLength = 50, Text = "hello" });

        Assert.Equal(["5", " / 50"], HelperTexts(box));

        box.Text = "hello there";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(["11", " / 50"], HelperTexts(box));
    });

    /// <summary>MudBlazor's Counter="0": the length alone. Here, a counter with no MaxLength.</summary>
    [Fact]
    public Task WithoutMaxLengthTheCounterIsTheLengthAlone() => UiThread.RunAsync(() =>
    {
        var empty = Matrix.Show(new TextBox { Width = 220, Classes = { "counter" } });
        Assert.Equal(["0"], HelperTexts(empty));

        var typed = Matrix.Show(new TextBox { Width = 220, Classes = { "counter" }, Text = "abc" });
        Assert.Equal(["3"], HelperTexts(typed));
    });

    [Fact]
    public Task NoCounterWithoutTheClass() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 220, MaxLength = 50, Text = "hello" });

        Assert.Empty(HelperTexts(box));
    });

    // --------------------------------------------------------------------------------------
    // Margin.Dense
    // --------------------------------------------------------------------------------------

    /// <summary>
    /// The dense deltas, not absolute heights: a standard field is its 48px minimum and a
    /// filled or outlined one 56px, so only the change is MudBlazor's - 3px off the standard
    /// field, 4px top and bottom off filled, 8px top and bottom off outlined.
    /// </summary>
    [Theory]
    [InlineData(null, 3)]
    [InlineData("filled", 8)]
    [InlineData("outlined", 16)]
    public Task DenseFieldsAreShorter(string? variant, double delta) => UiThread.RunAsync(() =>
    {
        TextBox Field(params string[] extra)
        {
            var box = new TextBox { Width = 220, PlaceholderText = "Label" };
            if (variant is not null) box.Classes.Add(variant);
            box.Classes.AddRange(extra);
            return Matrix.Show(box);
        }

        ComboBox Select(params string[] extra)
        {
            var combo = new ComboBox { Width = 220, PlaceholderText = "Label" };
            combo.Items.Add(new ComboBoxItem { Content = "One" });
            if (variant is not null) combo.Classes.Add(variant);
            combo.Classes.AddRange(extra);
            return Matrix.Show(combo);
        }

        Assert.Equal(delta, Field().Bounds.Height - Field("dense").Bounds.Height, 1);
        Assert.Equal(delta, Select().Bounds.Height - Select("dense").Bounds.Height, 1);
    });

    /// <summary>
    /// _input.scss pads filled content 12px and outlined 14px from the side, label included.
    /// <para>
    /// Both shipped flush to the edge: the template wrote the content margin inline, which binds
    /// at Template priority, and the variants' plain <c>^ /template/</c> styles cannot beat that
    /// - while their <c>.dense</c> styles, carrying an activator, could. Asserting the property
    /// rather than eyeballing a frame is what catches it.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData(null, 0, 20)]
    [InlineData("filled", 12, 20)]
    [InlineData("outlined", 14, 16)]
    public Task VariantsPadTheirContentFromTheSide(string? variant, double side, double top) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 220, PlaceholderText = "Label" };
        var combo = new ComboBox { Width = 220, PlaceholderText = "Label" };
        if (variant is not null)
        {
            box.Classes.Add(variant);
            combo.Classes.Add(variant);
        }
        Matrix.Show(box);
        Matrix.Show(combo);

        Assert.Equal(side, Named<DockPanel>(box, "PART_ContentArea").Margin.Left);
        Assert.Equal(top, Named<DockPanel>(box, "PART_ContentArea").Margin.Top);
        Assert.Equal(side, Block(box, "PART_FloatingLabel").Margin.Left);
        Assert.Equal(side, Named<Grid>(combo, "PART_ContentArea").Margin.Left);
        Assert.Equal(top, Named<Grid>(combo, "PART_ContentArea").Margin.Top);
        Assert.Equal(side, Block(combo, "PART_FloatingLabel").Margin.Left);
    });

    /// <summary>
    /// The outlined label floats onto the stroke, 6px above the field's top edge as MudBlazor's
    /// translate(14px, -6px) puts it. Avalonia clips a TextBox and a ComboBox to their bounds by
    /// default, which cut the top off the label and its mask; the fields now do not clip, and the
    /// select's own content clips itself so a long item still cannot spill past the chevron.
    /// </summary>
    [Fact]
    public Task AFloatedOutlinedLabelIsNotClippedByItsField() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Width = 190, Classes = { "outlined" }, PlaceholderText = "Title", Text = "Content" });
        var combo = ShowSelect("outlined", selected: true);

        Assert.False(box.ClipToBounds);
        Assert.False(combo.ClipToBounds);
        Assert.True(Named<ContentPresenter>(combo, "PART_ContentPresenter").ClipToBounds);

        // The label really does reach above the field - which is why the clip mattered.
        var label = Block(box, "PART_FloatingLabel");
        var top = label.TranslatePoint(default, box)!.Value.Y;
        Assert.True(top < 0, $"The floated label's top is at {top}, inside the field.");
    });

    /// <summary>
    /// A start adornment floats the label, as a value does: <c>MudInput.razor.cs</c> shrinks it
    /// on <c>Adornment == Adornment.Start</c>. A resting label lay over the adornment instead -
    /// "Search games" written across the magnifier. Foreground, not FontSize, because the size is
    /// transitioned and the colour is not: a floated label is text-primary, a resting one
    /// text-secondary.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("filled")]
    [InlineData("outlined")]
    [InlineData("dense")]
    public Task AStartAdornmentFloatsTheLabel(string? variant) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, PlaceholderText = "Search", InnerLeftContent = new TextBlock { Text = "$" } };
        if (variant is not null) box.Classes.Add(variant);
        Matrix.Show(box);
        var label = Block(box, "PART_FloatingLabel");

        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(label.Foreground));

        box.InnerLeftContent = null;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(label.Foreground));

        box.InnerRightContent = new TextBlock { Text = "kg" };
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(Matrix.Token("FiliTextSecondaryColor"), Matrix.Colour(label.Foreground));
    });

    /// <summary>
    /// A tall field's label stays at the top, floated or not, as a textarea's does in MudBlazor.
    /// The label was centred on the whole field and floated a fixed distance up from there, so in
    /// a four-line field it landed on the text - on an outlined one, its mask hid the first line.
    /// It now centres in a band of the single-line field's height, at the top, and the first line
    /// starts where a single-line field's text does, so the label and the text are as far apart as
    /// in a single-line field: flush with the top, a filled field's first line met its label.
    /// </summary>
    [Theory]
    [InlineData(null, 48, false)]
    [InlineData("dense", 45, false)]
    [InlineData("filled", 56, false)]
    [InlineData("filled", 56, true)]
    [InlineData("filled dense", 48, false)]
    [InlineData("outlined", 54, false)]
    [InlineData("outlined dense", 38, false)]
    public Task ATallFieldsLabelStaysAtTheTop(string? classes, double band, bool sizedByTheApp) => UiThread.RunAsync(() =>
    {
        TextBox Field(bool multiLine)
        {
            var box = new TextBox { Width = 200, PlaceholderText = "Notes", Text = "line 1" };
            if (multiLine)
            {
                box.AcceptsReturn = true;
                if (sizedByTheApp) box.Height = 120; else box.Text = "line 1\nline 2\nline 3\nline 4";
            }
            if (classes is not null) box.Classes.AddRange(classes.Split(' '));
            return box;
        }

        static double Gap(TextBox box)
        {
            var label = Block(box, "PART_FloatingLabel");
            var labelBottom = label.TranslatePoint(new Point(0, label.Bounds.Height), box)!.Value.Y;
            var textTop = Named<TextPresenter>(box, "PART_TextPresenter").TranslatePoint(default, box)!.Value.Y;
            return textTop - labelBottom;
        }

        var single = Field(multiLine: false);
        var tall = Field(multiLine: true);
        Matrix.Show(new StackPanel { Children = { single, tall } });

        var area = Named<Panel>(tall, "PART_LabelArea");
        Assert.Equal(band, area.Bounds.Height);
        Assert.Equal(0, area.Bounds.Y);
        Assert.True(tall.Bounds.Height > band + 40, $"The field is {tall.Bounds.Height} high, not multi-line.");
        Assert.True(Math.Abs(Gap(tall) - Gap(single)) <= 1,
            $"The label and the first line are {Gap(tall)} apart; in a single-line field, {Gap(single)}.");
    });

    /// <summary>
    /// The label's band never makes a single-line field taller, focused or not. Sized to the
    /// field's height, it made an outlined field 2px taller: the band sits inside the stroke.
    /// </summary>
    [Theory]
    [InlineData(null, 48)]
    [InlineData("dense", 45)]
    [InlineData("filled", 56)]
    [InlineData("filled dense", 48)]
    [InlineData("outlined", 56)]
    [InlineData("outlined dense", 40)]
    public Task TheLabelsBandLeavesTheFieldsHeight(string? classes, double height) => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, PlaceholderText = "Title" };
        if (classes is not null) box.Classes.AddRange(classes.Split(' '));
        var other = new TextBox();
        Matrix.Show(new StackPanel { Children = { box, other } });

        Assert.Equal(height, Named<Border>(box, "PART_Root").Bounds.Height);
        box.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(height, Named<Border>(box, "PART_Root").Bounds.Height);
    });

    // --------------------------------------------------------------------------------------
    // Variant on a select
    // --------------------------------------------------------------------------------------

    private static ComboBox ShowSelect(string? cls, bool selected)
    {
        var combo = new ComboBox { Width = 220, PlaceholderText = "Label" };
        combo.Items.Add(new ComboBoxItem { Content = "One" });
        if (cls is not null) combo.Classes.Add(cls);
        if (selected) combo.SelectedIndex = 0;
        return Matrix.Show(combo);
    }

    [Fact]
    public Task AFilledSelectIsAFilledField() => UiThread.RunAsync(() =>
    {
        var combo = ShowSelect("filled", selected: true);
        var root = Named<Border>(combo, "PART_Root");

        Assert.Equal(Matrix.Token("FiliInputFilledColor"), Matrix.Colour(root.Background));
        Assert.Equal(new CornerRadius(4, 4, 0, 0), root.CornerRadius);
        Assert.Equal(new Thickness(12, 0), Block(combo, "PART_SelectedLabel").Margin);
        Assert.Equal(-10, ((TransformOperations)Block(combo, "PART_SelectedLabel").RenderTransform!).Value.M32);
    });

    /// <summary>
    /// The selected label's resting position is written INLINE in the template, which a plain
    /// style cannot override - so this is the check that the outlined theme actually moves it
    /// onto the stroke, and masks it there.
    /// </summary>
    [Fact]
    public Task AnOutlinedSelectIsBoxedWithItsLabelOnTheStroke() => UiThread.RunAsync(() =>
    {
        var combo = ShowSelect("outlined", selected: true);
        var root = Named<Border>(combo, "PART_Root");
        var label = Block(combo, "PART_SelectedLabel");

        Assert.Equal(Matrix.Token("FiliLinesInputsColor"), Matrix.Colour(root.BorderBrush));
        Assert.Equal(new Thickness(1), root.BorderThickness);
        Assert.False(Named<Border>(combo, "PART_Underline").IsVisible);
        Assert.Equal(-26, ((TransformOperations)label.RenderTransform!).Value.M32);
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(label.Background));
    });

    [Fact]
    public Task ADenseOutlinedSelectFloatsLower() => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 220, PlaceholderText = "Label", Classes = { "outlined", "dense" } };
        combo.Items.Add(new ComboBoxItem { Content = "One" });
        combo.SelectedIndex = 0;
        Matrix.Show(combo);

        Assert.Equal(-18, ((TransformOperations)Block(combo, "PART_SelectedLabel").RenderTransform!).Value.M32);
    });

    /// <summary>The text the closed box presents.</summary>
    private static string? ClosedBoxText(ComboBox combo) =>
        Named<ContentPresenter>(combo, "PART_ContentPresenter")
            .GetVisualDescendants().OfType<TextBlock>().Single().Text;

    private static ComboBox ShowTemplatedSelect(bool withSelectionBoxTemplate)
    {
        var combo = new ComboBox
        {
            Width = 220,
            ItemsSource = new[] { "One" },
            ItemTemplate = new FuncDataTemplate<string>((s, _) => new TextBlock { Text = "list " + s }),
        };
        if (withSelectionBoxTemplate)
            combo.SelectionBoxItemTemplate = new FuncDataTemplate<string>((s, _) => new TextBlock { Text = "box " + s });
        combo.SelectedIndex = 0;
        return Matrix.Show(combo);
    }

    /// <summary>
    /// The closed box draws the selection with SelectionBoxItemTemplate, as the base's does. It drew
    /// it with ItemTemplate, so an app's closed-box template was silently ignored, and whatever the
    /// list's template set - a colour meant for the popup's surface - reached the closed box too.
    /// </summary>
    [Fact]
    public Task TheClosedBoxUsesTheSelectionBoxItemTemplate() => UiThread.RunAsync(() =>
        Assert.Equal("box One", ClosedBoxText(ShowTemplatedSelect(withSelectionBoxTemplate: true))));

    /// <summary>With no SelectionBoxItemTemplate the closed box falls back to ItemTemplate.</summary>
    [Fact]
    public Task TheClosedBoxFallsBackToTheItemTemplate() => UiThread.RunAsync(() =>
        Assert.Equal("list One", ClosedBoxText(ShowTemplatedSelect(withSelectionBoxTemplate: false))));

    /// <summary>A select used to have no error host at all: a failing binding showed nothing.</summary>
    [Fact]
    public Task AnInvalidSelectShowsItsErrorAndRedRule() => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 220, PlaceholderText = "Library" };
        combo.Items.Add(new ComboBoxItem { Content = "One" });
        AutomationProperties.SetHelpText(combo, "Where new files go");
        DataValidationErrors.SetError(combo, new InvalidOperationException("Pick a library"));
        Matrix.Show(combo);

        var texts = HelperTexts(combo);
        Assert.Contains("Pick a library", texts);
        Assert.DoesNotContain("Where new files go", texts);
        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(Named<Border>(combo, "PART_Underline").Background));
    });

    [Fact]
    public Task ASelectShowsHelperText() => UiThread.RunAsync(() =>
    {
        var combo = new ComboBox { Width = 220, PlaceholderText = "Library" };
        AutomationProperties.SetHelpText(combo, "Where new files go");
        Matrix.Show(combo);

        Assert.Equal(["Where new files go"], HelperTexts(combo));
    });

    /// <summary>
    /// `.mud-input.mud-disabled`, its label and its helper text are text-disabled, not the paler
    /// action-disabled; the select is the same field.
    /// </summary>
    [Fact]
    public Task ADisabledFieldIsTextDisabled() => UiThread.RunAsync(() =>
    {
        var box = new TextBox { Width = 200, PlaceholderText = "Name", IsEnabled = false };
        AutomationProperties.SetHelpText(box, "Your full name");
        Matrix.Show(box);

        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(box.Foreground));
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Block(box, "PART_FloatingLabel").Foreground));
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Block(box, "PART_HelperText").Foreground));

        var combo = new ComboBox { Width = 200, PlaceholderText = "Platform", IsEnabled = false };
        Matrix.Show(combo);
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(combo.Foreground));
        Assert.Equal(Matrix.Token("FiliTextDisabledColor"), Matrix.Colour(Named<Avalonia.Controls.Shapes.Path>(combo, "PART_Chevron").Fill));
    });

    /// <summary>
    /// `.mud-disabled:before`: the line keeps lines-inputs and turns dotted, 1px on and 1px off,
    /// on a text field and a select alike. An enabled line is solid.
    /// </summary>
    [Fact]
    public Task ADisabledFieldsLineIsDotted() => UiThread.RunAsync(() =>
    {
        foreach (var field in new Control[] { new TextBox { Width = 200 }, new ComboBox { Width = 200 } })
        {
            Matrix.Show(field);
            var line = Named<Border>(field, "PART_Underline");
            Assert.Null(line.OpacityMask);

            field.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(Matrix.Token("FiliLinesInputsColor"), Matrix.Colour(line.Background));
            var mask = Assert.IsType<DrawingBrush>(line.OpacityMask);
            Assert.Equal(TileMode.Tile, mask.TileMode);
            Assert.Equal(new RelativeRect(0, 0, 2, 1, RelativeUnit.Absolute), mask.DestinationRect);
            var dots = Assert.IsType<DrawingGroup>(mask.Drawing).Children.OfType<GeometryDrawing>().ToList();
            Assert.Equal(new Rect(0, 0, 2, 1), dots[0].Geometry!.Bounds);
            Assert.Equal(new Rect(0, 0, 1, 1), Assert.Single(dots, d => d.Brush is ISolidColorBrush { Color.A: 255 }).Geometry!.Bounds);
        }
    });

    /// <summary>`.mud-tooltip`: 12px at weight 500.</summary>
    [Fact]
    public Task ATooltipIsMedium() => UiThread.RunAsync(() =>
    {
        var tip = new ToolTip { Content = "Save" };
        Matrix.Show(new Border { Child = tip });

        Assert.Equal(FontWeight.Medium, tip.FontWeight);
        Assert.Equal(12, tip.FontSize);
    });

    /// <summary>MudSelect's and MudAutocomplete's MaxHeight: 300, where Avalonia's drop-down stops at 200.</summary>
    [Fact]
    public Task TheDropDownsAre300High() => UiThread.RunAsync(() =>
    {
        Assert.Equal(300, Matrix.Show(new ComboBox { Width = 200 }).MaxDropDownHeight);
        Assert.Equal(300, Matrix.Show(new AutoCompleteBox { Width = 200 }).MaxDropDownHeight);
    });

    /// <summary>
    /// An outlined label floats onto the stroke, 6px above the field, so nothing between it and
    /// the control may clip: not the TextBox, and not a control wrapping one. NumericUpDown and
    /// CalendarDatePicker clip by default, which cut the label's top off.
    /// </summary>
    [Theory]
    [InlineData(typeof(TextBox))]
    [InlineData(typeof(NumericUpDown))]
    [InlineData(typeof(AutoCompleteBox))]
    [InlineData(typeof(CalendarDatePicker))]
    [InlineData(typeof(ComboBox))]
    public Task NothingClipsAnOutlinedLabel(Type type) => UiThread.RunAsync(() =>
    {
        var field = (Control)Activator.CreateInstance(type)!;
        field.Width = 220;
        field.Classes.Add("outlined");
        Matrix.Show(field);
        var label = Block(field, "PART_FloatingLabel");
        var clipping = label.GetVisualAncestors().TakeWhile(v => v != field).Append(field)
            .Where(v => v.ClipToBounds).Select(v => v.GetType().Name + " " + (v as Control)?.Name);

        Assert.Empty(clipping);
    });

    /// <summary>
    /// A control wrapping a text field takes the field's variants: outlined draws the 1px stroke
    /// and filled the input-filled background. A local Background or BorderThickness on the
    /// TextBox inside it beat both, so outlined and filled drew a bare field.
    /// </summary>
    [Theory]
    [InlineData(typeof(NumericUpDown))]
    [InlineData(typeof(AutoCompleteBox))]
    [InlineData(typeof(CalendarDatePicker))]
    public Task AWrappedFieldTakesTheVariants(Type type) => UiThread.RunAsync(() =>
    {
        Border Root(string variant)
        {
            var field = (Control)Activator.CreateInstance(type)!;
            field.Width = 220;
            field.Classes.Add(variant);
            Matrix.Show(field);
            return Matrix.Part<Border>(Matrix.Part<TextBox>(field, "PART_TextBox"), "PART_Root");
        }

        Assert.Equal(new Thickness(1), Root("outlined").BorderThickness);
        Assert.Equal(Matrix.Token("FiliInputFilledColor"), Matrix.Colour(Root("filled").Background));
    });

    /// <summary>
    /// Every control that is a text field - a select, a number field, an autocomplete, a date
    /// picker - is exactly a text field's height in each variant, so a row of them lines up. A
    /// select's 24px arrow made the standard one 3px taller, and a number field's two 28px spin
    /// buttons held every variant at 56px, `dense` included.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("dense")]
    [InlineData("outlined")]
    [InlineData("outlined dense")]
    [InlineData("filled")]
    [InlineData("filled dense")]
    public Task EveryFieldIsATextFieldsHeight(string variant) => UiThread.RunAsync(() =>
    {
        var classes = variant.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        T Field<T>(T field) where T : Control
        {
            field.Width = 220;
            field.Classes.AddRange(classes);
            return Matrix.Show(field);
        }

        var height = Field(new TextBox { PlaceholderText = "Label", Text = "Value" }).Bounds.Height;
        var select = new ComboBox { PlaceholderText = "Label" };
        select.Items.Add("One");
        select.SelectedIndex = 0;

        Assert.Equal(height, Field(select).Bounds.Height);
        Assert.Equal(height, Field(new NumericUpDown { PlaceholderText = "Label", Value = 4 }).Bounds.Height);
        Assert.Equal(height, Field(new AutoCompleteBox { PlaceholderText = "Label", Text = "Value" }).Bounds.Height);
        Assert.Equal(height, Field(new CalendarDatePicker { PlaceholderText = "Label", SelectedDate = new DateTime(2026, 10, 11) }).Bounds.Height);
    });
}
