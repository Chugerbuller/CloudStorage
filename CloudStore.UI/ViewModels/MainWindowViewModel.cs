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
        public ReactiveCommand<Unit, Unit> CommandCloseWindow { get; }

        public event EventHandler? Closed;        
        public User? User { get; set; }
        public MainWindowViewModel(User? user)
        {
            User = user;
            CommandCloseWindow = ReactiveCommand.Create(() => Closed(this, new EventArgs()));
        }
    }
}