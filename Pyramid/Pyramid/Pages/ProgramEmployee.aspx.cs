using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

namespace Pyramid.Pages
{
    public partial class ProgramEmployee : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.ProgramEmployee currentProgramEmployee;
        private int programFK;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the employee program PK
            int programEmployeePK = 0;

            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Try to get the employee pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ProgramEmployeePK"]))
            {
                //Parse the employee pk
                int.TryParse(Request.QueryString["ProgramEmployeePK"], out programEmployeePK);
            }

            //Don't allow aggregate viewers into this page
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                Response.Redirect("/Pages/ProgramEmployeeDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the employee program object
                currentProgramEmployee = context.ProgramEmployee.AsNoTracking()
                                        .Include(pe => pe.Program)
                                        .Where(pe => pe.ProgramEmployeePK == programEmployeePK).FirstOrDefault();

                //Check to see if the program employee exists
                if (currentProgramEmployee == null)
                {
                    //The employee doesn't exist, set the employee to a new employee object
                    currentProgramEmployee = new Models.ProgramEmployee();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentProgramEmployee.Program.ProgramName;
                }

                //Fill the used emails hidden field
                List<string> usedEmails = context.ProgramEmployee.AsNoTracking()
                                    .Where(pe => pe.ProgramEmployeePK != currentProgramEmployee.ProgramEmployeePK
                                            && pe.ProgramFK == currentProgramEmployee.ProgramFK)
                                    .Select(pe => pe.EmailAddress.ToLower())
                                    .ToList();
                hfUsedEmails.Value = string.Join(",", usedEmails);
            }

            //Don't allow users to view employee information from other programs
            if (currentProgramEmployee.ProgramEmployeePK > 0 && !currentProgramRole.ProgramFKs.Contains(currentProgramEmployee.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/EmployeeDashboard.aspx?messageType={0}", "NoEmployee"));
            }

            //Get the proper program fk
            programFK = (currentProgramEmployee.ProgramEmployeePK > 0 ? currentProgramEmployee.ProgramFK : currentProgramRole.CurrentProgramFK.Value);


            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!String.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "EmployeeAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Employee successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Show the edit only div if this is an edit
                divEditOnly.Visible = (programEmployeePK > 0 ? true : false);

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
                BindClassroomAssignments();
                BindTrainings();
                BindJobFunctions();

                //Bind the drop-downs
                BindDropDowns();

                //Fill the form with data
                FillFormWithDataFromObject();

                //Allow adding/editing depending on the user's role and the action
                if (currentProgramEmployee.ProgramEmployeePK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitEmployee.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New Employee";
                }
                else if (currentProgramEmployee.ProgramEmployeePK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitEmployee.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit Employee Information";
                }
                else
                {
                    //Hide the submit button
                    submitEmployee.ShowSubmitButton = false;

                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View Employee Information";
                }

                //Set the max date of the training date field to now
                deTrainingDate.MaxDate = DateTime.Now;

                //Set focus to the first name field
                txtFirstName.Focus();
            }
        }

        /// <summary>
        /// This method fills the input fields with data from the currentProgramEmployee
        /// object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Only continue if this is an edit
            if (currentProgramEmployee.ProgramEmployeePK > 0)
            {
                //Fill the input fields
                txtFirstName.Value = currentProgramEmployee.FirstName;
                txtLastName.Value = currentProgramEmployee.LastName;
                txtEmail.Value = currentProgramEmployee.EmailAddress;
                deHireDate.Value = currentProgramEmployee.HireDate.ToString("MM/dd/yyyy");
                deTerminationDate.Value = (currentProgramEmployee.TermDate.HasValue ? currentProgramEmployee.TermDate.Value.ToString("MM/dd/yyyy") : "");
                ddTerminationReason.SelectedItem = ddTerminationReason.Items.FindByValue(currentProgramEmployee.TermReasonCodeFK);
                txtTerminationReasonSpecify.Value = currentProgramEmployee.TermReasonSpecify;
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            txtFirstName.ClientEnabled = enabled;
            txtLastName.ClientEnabled = enabled;
            txtEmail.ClientEnabled = enabled;
            deHireDate.ClientEnabled = enabled;
            deTerminationDate.ClientEnabled = enabled;
            ddTerminationReason.ClientEnabled = enabled;
            txtTerminationReasonSpecify.ClientEnabled = enabled;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindDropDowns()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the classrooms
                var allClassrooms = context.Classroom.AsNoTracking()
                                    .Where(c => c.ProgramFK == programFK)
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
                var allTrainingTypes = context.CodeTraining.AsNoTracking()
                                        .Where(ct => ct.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                                        .OrderBy(ct => ct.OrderBy)
                                        .ToList();
                ddTraining.DataSource = allTrainingTypes;
                ddTraining.DataBind();

                //Get all the job functions
                var allJobFunctions = context.CodeJobType.AsNoTracking()
                                        .Where(cjt => cjt.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                                        .OrderBy(cjt => cjt.OrderBy)
                                        .ToList();

                //Bind the job type dropdown
                ddJobType.DataSource = allJobFunctions;
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
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //To hold the type of change
                string successMessageType = null;

                //Fill the field values from the form
                currentProgramEmployee.FirstName = txtFirstName.Value.ToString();
                currentProgramEmployee.LastName = txtLastName.Value.ToString();
                currentProgramEmployee.EmailAddress = txtEmail.Value.ToString().ToLower();
                currentProgramEmployee.AspireID = null;
                currentProgramEmployee.HireDate = Convert.ToDateTime(deHireDate.Value);
                currentProgramEmployee.TermDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));
                currentProgramEmployee.TermReasonCodeFK = (ddTerminationReason.Value == null ? (int?)null : Convert.ToInt32(ddTerminationReason.Value));
                currentProgramEmployee.TermReasonSpecify = (txtTerminationReasonSpecify.Value == null ? null : txtTerminationReasonSpecify.Value.ToString());

                if (currentProgramEmployee.ProgramEmployeePK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "EmployeeEdited";

                        //Set the fields
                        currentProgramEmployee.EditDate = DateTime.Now;
                        currentProgramEmployee.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.ProgramEmployee existingProgramEmployee = context.ProgramEmployee.Find(currentProgramEmployee.ProgramEmployeePK);

                        //Set the employee object to the new values
                        context.Entry(existingProgramEmployee).CurrentValues.SetValues(currentProgramEmployee);

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Redirect the user to the dashboard
                    Response.Redirect(string.Format("/Pages/ProgramEmployeeDashboard.aspx?messageType={0}", successMessageType));
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
                        currentProgramEmployee.ProgramFK = programFK;

                        //Add it to the context
                        context.ProgramEmployee.Add(currentProgramEmployee);

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/ProgramEmployee.aspx?ProgramEmployeePK={0}&Action=Edit&messageType={1}",
                                                        currentProgramEmployee.ProgramEmployeePK.ToString(), successMessageType));
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
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
        /// This method fires when the validation for the txtEmail DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtEmail TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtEmail_Validation(object sender, ValidationEventArgs e)
        {
            //Get the email address and the used email addresses
            string email = (txtEmail.Value == null ? null : txtEmail.Value.ToString());
            string[] emailArray = hfUsedEmails.Value.Split(',');

            //Perform validation
            if (String.IsNullOrWhiteSpace(email))
            {
                e.IsValid = false;
                e.ErrorText = "Email is required!";
            }
            else if (emailArray.Contains(email.ToLower()))
            {
                e.IsValid = false;
                e.ErrorText = "That email is already taken!";
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
            //Get the hire date, enrollment date, and hire reason
            DateTime? hireDate = (deHireDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deHireDate.Value));
            DateTime? termDate = (deTerminationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTerminationDate.Value));

            //Perform the validation
            if (hireDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Hire Date is required!";
            }
            else if (termDate.HasValue && hireDate.Value >= termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Hire Date must be before the termination date!";
            }
            else if (hireDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Hire Date cannot be in the future!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the hire date against other forms' dates
                    var formValidationResults = context.spValidateHireTermDates(currentProgramEmployee.ProgramEmployeePK,
                                                    programFK, hireDate, (DateTime?)null).ToList();

                    //If there are results, the hire date is invalid
                    if (formValidationResults.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Hire Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "The Hire Date would invalidate these records if changed to that date:<br/><br/>";
                        foreach (spValidateHireTermDates_Result invalidForm in formValidationResults)
                        {
                            message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                            message += "<br/>";
                        }

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Hire Date Validation Error", message, 200000);
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
                e.ErrorText = "Termination Date is required if you have a termination reason!";
            }
            else if (hireDate.HasValue == false && terminationDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Hire Date must be entered before the termination date!";
            }
            else if (terminationDate.HasValue && terminationDate.Value < hireDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Termination Date must be after the hire date!";
            }
            else if (terminationDate.HasValue && terminationDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Termination Date cannot be in the future!";
            }
            else if (terminationDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Validate the termination date against other forms' dates
                    var formValidationResults = context.spValidateHireTermDates(currentProgramEmployee.ProgramEmployeePK,
                                                    programFK, (DateTime?)null, terminationDate).ToList();

                    //If there are results, the termination date is invalid
                    if (formValidationResults.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Termination Date is invalid, see notification message for details!";

                        //Create a message that contains the forms that would be invalidated
                        string message = "The Termination Date would invalidate these records if changed to that date:<br/><br/>";
                        foreach (spValidateHireTermDates_Result invalidForm in formValidationResults)
                        {
                            message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                            message += "<br/>";
                        }

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Termination Date Validation Error", message, 200000);
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
                e.ErrorText = "Termination Reason is required if you have a Termination Date!";
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
            if (ddTerminationReason.SelectedItem != null && ddTerminationReason.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(terminationReasonSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Termination Reason is required when the 'Other' termination reason is selected!";
            }
        }

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
                                 .Where(t => t.ProgramEmployeeFK == currentProgramEmployee.ProgramEmployeePK)
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

            //Get the hidden field with the PK for editing
            HiddenField hfTrainingPK = (HiddenField)item.FindControl("hfTrainingPK");

            //Get the PK from the hidden field
            int? trainingPK = (String.IsNullOrWhiteSpace(hfTrainingPK.Value) ? (int?)null : Convert.ToInt32(hfTrainingPK.Value));

            if (trainingPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the training to edit
                    Training editTraining = context.Training.AsNoTracking().Where(cn => cn.TrainingPK == trainingPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditTraining.Text = "Edit Training";
                    ddTraining.SelectedItem = ddTraining.Items.FindByValue(editTraining.TrainingCodeFK);
                    deTrainingDate.Value = editTraining.TrainingDate.ToString("MM/dd/yyyy");
                    hfAddEditTrainingPK.Value = trainingPK.Value.ToString();
                }

                //Show the training div
                divAddEditTraining.Visible = true;

                //Set focus to the training date field
                deTrainingDate.Focus();
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
            if (currentProgramRole.AllowedToEdit.Value)
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
                        currentTraining.ProgramEmployeeFK = currentProgramEmployee.ProgramEmployeePK;
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
                        currentTraining.ProgramEmployeeFK = currentProgramEmployee.ProgramEmployeePK;
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
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteTrainingPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTrainingPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the training to remove
                        Training trainingToRemove = context.Training.Where(cn => cn.TrainingPK == rowToRemovePK).FirstOrDefault();

                        if(trainingToRemove.CodeTraining.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                        {
                            //Remove the training
                            context.Training.Remove(trainingToRemove);
                            context.SaveChanges();

                            //Rebind the training repeater
                            BindTrainings();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted training!", 10000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Delete Failed", "You are not authorized to delete this file!", 10000);
                        }
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

            //Get the hidden field with the PK for editing
            HiddenField hfJobFunctionPK = (HiddenField)item.FindControl("hfJobFunctionPK");

            //Get the PK from the hidden field
            int? jobFunctionPK = (String.IsNullOrWhiteSpace(hfJobFunctionPK.Value) ? (int?)null : Convert.ToInt32(hfJobFunctionPK.Value));

            if (jobFunctionPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the jobFunction to edit
                    JobFunction editJobFunction = context.JobFunction.AsNoTracking().Where(cn => cn.JobFunctionPK == jobFunctionPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditJobFunction.Text = "Edit Job Function";
                    ddJobType.SelectedItem = ddJobType.Items.FindByValue(editJobFunction.JobTypeCodeFK);
                    deJobStartDate.Value = editJobFunction.StartDate.ToString("MM/dd/yyyy");
                    deJobEndDate.Value = (editJobFunction.EndDate.HasValue ? editJobFunction.EndDate.Value.ToString("MM/dd/yyyy") : "");
                    hfAddEditJobFunctionPK.Value = jobFunctionPK.Value.ToString();
                }

                //Show the jobFunction div
                divAddEditJobFunction.Visible = true;

                //Set focus to the job type field
                ddJobType.Focus();
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
            if (currentProgramRole.AllowedToEdit.Value)
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
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteJobFunctionPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteJobFunctionPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the jobFunction to remove
                        JobFunction jobFunctionToRemove = context.JobFunction.Where(cn => cn.JobFunctionPK == rowToRemovePK).FirstOrDefault();

                        //Remove the jobFunction
                        context.JobFunction.Remove(jobFunctionToRemove);
                        context.SaveChanges();

                        //Rebind the jobFunction repeater
                        BindJobFunctions();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted job function!", 10000);
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
                e.ErrorText = "The hire date must have a value before this is entered!";
            }
            else if(jobStartDate.Value < hireDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date cannot be before the hire date!";
            }
            else if(termDate.HasValue && jobStartDate.Value > termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Start Date cannot be after the termination date!";
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
                            e.ErrorText = "Assign Date must be between the hire date and termination date!";
                        }
                        else if (jobStartDate.HasValue && termDate.HasValue == false
                            && (jobStartDate.Value > DateTime.Now || jobStartDate.Value < hireDate.Value))
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date must be between the hire date and now!";
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
            else if (termDate.HasValue && jobEndDate.Value > termDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "End Date cannot be after the termination date!";
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
                        else if (jobEndDate.HasValue && termDate.HasValue
                                    && (jobEndDate.Value > termDate.Value || jobEndDate.Value < hireDate.Value))
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date must be between the hire date and termination date!";
                        }
                        else if (jobEndDate.HasValue && termDate.HasValue == false
                            && (jobEndDate.Value > DateTime.Now || jobEndDate.Value < hireDate.Value))
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date must be between the hire date and now!";
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
                                            .Where(ec => ec.EmployeeFK == currentProgramEmployee.ProgramEmployeePK
                                                && ec.Classroom.ProgramFK == programFK)
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

            //Get the hidden field with the PK for editing
            HiddenField hfClassroomAssignmentPK = (HiddenField)item.FindControl("hfClassroomAssignmentPK");

            //Get the PK from the hidden field
            int? assignmentPK = (String.IsNullOrWhiteSpace(hfClassroomAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfClassroomAssignmentPK.Value));

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
            if (currentProgramRole.AllowedToEdit.Value)
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
                        currentClassroomAssignment.EmployeeFK = currentProgramEmployee.ProgramEmployeePK;
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
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteClassroomAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteClassroomAssignmentPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the classroom assignment to remove
                        EmployeeClassroom assignmentToRemove = context.EmployeeClassroom.Where(cn => cn.EmployeeClassroomPK == rowToRemovePK).FirstOrDefault();

                        //Remove the classroom assignment
                        context.EmployeeClassroom.Remove(assignmentToRemove);
                        context.SaveChanges();

                        //Rebind the classroom assignment table
                        BindClassroomAssignments();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted classroom assignment!", 10000);
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
                        e.ErrorText = "Employee is not active in that job function as of the classroom assign date!";
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
                e.ErrorText = "Assign Date must be between the hire date and termination date!";
            }
            else if (assignDate.HasValue && termDate.HasValue == false
                && (assignDate.Value > DateTime.Now || assignDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be between the hire date and now!";
            }
            else if(jobTypeFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                                .Include(tc => tc.Classroom)
                                                .Include(tc => tc.CodeEmployeeLeaveReason)
                                                .Where(tc => tc.EmployeeFK == currentProgramEmployee.ProgramEmployeePK
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
                e.ErrorText = "Hire Date must be entered before this date!";
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
                        && (leaveDate.Value > termDate.Value || leaveDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be between the hire date and termination date!";
            }
            else if (leaveDate.HasValue && termDate.HasValue == false
                && (leaveDate.Value > DateTime.Now || leaveDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be between the hire date and now!";
            }
            else if (jobTypeFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                                .Include(tc => tc.Classroom)
                                                .Include(tc => tc.CodeEmployeeLeaveReason)
                                                .Where(tc => tc.EmployeeFK == currentProgramEmployee.ProgramEmployeePK
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
            if (ddLeaveReason.SelectedItem != null && ddLeaveReason.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(leaveReasonSpecify))
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

        /// <summary>
        /// This method fires when the validation for a specified date edit
        /// fires and it validates that control's value against the hire date
        /// and termination date
        /// </summary>
        /// <param name="sender">A DevExpress DateEdit control</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void CheckBetweenHireAndTermAndRequired_Validation(object sender, ValidationEventArgs e)
        {
            //Get the date to check
            DateTime? dateToCheck = (e.Value == null ? (DateTime?)null : Convert.ToDateTime(e.Value));

            //Get the hire date and term date
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
                e.ErrorText = "This date must be between the hire date and now!";
            }
            else if (termDate.HasValue && (dateToCheck < hireDate || dateToCheck > termDate))
            {
                e.IsValid = false;
                e.ErrorText = "This date must be between the hire date and termination date!";
            }
        }
    }
}