using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using Shared;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new ConfigurationBuilder();

            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json", optional: true);
            builder.AddEnvironmentVariables(prefix: "DATASHARECLIENT_");
            builder.AddCommandLine(args);

            var config = builder.Build();

            var channel = new Channel(config["host"], int.Parse(config["port"]), ChannelCredentials.Insecure);
            var client = MagicOnionClient.Create<IDataShareService>(channel);

            var uploadDirectory = config["UploadDirectory"];
            var downloadDirectory = config["DownloadDirectory"];
            var mode = config["mode"].ToUpper();

            switch (mode)
            {
                case "DOWNLOAD":
                    var files = await client.GetFilesAsync();
                    Log.Information(@"Files: {files}", string.Join(',', files));
                    if (!Directory.Exists(downloadDirectory))
                        Directory.CreateDirectory(downloadDirectory);
                    foreach (var fileName in files)
                    {
                        var bytes = await client.DownloadFileAsync(fileName);
                        var pathToSave = Path.Combine(downloadDirectory, fileName);
                        await File.WriteAllBytesAsync(pathToSave, bytes);
                        Log.Information(@"file {fileName} saved to {pathToSave}", fileName, pathToSave);
                    }

                    break;
                default:
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Log.Warning(@"Upload directory {uploadDirectory} doesn't exist", uploadDirectory);
                        break;
                    }
                    var filesToUpload = Directory
                        .GetFiles(uploadDirectory, "*.*", SearchOption.TopDirectoryOnly);
                    if (filesToUpload.Length == 0)
                        Log.Warning("Upload directory is empty => nothing to upload");
                    foreach (var filePath in filesToUpload)
                    {
                        var uploadResult = await client.UploadFileAsync(
                            Path.GetFileName(filePath),
                            System.IO.File.ReadAllBytes(filePath));
                        if (uploadResult)
                            Log.Information(@"file {fileName} was uploaded successfully", Path.GetFileName(filePath));
                        else
                            Log.Information(@"file {fileName} wasn't uploaded", Path.GetFileName(filePath));
                    }
                    break;
            }
        }

    }

}