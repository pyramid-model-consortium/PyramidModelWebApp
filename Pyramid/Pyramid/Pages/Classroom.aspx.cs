using Pyramid.Models;
using System;
using System.Linq;
using Pyramid.MasterPages;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Pyramid.Code;
using DevExpress.Web;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace Pyramid.Pages
{
    public partial class Classroom : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CLASS";
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
        private Models.Classroom currentClassroom;
        private int classroomPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the classroom pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ClassroomPK"]))
            {
                //Parse the classroom pk
                int.TryParse(Request.QueryString["ClassroomPK"], out classroomPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (classroomPK == 0 && !string.IsNullOrWhiteSpace(hfClassroomPK.Value))
            {
                int.TryParse(hfClassroomPK.Value, out classroomPK);
            }

            //Check to see if this is an edit
            isEdit = classroomPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/ClassroomDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the classroom program object
                currentClassroom = context.Classroom.AsNoTracking()
                                        .Include(c => c.Program)
                                        .Where(c => c.ClassroomPK == classroomPK)
                                        .FirstOrDefault();

                //If the classroom is null, this is an add
                if (currentClassroom == null)
                {
                    //Set the classroom to a new classroom
                    currentClassroom = new Models.Classroom();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the classroom's program
                    lblProgram.Text = currentClassroom.Program.ProgramName;
                }
            }

            //Don't allow users to view classrooms from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentClassroom.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/ClassroomDashboard.aspx?messageType={0}", "NoClassroom"));
            }

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
                        case "ClassroomAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Classroom successfully added!", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Bind the dropdowns
                BindDropdowns();

                //Bind the classroom assignments for the children and employees
                BindChildClassroomAssignments();
                BindEmployeeClassroomAssignments();

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

                //Allow adding/editing depending on the user's role and the action
                if (isEdit == false && action.ToLower() == "add" && FormPermissions.AllowedToAdd)
                {
                    //Populate the user control
                    classroomControl.InitializeWithData(0, currentProgramRole.CurrentProgramFK.Value, false);

                    //Set control usability
                    EnableControls(true);

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Classroom";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Populate the user control
                    classroomControl.InitializeWithData(currentClassroom.ClassroomPK, currentClassroom.ProgramFK, false);

                    //Set control usability
                    EnableControls(true);

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Classroom Information";
                }
                else
                {
                    //Populate the user control
                    classroomControl.InitializeWithData(currentClassroom.ClassroomPK, currentClassroom.ProgramFK, true);

                    //Set control usability
                    EnableControls(false);

                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Classroom Information";
                }

                //Set the max dates for the date edits
                deChildAssignDate.MaxDate = DateTime.Now;
                deChildLeaveDate.MaxDate = DateTime.Now;
                deEmployeeAssignDate.MaxDate = DateTime.Now;
                deEmployeeLeaveDate.MaxDate = DateTime.Now;

                //Set focus to the name field
                classroomControl.FocusClassroomName();

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
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Show/hide the submit button
            submitClassroom.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            bool useCancelConfirmations = enabled && areConfirmationsEnabled;

            submitClassroom.UseCancelConfirm = useCancelConfirmations;
            submitChildClassroomAssignment.UseCancelConfirm = useCancelConfirmations;
            submitEmployeeClassroomAssignment.UseCancelConfirm = useCancelConfirmations;
        }

        /// <summary>
        /// This method binds the dropdowns with the necessary values
        /// </summary>
        private void BindDropdowns()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the child leave reasons
                var allChildLeaveReasons = context.CodeChildLeaveReason.AsNoTracking()
                                        .OrderBy(cclr => cclr.OrderBy)
                                        .ToList();
                ddChildLeaveReason.DataSource = allChildLeaveReasons;
                ddChildLeaveReason.DataBind();

                //Get all the children
                var allChildren = from c in context.Child.Include(c => c.ChildProgram).AsNoTracking()
                                  join cp in context.ChildProgram on c.ChildPK equals cp.ChildFK
                                  where cp.ProgramFK == currentClassroom.ProgramFK
                                  orderby cp.ProgramSpecificID ascending
                                  select new
                                  {
                                      c.ChildPK,
                                      IdAndName = (currentProgramRole.ViewPrivateChildInfo.Value ?
                                                "(" + cp.ProgramSpecificID + ") " + c.FirstName + " " + c.LastName :
                                                cp.ProgramSpecificID)

                                  };
                ddChild.DataSource = allChildren.ToList();
                ddChild.DataBind();

                //Get all the program's employees
                var allProgramEmployees = context.ProgramEmployee
                                        .Include(pe => pe.Employee)
                                        .AsNoTracking()
                                        .Where(pe => pe.ProgramFK == currentClassroom.ProgramFK)
                                        .OrderBy(pe => pe.Employee.FirstName)
                                        .Select(pe => new
                                        {
                                            pe.ProgramEmployeePK,
                                            Name = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName : "HIDDEN")
                                        }).ToList();
                ddEmployee.DataSource = allProgramEmployees;
                ddEmployee.DataBind();

                //Get all the employee leave reasons
                var allEmployeeLeaveReasons = context.CodeEmployeeLeaveReason.AsNoTracking()
                                        .OrderBy(ctlr => ctlr.OrderBy)
                                        .ToList();
                ddEmployeeLeaveReason.DataSource = allEmployeeLeaveReasons;
                ddEmployeeLeaveReason.DataBind();

                //Get all the job functions
                var allJobFunctions = context.CodeJobType.AsNoTracking()
                                        .OrderBy(cjt => cjt.OrderBy)
                                        .ToList();

                //Filter the job functions by the user's current role
                var filteredJobFunctions = allJobFunctions.Where(ajf => ajf.RolesAuthorizedToModify.Split(',').ToList().Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString())).ToList();

                //Bind the classroom job type dropdown
                ddClassroomJobType.DataSource = filteredJobFunctions;
                ddClassroomJobType.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitClassroom user control 
        /// </summary>
        /// <param name="sender">The submitClassroom control</param>
        /// <param name="e">The Click event</param>
        protected void submitClassroom_Click(object sender, EventArgs e)
        {
            //To hold the type of change
            string successMessageType = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the dashboard
                Response.Redirect(string.Format("/Pages/ClassroomDashboard.aspx?messageType={0}", successMessageType));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitClassroom user control 
        /// </summary>
        /// <param name="sender">The submitClassroom control</param>
        /// <param name="e">The Click event</param>
        protected void submitClassroom_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the Classroom Dashboard
            Response.Redirect(string.Format("/Pages/ClassroomDashboard.aspx?messageType={0}", "ClassroomCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitClassroom control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitClassroom_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("danger", "Validation Error", classroomControl.ValidationMessageToDisplay, 22000);
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitClassroom.ValidationGroup))
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
                    Reports.PreBuiltReports.FormReports.RptClassroom report = new Reports.PreBuiltReports.FormReports.RptClassroom();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Classroom Information", classroomPK);
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
                    Response.Redirect(string.Format("/Pages/Classroom.aspx?ClassroomPK={0}&Action={1}&messageType={2}&Print=True",
                                                        classroomPK, action, successMessageType));
                }
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("danger", "Validation Error", classroomControl.ValidationMessageToDisplay, 22000);
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
            //To hold the success message
            string successMessageType = null;

            //Determine if the user is allowed to save the form
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Get the classroom object and classroom program
                Models.Classroom updatedClassroom = classroomControl.GetClassroom();

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "ClassroomEdited";

                        //Set the edit fields
                        updatedClassroom.EditDate = DateTime.Now;
                        updatedClassroom.Editor = User.Identity.Name;

                        //Get the current classroom object from the context
                        currentClassroom = context.Classroom.Find(updatedClassroom.ClassroomPK);

                        //Set the classroom and classroom program objects to the new values
                        context.Entry(currentClassroom).CurrentValues.SetValues(updatedClassroom);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local PK variable
                        hfClassroomPK.Value = updatedClassroom.ClassroomPK.ToString();
                        classroomPK = updatedClassroom.ClassroomPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "ClassroomAdded";

                        //Set the creator fields
                        updatedClassroom.CreateDate = DateTime.Now;
                        updatedClassroom.Creator = User.Identity.Name;

                        //Add the classroom to the context
                        context.Classroom.Add(updatedClassroom);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local PK variable
                        hfClassroomPK.Value = updatedClassroom.ClassroomPK.ToString();
                        classroomPK = updatedClassroom.ClassroomPK;
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

        #region Child Classroom Assignments

        /// <summary>
        /// This method populates the child classroom assignment repeater with up-to-date information
        /// </summary>
        private void BindChildClassroomAssignments()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var classroomAssignments = from cc in context.ChildClassroom.AsNoTracking()
                                                .Include(cc => cc.Child)
                                                .Include(cc => cc.Classroom)
                                                .Include(cc => cc.CodeChildLeaveReason)
                                           join cp in context.ChildProgram on cc.ChildFK equals cp.ChildFK
                                           where cc.ClassroomFK == currentClassroom.ClassroomPK 
                                                && cp.ProgramFK == currentClassroom.ProgramFK
                                           orderby cc.AssignDate ascending
                                           select new
                                           {
                                               cc.ChildClassroomPK,
                                               cc.AssignDate,
                                               cc.LeaveDate,
                                               LeaveReason = (cc.CodeChildLeaveReason != null ? cc.CodeChildLeaveReason.Description + (cc.LeaveReasonSpecify == null || cc.LeaveReasonSpecify == "" ? "" : " (" + cc.LeaveReasonSpecify + ")") : ""),
                                               cc.ChildFK,
                                               ChildIdAndName = (currentProgramRole.ViewPrivateChildInfo.Value ?
                                                                    "(" + cp.ProgramSpecificID + ") " + cc.Child.FirstName + " " + cc.Child.LastName :
                                                                    cp.ProgramSpecificID),
                                               cp.EnrollmentDate,
                                               cp.DischargeDate
                                           };

                repeatChildClassroomAssignments.DataSource = classroomAssignments.ToList();
                repeatChildClassroomAssignments.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a classroom assignment
        /// and it opens the edit div so that the user can edit the selected classroom assignment
        /// </summary>
        /// <param name="sender">The lbEditClassroomAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditChildClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Lock the Child dropdown (if changes were allowed, we would have to bind the hidden fields on index change, etc)
            ddChild.ClientEnabled = false;

            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the labels
            Label lblClassroomAssignmentPK = (Label)item.FindControl("lblClassroomAssignmentPK");
            Label lblChildFK = (Label)item.FindControl("lblChildFK");
            Label lblEnrollmentDate = (Label)item.FindControl("lblEnrollmentDate");
            Label lblDischargeDate = (Label)item.FindControl("lblDischargeDate");

            //Get the PK from the labels
            int? assignmentPK = (String.IsNullOrWhiteSpace(lblClassroomAssignmentPK.Text) ? (int?)null : Convert.ToInt32(lblClassroomAssignmentPK.Text));
            int? childFK = (String.IsNullOrWhiteSpace(lblChildFK.Text) ? (int?)null : Convert.ToInt32(lblChildFK.Text));

            if (assignmentPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the classroom assignment to edit
                    ChildClassroom editClassroomAssignment = context.ChildClassroom.AsNoTracking().Where(cn => cn.ChildClassroomPK == assignmentPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditChildClassroomAssignment.Text = "Edit Classroom Assignment";
                    deChildAssignDate.Value = editClassroomAssignment.AssignDate.ToString("MM/dd/yyyy");
                    ddChild.SelectedItem = ddChild.Items.FindByValue(editClassroomAssignment.ChildFK);
                    deChildLeaveDate.Value = (editClassroomAssignment.LeaveDate.HasValue ? editClassroomAssignment.LeaveDate.Value.ToString("MM/dd/yyyy") : "");
                    ddChildLeaveReason.SelectedItem = ddChildLeaveReason.Items.FindByValue(editClassroomAssignment.LeaveReasonCodeFK);
                    txtChildLeaveReasonSpecify.Value = (editClassroomAssignment.LeaveReasonSpecify == null ? "" : editClassroomAssignment.LeaveReasonSpecify.ToString());
                    hfAddEditChildClassroomAssignmentPK.Value = assignmentPK.Value.ToString();
                    hfAddEditChildClassroomChildPK.Value = (childFK.HasValue ? childFK.Value.ToString() : "0");
                    hfAddEditChildClassroomEnrollmentDate.Value = lblEnrollmentDate.Text;
                    hfAddEditChildClassroomDischargeDate.Value = lblDischargeDate.Text;
                }

                //Show the classroom assignment div
                divAddEditChildClassroomAssignment.Visible = true;

                //Set focus to the child assign date field
                deChildAssignDate.Focus();
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
        /// <param name="sender">The submitClassroomAssignment user control</param>
        /// <param name="e">The Click event</param>
        protected void submitChildClassroomAssignment_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditChildClassroomAssignmentPK.Value = "0";
            divAddEditChildClassroomAssignment.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitChildClassroomAssignment control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitChildClassroomAssignment_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the classroom assignment
        /// add/edit and it saves the classroom assignment information to the database
        /// </summary>
        /// <param name="sender">The submitClassroomAssignment user control</param>
        /// <param name="e">The Click event</param>
        protected void submitChildClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Since this is part of the classroom record, just determine if the user is allowed to edit the classroom info
            if (FormPermissions.AllowedToEdit)
            {
                //Get the classroom assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditChildClassroomAssignmentPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    ChildClassroom currentClassroomAssignment;
                    //Check to see if this is an add or an edit
                    if (assignmentPK == 0)
                    {
                        //Add
                        currentClassroomAssignment = new ChildClassroom();
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deChildAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = currentClassroom.ClassroomPK;
                        currentClassroomAssignment.LeaveDate = (deChildLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deChildLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddChildLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddChildLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtChildLeaveReasonSpecify.Value == null ? null : txtChildLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.ChildFK = Convert.ToInt32(ddChild.Value);
                        currentClassroomAssignment.CreateDate = DateTime.Now;
                        currentClassroomAssignment.Creator = User.Identity.Name;

                        //Save to the database
                        context.ChildClassroom.Add(currentClassroomAssignment);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added child classroom assignment!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentClassroomAssignment = context.ChildClassroom.Find(assignmentPK);
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deChildAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = currentClassroom.ClassroomPK;
                        currentClassroomAssignment.LeaveDate = (deChildLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deChildLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddChildLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddChildLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtChildLeaveReasonSpecify.Value == null ? null : txtChildLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.ChildFK = Convert.ToInt32(ddChild.Value);
                        currentClassroomAssignment.EditDate = DateTime.Now;
                        currentClassroomAssignment.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited classroom assignment!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditChildClassroomAssignmentPK.Value = "0";
                    divAddEditChildClassroomAssignment.Visible = false;

                    //Rebind the child classroom assignment table
                    BindChildClassroomAssignments();
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
        protected void lbDeleteChildClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Since this is part of the classroom record, just determine if the user is allowed to edit the classroom info
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteChildClassroomAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteChildClassroomAssignmentPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the classroom assignment to remove
                            ChildClassroom assignmentToRemove = context.ChildClassroom.Where(cn => cn.ChildClassroomPK == rowToRemovePK).FirstOrDefault();

                            //Remove the classroom assignment
                            context.ChildClassroom.Remove(assignmentToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row
                            context.ChildClassroomChanged
                                    .OrderByDescending(ccc => ccc.ChildClassroomChangedPK)
                                    .Where(ccc => ccc.ChildClassroomPK == assignmentToRemove.ChildClassroomPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the classroom assignment table
                            BindChildClassroomAssignments();

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
        /// This method fires when the validation for the deChildAssignDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deChildAssignDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deChildAssignDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary information to validate
            DateTime? enrollmentDate = (String.IsNullOrWhiteSpace(hfAddEditChildClassroomEnrollmentDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditChildClassroomEnrollmentDate.Value));
            DateTime? dischargeDate = (String.IsNullOrWhiteSpace(hfAddEditChildClassroomDischargeDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditChildClassroomDischargeDate.Value));
            DateTime? assignDate = (deChildAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deChildAssignDate.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditChildClassroomAssignmentPK.Value, out assignmentPK);

            //Get the child pk
            int childPK;
            int.TryParse(hfAddEditChildClassroomChildPK.Value, out childPK);

            //Perform the validation
            if (assignDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date is required!";
            }
            else if (assignDate.HasValue && dischargeDate.HasValue
                        && (assignDate.Value > dischargeDate.Value || assignDate.Value < enrollmentDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be between the enrollment date and separation date!";
            }
            else if (assignDate.HasValue && dischargeDate.HasValue == false
                && (assignDate.Value > DateTime.Now || assignDate.Value < enrollmentDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be between the enrollment date and now!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.ChildClassroom.AsNoTracking()
                                                .Include(cc => cc.Classroom)
                                                .Include(cc => cc.CodeChildLeaveReason)
                                                .Where(cc => cc.ChildFK == childPK
                                                        && cc.ChildClassroomPK != assignmentPK).ToList();

                    foreach (ChildClassroom assignment in classroomAssignments)
                    {
                        if (assignment.LeaveDate.HasValue == false && assignDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "The child is already active in this classroom: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                        else if (assignment.LeaveDate.HasValue && assignDate >= assignment.AssignDate && assignDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date cannot fall between the existing range of dates for this classroom assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deChildLeaveDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deChildLeaveDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deChildLeaveDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary information to validate
            DateTime? enrollmentDate = (String.IsNullOrWhiteSpace(hfAddEditChildClassroomEnrollmentDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditChildClassroomEnrollmentDate.Value));
            DateTime? dischargeDate = (String.IsNullOrWhiteSpace(hfAddEditChildClassroomDischargeDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditChildClassroomDischargeDate.Value));
            DateTime? assignDate = (deChildAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deChildAssignDate.Value));
            DateTime? leaveDate = (deChildLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deChildLeaveDate.Value));
            string leaveReason = (ddChildLeaveReason.Value == null ? null : ddChildLeaveReason.Value.ToString());

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditChildClassroomAssignmentPK.Value, out assignmentPK);

            //Get the child pk
            int childPK;
            int.TryParse(hfAddEditChildClassroomChildPK.Value, out childPK);

            //Perform the validation
            if (leaveDate.HasValue == false && !String.IsNullOrWhiteSpace(leaveReason))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date is required if you have a Leave Reason!";
            }
            else if (assignDate.HasValue == false && leaveDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be entered before the Leave Date!";
            }
            else if (leaveDate != null && leaveDate < assignDate)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be after the Assign Date!";
            }
            else if (leaveDate.HasValue && dischargeDate.HasValue
                        && (leaveDate.Value > dischargeDate.Value || leaveDate.Value < enrollmentDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be between the enrollment date and discharge date!";
            }
            else if (leaveDate.HasValue && dischargeDate.HasValue == false
                && (leaveDate.Value > DateTime.Now || leaveDate.Value < enrollmentDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be between the enrollment date and now!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.ChildClassroom.AsNoTracking()
                                                .Include(cc => cc.Classroom)
                                                .Include(cc => cc.CodeChildLeaveReason)
                                                .Where(cc => cc.ChildFK == childPK
                                                        && cc.ChildClassroomPK != assignmentPK).ToList();

                    foreach (ChildClassroom assignment in classroomAssignments)
                    {
                        if (leaveDate.HasValue == false)
                        {
                            if (assignDate.HasValue && assignDate.Value <= assignment.AssignDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "Leave Date is required when adding an assignment that starts before this assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                            }
                        }
                        else if (assignment.LeaveDate.HasValue == false && leaveDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "The child is already active in this classroom: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                        else if (assignment.LeaveDate.HasValue && leaveDate >= assignment.AssignDate && leaveDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Leave Date cannot fall between an existing range of dates for this classroom assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                        else if (assignment.AssignDate >= assignDate.Value && assignment.AssignDate <= leaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This assignment cannot encapsulate this classroom assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddChildLeaveReason DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddChildLeaveReason ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddChildLeaveReason_Validation(object sender, ValidationEventArgs e)
        {
            //Get the assign date, leave date, and leave reason
            DateTime? leaveDate = (deChildLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deChildLeaveDate.Value));
            string leaveReason = (ddChildLeaveReason.Value == null ? null : ddChildLeaveReason.Value.ToString());

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
        /// This method fires when the validation for the txtChildLeaveReasonSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtChildLeaveReasonSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtChildLeaveReasonSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string leaveReasonSpecify = (txtChildLeaveReasonSpecify.Value == null ? null : txtChildLeaveReasonSpecify.Value.ToString());

            //Perform validation
            if (ddChildLeaveReason.SelectedItem != null && ddChildLeaveReason.SelectedItem.Text.ToLower() == "other" && String.IsNullOrWhiteSpace(leaveReasonSpecify))
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

        #region Employee Classroom Assignments

        /// <summary>
        /// This method populates the employee classroom assignment repeater with up-to-date information
        /// </summary>
        private void BindEmployeeClassroomAssignments()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                            .Include(ec => ec.ProgramEmployee)
                                            .Include(ec => ec.ProgramEmployee.Employee)
                                            .Include(ec => ec.Classroom)
                                            .Include(ec => ec.CodeEmployeeLeaveReason)
                                            .Include(ec => ec.CodeJobType)
                                            .Where(ec => ec.ClassroomFK == currentClassroom.ClassroomPK)
                                            .OrderBy(ec => ec.AssignDate)
                                            .Select(ec => new
                                            {
                                                ec.EmployeeClassroomPK,
                                                ec.ProgramEmployeeFK,
                                                ec.ClassroomFK,
                                                ec.ProgramEmployee.HireDate,
                                                ec.ProgramEmployee.TermDate,
                                                ec.ProgramEmployee.EmployeeFK,
                                                EmployeeName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + ec.ProgramEmployee.ProgramSpecificID + ") " + ec.ProgramEmployee.Employee.FirstName + " " + ec.ProgramEmployee.Employee.LastName : ec.ProgramEmployee.ProgramSpecificID),
                                                ClassroomJob = ec.CodeJobType.Description,
                                                ec.AssignDate,
                                                ec.LeaveDate,
                                                LeaveReason = (ec.CodeEmployeeLeaveReason != null ? ec.CodeEmployeeLeaveReason.Description + (ec.LeaveReasonSpecify != null ? " (" + ec.LeaveReasonSpecify + ")" : "") : "")
                                            })
                                            .ToList();
                repeatEmployeeClassroomAssignments.DataSource = classroomAssignments;
                repeatEmployeeClassroomAssignments.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a classroom assignment
        /// and it opens the edit div so that the user can edit the selected classroom assignment
        /// </summary>
        /// <param name="sender">The lbEditClassroomAssignment LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditEmployeeClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Lock the Employee dropdown (if changes were allowed, we would have to bind the hidden fields on index change, etc)
            ddEmployee.ClientEnabled = false;

            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the labels
            Label lblClassroomAssignmentPK = (Label)item.FindControl("lblClassroomAssignmentPK");
            Label lblProgramEmployeeFK = (Label)item.FindControl("lblProgramEmployeeFK");
            Label lblHireDate = (Label)item.FindControl("lblHireDate");
            Label lblTermDate = (Label)item.FindControl("lblTermDate");

            //Get the PK from the labels
            int? assignmentPK = (string.IsNullOrWhiteSpace(lblClassroomAssignmentPK.Text) ? (int?)null : Convert.ToInt32(lblClassroomAssignmentPK.Text));
            int? programEmployeeFK = (string.IsNullOrWhiteSpace(lblProgramEmployeeFK.Text) ? (int?)null : Convert.ToInt32(lblProgramEmployeeFK.Text));

            if (assignmentPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the classroom assignment to edit
                    EmployeeClassroom editClassroomAssignment = context.EmployeeClassroom.AsNoTracking().Where(cn => cn.EmployeeClassroomPK == assignmentPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditEmployeeClassroomAssignment.Text = "Edit Classroom Assignment";
                    deEmployeeAssignDate.Value = editClassroomAssignment.AssignDate.ToString("MM/dd/yyyy");
                    ddEmployee.SelectedItem = ddEmployee.Items.FindByValue(editClassroomAssignment.ProgramEmployeeFK);
                    ddClassroomJobType.SelectedItem = ddClassroomJobType.Items.FindByValue(editClassroomAssignment.JobTypeCodeFK);
                    deEmployeeLeaveDate.Value = (editClassroomAssignment.LeaveDate.HasValue ? editClassroomAssignment.LeaveDate.Value.ToString("MM/dd/yyyy") : "");
                    ddEmployeeLeaveReason.SelectedItem = ddEmployeeLeaveReason.Items.FindByValue(editClassroomAssignment.LeaveReasonCodeFK);
                    txtEmployeeLeaveReasonSpecify.Value = (editClassroomAssignment.LeaveReasonSpecify == null ? "" : editClassroomAssignment.LeaveReasonSpecify.ToString());
                    hfAddEditEmployeeClassroomAssignmentPK.Value = assignmentPK.Value.ToString();
                    hfAddEditEmployeeClassroomProgramEmployeePK.Value = (programEmployeeFK.HasValue ? programEmployeeFK.Value.ToString() : "0");
                    hfAddEditEmployeeClassroomHireDate.Value = lblHireDate.Text;
                    hfAddEditEmployeeClassroomTermDate.Value = lblTermDate.Text;
                }

                //Show the classroom assignment div
                divAddEditEmployeeClassroomAssignment.Visible = true;

                //Set focus to the employee assign date field
                deEmployeeAssignDate.Focus();
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
        /// <param name="sender">The submitClassroomAssignment user control</param>
        /// <param name="e">The Click event</param>
        protected void submitEmployeeClassroomAssignment_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditEmployeeClassroomAssignmentPK.Value = "0";
            divAddEditEmployeeClassroomAssignment.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitEmployeeClassroomAssignment control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitEmployeeClassroomAssignment_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the classroom assignment
        /// add/edit and it saves the classroom assignment information to the database
        /// </summary>
        /// <param name="sender">The submitClassroomAssignment user control</param>
        /// <param name="e">The Click event</param>
        protected void submitEmployeeClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Since this is part of the classroom record, just determine if the user is allowed to edit the classroom info
            if (FormPermissions.AllowedToEdit)
            {
                //Get the classroom assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditEmployeeClassroomAssignmentPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    EmployeeClassroom currentClassroomAssignment;
                    //Check to see if this is an add or an edit
                    if (assignmentPK == 0)
                    {
                        //Add
                        currentClassroomAssignment = new EmployeeClassroom();
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deEmployeeAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = currentClassroom.ClassroomPK;
                        currentClassroomAssignment.LeaveDate = (deEmployeeLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddEmployeeLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddEmployeeLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtEmployeeLeaveReasonSpecify.Value == null ? null : txtEmployeeLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.ProgramEmployeeFK = Convert.ToInt32(ddEmployee.Value);
                        currentClassroomAssignment.JobTypeCodeFK = Convert.ToInt32(ddClassroomJobType.Value);
                        currentClassroomAssignment.CreateDate = DateTime.Now;
                        currentClassroomAssignment.Creator = User.Identity.Name;

                        //Save to the database
                        context.EmployeeClassroom.Add(currentClassroomAssignment);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added professional classroom assignment!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentClassroomAssignment = context.EmployeeClassroom.Find(assignmentPK);
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deEmployeeAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = currentClassroom.ClassroomPK;
                        currentClassroomAssignment.LeaveDate = (deEmployeeLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddEmployeeLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddEmployeeLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtEmployeeLeaveReasonSpecify.Value == null ? null : txtEmployeeLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.ProgramEmployeeFK = Convert.ToInt32(ddEmployee.Value);
                        currentClassroomAssignment.JobTypeCodeFK = Convert.ToInt32(ddClassroomJobType.Value);
                        currentClassroomAssignment.EditDate = DateTime.Now;
                        currentClassroomAssignment.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited classroom assignment!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditEmployeeClassroomAssignmentPK.Value = "0";
                    divAddEditEmployeeClassroomAssignment.Visible = false;

                    //Rebind the employee classroom assignment table
                    BindEmployeeClassroomAssignments();
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
        protected void lbDeleteEmployeeClassroomAssignment_Click(object sender, EventArgs e)
        {
            //Since this is part of the classroom record, just determine if the user is allowed to edit the classroom info
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteEmployeeClassroomAssignmentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteEmployeeClassroomAssignmentPK.Value));

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
                            BindEmployeeClassroomAssignments();

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
            int? programEmployeeFK = (ddEmployee.Value == null ? (int?)null : Convert.ToInt32(ddEmployee.Value));
            int? jobTypeFK = (ddClassroomJobType.Value == null ? (int?)null : Convert.ToInt32(ddClassroomJobType.Value));
            DateTime? assignDate = (deEmployeeAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeAssignDate.Value));
            List<JobFunction> activeJobFunctions = new List<JobFunction>();

            //Perform validation
            if(jobTypeFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Classroom Job is required!";
            }
            else if(programEmployeeFK.HasValue && assignDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Check to see if the employee has a valid job function during this time
                    activeJobFunctions = context.JobFunction.AsNoTracking()
                                                .Where(jf => jf.ProgramEmployeeFK == programEmployeeFK.Value
                                                        && jf.StartDate <= assignDate.Value
                                                        && (jf.EndDate.HasValue == false || jf.EndDate.Value >= assignDate.Value)
                                                        && jf.JobTypeCodeFK == jobTypeFK.Value)
                                                .ToList();

                    //Make sure that the employee has a valid job function
                    if(activeJobFunctions.Count < 1)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Professional is not active in that job function as of the classroom assign date!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deEmployeeAssignDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deEmployeeAssignDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deEmployeeAssignDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary information to validate
            DateTime? hireDate = (String.IsNullOrWhiteSpace(hfAddEditEmployeeClassroomHireDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditEmployeeClassroomHireDate.Value));
            DateTime? termDate = (String.IsNullOrWhiteSpace(hfAddEditEmployeeClassroomTermDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditEmployeeClassroomTermDate.Value));
            DateTime? assignDate = (deEmployeeAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeAssignDate.Value));
            int? jobTypeFK = (ddClassroomJobType.Value == null ? (int?)null : Convert.ToInt32(ddClassroomJobType.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditEmployeeClassroomAssignmentPK.Value, out assignmentPK);

            //Get the employee pk
            int programEmployeePK;
            int.TryParse(hfAddEditEmployeeClassroomProgramEmployeePK.Value, out programEmployeePK);

            //Perform the validation
            if (assignDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date is required!";
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
                                                .Include(cc => cc.Classroom)
                                                .Include(cc => cc.CodeEmployeeLeaveReason)
                                                .Include(cc => cc.CodeJobType)
                                                .Where(cc => cc.ProgramEmployeeFK == programEmployeePK
                                                        && cc.ClassroomFK == currentClassroom.ClassroomPK
                                                        && cc.EmployeeClassroomPK != assignmentPK
                                                        && cc.JobTypeCodeFK == jobTypeFK.Value)
                                                .ToList();

                    foreach (EmployeeClassroom assignment in classroomAssignments)
                    {
                        if (assignment.LeaveDate.HasValue == false && assignDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "The professional is already active as a " + assignment.CodeJobType.Description + " in this classroom: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                        else if (assignment.LeaveDate.HasValue && assignDate >= assignment.AssignDate && assignDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date cannot fall between the existing range of dates for this classroom assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name + " with a job of " + assignment.CodeJobType.Description;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deEmployeeLeaveDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deEmployeeLeaveDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deEmployeeLeaveDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary information to validate
            DateTime? hireDate = (String.IsNullOrWhiteSpace(hfAddEditEmployeeClassroomHireDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditEmployeeClassroomHireDate.Value));
            DateTime? termDate = (String.IsNullOrWhiteSpace(hfAddEditEmployeeClassroomTermDate.Value) ? (DateTime?)null : Convert.ToDateTime(hfAddEditEmployeeClassroomTermDate.Value));
            DateTime? assignDate = (deEmployeeAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeAssignDate.Value));
            DateTime? leaveDate = (deEmployeeLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeLeaveDate.Value));
            string leaveReason = (ddEmployeeLeaveReason.Value == null ? null : ddEmployeeLeaveReason.Value.ToString());
            int? jobTypeFK = (ddClassroomJobType.Value == null ? (int?)null : Convert.ToInt32(ddClassroomJobType.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditEmployeeClassroomAssignmentPK.Value, out assignmentPK);

            //Get the employee pk
            int programEmployeePK;
            int.TryParse(hfAddEditEmployeeClassroomProgramEmployeePK.Value, out programEmployeePK);

            //Perform the validation
            if (leaveDate.HasValue == false && !String.IsNullOrWhiteSpace(leaveReason))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date is required if you have a Leave Reason!";
            }
            else if (assignDate.HasValue == false && leaveDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Assign Date must be entered before the Leave Date!";
            }
            else if (leaveDate != null && leaveDate < assignDate)
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be after the Assign Date!";
            }
            else if (leaveDate.HasValue && termDate.HasValue
                        && (leaveDate.Value > termDate.Value || leaveDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be between the professional's start date and separation date!";
            }
            else if (leaveDate.HasValue && termDate.HasValue == false
                && (leaveDate.Value > DateTime.Now || leaveDate.Value < hireDate.Value))
            {
                e.IsValid = false;
                e.ErrorText = "Leave Date must be between the professional's start date and now!";
            }
            else if (jobTypeFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all OTHER existing assignments
                    var classroomAssignments = context.EmployeeClassroom.AsNoTracking()
                                                .Include(cc => cc.Classroom)
                                                .Include(cc => cc.CodeEmployeeLeaveReason)
                                                .Include(cc => cc.CodeJobType)
                                                .Where(cc => cc.ProgramEmployeeFK == programEmployeePK
                                                        && cc.ClassroomFK == currentClassroom.ClassroomPK
                                                        && cc.EmployeeClassroomPK != assignmentPK
                                                        && cc.JobTypeCodeFK == jobTypeFK.Value)
                                                .ToList();

                    foreach (EmployeeClassroom assignment in classroomAssignments)
                    {
                        if (leaveDate.HasValue == false)
                        {
                            if (assignDate.HasValue && assignDate.Value <= assignment.AssignDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "Leave Date is required when adding an assignment that starts before this assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name + " with a job of " + assignment.CodeJobType.Description;
                            }
                        }
                        else if (assignment.LeaveDate.HasValue == false && leaveDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "The professional is already active as a " + assignment.CodeJobType.Description + " in this classroom: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name;
                        }
                        else if (assignment.LeaveDate.HasValue && leaveDate >= assignment.AssignDate && leaveDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Leave Date cannot fall between an existing range of dates for this classroom assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name + " with a job of  " + assignment.CodeJobType.Description;
                        }
                        else if (assignment.AssignDate >= assignDate.Value && assignment.AssignDate <= leaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This assignment cannot encapsulate this classroom assignment: (" + assignment.Classroom.ProgramSpecificID + ") " + assignment.Classroom.Name + " with a job of " + assignment.CodeJobType.Description;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddEmployeeLeaveReason DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddEmployeeLeaveReason ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddEmployeeLeaveReason_Validation(object sender, ValidationEventArgs e)
        {
            //Get the assign date, leave date, and leave reason
            DateTime? leaveDate = (deEmployeeLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEmployeeLeaveDate.Value));
            string leaveReason = (ddEmployeeLeaveReason.Value == null ? null : ddEmployeeLeaveReason.Value.ToString());

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
        /// This method fires when the validation for the txtEmployeeLeaveReasonSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtEmployeeLeaveReasonSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtEmployeeLeaveReasonSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string leaveReasonSpecify = (txtEmployeeLeaveReasonSpecify.Value == null ? null : txtEmployeeLeaveReasonSpecify.Value.ToString());

            //Perform validation
            if (ddEmployeeLeaveReason.SelectedItem != null && ddEmployeeLeaveReason.SelectedItem.Text.ToLower() == "other" && String.IsNullOrWhiteSpace(leaveReasonSpecify))
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
    }
}