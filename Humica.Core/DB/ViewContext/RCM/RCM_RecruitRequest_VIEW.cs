namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class RCM_RecruitRequest_VIEW
    {
        public int? NoOfRecruit { get; set; }

        public DateTime? DocDate { get; set; }

        [StringLength(20)]
        public string DocType { get; set; }

        public DateTime? ExpectedDate { get; set; }

        [StringLength(15)]
        public string RecruitType { get; set; }

        public decimal? ProposedSalaryFrom { get; set; }

        public decimal? ProposedSalaryTo { get; set; }

        public DateTime? RequestedDate { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string RequestNo { get; set; }

        [StringLength(10)]
        public string Status { get; set; }

        public string StatusAPP { get; set; }
        [Key]
        [Column(Order = 1)]
        public int? ApproveLevel { get; set; }

        [StringLength(30)]
        public string Employee { get; set; }

        [StringLength(150)]
        public string ApproverName { get; set; }

        [StringLength(140)]
        public string AckedBy { get; set; }

        [StringLength(5)]
        public string AppTitle { get; set; }

        [StringLength(140)]
        public string RequestedBy { get; set; }

        [StringLength(5)]
        public string TitleReq { get; set; }

        [StringLength(100)]
        public string WorkingType { get; set; }

        [StringLength(50)]
        public string StaffType { get; set; }

        [StringLength(100)]
        public string Branch { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Section { get; set; }

        [StringLength(100)]
        public string JobLevel { get; set; }

        public string CheckBy { get; set; }
    }
}
