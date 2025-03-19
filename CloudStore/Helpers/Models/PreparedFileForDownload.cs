using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudStore.WebApi.Helpers.Models;

public class PreparedFileForDownload
{
    public PreparedFileForDownload(FileModel file, string connectionId)
    {
        File = file;
        ConnectionId = connectionId;
    }

    public readonly string ConnectionId;
    public Queue<byte[]> Queue { get; private set; } = new();
    public FileModel File { get; set; }

    
}