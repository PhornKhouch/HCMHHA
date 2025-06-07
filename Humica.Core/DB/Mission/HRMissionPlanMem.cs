namespace Humica.Core.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HRMissionPlanMem")]
    public partial class HRMissionPlanMem
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string MissionCode { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string EmpCode { get; set; }

        [StringLength(200)]
        public string EmpName { get; set; }

        [StringLength(150)]
        public string Position { get; set; }
    }
}
