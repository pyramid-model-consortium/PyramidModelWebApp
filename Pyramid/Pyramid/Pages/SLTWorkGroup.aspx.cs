using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Pyramid.Pages
{
    public partial class SLTWorkGroup : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "SLTWG";
            }
        }

        public CodeProgramRolePermission FormPermissions
        {
            get
            {
                return currentPermissions;
            }
            set
            {
                currentPermissions = value;
            }
        }

        private CodeProgramRolePermission currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.SLTWorkGroup currentSLTWorkGroup;
        private int currentStateFK;
        private int currentWorkGroupPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the Work Group pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["SLTWorkGroupPK"]))
            {
                //Parse the Work Group pk
                int.TryParse(Request.QueryString["SLTWorkGroupPK"], out currentWorkGroupPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentWorkGroupPK == 0 && !string.IsNullOrWhiteSpace(hfSLTWorkGroupPK.Value))
            {
                int.TryParse(hfSLTWorkGroupPK.Value, out currentWorkGroupPK);
            }

            //Check to see if this is an edit
            isEdit = currentWorkGroupPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Show a message after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect to the dashboard
                Response.Redirect("/Pages/SLTDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the Work Group program object
                currentSLTWorkGroup = context.SLTWorkGroup.AsNoTracking()
                                        .Include(sm => sm.State)
                                        .Where(sm => sm.SLTWorkGroupPK == currentWorkGroupPK).FirstOrDefault();

                //Check to see if the program Work Group exists
                if (currentSLTWorkGroup == null)
                {
                    //The Work Group doesn't exist, set the Work Group to a new Work Group object
                    currentSLTWorkGroup = new Models.SLTWorkGroup();

                    //Set the state label to the current user's state
                    lblState.Text = currentProgramRole.StateName;
                }
                else
                {
                    //Set the state label to the form's state
                    lblState.Text = currentSLTWorkGroup.State.Name;
                }
            }

            //Don't allow users to view work groups from other states
            if (isEdit && !currentProgramRole.StateFKs.Contains(currentSLTWorkGroup.StateFK))
            {
                //Add a message that will show after redirect
                msgSys.AddMessageToQueue("danger", "Not Found", "The work group you are attempting to access does not exist.", 10000);

                //Redirect the user to the dashboard
                Response.Redirect("/Pages/SLTDashboard.aspx");
            }

            //Get the proper state fk
            currentStateFK = (isEdit ? currentSLTWorkGroup.StateFK : currentProgramRole.CurrentStateFK.Value);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Try to get the action type
                if (!string.IsNullOrWhiteSpace(Request.QueryString["Action"]))
                {
                    action = Request.QueryString["Action"].ToString();
                }
                else
                {
                    action = "View";
                }

                //Fill the form with data
                FillFormWithDataFromObject();

                //Allow adding/editing depending on the user's role and the action
                if (isEdit == false && action.ToLower() == "add" && FormPermissions.AllowedToAdd)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Work Group";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Work Group Information";
                }
                else
                {
                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Work Group Information";
                }

                //Set focus to the name field
                txtWorkGroupName.Focus();
            }
        }

        /// <summary>
        /// This method fills the input fields with data from the currentSLTWorkGroup
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (isEdit)
            {
                //Fill the input fields
                txtWorkGroupName.Value = currentSLTWorkGroup.WorkGroupName;
                deStartDate.Value = currentSLTWorkGroup.StartDate;
                deEndDate.Value = currentSLTWorkGroup.EndDate;
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            txtWorkGroupName.ClientEnabled = enabled;
            deStartDate.ClientEnabled = enabled;
            deEndDate.ClientEnabled = enabled;

            //Show/hide the submit button
            submitSLTWorkGroup.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitSLTWorkGroup.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitSLTWorkGroup user control 
        /// </summary>
        /// <param name="sender">The submitSLTWorkGroup control</param>
        /// <param name="e">The Click event</param>
        protected void submitSLTWorkGroup_Click(object sender, EventArgs e)
        {
            //To hold the type of change
            bool wasFormSaved = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (wasFormSaved)
            {
                //Show different messages after redirect based on add vs edit
                if(isEdit)
                {
                    msgSys.AddMessageToQueue("success", "Success", "The Work Group was successfully edited!", 1000);
                }
                else
                {
                    msgSys.AddMessageToQueue("success", "Success", "The Work Group was successfully added!", 1000);
                }

                //Redirect the user to the dashboard
                Response.Redirect("/Pages/SLTDashboard.aspx");
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitSLTWorkGroup user control 
        /// </summary>
        /// <param name="sender">The submitSLTWorkGroup control</param>
        /// <param name="e">The Click event</param>
        protected void submitSLTWorkGroup_CancelClick(object sender, EventArgs e)
        {
            //Show a message on the next page load
            msgSys.AddMessageToQueue("info", "Canceled", "The action was canceled, no changes were saved.", 10000);

            //Redirect the user to the SLT dashboard
            Response.Redirect("/Pages/SLTDashboard.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitSLTWorkGroup control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitSLTWorkGroup_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the user clicks the print/download button
        /// and it displays the form as a report
        /// </summary>
        /// <param name="sender">The btnPrintPreview LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void btnPrintPreview_Click(object sender, EventArgs e)
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitSLTWorkGroup.ValidationGroup))
            {
                //Submit the form but don't show messages
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptSLTWorkGroup report = new Reports.PreBuiltReports.FormReports.RptSLTWorkGroup();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "SLT Work Group", currentWorkGroupPK);
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method populates and saves the form
        /// </summary>
        /// <param name="showMessages">Whether to show messages from the save</param>
        /// <returns>The success message type, null if the save failed</returns>
        private bool SaveForm(bool showMessages)
        {
            //Whether or not the save succeeded
            bool didSaveSucceed = false;

            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the field values from the form
                currentSLTWorkGroup.WorkGroupName = txtWorkGroupName.Text;
                currentSLTWorkGroup.StartDate = deStartDate.Date;
                currentSLTWorkGroup.EndDate = (deEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEndDate.Value));

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the fields
                        currentSLTWorkGroup.EditDate = DateTime.Now;
                        currentSLTWorkGroup.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.SLTWorkGroup existingSLTWorkGroup = context.SLTWorkGroup.Find(currentSLTWorkGroup.SLTWorkGroupPK);

                        //Set the Work Group object to the new values
                        context.Entry(existingSLTWorkGroup).CurrentValues.SetValues(currentSLTWorkGroup);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTWorkGroupPK.Value = currentSLTWorkGroup.SLTWorkGroupPK.ToString();
                        currentWorkGroupPK = currentSLTWorkGroup.SLTWorkGroupPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the field values
                        currentSLTWorkGroup.CreateDate = DateTime.Now;
                        currentSLTWorkGroup.Creator = User.Identity.Name;
                        currentSLTWorkGroup.StateFK = currentStateFK;

                        //Add it to the context
                        context.SLTWorkGroup.Add(currentSLTWorkGroup);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTWorkGroupPK.Value = currentSLTWorkGroup.SLTWorkGroupPK.ToString();
                        currentWorkGroupPK = currentSLTWorkGroup.SLTWorkGroupPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            //Return the success message type
            return didSaveSucceed;
        }

        /// <summary>
        /// This method fires when the validation for the deStartDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deStartDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates
            DateTime? workGroupStartDate = (deStartDate.Value == null ? (DateTime?)null : deStartDate.Date);
            DateTime? workGroupEndDate = (deEndDate.Value == null ? (DateTime?)null : deEndDate.Date);

            //Perform the validation
            if (workGroupStartDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date is required!";
            }
            else if (workGroupEndDate.HasValue && workGroupEndDate.Value <= workGroupStartDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be before the End Date!";
            }
            else if (currentWorkGroupPK > 0)
            {
                List<Models.SLTMemberWorkGroupAssignment> invalidWorkGroupAssignments;
                List<Models.SLTActionPlan> invalidActionPlans;

                //Since this is an edit, validate against related forms to ensure the work group isn't inactive as of the form
                using (PyramidContext context = new PyramidContext())
                {
                    //Get any work group assignments that would be invalidated
                    invalidWorkGroupAssignments = context.SLTMemberWorkGroupAssignment.AsNoTracking()
                                                                .Include(a => a.SLTMember)
                                                                .Where(a => a.SLTWorkGroupFK == currentWorkGroupPK &&
                                                                            ((workGroupEndDate.HasValue && a.StartDate > workGroupEndDate.Value) ||
                                                                            (a.EndDate.HasValue && a.EndDate.Value < workGroupStartDate)))
                                                                .ToList();

                    //Get any action plans that would be invalidated
                    invalidActionPlans = context.SLTActionPlan.AsNoTracking()
                                                        .Where(ap => ap.WorkGroupFK == currentWorkGroupPK &&
                                                                     (ap.ActionPlanEndDate < workGroupStartDate.Value ||
                                                                      (workGroupEndDate.HasValue && ap.ActionPlanStartDate > workGroupEndDate.Value)))
                                                        .ToList();
                }

                //Set validity
                if (invalidWorkGroupAssignments.Count > 0)
                {
                    e.IsValid = false;
                    e.ErrorText = string.Format("{0} work group assignments would be invalidated based on the selected work group start date and end date!  The invalid work group assignments should appear in a message at the bottom right of your screen.", invalidWorkGroupAssignments.Count);

                    //Create a string of the work group assignments split by breaks
                    string invalidAssignmentsString = string.Join("<br/>", invalidWorkGroupAssignments.Select(a => string.Format("Team Member: ({0}) {1} {2}, Start Date: {3:MM/dd/yyyy}, End Date: {4:MM/dd/yyyy}", a.SLTMember.IDNumber, a.SLTMember.FirstName, a.SLTMember.LastName, a.StartDate, a.EndDate)).ToList());

                    //Show a message with the invalid work group assignments
                    msgSys.ShowMessageToUser("danger", "Invalid Work Group Assignments", string.Format("The following work group assignments would be invalidated based on the selected work group start date and end date: <br/> {0}", invalidAssignmentsString), 30000);
                }

                if (invalidActionPlans.Count > 0)
                {
                    e.IsValid = false;
                    e.ErrorText = string.Format("{0} SLT Action Plans would be invalidated based on the selected work group start date and end date!  The invalid SLT Action Plans should appear in a message at the bottom right of your screen.", invalidActionPlans.Count);

                    //Create a string of the work group assignments split by breaks
                    string invalidActionPlansString = string.Join("<br/>", invalidActionPlans.Select(ap => string.Format("Action Plan Year: {0:MM/dd/yyyy} - {1:MM/dd/yyyy}", ap.ActionPlanStartDate, ap.ActionPlanEndDate)).ToList());

                    //Show a message with the invalid work group assignments
                    msgSys.ShowMessageToUser("danger", "Invalid SLT Action Plans", string.Format("The following SLT Action Plans would be invalidated based on the selected work group start date and end date: <br/> {0}", invalidActionPlansString), 30000);
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates
            DateTime? workGroupStartDate = (deStartDate.Value == null ? (DateTime?)null : deStartDate.Date);
            DateTime? workGroupEndDate = (deEndDate.Value == null ? (DateTime?)null : deEndDate.Date);

            //Perform the validation
            if (workGroupEndDate.HasValue && workGroupStartDate.HasValue && workGroupEndDate.Value <= workGroupStartDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date must be after the Start Date!";
            }
        }
    }
}