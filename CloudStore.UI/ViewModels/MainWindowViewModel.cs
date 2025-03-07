using Avalonia.Controls;
using Avalonia.Input;
using CloudStore.BL.Models;
using CloudStore.UI.Models;
using CloudStore.UI.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace CloudStore.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseable
    {
        private readonly ApiFileService _apiFileService;
        public ReactiveCommand<Unit, Unit> ToPrevDirecotryCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> SendFileCommand { get; }
        public ReactiveCommand<Unit, Unit> DownloadFileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteFileCommand { get; }
        public ReactiveCommand<Unit, Unit> EditFileCommand { get; }
        public ReactiveCommand<Unit, Unit> MakeDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> MakeDirectoryShowCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> LogOutCommand { get; }

        public ObservableCollection<CloudStoreUiListItem?> FilesAndDirectorys { get; set; } = [];

        [Reactive]
        public string UserPath { get; set; } = "";
        [Reactive]
        public bool EnableEditFile { get; set; } = false;

        [Reactive]
        public CloudStoreUiListItem SelectedFileOrDirectory { get; set; }

        [Reactive]
        public bool MakeDirectoryVisibility { get; set; } = false;

        [Reactive]
        public string NewDirectory { get; set; } = "";

        public event EventHandler? Closed;

        public User? User { get; set; }

        public MainWindowViewModel(User? user)
        {
            User = user;
            CloseWindowCommand = ReactiveCommand.Create(() => Closed(this, new EventArgs()));

            ToPrevDirecotryCommand = ReactiveCommand.CreateFromTask(ToPrevDirecotry);
            GoToDirectoryCommand = ReactiveCommand.CreateFromTask(GoToDirectory);
            SendFileCommand = ReactiveCommand.CreateFromTask(UploadFile);
            DownloadFileCommand = ReactiveCommand.CreateFromTask(DownloadFile);
            LogOutCommand = ReactiveCommand.Create(LogOut);
            DeleteFileCommand = ReactiveCommand.CreateFromTask(DeleteFile);
            MakeDirectoryShowCommand = ReactiveCommand.Create(MakeDirectoryShow);
            MakeDirectoryCommand = ReactiveCommand.CreateFromTask(MakeDirectory);
            //EditFileCommand = ReactiveCommand.CreateFromTask(EditFile);
            _apiFileService = new(User);
            _initList();
        }

        public async Task GoToDirectory()
        {
            if (SelectedFileOrDirectory is DirectoryForList directory)
            {
                var newItems = await _apiFileService.GetItemsFromDirectory(Path.Combine(UserPath, directory.Directory));

                if (newItems == null)
                    return;
                FilesAndDirectorys.Clear();
                FilesAndDirectorys.AddRange(newItems);

                if (UserPath == "")
                    UserPath += directory.Directory;
                else
                    UserPath += $@"\{directory.Directory}";
            }
        }

        public async Task DeleteFile()
        {
            if (SelectedFileOrDirectory == null)
                return;
            if (SelectedFileOrDirectory is FileForList file)
            {
                if (await _apiFileService.DeleteFile(file.File))
                {
                    FilesAndDirectorys.Remove(file);
                }
                else
                    return;
            }
        }

        public async Task DownloadFile()
        {
            try
            {
                if (SelectedFileOrDirectory == null)
                    return;
                if (SelectedFileOrDirectory is FileForList file)
                {
                    var dialog = new OpenFolderDialog();

                    var directory = await dialog.ShowAsync(new Window());
                    if (directory is null)
                        return;

                    await _apiFileService.DownloadFile(file.File, directory);
                    return;
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
            return;
        }

        public async Task UploadFile()
        {
            var dialog = new OpenFileDialog();
            dialog.AllowMultiple = false;
            try
            {
                var directory = await dialog.ShowAsync(new Window());
                if (directory is null)
                    return;

                var res = await _apiFileService.UploadFile(directory[0], UserPath);
                if (res is null)
                    return;
                FilesAndDirectorys.Add(res);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
        }

        public void MakeDirectoryShow()
        {
            MakeDirectoryVisibility = !MakeDirectoryVisibility;
        }

        public async Task MakeDirectory()
        {
            var newDirectory = await _apiFileService.MakeDirectory($@"{UserPath}\{NewDirectory}");

            if (newDirectory is null)
                return;
            FilesAndDirectorys.Add(newDirectory);

            NewDirectory = "";
            MakeDirectoryVisibility = !MakeDirectoryVisibility;
        }

        public async Task ToPrevDirecotry()
        {
            if (UserPath == "")
                return;

            var directoryList = UserPath.Split('\\');
        
            var newDirectoryList = new string[directoryList.Length - 1];
            for (int i = 0; i <= directoryList.Length - 2; i++)
            {
                newDirectoryList[i] = directoryList[i];
            }
            var newDir = string.Join('\\', newDirectoryList);
            UserPath = newDir;

            var newItems = await _apiFileService.GetItemsFromDirectory(UserPath);

            if (newItems == null)
                return;
            FilesAndDirectorys.Clear();
            FilesAndDirectorys.AddRange(newItems);
        }

        private async void _initList()
        {
            var items = await _apiFileService.GetStartingScreenItems();
            FilesAndDirectorys.AddRange(items);
        }
        public async Task EditFile()
        {
        }
        public void LogOut()
        {
            /*  var appCfg = JsonSerializer.Deserialize<ApplicationConfig>(File.ReadAllText("Configs\\ApplicationConfig.json"));
              appCfg.RememberUser = false;
              var appJsonByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(appCfg));
              using var writeAppCfg = new FileStream("Configs\\ApplicationConfig.json", FileMode.Truncate, FileAccess.Write);
              writeAppCfg.Write(appJsonByte);

              Closed(this, new EventArgs());*/
        }
    }
}