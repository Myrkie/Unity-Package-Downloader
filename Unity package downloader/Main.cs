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
        
        var bearerToken = new Option<string?>(
            name: "--bearer",
            description: "Bearer Token",
            getDefaultValue: () => null
        );
        
        rootCommand.AddGlobalOption(outputDirectoryOption);
        rootCommand.AddOption(bearerToken);
        
        rootCommand.SetHandler(async (outputDirectory, token) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                    .CreateLogger();

                _logger = Log.ForContext(typeof(program));

                _logger.Information("Starting...");

                var path = Path.Combine(outputDirectory, "UnityPackages");
                
                _logger.Debug("Using Path: {path}", path);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.Fatal("Token is null");
                    Environment.Exit(0);
                }
                
                await WebRequests.GetProductIds(token);

                await WebRequests.GetProductInfo();

                await WebRequests.DownloadProducts(path);

                Thread.Sleep(5000);
            }, outputDirectoryOption, bearerToken);
        var commandLineBuilder = new CommandLineBuilder(rootCommand)
            .UseHelp();

        var built = commandLineBuilder.Build();
        return await built.InvokeAsync(args);
    }
}