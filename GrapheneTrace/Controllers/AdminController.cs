using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using System.Security.Cryptography;
using System.Text;

namespace GrapheneTrace.Controllers
{
    /// <summary>
    /// AdminController handles all administrative operations including:
    /// - Dashboard display with user statistics
    /// - User CRUD operations (Create, Read, Update, Delete)
    /// - Patient-Clinician assignment management
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Inject database context via dependency injection
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Dashboard

        /// <summary>
        /// GET: Admin/Index
        /// Displays the main admin dashboard with statistics and user list
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Gather statistics for dashboard cards
            ViewBag.TotalPatients = await _context.Patients.CountAsync(p => p.IsActive);
            ViewBag.TotalClinicians = await _context.Clinicians.CountAsync(c => c.IsActive);
            ViewBag.TotalAdmins = await _context.Admins.CountAsync(a => a.IsActive);
            ViewBag.ActiveAlerts = 0; // Placeholder for future alert system

            // Get all active users for the management table
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Get recent registrations (last 5 users)
            ViewBag.RecentUsers = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(users);
        }

        #endregion

        #region Create User

        /// <summary>
        /// GET: Admin/Create
        /// Displays the user creation form
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Admin/Create
        /// Processes the user creation form submission
        /// Creates appropriate user type based on selected role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the password for security
                    string passwordHash = HashPassword(model.Password);

                    // Create appropriate user type based on role selection
                    switch (model.Role)
                    {
                        case "Patient":
                            var patient = new Patient
                            {
                                Email = model.Email,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                PasswordHash = passwordHash,
                                Role = "Patient",
                                IsActive = true,
                                CreatedAt = DateTime.Now,
                                MedicalCondition = model.MedicalCondition ?? "Not specified",
                                MobilityLevel = model.MobilityLevel ?? "Normal"
                            };
                            _context.Patients.Add(patient);
                            break;

                        case "Clinician":
                            var clinician = new Clinician
                            {
                                Email = model.Email,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                PasswordHash = passwordHash,
                                Role = "Clinician",
                                IsActive = true,
                                CreatedAt = DateTime.Now,
                                LicenseNumber = model.LicenseNumber ?? "N/A",
                                Specialization = model.Specialization ?? "General",
                                Department = model.Department ?? "General Medicine",
                                ContactPhone = model.ContactPhone ?? "Not provided"
                            };
                            _context.Clinicians.Add(clinician);
                            break;

                        case "Admin":
                            var admin = new Admin
                            {
                                Email = model.Email,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                PasswordHash = passwordHash,
                                Role = "Admin",
                                IsActive = true,
                                CreatedAt = DateTime.Now,
                                AccessLevel = model.AccessLevel ?? "Standard"
                            };
                            _context.Admins.Add(admin);
                            break;

                        default:
                            ModelState.AddModelError("Role", "Invalid role selected.");
                            return View(model);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User '{model.FirstName} {model.LastName}' created successfully as {model.Role}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log exception and show error message
                    ModelState.AddModelError("", $"Error creating user: {ex.Message}");
                }
            }

            return View(model);
        }

        #endregion

        #region Edit User

        /// <summary>
        /// GET: Admin/Edit/5
        /// Displays the edit form for a specific user
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Create view model with existing user data
            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive
            };

            // Load role-specific data
            switch (user.Role)
            {
                case "Patient":
                    var patient = await _context.Patients.FindAsync(id);
                    if (patient != null)
                    {
                        viewModel.MedicalCondition = patient.MedicalCondition;
                        viewModel.MobilityLevel = patient.MobilityLevel;
                    }
                    break;

                case "Clinician":
                    var clinician = await _context.Clinicians.FindAsync(id);
                    if (clinician != null)
                    {
                        viewModel.LicenseNumber = clinician.LicenseNumber;
                        viewModel.Specialization = clinician.Specialization;
                        viewModel.Department = clinician.Department;
                        viewModel.ContactPhone = clinician.ContactPhone;
                    }
                    break;

                case "Admin":
                    var admin = await _context.Admins.FindAsync(id);
                    if (admin != null)
                    {
                        viewModel.AccessLevel = admin.AccessLevel;
                    }
                    break;
            }

            return View(viewModel);
        }

        /// <summary>
        /// POST: Admin/Edit/5
        /// Processes the user edit form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            // Check if email is taken by another user
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "This email is already in use by another user.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update based on user role
                    switch (model.Role)
                    {
                        case "Patient":
                            var patient = await _context.Patients.FindAsync(id);
                            if (patient != null)
                            {
                                patient.Email = model.Email;
                                patient.FirstName = model.FirstName;
                                patient.LastName = model.LastName;
                                patient.IsActive = model.IsActive;
                                patient.MedicalCondition = model.MedicalCondition ?? patient.MedicalCondition;
                                patient.MobilityLevel = model.MobilityLevel ?? patient.MobilityLevel;

                                // Update password only if provided
                                if (!string.IsNullOrEmpty(model.NewPassword))
                                {
                                    patient.PasswordHash = HashPassword(model.NewPassword);
                                }
                            }
                            break;

                        case "Clinician":
                            var clinician = await _context.Clinicians.FindAsync(id);
                            if (clinician != null)
                            {
                                clinician.Email = model.Email;
                                clinician.FirstName = model.FirstName;
                                clinician.LastName = model.LastName;
                                clinician.IsActive = model.IsActive;
                                clinician.LicenseNumber = model.LicenseNumber ?? clinician.LicenseNumber;
                                clinician.Specialization = model.Specialization ?? clinician.Specialization;
                                clinician.Department = model.Department ?? clinician.Department;
                                clinician.ContactPhone = model.ContactPhone ?? clinician.ContactPhone;

                                if (!string.IsNullOrEmpty(model.NewPassword))
                                {
                                    clinician.PasswordHash = HashPassword(model.NewPassword);
                                }
                            }
                            break;

                        case "Admin":
                            var admin = await _context.Admins.FindAsync(id);
                            if (admin != null)
                            {
                                admin.Email = model.Email;
                                admin.FirstName = model.FirstName;
                                admin.LastName = model.LastName;
                                admin.IsActive = model.IsActive;
                                admin.AccessLevel = model.AccessLevel ?? admin.AccessLevel;

                                if (!string.IsNullOrEmpty(model.NewPassword))
                                {
                                    admin.PasswordHash = HashPassword(model.NewPassword);
                                }
                            }
                            break;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User '{model.FirstName} {model.LastName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExists(model.UserId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                }
            }

            return View(model);
        }

        #endregion

        #region Delete User

        /// <summary>
        /// GET: Admin/Delete/5
        /// Displays confirmation page for user deletion
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        /// <summary>
        /// POST: Admin/Delete/5
        /// Performs soft delete (sets IsActive = false)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // Soft delete - set IsActive to false instead of removing
                user.IsActive = false;

                // Also remove any patient-clinician assignments
                var assignments = await _context.PatientClinicians
                    .Where(pc => pc.PatientId == id || pc.ClinicianId == id)
                    .ToListAsync();

                _context.PatientClinicians.RemoveRange(assignments);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User '{user.FirstName} {user.LastName}' has been deactivated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: Admin/HardDelete/5
        /// Permanently removes user from database (use with caution)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // Remove any patient-clinician assignments first
                var assignments = await _context.PatientClinicians
                    .Where(pc => pc.PatientId == id || pc.ClinicianId == id)
                    .ToListAsync();

                _context.PatientClinicians.RemoveRange(assignments);

                // Permanently remove user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User '{user.FirstName} {user.LastName}' has been permanently deleted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region View User Details

        /// <summary>
        /// GET: Admin/Details/5
        /// Displays detailed information about a specific user
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            // Create view model with all user details
            var viewModel = new UserDetailsViewModel
            {
                User = user
            };

            // Load role-specific data
            switch (user.Role)
            {
                case "Patient":
                    viewModel.Patient = await _context.Patients.FindAsync(id);
                    // Get assigned clinicians
                    viewModel.Assignments = await _context.PatientClinicians
                        .Include(pc => pc.Clinician)
                        .Where(pc => pc.PatientId == id)
                        .ToListAsync();
                    break;

                case "Clinician":
                    viewModel.Clinician = await _context.Clinicians.FindAsync(id);
                    // Get assigned patients
                    viewModel.Assignments = await _context.PatientClinicians
                        .Include(pc => pc.Patient)
                        .Where(pc => pc.ClinicianId == id)
                        .ToListAsync();
                    break;

                case "Admin":
                    viewModel.Admin = await _context.Admins.FindAsync(id);
                    break;
            }

            return View(viewModel);
        }

        #endregion

        #region Patient-Clinician Assignment

        /// <summary>
        /// GET: Admin/AssignPatient
        /// Displays the patient-clinician assignment form
        /// </summary>
        public async Task<IActionResult> AssignPatient()
        {
            // Populate dropdowns with active patients and clinicians from database
            ViewBag.Patients = await _context.Patients
                .Where(p => p.IsActive)
                .Select(p => new { p.UserId, FullName = p.FirstName + " " + p.LastName })
                .ToListAsync();

            ViewBag.Clinicians = await _context.Clinicians
                .Where(c => c.IsActive)
                .Select(c => new { c.UserId, FullName = "Dr. " + c.FirstName + " " + c.LastName + " - " + c.Specialization })
                .ToListAsync();

            return View();
        }

        /// <summary>
        /// POST: Admin/AssignPatient
        /// Creates a new patient-clinician assignment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPatient(PatientClinicianAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate assignment
                var existingAssignment = await _context.PatientClinicians
                    .FirstOrDefaultAsync(pc =>
                        pc.PatientId == model.PatientId &&
                        pc.ClinicianId == model.ClinicianId);

                if (existingAssignment != null)
                {
                    TempData["ErrorMessage"] = "This patient is already assigned to this clinician.";
                    return RedirectToAction(nameof(AssignPatient));
                }

                try
                {
                    var assignment = new PatientClinician
                    {
                        PatientId = model.PatientId,
                        ClinicianId = model.ClinicianId,
                        AssignmentDate = DateTime.Now,
                        Notes = model.Notes ?? ""
                    };

                    _context.PatientClinicians.Add(assignment);
                    await _context.SaveChangesAsync();

                    // Get names for success message
                    var patient = await _context.Patients.FindAsync(model.PatientId);
                    var clinician = await _context.Clinicians.FindAsync(model.ClinicianId);

                    TempData["SuccessMessage"] = $"Patient '{patient?.FirstName} {patient?.LastName}' successfully assigned to Dr. {clinician?.FirstName} {clinician?.LastName}!";
                    return RedirectToAction(nameof(ViewAssignments));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating assignment: {ex.Message}";
                }
            }

            // Repopulate dropdowns on error
            ViewBag.Patients = await _context.Patients
                .Where(p => p.IsActive)
                .Select(p => new { p.UserId, FullName = p.FirstName + " " + p.LastName })
                .ToListAsync();

            ViewBag.Clinicians = await _context.Clinicians
                .Where(c => c.IsActive)
                .Select(c => new { c.UserId, FullName = "Dr. " + c.FirstName + " " + c.LastName + " - " + c.Specialization })
                .ToListAsync();

            return View(model);
        }

        /// <summary>
        /// GET: Admin/ViewAssignments
        /// Displays all current patient-clinician assignments
        /// </summary>
        public async Task<IActionResult> ViewAssignments()
        {
            var assignments = await _context.PatientClinicians
                .Include(pc => pc.Patient)
                .Include(pc => pc.Clinician)
                .OrderByDescending(pc => pc.AssignmentDate)
                .ToListAsync();

            return View(assignments);
        }

        /// <summary>
        /// POST: Admin/RemoveAssignment
        /// Removes a patient-clinician assignment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int patientId, int clinicianId)
        {
            var assignment = await _context.PatientClinicians
                .FirstOrDefaultAsync(pc =>
                    pc.PatientId == patientId &&
                    pc.ClinicianId == clinicianId);

            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Assignment not found.";
                return RedirectToAction(nameof(ViewAssignments));
            }

            try
            {
                _context.PatientClinicians.Remove(assignment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Assignment removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error removing assignment: {ex.Message}";
            }

            return RedirectToAction(nameof(ViewAssignments));
        }

        #endregion

        #region Toggle User Status

        /// <summary>
        /// POST: Admin/ToggleStatus/5
        /// Toggles user active/inactive status (AJAX endpoint)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            try
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    isActive = user.IsActive,
                    message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hashes password using SHA256 for secure storage
        /// Note: For production, use BCrypt or Argon2 instead
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Checks if a user exists in the database
        /// </summary>
        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        #endregion
    }
}
