using Microsoft.AspNetCore.Mvc;

namespace GrapheneTrace.Controllers
{
    /// <summary>
    /// HomeController - Landing page and public routes.
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If user is authenticated, redirect to their dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                return role switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Clinician" => RedirectToAction("Index", "Clinician"),
                    "Patient" => RedirectToAction("Index", "Patient"),
                    _ => View()
                };
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
