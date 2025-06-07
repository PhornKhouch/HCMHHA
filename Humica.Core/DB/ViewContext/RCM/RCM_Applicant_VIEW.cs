namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class RCM_Applicant_VIEW
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string VacNo { get; set; }

        public decimal? ExpectSalary { get; set; }

        [StringLength(40)]
        public string AllName { get; set; }

        [StringLength(60)]
        public string OthAllName { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(6)]
        public string Gender { get; set; }

        [StringLength(250)]
        public string POB { get; set; }

        [StringLength(20)]
        public string Phone1 { get; set; }

        [StringLength(250)]
        public string PermanentAddr { get; set; }

        [StringLength(250)]
        public string CurAddr { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string ApplicantID { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime ApplyDate { get; set; }

        public DateTime? DOB { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Key]
        [Column(Order = 4)]
        public bool IsHired { get; set; }

        [StringLength(100)]
        public string CurStage { get; set; }

        [StringLength(100)]
        public string Sourcedby { get; set; }

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
    }
}
