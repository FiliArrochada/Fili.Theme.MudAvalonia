using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Logging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery.Desktop;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// The unit suite's behaviour checks - no binding error, one Tab stop per control, access keys
/// recognised - run over every gallery page in every variant, rather than over a list of controls.
/// <para>
/// The list holds each control once, bare and unclassed, as it was thought of. The gallery holds
/// it in every class, state and combination a sample shows, and in all three variants, which is
/// where the bugs the list missed were: a menu row only themed in a flyout, a switch stretched
/// taller than its template. A check that is cheap per control is cheap per page too.
/// </para>
/// </summary>
public class GalleryAuditTests
{
    public static TheoryData<string> Frames
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var frame in GalleryFrames.All)
            {
                data.Add(frame.ToString());
            }

            return data;
        }
    }

    /// <summary>Opens a frame's view, logging every binding warning made while it builds and settles.</summary>
    private static (Window Window, List<string> BindingErrors) Open(string name)
    {
        var frame = GalleryFrames.All.Single(f => f.ToString() == name);
        Avalonia.Application.Current!.RequestedThemeVariant = frame.Variant;

        var sink = new BindingSink();
        var previous = Logger.Sink;
        Logger.Sink = sink;
        try
        {
            var window = new Window { Content = GalleryFrames.Build(frame.View), Width = 1180, Height = 800 };
            window.Show();

            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            return (window, sink.Errors);
        }
        finally
        {
            Logger.Sink = previous;
        }
    }

    [Theory]
    [MemberData(nameof(Frames))]
    public Task NoBindingFailsOnThePage(string name) => UiThread.RunAsync(() =>
    {
        var (window, errors) = Open(name);
        window.Close();

        Assert.True(errors.Count == 0, $"{name}, {errors.Count} binding errors:\n  " + string.Join("\n  ", errors.Distinct()));
    });

    /// <summary>
    /// Every templated control on the page is one Tab stop at most: itself, or the one part that
    /// stands for it. Disabled and hidden controls are skipped, as the keyboard skips them.
    /// <para>
    /// Two things that look like a second stop are not. A part marked
    /// <c>TemplatedControl.IsTemplateFocusTarget</c> is where the control's own focus lands - a
    /// tree row's selection border - so the two are one stop. And a pager's previous and next
    /// buttons are actions of their own, each a stop as MudPagination's buttons are.
    /// </para>
    /// </summary>
    [Theory]
    [MemberData(nameof(Frames))]
    public Task EveryControlOnThePageIsOneTabStopAtMost(string name) => UiThread.RunAsync(() =>
    {
        var (window, _) = Open(name);

        var extra = window.GetVisualDescendants().OfType<TemplatedControl>()
            .Where(c => c.IsEffectivelyVisible && c.IsEffectivelyEnabled)
            .Select(c => (Control: c, Stops: c.GetSelfAndVisualDescendants().OfType<InputElement>()
                .Where(e => e == c || (e.TemplatedParent == c && !(e is Control part && TemplatedControl.GetIsTemplateFocusTarget(part))))
                .Where(e => c is not PipsPager || e is not Button)
                .Where(e => e.Focusable && e.IsTabStop && e.IsEffectivelyVisible && e.IsEffectivelyEnabled)
                .ToList()))
            .Where(found => found.Stops.Count > 1)
            .Select(found => $"{Describe(found.Control)}: {string.Join(", ", found.Stops.Select(s => $"{s.GetType().Name}#{s.Name}"))}")
            .Distinct()
            .ToList();
        window.Close();

        Assert.True(extra.Count == 0, $"{name}, {extra.Count} controls with more than one Tab stop:\n  " + string.Join("\n  ", extra));
    });

    /// <summary>
    /// No control on the page shows an access key's underscore: a string it presents is drawn
    /// through an <see cref="AccessText"/>, which takes the underscore as the key and underlines
    /// its letter. Only presented text is checked - a TextBlock a presenter made for a string -
    /// since a caption's own underscores, a file name's, are just text.
    /// </summary>
    [Theory]
    [MemberData(nameof(Frames))]
    public Task NoTextShowsAnAccessKeysUnderscore(string name) => UiThread.RunAsync(() =>
    {
        var (window, _) = Open(name);

        var raw = window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t is not AccessText && t.IsEffectivelyVisible && t.Text?.Contains('_') == true)
            .Where(t => t.GetVisualParent() is ContentPresenter)
            .Select(t => $"\"{t.Text}\" in {Describe(t)}")
            .ToList();
        window.Close();

        Assert.True(raw.Count == 0, $"{name}, {raw.Count} texts drawn with their underscore:\n  " + string.Join("\n  ", raw));
    });

    /// <summary>A control, its classes, and the templated control that holds it.</summary>
    private static string Describe(Control control)
    {
        var owner = control.GetVisualAncestors().OfType<TemplatedControl>().FirstOrDefault();
        var classes = control.Classes.Where(c => !c.StartsWith(':')).ToList();

        return control.GetType().Name + (control.Name is { } n ? $"#{n}" : "")
            + (classes.Count > 0 ? "." + string.Join(".", classes) : "")
            + (owner is null ? "" : $" in {owner.GetType().Name}");
    }

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
