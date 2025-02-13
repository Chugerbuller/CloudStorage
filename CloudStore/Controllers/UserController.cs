using CloudStore.BL;
using CloudStore.BL.BL.Validation;
using CloudStore.BL.Models;
using CloudStore.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CloudStore.WebApi.Controllers;

[Route("cloud-store-api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly HashHelper _hashHelper;
    private readonly CloudValidation _validation;
    private readonly CSUsersDbHelper _dbContext;

    public UserController(HashHelper hashHelper, CSUsersDbHelper dbContext, CloudValidation validation)
    {
        _hashHelper = hashHelper;
        _dbContext = dbContext;
        _validation = validation;
    }

    [HttpPost("/user")]
    public async Task<IActionResult> PostUserAsync(string login, string password)
    {
        var user = await _dbContext.GetUserAsync(login);

        if (user == null)
            return NotFound($"{login} was not found.");

        if (_hashHelper.ConvertPasswordToHash(password) != user.Password)
            return BadRequest($"{password} is invalid.");

        return Ok(user);
    }

    [HttpPost("/create-user")]
    public async Task<IActionResult> PostCreateUserAsync(string login, string password)
    {
        var user = await _dbContext.GetUserAsync(login);

        if (user != null)
            return BadRequest($"{login} is exist.");

        if (!_validation.CheckLogin(login) && !_validation.CheckPassword(password))
            return BadRequest($"Login:{login} or password:{password} is not valid!");

        var hashPassword = _hashHelper.ConvertPasswordToHash(password);
        var newUser = new User
        {
            Login = login,
            Password = hashPassword
        };
        newUser.UserDirectory = newUser.Id.ToString();

        await _makeDirectory(newUser.UserDirectory);
        await _dbContext.CreateUserAsync(newUser);

        return Ok(user);
    }

    [HttpPut("update-user")]
    public async Task<IActionResult> PutAsync([FromBody] User user)
    {
        if (await _dbContext.GetUserAsync(user.Login) == null)
            return NotFound($"User was not found.");

        await _dbContext.UpdateUserAsync(user);

        return Ok(user);
    }

    [HttpDelete("delete-user")]
    public async Task<IActionResult> DeleteAsync([FromBody] User user)
    {
        if (await _dbContext.GetUserAsync(user.Login) == null)
            return NotFound($"User was not found.");

        await _dbContext.DeleteUserAsync(user);

        return Ok();
    }

    private async Task _makeDirectory(string directory)
    {
        var client = new HttpClient() { 
            BaseAddress = new Uri("https://localhost:7157/cloud-store-api/File/") 
        };

        HttpContent content = JsonContent.Create(directory);
        var response = await client.PostAsync($"new-directory", content);
        response.EnsureSuccessStatusCode();
    }
}