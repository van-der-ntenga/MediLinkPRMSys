//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MediLinkCB.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MedicalHistory
    {
        public int UserID { get; set; }
        public string SaID { get; set; }
        public string MedicationID { get; set; }
        public string PatientType { get; set; }
        public System.DateTime DiagnosisDate { get; set; }
        public string Allergies { get; set; }
        public string Condition { get; set; }
        public string Surgeries { get; set; }
        public System.DateTime LastUpdated { get; set; }
    
        public virtual Medication Medication { get; set; }
        public virtual UserProfile UserProfile { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
