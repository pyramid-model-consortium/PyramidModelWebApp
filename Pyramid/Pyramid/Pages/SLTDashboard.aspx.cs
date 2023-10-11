using Pyramid.Models;
using System;
using System.Linq;
using Pyramid.MasterPages;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Pyramid.Code;
using DevExpress.Web;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Collections.Generic;
using Z.EntityFramework.Plus;
using System.Web.UI.HtmlControls;
using DevExpress.Web.Bootstrap;

namespace Pyramid.Pages
{
    public partial class SLTDashboard : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "SLTAP",
                    "BOQSLT",
                    "SLTM",
                    "SLTA",
                    "SLTWG"
                };
            }
        }

        private List<CodeProgramRolePermission> FormPermissions;
        private CodeProgramRolePermission currentSLTAPPermissions;
        private CodeProgramRolePermission currentBOQPermissions;
        private CodeProgramRolePermission currentMemberPermissions;
        private CodeProgramRolePermission currentAgencyPermissions;
        private CodeProgramRolePermission currentWorkGroupPermissions;
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission objects
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permissions
            currentSLTAPPermissions = FormPermissions.Where(p => p.CodeForm.FormAbbreviation == "SLTAP").FirstOrDefault();
            currentBOQPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "BOQSLT").FirstOrDefault();
            currentMemberPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "SLTM").FirstOrDefault();
            currentAgencyPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "SLTA").FirstOrDefault();
            currentWorkGroupPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "SLTWG").FirstOrDefault();

            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Set the permissions for the sections
            //Action Plans
            SetPermissions(currentSLTAPPermissions, divSLTActionPlan, bsGRSLTActionPlans, hfSLTAPViewOnly);

            //BOQs
            SetPermissions(currentBOQPermissions, divBOQ, bsGRBOQs, hfBOQViewOnly);

            //Members
            SetPermissions(currentMemberPermissions, divMembers, bsGRMembers, hfMemberViewOnly);

            //Agencies
            SetPermissions(currentAgencyPermissions, divAgencies, bsGRAgencies, hfAgencyViewOnly);

            //Work groups
            SetPermissions(currentWorkGroupPermissions, divWorkGroups, bsGRWorkGroups, hfWorkGroupViewOnly);

            if (!IsPostBack)
            {
                //Check for a message in the query string
                string messagetype = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messagetype))
                {
                    switch (messagetype)
                    {
                        case "BOQAdded":
                            msgSys.ShowMessageToUser("success", "Success", "State Leadership Team Benchmarks Of Quality form successfully added!", 1000);
                            break;
                        case "BOQEdited":
                            msgSys.ShowMessageToUser("success", "Success", "State Leadership Team Benchmarks Of Quality form successfully edited!", 1000);
                            break;
                        case "BOQCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoBOQ":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified State Leadership Team Benchmarks Of Quality form could not be found, please try again.", 15000);
                            break;
                        case "MemberAdded":
                            msgSys.ShowMessageToUser("success", "Success", "State Leadership Team Member successfully added!", 1000);
                            break;
                        case "MemberEdited":
                            msgSys.ShowMessageToUser("success", "Success", "State Leadership Team Member successfully edited!", 1000);
                            break;
                        case "MemberCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoMember":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified State Leadership Team Member could not be found, please try again.", 15000);
                            break;
                        case "AgencyAdded":
                            msgSys.ShowMessageToUser("success", "Success", "State Leadership Team Agency successfully added!", 1000);
                            break;
                        case "AgencyEdited":
                            msgSys.ShowMessageToUser("success", "Success", "State Leadership Team Agency successfully edited!", 1000);
                            break;
                        case "AgencyCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoAgency":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified State Leadership Team Agency could not be found, please try again.", 15000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// This method sets the visibility and usability of dashboard sections based on the passed permission object
        /// </summary>
        /// <param name="permissions">The permissions for the section</param>
        /// <param name="sectionDiv">The section div</param>
        /// <param name="gridview">The section gridview</param>
        /// <param name="viewOnlyHiddenField">The view only hidden field</param>
        private void SetPermissions(CodeProgramRolePermission permissions, HtmlGenericControl sectionDiv, BootstrapGridView gridview, HiddenField viewOnlyHiddenField)
        {
            //Check permissions
            if (permissions.AllowedToViewDashboard == false)
            {
                //Not allowed to see the section
                sectionDiv.Visible = false;
            }
            else if (permissions.AllowedToView == false)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (gridview.Columns.Count - 1);

                //Hide the action column
                gridview.Columns[actionColumnIndex].Visible = false;

                //Hide management options
                viewOnlyHiddenField.Value = "True";
            }
            else if (permissions.AllowedToAdd == false && permissions.AllowedToEdit == false)
            {
                viewOnlyHiddenField.Value = "True";
            }
            else
            {
                viewOnlyHiddenField.Value = "False";
            }
        }

        #region SLT Action Plan

        /// <summary>
        /// This method fires when the data source for the Action Plan DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efSLTActionPlanDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efSLTActionPlanDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "SLTActionPlanPK";

            //Get the DB context
            PyramidContext context = new PyramidContext();

            //Only allow users to see action plans from their authorized states
            e.QueryableSource = context.SLTActionPlan.AsNoTracking()
                                        .Include(ap => ap.State)
                                        .Include(ap => ap.SLTMember)
                                        .Include(ap => ap.SLTWorkGroup)
                                        .Where(ap => currentProgramRole.StateFKs.Contains(ap.StateFK));
        }

        /// <summary>
        /// This method fires when the Prefill New Action Plan button is clicked
        /// </summary>
        /// <param name="sender">The lbPrefillSLTAP LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbPrefillSLTAP_Click(object sender, EventArgs e)
        {
            if (currentSLTAPPermissions.AllowedToAdd)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the most recent action plan for the user's state
                    Models.SLTActionPlan actionPlanToCopy = context.SLTActionPlan
                                                                            .Include(ap => ap.SLTActionPlanActionStep)
                                                                            .Include(ap => ap.SLTActionPlanActionStep.Select(s => s.SLTActionPlanActionStepStatus))
                                                                            .Include(ap => ap.SLTActionPlanGroundRule)
                                                                            .AsNoTracking()
                                                                            .Where(ap => ap.StateFK == currentProgramRole.CurrentStateFK.Value)
                                                                            .OrderByDescending(ap => ap.ActionPlanStartDate)
                                                                            .FirstOrDefault();

                    if (actionPlanToCopy != null && actionPlanToCopy.SLTActionPlanPK > 0)
                    {
                        //Set the necessary values in the action plan copy
                        actionPlanToCopy.SLTActionPlanPK = 0;
                        actionPlanToCopy.CreateDate = DateTime.Now;
                        actionPlanToCopy.Creator = User.Identity.Name;
                        actionPlanToCopy.EditDate = null;
                        actionPlanToCopy.Editor = null;
                        actionPlanToCopy.ReviewedActionSteps = false;
                        actionPlanToCopy.ReviewedBasicInfo = false;
                        actionPlanToCopy.ReviewedGroundRules = false;
                        actionPlanToCopy.IsPrefilled = true;

                        //Update the action step audit fields
                        foreach (var actionStep in actionPlanToCopy.SLTActionPlanActionStep)
                        {
                            actionStep.CreateDate = DateTime.Now;
                            actionStep.Creator = User.Identity.Name;
                            actionStep.EditDate = null;
                            actionStep.Editor = null;

                            //Update the status record audit fields
                            foreach (var stepStatus in actionStep.SLTActionPlanActionStepStatus)
                            {
                                stepStatus.CreateDate = DateTime.Now;
                                stepStatus.Creator = User.Identity.Name;
                                stepStatus.EditDate = null;
                                stepStatus.Editor = null;
                            }
                        }

                        //Update the ground rule audit fields
                        foreach (var groundRule in actionPlanToCopy.SLTActionPlanGroundRule)
                        {
                            groundRule.CreateDate = DateTime.Now;
                            groundRule.Creator = User.Identity.Name;
                            groundRule.EditDate = null;
                            groundRule.Editor = null;
                        }

                        //Add the copy to the database (the linked objects in the .Include statements above will be
                        //automatically added with the correct FKs)
                        context.SLTActionPlan.Add(actionPlanToCopy);
                        context.SaveChanges();

                        //Show a message after redirect
                        msgSys.AddMessageToQueue("success", "Pre-filled Successfully", "Successfully pre-filled this Action Plan from the most recent Action Plan!  You can now review this Action Plan and make changes.", 5000);
                        Response.Redirect(string.Format("/Pages/SLTActionPlan.aspx?SLTActionPlanPK={0}&action={1}", actionPlanToCopy.SLTActionPlanPK, "Edit"));
                    }
                    else
                    {
                        //No action plan existed to be copied, show a message and redirect to add a blank action plan
                        msgSys.AddMessageToQueue("warning", "Pre-fill Failed", "Unable to pre-fill this Action Plan since there is not an existing Action Plan for your state!  You may enter an Action Plan from scratch or cancel.", 1000);
                        Response.Redirect("/Pages/SLTActionPlan.aspx?SLTActionPlanPK=0&action=Add");
                    }
                }
            }
            else
            {
                //Not allowed to add, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a Action Plan form
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteSLTActionPlan LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteSLTActionPlan_Click(object sender, EventArgs e)
        {
            if (currentSLTAPPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeSLTActionPlanPK = string.IsNullOrWhiteSpace(hfDeleteSLTActionPlanPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteSLTActionPlanPK.Value);

                if (removeSLTActionPlanPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the Action Plan to remove
                            Models.SLTActionPlan actionPlanToRemove = context.SLTActionPlan.Where(ap => ap.SLTActionPlanPK == removeSLTActionPlanPK).FirstOrDefault();

                            //Get the meeting rows to remove and remove them
                            var meetingsToRemove = context.SLTActionPlanMeeting.Where(m => m.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK).ToList();
                            context.SLTActionPlanMeeting.RemoveRange(meetingsToRemove);

                            //Get the ground rule rows to remove and remove them
                            var groundRulesToRemove = context.SLTActionPlanGroundRule.Where(gr => gr.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK).ToList();
                            context.SLTActionPlanGroundRule.RemoveRange(groundRulesToRemove);

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.SLTActionPlanActionStepStatus
                                .Include(ass => ass.SLTActionPlanActionStep)
                                .Where(ass => ass.SLTActionPlanActionStep.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK).ToList();
                            context.SLTActionPlanActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Get the action step rows to remove and remove them
                            var actionStepsToRemove = context.SLTActionPlanActionStep.Where(step => step.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK).ToList();
                            context.SLTActionPlanActionStep.RemoveRange(actionStepsToRemove);

                            //Remove the Action Plan
                            context.SLTActionPlan.Remove(actionPlanToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the meeting deletions
                            if (meetingsToRemove.Count > 0)
                            {
                                //Get the meeting change rows and set the deleter
                                context.SLTActionPlanMeetingChanged.Where(mc => mc.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK)
                                                                .OrderByDescending(mc => mc.SLTActionPlanMeetingChangedPK)
                                                                .Take(meetingsToRemove.Count).ToList()
                                                                .Select(mc => { mc.Deleter = User.Identity.Name; return mc; }).Count();
                            }

                            //Check the ground rule deletions
                            if (groundRulesToRemove.Count > 0)
                            {
                                //Get the ground rule change rows and set the deleter
                                context.SLTActionPlanGroundRuleChanged.Where(grc => grc.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK)
                                                                .OrderByDescending(grc => grc.SLTActionPlanGroundRuleChangedPK)
                                                                .Take(groundRulesToRemove.Count).ToList()
                                                                .Select(grc => { grc.Deleter = User.Identity.Name; return grc; }).Count();
                            }

                            //Check the action step deletions
                            if (actionStepsToRemove.Count > 0)
                            {
                                //Get the action step change rows and set the deleter
                                context.SLTActionPlanActionStepChanged.Where(asc => asc.SLTActionPlanFK == actionPlanToRemove.SLTActionPlanPK)
                                                                .OrderByDescending(asc => asc.SLTActionPlanActionStepChangedPK)
                                                                .Take(actionStepsToRemove.Count).ToList()
                                                                .Select(asc => { asc.Deleter = User.Identity.Name; return asc; }).Count();
                            }

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.SLTActionPlanActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.SLTActionPlanActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.SLTActionPlanActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.SLTActionPlanActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.SLTActionPlanChanged
                                    .OrderByDescending(apc => apc.SLTActionPlanChangedPK)
                                    .Where(apc => apc.SLTActionPlanPK == actionPlanToRemove.SLTActionPlanPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the State Leadership Team Action Plan!", 1000);

                            //Bind the gridview
                            bsGRSLTActionPlans.DataBind();
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the State Leadership Team Action Plan, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Action Plan!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Action Plan!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the State Leadership Team Action Plan to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region BOQs

        /// <summary>
        /// This method fires when the data source for the Benchmarks of Quality DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efBOQSLTDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efBOQDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "BenchmarkOfQualitySLTPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.BOQSLTView.AsNoTracking().Where(boq => currentProgramRole.StateFKs.Contains(boq.StateFK));
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a Benchmarks of Quality form
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteBOQ LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteBOQ_Click(object sender, EventArgs e)
        {
            if (currentBOQPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeBOQSLTPK = string.IsNullOrWhiteSpace(hfDeleteBOQPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteBOQPK.Value);

                if (removeBOQSLTPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the BOQSLT to remove
                            Models.BenchmarkOfQualitySLT BOQSLTToRemove = context.BenchmarkOfQualitySLT.Where(boq => boq.BenchmarkOfQualitySLTPK == removeBOQSLTPK).FirstOrDefault();

                            //Get the participant rows to remove (for later)
                            List<int> participantPKsToDelete = context.BOQSLTParticipant.AsNoTracking().Where(p => p.BenchmarksOfQualitySLTFK == BOQSLTToRemove.BenchmarkOfQualitySLTPK).Select(p => p.BOQSLTParticipantPK).ToList();

                            //Remove the participant rows
                            context.BOQSLTParticipant.Where(p => p.BenchmarksOfQualitySLTFK == BOQSLTToRemove.BenchmarkOfQualitySLTPK).Delete();

                            //Remove the BOQSLT
                            context.BenchmarkOfQualitySLT.Remove(BOQSLTToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the BOQ delete change row and set the deleter
                            context.BenchmarkOfQualitySLTChanged
                                    .OrderByDescending(boqc => boqc.BenchmarkOfQualitySLTChangedPK)
                                    .Where(boqc => boqc.BenchmarkOfQualitySLTPK == BOQSLTToRemove.BenchmarkOfQualitySLTPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Get the participant change rows and update the deleter
                            context.BOQSLTParticipantChanged.Where(pc => participantPKsToDelete.Contains(pc.BOQSLTParticipantPK)).Update(pc => new BOQSLTParticipantChanged() { Deleter = User.Identity.Name });

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the State Leadership Team Benchmarks of Quality form!", 1000);

                            //Bind the gridview
                            bsGRBOQs.DataBind();
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the State Leadership Team Benchmark of Quality form, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Benchmark of Quality form!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Benchmark of Quality form!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the State Leadership Team Benchmark of Quality form to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region Members

        /// <summary>
        /// This method executes when the user clicks the delete button for a SLT Member
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteSLTMember LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteMember_Click(object sender, EventArgs e)
        {
            if (currentMemberPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeMemberPK = string.IsNullOrWhiteSpace(hfDeleteMemberPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteMemberPK.Value);

                if (removeMemberPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the member to remove
                            Models.SLTMember memberToRemove = context.SLTMember.Where(sm => sm.SLTMemberPK == removeMemberPK).FirstOrDefault();

                            //Get the agency assignment rows to remove (for later)
                            List<int> agencyAssignmentPKsToDelete = context.SLTMemberAgencyAssignment.AsNoTracking().Where(s => s.SLTMemberFK == memberToRemove.SLTMemberPK).Select(s => s.SLTMemberAgencyAssignmentPK).ToList();

                            //Get the work group assignment rows to remove (for later)
                            List<int> workGroupAssignmentPKsToDelete = context.SLTMemberWorkGroupAssignment.AsNoTracking().Where(s => s.SLTMemberFK == memberToRemove.SLTMemberPK).Select(s => s.SLTMemberWorkGroupAssignmentPK).ToList();

                            //Get the role rows to remove (for later)
                            List<int> rolePKsToDelete = context.SLTMemberRole.AsNoTracking().Where(s => s.SLTMemberFK == memberToRemove.SLTMemberPK).Select(s => s.SLTMemberRolePK).ToList();

                            //Remove the role rows
                            context.SLTMemberRole.Where(c => c.SLTMemberFK == memberToRemove.SLTMemberPK).Delete();

                            //Remove the agency assignment rows
                            context.SLTMemberAgencyAssignment.Where(c => c.SLTMemberFK == memberToRemove.SLTMemberPK).Delete();

                            //Remove the work group assignment rows
                            context.SLTMemberWorkGroupAssignment.Where(c => c.SLTMemberFK == memberToRemove.SLTMemberPK).Delete();

                            //Remove the member
                            context.SLTMember.Remove(memberToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTMemberChanged
                                    .OrderByDescending(smc => smc.SLTMemberChangedPK)
                                    .Where(smc => smc.SLTMemberPK == memberToRemove.SLTMemberPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Get the agency assignment change rows and update the deleter
                            context.SLTMemberAgencyAssignmentChanged.Where(sc => agencyAssignmentPKsToDelete.Contains(sc.SLTMemberAgencyAssignmentPK)).Update(sc => new SLTMemberAgencyAssignmentChanged() { Deleter = User.Identity.Name });

                            //Get the work group assignment change rows and update the deleter
                            context.SLTMemberWorkGroupAssignmentChanged.Where(sc => workGroupAssignmentPKsToDelete.Contains(sc.SLTMemberWorkGroupAssignmentPK)).Update(sc => new SLTMemberWorkGroupAssignmentChanged() { Deleter = User.Identity.Name });

                            //Get the role change rows and update the deleter
                            context.SLTMemberRoleChanged.Where(sc => rolePKsToDelete.Contains(sc.SLTMemberRolePK)).Update(sc => new SLTMemberRoleChanged() { Deleter = User.Identity.Name });

                            //Save the delete change rows to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the State Leadership Team Member!", 1000);

                            //Bind the gridview
                            bsGRMembers.DataBind();
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the State Leadership Team Member, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Member!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Member!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the State Leadership Team Member to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the SLT Member DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efSLTMemberDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efMemberDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "SLTMemberPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.SLTMember.AsNoTracking()
                                        .Include(sm => sm.State)
                                        .Where(sm => currentProgramRole.StateFKs.Contains(sm.StateFK));
        }

        #endregion

        #region Agencies

        /// <summary>
        /// This method executes when the user clicks the delete button for an Agency
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteAgency LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteAgency_Click(object sender, EventArgs e)
        {
            if (currentAgencyPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeAgencyPK = string.IsNullOrWhiteSpace(hfDeleteAgencyPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteAgencyPK.Value);

                if (removeAgencyPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the agency to remove
                            Models.SLTAgency agencyToRemove = context.SLTAgency.Where(sm => sm.SLTAgencyPK == removeAgencyPK).FirstOrDefault();

                            //Remove the agency
                            context.SLTAgency.Remove(agencyToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTAgencyChanged
                                    .OrderByDescending(smc => smc.SLTAgencyChangedPK)
                                    .Where(smc => smc.SLTAgencyPK == agencyToRemove.SLTAgencyPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the State Leadership Team Agency!", 1000);

                            //Bind the gridview
                            bsGRAgencies.DataBind();
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the State Leadership Team Agency, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Agency!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Agency!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the State Leadership Team Agency to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the Agency DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efAgencyDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efAgencyDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "SLTAgencyPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.SLTAgency.AsNoTracking()
                                    .Include(sa => sa.State)
                                    .Where(sa => currentProgramRole.StateFKs.Contains(sa.StateFK));
        }

        #endregion

        #region Work Groups

        /// <summary>
        /// This method executes when the user clicks the delete button for an WorkGroup
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteWorkGroup LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteWorkGroup_Click(object sender, EventArgs e)
        {
            if (currentWorkGroupPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeWorkGroupPK = string.IsNullOrWhiteSpace(hfDeleteWorkGroupPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteWorkGroupPK.Value);

                if (removeWorkGroupPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the work groupto remove
                            Models.SLTWorkGroup workGroupToRemove = context.SLTWorkGroup.Where(sm => sm.SLTWorkGroupPK == removeWorkGroupPK).FirstOrDefault();

                            //Remove the work group
                            context.SLTWorkGroup.Remove(workGroupToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTWorkGroupChanged
                                    .OrderByDescending(smc => smc.SLTWorkGroupChangedPK)
                                    .Where(smc => smc.SLTWorkGroupPK == workGroupToRemove.SLTWorkGroupPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the State Leadership Team Work Group!", 1000);

                            //Bind the gridview
                            bsGRAgencies.DataBind();
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the State Leadership Team Work Group, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Work Group!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the State Leadership Team Work Group!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the State Leadership Team Work Group to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the WorkGroup DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efWorkGroupDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efWorkGroupDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "SLTWorkGroupPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.SLTWorkGroup.AsNoTracking()
                                    .Include(swg => swg.State)
                                    .Where(swg => currentProgramRole.StateFKs.Contains(swg.StateFK));
        }

        #endregion
    }
}