using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Banana.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the BananaUser class
    public class BananaUser : IdentityUser
    {
        // public BananaUserGroup UserGroup { get; set; }
    }

    public enum BananaUserGroup
    {
        Normal = 0,
        Administrator = 10
    }
}
