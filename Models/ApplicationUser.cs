using Microsoft.AspNetCore.Identity;

namespace SIOMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties if needed
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // Add other custom properties here
    }
}