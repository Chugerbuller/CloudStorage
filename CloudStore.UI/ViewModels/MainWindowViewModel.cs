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
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseable
    {
        private readonly ApiFileService _apiFileService;
        public ReactiveCommand<Unit, Unit> ToPrevDirecotryCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> SendFileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ChangeFileCommand { get; }
        public ReactiveCommand<Unit, Unit> MakeDirectoryCommand { get; }

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
            CloseWindowCommand = ReactiveCommand.Create(() => Closed(this, new EventArgs()));
            UserPath = @"\";
            ToPrevDirecotryCommand = ReactiveCommand.Create(ToPrevDirecotry);

            _apiFileService = new(User);

            FilesAndDirectorys = new(items);
        }
        public async Task MakeDirectory()
        {

        }
        public void ToPrevDirecotry()
        {
            if (UserPath == @"\")
                return;

            var directoryList = UserPath.Split('/');
            var newDirectoryList = "";
            for (int i = 0; i < directoryList.Length - 1; i++)
            {
                newDirectoryList += directoryList[i];
            }

            UserPath = newDirectoryList;
        }
    }
}