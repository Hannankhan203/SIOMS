using Microsoft.AspNetCore.Identity;

namespace SIOMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}