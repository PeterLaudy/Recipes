using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.Account
{
    public class RegisterViewModel
    {
        // Parameterless constructor needed for the framework.
        public RegisterViewModel() { }

        [Required]
        public string Token { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string EMailAddress { get; set; }

        [Required, DataType(DataType.Password)] 
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Password), Compare(nameof(Password))] 
        public string ConfirmPassword { get; set; }
    }
}