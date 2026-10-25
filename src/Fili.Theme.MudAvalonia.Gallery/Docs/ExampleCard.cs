using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>
/// One live example on a docs page, as mudblazor.com shows them: a title and a line on what it
/// shows, the example itself, and its XAML behind a code button. The example is the card's
/// content; <see cref="Source"/> names the file it came from under Docs/Examples, whose text is
/// embedded, so the code shown is the code that runs. DocsTheme.axaml draws it.
/// </summary>
public sealed class ExampleCard : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ExampleCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ExampleCard, string?>(nameof(Description));

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<ExampleCard, string?>(nameof(Source));

    public static readonly DirectProperty<ExampleCard, string?> CodeProperty =
        AvaloniaProperty.RegisterDirect<ExampleCard, string?>(nameof(Code), o => o.Code);

    private string? _code;

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The example's path under Docs/Examples, without the extension: "Button/Variants".</summary>
    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>The XAML inside the example file's root element, as the page shows it.</summary>
    public string? Code
    {
        get => _code;
        private set => SetAndRaise(CodeProperty, ref _code, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty && Source is { } source)
        {
            // The example is the file itself, compiled and loaded by its path, so the page names
            // it once and shows exactly what it loaded.
            if (Content is null)
            {
                var example = AvaloniaXamlLoader.Load(new Uri($"avares://Fili.Theme.MudAvalonia.Gallery/Docs/Examples/{source}.axaml"));

                // A UserControl clips to its bounds, and an outlined field's floated label sits
                // 6px above the field: an example drawn by the theme is not cut at its own edge.
                if (example is Control control)
                {
                    control.ClipToBounds = false;
                }

                Content = example;
            }

            Code = Inner(Read(source));
        }
    }

    /// <summary>The embedded text of Docs/Examples/<paramref name="source"/>.axaml.</summary>
    public static string Read(string source)
    {
        var assembly = typeof(ExampleCard).Assembly;
        var wanted = $"Examples/{source}.axaml";
        var name = assembly.GetManifestResourceNames().Single(n => n.Replace('\\', '/') == wanted);
        using var reader = new StreamReader(assembly.GetManifestResourceStream(name)!);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// What a reader writes: the root element's content, without the UserControl wrapper and its
    /// namespaces, de-indented to the left margin.
    /// </summary>
    public static string Inner(string xaml)
    {
        var text = xaml.Replace("\r\n", "\n");
        var open = text.IndexOf('>', text.IndexOf("<UserControl", StringComparison.Ordinal)) + 1;
        var close = text.LastIndexOf("</UserControl>", StringComparison.Ordinal);
        var lines = text[open..close].Split('\n').ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        var indent = lines.Where(l => l.Trim().Length > 0).Select(l => l.Length - l.TrimStart().Length).DefaultIfEmpty(0).Min();
        return string.Join('\n', lines.Select(l => l.Length >= indent ? l[indent..] : l.TrimStart()));
    }
}
