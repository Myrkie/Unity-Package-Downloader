using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Serilog;
using Unity_package_downloader;

class program
{
    private static ILogger _logger = null!;

    // todo: Oath shit
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Unity Package Downloader");
        
        var outputDirectoryOption = new Option<string>(
            name: "--output-dir",
            description: "Output Directory",
            getDefaultValue: () => "./UnityPackages"
        );
        
        rootCommand.AddGlobalOption(outputDirectoryOption);
        
        rootCommand.SetHandler(async (outputDirectory) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                    .CreateLogger();

                _logger = Log.ForContext(typeof(program));


                var path = outputDirectory.Length == 0 ? @"G:\WINDownloads\upload" : outputDirectory;
                _logger.Information("Starting...");

                await WebRequests.GetProductIds();

                await WebRequests.GetProductInfo();

                await WebRequests.DownloadProducts(path);

                Thread.Sleep(5000);
            }, outputDirectoryOption);
        var commandLineBuilder = new CommandLineBuilder(rootCommand)
            .UseHelp();

        var built = commandLineBuilder.Build();
        return await built.InvokeAsync(args);
    }
}