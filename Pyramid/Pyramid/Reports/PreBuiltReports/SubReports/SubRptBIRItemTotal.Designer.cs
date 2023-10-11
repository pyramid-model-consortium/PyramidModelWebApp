namespace Pyramid.Reports.PreBuiltReports.SubReports
{
    partial class SubRptBIRItemTotal
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
            DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.ItemDetail = new DevExpress.XtraReports.UI.DetailBand();
            this.lblActivityPercent = new DevExpress.XtraReports.UI.XRLabel();
            this.lblActivityNumIncidents = new DevExpress.XtraReports.UI.XRLabel();
            this.lblItemAbbreviation = new DevExpress.XtraReports.UI.XRLabel();
            this.lblItemDescription = new DevExpress.XtraReports.UI.XRLabel();
            this.ItemReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.lblItemSection = new DevExpress.XtraReports.UI.XRLabel();
            this.xrPageBreak3 = new DevExpress.XtraReports.UI.XRPageBreak();
            this.ItemTotalChart = new DevExpress.XtraReports.UI.XRChart();
            this.lblItemTotalChartTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.ItemGroupHeader = new DevExpress.XtraReports.UI.GroupHeaderBand();
            this.lblChartInformationTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel21 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel20 = new DevExpress.XtraReports.UI.XRLabel();
            this.lblItemNameTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel18 = new DevExpress.XtraReports.UI.XRLabel();
            this.ItemGroupFooter = new DevExpress.XtraReports.UI.GroupFooterBand();
            this.xrPageBreak1 = new DevExpress.XtraReports.UI.XRPageBreak();
            this.lblActivityPercentAverage = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel19 = new DevExpress.XtraReports.UI.XRLabel();
            this.lblActivityNumIncidentsAverage = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel22 = new DevExpress.XtraReports.UI.XRLabel();
            ((System.ComponentModel.ISupportInitialize)(this.ItemTotalChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
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
            // ItemDetail
            // 
            this.ItemDetail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.lblActivityPercent,
            this.lblActivityNumIncidents,
            this.lblItemAbbreviation,
            this.lblItemDescription});
            this.ItemDetail.HeightF = 23F;
            this.ItemDetail.Name = "ItemDetail";
            // 
            // lblActivityPercent
            // 
            this.lblActivityPercent.LocationFloat = new DevExpress.Utils.PointFloat(667.0833F, 0F);
            this.lblActivityPercent.Multiline = true;
            this.lblActivityPercent.Name = "lblActivityPercent";
            this.lblActivityPercent.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblActivityPercent.SizeF = new System.Drawing.SizeF(144.625F, 23F);
            this.lblActivityPercent.StylePriority.UseTextAlignment = false;
            this.lblActivityPercent.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.lblActivityPercent.TextFormatString = "{0:0%}";
            // 
            // lblActivityNumIncidents
            // 
            this.lblActivityNumIncidents.LocationFloat = new DevExpress.Utils.PointFloat(548.3333F, 0F);
            this.lblActivityNumIncidents.Multiline = true;
            this.lblActivityNumIncidents.Name = "lblActivityNumIncidents";
            this.lblActivityNumIncidents.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblActivityNumIncidents.SizeF = new System.Drawing.SizeF(118.75F, 23F);
            this.lblActivityNumIncidents.StylePriority.UseTextAlignment = false;
            this.lblActivityNumIncidents.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // lblItemAbbreviation
            // 
            this.lblItemAbbreviation.LocationFloat = new DevExpress.Utils.PointFloat(429.5833F, 0F);
            this.lblItemAbbreviation.Multiline = true;
            this.lblItemAbbreviation.Name = "lblItemAbbreviation";
            this.lblItemAbbreviation.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblItemAbbreviation.SizeF = new System.Drawing.SizeF(118.75F, 23F);
            this.lblItemAbbreviation.StylePriority.UseTextAlignment = false;
            this.lblItemAbbreviation.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // lblItemDescription
            // 
            this.lblItemDescription.LocationFloat = new DevExpress.Utils.PointFloat(180.8333F, 0F);
            this.lblItemDescription.Multiline = true;
            this.lblItemDescription.Name = "lblItemDescription";
            this.lblItemDescription.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblItemDescription.SizeF = new System.Drawing.SizeF(248.75F, 23F);
            this.lblItemDescription.StylePriority.UseTextAlignment = false;
            this.lblItemDescription.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ItemReportHeader
            // 
            this.ItemReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.lblItemSection,
            this.xrPageBreak3,
            this.ItemTotalChart,
            this.lblItemTotalChartTitle});
            this.ItemReportHeader.HeightF = 506.3334F;
            this.ItemReportHeader.Name = "ItemReportHeader";
            // 
            // lblItemSection
            // 
            this.lblItemSection.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.lblItemSection.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.lblItemSection.Multiline = true;
            this.lblItemSection.Name = "lblItemSection";
            this.lblItemSection.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblItemSection.SizeF = new System.Drawing.SizeF(1000F, 2.166684F);
            this.lblItemSection.StylePriority.UseFont = false;
            this.lblItemSection.StylePriority.UseTextAlignment = false;
            this.lblItemSection.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // xrPageBreak3
            // 
            this.xrPageBreak3.LocationFloat = new DevExpress.Utils.PointFloat(0F, 504.3334F);
            this.xrPageBreak3.Name = "xrPageBreak3";
            // 
            // ItemTotalChart
            // 
            this.ItemTotalChart.BorderColor = System.Drawing.Color.Black;
            this.ItemTotalChart.Borders = DevExpress.XtraPrinting.BorderSide.None;
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            xyDiagram1.DefaultPane.EnableAxisXScrolling = DevExpress.Utils.DefaultBoolean.False;
            xyDiagram1.DefaultPane.EnableAxisXZooming = DevExpress.Utils.DefaultBoolean.False;
            xyDiagram1.DefaultPane.EnableAxisYScrolling = DevExpress.Utils.DefaultBoolean.False;
            xyDiagram1.DefaultPane.EnableAxisYZooming = DevExpress.Utils.DefaultBoolean.False;
            this.ItemTotalChart.Diagram = xyDiagram1;
            this.ItemTotalChart.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.Center;
            this.ItemTotalChart.Legend.AlignmentVertical = DevExpress.XtraCharts.LegendAlignmentVertical.TopOutside;
            this.ItemTotalChart.Legend.Name = "Default Legend";
            this.ItemTotalChart.LocationFloat = new DevExpress.Utils.PointFloat(0F, 25.1667F);
            this.ItemTotalChart.Name = "ItemTotalChart";
            this.ItemTotalChart.PaletteName = "Mixed";
            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;
            series1.Name = "# of BIRs";
            series1.SeriesPointsSorting = DevExpress.XtraCharts.SortingMode.Descending;
            series1.SeriesPointsSortingKey = DevExpress.XtraCharts.SeriesPointKey.Value_1;
            this.ItemTotalChart.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1};
            this.ItemTotalChart.SizeF = new System.Drawing.SizeF(1000F, 479.1667F);
            // 
            // lblItemTotalChartTitle
            // 
            this.lblItemTotalChartTitle.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.lblItemTotalChartTitle.LocationFloat = new DevExpress.Utils.PointFloat(0F, 2.166684F);
            this.lblItemTotalChartTitle.Multiline = true;
            this.lblItemTotalChartTitle.Name = "lblItemTotalChartTitle";
            this.lblItemTotalChartTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblItemTotalChartTitle.SizeF = new System.Drawing.SizeF(1000F, 23.00002F);
            this.lblItemTotalChartTitle.StylePriority.UseFont = false;
            this.lblItemTotalChartTitle.StylePriority.UseTextAlignment = false;
            this.lblItemTotalChartTitle.Text = "BIRs by Item Chart";
            this.lblItemTotalChartTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // ItemGroupHeader
            // 
            this.ItemGroupHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.lblChartInformationTitle,
            this.xrLabel21,
            this.xrLabel20,
            this.lblItemNameTitle,
            this.xrLabel18});
            this.ItemGroupHeader.HeightF = 63.12592F;
            this.ItemGroupHeader.Name = "ItemGroupHeader";
            // 
            // lblChartInformationTitle
            // 
            this.lblChartInformationTitle.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.lblChartInformationTitle.LocationFloat = new DevExpress.Utils.PointFloat(180.8333F, 0F);
            this.lblChartInformationTitle.Multiline = true;
            this.lblChartInformationTitle.Name = "lblChartInformationTitle";
            this.lblChartInformationTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblChartInformationTitle.SizeF = new System.Drawing.SizeF(630.8749F, 22.99994F);
            this.lblChartInformationTitle.StylePriority.UseFont = false;
            this.lblChartInformationTitle.StylePriority.UseTextAlignment = false;
            this.lblChartInformationTitle.Text = "BIRs by Item Table";
            this.lblChartInformationTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // xrLabel21
            // 
            this.xrLabel21.Borders = DevExpress.XtraPrinting.BorderSide.Bottom;
            this.xrLabel21.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel21.LocationFloat = new DevExpress.Utils.PointFloat(667.0833F, 40.12585F);
            this.xrLabel21.Multiline = true;
            this.xrLabel21.Name = "xrLabel21";
            this.xrLabel21.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel21.SizeF = new System.Drawing.SizeF(144.625F, 23.00006F);
            this.xrLabel21.StylePriority.UseBorders = false;
            this.xrLabel21.StylePriority.UseFont = false;
            this.xrLabel21.StylePriority.UseTextAlignment = false;
            this.xrLabel21.Text = "% of Total BIRs";
            this.xrLabel21.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
            // 
            // xrLabel20
            // 
            this.xrLabel20.Borders = DevExpress.XtraPrinting.BorderSide.Bottom;
            this.xrLabel20.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel20.LocationFloat = new DevExpress.Utils.PointFloat(548.5416F, 40.12585F);
            this.xrLabel20.Multiline = true;
            this.xrLabel20.Name = "xrLabel20";
            this.xrLabel20.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel20.SizeF = new System.Drawing.SizeF(118.5417F, 23.00006F);
            this.xrLabel20.StylePriority.UseBorders = false;
            this.xrLabel20.StylePriority.UseFont = false;
            this.xrLabel20.StylePriority.UseTextAlignment = false;
            this.xrLabel20.Text = "# of BIRs";
            this.xrLabel20.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
            // 
            // lblItemNameTitle
            // 
            this.lblItemNameTitle.Borders = DevExpress.XtraPrinting.BorderSide.Bottom;
            this.lblItemNameTitle.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.lblItemNameTitle.LocationFloat = new DevExpress.Utils.PointFloat(180.8333F, 40.12585F);
            this.lblItemNameTitle.Multiline = true;
            this.lblItemNameTitle.Name = "lblItemNameTitle";
            this.lblItemNameTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblItemNameTitle.SizeF = new System.Drawing.SizeF(248.75F, 23.00006F);
            this.lblItemNameTitle.StylePriority.UseBorders = false;
            this.lblItemNameTitle.StylePriority.UseFont = false;
            this.lblItemNameTitle.StylePriority.UseTextAlignment = false;
            this.lblItemNameTitle.Text = "Item";
            this.lblItemNameTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomLeft;
            // 
            // xrLabel18
            // 
            this.xrLabel18.Borders = DevExpress.XtraPrinting.BorderSide.Bottom;
            this.xrLabel18.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel18.LocationFloat = new DevExpress.Utils.PointFloat(429.7917F, 40.12585F);
            this.xrLabel18.Multiline = true;
            this.xrLabel18.Name = "xrLabel18";
            this.xrLabel18.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel18.SizeF = new System.Drawing.SizeF(118.75F, 23.00006F);
            this.xrLabel18.StylePriority.UseBorders = false;
            this.xrLabel18.StylePriority.UseFont = false;
            this.xrLabel18.StylePriority.UseTextAlignment = false;
            this.xrLabel18.Text = "Abbreviation";
            this.xrLabel18.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomLeft;
            // 
            // ItemGroupFooter
            // 
            this.ItemGroupFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrPageBreak1,
            this.lblActivityPercentAverage,
            this.xrLabel19,
            this.lblActivityNumIncidentsAverage,
            this.xrLabel22});
            this.ItemGroupFooter.HeightF = 25.00006F;
            this.ItemGroupFooter.Name = "ItemGroupFooter";
            // 
            // xrPageBreak1
            // 
            this.xrPageBreak1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 23.00006F);
            this.xrPageBreak1.Name = "xrPageBreak1";
            // 
            // lblActivityPercentAverage
            // 
            this.lblActivityPercentAverage.Borders = DevExpress.XtraPrinting.BorderSide.Top;
            this.lblActivityPercentAverage.LocationFloat = new DevExpress.Utils.PointFloat(667.0833F, 0F);
            this.lblActivityPercentAverage.Multiline = true;
            this.lblActivityPercentAverage.Name = "lblActivityPercentAverage";
            this.lblActivityPercentAverage.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblActivityPercentAverage.SizeF = new System.Drawing.SizeF(144.625F, 23F);
            this.lblActivityPercentAverage.StylePriority.UseBorders = false;
            this.lblActivityPercentAverage.StylePriority.UseTextAlignment = false;
            this.lblActivityPercentAverage.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.lblActivityPercentAverage.TextFormatString = "{0:0%}";
            // 
            // xrLabel19
            // 
            this.xrLabel19.Borders = DevExpress.XtraPrinting.BorderSide.Top;
            this.xrLabel19.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel19.LocationFloat = new DevExpress.Utils.PointFloat(429.5834F, 0F);
            this.xrLabel19.Multiline = true;
            this.xrLabel19.Name = "xrLabel19";
            this.xrLabel19.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel19.SizeF = new System.Drawing.SizeF(118.75F, 23.00006F);
            this.xrLabel19.StylePriority.UseBorders = false;
            this.xrLabel19.StylePriority.UseFont = false;
            this.xrLabel19.StylePriority.UseTextAlignment = false;
            this.xrLabel19.Text = "Average:";
            this.xrLabel19.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // lblActivityNumIncidentsAverage
            // 
            this.lblActivityNumIncidentsAverage.Borders = DevExpress.XtraPrinting.BorderSide.Top;
            this.lblActivityNumIncidentsAverage.LocationFloat = new DevExpress.Utils.PointFloat(548.3333F, 0F);
            this.lblActivityNumIncidentsAverage.Multiline = true;
            this.lblActivityNumIncidentsAverage.Name = "lblActivityNumIncidentsAverage";
            this.lblActivityNumIncidentsAverage.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.lblActivityNumIncidentsAverage.SizeF = new System.Drawing.SizeF(118.75F, 23F);
            this.lblActivityNumIncidentsAverage.StylePriority.UseBorders = false;
            this.lblActivityNumIncidentsAverage.StylePriority.UseTextAlignment = false;
            this.lblActivityNumIncidentsAverage.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.lblActivityNumIncidentsAverage.TextFormatString = "{0:#.00}";
            // 
            // xrLabel22
            // 
            this.xrLabel22.Borders = DevExpress.XtraPrinting.BorderSide.Top;
            this.xrLabel22.LocationFloat = new DevExpress.Utils.PointFloat(180.8333F, 0F);
            this.xrLabel22.Multiline = true;
            this.xrLabel22.Name = "xrLabel22";
            this.xrLabel22.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel22.SizeF = new System.Drawing.SizeF(248.75F, 23F);
            this.xrLabel22.StylePriority.UseBorders = false;
            this.xrLabel22.StylePriority.UseTextAlignment = false;
            this.xrLabel22.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // SubRptBIRItemTotal
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.ItemDetail,
            this.ItemReportHeader,
            this.ItemGroupHeader,
            this.ItemGroupFooter});
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Landscape = true;
            this.Margins = new DevExpress.Drawing.DXMargins(50, 50, 50, 50);
            this.PageHeight = 850;
            this.PageWidth = 1100;
            this.Version = "21.1";
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemTotalChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand ItemDetail;
        private DevExpress.XtraReports.UI.ReportHeaderBand ItemReportHeader;
        private DevExpress.XtraReports.UI.GroupHeaderBand ItemGroupHeader;
        private DevExpress.XtraReports.UI.GroupFooterBand ItemGroupFooter;
        private DevExpress.XtraReports.UI.XRChart ItemTotalChart;
        private DevExpress.XtraReports.UI.XRLabel lblItemTotalChartTitle;
        private DevExpress.XtraReports.UI.XRPageBreak xrPageBreak3;
        private DevExpress.XtraReports.UI.XRLabel lblChartInformationTitle;
        private DevExpress.XtraReports.UI.XRLabel xrLabel21;
        private DevExpress.XtraReports.UI.XRLabel xrLabel20;
        private DevExpress.XtraReports.UI.XRLabel lblItemNameTitle;
        private DevExpress.XtraReports.UI.XRLabel xrLabel18;
        private DevExpress.XtraReports.UI.XRLabel lblActivityPercent;
        private DevExpress.XtraReports.UI.XRLabel lblActivityNumIncidents;
        private DevExpress.XtraReports.UI.XRLabel lblItemAbbreviation;
        private DevExpress.XtraReports.UI.XRLabel lblItemDescription;
        private DevExpress.XtraReports.UI.XRLabel lblActivityPercentAverage;
        private DevExpress.XtraReports.UI.XRLabel xrLabel19;
        private DevExpress.XtraReports.UI.XRLabel lblActivityNumIncidentsAverage;
        private DevExpress.XtraReports.UI.XRLabel xrLabel22;
        private DevExpress.XtraReports.UI.XRLabel lblItemSection;
        private DevExpress.XtraReports.UI.XRPageBreak xrPageBreak1;
    }
}
