using CloudStore.BL.Models;
using System.IO;

namespace CloudStore.WebApi.Helpers.Models;

public class PreparedLargeFile
{
    public PreparedLargeFile(string filePath, string fileName, string extension, User user)
    {
        File = new FileModel
        {
            User = user,
            Path = filePath,
            Name = fileName,
            Extension = extension
        };
        Packages = new();
        FinishedFillingPackages = false;
        System.IO.File.Create(filePath);
    }

    public FileModel File { get; init; }
    public Queue<byte[]> Packages { get; }
    public bool FinishedFillingPackages { get; set; }
}