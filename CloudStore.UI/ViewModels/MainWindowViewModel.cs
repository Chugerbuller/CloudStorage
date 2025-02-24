using Avalonia.Input;
using CloudStore.BL.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Net.NetworkInformation;
using System.Reactive;

namespace CloudStore.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseable
    {
        [Reactive]
        public string Greeting { get; set; }

        public ReactiveCommand<Unit, Unit> CommandTest { get; }

        public event EventHandler? Closed;

        private User? _user;
        public User? User
        {
            get => _user;
            set 
            { 
                _user = value;
                Greeting = _user.Login;
            }
        }

        public MainWindowViewModel(User? user)
        {
            _user = user;
            CommandTest = ReactiveCommand.Create(() => Closed(this, new EventArgs()));
        }
    }
}