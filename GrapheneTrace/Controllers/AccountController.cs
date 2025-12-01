using Microsoft.AspNetCore.Mvc;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace GrapheneTrace.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // If they are already logged in, redirect them to Home
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Incorrect email or password. Please try again.";
                return View(model);
            }

            string hashedPassword = HashPassword(model.Password);

            // 1. Check if user is an Admin
            var admin = _context.Admins.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword);
            if (admin != null)
            {
                await CreateSession(admin.Email, "Admin", admin.FirstName);
                return RedirectToAction("Index", "Admin");
            }

            // 2. Check if user is a Clinician
            var clinician = _context.Clinicians.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword);
            if (clinician != null)
            {
                await CreateSession(clinician.Email, "Clinician", clinician.FirstName);
                return RedirectToAction("Index", "Home"); // Change to Clinician Dashboard later
            }

            // 3. Check if user is a Patient
            var patient = _context.Patients.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword);
            if (patient != null)
            {
                await CreateSession(patient.Email, "Patient", patient.FirstName);
                return RedirectToAction("Index", "Patient"); // Goes to Patient Dashboard
            }

            
            ModelState.AddModelError("", "Invalid login attempt.");

            // THIS IS THE LINE YOU WERE MISSING:
            ViewBag.Error = "Incorrect email or password. Please try again.";

            return View(model);
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Helper to Create the Login Cookie
        private async Task CreateSession(string email, string role, string name)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        // Helper to Match your Seed Data Hashing
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}