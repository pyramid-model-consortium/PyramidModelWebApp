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
using Z.EntityFramework.Plus;

namespace Pyramid.Pages
{
    public partial class PLTMember : System.Web.UI.Page
    {
        public string FormAbbreviation
        {
            get
            {
                return "PLTM";
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
        private Models.PLTMember currentPLTMember;
        private List<Models.PLTMemberRole> currentRoles;
        private int currentMemberPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the member pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["PLTMemberPK"]))
            {
                //Parse the member pk
                int.TryParse(Request.QueryString["PLTMemberPK"], out currentMemberPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentMemberPK == 0 && !string.IsNullOrWhiteSpace(hfPLTMemberPK.Value))
            {
                int.TryParse(hfPLTMemberPK.Value, out currentMemberPK);
            }

            //Check to see if this is an edit
            isEdit = currentMemberPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Add a message that will display after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect back to the dashboard
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the member object
                currentPLTMember = context.PLTMember.AsNoTracking()
                                        .Include(pm => pm.PLTMemberRole)
                                        .Include(pm => pm.Program)
                                        .Where(pm => pm.PLTMemberPK == currentMemberPK).FirstOrDefault();

                //Check to see if the member exists
                if (currentPLTMember == null)
                {
                    //The member doesn't exist, set the member to a new member object
                    currentPLTMember = new Models.PLTMember();

                    //Set the list of roles to a blank list
                    currentRoles = new List<Models.PLTMemberRole>();
                }
                else
                {
                    //Get the roles
                    currentRoles = currentPLTMember.PLTMemberRole.ToList();
                }
            }

            //Don't allow users to view member information from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentPLTMember.ProgramFK))
            {
                //Add a message to show after redirect
                msgSys.AddMessageToQueue("warning", "Warning", "The specified Program Leadership Team Member could not be found, please try again.", 15000);

                //Redirect the user to the dashboard with an error message
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }

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

                //Bind the drop-downs
                BindDataBoundControls();

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
                    lblPageTitle.Text = "Add New Leadership Team Member";
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
                    lblPageTitle.Text = "Edit Leadership Team Member Information";
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
                    lblPageTitle.Text = "View Leadership Team Member Information";
                }

                //Set focus to the first field
                ddProgram.Focus();

                //Check for the printing item in the query string
                string strIsPrinting = Request.QueryString["Print"];

                //Check to see if printing
                if (!string.IsNullOrWhiteSpace(strIsPrinting))
                {
                    //To hold the printing value
                    bool isPrinting = false;

                    //Print the form if the query string value is true
                    if (bool.TryParse(strIsPrinting, out isPrinting) && isPrinting == true)
                    {
                        //Print the form
                        PrintForm();
                    }
                }
            }
        }

        /// <summary>
        /// This method fills the input fields with data from the currentPLTMember
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (isEdit)
            {
                //Fill the input fields
                ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentPLTMember.ProgramFK);
                txtFirstName.Value = currentPLTMember.FirstName;
                txtLastName.Value = currentPLTMember.LastName;
                txtIDNumber.Value = currentPLTMember.IDNumber;
                txtEmailAddress.Value = currentPLTMember.EmailAddress;
                txtPhoneNumber.Value = (string.IsNullOrWhiteSpace(currentPLTMember.PhoneNumber) ? null : currentPLTMember.PhoneNumber);
                deStartDate.Value = currentPLTMember.StartDate.ToString("MM/dd/yyyy");
                deLeaveDate.Value = (currentPLTMember.LeaveDate.HasValue ? currentPLTMember.LeaveDate.Value.ToString("MM/dd/yyyy") : "");

                //Set the roles
                tbRoles.Value = string.Join(",", currentRoles.Select(r => r.TeamPositionCodeFK));
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            ddProgram.ClientEnabled = enabled;
            txtFirstName.ClientEnabled = enabled;
            txtLastName.ClientEnabled = enabled;
            txtIDNumber.ClientEnabled = enabled;
            txtEmailAddress.ClientEnabled = enabled;
            txtPhoneNumber.ClientEnabled = enabled;
            deStartDate.ClientEnabled = enabled;
            deLeaveDate.ClientEnabled = enabled;
            tbRoles.ClientEnabled = enabled;

            //Show/hide the submit button
            submitPLTMember.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            bool useCancelConfirmations = enabled && areConfirmationsEnabled;

            submitPLTMember.UseCancelConfirm = useCancelConfirmations;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDataBoundControls()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the relevant programs
                List<Program> allPrograms = context.Program.AsNoTracking()
                                        .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                        .OrderBy(p => p.ProgramName)
                                        .ToList();

                //Bind the program dropdown
                ddProgram.DataSource = allPrograms;
                ddProgram.DataBind();

                //Bind the role tag box
                List<Models.CodeTeamPosition> allRoles = context.CodeTeamPosition.AsNoTracking()
                                                            .OrderBy(ctp => ctp.OrderBy)
                                                            .ToList();
                tbRoles.DataSource = allRoles;
                tbRoles.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitPLTMember user control 
        /// </summary>
        /// <param name="sender">The submitPLTMember control</param>
        /// <param name="e">The Click event</param>
        protected void submitPLTMember_Click(object sender, EventArgs e)
        {
            //Try to save the form to the database
            bool formSaved = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (formSaved)
            {
                //Add a message to show after redirect
                msgSys.AddMessageToQueue("success", "Success", "The Program Leadership Team Member was successfully edited!", 10000);

                //Redirect the user to the dashboard
                Response.Redirect("/Pages/PLTDashboard.aspx");
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitPLTMember user control 
        /// </summary>
        /// <param name="sender">The submitPLTMember control</param>
        /// <param name="e">The Click event</param>
        protected void submitPLTMember_CancelClick(object sender, EventArgs e)
        {
            //Add a message that will show after redirect
            msgSys.AddMessageToQueue("info", "Canceled", "The action was canceled, no changes were saved.", 10000);

            //Redirect the user to the PLT dashboard
            Response.Redirect("/Pages/PLTDashboard.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitPLTMember control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitPLTMember_ValidationFailed(object sender, EventArgs e)
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
            //Print the form
            PrintForm();
        }

        /// <summary>
        /// This method prints the form
        /// </summary>
        private void PrintForm()
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitPLTMember.ValidationGroup))
            {
                //Try to save the form to the database
                bool formSaved = SaveForm(false);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptPLTMember report = new Reports.PreBuiltReports.FormReports.RptPLTMember();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "PLT Member Information", currentMemberPK);
                }
                else
                {
                    //Get the action
                    string action = "View";
                    if (formSaved)
                    {
                        //The save was successful, the user will be editing
                        action = "Edit";
                    }

                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "The Program Leadership Team Member was successfully saved!", 12000);

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/PLTMember.aspx?PLTMemberPK={0}&Action={1}&Print=True",
                                                        currentMemberPK, action));
                }
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

            //Check the user permissions
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //To hold the ID number
                string memberIDNumber;

                //Get the ID number
                if (string.IsNullOrWhiteSpace(txtIDNumber.Text))
                {
                    //The user didn't specify an ID, generate one
                    //Check to see if this is an edit
                    if (isEdit)
                    {
                        //This is an edit, use the current PK
                        memberIDNumber = string.Format("SID-{0}", currentPLTMember.PLTMemberPK);
                    }
                    else
                    {
                        //To hold the previous PLTMemberPK
                        int previousPK;

                        //Get the previous PLTMemberPK
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the previous PK
                            Models.PLTMember previousMember = context.PLTMember.OrderByDescending(pe => pe.PLTMemberPK).FirstOrDefault();
                            previousPK = (previousMember != null ? previousMember.PLTMemberPK : 0);

                            //Set the ID number to the previous PK plus one
                            memberIDNumber = string.Format("SID-{0}", (previousPK + 1));
                        }
                    }
                }
                else
                {
                    //Use the ID that the user provided
                    memberIDNumber = txtIDNumber.Text.Trim();
                }

                //Fill the field values from the form
                currentPLTMember.ProgramFK = Convert.ToInt32(ddProgram.Value);
                currentPLTMember.FirstName = txtFirstName.Value.ToString();
                currentPLTMember.LastName = txtLastName.Value.ToString();
                currentPLTMember.EmailAddress = txtEmailAddress.Value.ToString();
                currentPLTMember.PhoneNumber = (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text);
                currentPLTMember.IDNumber = memberIDNumber;
                currentPLTMember.StartDate = Convert.ToDateTime(deStartDate.Value);
                currentPLTMember.LeaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the fields
                        currentPLTMember.EditDate = DateTime.Now;
                        currentPLTMember.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.PLTMember existingPLTMember = context.PLTMember.Find(currentPLTMember.PLTMemberPK);

                        //Set the member object to the new values
                        context.Entry(existingPLTMember).CurrentValues.SetValues(currentPLTMember);

                        //Get the selected roles
                        List<int> selectedRoles = new List<int>();
                        if (tbRoles.Value != null && !string.IsNullOrWhiteSpace(tbRoles.Value.ToString()))
                        {
                            selectedRoles = tbRoles.Value.ToString().Split(',').Select(int.Parse).ToList();
                        }

                        //Fill the list of roles
                        foreach (int rolePK in selectedRoles)
                        {
                            PLTMemberRole existingRoleRecord = currentRoles.Where(r => r.TeamPositionCodeFK == rolePK).FirstOrDefault();

                            if (existingRoleRecord == null || existingRoleRecord.PLTMemberRolePK == 0)
                            {
                                //Add missing roles
                                existingRoleRecord = new PLTMemberRole();
                                existingRoleRecord.CreateDate = DateTime.Now;
                                existingRoleRecord.Creator = User.Identity.Name;
                                existingRoleRecord.PLTMemberFK = currentPLTMember.PLTMemberPK;
                                existingRoleRecord.TeamPositionCodeFK = rolePK;
                                context.PLTMemberRole.Add(existingRoleRecord);
                            }
                        }

                        //To hold the role PKs that will be removed
                        List<int> deletedRolePKs = new List<int>();

                        //Get all the roles that should no longer be linked
                        foreach (PLTMemberRole role in currentRoles)
                        {
                            bool keepRole = selectedRoles.Exists(r => r == role.TeamPositionCodeFK);

                            if (keepRole == false)
                            {
                                deletedRolePKs.Add(role.PLTMemberRolePK);
                            }
                        }

                        //Delete the role rows
                        context.PLTMemberRole.Where(pmr => deletedRolePKs.Contains(pmr.PLTMemberRolePK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows
                        context.PLTMemberRoleChanged.Where(pmrc => deletedRolePKs.Contains(pmrc.PLTMemberRolePK)).Update(pc => new PLTMemberRoleChanged() { Deleter = User.Identity.Name });

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfPLTMemberPK.Value = currentPLTMember.PLTMemberPK.ToString();
                        currentMemberPK = currentPLTMember.PLTMemberPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the field values
                        currentPLTMember.CreateDate = DateTime.Now;
                        currentPLTMember.Creator = User.Identity.Name;

                        //Add it to the context and save
                        context.PLTMember.Add(currentPLTMember);
                        context.SaveChanges();

                        //Get the selected roles
                        List<int> selectedRoles = new List<int>();
                        if (tbRoles.Value != null && !string.IsNullOrWhiteSpace(tbRoles.Value.ToString()))
                        {
                            selectedRoles = tbRoles.Value.ToString().Split(',').Select(int.Parse).ToList();
                        }

                        //Fill the list of roles
                        foreach (int rolePK in selectedRoles)
                        {
                            PLTMemberRole newRoleRecord = new PLTMemberRole();

                            newRoleRecord.CreateDate = DateTime.Now;
                            newRoleRecord.Creator = User.Identity.Name;
                            newRoleRecord.PLTMemberFK = currentPLTMember.PLTMemberPK;
                            newRoleRecord.TeamPositionCodeFK = rolePK;
                            context.PLTMemberRole.Add(newRoleRecord);
                        }

                        //Save the roles
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfPLTMemberPK.Value = currentPLTMember.PLTMemberPK.ToString();
                        currentMemberPK = currentPLTMember.PLTMemberPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            return didSaveSucceed;
        }

        #region Custom Validation

        /// <summary>
        /// This method fires when the validation for the txtIDNumber DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtIDNumber TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtIDNumber_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary values
            string programID = txtIDNumber.Text;
            int? currentProgramFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

            //Perform validation
            if (!string.IsNullOrWhiteSpace(programID) && currentProgramFK.HasValue)
            {
                //To hold all the other members with a matching ID Number
                List<Models.PLTMember> matchingMembers = new List<Models.PLTMember>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all other members in this program with a matching program ID
                    matchingMembers = context.PLTMember.AsNoTracking()
                                                    .Where(sm => sm.ProgramFK == currentProgramFK.Value &&
                                                                sm.PLTMemberPK != currentMemberPK &&
                                                                sm.IDNumber.ToLower().Trim() == programID.ToLower().Trim()).ToList();
                }

                //Check to see if another member already has this ID
                if (matchingMembers.Count > 0)
                {
                    //Invalid
                    e.IsValid = false;
                    e.ErrorText = "That ID Number is already in use by another Leadership Team Member for the selected program!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtEmailAddress DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtEmailAddress TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtEmailAddress_Validation(object sender, ValidationEventArgs e)
        {
            //Get the email address
            string email = (txtEmailAddress.Value == null ? null : txtEmailAddress.Value.ToString());

            //Perform validation
            if (string.IsNullOrWhiteSpace(email))
            {
                //Make sure an email is entered
                e.IsValid = false;
                e.ErrorText = "Email Address is required!";
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                //Make sure the email is in the correct format
                e.IsValid = false;
                e.ErrorText = "Must be a valid email address format!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtPhoneNumber DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPhoneNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtPhoneNumber_Validation(object sender, ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtPhoneNumber.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the deStartDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deStartDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start date and leave date
            DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));

            //Perform the validation
            if (startDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date is required!";
            }
            else if (leaveDate.HasValue && startDate.Value >= leaveDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be before the leave date!";
            }
            else if (startDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date cannot be in the future!";
            }
            else if (isEdit)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the dates against leadership coach debrief attendees
                    List<ProgramLCMeetingDebriefSessionAttendee> invalidLCSessionAttendees = context.ProgramLCMeetingDebriefSessionAttendee
                                                        .Include(sa => sa.ProgramLCMeetingDebriefSession)
                                                        .Include(sa => sa.ProgramLCMeetingDebriefSession.ProgramLCMeetingDebrief)
                                                        .AsNoTracking()
                                                        .Where(sa => sa.PLTMemberFK == currentMemberPK &&
                                                                (currentPLTMember.ProgramFK != sa.ProgramLCMeetingDebriefSession.ProgramLCMeetingDebrief.ProgramFK  ||
                                                                    startDate.Value.Year > sa.ProgramLCMeetingDebriefSession.ProgramLCMeetingDebrief.DebriefYear ||
                                                                         (leaveDate.HasValue == true &&
                                                                            leaveDate.Value.Year < sa.ProgramLCMeetingDebriefSession.ProgramLCMeetingDebrief.DebriefYear)))
                                                        .ToList();

                    if (invalidLCSessionAttendees.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Start Date or Leave Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "By changing the Start Date or Leave Date, this team member would no longer be active during at least one Leadership Team Meeting Debrief Session where they are marked as an attendee.  Please contact your leadership coach or state PIDS administrator if you have questions about this.";

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Validation Error", message, 200000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deLeaveDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deLeaveDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deLeaveDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the termination date, enrollment date, and termination reason
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));

            //Perform the validation
            if (startDate.HasValue == false && leaveDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be entered before the leave date!";
            }
            else if (leaveDate.HasValue && leaveDate.Value < startDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be after the start date!";
            }
            else if (leaveDate.HasValue && leaveDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date cannot be in the future!";
            }
        }

        #endregion
    }
}