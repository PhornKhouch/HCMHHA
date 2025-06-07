namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HISGenBonusFirstPayment")]
    public partial class HISGenBonusFirstPayment
    {
        [Key]
        public long TranNo { get; set; }

        public long? InYear { get; set; }

        public int? InMonth { get; set; }

        [StringLength(10)]
        public string EmpCode { get; set; }

        [StringLength(10)]
        public string BonusCode { get; set; }

        [StringLength(100)]
        public string BonusDesc { get; set; }

        [StringLength(100)]
        public string OthDesc { get; set; }

        public bool? TaxAble { get; set; }

        public decimal? WorkedDay { get; set; }

        public decimal? ActualWorkedDay { get; set; }

        public decimal? RatePerDay { get; set; }

        public decimal? BonusAM { get; set; }

        [StringLength(30)]
        public string CreateBy { get; set; }

        public DateTime? CreateOn { get; set; }

        [StringLength(15)]
        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }

        [StringLength(50)]
        public string Reckey { get; set; }

        public bool? FringTax { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FromDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ToDate { get; set; }
    }
}
