using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudStore.UI.ViewModels
{
    public class LoginAndRegistrationViewModel : ViewModelBase
    {
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

        public ReactiveCommand<Unit, Unit> AutorizationCommand { get; }
        public ReactiveCommand<Unit, Unit> RegistrationCommand { get; }

        public LoginAndRegistrationViewModel()
        {
            AutorizationCommand = ReactiveCommand.Create(() =>
            {
                LoginPartVisibility = !LoginPartVisibility;
                RegistrationPartVisibility = !RegistrationPartVisibility;
            });
            RegistrationCommand = AutorizationCommand;
        }
    }
}