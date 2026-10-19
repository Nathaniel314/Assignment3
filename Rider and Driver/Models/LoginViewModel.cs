using System.ComponentModel.DataAnnotations;

namespace Rider_and_Driver.Models
{
    public class LoginViewModel
    {
        //View Model used specfically for the login page
        [Required(ErrorMessage = "Username is Required")]
        [Display(Name ="Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
