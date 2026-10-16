using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// No template binding logs an error. Avalonia logs a failed binding as a warning and carries on,
/// so nothing here fails on its own; but an app that treats a binding error as a bug - Fili.Damselfly
/// fails its view tests on one - inherits every one this theme makes, on every page. The first
/// found that way: the text field's counter bound <c>Text.Length</c>, which warned on every
/// <c>TextBox</c> whose <c>Text</c> was still null, counter or not.
/// </summary>
public class BindingErrorTests
{
    public sealed record Row(string Name, bool Picked);

    /// <summary>The controls apps show most, as they first appear: empty, unset, unclassed.</summary>
    public static readonly TheoryData<string> Controls =
    [
        "TextBox", "TextBox.filled", "TextBox.outlined", "TextBox.counter", "TextBox.dense",
        "ComboBox", "ComboBox.filled", "AutoCompleteBox", "NumericUpDown",
        "CheckBox", "RadioButton", "ToggleSwitch", "ToggleButton", "ToggleButton.icon",
        "Button", "Button.filled", "Button.chip", "Button.fab", "SplitButton", "DropDownButton",
        "Slider", "ProgressBar", "ProgressBar.circular", "Expander", "ListBox", "TreeView",
        "TabControl", "Menu", "HeaderedContentControl.badge", "PipsPager", "DataGrid",
    ];

    private static Control Create(string spec)
    {
        var parts = spec.Split('.');
        Control control = parts[0] switch
        {
            "TextBox" => new TextBox { Width = 200, PlaceholderText = "Label" },
            "ComboBox" => new ComboBox { Width = 200, PlaceholderText = "Label", ItemsSource = new[] { "One", "Two" } },
            "AutoCompleteBox" => new AutoCompleteBox { Width = 200, ItemsSource = new[] { "One", "Two" } },
            "NumericUpDown" => new NumericUpDown { Width = 200 },
            "CheckBox" => new CheckBox { Content = "Check" },
            "RadioButton" => new RadioButton { Content = "Radio" },
            "ToggleSwitch" => new ToggleSwitch(),
            "ToggleButton" => new ToggleButton { Content = "Toggle" },
            "Button" => new Button { Content = "Button" },
            "SplitButton" => new SplitButton { Content = "Split" },
            "DropDownButton" => new DropDownButton { Content = "Drop" },
            "Slider" => new Slider { Width = 200 },
            "ProgressBar" => new ProgressBar { Width = 200 },
            "Expander" => new Expander { Header = "Panel", Content = "Body" },
            "ListBox" => new ListBox { ItemsSource = new[] { "One", "Two" } },
            "TreeView" => new TreeView { ItemsSource = new[] { "One", "Two" } },
            "TabControl" => new TabControl { Items = { new TabItem { Header = "One" }, new TabItem { Header = "Two" } } },
            "Menu" => new Menu { Items = { new MenuItem { Header = "File" } } },
            "HeaderedContentControl" => new HeaderedContentControl { Header = 3, Content = "Inbox" },
            "PipsPager" => new PipsPager { NumberOfPages = 5 },
            "DataGrid" => new DataGrid
            {
                Width = 300,
                ItemsSource = new AvaloniaList<Row> { new("One", true), new("Two", false) },
                Columns =
                {
                    new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Row.Name)) },
                    new DataGridCheckBoxColumn { Header = "Picked", Binding = new Binding(nameof(Row.Picked)) },
                },
            },
            _ => throw new ArgumentException(spec),
        };
        control.Classes.AddRange(parts.Skip(1));
        return control;
    }

    [Theory]
    [MemberData(nameof(Controls))]
    public Task NoTemplateBindingFails(string spec) => UiThread.RunAsync(() =>
    {
        var sink = new BindingSink();
        var previous = Logger.Sink;
        Logger.Sink = sink;
        try
        {
            Matrix.Show(Create(spec));
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            Logger.Sink = previous;
        }

        Assert.True(sink.Errors.Count == 0, $"{spec}:\n  " + string.Join("\n  ", sink.Errors));
    });

    private sealed class BindingSink : ILogSink
    {
        public List<string> Errors { get; } = [];

        public bool IsEnabled(LogEventLevel level, string area) => level >= LogEventLevel.Warning && area == LogArea.Binding;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate) =>
            Log(level, area, source, messageTemplate, []);

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            if (IsEnabled(level, area))
            {
                Errors.Add($"{source?.GetType().Name}: {messageTemplate} [{string.Join(", ", propertyValues)}]");
            }
        }
    }
}
