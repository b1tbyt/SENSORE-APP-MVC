using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{
    /// <summary>
    /// ViewModel for creating new users
    /// Contains validation rules and role-specific optional fields
    /// </summary>
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; } = string.Empty;

        // Patient-specific fields
        [Display(Name = "Medical Condition")]
        public string? MedicalCondition { get; set; }

        [Display(Name = "Mobility Level")]
        public string? MobilityLevel { get; set; }

        // Clinician-specific fields
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        public string? Specialization { get; set; }

        public string? Department { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        // Admin-specific fields
        [Display(Name = "Access Level")]
        public string? AccessLevel { get; set; }
    }

    /// <summary>
    /// ViewModel for editing existing users
    /// Password is optional (only changed if provided)
    /// </summary>
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password (leave blank to keep current)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }

        public string Role { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Patient-specific fields
        [Display(Name = "Medical Condition")]
        public string? MedicalCondition { get; set; }

        [Display(Name = "Mobility Level")]
        public string? MobilityLevel { get; set; }

        // Clinician-specific fields
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        public string? Specialization { get; set; }

        public string? Department { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        // Admin-specific fields
        [Display(Name = "Access Level")]
        public string? AccessLevel { get; set; }
    }

    /// <summary>
    /// ViewModel for displaying user details
    /// Includes role-specific data and assignments
    /// </summary>
    public class UserDetailsViewModel
    {
        public User User { get; set; } = null!;
        public Patient? Patient { get; set; }
        public Clinician? Clinician { get; set; }
        public Admin? Admin { get; set; }
        public List<PatientClinician> Assignments { get; set; } = new List<PatientClinician>();
    }

    /// <summary>
    /// ViewModel for patient-clinician assignment
    /// </summary>
    public class PatientClinicianAssignmentViewModel
    {
        [Required(ErrorMessage = "Please select a patient")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Please select a clinician")]
        [Display(Name = "Clinician")]
        public int ClinicianId { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes (Optional)")]
        public string? Notes { get; set; }
    }
}
