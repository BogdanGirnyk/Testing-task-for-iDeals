using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TestTask_Bogdan_iDeals.ViewModels
{
    public class UserGetUpdate
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Suraname")]
        public string Surname { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required]
        [StringLength(15)]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone number")]
        [RegularExpression(@"\(?\+[0-9]{1,3}\)? ?-?[0-9]{1,3} ?-?[0-9]{3,5} ?-?[0-9]{4}( ?-?[0-9]{3})?", ErrorMessage = "Invalid Phone number")]
        [Display(Name = "Mobile phone")]
        public string Mobilephone { get; set; }
    }
}