namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PRADVPay")]
    public partial class PRADVPay
    {
        [Key]
        public long TranNo { get; set; }

        [StringLength(10)]
        public string EmpCode { get; set; }

        [StringLength(100)]
        public string AllName { get; set; }

        public DateTime? TranDate { get; set; }

        public decimal? Amount { get; set; }

        public decimal? Balance { get; set; }

        [StringLength(200)]
        public string Reason { get; set; }

        [StringLength(200)]
        public string Remark { get; set; }

        public int? LCK { get; set; }

        [StringLength(30)]
        public string CreateBy { get; set; }

        public DateTime? CreateOn { get; set; }

        [StringLength(30)]
        public string ChangedBy { get; set; }

        public DateTime? ChangedOn { get; set; }
    }
}
