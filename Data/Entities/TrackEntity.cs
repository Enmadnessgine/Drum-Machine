using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drum_Machine.Data.Entities
{
    public class TrackEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SamplePath { get; set; } = string.Empty;
        public double Volume { get; set; } = 1.0;
        public string StepsData { get; set; } = "0000000000000000";
        public int ProjectId { get; set; }
    }
}
