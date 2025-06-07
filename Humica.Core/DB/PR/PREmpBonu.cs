namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("PREmpBonus")]
    public partial class PREmpBonu
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TranNo { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string EmpCode { get; set; }

        [StringLength(100)]
        public string EmpName { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string BonCode { get; set; }

        [StringLength(100)]
        public string BonDescription { get; set; }

        public long? InYear { get; set; }

        public int? InMonth { get; set; }

        public DateTime? TranDate { get; set; }

        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int? LCK { get; set; }

        [StringLength(30)]
        public string CreateBy { get; set; }

        public DateTime? CreateOn { get; set; }

        [StringLength(30)]
        public string ChangedBy { get; set; }

        public DateTime? ChangedOn { get; set; }

        [Column(TypeName = "date")]
        public DateTime Period { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FromDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ToDate { get; set; }

        public int Status { get; set; }
    }
}
