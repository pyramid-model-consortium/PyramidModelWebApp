using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;

namespace Pyramid.Pages
{
    public partial class ASQSE : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.ASQSE currentASQSE;
        private Models.ScoreASQSE currentScoreASQSE;
        private int currentASQSEPK = 0;
        private int currentProgramFK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the ASQSE PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ASQSEPK"]))
            {
                int.TryParse(Request.QueryString["ASQSEPK"], out currentASQSEPK);
            }

            //Don't allow aggregate viewers into this page
            if(currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                Response.Redirect("/Pages/ASQSEDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the ASQSE from the database
                currentASQSE = context.ASQSE
                                .AsNoTracking()
                                .Include(a => a.Program)
                                .Where(a => a.ASQSEPK == currentASQSEPK).FirstOrDefault();

                //Check to see if the ASQSE from the database exists
                if (currentASQSE == null)
                {
                    //The ASQSE from the database doesn't exist, set the current ASQSE to a default value
                    currentASQSE = new Models.ASQSE();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the ASQSE's program
                    lblProgram.Text = currentASQSE.Program.ProgramName;
                }

                //Get the current interval and version
                int intervalFK = (ddInterval.Value == null ? currentASQSE.IntervalCodeFK : Convert.ToInt32(ddInterval.Value));
                int versionNum = (ddVersion.Value == null ? currentASQSE.Version : Convert.ToInt32(ddVersion.Value));

                //Get the ScoreASQSE object
                currentScoreASQSE = context.ScoreASQSE.AsNoTracking()
                                        .Where(sa => sa.IntervalCodeFK == intervalFK
                                                && sa.Version == versionNum)
                                        .FirstOrDefault();
            }

            //Prevent users from viewing ASQSEs from other programs
            if (currentASQSE.ASQSEPK > 0 && !currentProgramRole.ProgramFKs.Contains(currentASQSE.ProgramFK))
            {
                Response.Redirect(string.Format("/Pages/ASQSEDashboard.aspx?messageType={0}", "NOASQSE"));
            }

            //Get the proper program fk
            currentProgramFK = (currentASQSE.ASQSEPK > 0 ? currentASQSE.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            //Set the max value for the form date
            deFormDate.MaxDate = DateTime.Now;

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Bind the dropdowns
                BindDropDowns();

                //Check to see if this is an edit
                if (currentASQSEPK > 0)
                {
                    //This is an edit
                    //Populate the page
                    PopulatePage(currentASQSE);

                    //Update the child age label, the score type label, and the cutoff score label
                    UpdateChildAge(currentASQSE.ChildFK, currentASQSE.FormDate);
                    UpdateScoreType(currentASQSE.TotalScore, currentScoreASQSE);
                    UpdateCutoffAndMonitoringLabels(currentScoreASQSE);
                }
                else
                {
                    //This is an add, make the interval and total score read-only for now
                    ddInterval.ReadOnly = true;
                    txtTotalScore.ReadOnly = true;
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
                if (currentASQSE.ASQSEPK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitASQSE.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New ASQ:SE Screening";
                }
                else if (currentASQSE.ASQSEPK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitASQSE.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit ASQ:SE Screening";
                }
                else
                {
                    //Hide the submit button
                    submitASQSE.ShowSubmitButton = false;

                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View ASQ:SE Screening";
                }

                //Set focus to the form date field
                deFormDate.Focus();
            }
        }

        #region Click Methods

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitASQSE user control 
        /// </summary>
        /// <param name="sender">The submitASQSE control</param>
        /// <param name="e">The Click event</param>
        protected void submitASQSE_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //To hold the success message type
                string successMessageType = null;

                //Fill the ASQSE fields from the form
                currentASQSE.FormDate = Convert.ToDateTime(deFormDate.Value);
                currentASQSE.HasDemographicInfoSheet = Convert.ToBoolean(ddDemographicSheet.Value);
                currentASQSE.HasPhysicianInfoLetter = Convert.ToBoolean(ddPhysicianLetter.Value);
                currentASQSE.ChildFK = Convert.ToInt32(ddChild.Value);
                currentASQSE.TotalScore = Convert.ToInt32(txtTotalScore.Value);
                currentASQSE.IntervalCodeFK = Convert.ToInt32(ddInterval.Value);
                currentASQSE.Version = Convert.ToInt32(ddVersion.Value);

                if (currentASQSEPK > 0)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "ASQSEEdited";

                        //Set the edit-only fields
                        currentASQSE.Editor = User.Identity.Name;
                        currentASQSE.EditDate = DateTime.Now;

                        //Get the existing ASQSE record
                        Models.ASQSE existingASQ = context.ASQSE.Find(currentASQSE.ASQSEPK);

                        //Overwrite the existing ASQSE record with the values from the form
                        context.Entry(existingASQ).CurrentValues.SetValues(currentASQSE);
                        context.SaveChanges();
                    }

                    //Redirect the user to the ASQSE dashboard
                    Response.Redirect(string.Format("/Pages/ASQSEDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "ASQSEAdded";

                        //Set the create-only fields
                        currentASQSE.Creator = User.Identity.Name;
                        currentASQSE.CreateDate = DateTime.Now;
                        currentASQSE.ProgramFK = currentProgramRole.CurrentProgramFK.Value;

                        //Add the ASQSE to the database
                        context.ASQSE.Add(currentASQSE);
                        context.SaveChanges();
                    }

                    //Redirect the user to the ASQSE dashboard
                    Response.Redirect(string.Format("/Pages/ASQSEDashboard.aspx?messageType={0}", successMessageType));
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitASQSE user control 
        /// </summary>
        /// <param name="sender">The submitASQSE control</param>
        /// <param name="e">The Click event</param>
        protected void submitASQSE_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the ASQSE dashboard
            Response.Redirect(string.Format("/Pages/ASQSEDashboard.aspx?messageType={0}", "ASQSECanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitASQSE control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitASQSE_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        #endregion

        #region Control Value/Index Change Methods

        /// <summary>
        /// This method fires when the value in the deIncidentDatetime DateEdit changes
        /// and it updates the child and classroom dropdowns
        /// </summary>
        /// <param name="sender">The deIncidentDatetime DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deFormDate_ValueChanged(object sender, EventArgs e)
        {
            //Get the form date and child FK
            DateTime? formDate = (deFormDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deFormDate.Value));
            int? childFK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.Value));

            //Bind the child dropdown
            BindChildDropDown(formDate, currentProgramFK, childFK);
        }

        /// <summary>
        /// This method fires when the value in the txtTotalScore TextBox changes
        /// and it updates the score type
        /// </summary>
        /// <param name="sender">The txtTotalScore TextBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void txtTotalScore_ValueChanged(object sender, EventArgs e)
        {
            int totalScore = 0;
            if (txtTotalScore.Value != null)
            {
                //Try to parse the total score
                if (int.TryParse(txtTotalScore.Value.ToString(), out totalScore))
                {
                    //Try to update the score type
                    if (ddInterval.Value != null && ddVersion.Value != null)
                    {
                        UpdateScoreType(totalScore, currentScoreASQSE);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the value in the ddChild ComboBox's selected index
        /// changes and it updates the age
        /// </summary>
        /// <param name="sender">The ddChild ComboBox</param>
        /// <param name="e">The SelectedIndexChanged event</param>
        protected void ddChild_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the child pk and form date
            int? childPK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.SelectedItem.Value));
            DateTime? formDate = (deFormDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deFormDate.Value));

            if (childPK.HasValue && formDate.HasValue)
            {
                //Update the child age
                UpdateChildAge(childPK.Value, formDate.Value);
            }
        }

        /// <summary>
        /// This method fires when the value in the ddVersion ComboBox's selected index
        /// changes and it updates the cutoff and score type
        /// </summary>
        /// <param name="sender">The ddVersion ComboBox</param>
        /// <param name="e">The SelectedIndexChanged event</param>
        protected void ddVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the total score, selected interval, and selected version number
            int totalScore = (txtTotalScore.Value == null ? 0 : Convert.ToInt32(txtTotalScore.Value));
            int? selectedIntervalFK = (ddInterval.Value == null ? (int?)null : Convert.ToInt32(ddInterval.Value));
            int? selectedVersionNum = (ddVersion.Value == null ? (int?)null : Convert.ToInt32(ddVersion.Value));

            //Enable/disable the interval and total score
            if (selectedVersionNum.HasValue == false)
            {
                ddInterval.ReadOnly = true;
                txtTotalScore.ReadOnly = true;
            }
            else
            {
                ddInterval.ReadOnly = false;

                if (selectedIntervalFK.HasValue)
                {
                    //Enable the total score textbox
                    txtTotalScore.ReadOnly = false;

                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the ScoreASQSE for the interval and version
                        currentScoreASQSE = context.ScoreASQSE.AsNoTracking()
                                                .Where(sa => sa.IntervalCodeFK == selectedIntervalFK && sa.Version == selectedVersionNum)
                                                .FirstOrDefault();
                    }

                    //Update the cutoff and score type
                    UpdateCutoffAndMonitoringLabels(currentScoreASQSE);
                    UpdateScoreType(totalScore, currentScoreASQSE);
                }
            }
        }

        /// <summary>
        /// This method fires when the value in the ddInterval ComboBox's selected index
        /// changes and it updates the cutoff and score type
        /// </summary>
        /// <param name="sender">The ddInterval ComboBox</param>
        /// <param name="e">The SelectedIndexChanged event</param>
        protected void ddInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the total score, selected interval, and selected version number
            int totalScore = (txtTotalScore.Value == null ? 0 : Convert.ToInt32(txtTotalScore.Value));
            int? selectedIntervalFK = (ddInterval.Value == null ? (int?)null : Convert.ToInt32(ddInterval.Value));
            int? selectedVersionNum = (ddVersion.Value == null ? (int?)null : Convert.ToInt32(ddVersion.Value));

            if (selectedIntervalFK.HasValue && selectedVersionNum.HasValue)
            {
                //Enable the total score text box
                txtTotalScore.ReadOnly = false;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the ScoreASQSE for the interval and version
                    currentScoreASQSE = context.ScoreASQSE.AsNoTracking()
                                            .Where(sa => sa.IntervalCodeFK == selectedIntervalFK && sa.Version == selectedVersionNum)
                                            .FirstOrDefault();
                }

                //Update the cutoff and score type
                UpdateCutoffAndMonitoringLabels(currentScoreASQSE);
                UpdateScoreType(totalScore, currentScoreASQSE);
            }
            else
            {
                txtTotalScore.ReadOnly = true;
            }
        }

        #endregion

        #region Dropdown Binding Methods

        /// <summary>
        /// This method populates the dropdowns from the database
        /// </summary>
        private void BindDropDowns()
        {
            //Bind the child and interval dropdowns
            if (currentASQSE.ASQSEPK > 0)
            {
                //If this is an edit, use the program fk from the behavior incident's classroom to filter
                BindChildDropDown(currentASQSE.FormDate, currentProgramFK, currentASQSE.ChildFK);
                BindIntervalDropdown();
            }
            else
            {
                //If this is an add, use the program FKs array from the program role to filter
                BindChildDropDown((DateTime?)null, currentProgramFK, (int?)null);
                BindIntervalDropdown();
            }
        }

        /// <summary>
        /// This method binds the child dropdown by getting all the children in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="formDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="childFK">The child's FK to be selected</param>
        private void BindChildDropDown(DateTime? formDate, int programFK, int? childFK)
        {
            if (formDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the children in the program that were active as of the form date
                    var allChildren = from c in context.Child.Include(c => c.ChildProgram).AsNoTracking()
                                      join cp in context.ChildProgram on c.ChildPK equals cp.ChildFK
                                      where cp.ProgramFK == programFK
                                         && cp.EnrollmentDate <= formDate.Value
                                        && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= formDate.Value)
                                      orderby cp.ProgramSpecificID ascending
                                      select new
                                      {
                                          c.ChildPK,
                                          IdAndName = "(" + cp.ProgramSpecificID + ") "
                                            + c.FirstName + " " + c.LastName

                                      };

                    //Bind the child dropdown to the list of children
                    ddChild.DataSource = allChildren.ToList();
                    ddChild.DataBind();
                }

                //Check to see how many children there are
                if (ddChild.Items.Count > 0)
                {
                    //There is at least 1 child, enable the child dropdown
                    ddChild.ReadOnly = false;

                    //Try to select the child passed to this method
                    ddChild.SelectedItem = ddChild.Items.FindByValue(childFK);
                }
                else
                {
                    //There are no kids in the list, disable the child dropdown
                    ddChild.ReadOnly = true;
                }
            }
            else
            {
                //No date was passed, clear and disable the child dropdown
                ddChild.Value = "";
                ddChild.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method populates the interval dropdown from the database
        /// </summary>
        private void BindIntervalDropdown()
        {
            using (PyramidContext context = new PyramidContext())
            {
                var allintervals = context.CodeASQSEInterval.AsNoTracking().OrderBy(cai => cai.OrderBy).ToList();

                ddInterval.DataSource = allintervals;
                ddInterval.DataBind();
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method populates the page controls from the passed ASQSE object
        /// </summary>
        /// <param name="ASQSEToPopulate">The ASQSE object with values to populate the page controls</param>
        private void PopulatePage(Models.ASQSE ASQSEToPopulate)
        {
            //Set the page controls to the values from the object
            deFormDate.Value = ASQSEToPopulate.FormDate;
            ddChild.SelectedItem = ddChild.Items.FindByValue(ASQSEToPopulate.ChildFK);
            ddVersion.SelectedItem = ddVersion.Items.FindByValue(ASQSEToPopulate.Version);
            ddInterval.SelectedItem = ddInterval.Items.FindByValue(ASQSEToPopulate.IntervalCodeFK);
            ddDemographicSheet.SelectedItem = ddDemographicSheet.Items.FindByValue(ASQSEToPopulate.HasDemographicInfoSheet);
            ddPhysicianLetter.SelectedItem = ddPhysicianLetter.Items.FindByValue(ASQSEToPopulate.HasPhysicianInfoLetter);
            txtTotalScore.Value = ASQSEToPopulate.TotalScore;
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            deFormDate.ClientEnabled = enabled;
            ddChild.ClientEnabled = enabled;
            ddVersion.ClientEnabled = enabled;
            ddInterval.ClientEnabled = enabled;
            txtTotalScore.ClientEnabled = enabled;
            submitASQSE.ShowSubmitButton = enabled;
            ddDemographicSheet.ClientEnabled = enabled;
            ddPhysicianLetter.ClientEnabled = enabled;
        }

        /// <summary>
        /// This method updates the child age label
        /// </summary>
        /// <param name="childFK">The FK for the child</param>
        /// <param name="formDate">The date of the ASQSE</param>
        private void UpdateChildAge(int childFK, DateTime formDate)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the child's DOB
                DateTime childDOB = context.Child.AsNoTracking()
                                        .Where(c => c.ChildPK == childFK)
                                        .FirstOrDefault()
                                        .BirthDate;

                //Calculate the child's age
                int age = Utilities.CalculateAgeDays(formDate, childDOB);
                double ageMonths = (age / 30.417);

                //Display the child's age
                lblAge.Text = ageMonths.ToString("0.##") + " months old";
                lblAge.Visible = true;
            }
        }

        /// <summary>
        /// This method updates the score type label using the passed parameters
        /// </summary>
        /// <param name="totalScore">The total ASQSE score</param>
        /// <param name="scoreASQSE">The ScoreASQSE object</param>
        private void UpdateScoreType(int totalScore, ScoreASQSE scoreASQSE)
        {
            lblScoreType.Text = Utilities.GetASQSEScoreType(totalScore, scoreASQSE);
        }

        /// <summary>
        /// This method updates the cutoff score label using the passed parameters
        /// </summary>
        /// <param name="scoreASQSE">The ScoreASQSE object</param>
        private void UpdateCutoffAndMonitoringLabels(ScoreASQSE scoreASQSE)
        {
            //Set the monitoring zone label
            lblMonitoringZone.Text = scoreASQSE.MonitoringScoreStart.ToString() + " - " + scoreASQSE.MonitoringScoreEnd.ToString();

            //Set the cutoff score label
            lblCutoffScore.Text = scoreASQSE.CutoffScore.ToString();
        }

        #endregion

        #region Custom Validation

        /// <summary>
        /// This method fires when the validation for the txtTotalScore DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTotalScore TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTotalScore_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Perform validation
            if (txtTotalScore.Value == null)
            {
                //The total score is required
                e.IsValid = false;
                e.ErrorText = "Total Score is required!";
            }
            else
            {
                //Validate against the max score
                if (currentScoreASQSE.MaxScore > 0)
                {
                    if (Convert.ToInt32(txtTotalScore.Text) > currentScoreASQSE.MaxScore)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Total Score cannot be greater than the max score of " + currentScoreASQSE.MaxScore + "!";
                    }
                }
            }
        }

        #endregion
    }
}