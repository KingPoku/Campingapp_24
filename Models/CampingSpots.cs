using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class CampingSpots
    {
        [Key]
        public int SpotID { get; set; }


        [Required]
        [StringLength(100)]
        public string Spot_Name { get; set; }


        [Required]
        public decimal Price_Per_Night { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(20)]
        public string Zip_Code { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [ForeignKey("Owner")]
        public int OwnerId { get; set; }
        public Owner Owner { get; set; }

        public List<string> Amenities { get; set; }

        public bool IsBooked { get; set; }
        public DateTime? Check_In_Date { get; set; }
        public DateTime? Check_Out_Date { get; set; }
    }
}
