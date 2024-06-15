using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Campingapp_24.Models
{
    public class Owner
    {
        [Key]
        public int OwnerId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string First_Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Last_Name { get; set; }

    }
}
