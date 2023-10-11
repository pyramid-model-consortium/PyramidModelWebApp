using DevExpress.Web;
using DevExpress.XtraEditors.Filtering.Templates;
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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Pyramid.Pages
{
    public partial class ProgramEmployee : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "PE",
                    "PEPA"
                };
            }
        }

        public List<CodeProgramRolePermission> FormPermissions
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

        private List<CodeProgramRolePermission> currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private CodeProgramRolePermission currentPEPermissions;
        private CodeProgramRolePermission currentPEPAPermissions;
        private Models.ProgramEmployee currentProgramEmployee;
        private int currentProgramFK;
        private int stateFK;
        private int programEmployeePK = 0;
        private bool isEdit = false;


        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission objects
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permissions
            currentPEPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "PE").FirstOrDefault();
            currentPEPAPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "PEPA").FirstOrDefault();

            //Try to get the employee pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ProgramEmployeePK"]))
            {
                //Parse the employee pk
                int.TryParse(Request.QueryString["ProgramEmployeePK"], out programEmployeePK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (programEmployeePK == 0 && !string.IsNullOrWhiteSpace(hfProgramEmployeePK.Value))
            {
                int.TryParse(hfProgramEmployeePK.Value, out programEmployeePK);
            }

            //Check to see if this is an edit
            isEdit = programEmployeePK > 0;

            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToView == true).Count() == 0)
            {
                Response.Redirect("/Pages/ProgramEmployeeDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the employee program object
                currentProgramEmployee = context.ProgramEmployee.AsNoTracking()
                                        .Include(pe => pe.Employee)
                                        .Include(pe => pe.Program)
                                        .Where(pe => pe.ProgramEmployeePK == programEmployeePK).FirstOrDefault();

                //Check to see if the program employee exists
                if (currentProgramEmployee == null)
                {
                    //The employee doesn't exist, set the employee to a new employee object
                    currentProgramEmployee = new Models.ProgramEmployee();
                    currentProgramEmployee.Employee = new Models.Employee();
                }
            }

            //Don't allow users to view employee information from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentProgramEmployee.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/EmployeeDashboard.aspx?messageType={0}", "NoEmployee"));
            }

            //Get the proper program fk
            currentProgramFK = (isEdit ? currentProgramEmployee.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            //Get the proper state fk
            stateFK = (isEdit ? currentProgramEmployee.Program.StateFK : currentProgramRole.CurrentStateFK.Value);

            //Show the ASPIRE column if this is NY
            if(stateFK == (int)Utilities.StateFKs.NEW_YORK)
            {
                hfIsNYEmployee.Value = "true";
            }

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
                        case "EmployeeAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Professional successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        case "FromOtherProgram":
                            msgSys.ShowMessageToUser("primary", "Opened Program Record", "This is another program record for the same professional.", 10000);
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

                //Only show the ASPIRE section if this is a NY employee
                if (stateFK == (int)Utilities.StateFKs.NEW_YORK)
                {
                    divASPIREInfo.Visible = true;
                }
                else
                {
                    divASPIREInfo.Visible = false;
                }

                //Set the permissions for the program assignments section
                SetSubSectionPermissions(currentPEPAPermissions, divProgramAssignments, hfPEPAViewOnly);

                //Bind the tables
                BindClassroomAssignments();
                BindTrainings();
                BindJobFunctions();
                BindProgramAssignments();

                //Bind the drop-downs
                BindDropDowns();

                //Fill the form with data
                FillFormWithDataFromObject();

                //Allow adding/editing depending on the user's role and the action
                if (isEdit == false && action.ToLower() == "add" && currentPEPermissions.AllowedToAdd)
                {
                    //Show other controls
                    hfPEViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Professional";
                }
                else if (isEdit == true && action.ToLower() == "edit" && currentPEPermissions.AllowedToEdit)
                {
                    //Show other controls
                    hfPEViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Professional Information";
                }
                else
                {
                    //Hide other controls
                    hfPEViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Professional Information";
                }

                //Set the max dates for the date edit fields
                deHireDate.MaxDate = DateTime.Now;
                deTerminationDate.MaxDate = DateTime.Now;
                deTrainingDate.MaxDate = DateTime.Now;
                deJobStartDate.MaxDate = DateTime.Now;
                deJobEndDate.MaxDate = DateTime.Now;
                deAssignDate.MaxDate = DateTime.Now;
                deLeaveDate.MaxDate = DateTime.Now;

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
        /// This method sets the visibility and usability of dashboard sections based on the passed permission object
        /// </summary>
        /// <param name="permissions">The permissions for the section</param>
        /// <param name="sectionDiv">The section div</param>
        /// <param name="sectionRepeater">The section gridview</param>
        /// <param name="viewOnlyHiddenField">The view only hidden field</param>
        private void SetSubSectionPermissions(CodeProgramRolePermission permissions, HtmlGenericControl sectionDiv, HiddenField viewOnlyHiddenField)
        {
            //Check permissions
            if (permissions.AllowedToView == false)
            {
                //Not allowed to see the section
                sectionDiv.Visible = false;
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

        /// <summary>
        /// This method fills the input fields with data from the currentProgramEmployee
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Check to see if this is an add or edit
            if (isEdit)
            {
                //Edit, fill the input fields from the object
                ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentProgramEmployee.ProgramFK);
                txtFirstName.Value = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? currentProgramEmployee.Employee.FirstName : "HIDDEN");
                txtLastName.Value = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? currentProgramEmployee.Employee.LastName : "HIDDEN");
                txtProgramID.Value = currentProgramEmployee.ProgramSpecificID;
                ddGender.SelectedItem = ddGender.Items.FindByValue(currentProgramEmployee.Employee.GenderCodeFK);
                txtGenderSpecify.Text = currentProgramEmployee.Employee.GenderSpecify;
                ddEthnicity.SelectedItem = ddEthnicity.Items.FindByValue(currentProgramEmployee.Employee.EthnicityCodeFK);
                txtEthnicitySpecify.Text = currentProgramEmployee.Employee.EthnicitySpecify;
                ddRace.SelectedItem = ddRace.Items.FindByValue(currentProgramEmployee.Employee.RaceCodeFK);
                txtRaceSpecify.Text = currentProgramEmployee.Employee.RaceSpecify;
                txtEmail.Value = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? currentProgramEmployee.Employee.EmailAddress : "HIDDEN");
                ddIsEmployeeOfProgram.SelectedItem = ddIsEmployeeOfProgram.Items.FindByValue(currentProgramEmployee.IsEmployeeOfProgram);
                deHireDate.Value = currentProgramEmployee.HireDate.ToString("MM/dd/yyyy");
                deTerminationDate.Value = (currentProgramEmployee.TermDate.HasValue ? currentProgramEmployee.TermDate.Value.ToString("MM/dd/yyyy") : "");
                ddTerminationReason.SelectedItem = ddTerminationReason.Items.FindByValue(currentProgramEmployee.TermReasonCodeFK);
                txtTerminationReasonSpecify.Value = currentProgramEmployee.TermReasonSpecify;
                txtAspireID.Value = currentProgramEmployee.Employee.AspireID;
                txtAspireEmail.Value = currentProgramEmployee.Employee.AspireEmail;
            }
            else
            {
                //This is an add, preset the program dropdown if the user is only authorized for one program
                if (currentProgramRole.ProgramFKs.Count == 1)
                {
                    ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentProgramRole.CurrentProgramFK);
                }
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            ddProgram.ClientEnabled = (isEdit ? false : enabled);  //If this is an edit, don't allow modification
            txtFirstName.ClientEnabled = enabled;
            txtLastName.ClientEnabled = enabled;
            txtProgramID.ClientEnabled = enabled;
            ddGender.ClientEnabled = enabled;
            txtGenderSpecify.ClientEnabled = enabled;
            ddEthnicity.ClientEnabled = enabled;
            txtEthnicitySpecify.ClientEnabled = enabled;
            ddRace.ClientEnabled = enabled;
            txtRaceSpecify.ClientEnabled = enabled;
            txtEmail.ClientEnabled = enabled;
            ddIsEmployeeOfProgram.ClientEnabled = enabled;
            deHireDate.ClientEnabled = enabled;
            deTerminationDate.ClientEnabled = enabled;
            ddTerminationReason.ClientEnabled = enabled;
            txtTerminationReasonSpecify.ClientEnabled = enabled;
            txtAspireID.ClientEnabled = enabled;
            txtAspireEmail.ClientEnabled = enabled;

            //Show/hide the submit button
            submitEmployee.ShowSubmitButton = enabled;
            submitTraining.ShowSubmitButton = enabled;
            submitJobFunction.ShowSubmitButton = enabled;
            submitClassroomAssignment.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            bool useCancelConfirmations = enabled && areConfirmationsEnabled;

            submitEmployee.UseCancelConfirm = useCancelConfirmations;
            submitTraining.UseCancelConfirm = useCancelConfirmations;
            submitJobFunction.UseCancelConfirm = useCancelConfirmations;
            submitClassroomAssignment.UseCancelConfirm = useCancelConfirmations;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDropDowns()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Fill the program drop-downs
                var programs = context.Program.AsNoTracking().Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK)).OrderBy(p => p.ProgramName).ToList();
                
                ddProgram.DataSource = programs;
                ddProgram.DataBind();

                ddPAProgram.DataSource = programs;
                ddPAProgram.DataBind();

                //Fill the Gender drop-down
                var genders = context.CodeGender.AsNoTracking().OrderBy(cg => cg.OrderBy).ToList();
                ddGender.DataSource = genders;
                ddGender.DataBind();

                //Fill the Ethnicity drop-down
                var ethnicities = context.CodeEthnicity.AsNoTracking().OrderBy(ce => ce.OrderBy).ToList();
                ddEthnicity.DataSource = ethnicities;
                ddEthnicity.DataBind();

                //Fill the Race drop-down
                var races = context.CodeRace.AsNoTracking().OrderBy(cr => cr.OrderBy).ToList();
                ddRace.DataSource = races;
                ddRace.DataBind();

                //Get all the classrooms
                var allClassrooms = context.Classroom.AsNoTracking()
                                    .Where(c => c.ProgramFK == currentProgramFK)
                                    .OrderBy(c => c.ProgramSpecificID)
                                    .Select(c => new
                                    {
                                        c.ClassroomPK,
                                        IdAndName = "(" + c.ProgramSpecificID + ") " + c.Name
                                    })
                                    .ToList();
                ddClassroom.DataSource = allClassrooms;
                ddClassroom.DataBind();

                //Get all the leave reasons
                var allLeaveReasons = context.CodeEmployeeLeaveReason.AsNoTracking()
                                        .OrderBy(ctlr => ctlr.OrderBy)
                                        .ToList();
                ddLeaveReason.DataSource = allLeaveReasons;
                ddLeaveReason.DataBind();

                //Get all the training types
                var allTrainingTypes = context.CodeTrainingAccess.Include(cta => cta.CodeTraining).AsNoTracking()
                                        .Where(cta => cta.StateFK == stateFK && cta.AllowedAccess == true)
                                        .Select(cta => cta.CodeTraining)
                                        .OrderBy(ct => ct.OrderBy)
                                        .ToList();

                //Limit the available trainings by the user's role
                var filteredTrainingTypes = allTrainingTypes.Where(att => att.RolesAuthorizedToModify.Split(',').ToList().Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString())).ToList();

                //Bind the dropdown
                ddTraining.DataSource = filteredTrainingTypes;
                ddTraining.DataBind();

                //Disable virtual item rendering because it causes issues with the dropdown not scrolling correctly
                ddTraining.EnableItemsVirtualRendering = DevExpress.Utils.DefaultBoolean.False;

                //Get all the job functions
                var allJobFunctions = context.CodeJobType.AsNoTracking()
                                        .OrderBy(cjt => cjt.OrderBy)
                                        .ToList();

                //Filter the job functions by the user's current role
                var filteredJobFunctions = allJobFunctions.Where(ajf => ajf.RolesAuthorizedToModify.Split(',').ToList().Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString())).ToList();

                //Bind the job type dropdown
                ddJobType.DataSource = filteredJobFunctions;
                ddJobType.DataBind();

                //Bind the classroom job type dropdown
                ddClassroomJobType.DataSource = allJobFunctions;
                ddClassroomJobType.DataBind();

                //Get all the termination reasons
                var allTerminationReasons = context.CodeTermReason.AsNoTracking()
                                            .OrderBy(ctr => ctr.OrderBy)
                                            .ToList();
                ddTerminationReason.DataSource = allTerminationReasons;
                ddTerminationReason.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitEmployee user control 
        /// </summary>
        /// <param name="sender">The submitEmployee control</param>
        /// <param name="e">The Click event</param>
        protected void submitEmployee_Click(object sender, EventArgs e)
        {
            //Determine if there are duplicate professionals in the program if the user is allowed to view private info
            //(this only happens after validation, so no need to worry about missing data)
            int numDuplicateProfessionals = 0;

            if (currentProgramRole.ViewPrivateEmployeeInfo.Value)
            {
                //Get the info from the form
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                DateTime hireDate = deHireDate.Date;
                int programFK = Convert.ToInt32(ddProgram.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    numDuplicateProfessionals = context.ProgramEmployee.Include(pe => pe.Employee).AsNoTracking()
                                                        .Where(pe => pe.Employee.FirstName == firstName &&
                                                                     pe.Employee.LastName == lastName &&
                                                                     pe.HireDate == hireDate &&
                                                                     pe.ProgramFK == programFK)
                                                        .Count();
                }
            }

            //Determine if the warning should be displayed
            if (numDuplicateProfessionals > 0 && hfDuplicateNameWarned.Value != null && Convert.ToBoolean(hfDuplicateNameWarned.Value) == false)
            {
                //Focus on the first name field
                txtFirstName.Focus();

                //Show a warning message
                msgSys.ShowMessageToUser("warning", "Duplicate Name Detected", "The first name, last name, and start date match another professional in this program, please check to ensure that this is not a duplicate.<br/><br/>This is just a warning, you can save the information by clicking the save button again.", 45000);

                //Indicate that the warning was shown
                hfDuplicateNameWarned.Value = "True";
            }
            else 
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
                        Response.Redirect(string.Format("/Pages/ProgramEmployeeDashboard.aspx?messageType={0}", successMessageType));
                    }
                    else
                    {
                        //Redirect the user back to this page with a message and the PK
                        Response.Redirect(string.Format("/Pages/ProgramEmployee.aspx?ProgramEmployeePK={0}&Action=Edit&messageType={1}",
                                                            programEmployeePK, successMessageType));
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitEmployee user control 
        /// </summary>
        /// <param name="sender">The submitEmployee control</param>
        /// <param name="e">The Click event</param>
        protected void submitEmployee_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the Employee Dashboard
            Response.Redirect(String.Format("/Pages/ProgramEmployeeDashboard.aspx?messageType={0}", "EmployeeCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitEmployee control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitEmployee_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitEmployee.ValidationGroup))
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
                    Reports.PreBuiltReports.FormReports.RptProgramEmployee report = new Reports.PreBuiltReports.FormReports.RptProgramEmployee();

                    //Show/hide the ASPIRE info
                    report.ParamShowAspireInfo.Value = (stateFK == (int)Utilities.StateFKs.NEW_YORK ? true : false);

                    //Set display of the program assignments section
                    report.ProgramAssignmentsDetailReport.Visible = currentPEPAPermissions.AllowedToView;

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Professional Information", programEmployeePK);
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
                    Response.Redirect(string.Format("/Pages/ProgramEmployee.aspx?ProgramEmployeePK={0}&Action={1}&messageType={2}&Print=True",
                                                        programEmployeePK, action, successMessageType));
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

            //To hold whether or not to import trainings
            bool importAspireTrainings = false;

            //Whether or not to remove ASPIRE trainings (need to remove them if an account becomes un-verified)
            bool deleteAspireTrainings = false;

            if ((isEdit && currentPEPermissions.AllowedToEdit) || (isEdit == false && currentPEPermissions.AllowedToAdd))
            {
                //To hold the program specific ID
                string programID;

                //To hold the employee PK
                int employeePK;

                //Get the program specific ID
                if(string.IsNullOrWhiteSpace(txtProgramID.Text))
                {
                    //The user didn't specify an ID, generate one
                    //Check to see if this is an edit
                    if(isEdit)
                    {
                        //This is an edit, use the current PK
                        programID = string.Format("SID-{0}", currentProgramEmployee.ProgramEmployeePK);
                    }
                    else
                    {
                        //To hold the previous ProgramEmployeePK
                        int previousPK;

                        //Get the previous ProgramEmployeePK
                        using(PyramidContext context = new PyramidContext())
                        {
                            //Get the previous PK
                            Models.ProgramEmployee programEmployee = context.ProgramEmployee
                                                                                .AsNoTracking()
                                                                                .OrderByDescending(pe => pe.ProgramEmployeePK)
                                                                                .FirstOrDefault();
                            previousPK = (programEmployee != null ? programEmployee.ProgramEmployeePK : 0);

                            //Set the program specific ID to the previous PK plus one
                            programID = string.Format("SID-{0}", (previousPK + 1));
                        }
                    }
                }
                else
                {
                    //Use the ID that the user provided
                    //Make sure to trim the input to ensure leading and trailing spaces are removed
                    programID = txtProgramID.Text.Trim();
                }

                //Fill the field values from the form
                if(currentProgramRole.ViewPrivateEmployeeInfo.Value)
                {
                    currentProgramEmployee.Employee.FirstName = txtFirstName.Text.Trim();
                    currentProgramEmployee.Employee.LastName = txtLastName.Text.Trim();
                    currentProgramEmployee.Employee.EmailAddress = txtEmail.Text.Trim();
                }
                currentProgramEmployee.IsEmployeeOfProgram = Convert.ToBoolean(ddIsEmployeeOfProgram.Value);
                currentProgramEmployee.Employee.GenderCodeFK = Convert.ToInt32(ddGender.Value);
                currentProgramEmployee.Employee.GenderSpecify = (string.IsNullOrWhiteSpace(txtGenderSpecify.Text) ? null : txtGenderSpecify.Text);
                currentProgramEmployee.Employee.EthnicityCodeFK = Convert.ToInt32(ddEthnicity.Value);
                currentProgramEmployee.Employee.EthnicitySpecify = (string.IsNullOrWhiteSpace(txtEthnicitySpecify.Text) ? null : txtEthnicitySpecify.Text);
                currentProgramEmployee.Employee.RaceCodeFK = Convert.ToInt32(ddRace.Value);
                currentProgramEmployee.Employee.RaceSpecify = (string.IsNullOrWhiteSpace(txtRaceSpecify.Text) ? null : txtRaceSpecify.Text);
                currentProgramEmployee.ProgramSpecificID = programID;
                currentProgramEmployee.HireDate = Convert.ToDateTime(deHireDate.Value);
                currentProgramEmployee.TermDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
                currentProgramEmployee.TermReasonCodeFK = (ddTerminationReason.Value == null ? (int?)null : Convert.ToInt32(ddTerminationReason.Value));
                currentProgramEmployee.TermReasonSpecify = (txtTerminationReasonSpecify.Value == null ? null : txtTerminationReasonSpecify.Value.ToString());

                //NY-only ASPIRE information
                if (stateFK == (int)Utilities.StateFKs.NEW_YORK)
                {
                    //Get the previous ASPIRE information
                    int? previousAspireID = currentProgramEmployee.Employee.AspireID;
                    string previousAspireEmail = currentProgramEmployee.Employee.AspireEmail;

                    //This is a NY employee, set the ASPIRE fields
                    currentProgramEmployee.Employee.AspireID = (string.IsNullOrWhiteSpace(txtAspireID.Text) ? (int?)null : Convert.ToInt32(txtAspireID.Text));
                    currentProgramEmployee.Employee.AspireEmail = (string.IsNullOrWhiteSpace(txtAspireEmail.Text) ? null : txtAspireEmail.Text.Trim());

                    //Set the verified bit if the ASPIRE fields are filled out and the employee is not already verified
                    if(currentProgramEmployee.Employee.AspireID.HasValue && 
                        !string.IsNullOrWhiteSpace(currentProgramEmployee.Employee.AspireEmail))
                    {
                        //Check to see if the employee is already verified
                        if(currentProgramEmployee.Employee.AspireVerified == false)
                        {
                            //Verify the employee
                            currentProgramEmployee.Employee.AspireVerified = true;

                            //Get the trainings for this employee
                            importAspireTrainings = true;
                        }
                        else if (previousAspireID != currentProgramEmployee.Employee.AspireID)
                        {
                            //The ASPIRE ID changed, delete and re-import
                            deleteAspireTrainings = true;
                            importAspireTrainings = true;
                        }
                    }
                    else
                    {
                        //ASPIRE fields not filled, therefore the employee is not verified
                        currentProgramEmployee.Employee.AspireVerified = false;

                        //Need to delete any ASPIRE trainings for this employee
                        deleteAspireTrainings = true;
                    }
                }
                else
                {
                    //This is not a NY employee, make sure ASPIRE fields are cleared
                    currentProgramEmployee.Employee.AspireID = null;
                    currentProgramEmployee.Employee.AspireEmail = null;
                    currentProgramEmployee.Employee.AspireVerified = false;
                }

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "EmployeeEdited";

                        //Set the fields
                        currentProgramEmployee.EditDate = DateTime.Now;
                        currentProgramEmployee.Editor = User.Identity.Name;
                        currentProgramEmployee.Employee.EditDate = DateTime.Now;
                        currentProgramEmployee.Employee.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.ProgramEmployee existingProgramEmployee = context.ProgramEmployee.Find(currentProgramEmployee.ProgramEmployeePK);

                        //Set the employee objects to the new values
                        context.Entry(existingProgramEmployee).CurrentValues.SetValues(currentProgramEmployee);
                        context.Entry(existingProgramEmployee.Employee).CurrentValues.SetValues(currentProgramEmployee.Employee);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfProgramEmployeePK.Value = currentProgramEmployee.ProgramEmployeePK.ToString();
                        programEmployeePK = currentProgramEmployee.ProgramEmployeePK;
                        employeePK = currentProgramEmployee.Employee.EmployeePK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "EmployeeAdded";

                        //Set the field values
                        currentProgramEmployee.CreateDate = DateTime.Now;
                        currentProgramEmployee.Creator = User.Identity.Name;
                        currentProgramEmployee.Employee.CreateDate = DateTime.Now;
                        currentProgramEmployee.Employee.Creator = User.Identity.Name;
                        currentProgramEmployee.ProgramFK = Convert.ToInt32(ddProgram.Value);

                        //Add it to the context
                        context.ProgramEmployee.Add(currentProgramEmployee);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfProgramEmployeePK.Value = currentProgramEmployee.ProgramEmployeePK.ToString();
                        programEmployeePK = currentProgramEmployee.ProgramEmployeePK;
                        employeePK = currentProgramEmployee.Employee.EmployeePK;
                    }
                }

                //Delete any ASPIRE trainings if necessary
                //NOTE: Must do this BEFORE importing new trainings.  This is because if the user switches credentials, we must delete the old and import the new.
                if(deleteAspireTrainings)
                {
                    using(PyramidContext context = new PyramidContext())
                    {
                        //Get the ASPIRE trainings for this employee
                        List<Training> aspireTrainings = context.Training.Where(t => t.EmployeeFK == employeePK && t.IsAspireTraining == true).ToList();

                        //Remove the ASPIRE trainings for this employee
                        context.Training.RemoveRange(aspireTrainings);

                        //Save the deletions
                        context.SaveChanges();

                        //Get the PKs of the trainings deleted
                        List<int> deletedPKs = aspireTrainings.Select(t => t.TrainingPK).ToList();

                        //Get the delete change rows and set the deleter
                        List<TrainingChanged> changeRows = context.TrainingChanged.Where(tc => deletedPKs.Contains(tc.TrainingPK)).ToList();

                        //Update the change rows
                        changeRows.ForEach(tc => tc.Deleter = User.Identity.Name);

                        //Save the delete change row to the database
                        context.SaveChanges();
                    }
                }

                //Once the employee details are saved to the database, check to see if importing trainings
                if (importAspireTrainings)
                {
                    //Move the trainings from the AspireTraining table to the regular Training table
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Import the trainings
                        context.spImportAspireTrainings(currentProgramEmployee.Employee.AspireID);

                        //Import the reliabilities
                        context.spImportAspireReliabilityRecords(currentProgramEmployee.Employee.AspireID);
                    }
                }
            }
            else if(showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            //Return the success message type
            return successMessageType;
        }

        #region Custom Employee Validation

        /// <summary>
        /// This method fires when the validation for the txtProgramID DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtProgramID TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtProgramID_Validation(object sender, ValidationEventArgs e)
        {
            //Get the program ID
            string programID = txtProgramID.Text;

            //Perform validation
            if (!string.IsNullOrWhiteSpace(programID))
            {
                //To hold all the other employees with a matching program ID
                List<Models.ProgramEmployee> matchingEmployees = new List<Models.ProgramEmployee>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all other employees in this program with a matching program ID
                    //Make sure to trim the input
                    matchingEmployees = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == currentProgramFK &&
                                                                pe.ProgramEmployeePK != programEmployeePK &&
                                                                pe.ProgramSpecificID.ToLower().Trim() == programID.ToLower().Trim()).ToList();
                }

                //Check to see if another employee already has this ID
                if(matchingEmployees.Count > 0)
                {
                    //Invalid
                    e.IsValid = false;
                    e.ErrorText = "That ID Number is already taken!";
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
            //Get the specify text
            string genderSpecify = txtGenderSpecify.Text;

            //Perform validation
            if (ddGender.SelectedItem != null && ddGender.SelectedItem.Text.ToLower() == "other" && string.IsNullOrWhiteSpace(genderSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Gender is required when the 'Other' gender is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtEthnicitySpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtEthnicitySpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtEthnicitySpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specify text
            string ethnicitySpecify = txtEthnicitySpecify.Text;

            //Perform validation
            if (ddEthnicity.SelectedItem != null && ddEthnicity.SelectedItem.Text.ToLower() == "other" && string.IsNullOrWhiteSpace(ethnicitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Ethnicity is required when the 'Other' ethnicity is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtRaceSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtRaceSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtRaceSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specify text
            string raceSpecify = txtRaceSpecify.Text;

            //Perform validation
            if (ddRace.SelectedItem != null && ddRace.SelectedItem.Text.ToLower() == "other" && string.IsNullOrWhiteSpace(raceSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Race is required when the 'Other' race is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtEmail DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtEmail TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtEmail_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if the user can edit the email
            if (currentProgramRole.ViewPrivateEmployeeInfo.Value)
            {
                //Get the email address
                string email = txtEmail.Text;

                //Perform validation
                if (string.IsNullOrWhiteSpace(email))
                {
                    //Make sure an email is entered
                    e.IsValid = false;
                    e.ErrorText = "Email is required!";
                }
                else if(!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    //Make sure the email is in the correct format
                    e.IsValid = false;
                    e.ErrorText = "Must be a valid email address format!";
                }
                else
                {
                    //Make sure the email is not already taken by an exising employee in this program

                    //To hold the list of other employees with the same email
                    List<Models.ProgramEmployee> matchingEmployees = new List<Models.ProgramEmployee>();

                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get all the other employees with the same email in the same program
                        matchingEmployees = context.ProgramEmployee.AsNoTracking()
                                            .Where(pe => pe.ProgramFK == currentProgramFK &&
                                                         pe.ProgramEmployeePK != programEmployeePK &&
                                                         pe.Employee.EmailAddress.ToLower().Trim() == email.ToLower().Trim()).ToList();
                    }

                    //Check to see if any other employees have the same email
                    if (matchingEmployees.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "That email is already taken!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtAspireEmail DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtAspireEmail TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtAspireEmail_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if this is a NY employee
            if (stateFK == (int)Utilities.StateFKs.NEW_YORK)
            {
                //To hold the used ASPIRE email addresses
                List<string> usedAspireEmails = new List<string>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the used ASPIRE email addresses
                    usedAspireEmails = context.ProgramEmployee.AsNoTracking()
                                    .Where(pe => pe.ProgramEmployeePK != currentProgramEmployee.ProgramEmployeePK
                                            && pe.ProgramFK == currentProgramEmployee.ProgramFK)
                                    .Select(pe => pe.Employee.AspireEmail.ToLower().Trim())
                                    .ToList();
                }

                //Get the ASPIRE email address and ASPIRE ID
                string email = txtAspireEmail.Text;
                int aspireID;

                //Perform validation
                if (string.IsNullOrWhiteSpace(email) && int.TryParse(txtAspireID.Text, out aspireID))
                {
                    e.IsValid = false;
                    e.ErrorText = "ASPIRE Email Address is required if an ASPIRE ID is entered!";
                }
                else if (!string.IsNullOrWhiteSpace(email) && usedAspireEmails.Contains(email.ToLower().Trim()))
                {
                    e.IsValid = false;
                    e.ErrorText = "That ASPIRE Email is already taken!";
                }
                else if (!string.IsNullOrWhiteSpace(email) && int.TryParse(txtAspireID.Text, out aspireID) && 
                    !Aspire.AspireAPI.FunctionCalls.IsAspireAccountValid(User.Identity.Name, email, Convert.ToInt32(txtAspireID.Text)))
                {
                    e.IsValid = false;
                    e.ErrorText = "No ASPIRE account with that combination of email and ID exists!";
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtAspireID DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtAspireID TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtAspireID_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if this is a NY employee
            if (stateFK == (int)Utilities.StateFKs.NEW_YORK)
            {
                //To hold the used ASPIRE IDs
                List<int?> usedAspireIDs = new List<int?>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the used ASPIRE IDs
                    usedAspireIDs = context.ProgramEmployee.AsNoTracking()
                                    .Where(pe => pe.ProgramEmployeePK != currentProgramEmployee.ProgramEmployeePK
                                            && pe.ProgramFK == currentProgramEmployee.ProgramFK)
                                    .Select(pe => pe.Employee.AspireID)
                                    .ToList();
                }

                //Get the ASPIRE email address and ASPIRE ID
                string email = (txtAspireEmail.Value == null ? null : txtAspireEmail.Value.ToString());
                int aspireID;

                //Perform validation
                if(!string.IsNullOrWhiteSpace(txtAspireID.Text) && !int.TryParse(txtAspireID.Text, out aspireID))
                {
                    e.IsValid = false;
                    e.ErrorText = "ASPIRE ID must be a valid number!";
                }
                else if (!int.TryParse(txtAspireID.Text, out aspireID) && !string.IsNullOrWhiteSpace(email))
                {
                    e.IsValid = false;
                    e.ErrorText = "ASPIRE ID is required if an ASPIRE Email is entered!";
                }
                else if (int.TryParse(txtAspireID.Text, out aspireID) && usedAspireIDs.Contains(aspireID))
                {
                    e.IsValid = false;
                    e.ErrorText = "That ASPIRE ID is already taken!";
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the deHireDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deHireDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deHireDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the professional's start date, enrollment date, and hire reason
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));

            //Perform the validation
            if (hireDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date is required!";
            }
            else if (termDate.HasValue && hireDate.Value >= termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be before the separation date!";
            }
            else if (hireDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date cannot be in the future!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the professional's start date against other forms' dates
                    var formValidationResults = context.spValidateHireTermDates(currentProgramEmployee.ProgramEmployeePK,
                                                    currentProgramFK, hireDate, (DateTime?)null).ToList();

                    //If there are results, the professional's start date is invalid
                    if (formValidationResults.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Start Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "The Start Date would invalidate these records if changed to that date:<br/><br/>";
                        foreach (spValidateHireTermDates_Result invalidForm in formValidationResults)
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
        /// This method fires when the validation for the deTerminationDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deTerminationDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deTerminationDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the termination date, enrollment date, and termination reason
            DateTime? terminationDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            string terminationReason = (ddTerminationReason.Value == null ? null : ddTerminationReason.Value.ToString());

            //Perform the validation
            if (terminationDate.HasValue == false && terminationReason != null)
            {
                e.IsValid = false;
                e.ErrorText = "Separation Date is required if you have a separation reason!";
            }
            else if (hireDate.HasValue == false && terminationDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date must be entered before the separation date!";
            }
            else if (terminationDate.HasValue && terminationDate.Value < hireDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Separation Date must be after the start date!";
            }
            else if (terminationDate.HasValue && terminationDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Separation Date cannot be in the future!";
            }
            else if (terminationDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the termination date against other forms' dates
                    var formValidationResults = context.spValidateHireTermDates(currentProgramEmployee.ProgramEmployeePK,
                                                    currentProgramFK, (DateTime?)null, terminationDate).ToList();

                    //If there are results, the termination date is invalid
                    if (formValidationResults.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Separation Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "The Separation Date would invalidate these records if changed to that date:<br/><br/>";
                        foreach (spValidateHireTermDates_Result invalidForm in formValidationResults)
                        {
                            message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                            message += "<br/>";
                        }

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Separation Date Validation Error", message, 200000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddTerminationReason DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddTerminationReason ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddTerminationReason_Validation(object sender, ValidationEventArgs e)
        {
            //Get the termination date and reason
            DateTime? terminationDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
            string terminationReason = (ddTerminationReason.Value == null ? null : ddTerminationReason.Value.ToString());

            //Perform validation
            if (terminationDate.HasValue == false && terminationReason != null)
            {
                e.IsValid = false;
                e.ErrorText = "Separation Reason is required if you have a Separation Date!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtTerminationReasonSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTerminationReasonSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTerminationReasonSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified termination reason
            string terminationReasonSpecify = (txtTerminationReasonSpecify.Value == null ? null : txtTerminationReasonSpecify.Value.ToString());

            //Perform validation
            if (ddTerminationReason.SelectedItem != null && ddTerminationReason.SelectedItem.Text.ToLower() == "other" && String.IsNullOrWhiteSpace(terminationReasonSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Separation Reason is required when the 'Other' separation reason is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for a specified date edit
        /// fires and it validates that control's value against the professional's start date
        /// and termination date
        /// </summary>
        /// <param name="sender">A DevExpress DateEdit control</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void CheckBetweenHireAndTermAndRequired_Validation(object sender, ValidationEventArgs e)
        {
            //Get the date to check
            DateTime? dateToCheck = (e.Value == null ? (DateTime?)null : Convert.ToDateTime(e.Value));

            //Get the professional's start date and separation date
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));

            //Perform validation
            if (dateToCheck.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "This date is required!";
            }
            else if (termDate.HasValue == false && (dateToCheck < hireDate || dateToCheck > DateTime.Now))
            {
                e.IsValid = false;
                e.ErrorText = "This date must be between the professional's start date and now!";
            }
            else if (termDate.HasValue && (dateToCheck < hireDate || dateToCheck > termDate))
            {
                e.IsValid = false;
                e.ErrorText = "This date must be between the professional's start date and separation date!";
            }
        }

        #endregion

        #region  Trainings

        /// <summary>
        /// This method populates the training repeater with up-to-date information
        /// </summary>
        private void BindTrainings()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var allTrainings = context.Training.AsNoTracking()
                                 .Where(t => t.EmployeeFK == currentProgramEmployee.Employee.EmployeePK)
                                 .OrderBy(t => t.TrainingDate)
                                 .ToList();
                repeatTrainings.DataSource = allTrainings;
                repeatTrainings.DataBind();

             
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the trainings
        /// and it opens a div that allows the user to add a training
        /// </summary>
        /// <param name="sender">The lbAddTraining LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTraining_Click(object sender, EventArgs e)
        {
            //Clear inputs in the training div
            hfAddEditTrainingPK.Value = "0";
            ddTraining.Value = "";
            deTrainingDate.Value = "";

            //Set the title
            lblAddEditTraining.Text = "Add Training";

            //Show the training div
            divAddEditTraining.Visible = true;

            //Set focus to the training date field
            deTrainingDate.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a training
        /// and it opens the training edit div so that the user can edit the selected training
        /// </summary>
        /// <param name="sender">The lbEditTraining LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTraining_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblTrainingPK = (Label)item.FindControl("lblTrainingPK");

            //Get the PK from the label
            int? trainingPK = (String.IsNullOrWhiteSpace(lblTrainingPK.Text) ? (int?)null : Convert.ToInt32(lblTrainingPK.Text));

            if (trainingPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the training to edit
                    Training editTraining = context.Training.Include(t => t.CodeTraining).AsNoTracking()
                                                            .Where(cn => cn.TrainingPK == trainingPK.Value).FirstOrDefault();

                    //Get the authorized roles
                    List<string> authorizedRoles = editTraining.CodeTraining.RolesAuthorizedToModify.Split(',').ToList();

                    //Check to see if this is an ASPIRE training
                    bool isAspireTraining;
                    if (stateFK == (int)Utilities.StateFKs.NEW_YORK && editTraining.IsAspireTraining == true)
                    {
                        isAspireTraining = true;
                    }
                    else
                    {
                        isAspireTraining = false;
                    }

                    //Make sure the user is allowed to edit the training
                    if(authorizedRoles.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()) && isAspireTraining == false) 
                    {
                        //Fill the inputs
                        lblAddEditTraining.Text = "Edit Training";
                        ddTraining.SelectedItem = ddTraining.Items.FindByValue(editTraining.TrainingCodeFK);
                        deTrainingDate.Value = editTraining.TrainingDate.ToString("MM/dd/yyyy");
                        hfAddEditTrainingPK.Value = trainingPK.Value.ToString();

                        //Show the training div
                        divAddEditTraining.Visible = true;

                        //Set focus to the training date field
                        deTrainingDate.Focus();
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to edit this training!", 10000);
                    }
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected training!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the trainings
        /// and it closes the training add/edit div
        /// </summary>
        /// <param name="sender">The submitTraining submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTraining_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditTrainingPK.Value = "0";
            divAddEditTraining.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitTraining control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTraining_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the  trainings
        /// and it saves the training information to the database
        /// </summary>
        /// <param name="sender">The submitTraining submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTraining_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit employee information
            if (currentPEPermissions.AllowedToEdit)
            {
                //Get the training pk
                int trainingPK = Convert.ToInt32(hfAddEditTrainingPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    Training currentTraining;
                    //Check to see if this is an add or an edit
                    if (trainingPK == 0)
                    {
                        //Add
                        currentTraining = new Training();
                        currentTraining.TrainingDate = Convert.ToDateTime(deTrainingDate.Value);
                        currentTraining.TrainingCodeFK = Convert.ToInt32(ddTraining.Value);
                        currentTraining.EmployeeFK = currentProgramEmployee.Employee.EmployeePK;
                        currentTraining.CreateDate = DateTime.Now;
                        currentTraining.Creator = User.Identity.Name;

                        //Save to the database
                        context.Training.Add(currentTraining);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added training!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentTraining = context.Training.Find(trainingPK);
                        currentTraining.TrainingDate = Convert.ToDateTime(deTrainingDate.Value);
                        currentTraining.TrainingCodeFK = Convert.ToInt32(ddTraining.Value);
                        currentTraining.EmployeeFK = currentProgramEmployee.Employee.EmployeePK;
                        currentTraining.EditDate = DateTime.Now;
                        currentTraining.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited training!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditTrainingPK.Value = "0";
                    divAddEditTraining.Visible = false;

                    //Rebind the training gridview
                    BindTrainings();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a training
        /// and it deletes the training information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteTraining LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTraining_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit employee information
            if (currentPEPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteTrainingPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTrainingPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the training to remove
                            Training trainingToRemove = context.Training.Include(t => t.CodeTraining)
                                                                        .Where(cn => cn.TrainingPK == rowToRemovePK).FirstOrDefault();

                            //Get the roles authorized to delete
                            List<string> rolesAuthorized = trainingToRemove.CodeTraining.RolesAuthorizedToModify.Split(',').ToList();

                            //Check to see if this is an ASPIRE training
                            bool isAspireTraining;
                            if (stateFK == (int)Utilities.StateFKs.NEW_YORK && trainingToRemove.IsAspireTraining == true)
                            {
                                isAspireTraining = true;
                            }
                            else
                            {
                                isAspireTraining = false;
                            }

                            //Make sure the user is allowed to delete the training
                            if (rolesAuthorized.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()) && isAspireTraining == false)
                            {
                                //Remove the training
                                context.Training.Remove(trainingToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.TrainingChanged
                                        .OrderByDescending(tc => tc.TrainingChangedPK)
                                        .Where(tc => tc.TrainingPK == trainingToRemove.TrainingPK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Rebind the training repeater
                                BindTrainings();

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted training!", 10000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Delete Failed", "You are not authorized to delete this training!", 10000);
                            }
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the training, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the training!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the training!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the training to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
        #endregion

        #region  Job Functions

        /// <summary>
        /// This method populates the job function repeater with up-to-date information
        /// </summary>
        private void BindJobFunctions()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var allJobFunctions = context.JobFunction.AsNoTracking()
                                 .Where(jf => jf.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK)
                                 .OrderBy(jf => jf.StartDate)
                                 .ToList();
                repeatJobFunctions.DataSource = allJobFunctions;
                repeatJobFunctions.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the job functions
        /// and it opens a div that allows the user to add a job function
        /// </summary>
        /// <param name="sender">The lbAddJobFunction LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddJobFunction_Click(object sender, EventArgs e)
        {
            //Clear inputs in the jobFunction div
            hfAddEditJobFunctionPK.Value = "0";
            ddJobType.Value = "";
            deJobStartDate.Value = "";
            deJobEndDate.Value = "";

            //Set the title
            lblAddEditJobFunction.Text = "Add Job Function";

            //Show the jobFunction div
            divAddEditJobFunction.Visible = true;

            //Set focus to the job type field
            ddJobType.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a job function
        /// and it opens the job function edit div so that the user can edit the selected job function
        /// </summary>
        /// <param name="sender">The lbEditJobFunction LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditJobFunction_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblJobFunctionPK = (Label)item.FindControl("lblJobFunctionPK");

            //Get the PK from the label
            int? jobFunctionPK = (String.IsNullOrWhiteSpace(lblJobFunctionPK.Text) ? (int?)null : Convert.ToInt32(lblJobFunctionPK.Text));

            if (jobFunctionPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the jobFunction to edit
                    JobFunction editJobFunction = context.JobFunction.Include(jf => jf.CodeJobType).AsNoTracking()
                                                                     .Where(cn => cn.JobFunctionPK == jobFunctionPK.Value).FirstOrDefault();

                    //Get the roles authorized to edit
                    List<string> authorizedRoles = editJobFunction.CodeJobType.RolesAuthorizedToModify.Split(',').ToList();

                    //Make sure the user is allowed to edit
                    if (authorizedRoles.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()))
                    {
                        //Fill the inputs
                        lblAddEditJobFunction.Text = "Edit Job Function";
                        ddJobType.SelectedItem = ddJobType.Items.FindByValue(editJobFunction.JobTypeCodeFK);
                        deJobStartDate.Value = editJobFunction.StartDate.ToString("MM/dd/yyyy");
                        deJobEndDate.Value = (editJobFunction.EndDate.HasValue ? editJobFunction.EndDate.Value.ToString("MM/dd/yyyy") : "");
                        hfAddEditJobFunctionPK.Value = jobFunctionPK.Value.ToString();

                        //Show the jobFunction div
                        divAddEditJobFunction.Visible = true;

                        //Set focus to the job type field
                        ddJobType.Focus();
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to edit this job function!", 10000);
                    }
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected job function!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the job functions
        /// and it closes the job function add/edit div
        /// </summary>
        /// <param name="sender">The submitJobFunction submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitJobFunction_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditJobFunctionPK.Value = "0";
            divAddEditJobFunction.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitJobFunction control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitJobFunction_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the  job functions
        /// and it saves the job function information to the database
        /// </summary>
        /// <param name="sender">The submitJobFunction submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitJobFunction_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit employee information
            if (currentPEPermissions.AllowedToEdit)
            {
                //Get the jobFunction pk
                int jobFunctionPK = Convert.ToInt32(hfAddEditJobFunctionPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    JobFunction currentJobFunction;
                    //Check to see if this is an add or an edit
                    if (jobFunctionPK == 0)
                    {
                        //Add
                        currentJobFunction = new JobFunction();
                        currentJobFunction.StartDate = Convert.ToDateTime(deJobStartDate.Value);
                        currentJobFunction.EndDate = (deJobEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deJobEndDate.Value));
                        currentJobFunction.JobTypeCodeFK = Convert.ToInt32(ddJobType.Value);
                        currentJobFunction.ProgramEmployeeFK = currentProgramEmployee.ProgramEmployeePK;
                        currentJobFunction.CreateDate = DateTime.Now;
                        currentJobFunction.Creator = User.Identity.Name;

                        //Save to the database
                        context.JobFunction.Add(currentJobFunction);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added job function!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentJobFunction = context.JobFunction.Find(jobFunctionPK);
                        currentJobFunction.StartDate = Convert.ToDateTime(deJobStartDate.Value);
                        currentJobFunction.EndDate = (deJobEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deJobEndDate.Value));
                        currentJobFunction.JobTypeCodeFK = Convert.ToInt32(ddJobType.Value);
                        currentJobFunction.ProgramEmployeeFK = currentProgramEmployee.ProgramEmployeePK;
                        currentJobFunction.EditDate = DateTime.Now;
                        currentJobFunction.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited job function!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditJobFunctionPK.Value = "0";
                    divAddEditJobFunction.Visible = false;

                    //Rebind the jobFunction gridview
                    BindJobFunctions();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a jobFunction
        /// and it deletes the jobFunction information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteJobFunction LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteJobFunction_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit employee information
            if (currentPEPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteJobFunctionPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteJobFunctionPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the jobFunction to remove
                            JobFunction jobFunctionToRemove = context.JobFunction.Include(jf => jf.CodeJobType)
                                                                                 .Where(cn => cn.JobFunctionPK == rowToRemovePK).FirstOrDefault();

                            //Get the authorized roles
                            List<string> authorizedRoles = jobFunctionToRemove.CodeJobType.RolesAuthorizedToModify.Split(',').ToList();

                            //Make sure the user is allowed to delete the job function
                            if (authorizedRoles.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()))
                            {
                                //Remove the jobFunction
                                context.JobFunction.Remove(jobFunctionToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.JobFunctionChanged
                                        .OrderByDescending(jfc => jfc.JobFunctionChangedPK)
                                        .Where(jfc => jfc.JobFunctionPK == jobFunctionToRemove.JobFunctionPK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Rebind the jobFunction repeater
                                BindJobFunctions();

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted job function!", 10000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Delete Failed", "You are not authorized to delete this job function!", 10000);
                            }
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the job function, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the job function!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the job function!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the job function to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the validation for the deJobStartDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deJobStartDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deJobStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
            DateTime? jobStartDate = (deJobStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deJobStartDate.Value));
            int? jobTypePK = (ddJobType.Value == null ? (int?)null : Convert.ToInt32(ddJobType.Value));

            //Get the job function pk
            int jobFunctionPK;
            int.TryParse(hfAddEditJobFunctionPK.Value, out jobFunctionPK);

            //Perform the validation
            if (jobStartDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date is required!";
            }
            else if (jobTypePK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "A Job Type needs to be chosen!";
            }
            else if (hireDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "The professional's start date must have a value before this is entered!";
            }
            else if(jobStartDate.Value < hireDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date cannot be before the professional's start date!";
            }
            else if(termDate.HasValue && jobStartDate.Value > termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date cannot be after the separation date!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing job function rows
                    var otherJobFunctions = context.JobFunction.AsNoTracking()
                                                .Where(jf => jf.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                        && jf.JobFunctionPK != jobFunctionPK
                                                        && jf.JobTypeCodeFK == jobTypePK).ToList();

                    foreach (JobFunction jobFunction in otherJobFunctions)
                    {
                        if (jobFunction.EndDate.HasValue == false && jobStartDate >= jobFunction.StartDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Start Date cannot be after the start date for an active job function with the same job type!";
                        }
                        else if (jobFunction.EndDate.HasValue && jobStartDate >= jobFunction.StartDate && jobStartDate <= jobFunction.EndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Start Date cannot fall between an existing range of dates for another job function with the same job type!";
                        }
                        else if (jobStartDate.HasValue && termDate.HasValue
                                    && (jobStartDate.Value > termDate.Value || jobStartDate.Value < hireDate.Value))
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date must be between the professional's start date and separation date!";
                        }
                        else if (jobStartDate.HasValue && termDate.HasValue == false
                            && (jobStartDate.Value > DateTime.Now || jobStartDate.Value < hireDate.Value))
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date must be between the professional's start date and now!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deJobEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deJobEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deJobEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
            DateTime? jobStartDate = (deJobStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deJobStartDate.Value));
            DateTime? jobEndDate = (deJobEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deJobEndDate.Value));
            int? jobTypePK = (ddJobType.Value == null ? (int?)null : Convert.ToInt32(ddJobType.Value));

            //Get the job function pk
            int jobFunctionPK;
            int.TryParse(hfAddEditJobFunctionPK.Value, out jobFunctionPK);

            //Perform the validation
            if (jobTypePK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "A Job Type needs to be chosen!";
            }
            else if (jobStartDate.HasValue == false && jobEndDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be entered before the Leave Date!";
            }
            else if (jobEndDate.HasValue && jobEndDate < jobStartDate)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be after the Assign Date!";
            }
            else if (termDate.HasValue && jobEndDate.HasValue && jobEndDate.Value > termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be after the separation date!";
            }
            else if (jobEndDate.HasValue && hireDate.HasValue
                        && jobEndDate.Value < hireDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date must be on or after the professional's start date!";
            }
            else if (jobEndDate.HasValue && jobEndDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be in the future!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing job function rows
                    var otherJobFunctions = context.JobFunction.AsNoTracking()
                                                .Where(jf => jf.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                        && jf.JobFunctionPK != jobFunctionPK
                                                        && jf.JobTypeCodeFK == jobTypePK).ToList();

                    foreach (JobFunction jobFunction in otherJobFunctions)
                    {
                        if (jobEndDate.HasValue == false)
                        {
                            if (jobStartDate.HasValue && jobStartDate.Value <= jobFunction.StartDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "End Date is required when adding an job function that starts before another job function with the same job type!";
                            }
                        }
                        else if (jobFunction.EndDate.HasValue == false && jobEndDate >= jobFunction.StartDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "End Date cannot be after the start date for an active job function with the same job type!";
                        }
                        else if (jobFunction.EndDate.HasValue && jobEndDate >= jobFunction.StartDate && jobEndDate <= jobFunction.EndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "End Date cannot fall between an existing range of dates for another job function with the same job type!";
                        }
                        else if (jobFunction.StartDate >= jobStartDate.Value && jobFunction.StartDate <= jobEndDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This job function cannot encapsulate another job function with the same job type!";
                        }
                    }
                }
            }
        }
        #endregion

        #region Classroom Assignments

        /// <summary>
        /// This method populates the classroom assignment repeater with up-to-date information
        /// </summary>
        private void BindClassroomAssignments()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                            .Include(ec => ec.Classroom)
                                            .Include(ec => ec.CodeEmployeeLeaveReason)
                                            .Include(ec => ec.CodeJobType)
                                            .Where(ec => ec.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                && ec.Classroom.ProgramFK == currentProgramFK)
                                            .OrderBy(ec => ec.AssignDate)    
                                            .ToList();
                repeatClassroomAssignments.DataSource = classroomAssignments;
                repeatClassroomAssignments.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the classroom
        /// assignments and it opens a div that allows the user to add a classroom assignment
        /// </summary>
        /// <param name="sender">The lbAddClassroomAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditClassroomAssignmentPK.Value = "0";
            deAssignDate.Value = "";
            ddClassroom.Value = "";
            ddClassroomJobType.Value = "";
            deLeaveDate.Value = "";
            ddLeaveReason.Value = "";
            txtLeaveReasonSpecify.Value = "";

            //Set the title
            lblAddEditClassroomAssignment.Text = "Add Classroom Assignment";

            //Show the input div
            divAddEditClassroomAssignment.Visible = true;

            //Set focus to the assign date field
            deAssignDate.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a classroom assignment
        /// and it opens the edit div so that the user can edit the selected classroom assignment
        /// </summary>
        /// <param name="sender">The lbEditClassroomAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblClassroomAssignmentPK = (Label)item.FindControl("lblClassroomAssignmentPK");

            //Get the PK from the label
            int? assignmentPK = (String.IsNullOrWhiteSpace(lblClassroomAssignmentPK.Text) ? (int?)null : Convert.ToInt32(lblClassroomAssignmentPK.Text));

            if (assignmentPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the classroom assignment to edit
                    EmployeeClassroom editClassroomAssignment = context.EmployeeClassroom.AsNoTracking().Where(cn => cn.EmployeeClassroomPK == assignmentPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditClassroomAssignment.Text = "Edit Classroom Assignment";
                    deAssignDate.Value = editClassroomAssignment.AssignDate.ToString("MM/dd/yyyy");
                    ddClassroom.SelectedItem = ddClassroom.Items.FindByValue(editClassroomAssignment.ClassroomFK);
                    ddClassroomJobType.SelectedItem = ddClassroomJobType.Items.FindByValue(editClassroomAssignment.JobTypeCodeFK);
                    deLeaveDate.Value = (editClassroomAssignment.LeaveDate.HasValue ? editClassroomAssignment.LeaveDate.Value.ToString("MM/dd/yyyy") : "");
                    ddLeaveReason.SelectedItem = ddLeaveReason.Items.FindByValue(editClassroomAssignment.LeaveReasonCodeFK);
                    txtLeaveReasonSpecify.Value = (editClassroomAssignment.LeaveReasonSpecify == null ? "" : editClassroomAssignment.LeaveReasonSpecify.ToString());
                    hfAddEditClassroomAssignmentPK.Value = assignmentPK.Value.ToString();
                }

                //Show the classroom assignment div
                divAddEditClassroomAssignment.Visible = true;

                //Set focus to the assign date field
                deAssignDate.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected classroom assignment!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the classroom assignment
        /// add/edit and it closes the classroom assignment add/edit div
        /// </summary>
        /// <param name="sender">The submitClassroomAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitClassroomAssignment_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditClassroomAssignmentPK.Value = "0";
            divAddEditClassroomAssignment.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitClassroomAssignment_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the classroom assignment
        /// add/edit and it saves the classroom assignment information to the database
        /// </summary>
        /// <param name="sender">The submitClassroomAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit employee information
            if (currentPEPermissions.AllowedToEdit)
            {
                //Get the classroom assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditClassroomAssignmentPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    EmployeeClassroom currentClassroomAssignment;
                    //Check to see if this is an add or an edit
                    if (assignmentPK == 0)
                    {
                        //Add
                        currentClassroomAssignment = new EmployeeClassroom();
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = Convert.ToInt32(ddClassroom.Value);
                        currentClassroomAssignment.JobTypeCodeFK = Convert.ToInt32(ddClassroomJobType.Value);
                        currentClassroomAssignment.LeaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtLeaveReasonSpecify.Value == null ? null : txtLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.ProgramEmployeeFK = currentProgramEmployee.ProgramEmployeePK;
                        currentClassroomAssignment.CreateDate = DateTime.Now;
                        currentClassroomAssignment.Creator = User.Identity.Name;

                        //Save to the database
                        context.EmployeeClassroom.Add(currentClassroomAssignment);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added classroom assignment!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentClassroomAssignment = context.EmployeeClassroom.Find(assignmentPK);
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = Convert.ToInt32(ddClassroom.Value);
                        currentClassroomAssignment.JobTypeCodeFK = Convert.ToInt32(ddClassroomJobType.Value);
                        currentClassroomAssignment.LeaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtLeaveReasonSpecify.Value == null ? null : txtLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.EditDate = DateTime.Now;
                        currentClassroomAssignment.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited classroom assignment!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditClassroomAssignmentPK.Value = "0";
                    divAddEditClassroomAssignment.Visible = false;

                    //Rebind the classroom assignment table
                    BindClassroomAssignments();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a classroom assignment
        /// and it deletes the classroom assignment information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteClassroomAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit employee information
            if (currentPEPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteClassroomAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteClassroomAssignmentPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the classroom assignment to remove
                            EmployeeClassroom assignmentToRemove = context.EmployeeClassroom.Where(cn => cn.EmployeeClassroomPK == rowToRemovePK).FirstOrDefault();

                            //Remove the classroom assignment
                            context.EmployeeClassroom.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.EmployeeClassroomChanged
                                    .OrderByDescending(ecc => ecc.EmployeeClassroomChangedPK)
                                    .Where(ecc => ecc.EmployeeClassroomPK == assignmentToRemove.EmployeeClassroomPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the classroom assignment table
                            BindClassroomAssignments();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted classroom assignment!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the classroom assignment, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the classroom assignment!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the classroom assignment!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the classroom assignment to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddClassroomJobType DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddClassroomJobType ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddClassroomJobType_Validation(object sender, ValidationEventArgs e)
        {
            int? jobTypeFK = (ddClassroomJobType.Value == null ? (int?)null : Convert.ToInt32(ddClassroomJobType.Value));
            DateTime? assignDate = (deAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignDate.Value));
            List<JobFunction> activeJobFunctions = new List<JobFunction>();

            //Perform validation
            if (jobTypeFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Classroom Job is required!";
            }
            else if (assignDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Check to see if the employee has a valid job function during this time
                    activeJobFunctions = context.JobFunction.AsNoTracking()
                                                .Where(jf => jf.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                        && jf.StartDate <= assignDate.Value
                                                        && (jf.EndDate.HasValue == false || jf.EndDate.Value >= assignDate.Value)
                                                        && jf.JobTypeCodeFK == jobTypeFK.Value)
                                                .ToList();

                    //Make sure that the employee has a valid job function
                    if (activeJobFunctions.Count < 1)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Professional is not active in that job function as of the classroom assign date!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deAssignDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deAssignDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deAssignDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary dates and pks
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
            DateTime? assignDate = (deAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignDate.Value));
            int? classroomPK = (ddClassroom.Value == null ? (int?)null : Convert.ToInt32(ddClassroom.Value));
            int? jobTypeFK = (ddClassroomJobType.Value == null ? (int?)null : Convert.ToInt32(ddClassroomJobType.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditClassroomAssignmentPK.Value, out assignmentPK);

            //Perform the validation
            if (assignDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date is required!";
            }
            else if (classroomPK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "A Classroom needs to be chosen!";
            }
            else if (assignDate.HasValue && termDate.HasValue
                        && (assignDate.Value > termDate.Value || assignDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be between the professional's start date and separation date!";
            }
            else if (assignDate.HasValue && termDate.HasValue == false
                && (assignDate.Value > DateTime.Now || assignDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be between the professional's start date and now!";
            }
            else if(jobTypeFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                                .Include(tc => tc.Classroom)
                                                .Include(tc => tc.CodeEmployeeLeaveReason)
                                                .Where(tc => tc.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                        && tc.EmployeeClassroomPK != assignmentPK
                                                        && tc.ClassroomFK == classroomPK.Value
                                                        && tc.JobTypeCodeFK == jobTypeFK.Value)
                                                .ToList();

                    foreach (EmployeeClassroom assignment in classroomAssignments)
                    {
                        if (assignment.LeaveDate.HasValue == false && assignDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date cannot be after the assign date for an active assignment with the same classroom and classroom job!";
                        }
                        else if (assignment.LeaveDate.HasValue && assignDate >= assignment.AssignDate && assignDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date cannot fall between an existing range of dates for another assignment with the same classroom and classroom job!";
                        }
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
            //Get the necessary dates and pks
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
            DateTime? assignDate = (deAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            string leaveReason = (ddLeaveReason.Value == null ? null : ddLeaveReason.Value.ToString());
            int? classroomPK = (ddClassroom.Value == null ? (int?)null : Convert.ToInt32(ddClassroom.Value));
            int? jobTypeFK = (ddClassroomJobType.Value == null ? (int?)null : Convert.ToInt32(ddClassroomJobType.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditClassroomAssignmentPK.Value, out assignmentPK);

            //Perform the validation
            if (hireDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Professional's start date must be entered before this date!";
            }
            else if (leaveDate.HasValue == false && !String.IsNullOrWhiteSpace(leaveReason))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date is required if you have a Leave Reason!";
            }
            else if (assignDate.HasValue == false && leaveDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be entered before the Leave Date!";
            }
            else if (leaveDate.HasValue && leaveDate.Value < assignDate)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be after the Assign Date!";
            }
            else if (leaveDate.HasValue && termDate.HasValue
                        && leaveDate.Value > termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date cannot be after the separation date!";
            }
            else if (leaveDate.HasValue && hireDate.HasValue 
                        && leaveDate.Value < hireDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be on or after the professional's start date!";
            }
            else if (leaveDate.HasValue && leaveDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date cannot be in the future!";
            }
            else if (jobTypeFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                                .Include(tc => tc.Classroom)
                                                .Include(tc => tc.CodeEmployeeLeaveReason)
                                                .Where(tc => tc.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                        && tc.EmployeeClassroomPK != assignmentPK
                                                        && tc.ClassroomFK == classroomPK.Value
                                                        && tc.JobTypeCodeFK == jobTypeFK.Value)
                                                .ToList();

                    foreach (EmployeeClassroom assignment in classroomAssignments)
                    {
                        if (leaveDate.HasValue == false)
                        {
                            if (assignDate.HasValue && assignDate.Value <= assignment.AssignDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "Leave Date is required when adding an assignment that starts before another assignment with the same classroom and classroom job!";
                            }
                        }
                        else if (assignment.LeaveDate.HasValue == false && leaveDate.Value >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Leave Date cannot be after the assign date for an active assignment with the same classroom and classroom job!";
                        }
                        else if (assignment.LeaveDate.HasValue && leaveDate.Value >= assignment.AssignDate && leaveDate.Value <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Leave Date cannot fall between an existing range of dates for another assignment with the same classroom and classroom job!";
                        }
                        else if (assignment.AssignDate >= assignDate.Value && assignment.AssignDate <= leaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This classroom assignment cannot encapsulate another assignment with the same classroom and classroom job!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddLeaveReason DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddLeaveReason ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddLeaveReason_Validation(object sender, ValidationEventArgs e)
        {
            //Get the leave date and reason
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            string leaveReason = (ddLeaveReason.Value == null ? null : ddLeaveReason.Value.ToString());

            //Perform validation
            if (leaveDate.HasValue == false && leaveReason != null)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Reason is required if you have a Leave Date!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtLeaveReasonSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtLeaveReasonSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtLeaveReasonSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string leaveReasonSpecify = (txtLeaveReasonSpecify.Value == null ? null : txtLeaveReasonSpecify.Value.ToString());

            //Perform validation
            if (ddLeaveReason.SelectedItem != null && ddLeaveReason.SelectedItem.Text.ToLower() == "other" && String.IsNullOrWhiteSpace(leaveReasonSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Leave Reason is required when the 'Other' leave reason is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        #endregion

        #region Program Assignments

        /// <summary>
        /// This method populates the program assignment repeater with up-to-date information
        /// </summary>
        private void BindProgramAssignments()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var programAssignments = context.ProgramEmployee.AsNoTracking()
                                            .Include(pe => pe.Employee)
                                            .Include(pe => pe.Program)
                                            .Include(pe => pe.CodeTermReason)
                                            .Where(pe => pe.EmployeeFK == currentProgramEmployee.EmployeeFK &&
                                                         currentProgramRole.ProgramFKs.Contains(pe.ProgramFK))
                                            .ToList();

                repeatProgramAssignments.DataSource = programAssignments;
                repeatProgramAssignments.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the program
        /// assignments and it opens a div that allows the user to add a program assignment
        /// </summary>
        /// <param name="sender">The lbAddProgramAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddProgramAssignment_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditProgramAssignmentPK.Value = "0";
            ddPAProgram.Value = "";
            txtPAProgramID.Value = "";
            dePAHireDate.Value = "";
            ddPAIsEmployeeOfProgram.Value = "";

            //Set the title
            lblAddEditProgramAssignment.Text = "Add Program Assignment";

            //Show the input div
            divAddEditProgramAssignment.Visible = true;

            //Set focus to the first field
            ddPAProgram.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the program assignment
        /// add/edit and it closes the program assignment add/edit div
        /// </summary>
        /// <param name="sender">The submitProgramAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgramAssignment_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditProgramAssignmentPK.Value = "0";
            divAddEditProgramAssignment.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitProgramAssignment_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the program assignment
        /// add/edit and it saves the program assignment information to the database
        /// </summary>
        /// <param name="sender">The submitProgramAssignment submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgramAssignment_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit program assignments
            if (currentPEPAPermissions.AllowedToEdit)
            {
                //Get the program assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditProgramAssignmentPK.Value);

                //To hold the program specific ID
                string programID;

                //Get the program specific ID
                if (string.IsNullOrWhiteSpace(txtPAProgramID.Text))
                {
                    //The user didn't specify an ID, generate one
                    //To hold the previous ProgramEmployeePK
                    int previousPK;

                    //Get the previous ProgramEmployeePK
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the previous PK
                        previousPK = context.ProgramEmployee.Max(pe => pe.ProgramEmployeePK);

                        //Set the program specific ID to the previous PK plus one
                        programID = string.Format("SID-{0}", (previousPK + 1));
                    }
                }
                else
                {
                    //Use the ID that the user provided
                    //Make sure to trim the input to ensure leading and trailing spaces are removed
                    programID = txtProgramID.Text.Trim();
                }

                using (PyramidContext context = new PyramidContext())
                {
                    Models.ProgramEmployee currentProgramAssignment;
                    
                    //We only allow additions and deletions on this page
                    //Add
                    currentProgramAssignment = new Models.ProgramEmployee();
                    currentProgramAssignment.ProgramFK = Convert.ToInt32(ddPAProgram.Value);
                    currentProgramAssignment.EmployeeFK = currentProgramEmployee.EmployeeFK;
                    currentProgramAssignment.ProgramSpecificID = programID;
                    currentProgramAssignment.HireDate = Convert.ToDateTime(dePAHireDate.Value);
                    currentProgramAssignment.IsEmployeeOfProgram = Convert.ToBoolean(ddPAIsEmployeeOfProgram.Value);
                    currentProgramAssignment.CreateDate = DateTime.Now;
                    currentProgramAssignment.Creator = User.Identity.Name;

                    //Save to the database
                    context.ProgramEmployee.Add(currentProgramAssignment);
                    context.SaveChanges();

                    //Show a success message
                    msgSys.ShowMessageToUser("success", "Success", "Successfully added program assignment!", 10000);

                    //Reset the values in the hidden field and hide the div
                    hfAddEditProgramAssignmentPK.Value = "0";
                    divAddEditProgramAssignment.Visible = false;

                    //Rebind the program assignment table
                    BindProgramAssignments();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtPAProgramID DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPAProgramID TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtPAProgramID_Validation(object sender, ValidationEventArgs e)
        {
            //Get the program FK and professional's program ID
            int newProgramFK;
            string programID = txtPAProgramID.Text;

            //Perform validation
            if (ddPAProgram.Value != null && int.TryParse(ddPAProgram.Value.ToString(), out newProgramFK) && !string.IsNullOrWhiteSpace(programID))
            {
                //To hold all the other employees with a matching program ID
                List<Models.ProgramEmployee> matchingEmployees = new List<Models.ProgramEmployee>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all other employees in this program with a matching program ID
                    //Make sure to trim the input
                    matchingEmployees = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == newProgramFK &&
                                                                pe.EmployeeFK != currentProgramEmployee.EmployeeFK &&
                                                                pe.ProgramSpecificID.ToLower().Trim() == programID.ToLower().Trim()).ToList();
                }

                //Check to see if another employee already has this ID
                if (matchingEmployees.Count > 0)
                {
                    //Invalid
                    e.IsValid = false;
                    e.ErrorText = "That ID Number is already taken!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddPAProgram DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddPAProgram control</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddPAProgram_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the selected program FK
            int newProgramFK;

            //Perform validation
            if (ddPAProgram.Value != null && int.TryParse(ddPAProgram.Value.ToString(), out newProgramFK))
            {
                //To hold all the other ProgramEmployee records for this employee and program
                List<Models.ProgramEmployee> matchingPERecords = new List<Models.ProgramEmployee>();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all other ProgramEmployee records for this employee and program
                    matchingPERecords = context.ProgramEmployee.AsNoTracking()
                                                    .Where(pe => pe.ProgramFK == newProgramFK &&
                                                                pe.EmployeeFK == currentProgramEmployee.EmployeeFK).ToList();
                }

                //Check to see if another employee already has this ID
                if (matchingPERecords.Count > 0)
                {
                    //Invalid
                    e.IsValid = false;
                    e.ErrorText = "This professional already has a record in that program.  Please edit that record instead of adding a new one!";
                }
            }
        }

        #endregion

    }
}