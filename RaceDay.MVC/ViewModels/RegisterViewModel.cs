using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; } = "Participant";
    }
}