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
    public partial class RptCWLTMember : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptCWLTMember()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptCWLTMember_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.CWLTMember currentCWLTMember;
            List<Models.CWLTMemberAgencyAssignment> currentAgencyAssignments;
            List<Models.CWLTMemberRole> currentRoleAssignments;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the CWLT Member object
                currentCWLTMember = context.CWLTMember
                                            .Include(cm => cm.Hub)
                                            .Include(cm => cm.CodeGender)
                                            .Include(cm => cm.CodeEthnicity)
                                            .Include(cm => cm.CodeHouseholdIncome)
                                            .Include(cm => cm.CodeRace)
                                            .AsNoTracking()
                                            .Where(cm => cm.CWLTMemberPK == formPK)
                                            .FirstOrDefault();

                //Get the agency assignments
                currentAgencyAssignments = context.CWLTMemberAgencyAssignment
                                                    .Include(aa => aa.CWLTAgency)
                                                    .AsNoTracking()
                                                    .Where(aa => aa.CWLTMemberFK == formPK)
                                                    .ToList();

                //Get the role assignments
                currentRoleAssignments = context.CWLTMemberRole
                                                    .Include(pmr => pmr.CodeTeamPosition)
                                                    .AsNoTracking()
                                                    .Where(pmr => pmr.CWLTMemberFK == formPK)
                                                    .ToList();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblBIHubName.Text = currentCWLTMember.Hub.Name;
            lblBIFirstName.Text = currentCWLTMember.FirstName;
            lblBILastName.Text = currentCWLTMember.LastName;
            lblBIIDNumber.Text = currentCWLTMember.IDNumber;
            lblBIEmail.Text = currentCWLTMember.EmailAddress;
            lblBIPhoneNumber.Text = (string.IsNullOrWhiteSpace(currentCWLTMember.PhoneNumber) ? "" : Code.Utilities.FormatPhoneNumber(currentCWLTMember.PhoneNumber, "US"));
            lblBIStartDate.Text = currentCWLTMember.StartDate.ToString("MM/dd/yyyy");
            lblBILeaveDate.Text = (currentCWLTMember.LeaveDate.HasValue ? currentCWLTMember.LeaveDate.Value.ToString("MM/dd/yyyy") : "");
            lblBIGender.Text = (currentCWLTMember.CodeGender == null ? "" : currentCWLTMember.CodeGender.Description);
            lblBIGenderSpecify.Text = currentCWLTMember.GenderSpecify;
            lblBIEthnicity.Text = (currentCWLTMember.CodeEthnicity == null ? "" : currentCWLTMember.CodeEthnicity.Description);
            lblBIRace.Text = (currentCWLTMember.CodeRace == null ? "" : currentCWLTMember.CodeRace.Description);
            lblBIHouseholdIncome.Text = (currentCWLTMember.CodeHouseholdIncome == null ? "" : currentCWLTMember.CodeHouseholdIncome.Description);

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
            lblAAAgencyName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "CWLTAgency.Name"));
            lblAAStartDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StartDate"));
            lblAAEndDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EndDate"));
        }
    }
}
