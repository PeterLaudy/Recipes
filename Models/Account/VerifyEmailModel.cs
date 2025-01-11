using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.Account
{
    public class VerifyEmailModel
    {
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Token { get; set; }
    }
}