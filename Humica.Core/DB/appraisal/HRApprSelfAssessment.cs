namespace Humica.Core.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HRApprSelfAssessment")]
    public partial class HRApprSelfAssessment
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string QuestionCode { get; set; }
        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string AppraiselType { get; set; }
        public string Description1 { get; set; }

        public bool IsQCM { get; set; }

        public string Description2 { get; set; }

        public int InOrder { get; set; }
    }
}
