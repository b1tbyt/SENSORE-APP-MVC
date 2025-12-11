using System;
using System.Collections.Generic;

namespace SENSORE_APP.Models
{
    /// <summary>
    /// View model for patient profile page showing patient info, care team, and clinician assignment
    /// </summary>
    public class PatientProfileViewModel
    {
        // Patient Information
        public string PatientId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string DateOfBirth { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Current Health Status
        public string CurrentCondition { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public int MobilityLevel { get; set; } // 0 = Immobile, 1 = Limited, 2 = Mobile
        public string MobilityDescription => MobilityLevel switch
        {
            0 => "Immobile - Requires full assistance",
            1 => "Limited Mobility - Requires some assistance",
            2 => "Mobile - Can move independently",
            _ => "Unknown"
        };

        // Assigned Clinician
        public Clinician AssignedClinician { get; set; } = new();

        // Care Team Members
        public List<CareTeamMember> CareTeam { get; set; } = new();

        // Recent Activity
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; } = string.Empty;
        public int TotalMessagesCount { get; set; }
        public int TotalRepositionsLogged { get; set; }
    }

    /// <summary>
    /// Represents the assigned clinician
    /// </summary>
    public class Clinician
    {
        public string ClinicianId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Title { get; set; } = string.Empty; // Dr., RN, PT, etc.
        public string Specialty { get; set; } = string.Empty; // Wound Care, Physical Therapy, etc.
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;
        public string AvailabilityStatus { get; set; } = "Available"; // Available, Busy, Away, Offline
        public DateTime LastContact { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a care team member
    /// </summary>
    public class CareTeamMember
    {
        public string MemberId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Role { get; set; } = string.Empty; // Nurse, PT, OT, Dietitian, etc.
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public bool IsAvailable { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
        public string ProfileImageUrl { get; set; } = string.Empty;
    }
}
