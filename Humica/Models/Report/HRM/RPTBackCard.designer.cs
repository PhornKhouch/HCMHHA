namespace Humica.Models.Report.HRM
{
    partial class RPTBackCard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RPTBackCard));
			DevExpress.DataAccess.Sql.StoredProcQuery storedProcQuery1 = new DevExpress.DataAccess.Sql.StoredProcQuery();
			DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
			this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
			this.Detail = new DevExpress.XtraReports.UI.DetailBand();
			this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
			this.pictureBox1 = new DevExpress.XtraReports.UI.XRPictureBox();
			this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
			this.EmpCode = new DevExpress.XtraReports.Parameters.Parameter();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// TopMargin
			// 
			this.TopMargin.HeightF = 10F;
			this.TopMargin.Name = "TopMargin";
			// 
			// Detail
			// 
			this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.pictureBox1});
			this.Detail.HeightF = 313.3333F;
			this.Detail.Name = "Detail";
			// 
			// BottomMargin
			// 
			this.BottomMargin.HeightF = 10F;
			this.BottomMargin.Name = "BottomMargin";
			// 
			// pictureBox1
			// 
			this.pictureBox1.ImageSource = new DevExpress.XtraPrinting.Drawing.ImageSource("img", resources.GetString("pictureBox1.ImageSource"));
			this.pictureBox1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.SizeF = new System.Drawing.SizeF(230F, 313.3333F);
			this.pictureBox1.Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage;
			// 
			// sqlDataSource1
			// 
			this.sqlDataSource1.ConnectionName = "ReportConnectionString";
			this.sqlDataSource1.Name = "sqlDataSource1";
			storedProcQuery1.Name = "HR_RPT_PrintCard";
			queryParameter1.Name = "@EmpCode";
			queryParameter1.Type = typeof(DevExpress.DataAccess.Expression);
			queryParameter1.Value = new DevExpress.DataAccess.Expression("?EmpCode", typeof(string));
			storedProcQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1});
			storedProcQuery1.StoredProcName = "HR_RPT_PrintCard";
			this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
			this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
			// 
			// EmpCode
			// 
			this.EmpCode.Name = "EmpCode";
			// 
			// RPTBackCard
			// 
			this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.Detail,
            this.BottomMargin});
			this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
			this.DataMember = "HR_RPT_PrintCard";
			this.DataSource = this.sqlDataSource1;
			this.DisplayName = "RPTBackCard";
			this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
			this.Margins = new DevExpress.Drawing.DXMargins(10F, 10F, 10F, 10F);
			this.PageHeight = 335;
			this.PageWidth = 250;
			this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.Custom;
			this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.EmpCode});
			this.Version = "23.2";
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

		#endregion

		private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
		private DevExpress.XtraReports.UI.DetailBand Detail;
		private DevExpress.XtraReports.UI.XRPictureBox pictureBox1;
		private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
		private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
		private DevExpress.XtraReports.Parameters.Parameter EmpCode;
	}
}
