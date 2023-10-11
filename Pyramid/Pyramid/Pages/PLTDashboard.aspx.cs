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
    public partial class PLTDashboard : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "PAP",
                    "PAPFCC",
                    "PLTM",
                    "PA"
                };
            }
        }


        private List<CodeProgramRolePermission> FormPermissions;
        private CodeProgramRolePermission PAPFormPermissions;
        private CodeProgramRolePermission PAPFCCFormPermissions;
        private CodeProgramRolePermission memberFormPermissions;
        private CodeProgramRolePermission PAFormPermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private List<PyramidUser> usersForDashboard;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the leadership coach records
            usersForDashboard = PyramidUser.GetProgramLeadershipCoachUserRecords(currentProgramRole.ProgramFKs);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permission objects
            PAPFormPermissions = FormPermissions.Where(p => p.CodeForm.FormAbbreviation == "PAP").FirstOrDefault();
            PAPFCCFormPermissions = FormPermissions.Where(p => p.CodeForm.FormAbbreviation == "PAPFCC").FirstOrDefault();
            memberFormPermissions = FormPermissions.Where(p => p.CodeForm.FormAbbreviation == "PLTM").FirstOrDefault();
            PAFormPermissions = FormPermissions.Where(p => p.CodeForm.FormAbbreviation == "PA").FirstOrDefault();


            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Set permissions for the forms
            bool showStateColumns = currentProgramRole.StateFKs.Count > 1;
            SetPermissions(PAPFormPermissions, divActionPlan, bsGRProgramActionPlans, hfPAPViewOnly, showStateColumns);
            SetPermissions(PAPFCCFormPermissions, divActionPlanFCC, bsGRProgramActionPlanFCCs, hfPAPFCCViewOnly, showStateColumns);
            SetPermissions(memberFormPermissions, divMembers, bsGRMembers, hfMemberViewOnly, showStateColumns);
            SetPermissions(PAFormPermissions, divProgramAddress, bsGRProgramAddresses, hfProgramAddressViewOnly, showStateColumns);

            if (!IsPostBack)
            {
                //Bind the dropdowns
                BindDropdowns();

                //Set visibility of the action plan based on the regular BOQ visibility
                if (currentProgramRole.ShowBOQ.HasValue == false || currentProgramRole.ShowBOQ.Value == false)
                {
                    divActionPlan.Visible = false;
                }

                //Set visibility of the FCC action plan based on the FCC BOQ visibility
                if (currentProgramRole.ShowBOQFCC.HasValue == false || currentProgramRole.ShowBOQFCC.Value == false)
                {
                    divActionPlanFCC.Visible = false;
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

        /// <summary>
        /// This method is used to bind the dropdowns on the page
        /// </summary>
        private void BindDropdowns()
        {
            //To hold all the programs
            List<Program> allPrograms = new List<Program>();

            //Get all the programs
            using (PyramidContext context = new PyramidContext())
            {
                allPrograms = context.Program.Include(p => p.ProgramType).AsNoTracking().Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK)).OrderBy(p => p.ProgramName).ToList();
            }

            var allFCCPrograms = allPrograms
                                        .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK) &&
                                                    p.ProgramType.Where(pt => pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.FAMILY_CHILD_CARE ||
                                                                            pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE).Count() > 0)
                                        .OrderBy(p => p.ProgramName)
                                        .ToList();

            var allNonFCCPrograms = allPrograms
                                        .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK) &&
                                                    p.ProgramType.Where(pt => pt.TypeCodeFK != (int)Utilities.ProgramTypeFKs.FAMILY_CHILD_CARE &&
                                                                            pt.TypeCodeFK != (int)Utilities.ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE).Count() > 0)
                                        .OrderBy(p => p.ProgramName)
                                        .ToList();

            
        }

        #region Program Action Plan

        /// <summary>
        /// This method fires for each row in the bsGRProgramActionPlan grid view and 
        /// it populates the Leadership Coach column.
        /// </summary>
        /// <param name="sender">The bsGRProgramActionPlan BootstrapGridView</param>
        /// <param name="e">The row creation event arguments</param>
        protected void bsGRProgramActionPlan_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the leadership Coach
                Label lblLeadershipCoachUsername = (Label)bsGRProgramActionPlans.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRProgramActionPlans.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");

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
                        //This is necessary now that the leadership coach roles are program-specific and can be removed
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
        /// <param name="sender">The efProgramActionPlanDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efProgramActionPlanDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "ProgramActionPlanPK";

            //Get the DB context
            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            e.QueryableSource = context.ProgramActionPlan.AsNoTracking()
                                            .Include(ap => ap.Program)
                                            .Include(ap => ap.Program.Cohort)
                                            .Include(ap => ap.Program.State)
                                            .Where(ap => currentProgramRole.ProgramFKs.Contains(ap.ProgramFK));
        }

        /// <summary>
        /// This method fires when the Prefill New Action Plan button is clicked
        /// </summary>
        /// <param name="sender">The lbPrefillPAP LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbPrefillPAP_Click(object sender, EventArgs e)
        {
            if (PAPFormPermissions.AllowedToAdd)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the most recent action plan for the user's program
                    Models.ProgramActionPlan actionPlanToCopy = context.ProgramActionPlan
                                                                            .Include(ap => ap.ProgramActionPlanActionStep)
                                                                            .Include(ap => ap.ProgramActionPlanActionStep.Select(s => s.ProgramActionPlanActionStepStatus))
                                                                            .Include(ap => ap.ProgramActionPlanGroundRule)
                                                                            .AsNoTracking()
                                                                            .Where(ap => ap.ProgramFK == currentProgramRole.CurrentProgramFK.Value)
                                                                            .OrderByDescending(ap => ap.ActionPlanStartDate)
                                                                            .FirstOrDefault();

                    if (actionPlanToCopy != null && actionPlanToCopy.ProgramActionPlanPK > 0)
                    {
                        //Set the necessary values in the action plan copy
                        actionPlanToCopy.ProgramActionPlanPK = 0;
                        actionPlanToCopy.CreateDate = DateTime.Now;
                        actionPlanToCopy.Creator = User.Identity.Name;
                        actionPlanToCopy.EditDate = null;
                        actionPlanToCopy.Editor = null;
                        actionPlanToCopy.ReviewedActionSteps = false;
                        actionPlanToCopy.ReviewedBasicInfo = false;
                        actionPlanToCopy.ReviewedGroundRules = false;
                        actionPlanToCopy.IsPrefilled = true;

                        //Update the action step audit fields
                        foreach (var actionStep in actionPlanToCopy.ProgramActionPlanActionStep)
                        {
                            actionStep.CreateDate = DateTime.Now;
                            actionStep.Creator = User.Identity.Name;
                            actionStep.EditDate = null;
                            actionStep.Editor = null;

                            //Update the status record audit fields
                            foreach (var stepStatus in actionStep.ProgramActionPlanActionStepStatus)
                            {
                                stepStatus.CreateDate = DateTime.Now;
                                stepStatus.Creator = User.Identity.Name;
                                stepStatus.EditDate = null;
                                stepStatus.Editor = null;
                            }
                        }

                        //Update the ground rule audit fields
                        foreach (var groundRule in actionPlanToCopy.ProgramActionPlanGroundRule)
                        {
                            groundRule.CreateDate = DateTime.Now;
                            groundRule.Creator = User.Identity.Name;
                            groundRule.EditDate = null;
                            groundRule.Editor = null;
                        }

                        //Add the copy to the database (the linked objects in the .Include statements above will be
                        //automatically added with the correct FKs)
                        context.ProgramActionPlan.Add(actionPlanToCopy);
                        context.SaveChanges();

                        //Show a message after redirect
                        msgSys.AddMessageToQueue("success", "Pre-filled Successfully", "Successfully pre-filled this Action Plan from the most recent Action Plan!  You can now review this Action Plan and make changes.", 5000);
                        Response.Redirect(string.Format("/Pages/ProgramActionPlan.aspx?ProgramActionPlanPK={0}&action={1}", actionPlanToCopy.ProgramActionPlanPK, "Edit"));
                    }
                    else
                    {
                        //No action plan existed to be copied, show a message and redirect to add a blank action plan
                        msgSys.AddMessageToQueue("warning", "Pre-fill Failed", "Unable to pre-fill this Action Plan since there is not an existing Action Plan for your program!  You may enter an Action Plan from scratch or cancel.", 1000);
                        Response.Redirect("/Pages/ProgramActionPlan.aspx?ProgramActionPlanPK=0&action=Add");
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
        /// <param name="sender">The lbDeleteProgramActionPlan LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteProgramActionPlan_Click(object sender, EventArgs e)
        {
            if (PAPFormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeProgramActionPlanPK = string.IsNullOrWhiteSpace(hfDeleteProgramActionPlanPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteProgramActionPlanPK.Value);

                if (removeProgramActionPlanPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the Action Plan to remove
                            Models.ProgramActionPlan actionPlanToRemove = context.ProgramActionPlan.Where(ap => ap.ProgramActionPlanPK == removeProgramActionPlanPK).FirstOrDefault();

                            //Get the meeting rows to remove and remove them
                            var meetingsToRemove = context.ProgramActionPlanMeeting.Where(m => m.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK).ToList();
                            context.ProgramActionPlanMeeting.RemoveRange(meetingsToRemove);

                            //Get the ground rule rows to remove and remove them
                            var groundRulesToRemove = context.ProgramActionPlanGroundRule.Where(gr => gr.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK).ToList();
                            context.ProgramActionPlanGroundRule.RemoveRange(groundRulesToRemove);

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.ProgramActionPlanActionStepStatus
                                .Include(ass => ass.ProgramActionPlanActionStep)
                                .Where(ass => ass.ProgramActionPlanActionStep.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK).ToList();
                            context.ProgramActionPlanActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Get the action step rows to remove and remove them
                            var actionStepsToRemove = context.ProgramActionPlanActionStep.Where(step => step.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK).ToList();
                            context.ProgramActionPlanActionStep.RemoveRange(actionStepsToRemove);

                            //Remove the Action Plan
                            context.ProgramActionPlan.Remove(actionPlanToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the meeting deletions
                            if (meetingsToRemove.Count > 0)
                            {
                                //Get the meeting change rows and set the deleter
                                context.ProgramActionPlanMeetingChanged.Where(mc => mc.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK)
                                                                .OrderByDescending(mc => mc.ProgramActionPlanMeetingChangedPK)
                                                                .Take(meetingsToRemove.Count).ToList()
                                                                .Select(mc => { mc.Deleter = User.Identity.Name; return mc; }).Count();
                            }

                            //Check the ground rule deletions
                            if (groundRulesToRemove.Count > 0)
                            {
                                //Get the ground rule change rows and set the deleter
                                context.ProgramActionPlanGroundRuleChanged.Where(grc => grc.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK)
                                                                .OrderByDescending(grc => grc.ProgramActionPlanGroundRuleChangedPK)
                                                                .Take(groundRulesToRemove.Count).ToList()
                                                                .Select(grc => { grc.Deleter = User.Identity.Name; return grc; }).Count();
                            }

                            //Check the action step deletions
                            if (actionStepsToRemove.Count > 0)
                            {
                                //Get the action step change rows and set the deleter
                                context.ProgramActionPlanActionStepChanged.Where(asc => asc.ProgramActionPlanFK == actionPlanToRemove.ProgramActionPlanPK)
                                                                .OrderByDescending(asc => asc.ProgramActionPlanActionStepChangedPK)
                                                                .Take(actionStepsToRemove.Count).ToList()
                                                                .Select(asc => { asc.Deleter = User.Identity.Name; return asc; }).Count();
                            }

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.ProgramActionPlanActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.ProgramActionPlanActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.ProgramActionPlanActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.ProgramActionPlanActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.ProgramActionPlanChanged
                                    .OrderByDescending(apc => apc.ProgramActionPlanChangedPK)
                                    .Where(apc => apc.ProgramActionPlanPK == actionPlanToRemove.ProgramActionPlanPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Action Plan!", 1000);

                            //Bind the gridview
                            bsGRProgramActionPlans.DataBind();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Action Plan, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Action Plan!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Action Plan!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Action Plan to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        

        #region Program Action Plan FCC

        /// <summary>
        /// This method fires for each row in the bsGRProgramActionPlanFCC grid view and 
        /// it populates the Leadership Coach column.
        /// </summary>
        /// <param name="sender">The bsGRProgramActionPlanFCC BootstrapGridView</param>
        /// <param name="e">The row creation event arguments</param>
        protected void bsGRProgramActionPlanFCC_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the leadership Coach
                Label lblLeadershipCoachUsername = (Label)bsGRProgramActionPlanFCCs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRProgramActionPlanFCCs.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");

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
                }
            }
        }

        /// <summary>
        /// This method fires when the data source for the Action Plan DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efProgramActionPlanFCCDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efProgramActionPlanFCCDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "ProgramActionPlanFCCPK";

            //Get the DB context
            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            e.QueryableSource = context.ProgramActionPlanFCC.AsNoTracking()
                                        .Include(ap => ap.Program)
                                        .Include(ap => ap.Program.Cohort)
                                        .Include(ap => ap.Program.State)
                                        .Where(ap => currentProgramRole.ProgramFKs.Contains(ap.ProgramFK));
        }

        /// <summary>
        /// This method fires when the Prefill New Action Plan button is clicked
        /// </summary>
        /// <param name="sender">The lbPrefillPAPFCC LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbPrefillPAPFCC_Click(object sender, EventArgs e)
        {
            if (PAPFCCFormPermissions.AllowedToAdd)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the most recent action plan for the user's program
                    Models.ProgramActionPlanFCC actionPlanToCopy = context.ProgramActionPlanFCC
                                                                            .Include(ap => ap.ProgramActionPlanFCCActionStep)
                                                                            .Include(ap => ap.ProgramActionPlanFCCActionStep.Select(s => s.ProgramActionPlanFCCActionStepStatus))
                                                                            .Include(ap => ap.ProgramActionPlanFCCGroundRule)
                                                                            .AsNoTracking()
                                                                            .Where(ap => ap.ProgramFK == currentProgramRole.CurrentProgramFK.Value)
                                                                            .OrderByDescending(ap => ap.ActionPlanStartDate)
                                                                            .FirstOrDefault();

                    if (actionPlanToCopy != null && actionPlanToCopy.ProgramActionPlanFCCPK > 0)
                    {
                        //Set the necessary values in the action plan copy
                        actionPlanToCopy.ProgramActionPlanFCCPK = 0;
                        actionPlanToCopy.CreateDate = DateTime.Now;
                        actionPlanToCopy.Creator = User.Identity.Name;
                        actionPlanToCopy.EditDate = null;
                        actionPlanToCopy.Editor = null;
                        actionPlanToCopy.ReviewedActionSteps = false;
                        actionPlanToCopy.ReviewedBasicInfo = false;
                        actionPlanToCopy.ReviewedGroundRules = false;
                        actionPlanToCopy.IsPrefilled = true;

                        //Update the action step audit fields
                        foreach (var actionStep in actionPlanToCopy.ProgramActionPlanFCCActionStep)
                        {
                            actionStep.CreateDate = DateTime.Now;
                            actionStep.Creator = User.Identity.Name;
                            actionStep.EditDate = null;
                            actionStep.Editor = null;

                            //Update the status record audit fields
                            foreach (var stepStatus in actionStep.ProgramActionPlanFCCActionStepStatus)
                            {
                                stepStatus.CreateDate = DateTime.Now;
                                stepStatus.Creator = User.Identity.Name;
                                stepStatus.EditDate = null;
                                stepStatus.Editor = null;
                            }
                        }

                        //Update the ground rule audit fields
                        foreach (var groundRule in actionPlanToCopy.ProgramActionPlanFCCGroundRule)
                        {
                            groundRule.CreateDate = DateTime.Now;
                            groundRule.Creator = User.Identity.Name;
                            groundRule.EditDate = null;
                            groundRule.Editor = null;
                        }

                        //Add the copy to the database (the linked objects in the .Include statements above will be
                        //automatically added with the correct FKs)
                        context.ProgramActionPlanFCC.Add(actionPlanToCopy);
                        context.SaveChanges();

                        //Show a message after redirect
                        msgSys.AddMessageToQueue("success", "Pre-filled Successfully", "Successfully pre-filled this Action Plan from the most recent Action Plan!  You can now review this Action Plan and make changes.", 5000);
                        Response.Redirect(string.Format("/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK={0}&action={1}", actionPlanToCopy.ProgramActionPlanFCCPK, "Edit"));
                    }
                    else
                    {
                        //No action plan existed to be copied, show a message and redirect to add a blank action plan
                        msgSys.AddMessageToQueue("warning", "Pre-fill Failed", "Unable to pre-fill this Action Plan since there is not an existing Action Plan for your program!  You may enter an Action Plan from scratch or cancel.", 1000);
                        Response.Redirect("/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK=0&action=Add");
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
        /// <param name="sender">The lbDeleteProgramActionPlanFCC LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteProgramActionPlanFCC_Click(object sender, EventArgs e)
        {
            if (PAPFCCFormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeProgramActionPlanFCCPK = string.IsNullOrWhiteSpace(hfDeleteProgramActionPlanFCCPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteProgramActionPlanFCCPK.Value);

                if (removeProgramActionPlanFCCPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the Action Plan to remove
                            Models.ProgramActionPlanFCC actionPlanToRemove = context.ProgramActionPlanFCC.Where(ap => ap.ProgramActionPlanFCCPK == removeProgramActionPlanFCCPK).FirstOrDefault();

                            //Get the meeting rows to remove and remove them
                            var meetingsToRemove = context.ProgramActionPlanFCCMeeting.Where(m => m.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK).ToList();
                            context.ProgramActionPlanFCCMeeting.RemoveRange(meetingsToRemove);

                            //Get the ground rule rows to remove and remove them
                            var groundRulesToRemove = context.ProgramActionPlanFCCGroundRule.Where(gr => gr.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK).ToList();
                            context.ProgramActionPlanFCCGroundRule.RemoveRange(groundRulesToRemove);

                            //Get the action step status rows to remove and remove them
                            var actionStepStatusesToRemove = context.ProgramActionPlanFCCActionStepStatus
                                .Include(ass => ass.ProgramActionPlanFCCActionStep)
                                .Where(ass => ass.ProgramActionPlanFCCActionStep.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK).ToList();
                            context.ProgramActionPlanFCCActionStepStatus.RemoveRange(actionStepStatusesToRemove);

                            //Get the action step rows to remove and remove them
                            var actionStepsToRemove = context.ProgramActionPlanFCCActionStep.Where(step => step.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK).ToList();
                            context.ProgramActionPlanFCCActionStep.RemoveRange(actionStepsToRemove);

                            //Remove the Action Plan
                            context.ProgramActionPlanFCC.Remove(actionPlanToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the meeting deletions
                            if (meetingsToRemove.Count > 0)
                            {
                                //Get the meeting change rows and set the deleter
                                context.ProgramActionPlanFCCMeetingChanged.Where(mc => mc.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK)
                                                                .OrderByDescending(mc => mc.ProgramActionPlanFCCMeetingChangedPK)
                                                                .Take(meetingsToRemove.Count).ToList()
                                                                .Select(mc => { mc.Deleter = User.Identity.Name; return mc; }).Count();
                            }

                            //Check the ground rule deletions
                            if (groundRulesToRemove.Count > 0)
                            {
                                //Get the ground rule change rows and set the deleter
                                context.ProgramActionPlanFCCGroundRuleChanged.Where(grc => grc.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK)
                                                                .OrderByDescending(grc => grc.ProgramActionPlanFCCGroundRuleChangedPK)
                                                                .Take(groundRulesToRemove.Count).ToList()
                                                                .Select(grc => { grc.Deleter = User.Identity.Name; return grc; }).Count();
                            }

                            //Check the action step deletions
                            if (actionStepsToRemove.Count > 0)
                            {
                                //Get the action step change rows and set the deleter
                                context.ProgramActionPlanFCCActionStepChanged.Where(asc => asc.ProgramActionPlanFCCFK == actionPlanToRemove.ProgramActionPlanFCCPK)
                                                                .OrderByDescending(asc => asc.ProgramActionPlanFCCActionStepChangedPK)
                                                                .Take(actionStepsToRemove.Count).ToList()
                                                                .Select(asc => { asc.Deleter = User.Identity.Name; return asc; }).Count();
                            }

                            //Check the action step status deletions
                            if (actionStepStatusesToRemove.Count > 0)
                            {
                                //Get a distinct list of action step status PKs
                                var actionStepStatusPKs = actionStepStatusesToRemove.Select(ass => ass.ProgramActionPlanFCCActionStepStatusPK).Distinct().ToList();

                                //Get the action step status change rows and set the deleter
                                context.ProgramActionPlanFCCActionStepStatusChanged.Where(assc => actionStepStatusPKs.Contains(assc.ProgramActionPlanFCCActionStepStatusPK))
                                                                .OrderByDescending(assc => assc.ProgramActionPlanFCCActionStepStatusChangedPK)
                                                                .Take(actionStepStatusesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }

                            //Get the delete change row and set the deleter
                            context.ProgramActionPlanFCCChanged
                                    .OrderByDescending(apc => apc.ProgramActionPlanFCCChangedPK)
                                    .Where(apc => apc.ProgramActionPlanFCCPK == actionPlanToRemove.ProgramActionPlanFCCPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Family Child Care Action Plan!", 1000);

                            //Bind the gridview
                            bsGRProgramActionPlanFCCs.DataBind();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Family Child Care Action Plan, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Family Child Care Action Plan!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Family Child Care Action Plan!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Family Child Care Action Plan to delete!", 120000);
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
        /// This method executes when the user clicks the delete button for a PLT Member
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteMember_Click LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteMember_Click(object sender, EventArgs e)
        {
            if (memberFormPermissions.AllowedToDelete)
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
                            Models.PLTMember memberToRemove = context.PLTMember.Where(sm => sm.PLTMemberPK == removeMemberPK).FirstOrDefault();

                            //Get the role rows to remove (for later)
                            List<int> rolePKsToDelete = context.PLTMemberRole.AsNoTracking().Where(s => s.PLTMemberFK == memberToRemove.PLTMemberPK).Select(s => s.PLTMemberRolePK).ToList();

                            //Remove the role rows
                            context.PLTMemberRole.Where(c => c.PLTMemberFK == memberToRemove.PLTMemberPK).Delete();

                            //Remove the member
                            context.PLTMember.Remove(memberToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.PLTMemberChanged
                                    .OrderByDescending(smc => smc.PLTMemberChangedPK)
                                    .Where(smc => smc.PLTMemberPK == memberToRemove.PLTMemberPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Get the role change rows and update the deleter
                            context.PLTMemberRoleChanged.Where(sc => rolePKsToDelete.Contains(sc.PLTMemberRolePK)).Update(sc => new PLTMemberRoleChanged() { Deleter = User.Identity.Name });

                            //Save the delete change rows to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Program Leadership Team Member!", 1000);

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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Program Leadership Team Member, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Program Leadership Team Member!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Program Leadership Team Member!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Program Leadership Team Member to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the PLT Member DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efPLTMemberDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efMemberDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "PLTMemberPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.PLTMember.AsNoTracking()
                                        .Include(sm => sm.Program)
                                        .Where(sm => currentProgramRole.ProgramFKs.Contains(sm.ProgramFK));
        }

        #endregion

        #region Program Address

        protected void lbDeleteProgramAddress_Click(object sender, EventArgs e)
        {
            if (PAFormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeAddressPK = string.IsNullOrWhiteSpace(hfDeleteProgramAddressPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteProgramAddressPK.Value);

                if (removeAddressPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the address to remove
                            Models.ProgramAddress addressToRemove = context.ProgramAddress.Where(sm => sm.ProgramAddressPK == removeAddressPK).FirstOrDefault();

                            //Remove the address
                            context.ProgramAddress.Remove(addressToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.ProgramAddressChanged
                                    .OrderByDescending(smc => smc.ProgramAddressChangedPK)
                                    .Where(smc => smc.ProgramAddressPK == addressToRemove.ProgramAddressPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change rows to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Program Address!", 1000);

                            //Bind the gridview
                            bsGRProgramAddresses.DataBind();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Program Address, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Program Address!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Program Address!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Program Address to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }


        /// <summary>
        /// This method fires when the data source for the Program Address DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efProgramAddressDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efProgramAddressDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set primary key of the table
            e.KeyExpression = "ProgramAddressPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.ProgramAddress.AsNoTracking()
                                        .Include(sm => sm.Program)
                                        .Where(sm => currentProgramRole.ProgramFKs.Contains(sm.ProgramFK));
        }

        #endregion

    }
}