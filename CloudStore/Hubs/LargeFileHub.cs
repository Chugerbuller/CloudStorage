using CloudStore.BL;
using CloudStore.BL.Models;
using CloudStore.DAL;
using CloudStore.WebApi.apiKeyValidation;
using CloudStore.WebApi.Helpers.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IO;

namespace CloudStore.Hubs;

[DisableRequestSizeLimit]
public class LargeFileHub : Hub
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly CSFilesDbHelper _dbFileHelper;
    private readonly CSUsersDbHelper _dbUserHelper;
    private readonly string _userDirectory;
    private readonly IApiKeyValidation _apiKeyValidation;
    private Dictionary<string, PreparedLargeFile> _queueForFiles = [];

    public LargeFileHub(IWebHostEnvironment webHostEnvironment, CSFilesDbHelper dbHelper,
        CSUsersDbHelper dbUserHelper, IApiKeyValidation apiKeyValidation)
    {
        _webHostEnvironment = webHostEnvironment;
        _dbFileHelper = dbHelper;
        _dbUserHelper = dbUserHelper;
        _apiKeyValidation = apiKeyValidation;
        _userDirectory = _webHostEnvironment.ContentRootPath + "\\Files";
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

    public async Task PrepareLargeFile(string apiKey, string filePath)
    {
        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            throw new UnauthorizedAccessException();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var path = Path.Combine(_userDirectory, user.UserDirectory);
        path = Path.Combine(path, filePath);

        var fileName = filePath.Split('\\')[^1];
        var extension = fileName.Split('.')[^1];

        if (!_queueForFiles.ContainsKey(apiKey))
            _queueForFiles.Add(apiKey, new(path, fileName, extension, user));
    }

    public async Task UploadLargeFile(string apiKey, byte[] package, bool end)
    {
        if (!_queueForFiles.ContainsKey(apiKey))
            throw new Exception("FirstPrepareFile");

        if (!end)
        {
            _queueForFiles[apiKey].Packages.Enqueue(package);
            return;
        }
        else if (end)
        {
            var path = _queueForFiles[apiKey].File.Path;
            try
            {
                await using var fs = new FileStream(path, FileMode.Append, FileAccess.Write);

                var packages = _queueForFiles[apiKey].Packages;

                while (packages.Count > 0)
                    await fs.WriteAsync(packages.Dequeue());
                _queueForFiles[apiKey].FinishedFillingPackages = true;
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
            throw new Exception("FirstPrepareFile");

        var file = preparedFile.File;

        await _dbFileHelper.AddFileAsync(file);

        return file;
    }
}