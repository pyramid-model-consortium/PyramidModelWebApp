namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    partial class RptClassroom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RptClassroom));
            this.xrLabel9 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLine1 = new DevExpress.XtraReports.UI.XRLine();
            this.xrLabel17 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel8 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel7 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel3 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel5 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel10 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel11 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel12 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel13 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel14 = new DevExpress.XtraReports.UI.XRLabel();
            this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            this.subRptChildClassroomAssignments = new DevExpress.XtraReports.UI.XRSubreport();
            this.subRptEmployeeClassroomAssignments = new DevExpress.XtraReports.UI.XRSubreport();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.subRptEmployeeClassroomAssignments,
            this.subRptChildClassroomAssignments,
            this.xrLabel13,
            this.xrLabel14,
            this.xrLabel11,
            this.xrLabel12,
            this.xrLabel6,
            this.xrLabel10,
            this.xrLabel4,
            this.xrLabel5,
            this.xrLabel2,
            this.xrLabel3,
            this.xrLabel8,
            this.xrLabel7,
            this.xrLabel9,
            this.xrLabel1,
            this.xrLine1,
            this.xrLabel17});
            this.Detail.HeightF = 263.25F;
            // 
            // lblCriteriaValues
            // 
            this.lblCriteriaValues.StylePriority.UseTextAlignment = false;
            // 
            // lblCriteria
            // 
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
            // xrLabel9
            // 
            this.xrLabel9.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[ProgramName]")});
            this.xrLabel9.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel9.LocationFloat = new DevExpress.Utils.PointFloat(70.83333F, 40.41667F);
            this.xrLabel9.Multiline = true;
            this.xrLabel9.Name = "xrLabel9";
            this.xrLabel9.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel9.SizeF = new System.Drawing.SizeF(310F, 23F);
            this.xrLabel9.StylePriority.UseFont = false;
            // 
            // xrLabel1
            // 
            this.xrLabel1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 40.41667F);
            this.xrLabel1.Multiline = true;
            this.xrLabel1.Name = "xrLabel1";
            this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel1.SizeF = new System.Drawing.SizeF(70.83334F, 23F);
            this.xrLabel1.StylePriority.UseFont = false;
            this.xrLabel1.Text = "Program:";
            // 
            // xrLine1
            // 
            this.xrLine1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 22.99999F);
            this.xrLine1.Name = "xrLine1";
            this.xrLine1.SizeF = new System.Drawing.SizeF(750F, 8.416687F);
            // 
            // xrLabel17
            // 
            this.xrLabel17.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel17.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.xrLabel17.Multiline = true;
            this.xrLabel17.Name = "xrLabel17";
            this.xrLabel17.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel17.SizeF = new System.Drawing.SizeF(230.8333F, 22.99999F);
            this.xrLabel17.StylePriority.UseFont = false;
            this.xrLabel17.Text = "Basic Information";
            // 
            // xrLabel8
            // 
            this.xrLabel8.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel8.LocationFloat = new DevExpress.Utils.PointFloat(0F, 77.41667F);
            this.xrLabel8.Multiline = true;
            this.xrLabel8.Name = "xrLabel8";
            this.xrLabel8.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel8.SizeF = new System.Drawing.SizeF(245.4166F, 22.99998F);
            this.xrLabel8.StylePriority.UseFont = false;
            this.xrLabel8.Text = "Classroom Name";
            // 
            // xrLabel7
            // 
            this.xrLabel7.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[ClassroomName]")});
            this.xrLabel7.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel7.LocationFloat = new DevExpress.Utils.PointFloat(0F, 100.4167F);
            this.xrLabel7.Multiline = true;
            this.xrLabel7.Name = "xrLabel7";
            this.xrLabel7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel7.SizeF = new System.Drawing.SizeF(245.4166F, 23F);
            this.xrLabel7.StylePriority.UseFont = false;
            // 
            // xrLabel2
            // 
            this.xrLabel2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(245.4166F, 77.41669F);
            this.xrLabel2.Multiline = true;
            this.xrLabel2.Name = "xrLabel2";
            this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel2.SizeF = new System.Drawing.SizeF(245.4166F, 22.99998F);
            this.xrLabel2.StylePriority.UseFont = false;
            this.xrLabel2.Text = "ID Number";
            // 
            // xrLabel3
            // 
            this.xrLabel3.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[ClassroomID]")});
            this.xrLabel3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel3.LocationFloat = new DevExpress.Utils.PointFloat(245.4166F, 100.4167F);
            this.xrLabel3.Multiline = true;
            this.xrLabel3.Name = "xrLabel3";
            this.xrLabel3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel3.SizeF = new System.Drawing.SizeF(245.4166F, 23F);
            this.xrLabel3.StylePriority.UseFont = false;
            // 
            // xrLabel4
            // 
            this.xrLabel4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(490.8332F, 77.4167F);
            this.xrLabel4.Multiline = true;
            this.xrLabel4.Name = "xrLabel4";
            this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel4.SizeF = new System.Drawing.SizeF(245.4166F, 22.99998F);
            this.xrLabel4.StylePriority.UseFont = false;
            this.xrLabel4.Text = "Location";
            // 
            // xrLabel5
            // 
            this.xrLabel5.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Location]")});
            this.xrLabel5.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel5.LocationFloat = new DevExpress.Utils.PointFloat(490.8332F, 100.4167F);
            this.xrLabel5.Multiline = true;
            this.xrLabel5.Name = "xrLabel5";
            this.xrLabel5.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel5.SizeF = new System.Drawing.SizeF(245.4166F, 23F);
            this.xrLabel5.StylePriority.UseFont = false;
            // 
            // xrLabel6
            // 
            this.xrLabel6.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(0F, 140.4167F);
            this.xrLabel6.Multiline = true;
            this.xrLabel6.Name = "xrLabel6";
            this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel6.SizeF = new System.Drawing.SizeF(245.4166F, 22.99998F);
            this.xrLabel6.StylePriority.UseFont = false;
            this.xrLabel6.Text = "Infant/Toddler Classroom?";
            // 
            // xrLabel10
            // 
            this.xrLabel10.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Iif([IsInfantToddler] == True, \'Yes\', \'No\')")});
            this.xrLabel10.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel10.LocationFloat = new DevExpress.Utils.PointFloat(0F, 163.4167F);
            this.xrLabel10.Multiline = true;
            this.xrLabel10.Name = "xrLabel10";
            this.xrLabel10.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel10.SizeF = new System.Drawing.SizeF(245.4166F, 23F);
            this.xrLabel10.StylePriority.UseFont = false;
            // 
            // xrLabel11
            // 
            this.xrLabel11.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel11.LocationFloat = new DevExpress.Utils.PointFloat(245.4166F, 140.4167F);
            this.xrLabel11.Multiline = true;
            this.xrLabel11.Name = "xrLabel11";
            this.xrLabel11.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel11.SizeF = new System.Drawing.SizeF(245.4166F, 22.99998F);
            this.xrLabel11.StylePriority.UseFont = false;
            this.xrLabel11.Text = "Preschool Classroom?";
            // 
            // xrLabel12
            // 
            this.xrLabel12.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Iif([IsPreschool] == True, \'Yes\', \'No\')")});
            this.xrLabel12.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel12.LocationFloat = new DevExpress.Utils.PointFloat(245.4166F, 163.4167F);
            this.xrLabel12.Multiline = true;
            this.xrLabel12.Name = "xrLabel12";
            this.xrLabel12.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel12.SizeF = new System.Drawing.SizeF(245.4166F, 23F);
            this.xrLabel12.StylePriority.UseFont = false;
            // 
            // xrLabel13
            // 
            this.xrLabel13.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.xrLabel13.LocationFloat = new DevExpress.Utils.PointFloat(490.8332F, 140.4167F);
            this.xrLabel13.Multiline = true;
            this.xrLabel13.Name = "xrLabel13";
            this.xrLabel13.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel13.SizeF = new System.Drawing.SizeF(245.4166F, 22.99998F);
            this.xrLabel13.StylePriority.UseFont = false;
            this.xrLabel13.Text = "Currently Served by Substitute?";
            // 
            // xrLabel14
            // 
            this.xrLabel14.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Iif([BeingServedSubstitute] == True, \'Yes\', \'No\')")});
            this.xrLabel14.Font = new System.Drawing.Font("Arial", 9.75F);
            this.xrLabel14.LocationFloat = new DevExpress.Utils.PointFloat(490.8332F, 163.4167F);
            this.xrLabel14.Multiline = true;
            this.xrLabel14.Name = "xrLabel14";
            this.xrLabel14.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel14.SizeF = new System.Drawing.SizeF(245.4166F, 23F);
            this.xrLabel14.StylePriority.UseFont = false;
            // 
            // sqlDataSource1
            // 
            this.sqlDataSource1.ConnectionName = "Pyramid";
            this.sqlDataSource1.Name = "sqlDataSource1";
            storedProcQuery1.Name = "rspClassroom_BasicInfo";
            queryParameter1.Name = "@ClassroomPK";
            queryParameter1.Type = typeof(DevExpress.DataAccess.Expression);
            queryParameter1.Value = new DevExpress.DataAccess.Expression("?ParamFormPK", typeof(int));
            storedProcQuery1.Parameters.Add(queryParameter1);
            storedProcQuery1.StoredProcName = "rspClassroom_BasicInfo";
            this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
            this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
            // 
            // subRptChildClassroomAssignments
            // 
            this.subRptChildClassroomAssignments.LocationFloat = new DevExpress.Utils.PointFloat(0F, 204.25F);
            this.subRptChildClassroomAssignments.Name = "subRptChildClassroomAssignments";
            this.subRptChildClassroomAssignments.ParameterBindings.Add(new DevExpress.XtraReports.UI.ParameterBinding("ParamFormPK", this.ParamFormPK));
            this.subRptChildClassroomAssignments.ParameterBindings.Add(new DevExpress.XtraReports.UI.ParameterBinding("ParamViewPrivateChildInfo", this.ParamViewPrivateChildInfo));
            this.subRptChildClassroomAssignments.ReportSource = new Pyramid.Reports.PreBuiltReports.FormReports.RptClassroom_ChildClassroomAssignments();
            this.subRptChildClassroomAssignments.SizeF = new System.Drawing.SizeF(750F, 23F);
            // 
            // subRptEmployeeClassroomAssignments
            // 
            this.subRptEmployeeClassroomAssignments.LocationFloat = new DevExpress.Utils.PointFloat(0F, 240.25F);
            this.subRptEmployeeClassroomAssignments.Name = "subRptEmployeeClassroomAssignments";
            this.subRptEmployeeClassroomAssignments.ParameterBindings.Add(new DevExpress.XtraReports.UI.ParameterBinding("ParamFormPK", this.ParamFormPK));
            this.subRptEmployeeClassroomAssignments.ParameterBindings.Add(new DevExpress.XtraReports.UI.ParameterBinding("ParamViewPrivateEmployeeInfo", this.ParamViewPrivateEmployeeInfo));
            this.subRptEmployeeClassroomAssignments.ReportSource = new Pyramid.Reports.PreBuiltReports.FormReports.RptClassroom_EmployeeClassroomAssignments();
            this.subRptEmployeeClassroomAssignments.SizeF = new System.Drawing.SizeF(750F, 23F);
            // 
            // RptClassroom
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.PageFooter,
            this.PageHeader,
            this.MasterReportFooter});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
            this.DataMember = "rspClassroom_BasicInfo";
            this.DataSource = this.sqlDataSource1;
            this.DisplayName = "Classroom Information";
            this.ExportOptions.Xls.SheetName = "Report";
            this.ExportOptions.Xls.ShowGridLines = true;
            this.ExportOptions.Xlsx.DocumentOptions.Category = "Pyramid Implementation Data System";
            this.ExportOptions.Xlsx.SheetName = "Report";
            this.ExportOptions.Xlsx.ShowGridLines = true;
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
            this.ParamViewPrivateEmployeeInfo,
            this.ParamFormPK});
            this.Version = "20.1";
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }


        #endregion

        private DevExpress.XtraReports.UI.XRLabel xrLabel9;
        private DevExpress.XtraReports.UI.XRLabel xrLabel1;
        private DevExpress.XtraReports.UI.XRLine xrLine1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel17;
        private DevExpress.XtraReports.UI.XRLabel xrLabel13;
        private DevExpress.XtraReports.UI.XRLabel xrLabel14;
        private DevExpress.XtraReports.UI.XRLabel xrLabel11;
        private DevExpress.XtraReports.UI.XRLabel xrLabel12;
        private DevExpress.XtraReports.UI.XRLabel xrLabel6;
        private DevExpress.XtraReports.UI.XRLabel xrLabel10;
        private DevExpress.XtraReports.UI.XRLabel xrLabel4;
        private DevExpress.XtraReports.UI.XRLabel xrLabel5;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel3;
        private DevExpress.XtraReports.UI.XRLabel xrLabel8;
        private DevExpress.XtraReports.UI.XRLabel xrLabel7;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.XtraReports.UI.XRSubreport subRptChildClassroomAssignments;
        private DevExpress.XtraReports.UI.XRSubreport subRptEmployeeClassroomAssignments;
    }
}
