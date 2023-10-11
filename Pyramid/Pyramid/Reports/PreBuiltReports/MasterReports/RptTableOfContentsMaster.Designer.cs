namespace Pyramid.Reports.PreBuiltReports.MasterReports
{
    partial class RptTableOfContentsMaster
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
            this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.TOCMaster = new DevExpress.XtraReports.UI.XRTableOfContents();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // PageHeader
            // 
            this.PageHeader.HeightF = 120.7083F;
            // 
            // lblCriteriaValues
            // 
            this.lblCriteriaValues.StylePriority.UseTextAlignment = false;
            // 
            // lblCriteria
            // 
            this.lblCriteria.Bookmark = "Criteria Used for this Report";
            this.lblCriteria.StylePriority.UseFont = false;
            this.lblCriteria.StylePriority.UseTextAlignment = false;
            // 
            // imgLogo
            // 
            this.imgLogo.StylePriority.UseBackColor = false;
            this.imgLogo.StylePriority.UsePadding = false;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.StylePriority.UseBackColor = false;
            this.lblAppTitle.StylePriority.UseFont = false;
            this.lblAppTitle.StylePriority.UseForeColor = false;
            this.lblAppTitle.StylePriority.UsePadding = false;
            this.lblAppTitle.StylePriority.UseTextAlignment = false;
            // 
            // lblReportTitle
            // 
            this.lblReportTitle.StylePriority.UseBackColor = false;
            this.lblReportTitle.StylePriority.UseBorderColor = false;
            this.lblReportTitle.StylePriority.UseFont = false;
            this.lblReportTitle.StylePriority.UseForeColor = false;
            this.lblReportTitle.StylePriority.UseTextAlignment = false;
            // 
            // lblStateName
            // 
            this.lblStateName.StylePriority.UseBackColor = false;
            this.lblStateName.StylePriority.UseFont = false;
            this.lblStateName.StylePriority.UseForeColor = false;
            this.lblStateName.StylePriority.UsePadding = false;
            this.lblStateName.StylePriority.UseTextAlignment = false;
            // 
            // lblStateCatchphrase
            // 
            this.lblStateCatchphrase.StylePriority.UseBackColor = false;
            this.lblStateCatchphrase.StylePriority.UseFont = false;
            this.lblStateCatchphrase.StylePriority.UseForeColor = false;
            this.lblStateCatchphrase.StylePriority.UsePadding = false;
            this.lblStateCatchphrase.StylePriority.UseTextAlignment = false;
            // 
            // masterPageInfoDate
            // 
            this.masterPageInfoDate.StylePriority.UseTextAlignment = false;
            // 
            // masterPageInfoNum
            // 
            this.masterPageInfoNum.StylePriority.UseTextAlignment = false;
            // 
            // lblGenerated
            // 
            this.lblGenerated.StylePriority.UseFont = false;
            this.lblGenerated.StylePriority.UseTextAlignment = false;
            // 
            // ReportHeader
            // 
            this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.TOCMaster});
            this.ReportHeader.HeightF = 46F;
            this.ReportHeader.Name = "ReportHeader";
            this.ReportHeader.PageBreak = DevExpress.XtraReports.UI.PageBreak.AfterBand;
            // 
            // TOCMaster
            // 
            this.TOCMaster.LevelDefault.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.TOCMaster.LevelTitle.Font = new DevExpress.Drawing.DXFont("Arial", 11.25F, DevExpress.Drawing.DXFontStyle.Bold, DevExpress.Drawing.DXGraphicsUnit.Point, new DevExpress.Drawing.DXFontAdditionalProperty[] {new DevExpress.Drawing.DXFontAdditionalProperty("GdiCharSet", ((byte)(0)))});
            this.TOCMaster.LevelTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.TOCMaster.LevelTitle.Text = "Table of Contents";
            this.TOCMaster.LevelTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.TOCMaster.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.TOCMaster.Name = "TOCMaster";
            // 
            // RptTableOfContentsMaster
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.Detail,
            this.PageFooter,
            this.PageHeader,
            this.MasterReportFooter,
            this.ReportHeader,
            this.TopMargin,
            this.BottomMargin});
            this.ExportOptions.Xls.SheetName = "Report";
            this.ExportOptions.Xls.ShowGridLines = true;
            this.ExportOptions.Xlsx.DocumentOptions.Category = "Pyramid Implementation Data System";
            this.ExportOptions.Xlsx.SheetName = "Report";
            this.ExportOptions.Xlsx.ShowGridLines = true;
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.ParamLogoPath,
            this.ParamViewPrivateChildInfo});
            this.Version = "20.1";
            this.Controls.SetChildIndex(this.BottomMargin, 0);
            this.Controls.SetChildIndex(this.TopMargin, 0);
            this.Controls.SetChildIndex(this.ReportHeader, 0);
            this.Controls.SetChildIndex(this.MasterReportFooter, 0);
            this.Controls.SetChildIndex(this.PageHeader, 0);
            this.Controls.SetChildIndex(this.PageFooter, 0);
            this.Controls.SetChildIndex(this.Detail, 0);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }


        #endregion

        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRTableOfContents TOCMaster;
    }
}
