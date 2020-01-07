using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pyramid.Pages
{
    public partial class BehaviorIncident : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.BehaviorIncident currentBehaviorIncident;
        int behaviorIncidentPK = 0;
        int programFK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow aggregate viewers into this page
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                Response.Redirect("/Pages/BehaviorIncidentDashboard.aspx?messageType=NotAuthorized");
            }

            //Get the BehaviorIncident PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["BehaviorIncidentPK"]))
            {
                int.TryParse(Request.QueryString["BehaviorIncidentPK"], out behaviorIncidentPK);
            }

            //Get the Behavior Incident from the database
            using (PyramidContext context = new PyramidContext())
            {
                //Get the Behavior Incident
                currentBehaviorIncident = context.BehaviorIncident
                                            .AsNoTracking()
                                            .Include(bi => bi.Classroom)
                                            .Include(bi => bi.Classroom.Program)
                                            .Where(bi => bi.BehaviorIncidentPK == behaviorIncidentPK)
                                            .FirstOrDefault();

                //If the Behavior Incident is null (this is an add)
                if (currentBehaviorIncident == null)
                {
                    //Set the current Behavior Incident to a blank Behavior Incident
                    currentBehaviorIncident = new Models.BehaviorIncident();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentBehaviorIncident.Classroom.Program.ProgramName;
                }
            }

            //Don't allow users to view Behavior Incidents from other programs
            if (currentBehaviorIncident.BehaviorIncidentPK > 0 && !currentProgramRole.ProgramFKs.Contains(currentBehaviorIncident.Classroom.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/BehaviorIncidentDashboard.aspx?messageType={0}", "NoBehaviorIncident"));
            }

            //Get the proper program fk
            programFK = (currentBehaviorIncident.BehaviorIncidentPK > 0 ? currentBehaviorIncident.Classroom.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            //Set the max value for the incident datetime date edit
            deIncidentDatetime.MaxDate = DateTime.Now;

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Bind the dropdowns
                BindDropDowns();

                //If this is an edit or view, populate the page with values
                if (behaviorIncidentPK != 0)
                {
                    PopulatePage(currentBehaviorIncident);
                }
                else
                {
                    ddChild.ReadOnly = true;
                }

                //Get the action
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
                if (currentBehaviorIncident.BehaviorIncidentPK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitBehaviorIncident.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New Behavior Incident Report";
                }
                else if (currentBehaviorIncident.BehaviorIncidentPK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitBehaviorIncident.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit Behavior Incident Report";
                }
                else
                {
                    //Hide the submit button
                    submitBehaviorIncident.ShowSubmitButton = false;

                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View Behavior Incident Report";
                }

                //Set focus on the incident datetime field
                deIncidentDatetime.Focus();
            }
        }

        #region Click Methods

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitBehaviorIncident user control 
        /// </summary>
        /// <param name="sender">The submitBehaviorIncident control</param>
        /// <param name="e">The Click event</param>
        protected void submitBehaviorIncident_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //To hold the success message
                string successMessageType = null;

                //Fill the fields of the object from the form
                currentBehaviorIncident.IncidentDatetime = Convert.ToDateTime(deIncidentDatetime.Value);
                currentBehaviorIncident.BehaviorDescription = txtBehaviorDescription.Value.ToString();
                currentBehaviorIncident.ProblemBehaviorCodeFK = Convert.ToInt32(ddProblemBehavior.Value);
                currentBehaviorIncident.ProblemBehaviorSpecify = (txtProblemBehaviorSpecify.Value == null ? null : txtProblemBehaviorSpecify.Value.ToString());
                currentBehaviorIncident.ActivityCodeFK = Convert.ToInt32(ddActivity.Value);
                currentBehaviorIncident.ActivitySpecify = (txtActivitySpecify.Value == null ? null : txtActivitySpecify.Value.ToString());
                currentBehaviorIncident.OthersInvolvedCodeFK = Convert.ToInt32(ddOthersInvolved.Value);
                currentBehaviorIncident.OthersInvolvedSpecify = (txtOthersInvolvedSpecify.Value == null ? null : txtOthersInvolvedSpecify.Value.ToString());
                currentBehaviorIncident.PossibleMotivationCodeFK = Convert.ToInt32(ddPossibleMotivation.Value);
                currentBehaviorIncident.PossibleMotivationSpecify = (txtPossibleMotivationSpecify.Value == null ? null : txtPossibleMotivationSpecify.Value.ToString());
                currentBehaviorIncident.StrategyResponseCodeFK = Convert.ToInt32(ddStrategyResponse.Value);
                currentBehaviorIncident.StrategyResponseSpecify = (txtStrategyResponseSpecify.Value == null ? null : txtStrategyResponseSpecify.Value.ToString());
                currentBehaviorIncident.AdminFollowUpCodeFK = Convert.ToInt32(ddAdminFollowUp.Value);
                currentBehaviorIncident.AdminFollowUpSpecify = (txtAdminFollowUpSpecify.Value == null ? null : txtAdminFollowUpSpecify.Value.ToString());
                currentBehaviorIncident.Notes = (txtNotes.Value == null ? null : txtNotes.Value.ToString());
                currentBehaviorIncident.ChildFK = Convert.ToInt32(ddChild.Value);
                currentBehaviorIncident.ClassroomFK = Convert.ToInt32(ddClassroom.Value);

                //Check to see if this is an add or edit
                if (behaviorIncidentPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an edit
                        successMessageType = "BehaviorIncidentEdited";

                        //Fill the edit-specific fields
                        currentBehaviorIncident.EditDate = DateTime.Now;
                        currentBehaviorIncident.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.BehaviorIncident existingBehaviorIncident = context.BehaviorIncident.Find(currentBehaviorIncident.BehaviorIncidentPK);

                        //Set the behavior incident object to the new values
                        context.Entry(existingBehaviorIncident).CurrentValues.SetValues(currentBehaviorIncident);

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Redirect the user to the dashboard with the success message
                    Response.Redirect(string.Format("/Pages/BehaviorIncidentDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an add
                        successMessageType = "BehaviorIncidentAdded";

                        //Set the create-specific fields
                        currentBehaviorIncident.CreateDate = DateTime.Now;
                        currentBehaviorIncident.Creator = User.Identity.Name;

                        //Add the behavior incident to the database and save
                        context.BehaviorIncident.Add(currentBehaviorIncident);
                        context.SaveChanges();
                    }

                    //Redirect the user to the dashboard with the success message
                    Response.Redirect(string.Format("/Pages/BehaviorIncidentDashboard.aspx?messageType={0}", successMessageType));
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitBehaviorIncident user control 
        /// </summary>
        /// <param name="sender">The submitBehaviorIncident control</param>
        /// <param name="e">The Click event</param>
        protected void submitBehaviorIncident_CancelClick(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("/Pages/BehaviorIncidentDashboard.aspx?messageType={0}", "BehaviorIncidentCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitBehaviorIncident control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitBehaviorIncident_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the user clicks the lbEditChild LinkButton
        /// and it opens a new tab for the user to edit the child selected
        /// in the ddChild ComboBox
        /// </summary>
        /// <param name="sender">The lbEditChild LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditChild_Click(object sender, EventArgs e)
        {
            //Get the incident datetime, the child FK, and the program FK
            DateTime? incidentDateTime = (deIncidentDatetime.Value == null ? (DateTime?)null : Convert.ToDateTime(deIncidentDatetime.Value));
            int? childFK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.Value));

            //Only continue if the incident datetime and child FK have values
            if (incidentDateTime.HasValue && childFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the ChildProgram record
                    var childProgram = context.ChildProgram
                                        .Where(cp => cp.ChildFK == childFK.Value
                                            && cp.ProgramFK == programFK)
                                            .OrderBy(cp => cp.EnrollmentDate).FirstOrDefault();

                    //Check to see if the ChildProgram record exists and the user's edit permissions
                    if (childProgram.ChildProgramPK > 0 && currentProgramRole.AllowedToEdit.Value)
                    {
                        //The ChildProgram record exists and the user can edit, open a new tab for the user to edit the child
                        string urlToRedirect = "Child.aspx?ChildProgramPK=" + childProgram.ChildProgramPK.ToString() + "&Action=Edit";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenChildTab", "window.open('" + urlToRedirect + "')", true);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the lbRefreshClassrooms LinkButton
        /// and it updates the classroom dropdown
        /// </summary>
        /// <param name="sender">The lbRefreshClassrooms LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbRefreshClassrooms_Click(object sender, EventArgs e)
        {
            //Get the incident datetime, the child FK, and the program FK
            DateTime? incidentDateTime = (deIncidentDatetime.Value == null ? (DateTime?)null : Convert.ToDateTime(deIncidentDatetime.Value));
            int? childFK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.Value));

            //Re-bind the classroom dropdown
            BindClassroomDropDown(incidentDateTime, programFK, childFK);
        }

        #endregion

        #region Control Value/Index Change Methods

        /// <summary>
        /// This method fires when the value in the deIncidentDatetime DateEdit changes
        /// and it updates the child and classroom dropdowns
        /// </summary>
        /// <param name="sender">The deIncidentDatetime DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deIncidentDatetime_ValueChanged(object sender, EventArgs e)
        {
            //Get the incident datetime, the child FK, and the program FK
            DateTime? incidentDateTime = (deIncidentDatetime.Value == null ? (DateTime?)null : Convert.ToDateTime(deIncidentDatetime.Value));
            int? childFK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.Value));

            //Re-bind the child and classroom dropdowns
            BindChildDropDown(incidentDateTime, programFK, childFK);
            BindClassroomDropDown(incidentDateTime, programFK, childFK);
        }

        /// <summary>
        /// This method fires when the value in the ddChild ComboBox's selected index
        /// changes and it updates the child and classroom dropdowns
        /// </summary>
        /// <param name="sender">The ddChild ComboBox</param>
        /// <param name="e">The SelectedIndexChanged event</param>
        protected void ddChild_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the incident datetime, the child FK, and the program FK
            DateTime? incidentDateTime = (deIncidentDatetime.Value == null ? (DateTime?)null : Convert.ToDateTime(deIncidentDatetime.Value));
            int? childFK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.Value));

            //Re-bind the classroom dropdown
            BindClassroomDropDown(incidentDateTime, programFK, childFK);
        }

        #endregion

        #region Dropdown Binding Methods

        /// <summary>
        /// This method binds the child dropdown by getting all the children in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="incidentDateTime">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="childFK">The child's FK to be selected</param>
        private void BindChildDropDown(DateTime? incidentDateTime, int programFK, int? childFK)
        {
            //Only continue if the date has a value
            if (incidentDateTime.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the children in the program
                    var allChildren = from c in context.Child.Include(c => c.ChildProgram).AsNoTracking()
                                      join cp in context.ChildProgram on c.ChildPK equals cp.ChildFK
                                      where cp.ProgramFK == programFK
                                        && cp.EnrollmentDate <= incidentDateTime.Value
                                        && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= incidentDateTime.Value)
                                      orderby cp.ProgramSpecificID ascending
                                      select new
                                      {
                                          c.ChildPK,
                                          cp.ChildProgramPK,
                                          IdAndName = "(" + cp.ProgramSpecificID + ") "
                                            + c.FirstName + " " + c.LastName
                                      };
                    ddChild.DataSource = allChildren.ToList();
                    ddChild.DataBind();
                }

                //Check to see how many kids there are
                if (ddChild.Items.Count > 0)
                {
                    //There are kids in the list, enable the child and classroom dropdown
                    ddChild.ReadOnly = false;
                    ddClassroom.ReadOnly = false;

                    //Show the edit child link
                    lbEditChild.Visible = true;

                    //Try to select the child passed to this method
                    ddChild.SelectedItem = ddChild.Items.FindByValue(childFK);
                }
                else
                {
                    //There are no kids in the list, disable the child and classroom dropdown
                    ddChild.ReadOnly = true;
                    ddClassroom.ReadOnly = true;

                    //Hide the edit child link
                    lbEditChild.Visible = false;
                }
            }
            else
            {
                //No date was passed, clear and disable the child and classroom dropdowns
                ddChild.Value = "";
                ddChild.ReadOnly = true;
                ddClassroom.Value = "";
                ddClassroom.ReadOnly = true;

                //Hide the edit child link
                lbEditChild.Visible = false;
            }
        }

        /// <summary>
        /// This method binds the classroom dropdown by getting all the classrooms in
        /// the program for a child that the child was active in at the point of time passed to this method.
        /// </summary>
        /// <param name="incidentDateTime">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="childFK">The child FK</param>
        private void BindClassroomDropDown(DateTime? incidentDateTime, int programFK, int? childFK)
        {
            //Only continue if the date and child fk have values
            if (incidentDateTime.HasValue && childFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the classrooms in the program for the child
                    var allClassrooms = from cc in context.ChildClassroom.Include(c => c.Classroom).AsNoTracking()
                                        join c in context.Classroom on cc.ClassroomFK equals c.ClassroomPK
                                        where c.ProgramFK == programFK
                                            && cc.ChildFK == childFK.Value
                                            && cc.AssignDate <= incidentDateTime.Value
                                            && (cc.LeaveDate.HasValue == false || cc.LeaveDate >= incidentDateTime.Value)
                                        orderby c.ProgramSpecificID ascending
                                        select new
                                        {
                                            c.ClassroomPK,
                                            IdAndName = "(" + c.ProgramSpecificID + ") " + c.Name
                                        };
                    ddClassroom.DataSource = allClassrooms.ToList();
                    ddClassroom.DataBind();
                }

                //Check to see how many classrooms there are
                if (ddClassroom.Items.Count > 0)
                {
                    //There is at least 1, enable the classroom dropdown
                    ddClassroom.ReadOnly = false;
                }
                else
                {
                    //There are none, clear the classroom dropdown and disable it
                    ddClassroom.Value = "";
                    ddClassroom.ReadOnly = true;
                }
            }
            else
            {
                //Either the date or the child FK was null, disable the classroom dropdown
                ddClassroom.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method binds the dropdowns to the proper values
        /// </summary>
        private void BindDropDowns()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the child and classroom dropdowns
                if (currentBehaviorIncident.BehaviorIncidentPK > 0)
                {
                    //If this is an edit, use the program fk from the behavior incident's classroom to filter
                    BindChildDropDown(currentBehaviorIncident.IncidentDatetime, currentBehaviorIncident.Classroom.ProgramFK, currentBehaviorIncident.ChildFK);
                    BindClassroomDropDown(currentBehaviorIncident.IncidentDatetime, currentBehaviorIncident.Classroom.ProgramFK, currentBehaviorIncident.ChildFK);
                }
                else
                {
                    //If this is an add, use the program FKs array from the program role to filter
                    BindChildDropDown((DateTime?)null, currentProgramRole.CurrentProgramFK.Value, (int?)null);
                    BindClassroomDropDown((DateTime?)null, currentProgramRole.CurrentProgramFK.Value, (int?)null);
                }

                //Bind the code table dropdowns
                //Problem behaviors
                var allProblemBehaviors = context.CodeProblemBehavior.AsNoTracking().OrderBy(cpb => cpb.OrderBy).ToList();
                ddProblemBehavior.DataSource = allProblemBehaviors;
                ddProblemBehavior.DataBind();

                //Activities
                var allActivitys = context.CodeActivity.AsNoTracking().OrderBy(cpb => cpb.OrderBy).ToList();
                ddActivity.DataSource = allActivitys;
                ddActivity.DataBind();

                //Others involved
                var allOthersInvolveds = context.CodeOthersInvolved.AsNoTracking().OrderBy(cpb => cpb.OrderBy).ToList();
                ddOthersInvolved.DataSource = allOthersInvolveds;
                ddOthersInvolved.DataBind();

                //Possible motivations
                var allPossibleMotivations = context.CodePossibleMotivation.AsNoTracking().OrderBy(cpb => cpb.OrderBy).ToList();
                ddPossibleMotivation.DataSource = allPossibleMotivations;
                ddPossibleMotivation.DataBind();

                //Strategy responses
                var allStrategyResponses = context.CodeStrategyResponse.AsNoTracking().OrderBy(cpb => cpb.OrderBy).ToList();
                ddStrategyResponse.DataSource = allStrategyResponses;
                ddStrategyResponse.DataBind();

                //Admin follow-ups
                var allAdminFollowUps = context.CodeAdminFollowUp.AsNoTracking().OrderBy(cpb => cpb.OrderBy).ToList();
                ddAdminFollowUp.DataSource = allAdminFollowUps;
                ddAdminFollowUp.DataBind();
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            deIncidentDatetime.ClientEnabled = enabled;
            ddChild.ClientEnabled = enabled;
            ddClassroom.ClientEnabled = enabled;
            txtBehaviorDescription.ClientEnabled = enabled;
            ddProblemBehavior.ClientEnabled = enabled;
            txtProblemBehaviorSpecify.ClientEnabled = enabled;
            ddActivity.ClientEnabled = enabled;
            txtActivitySpecify.ClientEnabled = enabled;
            ddOthersInvolved.ClientEnabled = enabled;
            txtOthersInvolvedSpecify.ClientEnabled = enabled;
            ddPossibleMotivation.ClientEnabled = enabled;
            txtPossibleMotivationSpecify.ClientEnabled = enabled;
            ddStrategyResponse.ClientEnabled = enabled;
            txtStrategyResponseSpecify.ClientEnabled = enabled;
            ddAdminFollowUp.ClientEnabled = enabled;
            txtAdminFollowUpSpecify.ClientEnabled = enabled;
            txtNotes.ClientEnabled = enabled;
            lbEditChild.Visible = enabled;
            lbRefreshClassrooms.Visible = enabled;
        }

        /// <summary>
        /// This method populates the controls on the page with values
        /// from the passed BehaviorIncident object
        /// </summary>
        /// <param name="boq">The BehaviorIncident object to fill the page controls</param>
        private void PopulatePage(Models.BehaviorIncident bi)
        {
            //Fill the controls from the object
            deIncidentDatetime.Value = bi.IncidentDatetime;
            ddChild.SelectedItem = ddChild.Items.FindByValue(bi.ChildFK);
            ddClassroom.SelectedItem = ddClassroom.Items.FindByValue(bi.ClassroomFK);
            txtBehaviorDescription.Value = bi.BehaviorDescription;
            ddProblemBehavior.SelectedItem = ddProblemBehavior.Items.FindByValue(bi.ProblemBehaviorCodeFK);
            txtProblemBehaviorSpecify.Value = bi.ProblemBehaviorSpecify;
            ddActivity.SelectedItem = ddActivity.Items.FindByValue(bi.ActivityCodeFK);
            txtActivitySpecify.Value = bi.ActivitySpecify;
            ddOthersInvolved.SelectedItem = ddOthersInvolved.Items.FindByValue(bi.OthersInvolvedCodeFK);
            txtOthersInvolvedSpecify.Value = bi.OthersInvolvedSpecify;
            ddPossibleMotivation.SelectedItem = ddPossibleMotivation.Items.FindByValue(bi.PossibleMotivationCodeFK);
            txtPossibleMotivationSpecify.Value = bi.PossibleMotivationSpecify;
            ddStrategyResponse.SelectedItem = ddStrategyResponse.Items.FindByValue(bi.StrategyResponseCodeFK);
            txtStrategyResponseSpecify.Value = bi.StrategyResponseSpecify;
            ddAdminFollowUp.SelectedItem = ddAdminFollowUp.Items.FindByValue(bi.AdminFollowUpCodeFK);
            txtAdminFollowUpSpecify.Value = bi.AdminFollowUpSpecify;
            txtNotes.Value = bi.Notes;
        }

        #endregion

        #region Custom Validation

        /// <summary>
        /// This method fires when the validation for the txtProblemBehaviorSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtProblemBehaviorSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtProblemBehaviorSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string problemBehaviorSpecify = (txtProblemBehaviorSpecify.Value == null ? null : txtProblemBehaviorSpecify.Value.ToString());

            //Perform validation
            if (ddProblemBehavior.SelectedItem != null && ddProblemBehavior.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(problemBehaviorSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Problem Behavior is required when the 'Other' problem behavior is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtActivitySpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtActivitySpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtActivitySpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string activitySpecify = (txtActivitySpecify.Value == null ? null : txtActivitySpecify.Value.ToString());

            //Perform validation
            if (ddActivity.SelectedItem != null && ddActivity.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(activitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Activity is required when the 'Other' activity is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtOthersInvolvedSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtOthersInvolvedSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtOthersInvolvedSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string othersInvolvedSpecify = (txtOthersInvolvedSpecify.Value == null ? null : txtOthersInvolvedSpecify.Value.ToString());

            //Perform validation
            if (ddOthersInvolved.SelectedItem != null && ddOthersInvolved.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(othersInvolvedSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Others Involved is required when the 'Other' others involved is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtPossibleMotivationSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPossibleMotivationSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtPossibleMotivationSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string possibleMotivationSpecify = (txtPossibleMotivationSpecify.Value == null ? null : txtPossibleMotivationSpecify.Value.ToString());

            //Perform validation
            if (ddPossibleMotivation.SelectedItem != null && ddPossibleMotivation.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(possibleMotivationSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Possible Motivation is required when the 'Other' possible motivation is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtStrategyResponseSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtStrategyResponseSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtStrategyResponseSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string strategyResponseSpecify = (txtStrategyResponseSpecify.Value == null ? null : txtStrategyResponseSpecify.Value.ToString());

            //Perform validation
            if (ddStrategyResponse.SelectedItem != null && ddStrategyResponse.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(strategyResponseSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Strategy Response is required when the 'Other' strategy response is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtAdminFollowUpSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtAdminFollowUpSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtAdminFollowUpSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified leave reason
            string adminFollowUpSpecify = (txtAdminFollowUpSpecify.Value == null ? null : txtAdminFollowUpSpecify.Value.ToString());

            //Perform validation
            if (ddAdminFollowUp.SelectedItem != null && ddAdminFollowUp.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(adminFollowUpSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Admin Follow-up is required when the 'Other' admin follow-up is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }
        #endregion
    }
}