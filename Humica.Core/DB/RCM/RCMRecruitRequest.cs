namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RCMRecruitRequest")]
    public partial class RCMRecruitRequest
    {
        [Key]
        [StringLength(10)]
        public string RequestNo { get; set; }

        [StringLength(10)]
        public string Branch { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(10)]
        public string POST { get; set; }

        [StringLength(10)]
        public string Status { get; set; }

        [StringLength(15)]
        public string RecruitType { get; set; }

        public int? NoOfRecruit { get; set; }

        [StringLength(15)]
        public string WorkingType { get; set; }

        public decimal? ProposedSalaryFrom { get; set; }

        public decimal? ProposedSalaryTo { get; set; }

        [StringLength(5)]
        public string Gender { get; set; }

        [StringLength(10)]
        public string JobLevel { get; set; }

        [StringLength(15)]
        public string TermEmp { get; set; }

        [StringLength(20)]
        public string RecruitFor { get; set; }

        [StringLength(20)]
        public string RequestedBy { get; set; }

        public DateTime? RequestedDate { get; set; }

        [StringLength(20)]
        public string AckedBy { get; set; }

        [StringLength(20)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? ExpectedDate { get; set; }

        [StringLength(10)]
        public string JDCode { get; set; }

        public string JobDesc { get; set; }

        [StringLength(10)]
        public string LineMg { get; set; }

        [StringLength(20)]
        public string LineMgPostition { get; set; }

        public DateTime? LineMgDate { get; set; }

        [StringLength(10)]
        public string DivHead { get; set; }

        [StringLength(10)]
        public string DivHeadPosition { get; set; }

        public DateTime? DivHeadDate { get; set; }

        [StringLength(10)]
        public string HRDirector { get; set; }

        [StringLength(20)]
        public string HRDirectorPosition { get; set; }

        public DateTime? HRDirectorDate { get; set; }

        [StringLength(10)]
        public string GenDirector { get; set; }

        [StringLength(20)]
        public string GenDirectorPosition { get; set; }

        public DateTime? GenDirectorDate { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [StringLength(30)]
        public string ChangedBy { get; set; }

        public DateTime? ChangedOn { get; set; }

        public decimal? ContPeriod { get; set; }

        public DateTime? ContFromDate { get; set; }

        public DateTime? ContToDate { get; set; }

        [StringLength(300)]
        public string Attachment { get; set; }

        [StringLength(10)]
        public string Sect { get; set; }

        public DateTime? DocDate { get; set; }

        [StringLength(20)]
        public string DocType { get; set; }

        [StringLength(10)]
        public string StaffType { get; set; }
        public string JobResponsibility { get; set; }
        public string JobRequirement { get; set; }
        [StringLength(20)]
        public string CheckBy { get; set; }
    }
}
