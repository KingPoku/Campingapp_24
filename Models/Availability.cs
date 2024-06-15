using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class Availability
    {
        [Key]
        public int AvailabilityID { get; set; }

        [ForeignKey("CampingSpot")]
        public int SpotID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool Isbooked { get; set; }
    }
}
