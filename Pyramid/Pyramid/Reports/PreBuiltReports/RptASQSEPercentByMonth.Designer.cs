namespace Pyramid.Reports.PreBuiltReports
{
    partial class RptASQSEPercentByMonth
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
            DevExpress.DataAccess.Sql.StoredProcQuery storedProcQuery1 = new DevExpress.DataAccess.Sql.StoredProcQuery();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter2 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter3 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter4 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter5 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter6 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter7 = new DevExpress.DataAccess.Sql.QueryParameter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RptASQSEPercentByMonth));
            DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.StackedBarSeriesView stackedBarSeriesView1 = new DevExpress.XtraCharts.StackedBarSeriesView();
            this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            this.xrChart1 = new DevExpress.XtraReports.UI.XRChart();
            this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel7 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel5 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
            this.GroupHeader1 = new DevExpress.XtraReports.UI.GroupHeaderBand();
            this.xrLine1 = new DevExpress.XtraReports.UI.XRLine();
            this.xrLabel14 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel10 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel11 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel12 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel13 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel3 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrPageBreak1 = new DevExpress.XtraReports.UI.XRPageBreak();
            ((System.ComponentModel.ISupportInitialize)(this.xrChart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(stackedBarSeriesView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // PageHeader
            // 
            this.PageHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel3,
            this.xrLabel14,
            this.xrLabel10,
            this.xrLabel11,
            this.xrLabel12,
            this.xrLabel13,
            this.xrLabel2});
            this.PageHeader.HeightF = 59.54168F;
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel4,
            this.xrLabel1});
            this.Detail.HeightF = 23F;
            this.Detail.KeepTogether = true;
            this.Detail.MultiColumn.ColumnCount = 4;
            this.Detail.MultiColumn.ColumnWidth = 200F;
            this.Detail.MultiColumn.Mode = DevExpress.XtraReports.UI.MultiColumnMode.UseColumnWidth;
            // 
            // lblCriteriaValues
            // 
            this.lblCriteriaValues.LocationFloat = new DevExpress.Utils.PointFloat(0F, 546.2883F);
            this.lblCriteriaValues.SizeF = new System.Drawing.SizeF(1000F, 145.7083F);
            this.lblCriteriaValues.StylePriority.UseTextAlignment = false;
            // 
            // lblCriteria
            // 
            this.lblCriteria.LocationFloat = new DevExpress.Utils.PointFloat(0F, 511.7883F);
            this.lblCriteria.SizeF = new System.Drawing.SizeF(1000F, 23F);
            this.lblCriteria.StylePriority.UseFont = false;
            this.lblCriteria.StylePriority.UseTextAlignment = false;
            // 
            // MasterReportFooter
            // 
            this.MasterReportFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrPageBreak1,
            this.xrChart1,
            this.xrLabel7});
            this.MasterReportFooter.HeightF = 691.9966F;
            this.MasterReportFooter.Controls.SetChildIndex(this.xrLabel7, 0);
            this.MasterReportFooter.Controls.SetChildIndex(this.MasterFooterLine, 0);
            this.MasterReportFooter.Controls.SetChildIndex(this.xrChart1, 0);
            this.MasterReportFooter.Controls.SetChildIndex(this.lblCriteria, 0);
            this.MasterReportFooter.Controls.SetChildIndex(this.lblCriteriaValues, 0);
            this.MasterReportFooter.Controls.SetChildIndex(this.xrPageBreak1, 0);
            // 
            // MasterFooterLine
            // 
            this.MasterFooterLine.LocationFloat = new DevExpress.Utils.PointFloat(0F, 496.08F);
            this.MasterFooterLine.SizeF = new System.Drawing.SizeF(1000F, 15.70831F);
            // 
            // imgLogo
            // 
            this.imgLogo.StylePriority.UseBackColor = false;
            this.imgLogo.StylePriority.UsePadding = false;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.SizeF = new System.Drawing.SizeF(830F, 34F);
            this.lblAppTitle.StylePriority.UseBackColor = false;
            this.lblAppTitle.StylePriority.UseFont = false;
            this.lblAppTitle.StylePriority.UseForeColor = false;
            this.lblAppTitle.StylePriority.UsePadding = false;
            this.lblAppTitle.StylePriority.UseTextAlignment = false;
            // 
            // lblReportTitle
            // 
            this.lblReportTitle.SizeF = new System.Drawing.SizeF(1000F, 34.45834F);
            this.lblReportTitle.StylePriority.UseBackColor = false;
            this.lblReportTitle.StylePriority.UseBorderColor = false;
            this.lblReportTitle.StylePriority.UseFont = false;
            this.lblReportTitle.StylePriority.UseForeColor = false;
            this.lblReportTitle.StylePriority.UseTextAlignment = false;
            // 
            // lblStateName
            // 
            this.lblStateName.SizeF = new System.Drawing.SizeF(830F, 33F);
            this.lblStateName.StylePriority.UseBackColor = false;
            this.lblStateName.StylePriority.UseFont = false;
            this.lblStateName.StylePriority.UseForeColor = false;
            this.lblStateName.StylePriority.UsePadding = false;
            this.lblStateName.StylePriority.UseTextAlignment = false;
            // 
            // lblStateCatchphrase
            // 
            this.lblStateCatchphrase.SizeF = new System.Drawing.SizeF(830F, 33F);
            this.lblStateCatchphrase.StylePriority.UseBackColor = false;
            this.lblStateCatchphrase.StylePriority.UseFont = false;
            this.lblStateCatchphrase.StylePriority.UseForeColor = false;
            this.lblStateCatchphrase.StylePriority.UsePadding = false;
            this.lblStateCatchphrase.StylePriority.UseTextAlignment = false;
            // 
            // masterPageInfoDate
            // 
            this.masterPageInfoDate.LocationFloat = new DevExpress.Utils.PointFloat(853.0417F, 45.99994F);
            this.masterPageInfoDate.StylePriority.UseTextAlignment = false;
            // 
            // masterPageInfoNum
            // 
            this.masterPageInfoNum.LocationFloat = new DevExpress.Utils.PointFloat(871.7917F, 22.99999F);
            this.masterPageInfoNum.StylePriority.UseTextAlignment = false;
            // 
            // lblGenerated
            // 
            this.lblGenerated.LocationFloat = new DevExpress.Utils.PointFloat(774.8334F, 45.99994F);
            this.lblGenerated.StylePriority.UseFont = false;
            this.lblGenerated.StylePriority.UseTextAlignment = false;
            // 
            // footerLine
            // 
            this.footerLine.SizeF = new System.Drawing.SizeF(1000F, 23F);
            // 
            // sqlDataSource1
            // 
            this.sqlDataSource1.ConnectionName = "Pyramid";
            this.sqlDataSource1.Name = "sqlDataSource1";
            storedProcQuery1.Name = "rspASQSEPercentByMonth";
            queryParameter1.Name = "@ClassroomFKs";
            queryParameter1.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter1.Value = new DevExpress.DataAccess.Expression("?ParamClassroomFKs", typeof(string));
            queryParameter2.Name = "@StartDate";
            queryParameter2.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter2.Value = new DevExpress.DataAccess.Expression("?ParamStartDate", typeof(System.DateTime));
            queryParameter3.Name = "@EndDate";
            queryParameter3.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter3.Value = new DevExpress.DataAccess.Expression("?ParamEndDate", typeof(System.DateTime));
            queryParameter4.Name = "@ProgramFKs";
            queryParameter4.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter4.Value = new DevExpress.DataAccess.Expression("?ParamProgramFKs", typeof(string));
            queryParameter5.Name = "@HubFKs";
            queryParameter5.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter5.Value = new DevExpress.DataAccess.Expression("?ParamHubFKs", typeof(string));
            queryParameter6.Name = "@CohortFKs";
            queryParameter6.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter6.Value = new DevExpress.DataAccess.Expression("?ParamCohortFKs", typeof(string));
            queryParameter7.Name = "@StateFKs";
            queryParameter7.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter7.Value = new DevExpress.DataAccess.Expression("?ParamStateFKs", typeof(string));
            storedProcQuery1.Parameters.Add(queryParameter1);
            storedProcQuery1.Parameters.Add(queryParameter2);
            storedProcQuery1.Parameters.Add(queryParameter3);
            storedProcQuery1.Parameters.Add(queryParameter4);
            storedProcQuery1.Parameters.Add(queryParameter5);
            storedProcQuery1.Parameters.Add(queryParameter6);
            storedProcQuery1.Parameters.Add(queryParameter7);
            storedProcQuery1.StoredProcName = "rspASQSEPercentByMonth";
            this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
            this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
            // 
            // xrChart1
            // 
            this.xrChart1.BorderColor = System.Drawing.Color.Black;
            this.xrChart1.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrChart1.DataMember = "rspASQSEPercentByMonth";
            this.xrChart1.DataSource = this.sqlDataSource1;
            xyDiagram1.AxisX.Label.TextPattern = "{A} ASQ:SE";
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.Label.TextPattern = "{V:0%}";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.VisualRange.Auto = false;
            xyDiagram1.AxisY.VisualRange.AutoSideMargins = false;
            xyDiagram1.AxisY.VisualRange.EndSideMargin = 0D;
            xyDiagram1.AxisY.VisualRange.MaxValueSerializable = "1.1";
            xyDiagram1.AxisY.VisualRange.MinValueSerializable = "0";
            xyDiagram1.AxisY.VisualRange.StartSideMargin = 0D;
            xyDiagram1.AxisY.WholeRange.Auto = false;
            xyDiagram1.AxisY.WholeRange.AutoSideMargins = false;
            xyDiagram1.AxisY.WholeRange.EndSideMargin = 0D;
            xyDiagram1.AxisY.WholeRange.MaxValueSerializable = "1.1";
            xyDiagram1.AxisY.WholeRange.MinValueSerializable = "0";
            xyDiagram1.AxisY.WholeRange.StartSideMargin = 0D;
            xyDiagram1.DefaultPane.EnableAxisXScrolling = DevExpress.Utils.DefaultBoolean.False;
            xyDiagram1.DefaultPane.EnableAxisXZooming = DevExpress.Utils.DefaultBoolean.False;
            xyDiagram1.DefaultPane.EnableAxisYScrolling = DevExpress.Utils.DefaultBoolean.False;
            xyDiagram1.DefaultPane.EnableAxisYZooming = DevExpress.Utils.DefaultBoolean.False;
            this.xrChart1.Diagram = xyDiagram1;
            this.xrChart1.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.Center;
            this.xrChart1.Legend.AlignmentVertical = DevExpress.XtraCharts.LegendAlignmentVertical.TopOutside;
            this.xrChart1.Legend.Direction = DevExpress.XtraCharts.LegendDirection.LeftToRight;
            this.xrChart1.Legend.Name = "Default Legend";
            this.xrChart1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 23.00002F);
            this.xrChart1.Name = "xrChart1";
            this.xrChart1.SeriesDataMember = "ScoreType";
            this.xrChart1.SeriesSerializable = new DevExpress.XtraCharts.Series[0];
            this.xrChart1.SeriesTemplate.ArgumentDataMember = "IntervalDescription";
            this.xrChart1.SeriesTemplate.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;
            this.xrChart1.SeriesTemplate.SeriesDataMember = "ScoreType";
            this.xrChart1.SeriesTemplate.ValueDataMembersSerializable = "PercentForIntervalScoreType";
            this.xrChart1.SeriesTemplate.View = stackedBarSeriesView1;
            this.xrChart1.SizeF = new System.Drawing.SizeF(1000F, 471.08F);
            // 
            // xrLabel1
            // 
            this.xrLabel1.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[ScoreType]")});
            this.xrLabel1.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(16.00001F, 0F);
            this.xrLabel1.Multiline = true;
            this.xrLabel1.Name = "xrLabel1";
            this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel1.SizeF = new System.Drawing.SizeF(100F, 23F);
            this.xrLabel1.StylePriority.UseFont = false;
            this.xrLabel1.StylePriority.UseTextAlignment = false;
            this.xrLabel1.Text = "xrLabel1";
            this.xrLabel1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // xrLabel4
            // 
            this.xrLabel4.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "FormatString(\'{0} ({1:0%})\', [NumIntervalScoreTypeForms], [PercentForIntervalScor" +
                    "eType])")});
            this.xrLabel4.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(116F, 0F);
            this.xrLabel4.Multiline = true;
            this.xrLabel4.Name = "xrLabel4";
            this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel4.SizeF = new System.Drawing.SizeF(65.83334F, 23F);
            this.xrLabel4.StylePriority.UseFont = false;
            this.xrLabel4.StylePriority.UseTextAlignment = false;
            this.xrLabel4.Text = "xrLabel4";
            this.xrLabel4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // xrLabel7
            // 
            this.xrLabel7.Font = new DevExpress.Drawing.DXFont("Arial", 11F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel7.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.xrLabel7.Multiline = true;
            this.xrLabel7.Name = "xrLabel7";
            this.xrLabel7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel7.SizeF = new System.Drawing.SizeF(1000F, 22.99998F);
            this.xrLabel7.StylePriority.UseFont = false;
            this.xrLabel7.StylePriority.UseTextAlignment = false;
            this.xrLabel7.Text = "Score Type Percentage by Month Chart";
            this.xrLabel7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // xrLabel5
            // 
            this.xrLabel5.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Tag", "[IntervalDescription]"),
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Concat([NumIntervalForms], \' Screenings\')")});
            this.xrLabel5.LocationFloat = new DevExpress.Utils.PointFloat(15.99997F, 23.00002F);
            this.xrLabel5.Multiline = true;
            this.xrLabel5.Name = "xrLabel5";
            this.xrLabel5.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel5.SizeF = new System.Drawing.SizeF(165.8333F, 23F);
            this.xrLabel5.StylePriority.UseTextAlignment = false;
            this.xrLabel5.Text = "xrLabel3";
            this.xrLabel5.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // xrLabel6
            // 
            this.xrLabel6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Tag", "[IntervalDescription]"),
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Concat([IntervalMonth], \' Month ASQ:SE\')")});
            this.xrLabel6.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(16.00001F, 0F);
            this.xrLabel6.Multiline = true;
            this.xrLabel6.Name = "xrLabel6";
            this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel6.SizeF = new System.Drawing.SizeF(165.8333F, 23F);
            this.xrLabel6.StylePriority.UseFont = false;
            this.xrLabel6.StylePriority.UseTextAlignment = false;
            this.xrLabel6.Text = "xrLabel2";
            this.xrLabel6.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // GroupHeader1
            // 
            this.GroupHeader1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLine1,
            this.xrLabel5,
            this.xrLabel6});
            this.GroupHeader1.GroupFields.AddRange(new DevExpress.XtraReports.UI.GroupField[] {
            new DevExpress.XtraReports.UI.GroupField("IntervalMonth", DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending)});
            this.GroupHeader1.HeightF = 58.99998F;
            this.GroupHeader1.Name = "GroupHeader1";
            this.GroupHeader1.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBandExceptFirstEntry;
            // 
            // xrLine1
            // 
            this.xrLine1.LocationFloat = new DevExpress.Utils.PointFloat(16.00001F, 46F);
            this.xrLine1.Name = "xrLine1";
            this.xrLine1.SizeF = new System.Drawing.SizeF(165.8333F, 12.99998F);
            // 
            // xrLabel14
            // 
            this.xrLabel14.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel14.LocationFloat = new DevExpress.Utils.PointFloat(602.2083F, 0F);
            this.xrLabel14.Multiline = true;
            this.xrLabel14.Name = "xrLabel14";
            this.xrLabel14.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel14.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel14.SizeF = new System.Drawing.SizeF(219.7917F, 23.00002F);
            this.xrLabel14.StylePriority.UseFont = false;
            this.xrLabel14.Text = "Number of Screenings Included:";
            // 
            // xrLabel10
            // 
            this.xrLabel10.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel10.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.xrLabel10.Multiline = true;
            this.xrLabel10.Name = "xrLabel10";
            this.xrLabel10.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel10.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel10.SizeF = new System.Drawing.SizeF(145F, 23.00002F);
            this.xrLabel10.StylePriority.UseFont = false;
            this.xrLabel10.Text = "First Screening Date:";
            // 
            // xrLabel11
            // 
            this.xrLabel11.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[FirstFormDate]")});
            this.xrLabel11.LocationFloat = new DevExpress.Utils.PointFloat(144.9999F, 0F);
            this.xrLabel11.Multiline = true;
            this.xrLabel11.Name = "xrLabel11";
            this.xrLabel11.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel11.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel11.SizeF = new System.Drawing.SizeF(100F, 23F);
            this.xrLabel11.Text = "xrLabel3";
            this.xrLabel11.TextFormatString = "{0:MM/dd/yyyy}";
            // 
            // xrLabel12
            // 
            this.xrLabel12.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[LastFormDate]")});
            this.xrLabel12.LocationFloat = new DevExpress.Utils.PointFloat(447.2083F, 0F);
            this.xrLabel12.Multiline = true;
            this.xrLabel12.Name = "xrLabel12";
            this.xrLabel12.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel12.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel12.SizeF = new System.Drawing.SizeF(100F, 23F);
            this.xrLabel12.Text = "xrLabel5";
            this.xrLabel12.TextFormatString = "{0:MM/dd/yyyy}";
            // 
            // xrLabel13
            // 
            this.xrLabel13.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel13.LocationFloat = new DevExpress.Utils.PointFloat(302.2084F, 0F);
            this.xrLabel13.Multiline = true;
            this.xrLabel13.Name = "xrLabel13";
            this.xrLabel13.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel13.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel13.SizeF = new System.Drawing.SizeF(145F, 23.00002F);
            this.xrLabel13.StylePriority.UseFont = false;
            this.xrLabel13.Text = "Last Screening Date:";
            // 
            // xrLabel2
            // 
            this.xrLabel2.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[TotalForms]")});
            this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(822F, 0F);
            this.xrLabel2.Multiline = true;
            this.xrLabel2.Name = "xrLabel2";
            this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel2.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel2.SizeF = new System.Drawing.SizeF(100F, 23F);
            this.xrLabel2.StylePriority.UseTextAlignment = false;
            this.xrLabel2.Text = "xrLabel5";
            this.xrLabel2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // xrLabel3
            // 
            this.xrLabel3.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrLabel3.Font = new DevExpress.Drawing.DXFont("Arial", 11F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel3.LocationFloat = new DevExpress.Utils.PointFloat(0F, 36.54169F);
            this.xrLabel3.Multiline = true;
            this.xrLabel3.Name = "xrLabel3";
            this.xrLabel3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel3.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.SuppressAndShrink;
            this.xrLabel3.SizeF = new System.Drawing.SizeF(1000F, 22.99998F);
            this.xrLabel3.StylePriority.UseBorders = false;
            this.xrLabel3.StylePriority.UseFont = false;
            this.xrLabel3.StylePriority.UseTextAlignment = false;
            this.xrLabel3.Text = "Score Type Percentage by Month Details";
            this.xrLabel3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // xrPageBreak1
            // 
            this.xrPageBreak1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 494.08F);
            this.xrPageBreak1.Name = "xrPageBreak1";
            // 
            // RptASQSEPercentByMonth
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.Detail,
            this.PageFooter,
            this.PageHeader,
            this.GroupHeader1,
            this.MasterReportFooter,
            this.TopMargin,
            this.BottomMargin});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
            this.DataMember = "rspASQSEPercentByMonth";
            this.DataSource = this.sqlDataSource1;
            this.ExportOptions.Xls.SheetName = "Report";
            this.ExportOptions.Xls.ShowGridLines = true;
            this.ExportOptions.Xlsx.DocumentOptions.Category = "Pyramid Implementation Data System";
            this.ExportOptions.Xlsx.SheetName = "Report";
            this.ExportOptions.Xlsx.ShowGridLines = true;
            this.Landscape = true;
            this.PageHeight = 850;
            this.PageWidth = 1100;
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.ParamLogoPath,
            this.ParamViewPrivateChildInfo,
            this.ParamProgramFKs,
            this.ParamStateFKs,
            this.ParamStartDate,
            this.ParamEndDate,
            this.ParamPointInTime,
            this.ParamYear,
            this.ParamBIRProfileGroup,
            this.ParamBIRItem,
            this.ParamClassroomFKs,
            this.ParamChildFKs,
            this.ParamRaceFKs,
            this.ParamEthnicityFKs,
            this.ParamGenderFKs,
            this.ParamIEP,
            this.ParamDLL,
            this.ParamEmployeeFKs,
            this.ParamTeacherFKs,
            this.ParamCoachFKs,
            this.ParamProblemBehaviorFKs,
            this.ParamActivityFKs,
            this.ParamOthersInvolvedFKs,
            this.ParamPossibleMotivationFKs,
            this.ParamStrategyResponseFKs,
            this.ParamAdminFollowUpFKs,
            this.ParamViewPrivateEmployeeInfo});
            this.Version = "21.1";
            this.Controls.SetChildIndex(this.BottomMargin, 0);
            this.Controls.SetChildIndex(this.TopMargin, 0);
            this.Controls.SetChildIndex(this.MasterReportFooter, 0);
            this.Controls.SetChildIndex(this.GroupHeader1, 0);
            this.Controls.SetChildIndex(this.PageHeader, 0);
            this.Controls.SetChildIndex(this.PageFooter, 0);
            this.Controls.SetChildIndex(this.Detail, 0);
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(stackedBarSeriesView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xrChart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }


        #endregion

        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.XtraReports.UI.XRChart xrChart1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel4;
        private DevExpress.XtraReports.UI.XRLabel xrLabel1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel5;
        private DevExpress.XtraReports.UI.XRLabel xrLabel6;
        private DevExpress.XtraReports.UI.GroupHeaderBand GroupHeader1;
        private DevExpress.XtraReports.UI.XRLine xrLine1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel14;
        private DevExpress.XtraReports.UI.XRLabel xrLabel10;
        private DevExpress.XtraReports.UI.XRLabel xrLabel11;
        private DevExpress.XtraReports.UI.XRLabel xrLabel12;
        private DevExpress.XtraReports.UI.XRLabel xrLabel13;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel3;
        private DevExpress.XtraReports.UI.XRLabel xrLabel7;
        private DevExpress.XtraReports.UI.XRPageBreak xrPageBreak1;
    }
}
