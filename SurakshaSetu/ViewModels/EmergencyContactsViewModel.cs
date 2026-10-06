// using System.Collections.Generic;
// using SurakshaSetu.Models;

// namespace SurakshaSetu.ViewModels
// {
//     public class EmergencyContactsViewModel
//     {
//         public List<EmergencyContact> Contacts { get; set; } = new List<EmergencyContact>();
//         public List<string> Categories { get; set; } = new List<string>();
//         public SOSAlert ActiveSOS { get; set; }
//     }
// }



using System.Collections.Generic;
using SurakshaSetu.Models;
using System.Linq;
namespace SurakshaSetu.ViewModels
{
    public class EmergencyContactsViewModel
    {
        // All active emergency/support contacts
        public List<EmergencyContact> Contacts { get; set; } = new();

        // Categories used by the filter
        public List<string> Categories { get; set; } = new();

        // Currently active SOS request
        public SOSAlert? ActiveSOS { get; set; }

        // Emergency contacts only
        public List<EmergencyContact> EmergencyContacts { get; set; } = new();

        // Security contacts
        public List<EmergencyContact> SecurityContacts { get; set; } = new();

        // Medical contacts
        public List<EmergencyContact> MedicalContacts { get; set; } = new();

        // Women's safety contacts
        public List<EmergencyContact> WomenSafetyContacts { get; set; } = new();

        // Hostel contacts
        public List<EmergencyContact> HostelContacts { get; set; } = new();

        // Counselling contacts
        public List<EmergencyContact> CounsellingContacts { get; set; } = new();

        // Grievance contacts
        public List<EmergencyContact> GrievanceContacts { get; set; } = new();

        // Cyber safety contacts
        public List<EmergencyContact> CyberSafetyContacts { get; set; } = new();

        // Student welfare contacts
        public List<EmergencyContact> StudentWelfareContacts { get; set; } = new();

        // Transport contacts
        public List<EmergencyContact> TransportContacts { get; set; } = new();

        // Indicates whether there is currently an active SOS
        public bool HasActiveSOS => ActiveSOS != null;

        // Number of available contacts
        public int TotalContacts => Contacts.Count;

        // Number of 24x7 contacts
        public int TwentyFourSevenContacts =>
            Contacts.Count(c => c.Is24x7);
    }
}