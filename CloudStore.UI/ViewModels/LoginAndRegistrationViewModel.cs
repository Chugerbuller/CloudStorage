using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using CloudStore.UI.Exceptions;
using CloudStore.UI.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudStore.UI.ViewModels;

public class LoginAndRegistrationViewModel : ViewModelBase
{
    private readonly ApiUserService _userService;

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
    public string WatermarkPasswordReg { get; set; } = "Введите пароль";

    public ReactiveCommand<Unit, Unit> AuthorizationCommand { get; }
    public ReactiveCommand<Unit, Unit> RegistrationCommand { get; }
    public ReactiveCommand<Unit, Unit> RegistrationUserCommand { get; }

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

    public async Task Authorize()
    {
        if (!LoginPartVisibility)
            return;
        try
        {
            var user = await _userService.AuthorizeUser(Login, Password);
            if (user is not null)
            {
                LoginPartVisibility = !LoginPartVisibility;
                RegistrationPartVisibility = !RegistrationPartVisibility;
            }
        }
        catch (ExistentLoginException)
        {
            WatermarkLogin = "Существующий логин";
            return;
        }
        catch (NotValidException)
        {
            WatermarkLogin = "Неправильный логин";
            return;
        }
    }

    public async Task Registration()
    {
        if (!RegistrationPartVisibility)
            return;
        if (PasswordRegistration != PasswordRegistrationRepeat)
        {
            WatermarkPasswordReg = "Пароли должны совпадать";
        }
        try
        {
            var user = await _userService.RegistrationUser(LoginRegistration, PasswordRegistration);
            if (user is not null)
            {
                LoginPartVisibility = !LoginPartVisibility;
                RegistrationPartVisibility = !RegistrationPartVisibility;
            }
        }
        catch (ExistentLoginException)
        {
            WatermarkPasswordReg = "Существующий логин";
        }
        catch (NotValidException)
        {
            WatermarkPasswordReg = "невалидный пароль";
        }
    }
}