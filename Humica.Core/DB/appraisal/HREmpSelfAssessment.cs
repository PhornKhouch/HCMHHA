namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HREmpSelfAssessment")]
    public partial class HREmpSelfAssessment
    {
        [Key]
        [StringLength(30)]
        public string AssessmentCode { get; set; }
        [StringLength(10)]
        public string AppraiselType { get; set; }
        [StringLength(300)]
        public string AppraiselName { get; set; }

        [StringLength(30)]
        public string EmpCode { get; set; }

        [StringLength(200)]
        public string EmpName { get; set; }

        [StringLength(200)]
        public string Department { get; set; }

        [StringLength(200)]
        public string Position { get; set; }

        [Column(TypeName = "date")]
        public DateTime AssessmentDate { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
        [Column(TypeName = "date")]
        public DateTime ExpectedDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime Deadline { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [StringLength(30)]
        public string ChangedBy { get; set; }

        public DateTime? ChangedOn { get; set; }
        public bool? IsRead { get; set; }
    }
}
