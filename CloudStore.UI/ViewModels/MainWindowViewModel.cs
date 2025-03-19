using Avalonia.Controls;
using Avalonia.Input;
using CloudStore.BL.Models;
using CloudStore.UI.Configs;
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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudStore.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseable
    {
        #region ReadonlyProps

        private readonly ApiFileService _apiFileService;

        #endregion ReadonlyProps

        #region ReactiveCommands

        public ReactiveCommand<Unit, Unit> ToPrevDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelNewFolderCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> SendFileCommand { get; }
        public ReactiveCommand<Unit, Unit> DownloadFileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteFileCommand { get; }
        public ReactiveCommand<Unit, Unit> EditFileCommand { get; }
        public ReactiveCommand<Unit, Unit> AvailableEditFileCommand { get; }
        public ReactiveCommand<Unit, Unit> MakeDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> MakeDirectoryShowCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> LogOutCommand { get; }

        #endregion ReactiveCommands

        #region ReactiveProps

        public ObservableCollection<CloudStoreUiListItem?> FilesAndDirectorys { get; set; } = [];

        [Reactive]
        public bool LoadVisibility { get; set; } = false;

        [Reactive] public string LoadFileText { get; set; } = "";
        [Reactive] public string fileSizeGradation { get; set; } = "";

        [Reactive]
        public int ProgressBarMax { get; set; } = 0;

        [Reactive]
        public int ProgressBarValue { get; set; } = 0;

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

        [Reactive]
        public string newFileName { get; set; } = "";

        #endregion ReactiveProps

        #region Events

        public event EventHandler? Closed;

        #endregion Events

        #region Props

        public User? User { get; set; }
        public IProgress<int> Progress { get; set; }
        private int bytesLoaded = 0;

        #endregion Props

        #region Ctor

        public MainWindowViewModel(User? user)
        {
            User = user;
            CloseWindowCommand = ReactiveCommand.Create(() => Closed(this, new EventArgs()));

            CancelEditCommand = ReactiveCommand.Create(CancelEdit);
            CancelNewFolderCommand = ReactiveCommand.Create(CancelNewFolder);
            AvailableEditFileCommand = ReactiveCommand.Create(MakeVisibleEdit);
            ToPrevDirectoryCommand = ReactiveCommand.CreateFromTask(ToPrevDirectory);
            GoToDirectoryCommand = ReactiveCommand.CreateFromTask(GoToDirectory);
            SendFileCommand = ReactiveCommand.CreateFromTask(UploadFile);
            DownloadFileCommand = ReactiveCommand.CreateFromTask(DownloadFile);
            LogOutCommand = ReactiveCommand.Create(LogOut);
            DeleteFileCommand = ReactiveCommand.CreateFromTask(DeleteFile);
            MakeDirectoryShowCommand = ReactiveCommand.Create(MakeDirectoryShow);
            MakeDirectoryCommand = ReactiveCommand.CreateFromTask(MakeDirectory);
            EditFileCommand = ReactiveCommand.CreateFromTask(EditDirOrFile);
            _apiFileService = new(User);
            Progress = new Progress<int>(i =>
            {
                bytesLoaded++;
                LoadFileText = $"{bytesLoaded}/{ProgressBarMax}";
                ProgressBarValue++;
            });
            _initList();
        }

        #endregion Ctor

        #region Methods

        public async Task GoToDirectory()
        {
            if (SelectedFileOrDirectory is DirectoryForList directory)
            {
                var newItems = await _apiFileService.GetItemsFromDirectoryAsync(Path.Combine(UserPath, directory.Directory));

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
                if (await _apiFileService.DeleteFileAsync(file.File))
                {
                    FilesAndDirectorys.Remove(file);
                }
                else
                    return;
            }
            else if (SelectedFileOrDirectory is DirectoryForList directory)
            {
                bool res;
                DirectoryForList? deleteDir;
                if (UserPath == "")
                    res = await _apiFileService.DeleteDirectoryAsync(directory.Name);
                else
                    res = await _apiFileService.DeleteDirectoryAsync(UserPath + "\\" + directory.Name);

                if (!res)
                    return;
                FilesAndDirectorys.Remove(directory);
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
                    var fileSize = await _apiFileService.CheckFileSizeAsync(file.File);
                    ProgressBarMax = Convert.ToInt32(fileSize / 1024 / 1024) * 2;
                    bytesLoaded = 0;
                    LoadVisibility = true;
                    if (fileSize >= 1024 * 1024 * 100)
                        await _apiFileService.DownloadLargeFileAsync(file.File, directory, Progress);
                    else
                        await _apiFileService.DownloadFileAsync(file.File, directory);
                    
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

                var filePath = directory[0];

                var fileInfo = new FileInfo(filePath);

                CloudStoreUiListItem? res;

                var fileSize = fileInfo.Length;
                LoadVisibility = true;
                if (fileSize / 1024 > 0)
                {
                    ProgressBarMax = Convert.ToInt32(fileSize / (10240 * 10));
                    fileSizeGradation = "100Kb";
                }
                else
                {
                    ProgressBarMax = Convert.ToInt32(fileSize);
                    fileSizeGradation = "b";
                }

                if (fileSize >= (1024 * 1024 * 100))
                    res = await _apiFileService.UploadLargeFile(filePath, UserPath, Progress);
                else
                    res = await _apiFileService.UploadFileAsync(filePath, UserPath);

                bytesLoaded = 0;
                LoadVisibility = false;
                ProgressBarValue = 0;

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
            DirectoryForList? newDirectory;
            if (UserPath == "")
                newDirectory = await _apiFileService.MakeDirectoryAsync($"{NewDirectory}");

            newDirectory = await _apiFileService.MakeDirectoryAsync($@"{UserPath}\{NewDirectory}");

            if (newDirectory is null)
                return;
            FilesAndDirectorys.Add(newDirectory);

            NewDirectory = "";
            MakeDirectoryVisibility = !MakeDirectoryVisibility;
        }

        public void CancelNewFolder()
        {
            MakeDirectoryVisibility = !MakeDirectoryVisibility;
            NewDirectory = "";
        }

        public void CancelEdit()
        {
            EnableEditFile = !EnableEditFile;
            newFileName = "";
        }

        public async Task ToPrevDirectory()
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

            var newItems = await _apiFileService.GetItemsFromDirectoryAsync(UserPath);

            if (newItems == null)
                return;
            FilesAndDirectorys.Clear();
            FilesAndDirectorys.AddRange(newItems);
        }

        private async void _initList()
        {
            var items = await _apiFileService.GetStartingScreenItemsAsync();
            FilesAndDirectorys.AddRange(items);
        }

        public async Task EditDirOrFile()
        {
            if (SelectedFileOrDirectory is FileForList file)
            {
                file.Name = newFileName + '.' + file.Extension;
                var updateFile = await _apiFileService.UpdateFileAsync(file.File);
                if (updateFile is null)
                    return;
                FilesAndDirectorys.Replace(file, updateFile);
                EnableEditFile = !EnableEditFile;
            }
            else if (SelectedFileOrDirectory is DirectoryForList directory)
            {
                DirectoryForList? updatedDir;
                if (UserPath == "")
                    updatedDir = await _apiFileService.ChangeDirectoryNameAsync(directory.Name, newFileName);
                else
                    updatedDir = await _apiFileService.ChangeDirectoryNameAsync(UserPath + '\\' + directory.Name, newFileName);

                if (updatedDir is null)
                    return;
                FilesAndDirectorys.Replace(directory, updatedDir);
                EnableEditFile = !EnableEditFile;
            }
            else
                return;
        }

        public void MakeVisibleEdit()
        {
            if (SelectedFileOrDirectory != null)
            {
                if (SelectedFileOrDirectory is FileForList file)
                    newFileName = file.Name.Split('.')[0];
                else if (SelectedFileOrDirectory is DirectoryForList directory)
                    newFileName = directory.Name.Split('\\')[^1];
            }
            else
                return;
            EnableEditFile = !EnableEditFile;
        }

        public void LogOut()
        {
            var appCfg = JsonSerializer.Deserialize<ApplicationConfig>(File.ReadAllText("Configs\\ApplicationConfig.json"));
            appCfg.RememberUser = false;
            var appJsonByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(appCfg));
            using var writeAppCfg = new FileStream("Configs\\ApplicationConfig.json", FileMode.Truncate, FileAccess.Write);
            writeAppCfg.Write(appJsonByte);

            Closed(this, new EventArgs());
        }

        #endregion Methods
    }
}