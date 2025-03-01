using CloudStore.BL.Models;
using CloudStore.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5108/cloud-store-api/File")
        };
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _user.ApiKey);
        _webClient = new WebClient();
    }

    public async Task<List<CloudStoreUiListItem>?> GetStartingScreenItems()
    {
        var res = new List<CloudStoreUiListItem>();
        List<FileForList> files;
        List<DirectoryForList> directorys;

        var rawFiles = await _httpClient.GetFromJsonAsync<IEnumerable<FileModel>>($"api-key:{_user.ApiKey}/all-files-from-directory");

        if (rawFiles == null)
            files = null;
        else
            files = rawFiles.Select(f => new FileForList(f)).ToList();
        var rawDirectorys = await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"api-key:{_user.ApiKey}/scan-directory");

        if (rawDirectorys == null)
            directorys = null;
        else
            directorys = rawDirectorys.Select(d => new DirectoryForList(d)).ToList();

        res.AddRange(directorys);
        res.AddRange(files);

        return res;
    }

    public async Task<List<CloudStoreUiListItem>?> GetItemsFromDirectory(string directory)
    {
        var res = new List<CloudStoreUiListItem>();
        IEnumerable<FileForList> files;
        IEnumerable<DirectoryForList> directorys;

        var rawFiles = await _httpClient.GetFromJsonAsync<IEnumerable<FileModel>>($"api-key:{_user.ApiKey}/all-files-from-directory/{directory}");

        if (rawFiles == null)
            files = null;
        else
            files = rawFiles.Select(f => new FileForList(f));
        var rawDirectorys = await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"api-key:{_user.ApiKey}/scan-directory/{directory}");

        if (rawDirectorys == null)
            directorys = null;
        else
            directorys = rawDirectorys.Select(d => new DirectoryForList(d));

        res.AddRange(directorys);
        res.AddRange(files);

        return res;
    }
}