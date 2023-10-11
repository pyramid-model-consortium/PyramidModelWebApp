using Pyramid.Models;
using System;
using System.Linq;
using Pyramid.MasterPages;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Pyramid.Code;
using DevExpress.Web;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Collections.Generic;
using Z.EntityFramework.Plus;
using System.Web.UI.HtmlControls;
using DevExpress.Web.Bootstrap;

namespace Pyramid.Pages
{
    public partial class MasterCadreDashboard : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "MCTT",
                    "MCTD"
                };
            }
        }

        public List<CodeProgramRolePermission> FormPermissions
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

        private List<CodeProgramRolePermission> currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private CodeProgramRolePermission currentTrainingTrackerPermissions;
        private CodeProgramRolePermission currentTrainingDebriefPermissions;
        private List<PyramidUser> usersForDashboard;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the user records
            usersForDashboard = PyramidUser.GetMasterCadreUserRecords(currentProgramRole, User.Identity.Name);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the permission objects
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permissions
            currentTrainingTrackerPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "MCTT").FirstOrDefault();
            currentTrainingDebriefPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "MCTD").FirstOrDefault();

            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Set the permissions for the sections
            //Training trackers
            SetPermissions(currentTrainingTrackerPermissions, divTrainingTrackers, bsGRTrainingTrackers, hfTrainingTrackerViewOnly);

            //Training debriefs
            SetPermissions(currentTrainingDebriefPermissions, divTrainingDebriefs, bsGRTrainingDebriefs, hfTrainingDebriefViewOnly);

            //Disable virtual item rendering because it causes issues with the dropdown showing too much whitespace
            ddTrackerActivity.EnableItemsVirtualRendering = DevExpress.Utils.DefaultBoolean.False;

            if (!IsPostBack)
            {
                //Set the hidden fields to have the activity PKs for the Course and Event IDs
                setHiddenPKFields();

                //Bind the dropdowns
                BindDropdowns();

                //Show/hide the ASPIRE options
                if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN &&
                    currentProgramRole.CurrentStateFK.Value != (int)Utilities.StateFKs.NEW_YORK)
                {
                    //Hide the GridView columns
                    bsGRTrainingDebriefs.Columns["AspireUploadColumn"].Visible = false;

                    //Hide the ASPIRE controls
                    ddDebriefWasUploadedToAspire.Visible = false;
                }
            }
        }

        /// <summary>
        /// This method sets the visibility and usability of dashboard sections based on the passed permission object
        /// </summary>
        /// <param name="permissions">The permissions for the section</param>
        /// <param name="sectionDiv">The section div</param>
        /// <param name="gridview">The section gridview</param>
        /// <param name="viewOnlyHiddenField">The view only hidden field</param>
        private void SetPermissions(CodeProgramRolePermission permissions, HtmlGenericControl sectionDiv, BootstrapGridView gridview, HiddenField viewOnlyHiddenField)
        {
            //Check permissions
            if (permissions.AllowedToViewDashboard == false)
            {
                //Not allowed to see the section
                sectionDiv.Visible = false;
            }
            else if (permissions.AllowedToView == false)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (gridview.Columns.Count - 1);

                //Hide the action column
                gridview.Columns[actionColumnIndex].Visible = false;

                //Hide management options
                viewOnlyHiddenField.Value = "True";
            }
            else if (permissions.AllowedToAdd == false && permissions.AllowedToEdit == false)
            {
                viewOnlyHiddenField.Value = "True";
            }
            else
            {
                viewOnlyHiddenField.Value = "False";
            }
        }

        private void BindDropdowns()
        {
            //To hold all the necessary objects
            List<Models.CodeMasterCadreFundingSource> allFundingSources;
            List<Models.CodeMeetingFormat> allMeetingFormats;
            List<Models.CodeMasterCadreActivity> allActivities;

            //Get all the necessary objects from the database
            using (PyramidContext context = new PyramidContext())
            {
                allFundingSources = context.CodeMasterCadreFundingSource.AsNoTracking().OrderBy(s => s.OrderBy).ToList();
                allMeetingFormats = context.CodeMeetingFormat.AsNoTracking().OrderBy(f => f.OrderBy).ToList();
                allActivities = context.CodeMasterCadreActivity.AsNoTracking().OrderBy(tm => tm.OrderBy).ToList();
            }

            //Bind the dropdowns
            ddTrackerFundingSource.DataSource = allFundingSources;
            ddTrackerFundingSource.DataBind();

            ddTrackerActivity.DataSource = allActivities;
            ddTrackerActivity.DataBind();

            ddTrackerMeetingType.DataSource = allMeetingFormats;
            ddTrackerMeetingType.DataBind();

            ddDebriefTrainingFormat.DataSource = allMeetingFormats;
            ddDebriefTrainingFormat.DataBind();

            ddDebriefActivity.DataSource = allActivities;
            ddDebriefActivity.DataBind();
        }

        private void setHiddenPKFields()
        {
            using (PyramidContext context = new PyramidContext())
            {
                List<int> coursePKList = context.CodeMasterCadreActivity.Where(x => x.RequireCourseID == true).Select(y => y.CodeMasterCadreActivityPK).ToList();
                string coursePKs = string.Join(",", coursePKList);

                List<int> eventPKList = context.CodeMasterCadreActivity.Where(x => x.RequireEventID == true).Select(y => y.CodeMasterCadreActivityPK).ToList();
                string eventPKs = string.Join(",", eventPKList);

                hfActivityPKsRequiringCourseID.Value = coursePKs;
                hfActivityPKsRequiringEventID.Value = eventPKs;
            }
        }

        #region Activity Tracker

        protected void efTrainingTrackerDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "MasterCadreTrainingTrackerItemPK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER)
            {

                e.QueryableSource = from m in context.MasterCadreTrainingTrackerItem.AsNoTracking()
                                    join d in context.MasterCadreTrainingTrackerItemDate on m.MasterCadreTrainingTrackerItemPK equals d.MasterCadreTrainingTrackerItemFK into dates
                                    where m.MasterCadreMemberUsername == User.Identity.Name && m.StateFK == currentProgramRole.CurrentStateFK.Value
                                    select new
                                    {
                                        m.MasterCadreTrainingTrackerItemPK,
                                        m.ParticipantFee,
                                        m.MasterCadreMemberUsername,
                                        m.IsOpenToPublic,
                                        m.TargetAudience,
                                        m.MeetingLocation,
                                        m.AspireEventNum,
                                        m.CourseIDNum,
                                        m.NumHours,
                                        m.DidEventOccur,
                                        StartDateTime = (from dt in dates
                                                         select dt.StartDateTime).Min(),
                                        EndDateTime = (from dt in dates
                                                       select dt.EndDateTime).Max(),
                                        Activity = m.CodeMasterCadreActivity.Description,
                                        State = m.State.Name,
                                        MeetingFormat = m.CodeMeetingFormat.Description,
                                        FundingSource = m.CodeMasterCadreFundingSource.Description
                                    };

            }
            else
            {

                e.QueryableSource = from m in context.MasterCadreTrainingTrackerItem.AsNoTracking()
                                    join d in context.MasterCadreTrainingTrackerItemDate on m.MasterCadreTrainingTrackerItemPK equals d.MasterCadreTrainingTrackerItemFK into dates
                                    where currentProgramRole.StateFKs.Contains(m.StateFK)
                                    select new
                                    {
                                        m.MasterCadreTrainingTrackerItemPK,
                                        m.ParticipantFee,
                                        m.MasterCadreMemberUsername,
                                        m.IsOpenToPublic,
                                        m.TargetAudience,
                                        m.MeetingLocation,
                                        m.AspireEventNum,
                                        m.CourseIDNum,
                                        m.NumHours,
                                        m.DidEventOccur,
                                        StartDateTime = (from dt in dates
                                                         select dt.StartDateTime).Min(),
                                        EndDateTime = (from dt in dates
                                                       select dt.EndDateTime).Max(),
                                        Activity = m.CodeMasterCadreActivity.Description,
                                        State = m.State.Name,
                                        MeetingFormat = m.CodeMeetingFormat.Description,
                                        FundingSource = m.CodeMasterCadreFundingSource.Description
                                    };
            }
        }

        protected void bsGRTrainingTrackers_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the master cadre member
                Label lblMasterCadreMemberUsername = (Label)bsGRTrainingTrackers.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRTrainingTrackers.Columns["MasterCadreMemberColumn"], "lblMasterCadreMemberUsername");

                //Get the current coach username
                string currentUsername = (e.GetValue("MasterCadreMemberUsername") == null ? null : Convert.ToString(e.GetValue("MasterCadreMemberUsername")));

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching)
                        lblMasterCadreMemberUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetTrainingTrackerControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddTrackerActivity.ClientEnabled = enabled;
            beTrackerParticipantFee.ClientEnabled = enabled;
            ddTrackerFundingSource.ClientEnabled = enabled;
            ddTrackerIsOpenToPublic.ClientEnabled = enabled;
            txtTrackerTargetAudience.ClientEnabled = enabled;
            ddTrackerMeetingType.ClientEnabled = enabled;
            txtTrackerMeetingLocation.ClientEnabled = enabled;
            txtTrackerNumHours.ClientEnabled = enabled;
            ddTrackerDidEventOccur.ClientEnabled = enabled;
            txtTrackerEventNum.ClientEnabled = enabled;
            txtTrackerCourseNum.ClientEnabled = enabled;

            //Show/hide the submit button
            submitTrainingTracker.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitTrainingTracker.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit control's properties
            submitTrainingTracker.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the activity tracker
        /// and it opens a div that allows the user to add a new item
        /// </summary>
        /// <param name="sender">The lbAddTrainingTracker LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTrainingTracker_Click(object sender, EventArgs e)
        {
            //Try to get the user record
            PyramidUser masterCadreMemberUser = usersForDashboard.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            //Make sure the record exists
            if (masterCadreMemberUser != null && currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER)
            {
                //Clear inputs in the div
                hfAddEditTrainingTrackerPK.Value = "0";
                ddTrackerActivity.Value = "";
                beTrackerParticipantFee.Value = "";
                ddTrackerFundingSource.Value = "";
                ddTrackerIsOpenToPublic.Value = "";
                txtTrackerTargetAudience.Value = "";
                ddTrackerMeetingType.Value = "";
                txtTrackerMeetingLocation.Value = "";
                txtTrackerNumHours.Value = "";
                ddTrackerDidEventOccur.Value = "";
                txtTrackerEventNum.Value = "";
                txtTrackerCourseNum.Value = "";

                //Set the title
                lblAddEditTrainingTracker.Text = "Add Tracker Item";

                //Set the username label
                lblTrackerMCUsername.Text = string.Format("{0} {1} ({2})", masterCadreMemberUser.FirstName, masterCadreMemberUser.LastName, masterCadreMemberUser.UserName);

                //Show the div
                divAddEditTrainingTracker.Visible = true;

                //Clear inputs in initial date div
                deTrackerItemInitialDate.Value = "";
                teTrackerItemInitialStartTime.Value = "";
                teTrackerItemInitialEndTime.Value = "";

                //Show initial date div and alert
                divTrainingTrackerItemInitialDate.Visible = true;
                divAddTrainingTrackerAlert.Visible = true;

                //Hide date history div
                divTrainingTrackerItemDateHistory.Visible = false;

                //Set focus to the first field
                ddTrackerActivity.Focus();

                //Enable the controls
                SetTrainingTrackerControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized because you are not logged in as a Master Cadre Member!", 10000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a activity tracker
        /// and it opens the activity tracker edit div in view-only mode
        /// </summary>
        /// <param name="sender">The lbViewTrainingTracker LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewTrainingTracker_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblTrainingTrackerPK = (Label)item.FindControl("lblTrainingTrackerPK");

            //Get the PK from the label
            int? trainingTrackerPK = (string.IsNullOrWhiteSpace(lblTrainingTrackerPK.Text) ? (int?)null : Convert.ToInt32(lblTrainingTrackerPK.Text));

            if (trainingTrackerPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the tracker item
                    MasterCadreTrainingTrackerItem currentTrackerItem = context.MasterCadreTrainingTrackerItem.AsNoTracking().Where(i => i.MasterCadreTrainingTrackerItemPK == trainingTrackerPK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditTrainingTrackerPK.Value = currentTrackerItem.MasterCadreTrainingTrackerItemPK.ToString();
                    ddTrackerActivity.SelectedItem = ddTrackerActivity.Items.FindByValue(currentTrackerItem.MasterCadreActivityCodeFK);
                    beTrackerParticipantFee.Text = currentTrackerItem.ParticipantFee.ToString("#.00");
                    ddTrackerFundingSource.SelectedItem = ddTrackerFundingSource.Items.FindByValue(currentTrackerItem.MasterCadreFundingSourceCodeFK);
                    ddTrackerIsOpenToPublic.SelectedItem = ddTrackerIsOpenToPublic.Items.FindByValue(currentTrackerItem.IsOpenToPublic);
                    ddTrackerMeetingType.SelectedItem = ddTrackerMeetingType.Items.FindByValue(currentTrackerItem.MeetingFormatCodeFK);
                    txtTrackerTargetAudience.Text = currentTrackerItem.TargetAudience;
                    txtTrackerMeetingLocation.Text = currentTrackerItem.MeetingLocation;
                    txtTrackerNumHours.Text = currentTrackerItem.NumHours.ToString("#.00");
                    ddTrackerDidEventOccur.SelectedItem = ddTrackerDidEventOccur.Items.FindByValue(currentTrackerItem.DidEventOccur);
                    txtTrackerEventNum.Text = currentTrackerItem.AspireEventNum;
                    txtTrackerCourseNum.Text = currentTrackerItem.CourseIDNum;

                    //Set the username label
                    PyramidUser masterCadreMemberUser = usersForDashboard.Where(u => u.UserName == currentTrackerItem.MasterCadreMemberUsername).FirstOrDefault();
                    lblTrackerMCUsername.Text = string.Format("{0} {1} ({2})", masterCadreMemberUser.FirstName, masterCadreMemberUser.LastName, masterCadreMemberUser.UserName);
                }

                //Set the title
                lblAddEditTrainingTracker.Text = "View Tracker Item";

                //Bind the dates
                BindTrackerItemDates();

                //Show tracker date history
                divTrainingTrackerItemDateHistory.Visible = true;

                //Show the div
                divAddEditTrainingTracker.Visible = true;

                //Hide initial date div and alert
                divTrainingTrackerItemInitialDate.Visible = false;
                divAddTrainingTrackerAlert.Visible = false;

                //Set focus to the first field
                ddTrackerActivity.Focus();

                //Disable the controls
                SetTrainingTrackerControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected tracker item!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a activity tracker
        /// and it opens the activity tracker edit div so that the user can edit the selected activity tracker
        /// </summary>
        /// <param name="sender">The lbEditTrainingTracker LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTrainingTracker_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the label with the PK for editing
            Label lblTrainingTrackerPK = (Label)item.FindControl("lblTrainingTrackerPK");

            //Get the PK from the label
            int? trainingTrackerPK = (string.IsNullOrWhiteSpace(lblTrainingTrackerPK.Text) ? (int?)null : Convert.ToInt32(lblTrainingTrackerPK.Text));

            if (trainingTrackerPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the tracker item
                    MasterCadreTrainingTrackerItem currentTrackerItem = context.MasterCadreTrainingTrackerItem.AsNoTracking().Where(i => i.MasterCadreTrainingTrackerItemPK == trainingTrackerPK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditTrainingTrackerPK.Value = currentTrackerItem.MasterCadreTrainingTrackerItemPK.ToString();
                    ddTrackerActivity.SelectedItem = ddTrackerActivity.Items.FindByValue(currentTrackerItem.MasterCadreActivityCodeFK);
                    beTrackerParticipantFee.Text = currentTrackerItem.ParticipantFee.ToString("#.00");
                    ddTrackerFundingSource.SelectedItem = ddTrackerFundingSource.Items.FindByValue(currentTrackerItem.MasterCadreFundingSourceCodeFK);
                    ddTrackerIsOpenToPublic.SelectedItem = ddTrackerIsOpenToPublic.Items.FindByValue(currentTrackerItem.IsOpenToPublic);
                    ddTrackerMeetingType.SelectedItem = ddTrackerMeetingType.Items.FindByValue(currentTrackerItem.MeetingFormatCodeFK);
                    txtTrackerTargetAudience.Text = currentTrackerItem.TargetAudience;
                    txtTrackerMeetingLocation.Text = currentTrackerItem.MeetingLocation;
                    txtTrackerNumHours.Text = currentTrackerItem.NumHours.ToString("#.00");
                    txtTrackerCourseNum.Text = currentTrackerItem.CourseIDNum;
                    ddTrackerDidEventOccur.SelectedItem = ddTrackerDidEventOccur.Items.FindByValue(currentTrackerItem.DidEventOccur);
                    txtTrackerEventNum.Text = currentTrackerItem.AspireEventNum;

                    //Set the username label
                    PyramidUser masterCadreMemberUser = usersForDashboard.Where(u => u.UserName == currentTrackerItem.MasterCadreMemberUsername).FirstOrDefault();
                    lblTrackerMCUsername.Text = string.Format("{0} {1} ({2})", masterCadreMemberUser.FirstName, masterCadreMemberUser.LastName, masterCadreMemberUser.UserName);
                }

                //Set the title
                lblAddEditTrainingTracker.Text = "Edit Tracker Item";

                //Show the div
                divAddEditTrainingTracker.Visible = true;

                //Hide initial date div and alert
                divTrainingTrackerItemInitialDate.Visible = false;
                divAddTrainingTrackerAlert.Visible = false;

                //Bind the tracker dates
                BindTrackerItemDates();

                //Show the date history
                divTrainingTrackerItemDateHistory.Visible = true;

                //Enable the controls
                SetTrainingTrackerControlUsability(true);

                //Set focus to the first field
                ddTrackerActivity.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected tracker item!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the activity trackers
        /// and it closes the activity tracker add/edit div
        /// </summary>
        /// <param name="sender">The submitTrainingTracker user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTrainingTracker_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditTrainingTrackerPK.Value = "0";
            divAddEditTrainingTracker.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitTrainingTracker control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTrainingTracker_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the activity trackers
        /// and it saves the tracker information to the database
        /// </summary>
        /// <param name="sender">The btnSaveTrainingTracker DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitTrainingTracker_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentTrainingTrackerPermissions.AllowedToEdit)
            {
                //Get the activity tracker pk
                int trackerItemPK = Convert.ToInt32(hfAddEditTrainingTrackerPK.Value);

                //Retrieve a list of the PKs that allow course ID
                string coursePKs = hfActivityPKsRequiringCourseID.Value;
                List<int> listCoursePKs = coursePKs.Split(',').Select(int.Parse).ToList();

                //Retrieve a list of the PKs that allow event ID
                string eventPKs = hfActivityPKsRequiringEventID.Value;
                List<int> listEventPKs = eventPKs.Split(',').Select(int.Parse).ToList();

                //Get the selected PK from the dropdown
                int selectedActivityPK = Convert.ToInt32(ddTrackerActivity.Value);

                string eventNum = (string.IsNullOrWhiteSpace(txtTrackerEventNum.Text) || listEventPKs.Contains(selectedActivityPK) == false ? null : txtTrackerEventNum.Text);
                string courseNum = (string.IsNullOrWhiteSpace(txtTrackerCourseNum.Text) || listCoursePKs.Contains(selectedActivityPK) == false ? null : txtTrackerCourseNum.Text);

                using (PyramidContext context = new PyramidContext())
                {
                    //Check to see if this is an add or an edit
                    if (trackerItemPK == 0)
                    {
                        DateTime initialStartTime = teTrackerItemInitialStartTime.DateTime;
                        DateTime initialEndTime = teTrackerItemInitialEndTime.DateTime;

                        DateTime initialStartDateTime = deTrackerItemInitialDate.Date.AddHours(initialStartTime.Hour).AddMinutes(initialStartTime.Minute);
                        DateTime initialEndDateTime = deTrackerItemInitialDate.Date.AddHours(initialEndTime.Hour).AddMinutes(initialEndTime.Minute);

                        //Add
                        //Fill the object
                        MasterCadreTrainingTrackerItem currentTrackerItem = new MasterCadreTrainingTrackerItem();
                        currentTrackerItem.Creator = User.Identity.Name;
                        currentTrackerItem.CreateDate = DateTime.Now;
                        currentTrackerItem.MasterCadreActivityCodeFK = selectedActivityPK;
                        currentTrackerItem.ParticipantFee = Convert.ToDecimal(beTrackerParticipantFee.Text);
                        currentTrackerItem.MasterCadreFundingSourceCodeFK = Convert.ToInt32(ddTrackerFundingSource.Value);
                        currentTrackerItem.IsOpenToPublic = Convert.ToBoolean(ddTrackerIsOpenToPublic.Value);
                        currentTrackerItem.MeetingFormatCodeFK = Convert.ToInt32(ddTrackerMeetingType.Value);
                        currentTrackerItem.TargetAudience = txtTrackerTargetAudience.Text;
                        currentTrackerItem.MeetingLocation = txtTrackerMeetingLocation.Text;
                        currentTrackerItem.NumHours = Convert.ToDecimal(txtTrackerNumHours.Text);
                        currentTrackerItem.DidEventOccur = Convert.ToBoolean(ddTrackerDidEventOccur.Value);
                        currentTrackerItem.AspireEventNum = eventNum;
                        currentTrackerItem.CourseIDNum = courseNum;
                        currentTrackerItem.MasterCadreMemberUsername = User.Identity.Name;
                        currentTrackerItem.StateFK = currentProgramRole.CurrentStateFK.Value;

                        //Add to the database
                        context.MasterCadreTrainingTrackerItem.Add(currentTrackerItem);

                        //Add
                        //Fill the Date object
                        MasterCadreTrainingTrackerItemDate currentTrackerDate = new MasterCadreTrainingTrackerItemDate();
                        currentTrackerDate.Creator = User.Identity.Name;
                        currentTrackerDate.CreateDate = DateTime.Now;
                        currentTrackerDate.MasterCadreTrainingTrackerItemFK = currentTrackerItem.MasterCadreTrainingTrackerItemPK;
                        currentTrackerDate.StartDateTime = initialStartDateTime;
                        currentTrackerDate.EndDateTime = initialEndDateTime;

                        //Add date object to database
                        context.MasterCadreTrainingTrackerItemDate.Add(currentTrackerDate);

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added the tracker item!", 10000);
                    }
                    else
                    {
                        //Edit
                        //Fill the object
                        MasterCadreTrainingTrackerItem currentTrackerItem = context.MasterCadreTrainingTrackerItem.Find(trackerItemPK);
                        currentTrackerItem.Editor = User.Identity.Name;
                        currentTrackerItem.EditDate = DateTime.Now;
                        currentTrackerItem.MasterCadreActivityCodeFK = selectedActivityPK;
                        currentTrackerItem.ParticipantFee = Convert.ToDecimal(beTrackerParticipantFee.Text);
                        currentTrackerItem.MasterCadreFundingSourceCodeFK = Convert.ToInt32(ddTrackerFundingSource.Value);
                        currentTrackerItem.IsOpenToPublic = Convert.ToBoolean(ddTrackerIsOpenToPublic.Value);
                        currentTrackerItem.MeetingFormatCodeFK = Convert.ToInt32(ddTrackerMeetingType.Value);
                        currentTrackerItem.TargetAudience = txtTrackerTargetAudience.Text;
                        currentTrackerItem.MeetingLocation = txtTrackerMeetingLocation.Text;
                        currentTrackerItem.NumHours = Convert.ToDecimal(txtTrackerNumHours.Text);
                        currentTrackerItem.DidEventOccur = Convert.ToBoolean(ddTrackerDidEventOccur.Value);
                        currentTrackerItem.AspireEventNum = eventNum;
                        currentTrackerItem.CourseIDNum = courseNum;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited the tracker item!", 10000);
                    }
                }

                //Reset the values in the hidden field and hide the div
                hfAddEditTrainingTrackerPK.Value = "0";
                divAddEditTrainingTracker.Visible = false;

                //Rebind the TrainingTracker gridview
                bsGRTrainingTrackers.DataBind();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a activity tracker
        /// and it deletes the tracker information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteTrainingTracker LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTrainingTracker_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentTrainingTrackerPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteTrainingTrackerPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTrainingTrackerPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the tracker item to remove
                            MasterCadreTrainingTrackerItem trackerItemToRemove = context.MasterCadreTrainingTrackerItem.Where(i => i.MasterCadreTrainingTrackerItemPK == rowToRemovePK).FirstOrDefault();

                            //Get the item dates to remove
                            var itemDatesToRemove = context.MasterCadreTrainingTrackerItemDate
                                .Where(id => id.MasterCadreTrainingTrackerItemFK == trackerItemToRemove.MasterCadreTrainingTrackerItemPK).ToList();

                            context.MasterCadreTrainingTrackerItemDate.RemoveRange(itemDatesToRemove);

                            //Remove the item
                            context.MasterCadreTrainingTrackerItem.Remove(trackerItemToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Check the item date deletions
                            if (itemDatesToRemove.Count > 0)
                            {
                                //Get a distinct list of item date PKs
                                var itemDatePKs = itemDatesToRemove.Select(dtr => dtr.MasterCadreTrainingTrackerItemDatePK).Distinct().ToList();

                                //Get the item date change rows and set the deleter
                                context.MasterCadreTrainingTrackerItemDateChanged.Where(assc => itemDatePKs.Contains(assc.MasterCadreTrainingTrackerItemDatePK))
                                                                .OrderByDescending(assc => assc.MasterCadreTrainingTrackerItemDateChangedPK)
                                                                .Take(itemDatesToRemove.Count).ToList()
                                                                .Select(assc => { assc.Deleter = User.Identity.Name; return assc; }).Count();
                            }



                            //Get the delete change row and set the deleter
                            context.MasterCadreTrainingTrackerItemChanged
                                    .OrderByDescending(c => c.MasterCadreTrainingTrackerItemPK)
                                    .Where(c => c.MasterCadreTrainingTrackerItemPK == trackerItemToRemove.MasterCadreTrainingTrackerItemPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();
                        }

                        //Rebind the activity tracker gridview
                        bsGRTrainingTrackers.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the activity tracker item!", 10000);
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
                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the activity tracker item, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the activity tracker item!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the activity tracker item!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the activity tracker item to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }


        protected void beTrackerParticipantFee_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            decimal participantFee;

            //Validate
            if (decimal.TryParse(beTrackerParticipantFee.Text, out participantFee))
            {
                if (participantFee < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "Fee for Participants must be over zero!";
                }
                else if (participantFee > 99999)
                {
                    e.IsValid = false;
                    e.ErrorText = "Fee for Participants must be below 99999!";
                }
            }
            else
            {
                e.IsValid = false;
                e.ErrorText = "Fee for Participants is required and must be a valid number!";
            }
        }

        protected void txtTrackerNumHours_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            decimal numHours;

            //Validate
            if (decimal.TryParse(txtTrackerNumHours.Text, out numHours))
            {
                if (numHours < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "# of Hours Planned for Session must be a positive number!";
                }
                else if (numHours > 999)
                {
                    e.IsValid = false;
                    e.ErrorText = "# of Hours Planned for Session must be below 999!";
                }
            }
            else
            {
                e.IsValid = false;
                e.ErrorText = "# of Hours Planned for Session is required and must be a valid number!";
            }
        }

        protected void txtTrackerEventNum_Validation(object sender, ValidationEventArgs e)
        {
            //Retrieve a list of the PKs that allow event ID
            string eventPKs = hfActivityPKsRequiringEventID.Value;
            List<int> PKs = eventPKs.Split(',').Select(int.Parse).ToList();

            //To hold the selected PK from the dropdown
            int selectedPK;

            //Parse the selected PK
            if (ddTrackerActivity.Value != null && int.TryParse(ddTrackerActivity.Value.ToString(), out selectedPK))
            {
                //If the field is empty, the current state is NY and the selected activity requires an event num display a required error
                if (string.IsNullOrWhiteSpace(txtTrackerEventNum.Text) && currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK && PKs.Contains(selectedPK))
                {
                    e.IsValid = false;
                    e.ErrorText = "Event ID number is required!";
                }
            }
        }

        protected void txtTrackerCourseNum_Validation(object sender, ValidationEventArgs e)
        {
            //Retrieve a list of the PKs that allow course ID
            string coursePKs = hfActivityPKsRequiringCourseID.Value;
            List<int> PKs = coursePKs.Split(',').Select(int.Parse).ToList();

            //To hold the selected PK from the dropdown
            int selectedPK;

            //Parse the selected PK
            if (ddTrackerActivity.Value != null && int.TryParse(ddTrackerActivity.Value.ToString(), out selectedPK))
            {
                //If the field is empty, the current state is NY and the selected activity requires a course num display a required error
                if (string.IsNullOrWhiteSpace(txtTrackerCourseNum.Text) && currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK && PKs.Contains(selectedPK))
                {
                    e.IsValid = false;
                    e.ErrorText = "Course ID number is required!";
                }
            }
        }

        protected void teTrackerItemInitialEndTime_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end time
            DateTime? startTime = (teTrackerItemInitialStartTime.Value == null ? (DateTime?)null : teTrackerItemInitialStartTime.DateTime);
            DateTime? endTime = (teTrackerItemInitialEndTime.Value == null ? (DateTime?)null : teTrackerItemInitialEndTime.DateTime);

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

        }

        #endregion

        #region Meeting Debriefs

        protected void efTrainingDebriefDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "MasterCadreTrainingDebriefPK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER)
            {
                //Master Cadre members only see their forms for their state
                e.QueryableSource = context.MasterCadreTrainingDebrief
                                            .Include(d => d.State)
                                            .AsNoTracking()
                                            .Where(d => d.MasterCadreMemberUsername == User.Identity.Name &&
                                                        d.StateFK == currentProgramRole.CurrentStateFK.Value);
            }
            else
            {
                //Other users see all their state's forms
                e.QueryableSource = context.MasterCadreTrainingDebrief
                                            .Include(d => d.State)
                                            .AsNoTracking()
                                            .Where(d => currentProgramRole.StateFKs.Contains(d.StateFK));
            }
        }

        protected void bsGRTrainingDebriefs_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the master cadre member
                Label lblMasterCadreMemberUsername = (Label)bsGRTrainingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRTrainingDebriefs.Columns["MasterCadreMemberColumn"], "lblMasterCadreMemberUsername");

                //Get the current coach username
                string currentUsername = (e.GetValue("MasterCadreMemberUsername") == null ? null : Convert.ToString(e.GetValue("MasterCadreMemberUsername")));

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching)
                        lblMasterCadreMemberUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetTrainingDebriefControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddDebriefActivity.ClientEnabled = enabled;
            deDebriefDateCompleted.ClientEnabled = enabled;
            txtDebriefEventNum.ClientEnabled = enabled;
            ddDebriefTrainingFormat.ClientEnabled = enabled;
            txtDebriefMeetingLocation.ClientEnabled = enabled;
            txtDebriefNumAttendees.ClientEnabled = enabled;
            txtDebriefNumEvalsReceived.ClientEnabled = enabled;
            ddDebriefWasUploadedToAspire.ClientEnabled = enabled;
            txtDebriefCoachingInterest.ClientEnabled = enabled;
            txtDebriefReflection.ClientEnabled = enabled;
            txtDebriefAssistanceNeeded.ClientEnabled = enabled;
            txtDebriefCourseNum.ClientEnabled = enabled;

            //Show/hide the submit button
            submitTrainingDebrief.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitTrainingDebrief.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit control's properties
            submitTrainingDebrief.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the training debriefs
        /// and it opens a div that allows the user to add a new item
        /// </summary>
        /// <param name="sender">The lbAddTrainingDebrief LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTrainingDebrief_Click(object sender, EventArgs e)
        {
            //Try to get the user record
            PyramidUser masterCadreMemberUser = usersForDashboard.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            //Make sure the record exists
            if (masterCadreMemberUser != null && currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER)
            {
                //Clear inputs in the div
                hfAddEditTrainingDebriefPK.Value = "0";
                ddDebriefActivity.Value = "";
                deDebriefDateCompleted.Value = "";
                txtDebriefEventNum.Value = "";
                ddDebriefTrainingFormat.Value = "";
                txtDebriefMeetingLocation.Value = "";
                txtDebriefNumAttendees.Value = "";
                txtDebriefNumEvalsReceived.Value = "";
                ddDebriefWasUploadedToAspire.Value = "";
                txtDebriefCoachingInterest.Value = "";
                txtDebriefReflection.Value = "";
                txtDebriefAssistanceNeeded.Value = "";
                txtDebriefCourseNum.Value = "";

                //Set the title
                lblAddEditTrainingDebrief.Text = "Add Training Debrief";

                //Set the username label
                lblDebriefMCUsername.Text = string.Format("{0} {1} ({2})", masterCadreMemberUser.FirstName, masterCadreMemberUser.LastName, masterCadreMemberUser.UserName);

                //Show the div
                divAddEditTrainingDebrief.Visible = true;

                //Set focus to the first field
                ddDebriefActivity.Focus();

                //Enable the controls
                SetTrainingDebriefControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized because you are not logged in as a Master Cadre Member!", 10000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a training debrief
        /// and it opens the training debrief edit div in view-only mode
        /// </summary>
        /// <param name="sender">The lbViewTrainingDebrief LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewTrainingDebrief_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblTrainingDebriefPK = (Label)item.FindControl("lblTrainingDebriefPK");

            //Get the PK from the label
            int? trainingDebriefPK = (string.IsNullOrWhiteSpace(lblTrainingDebriefPK.Text) ? (int?)null : Convert.ToInt32(lblTrainingDebriefPK.Text));

            if (trainingDebriefPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the debrief
                    MasterCadreTrainingDebrief currentDebrief = context.MasterCadreTrainingDebrief.AsNoTracking().Where(d => d.MasterCadreTrainingDebriefPK == trainingDebriefPK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditTrainingDebriefPK.Value = currentDebrief.MasterCadreTrainingDebriefPK.ToString();
                    ddDebriefActivity.SelectedItem = ddDebriefActivity.Items.FindByValue(currentDebrief.MasterCadreActivityCodeFK);
                    deDebriefDateCompleted.Date = currentDebrief.DateCompleted;
                    txtDebriefEventNum.Text = currentDebrief.AspireEventNum;
                    ddDebriefTrainingFormat.SelectedItem = ddDebriefTrainingFormat.Items.FindByValue(currentDebrief.MeetingFormatCodeFK);
                    txtDebriefMeetingLocation.Text = currentDebrief.MeetingLocation;
                    txtDebriefNumAttendees.Text = currentDebrief.NumAttendees.ToString();
                    txtDebriefNumEvalsReceived.Text = currentDebrief.NumEvalsReceived.ToString();
                    ddDebriefWasUploadedToAspire.SelectedItem = ddDebriefWasUploadedToAspire.Items.FindByValue(currentDebrief.WasUploadedToAspire);
                    txtDebriefCoachingInterest.Text = currentDebrief.CoachingInterest;
                    txtDebriefReflection.Text = currentDebrief.Reflection;
                    txtDebriefAssistanceNeeded.Text = currentDebrief.AssistanceNeeded;
                    txtDebriefCourseNum.Text = currentDebrief.CourseIDNum;

                    //Set the username label
                    PyramidUser masterCadreMemberUser = usersForDashboard.Where(u => u.UserName == currentDebrief.MasterCadreMemberUsername).FirstOrDefault();
                    lblDebriefMCUsername.Text = string.Format("{0} {1} ({2})", masterCadreMemberUser.FirstName, masterCadreMemberUser.LastName, masterCadreMemberUser.UserName);
                }

                //Set the title
                lblAddEditTrainingDebrief.Text = "View Activity Debrief";

                //Show the div
                divAddEditTrainingDebrief.Visible = true;

                //Set focus to the first field
                ddDebriefActivity.Focus();

                //Disable the controls
                SetTrainingDebriefControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected training debrief!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a training debrief
        /// and it opens the training debrief edit div
        /// </summary>
        /// <param name="sender">The lbEditTrainingDebrief LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTrainingDebrief_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the label with the PK for editing
            Label lblTrainingDebriefPK = (Label)item.FindControl("lblTrainingDebriefPK");

            //Get the PK from the label
            int? trainingDebriefPK = (string.IsNullOrWhiteSpace(lblTrainingDebriefPK.Text) ? (int?)null : Convert.ToInt32(lblTrainingDebriefPK.Text));

            if (trainingDebriefPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the debrief
                    MasterCadreTrainingDebrief currentDebrief = context.MasterCadreTrainingDebrief.AsNoTracking().Where(d => d.MasterCadreTrainingDebriefPK == trainingDebriefPK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditTrainingDebriefPK.Value = currentDebrief.MasterCadreTrainingDebriefPK.ToString();
                    ddDebriefActivity.SelectedItem = ddDebriefActivity.Items.FindByValue(currentDebrief.MasterCadreActivityCodeFK);
                    deDebriefDateCompleted.Date = currentDebrief.DateCompleted;
                    txtDebriefEventNum.Text = currentDebrief.AspireEventNum;
                    ddDebriefTrainingFormat.SelectedItem = ddDebriefTrainingFormat.Items.FindByValue(currentDebrief.MeetingFormatCodeFK);
                    txtDebriefMeetingLocation.Text = currentDebrief.MeetingLocation;
                    txtDebriefNumAttendees.Text = currentDebrief.NumAttendees.ToString();
                    txtDebriefNumEvalsReceived.Text = currentDebrief.NumEvalsReceived.ToString();
                    ddDebriefWasUploadedToAspire.SelectedItem = ddDebriefWasUploadedToAspire.Items.FindByValue(currentDebrief.WasUploadedToAspire);
                    txtDebriefCoachingInterest.Text = currentDebrief.CoachingInterest;
                    txtDebriefReflection.Text = currentDebrief.Reflection;
                    txtDebriefAssistanceNeeded.Text = currentDebrief.AssistanceNeeded;
                    txtDebriefCourseNum.Text = currentDebrief.CourseIDNum;

                    //Set the username label
                    PyramidUser masterCadreMemberUser = usersForDashboard.Where(u => u.UserName == currentDebrief.MasterCadreMemberUsername).FirstOrDefault();
                    lblDebriefMCUsername.Text = string.Format("{0} {1} ({2})", masterCadreMemberUser.FirstName, masterCadreMemberUser.LastName, masterCadreMemberUser.UserName);
                }

                //Set the title
                lblAddEditTrainingDebrief.Text = "Edit Activity Debrief";

                //Show the div
                divAddEditTrainingDebrief.Visible = true;

                //Set focus to the first field
                ddDebriefActivity.Focus();

                //Enable the controls
                SetTrainingDebriefControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected training debrief!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the training debriefs
        /// and it closes the training debrief add/edit div
        /// </summary>
        /// <param name="sender">The submitTrainingDebrief user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTrainingDebrief_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditTrainingDebriefPK.Value = "0";
            divAddEditTrainingDebrief.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitTrainingDebrief control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTrainingDebrief_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the training debriefs
        /// and it saves the debrief information to the database
        /// </summary>
        /// <param name="sender">The btnSaveTrainingDebrief DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitTrainingDebrief_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentTrainingDebriefPermissions.AllowedToEdit)
            {
                //Get the training debrief pk
                int debriefPK = Convert.ToInt32(hfAddEditTrainingDebriefPK.Value);

                //Retrieve a list of the PKs that allow course ID
                string coursePKs = hfActivityPKsRequiringCourseID.Value;
                List<int> listCoursePKs = coursePKs.Split(',').Select(int.Parse).ToList();

                //Retrieve a list of the PKs that allow event ID
                string eventPKs = hfActivityPKsRequiringEventID.Value;
                List<int> listEventPKs = eventPKs.Split(',').Select(int.Parse).ToList();

                //Get the selected PK from the dropdown
                int selectedActivityPK = Convert.ToInt32(ddDebriefActivity.Value);

                string eventNum = (string.IsNullOrWhiteSpace(txtDebriefEventNum.Text) || listEventPKs.Contains(selectedActivityPK) == false ? null : txtDebriefEventNum.Text);
                string courseNum = (string.IsNullOrWhiteSpace(txtDebriefCourseNum.Text) || listCoursePKs.Contains(selectedActivityPK) == false ? null : txtDebriefCourseNum.Text);

                using (PyramidContext context = new PyramidContext())
                {
                    //Check to see if this is an add or an edit
                    if (debriefPK == 0)
                    {
                        //Add
                        //Fill the object
                        MasterCadreTrainingDebrief currentDebrief = new MasterCadreTrainingDebrief();
                        currentDebrief.Creator = User.Identity.Name;
                        currentDebrief.CreateDate = DateTime.Now;
                        currentDebrief.MasterCadreActivityCodeFK = selectedActivityPK;
                        currentDebrief.DateCompleted = deDebriefDateCompleted.Date;
                        currentDebrief.MeetingFormatCodeFK = Convert.ToInt32(ddDebriefTrainingFormat.Value);
                        currentDebrief.MeetingLocation = txtDebriefMeetingLocation.Text;
                        currentDebrief.NumAttendees = Convert.ToInt32(txtDebriefNumAttendees.Text);
                        currentDebrief.NumEvalsReceived = Convert.ToInt32(txtDebriefNumEvalsReceived.Text);
                        currentDebrief.WasUploadedToAspire = (ddDebriefWasUploadedToAspire.Value == null ? (bool?)null : Convert.ToBoolean(ddDebriefWasUploadedToAspire.Value));
                        currentDebrief.CoachingInterest = txtDebriefCoachingInterest.Text;
                        currentDebrief.Reflection = txtDebriefReflection.Text;
                        currentDebrief.AssistanceNeeded = txtDebriefAssistanceNeeded.Text;
                        currentDebrief.MasterCadreMemberUsername = User.Identity.Name;
                        currentDebrief.StateFK = currentProgramRole.CurrentStateFK.Value;
                        currentDebrief.AspireEventNum = eventNum;
                        currentDebrief.CourseIDNum = courseNum;

                        //Add to the database
                        context.MasterCadreTrainingDebrief.Add(currentDebrief);

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added the training debrief!", 10000);
                    }
                    else
                    {

                        //Edit
                        //Fill the object
                        MasterCadreTrainingDebrief currentDebrief = context.MasterCadreTrainingDebrief.Find(debriefPK);
                        currentDebrief.Editor = User.Identity.Name;
                        currentDebrief.EditDate = DateTime.Now;
                        currentDebrief.MasterCadreActivityCodeFK = selectedActivityPK;
                        currentDebrief.DateCompleted = deDebriefDateCompleted.Date;
                        currentDebrief.MeetingFormatCodeFK = Convert.ToInt32(ddDebriefTrainingFormat.Value);
                        currentDebrief.MeetingLocation = txtDebriefMeetingLocation.Text;
                        currentDebrief.NumAttendees = Convert.ToInt32(txtDebriefNumAttendees.Text);
                        currentDebrief.NumEvalsReceived = Convert.ToInt32(txtDebriefNumEvalsReceived.Text);
                        currentDebrief.WasUploadedToAspire = (ddDebriefWasUploadedToAspire.Value == null ? (bool?)null : Convert.ToBoolean(ddDebriefWasUploadedToAspire.Value));
                        currentDebrief.CoachingInterest = txtDebriefCoachingInterest.Text;
                        currentDebrief.Reflection = txtDebriefReflection.Text;
                        currentDebrief.AssistanceNeeded = txtDebriefAssistanceNeeded.Text;
                        currentDebrief.AspireEventNum = eventNum;
                        currentDebrief.CourseIDNum = courseNum;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited the training debrief!", 10000);
                    }
                }

                //Reset the values in the hidden field and hide the div
                hfAddEditTrainingDebriefPK.Value = "0";
                divAddEditTrainingDebrief.Visible = false;

                //Rebind the TrainingDebrief gridview
                bsGRTrainingDebriefs.DataBind();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a training debrief
        /// and it deletes the debrief information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteTrainingDebrief LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTrainingDebrief_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentTrainingDebriefPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteTrainingDebriefPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTrainingDebriefPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the debrief item to remove
                            MasterCadreTrainingDebrief debriefToRemove = context.MasterCadreTrainingDebrief.Where(d => d.MasterCadreTrainingDebriefPK == rowToRemovePK).FirstOrDefault();

                            //Remove the item
                            context.MasterCadreTrainingDebrief.Remove(debriefToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.MasterCadreTrainingDebriefChanged
                                    .OrderByDescending(c => c.MasterCadreTrainingDebriefPK)
                                    .Where(c => c.MasterCadreTrainingDebriefPK == debriefToRemove.MasterCadreTrainingDebriefPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();
                        }

                        //Rebind the training debrief gridview
                        bsGRTrainingDebriefs.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the training debrief!", 10000);
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
                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the training debrief, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the training debrief!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the training debrief!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the training debrief to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        protected void deDebriefDateCompleted_Validation(object sender, ValidationEventArgs e)
        {
            //Get the date from the control
            DateTime currentDateCompleted = deDebriefDateCompleted.Date;

            //Validate
            if (currentDateCompleted == DateTime.MinValue)
            {
                e.IsValid = false;
                e.ErrorText = "Date Completed is required!";
            }
            else if (currentDateCompleted > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Date Completed cannot be in the future!";
            }
        }

        protected void txtDebriefNumAttendees_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int numAttendees;

            //Validate
            if (int.TryParse(txtDebriefNumAttendees.Text, out numAttendees))
            {
                if (numAttendees < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "# of Attendees must be a positive number!";
                }
                else if (numAttendees > 99999)
                {
                    e.IsValid = false;
                    e.ErrorText = "# of Attendees must be below 99999!";
                }
            }
            else
            {
                e.IsValid = false;
                e.ErrorText = "# of Attendees is required and must be a valid number!";
            }
        }

        protected void txtDebriefNumEvalsReceived_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int numEvalsReceived;

            //Validate
            if (int.TryParse(txtDebriefNumEvalsReceived.Text, out numEvalsReceived))
            {
                if (numEvalsReceived < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "# of Evaluations Received must be a positive number!";
                }
                else if (numEvalsReceived > 99999)
                {
                    e.IsValid = false;
                    e.ErrorText = "# of Evaluations Received must be below 99999!";
                }
            }
            else
            {
                e.IsValid = false;
                e.ErrorText = "# of Evaluations Received is required and must be a valid number!";
            }
        }

        protected void txtDebriefEventNum_Validation(object sender, ValidationEventArgs e)
        {
            //Retrieve a list of the PKs that allow event ID
            string eventPKs = hfActivityPKsRequiringEventID.Value;
            List<int> PKs = eventPKs.Split(',').Select(int.Parse).ToList();

            //To hold the selected PK from the dropdown
            int selectedPK;

            //Parse the selected PK
            if (ddDebriefActivity.Value != null && int.TryParse(ddDebriefActivity.Value.ToString(), out selectedPK))
            {
                //If the field is empty, the current state is NY and the selected activity requires an event num display a required error
                if (string.IsNullOrWhiteSpace(txtDebriefEventNum.Text) && currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK && PKs.Contains(selectedPK))
                {
                    e.IsValid = false;
                    e.ErrorText = "Event ID number is required!";
                }
            }
        }

        protected void txtDebriefCourseNum_Validation(object sender, ValidationEventArgs e)
        {
            //Retrieve a list of the PKs that allow course ID
            string coursePKs = hfActivityPKsRequiringCourseID.Value;
            List<int> PKs = coursePKs.Split(',').Select(int.Parse).ToList();

            //To hold the selected PK from the dropdown
            int selectedPK;

            //Parse the selected PK
            if (ddDebriefActivity.Value != null && int.TryParse(ddDebriefActivity.Value.ToString(), out selectedPK))
            {
                //If the field is empty, the current state is NY and the selected activity requires a course num display a required error
                if (string.IsNullOrWhiteSpace(txtDebriefCourseNum.Text) && currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK && PKs.Contains(selectedPK))
                {
                    e.IsValid = false;
                    e.ErrorText = "Course ID number is required!";
                }
            }
        }

        #endregion

        #region Tracker Dates

        /// <summary>
        /// This method populates the Tracker Item Dates repeater with up-to-date information
        /// </summary>
        private void BindTrackerItemDates()
        {
            //Get the Tracker PK
            int trackerPK = Convert.ToInt32(hfAddEditTrainingTrackerPK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                List<MasterCadreTrainingTrackerItemDate> itemDates = context.MasterCadreTrainingTrackerItemDate
                    .Include(s => s.MasterCadreTrainingTrackerItem)
                    .AsNoTracking()
                    .Where(t => t.MasterCadreTrainingTrackerItemFK == trackerPK)
                    .ToList();

                repeatTrainingTrackerItemDates.DataSource = itemDates;
                repeatTrainingTrackerItemDates.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for an item date
        /// and it opens the edit div
        /// </summary>
        /// <param name="sender">The lbEditTrainingTrackerItemDate_Click LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditTrainingTrackerItemDate_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the label with the PK
            Label lblTrackerItemDatePK = (Label)item.FindControl("lblMasterCadreTrainingTrackerItemDatePK");

            //Get the PK from label
            int? trackerItemDatePK = (string.IsNullOrWhiteSpace(lblTrackerItemDatePK.Text) ? (int?)null : Convert.ToInt32(lblTrackerItemDatePK.Text));

            if (trackerItemDatePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the item date to edit
                    MasterCadreTrainingTrackerItemDate editItemDate = context.MasterCadreTrainingTrackerItemDate.AsNoTracking().Where(s => s.MasterCadreTrainingTrackerItemDatePK == trackerItemDatePK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditTrainingTrackerItemDate.Text = "Edit Tracker Item Date";
                    deTrainingTrackerItemDate.Date = editItemDate.StartDateTime.Date;  //Make sure to only use the date part of the datetime
                    teTrainingTrackerItemStartTime.DateTime = editItemDate.StartDateTime;
                    teTrainingTrackerItemEndTime.DateTime = editItemDate.EndDateTime;
                    hfAddEditTrainingTrackerItemDatePK.Value = trackerItemDatePK.Value.ToString();
                }

                //Show edit div
                divAddEditTrainingTrackerItemDate.Visible = true;

                //Enable controls
                SetTrackerItemDateControlUsability(true);

                //Set focus to the first field
                deTrainingTrackerItemDate.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected tracker item date!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for an item date
        /// </summary>
        /// <param name="sender">The lbDeleteTrainingTrackerItemDate_Click LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTrainingTrackerItemDate_Click(object sender, EventArgs e)
        {
            //Check to see if the user is allowed to edit training tracker information
            if (currentTrainingTrackerPermissions.AllowedToEdit)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteMasterCadreTrainingTrackerItemDatePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteMasterCadreTrainingTrackerItemDatePK.Value));

                //Remove the row if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the item date to remove
                            MasterCadreTrainingTrackerItemDate itemDateToRemove = context.MasterCadreTrainingTrackerItemDate.Where(t => t.MasterCadreTrainingTrackerItemDatePK == rowToRemovePK).FirstOrDefault();

                            //Get the number of other date records for this tracker item
                            int numOtherDateRecords = context.MasterCadreTrainingTrackerItemDate
                                                                .Where(s => s.MasterCadreTrainingTrackerItemDatePK != itemDateToRemove.MasterCadreTrainingTrackerItemDatePK &&
                                                                            s.MasterCadreTrainingTrackerItemFK == itemDateToRemove.MasterCadreTrainingTrackerItemFK)
                                                                .Count();

                            //Don't allow deletion if there are no other date records for this tracker item
                            if (numOtherDateRecords > 0)
                            {
                                //Remove the item date
                                context.MasterCadreTrainingTrackerItemDate.Remove(itemDateToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.MasterCadreTrainingTrackerItemDateChanged
                                        .OrderByDescending(c => c.MasterCadreTrainingTrackerItemDateChangedPK)
                                        .Where(c => c.MasterCadreTrainingTrackerItemDatePK == itemDateToRemove.MasterCadreTrainingTrackerItemDatePK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Hide the add/edit div after deletion
                                divAddEditTrainingTrackerItemDate.Visible = false;

                                //Rebind the item date table
                                BindTrackerItemDates();

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the tracker item date!", 10000);
                            }
                            else
                            {
                                //Show an error message
                                msgSys.ShowMessageToUser("danger", "Deletion Failed", "Cannot delete the only tracker item date record!  There must be at least one date for each tracker item.", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the tracker item date, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the tracker item date!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the tracker item date!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the tracker item date to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

        }

        /// <summary>
        /// This method executes when the user clicks the add button for an item date
        /// and it opens the add div
        /// </summary>
        /// <param name="sender">The lbAddTrainingTrackerItemDate_Click LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddTrainingTrackerItemDate_Click(object sender, EventArgs e)
        {
            //Clear inputs in the input div
            hfAddEditTrainingTrackerItemDatePK.Value = "0";
            deTrainingTrackerItemDate.Value = "";
            teTrainingTrackerItemStartTime.Value = "";
            teTrainingTrackerItemEndTime.Value = "";

            //Set the title
            lblAddEditTrainingTrackerItemDate.Text = "Add Tracker Item Date";

            //Show the input div
            divAddEditTrainingTrackerItemDate.Visible = true;

            //Set focus to the first field
            deTrainingTrackerItemDate.Focus();

            //Enable the controls
            SetTrackerItemDateControlUsability(true);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the tracker date
        /// add/edit and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitTrainingTrackerItemDate submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitTrainingTrackerItemDate_Click(object sender, EventArgs e)
        {
            //Check if the user is allowed to edit information
            if (currentTrainingTrackerPermissions.AllowedToEdit)
            {
                //Get the necessary values
                int trackerDatePK = Convert.ToInt32(hfAddEditTrainingTrackerItemDatePK.Value);
                int trackerItemFK = Convert.ToInt32(hfAddEditTrainingTrackerPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //Hold the object
                    MasterCadreTrainingTrackerItemDate currentTrackerItemDate;

                    //Get the datetimes
                    DateTime startTime = deTrainingTrackerItemDate.Date.AddHours(teTrainingTrackerItemStartTime.DateTime.Hour).AddMinutes(teTrainingTrackerItemStartTime.DateTime.Minute);
                    DateTime endTime = deTrainingTrackerItemDate.Date.AddHours(teTrainingTrackerItemEndTime.DateTime.Hour).AddMinutes(teTrainingTrackerItemEndTime.DateTime.Minute);

                    if (trackerDatePK == 0)
                    {
                        //add
                        currentTrackerItemDate = new MasterCadreTrainingTrackerItemDate();
                        currentTrackerItemDate.MasterCadreTrainingTrackerItemFK = trackerItemFK;
                        currentTrackerItemDate.StartDateTime = startTime;
                        currentTrackerItemDate.EndDateTime = endTime;
                        currentTrackerItemDate.Creator = User.Identity.Name;
                        currentTrackerItemDate.CreateDate = DateTime.Now;

                        //Save to the database
                        context.MasterCadreTrainingTrackerItemDate.Add(currentTrackerItemDate);
                        context.SaveChanges();

                        msgSys.ShowMessageToUser("success", "Success", "Successfully added tracker item date!", 10000);
                    }
                    else
                    {
                        //edit
                        currentTrackerItemDate = context.MasterCadreTrainingTrackerItemDate.Find(trackerDatePK);
                        currentTrackerItemDate.StartDateTime = startTime;
                        currentTrackerItemDate.EndDateTime = endTime;
                        currentTrackerItemDate.Editor = User.Identity.Name;
                        currentTrackerItemDate.EditDate = DateTime.Now;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited tracker item date!", 10000);

                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditTrainingTrackerItemDatePK.Value = "0";
                    divAddEditTrainingTrackerItemDate.Visible = false;

                    //Rebind item date table
                    BindTrackerItemDates();

                    //Rebind the TrainingTracker gridview
                    bsGRTrainingTrackers.DataBind();
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        protected void submitTrainingTrackerItemDate_CancelClick(object sender, EventArgs e)
        {
            //Clear necessary values 
            hfAddEditTrainingTrackerItemDatePK.Value = "0";
            divAddEditTrainingTrackerItemDate.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitTrainingTrackerItemDate control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitTrainingTrackerItemDate_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);

        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetTrackerItemDateControlUsability(bool enabled)
        {
            //Enable/disable the controls
            deTrainingTrackerItemDate.ClientEnabled = enabled;
            teTrainingTrackerItemStartTime.ClientEnabled = enabled;
            teTrainingTrackerItemEndTime.ClientEnabled = enabled;

            //Show/hide the submit button
            submitTrainingTrackerItemDate.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitTrainingTrackerItemDate.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit properties
            submitTrainingTrackerItemDate.UpdateProperties();
        }

        protected void teTrainingTrackerItemEndTime_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end time
            DateTime? startTime = (teTrainingTrackerItemStartTime.Value == null ? (DateTime?)null : teTrainingTrackerItemStartTime.DateTime);
            DateTime? endTime = (teTrainingTrackerItemEndTime.Value == null ? (DateTime?)null : teTrainingTrackerItemEndTime.DateTime);

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
        }

        #endregion
    }
}