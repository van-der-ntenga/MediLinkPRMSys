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
    
    public partial class NextOfKin
    {
        public string SaID { get; set; }
        public string NOKFirstName { get; set; }
        public string NOKLastName { get; set; }
        public string Relationship { get; set; }
        public string NOKCellNumber { get; set; }
    
        public virtual Patient Patient { get; set; }
    }
}