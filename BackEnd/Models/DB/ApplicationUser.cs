using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace Recepten.Models.DB
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// Gets or sets the firstname for this user.
        /// </summary>
        [Required]
        [ProtectedPersonalData]
        public virtual string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the lastname for this user.
        /// </summary>
        [Required]
        [ProtectedPersonalData]
        public virtual string LastName { get; set; }
    }
}