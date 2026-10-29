using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

public partial class CodeBlock : UserControl
{
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<CodeBlock, string?>(nameof(Code));

    private readonly SelectableTextBlock _text;

    public CodeBlock()
    {
        AvaloniaXamlLoader.Load(this);
        _text = this.FindControl<SelectableTextBlock>("Text")!;
        this.FindControl<Button>("Copy")!.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && Code is { } code)
            {
                // An async void handler: an exception here would end the app. A browser can
                // refuse the clipboard (no permission, the page not focused), and a copy that
                // fails leaves the code on the page to select by hand.
                try
                {
                    await clipboard.SetTextAsync(code);
                }
                catch (Exception)
                {
                }
            }
        };
    }

    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CodeProperty)
        {
            _text.Text = Code;
        }
    }
}
