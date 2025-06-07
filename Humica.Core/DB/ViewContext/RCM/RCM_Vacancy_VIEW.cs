namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class RCM_Vacancy_VIEW
    {
        public DateTime? DocDate { get; set; }

        [StringLength(30)]
        public string DocRef { get; set; }

        [StringLength(15)]
        public string VacancyType { get; set; }

        [Key]
        [StringLength(10)]
        public string Code { get; set; }

        [StringLength(15)]
        public string Status { get; set; }

        public DateTime? ClosedDate { get; set; }

        public int? AppApplied { get; set; }

        public DateTime? ProcessDate { get; set; }

        [StringLength(140)]
        public string ClosedBy { get; set; }

        [StringLength(140)]
        public string ProcessBy { get; set; }

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
