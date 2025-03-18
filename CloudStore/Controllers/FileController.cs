using CloudStore.BL;
using CloudStore.BL.Models;
using CloudStore.DAL;
using CloudStore.WebApi.apiKeyValidation;
using Microsoft.AspNetCore.Mvc;

namespace CloudStore.Controllers;

[Route("cloud-store-api/[controller]")]
[ApiController]
[DisableRequestSizeLimit]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly CSFilesDbHelper _dbHelper;
    private readonly CSUsersDbHelper _dbUserHelper;
    private readonly string _userDirectory;
    private readonly IApiKeyValidation _apiKeyValidation;

    public FileController(IWebHostEnvironment webHostEnvironment, CSFilesDbHelper dbHelper,
                         CSUsersDbHelper dbUserHelper, IApiKeyValidation apiKeyValidation)
    {
        _webHostEnvironment = webHostEnvironment;
        _dbHelper = dbHelper;
        _dbUserHelper = dbUserHelper;
        _apiKeyValidation = apiKeyValidation;
        _userDirectory = _webHostEnvironment.ContentRootPath + "\\Files";
    }

    [HttpGet("api-key:{apiKey}/download/{id}")]
    public async Task<IActionResult> DownloadAsync(int id, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var path = Path.Combine(_userDirectory, user.UserDirectory);
        var fileToDownload = await _dbHelper.GetFileByIdAsync(id, user);

        if (fileToDownload == null)
            return NotFound();

        string filePath = Path.Combine(path, $"{fileToDownload.Path}");
        string file_type = $"application/{fileToDownload.Extension}";
        string file_name = $"{fileToDownload.Name}";

        return PhysicalFile(filePath, file_type, file_name);
    }

    [HttpGet("api-key:{apiKey}/{id}")]
    public async Task<IActionResult> GetFileAsync(int id, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        return Ok(await _dbHelper.GetFileByIdAsync(id, user));
    }

    [HttpGet("api-key:{apiKey}/all-files")]
    public async Task<IActionResult> GetAllFilesAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        if (user is null)
            throw new Exception("Not authorized!");

        return Ok(await _dbHelper.GetAllFilesAsync(user));
    }

    [HttpGet("api-key:{apiKey}/all-files-from-directory")]
    public async Task<IActionResult> GetAllFilesFromDirectoryAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var userDir = Path.Combine(_userDirectory, user.UserDirectory);

        return Ok(await _dbHelper.GetAllFilesInDirectory(user, userDir));
    }

    [HttpGet("api-key:{apiKey}/all-files-from-directory/{directory}")]
    public async Task<IActionResult> GetAllFilesFromDirectoryAsync(string apiKey, string directory)
    {
        directory = string.Join("\\", directory.Split("|"));
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        var userDir = Path.Combine(_userDirectory, user.UserDirectory);

        var path = Path.Combine(userDir, directory);

        return Ok(await _dbHelper.GetAllFilesInDirectory(user, path));
    }

    [HttpPost("api-key:{apiKey}")]
    public async Task<IActionResult> PostAsync(string apiKey, [FromBody] FileModel file)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        var isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        await _dbHelper.AddFileAsync(file);

        return Ok();
    }

    [HttpPut("api-key:{apiKey}/update-file/{id}")]
    public async Task<IActionResult> UpdateFileAsync(string apiKey, int id, [FromBody] string newFileName)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();
        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        var oldFile = await _dbHelper.GetFileByIdAsync(id, user);
        if (oldFile == null)
            return BadRequest();
        var oldPath = oldFile.Path;

        await _dbHelper.UpdateFileName(oldFile.Id, newFileName);

        try
        {
            System.IO.File.Move(oldPath, oldFile.Path);
        }
        catch (IOException ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(oldFile);
    }

    [HttpGet("api-key:{apiKey}/file-size/{id}")]
    public async Task<IActionResult> GetFileSizeAsync(string apiKey, int id)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();
        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        var file = await _dbHelper.GetFileByIdAsync(id, user);
        
        return Ok(new FileInfo(file.Path).Length);
    }
    [HttpDelete("api-key:{apiKey}/{id}")]
    public async Task<IActionResult> DeleteAsync(string apiKey, int id)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);
        var file = await _dbHelper.GetFileByIdAsync(id, user);
        System.IO.File.Delete(file.Path);

        await _dbHelper.DeleteFileByIdAsync(id, user);

        return Ok();
    }

    [HttpPost("api-key:{apiKey}/new-directory")]
    public IActionResult CreateDirectory(string apiKey, [FromBody] string directory)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        var isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var path = Path.Combine(_userDirectory, user.UserDirectory);

        var newDirectory = Path.Combine(path, directory);

        if (Directory.Exists(newDirectory))
            return Ok("Directory is already exist");

        try
        {
            Directory.CreateDirectory(newDirectory);
        }
        catch (IOException ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok("new Directory was created");
    }

    [HttpPost("api-key:{apiKey}/upload-file")]
    public async Task<IActionResult> UploadFileAsync(string apiKey, IFormFile uploadedFile)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        if (uploadedFile is null)
            return BadRequest("test");

        var path = Path.Combine(_userDirectory, user.UserDirectory);

        path += "\\" + uploadedFile.FileName;

        using var fs = new FileStream(path, FileMode.Create);
        await uploadedFile.CopyToAsync(fs);

        var extension = uploadedFile.FileName.Split('.')[^1];

        var newFile = new FileModel
        {
            Name = uploadedFile.FileName,
            Path = path,
            Extension = extension,
            UserId = user.Id
        };
        await _dbHelper.AddFileAsync(newFile);

        return Ok(newFile);
    }

    [HttpPost("api-key:{apiKey}/upload-file/{directory}")]
    public async Task<IActionResult> UploadFileAsync(string apiKey, string directory, IFormFile uploadedFile)
    {
        directory = string.Join("\\", directory.Split("|"));
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        if (!_apiKeyValidation.IsValidApiKey(apiKey))
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        if (uploadedFile is null)
            return BadRequest("test");

        var path = Path.Combine(_userDirectory, user.UserDirectory);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(Path.Combine(path, directory)))
            return NotFound("Directory is not exist");

        if (string.IsNullOrEmpty(directory))
            path += "\\" + uploadedFile.FileName;
        else
            path += "\\" + directory + "\\" + uploadedFile.FileName;

        await using var fs = new FileStream(path, FileMode.Create);
        await uploadedFile.CopyToAsync(fs);

        var extension = uploadedFile.FileName.Split('.')[^1];

        var newFile = new FileModel
        {
            Name = uploadedFile.FileName,
            Path = path,
            Extension = extension,
            UserId = user.Id
        };
        await _dbHelper.AddFileAsync(newFile);

        return Ok(newFile);
    }

    [HttpGet("api-key:{apiKey}/scan-directory/{directory}")]
    public async Task<IActionResult> ScanDirectoryAsync(string apiKey, string directory)
    {
        directory = string.Join("\\", directory.Split("|"));
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var temp = Path.Combine(_userDirectory, user.UserDirectory);
        var path = Path.Combine(temp, directory);

        if (!Directory.Exists(path))
            return BadRequest("This directory isn't exist!");

        var absolutPaths = Directory.EnumerateDirectories(path).ToList();

        var res = absolutPaths.Select(p => Path.GetRelativePath(path, p)).ToList();

        return Ok(res);
    }

    [HttpPut("api-key:{apiKey}/rename-directory/{newDirectoryName}")]
    public async Task<IActionResult> RenameDirectoryAsync(string apiKey, string newDirectoryName, [FromBody] string oldDirectory)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        if (string.IsNullOrWhiteSpace(oldDirectory))
            return BadRequest();

        var temp = Path.Combine(_userDirectory, user.UserDirectory);
        var oldPath = Path.Combine(temp, oldDirectory);
        var splitOldPath = oldPath.Split('\\');
        splitOldPath[^1] = newDirectoryName;
        var newPath = string.Join('\\', splitOldPath);

        try
        {
            Directory.Move(oldPath, newPath);
        }
        catch (IOException)
        {
            return BadRequest("Server problem");
        }
        return Ok(Path.GetRelativePath(temp, newPath));
    }

    [HttpPut("api-key:{apiKey}/delete-directory")]
    public async Task<IActionResult> DeleteDirectoryAsync(string apiKey, [FromBody] string directory)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        if (string.IsNullOrWhiteSpace(directory))
            return BadRequest();

        var temp = Path.Combine(_userDirectory, user.UserDirectory);
        var path = Path.Combine(temp, directory);

        if (!Directory.Exists(path))
            return BadRequest("This directory isn't exist!");
        try
        {
            Directory.Delete(path, true);
        }
        catch (IOException)
        {
            return BadRequest("Server problem");
        }

        return Ok("Directory was deleted");
    }

    [HttpGet("api-key:{apiKey}/scan-directory")]
    public async Task<IActionResult> ScanDirectoryAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest();

        bool isValid = _apiKeyValidation.IsValidApiKey(apiKey);
        if (!isValid)
            return Unauthorized();

        var user = _dbUserHelper.GetUserByApiKey(apiKey);

        var path = Path.Combine(_userDirectory, user.UserDirectory);

        var absolutPaths = Directory.EnumerateDirectories(path).ToList();
        var res = absolutPaths.Select(p => Path.GetRelativePath(path, p)).ToList();

        return Ok(res);
    }
}