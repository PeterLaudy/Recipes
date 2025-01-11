using System;
using Microsoft.AspNetCore.Identity;

namespace Recepten.Models.DB
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public static readonly string AdminRole = "ADMIN";
        public static readonly string EditorRole = "EDIT";
        public static readonly string EmailVerifiedRole = "EMAILVERIFIED";
    }
}