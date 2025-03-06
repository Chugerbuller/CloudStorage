using Avalonia.Media.Imaging;
using CloudStore.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Models
{
    public class DirectoryForList : CloudStoreUiListItem
    {
        public DirectoryForList(string directory) : base(directory, ItemType.Directory, "")
        {
            Directory = directory;
            base.ImageSource = new Bitmap("Assets\\folder.ico");
        }

        public string Directory { get; init; }
        public string Name { get => Directory; set => Name = value; }
        public ItemType Type { get; } = ItemType.Directory;

        public string Extension => "";
    }
}