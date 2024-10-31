using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediLinkCB.Models
{
    public class PatientSummaryViewModel
    {
        // Gender
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int OtherGenderCount { get; set; }

        // Race
        public int BlackCount { get; set; }
        public int WhiteCount { get; set; }
        public int AsianCount { get; set; }
        public int ColouredCount { get; set; }

        // Age Groups
        public int AgeGroup0To12 { get; set; }
        public int AgeGroup13To19 { get; set; }
        public int AgeGroup20To35 { get; set; }
        public int AgeGroup36To60 { get; set; }
        public int AgeGroup60Plus { get; set; }

        // Marital Status
        public int SingleCount { get; set; }
        public int MarriedCount { get; set; }
        public int WidowedCount { get; set; }

        // Total Patients
        public int TotalPatientCount { get; set; }
    }


}