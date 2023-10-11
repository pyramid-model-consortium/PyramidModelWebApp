using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Z.EntityFramework.Plus;

namespace Pyramid.Pages
{
    public partial class SLTMember : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "SLTM";
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
        private Models.SLTMember currentSLTMember;
        private List<Models.SLTMemberRole> currentRoles;
        private int currentStateFK;
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
            if (!string.IsNullOrWhiteSpace(Request.QueryString["SLTMemberPK"]))
            {
                //Parse the member pk
                int.TryParse(Request.QueryString["SLTMemberPK"], out currentMemberPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentMemberPK == 0 && !string.IsNullOrWhiteSpace(hfSLTMemberPK.Value))
            {
                int.TryParse(hfSLTMemberPK.Value, out currentMemberPK);
            }

            //Check to see if this is an edit
            isEdit = currentMemberPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/SLTDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the member object
                currentSLTMember = context.SLTMember.AsNoTracking()
                                        .Include(sm => sm.SLTMemberRole)
                                        .Include(sm => sm.State)
                                        .Where(sm => sm.SLTMemberPK == currentMemberPK).FirstOrDefault();

                //Check to see if the member exists
                if (currentSLTMember == null)
                {
                    //The member doesn't exist, set the member to a new member object
                    currentSLTMember = new Models.SLTMember();

                    //Set the list of roles to a blank list
                    currentRoles = new List<Models.SLTMemberRole>();

                    //Set the state label to the current user's state
                    lblState.Text = currentProgramRole.StateName;
                }
                else
                {
                    //Get the participants
                    currentRoles = currentSLTMember.SLTMemberRole.ToList();

                    //Set the state label to the form's state
                    lblState.Text = currentSLTMember.State.Name;
                }
            }

            //Don't allow users to view member information from other states
            if (isEdit && !currentProgramRole.StateFKs.Contains(currentSLTMember.StateFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/SLTDashboard.aspx?messageType={0}", "NoMember"));
            }

            //Get the proper state fk
            currentStateFK = (isEdit ? currentSLTMember.StateFK : currentProgramRole.CurrentStateFK.Value);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "MemberAdded":
                            msgSys.ShowMessageToUser("success", "Success", "The State Leadership Team member was successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Show the edit only div if this is an edit
                divEditOnly.Visible = isEdit;

                //Try to get the action type
                if (!string.IsNullOrWhiteSpace(Request.QueryString["Action"]))
                {
                    action = Request.QueryString["Action"].ToString();
                }
                else
                {
                    action = "View";
                }

                //Bind the tables
                BindAgencyAssignments();
                BindWorkGroupAssignments();

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
                    lblPageTitle.Text = "Add New SLT Member";
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
                    lblPageTitle.Text = "Edit SLT Member Information";
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
                    lblPageTitle.Text = "View SLT Member Information";
                }

                //Set the max dates for the date edit fields
                deStartDate.MaxDate = DateTime.Now;
                deLeaveDate.MaxDate = DateTime.Now;
                deAssignmentStartDate.MaxDate = DateTime.Now;
                deAssignmentEndDate.MaxDate = DateTime.Now;

                //Set focus to the first name field
                txtFirstName.Focus();

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
        /// This method fills the input fields with data from the currentSLTMember
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (isEdit)
            {
                //Fill the input fields
                txtFirstName.Value = currentSLTMember.FirstName;
                txtLastName.Value = currentSLTMember.LastName;
                txtIDNumber.Value = currentSLTMember.IDNumber;
                txtEmailAddress.Value = currentSLTMember.EmailAddress;
                txtPhoneNumber.Value = (string.IsNullOrWhiteSpace(currentSLTMember.PhoneNumber) ? null : currentSLTMember.PhoneNumber);
                deStartDate.Value = currentSLTMember.StartDate.ToString("MM/dd/yyyy");
                deLeaveDate.Value = (currentSLTMember.LeaveDate.HasValue ? currentSLTMember.LeaveDate.Value.ToString("MM/dd/yyyy") : "");
                ddEthnicity.SelectedItem = ddEthnicity.Items.FindByValue(currentSLTMember.EthnicityCodeFK);
                ddGender.SelectedItem = ddGender.Items.FindByValue(currentSLTMember.GenderCodeFK);
                txtGenderSpecify.Value = currentSLTMember.GenderSpecify;
                ddHouseholdIncome.SelectedItem = ddHouseholdIncome.Items.FindByValue(currentSLTMember.HouseholdIncomeCodeFK);
                ddRace.SelectedItem = ddRace.Items.FindByValue(currentSLTMember.RaceCodeFK);

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
            txtFirstName.ClientEnabled = enabled;
            txtLastName.ClientEnabled = enabled;
            txtIDNumber.ClientEnabled = enabled;
            txtEmailAddress.ClientEnabled = enabled;
            txtPhoneNumber.ClientEnabled = enabled;
            deStartDate.ClientEnabled = enabled;
            deLeaveDate.ClientEnabled = enabled;
            ddEthnicity.ClientEnabled = enabled;
            ddGender.ClientEnabled = enabled;
            txtGenderSpecify.ClientEnabled = enabled;
            ddHouseholdIncome.ClientEnabled = enabled;
            ddRace.ClientEnabled = enabled;
            tbRoles.ClientEnabled = enabled;

            //Show/hide the submit button
            submitSLTMember.ShowSubmitButton = enabled;
            submitAgencyAssignment.ShowSubmitButton = enabled;
            submitWorkGroupAssignment.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            bool useCancelConfirmations = enabled && areConfirmationsEnabled;

            submitSLTMember.UseCancelConfirm = useCancelConfirmations;
            submitAgencyAssignment.UseCancelConfirm = useCancelConfirmations;
            submitWorkGroupAssignment.UseCancelConfirm = useCancelConfirmations;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDataBoundControls()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the agencies
                var allAgencies = context.SLTAgency.AsNoTracking()
                                    .Where(a => a.StateFK == currentStateFK)
                                    .OrderBy(a => a.Name)
                                    .ToList();

                //Bind the dropdown
                ddAssignmentAgency.DataSource = allAgencies;
                ddAssignmentAgency.DataBind();

                //Bind the role tag box
                List<Models.CodeTeamPosition> allRoles = context.CodeTeamPosition.AsNoTracking()
                                                            .OrderBy(ctp => ctp.OrderBy)
                                                            .ToList();
                tbRoles.DataSource = allRoles;
                tbRoles.DataBind();

                //Get all the gender rows
                var allGenders = context.CodeGender.AsNoTracking()
                                    .OrderBy(cg => cg.OrderBy)
                                    .ToList();

                //Bind the dropdown
                ddGender.DataSource = allGenders;
                ddGender.DataBind();

                //Get all the ethnicity rows
                var allEthnicities = context.CodeEthnicity.AsNoTracking()
                                    .OrderBy(ce => ce.OrderBy)
                                    .ToList();

                //Bind the dropdown
                ddEthnicity.DataSource = allEthnicities;
                ddEthnicity.DataBind();

                //Get all the race rows
                var allRaces = context.CodeRace.AsNoTracking()
                                    .OrderBy(cr => cr.OrderBy)
                                    .ToList();

                //Bind the dropdown
                ddRace.DataSource = allRaces;
                ddRace.DataBind();

                //Get all the household income rows
                var allHouseholdIncomes = context.CodeHouseholdIncome.AsNoTracking()
                                    .OrderBy(chi => chi.OrderBy)
                                    .ToList();

                //Bind the dropdown
                ddHouseholdIncome.DataSource = allHouseholdIncomes;
                ddHouseholdIncome.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitSLTMember user control 
        /// </summary>
        /// <param name="sender">The submitSLTMember control</param>
        /// <param name="e">The Click event</param>
        protected void submitSLTMember_Click(object sender, EventArgs e)
        {
            //To hold the type of change
            string successMessageType = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect differently if add or edit
                if (isEdit)
                {
                    //Redirect the user to the dashboard
                    Response.Redirect(string.Format("/Pages/SLTDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/SLTMember.aspx?SLTMemberPK={0}&Action=Edit&messageType={1}",
                                                        currentMemberPK, successMessageType));
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitSLTMember user control 
        /// </summary>
        /// <param name="sender">The submitSLTMember control</param>
        /// <param name="e">The Click event</param>
        protected void submitSLTMember_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the SLT dashboard
            Response.Redirect(string.Format("/Pages/SLTDashboard.aspx?messageType={0}", "MemberCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitSLTMember control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitSLTMember_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitSLTMember.ValidationGroup))
            {
                //To hold the success message type
                string successMessageType;

                //Submit the form
                successMessageType = SaveForm(false);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptSLTMember report = new Reports.PreBuiltReports.FormReports.RptSLTMember();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "SLT Member Information", currentMemberPK);
                }
                else
                {
                    //Get the action
                    string action = "View";
                    if (!string.IsNullOrWhiteSpace(successMessageType))
                    {
                        //The save was successful, the user will be editing
                        action = "Edit";
                    }

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/SLTMember.aspx?SLTMemberPK={0}&Action={1}&messageType={2}&Print=True",
                                                        currentMemberPK, action, successMessageType));
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
        private string SaveForm(bool showMessages)
        {
            //To hold the type of change
            string successMessageType = null;

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
                        memberIDNumber = string.Format("SID-{0}", currentSLTMember.SLTMemberPK);
                    }
                    else
                    {
                        //To hold the previous SLTMemberPK
                        int previousPK;

                        //Get the previous SLTMemberPK
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the previous PK
                            Models.SLTMember previousMember = context.SLTMember.OrderByDescending(pe => pe.SLTMemberPK).FirstOrDefault();
                            previousPK = (previousMember != null ? previousMember.SLTMemberPK : 0);

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
                currentSLTMember.FirstName = txtFirstName.Value.ToString();
                currentSLTMember.LastName = txtLastName.Value.ToString();
                currentSLTMember.EmailAddress = txtEmailAddress.Value.ToString();
                currentSLTMember.PhoneNumber = (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text);
                currentSLTMember.IDNumber = memberIDNumber;
                currentSLTMember.StartDate = Convert.ToDateTime(deStartDate.Value);
                currentSLTMember.LeaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
                currentSLTMember.EthnicityCodeFK = (ddEthnicity.Value == null ? (int?)null : Convert.ToInt32(ddEthnicity.Value));
                currentSLTMember.GenderCodeFK = (ddGender.Value == null ? (int?)null : Convert.ToInt32(ddGender.Value));
                currentSLTMember.GenderSpecify = (txtGenderSpecify.Value == null ? null : txtGenderSpecify.Value.ToString());
                currentSLTMember.HouseholdIncomeCodeFK = (ddHouseholdIncome.Value == null ? (int?)null : Convert.ToInt32(ddHouseholdIncome.Value));
                currentSLTMember.RaceCodeFK = (ddRace.Value == null ? (int?)null : Convert.ToInt32(ddRace.Value));

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "MemberEdited";

                        //Set the fields
                        currentSLTMember.EditDate = DateTime.Now;
                        currentSLTMember.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.SLTMember existingSLTMember = context.SLTMember.Find(currentSLTMember.SLTMemberPK);

                        //Set the member object to the new values
                        context.Entry(existingSLTMember).CurrentValues.SetValues(currentSLTMember);

                        //Get the selected roles
                        List<int> selectedRoles = new List<int>();
                        if (tbRoles.Value != null && !string.IsNullOrWhiteSpace(tbRoles.Value.ToString()))
                        {
                            selectedRoles = tbRoles.Value.ToString().Split(',').Select(int.Parse).ToList();
                        }

                        //Fill the list of roles
                        foreach (int rolePK in selectedRoles)
                        {
                            SLTMemberRole existingRoleRecord = currentRoles.Where(r => r.TeamPositionCodeFK == rolePK).FirstOrDefault();

                            if (existingRoleRecord == null || existingRoleRecord.SLTMemberRolePK == 0)
                            {
                                //Add missing roles
                                existingRoleRecord = new SLTMemberRole();
                                existingRoleRecord.CreateDate = DateTime.Now;
                                existingRoleRecord.Creator = User.Identity.Name;
                                existingRoleRecord.SLTMemberFK = currentSLTMember.SLTMemberPK;
                                existingRoleRecord.TeamPositionCodeFK = rolePK;
                                context.SLTMemberRole.Add(existingRoleRecord);
                            }
                        }

                        //To hold the role PKs that will be removed
                        List<int> deletedRolePKs = new List<int>();

                        //Get all the roles that should no longer be linked
                        foreach (SLTMemberRole role in currentRoles)
                        {
                            bool keepRole = selectedRoles.Exists(r => r == role.TeamPositionCodeFK);

                            if (keepRole == false)
                            {
                                deletedRolePKs.Add(role.SLTMemberRolePK);
                            }
                        }

                        //Delete the role rows
                        context.SLTMemberRole.Where(pmr => deletedRolePKs.Contains(pmr.SLTMemberRolePK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows
                        context.SLTMemberRoleChanged.Where(pmrc => deletedRolePKs.Contains(pmrc.SLTMemberRolePK)).Update(pc => new SLTMemberRoleChanged() { Deleter = User.Identity.Name });

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTMemberPK.Value = currentSLTMember.SLTMemberPK.ToString();
                        currentMemberPK = currentSLTMember.SLTMemberPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "MemberAdded";

                        //Set the field values
                        currentSLTMember.CreateDate = DateTime.Now;
                        currentSLTMember.Creator = User.Identity.Name;
                        currentSLTMember.StateFK = currentStateFK;

                        //Add it to the context and save
                        context.SLTMember.Add(currentSLTMember);
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
                            SLTMemberRole newRoleRecord = new SLTMemberRole();

                            newRoleRecord.CreateDate = DateTime.Now;
                            newRoleRecord.Creator = User.Identity.Name;
                            newRoleRecord.SLTMemberFK = currentSLTMember.SLTMemberPK;
                            newRoleRecord.TeamPositionCodeFK = rolePK;
                            context.SLTMemberRole.Add(newRoleRecord);
                        }

                        //Save the roles
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfSLTMemberPK.Value = currentSLTMember.SLTMemberPK.ToString();
                        currentMemberPK = currentSLTMember.SLTMemberPK;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            //Return the success message type
            return successMessageType;
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
            //Get the ID number
            string programID = txtIDNumber.Text;

            //Perform validation
            if (!string.IsNullOrWhiteSpace(programID))
            {
                //To hold all the other members with a matching ID Number
                List<Models.SLTMember> matchingMembers = new List<Models.SLTMember>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all other members in this state with a matching program ID
                    matchingMembers = context.SLTMember.AsNoTracking()
                                                    .Where(sm => sm.StateFK == currentStateFK &&
                                                                sm.SLTMemberPK != currentMemberPK &&
                                                                sm.IDNumber.ToLower().Trim() == programID.ToLower().Trim()).ToList();
                }

                //Check to see if another member already has this ID
                if (matchingMembers.Count > 0)
                {
                    //Invalid
                    e.IsValid = false;
                    e.ErrorText = "That ID Number is already taken!";
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
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the start date against other forms' dates
                    var formValidationResults = context.spValidateSLTMemberStartLeaveDates(currentSLTMember.SLTMemberPK,
                                                    currentStateFK, startDate, null).ToList();

                    //If there are results, the start date is invalid
                    if (formValidationResults.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Start Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "The Start Date would invalidate these records if changed to that date:<br/><br/>";

                        foreach (spValidateSLTMemberStartLeaveDates_Result invalidForm in formValidationResults)
                        {
                            message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                            message += "<br/>";
                        }

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Start Date Validation Error", message, 200000);
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
            else if (leaveDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the termination date against other forms' dates
                    var formValidationResults = context.spValidateSLTMemberStartLeaveDates(currentSLTMember.SLTMemberPK,
                                                    currentStateFK, (DateTime?)null, leaveDate).ToList();

                    //If there are results, the termination date is invalid
                    if (formValidationResults.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Leave Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "The Leave Date would invalidate these records if changed to that date:<br/><br/>";

                        foreach (spValidateSLTMemberStartLeaveDates_Result invalidForm in formValidationResults)
                        {
                            message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                            message += "<br/>";
                        }

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Leave Date Validation Error", message, 200000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtGenderSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtGenderSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtGenderSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string genderSpecify = (txtGenderSpecify.Value == null ? null : txtGenderSpecify.Value.ToString());

            //Perform validation
            if (ddGender.SelectedItem != null && ddGender.SelectedItem.Text.ToLower() == "prefer to self-describe" && string.IsNullOrWhiteSpace(genderSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Gender is required when the 'Prefer to self-describe' gender option is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        #endregion

        #region Agency Assignments

        /// <summary>
        /// This method populates the agency assignment repeater with up-to-date information
        /// </summary>
        private void BindAgencyAssignments()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var agencyAssignments = context.SLTMemberAgencyAssignment.AsNoTracking()
                                            .Include(smaa => smaa.SLTAgency)
                                            .Where(smaa => smaa.SLTMemberFK == currentSLTMember.SLTMemberPK
                                                && smaa.SLTAgency.StateFK == currentStateFK)
                                            .OrderBy(smaa => smaa.StartDate)
                                            .ToList();
                repeatAgencyAssignments.DataSource = agencyAssignments;
                repeatAgencyAssignments.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the agency
        /// assignments and it opens a div that allows the user to add a agency assignment
        /// </summary>
        /// <param name="sender">The lbAddAgencyAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddAgencyAssignment_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditAgencyAssignmentPK.Value = "0";
            ddAssignmentAgency.Value = "";
            deAssignmentStartDate.Value = "";
            deAssignmentEndDate.Value = "";

            //Set the title
            lblAddEditAgencyAssignment.Text = "Add Agency Assignment";

            //Show the input div
            divAddEditAgencyAssignment.Visible = true;

            //Set focus to the first field
            ddAssignmentAgency.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a agency assignment
        /// and it opens the edit div so that the user can edit the selected agency assignment
        /// </summary>
        /// <param name="sender">The lbEditAgencyAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditAgencyAssignment_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblAgencyAssignmentPK = (Label)item.FindControl("lblAgencyAssignmentPK");

            //Get the PK from the label
            int? assignmentPK = (string.IsNullOrWhiteSpace(lblAgencyAssignmentPK.Text) ? (int?)null : Convert.ToInt32(lblAgencyAssignmentPK.Text));

            if (assignmentPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the agency assignment to edit
                    SLTMemberAgencyAssignment editAgencyAssignment = context.SLTMemberAgencyAssignment.AsNoTracking().Where(cn => cn.SLTMemberAgencyAssignmentPK == assignmentPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditAgencyAssignment.Text = "Edit Agency Assignment";
                    ddAssignmentAgency.SelectedItem = ddAssignmentAgency.Items.FindByValue(editAgencyAssignment.SLTAgencyFK);
                    deAssignmentStartDate.Value = editAgencyAssignment.StartDate.ToString("MM/dd/yyyy");
                    deAssignmentEndDate.Value = (editAgencyAssignment.EndDate.HasValue ? editAgencyAssignment.EndDate.Value.ToString("MM/dd/yyyy") : "");
                    hfAddEditAgencyAssignmentPK.Value = assignmentPK.Value.ToString();
                }

                //Show the agency assignment div
                divAddEditAgencyAssignment.Visible = true;

                //Set focus to the first field
                ddAssignmentAgency.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected agency assignment!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the agency assignment
        /// add/edit and it closes the agency assignment add/edit div
        /// </summary>
        /// <param name="sender">The submitAgencyAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitAgencyAssignment_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditAgencyAssignmentPK.Value = "0";
            divAddEditAgencyAssignment.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitAgencyAssignment_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the agency assignment
        /// add/edit and it saves the agency assignment information to the database
        /// </summary>
        /// <param name="sender">The submitAgencyAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitAgencyAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit member information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the agency assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditAgencyAssignmentPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    SLTMemberAgencyAssignment currentAgencyAssignment;
                    //Check to see if this is an add or an edit
                    if (assignmentPK == 0)
                    {
                        //Add
                        currentAgencyAssignment = new SLTMemberAgencyAssignment();
                        currentAgencyAssignment.SLTAgencyFK = Convert.ToInt32(ddAssignmentAgency.Value);
                        currentAgencyAssignment.StartDate = Convert.ToDateTime(deAssignmentStartDate.Value);
                        currentAgencyAssignment.EndDate = (deAssignmentEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignmentEndDate.Value));
                        currentAgencyAssignment.SLTMemberFK = currentSLTMember.SLTMemberPK;
                        currentAgencyAssignment.CreateDate = DateTime.Now;
                        currentAgencyAssignment.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTMemberAgencyAssignment.Add(currentAgencyAssignment);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added agency assignment!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentAgencyAssignment = context.SLTMemberAgencyAssignment.Find(assignmentPK);
                        currentAgencyAssignment.SLTAgencyFK = Convert.ToInt32(ddAssignmentAgency.Value);
                        currentAgencyAssignment.StartDate = Convert.ToDateTime(deAssignmentStartDate.Value);
                        currentAgencyAssignment.EndDate = (deAssignmentEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignmentEndDate.Value));
                        currentAgencyAssignment.EditDate = DateTime.Now;
                        currentAgencyAssignment.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited agency assignment!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditAgencyAssignmentPK.Value = "0";
                    divAddEditAgencyAssignment.Visible = false;

                    //Rebind the agency assignment table
                    BindAgencyAssignments();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a agency assignment
        /// and it deletes the agency assignment information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteAgencyAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteAgencyAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit member information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteAgencyAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteAgencyAssignmentPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the agency assignment to remove
                            SLTMemberAgencyAssignment assignmentToRemove = context.SLTMemberAgencyAssignment.Where(cn => cn.SLTMemberAgencyAssignmentPK == rowToRemovePK).FirstOrDefault();

                            //Remove the agency assignment
                            context.SLTMemberAgencyAssignment.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTMemberAgencyAssignmentChanged
                                    .OrderByDescending(ecc => ecc.SLTMemberAgencyAssignmentChangedPK)
                                    .Where(ecc => ecc.SLTMemberAgencyAssignmentPK == assignmentToRemove.SLTMemberAgencyAssignmentPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the agency assignment table
                            BindAgencyAssignments();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted agency assignment!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the agency assignment, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the agency assignment!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the agency assignment!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the agency assignment to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the validation for the deAssignmentStartDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deAssignmentStartDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deAssignmentStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            DateTime? assignmentStartDate = (deAssignmentStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignmentStartDate.Value));
            int? agencyPK = (ddAssignmentAgency.Value == null ? (int?)null : Convert.ToInt32(ddAssignmentAgency.Value));

            //Get the agency assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditAgencyAssignmentPK.Value, out assignmentPK);

            //Perform the validation
            if (startDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The member's start date must be entered before this date!";
            }
            else if (assignmentStartDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date is required!";
            }
            else if (agencyPK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "An Agency needs to be chosen!";
            }
            else if (assignmentStartDate.HasValue && leaveDate.HasValue
                        && (assignmentStartDate.Value > leaveDate.Value || assignmentStartDate.Value < startDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be between the member's start date and leave date!";
            }
            else if (assignmentStartDate.HasValue && leaveDate.HasValue == false
                && (assignmentStartDate.Value > DateTime.Now || assignmentStartDate.Value < startDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be between the member's start date and now!";
            }
            else if (agencyPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var agencyAssignments = context.SLTMemberAgencyAssignment.AsNoTracking()
                                                .Include(tc => tc.SLTAgency)
                                                .Where(tc => tc.SLTMemberFK == currentSLTMember.SLTMemberPK
                                                        && tc.SLTMemberAgencyAssignmentPK != assignmentPK
                                                        && tc.SLTAgencyFK == agencyPK.Value)
                                                .ToList();

                    foreach (SLTMemberAgencyAssignment assignment in agencyAssignments)
                    {
                        if (assignment.EndDate.HasValue == false && assignmentStartDate >= assignment.StartDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Start Date cannot be after the start date for an active assignment with the same agency!";
                        }
                        else if (assignment.EndDate.HasValue && assignmentStartDate >= assignment.StartDate && assignmentStartDate <= assignment.EndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Start Date cannot fall between the start and end dates for another assignment with the same agency!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deAssignmentEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deAssignmentEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deAssignmentEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            DateTime? assignmentStartDate = (deAssignmentStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignmentStartDate.Value));
            DateTime? assignmentEndDate = (deAssignmentEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignmentEndDate.Value));
            int? agencyPK = (ddAssignmentAgency.Value == null ? (int?)null : Convert.ToInt32(ddAssignmentAgency.Value));

            //Get the agency assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditAgencyAssignmentPK.Value, out assignmentPK);

            //Perform the validation
            if (startDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The member's start date must be entered before this date!";
            }
            else if (assignmentStartDate.HasValue == false && assignmentEndDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be entered before the End Date!";
            }
            else if (assignmentEndDate.HasValue && assignmentEndDate.Value < assignmentStartDate)
            {
                e.IsValid = false;
                e.ErrorText = "End Date must be after the Start Date!";
            }
            else if (assignmentEndDate.HasValue && leaveDate.HasValue
                        && assignmentEndDate.Value > leaveDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be after the member's leave date!";
            }
            else if (assignmentEndDate.HasValue && startDate.HasValue
                        && assignmentEndDate.Value < startDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date must be on or after the member's start date!";
            }
            else if (assignmentEndDate.HasValue && assignmentEndDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be in the future!";
            }
            else if(agencyPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var agencyAssignments = context.SLTMemberAgencyAssignment.AsNoTracking()
                                                .Include(tc => tc.SLTAgency)
                                                .Where(tc => tc.SLTMemberFK == currentSLTMember.SLTMemberPK
                                                        && tc.SLTMemberAgencyAssignmentPK != assignmentPK
                                                        && tc.SLTAgencyFK == agencyPK.Value)
                                                .ToList();

                    foreach (SLTMemberAgencyAssignment assignment in agencyAssignments)
                    {
                        if (assignmentEndDate.HasValue == false)
                        {
                            if (assignmentStartDate.HasValue && assignmentStartDate.Value <= assignment.StartDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "End Date is required when adding an assignment that starts before another assignment with the same agency!";
                            }
                        }
                        else if (assignment.EndDate.HasValue == false && assignmentEndDate.Value >= assignment.StartDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "End Date must be before the start date for an active assignment with the same agency!";
                        }
                        else if (assignment.EndDate.HasValue && assignmentEndDate.Value >= assignment.StartDate && assignmentEndDate.Value <= assignment.EndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "End Date cannot fall between an existing range of dates for another assignment with the same agency!";
                        }
                        else if (assignment.StartDate >= assignmentStartDate.Value && assignment.StartDate <= assignmentEndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This agency assignment cannot encapsulate another assignment with the same agency!";
                        }
                    }
                }
            }
        }

        #endregion

        #region Work Group Assignments

        private void BindWorkGroupDropDown(DateTime? assignmentStartDate, DateTime? assignmentEndDate, int? selectWorkGroupFK)
        {
            if(assignmentStartDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the work groups that were active between the passed start date and end date
                    var allWorkGroups = context.SLTWorkGroup.AsNoTracking()
                                            .Where(wg => wg.StateFK == currentStateFK &&
                                                         wg.StartDate <= assignmentStartDate.Value &&
                                                         (wg.EndDate.HasValue == false || wg.EndDate.Value >= assignmentStartDate.Value) &&
                                                         (assignmentEndDate.HasValue == false || 
                                                            (wg.StartDate <= assignmentEndDate.Value) &&
                                                            (wg.EndDate.HasValue == false || wg.EndDate.Value >= assignmentEndDate.Value)))
                                            .OrderBy(wg => wg.WorkGroupName)
                                            .ToList();

                    //Bind the dropdown
                    ddWGAWorkGroup.DataSource = allWorkGroups;
                    ddWGAWorkGroup.DataBind();

                    //Try to re-select the work group
                    if (selectWorkGroupFK.HasValue)
                    {
                        ddWGAWorkGroup.SelectedItem = ddWGAWorkGroup.Items.FindByValue(selectWorkGroupFK.Value);
                    }
                }
            }
            else
            {
                ddWGAWorkGroup.Items.Clear();
            }
        }

        /// <summary>
        /// This method populates the work group assignment repeater with up-to-date information
        /// </summary>
        private void BindWorkGroupAssignments()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var workGroupAssignments = context.SLTMemberWorkGroupAssignment.AsNoTracking()
                                            .Include(smaa => smaa.SLTWorkGroup)
                                            .Where(smaa => smaa.SLTMemberFK == currentSLTMember.SLTMemberPK
                                                && smaa.SLTWorkGroup.StateFK == currentStateFK)
                                            .OrderBy(smaa => smaa.StartDate)
                                            .ToList();
                repeatWorkGroupAssignments.DataSource = workGroupAssignments;
                repeatWorkGroupAssignments.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the work group
        /// assignments and it opens a div that allows the user to add a work group assignment
        /// </summary>
        /// <param name="sender">The lbAddWorkGroupAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddWorkGroupAssignment_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditWorkGroupAssignmentPK.Value = "0";
            ddWGAWorkGroup.Value = "";
            deWGAStartDate.Value = "";
            deWGAEndDate.Value = "";

            //Set the title
            lblAddEditWorkGroupAssignment.Text = "Add Work Group Assignment";

            //Show the input div
            divAddEditWorkGroupAssignment.Visible = true;

            //Bind the workgroup drop down
            BindWorkGroupDropDown(null, null, null);

            //Set focus to the first field
            deWGAStartDate.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a work group assignment
        /// and it opens the edit div so that the user can edit the selected work group assignment
        /// </summary>
        /// <param name="sender">The lbEditWorkGroupAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditWorkGroupAssignment_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblWorkGroupAssignmentPK = (Label)item.FindControl("lblWorkGroupAssignmentPK");

            //Get the PK from the label
            int? assignmentPK = (string.IsNullOrWhiteSpace(lblWorkGroupAssignmentPK.Text) ? (int?)null : Convert.ToInt32(lblWorkGroupAssignmentPK.Text));

            if (assignmentPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the work group assignment to edit
                    SLTMemberWorkGroupAssignment editWorkGroupAssignment = context.SLTMemberWorkGroupAssignment.AsNoTracking().Where(cn => cn.SLTMemberWorkGroupAssignmentPK == assignmentPK.Value).FirstOrDefault();

                    //Bind the workgroup drop down
                    BindWorkGroupDropDown(editWorkGroupAssignment.StartDate, editWorkGroupAssignment.EndDate, editWorkGroupAssignment.SLTWorkGroupFK);

                    //Fill the inputs
                    lblAddEditWorkGroupAssignment.Text = "Edit Work Group Assignment";
                    ddWGAWorkGroup.SelectedItem = ddWGAWorkGroup.Items.FindByValue(editWorkGroupAssignment.SLTWorkGroupFK);
                    deWGAStartDate.Value = editWorkGroupAssignment.StartDate.ToString("MM/dd/yyyy");
                    deWGAEndDate.Value = (editWorkGroupAssignment.EndDate.HasValue ? editWorkGroupAssignment.EndDate.Value.ToString("MM/dd/yyyy") : "");
                    hfAddEditWorkGroupAssignmentPK.Value = assignmentPK.Value.ToString();
                }

                //Show the work group assignment div
                divAddEditWorkGroupAssignment.Visible = true;

                //Set focus to the first field
                ddWGAWorkGroup.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected work group assignment!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the work group assignment
        /// add/edit and it closes the work group assignment add/edit div
        /// </summary>
        /// <param name="sender">The submitWorkGroupAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitWorkGroupAssignment_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditWorkGroupAssignmentPK.Value = "0";
            divAddEditWorkGroupAssignment.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitWorkGroupAssignment_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the work group assignment
        /// add/edit and it saves the work group assignment information to the database
        /// </summary>
        /// <param name="sender">The submitWorkGroupAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitWorkGroupAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit member information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the work group assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditWorkGroupAssignmentPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    SLTMemberWorkGroupAssignment currentWorkGroupAssignment;
                    //Check to see if this is an add or an edit
                    if (assignmentPK == 0)
                    {
                        //Add
                        currentWorkGroupAssignment = new SLTMemberWorkGroupAssignment();
                        currentWorkGroupAssignment.SLTWorkGroupFK = Convert.ToInt32(ddWGAWorkGroup.Value);
                        currentWorkGroupAssignment.StartDate = Convert.ToDateTime(deWGAStartDate.Value);
                        currentWorkGroupAssignment.EndDate = (deWGAEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAEndDate.Value));
                        currentWorkGroupAssignment.SLTMemberFK = currentSLTMember.SLTMemberPK;
                        currentWorkGroupAssignment.CreateDate = DateTime.Now;
                        currentWorkGroupAssignment.Creator = User.Identity.Name;

                        //Save to the database
                        context.SLTMemberWorkGroupAssignment.Add(currentWorkGroupAssignment);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added work group assignment!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentWorkGroupAssignment = context.SLTMemberWorkGroupAssignment.Find(assignmentPK);
                        currentWorkGroupAssignment.SLTWorkGroupFK = Convert.ToInt32(ddWGAWorkGroup.Value);
                        currentWorkGroupAssignment.StartDate = Convert.ToDateTime(deWGAStartDate.Value);
                        currentWorkGroupAssignment.EndDate = (deWGAEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAEndDate.Value));
                        currentWorkGroupAssignment.EditDate = DateTime.Now;
                        currentWorkGroupAssignment.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited work group assignment!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditWorkGroupAssignmentPK.Value = "0";
                    divAddEditWorkGroupAssignment.Visible = false;

                    //Rebind the work group assignment table
                    BindWorkGroupAssignments();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a work group assignment
        /// and it deletes the work group assignment information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteWorkGroupAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteWorkGroupAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit member information
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteWorkGroupAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteWorkGroupAssignmentPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the work group assignment to remove
                            SLTMemberWorkGroupAssignment assignmentToRemove = context.SLTMemberWorkGroupAssignment.Where(cn => cn.SLTMemberWorkGroupAssignmentPK == rowToRemovePK).FirstOrDefault();

                            //Remove the work group assignment
                            context.SLTMemberWorkGroupAssignment.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.SLTMemberWorkGroupAssignmentChanged
                                    .OrderByDescending(ecc => ecc.SLTMemberWorkGroupAssignmentChangedPK)
                                    .Where(ecc => ecc.SLTMemberWorkGroupAssignmentPK == assignmentToRemove.SLTMemberWorkGroupAssignmentPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the work group assignment table
                            BindWorkGroupAssignments();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted work group assignment!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the work group assignment, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the work group assignment!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the work group assignment!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the work group assignment to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the value in a WGA DevExpress
        /// Bootstrap DateEdit changes
        /// </summary>
        /// <param name="sender">The DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deWGADateChanged_ValueChanged(object sender, EventArgs e)
        {
            //Get the current control
            BootstrapDateEdit currentDateEdit = (BootstrapDateEdit)sender;

            //Get the values from the controls
            DateTime? assignmentStartDate = (deWGAStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAStartDate.Value));
            DateTime? assignmentEndDate = (deWGAEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAEndDate.Value));
            int? selectedWorkGroupFK = (ddWGAWorkGroup.Value == null ? (int?)null : Convert.ToInt32(ddWGAWorkGroup.Value));

            //Bind the workgroup dropdown
            BindWorkGroupDropDown(assignmentStartDate, assignmentEndDate, selectedWorkGroupFK);

            //Set the focus back to the field
            currentDateEdit.Focus();
        }

        /// <summary>
        /// This method fires when the validation for the deWGAStartDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deWGAStartDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deWGAStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            DateTime? assignmentStartDate = (deWGAStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAStartDate.Value));
            int? workGroupPK = (ddWGAWorkGroup.Value == null ? (int?)null : Convert.ToInt32(ddWGAWorkGroup.Value));

            //Get the work group assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditWorkGroupAssignmentPK.Value, out assignmentPK);

            //Perform the validation
            if (startDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The member's start date must be entered before this date!";
            }
            else if (assignmentStartDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date is required!";
            }
            else if (assignmentStartDate.HasValue && leaveDate.HasValue
                        && (assignmentStartDate.Value > leaveDate.Value || assignmentStartDate.Value < startDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be between the member's start date and leave date!";
            }
            else if (assignmentStartDate.HasValue && leaveDate.HasValue == false
                && (assignmentStartDate.Value > DateTime.Now || assignmentStartDate.Value < startDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be between the member's start date and now!";
            }
            else if (workGroupPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var workGroupAssignments = context.SLTMemberWorkGroupAssignment.AsNoTracking()
                                                .Include(tc => tc.SLTWorkGroup)
                                                .Where(tc => tc.SLTMemberFK == currentSLTMember.SLTMemberPK
                                                        && tc.SLTMemberWorkGroupAssignmentPK != assignmentPK
                                                        && tc.SLTWorkGroupFK == workGroupPK.Value)
                                                .ToList();

                    foreach (SLTMemberWorkGroupAssignment assignment in workGroupAssignments)
                    {
                        if (assignment.EndDate.HasValue == false && assignmentStartDate >= assignment.StartDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Start Date cannot be after the start date for an active assignment with the same work group!";
                        }
                        else if (assignment.EndDate.HasValue && assignmentStartDate >= assignment.StartDate && assignmentStartDate <= assignment.EndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Start Date cannot fall between the start and end dates for another assignment with the same work group!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deWGAEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deWGAEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deWGAEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            DateTime? assignmentStartDate = (deWGAStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAStartDate.Value));
            DateTime? assignmentEndDate = (deWGAEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deWGAEndDate.Value));
            int? workGroupPK = (ddWGAWorkGroup.Value == null ? (int?)null : Convert.ToInt32(ddWGAWorkGroup.Value));

            //Get the work group assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditWorkGroupAssignmentPK.Value, out assignmentPK);

            //Perform the validation
            if (startDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The member's start date must be entered before this date!";
            }
            else if (assignmentStartDate.HasValue == false && assignmentEndDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be entered before the End Date!";
            }
            else if (assignmentEndDate.HasValue && assignmentEndDate.Value < assignmentStartDate)
            {
                e.IsValid = false;
                e.ErrorText = "End Date must be after the Start Date!";
            }
            else if (assignmentEndDate.HasValue && leaveDate.HasValue
                        && assignmentEndDate.Value > leaveDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be after the member's leave date!";
            }
            else if (assignmentEndDate.HasValue && startDate.HasValue
                        && assignmentEndDate.Value < startDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date must be on or after the member's start date!";
            }
            else if (assignmentEndDate.HasValue && assignmentEndDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be in the future!";
            }
            else if (workGroupPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var workGroupAssignments = context.SLTMemberWorkGroupAssignment.AsNoTracking()
                                                .Include(tc => tc.SLTWorkGroup)
                                                .Where(tc => tc.SLTMemberFK == currentSLTMember.SLTMemberPK
                                                        && tc.SLTMemberWorkGroupAssignmentPK != assignmentPK
                                                        && tc.SLTWorkGroupFK == workGroupPK.Value)
                                                .ToList();

                    foreach (SLTMemberWorkGroupAssignment assignment in workGroupAssignments)
                    {
                        if (assignmentEndDate.HasValue == false)
                        {
                            if (assignmentStartDate.HasValue && assignmentStartDate.Value <= assignment.StartDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "End Date is required when adding an assignment that starts before another assignment with the same work group!";
                            }
                        }
                        else if (assignment.EndDate.HasValue == false && assignmentEndDate.Value >= assignment.StartDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "End Date must be before the start date for an active assignment with the same work group!";
                        }
                        else if (assignment.EndDate.HasValue && assignmentEndDate.Value >= assignment.StartDate && assignmentEndDate.Value <= assignment.EndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "End Date cannot fall between an existing range of dates for another assignment with the same work group!";
                        }
                        else if (assignment.StartDate >= assignmentStartDate.Value && assignment.StartDate <= assignmentEndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This work group assignment cannot encapsulate another assignment with the same work group!";
                        }
                    }
                }
            }
        }

        #endregion
    }
}