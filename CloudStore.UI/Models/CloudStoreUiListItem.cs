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
        public string Name { get; set; }
        public ItemType Type { get; }
        public string Extension { get; }
    }
}
