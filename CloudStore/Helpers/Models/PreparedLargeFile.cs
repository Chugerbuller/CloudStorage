using CloudStore.BL.Models;
using System.IO;

namespace CloudStore.WebApi.Helpers.Models;

public class PreparedLargeFile
{
    public PreparedLargeFile(string filePath, string fileName, string extension, User user)
    {
        File = new FileModel
        {
            UserId = user.Id,
            Path = filePath,
            Name = fileName,
            Extension = extension
        };
        Packages = new();
        FinishedFillingPackages = false;
        
    }

    public FileModel File { get; init; }
    public Queue<byte[]> Packages { get; }
    public bool FinishedFillingPackages { get; set; }
}