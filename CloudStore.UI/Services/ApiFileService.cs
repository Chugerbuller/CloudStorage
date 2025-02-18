using CloudStore.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

}
