using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class Reviews
    {
        [Key]
        public int ReviewID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("CampingSpot")]
        public int SpotID { get; set; }

        [Required]
        [StringLength(1000)]
        public string ReviewText { get; set; }
    }
}
