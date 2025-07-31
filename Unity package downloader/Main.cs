using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Serilog;

namespace Unity_package_downloader
{
    class Program
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(Program));
        static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                .CreateLogger();
        
            var rootCommand = new RootCommand("Unity Package Downloader");
        
            var outputDirectoryOption = new Option<string>(
                name: "--output-dir",
                description: "Output Directory",
                getDefaultValue: () => "./Out"
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
                Logger.Information("Starting...");

                Logger.Debug("Using Path: {path}", outputDirectory);

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Fatal("Token is null");
                    Environment.Exit(0);
                }

                if (!Path.Exists(outputDirectory))
                {
                    Logger.Fatal("Output directory does not exist");
                    Environment.Exit(0);
                }
                
                var webRequests = new WebRequests();
                await webRequests.GetProductIds(token);
                await webRequests.DownloadProducts(outputDirectory);

                Thread.Sleep(5000);
            }, outputDirectoryOption, bearerToken);
            var commandLineBuilder = new CommandLineBuilder(rootCommand)
                .UseHelp();

            var built = commandLineBuilder.Build();
            return await built.InvokeAsync(args);
        }
    }
}