using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text;
using Serilog;
using static Serilog.Log;

namespace Unity_package_downloader
{
    internal abstract class Program
    {
        private static readonly ILogger Logger = ForContext<Program>();
        private static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                .CreateLogger();
        
            var rootCommand = new RootCommand("Unity Package Downloader");
        
            var outputDirectoryOption = new Option<string>(
                name: "--output-dir",
                description: "Output Directory",
                getDefaultValue: () => $"./{rootCommand.Name}"
            );
        
            var bearerToken = new Option<string?>(
                name: "--bearer",
                description: "Bearer Token",
                getDefaultValue: () => null
            );
            
            var limitedOption = new Option<bool>(
                name: "--limited",
                description: "Limits downloads to a single page",
                getDefaultValue: () => false
            );
        
            rootCommand.AddGlobalOption(outputDirectoryOption);
            rootCommand.AddOption(bearerToken);
            rootCommand.AddOption(limitedOption);
        
            rootCommand.SetHandler(async (outputDirectory, token, limited) =>
            {
                Logger.Information("Starting...");

                Logger.Debug("Using Path: {Path}", outputDirectory);

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Fatal("Token is null");
                    Environment.Exit(0);
                }

                if (!Path.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                
                var webRequests = new WebRequests();
                await webRequests.GetProductIds(token, limited);
                await webRequests.DownloadProducts(outputDirectory);

                Thread.Sleep(5000);
                Logger.Information("Downloads completed");
            }, outputDirectoryOption, bearerToken, limitedOption);
            var commandLineBuilder = new CommandLineBuilder(rootCommand).UseHelp();

            var built = commandLineBuilder.Build();
            return await built.InvokeAsync(args);
        }
    }
}