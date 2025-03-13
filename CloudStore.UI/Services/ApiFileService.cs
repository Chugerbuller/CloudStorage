using CloudStore.BL.Models;
using CloudStore.UI.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CloudStore.UI.Services;

public class ApiFileService
{
    private readonly HttpClient _httpClient;
    private readonly WebClient _webClient;
    private readonly User _user;
    private readonly HubConnection _signalRClient;

    public ApiFileService(User user)
    {
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        _user = user;
        _httpClient = new HttpClient(clientHandler);
        _webClient = new WebClient();
        _signalRClient = new HubConnectionBuilder()
            .WithUrl("https://localhost:7157/cloud-store-api/large-file-hub")
            .Build();
    }

    public async Task<List<CloudStoreUiListItem>?> GetStartingScreenItemsAsync()
    {
        var res = new List<CloudStoreUiListItem>();
        List<FileForList>? files;
        List<DirectoryForList>? directorys;
        //Fix me "api-key" scheme is not supported
        var rawFiles = await _httpClient.GetFromJsonAsync<IEnumerable<FileModel>>("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/all-files-from-directory");

        if (rawFiles == null)
            files = null;
        else
            files = rawFiles.Select(f => new FileForList(f)).ToList();

        var rawDirectorys = await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"https://localhost:7157/cloud-store-api/File/api-key:{_user.ApiKey}/scan-directory");

        if (rawDirectorys == null)
            directorys = null;
        else
            directorys = rawDirectorys.Select(d => new DirectoryForList(d)).ToList();

        res.AddRange(directorys);
        res.AddRange(files);

        return res;
    }

    public async Task<List<CloudStoreUiListItem>?> GetItemsFromDirectoryAsync(string directory)
    {
        directory = string.Join("|", directory.Split("\\"));
        var res = new List<CloudStoreUiListItem>();
        IEnumerable<FileForList> files;
        IEnumerable<DirectoryForList> directorys;
        try
        {
            var rawFiles = await _httpClient.GetFromJsonAsync<IEnumerable<FileModel>>("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + $"/all-files-from-directory/{directory}");

            if (rawFiles == null)
                files = [];
            else
                files = rawFiles.Select(f => new FileForList(f));
        }
        catch (Exception)
        {
            files = [];
        }
        try
        {
            var rawDirectorys = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + $"/scan-directory/{directory}");

            if (rawDirectorys == null)
                directorys = [];
            else
                directorys = rawDirectorys.Select(d => new DirectoryForList(d));
        }
        catch (Exception)
        {
            directorys = [];
        }

        res.AddRange(directorys);
        res.AddRange(files);

        return res;
    }

    public async Task<CloudStoreUiListItem?> UploadFileAsync(string filePath, string directory = "")
    {
        using var multipartFormContent = new MultipartFormDataContent();
        var url = "localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey;
        if (directory is not null)
            url += $@"/upload-file/{string.Join("|", directory.Split("\\"))}";
        else
            url += "/upload-file";

        var fileName = filePath.Split(@"\")[^1];
        var extension = fileName.Split(".")[^1];

        await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var fileStreamContent = new StreamContent(fs);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"application/{extension}");
        multipartFormContent.Add(fileStreamContent, name: "uploadedFile", fileName: fileName);
        var resposne = await _httpClient.PostAsync(url, multipartFormContent);

        return resposne.StatusCode switch
        {
            HttpStatusCode.BadRequest => null,
            HttpStatusCode.Unauthorized => throw new Exception("Unauthorized"),
            HttpStatusCode.OK => new FileForList(await resposne.Content.ReadFromJsonAsync<FileModel?>()),
            _ => null,
        };
    }

    public async Task<CloudStoreUiListItem?> UploadLargeFile(string filePath, string? directory)
    {
        var fileName = filePath.Split('\\')[^1];

        if (directory != null && !string.IsNullOrEmpty(directory))
            fileName = directory + "\\" + fileName;

        try
        {
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];

            await fs.ReadExactlyAsync(buffer);

            await _signalRClient.StartAsync();

            var res = await _signalRClient.InvokeAsync<FileModel>("SendLargeFile", _user.ApiKey, fileName, buffer);
            if (res is not null)
                return new FileForList(res);
        }
        catch (UnauthorizedAccessException unEx)
        {
            throw new UnauthorizedAccessException("Not authorized");
        }
        catch (SocketException ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
        return null;
    }

    public async Task<FileModel?> UploadLargeFile(stirng filePath)
    {
        await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            _signalRClient.StartAsync();
            await _signalRClient.InvokeAsync("PrepareLargeFile", _user.ApiKey, filePath);

            var size = fs.Length / 10240;
            var lastSize = fs.Length % 10240;
            for (var i = 0; i < size - 1; i++)
            {
                var package = new byte[10240];
                fs.Read(package, 0, package.Length);
                _signalRClient.InvokeAsync("UploadLargeFile", _user.ApiKey, package, false);
            }
            var lastPackage = new byte[lastSize];
            fs.Read(lastPackage, 0, lastPackage.Length);
            await _signalRClient.InvokeAsync("UploadLargeFile", _user.ApiKey, lastPackage, true);

            return await _signalRClient.InvokeAsync<FileModel?>("FinishFileUploading", _user.ApiKey);
        }
    }

    public async Task<bool> DownloadFileAsync(FileModel file, string path)
    {
        var response = await _httpClient
            .GetAsync("localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/download/" + file.Id);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:

                _webClient.DownloadFileAsync(
                                    new Uri("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/download/" + file.Id),
                                    Path.Combine(path, file.Name));
                return true;

            case HttpStatusCode.Unauthorized:
                throw new Exception("Unauthorized");
            case HttpStatusCode.BadRequest:
                return false;

            default:
                return false;
        }
    }

    public async Task<bool> DeleteFileAsync(FileModel file)
    {
        var response = await _httpClient.DeleteAsync("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/" + file.Id);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => false,
            HttpStatusCode.Unauthorized => throw new Exception("Unauthorized"),
            HttpStatusCode.OK => true,
            _ => false,
        };
    }

    public async Task<CloudStoreUiListItem?> UpdateFileAsync(FileModel file)
    {
        HttpContent content = JsonContent.Create(file.Name);

        var response =
            await _httpClient.PutAsync(
               "https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + $"/update-file/{file.Id}", content);
        if (response.IsSuccessStatusCode)
        {
            var updateFile = await response.Content.ReadFromJsonAsync<FileModel>();

            return new FileForList(updateFile);
        }
        else return null;
    }

    public async Task<DirectoryForList?> MakeDirectoryAsync(string directory)
    {
        //https://localhost:7157/cloud-store-api/File/api-key:90546392470C5E893709E707FB70F88E/new-directory/testDir%2FDir
        var url = "https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/new-directory";
        var jsonContent = JsonContent.Create(directory);
        var response = await _httpClient.PostAsync(url, jsonContent);

        if (response.StatusCode == HttpStatusCode.OK)
            return new DirectoryForList(directory.Split("\\")[^1]);
        else
            return null;
    }

    public async Task<DirectoryForList?> ChangeDirectoryNameAsync(string oldDirectory, string newDirectoryName)
    {
        HttpContent content = JsonContent.Create(oldDirectory);

        var response = await _httpClient.PutAsync(
            "https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + $"/rename-directory/{newDirectoryName}", content);

        if (response.IsSuccessStatusCode)
            return new DirectoryForList(await response.Content.ReadAsStringAsync());

        return null;
    }

    public async Task<bool> DeleteDirectoryAsync(string directory)
    {
        HttpContent content = JsonContent.Create(directory);
        var response = await _httpClient.PutAsync(
            "https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/delete-directory", content);
        if (response.IsSuccessStatusCode)
            return true;
        return false;
    }
}