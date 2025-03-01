using CloudStore.BL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CloudStore.UI.Exceptions;

namespace CloudStore.UI.Services
{
    public class ApiUserService
    {
        private readonly HttpClient _httpClient;

        public ApiUserService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://localhost:7157/cloud-store-api/User/")
            };
        }

        public async Task<User?> RegistrationUser(string login, string password)
        {
            var res = await _httpClient.PostAsJsonAsync("create-user", new LoginAndPassword
            {
                Login = login,
                Password = password
            });
            return res.StatusCode switch
            {
                HttpStatusCode.OK => await res.Content.ReadFromJsonAsync<User>(), 
                HttpStatusCode.Conflict => throw new ExistentLoginException(),
                HttpStatusCode.BadRequest => throw new NotValidException(),
                _ => null,
            };
        }

        public async Task<User?> AuthorizeUser(string login, string password)
        {
            var res = await _httpClient.PostAsJsonAsync("authorize-user", new LoginAndPassword
            {
                Login = login,
                Password = password
            });
            return res.StatusCode switch
            {
                HttpStatusCode.OK => await res.Content.ReadFromJsonAsync<User>(),
                HttpStatusCode.NotFound => throw new ExistentLoginException(),
                HttpStatusCode.BadRequest => throw new PasswordException(),
                _ => null,
            };
        }

    }
}