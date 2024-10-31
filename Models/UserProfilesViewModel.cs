using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MediLinkCB.Models
{
    public class UserProfilesViewModel
    {
        public int UserID { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$", 
            ErrorMessage = "Password must be at least 8 characters long, contain at least one number, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        public string UserType { get; set; }

        [Required]
        public bool IsActive { get; set; }
        public string OTP { get; set; }
        public Nullable<System.DateTime> OTPExpiry { get; set; }
        public string Specialisation { get; set; }

    }
}