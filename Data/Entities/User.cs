using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drum_Machine.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public virtual UserSettings Settings { get; set; } = null!;
        public virtual List<ProjectEntity> Projects { get; set; } = new();
        public virtual List<SampleEntity> Library { get; set; } = new();
    }

}
