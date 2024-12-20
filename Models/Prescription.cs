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
    
    public partial class Prescription
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Prescription()
        {
            this.MedicalHistories = new HashSet<MedicalHistory>();
            this.Medications = new HashSet<Medication>();
        }
    
        public Nullable<int> UserID { get; set; }
        public string PatientID { get; set; }
        public int PrescriptionID { get; set; }
        public Nullable<int> AppointmentID { get; set; }
        public bool IsCollected { get; set; }
        public System.DateTime PrescriptionDateTime { get; set; }
    
        public virtual Appointment Appointment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MedicalHistory> MedicalHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Medication> Medications { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
