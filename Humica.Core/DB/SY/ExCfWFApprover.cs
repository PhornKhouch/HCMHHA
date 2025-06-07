namespace Humica.Core.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ExCfWFApprover")]
    public partial class ExCfWFApprover
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string WFObject { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string Branch { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public string Employee { get; set; }

        [StringLength(150)]
        public string EmployeeName { get; set; }

        public bool? IsSkip { get; set; }

        public int ApproveLevel { get; set; }

        public bool IsSelected { get; set; }
    }
}
