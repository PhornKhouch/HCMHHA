
namespace HUMICA.Models.Report.Payroll
{
    partial class RptEmpAsset
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
            DevExpress.XtraReports.UI.XRSummary xrSummary1 = new DevExpress.XtraReports.UI.XRSummary();
            DevExpress.DataAccess.Sql.CustomSqlQuery customSqlQuery1 = new DevExpress.DataAccess.Sql.CustomSqlQuery();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RptEmpAsset));
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.table2 = new DevExpress.XtraReports.UI.XRTable();
            this.tableRow2 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell6 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableCell7 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableCell8 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableCell9 = new DevExpress.XtraReports.UI.XRTableCell();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.label10 = new DevExpress.XtraReports.UI.XRLabel();
            this.label11 = new DevExpress.XtraReports.UI.XRLabel();
            this.label14 = new DevExpress.XtraReports.UI.XRLabel();
            this.label9 = new DevExpress.XtraReports.UI.XRLabel();
            this.label6 = new DevExpress.XtraReports.UI.XRLabel();
            this.pictureBox1 = new DevExpress.XtraReports.UI.XRPictureBox();
            this.label16 = new DevExpress.XtraReports.UI.XRLabel();
            this.label4 = new DevExpress.XtraReports.UI.XRLabel();
            this.label3 = new DevExpress.XtraReports.UI.XRLabel();
            this.label1 = new DevExpress.XtraReports.UI.XRLabel();
            this.label2 = new DevExpress.XtraReports.UI.XRLabel();
            this.PageHeader = new DevExpress.XtraReports.UI.PageHeaderBand();
            this.table1 = new DevExpress.XtraReports.UI.XRTable();
            this.tableRow1 = new DevExpress.XtraReports.UI.XRTableRow();
            this.tableCell1 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableCell2 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableCell3 = new DevExpress.XtraReports.UI.XRTableCell();
            this.tableCell4 = new DevExpress.XtraReports.UI.XRTableCell();
            this.ReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
            this.label17 = new DevExpress.XtraReports.UI.XRLabel();
            this.label15 = new DevExpress.XtraReports.UI.XRLabel();
            this.label13 = new DevExpress.XtraReports.UI.XRLabel();
            this.label12 = new DevExpress.XtraReports.UI.XRLabel();
            this.label8 = new DevExpress.XtraReports.UI.XRLabel();
            this.label7 = new DevExpress.XtraReports.UI.XRLabel();
            this.label5 = new DevExpress.XtraReports.UI.XRLabel();
            this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            this.EmpCode = new DevExpress.XtraReports.Parameters.Parameter();
            ((System.ComponentModel.ISupportInitialize)(this.table2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.table1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.HeightF = 25F;
            this.TopMargin.Name = "TopMargin";
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.table2});
            this.Detail.HeightF = 25F;
            this.Detail.Name = "Detail";
            // 
            // table2
            // 
            this.table2.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.table2.Name = "table2";
            this.table2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
            this.table2.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.tableRow2});
            this.table2.SizeF = new System.Drawing.SizeF(777.0001F, 25F);
            // 
            // tableRow2
            // 
            this.tableRow2.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell6,
            this.tableCell7,
            this.tableCell8,
            this.tableCell9});
            this.tableRow2.Name = "tableRow2";
            this.tableRow2.Weight = 1D;
            // 
            // tableCell6
            // 
            this.tableCell6.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumRecordNumber()")});
            this.tableCell6.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.tableCell6.Multiline = true;
            this.tableCell6.Name = "tableCell6";
            this.tableCell6.StylePriority.UseBorders = false;
            this.tableCell6.StylePriority.UseFont = false;
            this.tableCell6.StylePriority.UseTextAlignment = false;
            xrSummary1.Running = DevExpress.XtraReports.UI.SummaryRunning.Report;
            this.tableCell6.Summary = xrSummary1;
            this.tableCell6.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.tableCell6.Weight = 0.47916667938232416D;
            // 
            // tableCell7
            // 
            this.tableCell7.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell7.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Description]")});
            this.tableCell7.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.tableCell7.Multiline = true;
            this.tableCell7.Name = "tableCell7";
            this.tableCell7.StylePriority.UseBorders = false;
            this.tableCell7.StylePriority.UseFont = false;
            this.tableCell7.StylePriority.UseTextAlignment = false;
            this.tableCell7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.tableCell7.Weight = 4.072916393285344D;
            // 
            // tableCell8
            // 
            this.tableCell8.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell8.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.tableCell8.Multiline = true;
            this.tableCell8.Name = "tableCell8";
            this.tableCell8.StylePriority.UseBorders = false;
            this.tableCell8.StylePriority.UseFont = false;
            this.tableCell8.StylePriority.UseTextAlignment = false;
            this.tableCell8.Text = "1";
            this.tableCell8.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.tableCell8.Weight = 1.4062503332430927D;
            // 
            // tableCell9
            // 
            this.tableCell9.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell9.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AssetCode]")});
            this.tableCell9.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.tableCell9.Multiline = true;
            this.tableCell9.Name = "tableCell9";
            this.tableCell9.StylePriority.UseBorders = false;
            this.tableCell9.StylePriority.UseFont = false;
            this.tableCell9.StylePriority.UseTextAlignment = false;
            this.tableCell9.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.tableCell9.Weight = 1.811666681041797D;
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 25F;
            this.BottomMargin.Name = "BottomMargin";
            // 
            // ReportHeader
            // 
            this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.label10,
            this.label11,
            this.label14,
            this.label9,
            this.label6,
            this.pictureBox1,
            this.label16,
            this.label4,
            this.label3,
            this.label1,
            this.label2});
            this.ReportHeader.HeightF = 228.2916F;
            this.ReportHeader.Name = "ReportHeader";
            // 
            // label10
            // 
            this.label10.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right)));
            this.label10.CanGrow = false;
            this.label10.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[EmpCode]")});
            this.label10.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.label10.LocationFloat = new DevExpress.Utils.PointFloat(100.0002F, 125.2916F);
            this.label10.Multiline = true;
            this.label10.Name = "label10";
            this.label10.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label10.SizeF = new System.Drawing.SizeF(226.0416F, 29.99999F);
            this.label10.StylePriority.UseBorders = false;
            this.label10.StylePriority.UseFont = false;
            this.label10.StylePriority.UseTextAlignment = false;
            this.label10.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right)));
            this.label11.CanGrow = false;
            this.label11.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AllName]")});
            this.label11.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.label11.LocationFloat = new DevExpress.Utils.PointFloat(100.0002F, 155.2916F);
            this.label11.Multiline = true;
            this.label11.Name = "label11";
            this.label11.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label11.SizeF = new System.Drawing.SizeF(226.0416F, 30F);
            this.label11.StylePriority.UseBorders = false;
            this.label11.StylePriority.UseFont = false;
            this.label11.StylePriority.UseTextAlignment = false;
            this.label11.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label14.CanGrow = false;
            this.label14.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Branch]")});
            this.label14.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.label14.LocationFloat = new DevExpress.Utils.PointFloat(100.0002F, 185.2916F);
            this.label14.Multiline = true;
            this.label14.Name = "label14";
            this.label14.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label14.SizeF = new System.Drawing.SizeF(226.0416F, 29.99998F);
            this.label14.StylePriority.UseBorders = false;
            this.label14.StylePriority.UseFont = false;
            this.label14.StylePriority.UseTextAlignment = false;
            this.label14.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label9.CanGrow = false;
            this.label9.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.label9.LocationFloat = new DevExpress.Utils.PointFloat(0F, 185.2916F);
            this.label9.Multiline = true;
            this.label9.Name = "label9";
            this.label9.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label9.SizeF = new System.Drawing.SizeF(100.0002F, 29.99999F);
            this.label9.StylePriority.UseBorders = false;
            this.label9.StylePriority.UseFont = false;
            this.label9.StylePriority.UseTextAlignment = false;
            this.label9.Text = "Branch";
            this.label9.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top)));
            this.label6.CanGrow = false;
            this.label6.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.label6.LocationFloat = new DevExpress.Utils.PointFloat(0F, 155.2916F);
            this.label6.Multiline = true;
            this.label6.Name = "label6";
            this.label6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label6.SizeF = new System.Drawing.SizeF(100.0002F, 29.99999F);
            this.label6.StylePriority.UseBorders = false;
            this.label6.StylePriority.UseFont = false;
            this.label6.StylePriority.UseTextAlignment = false;
            this.label6.Text = "Name";
            this.label6.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "ImageUrl", "[LOGO]")});
            this.pictureBox1.LocationFloat = new DevExpress.Utils.PointFloat(0.0001525879F, 0F);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.SizeF = new System.Drawing.SizeF(100F, 100F);
            this.pictureBox1.Sizing = DevExpress.XtraPrinting.ImageSizeMode.ZoomImage;
            // 
            // label16
            // 
            this.label16.CanGrow = false;
            this.label16.LocationFloat = new DevExpress.Utils.PointFloat(0F, 215.2916F);
            this.label16.Multiline = true;
            this.label16.Name = "label16";
            this.label16.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label16.SizeF = new System.Drawing.SizeF(777.0001F, 12.99998F);
            // 
            // label4
            // 
            this.label4.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top)));
            this.label4.CanGrow = false;
            this.label4.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.label4.LocationFloat = new DevExpress.Utils.PointFloat(0F, 125.2916F);
            this.label4.Multiline = true;
            this.label4.Name = "label4";
            this.label4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label4.SizeF = new System.Drawing.SizeF(100.0002F, 29.99999F);
            this.label4.StylePriority.UseBorders = false;
            this.label4.StylePriority.UseFont = false;
            this.label4.StylePriority.UseTextAlignment = false;
            this.label4.Text = "ID";
            this.label4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label3.CanGrow = false;
            this.label3.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 11F, DevExpress.Drawing.DXFontStyle.Bold);
            this.label3.LocationFloat = new DevExpress.Utils.PointFloat(143.75F, 62.99998F);
            this.label3.Multiline = true;
            this.label3.Name = "label3";
            this.label3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label3.SizeF = new System.Drawing.SizeF(483.1666F, 30F);
            this.label3.StylePriority.UseBorders = false;
            this.label3.StylePriority.UseFont = false;
            this.label3.StylePriority.UseTextAlignment = false;
            this.label3.Text = "លិខិតផ្ទេរ-ប្រគល់ ទ្រព្យសម្បត្តិ";
            this.label3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label1.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 12F);
            this.label1.LocationFloat = new DevExpress.Utils.PointFloat(143.75F, 0F);
            this.label1.Multiline = true;
            this.label1.Name = "label1";
            this.label1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label1.SizeF = new System.Drawing.SizeF(483.1666F, 32.99999F);
            this.label1.StylePriority.UseBorders = false;
            this.label1.StylePriority.UseFont = false;
            this.label1.StylePriority.UseTextAlignment = false;
            this.label1.Text = "ព្រះរាជាណាចក្រកម្ពុជា";
            this.label1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label2.CanGrow = false;
            this.label2.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 12F);
            this.label2.LocationFloat = new DevExpress.Utils.PointFloat(143.75F, 32.99999F);
            this.label2.Multiline = true;
            this.label2.Name = "label2";
            this.label2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label2.SizeF = new System.Drawing.SizeF(483.1666F, 30F);
            this.label2.StylePriority.UseBorders = false;
            this.label2.StylePriority.UseFont = false;
            this.label2.StylePriority.UseTextAlignment = false;
            this.label2.Text = "ជាតិ សាសនា ព្រះមហាក្សត្រ";
            this.label2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // PageHeader
            // 
            this.PageHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.table1});
            this.PageHeader.HeightF = 25F;
            this.PageHeader.Name = "PageHeader";
            // 
            // table1
            // 
            this.table1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.table1.Name = "table1";
            this.table1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
            this.table1.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.tableRow1});
            this.table1.SizeF = new System.Drawing.SizeF(777.0001F, 25F);
            // 
            // tableRow1
            // 
            this.tableRow1.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.tableCell1,
            this.tableCell2,
            this.tableCell3,
            this.tableCell4});
            this.tableRow1.Name = "tableRow1";
            this.tableRow1.Weight = 1D;
            // 
            // tableCell1
            // 
            this.tableCell1.BackColor = System.Drawing.Color.Transparent;
            this.tableCell1.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell1.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.tableCell1.Multiline = true;
            this.tableCell1.Name = "tableCell1";
            this.tableCell1.StylePriority.UseBackColor = false;
            this.tableCell1.StylePriority.UseBorders = false;
            this.tableCell1.StylePriority.UseFont = false;
            this.tableCell1.StylePriority.UseTextAlignment = false;
            this.tableCell1.Text = "ល.រ";
            this.tableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.tableCell1.Weight = 0.47916667938232416D;
            // 
            // tableCell2
            // 
            this.tableCell2.BackColor = System.Drawing.Color.Transparent;
            this.tableCell2.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell2.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.tableCell2.Multiline = true;
            this.tableCell2.Name = "tableCell2";
            this.tableCell2.StylePriority.UseBackColor = false;
            this.tableCell2.StylePriority.UseBorders = false;
            this.tableCell2.StylePriority.UseFont = false;
            this.tableCell2.StylePriority.UseTextAlignment = false;
            this.tableCell2.Text = "ឈ្មោះទ្រព្យសម្បត្តិ";
            this.tableCell2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.tableCell2.Weight = 4.0729163981291725D;
            // 
            // tableCell3
            // 
            this.tableCell3.BackColor = System.Drawing.Color.Transparent;
            this.tableCell3.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell3.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.tableCell3.Multiline = true;
            this.tableCell3.Name = "tableCell3";
            this.tableCell3.StylePriority.UseBackColor = false;
            this.tableCell3.StylePriority.UseBorders = false;
            this.tableCell3.StylePriority.UseFont = false;
            this.tableCell3.StylePriority.UseTextAlignment = false;
            this.tableCell3.Text = "ចំនួន";
            this.tableCell3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.tableCell3.Weight = 1.4062499976060172D;
            // 
            // tableCell4
            // 
            this.tableCell4.BackColor = System.Drawing.Color.Transparent;
            this.tableCell4.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.tableCell4.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.tableCell4.Multiline = true;
            this.tableCell4.Name = "tableCell4";
            this.tableCell4.StylePriority.UseBackColor = false;
            this.tableCell4.StylePriority.UseBorders = false;
            this.tableCell4.StylePriority.UseFont = false;
            this.tableCell4.StylePriority.UseTextAlignment = false;
            this.tableCell4.Text = "កូដទ្រព្យ";
            this.tableCell4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.tableCell4.Weight = 1.8116666432556432D;
            // 
            // ReportFooter
            // 
            this.ReportFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.label17,
            this.label15,
            this.label13,
            this.label12,
            this.label8,
            this.label7,
            this.label5});
            this.ReportFooter.HeightF = 214.7918F;
            this.ReportFooter.Name = "ReportFooter";
            // 
            // label17
            // 
            this.label17.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label17.CanGrow = false;
            this.label17.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 10F);
            this.label17.LocationFloat = new DevExpress.Utils.PointFloat(455.2084F, 134.9167F);
            this.label17.Multiline = true;
            this.label17.Name = "label17";
            this.label17.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label17.SizeF = new System.Drawing.SizeF(321.7917F, 79.87518F);
            this.label17.StylePriority.UseBorders = false;
            this.label17.StylePriority.UseFont = false;
            this.label17.StylePriority.UseTextAlignment = false;
            this.label17.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label15.CanGrow = false;
            this.label15.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 10F);
            this.label15.LocationFloat = new DevExpress.Utils.PointFloat(0F, 134.9167F);
            this.label15.Multiline = true;
            this.label15.Name = "label15";
            this.label15.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label15.SizeF = new System.Drawing.SizeF(326.0417F, 79.87518F);
            this.label15.StylePriority.UseBorders = false;
            this.label15.StylePriority.UseFont = false;
            this.label15.StylePriority.UseTextAlignment = false;
            this.label15.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label13.CanGrow = false;
            this.label13.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.label13.LocationFloat = new DevExpress.Utils.PointFloat(455.2083F, 104.9167F);
            this.label13.Multiline = true;
            this.label13.Name = "label13";
            this.label13.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label13.SizeF = new System.Drawing.SizeF(321.7917F, 30F);
            this.label13.StylePriority.UseBorders = false;
            this.label13.StylePriority.UseFont = false;
            this.label13.StylePriority.UseTextAlignment = false;
            this.label13.Text = "អ្នកប្រគល់";
            this.label13.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label12.CanGrow = false;
            this.label12.Font = new DevExpress.Drawing.DXFont("Khmer OS Muol Light", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.label12.LocationFloat = new DevExpress.Utils.PointFloat(0F, 104.9167F);
            this.label12.Multiline = true;
            this.label12.Name = "label12";
            this.label12.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label12.SizeF = new System.Drawing.SizeF(326.0417F, 30F);
            this.label12.StylePriority.UseBorders = false;
            this.label12.StylePriority.UseFont = false;
            this.label12.StylePriority.UseTextAlignment = false;
            this.label12.Text = "អ្នកទទួល";
            this.label12.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label8.CanGrow = false;
            this.label8.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F);
            this.label8.LocationFloat = new DevExpress.Utils.PointFloat(455.2084F, 74.91668F);
            this.label8.Multiline = true;
            this.label8.Name = "label8";
            this.label8.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label8.SizeF = new System.Drawing.SizeF(321.7916F, 30F);
            this.label8.StylePriority.UseBorders = false;
            this.label8.StylePriority.UseFont = false;
            this.label8.StylePriority.UseTextAlignment = false;
            this.label8.Text = "ថ្ងៃទី........ខែ.............ឆ្នាំ២០២.....";
            this.label8.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label7.CanGrow = false;
            this.label7.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F);
            this.label7.LocationFloat = new DevExpress.Utils.PointFloat(0F, 74.91668F);
            this.label7.Multiline = true;
            this.label7.Name = "label7";
            this.label7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label7.SizeF = new System.Drawing.SizeF(326.0417F, 30F);
            this.label7.StylePriority.UseBorders = false;
            this.label7.StylePriority.UseFont = false;
            this.label7.StylePriority.UseTextAlignment = false;
            this.label7.Text = "ថ្ងៃទី........ខែ.............ឆ្នាំ២០២.....";
            this.label7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.label5.CanGrow = false;
            this.label5.Font = new DevExpress.Drawing.DXFont("Khmer OS Battambang", 10F);
            this.label5.LocationFloat = new DevExpress.Utils.PointFloat(0.0001509984F, 0F);
            this.label5.Multiline = true;
            this.label5.Name = "label5";
            this.label5.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.label5.SizeF = new System.Drawing.SizeF(776.9999F, 30F);
            this.label5.StylePriority.UseBorders = false;
            this.label5.StylePriority.UseFont = false;
            this.label5.StylePriority.UseTextAlignment = false;
            this.label5.Text = "ខ្ញុំបាទ/នាងខ្ញុំទទួលខុសត្រូវលើឧបករណ៍សម្ភារៈជាទ្រព្យសម្បត្តិក្រុមហ៊ុន ប្រសិនបើមាន" +
    "ការបាត់បង់ ឬខូចខាត  ។";
            this.label5.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // sqlDataSource1
            // 
            this.sqlDataSource1.ConnectionName = "ReportConnectionString";
            this.sqlDataSource1.Name = "sqlDataSource1";
            customSqlQuery1.Name = "Query";
            queryParameter1.Name = "EmpCode";
            queryParameter1.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter1.Value = new DevExpress.DataAccess.Expression("?EmpCode", typeof(string));
            customSqlQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1});
            customSqlQuery1.Sql = resources.GetString("customSqlQuery1.Sql");
            this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            customSqlQuery1});
            this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
            // 
            // EmpCode
            // 
            this.EmpCode.Name = "EmpCode";
            // 
            // RptEmpAsset
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.Detail,
            this.BottomMargin,
            this.ReportHeader,
            this.PageHeader,
            this.ReportFooter});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
            this.DataMember = "Query";
            this.DataSource = this.sqlDataSource1;
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Margins = new DevExpress.Drawing.DXMargins(25F, 25F, 25F, 25F);
            this.PageHeight = 1169;
            this.PageWidth = 827;
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.EmpCode});
            this.Version = "23.2";
            ((System.ComponentModel.ISupportInitialize)(this.table2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.table1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.UI.XRTable table2;
        private DevExpress.XtraReports.UI.XRTableRow tableRow2;
        private DevExpress.XtraReports.UI.XRTableCell tableCell6;
        private DevExpress.XtraReports.UI.XRTableCell tableCell7;
        private DevExpress.XtraReports.UI.XRTableCell tableCell8;
        private DevExpress.XtraReports.UI.XRTableCell tableCell9;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRLabel label10;
        private DevExpress.XtraReports.UI.XRLabel label11;
        private DevExpress.XtraReports.UI.XRLabel label14;
        private DevExpress.XtraReports.UI.XRLabel label9;
        private DevExpress.XtraReports.UI.XRLabel label6;
        private DevExpress.XtraReports.UI.XRPictureBox pictureBox1;
        private DevExpress.XtraReports.UI.XRLabel label16;
        private DevExpress.XtraReports.UI.XRLabel label4;
        private DevExpress.XtraReports.UI.XRLabel label3;
        private DevExpress.XtraReports.UI.XRLabel label1;
        private DevExpress.XtraReports.UI.XRLabel label2;
        private DevExpress.XtraReports.UI.PageHeaderBand PageHeader;
        private DevExpress.XtraReports.UI.XRTable table1;
        private DevExpress.XtraReports.UI.XRTableRow tableRow1;
        private DevExpress.XtraReports.UI.XRTableCell tableCell1;
        private DevExpress.XtraReports.UI.XRTableCell tableCell2;
        private DevExpress.XtraReports.UI.XRTableCell tableCell3;
        private DevExpress.XtraReports.UI.XRTableCell tableCell4;
        private DevExpress.XtraReports.UI.ReportFooterBand ReportFooter;
        private DevExpress.XtraReports.UI.XRLabel label17;
        private DevExpress.XtraReports.UI.XRLabel label15;
        private DevExpress.XtraReports.UI.XRLabel label13;
        private DevExpress.XtraReports.UI.XRLabel label12;
        private DevExpress.XtraReports.UI.XRLabel label8;
        private DevExpress.XtraReports.UI.XRLabel label7;
        private DevExpress.XtraReports.UI.XRLabel label5;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.XtraReports.Parameters.Parameter EmpCode;
    }
}
