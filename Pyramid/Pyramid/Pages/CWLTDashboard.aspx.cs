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
    public partial class CWLTDashboard : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "CWLTAP",
                    "BOQCWLT",
                    "CWLTM",
                    "CWLTA",
                    "CWLTAT"
                };
            }
        }

        private List<CodeProgramRolePermission> FormPermissions;
        private CodeProgramRolePermission currentCWLTAPPermissions;
        private CodeProgramRolePermission currentBOQPermissions;
        private CodeProgramRolePermission currentMemberPermissions;
        private CodeProgramRolePermission currentAgencyPermissions;
        private CodeProgramRolePermission currentAgencyTypePermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private List<PyramidUser> usersForDashboard;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the user records
            usersForDashboard = PyramidUser.GetHubLeadershipCoachUserRecords(currentProgramRole.HubFKs, User.Identity.Name);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission objects
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permissions
            currentCWLTAPPermissions = FormPermissions.Where(p => p.CodeForm.FormAbbreviation == "CWLTAP").FirstOrDefault();
            currentBOQPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "BOQCWLT").FirstOrDefault();
            currentMemberPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "CWLTM").FirstOrDefault();
            currentAgencyPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "CWLTA").FirstOrDefault();
            currentAgencyTypePermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "CWLTAT").FirstOrDefault();

            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Determine if the state column should display
            bool showStateColumns = currentProgramRole.StateFKs.Count > 1;

            //Set the permissions for the sections
            //Action Plans
            SetPermissions(currentCWLTAPPermissions, divCWLTActionPlan, bsGRCWLTActionPlans, hfCWLTAPViewOnly, showStateColumns);

            //BOQs
            SetPermissions(currentBOQPermissions, divBOQ, bsGRBOQs, hfBOQViewOnly, showStateColumns);

            //Members
            SetPermissions(currentMemberPermissions, divMembers, bsGRMembers, hfMemberViewOnly, showStateColumns);

            //Agencies
            SetPermissions(currentAgencyPermissions, divAgencies, bsGRAgencies, hfAgencyViewOnly, showStateColumns);

            //Agency Types
            SetPermissions(currentAgencyTypePermissions, divAgencyTypes, bsGRAgencyTypes, hfAgencyTypeViewOnly, showStateColumns);

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
                            msgSys.ShowMessageToUser("success", "Success", "Community-Wide Benchmarks Of Quality form successfully added!", 1000);
                            break;
                        case "BOQEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Community-Wide Benchmarks Of Quality form successfully edited!", 1000);
                            break;
                        case "BOQCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoBOQ":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Community-Wide Benchmarks Of Quality form could not be found, please try again.", 15000);
                            break;
                        case "MemberAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Community Leadership Team Member successfully added!", 1000);
                            break;
                        case "MemberEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Community Leadership Team Member successfully edited!", 1000);
                            break;
                        case "MemberCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoMember":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Community Leadership Team Member could not be found, please try again.", 15000);
                            break;
                        case "AgencyAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Community Leadership Team Agency successfully added!", 1000);
                            break;
                        case "AgencyEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Community Leadership Team Agency successfully edited!", 1000);
                            break;
                        case "AgencyCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoAgency":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Community Leadership Team Agency could not be found, please try again.", 15000);
                            break;
                        case "AgencyTypeAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Community Leadership Team Agency Type successfully added!", 1000);
                            break;
                        case "AgencyTypeEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Community Leadership Team Agency Type successfully edited!", 1000);
                            break;
                        case "AgencyTypeCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoAgencyType":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Community Leadership Team Agency Type could not be found, please try again.", 15000);
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
        private void SetPermissions(CodeProgramRolePermission permissions, HtmlGenericControl sectionDiv, BootstrapGridView gridview, HiddenField viewOnlyHiddenField, bool showStateNameColumns)
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

            //Set visibility of the state name column in the gridview
            if (gridview.Columns["StateNameColumn"] != null)
            {
                gridview.Columns["StateNameColumn"].Visible = showStateNameColumns;
            }
        }
        #region CWLT Action Plan

        /// <summary>
        /// This method fires for each row in the bsGRCWLTActionPlan grid view and 
        /// it populates the Leadership Coach column.
        /// </summary>
        /// <param name="sender">The bsGRCWLTActionPlan BootstrapGridView</param>
        /// <param name="e">The row creation event arguments</param>
        protected void bsGRCWLTActionPlan_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the leadership Coach
                Label lblLeadershipCoachUsername = (Label)bsGRCWLTActionPlans.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRCWLTActionPlans.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");

                //Get the current coach username
                string currentUsername = (e.GetValue("LeadershipCoachUsername") == null ? null : Convert.ToString(e.GetValue("LeadershipCoachUsername")));

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                    else
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are hub-specific and can be removed
                        currentUser = PyramidUser.GetUserRecordByUsername(currentUsername);

                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the data source for the Action Plan DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efCWLTActionPlanDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efCWLTActionPlanDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CWLTActionPlanPK";

            //Get the DB context
            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            e.QueryableSource = context.CWLTActionPlan.AsNoTracking()
                                        .Include(ap => ap.Hub)
                                        .Include(ap => ap.Hub.State)
                                        .Include(ap => ap.CWLTMember)
                                        .Where(ap => currentProgramRole.HubFKs.Contains(ap.HubFK));
        }

        /// <summary>
        /// This method fires when the Prefill New Action Plan button is clicked
        /// </summary>
        /// <param name="sender">The lbPrefillCWLTAP LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbPrefillCWLTAP_Click(object sender, EventArgs e)
        {
            if (currentCWLTAPPermissions.AllowedToAdd)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the most recent action plan for the user's program
                    Models.CWLTActionPlan actionPlanToCopy = context.CWLTActionPlan
                                                                            .Include(ap => ap.CWLTActionPlanActionStep)
                                                                            .Include(ap => ap.CWLTActionPlanActionStep.Select(s => s.CWLTActionPlanActionStepStatus))
                                                                            .Include(ap => ap.CWLTActionPlanGroundRule)
                                                                            .AsNoTracking()
                                                                            .Where(ap => ap.HubFK == currentProgramRole.CurrentHubFK.Value)
                                                                            .OrderByDescending(ap => ap.ActionPlanStartDate)
                                                                            .FirstOrDefault();

                    if (actionPlanToCopy != null && actionPlanToCopy.CWLTActionPlanPK > 0)
                    {
                        //Set the necessary values in the action plan copy
                        actionPlanToCopy.CWLTActionPlanPK = 0;
                        actionPlanToCopy.CreateDate = DateTime.Now;
                        actionPlanToCopy.Creator = User.Identity.Name;
                        actionPlanToCopy.EditDate = null;
                        actionPlanToCopy.Editor = null;
                        actionPlanToCopy.ReviewedActionSteps = false;
                        actionPlanToCopy.ReviewedBasicInfo = false;
                        actionPlanToCopy.ReviewedGroundRules = false;
                        actionPlanToCopy.IsPrefilled = true;

                        //Update the action step audit fields
                        foreach (var actionStep in actionPlanToCopy.CWLTActionPlanActionStep)
                        {
                            actionStep.CreateDate = DateTime.Now;
                            actionStep.Creator = User.Identity.Name;
                            actionStep.EditDate = null;
                            actionStep.Editor = null;

                            //Update the status record audit fields
                            foreach (var stepStatus in actionStep.CWLTActionPlanActionStepStatus)
                            {
                                stepStatus.CreateDate = DateTime.Now;
                                stepStatus.Creator = User.Identity.Name;
                                stepStatus.EditDate = null;
                                stepStatus.Editor = null;
                            }
                        }

                        //Update the ground rule audit fields
                        foreach (var groundRule in actionPlanToCopy.CWLTActionPlanGroundRule)
                        {
                            groundRule.CreateDate = DateTime.Now;
                            groundRule.Creator = User.Identity.Name;
                            groundRule.EditDate = null;
                            groundRule.Editor = null;
                        }

                        //Add the copy to the database (the linked objects in the .Include statements above will be
                        //automatically added with the correct FKs)
                        context.CWLTActionPlan.Add(actionPlanToCopy);
                        context.SaveChanges();

                        //Show a message after redirect
                        msgSys.AddMessageToQueue("success", "Pre-filled Successfully", "Successfully pre-filled this Action Plan from the most recent Action Plan!  You can now review this Action Plan and make changes.", 5000);
                        Response.Redirect(string.Format("/Pages/CWLTActionPlan.aspx?CWLTActionPlanPK={0}&action={1}", actionPlanToCopy.CWLTActionPlanPK, "Edit"));
                    }
                    else
                    {
                        //No action plan existed to be copied, show a message and redirect to add a blank action plan
                        msgSys.AddMessageToQueue("warning", "Pre-fill Failed", "Unable to pre-fill this Action Plan since there is not an existing Action Plan for your hub!  You may enter an Action Plan from scratch or cancel.", 1000);
                        Response.Redirect("/Pages/CWLTActionPlan.aspx?CWLTActionPlanPK=0&action=Add");
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
        /// <param name="sender">The lbDeleteCWLTActionPlan LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteCWLTActionPlan_Click(object sender, EventArgs e)
        {
            if (currentCWLTAPPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeCWLTActionPlanPK = string.IsNullOrWhiteSpace(hfDeleteCWLTActionPlanPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCWLTActionPlanPK.Value);

                if (removeCWLTActionPlanPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the Action Plan to remove
                            Models.CWLTActionPlan actionPlanToRemove = context.CWLTActionPlan.Where(ap => ap.CWLTActionPlanPK == removeCWLTActionPlanPK).FirstOrDefault();

                            //Get the meeting rows to remove and remove them
                            var meetingsToRemove = context.CWLTActionPlanMeeting.Where(m => m.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK).ToList();
                            context.CWLTActionPlanMeeting.RemoveRange(meetingsToRemove);

                            //Get the ground rule rows to remove and remove them
                            var groundRulesToRemove = context.CWLTActionPlanGroundRule.Where(gr => gr.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK).ToList();
                            context.CWLTActionPlanGroundRule.RemoveRange(groundRulesToRemove);

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.CWLTActionPlanActionStepStatus
                                .Include(ass => ass.CWLTActionPlanActionStep)
                                .Where(ass => ass.CWLTActionPlanActionStep.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK).ToList();
                            context.CWLTActionPlanActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Get the action step rows to remove and remove them
                            var actionStepsToRemove = context.CWLTActionPlanActionStep.Where(step => step.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK).ToList();
                            context.CWLTActionPlanActionStep.RemoveRange(actionStepsToRemove);

                            //Remove the Action Plan
                            context.CWLTActionPlan.Remove(actionPlanToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the meeting deletions
                            if (meetingsToRemove.Count > 0)
                            {
                                //Get the meeting change rows and set the deleter
                                context.CWLTActionPlanMeetingChanged.Where(mc => mc.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK)
                                                                .OrderByDescending(mc => mc.CWLTActionPlanMeetingChangedPK)
                                                                .Take(meetingsToRemove.Count).ToList()
                                                                .Select(mc => { mc.Deleter = User.Identity.Name; return mc; }).Count();
                            }

                            //Check the ground rule deletions
                            if (groundRulesToRemove.Count > 0)
                            {
                                //Get the ground rule change rows and set the deleter
                                context.CWLTActionPlanGroundRuleChanged.Where(grc => grc.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK)
                                                                .OrderByDescending(grc => grc.CWLTActionPlanGroundRuleChangedPK)
                                                                .Take(groundRulesToRemove.Count).ToList()
                                                                .Select(grc => { grc.Deleter = User.Identity.Name; return grc; }).Count();
                            }

                            //Check the action step deletions
                            if (actionStepsToRemove.Count > 0)
                            {
                                //Get the action step change rows and set the deleter
                                context.CWLTActionPlanActionStepChanged.Where(asc => asc.CWLTActionPlanFK == actionPlanToRemove.CWLTActionPlanPK)
                                                                .OrderByDescending(asc => asc.CWLTActionPlanActionStepChangedPK)
                                                                .Take(actionStepsToRemove.Count).ToList()
                                                                .Select(asc => { asc.Deleter = User.Identity.Name; return asc; }).Count();
                            }

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.CWLTActionPlanActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.CWLTActionPlanActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.CWLTActionPlanActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.CWLTActionPlanActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.CWLTActionPlanChanged
                                    .OrderByDescending(apc => apc.CWLTActionPlanChangedPK)
                                    .Where(apc => apc.CWLTActionPlanPK == actionPlanToRemove.CWLTActionPlanPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Community-Wide Leadership Team Action Plan!", 1000);

                            //Bind the gridview
                            bsGRCWLTActionPlans.DataBind();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Community-Wide Leadership Team Action Plan, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community-Wide Leadership Team Action Plan!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community-Wide Leadership Team Action Plan!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Community-Wide Leadership Team Action Plan to delete!", 120000);
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
        /// <param name="sender">The efBOQCWLTDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efBOQDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "BenchmarkOfQualityCWLTPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.BOQCWLTView.AsNoTracking().Where(boq => currentProgramRole.HubFKs.Contains(boq.HubFK));
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
                int? removeBOQCWLTPK = string.IsNullOrWhiteSpace(hfDeleteBOQPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteBOQPK.Value);

                if (removeBOQCWLTPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the BOQCWLT to remove
                            Models.BenchmarkOfQualityCWLT BOQCWLTToRemove = context.BenchmarkOfQualityCWLT.Where(boq => boq.BenchmarkOfQualityCWLTPK == removeBOQCWLTPK).FirstOrDefault();

                            //Get the participant rows to remove (for later)
                            List<int> participantPKsToDelete = context.BOQCWLTParticipant.AsNoTracking().Where(p => p.BenchmarksOfQualityCWLTFK == BOQCWLTToRemove.BenchmarkOfQualityCWLTPK).Select(p => p.BOQCWLTParticipantPK).ToList();

                            //Remove the participant rows
                            context.BOQCWLTParticipant.Where(p => p.BenchmarksOfQualityCWLTFK == BOQCWLTToRemove.BenchmarkOfQualityCWLTPK).Delete();

                            //Remove the BOQCWLT
                            context.BenchmarkOfQualityCWLT.Remove(BOQCWLTToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the BOQ delete change row and set the deleter
                            context.BenchmarkOfQualityCWLTChanged
                                    .OrderByDescending(boqc => boqc.BenchmarkOfQualityCWLTChangedPK)
                                    .Where(boqc => boqc.BenchmarkOfQualityCWLTPK == BOQCWLTToRemove.BenchmarkOfQualityCWLTPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Get the participant change rows and update the deleter
                            context.BOQCWLTParticipantChanged.Where(pc => participantPKsToDelete.Contains(pc.BOQCWLTParticipantPK)).Update(pc => new BOQCWLTParticipantChanged() { Deleter = User.Identity.Name });

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Community-Wide Benchmarks of Quality form!", 1000);

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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Community-Wide Benchmark of Quality form, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community-Wide Benchmark of Quality form!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community-Wide Benchmark of Quality form!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Community-Wide Benchmark of Quality form to delete!", 120000);
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
        /// This method executes when the user clicks the delete button for a CWLT Member
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteCWLTMember LinkButton</param>
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
                            Models.CWLTMember memberToRemove = context.CWLTMember.Where(cm => cm.CWLTMemberPK == removeMemberPK).FirstOrDefault();

                            //Get the assignment rows to remove (for later)
                            List<int> assignmentPKsToDelete = context.CWLTMemberAgencyAssignment.AsNoTracking().Where(c => c.CWLTMemberFK == memberToRemove.CWLTMemberPK).Select(c => c.CWLTMemberAgencyAssignmentPK).ToList();

                            //Get the role rows to remove (for later)
                            List<int> rolePKsToDelete = context.CWLTMemberRole.AsNoTracking().Where(s => s.CWLTMemberFK == memberToRemove.CWLTMemberPK).Select(s => s.CWLTMemberRolePK).ToList();

                            //Remove the role rows
                            context.CWLTMemberRole.Where(c => c.CWLTMemberFK == memberToRemove.CWLTMemberPK).Delete();

                            //Remove the assignment rows
                            context.CWLTMemberAgencyAssignment.Where(c => c.CWLTMemberFK == memberToRemove.CWLTMemberPK).Delete();

                            //Remove the member
                            context.CWLTMember.Remove(memberToRemove);

                            //Save the deletions to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CWLTMemberChanged
                                    .OrderByDescending(cmc => cmc.CWLTMemberChangedPK)
                                    .Where(cmc => cmc.CWLTMemberPK == memberToRemove.CWLTMemberPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Get the assignment change rows and update the deleter
                            context.CWLTMemberAgencyAssignmentChanged.Where(cc => assignmentPKsToDelete.Contains(cc.CWLTMemberAgencyAssignmentPK)).Update(cc => new CWLTMemberAgencyAssignmentChanged() { Deleter = User.Identity.Name });

                            //Get the role change rows and update the deleter
                            context.CWLTMemberRoleChanged.Where(sc => rolePKsToDelete.Contains(sc.CWLTMemberRolePK)).Update(sc => new CWLTMemberRoleChanged() { Deleter = User.Identity.Name });

                            //Save the delete change rows to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Community Leadership Team Member!", 1000);

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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Community Leadership Team Member, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community Leadership Team Member!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community Leadership Team Member!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Community Leadership Team Member to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the CWLT Member DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efCWLTMemberDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efMemberDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CWLTMemberPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.CWLTMember.AsNoTracking()
                                        .Include(cm => cm.Hub)
                                        .Include(cm => cm.Hub.State)
                                        .Where(cm => currentProgramRole.HubFKs.Contains(cm.HubFK));
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
                            Models.CWLTAgency agencyToRemove = context.CWLTAgency.Where(cm => cm.CWLTAgencyPK == removeAgencyPK).FirstOrDefault();

                            //Remove the agency
                            context.CWLTAgency.Remove(agencyToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CWLTAgencyChanged
                                    .OrderByDescending(cmc => cmc.CWLTAgencyChangedPK)
                                    .Where(cmc => cmc.CWLTAgencyPK == agencyToRemove.CWLTAgencyPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Community Leadership Team Agency!", 1000);

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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Community Leadership Team Agency, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community Leadership Team Agency!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community Leadership Team Agency!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Community Leadership Team Agency to delete!", 120000);
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
            e.KeyExpression = "CWLTAgencyPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.CWLTAgency.AsNoTracking()
                                    .Include(ca => ca.Hub)
                                    .Include(ca => ca.Hub.State)
                                    .Include(ca => ca.CWLTAgencyType)
                                    .Where(ca => currentProgramRole.HubFKs.Contains(ca.HubFK));
        }

        #endregion

        #region Agency types

        /// <summary>
        /// This method executes when the user clicks the delete button for an agency type
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteAgencyType LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteAgencyType_Click(object sender, EventArgs e)
        {
            if (currentAgencyTypePermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeAgencyTypePK = string.IsNullOrWhiteSpace(hfDeleteAgencyTypePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteAgencyTypePK.Value);

                if (removeAgencyTypePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the agencyType to remove
                            Models.CWLTAgencyType agencyTypeToRemove = context.CWLTAgencyType.Where(cm => cm.CWLTAgencyTypePK == removeAgencyTypePK).FirstOrDefault();

                            //Remove the agencyType
                            context.CWLTAgencyType.Remove(agencyTypeToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CWLTAgencyTypeChanged
                                    .OrderByDescending(cmc => cmc.CWLTAgencyTypeChangedPK)
                                    .Where(cmc => cmc.CWLTAgencyTypePK == agencyTypeToRemove.CWLTAgencyTypePK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Community Leadership Team Agency Type!", 1000);

                            //Bind the gridview
                            bsGRAgencyTypes.DataBind();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Community Leadership Team Agency Type, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community Leadership Team Agency Type!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Community Leadership Team Agency Type!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Community Leadership Team Agency Type to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the AgencyType DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efAgencyTypeDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efAgencyTypeDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CWLTAgencyTypePK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.CWLTAgencyType.AsNoTracking()
                                    .Include(cat => cat.State)
                                    .Where(cat => currentProgramRole.StateFKs.Contains(cat.StateFK));
        }

        #endregion
    }
}