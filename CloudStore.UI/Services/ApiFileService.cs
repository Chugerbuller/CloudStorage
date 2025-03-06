using CloudStore.BL.Models;
using CloudStore.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CloudStore.UI.Services;

public class ApiFileService
{
    private readonly HttpClient _httpClient;
    private readonly WebClient _webClient;
    private readonly User _user;

    public ApiFileService(User user)
    {
        _user = user;
        _httpClient = new HttpClient();
        _webClient = new WebClient();
    }

    public async Task<List<UCListItem>?> GetStartingScreenItems()
    {
        var res = new List<UCListItem>();
        List<UCListItem>? files;
        List<UCListItem>? directorys;
        //Fix me "api-key" scheme is not supported
        var rawFiles = await _httpClient.GetFromJsonAsync<IEnumerable<FileModel>>("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/all-files-from-directory");

        if (rawFiles == null)
            files = null;
        else
            files = rawFiles.Select(f => new FileForList(f)).Select(f => new UCListItem(f)).ToList();

        var rawDirectorys = await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"https://localhost:7157/cloud-store-api/File/api-key:{_user.ApiKey}/scan-directory");

        if (rawDirectorys == null)
            directorys = null;
        else
            directorys = rawDirectorys.Select(d => new DirectoryForList(d)).Select(d => new UCListItem(d)).ToList();

        res.AddRange(directorys);
        res.AddRange(files);

        return res;
    }

    public async Task<List<UCListItem>?> GetItemsFromDirectory(string directory)
    {
        var res = new List<UCListItem>();
        IEnumerable<UCListItem?> files;
        IEnumerable<UCListItem?> directorys;

        var rawFiles = await _httpClient.GetFromJsonAsync<IEnumerable<FileModel>>("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/all-files-from-directory/{directory}");

        if (rawFiles == null)
            files = null;
        else
            files = rawFiles.Select(f => new FileForList(f)).Select(f => new UCListItem(f)).ToList(); ;
        var rawDirectorys = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/scan-directory/{directory}");

        if (rawDirectorys == null)
            directorys = null;
        else
            directorys = rawDirectorys.Select(d => new DirectoryForList(d)).Select(d => new UCListItem(d)).ToList();

        res.AddRange(directorys);
        res.AddRange(files);

        return res;
    }

    public async Task<UCListItem?> UploadFile(string filePath, string? directory = "")
    {
        using var multipartFormContent = new MultipartFormDataContent();

        if (directory is not null)
            multipartFormContent.Add(new StringContent(directory), "directory");

        var fileName = filePath.Split(@"/")[^1];
        var extension = fileName.Split(".")[^1];

        var fileStreamContent = new StreamContent(File.OpenRead(filePath));
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"application/{extension}");
        multipartFormContent.Add(fileStreamContent, name: "uploadedFile", fileName: fileName);
        var resposne = await _httpClient.PostAsync("https://localhost:7157/cloud-store-api/File/api-key:" + _user.ApiKey + "/upload-file/" + directory, multipartFormContent);

        return resposne.StatusCode switch
        {
            HttpStatusCode.BadRequest => null,
            HttpStatusCode.Unauthorized => throw new Exception("Unauthorized"),
            HttpStatusCode.OK => new(new FileForList(await resposne.Content.ReadFromJsonAsync<FileModel?>())),
            _ => null,
        };
    }

    public async Task<bool> DeleteFile(FileModel file)
    {
        return false;
    }

    public async Task<CloudStoreUiListItem?> ChangeFile(FileModel file)
    {
        return null;
    }

    public async Task<UCListItem?> MakeDirectory(string directory)
    {
        var response = await _httpClient.PostAsJsonAsync("https://localhost:7157/cloud-store-api/File/api-key:\" + _user.ApiKey + \"/new-directory", new { directory });

        if (response.StatusCode == HttpStatusCode.OK)
            return new(new DirectoryForList(directory));
        else
            return null;
    }
}