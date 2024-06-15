using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("CampingSpot")]
        public int SpotID { get; set; }

        [Required]
        public DateTime Check_In_Date { get; set; }

        [Required]
        public DateTime Check_Out_Date { get; set; }

        [Required]
        public decimal Total_Price { get; set; }
    }
}
