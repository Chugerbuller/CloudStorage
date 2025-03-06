using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CloudStore.UI.Models;
using CloudStore.UI.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;

namespace CloudStore.UI;

public partial class UCListItem
{
    public CloudStoreUiListItem Item { get; set; }

    public UCListItem(CloudStoreUiListItem item)
    {
        Item = item;
        FileNameBox.Text = item.Name;

        if (item is DirectoryForList folder)
        {
            ImageExt.Source = new Bitmap("Assets\\Folder.ico");
        }
        else if (item is FileForList file)
        {
            if (File.Exists($"Assets\\{file.Extension}.ico"))
                ImageExt.Source = new Bitmap($"Assets\\{file.Extension}.ico");
            else
                ImageExt.Source = new Bitmap($"Assets\\file.ico");
            var buttonDownload = new Button();
            buttonDownload.Content = new Image()
            {
                Source = new Bitmap($"Assets\\DownloadFile.ico")
            };
            var buttonRename = new Button();
            buttonDownload.Content = new Image()
            {
                Source = new Bitmap($"Assets\\RenameFile.ico")
            };
            var buttonDelete = new Button();
            buttonDownload.Content = new Image()
            {
                Source = new Bitmap(AssetLoader.Open(new Uri($"Assets\\DeleteFile.ico")))
            };
            ButtonPanel.Children.Add(buttonDownload);
            ButtonPanel.Children.Add(buttonRename);
            ButtonPanel.Children.Add(buttonDelete);
        }
        else
            throw new Exception("KAPEC");
    }
}