namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HRMissionPlan")]
    public partial class HRMissionPlan
    {
        [Key]
        [StringLength(20)]
        public string MissionCode { get; set; }

        [StringLength(20)]
        public string Branch { get; set; }

        [StringLength(20)]
        public string EmpCode { get; set; }

        [StringLength(200)]
        public string PlannerName { get; set; }

        [StringLength(20)]
        public string MissionType { get; set; }

        [StringLength(150)]
        public string Position { get; set; }

        [Column(TypeName = "date")]
        public DateTime PlanDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime FromDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime ToDate { get; set; }

        public string WorkingPlan { get; set; }

        public int Member { get; set; }

        [StringLength(30)]
        public string TravelBy { get; set; }

        [StringLength(20)]
        public string Province { get; set; }

        public bool IsDriver { get; set; }

        [StringLength(200)]
        public string DriverName { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(20)]
        public string ReStatus { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? ChangedOn { get; set; }

        [StringLength(30)]
        public string ChangedBy { get; set; }
    }
}
