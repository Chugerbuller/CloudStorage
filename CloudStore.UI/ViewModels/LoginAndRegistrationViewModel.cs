using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Input;
using CloudStore.BL.Models;
using CloudStore.UI.Configs;
using CloudStore.UI.Exceptions;
using CloudStore.UI.Models;
using CloudStore.UI.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudStore.UI.ViewModels;

public class LoginAndRegistrationViewModel : ViewModelBase, ICloseable
{
    private readonly ApiUserService _userService;

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
    public string WatermarkLogin { get; set; } = "������� �����";

    [Reactive]
    public string WatermarkPasswordReg { get; set; } = "������� ������";

    #endregion ReactiveProps

    #region ReactiveCommands

    public ReactiveCommand<Unit, Unit> AuthorizationCommand { get; }
    public ReactiveCommand<Unit, Unit> RegistrationCommand { get; }
    public ReactiveCommand<Unit, Unit> RegistrationUserCommand { get; }

    #endregion ReactiveCommands

    public event EventHandler? Closed;
    public User? User { get; private set; }
    public List<CloudStoreUiListItem>? Items { get; set; }

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
            User = await _userService.AuthorizeUser(Login, Password);
            if (User is not null)
            {
                var temp = new ApiFileService(User);
                Items = await temp.GetStartingScreenItems(); //Fix me ������ ��� ��������

                if (RememberMe)
                {
                    var userJsonByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(User));
                    using var fsUser = new FileStream("Configs\\UserConfig.json", FileMode.Truncate, FileAccess.Write);

                    await fsUser.WriteAsync(userJsonByte);

                    using var fsAppCfg = new FileStream("Configs\\ApplicationConfig.json", FileMode.Open, FileAccess.ReadWrite);
                    var appJson = await JsonSerializer.DeserializeAsync<ApplicationConfig>(fsAppCfg);

                    appJson.RememberUser = true;

                    var appJsonByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(appJson));
                    fsAppCfg.Dispose();
                    using var writeAppCfg = new FileStream("Configs\\ApplicationConfig.json", FileMode.Truncate, FileAccess.Write);
                    writeAppCfg.Write(appJsonByte);

                }
                
                Closed(this, new EventArgs());
            }
        }
        catch (ExistentLoginException)
        {
            WatermarkLogin = "������������ �����";
            return;
        }
        catch (PasswordException)
        {
            WatermarkLogin = "������������ ������";
            return;
        }
    }

    public async Task Registration()
    {
        if (!RegistrationPartVisibility)
            return;
        if (PasswordRegistration != PasswordRegistrationRepeat)
        {
            WatermarkPasswordReg = "������ ������ ���������";
            return;
        }
        try
        {
            User = await _userService.RegistrationUser(LoginRegistration, PasswordRegistration);
           
            if (User is not null)
            {
                var temp = new ApiFileService(User);
                Items = await temp.GetStartingScreenItems();  //Fix me ������ ��� ��������
                Closed(this, new EventArgs());
            }
                
        }
        catch (ExistentLoginException)
        {
            WatermarkPasswordReg = "������������ �����";
        }
        catch (NotValidException)
        {
            WatermarkPasswordReg = "���������� ������";
        }
    }
}