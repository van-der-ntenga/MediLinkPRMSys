using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MediLinkCB.Models
{
    public class PatientResidentialAddressNOKViewModel
    {

        public string SaID { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public Nullable<int> Age { get; set; }
        public Nullable<decimal> PatientHeight { get; set; }
        public Nullable<decimal> PatientWeight { get; set; }
        public string Disability { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public string HomeLang { get; set; }
        public string MaritalStatus { get; set; }
        public string EmailAddress { get; set; }
        public string CellNumber { get; set; }
        // address
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        [Required]
        public string Province { get; set; }
        public string NOKFirstName { get; set; }
        public string NOKLastName { get; set; }
        public string Relationship { get; set; }
        public string NOKCellNumber { get; set; }

    }
}