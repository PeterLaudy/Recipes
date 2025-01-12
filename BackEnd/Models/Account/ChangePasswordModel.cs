using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.Account
{
    public class ChangePasswordModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}