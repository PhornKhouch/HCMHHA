namespace Humica.Core.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HREmpBankAcc")]
    public partial class HREmpBankAcc
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string EmpCode { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LineItem { get; set; }

        [StringLength(50)]
        public string BankName { get; set; }

        [StringLength(50)]
        public string BankAccount { get; set; }

        public decimal Salary { get; set; }

        public bool? IsTax { get; set; }
    }
}
