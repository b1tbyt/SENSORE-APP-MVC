using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{
    /// <summary>
    /// Admin class - represents system administrators
    /// Inherits from User and adds admin-specific properties
    /// </summary>
    public class Admin : User
    {
        [StringLength(50)]
        [Display(Name = "Access Level")]
        public string AccessLevel { get; set; } = "Standard";
    }
}
