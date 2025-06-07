namespace Humica.Core.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class HR_OrgChart_View
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(50)]
        public string EmpCode { get; set; }

        [StringLength(140)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Designation { get; set; }

        [StringLength(50)]
        public string ReportingManager { get; set; }

        [StringLength(50)]
        public string ReportingID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string Branch { get; set; }

        [StringLength(500)]
        public string Images { get; set; }
        public string Department { get; set; }
        public string JobCode { get; set; }
    }
}
