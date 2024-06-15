using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class CampingSpotImage
    {
        [Key]
        public int CampingSpotImage_ID { get; set; }

        [ForeignKey("CampingSpot")]
        public int SpotID { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; }
    }
}
