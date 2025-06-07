namespace Humica.Core.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HREmpJobDescription")]
	public partial class HREmpJobDescription
	{
		[Key]
		public int TranNo { get; set; }
		public int LineItem { get; set; }
		[StringLength(20)]
		public string CompanyCode { get; set; }
		[StringLength(15)]
		public string EmpCode { get; set; }

		public string JobResponsive { get; set; }

		public string JobDescription { get; set; }

		public string Attachment { get; set; }
		public string Document { get; set; }
		public bool? IsActive { get; set; }
	}
}
