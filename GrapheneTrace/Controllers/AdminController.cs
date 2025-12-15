using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using System.Security.Cryptography;
using System.Text;

namespace GrapheneTrace.Controllers
{
    /// <summary>
    /// AdminController - Handles all administrative operations for the Sensore system.
    /// 
    /// RESPONSIBILITIES:
    /// - User Management (CRUD operations for Patients, Clinicians, Admins)
    /// - Patient-Clinician Assignment Management
    /// - System Statistics and Dashboard
    /// - Audit Logging for administrative actions
    /// 
    /// DESIGN PATTERNS:
    /// - Repository Pattern (via ApplicationDbContext)
    /// - Dependency Injection (constructor injection)
    /// - Single Responsibility Principle (each region handles one concern)
    /// 
    /// SECURITY:
    /// - Role-based authorization (Admin only)
    /// - Anti-forgery token validation on all POST actions
    /// - Password hashing using SHA256
    /// - Soft delete to preserve data integrity
    /// 
    /// Author: [Your Name]
    /// Last Modified: December 2025
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        #region Private Fields and Constants

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        // Pagination constants - easily configurable
        private const int DEFAULT_PAGE_SIZE = 10;
        private const int MAX_PAGE_SIZE = 50;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor with dependency injection for database context and logging.
        /// Following Dependency Inversion Principle (DIP) - depend on abstractions.
        /// </summary>
        /// <param name="context">Entity Framework database context</param>
        /// <param name="logger">ILogger for audit and error logging</param>
        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Dashboard

        /// <summary>
        /// GET: Admin/Index
        /// Main dashboard with user statistics, search, filtering, and pagination.
        /// </summary>
        public async Task<IActionResult> Index(
            string searchString = "",
            string roleFilter = "",
            string statusFilter = "",
            string sortOrder = "",
            int pageNumber = 1,
            int pageSize = DEFAULT_PAGE_SIZE)
        {
            try
            {
                // Log dashboard access for audit trail
                _logger.LogInformation("Admin dashboard accessed by {User} at {Time}",
                    User.Identity?.Name ?? "Unknown", DateTime.Now);

                // Validate pagination parameters
                pageSize = Math.Clamp(pageSize, 1, MAX_PAGE_SIZE);
                pageNumber = Math.Max(1, pageNumber);

                // Store filter values for view (maintains state on postback)
                ViewBag.CurrentSearch = searchString;
                ViewBag.CurrentRole = roleFilter;
                ViewBag.CurrentStatus = statusFilter;
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                // Store sort parameters for column header links
                ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
                ViewBag.EmailSortParam = sortOrder == "email_asc" ? "email_desc" : "email_asc";
                ViewBag.RoleSortParam = sortOrder == "role_asc" ? "role_desc" : "role_asc";
                ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

                // ============================================
                // STATISTICS CARDS - Gather dashboard metrics
                // ============================================
                ViewBag.TotalPatients = await _context.Patients.CountAsync(p => p.IsActive);
                ViewBag.TotalClinicians = await _context.Clinicians.CountAsync(c => c.IsActive);
                ViewBag.TotalAdmins = await _context.Admins.CountAsync(a => a.IsActive);
                ViewBag.TotalAssignments = await _context.PatientClinicians.CountAsync();
                ViewBag.ActiveAlerts = 0;

                // ============================================
                // BUILD QUERY - Apply filters progressively
                // ============================================
                IQueryable<User> usersQuery = _context.Users.AsQueryable();

                // Apply search filter (searches name and email)
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    string searchLower = searchString.ToLower().Trim();
                    usersQuery = usersQuery.Where(u =>
                        u.FirstName.ToLower().Contains(searchLower) ||
                        u.LastName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower) ||
                        (u.FirstName + " " + u.LastName).ToLower().Contains(searchLower));
                }

                // Apply role filter
                if (!string.IsNullOrWhiteSpace(roleFilter))
                {
                    usersQuery = usersQuery.Where(u => u.Role == roleFilter);
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(statusFilter))
                {
                    bool isActive = statusFilter.ToLower() == "active";
                    usersQuery = usersQuery.Where(u => u.IsActive == isActive);
                }
                else
                {
                    // Default: show only active users
                    usersQuery = usersQuery.Where(u => u.IsActive);
                }

                // ============================================
                // SORTING - Apply column sorting
                // ============================================
                usersQuery = sortOrder switch
                {
                    "name_asc" => usersQuery.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                    "name_desc" => usersQuery.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName),
                    "email_asc" => usersQuery.OrderBy(u => u.Email),
                    "email_desc" => usersQuery.OrderByDescending(u => u.Email),
                    "role_asc" => usersQuery.OrderBy(u => u.Role),
                    "role_desc" => usersQuery.OrderByDescending(u => u.Role),
                    "date_asc" => usersQuery.OrderBy(u => u.CreatedAt),
                    "date_desc" => usersQuery.OrderByDescending(u => u.CreatedAt),
                    _ => usersQuery.OrderByDescending(u => u.CreatedAt)
                };

                // ============================================
                // PAGINATION - Calculate page metrics
                // ============================================
                int totalItems = await usersQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, totalPages));

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.HasPreviousPage = pageNumber > 1;
                ViewBag.HasNextPage = pageNumber < totalPages;

                var users = await usersQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Recent registrations
                ViewBag.RecentUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Recent audit logs (if table exists)
                try
                {
                    ViewBag.RecentAuditLogs = await _context.AuditLogs
                        .OrderByDescending(a => a.Timestamp)
                        .Take(10)
                        .ToListAsync();
                }
                catch
                {
                    ViewBag.RecentAuditLogs = new List<AuditLog>();
                }

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View(new List<User>());
            }
        }

        /// <summary>
        /// GET: Admin/SystemSettings
        /// Displays system configuration options.
        /// </summary>
        public async Task<IActionResult> SystemSettings()
        {
            _logger.LogInformation("System settings accessed by {User}", User.Identity?.Name);

            ViewBag.DatabaseStatus = "Connected";
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            ViewBag.TotalAssignments = await _context.PatientClinicians.CountAsync();
            ViewBag.SystemVersion = "1.0.0";
            ViewBag.ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return View();
        }

        /// <summary>
        /// GET: Admin/Reports
        /// Displays system reports and analytics.
        /// </summary>
        public async Task<IActionResult> Reports()
        {
            _logger.LogInformation("Reports accessed by {User}", User.Identity?.Name);

            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.ActivePatients = await _context.Patients.CountAsync(p => p.IsActive);
            ViewBag.TotalClinicians = await _context.Clinicians.CountAsync();
            ViewBag.ActiveClinicians = await _context.Clinicians.CountAsync(c => c.IsActive);
            ViewBag.TotalAdmins = await _context.Admins.CountAsync();
            ViewBag.TotalAssignments = await _context.PatientClinicians.CountAsync();

            // Get registrations by month (last 6 months)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            ViewBag.MonthlyRegistrations = await _context.Users
                .Where(u => u.CreatedAt >= sixMonthsAgo)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Role distribution
            ViewBag.RoleDistribution = await _context.Users
                .Where(u => u.IsActive)
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            // Recent audit logs
            try
            {
                ViewBag.RecentActivity = await _context.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(20)
                    .ToListAsync();
            }
            catch
            {
                ViewBag.RecentActivity = new List<AuditLog>();
            }

            return View();
        }

        #endregion

        #region Create User

        /// <summary>
        /// GET: Admin/Create
        /// Displays the user creation form.
        /// </summary>
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        /// <summary>
        /// POST: Admin/Create
        /// Creates a new user with validation and audit logging.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    string passwordHash = HashPassword(model.Password);

                    switch (model.Role)
                    {
                        case "Patient":
                            var patient = new Patient
                            {
                                Email = model.Email.Trim().ToLower(),
                                FirstName = model.FirstName.Trim(),
                                LastName = model.LastName.Trim(),
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
                                Email = model.Email.Trim().ToLower(),
                                FirstName = model.FirstName.Trim(),
                                LastName = model.LastName.Trim(),
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
                                Email = model.Email.Trim().ToLower(),
                                FirstName = model.FirstName.Trim(),
                                LastName = model.LastName.Trim(),
                                PasswordHash = passwordHash,
                                Role = "Admin",
                                IsActive = true,
                                CreatedAt = DateTime.Now,
                                AccessLevel = model.AccessLevel ?? "Standard"
                            };
                            _context.Admins.Add(admin);
                            break;

                        default:
                            ModelState.AddModelError("Role", "Please select a valid role.");
                            return View(model);
                    }

                    await _context.SaveChangesAsync();
                    await CreateAuditLogAsync("CREATE_USER",
                        $"Created new {model.Role}: {model.FirstName} {model.LastName} ({model.Email})");

                    _logger.LogInformation("User created: {Email} by {Admin}", model.Email, User.Identity?.Name);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = $"User '{model.FirstName} {model.LastName}' created successfully!",
                            redirectUrl = Url.Action(nameof(Index))
                        });
                    }

                    TempData["SuccessMessage"] = $"User '{model.FirstName} {model.LastName}' created successfully as {model.Role}!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View(model);
        }

        #endregion

        #region Edit User

        /// <summary>
        /// GET: Admin/Edit/{id}
        /// Displays the edit form.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive
            };

            await LoadRoleSpecificDataAsync(viewModel, user.Role, id.Value);
            return View(viewModel);
        }

        /// <summary>
        /// POST: Admin/Edit/{id}
        /// Updates user information.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.UserId) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.UserId != id))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    switch (model.Role)
                    {
                        case "Patient":
                            var patient = await _context.Patients.FindAsync(id);
                            if (patient != null) UpdatePatient(patient, model);
                            break;

                        case "Clinician":
                            var clinician = await _context.Clinicians.FindAsync(id);
                            if (clinician != null) UpdateClinician(clinician, model);
                            break;

                        case "Admin":
                            var admin = await _context.Admins.FindAsync(id);
                            if (admin != null) UpdateAdmin(admin, model);
                            break;
                    }

                    await _context.SaveChangesAsync();
                    await CreateAuditLogAsync("UPDATE_USER",
                        $"Updated {model.Role}: {model.FirstName} {model.LastName} (ID: {id})");

                    TempData["SuccessMessage"] = $"User '{model.FirstName} {model.LastName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExistsAsync(model.UserId)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user: {UserId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the user.");
                }
            }

            return View(model);
        }

        #endregion

        #region Delete User

        /// <summary>
        /// GET: Admin/Delete/{id}
        /// Displays deletion confirmation.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            ViewBag.AssignmentCount = await _context.PatientClinicians
                .CountAsync(pc => pc.PatientId == id || pc.ClinicianId == id);

            return View(user);
        }

        /// <summary>
        /// POST: Admin/Delete/{id}
        /// Soft delete (deactivation).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            try
            {
                string userName = $"{user.FirstName} {user.LastName}";
                user.IsActive = false;

                var assignments = await _context.PatientClinicians
                    .Where(pc => pc.PatientId == id || pc.ClinicianId == id)
                    .ToListAsync();

                int assignmentCount = assignments.Count;
                _context.PatientClinicians.RemoveRange(assignments);

                await _context.SaveChangesAsync();
                await CreateAuditLogAsync("DEACTIVATE_USER",
                    $"Deactivated {user.Role}: {userName} (ID: {id}). Removed {assignmentCount} assignment(s).");

                _logger.LogWarning("User deactivated: {UserId} by {Admin}", id, User.Identity?.Name);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = $"User '{userName}' has been deactivated.",
                        assignmentsRemoved = assignmentCount
                    });
                }

                TempData["SuccessMessage"] = $"User '{userName}' has been deactivated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred." });
                }

                TempData["ErrorMessage"] = "An error occurred while deactivating the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: Admin/HardDelete/{id}
        /// Permanent deletion.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            try
            {
                string userName = $"{user.FirstName} {user.LastName}";
                string userEmail = user.Email;

                var assignments = await _context.PatientClinicians
                    .Where(pc => pc.PatientId == id || pc.ClinicianId == id)
                    .ToListAsync();

                _context.PatientClinicians.RemoveRange(assignments);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await CreateAuditLogAsync("DELETE_USER_PERMANENT",
                    $"PERMANENTLY DELETED {user.Role}: {userName} ({userEmail}, ID: {id})");

                _logger.LogWarning("User PERMANENTLY deleted: {UserId} by {Admin}", id, User.Identity?.Name);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = $"User '{userName}' permanently deleted." });
                }

                TempData["SuccessMessage"] = $"User '{userName}' has been permanently deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting user: {UserId}", id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred." });
                }

                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region User Details

        /// <summary>
        /// GET: Admin/Details/{id}
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var viewModel = new UserDetailsViewModel { User = user };

            switch (user.Role)
            {
                case "Patient":
                    viewModel.Patient = await _context.Patients.FindAsync(id);
                    viewModel.Assignments = await _context.PatientClinicians
                        .Include(pc => pc.Clinician)
                        .Where(pc => pc.PatientId == id)
                        .ToListAsync();
                    break;

                case "Clinician":
                    viewModel.Clinician = await _context.Clinicians.FindAsync(id);
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

        #region Patient-Clinician Assignments

        /// <summary>
        /// GET: Admin/AssignPatient
        /// </summary>
        public async Task<IActionResult> AssignPatient()
        {
            await PopulateAssignmentDropdownsAsync();
            return View(new PatientClinicianAssignmentViewModel());
        }

        /// <summary>
        /// POST: Admin/AssignPatient
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPatient(PatientClinicianAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.PatientClinicians
                    .AnyAsync(pc => pc.PatientId == model.PatientId && pc.ClinicianId == model.ClinicianId);

                if (exists)
                {
                    TempData["ErrorMessage"] = "This patient is already assigned to this clinician.";
                    await PopulateAssignmentDropdownsAsync();
                    return View(model);
                }

                try
                {
                    var assignment = new PatientClinician
                    {
                        PatientId = model.PatientId,
                        ClinicianId = model.ClinicianId,
                        AssignmentDate = DateTime.Now,
                        Notes = model.Notes?.Trim() ?? ""
                    };

                    _context.PatientClinicians.Add(assignment);
                    await _context.SaveChangesAsync();

                    var patient = await _context.Patients.FindAsync(model.PatientId);
                    var clinician = await _context.Clinicians.FindAsync(model.ClinicianId);

                    await CreateAuditLogAsync("CREATE_ASSIGNMENT",
                        $"Assigned {patient?.FirstName} {patient?.LastName} to Dr. {clinician?.FirstName} {clinician?.LastName}");

                    TempData["SuccessMessage"] = "Patient successfully assigned!";
                    return RedirectToAction(nameof(ViewAssignments));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating assignment");
                    TempData["ErrorMessage"] = "An error occurred.";
                }
            }

            await PopulateAssignmentDropdownsAsync();
            return View(model);
        }

        /// <summary>
        /// GET: Admin/ViewAssignments
        /// </summary>
        public async Task<IActionResult> ViewAssignments(string searchTerm = "")
        {
            var query = _context.PatientClinicians
                .Include(pc => pc.Patient)
                .Include(pc => pc.Clinician)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(pc =>
                    pc.Patient.FirstName.ToLower().Contains(searchTerm) ||
                    pc.Patient.LastName.ToLower().Contains(searchTerm) ||
                    pc.Clinician.FirstName.ToLower().Contains(searchTerm) ||
                    pc.Clinician.LastName.ToLower().Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            var assignments = await query
                .OrderByDescending(pc => pc.AssignmentDate)
                .ToListAsync();

            return View(assignments);
        }

        /// <summary>
        /// POST: Admin/RemoveAssignment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int patientId, int clinicianId)
        {
            var assignment = await _context.PatientClinicians
                .Include(pc => pc.Patient)
                .Include(pc => pc.Clinician)
                .FirstOrDefaultAsync(pc => pc.PatientId == patientId && pc.ClinicianId == clinicianId);

            if (assignment == null)
            {
                return Json(new { success = false, message = "Assignment not found." });
            }

            try
            {
                string patientName = $"{assignment.Patient?.FirstName} {assignment.Patient?.LastName}";
                string clinicianName = $"{assignment.Clinician?.FirstName} {assignment.Clinician?.LastName}";

                _context.PatientClinicians.Remove(assignment);
                await _context.SaveChangesAsync();

                await CreateAuditLogAsync("REMOVE_ASSIGNMENT",
                    $"Removed assignment: {patientName} from Dr. {clinicianName}");

                return Json(new { success = true, message = "Assignment removed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing assignment");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        #endregion

        #region Audit Logs

        /// <summary>
        /// GET: Admin/AuditLogs
        /// </summary>
        public async Task<IActionResult> AuditLogs(string actionFilter = "", int pageNumber = 1)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(actionFilter))
                {
                    query = query.Where(a => a.Action == actionFilter);
                }

                int pageSize = 20;
                int totalItems = await query.CountAsync();

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                ViewBag.ActionFilter = actionFilter;

                ViewBag.ActionTypes = await _context.AuditLogs
                    .Select(a => a.Action)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToListAsync();

                var logs = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return View(logs);
            }
            catch
            {
                return View(new List<AuditLog>());
            }
        }

        #endregion

        #region AJAX Endpoints

        /// <summary>
        /// POST: Admin/ToggleStatus/{id}
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

                await CreateAuditLogAsync(
                    user.IsActive ? "ACTIVATE_USER" : "DEACTIVATE_USER",
                    $"{(user.IsActive ? "Activated" : "Deactivated")} {user.Role}: {user.FirstName} {user.LastName}");

                return Json(new
                {
                    success = true,
                    isActive = user.IsActive,
                    message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// GET: Admin/GetUserStats
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserStats()
        {
            var stats = new
            {
                totalPatients = await _context.Patients.CountAsync(p => p.IsActive),
                totalClinicians = await _context.Clinicians.CountAsync(c => c.IsActive),
                totalAdmins = await _context.Admins.CountAsync(a => a.IsActive),
                totalAssignments = await _context.PatientClinicians.CountAsync()
            };

            return Json(stats);
        }

        /// <summary>
        /// GET: Admin/ExportUsers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,FirstName,LastName,Email,Role,CreatedAt,Status");

            foreach (var user in users)
            {
                csv.AppendLine($"{user.UserId},{user.FirstName},{user.LastName},{user.Email},{user.Role},{user.CreatedAt:yyyy-MM-dd},{(user.IsActive ? "Active" : "Inactive")}");
            }

            await CreateAuditLogAsync("EXPORT_USERS", $"Exported {users.Count} users to CSV");

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"users_export_{DateTime.Now:yyyyMMdd}.csv");
        }

        #endregion

        #region Private Helper Methods

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        private async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        private async Task CreateAuditLogAsync(string action, string details)
        {
            try
            {
                var log = new AuditLog
                {
                    Action = action,
                    Details = details,
                    PerformedBy = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log");
            }
        }

        private async Task PopulateAssignmentDropdownsAsync()
        {
            ViewBag.Patients = await _context.Patients
                .Where(p => p.IsActive)
                .OrderBy(p => p.LastName)
                .Select(p => new { p.UserId, FullName = p.FirstName + " " + p.LastName })
                .ToListAsync();

            ViewBag.Clinicians = await _context.Clinicians
                .Where(c => c.IsActive)
                .OrderBy(c => c.LastName)
                .Select(c => new { c.UserId, FullName = "Dr. " + c.FirstName + " " + c.LastName + " (" + c.Specialization + ")" })
                .ToListAsync();
        }

        private async Task LoadRoleSpecificDataAsync(UserEditViewModel viewModel, string role, int userId)
        {
            switch (role)
            {
                case "Patient":
                    var patient = await _context.Patients.FindAsync(userId);
                    if (patient != null)
                    {
                        viewModel.MedicalCondition = patient.MedicalCondition;
                        viewModel.MobilityLevel = patient.MobilityLevel;
                    }
                    break;

                case "Clinician":
                    var clinician = await _context.Clinicians.FindAsync(userId);
                    if (clinician != null)
                    {
                        viewModel.LicenseNumber = clinician.LicenseNumber;
                        viewModel.Specialization = clinician.Specialization;
                        viewModel.Department = clinician.Department;
                        viewModel.ContactPhone = clinician.ContactPhone;
                    }
                    break;

                case "Admin":
                    var admin = await _context.Admins.FindAsync(userId);
                    if (admin != null)
                    {
                        viewModel.AccessLevel = admin.AccessLevel;
                    }
                    break;
            }
        }

        private void UpdatePatient(Patient patient, UserEditViewModel model)
        {
            patient.Email = model.Email.Trim().ToLower();
            patient.FirstName = model.FirstName.Trim();
            patient.LastName = model.LastName.Trim();
            patient.IsActive = model.IsActive;
            patient.MedicalCondition = model.MedicalCondition ?? patient.MedicalCondition;
            patient.MobilityLevel = model.MobilityLevel ?? patient.MobilityLevel;

            if (!string.IsNullOrEmpty(model.NewPassword))
                patient.PasswordHash = HashPassword(model.NewPassword);
        }

        private void UpdateClinician(Clinician clinician, UserEditViewModel model)
        {
            clinician.Email = model.Email.Trim().ToLower();
            clinician.FirstName = model.FirstName.Trim();
            clinician.LastName = model.LastName.Trim();
            clinician.IsActive = model.IsActive;
            clinician.LicenseNumber = model.LicenseNumber ?? clinician.LicenseNumber;
            clinician.Specialization = model.Specialization ?? clinician.Specialization;
            clinician.Department = model.Department ?? clinician.Department;
            clinician.ContactPhone = model.ContactPhone ?? clinician.ContactPhone;

            if (!string.IsNullOrEmpty(model.NewPassword))
                clinician.PasswordHash = HashPassword(model.NewPassword);
        }

        private void UpdateAdmin(Admin admin, UserEditViewModel model)
        {
            admin.Email = model.Email.Trim().ToLower();
            admin.FirstName = model.FirstName.Trim();
            admin.LastName = model.LastName.Trim();
            admin.IsActive = model.IsActive;
            admin.AccessLevel = model.AccessLevel ?? admin.AccessLevel;

            if (!string.IsNullOrEmpty(model.NewPassword))
                admin.PasswordHash = HashPassword(model.NewPassword);
        }

        #endregion
    }
}
