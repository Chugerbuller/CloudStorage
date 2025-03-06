using Avalonia.Media.Imaging;
using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Models
{
    public class FileForList : CloudStoreUiListItem
    {
        public FileForList(FileModel file) : base(file.Name, ItemType.File, file.Extension)
        {
            File = file;
            if (System.IO.File.Exists($"Assets\\{file.Extension}.ico"))
            {
                base.ImageSource = new Bitmap($"Assets\\{file.Extension}.ico");
            }
            else
            {
                base.ImageSource = new Bitmap($"Assets\\file.ico");
            }
        }
        public FileModel File { get; init; }
        public string Name { get => File.Name; set => File.Name = value; }
        public ItemType Type { get; } = ItemType.File;
        public string Extension => File.Extension;
        public Bitmap ImageSource { get; set; }
    }
}