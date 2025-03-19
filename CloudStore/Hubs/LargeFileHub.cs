using CloudStore.BL;
using CloudStore.BL.Models;
using CloudStore.DAL;
using CloudStore.WebApi.apiKeyValidation;
using CloudStore.WebApi.Helpers.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Reflection.Metadata;

namespace CloudStore.Hubs;

[DisableRequestSizeLimit]
public class LargeFileHub : Hub
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly CSFilesDbHelper _dbFileHelper;
    private readonly CSUsersDbHelper _dbUserHelper;
    private readonly string _userDirectory;
    private readonly IApiKeyValidation _apiKeyValidation;
    private Dictionary<string, PreparedLargeFile> _queueForFiles;
    private IMemoryCache _cache;

    public LargeFileHub(IWebHostEnvironment webHostEnvironment, CSFilesDbHelper dbHelper,
        CSUsersDbHelper dbUserHelper, IApiKeyValidation apiKeyValidation, IMemoryCache cache)
    {
        _webHostEnvironment = webHostEnvironment;
        _dbFileHelper = dbHelper;
        _dbUserHelper = dbUserHelper;
        _apiKeyValidation = apiKeyValidation;
        _userDirectory = _webHostEnvironment.ContentRootPath + "\\Files";
        _queueForFiles = new();
        _cache = cache;
    }

    public async Task<FileModel?> SendLargeFile(string apiKey, string filePath, byte[] file)
    {
        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            throw new UnauthorizedAccessException();
        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        var path = Path.Combine(_userDirectory, user.UserDirectory);
        path = Path.Combine(path, filePath);
        try
        {
            await using var stream = new FileStream(path, FileMode.Create);
            stream.Write(file, 0, file.Length);
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return null;
        }

        var newFile = new FileModel()
        {
            Name = filePath.Split("\\")[^1],
            Extension = filePath.Split("\\")[^1].Split('.')[^1],
            Path = path,
            UserId = user.Id
        };

        await _dbFileHelper.AddFileAsync(newFile);
        return await _dbFileHelper.GetFileByIdAsync(newFile.Id, newFile.User);
    }

    public bool PrepareLargeFile(string apiKey, string filePath)
    {
        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            throw new UnauthorizedAccessException();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var path = Path.Combine(_userDirectory, user.UserDirectory);
        path = Path.Combine(path, filePath);

        var fileName = filePath.Split('\\')[^1];
        var extension = fileName.Split('.')[^1];

        if (!_cache.TryGetValue(apiKey, out PreparedLargeFile cache))
        {
            _cache.Set<PreparedLargeFile>(apiKey, new(path, fileName, extension, user));
            return true;
        }
        if (!_queueForFiles.ContainsKey(apiKey))
            if (!_queueForFiles.TryAdd(apiKey, new(path, fileName, extension, user)))
                throw new Exception("can't add");
            else return true;
        
        return false;
    }

    public async Task UploadLargeFile(string apiKey, byte[] package, bool end)
    {
        if (!_queueForFiles.TryGetValue(apiKey, out PreparedLargeFile? queue))
        {
            queue = _cache.Get<PreparedLargeFile>(apiKey);
        }
        
        if (!end)
        {
            queue.Packages.Enqueue(package);
            return;
        }
        else if (end)
        {
            var path = queue.File.Path;

            if (!File.Exists(path))
            {
                using var fs = File.Create(path);
                fs.Dispose();
            }

            try
            {
                await using var fs = new FileStream(path, FileMode.Append, FileAccess.Write);

                var packages = queue.Packages;

                while (packages.Count > 0)
                    await fs.WriteAsync(packages.Dequeue());
                queue.FinishedFillingPackages = true;
            }
            catch (IOException ioEx)
            {
                File.Delete(path);
                return;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); //add logs
            }
        }
    }

    public async Task<FileModel?> FinishFileUploading(string apiKey)
    {
        if (!_queueForFiles.TryGetValue(apiKey, out PreparedLargeFile? preparedFile))
        {
            preparedFile = _cache.Get<PreparedLargeFile>(apiKey);
            if (preparedFile == null)
                throw new Exception("Why");
        }
        if (!preparedFile.FinishedFillingPackages)
            throw new Exception("File packages is not filled");

        var file = new FileModel
        {
            UserId = preparedFile.File.UserId,
            Path = preparedFile.File.Path,
            Extension = preparedFile.File.Extension,
            Name = preparedFile.File.Name
        };

        await _dbFileHelper.AddFileAsync(file);
        _cache.Remove(apiKey);
        return file;
    }

    public async Task<string> PrepareLargeFileForDownload(string apiKey, int fileId)
    {
        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            throw new UnauthorizedAccessException();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        var file = await _dbFileHelper.GetFileByIdAsync(fileId, user);
        
        var connectionId = Context.ConnectionId;
        var downloadId = Guid.NewGuid().ToString();
        _cache.Set(downloadId, new PreparedFileForDownload(file, connectionId));
        
        return downloadId;
    }

    public async Task DownloadLargeFile(string downloadId)
    {
        const int MB = 1024*1024;
        var file = _cache.Get<PreparedFileForDownload>(downloadId);

        var fileInfo = new FileInfo(file.File.Path);
        var quantityOfPackages = fileInfo.Length / (MB);
        var sizeOfLastPackage = fileInfo.Length % (MB);

        await using var fs = new FileStream(file.File.Path, FileMode.Open, FileAccess.Read);
        for (int i = 0; i < quantityOfPackages - 1; i++)
        {
            var package = new byte[MB];
            await fs.ReadExactlyAsync(package);
            await Clients.Client(file.ConnectionId).SendAsync("DownloadLargeFileCLient", package, downloadId,false);
        }
        var lastPackage = new byte[sizeOfLastPackage];
        
        await fs.ReadExactlyAsync(lastPackage);
        await Clients.Client(file.ConnectionId).SendAsync("DownloadLargeFileCLient", lastPackage,downloadId ,true );
        
        
    }
}