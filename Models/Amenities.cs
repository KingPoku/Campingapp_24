using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class Amenities
    {
        [Key]
        public int Amenities_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Amenities_Name { get; set; }
    }
}
