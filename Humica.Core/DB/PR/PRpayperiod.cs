namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("PRpayperiods")]
    public partial class PRpayperiod
    {
        [Key]
        public int PayPeriodId { get; set; }

        [StringLength(100)]
        public string AttendanceDesc { get; set; }

        [Column(TypeName = "date")]
        public DateTime AttendanceStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime AttendanceEndDate { get; set; }

        [StringLength(100)]
        public string SalaryDesc { get; set; }

        [Column(TypeName = "date")]
        public DateTime? SalaryStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? SalaryEndDate { get; set; }
    }
}
