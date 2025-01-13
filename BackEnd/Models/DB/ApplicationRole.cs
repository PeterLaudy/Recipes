using System;

using Microsoft.AspNetCore.Identity;

namespace Recepten.Models.DB
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public const string AdminRole = "ADMIN";
        public const string EditorRole = "EDIT";
        public const string EmailVerifiedRole = "EMAILVERIFIED";
    }
}