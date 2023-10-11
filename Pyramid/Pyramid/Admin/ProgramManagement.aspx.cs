using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Pyramid.Code;
using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using System.IO;
using DevExpress.XtraRichEdit.Import.Html;
using DevExpress.XtraEditors.Filtering.Templates.DateTimeRange;

namespace Pyramid.Admin
{
    public partial class ProgramManagement : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's selected role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow non-admins to use the page
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN)
            {
                //Kick out any non-admins
                Response.Redirect("/Default.aspx");
            }

            //Hide the states if the user is not a super admin
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
            {
                divStates.Visible = false;
            }

            if (!IsPostBack)
            {
                //Bind the data-bound controls
                BindCohorts();
                BindHubs();
                BindPrograms();
                BindStates();
                BindStateProgramCounts();


                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program types
                    var programTypes = context.CodeProgramType.AsNoTracking().OrderBy(cpt => cpt.OrderBy).ToList();
                    lstBxProgramType.DataSource = programTypes;
                    lstBxProgramType.DataBind();

                    //Get the program statuses
                    List<CodeProgramStatus> allStatuses = context.CodeProgramStatus
                                        .OrderBy(cps => cps.OrderBy)
                                        .AsNoTracking()
                                        .ToList();

                    //Bind the status drop-downs
                    ddProgramInitialStatus.DataSource = allStatuses;
                    ddProgramStatus.DataSource = allStatuses;
                    ddProgramStatus.DataBind();
                    ddProgramInitialStatus.DataBind();

                }

                //Prevent future dates in certain date edit fields
                deConfidentialityChangeDate.MaxDate = DateTime.Now;
                deCohortStartDate.MaxDate = DateTime.Now;
                deProgramStartDate.MaxDate = DateTime.Now;

                //Use cancel confirmations if the customization option for cancel confirmations is true (default to true)
                bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
                bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
                submitState.UseCancelConfirm = areConfirmationsEnabled;
                submitHub.UseCancelConfirm = areConfirmationsEnabled;
                submitCohort.UseCancelConfirm = areConfirmationsEnabled;
                submitProgram.UseCancelConfirm = areConfirmationsEnabled;
            }
        }


        /// <summary>
        /// This method calculates the max number of programs for the current state and the
        /// number of programs currently in the current state.
        /// </summary>
        private void BindStateProgramCounts()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the current state row
                State currentState = context.State.AsNoTracking().Where(s => s.StatePK == currentProgramRole.CurrentStateFK.Value).FirstOrDefault();

                //Get the count of active programs for the current state
                int programsInState = context.Program.AsNoTracking()
                                            .Where(p => p.StateFK == currentProgramRole.CurrentStateFK.Value &&
                                                        (p.ProgramEndDate.HasValue == false || p.ProgramEndDate.Value > DateTime.Now))
                                            .Count();

                //Set the hidden field values
                hfStateProgramCount.Value = programsInState.ToString();
                hfMaxStateProgramCount.Value = currentState.MaxNumberOfPrograms.ToString();
            }
        }

        /// <summary>
        /// Bind the state repeater and dropdowns
        /// </summary>
        private void BindStates()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all allowed states
                List<State> allStates = context.State
                                    .Include(s => s.Program)
                                    .Where(s => currentProgramRole.StateFKs.Contains(s.StatePK))
                                    .OrderBy(s => s.Name)
                                    .AsNoTracking()
                                    .ToList();

                //Bind the state drop-downs
                ddProgramState.DataSource = allStates;
                ddProgramState.DataBind();

                ddHubState.DataSource = allStates;
                ddHubState.DataBind();

                ddCohortState.DataSource = allStates;
                ddCohortState.DataBind();

                //Bind the state gridview
                bsGRStates.DataBind();
            }
        }

        /// <summary>
        /// Bind the cohort repeater and dropdowns
        /// </summary>
        private void BindCohorts()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all cohorts the user is allowed to see
                List<Cohort> allCohorts = context.Cohort
                                    .Include(c => c.Program)
                                    .Where(c => currentProgramRole.CohortFKs.Contains(c.CohortPK))
                                    .OrderBy(c => c.CohortName)
                                    .AsNoTracking()
                                    .ToList();

                //Bind the cohort gridview
                bsGRCohort.DataBind();

                //Bind the cohort drop-down
                ddProgramCohort.DataSource = allCohorts;
                ddProgramCohort.DataBind();
            }
        }

        /// <summary>
        /// Bind the hub repeater and dropdowns
        /// </summary>
        private void BindHubs()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all allowed hubs
                List<Hub> allHubs = context.Hub
                                        .Include(h => h.Program)
                                        .Where(h => currentProgramRole.HubFKs.Contains(h.HubPK))
                                        .OrderBy(h => h.Name)
                                        .AsNoTracking()
                                        .ToList();

                //Bind the hub gridview
                bsGRHubs.DataBind();

                //Bind the hub drop-down
                ddProgramHub.DataSource = allHubs;
                ddProgramHub.DataBind();
            }
        }

        /// <summary>
        /// Bind the programs repeater
        /// </summary>
        private void BindPrograms()
        {
            bsGRPrograms.DataBind();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the states
        /// and it opens a div that allows the user to add a state
        /// </summary>
        /// <param name="sender">The lbAddState LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddState_Click(object sender, EventArgs e)
        {
            //Clear inputs in the state div
            hfAddEditStatePK.Value = "0";
            txtStateAbbreviation.Value = "";
            txtStateName.Value = "";
            txtStateLogoFilePath.Value = "";
            txtStateThumbnailLogoFilePath.Value = "";
            ddStateHomePageLogoOption.Value = "";
            txtStateCatchphrase.Value = "";
            txtStateDisclaimer.Value = "";
            ddStateConfidentialityEnabled.Value = true;
            ddStateShareDataNationally.Value = false;
            ddStateUtilizingPIDS.Value = false;
            ddStateLockEndedPrograms.Value = true;
            ddUpdateConfidentialityDocument.Value = true;
            deConfidentialityChangeDate.Value = null;
            txtStateMaxNumberOfPrograms.Value = null;

            //Set the title
            lblAddEditState.Text = "Add State";

            //Show the state div
            divAddEditState.Visible = true;

            //Set focus to the state abbreviation field
            txtStateAbbreviation.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for the states
        /// and it opens the state edit div so that the user can edit the selected state
        /// </summary>
        /// <param name="sender">The lbEditState LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditState_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the container item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfStatePK = (HiddenField)item.FindControl("hfStatePK");

            //Get the PK from the hidden field
            int? statePK = (String.IsNullOrWhiteSpace(hfStatePK.Value) ? (int?)null : Convert.ToInt32(hfStatePK.Value));

            if (statePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the state to edit
                    State editState = context.State.Find(statePK.Value);

                    //Fill the inputs
                    lblAddEditState.Text = "Edit State";
                    txtStateAbbreviation.Value = editState.Abbreviation;
                    txtStateName.Value = editState.Name;
                    txtStateLogoFilePath.Value = editState.LogoFilename;
                    txtStateThumbnailLogoFilePath.Value = editState.ThumbnailLogoFilename;
                    ddStateHomePageLogoOption.Value = editState.HomePageLogoOption;
                    txtStateCatchphrase.Value = editState.Catchphrase;
                    txtStateDisclaimer.Value = editState.Disclaimer;
                    ddStateConfidentialityEnabled.Value = editState.ConfidentialityEnabled;
                    ddStateShareDataNationally.Value = editState.ShareDataNationally;
                    ddStateUtilizingPIDS.Value = editState.UtilizingPIDS;
                    ddStateLockEndedPrograms.Value = editState.LockEndedPrograms;
                    ddUpdateConfidentialityDocument.Value = false;
                    deConfidentialityChangeDate.Value = editState.ConfidentialityChangeDate;
                    txtStateMaxNumberOfPrograms.Value = editState.MaxNumberOfPrograms;
                    hfAddEditStatePK.Value = statePK.Value.ToString();

                    //Show/hide the current document link
                    if (!string.IsNullOrWhiteSpace(editState.ConfidentialityFilename))
                    {
                        //Show the current document link
                        bsLnkCurrentDocument.Visible = true;

                        //Set the navigation URL for the current document link
                        bsLnkCurrentDocument.NavigateUrl = string.Format("/Pages/ViewFile.aspx?StatePK={0}", editState.StatePK.ToString());
                    }
                    else
                    {
                        //Hide the current document link
                        bsLnkCurrentDocument.Visible = false;
                    }
                }

                //Show the state div
                divAddEditState.Visible = true;

                //Set focus to the state name field
                txtStateName.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected state!", 20000);

            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the states
        /// and it saves the state information to the database
        /// </summary>
        /// <param name="sender">The submitState control</param>
        /// <param name="e">The Click event</param>
        protected void submitState_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitState.ValidationGroup))
            {
                //To hold the confidentiality agreement file path and name
                string filePath = null, fileName = null;

                //Whether or not confidentiality is enabled
                bool confidentialityEnabled = Convert.ToBoolean(ddStateConfidentialityEnabled.Value);

                //Whether or not the user is updating the confidentiality document
                bool updatingConfidentiality = (confidentialityEnabled ? Convert.ToBoolean(ddUpdateConfidentialityDocument.Value) : false);

                //Get the state PK and name
                int statePK = Convert.ToInt32(hfAddEditStatePK.Value);

                //Get the confidentiality file to upload
                UploadedFile file = bucConfidentialityDocument.UploadedFiles[0];

                //Check to see if the confidentiality file is valid and if updating the file
                if (confidentialityEnabled && updatingConfidentiality && file.ContentLength > 0 && file.IsValid)
                {
                    //Get the actual file name
                    fileName = Path.GetFileNameWithoutExtension(file.FileName) + "-" +
                        Path.GetRandomFileName().Substring(0, 6) +
                        Path.GetExtension(file.FileName);

                    //Upload the file to Azure storage
                    filePath = Utilities.UploadFileToAzureStorage(file.FileBytes, fileName,
                                        Utilities.ConstantAzureStorageContainerName.CONFIDENTIALITY_AGREEMENTS.ToString());
                }

                //Check to see if the user is updating the confidentiality and the file was uploaded/valid
                if (confidentialityEnabled && updatingConfidentiality && string.IsNullOrWhiteSpace(filePath))
                {
                    //The user is updating the confidentiality and the file was not valid, show a message
                    msgSys.ShowMessageToUser("danger", "Validation Error", "No valid confidentiality document was selected to be uploaded!", 120000);
                }
                else
                {
                    //The file was valid
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the state
                        State currentState = context.State.Where(s => s.StatePK == statePK).FirstOrDefault();

                        //Set the state to a new state if it is null
                        if (currentState == null)
                        {
                            currentState = new State();
                        }

                        //Get the confidentiality file name
                        string originalConfidentialityFilename = (currentState != null ? currentState.ConfidentialityFilename : null);

                        //Get a link to the confidentiality file
                        string confidentialityDocumentLink = Utilities.GetFileLinkFromAzureStorage(originalConfidentialityFilename,
                                            true, Utilities.ConstantAzureStorageContainerName.CONFIDENTIALITY_AGREEMENTS.ToString(), 30);

                        //Make sure there is a confidentiality document if confidentiality is enabled
                        if (confidentialityEnabled == true
                            && updatingConfidentiality == false
                            && (string.IsNullOrWhiteSpace(originalConfidentialityFilename)
                                    || string.IsNullOrWhiteSpace(confidentialityDocumentLink)))
                        {
                            //Confidentiality is enabled, but there is no document, show a validation message
                            msgSys.ShowMessageToUser("danger", "Validation Error",
                                                        "No confidentiality document currently exists, please add one!", 120000);
                        }
                        else
                        {
                            //Fill the fields
                            currentState.Abbreviation = txtStateAbbreviation.Text;
                            currentState.Name = txtStateName.Text;
                            currentState.LogoFilename = txtStateLogoFilePath.Text;
                            currentState.ThumbnailLogoFilename = txtStateThumbnailLogoFilePath.Text;
                            currentState.HomePageLogoOption = Convert.ToInt32(ddStateHomePageLogoOption.Value);
                            currentState.Catchphrase = (string.IsNullOrWhiteSpace(txtStateCatchphrase.Text) ? null : txtStateCatchphrase.Text);
                            currentState.Disclaimer = (string.IsNullOrWhiteSpace(txtStateDisclaimer.Text) ? null : txtStateDisclaimer.Text);
                            currentState.ConfidentialityEnabled = confidentialityEnabled;
                            currentState.ShareDataNationally = Convert.ToBoolean(ddStateShareDataNationally.Value);
                            currentState.UtilizingPIDS = Convert.ToBoolean(ddStateUtilizingPIDS.Value);
                            currentState.LockEndedPrograms = Convert.ToBoolean(ddStateLockEndedPrograms.Value);
                            currentState.ConfidentialityFilename = (updatingConfidentiality ? fileName : currentState.ConfidentialityFilename);
                            currentState.ConfidentialityChangeDate = (updatingConfidentiality ? Convert.ToDateTime(deConfidentialityChangeDate.Value) : currentState.ConfidentialityChangeDate);
                            currentState.MaxNumberOfPrograms = Convert.ToInt32(txtStateMaxNumberOfPrograms.Value);

                            //Check to see if this is an add or an edit
                            if (statePK == 0)
                            {
                                //Add
                                //Fill the create fields
                                currentState.CreateDate = DateTime.Now;
                                currentState.Creator = User.Identity.Name;

                                //Add the state to the database
                                context.State.Add(currentState);

                                //Create the state settings row
                                StateSettings stateSettings = new StateSettings();
                                stateSettings.CreateDate = DateTime.Now;
                                stateSettings.Creator = "SYSTEM";
                                stateSettings.DueDatesEnabled = false;
                                stateSettings.DueDatesBeginDate = null;
                                stateSettings.DueDatesMonthsStart = null;
                                stateSettings.DueDatesMonthsEnd = null;
                                stateSettings.StateFK = currentState.StatePK;

                                //Add the state settings to the database
                                context.StateSettings.Add(stateSettings);

                                //Save the changes
                                context.SaveChanges();

                                //Add the state to the list of authorized states in session
                                currentProgramRole.StateFKs.Add(currentState.StatePK);
                                Utilities.SetProgramRoleInSession(Session, currentProgramRole);

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully added state!", 10000);
                            }
                            else
                            {
                                //Edit
                                //Fill the edit fields
                                currentState.EditDate = DateTime.Now;
                                currentState.Editor = User.Identity.Name;

                                //Save to the database
                                context.SaveChanges();

                                //Delete the old confidentiality agreement if it exists
                                if (updatingConfidentiality && !string.IsNullOrWhiteSpace(originalConfidentialityFilename))
                                {
                                    Utilities.DeleteFileFromAzureStorage(originalConfidentialityFilename,
                                        Utilities.ConstantAzureStorageContainerName.CONFIDENTIALITY_AGREEMENTS.ToString());
                                }

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully edited state!", 10000);
                            }

                            //Reset the values
                            hfAddEditStatePK.Value = "0";
                            divAddEditState.Visible = false;

                            //Bind the state controls
                            BindStates();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This method executes when the user clicks the delete button for a state
        /// and it deletes the state information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteState LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteState_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteStatePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteStatePK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //To hold the confidentiality file name
                        string confidentialityFileToRemove = null;

                        //Get the state to remove
                        State stateToRemove = context.State.Where(s => s.StatePK == rowToRemovePK).FirstOrDefault();

                        //Get the confidentiality file name
                        confidentialityFileToRemove = stateToRemove.ConfidentialityFilename;

                        //Get the state settings row to remove
                        StateSettings settingsToRemove = context.StateSettings.Where(ss => ss.StateFK == stateToRemove.StatePK).FirstOrDefault();

                        //Remove the state settings row if it exists
                        if (settingsToRemove != null)
                        {
                            context.StateSettings.Remove(settingsToRemove);
                        }

                        //Get the form due date rows to remove
                        List<FormDueDate> formDueDates = context.FormDueDate.Where(fdd => fdd.StateFK == stateToRemove.StatePK).ToList();

                        //Remove the form due date rows if they exist
                        if (formDueDates.Count > 0)
                        {
                            context.FormDueDate.RemoveRange(formDueDates);
                        }

                        //Get the training access rows to remove
                        List<CodeTrainingAccess> trainingAccessRows = context.CodeTrainingAccess.Where(cta => cta.StateFK == stateToRemove.StatePK).ToList();

                        //Remove the form due date rows if they exist
                        if (trainingAccessRows.Count > 0)
                        {
                            context.CodeTrainingAccess.RemoveRange(trainingAccessRows);
                        }

                        //Remove the state
                        context.State.Remove(stateToRemove);

                        //Delete the confidentiality agreement if it exists
                        if (!string.IsNullOrWhiteSpace(stateToRemove.ConfidentialityFilename))
                        {
                            Utilities.DeleteFileFromAzureStorage(stateToRemove.ConfidentialityFilename,
                                Utilities.ConstantAzureStorageContainerName.CONFIDENTIALITY_AGREEMENTS.ToString());
                        }

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Re-bind the state controls
                    BindStates();

                    //Show a success message
                    msgSys.ShowMessageToUser("success", "Success", "Successfully deleted state!", 10000);
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
                            string messageForUser = "";

                            if (errorMessage.Contains("cohort"))
                            {
                                messageForUser = "there are cohorts assigned to this state!";
                            }
                            else if (errorMessage.Contains("hub"))
                            {
                                messageForUser = "there are hubs assigned to this state!";
                            }
                            else if (errorMessage.Contains("program"))
                            {
                                messageForUser = "there are programs assigned to this state!";
                            }
                            else if (errorMessage.Contains("confidentialityagreement"))
                            {
                                messageForUser = "users have accepted the confidentiality agreement for this state!";
                            }
                            else if (errorMessage.Contains("userfileupload"))
                            {
                                messageForUser = "files have been uploaded for this state!";
                            }
                            else if (errorMessage.Contains("newsentry"))
                            {
                                messageForUser = "there are news entries entered for this state!";
                            }
                            else
                            {
                                messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                            }

                            //Show the error message
                            msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the state, {0}", messageForUser), 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the state!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the state!", 120000);
                    }

                    //Log the error
                    Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the state to delete!", 120000);

            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the states
        /// and it closes the state add/edit div
        /// </summary>
        /// <param name="sender">The submitState control</param>
        /// <param name="e">The Click event</param>
        protected void submitState_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditStatePK.Value = "0";
            divAddEditState.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitState control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitState_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method validates the max number of programs field
        /// </summary>
        /// <param name="sender">The txtStateMaxNumberOfPrograms BootstrapTextBox</param>
        /// <param name="e">The validation event arguments</param>
        protected void txtStateMaxNumberOfPrograms_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the maximum number of programs
            int maxNumOfPrograms;

            //Validate
            if (txtStateMaxNumberOfPrograms.Value == null)
            {
                //This field is required
                e.IsValid = false;
                e.ErrorText = "Required!";
            }
            else if (int.TryParse(txtStateMaxNumberOfPrograms.Text, out maxNumOfPrograms))
            {
                if (maxNumOfPrograms < -1)
                {
                    //This field must be over -1
                    e.IsValid = false;
                    e.ErrorText = "Must be -1 (unlimited programs), zero, or a positive integer!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                //Invalid number
                e.IsValid = false;
                e.ErrorText = "Must be -1 (unlimited programs), zero, or a positive integer!";
            }
        }

        /// <summary>
        /// This method validates the confidentiality document change date field
        /// </summary>
        /// <param name="sender">The deConfidentialityChangeDate DateEdit</param>
        /// <param name="e">The event args</param>
        protected void deConfidentialityChangeDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the change date
            DateTime? changeDate = (deConfidentialityChangeDate.Value == null ?
                                        (DateTime?)null :
                                        Convert.ToDateTime(deConfidentialityChangeDate.Value));

            //Determine if the user is updating the document
            bool isUpdatingDoc = (ddUpdateConfidentialityDocument.Value == null ?
                                        false :
                                        Convert.ToBoolean(ddUpdateConfidentialityDocument.Value));

            //Validate
            if (isUpdatingDoc && changeDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Document Change Date is required!";
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the hubs
        /// and it opens a div that allows the user to add a hub
        /// </summary>
        /// <param name="sender">The lbAddHub LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddHub_Click(object sender, EventArgs e)
        {
            //Clear inputs in the hub div
            hfAddEditHubPK.Value = "0";
            txtHubName.Value = "";
            ddHubState.Value = "";

            //Set the title
            lblAddEditHub.Text = "Add Hub";

            //Show the hub div
            divAddEditHub.Visible = true;

            //Set focus to the hub name field
            txtHubName.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for the hubs
        /// and it opens the hub edit div so that the user can edit the selected hub
        /// </summary>
        /// <param name="sender">The lbEditHub LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditHub_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the container item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfHubPK = (HiddenField)item.FindControl("hfHubPK");

            //Get the PK from the hidden field
            int? hubPK = (String.IsNullOrWhiteSpace(hfHubPK.Value) ? (int?)null : Convert.ToInt32(hfHubPK.Value));

            if (hubPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the hub to edit
                    Hub editHub = context.Hub.Find(hubPK.Value);

                    //Fill the inputs
                    lblAddEditHub.Text = "Edit Hub";
                    txtHubName.Value = editHub.Name;
                    ddHubState.SelectedItem = ddHubState.Items.FindByValue(editHub.StateFK);
                    hfAddEditHubPK.Value = hubPK.Value.ToString();
                }

                //Show the hub div
                divAddEditHub.Visible = true;

                //Set focus to the hub name field
                txtHubName.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected hub!", 20000);

            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the hubs
        /// and it saves the hub information to the database
        /// </summary>
        /// <param name="sender">The submitHub control</param>
        /// <param name="e">The Click event</param>
        protected void submitHub_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitHub.ValidationGroup))
            {
                //Get the hub PK and name
                int hubPK = Convert.ToInt32(hfAddEditHubPK.Value);
                string hubName = txtHubName.Value.ToString();

                using (PyramidContext context = new PyramidContext())
                {
                    Hub currentHub;
                    //Check to see if this is an add or an edit
                    if (hubPK == 0)
                    {
                        //Add
                        currentHub = new Hub();
                        currentHub.Name = hubName;
                        currentHub.StateFK = Convert.ToInt32(ddHubState.Value);
                        currentHub.CreateDate = DateTime.Now;
                        currentHub.Creator = User.Identity.Name;

                        //Save to the database
                        context.Hub.Add(currentHub);
                        context.SaveChanges();

                        //Add the hub to the list of authorized hubs in session
                        currentProgramRole.HubFKs.Add(currentHub.HubPK);
                        Utilities.SetProgramRoleInSession(Session, currentProgramRole);

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added hub!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentHub = context.Hub.Find(hubPK);
                        currentHub.Name = hubName;
                        currentHub.StateFK = Convert.ToInt32(ddHubState.Value);
                        currentHub.EditDate = DateTime.Now;
                        currentHub.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited hub!", 10000);
                    }

                    //Reset the values
                    hfAddEditHubPK.Value = "0";
                    divAddEditHub.Visible = false;

                    //Bind the hub controls
                    BindHubs();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a hub
        /// and it deletes the hub information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteHub LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteHub_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteHubPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteHubPK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the hub to remove and remove it
                        Hub hubToRemove = context.Hub.Where(h => h.HubPK == rowToRemovePK).FirstOrDefault();
                        context.Hub.Remove(hubToRemove);

                        //Save the deletion to the database
                        context.SaveChanges();

                        //Get the delete change row and set the deleter
                        context.HubChanged
                                .OrderByDescending(hc => hc.HubChangedPK)
                                .Where(hc => hc.HubPK == hubToRemove.HubPK)
                                .FirstOrDefault().Deleter = User.Identity.Name;

                        //Save the delete change row to the database
                        context.SaveChanges();

                        //Re-bind the hub controls
                        BindHubs();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted hub!", 10000);
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
                            string messageForUser = "";

                            if (errorMessage.Contains("program"))
                            {
                                messageForUser = "there are programs assigned to this hub!";
                            }
                            else if (errorMessage.Contains("userfileupload"))
                            {
                                messageForUser = "files have been uploaded for this hub!";
                            }
                            else if (errorMessage.Contains("newsentry"))
                            {
                                messageForUser = "there are news entries entered for this hub!";
                            }
                            else
                            {
                                messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                            }

                            //Show the error message
                            msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the hub, {0}", messageForUser), 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the hub!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the hub!", 120000);
                    }

                    //Log the error
                    Utilities.LogException(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the hub to delete!", 120000);

            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the hubs
        /// and it closes the hub add/edit div
        /// </summary>
        /// <param name="sender">The submitHub control</param>
        /// <param name="e">The Click event</param>
        protected void submitHub_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditHubPK.Value = "0";
            divAddEditHub.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitHub control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitHub_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the cohorts
        /// and it opens a div that allows the user to add a cohort
        /// </summary>
        /// <param name="sender">The lbAddCohort LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddCohort_Click(object sender, EventArgs e)
        {
            //Clear inputs in the cohort div
            hfAddEditCohortPK.Value = "0";
            txtCohortName.Value = "";
            deCohortStartDate.Value = "";
            deCohortEndDate.Value = "";
            ddCohortState.Value = "";

            //Set the title
            lblAddEditCohort.Text = "Add cohort";

            //Show the cohort div
            divAddEditCohort.Visible = true;

            //Set focus to the cohort name field
            txtCohortName.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a cohort
        /// and it opens the cohort edit div so that the user can edit the selected cohort
        /// </summary>
        /// <param name="sender">The lbEditCohort LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditCohort_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfCohortPK = (HiddenField)item.FindControl("hfCohortPK");

            //Get the PK from the hidden field
            int? cohortPK = (String.IsNullOrWhiteSpace(hfCohortPK.Value) ? (int?)null : Convert.ToInt32(hfCohortPK.Value));

            if (cohortPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the cohort to edit
                    Cohort editCohort = context.Cohort.Find(cohortPK.Value);

                    //Fill the inputs
                    lblAddEditCohort.Text = "Edit Cohort";
                    txtCohortName.Value = editCohort.CohortName;
                    deCohortStartDate.Value = editCohort.StartDate.ToString("MM/dd/yyyy");
                    deCohortEndDate.Value = (editCohort.EndDate.HasValue ? editCohort.EndDate.Value.ToString("MM/dd/yyyy") : "");
                    ddCohortState.SelectedItem = ddCohortState.Items.FindByValue(editCohort.StateFK);
                    hfAddEditCohortPK.Value = cohortPK.Value.ToString();
                }

                //Show the cohort div
                divAddEditCohort.Visible = true;

                //Set focus to the cohort name field
                txtCohortName.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected cohort!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the cohorts
        /// and it saves the cohort information to the database
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event</param>
        protected void submitCohort_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitCohort.ValidationGroup))
            {
                //Get the cohort information
                int cohortPK = Convert.ToInt32(hfAddEditCohortPK.Value);
                string cohortName = txtCohortName.Value.ToString();
                DateTime startDate = Convert.ToDateTime(deCohortStartDate.Value);
                DateTime? endDate = (deCohortEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deCohortEndDate.Value));

                using (PyramidContext context = new PyramidContext())
                {
                    Cohort currentCohort;
                    //Check to see if this is an add or an edit
                    if (cohortPK == 0)
                    {
                        //Add
                        currentCohort = new Cohort();
                        currentCohort.CohortName = cohortName;
                        currentCohort.StartDate = startDate;
                        currentCohort.EndDate = endDate;
                        currentCohort.StateFK = Convert.ToInt32(ddCohortState.Value);
                        currentCohort.CreateDate = DateTime.Now;
                        currentCohort.Creator = User.Identity.Name;

                        //Save to the database
                        context.Cohort.Add(currentCohort);
                        context.SaveChanges();

                        //Add the cohort to the list of authorized cohorts in session
                        currentProgramRole.CohortFKs.Add(currentCohort.CohortPK);
                        Utilities.SetProgramRoleInSession(Session, currentProgramRole);

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added cohort!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentCohort = context.Cohort.Find(cohortPK);
                        currentCohort.CohortName = cohortName;
                        currentCohort.StartDate = startDate;
                        currentCohort.EndDate = endDate;
                        currentCohort.StateFK = Convert.ToInt32(ddCohortState.Value);
                        currentCohort.EditDate = DateTime.Now;
                        currentCohort.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited cohort!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditCohortPK.Value = "0";
                    divAddEditCohort.Visible = false;

                    //Re-bind the cohort controls
                    BindCohorts();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a cohort
        /// and it deletes the cohort information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteCohort LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteCohort_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteCohortPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCohortPK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the cohort to remove and remove it
                        Cohort cohortToRemove = context.Cohort.Where(c => c.CohortPK == rowToRemovePK).FirstOrDefault();
                        context.Cohort.Remove(cohortToRemove);

                        //Save the deletion to the database
                        context.SaveChanges();

                        //Get the delete change row and set the deleter
                        context.CohortChanged
                                .OrderByDescending(cc => cc.CohortChangedPK)
                                .Where(cc => cc.CohortPK == cohortToRemove.CohortPK)
                                .FirstOrDefault().Deleter = User.Identity.Name;

                        //Save the delete change row to the database
                        context.SaveChanges();

                        //Rebind the cohort controls
                        BindCohorts();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted cohort!", 10000);
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
                            string messageForUser = "";

                            if (errorMessage.Contains("program"))
                            {
                                messageForUser = "there are programs assigned to this cohort!";
                            }
                            else if (errorMessage.Contains("userfileupload"))
                            {
                                messageForUser = "files have been uploaded for this cohort!";
                            }
                            else if (errorMessage.Contains("newsentry"))
                            {
                                messageForUser = "there are news entries entered for this cohort!";
                            }
                            else
                            {
                                messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                            }

                            //Show the error message
                            msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the cohort, {0}", messageForUser), 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the cohort!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the cohort!", 120000);
                    }

                    //Log the error
                    Utilities.LogException(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the cohort to delete!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the cohorts
        /// and it closes the cohort add/edit div
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event</param>
        protected void submitCohort_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditCohortPK.Value = "0";
            divAddEditCohort.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCohort_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the programs
        /// and it opens a div that allows the user to add a program
        /// </summary>
        /// <param name="sender">The lbAddProgram LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddProgram_Click(object sender, EventArgs e)
        {
            //Clear inputs in the program div
            hfAddEditProgramPK.Value = "0";
            txtProgramName.Value = "";
            txtProgramLocation.Value = "";
            txtProgramIDNumber.Value = "";
            deProgramStartDate.Value = "";
            deProgramEndDate.Value = "";
            ddProgramState.Value = "";
            ddProgramHub.Value = "";
            ddProgramCohort.Value = "";
            ddProgramInitialStatus.Value = "";
            deProgramInitialStatusDate.Value = "";
            txtProgramInitialStatusDetails.Value = "";

            //Clear the program types
            foreach (BootstrapListEditItem item in lstBxProgramType.Items)
            {
                item.Selected = false;
            }

            //Set the title
            lblAddEditProgram.Text = "Add Program";

            //Show the program div
            divAddEditProgram.Visible = true;

            //Show initial status
            divInitialProgramStatus.Visible = true;

            //Hide status history section
            divProgramStatusHistory.Visible = false;

            //Set focus to the program name field
            txtProgramName.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a program
        /// and it opens the program edit div so that the user can edit the selected program
        /// </summary>
        /// <param name="sender">The lbEditProgram LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditProgram_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;


            //Get the hidden field with the PK for deletion
            HiddenField hfProgramPK = (HiddenField)item.FindControl("hfProgramPK");

            //Get the PK from the hidden field
            int? programPK = (String.IsNullOrWhiteSpace(hfProgramPK.Value) ? (int?)null : Convert.ToInt32(hfProgramPK.Value));

            if (programPK.HasValue)
            {

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program to edit
                    Program editProgram = context.Program.Find(programPK.Value);

                    //Fill the inputs
                    lblAddEditProgram.Text = "Edit Program";
                    txtProgramName.Value = editProgram.ProgramName;
                    txtProgramLocation.Value = editProgram.Location;
                    txtProgramIDNumber.Value = editProgram.IDNumber;
                    deProgramStartDate.Value = editProgram.ProgramStartDate;
                    deProgramEndDate.Value = editProgram.ProgramEndDate;
                    ddProgramCohort.SelectedItem = ddProgramCohort.Items.FindByValue(editProgram.CohortFK);
                    ddProgramHub.SelectedItem = ddProgramHub.Items.FindByValue(editProgram.HubFK);
                    ddProgramState.SelectedItem = ddProgramState.Items.FindByValue(editProgram.StateFK);
                    hfAddEditProgramPK.Value = programPK.Value.ToString();

                    //Clear the program types
                    foreach (BootstrapListEditItem typeItem in lstBxProgramType.Items)
                    {
                        typeItem.Selected = false;
                    }

                    //Select the program types
                    var editProgramTypes = context.ProgramType.Include(pt => pt.CodeProgramType).Where(pt => pt.ProgramFK == programPK).ToList();
                    foreach (ProgramType pt in editProgramTypes)
                    {
                        lstBxProgramType.Items.FindByValue(pt.CodeProgramType.CodeProgramTypePK).Selected = true;
                    }
                }

                //Clear inputs in the program status section
                hfAddEditStatusPK.Value = "0";
                ddProgramStatus.Value = "";
                deProgramStatusDate.Value = "";
                txtProgramStatusDetails.Value = "";

                //Hide the add/edit program status div
                divAddEditProgramStatus.Visible = false;

                //Show the program div
                divAddEditProgram.Visible = true;

                //Show status history div
                divProgramStatusHistory.Visible = true;

                //Hide initial status
                divInitialProgramStatus.Visible = false;

                //Bind the status history
                BindProgramStatuses();

                //Set focus to the program name field
                txtProgramName.Focus();

                //enable controls
                SetProgramStatusControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected program!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the programs
        /// and it saves the program information to the database
        /// </summary>
        /// <param name="sender">The submitProgram control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgram_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitProgram.ValidationGroup))
            {
                //Get the program information
                int programPK = Convert.ToInt32(hfAddEditProgramPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    Program currentProgram;
                    //Check to see if this is an add or an edit
                    if (programPK == 0)
                    {
                        //Add
                        currentProgram = new Program();
                        currentProgram.ProgramName = txtProgramName.Value.ToString();
                        currentProgram.Location = txtProgramLocation.Value.ToString();
                        currentProgram.IDNumber = (string.IsNullOrWhiteSpace(txtProgramIDNumber.Text) ? null : txtProgramIDNumber.Text);
                        currentProgram.ProgramStartDate = (Convert.ToDateTime(deProgramStartDate.Value));
                        currentProgram.ProgramEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));
                        currentProgram.CohortFK = Convert.ToInt32(ddProgramCohort.Value);
                        currentProgram.HubFK = Convert.ToInt32(ddProgramHub.Value);
                        currentProgram.StateFK = Convert.ToInt32(ddProgramState.Value);
                        currentProgram.CreateDate = DateTime.Now;
                        currentProgram.Creator = User.Identity.Name;

                        //Save to database
                        context.Program.Add(currentProgram);
                        context.SaveChanges();

                        //Add initial status date record
                        ProgramStatus currentStatus = new ProgramStatus();
                        currentStatus.ProgramFK = currentProgram.ProgramPK;
                        currentStatus.StatusFK = Convert.ToInt32(ddProgramInitialStatus.Value);
                        currentStatus.StatusDate = deProgramInitialStatusDate.Date;
                        currentStatus.StatusDetails = (string.IsNullOrWhiteSpace(txtProgramInitialStatusDetails.Text) ? null : txtProgramInitialStatusDetails.Text);
                        currentStatus.CreateDate = DateTime.Now;
                        currentStatus.Creator = User.Identity.Name;

                        //Save to the database
                        context.ProgramStatus.Add(currentStatus);
                        context.SaveChanges();

                        //Clear the program type rows
                        var currentProgramTypes = context.ProgramType.Where(pt => pt.ProgramFK == currentProgram.ProgramPK).ToList();
                        context.ProgramType.RemoveRange(currentProgramTypes);

                        //Save the program type rows
                        foreach (BootstrapListEditItem item in lstBxProgramType.Items)
                        {
                            if (item.Selected)
                            {
                                ProgramType newProgramType = new ProgramType();
                                newProgramType.CreateDate = DateTime.Now;
                                newProgramType.Creator = User.Identity.Name;
                                newProgramType.ProgramFK = currentProgram.ProgramPK;
                                newProgramType.TypeCodeFK = Convert.ToInt32(item.Value);
                                context.ProgramType.Add(newProgramType);
                            }
                        }
                        context.SaveChanges();

                        //Add the program to the list of authorized programs in session
                        currentProgramRole.ProgramFKs.Add(currentProgram.ProgramPK);
                        Utilities.SetProgramRoleInSession(Session, currentProgramRole);

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added program!", 10000);
                    }
                    else
                    {
                        //Edit                        
                        currentProgram = context.Program.Find(programPK);
                        currentProgram.ProgramName = txtProgramName.Value.ToString();
                        currentProgram.Location = txtProgramLocation.Value.ToString();
                        currentProgram.IDNumber = (string.IsNullOrWhiteSpace(txtProgramIDNumber.Text) ? null : txtProgramIDNumber.Text);
                        currentProgram.ProgramStartDate = (Convert.ToDateTime(deProgramStartDate.Value));
                        currentProgram.ProgramEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));
                        currentProgram.CohortFK = Convert.ToInt32(ddProgramCohort.Value);
                        currentProgram.HubFK = Convert.ToInt32(ddProgramHub.Value);
                        currentProgram.StateFK = Convert.ToInt32(ddProgramState.Value);
                        currentProgram.EditDate = DateTime.Now;
                        currentProgram.Editor = User.Identity.Name;

                        //Clear the program type rows
                        var currentProgramTypes = context.ProgramType.Where(pt => pt.ProgramFK == currentProgram.ProgramPK).ToList();
                        context.ProgramType.RemoveRange(currentProgramTypes);

                        //Save the program type rows
                        foreach (BootstrapListEditItem item in lstBxProgramType.Items)
                        {
                            if (item.Selected)
                            {
                                ProgramType newProgramType = new ProgramType();
                                newProgramType.CreateDate = DateTime.Now;
                                newProgramType.Creator = User.Identity.Name;
                                newProgramType.ProgramFK = currentProgram.ProgramPK;
                                newProgramType.TypeCodeFK = Convert.ToInt32(item.Value);
                                context.ProgramType.Add(newProgramType);
                            }
                        }

                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited program!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditProgramPK.Value = "0";
                    divAddEditProgram.Visible = false;

                    //Re-bind the program controls
                    BindPrograms();
                    BindStateProgramCounts();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a program
        /// and it deletes the program information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteProgram LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteProgram_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteProgramPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteProgramPK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the program to remove
                        Program programToRemove = context.Program.Where(h => h.ProgramPK == rowToRemovePK).FirstOrDefault();

                        //Get the program statuses to remove
                        var programStatusesToRemove = context.ProgramStatus.Where(ps => ps.ProgramFK == programToRemove.ProgramPK).ToList();

                        //Remove the program status records
                        context.ProgramStatus.RemoveRange(programStatusesToRemove);

                        //Get the program type rows to remove
                        var programTypesToRemove = context.ProgramType.Where(pt => pt.ProgramFK == rowToRemovePK).ToList();

                        //Remove the program type rows
                        context.ProgramType.RemoveRange(programTypesToRemove);

                        //Get the login history rows to remove
                        List<LoginHistory> loginRowsToRemove = context.LoginHistory.Where(lh => lh.ProgramFK == programToRemove.ProgramPK).ToList();

                        //Remove the login history rows
                        context.LoginHistory.RemoveRange(loginRowsToRemove);

                        //Remove the program
                        context.Program.Remove(programToRemove);

                        //Save the deletions to the database
                        context.SaveChanges();

                        //Get the delete change row and set the deleter
                        context.ProgramChanged
                                .OrderByDescending(pc => pc.ProgramChangedPK)
                                .Where(pc => pc.ProgramPK == programToRemove.ProgramPK)
                                .FirstOrDefault().Deleter = User.Identity.Name;

                        //Save the delete change row to the database
                        context.SaveChanges();

                        //Clear inputs in the program status section
                        hfAddEditStatusPK.Value = "0";
                        ddProgramStatus.Value = "";
                        deProgramStatusDate.Value = "";
                        txtProgramStatusDetails.Value = "";

                        //Hide the add/edit program status div
                        divAddEditProgramStatus.Visible = false;

                        //Hide the add/edit program div
                        divAddEditProgram.Visible = false;

                        //Rebind the program controls
                        BindPrograms();
                        BindStateProgramCounts();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted program!", 10000);
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
                            string messageForUser = "";

                            if (errorMessage.Contains("asqse"))
                            {
                                messageForUser = "there are ASQSE forms entered for this program!";
                            }
                            else if (errorMessage.Contains("benchmarkofqualityfcc"))
                            {
                                messageForUser = "there are Benchmark of Quality FCC forms entered for this program!";
                            }
                            else if (errorMessage.Contains("benchmarkofquality"))
                            {
                                messageForUser = "there are Benchmark of Quality forms entered for this program!";
                            }
                            else if (errorMessage.Contains("childnote"))
                            {
                                messageForUser = "there are children entered for this program!";
                            }
                            else if (errorMessage.Contains("childprogram"))
                            {
                                messageForUser = "there are children entered for this program!";
                            }
                            else if (errorMessage.Contains("childstatus"))
                            {
                                messageForUser = "there are children entered for this program!";
                            }
                            else if (errorMessage.Contains("classroom"))
                            {
                                messageForUser = "there are classrooms entered for this program!";
                            }
                            else if (errorMessage.Contains("coachinglog"))
                            {
                                messageForUser = "there are coaching logs entered for this program!";
                            }
                            else if (errorMessage.Contains("newsentry"))
                            {
                                messageForUser = "there are news entries entered for this program!";
                            }
                            else if (errorMessage.Contains("othersescreen"))
                            {
                                messageForUser = "there are social emotional assessments entered for this program!";
                            }
                            else if (errorMessage.Contains("programemployee"))
                            {
                                messageForUser = "there are professionals entered for this program!";
                            }
                            else if (errorMessage.Contains("userfileupload"))
                            {
                                messageForUser = "files have been uploaded for this program!";
                            }
                            else if (errorMessage.Contains("userprogramrole"))
                            {
                                messageForUser = "there are users that have roles in this program!";
                            }
                            else
                            {
                                messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                            }

                            //Show the error message
                            msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the program, {0}", messageForUser), 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the program!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the program!", 120000);
                    }

                    //Log the error
                    Utilities.LogException(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the program to delete!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the programs
        /// and it closes the program add/edit div
        /// </summary>
        /// <param name="sender">The submitProgram control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgram_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditProgramPK.Value = "0";
            divAddEditProgram.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitProgram control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitProgram_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the validation for the deCohortEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deCohortEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deCohortEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end dates
            DateTime? cohortStartDate = (deCohortStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deCohortStartDate.Value));
            DateTime? cohortEndDate = (deCohortEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deCohortEndDate.Value));

            //Validate the end date
            if (cohortEndDate.HasValue && cohortStartDate.HasValue
                        && cohortStartDate.Value >= cohortEndDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Cohort End Date must be after the Cohort Start Date!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtProgramIDNumber DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtProgramIDNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtProgramIDNumber_Validation(object sender, ValidationEventArgs e)
        {
            //Get the selected state
            int? selectedStateFK = (ddProgramState.Value == null ? (int?)null : Convert.ToInt32(ddProgramState.Value));

            //Validate only if the information is valid
            if (string.IsNullOrWhiteSpace(txtProgramIDNumber.Text) == false && 
                int.TryParse(hfAddEditProgramPK.Value, out int parsedProgramPK) &&
                selectedStateFK.HasValue)
            {
                //Make sure there are no duplicate IDs in the state
                using (PyramidContext context = new PyramidContext())
                {
                    int numDuplicateIDs = context.Program
                                            .AsNoTracking()
                                            .Where(p => p.ProgramPK != parsedProgramPK &&  //Make sure we are only checking for other programs
                                                        p.StateFK == selectedStateFK.Value && //Make sure we are only comparing IDs within the state
                                                        p.IDNumber != null &&
                                                        p.IDNumber.ToLower() == txtProgramIDNumber.Text.ToLower())
                                            .Count();

                    if (numDuplicateIDs > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "This ID Number is already being used by a program in this state!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deProgramEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deProgramEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deProgramEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end dates
            DateTime? programStartDate = (deProgramStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramStartDate.Value));
            DateTime? programEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));

            //Validate the end date
            if (programEndDate.HasValue && programStartDate.HasValue
                        && programStartDate.Value >= programEndDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Program End Date must be after the Program Start Date!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddProgramCohort DevExpress
        /// ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddProgramCohort ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddProgramCohort_Validation(object sender, ValidationEventArgs e)
        {
            //Get the selected state and cohort for the program
            int? selectedStateFK = (ddProgramState.Value == null ? (int?)null : Convert.ToInt32(ddProgramState.Value));
            int? selectedCohortFK = (ddProgramCohort.Value == null ? (int?)null : Convert.ToInt32(ddProgramCohort.Value));

            //Validate
            if (selectedCohortFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Cohort is required!";
            }
            else if (selectedStateFK.HasValue && selectedCohortFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the cohort from the database
                    Cohort selectedCohort = context.Cohort.AsNoTracking()
                                                    .Where(c => c.CohortPK == selectedCohortFK.Value).FirstOrDefault();

                    //Make sure the cohort has the same state FK as the selected state
                    if (selectedCohort != null && selectedCohort.StateFK != selectedStateFK.Value)
                    {
                        e.IsValid = false;
                        e.ErrorText = "The cohort's state must be match the state you selected for this program!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddProgramHub DevExpress
        /// ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddProgramHub ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddProgramHub_Validation(object sender, ValidationEventArgs e)
        {
            //Get the selected state and hub for the program
            int? selectedStateFK = (ddProgramState.Value == null ? (int?)null : Convert.ToInt32(ddProgramState.Value));
            int? selectedHubFK = (ddProgramHub.Value == null ? (int?)null : Convert.ToInt32(ddProgramHub.Value));

            //Validate
            if (selectedHubFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Hub is required!";
            }
            else if (selectedStateFK.HasValue && selectedHubFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the cohort from the database
                    Hub selectedHub = context.Hub.AsNoTracking()
                                                    .Where(h => h.HubPK == selectedHubFK.Value).FirstOrDefault();

                    //Make sure the cohort has the same state FK as the selected state
                    if (selectedHub != null && selectedHub.StateFK != selectedStateFK.Value)
                    {
                        e.IsValid = false;
                        e.ErrorText = "The hub's state must be match the state you selected for this program!";
                    }
                }
            }

        }

        /// <summary>
        /// This method fires when the validation for the ddProgramState DevExpress
        /// ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddProgramState ComboBox</param>
        /// <param name="e">The validation event arguments</param>
        protected void ddProgramState_Validation(object sender, ValidationEventArgs e)
        {
            //Get the selected state and hub for the program
            int? selectedStateFK = (ddProgramState.Value == null ? (int?)null : Convert.ToInt32(ddProgramState.Value));

            //Validate
            if (selectedStateFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "State is required!";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program pk
                    int programPK = Convert.ToInt32(hfAddEditProgramPK.Value);

                    //Get the program end date
                    DateTime? programEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));

                    //Get the current state
                    State currentState = context.State.AsNoTracking().Where(s => s.StatePK == selectedStateFK.Value).FirstOrDefault();

                    //Make sure the state is valid and has a maximum number of programs (-1 means the state has unlimited programs)
                    if (currentState != null && currentState.MaxNumberOfPrograms >= 0)
                    {
                        //Get the current program
                        Program currentProgram = context.Program.AsNoTracking().Where(p => p.ProgramPK == programPK).FirstOrDefault();

                        //Whether or not to check the number of active programs
                        bool needToValidate;

                        //Check to see if the program needs to be validated against the number of active programs
                        if (currentProgram == null &&
                            (programEndDate.HasValue == false || programEndDate.Value > DateTime.Now))
                        {
                            //This is a new program and is active, need to validate it against active programs
                            needToValidate = true;
                        }
                        else if (currentProgram != null &&
                                currentProgram.ProgramEndDate.HasValue == true &&
                                currentProgram.ProgramEndDate.Value <= DateTime.Now &&
                                (programEndDate.HasValue == false || programEndDate.Value > DateTime.Now))
                        {
                            //This is an existing program that was inactive and is becoming active, need to validate it against active programs
                            needToValidate = true;
                        }
                        else
                        {
                            //The program doesn't need to be validated against the number of active programs
                            needToValidate = false;
                        }

                        //Only continue validating if necessary
                        if (needToValidate)
                        {
                            //Get the number of active programs in the state
                            int activeStatePrograms = context.Program.AsNoTracking()
                                                        .Where(p => p.StateFK == selectedStateFK.Value &&
                                                                    (p.ProgramEndDate.HasValue == false || p.ProgramEndDate.Value > DateTime.Now))
                                                        .Count();

                            //Check to see if the number of active programs is equal to or over the max
                            if (activeStatePrograms >= currentState.MaxNumberOfPrograms)
                            {
                                e.IsValid = false;
                                e.ErrorText = "You have reached the limit of active programs for the state!";
                            }
                        }
                    }
                    else
                    {
                        e.IsValid = true;
                    }
                }
            }
        }

        #region Program Statuses

        /// <summary>
        /// This method populates the status repeater with up-to-date information
        /// </summary>
        private void BindProgramStatuses()
        {
            //Get the necessary values
            int programFK = Convert.ToInt32(hfAddEditProgramPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<ProgramStatus> allStatusRecords = context.ProgramStatus
                                            .Include(s => s.Program)
                                            .AsNoTracking()
                                            .Where(s => s.ProgramFK == programFK)
                                            .ToList();
                repeatProgramStatuses.DataSource = allStatusRecords;
                repeatProgramStatuses.DataBind();
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetProgramStatusControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddProgramStatus.ClientEnabled = enabled;
            deProgramStatusDate.ClientEnabled = enabled;
            txtProgramStatusDetails.ClientEnabled = enabled;

            //Show/hide the submit button
            submitStatus.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitStatus.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitStatus.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the program status
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitStatus submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitStatus_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitStatus.ValidationGroup))
            {
                //Get necessary values
                int programPK = Convert.ToInt32(hfAddEditProgramPK.Value);
                int programStatusPK = Convert.ToInt32(hfAddEditStatusPK.Value);
                using (PyramidContext context = new PyramidContext())
                {
                    //To hold the object
                    ProgramStatus currentStatus;

                    //Check to see if this is an add or an edit
                    if (programStatusPK == 0)
                    {
                        //Add
                        currentStatus = new ProgramStatus();
                        currentStatus.ProgramFK = programPK;
                        currentStatus.StatusFK = Convert.ToInt32(ddProgramStatus.Value);
                        currentStatus.StatusDate = deProgramStatusDate.Date;
                        currentStatus.StatusDetails = (string.IsNullOrWhiteSpace(txtProgramStatusDetails.Text) ? null : txtProgramStatusDetails.Text);
                        currentStatus.Creator = User.Identity.Name;
                        currentStatus.CreateDate = DateTime.Now;

                        //Save changes
                        context.ProgramStatus.Add(currentStatus);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added status!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentStatus = context.ProgramStatus.Find(programStatusPK);
                        currentStatus.StatusFK = Convert.ToInt32(ddProgramStatus.Value);
                        currentStatus.StatusDate = deProgramStatusDate.Date;
                        currentStatus.StatusDetails = (string.IsNullOrWhiteSpace(txtProgramStatusDetails.Text) ? null : txtProgramStatusDetails.Text);
                        currentStatus.EditDate = DateTime.Now;
                        currentStatus.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited status!", 10000);
                    }

                    //Reset the values in the hidden field
                    hfAddEditStatusPK.Value = "0";

                    //Hide the add edit section
                    divAddEditProgramStatus.Visible = false;

                    //Rebind the programs and status history
                    BindProgramStatuses();
                    BindPrograms();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the statuses
        /// and it opens a div that allows the user to add a status
        /// </summary>
        /// <param name="sender">The lbAddProgramStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddProgramStatus_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditStatusPK.Value = "0";
            ddProgramStatus.Value = "";
            deProgramStatusDate.Value = "";
            txtProgramStatusDetails.Value = "";

            //Set the title
            lblAddEditProgramStatus.Text = "Add Program Status";

            //Show the input div
            divAddEditProgramStatus.Visible = true;

            //Set focus to the first field
            ddProgramStatus.Focus();

            //Enable the controls
            SetProgramStatusControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a program status
        /// and it opens the edit div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewProgramStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewProgramStatus_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblProgramStatusPK = (Label)item.FindControl("lblProgramStatusPK");

            //Get the PK from the label
            int? statusDatePK = (string.IsNullOrWhiteSpace(lblProgramStatusPK.Text) ? (int?)null : Convert.ToInt32(lblProgramStatusPK.Text));

            if (statusDatePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program status to view
                    ProgramStatus currentStatusRecord = context.ProgramStatus
                                                                .AsNoTracking()
                                                                .Where(a => a.ProgramStatusPK == statusDatePK.Value)
                                                                .FirstOrDefault();

                    //Fill the inputs
                    lblAddEditProgramStatus.Text = "View Program Status";
                    ddProgramStatus.SelectedItem = ddProgramStatus.Items.FindByValue(currentStatusRecord.StatusFK);
                    deProgramStatusDate.Date = currentStatusRecord.StatusDate;
                    txtProgramStatusDetails.Text = currentStatusRecord.StatusDetails;
                    hfAddEditStatusPK.Value = statusDatePK.Value.ToString();
                }

                //Show the edit div
                divAddEditProgramStatus.Visible = true;

                //Set focus to the first field
                ddProgramStatus.Focus();

                //Disable the controls since this is a view
                SetProgramStatusControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected program status!", 20000);
            }
        }

        protected void lbEditProgramStatus_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK for editing
            Label lblProgramStatusPK = (Label)item.FindControl("lblProgramStatusPK");

            //Get the PK from the label
            int? statusDatePK = (string.IsNullOrWhiteSpace(lblProgramStatusPK.Text) ? (int?)null : Convert.ToInt32(lblProgramStatusPK.Text));

            if (statusDatePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the status date to edit
                    ProgramStatus editProgramStatus = context.ProgramStatus.AsNoTracking().Where(m => m.ProgramStatusPK == statusDatePK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditProgramStatus.Text = "Edit Program Status";
                    ddProgramStatus.SelectedItem = ddProgramStatus.Items.FindByValue(editProgramStatus.StatusFK);
                    deProgramStatusDate.Date = editProgramStatus.StatusDate;
                    txtProgramStatusDetails.Text = editProgramStatus.StatusDetails;
                    hfAddEditStatusPK.Value = statusDatePK.Value.ToString();
                }

                //Show the edit div
                divAddEditProgramStatus.Visible = true;

                //Set focus to the first field
                ddProgramStatus.Focus();

                //Enable the controls
                SetProgramStatusControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected status!", 20000);
            }
        }

        /// <summary>
        /// This method fires when the deProgramStatusDate field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The deProgramStatusDate BootstrapDateEdit</param>
        /// <param name="e">The validation event arguments</param>
        protected void deProgramInitialStatusDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the session date and debrief year
            DateTime? statusDate = (deProgramInitialStatusDate.Value == null ? (DateTime?)null : deProgramInitialStatusDate.Date);

            //Validate
            if (statusDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Status Date is required!";
            }
            else if (statusDate > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Status Date cannot be in the future!";
            }
        }

        /// <summary>
        /// This method fires when the deProgramStatusDate field is evaluated for validity.
        /// </summary>
        /// <param name="sender">The deProgramStatusDate BootstrapDateEdit</param>
        /// <param name="e">The validation event arguments</param>
        protected void deProgramStatusDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the session date and debrief year
            DateTime? statusDate = (deProgramStatusDate.Value == null ? (DateTime?)null : deProgramStatusDate.Date);

            //Validate
            if (statusDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Status Date is required!";
            }
            else if (statusDate > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Status Date cannot be in the future!";
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the status
        /// add/edit and it closes the add/edit div
        /// </summary>
        /// <param name="sender">The submitStatus submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitStatus_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditStatusPK.Value = "0";
            divAddEditProgramStatus.Visible = false;
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
        /// This method executes when the user clicks the delete button for a program status
        /// and it deletes the program status information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteProgramStatus LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteProgramStatus_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitStatus.ValidationGroup))
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteStatusDatePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteStatusDatePK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the status date to remove
                            ProgramStatus programStatusToRemove = context.ProgramStatus.Where(t => t.ProgramStatusPK == rowToRemovePK).FirstOrDefault();

                            //Don't allow deletion if there are no other status records for this Program
                            if (context.ProgramStatus.AsNoTracking().Where(s => s.ProgramStatusPK != programStatusToRemove.ProgramStatusPK &&
                                                                                s.ProgramFK == programStatusToRemove.ProgramFK).Count() > 0)
                            {
                                //Remove the status date
                                context.ProgramStatus.Remove(programStatusToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.ProgramStatusChanged
                                        .OrderByDescending(c => c.ProgramStatusChangedPK)
                                        .Where(c => c.ProgramStatusPK == programStatusToRemove.ProgramStatusPK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Rebind the status table
                                BindProgramStatuses();

                                //Rebind the program table
                                BindPrograms();

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the program status!", 10000);
                            }
                            else
                            {
                                //Show an error message
                                msgSys.ShowMessageToUser("danger", "Deletion Failed", "Cannot delete the only status record!  There must be at least one status record for each program.", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the program status, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the program status!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the program status!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the program status to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        /// <summary>
        /// This method fires when the data source for the state DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efStateDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efStateDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "StatePK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();

            //Get all allowed states
            e.QueryableSource = context.State.AsNoTracking().Where(s => currentProgramRole.StateFKs.Contains(s.StatePK));
        }

        /// <summary>
        /// This method fires when the data source for the hub DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efHubDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efHubDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "HubPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();

            //Get all allowed hubs
            e.QueryableSource = context.Hub.AsNoTracking()
                                        .Include(h => h.Program)
                                        .Where(h => currentProgramRole.HubFKs.Contains(h.HubPK));
        }

        /// <summary>
        /// This method fires when the data source for the cohort DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efCohortDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efCohortDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "CohortPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();

            //Get all allowed cohorts
            e.QueryableSource = context.Cohort.AsNoTracking()
                                        .Include(c => c.Program)
                                        .Where(c => currentProgramRole.CohortFKs.Contains(c.CohortPK));
        }

        /// <summary>
        /// This method fires when the data source for the program DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efProgramDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efProgramDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "ProgramPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();

            //Get all allowed programs
            e.QueryableSource = from p in context.Program
                                join pt in context.ProgramType on p.ProgramPK equals pt.ProgramFK into types
                                join ps in context.ProgramStatus on p.ProgramPK equals ps.ProgramFK into statuses
                                where currentProgramRole.ProgramFKs.Contains(p.ProgramPK)
                                select new
                                {
                                    p.ProgramPK,
                                    p.ProgramName,
                                    p.IDNumber,
                                    p.Location,
                                    p.ProgramStartDate,
                                    p.ProgramEndDate,
                                    HubName = p.Hub.Name,
                                    CohortName = p.Cohort.CohortName,
                                    StateName = p.State.Name,
                                    ProgramTypes = from pt in types select pt.CodeProgramType.Description,
                                    ProgramStatus = (from ps in statuses
                                                     orderby ps.StatusDate descending
                                                     select ps.CodeProgramStatus.Description).FirstOrDefault()


                                };
        }
    }
}