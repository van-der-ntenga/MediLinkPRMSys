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
        public int MedicalHistoryID { get; set; }
        public int UserID { get; set; }
        public string PatientID { get; set; }
        public Nullable<int> AppointmentID { get; set; }
        public Nullable<int> PrescriptionID { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string AppointmentReason { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> AppointmentDateTime { get; set; }
        public Nullable<bool> FollowUp { get; set; }
    
        public virtual Appointment Appointment { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual UserProfile UserProfile { get; set; }
        public virtual Prescription Prescription { get; set; }
    }
}
