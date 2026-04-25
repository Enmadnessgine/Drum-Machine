using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drum_Machine.Data.Entities
{
    public class ProjectEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "New Beat";
        public int BPM { get; set; } = 120;

        public int UserId { get; set; }
        public virtual List<TrackEntity> Tracks { get; set; } = new();

        public virtual ExportedTrack? ExportedTrack { get; set; }
    }
}
