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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UserController(HashHelper hashHelper, CSUsersDbHelper dbContext, CloudValidation validation,
                            IWebHostEnvironment webHostEnvironment)
    {
        _hashHelper = hashHelper;
        _dbContext = dbContext;
        _validation = validation;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpPost("authorize-user")]
    public async Task<IActionResult> AuthorizeUser([FromBody] LoginAndPassword lp)
    {
        var user = await _dbContext.GetUserAsync(lp.Login);

        if (user == null)
            return NotFound($"{lp.Login} was not found.");

        if (_hashHelper.ConvertStringToHash(lp.Password) != user.Password)
            return BadRequest($"{lp.Password} is invalid.");

        return Ok(user);
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUserAsync([FromBody] LoginAndPassword lp)
    {
        var user = await _dbContext.GetUserAsync(lp.Login);

        if (user != null)
            return Conflict($"{lp.Login} is exist.");

        if (!_validation.CheckLogin(lp.Login) && !_validation.CheckPassword(lp.Password))
            return BadRequest($"Login:{lp.Login} or password:{lp.Password} is not valid!");

        var hashPassword = _hashHelper.ConvertStringToHash(lp.Password);
        var newUser = new User
        {
            Login = lp.Login,
            Password = hashPassword
        };

        newUser.UserDirectory = newUser.Id.ToString();

        newUser.ApiKey = _hashHelper.ConvertStringWishShuffleToHash(newUser.Login + newUser.Password);

        Task.WaitAny([_dbContext.CreateUserAsync(newUser)]);
        var check = _makeDirectory(newUser.UserDirectory);

        if (check)
            return Ok(user);

        return BadRequest();
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

    private bool _makeDirectory(string directory)
    {
        var userDirectory = _webHostEnvironment.ContentRootPath + "\\Files";
        var path = Path.Combine(userDirectory, directory);

        if (Directory.Exists(path))
            return false;

        try
        {
            Directory.CreateDirectory(path);
        }
        catch (IOException ex)
        {
            return false;
        }
        return true;
    }
}