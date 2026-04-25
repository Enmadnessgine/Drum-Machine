using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drum_Machine.Data.Entities
{
    public class UserSettings
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public string Theme { get; set; } = "Dark";
        public double MasterVolume { get; set; } = 1.0;

        public virtual User User { get; set; } = null!;
    }
}