using MagicOnion;
using System.Collections.Generic;

namespace Shared
{
    public interface IDataShareService : IService<IDataShareService>
    {
        UnaryResult<IList<string>> GetFilesAsync();

        UnaryResult<bool> UploadFileAsync(string fileName, byte[] bytes);

        UnaryResult<byte[]> DownloadFileAsync(string fileName);
    }
}
