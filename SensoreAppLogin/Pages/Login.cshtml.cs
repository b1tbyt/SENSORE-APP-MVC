using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SensoreAppLogin.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Role { get; set; } = "Patient";

        public IActionResult OnPost()
        {
            // Eğer e-posta boşsa hata döndür
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Email is required.");
                return Page();
            }

            // Role göre yönlendirme
            return Role switch
            {
                "Patient" => RedirectToPage("/Patient/Dashboard"),
                "Clinician" => RedirectToPage("/Clinician/Dashboard"),
                "Administrator" => RedirectToPage("/Admin/Dashboard"),
                _ => Page()
            };
        }
    }
}
