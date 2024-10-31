using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediLinkCB.Models
{
    public class UserReportViewModel
    {
        public int TotalUserCount { get; set; }
        public int DoctorCount { get; set; }
        public int ClerkCount { get; set; }
        public int NurseCount { get; set; }
    }
}
