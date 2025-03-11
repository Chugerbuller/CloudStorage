using System;
using System.IO;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Input;
using CloudStore.BL.Models;
using CloudStore.UI.Configs;
using CloudStore.UI.Exceptions;
using CloudStore.UI.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudStore.UI.ViewModels;

public class LoginAndRegistrationViewModel : ViewModelBase, ICloseable
{
    #region ReadonlyProps
    private readonly ApiUserService _userService;
    #endregion

    #region ReactiveProps

    [Reactive]
    public bool LoginPartVisibility { get; set; } = true;

    [Reactive]
    public bool RegistrationPartVisibility { get; set; } = false;

    [Reactive]
    public string Login { get; set; }

    [Reactive]
    public string Password { get; set; }

    [Reactive]
    public bool RememberMe { get; set; }

    [Reactive]
    public string LoginRegistration { get; set; }

    [Reactive]
    public string PasswordRegistration { get; set; }

    [Reactive]
    public string PasswordRegistrationRepeat { get; set; }

    [Reactive]
    public string WatermarkLogin { get; set; } = "Введите логин";
    [Reactive]
    public string WatermarkPassword { get; set; } = "Введите пароль";

    [Reactive]
    public string WatermarkPasswordReg { get; set; } = "Введите пароль";
    [Reactive]
    public string WatermarkLoginReg { get; set; } = "Введите логин";

    #endregion ReactiveProps

    #region ReactiveCommands

    public ReactiveCommand<Unit, Unit> AuthorizationCommand { get; }
    public ReactiveCommand<Unit, Unit> RegistrationCommand { get; }
    public ReactiveCommand<Unit, Unit> RegistrationUserCommand { get; }

    #endregion ReactiveCommands

    #region Events
    public event EventHandler? Closed;
    #endregion

    #region Props
    public User? User { get; private set; }
    #endregion

    #region Ctor
    public LoginAndRegistrationViewModel()
    {
        _userService = new ApiUserService();
        AuthorizationCommand = ReactiveCommand.CreateFromTask(Authorize);
        RegistrationCommand = ReactiveCommand.Create(() =>
        {
            LoginPartVisibility = !LoginPartVisibility;
            RegistrationPartVisibility = !RegistrationPartVisibility;
        });
        RegistrationUserCommand = ReactiveCommand.CreateFromTask(Registration);
    }
    #endregion

    #region Methods
    public async Task Authorize()
    {
        if (!LoginPartVisibility)
            return;
        try
        {
            User = await _userService.AuthorizeUser(Login, Password);
            if (User is not null)
            {

                if (RememberMe)
                {
                    var userJsonByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(User));
                    using var fsUser = new FileStream("Configs\\UserConfig.json", FileMode.Truncate, FileAccess.Write);

                    await fsUser.WriteAsync(userJsonByte);

                    using var fsAppCfg = new FileStream("Configs\\ApplicationConfig.json", FileMode.Open, FileAccess.ReadWrite);
                    var appJson = await JsonSerializer.DeserializeAsync<ApplicationConfig>(fsAppCfg);

                    appJson.RememberUser = true;
                    fsAppCfg.Dispose();
                    var appJsonByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(appJson));
                    using var writeAppCfg = new FileStream("Configs\\ApplicationConfig.json", FileMode.Truncate, FileAccess.Write);
                    writeAppCfg.Write(appJsonByte);
                }
                Closed(this, new EventArgs());
            }
        }
        catch (ExistentLoginException)
        {
            WatermarkLogin = "Существующий логин";
            return;
        }
        catch (PasswordException)
        {
            WatermarkPassword = "Неправильный пароль";
            return;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task Registration()
    {
        if (!RegistrationPartVisibility)
            return;
        if (PasswordRegistration != PasswordRegistrationRepeat)
        {
            WatermarkPasswordReg = "Пароли должны совпадать";
            return;
        }
        try
        {
            User = await _userService.RegistrationUserAsync(LoginRegistration, PasswordRegistration);

            if (User is not null)
            {
                Closed(this, new EventArgs());
            }
        }
        catch (ExistentLoginException)
        {
            WatermarkLoginReg = "Существующий логин";
        }
        catch (NotValidException)
        {
            WatermarkPasswordReg = "Невалидный пароль";
        }
        catch (Exception)
        {
            return;
        }
    }
    #endregion
}