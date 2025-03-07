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
        public bool ButtonDowloadIsVisible { 
            get
            {
                if (Type == "Directory")
                    return false;
                else 
                    return true;
            }
        }
        public bool ButtonEditIsVisible
        {
            get
            {
                if (Type == "Directory")
                    return false;
                else
                    return true;
            }
        }
        public bool ButtonDeleteIsVisible
        {
            get
            {
                if (Type == "Directory")
                    return false;
                else
                    return true;
            }
        }
        public bool ButtonGoToIsVisible
        {
            get
            {
                if (Type == "Directory")
                    return true;
                else
                    return false;
            }
        }
    }
}
