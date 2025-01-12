using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.Account
{
    public class RegisterUserModel
    {
        // Parameterless constructor needed for the framework.
        public RegisterUserModel() { }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string EMailAddress { get; set; }
    }
}