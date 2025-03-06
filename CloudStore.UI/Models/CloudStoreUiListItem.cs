using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Models
{
    public enum ItemType
    {
        Directory,
        File
    }
    public class CloudStoreUiListItem
    {
        public CloudStoreUiListItem(string name, ItemType type, string extension)
        {
            Name = name;
            if (type == ItemType.Directory)
                Type = "Directory";
            else
                Type = "File";
            Extension = extension;
        }
        public string Name { get; set; }
        public string Type { get; }
        public string Extension { get; }
        public Bitmap ImageSource { get; init; }
    }
}
