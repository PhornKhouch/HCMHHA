namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HisPendingAppSalary")]
    public partial class HisPendingAppSalary
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InYear { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InMonth { get; set; }

        [Column(TypeName = "date")]
        public DateTime FromDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime ToDate { get; set; }

        public bool IsLock { get; set; }
    }
}
