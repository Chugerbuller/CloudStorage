using Avalonia.Input;
using Avalonia.Threading;
using CloudStore.BL.Models;
using CloudStore.UI.Models;
using CloudStore.UI.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;

namespace CloudStore.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseable
    {
        private readonly ApiFileService _apiFileService;
        public ReactiveCommand<Unit, Unit> CommandCloseWindow { get; }
        public ReactiveCommand<Unit,Unit> SendFileCommand { get; }
        public ReactiveCommand<Unit,Unit> DeleteFileCommand { get; }
        public ReactiveCommand<Unit,Unit> ChangeFileCommand { get; }
        public ReactiveCommand<Unit,Unit> MakeDirectoryCommand { get; }

        public ObservableCollection<CloudStoreUiListItem?> FilesAndDirectorys { get; set; } = [];

        [Reactive]
        public string UserPath { get; set; }

        [Reactive]
        public CloudStoreUiListItem SelectedFileOrDirectory { get; set; }

        public event EventHandler? Closed;

        public User? User { get; set; }

        public MainWindowViewModel(User? user, List<CloudStoreUiListItem>? items)
        {
            User = user;
            CommandCloseWindow = ReactiveCommand.Create(() => Closed(this, new EventArgs()));
            UserPath = @"\";

            _apiFileService = new(User);
            
            FilesAndDirectorys = new(items);

            
        }
    }
}