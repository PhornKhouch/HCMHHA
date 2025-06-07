namespace Humica.Core.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HRKPIList")]
    public partial class HRKPIList
    {
        [Key]
        [StringLength(10)]
        public string Code { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [StringLength(250)]
        public string SecDescription { get; set; }
        [StringLength(20)]
        public string Department { get; set; }
        [StringLength(20)]
        public string Category { get; set; }
    }
}
