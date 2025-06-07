namespace Humica.Training.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("TRTrainingAttendance")]
    public partial class TRTrainingAttendance
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TrainNo { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CalendarID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LineItem { get; set; }

        [StringLength(15)]
        public string EmpCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime InDate { get; set; }

        public bool IsAttend { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public DateTime CreatedOn { get; set; }

        [StringLength(15)]
        public string CreatedBy { get; set; }

        public DateTime? ChangedOn { get; set; }

        [StringLength(15)]
        public string ChangedBy { get; set; }
    }
}
