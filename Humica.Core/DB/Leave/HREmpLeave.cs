namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HREmpLeave")]
    public partial class HREmpLeave
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public long TranNo { get; set; }

        [StringLength(10)]
        public string EmpCode { get; set; }

        public int Increment { get; set; }

        [StringLength(10)]
        public string LeaveCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime FromDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime ToDate { get; set; }

        public double? NoDay { get; set; }

        public double? NoRest { get; set; }

        public double? NoPH { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(250)]
        public string Reason { get; set; }

        [StringLength(200)]
        public string Remark1 { get; set; }

        public int? DSApprove { get; set; }

        [StringLength(10)]
        public string HODCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RejectDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ApproveDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RequestDate { get; set; }

        public int? LCK { get; set; }

        [StringLength(30)]
        public string CreateBy { get; set; }

        public DateTime? CreateOn { get; set; }

        [StringLength(30)]
        public string ChangedBy { get; set; }

        public DateTime? ChangedOn { get; set; }

        [StringLength(50)]
        public string DocNo { get; set; }

        public DateTime? BackToWorkOn { get; set; }

        [StringLength(1000)]
        public string TaskHand_Over { get; set; }

        [StringLength(30)]
        public string APP1Code { get; set; }

        public DateTime? APP1Date { get; set; }

        public int? APP1Status { get; set; }

        [StringLength(1000)]
        public string APP1Comments { get; set; }

        [StringLength(30)]
        public string APP2Code { get; set; }

        public DateTime? APP2Date { get; set; }

        public int? APP2Status { get; set; }

        [StringLength(1000)]
        public string APP2Comments { get; set; }

        [StringLength(30)]
        public string APP3Code { get; set; }

        public DateTime? APP3Date { get; set; }

        public int? APP3Status { get; set; }

        [StringLength(1000)]
        public string APP3Comments { get; set; }

        [StringLength(30)]
        public string APP4Code { get; set; }

        public DateTime? APP4Date { get; set; }

        public int? APP4Status { get; set; }

        [StringLength(1000)]
        public string APP4Comments { get; set; }

        [StringLength(30)]
        public string APP5Code { get; set; }

        public DateTime? APP5Date { get; set; }

        public int? APP5Status { get; set; }

        [StringLength(1000)]
        public string APP5Commrnts { get; set; }

        public bool? Urgent { get; set; }

        public bool TranType { get; set; }

        [StringLength(500)]
        public string Attachment { get; set; }
        [StringLength(20)]
        public string Units { get; set; }
    }
}
