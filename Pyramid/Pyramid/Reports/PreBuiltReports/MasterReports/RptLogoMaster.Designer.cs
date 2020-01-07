namespace Pyramid.Reports.PreBuiltReports.MasterReports
{
    partial class RptLogoMaster
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
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.PageFooter = new DevExpress.XtraReports.UI.PageFooterBand();
            this.footerLine = new DevExpress.XtraReports.UI.XRLine();
            this.lblGenerated = new DevExpress.XtraReports.UI.XRLabel();
            this.masterPageInfoNum = new DevExpress.XtraReports.UI.XRPageInfo();
            this.masterPageInfoDate = new DevExpress.XtraReports.UI.XRPageInfo();
            this.lblCriteriaValues = new DevExpress.XtraReports.UI.XRLabel();
            this.lblCriteria = new DevExpress.XtraReports.UI.XRLabel();
            this.imgLogo = new DevExpress.XtraReports.UI.XRPictureBox();
            this.lblAppTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.lblReportTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.PageHeader = new DevExpress.XtraReports.UI.PageHeaderBand();
            this.lblStateCatchphrase = new DevExpress.XtraReports.UI.XRLabel();
            this.lblStateName = new DevExpress.XtraReports.UI.XRLabel();
            this.ParamLogoPath = new DevExpress.XtraReports.Parameters.Parameter();
            this.MasterReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
            this.MasterFooterLine = new DevExpress.XtraReports.UI.XRLine();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.HeightF = 50F;
            this.TopMargin.Name = "TopMargin";
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 50F;
            this.BottomMargin.Name = "BottomMargin";
            // 
            // Detail
            // 
            this.Detail.Name = "Detail";
            // 
            // PageFooter
            // 
            this.PageFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.footerLine,
            this.lblGenerated,
            this.masterPageInfoNum,
            this.masterPageInfoDate});
            this.PageFooter.HeightF = 71.87503F;
            this.PageFooter.Name = "PageFooter";
            // 
            // footerLine
            // 
            this.footerLine.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.footerLine.Name = "footerLine";
            this.footerLine.SizeF = new System.Drawing.SizeF(750F, 23F);
            // 
            // lblGenerated
            // 
            this.lblGenerated.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblGenerated.LocationFloat = new DevExpress.Utils.PointFloat(524.8334F, 45.99994F);
            this.lblGenerated.Multiline = true;
            this.lblGenerated.Name = "lblGenerated";
            this.lblGenerated.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblGenerated.SizeF = new System.Drawing.SizeF(78.20828F, 23F);
            this.lblGenerated.StylePriority.UseFont = false;
            this.lblGenerated.StylePriority.UseTextAlignment = false;
            this.lblGenerated.Text = "Generated:";
            this.lblGenerated.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // masterPageInfoNum
            // 
            this.masterPageInfoNum.LocationFloat = new DevExpress.Utils.PointFloat(621.7917F, 22.99999F);
            this.masterPageInfoNum.Name = "masterPageInfoNum";
            this.masterPageInfoNum.SizeF = new System.Drawing.SizeF(118.2083F, 23F);
            this.masterPageInfoNum.StylePriority.UseTextAlignment = false;
            this.masterPageInfoNum.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.masterPageInfoNum.TextFormatString = "Page {0} of {1}";
            // 
            // masterPageInfoDate
            // 
            this.masterPageInfoDate.LocationFloat = new DevExpress.Utils.PointFloat(603.0417F, 45.99994F);
            this.masterPageInfoDate.Name = "masterPageInfoDate";
            this.masterPageInfoDate.PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime;
            this.masterPageInfoDate.SizeF = new System.Drawing.SizeF(136.9583F, 23F);
            this.masterPageInfoDate.StylePriority.UseTextAlignment = false;
            this.masterPageInfoDate.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.masterPageInfoDate.TextFormatString = "{0:MM/dd/yyyy hh:mm tt}";
            // 
            // lblCriteriaValues
            // 
            this.lblCriteriaValues.AllowMarkupText = true;
            this.lblCriteriaValues.LocationFloat = new DevExpress.Utils.PointFloat(0F, 46.20839F);
            this.lblCriteriaValues.Multiline = true;
            this.lblCriteriaValues.Name = "lblCriteriaValues";
            this.lblCriteriaValues.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblCriteriaValues.SizeF = new System.Drawing.SizeF(750F, 111.0001F);
            this.lblCriteriaValues.StylePriority.UseTextAlignment = false;
            this.lblCriteriaValues.Text = "lblCriteriaValues";
            this.lblCriteriaValues.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // lblCriteria
            // 
            this.lblCriteria.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.lblCriteria.LocationFloat = new DevExpress.Utils.PointFloat(0F, 15.70832F);
            this.lblCriteria.Multiline = true;
            this.lblCriteria.Name = "lblCriteria";
            this.lblCriteria.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblCriteria.SizeF = new System.Drawing.SizeF(750F, 23F);
            this.lblCriteria.StylePriority.UseFont = false;
            this.lblCriteria.StylePriority.UseTextAlignment = false;
            this.lblCriteria.Text = "Criteria Used for this Report";
            this.lblCriteria.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // imgLogo
            // 
            this.imgLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.imgLogo.ImageUrl = "Content\\images\\GenericLogo.png";
            this.imgLogo.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.imgLogo.Name = "imgLogo";
            this.imgLogo.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100F);
            this.imgLogo.SizeF = new System.Drawing.SizeF(130F, 80F);
            this.imgLogo.Sizing = DevExpress.XtraPrinting.ImageSizeMode.Squeeze;
            this.imgLogo.StylePriority.UseBackColor = false;
            this.imgLogo.StylePriority.UsePadding = false;
            this.imgLogo.BeforePrint += new System.Drawing.Printing.PrintEventHandler(this.imgLogo_BeforePrint);
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblAppTitle.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.LocationFloat = new DevExpress.Utils.PointFloat(130F, 25.62501F);
            this.lblAppTitle.Multiline = true;
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 2, 0, 0, 100F);
            this.lblAppTitle.SizeF = new System.Drawing.SizeF(620F, 28.74998F);
            this.lblAppTitle.StylePriority.UseBackColor = false;
            this.lblAppTitle.StylePriority.UseFont = false;
            this.lblAppTitle.StylePriority.UseForeColor = false;
            this.lblAppTitle.StylePriority.UsePadding = false;
            this.lblAppTitle.StylePriority.UseTextAlignment = false;
            this.lblAppTitle.Text = "Pyramid Model Implementation Data System";
            this.lblAppTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // lblReportTitle
            // 
            this.lblReportTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblReportTitle.BorderColor = System.Drawing.Color.Black;
            this.lblReportTitle.Font = new System.Drawing.Font("Arial", 14F);
            this.lblReportTitle.ForeColor = System.Drawing.Color.White;
            this.lblReportTitle.LocationFloat = new DevExpress.Utils.PointFloat(0F, 79.99998F);
            this.lblReportTitle.Multiline = true;
            this.lblReportTitle.Name = "lblReportTitle";
            this.lblReportTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblReportTitle.SizeF = new System.Drawing.SizeF(750F, 34.45834F);
            this.lblReportTitle.StylePriority.UseBackColor = false;
            this.lblReportTitle.StylePriority.UseBorderColor = false;
            this.lblReportTitle.StylePriority.UseFont = false;
            this.lblReportTitle.StylePriority.UseForeColor = false;
            this.lblReportTitle.StylePriority.UseTextAlignment = false;
            this.lblReportTitle.Text = "Title";
            this.lblReportTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // PageHeader
            // 
            this.PageHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.lblStateCatchphrase,
            this.lblStateName,
            this.lblReportTitle,
            this.lblAppTitle,
            this.imgLogo});
            this.PageHeader.HeightF = 120.7083F;
            this.PageHeader.Name = "PageHeader";
            // 
            // lblStateCatchphrase
            // 
            this.lblStateCatchphrase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStateCatchphrase.Font = new System.Drawing.Font("Arial", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.lblStateCatchphrase.ForeColor = System.Drawing.Color.White;
            this.lblStateCatchphrase.LocationFloat = new DevExpress.Utils.PointFloat(130F, 54.37498F);
            this.lblStateCatchphrase.Multiline = true;
            this.lblStateCatchphrase.Name = "lblStateCatchphrase";
            this.lblStateCatchphrase.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 2, 0, 0, 100F);
            this.lblStateCatchphrase.SizeF = new System.Drawing.SizeF(620F, 25.625F);
            this.lblStateCatchphrase.StylePriority.UseBackColor = false;
            this.lblStateCatchphrase.StylePriority.UseFont = false;
            this.lblStateCatchphrase.StylePriority.UseForeColor = false;
            this.lblStateCatchphrase.StylePriority.UsePadding = false;
            this.lblStateCatchphrase.StylePriority.UseTextAlignment = false;
            this.lblStateCatchphrase.Text = "State Catchphrase";
            this.lblStateCatchphrase.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // lblStateName
            // 
            this.lblStateName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStateName.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            this.lblStateName.ForeColor = System.Drawing.Color.White;
            this.lblStateName.LocationFloat = new DevExpress.Utils.PointFloat(130F, 0F);
            this.lblStateName.Multiline = true;
            this.lblStateName.Name = "lblStateName";
            this.lblStateName.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 2, 0, 0, 100F);
            this.lblStateName.SizeF = new System.Drawing.SizeF(620F, 25.62501F);
            this.lblStateName.StylePriority.UseBackColor = false;
            this.lblStateName.StylePriority.UseFont = false;
            this.lblStateName.StylePriority.UseForeColor = false;
            this.lblStateName.StylePriority.UsePadding = false;
            this.lblStateName.StylePriority.UseTextAlignment = false;
            this.lblStateName.Text = "State Name";
            this.lblStateName.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ParamLogoPath
            // 
            this.ParamLogoPath.AllowNull = true;
            this.ParamLogoPath.Description = "The relative file path to the logo";
            this.ParamLogoPath.Name = "ParamLogoPath";
            this.ParamLogoPath.Visible = false;
            // 
            // MasterReportFooter
            // 
            this.MasterReportFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.MasterFooterLine,
            this.lblCriteriaValues,
            this.lblCriteria});
            this.MasterReportFooter.HeightF = 157.2085F;
            this.MasterReportFooter.Name = "MasterReportFooter";
            this.MasterReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBand;
            // 
            // MasterFooterLine
            // 
            this.MasterFooterLine.LineStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            this.MasterFooterLine.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.MasterFooterLine.Name = "MasterFooterLine";
            this.MasterFooterLine.SizeF = new System.Drawing.SizeF(750F, 15.70832F);
            // 
            // RptLogoMaster
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.PageFooter,
            this.PageHeader,
            this.MasterReportFooter});
            this.ExportOptions.Xls.SheetName = "Report";
            this.ExportOptions.Xls.ShowGridLines = true;
            this.ExportOptions.Xlsx.DocumentOptions.Category = "Pyramid Implementation Data System";
            this.ExportOptions.Xlsx.SheetName = "Report";
            this.ExportOptions.Xlsx.ShowGridLines = true;
            this.Font = new System.Drawing.Font("Arial", 9.75F);
            this.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.ParamLogoPath});
            this.Version = "19.1";
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        protected DevExpress.XtraReports.UI.PageHeaderBand PageHeader;
        public DevExpress.XtraReports.UI.DetailBand Detail;
        public DevExpress.XtraReports.UI.XRPictureBox imgLogo;
        public DevExpress.XtraReports.UI.XRLabel lblReportTitle;
        public DevExpress.XtraReports.UI.XRLabel lblCriteriaValues;
        public DevExpress.XtraReports.UI.XRLabel lblStateCatchphrase;
        public DevExpress.XtraReports.UI.XRLabel lblStateName;
        public DevExpress.XtraReports.UI.XRLabel lblAppTitle;
        public DevExpress.XtraReports.UI.PageFooterBand PageFooter;
        public DevExpress.XtraReports.UI.XRPageInfo masterPageInfoNum;
        public DevExpress.XtraReports.UI.XRPageInfo masterPageInfoDate;
        public DevExpress.XtraReports.UI.XRLabel lblGenerated;
        public DevExpress.XtraReports.UI.XRLine footerLine;
        public DevExpress.XtraReports.UI.XRLabel lblCriteria;
        private DevExpress.XtraReports.Parameters.Parameter ParamLogoPath;
        public DevExpress.XtraReports.UI.ReportFooterBand MasterReportFooter;
        public DevExpress.XtraReports.UI.XRLine MasterFooterLine;
    }
}
