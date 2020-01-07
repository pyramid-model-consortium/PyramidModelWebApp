using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;
using System.Collections.Generic;
using DevExpress.Web.Bootstrap;

namespace Pyramid.Pages
{
    public partial class TPITOS : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.TPITOS currentTPITOS;
        private int currentTPITOSPK = 0;
        private int currentProgramFK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the TPITOS PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["TPITOSPK"]))
            {
                int.TryParse(Request.QueryString["TPITOSPK"], out currentTPITOSPK);
            }

            //Don't allow aggregate viewers into this page
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                Response.Redirect("/Pages/TPITOSDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the TPITOS from the database
                currentTPITOS = context.TPITOS.AsNoTracking()
                                    .Include(t => t.Classroom)
                                    .Include(t => t.Classroom.Program)
                                    .Include(t => t.TPITOSRedFlags)
                                    .Where(a => a.TPITOSPK == currentTPITOSPK)
                                    .FirstOrDefault();

                //Check to see if the TPITOS from the database exists
                if (currentTPITOS == null)
                {
                    //The TPITOS from the database doesn't exist, set the current TPITOS to a default value
                    currentTPITOS = new Models.TPITOS();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentTPITOS.Classroom.Program.ProgramName;
                }
            }

            //Prevent users from viewing TPITOSs from other programs
            if (currentTPITOS.TPITOSPK > 0 && !currentProgramRole.ProgramFKs.Contains(currentTPITOS.Classroom.ProgramFK))
            {
                Response.Redirect(string.Format("/Pages/TPITOSDashboard.aspx?messageType={0}", "NOTPITOS"));
            }

            //Get the proper program fk
            currentProgramFK = (currentTPITOS.TPITOSPK > 0 ? currentTPITOS.Classroom.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            //Set the max value for the observation date
            deObservationDate.MaxDate = DateTime.Now;

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
                        case "TPITOSAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Basic Information successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Show certain divs based on whether this is an add or edit
                if (currentTPITOSPK > 0)
                {
                    divAddOnlyMessage.Visible = false;
                    divEditOnly.Visible = true;
                }
                else
                {
                    divAddOnlyMessage.Visible = true;
                    divEditOnly.Visible = false;
                }

                //Bind the data bound controls
                BindDataBoundControls();

                //Check to see if this is an edit
                if (currentTPITOSPK > 0)
                {
                    //This is an edit
                    //Populate the page
                    PopulatePage(currentTPITOS);
                }

                //Get the action from the query string
                string action;
                if (Request.QueryString["action"] != null)
                {
                    action = Request.QueryString["action"];
                }
                else
                {
                    action = "View";
                }

                //Allow adding/editing depending on the user's role and the action
                if (currentTPITOS.TPITOSPK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitTPITOS.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New TPITOS Observation";
                }
                else if (currentTPITOS.TPITOSPK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitTPITOS.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit TPITOS Observation";
                }
                else
                {
                    //Hide the submit button
                    submitTPITOS.ShowSubmitButton = false;

                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View TPITOS Observation";
                }

                //Set focus on the observation date field
                deObservationDate.Focus();
            }
        }

        #region Click Methods

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitTPITOS user control 
        /// </summary>
        /// <param name="sender">The submitTPITOS control</param>
        /// <param name="e">The Click event</param>
        protected void submitTPITOS_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {

                //To hold the success message type
                string successMessageType = null;

                //Fill the TPITOS fields from the observation
                //Calculate the start and end datetimes
                DateTime startDateTime = Convert.ToDateTime(deObservationDate.Value);
                DateTime endDateTime = Convert.ToDateTime(deObservationDate.Value);
                DateTime startTime = Convert.ToDateTime(teObservationStartTime.Value);
                DateTime endTime = Convert.ToDateTime(teObservationEndTime.Value);
                startDateTime = startDateTime.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
                endDateTime = endDateTime.AddHours(endTime.Hour).AddMinutes(endTime.Minute);

                //Basic information
                currentTPITOS.ObservationStartDateTime = startDateTime;
                currentTPITOS.ObservationEndDateTime = endDateTime;
                currentTPITOS.ClassroomFK = Convert.ToInt32(ddClassroom.Value);
                currentTPITOS.ObserverFK = Convert.ToInt32(ddObserver.Value);
                currentTPITOS.NumAdultsBegin = Convert.ToInt32(txtAdultsBegin.Value);
                currentTPITOS.NumAdultsEnd = Convert.ToInt32(txtAdultsEnd.Value);
                currentTPITOS.NumAdultsEntered = Convert.ToInt32(txtAdultsEntered.Value);
                currentTPITOS.NumKidsBegin = Convert.ToInt32(txtChildrenBegin.Value);
                currentTPITOS.NumKidsEnd = Convert.ToInt32(txtChildrenEnd.Value);
                currentTPITOS.Notes = (txtNotes.Value == null ? null : txtNotes.Value.ToString());

                //Key practices
                currentTPITOS.Item1NumYes = (txtItem1NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem1NumYes.Value));
                currentTPITOS.Item1NumNo = (txtItem1NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem1NumNo.Value));
                currentTPITOS.Item2NumYes = (txtItem2NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem2NumYes.Value));
                currentTPITOS.Item2NumNo = (txtItem2NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem2NumNo.Value));
                currentTPITOS.Item3NumYes = (txtItem3NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem3NumYes.Value));
                currentTPITOS.Item3NumNo = (txtItem3NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem3NumNo.Value));
                currentTPITOS.Item4NumYes = (txtItem4NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem4NumYes.Value));
                currentTPITOS.Item4NumNo = (txtItem4NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem4NumNo.Value));
                currentTPITOS.Item5NumYes = (txtItem5NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem5NumYes.Value));
                currentTPITOS.Item5NumNo = (txtItem5NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem5NumNo.Value));
                currentTPITOS.Item6NumYes = (txtItem6NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem6NumYes.Value));
                currentTPITOS.Item6NumNo = (txtItem6NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem6NumNo.Value));
                currentTPITOS.Item7NumYes = (txtItem7NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem7NumYes.Value));
                currentTPITOS.Item7NumNo = (txtItem7NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem7NumNo.Value));
                currentTPITOS.Item8NumYes = (txtItem8NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem8NumYes.Value));
                currentTPITOS.Item8NumNo = (txtItem8NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem8NumNo.Value));
                currentTPITOS.Item9NumYes = (txtItem9NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem9NumYes.Value));
                currentTPITOS.Item9NumNo = (txtItem9NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem9NumNo.Value));
                currentTPITOS.Item10NumYes = (txtItem10NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem10NumYes.Value));
                currentTPITOS.Item10NumNo = (txtItem10NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem10NumNo.Value));
                currentTPITOS.Item11NumYes = (txtItem11NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem11NumYes.Value));
                currentTPITOS.Item11NumNo = (txtItem11NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem11NumNo.Value));
                currentTPITOS.Item12NumYes = (txtItem12NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem12NumYes.Value));
                currentTPITOS.Item12NumNo = (txtItem12NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem12NumNo.Value));
                currentTPITOS.Item13NumYes = (txtItem13NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem13NumYes.Value));
                currentTPITOS.Item13NumNo = (txtItem13NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem13NumNo.Value));

                //Red flags
                currentTPITOS.LeadTeacherRedFlagsNumYes = (txtLeadTeacherRedFlagsYes.Value == null ? (int?)null : Convert.ToInt32(txtLeadTeacherRedFlagsYes.Value));
                currentTPITOS.LeadTeacherRedFlagsNumPossible = (txtLeadTeacherRedFlagsPossible.Value == null ? (int?)null : Convert.ToInt32(txtLeadTeacherRedFlagsPossible.Value));
                currentTPITOS.OtherTeacherRedFlagsNumYes = (txtOtherTeacherRedFlagsYes.Value == null ? (int?)null : Convert.ToInt32(txtOtherTeacherRedFlagsYes.Value));
                currentTPITOS.OtherTeacherRedFlagsNumPossible = (txtOtherTeacherRedFlagsPossible.Value == null ? (int?)null : Convert.ToInt32(txtOtherTeacherRedFlagsPossible.Value));
                currentTPITOS.ClassroomRedFlagsNumYes = (txtClassroomRedFlagsYes.Value == null ? (int?)null : Convert.ToInt32(txtClassroomRedFlagsYes.Value));
                currentTPITOS.ClassroomRedFlagsNumPossible = (txtClassroomRedFlagsPossible.Value == null ? (int?)null : Convert.ToInt32(txtClassroomRedFlagsPossible.Value));

                if (currentTPITOSPK > 0)
                {
                    if (repeatParticipants.Items.Count > 0)
                    {
                        //This is an edit
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Set the success message
                            successMessageType = "TPITOSEdited";

                            //Set the edit-only fields
                            currentTPITOS.Editor = User.Identity.Name;
                            currentTPITOS.EditDate = DateTime.Now;

                            //Clear the red flag type rows
                            var currentRedFlags = context.TPITOSRedFlags.Where(trf => trf.TPITOSFK == currentTPITOSPK).ToList();
                            context.TPITOSRedFlags.RemoveRange(currentRedFlags);

                            //Save the red flag types
                            foreach (BootstrapListEditItem item in lstBxRedFlags.Items)
                            {
                                if (item.Selected)
                                {
                                    TPITOSRedFlags newRedFlag = new TPITOSRedFlags();
                                    newRedFlag.CreateDate = DateTime.Now;
                                    newRedFlag.Creator = User.Identity.Name;
                                    newRedFlag.RedFlagCodeFK = Convert.ToInt32(item.Value);
                                    newRedFlag.TPITOSFK = currentTPITOSPK;
                                    context.TPITOSRedFlags.Add(newRedFlag);
                                }
                            }

                            //Set isValid to true because validation passed on the complete form
                            currentTPITOS.IsValid = true;

                            //Get the existing TPITOS record
                            Models.TPITOS existingASQ = context.TPITOS.Find(currentTPITOS.TPITOSPK);

                            //Overwrite the existing TPITOS record with the values from the observation
                            context.Entry(existingASQ).CurrentValues.SetValues(currentTPITOS);
                            context.SaveChanges();
                        }

                        //Redirect the user to the TPITOS dashboard
                        Response.Redirect(string.Format("/Pages/TPITOSDashboard.aspx?messageType={0}", successMessageType));
                    }
                    else
                    {
                        //Tell the user that validation failed
                        msgSys.ShowMessageToUser("danger", "TPITOS Participants Required!", "You must add at least one TPITOS participant!", 22000);
                        msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
                    }
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "TPITOSAdded";

                        //Set the create-only fields
                        currentTPITOS.Creator = User.Identity.Name;
                        currentTPITOS.CreateDate = DateTime.Now;

                        //Set isValid to false because validation may not have passed on the complete form
                        currentTPITOS.IsValid = false;

                        //Add the TPITOS to the database
                        context.TPITOS.Add(currentTPITOS);
                        context.SaveChanges();
                    }

                    //Redirect the user to the  this page
                    Response.Redirect(string.Format("/Pages/TPITOS.aspx?TPITOSPK={0}&Action=Edit&messageType={1}",
                                                        currentTPITOS.TPITOSPK.ToString(), successMessageType));
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Unauthorized!", "You are not authorized to make changes!", 22000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitTPITOS user control 
        /// </summary>
        /// <param name="sender">The submitTPITOS control</param>
        /// <param name="e">The Click event</param>
        protected void submitTPITOS_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the TPITOS dashboard
            Response.Redirect(string.Format("/Pages/TPITOSDashboard.aspx?messageType={0}", "TPITOSCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitTPITOS control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTPITOS_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the participants
        /// and it opens a div that allows the user to add a participant
        /// </summary>
        /// <param name="sender">The lbAddTPITOSParticipant LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTPITOSParticipant_Click(object sender, EventArgs e)
        {
            //Clear inputs in the participant div
            hfAddEditParticipantPK.Value = "0";
            ddParticipant.Value = "";
            ddParticipantRole.Value = "";

            //Set the title
            lblAddEditParticipant.Text = "Add TPITOS Participant";

            //Show the participant div
            divAddEditTPITOSParticipant.Visible = true;

            //Set focus to the participant dropdown
            ddParticipant.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a participant
        /// and it opens the participant edit div so that the user can edit the selected participant
        /// </summary>
        /// <param name="sender">The lbEditTPITOSParticipant LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTPITOSParticipant_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the hidden field with the PK for editing
            HiddenField hfParticipantPK = (HiddenField)item.FindControl("hfTPITOSParticipantPK");

            //Get the PK from the hidden field
            int? participantPK = (String.IsNullOrWhiteSpace(hfParticipantPK.Value) ? (int?)null : Convert.ToInt32(hfParticipantPK.Value));

            if (participantPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the participant to edit
                    TPITOSParticipant editParticipant = context.TPITOSParticipant.AsNoTracking().Where(cn => cn.TPITOSParticipantPK == participantPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditParticipant.Text = "Edit Participant";
                    ddParticipant.SelectedItem = ddParticipant.Items.FindByValue(editParticipant.ProgramEmployeeFK);
                    ddParticipantRole.SelectedItem = ddParticipantRole.Items.FindByValue(editParticipant.ParticipantTypeCodeFK);
                    hfAddEditParticipantPK.Value = participantPK.Value.ToString();
                }

                //Show the participant div
                divAddEditTPITOSParticipant.Visible = true;

                //Set focus to the participant dropdown
                ddParticipant.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected TPITOS participant!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the  participants
        /// and it saves the participant information to the database
        /// </summary>
        /// <param name="sender">The btnSaveTPITOSParticipant DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitParticipant_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the participant pk
                int participantPK = Convert.ToInt32(hfAddEditParticipantPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    TPITOSParticipant currentParticipant;
                    //Check to see if this is an add or an edit
                    if (participantPK == 0)
                    {
                        //Add
                        currentParticipant = new TPITOSParticipant();
                        currentParticipant.TPITOSFK = currentTPITOS.TPITOSPK;
                        currentParticipant.ProgramEmployeeFK = Convert.ToInt32(ddParticipant.Value);
                        currentParticipant.ParticipantTypeCodeFK = Convert.ToInt32(ddParticipantRole.Value);
                        currentParticipant.CreateDate = DateTime.Now;
                        currentParticipant.Creator = User.Identity.Name;

                        //Save to the database
                        context.TPITOSParticipant.Add(currentParticipant);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added TPITOS participant!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentParticipant = context.TPITOSParticipant.Find(participantPK);
                        currentParticipant.TPITOSFK = currentTPITOS.TPITOSPK;
                        currentParticipant.ProgramEmployeeFK = Convert.ToInt32(ddParticipant.Value);
                        currentParticipant.ParticipantTypeCodeFK = Convert.ToInt32(ddParticipantRole.Value);
                        currentParticipant.EditDate = DateTime.Now;
                        currentParticipant.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited TPITOS participant!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditParticipantPK.Value = "0";
                    divAddEditTPITOSParticipant.Visible = false;

                    //Rebind the participant gridview
                    BindParticipants();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a participant
        /// and it deletes the participant information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteTPITOSParticipant LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTPITOSParticipant_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteTPITOSParticipantPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTPITOSParticipantPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the participant to remove
                        TPITOSParticipant participantToRemove = context.TPITOSParticipant.Where(cn => cn.TPITOSParticipantPK == rowToRemovePK).FirstOrDefault();

                        //Remove the participant
                        context.TPITOSParticipant.Remove(participantToRemove);
                        context.SaveChanges();

                        //Rebind the participant repeater
                        BindParticipants();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted TPITOS participant!", 10000);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the TPITOS participant to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the participants
        /// and it closes the participant add/edit div
        /// </summary>
        /// <param name="sender">The submitParticipant user control</param>
        /// <param name="e">The Click event</param>
        protected void submitParticipant_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditParticipantPK.Value = "0";
            divAddEditTPITOSParticipant.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitParticipant control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitParticipant_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        #endregion

        #region Control Value/Index Change Methods

        /// <summary>
        /// This method fires when the value in the deIncidentDatetime DateEdit changes
        /// and it updates the observer and classroom dropdowns
        /// </summary>
        /// <param name="sender">The deIncidentDatetime DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deObservationDate_ValueChanged(object sender, EventArgs e)
        {
            //Get the observation date and observer FK
            DateTime? observationDate = (deObservationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deObservationDate.Value));
            int? observerFK = (ddObserver.Value == null ? (int?)null : Convert.ToInt32(ddObserver.Value));

            //Bind the observer dropdown
            BindObserverDropDown(observationDate, currentProgramFK, observerFK);
            BindParticipantDropDown(observationDate, currentProgramFK, observerFK);
        }

        #endregion

        #region Binding Methods

        /// <summary>
        /// This method populates the data bound controls from the database
        /// </summary>
        private void BindDataBoundControls()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the classroom dropdown
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

                //Bind the participant type dropdown
                var allParticipantTypes = context.CodeParticipantType.AsNoTracking()
                                            .OrderBy(cpt => cpt.OrderBy)
                                            .ToList();
                ddParticipantRole.DataSource = allParticipantTypes;
                ddParticipantRole.DataBind();

                //Bind the red flag list box
                var allTPITOSRedFlags = context.CodeTPITOSRedFlag.AsNoTracking()
                                            .OrderBy(ctrf => ctrf.OrderBy)
                                            .ToList();
                lstBxRedFlags.DataSource = allTPITOSRedFlags;
                lstBxRedFlags.DataBind();
            }

            //Bind other controls based on the pk
            if (currentTPITOS.TPITOSPK > 0)
            {
                //This is an edit
                //Bind the observer and participant dropdown
                BindObserverDropDown(currentTPITOS.ObservationStartDateTime.Date, currentProgramFK, currentTPITOS.ObserverFK);
                BindParticipantDropDown(currentTPITOS.ObservationStartDateTime.Date, currentProgramFK, currentTPITOS.ObserverFK);

                //Bind the participants repeater
                BindParticipants();
            }
            else
            {
                //This is an add
                //Bind the observer dropdown
                BindObserverDropDown((DateTime?)null, currentProgramFK, (int?)null);
            }
        }

        /// <summary>
        /// This method binds the observer dropdown by getting all the observer in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="observationDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="observerFK">The observer's FK to be selected</param>
        private void BindObserverDropDown(DateTime? observationDate, int programFK, int? observerFK)
        {
            if (observationDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the observers in the program that were active as of the observation date
                    string observerTypes = (((int)Utilities.TrainingFKs.TPITOS_OBSERVER).ToString() + ",");
                    var allObservers = context.spGetAllObservers(programFK, observationDate, observerTypes).ToList();

                    //Bind the observer dropdown to the list of observer
                    ddObserver.DataSource = allObservers.ToList();
                    ddObserver.DataBind();
                }

                //Check to see how many observers there are
                if (ddObserver.Items.Count > 0)
                {
                    //There is at least 1 observer, enable the observer dropdown
                    ddObserver.ReadOnly = false;

                    //Try to select the observer passed to this method
                    ddObserver.SelectedItem = ddObserver.Items.FindByValue(observerFK);
                }
                else
                {
                    //There are no observers in the list, disable the observer dropdown
                    ddObserver.ReadOnly = true;
                }
            }
            else
            {
                //No date was passed, clear and disable the observer dropdown
                ddObserver.Value = "";
                ddObserver.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method binds the participant list box by getting all the participants in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="logDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="participantFK">The participant's FK to be selected</param>
        private void BindParticipantDropDown(DateTime? observationDate, int programFK, int? observerFK)
        {
            if (observationDate.HasValue && observerFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the participants in the program that were active as of the log date
                    var allParticipants = (from pe in context.ProgramEmployee.AsNoTracking()
                                                    .Include(pe => pe.JobFunction)
                                          join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK
                                          where pe.ProgramFK == programFK
                                            && pe.ProgramEmployeePK != observerFK.Value
                                            && pe.HireDate <= observationDate.Value
                                            && (pe.TermDate.HasValue == false || pe.TermDate.Value >= observationDate.Value)
                                            && (jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHER || jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHING_ASSISTANT)
                                            && jf.StartDate <= observationDate.Value
                                            && (jf.EndDate.HasValue == false || jf.EndDate.Value >= observationDate.Value)
                                          select new
                                          {
                                              pe.ProgramEmployeePK,
                                              ParticipantName = pe.FirstName + " " + pe.LastName
                                          }).Distinct();

                    //Bind the participant list box to the list of participants
                    ddParticipant.DataSource = allParticipants.OrderBy(ap => ap.ParticipantName).ToList();
                    ddParticipant.DataBind();
                }

                //Check to see how many participants there are
                if (ddParticipant.Items.Count > 0)
                {
                    //There is at least 1 participant, enable the participant list box
                    ddParticipant.ReadOnly = false;
                }
                else
                {
                    //There are no participants in the list, disable the participant list box
                    ddParticipant.ReadOnly = true;
                }
            }
            else
            {
                //No date was passed, clear and disable the participant list box
                ddParticipant.Value = "";
                ddParticipant.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method populates the participant repeater with up-to-date information
        /// </summary>
        private void BindParticipants()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var allTPITOSParticipants = context.TPITOSParticipant.AsNoTracking()
                                            .Include(tp => tp.ProgramEmployee)
                                            .Include(tp => tp.CodeParticipantType)
                                            .Where(tp => tp.TPITOSFK == currentTPITOS.TPITOSPK)
                                            .OrderBy(tp => tp.ProgramEmployee.FirstName)
                                            .ToList();
                repeatParticipants.DataSource = allTPITOSParticipants;
                repeatParticipants.DataBind();
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method populates the page controls from the passed TPITOS object
        /// </summary>
        /// <param name="TPITOSToPopulate">The TPITOS object with values to populate the page controls</param>
        private void PopulatePage(Models.TPITOS tpitos)
        {
            //Set the page controls to the values from the object
            //Basic information
            deObservationDate.Value = tpitos.ObservationStartDateTime.Date;
            teObservationStartTime.Value = tpitos.ObservationStartDateTime;
            teObservationEndTime.Value = tpitos.ObservationEndDateTime;
            ddClassroom.SelectedItem = ddClassroom.Items.FindByValue(tpitos.ClassroomFK);
            ddObserver.SelectedItem = ddObserver.Items.FindByValue(tpitos.ObserverFK);
            txtAdultsBegin.Value = tpitos.NumAdultsBegin;
            txtAdultsEnd.Value = tpitos.NumAdultsEnd;
            txtAdultsEntered.Value = tpitos.NumAdultsEntered;
            txtChildrenBegin.Value = tpitos.NumKidsBegin;
            txtChildrenEnd.Value = tpitos.NumKidsEnd;
            txtNotes.Value = tpitos.Notes;

            //Key practices
            txtItem1NumYes.Value = tpitos.Item1NumYes;
            txtItem1NumNo.Value = tpitos.Item1NumNo;
            txtItem2NumYes.Value = tpitos.Item2NumYes;
            txtItem2NumNo.Value = tpitos.Item2NumNo;
            txtItem3NumYes.Value = tpitos.Item3NumYes;
            txtItem3NumNo.Value = tpitos.Item3NumNo;
            txtItem4NumYes.Value = tpitos.Item4NumYes;
            txtItem4NumNo.Value = tpitos.Item4NumNo;
            txtItem5NumYes.Value = tpitos.Item5NumYes;
            txtItem5NumNo.Value = tpitos.Item5NumNo;
            txtItem6NumYes.Value = tpitos.Item6NumYes;
            txtItem6NumNo.Value = tpitos.Item6NumNo;
            txtItem7NumYes.Value = tpitos.Item7NumYes;
            txtItem7NumNo.Value = tpitos.Item7NumNo;
            txtItem8NumYes.Value = tpitos.Item8NumYes;
            txtItem8NumNo.Value = tpitos.Item8NumNo;
            txtItem9NumYes.Value = tpitos.Item9NumYes;
            txtItem9NumNo.Value = tpitos.Item9NumNo;
            txtItem10NumYes.Value = tpitos.Item10NumYes;
            txtItem10NumNo.Value = tpitos.Item10NumNo;
            txtItem11NumYes.Value = tpitos.Item11NumYes;
            txtItem11NumNo.Value = tpitos.Item11NumNo;
            txtItem12NumYes.Value = tpitos.Item12NumYes;
            txtItem12NumNo.Value = tpitos.Item12NumNo;
            txtItem13NumYes.Value = tpitos.Item13NumYes;
            txtItem13NumNo.Value = tpitos.Item13NumNo;

            //Red flags
            txtLeadTeacherRedFlagsYes.Value = tpitos.LeadTeacherRedFlagsNumYes;
            txtLeadTeacherRedFlagsPossible.Value = tpitos.LeadTeacherRedFlagsNumPossible;
            txtOtherTeacherRedFlagsYes.Value = tpitos.OtherTeacherRedFlagsNumYes;
            txtOtherTeacherRedFlagsPossible.Value = tpitos.OtherTeacherRedFlagsNumPossible;
            txtClassroomRedFlagsYes.Value = tpitos.ClassroomRedFlagsNumYes;
            txtClassroomRedFlagsPossible.Value = tpitos.ClassroomRedFlagsNumPossible;

            //Red flag types
            foreach (TPITOSRedFlags redFlag in tpitos.TPITOSRedFlags)
            {
                lstBxRedFlags.Items.FindByValue(redFlag.RedFlagCodeFK).Selected = true;
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            //Basic information
            deObservationDate.ClientEnabled = enabled;
            teObservationStartTime.ClientEnabled = enabled;
            teObservationEndTime.ClientEnabled = enabled;
            ddClassroom.ClientEnabled = enabled;
            ddObserver.ClientEnabled = enabled;
            txtAdultsBegin.ClientEnabled = enabled;
            txtAdultsEnd.ClientEnabled = enabled;
            txtAdultsEntered.ClientEnabled = enabled;
            txtChildrenBegin.ClientEnabled = enabled;
            txtChildrenEnd.ClientEnabled = enabled;
            txtNotes.ClientEnabled = enabled;

            //Key practices
            txtItem1NumYes.ClientEnabled = enabled;
            txtItem1NumNo.ClientEnabled = enabled;
            txtItem2NumYes.ClientEnabled = enabled;
            txtItem2NumNo.ClientEnabled = enabled;
            txtItem3NumYes.ClientEnabled = enabled;
            txtItem3NumNo.ClientEnabled = enabled;
            txtItem4NumYes.ClientEnabled = enabled;
            txtItem4NumNo.ClientEnabled = enabled;
            txtItem5NumYes.ClientEnabled = enabled;
            txtItem5NumNo.ClientEnabled = enabled;
            txtItem6NumYes.ClientEnabled = enabled;
            txtItem6NumNo.ClientEnabled = enabled;
            txtItem7NumYes.ClientEnabled = enabled;
            txtItem7NumNo.ClientEnabled = enabled;
            txtItem8NumYes.ClientEnabled = enabled;
            txtItem8NumNo.ClientEnabled = enabled;
            txtItem9NumYes.ClientEnabled = enabled;
            txtItem9NumNo.ClientEnabled = enabled;
            txtItem10NumYes.ClientEnabled = enabled;
            txtItem10NumNo.ClientEnabled = enabled;
            txtItem11NumYes.ClientEnabled = enabled;
            txtItem11NumNo.ClientEnabled = enabled;
            txtItem12NumYes.ClientEnabled = enabled;
            txtItem12NumNo.ClientEnabled = enabled;
            txtItem13NumYes.ClientEnabled = enabled;
            txtItem13NumNo.ClientEnabled = enabled;

            //Red flags
            txtLeadTeacherRedFlagsYes.ClientEnabled = enabled;
            txtLeadTeacherRedFlagsPossible.ClientEnabled = enabled;
            txtOtherTeacherRedFlagsYes.ClientEnabled = enabled;
            txtOtherTeacherRedFlagsPossible.ClientEnabled = enabled;
            txtClassroomRedFlagsYes.ClientEnabled = enabled;
            txtClassroomRedFlagsPossible.ClientEnabled = enabled;
            lstBxRedFlags.ReadOnly = !enabled;
        }

        #endregion

        #region Custom Validation

        /// <summary>
        /// This method fires when the validation for the deObservationDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deObservationDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deObservationDate_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the observation date
            DateTime? observationDate = (deObservationDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deObservationDate.Value));

            //Perform validation
            if (!observationDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Observation Date is required!";
            }
            else if (observationDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Observation Date cannot be in the future!";
            }
            else if (currentTPITOSPK > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the validated participants
                    var validatedParticipants = context.spValidateTPITOSParticipants(currentTPITOSPK, observationDate).ToList();

                    //Get the invalid participants from the list
                    List<String> lstInvalidParticipantNames = validatedParticipants
                                                                .Where(vp => vp.IsValid == false)
                                                                .Select(vp => vp.EmployeeName)
                                                                .ToList();

                    //Tell the user if there are invalid participants
                    if (lstInvalidParticipantNames.Count > 0)
                    {
                        //Set the validation message
                        e.IsValid = false;
                        e.ErrorText = "At least one TPITOS participant would be invalidated by this observation date!  See alert message for details.";

                        //Get a comma-separated list of the employee names
                        string invalidParticipants = string.Join(", ", lstInvalidParticipantNames);

                        //Show the user a message that tells them what employees are invalid
                        msgSys.ShowMessageToUser("danger", "Invalid Observation Date", "The following Employees were not active in a Teacher or TA job function as of the new observation date: " + invalidParticipants, 25000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the teObservationStartTime DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The teObservationStartTime DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void teObservationStartTime_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the observation date
            DateTime? observationDate = Convert.ToDateTime(deObservationDate.Value);

            //Get the start and end time
            DateTime? startTime = (teObservationStartTime.Value == null ? (DateTime?)null : Convert.ToDateTime(teObservationStartTime.Value));
            DateTime? endTime = (teObservationEndTime.Value == null ? (DateTime?)null : Convert.ToDateTime(teObservationEndTime.Value));

            //Perform validation
            if (startTime.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Time is required!";
            }
            else if (endTime.HasValue && startTime >= endTime)
            {
                e.IsValid = false;
                e.ErrorText = "Start Time must be before the End Time!";
            }
            else if (observationDate.HasValue)
            {
                //Calculate the start datetime
                DateTime startDateTime = observationDate.Value.AddHours(startTime.Value.Hour).AddMinutes(startTime.Value.Minute);

                //Make sure the start time is not in the future
                if (startDateTime > DateTime.Now)
                {
                    e.IsValid = false;
                    e.ErrorText = "Start Time cannot be in the future!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the teObservationEndTime DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The teObservationEndTime DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void teObservationEndTime_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the observation date
            DateTime? observationDate = Convert.ToDateTime(deObservationDate.Value);

            //Get the start and end time
            DateTime? startTime = (teObservationStartTime.Value == null ? (DateTime?)null : Convert.ToDateTime(teObservationStartTime.Value));
            DateTime? endTime = (teObservationEndTime.Value == null ? (DateTime?)null : Convert.ToDateTime(teObservationEndTime.Value));


            //Perform validation
            if (endTime.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Start Time is required!";
            }
            else if (startTime >= endTime)
            {
                e.IsValid = false;
                e.ErrorText = "End Time must be after the Start Time!";
            }
            else if(observationDate.HasValue)
            {
                //Calculate the end datetime
                DateTime endDateTime = observationDate.Value.AddHours(endTime.Value.Hour).AddMinutes(endTime.Value.Minute);

                //Make sure the end time is not in the future
                if(endDateTime > DateTime.Now)
                {
                    e.IsValid = false;
                    e.ErrorText = "End Time cannot be in the future!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for one of the DevExpress
        /// Bootstrap TextBoxes in the items repeater fires and it ensures that
        /// the values in that textbox are valid
        /// </summary>
        /// <param name="sender">A BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void TPITOSItemNumbers_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            BootstrapTextBox thisTextBox = (BootstrapTextBox)sender;
            BootstrapTextBox otherTextBox;
            string otherTextBoxID;
            string otherFieldName;

            if (thisTextBox.ID.Contains("Yes"))
            {
                //This is the # Yes textbox, set the other textbox variables
                otherTextBoxID = thisTextBox.ID.Replace("Yes", "No");
                otherTextBox = (BootstrapTextBox)upSubscales.FindControl(otherTextBoxID);
                otherFieldName = "# No";
            }
            else
            {
                //This is the # No textbox, set the other textbox variables
                otherTextBoxID = thisTextBox.ID.Replace("No", "Yes");
                otherTextBox = (BootstrapTextBox)upSubscales.FindControl(otherTextBoxID);
                otherFieldName = "# Yes";
            }

            //Perform validation
            if (thisTextBox.Value == null && otherTextBox.Value != null)
            {
                //This field is not valid
                e.IsValid = false;
                e.ErrorText = "This is required when the " + otherFieldName + " field has a value!";
            }
        }

        /// <summary>
        /// This method fires when the validation for one of the DevExpress
        /// Bootstrap TextBoxes in the red flags repeater fires and it ensures that
        /// the values in that textbox are valid
        /// </summary>
        /// <param name="sender">A BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void TPITOSRedFlagNumbers_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            BootstrapTextBox thisTextBox = (BootstrapTextBox)sender;
            BootstrapTextBox otherTextBox;
            string otherTextBoxID;
            string otherFieldName;
            int thisTextBoxNum;
            int otherTextBoxNum;

            if (thisTextBox.ID.Contains("Yes"))
            {
                //This is the # Yes textbox, set the other textbox variables
                otherTextBoxID = thisTextBox.ID.Replace("Yes", "Possible");
                otherTextBox = (BootstrapTextBox)upSubscales.FindControl(otherTextBoxID);
                otherFieldName = "# Possible";
            }
            else
            {
                //This is the # Possible textbox, set the other textbox variables
                otherTextBoxID = thisTextBox.ID.Replace("Possible", "Yes");
                otherTextBox = (BootstrapTextBox)upSubscales.FindControl(otherTextBoxID);
                otherFieldName = "# Yes";
            }

            //Perform validation
            if (thisTextBox.Value == null && otherTextBox.Value != null)
            {
                //This field is not valid
                e.IsValid = false;
                e.ErrorText = "This is required when the " + otherFieldName + " field has a value!";
            }
            else if(otherFieldName != "# Possible" && thisTextBox.Value != null & otherTextBox.Value != null)
            {
                //Validate the number possible
                if (int.TryParse(thisTextBox.Value.ToString(), out thisTextBoxNum) 
                    && int.TryParse(otherTextBox.Value.ToString(), out otherTextBoxNum))
                {
                    //Make sure the number possible is over the number yes
                    if(thisTextBoxNum < otherTextBoxNum)
                    {
                        e.IsValid = false;
                        e.ErrorText = "This must be equal to or more than the " + otherFieldName + " field!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddParticipant DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddParticipant ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddParticipant_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the participant pk
            int TPITOSParticipantPK = (string.IsNullOrWhiteSpace(hfAddEditParticipantPK.Value) ? 0 : Convert.ToInt32(hfAddEditParticipantPK.Value));

            //Get the employee and role
            int? employeePK = (ddParticipant.Value == null ? (int?)null : Convert.ToInt32(ddParticipant.Value));
            int? rolePK = (ddParticipantRole.Value == null ? (int?)null : Convert.ToInt32(ddParticipantRole.Value));

            //Perform the validation
            if (employeePK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "TPITOS Participant is required!";
            }
            else if (rolePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the participant row that matches the employee and role, but not the PK
                    TPITOSParticipant thisTPITOSParticipant = context.TPITOSParticipant.AsNoTracking()
                                                .Where(tp => tp.TPITOSFK == currentTPITOS.TPITOSPK
                                                    && tp.ProgramEmployeeFK == employeePK.Value
                                                    && tp.ParticipantTypeCodeFK == rolePK.Value
                                                    && tp.TPITOSParticipantPK != TPITOSParticipantPK)
                                                .FirstOrDefault();

                    //If the employee is already a participant with the selected role, this is invalid
                    if (thisTPITOSParticipant != null)
                    {
                        e.IsValid = false;
                        e.ErrorText = "This participant is already recorded for this TPITOS with the selected role!";
                    }
                }
            }
        }

        #endregion
    }
}