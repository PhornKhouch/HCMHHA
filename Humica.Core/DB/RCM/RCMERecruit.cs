namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RCMERecruit")]
    public partial class RCMERecruit
    {
        [Key]
        [StringLength(10)]
        public string JobID { get; set; }

        [StringLength(10)]
        public string HirePosition { get; set; }

        [StringLength(15)]
        public string Status { get; set; }

        public int PosterNo { get; set; }

        [StringLength(30)]
        public string Location { get; set; }

        public string ContactInfo { get; set; }

        [StringLength(100)]
        public string UploadPoster { get; set; }

        [StringLength(10)]
        public string VerifyBy { get; set; }

        [StringLength(10)]
        public string ReviewBy { get; set; }

        [StringLength(10)]
        public string AckBy { get; set; }

        public DateTime DatePosting { get; set; }

        public DateTime? DateLine { get; set; }

        [StringLength(20)]
        public string PersonInCharge { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [StringLength(50)]
        public string ChangedBy { get; set; }

        public DateTime? ChangedOn { get; set; }

        [StringLength(300)]
        
        public string Attachfile { get; set;}


        [StringLength(150)]
        public string MediaType { get; set; }
    }
}
