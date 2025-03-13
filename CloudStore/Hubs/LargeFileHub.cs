using CloudStore.BL;
using CloudStore.DAL;
using CloudStore.WebApi.apiKeyValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CloudStore.Hubs;
[DisableRequestSizeLimit]
public class LargeFileHub : Hub
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly CSFilesDbHelper _dbFileHelper;
    private readonly CSUsersDbHelper _dbUserHelper;
    private readonly string _userDirectory;
    private readonly IApiKeyValidation _apiKeyValidation;
    
    public LargeFileHub(IWebHostEnvironment webHostEnvironment, CSFilesDbHelper dbHelper,
        CSUsersDbHelper dbUserHelper, IApiKeyValidation apiKeyValidation)
        {
            
            _webHostEnvironment = webHostEnvironment;
            _dbFileHelper = dbHelper;
            _dbUserHelper = dbUserHelper;
            _apiKeyValidation = apiKeyValidation;
            _userDirectory = _webHostEnvironment.ContentRootPath + "\\Files";
        }
    public async Task<bool> SendLargeFile(string apiKey,string filePath, byte[] file)
    {
        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            return false;
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
            return false;
        }

        await _dbFileHelper.AddFileAsync(new()
        {
            Name = filePath.Split("\\")[^1],
            Extension = filePath.Split("\\")[^1].Split('.')[^1],
            Path = path,
            UserId = user.Id,
            
        });
        return true;
    }
}