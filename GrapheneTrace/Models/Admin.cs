using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{

    public class Admin : User
    {
        [StringLength(50)]
        [Display(Name = "Access Level")]
        public string AccessLevel { get; set; } = "Standard";
    }
}
