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

    public partial class TrainingAnouncementItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int LineItem { get; set; }
        public string AnounceCode { get; set; }
        public string TR { get; set; }
        public string DealerName { get; set; }
        public string Status { get; set; }
        public bool IsDlrRead { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> ReceivedDate { get; set; }
        public int CountRead { get; set; }
        public string CountReadtxt { get; set; }
    }
}
