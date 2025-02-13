using System;
using System.ComponentModel.DataAnnotations;

namespace Assignment1.Model
{
    public class UserProfile
    {
        [Key]
        public string UserId { get; set; }  // Links to IdentityUser

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string NRIC { get; set; } // Make sure this is encrypted

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(1000)]
        public string WhoAmI { get; set; }

        public string ResumePath { get; set; }
    }
}
