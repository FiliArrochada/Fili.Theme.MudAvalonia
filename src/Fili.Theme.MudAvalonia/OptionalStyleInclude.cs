using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// A <c>StyleInclude</c> for a control package this theme builds against but does not depend on:
/// the file loads only when the app has the package, at the version the theme was built against
/// or later.
/// <code>
/// &lt;theme:OptionalStyleInclude Requires="Avalonia.Controls.DataGrid" MinimumVersion="12.1.2"
///                             Source="avares://Fili.Theme.MudAvalonia/Themes/Optional/DataGrid/DataGridStyles.axaml" /&gt;
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// A plain <c>StyleInclude</c> cannot do this: the XAML compiler turns it into a direct call, so
/// the file loads, and asks for the package's assembly, in every app. This one loads by URI at
/// run time, after the check, and only files under <c>Themes/Optional/</c> may name the package's
/// types (<c>OptionalBoundaryTests</c>).
/// </para>
/// <para>
/// An older package is skipped with a warning rather than half themed: the theme's templates
/// name parts of the version it was built against. The app then shows the package's own look.
/// </para>
/// <para>
/// The constructor keeps the XAML service provider so the loaded file sees the theme above it,
/// which is what its <c>StaticResource</c>s resolve through.
/// </para>
/// </remarks>
internal sealed class OptionalStyleInclude : Styles, ISupportInitialize
{
    private readonly IServiceProvider? _serviceProvider;

    public OptionalStyleInclude(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>The simple name of the assembly the file needs.</summary>
    public string? Requires { get; set; }

    /// <summary>The oldest version of it the file works with: the one the theme is built against.</summary>
    public Version? MinimumVersion { get; set; }

    /// <summary>The file, an <c>avares://</c> URI whose root is <c>Styles</c>.</summary>
    public Uri? Source { get; set; }

    void ISupportInitialize.BeginInit()
    {
    }

    void ISupportInitialize.EndInit()
    {
        if (Requires is null || Source is null)
        {
            throw new InvalidOperationException($"{nameof(OptionalStyleInclude)} needs both {nameof(Requires)} and {nameof(Source)}.");
        }

        var version = FindVersion(Requires);
        if (version is null)
        {
            return;
        }

        if (MinimumVersion is not null && version < MinimumVersion)
        {
            Logger.TryGet(LogEventLevel.Warning, LogArea.Visual)?.Log(
                this,
                "Fili.Theme.MudAvalonia: {Assembly} {Version} is older than {Minimum}, the version this theme is built against; its controls keep their own look. Reference {Minimum} or later to theme them.",
                Requires,
                version,
                MinimumVersion,
                MinimumVersion);
            return;
        }

        Add((Styles)AvaloniaXamlLoader.Load(_serviceProvider, Source));
    }

    /// <summary>The version of the named assembly if the app can load it, otherwise null.</summary>
    private static Version? FindVersion(string assemblyName)
    {
        try
        {
            return Assembly.Load(new AssemblyName(assemblyName)).GetName().Version;
        }
        catch (Exception exception) when (exception is FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            return null;
        }
    }
}
