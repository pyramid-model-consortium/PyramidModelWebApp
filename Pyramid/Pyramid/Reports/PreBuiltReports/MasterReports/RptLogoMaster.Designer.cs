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
            this.imgLogo = new DevExpress.XtraReports.UI.XRPictureBox();
            this.lblAppTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.lblReportTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.lblStateName = new DevExpress.XtraReports.UI.XRLabel();
            this.lblStateCatchphrase = new DevExpress.XtraReports.UI.XRLabel();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.masterPageInfoDate = new DevExpress.XtraReports.UI.XRPageInfo();
            this.masterPageInfoNum = new DevExpress.XtraReports.UI.XRPageInfo();
            this.lblGenerated = new DevExpress.XtraReports.UI.XRLabel();
            this.footerLine = new DevExpress.XtraReports.UI.XRLine();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.PageFooter = new DevExpress.XtraReports.UI.PageFooterBand();
            this.lblCriteriaValues = new DevExpress.XtraReports.UI.XRLabel();
            this.lblCriteria = new DevExpress.XtraReports.UI.XRLabel();
            this.PageHeader = new DevExpress.XtraReports.UI.PageHeaderBand();
            this.ParamLogoPath = new DevExpress.XtraReports.Parameters.Parameter();
            this.MasterReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
            this.MasterFooterLine = new DevExpress.XtraReports.UI.XRLine();
            this.ParamViewPrivateChildInfo = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamProgramFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamStateFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamStartDate = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamEndDate = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamPointInTime = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamYear = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamBIRProfileGroup = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamBIRItem = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamClassroomFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamChildFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamRaceFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamEthnicityFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamGenderFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamIEP = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamDLL = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamEmployeeFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamTeacherFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamCoachFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamProblemBehaviorFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamActivityFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamOthersInvolvedFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamPossibleMotivationFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamStrategyResponseFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamAdminFollowUpFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamViewPrivateEmployeeInfo = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamEmployeeRole = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamReportFocus = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamHubFKs = new DevExpress.XtraReports.Parameters.Parameter();
            this.ParamCohortFKs = new DevExpress.XtraReports.Parameters.Parameter();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.imgLogo,
            this.lblAppTitle,
            this.lblReportTitle,
            this.lblStateName,
            this.lblStateCatchphrase});
            this.TopMargin.HeightF = 199.125F;
            this.TopMargin.Name = "TopMargin";
            // 
            // imgLogo
            // 
            this.imgLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.imgLogo.ImageUrl = "Content\\images\\CustomPIDSLogoSquare.png";
            this.imgLogo.LocationFloat = new DevExpress.Utils.PointFloat(0F, 50.54169F);
            this.imgLogo.Name = "imgLogo";
            this.imgLogo.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 2, 2, 100F);
            this.imgLogo.SizeF = new System.Drawing.SizeF(170F, 100F);
            this.imgLogo.Sizing = DevExpress.XtraPrinting.ImageSizeMode.Squeeze;
            this.imgLogo.StylePriority.UseBackColor = false;
            this.imgLogo.StylePriority.UsePadding = false;
            this.imgLogo.BeforePrint += new DevExpress.XtraReports.UI.BeforePrintEventHandler(this.imgLogo_BeforePrint);
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblAppTitle.Font = new DevExpress.Drawing.DXFont("Arial", 14F, DevExpress.Drawing.DXFontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.LocationFloat = new DevExpress.Utils.PointFloat(170F, 83.54169F);
            this.lblAppTitle.Multiline = true;
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 2, 0, 0, 100F);
            this.lblAppTitle.SizeF = new System.Drawing.SizeF(580F, 34F);
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
            this.lblReportTitle.Font = new DevExpress.Drawing.DXFont("Arial", 14F);
            this.lblReportTitle.ForeColor = System.Drawing.Color.White;
            this.lblReportTitle.LocationFloat = new DevExpress.Utils.PointFloat(0F, 150.5417F);
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
            // lblStateName
            // 
            this.lblStateName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStateName.Font = new DevExpress.Drawing.DXFont("Arial", 14F, DevExpress.Drawing.DXFontStyle.Bold);
            this.lblStateName.ForeColor = System.Drawing.Color.White;
            this.lblStateName.LocationFloat = new DevExpress.Utils.PointFloat(170F, 50.54169F);
            this.lblStateName.Multiline = true;
            this.lblStateName.Name = "lblStateName";
            this.lblStateName.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 2, 0, 0, 100F);
            this.lblStateName.SizeF = new System.Drawing.SizeF(580F, 33F);
            this.lblStateName.StylePriority.UseBackColor = false;
            this.lblStateName.StylePriority.UseFont = false;
            this.lblStateName.StylePriority.UseForeColor = false;
            this.lblStateName.StylePriority.UsePadding = false;
            this.lblStateName.StylePriority.UseTextAlignment = false;
            this.lblStateName.Text = "State Name";
            this.lblStateName.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // lblStateCatchphrase
            // 
            this.lblStateCatchphrase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStateCatchphrase.Font = new DevExpress.Drawing.DXFont("Arial", 10F, ((DevExpress.Drawing.DXFontStyle)((DevExpress.Drawing.DXFontStyle.Bold | DevExpress.Drawing.DXFontStyle.Italic))));
            this.lblStateCatchphrase.ForeColor = System.Drawing.Color.White;
            this.lblStateCatchphrase.LocationFloat = new DevExpress.Utils.PointFloat(170F, 117.5417F);
            this.lblStateCatchphrase.Multiline = true;
            this.lblStateCatchphrase.Name = "lblStateCatchphrase";
            this.lblStateCatchphrase.Padding = new DevExpress.XtraPrinting.PaddingInfo(20, 2, 0, 0, 100F);
            this.lblStateCatchphrase.SizeF = new System.Drawing.SizeF(580F, 33F);
            this.lblStateCatchphrase.StylePriority.UseBackColor = false;
            this.lblStateCatchphrase.StylePriority.UseFont = false;
            this.lblStateCatchphrase.StylePriority.UseForeColor = false;
            this.lblStateCatchphrase.StylePriority.UsePadding = false;
            this.lblStateCatchphrase.StylePriority.UseTextAlignment = false;
            this.lblStateCatchphrase.Text = "State Catchphrase";
            this.lblStateCatchphrase.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // BottomMargin
            // 
            this.BottomMargin.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.masterPageInfoDate,
            this.masterPageInfoNum,
            this.lblGenerated,
            this.footerLine});
            this.BottomMargin.HeightF = 120F;
            this.BottomMargin.Name = "BottomMargin";
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
            // masterPageInfoNum
            // 
            this.masterPageInfoNum.LocationFloat = new DevExpress.Utils.PointFloat(621.7917F, 22.99999F);
            this.masterPageInfoNum.Name = "masterPageInfoNum";
            this.masterPageInfoNum.SizeF = new System.Drawing.SizeF(118.2083F, 23F);
            this.masterPageInfoNum.StylePriority.UseTextAlignment = false;
            this.masterPageInfoNum.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.masterPageInfoNum.TextFormatString = "Page {0} of {1}";
            // 
            // lblGenerated
            // 
            this.lblGenerated.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F, DevExpress.Drawing.DXFontStyle.Bold);
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
            // footerLine
            // 
            this.footerLine.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.footerLine.Name = "footerLine";
            this.footerLine.SizeF = new System.Drawing.SizeF(750F, 23F);
            // 
            // Detail
            // 
            this.Detail.Name = "Detail";
            // 
            // PageFooter
            // 
            this.PageFooter.HeightF = 0F;
            this.PageFooter.Name = "PageFooter";
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
            this.lblCriteria.Font = new DevExpress.Drawing.DXFont("Arial", 11F, DevExpress.Drawing.DXFontStyle.Bold);
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
            // PageHeader
            // 
            this.PageHeader.HeightF = 0F;
            this.PageHeader.Name = "PageHeader";
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
            // ParamViewPrivateChildInfo
            // 
            this.ParamViewPrivateChildInfo.Description = "Show/hide private child information";
            this.ParamViewPrivateChildInfo.Name = "ParamViewPrivateChildInfo";
            this.ParamViewPrivateChildInfo.Type = typeof(bool);
            this.ParamViewPrivateChildInfo.ValueInfo = "False";
            this.ParamViewPrivateChildInfo.Visible = false;
            // 
            // ParamProgramFKs
            // 
            this.ParamProgramFKs.AllowNull = true;
            this.ParamProgramFKs.Description = "All the program FKs in a comma-separated list";
            this.ParamProgramFKs.Name = "ParamProgramFKs";
            this.ParamProgramFKs.Visible = false;
            // 
            // ParamStateFKs
            // 
            this.ParamStateFKs.AllowNull = true;
            this.ParamStateFKs.Description = "A comma-separated list of state FKs";
            this.ParamStateFKs.Name = "ParamStateFKs";
            this.ParamStateFKs.Visible = false;
            // 
            // ParamStartDate
            // 
            this.ParamStartDate.AllowNull = true;
            this.ParamStartDate.Description = "A start date";
            this.ParamStartDate.Name = "ParamStartDate";
            this.ParamStartDate.Type = typeof(System.DateTime);
            this.ParamStartDate.Visible = false;
            // 
            // ParamEndDate
            // 
            this.ParamEndDate.AllowNull = true;
            this.ParamEndDate.Description = "A end date";
            this.ParamEndDate.Name = "ParamEndDate";
            this.ParamEndDate.Type = typeof(System.DateTime);
            this.ParamEndDate.Visible = false;
            // 
            // ParamPointInTime
            // 
            this.ParamPointInTime.AllowNull = true;
            this.ParamPointInTime.Description = "A point in time date";
            this.ParamPointInTime.Name = "ParamPointInTime";
            this.ParamPointInTime.Type = typeof(System.DateTime);
            this.ParamPointInTime.Visible = false;
            // 
            // ParamYear
            // 
            this.ParamYear.AllowNull = true;
            this.ParamYear.Description = "A year";
            this.ParamYear.Name = "ParamYear";
            this.ParamYear.Type = typeof(int);
            this.ParamYear.ValueInfo = "0";
            this.ParamYear.Visible = false;
            // 
            // ParamBIRProfileGroup
            // 
            this.ParamBIRProfileGroup.AllowNull = true;
            this.ParamBIRProfileGroup.Description = "The BIR Profiles report grouping";
            this.ParamBIRProfileGroup.Name = "ParamBIRProfileGroup";
            this.ParamBIRProfileGroup.Visible = false;
            // 
            // ParamBIRItem
            // 
            this.ParamBIRItem.AllowNull = true;
            this.ParamBIRItem.Description = "The BIR Profile report item";
            this.ParamBIRItem.Name = "ParamBIRItem";
            this.ParamBIRItem.Visible = false;
            // 
            // ParamClassroomFKs
            // 
            this.ParamClassroomFKs.AllowNull = true;
            this.ParamClassroomFKs.Description = "A comma-separated list of classroom FKs";
            this.ParamClassroomFKs.Name = "ParamClassroomFKs";
            this.ParamClassroomFKs.Visible = false;
            // 
            // ParamChildFKs
            // 
            this.ParamChildFKs.AllowNull = true;
            this.ParamChildFKs.Description = "A comma-separated list of child FKs";
            this.ParamChildFKs.Name = "ParamChildFKs";
            this.ParamChildFKs.Visible = false;
            // 
            // ParamRaceFKs
            // 
            this.ParamRaceFKs.AllowNull = true;
            this.ParamRaceFKs.Description = "A comma-separated list of race FKs";
            this.ParamRaceFKs.Name = "ParamRaceFKs";
            this.ParamRaceFKs.Visible = false;
            // 
            // ParamEthnicityFKs
            // 
            this.ParamEthnicityFKs.AllowNull = true;
            this.ParamEthnicityFKs.Description = "A comma-separated list of ethnicity FKs";
            this.ParamEthnicityFKs.Name = "ParamEthnicityFKs";
            this.ParamEthnicityFKs.Visible = false;
            // 
            // ParamGenderFKs
            // 
            this.ParamGenderFKs.AllowNull = true;
            this.ParamGenderFKs.Description = "A comma-separated list of gender FKs";
            this.ParamGenderFKs.Name = "ParamGenderFKs";
            this.ParamGenderFKs.Visible = false;
            // 
            // ParamIEP
            // 
            this.ParamIEP.AllowNull = true;
            this.ParamIEP.Description = "A boolean for the IEP status";
            this.ParamIEP.Name = "ParamIEP";
            this.ParamIEP.Type = typeof(bool);
            this.ParamIEP.Visible = false;
            // 
            // ParamDLL
            // 
            this.ParamDLL.AllowNull = true;
            this.ParamDLL.Description = "A boolean for DLL status";
            this.ParamDLL.Name = "ParamDLL";
            this.ParamDLL.Type = typeof(bool);
            this.ParamDLL.Visible = false;
            // 
            // ParamEmployeeFKs
            // 
            this.ParamEmployeeFKs.AllowNull = true;
            this.ParamEmployeeFKs.Description = "A comma-separated list of professional FKs";
            this.ParamEmployeeFKs.Name = "ParamEmployeeFKs";
            this.ParamEmployeeFKs.Visible = false;
            // 
            // ParamTeacherFKs
            // 
            this.ParamTeacherFKs.AllowNull = true;
            this.ParamTeacherFKs.Description = "A comma-separated list of teacher FKs";
            this.ParamTeacherFKs.Name = "ParamTeacherFKs";
            this.ParamTeacherFKs.Visible = false;
            // 
            // ParamCoachFKs
            // 
            this.ParamCoachFKs.AllowNull = true;
            this.ParamCoachFKs.Description = "A comma-separated list of coach FKs";
            this.ParamCoachFKs.Name = "ParamCoachFKs";
            this.ParamCoachFKs.Visible = false;
            // 
            // ParamProblemBehaviorFKs
            // 
            this.ParamProblemBehaviorFKs.AllowNull = true;
            this.ParamProblemBehaviorFKs.Description = "A comma-separed list of problem behavior FKs";
            this.ParamProblemBehaviorFKs.Name = "ParamProblemBehaviorFKs";
            this.ParamProblemBehaviorFKs.Visible = false;
            // 
            // ParamActivityFKs
            // 
            this.ParamActivityFKs.AllowNull = true;
            this.ParamActivityFKs.Description = "A comma-separated list of activity FKs";
            this.ParamActivityFKs.Name = "ParamActivityFKs";
            this.ParamActivityFKs.Visible = false;
            // 
            // ParamOthersInvolvedFKs
            // 
            this.ParamOthersInvolvedFKs.AllowNull = true;
            this.ParamOthersInvolvedFKs.Description = "A comma-separated list of others involved FKs";
            this.ParamOthersInvolvedFKs.Name = "ParamOthersInvolvedFKs";
            this.ParamOthersInvolvedFKs.Visible = false;
            // 
            // ParamPossibleMotivationFKs
            // 
            this.ParamPossibleMotivationFKs.AllowNull = true;
            this.ParamPossibleMotivationFKs.Description = "A comma-separated list of possible motivation FKs";
            this.ParamPossibleMotivationFKs.Name = "ParamPossibleMotivationFKs";
            this.ParamPossibleMotivationFKs.Visible = false;
            // 
            // ParamStrategyResponseFKs
            // 
            this.ParamStrategyResponseFKs.AllowNull = true;
            this.ParamStrategyResponseFKs.Description = "A comma-separated list of strategy response FKs";
            this.ParamStrategyResponseFKs.Name = "ParamStrategyResponseFKs";
            this.ParamStrategyResponseFKs.Visible = false;
            // 
            // ParamAdminFollowUpFKs
            // 
            this.ParamAdminFollowUpFKs.AllowNull = true;
            this.ParamAdminFollowUpFKs.Description = "A comma-separated list of admin follow up FKs";
            this.ParamAdminFollowUpFKs.Name = "ParamAdminFollowUpFKs";
            this.ParamAdminFollowUpFKs.Visible = false;
            // 
            // ParamViewPrivateEmployeeInfo
            // 
            this.ParamViewPrivateEmployeeInfo.Description = "Show/hide private professional information";
            this.ParamViewPrivateEmployeeInfo.Name = "ParamViewPrivateEmployeeInfo";
            this.ParamViewPrivateEmployeeInfo.Type = typeof(bool);
            this.ParamViewPrivateEmployeeInfo.ValueInfo = "False";
            this.ParamViewPrivateEmployeeInfo.Visible = false;
            // 
            // ParamEmployeeRole
            // 
            this.ParamEmployeeRole.AllowNull = true;
            this.ParamEmployeeRole.Description = "Professional roles";
            this.ParamEmployeeRole.Name = "ParamEmployeeRole";
            this.ParamEmployeeRole.Visible = false;
            // 
            // ParamReportFocus
            // 
            this.ParamReportFocus.AllowNull = true;
            this.ParamReportFocus.Description = "The focus of the report analysis";
            this.ParamReportFocus.Name = "ParamReportFocus";
            this.ParamReportFocus.Visible = false;
            // 
            // ParamHubFKs
            // 
            this.ParamHubFKs.AllowNull = true;
            this.ParamHubFKs.Description = "All the relevant Hub FKs in a comma separated list";
            this.ParamHubFKs.Name = "ParamHubFKs";
            this.ParamHubFKs.Visible = false;
            // 
            // ParamCohortFKs
            // 
            this.ParamCohortFKs.AllowNull = true;
            this.ParamCohortFKs.Description = "All the relevant Cohort FKs in a comma separated list";
            this.ParamCohortFKs.Name = "ParamCohortFKs";
            this.ParamCohortFKs.Visible = false;
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
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Margins = new DevExpress.Drawing.DXMargins(50, 50, 199, 120);
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.ParamLogoPath,
            this.ParamHubFKs,
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
            this.ParamEmployeeRole,
            this.ParamReportFocus,
            this.ParamCohortFKs});
            this.Version = "21.1";
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion
        protected DevExpress.XtraReports.UI.PageHeaderBand PageHeader;
        public DevExpress.XtraReports.UI.DetailBand Detail;
        public DevExpress.XtraReports.UI.XRLabel lblCriteriaValues;
        public DevExpress.XtraReports.UI.PageFooterBand PageFooter;
        public DevExpress.XtraReports.UI.XRLabel lblCriteria;
        public DevExpress.XtraReports.UI.ReportFooterBand MasterReportFooter;
        public DevExpress.XtraReports.UI.XRLine MasterFooterLine;
        public DevExpress.XtraReports.Parameters.Parameter ParamViewPrivateChildInfo;
        public DevExpress.XtraReports.Parameters.Parameter ParamLogoPath;
        public DevExpress.XtraReports.UI.XRPictureBox imgLogo;
        public DevExpress.XtraReports.UI.XRLabel lblAppTitle;
        public DevExpress.XtraReports.UI.XRLabel lblReportTitle;
        public DevExpress.XtraReports.UI.XRLabel lblStateName;
        public DevExpress.XtraReports.UI.XRLabel lblStateCatchphrase;
        public DevExpress.XtraReports.UI.XRPageInfo masterPageInfoDate;
        public DevExpress.XtraReports.UI.XRPageInfo masterPageInfoNum;
        public DevExpress.XtraReports.UI.XRLabel lblGenerated;
        public DevExpress.XtraReports.UI.XRLine footerLine;
        public DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        public DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        public DevExpress.XtraReports.Parameters.Parameter ParamProgramFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamStateFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamStartDate;
        public DevExpress.XtraReports.Parameters.Parameter ParamEndDate;
        public DevExpress.XtraReports.Parameters.Parameter ParamPointInTime;
        public DevExpress.XtraReports.Parameters.Parameter ParamYear;
        public DevExpress.XtraReports.Parameters.Parameter ParamBIRProfileGroup;
        public DevExpress.XtraReports.Parameters.Parameter ParamBIRItem;
        public DevExpress.XtraReports.Parameters.Parameter ParamClassroomFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamChildFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamRaceFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamEthnicityFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamGenderFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamIEP;
        public DevExpress.XtraReports.Parameters.Parameter ParamDLL;
        public DevExpress.XtraReports.Parameters.Parameter ParamEmployeeFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamTeacherFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamCoachFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamProblemBehaviorFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamActivityFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamOthersInvolvedFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamPossibleMotivationFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamStrategyResponseFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamAdminFollowUpFKs;
        public DevExpress.XtraReports.Parameters.Parameter ParamViewPrivateEmployeeInfo;
        private DevExpress.XtraReports.Parameters.Parameter ParamEmployeeRole;
        private DevExpress.XtraReports.Parameters.Parameter ParamReportFocus;
        private DevExpress.XtraReports.Parameters.Parameter ParamHubFKs;
        private DevExpress.XtraReports.Parameters.Parameter ParamCohortFKs;
    }
}
