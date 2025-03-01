using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudStore.BL.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Login { get; set; }
        public string Password { get; set; }
        public string? UserDirectory { get; set; }
        public string? ApiKey { get; set; }
        [JsonIgnore]
        public ICollection<FileModel> Files { get; set; }
    }
}
