using Avalonia.Input;
using CloudStore.BL.Models;
using CloudStore.UI.Models;
using CloudStore.UI.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace CloudStore.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseable
    {
        private readonly ApiFileService _apiFileService;
        public ReactiveCommand<Unit, Unit> CommandCloseWindow { get; }

        public ObservableCollection<CloudStoreUiListItem?> FilesAndDirectorys { get; set; } = [];
        [Reactive]
        public string UserPath { get; set; }
        [Reactive]
        public CloudStoreUiListItem SelectedFileOrDirectory { get; set; }

        public event EventHandler? Closed;

        public User? User { get; set; }

        public MainWindowViewModel(User? user)
        {
            User = user;
            CommandCloseWindow = ReactiveCommand.Create(() => Closed(this, new EventArgs()));
            UserPath = @"\";
           if (User != null)
            {
                _apiFileService = new(User);
                //FilesAndDirectorys.AddRange(_apiFileService.GetStartingScreenItems().Result!);
            }
            else
                FilesAndDirectorys = new();
        }
    }
}