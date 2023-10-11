using System;
using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using DevExpress.DataProcessing;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptSLTMember : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptSLTMember()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptSLTMember_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.SLTMember currentSLTMember;
            List<Models.SLTMemberAgencyAssignment> currentAgencyAssignments;
            List<Models.SLTMemberWorkGroupAssignment> currentWorkGroupAssignments;
            List<Models.SLTMemberRole> currentRoleAssignments;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the SLT Member object
                currentSLTMember = context.SLTMember
                                            .Include(sm => sm.State)
                                            .Include(sm => sm.CodeGender)
                                            .Include(sm => sm.CodeEthnicity)
                                            .Include(sm => sm.CodeHouseholdIncome)
                                            .Include(sm => sm.CodeRace)
                                            .AsNoTracking()
                                            .Where(sm => sm.SLTMemberPK == formPK)
                                            .FirstOrDefault();

                //Get the agency assignments
                currentAgencyAssignments = context.SLTMemberAgencyAssignment
                                                    .Include(aa => aa.SLTAgency)
                                                    .AsNoTracking()
                                                    .Where(aa => aa.SLTMemberFK == formPK)
                                                    .ToList();

                //Get the work group assignments
                currentWorkGroupAssignments = context.SLTMemberWorkGroupAssignment
                                                    .Include(wga => wga.SLTWorkGroup)
                                                    .AsNoTracking()
                                                    .Where(wga => wga.SLTMemberFK == formPK)
                                                    .ToList();

                //Get the role assignments
                currentRoleAssignments = context.SLTMemberRole
                                                    .Include(pmr => pmr.CodeTeamPosition)
                                                    .AsNoTracking()
                                                    .Where(pmr => pmr.SLTMemberFK == formPK)
                                                    .ToList();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblBIStateName.Text = currentSLTMember.State.Name;
            lblBIFirstName.Text = currentSLTMember.FirstName;
            lblBILastName.Text = currentSLTMember.LastName;
            lblBIIDNumber.Text = currentSLTMember.IDNumber;
            lblBIEmail.Text = currentSLTMember.EmailAddress;
            lblBIPhoneNumber.Text = (string.IsNullOrWhiteSpace(currentSLTMember.PhoneNumber) ? "" : Code.Utilities.FormatPhoneNumber(currentSLTMember.PhoneNumber, "US"));
            lblBIStartDate.Text = currentSLTMember.StartDate.ToString("MM/dd/yyyy");
            lblBILeaveDate.Text = (currentSLTMember.LeaveDate.HasValue ? currentSLTMember.LeaveDate.Value.ToString("MM/dd/yyyy") : "");
            lblBIGender.Text = (currentSLTMember.CodeGender == null ? "" : currentSLTMember.CodeGender.Description);
            lblBIGenderSpecify.Text = currentSLTMember.GenderSpecify;
            lblBIEthnicity.Text = (currentSLTMember.CodeEthnicity == null ? "" : currentSLTMember.CodeEthnicity.Description);
            lblBIRace.Text = (currentSLTMember.CodeRace == null ? "" : currentSLTMember.CodeRace.Description);
            lblBIHouseholdIncome.Text = (currentSLTMember.CodeHouseholdIncome == null ? "" : currentSLTMember.CodeHouseholdIncome.Description);

            //Handles the roles
            if (currentRoleAssignments != null && currentRoleAssignments.Count > 0)
            {
                lblBIRoles.Text = string.Join(", ", currentRoleAssignments.Select(ra => ra.CodeTeamPosition.Description).ToList());
            }

            //Hide the demographic information until told otherwise
            foreach (XRTableRow row in tblDemographicInfo.Rows)
            {
                row.CanShrink = true;
                row.Visible = false;
            }

            //------ Agency Assignments -------
            //Set the data source and add a sort field
            this.AgencyAssignmentsDetailReport.DataSource = currentAgencyAssignments;
            this.AgencyAssignmentsDetail.SortFields.Add(new GroupField("StartDate", XRColumnSortOrder.Descending));

            //Set the label expressions
            lblAAAgencyName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SLTAgency.Name"));
            lblAAStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));
            lblAAEndDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EndDate"));

            //------ Work Group Assignments -------
            //Set the data source and add a sort field
            this.WorkGroupAssignmentDetailReport.DataSource = currentWorkGroupAssignments;
            this.WorkGroupAssignmentDetail.SortFields.Add(new GroupField("StartDate", XRColumnSortOrder.Descending));

            //Set the label expressions
            lblWGAWorkGroupName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "SLTWorkGroup.WorkGroupName"));
            lblWGAStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));
            lblWGAEndDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EndDate"));
        }
    }
}
