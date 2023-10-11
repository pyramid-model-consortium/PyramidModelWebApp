using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;
using System.Collections.Generic;
using DevExpress.Web.Bootstrap;
using DevExpress.Web;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace Pyramid.Pages
{
    public partial class TPOT : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "TPOT";
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
        private Models.TPOT currentTPOT;
        private int currentTPOTPK = 0;
        private int currentProgramFK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the TPOT PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["TPOTPK"]))
            {
                int.TryParse(Request.QueryString["TPOTPK"], out currentTPOTPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentTPOTPK == 0 && !string.IsNullOrWhiteSpace(hfTPOTPK.Value))
            {
                int.TryParse(hfTPOTPK.Value, out currentTPOTPK);
            }

            //Check to see if this is an edit
            isEdit = currentTPOTPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/TPOTDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the TPOT from the database
                currentTPOT = context.TPOT.AsNoTracking()
                                    .Include(t => t.Classroom)
                                    .Include(t => t.Classroom.Program)
                                    .Include(t => t.TPOTRedFlags)
                                    .Include(t => t.TPOTBehaviorResponses)
                                    .Where(a => a.TPOTPK == currentTPOTPK)
                                    .FirstOrDefault();

                //Check to see if the TPOT from the database exists
                if (currentTPOT == null)
                {
                    //The TPOT from the database doesn't exist, set the current TPOT to a default value
                    currentTPOT = new Models.TPOT();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentTPOT.Classroom.Program.ProgramName;
                }
            }

            //Prevent users from viewing TPOTs from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentTPOT.Classroom.ProgramFK))
            {
                Response.Redirect(string.Format("/Pages/TPOTDashboard.aspx?messageType={0}", "NOTPOT"));
            }

            //Get the proper program fk
            currentProgramFK = (isEdit ? currentTPOT.Classroom.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

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
                        case "TPOTAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Basic Information successfully added!<br/><br/>More detailed information can now be added.", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Bind the data bound controls
                BindDataBoundControls();

                //Check to see if this is an edit
                if (isEdit)
                {
                    //This is an edit
                    //Populate the page
                    PopulatePage(currentTPOT);

                    //Show the print preview button
                    btnPrintPreview.Visible = true;

                    //Hide the add only message and show the edit only div
                    divAddOnlyMessage.Visible = false;
                    divEditOnly.Visible = true;
                }
                else
                {
                    //Hide the print preview button
                    btnPrintPreview.Visible = false;

                    //Show the add only message and hide the edit only div
                    divAddOnlyMessage.Visible = true;
                    divEditOnly.Visible = false;
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
                if (isEdit == false && action.ToLower() == "add" && FormPermissions.AllowedToAdd)
                {
                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New TPOT Observation";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit TPOT Observation";
                }
                else
                {
                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View TPOT Observation";
                }

                //Set the max value for the observation date
                deObservationDate.MaxDate = DateTime.Now;

                //Set focus on the observation date field
                deObservationDate.Focus();
            }
        }

        #region Click Methods

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitTPOT user control 
        /// </summary>
        /// <param name="sender">The submitTPOT control</param>
        /// <param name="e">The Click event</param>
        protected void submitTPOT_Click(object sender, EventArgs e)
        {
            //To hold the success message type
            string successMessageType = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect differently if add or edit
                if (isEdit)
                {
                    //Redirect the user to the TPOT dashboard
                    Response.Redirect(string.Format("/Pages/TPOTDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    //Redirect the user back to this page with a message and the PK
                    Response.Redirect(string.Format("/Pages/TPOT.aspx?TPOTPK={0}&Action=Edit&messageType={1}",
                                                        currentTPOTPK, successMessageType));
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitTPOT user control 
        /// </summary>
        /// <param name="sender">The submitTPOT control</param>
        /// <param name="e">The Click event</param>
        protected void submitTPOT_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the TPOT dashboard
            Response.Redirect(string.Format("/Pages/TPOTDashboard.aspx?messageType={0}", "TPOTCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitTPOT control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTPOT_ValidationFailed(object sender, EventArgs e)
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
        /// This method executes when the user clicks the add button for the participants
        /// and it opens a div that allows the user to add a participant
        /// </summary>
        /// <param name="sender">The lbAddTPOTParticipant LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTPOTParticipant_Click(object sender, EventArgs e)
        {
            //Clear inputs in the participant div
            hfAddEditParticipantPK.Value = "0";
            ddParticipant.Value = "";
            ddParticipantRole.Value = "";

            //Set the title
            lblAddEditParticipant.Text = "Add TPOT Participant";

            //Show the participant div
            divAddEditTPOTParticipant.Visible = true;

            //Set focus to the participant dropdown
            ddParticipant.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a participant
        /// and it opens the participant edit div so that the user can edit the selected participant
        /// </summary>
        /// <param name="sender">The lbEditTPOTParticipant LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTPOTParticipant_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblParticipantPK = (Label)item.FindControl("lblTPOTParticipantPK");

            //Get the PK from the label
            int? participantPK = (String.IsNullOrWhiteSpace(lblParticipantPK.Text) ? (int?)null : Convert.ToInt32(lblParticipantPK.Text));

            if (participantPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the participant to edit
                    TPOTParticipant editParticipant = context.TPOTParticipant.AsNoTracking().Where(cn => cn.TPOTParticipantPK == participantPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditParticipant.Text = "Edit TPOT Participant";
                    ddParticipant.SelectedItem = ddParticipant.Items.FindByValue(editParticipant.ProgramEmployeeFK);
                    ddParticipantRole.SelectedItem = ddParticipantRole.Items.FindByValue(editParticipant.ParticipantTypeCodeFK);
                    hfAddEditParticipantPK.Value = participantPK.Value.ToString();
                }

                //Show the participant div
                divAddEditTPOTParticipant.Visible = true;

                //Set focus to the participant dropdown
                ddParticipant.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected TPOT participant!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the  participants
        /// and it saves the participant information to the database
        /// </summary>
        /// <param name="sender">The btnSaveTPOTParticipant DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitParticipant_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit the TPOT
            if (FormPermissions.AllowedToEdit)
            {
                //Get the participant pk
                int participantPK = Convert.ToInt32(hfAddEditParticipantPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    TPOTParticipant currentParticipant;
                    //Check to see if this is an add or an edit
                    if (participantPK == 0)
                    {
                        //Add
                        currentParticipant = new TPOTParticipant();
                        currentParticipant.TPOTFK = currentTPOT.TPOTPK;
                        currentParticipant.ProgramEmployeeFK = Convert.ToInt32(ddParticipant.Value);
                        currentParticipant.ParticipantTypeCodeFK = Convert.ToInt32(ddParticipantRole.Value);
                        currentParticipant.CreateDate = DateTime.Now;
                        currentParticipant.Creator = User.Identity.Name;

                        //Save to the database
                        context.TPOTParticipant.Add(currentParticipant);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added TPOT participant!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentParticipant = context.TPOTParticipant.Find(participantPK);
                        currentParticipant.TPOTFK = currentTPOT.TPOTPK;
                        currentParticipant.ProgramEmployeeFK = Convert.ToInt32(ddParticipant.Value);
                        currentParticipant.ParticipantTypeCodeFK = Convert.ToInt32(ddParticipantRole.Value);
                        currentParticipant.EditDate = DateTime.Now;
                        currentParticipant.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited TPOT participant!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditParticipantPK.Value = "0";
                    divAddEditTPOTParticipant.Visible = false;

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
        /// <param name="sender">The btnDeleteTPOTParticipant LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTPOTParticipant_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit the TPOT
            if (FormPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteTPOTParticipantPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTPOTParticipantPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the participant to remove
                            TPOTParticipant participantToRemove = context.TPOTParticipant.Where(cn => cn.TPOTParticipantPK == rowToRemovePK).FirstOrDefault();

                            //Remove the participant
                            context.TPOTParticipant.Remove(participantToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.TPOTParticipantChanged
                                    .OrderByDescending(tpc => tpc.TPOTParticipantChangedPK)
                                    .Where(tpc => tpc.TPOTParticipantPK == participantToRemove.TPOTParticipantPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Rebind the participant repeater
                            BindParticipants();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted TPOT participant!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the TPOT participant, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the TPOT participant!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the TPOT participant!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the TPOT participant to delete!", 120000);
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
            divAddEditTPOTParticipant.Visible = false;
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

            //Set focus to the observation date
            deObservationDate.Focus();
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
                var allTPOTRedFlags = context.CodeTPOTRedFlag.AsNoTracking()
                                            .OrderBy(ctrf => ctrf.OrderBy)
                                            .ToList();
                lstBxRedFlags.DataSource = allTPOTRedFlags;
                lstBxRedFlags.DataBind();

                //Bind the essential strategies dropdown
                var allEssentialStrategiesUsed = context.CodeEssentialStrategiesUsed.AsNoTracking()
                                                    .OrderBy(cesu => cesu.OrderBy)
                                                    .ToList();
                ddEssentialStrategiesUsed.DataSource = allEssentialStrategiesUsed;
                ddEssentialStrategiesUsed.DataBind();

                //Bind the behavior response list box
                var allBehaviorResponses = context.CodeTPOTBehaviorResponse.AsNoTracking()
                                                .OrderBy(ctbr => ctbr.OrderBy)
                                                .ToList();
                lstBxBehaviorResponses.DataSource = allBehaviorResponses;
                lstBxBehaviorResponses.DataBind();
            }

            //Bind other controls based on the pk
            if (isEdit)
            {
                //This is an edit
                //Bind the observer and participant dropdown
                BindObserverDropDown(currentTPOT.ObservationStartDateTime.Date, currentProgramFK, currentTPOT.ObserverFK);
                BindParticipantDropDown(currentTPOT.ObservationStartDateTime.Date, currentProgramFK, currentTPOT.ObserverFK);

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
                    string observerTrainings = (((int)Utilities.TrainingFKs.TPOT_OBSERVER).ToString() + ",");
                    var allObservers = context.spGetAllObservers(programFK, observationDate, observerTrainings).Select(ob => new {
                        ob.ProgramEmployeePK,
                        ob.ObserverID,
                        ObserverName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + ob.ObserverID + ") " + ob.ObserverName : ob.ObserverID)
                    }).ToList();

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
                    var allParticipants = (from pe in context.ProgramEmployee
                                                    .Include(pe => pe.Employee)
                                                    .Include(pe => pe.JobFunction)
                                                    .AsNoTracking()
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
                                               pe.ProgramSpecificID,
                                               ParticipantName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName : pe.ProgramSpecificID)
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
                var allTPOTParticipants = context.TPOTParticipant.AsNoTracking()
                                            .Include(tp => tp.ProgramEmployee)
                                            .Include(tp => tp.ProgramEmployee.Employee)
                                            .Include(tp => tp.CodeParticipantType)
                                            .Where(tp => tp.TPOTFK == currentTPOT.TPOTPK)
                                            .OrderBy(tp => tp.ProgramEmployee.Employee.FirstName)
                                            .ThenBy(tp => tp.ProgramEmployee.Employee.LastName)
                                            .Select(tp => new
                                            {
                                                tp.TPOTParticipantPK,
                                                tp.ProgramEmployeeFK,
                                                tp.TPOTFK,
                                                tp.ParticipantTypeCodeFK,
                                                ParticipantName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + tp.ProgramEmployee.ProgramSpecificID + ") " + tp.ProgramEmployee.Employee.FirstName + " " + tp.ProgramEmployee.Employee.LastName : tp.ProgramEmployee.ProgramSpecificID),
                                                ParticipantType = tp.CodeParticipantType.Description
                                            })
                                            .ToList();
                repeatParticipants.DataSource = allTPOTParticipants;
                repeatParticipants.DataBind();
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method populates the page controls from the passed TPOT object
        /// </summary>
        /// <param name="TPOTToPopulate">The TPOT object with values to populate the page controls</param>
        private void PopulatePage(Models.TPOT tpot)
        {
            //Set the page controls to the values from the object
            //Basic information
            deObservationDate.Value = tpot.ObservationStartDateTime.Date;
            teObservationStartTime.Value = tpot.ObservationStartDateTime;
            teObservationEndTime.Value = tpot.ObservationEndDateTime;
            ddClassroom.SelectedItem = ddClassroom.Items.FindByValue(tpot.ClassroomFK);
            ddObserver.SelectedItem = ddObserver.Items.FindByValue(tpot.ObserverFK);
            txtAdultsBegin.Value = tpot.NumAdultsBegin;
            txtAdultsEnd.Value = tpot.NumAdultsEnd;
            txtAdultsEntered.Value = tpot.NumAdultsEntered;
            txtChildrenBegin.Value = tpot.NumKidsBegin;
            txtChildrenEnd.Value = tpot.NumKidsEnd;
            txtNotes.Value = tpot.Notes;

            //Key practices
            txtItem1NumYes.Value = tpot.Item1NumYes;
            txtItem1NumNo.Value = tpot.Item1NumNo;
            txtItem2NumYes.Value = tpot.Item2NumYes;
            txtItem2NumNo.Value = tpot.Item2NumNo;
            txtItem3NumYes.Value = tpot.Item3NumYes;
            txtItem3NumNo.Value = tpot.Item3NumNo;
            txtItem4NumYes.Value = tpot.Item4NumYes;
            txtItem4NumNo.Value = tpot.Item4NumNo;
            txtItem5NumYes.Value = tpot.Item5NumYes;
            txtItem5NumNo.Value = tpot.Item5NumNo;
            txtItem6NumYes.Value = tpot.Item6NumYes;
            txtItem6NumNo.Value = tpot.Item6NumNo;
            txtItem7NumYes.Value = tpot.Item7NumYes;
            txtItem7NumNo.Value = tpot.Item7NumNo;
            txtItem8NumYes.Value = tpot.Item8NumYes;
            txtItem8NumNo.Value = tpot.Item8NumNo;
            txtItem9NumYes.Value = tpot.Item9NumYes;
            txtItem9NumNo.Value = tpot.Item9NumNo;
            txtItem10NumYes.Value = tpot.Item10NumYes;
            txtItem10NumNo.Value = tpot.Item10NumNo;
            txtItem11NumYes.Value = tpot.Item11NumYes;
            txtItem11NumNo.Value = tpot.Item11NumNo;
            txtItem12NumYes.Value = tpot.Item12NumYes;
            txtItem12NumNo.Value = tpot.Item12NumNo;
            txtItem13NumYes.Value = tpot.Item13NumYes;
            txtItem13NumNo.Value = tpot.Item13NumNo;
            txtItem14NumYes.Value = tpot.Item14NumYes;
            txtItem14NumNo.Value = tpot.Item14NumNo;

            //Red flags
            txtRedFlagsNumYes.Value = tpot.RedFlagsNumYes;
            txtRedFlagsNumNo.Value = tpot.RedFlagsNumNo;

            //Red flag types
            foreach (TPOTRedFlags redFlag in tpot.TPOTRedFlags)
            {
                lstBxRedFlags.Items.FindByValue(redFlag.RedFlagCodeFK).Selected = true;
            }

            //Behavior Responses
            txtChallengingBehaviorsNumObserved.Value = tpot.ChallengingBehaviorsNumObserved;
            ddEssentialStrategiesUsed.SelectedItem = ddEssentialStrategiesUsed.Items.FindByValue(tpot.EssentialStrategiesUsedCodeFK);
            txtAdditionalStrategiesNumUsed.Value = tpot.AdditionalStrategiesNumUsed;

            //Response types
            foreach (TPOTBehaviorResponses br in tpot.TPOTBehaviorResponses)
            {
                lstBxBehaviorResponses.Items.FindByValue(br.BehaviorResponseCodeFK).Selected = true;
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
            txtItem14NumYes.ClientEnabled = enabled;
            txtItem14NumNo.ClientEnabled = enabled;

            //Red flags
            txtRedFlagsNumYes.ClientEnabled = enabled;
            txtRedFlagsNumNo.ClientEnabled = enabled;
            lstBxRedFlags.ReadOnly = !enabled;

            //Responses to challenging behavior
            txtChallengingBehaviorsNumObserved.ClientEnabled = enabled;
            ddEssentialStrategiesUsed.ClientEnabled = enabled;
            txtAdditionalStrategiesNumUsed.ClientEnabled = enabled;
            lstBxBehaviorResponses.ReadOnly = !enabled;

            //Show/hide the submit button
            submitTPOT.ShowSubmitButton = enabled;
            submitParticipant.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            bool useCancelConfirmations = enabled && areConfirmationsEnabled;

            submitTPOT.UseCancelConfirm = useCancelConfirmations;
            submitParticipant.UseCancelConfirm = useCancelConfirmations;
        }

        /// <summary>
        /// This method prints the form
        /// </summary>
        private void PrintForm()
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitTPOT.ValidationGroup))
            {
                //Submit the form
                SaveForm(false);

                //Only print on edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptTPOT report = new Reports.PreBuiltReports.FormReports.RptTPOT();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "TPOT Observation", currentTPOTPK);
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
            //To hold the success message type
            string successMessageType = null;

            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the TPOT fields from the observation
                //Calculate the start and end datetimes
                DateTime startDateTime = Convert.ToDateTime(deObservationDate.Value);
                DateTime endDateTime = Convert.ToDateTime(deObservationDate.Value);
                DateTime startTime = Convert.ToDateTime(teObservationStartTime.Value);
                DateTime endTime = Convert.ToDateTime(teObservationEndTime.Value);
                startDateTime = startDateTime.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
                endDateTime = endDateTime.AddHours(endTime.Hour).AddMinutes(endTime.Minute);

                //Basic information
                currentTPOT.ObservationStartDateTime = startDateTime;
                currentTPOT.ObservationEndDateTime = endDateTime;
                currentTPOT.ClassroomFK = Convert.ToInt32(ddClassroom.Value);
                currentTPOT.ObserverFK = Convert.ToInt32(ddObserver.Value);
                currentTPOT.NumAdultsBegin = Convert.ToInt32(txtAdultsBegin.Value);
                currentTPOT.NumAdultsEnd = Convert.ToInt32(txtAdultsEnd.Value);
                currentTPOT.NumAdultsEntered = Convert.ToInt32(txtAdultsEntered.Value);
                currentTPOT.NumKidsBegin = Convert.ToInt32(txtChildrenBegin.Value);
                currentTPOT.NumKidsEnd = Convert.ToInt32(txtChildrenEnd.Value);
                currentTPOT.Notes = (txtNotes.Value == null ? null : txtNotes.Value.ToString());

                //Key practices
                currentTPOT.Item1NumYes = (txtItem1NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem1NumYes.Value));
                currentTPOT.Item1NumNo = (txtItem1NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem1NumNo.Value));
                currentTPOT.Item2NumYes = (txtItem2NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem2NumYes.Value));
                currentTPOT.Item2NumNo = (txtItem2NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem2NumNo.Value));
                currentTPOT.Item3NumYes = (txtItem3NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem3NumYes.Value));
                currentTPOT.Item3NumNo = (txtItem3NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem3NumNo.Value));
                currentTPOT.Item4NumYes = (txtItem4NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem4NumYes.Value));
                currentTPOT.Item4NumNo = (txtItem4NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem4NumNo.Value));
                currentTPOT.Item5NumYes = (txtItem5NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem5NumYes.Value));
                currentTPOT.Item5NumNo = (txtItem5NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem5NumNo.Value));
                currentTPOT.Item6NumYes = (txtItem6NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem6NumYes.Value));
                currentTPOT.Item6NumNo = (txtItem6NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem6NumNo.Value));
                currentTPOT.Item7NumYes = (txtItem7NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem7NumYes.Value));
                currentTPOT.Item7NumNo = (txtItem7NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem7NumNo.Value));
                currentTPOT.Item8NumYes = (txtItem8NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem8NumYes.Value));
                currentTPOT.Item8NumNo = (txtItem8NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem8NumNo.Value));
                currentTPOT.Item9NumYes = (txtItem9NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem9NumYes.Value));
                currentTPOT.Item9NumNo = (txtItem9NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem9NumNo.Value));
                currentTPOT.Item10NumYes = (txtItem10NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem10NumYes.Value));
                currentTPOT.Item10NumNo = (txtItem10NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem10NumNo.Value));
                currentTPOT.Item11NumYes = (txtItem11NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem11NumYes.Value));
                currentTPOT.Item11NumNo = (txtItem11NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem11NumNo.Value));
                currentTPOT.Item12NumYes = (txtItem12NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem12NumYes.Value));
                currentTPOT.Item12NumNo = (txtItem12NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem12NumNo.Value));
                currentTPOT.Item13NumYes = (txtItem13NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem13NumYes.Value));
                currentTPOT.Item13NumNo = (txtItem13NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem13NumNo.Value));
                currentTPOT.Item14NumYes = (txtItem14NumYes.Value == null ? (int?)null : Convert.ToInt32(txtItem14NumYes.Value));
                currentTPOT.Item14NumNo = (txtItem14NumNo.Value == null ? (int?)null : Convert.ToInt32(txtItem14NumNo.Value));

                //Red flags
                currentTPOT.RedFlagsNumYes = (txtRedFlagsNumYes.Value == null ? (int?)null : Convert.ToInt32(txtRedFlagsNumYes.Value));
                currentTPOT.RedFlagsNumNo = (txtRedFlagsNumNo.Value == null ? (int?)null : Convert.ToInt32(txtRedFlagsNumNo.Value));

                //Responses to challenging behavior
                currentTPOT.ChallengingBehaviorsNumObserved = (txtChallengingBehaviorsNumObserved.Value == null ? (int?)null : Convert.ToInt32(txtChallengingBehaviorsNumObserved.Value));
                currentTPOT.EssentialStrategiesUsedCodeFK = (ddEssentialStrategiesUsed.Value == null ? (int?)null : Convert.ToInt32(ddEssentialStrategiesUsed.Value));
                currentTPOT.AdditionalStrategiesNumUsed = (txtAdditionalStrategiesNumUsed.Value == null ? (int?)null : Convert.ToInt32(txtAdditionalStrategiesNumUsed.Value));

                if (isEdit)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "TPOTEdited";

                        //Set the edit-only fields
                        currentTPOT.Editor = User.Identity.Name;
                        currentTPOT.EditDate = DateTime.Now;

                        //Clear the red flag type rows
                        var currentRedFlags = context.TPOTRedFlags.Where(trf => trf.TPOTFK == currentTPOTPK).ToList();
                        context.TPOTRedFlags.RemoveRange(currentRedFlags);

                        //Save the red flag types
                        foreach (BootstrapListEditItem item in lstBxRedFlags.Items)
                        {
                            if (item.Selected)
                            {
                                TPOTRedFlags newRedFlag = new TPOTRedFlags();
                                newRedFlag.CreateDate = DateTime.Now;
                                newRedFlag.Creator = User.Identity.Name;
                                newRedFlag.RedFlagCodeFK = Convert.ToInt32(item.Value);
                                newRedFlag.TPOTFK = currentTPOTPK;
                                context.TPOTRedFlags.Add(newRedFlag);
                            }
                        }

                        //Clear the responses to challenging behavior
                        var currentBehaviorResponses = context.TPOTBehaviorResponses.Where(tbr => tbr.TPOTFK == currentTPOTPK).ToList();
                        context.TPOTBehaviorResponses.RemoveRange(currentBehaviorResponses);

                        //Save the responses to challenging behavior
                        foreach (BootstrapListEditItem item in lstBxBehaviorResponses.Items)
                        {
                            if (item.Selected)
                            {
                                TPOTBehaviorResponses newBehaviorResponse = new TPOTBehaviorResponses();
                                newBehaviorResponse.CreateDate = DateTime.Now;
                                newBehaviorResponse.Creator = User.Identity.Name;
                                newBehaviorResponse.BehaviorResponseCodeFK = Convert.ToInt32(item.Value);
                                newBehaviorResponse.TPOTFK = currentTPOTPK;
                                context.TPOTBehaviorResponses.Add(newBehaviorResponse);
                            }
                        }

                        //Set IsComplete to true because validation passed on the complete form
                        currentTPOT.IsComplete = true;

                        //Get the existing TPOT record
                        Models.TPOT existingTPOT = context.TPOT.Find(currentTPOT.TPOTPK);

                        //Overwrite the existing TPOT record with the values from the observation
                        context.Entry(existingTPOT).CurrentValues.SetValues(currentTPOT);
                        context.SaveChanges();

                        //To hold the change rows
                        List<TPOTRedFlagsChanged> redFlagsChangeRows;
                        List<TPOTBehaviorResponsesChanged> behaviorResponseChangeRows;

                        //Check the red flag deletions
                        if (currentRedFlags.Count > 0)
                        {
                            //Get the red flag change rows and set the deleter
                            redFlagsChangeRows = context.TPOTRedFlagsChanged.Where(trfc => trfc.TPOTFK == currentTPOT.TPOTPK)
                                                            .OrderByDescending(trfc => trfc.TPOTRedFlagsChangedPK)
                                                            .Take(currentRedFlags.Count).ToList()
                                                            .Select(trfc => { trfc.Deleter = User.Identity.Name; return trfc; }).ToList();
                        }

                        //Check the behavior response deletions
                        if (currentBehaviorResponses.Count > 0)
                        {
                            //Get the behavior response change rows and set the deleter
                            behaviorResponseChangeRows = context.TPOTBehaviorResponsesChanged.Where(tbrc => tbrc.TPOTFK == currentTPOT.TPOTPK)
                                                            .OrderByDescending(tbrc => tbrc.TPOTBehaviorResponsesChangedPK)
                                                            .Take(currentBehaviorResponses.Count).ToList()
                                                            .Select(tbrc => { tbrc.Deleter = User.Identity.Name; return tbrc; }).ToList();
                        }

                        //Save the delete row changes to the database
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfTPOTPK.Value = currentTPOT.TPOTPK.ToString();
                        currentTPOTPK = currentTPOT.TPOTPK;
                    }
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "TPOTAdded";

                        //Set the create-only fields
                        currentTPOT.Creator = User.Identity.Name;
                        currentTPOT.CreateDate = DateTime.Now;

                        //Set IsComplete to false because we can't validate the entire form yet
                        currentTPOT.IsComplete = false;

                        //Add the TPOT to the database
                        context.TPOT.Add(currentTPOT);
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfTPOTPK.Value = currentTPOT.TPOTPK.ToString();
                        currentTPOTPK = currentTPOT.TPOTPK;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Unauthorized!", "You are not authorized to make changes!", 20000);
            }

            //Return the success message type
            return successMessageType;
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
            else if (isEdit)
            {
                //Check to see if any participants exist
                if(repeatParticipants.Items.Count > 0) 
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the validated participants
                        var validatedParticipants = context.spValidateTPOTParticipants(currentTPOTPK, observationDate).ToList();

                        //Get the invalid participants from the list
                        List<String> lstInvalidParticipantNames = validatedParticipants
                                                                    .Where(vp => vp.IsValid == false)
                                                                    .Select(vp => (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + vp.EmployeeID + ") " + vp.EmployeeName : vp.EmployeeID))
                                                                    .ToList();

                        //Tell the user if there are invalid participants
                        if (lstInvalidParticipantNames.Count > 0)
                        {
                            //Set the validation message
                            e.IsValid = false;
                            e.ErrorText = "At least one TPOT participant would be invalidated by this observation date!  See alert message for details.";

                            //Get a comma-separated list of the employee names
                            string invalidParticipants = string.Join(", ", lstInvalidParticipantNames);

                            //Show the user a message that tells them what employees are invalid
                            msgSys.ShowMessageToUser("danger", "Invalid Observation Date", "The following Professionals were not active in a Teacher or TA job function as of the new observation date: " + invalidParticipants, 25000);
                        }
                    }
                }
                else
                {
                    //Participants are required
                    e.IsValid = false;
                    e.ErrorText = "You must add at least one TPOT participant below!";

                    //Show an error message
                    msgSys.ShowMessageToUser("danger", "TPOT Participants Required!", "You must add at least one TPOT participant!", 22000);
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
                e.ErrorText = "End Time is required!";
            }
            else if (startTime >= endTime)
            {
                e.IsValid = false;
                e.ErrorText = "End Time must be after the Start Time!";
            }
            else if (observationDate.HasValue)
            {
                //Calculate the end datetime
                DateTime endDateTime = observationDate.Value.AddHours(endTime.Value.Hour).AddMinutes(endTime.Value.Minute);

                //Make sure the end time is not in the future
                if (endDateTime > DateTime.Now)
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
        protected void TPOTItemNumbers_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
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
        protected void TPOTRedFlagNumbers_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
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
        /// This method fires when the validation for the ddParticipant DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddParticipant ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddParticipant_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the participant pk
            int TPOTParticipantPK = (string.IsNullOrWhiteSpace(hfAddEditParticipantPK.Value) ? 0 : Convert.ToInt32(hfAddEditParticipantPK.Value));

            //Get the employee and role
            int? employeePK = (ddParticipant.Value == null ? (int?)null : Convert.ToInt32(ddParticipant.Value));
            int? rolePK = (ddParticipantRole.Value == null ? (int?)null : Convert.ToInt32(ddParticipantRole.Value));

            //Perform the validation
            if (employeePK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "TPOT Participant is required!";
            }
            else if (rolePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the participant row that matches the employee and role, but not the PK
                    TPOTParticipant thisTPOTParticipant = context.TPOTParticipant.AsNoTracking()
                                                .Where(tp => tp.TPOTFK == currentTPOT.TPOTPK
                                                    && tp.ProgramEmployeeFK == employeePK.Value
                                                    && tp.ParticipantTypeCodeFK == rolePK.Value
                                                    && tp.TPOTParticipantPK != TPOTParticipantPK)
                                                .FirstOrDefault();

                    //If the employee is already a participant with the selected role, this is invalid
                    if (thisTPOTParticipant != null)
                    {
                        e.IsValid = false;
                        e.ErrorText = "This participant is already recorded for this TPOT with the selected role!";
                    }
                }
            }
        }

        #endregion
    }
}