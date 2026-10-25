using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Fili.Theme.MudAvalonia.Gallery.Docs.Pages;

public partial class ParityPage : UserControl
{
    public ParityPage()
    {
        AvaloniaXamlLoader.Load(this);
        this.FindControl<ItemsControl>("Rows")!.ItemsSource = ComponentIndex.All;
    }
}
