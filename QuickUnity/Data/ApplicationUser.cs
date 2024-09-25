using Microsoft.AspNetCore.Identity;
using QuickUnity.Data.Tables;

namespace QuickUnity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ProfileRow? Profile { get; set; }
}