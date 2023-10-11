using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;
using DevExpress.Web;
using System.Collections.Generic;
using Z.EntityFramework.Plus;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace Pyramid.Pages
{
    public partial class CoachingLog : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CCL";
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
        private Models.CoachingLog currentCoachingLog;
        private List<Models.CoachingLogCoachees> currentCoachees;
        private int currentCoachingLogPK = 0;
        private int currentProgramFK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the CoachingLog PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CoachingLogPK"]))
            {
                int.TryParse(Request.QueryString["CoachingLogPK"], out currentCoachingLogPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentCoachingLogPK == 0 && !string.IsNullOrWhiteSpace(hfCoachingLogPK.Value))
            {
                int.TryParse(hfCoachingLogPK.Value, out currentCoachingLogPK);
            }

            //Check to see if this is an edit
            isEdit = currentCoachingLogPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/CoachingLogDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the CoachingLog from the database
                currentCoachingLog = context.CoachingLog
                                        .AsNoTracking()
                                        .Include(cl => cl.Program)
                                        .Where(a => a.CoachingLogPK == currentCoachingLogPK)
                                        .FirstOrDefault();

                //Check to see if the CoachingLog from the database exists
                if (currentCoachingLog == null)
                {
                    //The CoachingLog from the database doesn't exist, set the current CoachingLog to a default value
                    currentCoachingLog = new Models.CoachingLog();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;

                    //Set the list of coachees to a blank list
                    currentCoachees = new List<Models.CoachingLogCoachees>();
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentCoachingLog.Program.ProgramName;

                    //Get the coachees
                    currentCoachees = currentCoachingLog.CoachingLogCoachees.ToList();
                }
            }

            //Prevent users from viewing CoachingLogs from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentCoachingLog.ProgramFK))
            {
                Response.Redirect(string.Format("/Pages/CoachingLogDashboard.aspx?messageType={0}", "NOCoachingLog"));
            }

            //Get the proper program fk
            currentProgramFK = (isEdit ? currentCoachingLog.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Check to see if this is an edit or view
                if (isEdit)
                {
                    //This is an edit or view
                    //Get the coachee PKs
                    List<int> coacheePKs = currentCoachingLog.CoachingLogCoachees.Select(c => c.CoacheeFK).ToList();

                    //Bind the dropdowns
                    BindDropDowns(currentCoachingLog.LogDate, currentCoachingLog.ProgramFK,
                        currentCoachingLog.CoachFK, coacheePKs);

                    //Populate the page
                    PopulatePage(currentCoachingLog);

                    //Show the print preview button
                    btnPrintPreview.Visible = true;
                }
                else
                {
                    //This is an add
                    //Bind the dropdowns
                    BindDropDowns(null, currentProgramFK, null, new List<int>());

                    //Hide the print preview button
                    btnPrintPreview.Visible = false;
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
                    lblPageTitle.Text = "Add New Classroom Coaching Log";
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
                    lblPageTitle.Text = "Edit Classroom Coaching Log";
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
                    lblPageTitle.Text = "View Classroom Coaching Log";
                }

                //Set the max date for the coaching date field
                deLogDate.MaxDate = DateTime.Now;

                //Set focus to the coaching date field
                deLogDate.Focus();
            }
        }

        #region Click Methods

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitCoachingLog user control 
        /// </summary>
        /// <param name="sender">The submitCoachingLog control</param>
        /// <param name="e">The Click event</param>
        protected void submitCoachingLog_Click(object sender, EventArgs e)
        {
            //To hold the success message type
            string successMessageType = SaveForm(true);

            //Only redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the dashboard
                Response.Redirect(string.Format("/Pages/CoachingLogDashboard.aspx?messageType={0}", successMessageType));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitCoachingLog user control 
        /// </summary>
        /// <param name="sender">The submitCoachingLog control</param>
        /// <param name="e">The Click event</param>
        protected void submitCoachingLog_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the CoachingLog dashboard
            Response.Redirect(string.Format("/Pages/CoachingLogDashboard.aspx?messageType={0}", "CoachingLogCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCoachingLog control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCoachingLog_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitCoachingLog.ValidationGroup))
            {
                //Submit the form
                SaveForm(false);

                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptCoachingLog report = new Reports.PreBuiltReports.FormReports.RptCoachingLog();

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Classroom Coaching Log", currentCoachingLogPK);
                }
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        #endregion

        #region Control Value/Index Change Methods

        /// <summary>
        /// This method fires when the value in the deLogDate DateEdit changes
        /// and it updates the coach dropdown
        /// </summary>
        /// <param name="sender">The deLogDate DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deLogDate_ValueChanged(object sender, EventArgs e)
        {
            //Get the log date
            DateTime? logDate = (deLogDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLogDate.Value));

            //Get the coach
            int? coachFK = (ddCoach.Value == null ? (int?)null : Convert.ToInt32(ddCoach.Value));

            //Get the selected coachees
            List<int> selectedCoachees = GetSelectedCoachees();

            //Bind the dropdowns
            BindDropDowns(logDate, currentProgramFK, coachFK, selectedCoachees);

            //Focus on the log date field
            deLogDate.Focus();
        }
        
        #endregion

        #region Binding Methods

        /// <summary>
        /// This method populates the dropdowns from the database
        /// </summary>
        private void BindDropDowns(DateTime? logDate, int programFK, int? coachFK, List<int> coachees)
        {
            //Bind the coach dropdown
            BindCoachDropDown(logDate, programFK, coachFK);

            //Bind the coachee tag box
            BindCoacheeTagBox(logDate, programFK, coachees);
        }

        /// <summary>
        /// This method binds the coach dropdown by getting all the coaches in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="logDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="coachFK">The coach's FK to be selected</param>
        private void BindCoachDropDown(DateTime? logDate, int programFK, int? coachFK)
        {
            if (logDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the coaches in the program that were active as of the log date
                    var allCoaches = context.spGetAllCoaches(programFK, logDate).Select(c => new {
                        c.ProgramEmployeePK,
                        c.CoachID,
                        CoachName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + c.CoachID + ") " + c.CoachName : c.CoachID)
                    }).ToList();

                    //Bind the coach dropdown to the list of coaches
                    ddCoach.DataSource = allCoaches;
                    ddCoach.DataBind();
                }
                
                //Try to select the coach passed to this method
                ddCoach.SelectedItem = ddCoach.Items.FindByValue(coachFK);

                //Enable the coach dropdown
                ddCoach.ReadOnly = false;
            }
            else
            {
                //No date was passed, clear and disable the coach dropdown
                ddCoach.Value = "";
                ddCoach.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method binds the coachee tag box by getting all the teachers and teaching assistants in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="logDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="selectedCoachees">The coachees selected in the box</param>
        private void BindCoacheeTagBox(DateTime? logDate, int programFK, List<int> selectedCoachees)
        {
            if (logDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the coaches in the program that were active as of the log date
                    var allCoachees = context.ProgramEmployee
                                            .Include(pe => pe.Employee)
                                            .Include(pe => pe.JobFunction)
                                            .AsNoTracking()
                                            .Where(pe => 
                                                pe.ProgramFK == programFK
                                                    && (selectedCoachees.Contains(pe.ProgramEmployeePK)
                                                        || (pe.HireDate <= logDate.Value
                                                        && (pe.TermDate.HasValue == false || pe.TermDate.Value >= logDate.Value)
                                                        && pe.JobFunction.Where(jf =>
                                                                (jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHER ||
                                                                jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHING_ASSISTANT) &&
                                                                jf.StartDate <= logDate.Value &&
                                                                (jf.EndDate.HasValue == false ||
                                                                    jf.EndDate.Value >= logDate.Value)).Count() > 0)))
                                            .Select(pe =>
                                               new
                                               {
                                                   pe.ProgramEmployeePK,
                                                   CoacheeIDAndName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName : pe.ProgramSpecificID)
                                               })
                                            .Distinct()
                                            .ToList();

                    //Bind the coach dropdown to the list of coachees
                    tbCoachees.DataSource = allCoachees;
                    tbCoachees.DataBind();

                    if(allCoachees.Count > 0)
                    {
                        //Enable the coachee tag box
                        tbCoachees.ReadOnly = false;
                    }
                    else
                    {
                        //Disable the coachee tag box
                        tbCoachees.ReadOnly = true;
                    }
                }
            }
            else
            {
                //No date was passed, clear and disable the coachee tag box
                tbCoachees.Value = "";
                tbCoachees.ReadOnly = true;
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method returns the coachees selected on the form
        /// </summary>
        /// <returns>The selected coachees, or an empty list if none selected</returns>
        private List<int> GetSelectedCoachees()
        {
            //Get the selected coachees
            List<int> selectedCoachees = new List<int>();
            if (tbCoachees.Value != null && !string.IsNullOrWhiteSpace(tbCoachees.Value.ToString()))
            {
                selectedCoachees = tbCoachees.Value.ToString().Split(',').Select(int.Parse).ToList();
            }

            return selectedCoachees;
        }

        /// <summary>
        /// This method populates the page controls from the passed CoachingLog object
        /// </summary>
        /// <param name="coachingLog">The CoachingLog object with values to populate the page controls</param>
        private void PopulatePage(Models.CoachingLog coachingLog)
        {
            //Set the page controls to the values from the object
            //Basic info
            deLogDate.Value = coachingLog.LogDate;
            ddCoach.SelectedItem = ddCoach.Items.FindByValue(coachingLog.CoachFK);
            txtDurationMinutes.Value = coachingLog.DurationMinutes;

            //Set the coachees
            tbCoachees.Value = string.Join(",", currentCoachees.Select(c => c.CoacheeFK));

            //Observations
            ddOBSObserving.SelectedItem = ddOBSObserving.Items.FindByValue(coachingLog.OBSObserving);
            ddOBSModeling.SelectedItem = ddOBSModeling.Items.FindByValue(coachingLog.OBSModeling);
            ddOBSVerbalSupport.SelectedItem = ddOBSVerbalSupport.Items.FindByValue(coachingLog.OBSVerbalSupport);
            ddOBSSideBySide.SelectedItem = ddOBSSideBySide.Items.FindByValue(coachingLog.OBSSideBySide);
            ddOBSProblemSolving.SelectedItem = ddOBSProblemSolving.Items.FindByValue(coachingLog.OBSProblemSolving);
            ddOBSReflectiveConversation.SelectedItem = ddOBSReflectiveConversation.Items.FindByValue(coachingLog.OBSReflectiveConversation);
            ddOBSEnvironment.SelectedItem = ddOBSEnvironment.Items.FindByValue(coachingLog.OBSEnvironment);
            ddOBSOtherHelp.SelectedItem = ddOBSOtherHelp.Items.FindByValue(coachingLog.OBSOtherHelp);
            ddOBSConductTPOT.SelectedItem = ddOBSConductTPOT.Items.FindByValue(coachingLog.OBSConductTPOT);
            ddOBSConductTPITOS.SelectedItem = ddOBSConductTPITOS.Items.FindByValue(coachingLog.OBSConductTPITOS);
            ddOBSOther.SelectedItem = ddOBSOther.Items.FindByValue(coachingLog.OBSOther);
            txtOBSOtherSpecify.Value = coachingLog.OBSOtherSpecify;

            //Meetings
            ddMEETProblemSolving.SelectedItem = ddMEETProblemSolving.Items.FindByValue(coachingLog.MEETProblemSolving);
            ddMEETReflectiveConversation.SelectedItem = ddMEETReflectiveConversation.Items.FindByValue(coachingLog.MEETReflectiveConversation);
            ddMEETEnvironment.SelectedItem = ddMEETEnvironment.Items.FindByValue(coachingLog.MEETEnvironment);
            ddMEETRoleplay.SelectedItem = ddMEETRoleplay.Items.FindByValue(coachingLog.MEETRoleplay);
            ddMEETVideo.SelectedItem = ddMEETVideo.Items.FindByValue(coachingLog.MEETVideo);
            ddMEETGraphic.SelectedItem = ddMEETGraphic.Items.FindByValue(coachingLog.MEETGraphic);
            ddMEETGoalSetting.SelectedItem = ddMEETGoalSetting.Items.FindByValue(coachingLog.MEETGoalSetting);
            ddMEETPerformance.SelectedItem = ddMEETPerformance.Items.FindByValue(coachingLog.MEETPerformance);
            ddMEETMaterial.SelectedItem = ddMEETMaterial.Items.FindByValue(coachingLog.MEETMaterial);
            ddMEETDemonstration.SelectedItem = ddMEETDemonstration.Items.FindByValue(coachingLog.MEETDemonstration);
            ddMEETOther.SelectedItem = ddMEETOther.Items.FindByValue(coachingLog.MEETOther);
            txtMEETOtherSpecify.Value = coachingLog.MEETOtherSpecify;

            //Follow-up
            ddFUEmail.SelectedItem = ddFUEmail.Items.FindByValue(coachingLog.FUEmail);
            ddFUPhone.SelectedItem = ddFUPhone.Items.FindByValue(coachingLog.FUPhone);
            ddFUInPerson.SelectedItem = ddFUInPerson.Items.FindByValue(coachingLog.FUInPerson);

            //Narrative
            txtNarrative.Value = coachingLog.Narrative;
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            //Basic info
            deLogDate.ClientEnabled = enabled;
            ddCoach.ClientEnabled = enabled;
            txtDurationMinutes.ClientEnabled = enabled;
            tbCoachees.ClientEnabled = enabled;

            //Observations
            ddOBSObserving.ClientEnabled = enabled;
            ddOBSModeling.ClientEnabled = enabled;
            ddOBSVerbalSupport.ClientEnabled = enabled;
            ddOBSSideBySide.ClientEnabled = enabled;
            ddOBSProblemSolving.ClientEnabled = enabled;
            ddOBSReflectiveConversation.ClientEnabled = enabled;
            ddOBSEnvironment.ClientEnabled = enabled;
            ddOBSOtherHelp.ClientEnabled = enabled;
            ddOBSConductTPOT.ClientEnabled = enabled;
            ddOBSConductTPITOS.ClientEnabled = enabled;
            ddOBSOther.ClientEnabled = enabled;
            txtOBSOtherSpecify.ClientEnabled = enabled;

            //Meetings
            ddMEETProblemSolving.ClientEnabled = enabled;
            ddMEETReflectiveConversation.ClientEnabled = enabled;
            ddMEETEnvironment.ClientEnabled = enabled;
            ddMEETRoleplay.ClientEnabled = enabled;
            ddMEETVideo.ClientEnabled = enabled;
            ddMEETGraphic.ClientEnabled = enabled;
            ddMEETGoalSetting.ClientEnabled = enabled;
            ddMEETPerformance.ClientEnabled = enabled;
            ddMEETMaterial.ClientEnabled = enabled;
            ddMEETDemonstration.ClientEnabled = enabled;
            ddMEETOther.ClientEnabled = enabled;
            txtMEETOtherSpecify.ClientEnabled = enabled;

            //Follow-up
            ddFUEmail.ClientEnabled = enabled;
            ddFUPhone.ClientEnabled = enabled;
            ddFUInPerson.ClientEnabled = enabled;

            //Narrative
            txtNarrative.ClientEnabled = enabled;
            
            //Show/hide the submit button
            submitCoachingLog.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitCoachingLog.UseCancelConfirm = enabled && areConfirmationsEnabled;
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

            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the CoachingLog fields from the form
                //Basic info
                currentCoachingLog.LogDate = Convert.ToDateTime(deLogDate.Value);
                currentCoachingLog.CoachFK = Convert.ToInt32(ddCoach.Value);
                currentCoachingLog.DurationMinutes = Convert.ToInt32(txtDurationMinutes.Value);

                //Observations
                currentCoachingLog.OBSObserving = Convert.ToBoolean(ddOBSObserving.Value);
                currentCoachingLog.OBSModeling = Convert.ToBoolean(ddOBSModeling.Value);
                currentCoachingLog.OBSVerbalSupport = Convert.ToBoolean(ddOBSVerbalSupport.Value);
                currentCoachingLog.OBSSideBySide = Convert.ToBoolean(ddOBSSideBySide.Value);
                currentCoachingLog.OBSProblemSolving = Convert.ToBoolean(ddOBSProblemSolving.Value);
                currentCoachingLog.OBSReflectiveConversation = Convert.ToBoolean(ddOBSReflectiveConversation.Value);
                currentCoachingLog.OBSEnvironment = Convert.ToBoolean(ddOBSEnvironment.Value);
                currentCoachingLog.OBSOtherHelp = Convert.ToBoolean(ddOBSOtherHelp.Value);
                currentCoachingLog.OBSConductTPOT = Convert.ToBoolean(ddOBSConductTPOT.Value);
                currentCoachingLog.OBSConductTPITOS = Convert.ToBoolean(ddOBSConductTPITOS.Value);
                currentCoachingLog.OBSOther = Convert.ToBoolean(ddOBSOther.Value);
                currentCoachingLog.OBSOtherSpecify = (txtOBSOtherSpecify.Value == null ? null : txtOBSOtherSpecify.Value.ToString());

                //Meetings
                currentCoachingLog.MEETProblemSolving = Convert.ToBoolean(ddMEETProblemSolving.Value);
                currentCoachingLog.MEETReflectiveConversation = Convert.ToBoolean(ddMEETReflectiveConversation.Value);
                currentCoachingLog.MEETEnvironment = Convert.ToBoolean(ddMEETEnvironment.Value);
                currentCoachingLog.MEETRoleplay = Convert.ToBoolean(ddMEETRoleplay.Value);
                currentCoachingLog.MEETVideo = Convert.ToBoolean(ddMEETVideo.Value);
                currentCoachingLog.MEETGraphic = Convert.ToBoolean(ddMEETGraphic.Value);
                currentCoachingLog.MEETGoalSetting = Convert.ToBoolean(ddMEETGoalSetting.Value);
                currentCoachingLog.MEETPerformance = Convert.ToBoolean(ddMEETPerformance.Value);
                currentCoachingLog.MEETMaterial = Convert.ToBoolean(ddMEETMaterial.Value);
                currentCoachingLog.MEETDemonstration = Convert.ToBoolean(ddMEETDemonstration.Value);
                currentCoachingLog.MEETOther = Convert.ToBoolean(ddMEETOther.Value);
                currentCoachingLog.MEETOtherSpecify = (txtMEETOtherSpecify.Value == null ? null : txtMEETOtherSpecify.Value.ToString());

                //Follow-up
                currentCoachingLog.FUEmail = Convert.ToBoolean(ddFUEmail.Value);
                currentCoachingLog.FUPhone = Convert.ToBoolean(ddFUPhone.Value);
                currentCoachingLog.FUInPerson = Convert.ToBoolean(ddFUInPerson.Value);
                
                //Narrative
                currentCoachingLog.Narrative = (txtNarrative.Value == null ? null : txtNarrative.Value.ToString());

                if (currentCoachingLog.FUEmail || currentCoachingLog.FUPhone || currentCoachingLog.FUInPerson)
                {
                    currentCoachingLog.FUNone = false;
                }
                else
                {
                    currentCoachingLog.FUNone = true;
                }

                if (isEdit)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "CoachingLogEdited";

                        //Set the edit-only fields
                        currentCoachingLog.Editor = User.Identity.Name;
                        currentCoachingLog.EditDate = DateTime.Now;

                        //Get the existing CoachingLog record
                        Models.CoachingLog existingCoachingLog = context.CoachingLog.Find(currentCoachingLog.CoachingLogPK);

                        //Overwrite the existing CoachingLog record with the values from the form
                        context.Entry(existingCoachingLog).CurrentValues.SetValues(currentCoachingLog);

                        //Get the selected coachees
                        List<int> selectedCoachees = GetSelectedCoachees();

                        //Fill the list of coachees
                        foreach (int employeePK in selectedCoachees)
                        {
                            CoachingLogCoachees existingCoacheeRecord = currentCoachees.Where(c => c.CoacheeFK == employeePK).FirstOrDefault();

                            if (existingCoacheeRecord == null || existingCoacheeRecord.CoachingLogCoacheesPK == 0)
                            {
                                //Add missing coachees
                                existingCoacheeRecord = new CoachingLogCoachees();
                                existingCoacheeRecord.CreateDate = DateTime.Now;
                                existingCoacheeRecord.Creator = User.Identity.Name;
                                existingCoacheeRecord.CoacheeFK = employeePK;
                                existingCoacheeRecord.CoachingLogFK = currentCoachingLog.CoachingLogPK;
                                context.CoachingLogCoachees.Add(existingCoacheeRecord);
                            }
                        }

                        //To hold the coachee PKs that will be removed
                        List<int> deletedCoacheePKs = new List<int>();

                        //Get all the coachees that should no longer be linked
                        foreach (CoachingLogCoachees coachee in currentCoachees)
                        {
                            bool keepCoachee = selectedCoachees.Exists(c => c == coachee.CoacheeFK);

                            if (keepCoachee == false)
                            {
                                deletedCoacheePKs.Add(coachee.CoachingLogCoacheesPK);
                            }
                        }

                        //Delete the coachee rows
                        context.CoachingLogCoachees.Where(clc => deletedCoacheePKs.Contains(clc.CoachingLogCoacheesPK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows
                        context.CoachingLogCoacheesChanged.Where(clcc => deletedCoacheePKs.Contains(clcc.CoachingLogCoacheesPK)).Update(pc => new CoachingLogCoacheesChanged() { Deleter = User.Identity.Name });

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCoachingLogPK.Value = currentCoachingLog.CoachingLogPK.ToString();
                        currentCoachingLogPK = currentCoachingLog.CoachingLogPK;
                    }
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "CoachingLogAdded";

                        //Set the create-only fields
                        currentCoachingLog.Creator = User.Identity.Name;
                        currentCoachingLog.CreateDate = DateTime.Now;
                        currentCoachingLog.ProgramFK = currentProgramRole.CurrentProgramFK.Value;

                        //Add the coaching log to the database
                        context.CoachingLog.Add(currentCoachingLog);
                        context.SaveChanges();

                        //Get the selected coachees
                        List<int> selectedCoachees = GetSelectedCoachees();

                        //Fill the list of coachees
                        foreach (int coacheePK in selectedCoachees)
                        {
                            CoachingLogCoachees newCoacheeRecord = new CoachingLogCoachees();

                            newCoacheeRecord.CreateDate = DateTime.Now;
                            newCoacheeRecord.Creator = User.Identity.Name;
                            newCoacheeRecord.CoachingLogFK = currentCoachingLog.CoachingLogPK;
                            newCoacheeRecord.CoacheeFK = coacheePK;
                            context.CoachingLogCoachees.Add(newCoacheeRecord);
                        }

                        //Save the coachees
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfCoachingLogPK.Value = currentCoachingLog.CoachingLogPK.ToString();
                        currentCoachingLogPK = currentCoachingLog.CoachingLogPK;
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

        #endregion

        #region Custom Validation        

        /// <summary>
        /// This method fires when the validation for the deLogDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deLogDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deLogDate_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the log date
            DateTime? logDate = (deLogDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLogDate.Value));

            //Perform validation
            if (!logDate.HasValue)
            {
                e.IsValid = false;
                e.ErrorText = "Log Date is required!";
            }
            else if (logDate.Value > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Log Date cannot be in the future!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddCoach DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddCoach ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddCoach_Validation(object sender, ValidationEventArgs e)
        {
            //Get the coach
            int? coachFK = (ddCoach.Value == null ? (int?)null : Convert.ToInt32(ddCoach.Value));

            //Perform validation
            if (coachFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Coach is required!  If this control is not enabled, you still need to select a coaching log date.";
            }
            else
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Make sure the coach is not listed as a coachee as well
                    List<int> selectedCoachees = GetSelectedCoachees();

                    if (selectedCoachees.Contains(coachFK.Value))
                    {
                        e.IsValid = false;
                        e.ErrorText = "The coach is currently listed as a coachee.  If this is the correct coach, remove them from the coachee field!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the tbCoachees DevExpress
        /// Bootstrap TagBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The tbCoachees TagBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void tbCoachees_Validation(object sender, ValidationEventArgs e)
        {
            //Get the selected coachees
            List<int> selectedCoachees = GetSelectedCoachees();

            //Get the log date
            DateTime? logDate = (deLogDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLogDate.Value));

            //Validate
            if (selectedCoachees == null && selectedCoachees.Count == 0)
            {
                e.IsValid = false;
                e.ErrorText = "At least one coachee must be selected!";
            }
            else if (logDate.HasValue)
            {
                //Of the selected coachees, determine if any are invalid
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the coachee employee records
                    var coacheeRecords = context.ProgramEmployee
                                                    .Include(pe => pe.Employee)
                                                    .Include(pe => pe.JobFunction)
                                                    .AsNoTracking()
                                                    .Where(pe => selectedCoachees.Contains(pe.ProgramEmployeePK))
                                                    .ToList();

                    //Get the invalid coachee records
                    var invalidCoachees = coacheeRecords.Where(c => c.HireDate > logDate.Value ||
                                                                    (c.TermDate.HasValue && c.TermDate.Value < logDate.Value) ||
                                                                    c.JobFunction.Where(jf => 
                                                                        (jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHER ||
                                                                        jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHING_ASSISTANT) &&
                                                                        jf.StartDate <= logDate.Value &&
                                                                        (jf.EndDate.HasValue == false || 
                                                                            jf.EndDate.Value >= logDate.Value)).Count() == 0)
                                                        .ToList();

                    //Check if any coachees are invalid
                    if (invalidCoachees.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "At least one coachee is invalid.  See the notification message for details.";

                        //Convert the invalid coachee objects to a string to display to the user in a notification message
                        string invalidCoacheeString = string.Join("<br/>", invalidCoachees.Select(c => (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + c.ProgramSpecificID + ") " + c.Employee.FirstName + " " + c.Employee.LastName : c.ProgramSpecificID)).ToList());

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Coachee Validation Error", string.Format("The following coachees are invalid because they are either not active as of the coaching log date, or they don't have an active teacher or teaching assistant job function:<br/><br/> {0}", invalidCoacheeString), 200000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtOBSOtherSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtOBSOtherSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtOBSOtherSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string activitySpecify = (txtOBSOtherSpecify.Value == null ? null : txtOBSOtherSpecify.Value.ToString());

            //Perform validation
            if (ddOBSOther.SelectedItem != null && ddOBSOther.SelectedItem.Text.ToLower() == "yes" && String.IsNullOrWhiteSpace(activitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify OBSOther is required when the 'Other' activity is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtMEETOtherSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtMEETOtherSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtMEETOtherSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string activitySpecify = (txtMEETOtherSpecify.Value == null ? null : txtMEETOtherSpecify.Value.ToString());

            //Perform validation
            if (ddMEETOther.SelectedItem != null && ddMEETOther.SelectedItem.Text.ToLower() == "yes" && String.IsNullOrWhiteSpace(activitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify MEETOther is required when the 'Other' activity is selected!";
            }
        }

        #endregion
    }
}