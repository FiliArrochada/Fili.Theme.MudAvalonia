using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// A <c>StyleInclude</c> for a control package this theme builds against but does not depend on:
/// the file loads only when the app has the package, at the version the theme was built against
/// or later, and below the next major version.
/// <code>
/// &lt;theme:OptionalStyleInclude Requires="Avalonia.Controls.DataGrid" MinimumVersion="12.1.0" BelowVersion="13.0.0"
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
/// name parts of the version it was built against. So is one of the next major version, whose
/// parts this theme has never seen. Its controls then have only whatever theme the app includes
/// for them - Avalonia has no default, so none at all if the app includes none.
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

    /// <summary>The first version the file is not for: the next major version after the minimum.</summary>
    public Version? BelowVersion { get; set; }

    /// <summary>The file, an <c>avares://</c> URI whose root is <c>Styles</c>.</summary>
    public Uri? Source { get; set; }

    void ISupportInitialize.BeginInit()
    {
    }

    /// <remarks>
    /// Loading by URI finds the compiled XAML through reflection, by the name of the type the XAML
    /// compiler generates, so a trimmed app - the browser gallery, or any app published trimmed
    /// or AOT - dropped it, and with the DataGrid present the theme threw at startup: "No
    /// precompiled XAML found". The dependency keeps that type, and every file it can load, in
    /// this assembly.
    /// </remarks>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods, "CompiledAvaloniaXaml.!XamlLoader", "Fili.Theme.MudAvalonia")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The compiled XAML the URI names is kept by the DynamicDependency above.")]
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
                "Fili.Theme.MudAvalonia: {Assembly} {Version} is older than {Minimum}, the version this theme is built against, so this theme leaves its controls alone. Reference {Minimum} or later to theme them.",
                Requires,
                version,
                MinimumVersion,
                MinimumVersion);
            return;
        }

        if (BelowVersion is not null && version >= BelowVersion)
        {
            Logger.TryGet(LogEventLevel.Warning, LogArea.Visual)?.Log(
                this,
                "Fili.Theme.MudAvalonia: {Assembly} {Version} is {Below} or later, a version this theme was not built for, so this theme leaves its controls alone. Use a version of this theme for it, or {Assembly} below {Below}.",
                Requires,
                version,
                BelowVersion,
                Requires,
                BelowVersion);
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
