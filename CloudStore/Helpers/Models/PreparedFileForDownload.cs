using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudStore.WebApi.Helpers.Models;

public class PreparedFileForDownload
{
    public PreparedFileForDownload(FileModel file)
    {
        File = file;
    }
    public Queue<byte[]> Queue { get; private set; }
    public FileModel File { get; set; }

    public async Task InitializeQueue()
    {
        const int MB = 1024 * 1024;
        await using var fs = new FileStream(File.Path, FileMode.Open, FileAccess.Read);
        
        var fileInfo = new FileInfo(File.Path);
        var quantityOfPackets = fileInfo.Length / (MB);
        var lastSize = fileInfo.Length % (MB);
        Queue = new Queue<byte[]>();
        for (int i = 0; i < quantityOfPackets - 1; i++)
        {
            var package = new byte[MB];
            await fs.ReadExactlyAsync(package);
            Queue.Enqueue(package);
        }
        var lastPackage = new byte[lastSize];
        await fs.ReadExactlyAsync(lastPackage);
        Queue.Enqueue(lastPackage);
    }
}