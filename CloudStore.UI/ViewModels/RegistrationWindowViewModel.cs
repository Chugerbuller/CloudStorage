using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudStore.UI.ViewModels
{
	public class RegistrationWindowViewModel : ReactiveObject
	{
        [Reactive]
        public string Login { get; set; }

        [Reactive]
        public string Password { get; set; }
    }
}