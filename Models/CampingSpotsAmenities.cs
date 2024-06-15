using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class CampingSpotsAmenities
    {
        [Key]
        public int CampingSpotsAmenitiesID { get; set; }

        [ForeignKey("CampingSpot")]
        public int SpotID { get; set; }

        [ForeignKey("Amenities")]
        public int Amenities_ID { get; set; }
    }
}
