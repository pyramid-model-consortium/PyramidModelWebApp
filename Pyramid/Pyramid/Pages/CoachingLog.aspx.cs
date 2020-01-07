using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;
using DevExpress.Web;

namespace Pyramid.Pages
{
    public partial class CoachingLog : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.CoachingLog currentCoachingLog;
        private int currentCoachingLogPK = 0;
        private int currentProgramFK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the CoachingLog PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CoachingLogPK"]))
            {
                int.TryParse(Request.QueryString["CoachingLogPK"], out currentCoachingLogPK);
            }

            //Don't allow aggregate viewers into this page
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
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
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentCoachingLog.Program.ProgramName;
                }
            }

            //Prevent users from viewing CoachingLogs from other programs
            if (currentCoachingLog.CoachingLogPK > 0 && !currentProgramRole.ProgramFKs.Contains(currentCoachingLog.ProgramFK))
            {
                Response.Redirect(string.Format("/Pages/CoachingLogDashboard.aspx?messageType={0}", "NOCoachingLog"));
            }

            //Get the proper program fk
            currentProgramFK = (currentCoachingLog.CoachingLogPK > 0 ? currentCoachingLog.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            //Set the max value for the coaching date
            deLogDate.MaxDate = DateTime.Now;

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Check to see if this is an edit or view
                if (currentCoachingLogPK > 0)
                {
                    //This is an edit or view
                    //Populate the page
                    PopulatePage(currentCoachingLog);

                    //Bind the dropdowns
                    BindDropDowns(currentCoachingLog.LogDate, currentCoachingLog.ProgramFK, currentCoachingLog.CoachFK, currentCoachingLog.TeacherFK);
                }
                else
                {
                    //This is an add, make the coach and teacher dropdown read-only for now
                    ddCoach.ReadOnly = true;
                    ddTeacher.ReadOnly = true;
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
                if (currentCoachingLog.CoachingLogPK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitCoachingLog.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New Classroom Coaching Log";
                }
                else if (currentCoachingLog.CoachingLogPK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitCoachingLog.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit Classroom Coaching Log";
                }
                else
                {
                    //Hide the submit button
                    submitCoachingLog.ShowSubmitButton = false;

                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View Classroom Coaching Log";
                }

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
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //To hold the success message type
                string successMessageType = null;

                //Fill the CoachingLog fields from the form
                //Basic info
                currentCoachingLog.LogDate = Convert.ToDateTime(deLogDate.Value);
                currentCoachingLog.CoachFK = Convert.ToInt32(ddCoach.Value);
                currentCoachingLog.TeacherFK = Convert.ToInt32(ddTeacher.Value);
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

                if (currentCoachingLog.FUEmail || currentCoachingLog.FUPhone || currentCoachingLog.FUInPerson)
                {
                    currentCoachingLog.FUNone = false;
                }
                else
                {
                    currentCoachingLog.FUNone = true;
                }

                if (currentCoachingLogPK > 0)
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
                        context.SaveChanges();
                    }

                    //Redirect the user to the CoachingLog dashboard
                    Response.Redirect(string.Format("/Pages/CoachingLogDashboard.aspx?messageType={0}", successMessageType));
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

                        //Add the CoachingLog to the database
                        context.CoachingLog.Add(currentCoachingLog);
                        context.SaveChanges();
                    }

                    //Redirect the user to the CoachingLog dashboard
                    Response.Redirect(string.Format("/Pages/CoachingLogDashboard.aspx?messageType={0}", successMessageType));
                }
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

        #endregion

        #region Control Value/Index Change Methods

        /// <summary>
        /// This method fires when the value in the deLogDate DateEdit changes
        /// and it updates the coach and teacher dropdowns
        /// </summary>
        /// <param name="sender">The deLogDate DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deLogDate_ValueChanged(object sender, EventArgs e)
        {
            //Get the log date
            DateTime? logDate = (deLogDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLogDate.Value));

            //Get the coach and teacher FKs
            int? coachFK = (ddCoach.Value == null ? (int?)null : Convert.ToInt32(ddCoach.Value));
            int? teacherFK = (ddTeacher.Value == null ? (int?)null : Convert.ToInt32(ddTeacher.Value));

            if (logDate.HasValue)
            {
                //Bind the coach and teacher dropdowns
                BindDropDowns(logDate.Value, currentProgramFK, coachFK, teacherFK);
            }
        }

        #endregion

        #region Dropdown Binding Methods

        /// <summary>
        /// This method populates the dropdowns from the database
        /// </summary>
        private void BindDropDowns(DateTime logDate, int programFK, int? coachFK, int? teacherFK)
        {
            //Bind the coach and teacher dropdowns
            BindCoachDropDown(logDate, programFK, coachFK);
            BindTeacherDropDown(logDate, programFK, teacherFK);
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
                    var allCoaches = context.spGetAllCoaches(programFK, logDate).ToList();

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
        /// This method binds the teacher dropdown by getting all the teachers in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="logDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="teacherFK">The teacher's FK to be selected</param>
        private void BindTeacherDropDown(DateTime? logDate, int programFK, int? teacherFK)
        {
            if (logDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the teachers in the program that were active as of the log date
                    var allTeachers = from pe in context.ProgramEmployee.AsNoTracking()
                                                    .Include(pe => pe.JobFunction)
                                      join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK
                                      where pe.ProgramFK == programFK
                                        && pe.HireDate <= logDate.Value
                                        && (pe.TermDate.HasValue == false || pe.TermDate.Value >= logDate.Value)
                                        && jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHER
                                        && jf.StartDate <= logDate.Value
                                        && (jf.EndDate.HasValue == false || jf.EndDate.Value >= logDate.Value)
                                      orderby pe.FirstName ascending
                                      select new
                                      {
                                          pe.ProgramEmployeePK,
                                          Name = pe.FirstName + " " + pe.LastName
                                      };

                    //Bind the teacher dropdown to the list of teachers
                    ddTeacher.DataSource = allTeachers.ToList();
                    ddTeacher.DataBind();
                }
                
                //Try to select the teacher passed to this method
                ddTeacher.SelectedItem = ddTeacher.Items.FindByValue(teacherFK);

                //Enable the teacher dropdown
                ddTeacher.ReadOnly = false;
            }
            else
            {
                //No date was passed, clear and disable the teacher dropdown
                ddTeacher.Value = "";
                ddTeacher.ReadOnly = true;
            }
        }

        #endregion

        #region MISC Methods

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
            ddTeacher.SelectedItem = ddTeacher.Items.FindByValue(coachingLog.TeacherFK);
            txtDurationMinutes.Value = coachingLog.DurationMinutes;

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
            ddTeacher.ClientEnabled = enabled;
            txtDurationMinutes.ClientEnabled = enabled;

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
        }

        #endregion

        #region Custom Validation

        /// <summary>
        /// This method fires when the validation for the ddCoach DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddCoach ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddCoach_Validation(object sender, ValidationEventArgs e)
        {
            //Get the coach and teacher
            int? coachFK = (ddCoach.Value == null ? (int?)null : Convert.ToInt32(ddCoach.Value));
            int? teacherFK = (ddTeacher.Value == null ? (int?)null : Convert.ToInt32(ddTeacher.Value));

            //Perform validation
            if (coachFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Coach is required!  If this control is not enabled, you still need to select a coaching log date.";
            }
            else if(teacherFK.HasValue && coachFK.Value == teacherFK.Value)
            {
                e.IsValid = false;
                e.ErrorText = "The selected coach cannot be the same as the selected teacher!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddTeacher DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddTeacher ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddTeacher_Validation(object sender, ValidationEventArgs e)
        {
            //Get the coach and teacher
            int? coachFK = (ddCoach.Value == null ? (int?)null : Convert.ToInt32(ddCoach.Value));
            int? teacherFK = (ddTeacher.Value == null ? (int?)null : Convert.ToInt32(ddTeacher.Value));

            //Perform validation
            if (teacherFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Teacher is required!  If this control is not enabled, you still need to select a coaching log date.";
            }
            else if (coachFK.HasValue && teacherFK.Value == coachFK.Value)
            {
                e.IsValid = false;
                e.ErrorText = "The selected teacher cannot be the same as the selected coach!";
            }
            else
            {
                e.IsValid = true;
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
            if (ddOBSOther.SelectedItem != null && ddOBSOther.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(activitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify OBSOther is required when the 'Other' activity is selected!";
            }
            else
            {
                e.IsValid = true;
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
            if (ddMEETOther.SelectedItem != null && ddMEETOther.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(activitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify MEETOther is required when the 'Other' activity is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        #endregion
    }
}