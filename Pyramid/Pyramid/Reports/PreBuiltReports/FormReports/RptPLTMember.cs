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
    public partial class RptPLTMember : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptPLTMember()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptPLTMember_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.PLTMember currentPLTMember;
            List<Models.PLTMemberRole> currentRoleAssignments;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the PLT Member object
                currentPLTMember = context.PLTMember
                                            .Include(cm => cm.Program)
                                            .AsNoTracking()
                                            .Where(cm => cm.PLTMemberPK == formPK)
                                            .FirstOrDefault();

                //Get the role assignments
                currentRoleAssignments = context.PLTMemberRole
                                                    .Include(pmr => pmr.CodeTeamPosition)
                                                    .AsNoTracking()
                                                    .Where(pmr => pmr.PLTMemberFK == formPK)
                                                    .ToList();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblBIProgram.Text = currentPLTMember.Program.ProgramName;
            lblBIFirstName.Text = currentPLTMember.FirstName;
            lblBILastName.Text = currentPLTMember.LastName;
            lblBIIDNumber.Text = currentPLTMember.IDNumber;
            lblBIEmail.Text = currentPLTMember.EmailAddress;
            lblBIPhoneNumber.Text = (string.IsNullOrWhiteSpace(currentPLTMember.PhoneNumber) ? "" : Code.Utilities.FormatPhoneNumber(currentPLTMember.PhoneNumber, "US"));
            lblBIStartDate.Text = currentPLTMember.StartDate.ToString("MM/dd/yyyy");
            lblBILeaveDate.Text = (currentPLTMember.LeaveDate.HasValue ? currentPLTMember.LeaveDate.Value.ToString("MM/dd/yyyy") : "");

            //Handles the roles
            if (currentRoleAssignments != null && currentRoleAssignments.Count > 0) 
            {
                lblBIRoles.Text = string.Join(", ", currentRoleAssignments.Select(ra => ra.CodeTeamPosition.Description).ToList());
            }
        }
    }
}
