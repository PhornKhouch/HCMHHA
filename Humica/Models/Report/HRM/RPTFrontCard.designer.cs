namespace Humica.Models.Report.HRM
{
    partial class RPTFrontCard
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RPTFrontCard));
			DevExpress.DataAccess.Sql.StoredProcQuery storedProcQuery1 = new DevExpress.DataAccess.Sql.StoredProcQuery();
			DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
			this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
			this.Detail = new DevExpress.XtraReports.UI.DetailBand();
			this.pictureBox2 = new DevExpress.XtraReports.UI.XRPictureBox();
			this.xrLabel18 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel17 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel15 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel16 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel14 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel9 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel3 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel12 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel10 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel11 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel7 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
			this.pictureBox1 = new DevExpress.XtraReports.UI.XRPictureBox();
			this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
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
            this.pictureBox2,
            this.xrLabel18,
            this.xrLabel17,
            this.xrLabel15,
            this.xrLabel16,
            this.xrLabel14,
            this.xrLabel9,
            this.xrLabel3,
            this.xrLabel12,
            this.xrLabel10,
            this.xrLabel2,
            this.xrLabel11,
            this.xrLabel7,
            this.xrLabel1,
            this.xrLabel4,
            this.pictureBox1});
			this.Detail.HeightF = 313.3333F;
			this.Detail.Name = "Detail";
			// 
			// pictureBox2
			// 
			this.pictureBox2.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "ImageUrl", "[Photo]")});
			this.pictureBox2.LocationFloat = new DevExpress.Utils.PointFloat(65.3875F, 54.13417F);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.SizeF = new System.Drawing.SizeF(97.50693F, 106.9792F);
			this.pictureBox2.Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage;
			// 
			// xrLabel18
			// 
			this.xrLabel18.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel18.CanGrow = false;
			this.xrLabel18.Font = new DevExpress.Drawing.DXFont("Arial", 6F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel18.LocationFloat = new DevExpress.Utils.PointFloat(15.58559F, 291.3333F);
			this.xrLabel18.Multiline = true;
			this.xrLabel18.Name = "xrLabel18";
			this.xrLabel18.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel18.SizeF = new System.Drawing.SizeF(204.1944F, 21.99997F);
			this.xrLabel18.StylePriority.UseBorders = false;
			this.xrLabel18.StylePriority.UseFont = false;
			this.xrLabel18.StylePriority.UseTextAlignment = false;
			this.xrLabel18.Text = "This card is automatically printed";
			this.xrLabel18.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel17
			// 
			this.xrLabel17.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel17.CanGrow = false;
			this.xrLabel17.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel17.ForeColor = System.Drawing.Color.Black;
			this.xrLabel17.LocationFloat = new DevExpress.Utils.PointFloat(98.33484F, 260.3333F);
			this.xrLabel17.Multiline = true;
			this.xrLabel17.Name = "xrLabel17";
			this.xrLabel17.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel17.SizeF = new System.Drawing.SizeF(6.972202F, 22F);
			this.xrLabel17.StylePriority.UseBorders = false;
			this.xrLabel17.StylePriority.UseFont = false;
			this.xrLabel17.StylePriority.UseForeColor = false;
			this.xrLabel17.StylePriority.UseTextAlignment = false;
			this.xrLabel17.Text = ":";
			this.xrLabel17.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel15
			// 
			this.xrLabel15.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel15.CanGrow = false;
			this.xrLabel15.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F);
			this.xrLabel15.ForeColor = System.Drawing.Color.Black;
			this.xrLabel15.LocationFloat = new DevExpress.Utils.PointFloat(43.22223F, 260.3333F);
			this.xrLabel15.Multiline = true;
			this.xrLabel15.Name = "xrLabel15";
			this.xrLabel15.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel15.SizeF = new System.Drawing.SizeF(55.11261F, 22F);
			this.xrLabel15.StylePriority.UseBorders = false;
			this.xrLabel15.StylePriority.UseFont = false;
			this.xrLabel15.StylePriority.UseForeColor = false;
			this.xrLabel15.StylePriority.UseTextAlignment = false;
			this.xrLabel15.Text = "Date Join";
			this.xrLabel15.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel16
			// 
			this.xrLabel16.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel16.CanGrow = false;
			this.xrLabel16.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[StartDate]")});
			this.xrLabel16.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel16.ForeColor = System.Drawing.Color.Black;
			this.xrLabel16.LocationFloat = new DevExpress.Utils.PointFloat(105.307F, 260.3333F);
			this.xrLabel16.Name = "xrLabel16";
			this.xrLabel16.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel16.SizeF = new System.Drawing.SizeF(117.473F, 22.00003F);
			this.xrLabel16.StylePriority.UseBorders = false;
			this.xrLabel16.StylePriority.UseFont = false;
			this.xrLabel16.StylePriority.UseForeColor = false;
			this.xrLabel16.StylePriority.UseTextAlignment = false;
			this.xrLabel16.Text = "xrLabel3";
			this.xrLabel16.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			this.xrLabel16.TextFormatString = "{0:MMM dd, yyyy}";
			// 
			// xrLabel14
			// 
			this.xrLabel14.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel14.CanGrow = false;
			this.xrLabel14.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel14.ForeColor = System.Drawing.Color.Black;
			this.xrLabel14.LocationFloat = new DevExpress.Utils.PointFloat(91.36263F, 238.3333F);
			this.xrLabel14.Multiline = true;
			this.xrLabel14.Name = "xrLabel14";
			this.xrLabel14.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel14.SizeF = new System.Drawing.SizeF(7.000008F, 21.99998F);
			this.xrLabel14.StylePriority.UseBorders = false;
			this.xrLabel14.StylePriority.UseFont = false;
			this.xrLabel14.StylePriority.UseForeColor = false;
			this.xrLabel14.StylePriority.UseTextAlignment = false;
			this.xrLabel14.Text = ":";
			this.xrLabel14.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel9
			// 
			this.xrLabel9.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel9.CanGrow = false;
			this.xrLabel9.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F);
			this.xrLabel9.ForeColor = System.Drawing.Color.Black;
			this.xrLabel9.LocationFloat = new DevExpress.Utils.PointFloat(43.25002F, 238.3333F);
			this.xrLabel9.Multiline = true;
			this.xrLabel9.Name = "xrLabel9";
			this.xrLabel9.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel9.SizeF = new System.Drawing.SizeF(48.11261F, 22F);
			this.xrLabel9.StylePriority.UseBorders = false;
			this.xrLabel9.StylePriority.UseFont = false;
			this.xrLabel9.StylePriority.UseForeColor = false;
			this.xrLabel9.StylePriority.UseTextAlignment = false;
			this.xrLabel9.Text = "Position";
			this.xrLabel9.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel3
			// 
			this.xrLabel3.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel3.CanGrow = false;
			this.xrLabel3.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Position]")});
			this.xrLabel3.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel3.ForeColor = System.Drawing.Color.Black;
			this.xrLabel3.LocationFloat = new DevExpress.Utils.PointFloat(98.36264F, 238.3333F);
			this.xrLabel3.Name = "xrLabel3";
			this.xrLabel3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel3.SizeF = new System.Drawing.SizeF(124.6374F, 21.99998F);
			this.xrLabel3.StylePriority.UseBorders = false;
			this.xrLabel3.StylePriority.UseFont = false;
			this.xrLabel3.StylePriority.UseForeColor = false;
			this.xrLabel3.StylePriority.UseTextAlignment = false;
			this.xrLabel3.Text = "xrLabel3";
			this.xrLabel3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel12
			// 
			this.xrLabel12.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel12.CanGrow = false;
			this.xrLabel12.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel12.ForeColor = System.Drawing.Color.Black;
			this.xrLabel12.LocationFloat = new DevExpress.Utils.PointFloat(91.36263F, 216.3333F);
			this.xrLabel12.Multiline = true;
			this.xrLabel12.Name = "xrLabel12";
			this.xrLabel12.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel12.SizeF = new System.Drawing.SizeF(7.000008F, 22F);
			this.xrLabel12.StylePriority.UseBorders = false;
			this.xrLabel12.StylePriority.UseFont = false;
			this.xrLabel12.StylePriority.UseForeColor = false;
			this.xrLabel12.StylePriority.UseTextAlignment = false;
			this.xrLabel12.Text = ":";
			this.xrLabel12.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel10
			// 
			this.xrLabel10.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel10.CanGrow = false;
			this.xrLabel10.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F);
			this.xrLabel10.ForeColor = System.Drawing.Color.Black;
			this.xrLabel10.LocationFloat = new DevExpress.Utils.PointFloat(43.22223F, 216.3333F);
			this.xrLabel10.Multiline = true;
			this.xrLabel10.Name = "xrLabel10";
			this.xrLabel10.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel10.SizeF = new System.Drawing.SizeF(48.1404F, 21.99998F);
			this.xrLabel10.StylePriority.UseBorders = false;
			this.xrLabel10.StylePriority.UseFont = false;
			this.xrLabel10.StylePriority.UseForeColor = false;
			this.xrLabel10.StylePriority.UseTextAlignment = false;
			this.xrLabel10.Text = "Name";
			this.xrLabel10.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel2
			// 
			this.xrLabel2.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel2.CanGrow = false;
			this.xrLabel2.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AllName]")});
			this.xrLabel2.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel2.ForeColor = System.Drawing.Color.Black;
			this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(98.36264F, 216.3333F);
			this.xrLabel2.Name = "xrLabel2";
			this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel2.SizeF = new System.Drawing.SizeF(124.6374F, 22.00002F);
			this.xrLabel2.StylePriority.UseBorders = false;
			this.xrLabel2.StylePriority.UseFont = false;
			this.xrLabel2.StylePriority.UseForeColor = false;
			this.xrLabel2.StylePriority.UseTextAlignment = false;
			this.xrLabel2.Text = "xrLabel2";
			this.xrLabel2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel11
			// 
			this.xrLabel11.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel11.CanGrow = false;
			this.xrLabel11.Font = new DevExpress.Drawing.DXFont("Times New Roman", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel11.ForeColor = System.Drawing.Color.Black;
			this.xrLabel11.LocationFloat = new DevExpress.Utils.PointFloat(91.39044F, 193.3333F);
			this.xrLabel11.Multiline = true;
			this.xrLabel11.Name = "xrLabel11";
			this.xrLabel11.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel11.SizeF = new System.Drawing.SizeF(6.972202F, 23F);
			this.xrLabel11.StylePriority.UseBorders = false;
			this.xrLabel11.StylePriority.UseFont = false;
			this.xrLabel11.StylePriority.UseForeColor = false;
			this.xrLabel11.StylePriority.UseTextAlignment = false;
			this.xrLabel11.Text = ":";
			this.xrLabel11.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel7
			// 
			this.xrLabel7.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel7.CanGrow = false;
			this.xrLabel7.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 9F);
			this.xrLabel7.ForeColor = System.Drawing.Color.Black;
			this.xrLabel7.LocationFloat = new DevExpress.Utils.PointFloat(43.25002F, 193.3333F);
			this.xrLabel7.Multiline = true;
			this.xrLabel7.Name = "xrLabel7";
			this.xrLabel7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel7.SizeF = new System.Drawing.SizeF(48.11262F, 23F);
			this.xrLabel7.StylePriority.UseBorders = false;
			this.xrLabel7.StylePriority.UseFont = false;
			this.xrLabel7.StylePriority.UseForeColor = false;
			this.xrLabel7.StylePriority.UseTextAlignment = false;
			this.xrLabel7.Text = "ឈ្មោះ";
			this.xrLabel7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel1
			// 
			this.xrLabel1.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel1.CanGrow = false;
			this.xrLabel1.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[OthAllName]")});
			this.xrLabel1.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 9F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel1.ForeColor = System.Drawing.Color.Black;
			this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(98.36264F, 193.3333F);
			this.xrLabel1.Name = "xrLabel1";
			this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel1.SizeF = new System.Drawing.SizeF(124.6374F, 23F);
			this.xrLabel1.StylePriority.UseBorders = false;
			this.xrLabel1.StylePriority.UseFont = false;
			this.xrLabel1.StylePriority.UseForeColor = false;
			this.xrLabel1.StylePriority.UseTextAlignment = false;
			this.xrLabel1.Text = "xrLabel1";
			this.xrLabel1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
			// 
			// xrLabel4
			// 
			this.xrLabel4.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel4.CanGrow = false;
			this.xrLabel4.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "\'ID # \' + [EmpCode]")});
			this.xrLabel4.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel4.ForeColor = System.Drawing.Color.Black;
			this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(31.36113F, 171.3333F);
			this.xrLabel4.Name = "xrLabel4";
			this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel4.SizeF = new System.Drawing.SizeF(166.9722F, 21.99998F);
			this.xrLabel4.StylePriority.UseBorders = false;
			this.xrLabel4.StylePriority.UseFont = false;
			this.xrLabel4.StylePriority.UseForeColor = false;
			this.xrLabel4.StylePriority.UseTextAlignment = false;
			this.xrLabel4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.pictureBox1.ImageSource = new DevExpress.XtraPrinting.Drawing.ImageSource("img", resources.GetString("pictureBox1.ImageSource"));
			this.pictureBox1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.SizeF = new System.Drawing.SizeF(230F, 313.3333F);
			this.pictureBox1.Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage;
			this.pictureBox1.StylePriority.UseBorders = false;
			// 
			// BottomMargin
			// 
			this.BottomMargin.HeightF = 10F;
			this.BottomMargin.Name = "BottomMargin";
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
			// RPTFrontCard
			// 
			this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.Detail,
            this.BottomMargin});
			this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
			this.DataMember = "HR_RPT_PrintCard";
			this.DataSource = this.sqlDataSource1;
			this.DisplayName = "RPTFrontCard";
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
		private DevExpress.XtraReports.UI.XRPictureBox pictureBox2;
		private DevExpress.XtraReports.UI.XRLabel xrLabel18;
		private DevExpress.XtraReports.UI.XRLabel xrLabel17;
		private DevExpress.XtraReports.UI.XRLabel xrLabel15;
		private DevExpress.XtraReports.UI.XRLabel xrLabel16;
		private DevExpress.XtraReports.UI.XRLabel xrLabel14;
		private DevExpress.XtraReports.UI.XRLabel xrLabel9;
		private DevExpress.XtraReports.UI.XRLabel xrLabel3;
		private DevExpress.XtraReports.UI.XRLabel xrLabel12;
		private DevExpress.XtraReports.UI.XRLabel xrLabel10;
		private DevExpress.XtraReports.UI.XRLabel xrLabel2;
		private DevExpress.XtraReports.UI.XRLabel xrLabel11;
		private DevExpress.XtraReports.UI.XRLabel xrLabel7;
		private DevExpress.XtraReports.UI.XRLabel xrLabel1;
		private DevExpress.XtraReports.UI.XRLabel xrLabel4;
		private DevExpress.XtraReports.UI.XRPictureBox pictureBox1;
		private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
		private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
		private DevExpress.XtraReports.Parameters.Parameter EmpCode;
	}
}
