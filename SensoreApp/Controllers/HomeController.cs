using Microsoft.AspNetCore.Mvc;

namespace SensoreApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
