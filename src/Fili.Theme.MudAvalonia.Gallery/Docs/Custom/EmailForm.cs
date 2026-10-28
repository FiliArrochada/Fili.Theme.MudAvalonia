using System.Collections;
using System.ComponentModel;

namespace Fili.Theme.MudAvalonia.Gallery.Docs.Custom;

/// <summary>
/// A view model whose Email fails validation, as MudTextField's Validation does: through
/// INotifyDataErrorInfo, which Avalonia's bindings read, so a field bound to Email paints as
/// <c>error</c> with the message under it, the app setting neither. It starts invalid, so the
/// failure shows before anything is typed; type a valid address and it clears.
/// </summary>
public sealed class EmailForm : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private string _email = "not-an-email";

    /// <summary>A form for one field. Each read is a new one, so two fields never share it.</summary>
    public static EmailForm Sample => new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public string Email
    {
        get => _email;
        set
        {
            if (_email == value)
            {
                return;
            }

            _email = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email)));
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Email)));
        }
    }

    public bool HasErrors => EmailErrors().Any();

    public IEnumerable GetErrors(string? propertyName) =>
        propertyName == nameof(Email) ? EmailErrors().ToList() : [];

    private IEnumerable<string> EmailErrors()
    {
        if (string.IsNullOrWhiteSpace(_email))
        {
            yield return "Email is required";
        }
        else if (!_email.Contains('@') || _email.StartsWith('@') || _email.EndsWith('@'))
        {
            yield return "Must be a valid email address";
        }
    }
}
