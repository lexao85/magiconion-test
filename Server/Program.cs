using System;
using System.IO;
using System.Threading.Tasks;
using MagicOnion.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Server.Options;
using Server.Services;

namespace Server
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting host...");
                await new HostBuilder()
                    .UseSerilog()
                    .ConfigureAppConfiguration((hostContext, configApp) =>
                    {
                        configApp.SetBasePath(Directory.GetCurrentDirectory());
                        configApp.AddJsonFile("appsettings.json", optional: true);
                        configApp.AddJsonFile(
                            $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true);
                        configApp.AddEnvironmentVariables(prefix: "DATASHARESERVICE_");
                        configApp.AddCommandLine(args);
                    })
                    .UseMagicOnion(types: new[] { typeof(DataShareService) })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.Configure<DataShareServiceSettings>(hostContext.Configuration.GetSection("DataShareServiceSettings"));
                    })
                    .RunConsoleAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
