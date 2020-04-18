
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Options;
using Shared;

namespace Server.Services
{
    public class DataShareService : ServiceBase<IDataShareService>, IDataShareService
    {
        DataShareServiceSettings config;
        ILogger<DataShareService> logger;

        public DataShareService(IOptions<DataShareServiceSettings> config, ILogger<DataShareService> logger)
        {
            this.config = config.Value;
            this.logger = logger;
            if (!Directory.Exists(this.config.Directory))
                Directory.CreateDirectory(this.config.Directory);
        }

        public async UnaryResult<byte[]> DownloadFileAsync(string fileName)
        {
            this.logger.LogInformation(@"download file: {fileName}", fileName);
            var res = await File.ReadAllBytesAsync(this.GetServiceFilePath(fileName));
            return res;
        }

        public async UnaryResult<IList<string>> GetFilesAsync() {
            if (!Directory.Exists(this.config.Directory))
                return new List<string>();

            var res = Directory.GetFiles(this.config.Directory, "*.*", SearchOption.TopDirectoryOnly)
                .Select(p => Path.GetFileName(p));
            
            return res.ToArray();
        }

        public async UnaryResult<bool> UploadFileAsync(string fileName, byte[] bytes)
        {
            this.logger.LogInformation(@"upload file: {fileName}", fileName);
            await System.IO.File.WriteAllBytesAsync(this.GetServiceFilePath(fileName), bytes);
            return true;
        }

        private string GetServiceFilePath(string fileName) => 
            Path.Combine(this.config.Directory, fileName);
    }
}