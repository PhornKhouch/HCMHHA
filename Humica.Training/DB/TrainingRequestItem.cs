﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Humica.Training.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class TrainingRequestItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int LineItem { get; set; }
        public string RequestCode { get; set; }
        public string StaffID { get; set; }
        public string InvCode { get; set; }
        public string ChangeStaffID { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public string StaffName { get; set; }
        public string Program { get; set; }
        public string Course { get; set; }
        public Nullable<int> Group { get; set; }
        public string Status { get; set; }
        public string ReStatus { get; set; }
        public string MGStatus { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> HireDate { get; set; }
        public string Position { get; set; }
        public Nullable<int> StaffLevel { get; set; }
        public string NatID { get; set; }
        public string PhoneNo { get; set; }
        public decimal WorkExperience { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public bool IsDLRRead { get; set; }
        public bool IsNCXRead { get; set; }
    }
}
