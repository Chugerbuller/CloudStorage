using CloudStore.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Models
{
    public class FileForList : CloudStoreUiListItem
    {
        public FileForList(FileModel file)
        {
            File = file;
        }
        public FileModel File { get; init; }
        public string Name { get => File.Name; set => File.Name = value; }
        public ItemType Type { get; } = ItemType.File;

        public string Extension => File.Extension;
    }
}