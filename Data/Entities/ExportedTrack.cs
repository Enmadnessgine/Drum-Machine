using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drum_Machine.Data.Entities
{
    public class ExportedTrack
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime ExportedAt { get; set; }
        public int ProjectId { get; set; }
    }
}
