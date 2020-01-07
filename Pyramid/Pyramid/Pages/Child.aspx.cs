using Pyramid.Models;
using System;
using System.Linq;
using Pyramid.MasterPages;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Pyramid.Code;
using DevExpress.Web;

namespace Pyramid.Pages
{
    public partial class Child : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.ChildProgram currentChildProgram;
        private int programFK;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the child program PK
            int childProgramPK = 0;

            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Try to get the child pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ChildProgramPK"]))
            {
                //Parse the child pk
                int.TryParse(Request.QueryString["ChildProgramPK"], out childProgramPK);
            }

            //Don't allow aggregate viewers into this page
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                Response.Redirect("/Pages/ChildrenDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the child program object
                currentChildProgram = context.ChildProgram.AsNoTracking()
                                        .Include(cp => cp.Program)
                                        .Where(cp => cp.ChildProgramPK == childProgramPK).FirstOrDefault();

                if (currentChildProgram == null)
                {
                    currentChildProgram = new ChildProgram();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the child's program
                    lblProgram.Text = currentChildProgram.Program.ProgramName;
                }
            }

            //Get the proper program fk
            programFK = (currentChildProgram.ChildProgramPK > 0 ? currentChildProgram.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            //Don't allow users to view children from other programs
            if (currentChildProgram.ChildProgramPK > 0 && !currentProgramRole.ProgramFKs.Contains(currentChildProgram.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/ChildrenDashboard.aspx?messageType={0}", "NoChild"));
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
                        case "ChildAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Child successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Show the edit only div if this is an edit
                divEditOnly.Visible = (childProgramPK > 0 ? true : false);

                //Try to get the action type
                if (!string.IsNullOrWhiteSpace(Request.QueryString["Action"]))
                {
                    action = Request.QueryString["Action"].ToString();
                }
                else
                {
                    action = "View";
                }

                using (PyramidContext context = new PyramidContext())
                {
                    //Bind the dropdowns
                    //Get the child status types
                    var allChildStatusTypes = context.CodeChildStatus.AsNoTracking()
                                                .OrderBy(ccs => ccs.OrderBy)
                                                .ToList();
                    ddStatus.DataSource = allChildStatusTypes;
                    ddStatus.DataBind();

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
                    var allLeaveReasons = context.CodeChildLeaveReason.AsNoTracking()
                                            .OrderBy(cclr => cclr.OrderBy)
                                            .ToList();
                    ddLeaveReason.DataSource = allLeaveReasons;
                    ddLeaveReason.DataBind();
                }

                //Allow adding/editing depending on the user's role and the action
                if (currentChildProgram.ChildProgramPK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Populate the user control
                    childControl.InitializeWithData(0, programFK, false);

                    //Show the submit button
                    submitChild.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Set the page title
                    lblPageTitle.Text = "Add New Child";
                }
                else if (currentChildProgram.ChildProgramPK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Populate the user control
                    childControl.InitializeWithData(currentChildProgram.ChildProgramPK, programFK, false);

                    //Show the submit button
                    submitChild.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Set the page title
                    lblPageTitle.Text = "Edit Child Information";
                }
                else
                {
                    //Populate the user control
                    childControl.InitializeWithData(currentChildProgram.ChildProgramPK, programFK, true);

                    //Hide the submit button
                    submitChild.ShowSubmitButton = false;

                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Set the page title
                    lblPageTitle.Text = "View Child Information";
                }

                //Bind the tables
                BindNotes();
                BindStatus();
                BindClassroomAssignments();

                //Set focus to the first name field
                childControl.FocusFirstName();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitChild user control 
        /// </summary>
        /// <param name="sender">The submitChild control</param>
        /// <param name="e">The Click event</param>
        protected void submitChild_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //To hold the child and child program objects
                Models.Child currentChild;

                //To hold the type of change
                string successMessageType = null;

                //Get the child object and child program
                Models.Child updatedChild = childControl.GetChild();
                Models.ChildProgram updatedChildProgram = childControl.GetChildProgram();

                if (updatedChild.ChildPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit success message
                        successMessageType = "ChildEdited";

                        //Set the edit fields
                        updatedChild.EditDate = DateTime.Now;
                        updatedChild.Editor = User.Identity.Name;
                        updatedChildProgram.EditDate = DateTime.Now;
                        updatedChildProgram.Editor = User.Identity.Name;

                        //Get the current child object from the context
                        currentChild = context.Child.Find(updatedChild.ChildPK);

                        //Get the current child program object from the context
                        currentChildProgram = context.ChildProgram.Find(updatedChildProgram.ChildProgramPK);

                        //Set the child and child program objects to the new values
                        context.Entry(currentChild).CurrentValues.SetValues(updatedChild);
                        context.Entry(currentChildProgram).CurrentValues.SetValues(updatedChildProgram);

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Redirect the user to the dashboard
                    Response.Redirect(string.Format("/Pages/ChildrenDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the add success message
                        successMessageType = "ChildAdded";

                        //Set the creator fields
                        updatedChild.CreateDate = DateTime.Now;
                        updatedChild.Creator = User.Identity.Name;
                        updatedChildProgram.CreateDate = DateTime.Now;
                        updatedChildProgram.Creator = User.Identity.Name;

                        //Add the child to the context
                        context.Child.Add(updatedChild);

                        //Set the child FK and add to the context
                        updatedChildProgram.ChildFK = updatedChild.ChildPK;
                        context.ChildProgram.Add(updatedChildProgram);

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/Child.aspx?ChildProgramPK={0}&Action=Edit&messageType={1}",
                                                        updatedChildProgram.ChildProgramPK.ToString(), successMessageType));
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitChild user control 
        /// </summary>
        /// <param name="sender">The submitChild control</param>
        /// <param name="e">The Click event</param>
        protected void submitChild_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the Children Dashboard
            Response.Redirect(string.Format("/Pages/ChildrenDashboard.aspx?messageType={0}", "ChildCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitChild control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitChild_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("danger", "Validation Error", childControl.ValidationMessageToDisplay, 22000);
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        #region Child Notes

        /// <summary>
        /// This method populates the note repeater with up-to-date information
        /// </summary>
        private void BindNotes()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var allChildNotes = context.ChildNote.AsNoTracking()
                                 .Where(cn => cn.ChildFK == currentChildProgram.ChildFK
                                    && cn.ProgramFK == currentChildProgram.ProgramFK)
                                 .OrderByDescending(cn => cn.NoteDate)
                                 .ToList();
                repeatChildNotes.DataSource = allChildNotes;
                repeatChildNotes.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the notes
        /// and it opens a div that allows the user to add a note
        /// </summary>
        /// <param name="sender">The lbAddChildNote LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddChildNote_Click(object sender, EventArgs e)
        {
            //Clear inputs in the note div
            hfAddEditNotePK.Value = "0";
            txtNoteContents.Value = "";
            deNoteDate.Value = "";

            //Set the title
            lblAddEditNote.Text = "Add Note";

            //Show the note div
            divAddEditNote.Visible = true;

            //Set focus to the date field
            deNoteDate.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a note
        /// and it opens the note edit div so that the user can edit the selected note
        /// </summary>
        /// <param name="sender">The lbEditChildNote LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditChildNote_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the hidden field with the PK for editing
            HiddenField hfNotePK = (HiddenField)item.FindControl("hfChildNotePK");

            //Get the PK from the hidden field
            int? notePK = (String.IsNullOrWhiteSpace(hfNotePK.Value) ? (int?)null : Convert.ToInt32(hfNotePK.Value));

            if (notePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the note to edit
                    ChildNote editNote = context.ChildNote.AsNoTracking().Where(cn => cn.ChildNotePK == notePK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditNote.Text = "Edit Note";
                    txtNoteContents.Value = editNote.Contents;
                    deNoteDate.Value = editNote.NoteDate.ToString("MM/dd/yyyy");
                    hfAddEditNotePK.Value = notePK.Value.ToString();
                }

                //Show the note div
                divAddEditNote.Visible = true;

                //Set focus to the date field
                deNoteDate.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected note!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the notes
        /// and it closes the note add/edit div
        /// </summary>
        /// <param name="sender">The submitNote user control</param>
        /// <param name="e">The Click event</param>
        protected void submitNote_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditNotePK.Value = "0";
            divAddEditNote.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitNote control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitNote_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the child notes
        /// and it saves the note information to the database
        /// </summary>
        /// <param name="sender">The btnSaveChildNote DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitNote_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the note pk
                int notePK = Convert.ToInt32(hfAddEditNotePK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    ChildNote currentNote;
                    //Check to see if this is an add or an edit
                    if (notePK == 0)
                    {
                        //Add
                        currentNote = new ChildNote();
                        currentNote.Contents = txtNoteContents.Value.ToString();
                        currentNote.NoteDate = Convert.ToDateTime(deNoteDate.Value);
                        currentNote.ChildFK = currentChildProgram.ChildFK;
                        currentNote.ProgramFK = programFK;
                        currentNote.CreateDate = DateTime.Now;
                        currentNote.Creator = User.Identity.Name;

                        //Save to the database
                        context.ChildNote.Add(currentNote);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added note!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentNote = context.ChildNote.Find(notePK);
                        currentNote.Contents = txtNoteContents.Value.ToString();
                        currentNote.NoteDate = Convert.ToDateTime(deNoteDate.Value);
                        currentNote.EditDate = DateTime.Now;
                        currentNote.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited note!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditNotePK.Value = "0";
                    divAddEditNote.Visible = false;

                    //Rebind the note gridview
                    BindNotes();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a note
        /// and it deletes the note information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteChildNote LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteChildNote_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteChildNotePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteChildNotePK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the note to remove
                        ChildNote noteToRemove = context.ChildNote.Where(cn => cn.ChildNotePK == rowToRemovePK).FirstOrDefault();

                        //Remove the note
                        context.ChildNote.Remove(noteToRemove);
                        context.SaveChanges();

                        //Rebind the note repeater
                        BindNotes();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted note!", 10000);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the note to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
        #endregion

        #region Child Status

        /// <summary>
        /// This method populates the status repeater with up-to-date information
        /// </summary>
        private void BindStatus()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var statusHistory = context.spGetChildStatusHistory(currentChildProgram.ChildFK, programFK);
                repeatChildStatus.DataSource = statusHistory;
                repeatChildStatus.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the statuses
        /// and it opens a div that allows the user to add a status
        /// </summary>
        /// <param name="sender">The lbAddChildStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddChildStatus_Click(object sender, EventArgs e)
        {
            //Clear inputs in the status div
            hfAddEditStatusPK.Value = "0";
            ddStatus.Value = "";
            deStatusDate.Value = "";

            //Set the title
            lblAddEditStatus.Text = "Add Status";

            //Show the status div
            divAddEditStatus.Visible = true;

            //Set focus to the status date field
            deStatusDate.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a status
        /// and it opens the status edit div so that the user can edit the selected status
        /// </summary>
        /// <param name="sender">The lbEditChildStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditChildStatus_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the hidden field with the PK for editing
            HiddenField hfStatusPK = (HiddenField)item.FindControl("hfChildStatusPK");

            //Get the PK from the hidden field
            int? statusPK = (String.IsNullOrWhiteSpace(hfStatusPK.Value) ? (int?)null : Convert.ToInt32(hfStatusPK.Value));

            if (statusPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the status to edit
                    ChildStatus editStatus = context.ChildStatus.AsNoTracking().Where(cn => cn.ChildStatusPK == statusPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditStatus.Text = "Edit Status";
                    ddStatus.SelectedItem = ddStatus.Items.FindByValue(editStatus.ChildStatusCodeFK);
                    deStatusDate.Value = editStatus.StatusDate.ToString("MM/dd/yyyy");
                    hfAddEditStatusPK.Value = statusPK.Value.ToString();
                }

                //Show the status div
                divAddEditStatus.Visible = true;

                //Set focus to the status date field
                deStatusDate.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected status!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the statuses
        /// and it closes the status add/edit div
        /// </summary>
        /// <param name="sender">The submitStatus user contrl</param>
        /// <param name="e">The Click event</param>
        protected void submitStatus_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditStatusPK.Value = "0";
            divAddEditStatus.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitStatus control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitStatus_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the child statuses
        /// and it saves the status information to the database
        /// </summary>
        /// <param name="sender">The submitStatus user control</param>
        /// <param name="e">The Click event</param>
        protected void submitStatus_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the status pk
                int statusPK = Convert.ToInt32(hfAddEditStatusPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    ChildStatus currentStatus;
                    //Check to see if this is an add or an edit
                    if (statusPK == 0)
                    {
                        //Add
                        currentStatus = new ChildStatus();
                        currentStatus.ChildStatusCodeFK = Convert.ToInt32(ddStatus.Value);
                        currentStatus.StatusDate = Convert.ToDateTime(deStatusDate.Value);
                        currentStatus.ChildFK = currentChildProgram.ChildFK;
                        currentStatus.ProgramFK = programFK;
                        currentStatus.CreateDate = DateTime.Now;
                        currentStatus.Creator = User.Identity.Name;

                        //Save to the database
                        context.ChildStatus.Add(currentStatus);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added status!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentStatus = context.ChildStatus.Find(statusPK);
                        currentStatus.ChildStatusCodeFK = Convert.ToInt32(ddStatus.Value);
                        currentStatus.StatusDate = Convert.ToDateTime(deStatusDate.Value);
                        currentStatus.EditDate = DateTime.Now;
                        currentStatus.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited status!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditStatusPK.Value = "0";
                    divAddEditStatus.Visible = false;

                    //Rebind the status table
                    BindStatus();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a status
        /// and it deletes the status information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteChildStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteChildStatus_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteChildStatusPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteChildStatusPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the status to remove
                        ChildStatus statusToRemove = context.ChildStatus.Where(cn => cn.ChildStatusPK == rowToRemovePK).FirstOrDefault();

                        //Remove the status
                        context.ChildStatus.Remove(statusToRemove);
                        context.SaveChanges();

                        //Rebind the status table
                        BindStatus();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted status!", 10000);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the status to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
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
                var classroomAssignments = context.ChildClassroom.AsNoTracking()
                                            .Include(cc => cc.Classroom)
                                            .Include(cc => cc.CodeChildLeaveReason)
                                            .Where(cc => cc.ChildFK == currentChildProgram.ChildFK
                                                && cc.Classroom.ProgramFK == programFK)
                                            .OrderBy(cc => cc.AssignDate)
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
            //Show the leave information
            deLeaveDate.Visible = true;
            ddLeaveReason.Visible = true;
            txtLeaveReasonSpecify.Visible = true;

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
                    ChildClassroom editClassroomAssignment = context.ChildClassroom.AsNoTracking().Where(cn => cn.ChildClassroomPK == assignmentPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditClassroomAssignment.Text = "Edit Classroom Assignment";
                    deAssignDate.Value = editClassroomAssignment.AssignDate.ToString("MM/dd/yyyy");
                    ddClassroom.SelectedItem = ddClassroom.Items.FindByValue(editClassroomAssignment.ClassroomFK);
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
        /// <param name="sender">The submitClassroomAssignment user control</param>
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
        /// <param name="sender">The submitClassroomAssignment control</param>
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
        /// <param name="sender">The submitClassroomAssignment user control</param>
        /// <param name="e">The Click event</param>
        protected void submitClassroomAssignment_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the classroom assignment pk
                int assignmentPK = Convert.ToInt32(hfAddEditClassroomAssignmentPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    ChildClassroom currentClassroomAssignment;
                    //Check to see if this is an add or an edit
                    if (assignmentPK == 0)
                    {
                        //Add
                        currentClassroomAssignment = new ChildClassroom();
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = Convert.ToInt32(ddClassroom.Value);
                        currentClassroomAssignment.LeaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
                        currentClassroomAssignment.LeaveReasonCodeFK = (ddLeaveReason.Value == null ? (int?)null : Convert.ToInt32(ddLeaveReason.Value));
                        currentClassroomAssignment.LeaveReasonSpecify = (txtLeaveReasonSpecify.Value == null ? null : txtLeaveReasonSpecify.Value.ToString());
                        currentClassroomAssignment.ChildFK = currentChildProgram.ChildFK;
                        currentClassroomAssignment.CreateDate = DateTime.Now;
                        currentClassroomAssignment.Creator = User.Identity.Name;

                        //Save to the database
                        context.ChildClassroom.Add(currentClassroomAssignment);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added classroom assignment!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentClassroomAssignment = context.ChildClassroom.Find(assignmentPK);
                        currentClassroomAssignment.AssignDate = Convert.ToDateTime(deAssignDate.Value);
                        currentClassroomAssignment.ClassroomFK = Convert.ToInt32(ddClassroom.Value);
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
                        ChildClassroom assignmentToRemove = context.ChildClassroom.Where(cn => cn.ChildClassroomPK == rowToRemovePK).FirstOrDefault();

                        //Remove the classroom assignment
                        context.ChildClassroom.Remove(assignmentToRemove);
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
        /// This method fires when the validation for the deAssignDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deAssignDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deAssignDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary information to validate
            DateTime? enrollmentDate = childControl.EnrollmentDate;
            DateTime? dischargeDate = childControl.DischargeDate;
            DateTime? assignDate = (deAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignDate.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditClassroomAssignmentPK.Value, out assignmentPK);

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
                e.ErrorText = "Assign Date must be between the enrollment date and discharge date!";
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
                                                .Where(cc => cc.ChildFK == currentChildProgram.ChildFK
                                                        && cc.Classroom.ProgramFK == programFK
                                                        && cc.ChildClassroomPK != assignmentPK).ToList();

                    //Validate against each assignment
                    foreach (ChildClassroom assignment in classroomAssignments)
                    {
                        if (assignment.LeaveDate.HasValue == false && assignDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "The child is already active in a classroom!";
                        }
                        else if (assignment.LeaveDate.HasValue && assignDate >= assignment.AssignDate && assignDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Assign Date cannot fall between an existing range of dates for a classroom assignment!";
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
            //Get the necessary information to validate
            DateTime? enrollmentDate = childControl.EnrollmentDate;
            DateTime? dischargeDate = childControl.DischargeDate;
            DateTime? assignDate = (deAssignDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deAssignDate.Value));
            DateTime? leaveDate = (deLeaveDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLeaveDate.Value));
            string leaveReason = (ddLeaveReason.Value == null ? null : ddLeaveReason.Value.ToString());
            int? classroomPK = (ddClassroom.Value == null ? (int?)null : Convert.ToInt32(ddClassroom.Value));

            //Get the classroom assignment pk
            int assignmentPK;
            int.TryParse(hfAddEditClassroomAssignmentPK.Value, out assignmentPK);

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
                                                .Where(cc => cc.ChildFK == currentChildProgram.ChildFK
                                                        && cc.Classroom.ProgramFK == programFK
                                                        && cc.ChildClassroomPK != assignmentPK).ToList();

                    //Validate against each assignment
                    foreach (ChildClassroom assignment in classroomAssignments)
                    {
                        if (leaveDate.HasValue == false)
                        {
                            if (assignDate.HasValue && assignDate.Value <= assignment.AssignDate)
                            {
                                e.IsValid = false;
                                e.ErrorText = "Leave Date is required when adding an assignment that starts before another assignment!";
                            }
                        }
                        else if (assignment.LeaveDate.HasValue == false && leaveDate >= assignment.AssignDate)
                        {
                            e.IsValid = false;
                            e.ErrorText = "The child is already active in a classroom!";
                        }
                        else if (assignment.LeaveDate.HasValue && leaveDate >= assignment.AssignDate && leaveDate <= assignment.LeaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Leave Date cannot fall between an existing range of dates for an assignment!";
                        }
                        else if (assignment.AssignDate >= assignDate.Value && assignment.AssignDate <= leaveDate.Value)
                        {
                            e.IsValid = false;
                            e.ErrorText = "This assignment cannot encapsulate another assignment!";
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
            //Get the assign date, leave date, and leave reason
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
        /// fires and it validates that control's value against the enrollment date
        /// and discharge date
        /// </summary>
        /// <param name="sender">A DevExpress DateEdit control</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void CheckBetweenEnrollmentAndDischargeAndRequired_Validation(object sender, ValidationEventArgs e)
        {
            //Get the date to check
            DateTime? dateToCheck = (e.Value == null ? (DateTime?)null : Convert.ToDateTime(e.Value));

            //Get the enrollment date and discharge date
            DateTime? enrollmentDate = childControl.EnrollmentDate;
            DateTime? dischargeDate = childControl.DischargeDate;

            //Perform validation
            if (dateToCheck.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "This date is required!";
            }
            else if (dischargeDate.HasValue == false && (dateToCheck < enrollmentDate || dateToCheck > DateTime.Now))
            {
                e.IsValid = false;
                e.ErrorText = "This date must be between the enrollment date and now!";
            }
            else if (dischargeDate.HasValue && (dateToCheck < enrollmentDate || dateToCheck > dischargeDate))
            {
                e.IsValid = false;
                e.ErrorText = "This date must be between the enrollment date and discharge date!";
            }
        }
    }
}