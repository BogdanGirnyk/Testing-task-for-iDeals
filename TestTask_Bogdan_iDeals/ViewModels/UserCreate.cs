using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestTask_Bogdan_iDeals.ViewModels
{
    public class UserCreate
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Surname { get; set; }

        [Required]
        [Display(Name="E-mail")]
        [StringLength(125)]
        [EmailAddress(ErrorMessage = "The email address is not valid")]
        public string Email { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "Mobile phone")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone number")]
        [RegularExpression(@"\(?\+[0-9]{1,3}\)? ?-?[0-9]{1,3} ?-?[0-9]{3,5} ?-?[0-9]{4}( ?-?[0-9]{3})?", ErrorMessage = "Invalid Phone number")]
        public string Mobilephone { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Repeat password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match")]
        public string Passwordrepeat { get; set; }
    }
}