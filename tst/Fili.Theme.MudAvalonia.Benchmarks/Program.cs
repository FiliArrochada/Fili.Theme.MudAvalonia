using Avalonia;
using Avalonia.Headless;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Fili.Theme.MudAvalonia.Benchmarks;

// The headless application the benchmarks dispatch to, as the test projects declare theirs.
[assembly: AvaloniaTestApplication(typeof(BenchmarkAppBuilder))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]

// In process: Avalonia's headless session is a UI thread in this process, and a benchmark run in
// a generated child project would boot its own and build that project under this repository's
// props. Run from the repository root:
//
//     dotnet run -c Release --project tst/Fili.Theme.MudAvalonia.Benchmarks
//     dotnet run -c Release --project tst/Fili.Theme.MudAvalonia.Benchmarks -- --filter "*BuildScreen*"
//
// Results land in BenchmarkDotNet.Artifacts/ under the working directory (git-ignored).
var config = DefaultConfig.Instance
    .AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));

BenchmarkSwitcher.FromAssembly(typeof(ThemeBenchmarks).Assembly).Run(args, config);

namespace Fili.Theme.MudAvalonia.Benchmarks
{
    public class BenchmarkAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
