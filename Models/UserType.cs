using System;
using System.ComponentModel.DataAnnotations;

namespace Campingapp_24.Models
{
    public class UserType
    {
        [Key]
        public int UserTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string UserType_Name { get; set; }
    }
}
