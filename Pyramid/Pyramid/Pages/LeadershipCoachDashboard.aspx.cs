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
    public partial class LeadershipCoachDashboard : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "LCL",
                    "LCPS",
                    "LCHS",
                    "LCPD",
                    "LCHD",
                    "LCCS",
                    "LCCD"
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
        private CodeProgramRolePermission currentLeadershipCoachLogPermissions;
        private CodeProgramRolePermission currentProgramSchedulePermissions;
        private CodeProgramRolePermission currentHubSchedulePermissions;
        private CodeProgramRolePermission currentProgramDebriefPermissions;
        private CodeProgramRolePermission currentHubDebriefPermissions;
        private CodeProgramRolePermission currentCoachingCircleSchedulePermissions;
        private CodeProgramRolePermission currentCoachingCircleDebriefPermissions;
        private List<PyramidUser> usersForDashboard;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the leadership coach records
            usersForDashboard = new List<PyramidUser>();
            usersForDashboard.AddRange(PyramidUser.GetProgramLeadershipCoachUserRecords(currentProgramRole.ProgramFKs));
            usersForDashboard.AddRange(PyramidUser.GetHubLeadershipCoachUserRecords(currentProgramRole.HubFKs));

            //Get the permission objects
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permissions
            currentLeadershipCoachLogPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCL").FirstOrDefault();
            currentProgramSchedulePermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCPS").FirstOrDefault();
            currentHubSchedulePermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCHS").FirstOrDefault();
            currentProgramDebriefPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCPD").FirstOrDefault();
            currentHubDebriefPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCHD").FirstOrDefault();
            currentCoachingCircleSchedulePermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCCS").FirstOrDefault();
            currentCoachingCircleDebriefPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "LCCD").FirstOrDefault();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Set the permissions for the sections
            //Leadership Coach Logs
            SetPermissions(new List<CodeProgramRolePermission> { currentLeadershipCoachLogPermissions }, divLeadershipCoachLogs, bsGRLeadershipCoachLogs, hfLeadershipCoachLogViewOnly);

            //Hub and Program Meeting Schedules
            SetPermissions(new List<CodeProgramRolePermission> { currentProgramSchedulePermissions, currentHubSchedulePermissions }, divMeetingSchedules, bsGRMeetingSchedules, hfMeetingScheduleViewOnly);

            //Hub and Program Meeting Debriefs
            SetPermissions(new List<CodeProgramRolePermission> { currentProgramDebriefPermissions, currentHubDebriefPermissions }, divMeetingDebriefs, bsGRMeetingDebriefs, hfMeetingDebriefViewOnly);

            //Coaching Circle/Community of Practice meeting schedules
            SetPermissions(new List<CodeProgramRolePermission> { currentCoachingCircleSchedulePermissions }, divCCSchedules, bsGRCCMeetingSchedules, hfCCMeetingScheduleViewOnly);

            //Coaching Circle/Community of Practice meeting debriefs
            SetPermissions(new List<CodeProgramRolePermission> { currentCoachingCircleDebriefPermissions }, divCCDebriefs, bsGRCCMeetingDebriefs, hfCCMeetingDebriefViewOnly);

            if (!IsPostBack)
            {
                //Bind the dropdowns
                BindDropdowns();

                //Set the display of the debrief add buttons
                lnkAddProgramDebrief.Visible = currentProgramDebriefPermissions.AllowedToAdd;
                lnkAddHubDebrief.Visible = currentHubDebriefPermissions.AllowedToAdd;

                //Check for a message in the query string
                string messagetype = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messagetype))
                {
                    switch (messagetype)
                    {
                        case "DebriefAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Leadership Team Meeting Debrief successfully added!", 1000);
                            break;
                        case "DebriefEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Leadership Team Meeting Debrief successfully edited!", 1000);
                            break;
                        case "DebriefCancelled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoDebrief":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Leadership Team Meeting Debrief could not be found, please try again.", 15000);
                            break;
                        case "CCDebriefAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Coaching Circle/Community of Practice Meeting Debrief successfully added!", 1000);
                            break;
                        case "CCDebriefEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Coaching Circle/Community of Practice Meeting Debrief successfully edited!", 1000);
                            break;
                        case "CCDebriefCancelled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoCCDebrief":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Coaching Circle/Community of Practice Meeting Debrief could not be found, please try again.", 15000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
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
        private void SetPermissions(List<CodeProgramRolePermission> permissions, HtmlGenericControl sectionDiv, BootstrapGridView gridview, HiddenField viewOnlyHiddenField)
        {
            //Check permissions
            if (permissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                //Not allowed to see the section
                sectionDiv.Visible = false;
            }
            else if (permissions.Where(p => p.AllowedToView == true).Count() == 0)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (gridview.Columns.Count - 1);

                //Hide the action column
                gridview.Columns[actionColumnIndex].Visible = false;

                //Hide management options
                viewOnlyHiddenField.Value = "True";
            }
            else if (permissions.Where(p => p.AllowedToAdd == true || p.AllowedToEdit == true).Count() == 0)
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
            //To hold all the programs and hubs
            List<Program> allPrograms = new List<Program>();
            List<Hub> allHubs = new List<Hub>();

            //Get all the programs and hubs
            using (PyramidContext context = new PyramidContext())
            {
                allPrograms = context.Program.AsNoTracking().Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK)).OrderBy(p => p.ProgramName).ToList();
                allHubs = context.Hub.AsNoTracking().Where(h => currentProgramRole.HubFKs.Contains(h.HubPK)).OrderBy(h => h.Name).ToList();
            }

            //Bind the program dropdowns
            ddMeetingScheduleProgram.DataSource = allPrograms;
            ddMeetingScheduleProgram.DataBind();

            //Bind the hub dropdowns
            ddMeetingScheduleHub.DataSource = allHubs;
            ddMeetingScheduleHub.DataBind();
        }

        #region Meeting Schedules

        protected void efMeetingScheduleDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CustomViewPK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            e.QueryableSource = context.LCMeetingScheduleView
                                        .AsNoTracking()
                                        .Where(s => (currentProgramSchedulePermissions.AllowedToViewDashboard && s.ProgramFK.HasValue && currentProgramRole.ProgramFKs.Contains(s.ProgramFK.Value)) ||
                                                        (currentHubSchedulePermissions.AllowedToViewDashboard && s.HubFK.HasValue && currentProgramRole.HubFKs.Contains(s.HubFK.Value)));
        }

        protected void bsGRMeetingSchedules_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the controls from the row
                Label lblLeadershipCoachUsername = (Label)bsGRMeetingSchedules.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingSchedules.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");
                LinkButton lbViewMeetingSchedule = (LinkButton)bsGRMeetingSchedules.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingSchedules.Columns["ActionColumn"], "lbViewMeetingSchedule");
                LinkButton lbEditMeetingSchedule = (LinkButton)bsGRMeetingSchedules.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingSchedules.Columns["ActionColumn"], "lbEditMeetingSchedule");
                HtmlButton btnDeleteMeetingSchedule = (HtmlButton)bsGRMeetingSchedules.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingSchedules.Columns["ActionColumn"], "btnDeleteMeetingSchedule");
                Label lblMeetingScheduleType = (Label)bsGRMeetingSchedules.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingSchedules.Columns["ActionColumn"], "lblMeetingScheduleType");

                //Get the necessary values
                string currentUsername = (e.GetValue("LeadershipCoachUsername") == null ? null : Convert.ToString(e.GetValue("LeadershipCoachUsername")));
                string scheduleType = (e.GetValue("MeetingScheduleType") == null ? null : Convert.ToString(e.GetValue("MeetingScheduleType")).ToLower());

                //Set the display of the buttons based on type of schedule and permissions
                if (scheduleType == "program")
                {
                    if (lbViewMeetingSchedule != null)
                        lbViewMeetingSchedule.Visible = currentProgramSchedulePermissions.AllowedToView;

                    if (lbEditMeetingSchedule != null)
                        lbEditMeetingSchedule.Visible = currentProgramSchedulePermissions.AllowedToEdit;

                    if (btnDeleteMeetingSchedule != null)
                        btnDeleteMeetingSchedule.Visible = currentProgramSchedulePermissions.AllowedToDelete;
                }
                else if (scheduleType == "hub")
                {
                    if (lbViewMeetingSchedule != null)
                        lbViewMeetingSchedule.Visible = currentHubSchedulePermissions.AllowedToView;

                    if (lbEditMeetingSchedule != null)
                        lbEditMeetingSchedule.Visible = currentHubSchedulePermissions.AllowedToEdit;

                    if (btnDeleteMeetingSchedule != null)
                        btnDeleteMeetingSchedule.Visible = currentHubSchedulePermissions.AllowedToDelete;
                }

                //Make sure the username exists
                if(string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if(currentUser != null)
                    {
                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                    else
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        currentUser = PyramidUser.GetUserRecordByUsername(currentUsername);

                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddMeetingScheduleHub DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddMeetingScheduleHub BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddMeetingScheduleHub_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int? selectedHubFK;
            string selectedScheduleType, leadershipCoachUsername;
            int currentSchedulePK, selectedYear;

            //Get the necessary values
            int.TryParse(hfAddEditMeetingSchedulePK.Value, out currentSchedulePK);
            leadershipCoachUsername = (string.IsNullOrWhiteSpace(hfAddEditMeetingScheduleLeadershipCoachUsername.Value) ? User.Identity.Name : hfAddEditMeetingScheduleLeadershipCoachUsername.Value);
            selectedScheduleType = (ddMeetingScheduleType.Value == null ? null : ddMeetingScheduleType.Value.ToString());

            //Check to see if the schedule type is selected
            if (string.IsNullOrWhiteSpace(selectedScheduleType) == false && selectedScheduleType.ToLower() == "hub")
            {
                //Get the Hub FK
                selectedHubFK = (ddMeetingScheduleHub.Value == null ? (int?)null : Convert.ToInt32(ddMeetingScheduleHub.Value));

                //Validate
                if (selectedHubFK.HasValue == false)
                {
                    e.IsValid = false;
                    e.ErrorText = "Hub is required!";
                }
                else
                {
                    //Check for duplication
                    if (deMeetingScheduleYear.Date != DateTime.MinValue)
                    {
                        //Get the selected year
                        selectedYear = deMeetingScheduleYear.Date.Year;

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Check to see if there are any schedules for this coach and year and Hub
                            List<HubLCMeetingSchedule> duplicateSchedules = context.HubLCMeetingSchedule.AsNoTracking()
                                            .Where(s => s.HubFK == selectedHubFK.Value &&
                                                    s.MeetingYear == selectedYear &&
                                                    s.HubLCMeetingSchedulePK != currentSchedulePK &&
                                                    s.LeadershipCoachUsername == leadershipCoachUsername).ToList();

                            //Check the count of duplicate schedules
                            if (duplicateSchedules.Count > 0)
                            {
                                e.IsValid = false;
                                e.ErrorText = "A meeting schedule already exists for this combination of leadership coach, hub, and year!";
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddMeetingScheduleProgram DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddMeetingScheduleProgram BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddMeetingScheduleProgram_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int? selectedProgramFK;
            string selectedScheduleType, leadershipCoachUsername;
            int currentSchedulePK, selectedYear;

            //Get the necessary values
            int.TryParse(hfAddEditMeetingSchedulePK.Value, out currentSchedulePK);
            leadershipCoachUsername = (string.IsNullOrWhiteSpace(hfAddEditMeetingScheduleLeadershipCoachUsername.Value) ? User.Identity.Name : hfAddEditMeetingScheduleLeadershipCoachUsername.Value);
            selectedScheduleType = (ddMeetingScheduleType.Value == null ? null : ddMeetingScheduleType.Value.ToString());

            //Check to see if the schedule type is selected
            if (string.IsNullOrWhiteSpace(selectedScheduleType) == false && selectedScheduleType.ToLower() == "program")
            {
                //Get the program FK
                selectedProgramFK = (ddMeetingScheduleProgram.Value == null ? (int?)null : Convert.ToInt32(ddMeetingScheduleProgram.Value));

                //Validate
                if (selectedProgramFK.HasValue == false)
                {
                    e.IsValid = false;
                    e.ErrorText = "Program is required!";
                }
                else
                {
                    //Check for duplication
                    if (deMeetingScheduleYear.Date != DateTime.MinValue)
                    {
                        //Get the selected year
                        selectedYear = deMeetingScheduleYear.Date.Year;

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Check to see if there are any schedules for this coach and year and program
                            List<ProgramLCMeetingSchedule> duplicateSchedules = context.ProgramLCMeetingSchedule.AsNoTracking()
                                            .Where(s => s.ProgramFK == selectedProgramFK.Value &&
                                                    s.MeetingYear == selectedYear &&
                                                    s.ProgramLCMeetingSchedulePK != currentSchedulePK &&
                                                    s.LeadershipCoachUsername == leadershipCoachUsername).ToList();

                            //Check the count of duplicate schedules
                            if(duplicateSchedules.Count > 0)
                            {
                                e.IsValid = false;
                                e.ErrorText = "A meeting schedule already exists for this combination of leadership coach, program, and year!";
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtMeetingScheduleTotalMeetings DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtMeetingScheduleTotalMeetings BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtMeetingScheduleTotalMeetings_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //To hold the necessary values
            int totalMeetings;

            //Validate
            if(int.TryParse(txtMeetingScheduleTotalMeetings.Text, out totalMeetings))
            {
                if(totalMeetings < 0 || totalMeetings > 999)
                {
                    e.IsValid = false;
                    e.ErrorText = "Total Meetings must be over zero and below 999!";
                }
            }
            else
            {
                e.IsValid = false;
                e.ErrorText = "Total Meetings is required and must be a valid whole number!";
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetMeetingScheduleControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddMeetingScheduleType.ClientEnabled = enabled;
            ddMeetingScheduleProgram.ClientEnabled = enabled;
            ddMeetingScheduleHub.ClientEnabled = enabled;
            deMeetingScheduleYear.ClientEnabled = enabled;
            txtMeetingScheduleTotalMeetings.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInJan.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInFeb.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInMar.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInApr.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInMay.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInJun.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInJul.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInAug.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInSep.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInOct.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInNov.ClientEnabled = enabled;
            chkMeetingScheduleMeetingInDec.ClientEnabled = enabled;

            //Show/hide the submit button
            submitMeetingSchedule.ShowSubmitButton  = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitMeetingSchedule.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit control's properties
            submitMeetingSchedule.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the Meeting Schedules
        /// and it opens a div that allows the user to add a Meeting Schedule
        /// </summary>
        /// <param name="sender">The lbAddMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Try to get the user record
            PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            //Make sure the record exists
            if (leadershipCoachUser != null &&
                (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                    currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ||
                        currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH))
            {
                //Clear inputs in the Meeting Schedule div
                hfAddEditMeetingSchedulePK.Value = "0";
                hfAddEditMeetingScheduleType.Value = "";
                hfAddEditMeetingScheduleLeadershipCoachUsername.Value = "";
                ddMeetingScheduleType.Value = "";
                ddMeetingScheduleProgram.Value = "";
                ddMeetingScheduleHub.Value = "";
                deMeetingScheduleYear.Value = "";
                txtMeetingScheduleTotalMeetings.Value = "";
                chkMeetingScheduleMeetingInJan.Checked = false;
                chkMeetingScheduleMeetingInFeb.Checked = false;
                chkMeetingScheduleMeetingInMar.Checked = false;
                chkMeetingScheduleMeetingInApr.Checked = false;
                chkMeetingScheduleMeetingInMay.Checked = false;
                chkMeetingScheduleMeetingInJun.Checked = false;
                chkMeetingScheduleMeetingInJul.Checked = false;
                chkMeetingScheduleMeetingInAug.Checked = false;
                chkMeetingScheduleMeetingInSep.Checked = false;
                chkMeetingScheduleMeetingInOct.Checked = false;
                chkMeetingScheduleMeetingInNov.Checked = false;
                chkMeetingScheduleMeetingInDec.Checked = false;

                //Set the type input based on permission
                if (currentProgramSchedulePermissions.AllowedToAdd && currentHubSchedulePermissions.AllowedToAdd)
                {
                    //Allow both items in the type drop-down
                    ddMeetingScheduleType.ReadOnly = false;
                }
                else if (currentHubSchedulePermissions.AllowedToAdd)
                {
                    //Only allow hub
                    ddMeetingScheduleType.Value = "Hub";
                    ddMeetingScheduleType.ReadOnly = true;
                }
                else
                {
                    //Only allow program
                    ddMeetingScheduleType.Value = "Program";
                    ddMeetingScheduleType.ReadOnly = true;
                }

                //Set the title
                lblAddEditMeetingSchedule.Text = "Add Meeting Schedule";

                //Set the username label
                lblMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);

                //Show the Meeting Schedule div
                divAddEditMeetingSchedule.Visible = true;

                //Set focus to the first field
                ddMeetingScheduleType.Focus();

                //Enable the controls
                SetMeetingScheduleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized because you are not logged in as a Leadership Coach!", 10000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a Meeting Schedule
        /// and it opens the Meeting Schedule edit div in view-only mode
        /// </summary>
        /// <param name="sender">The lbViewMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblMeetingSchedulePK = (Label)item.FindControl("lblMeetingSchedulePK");
            Label lblMeetingScheduleType = (Label)item.FindControl("lblMeetingScheduleType");

            //Get the PK from the label
            int? meetingSchedulePK = (string.IsNullOrWhiteSpace(lblMeetingSchedulePK.Text) ? (int?)null : Convert.ToInt32(lblMeetingSchedulePK.Text));

            //Get the schedule type from the label
            string meetingScheduleType = lblMeetingScheduleType.Text;

            if (meetingSchedulePK.HasValue && string.IsNullOrWhiteSpace(meetingScheduleType) == false)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    if (meetingScheduleType.ToLower() == "program")
                    {
                        //Program
                        //Get the schedule
                        ProgramLCMeetingSchedule currentSchedule = context.ProgramLCMeetingSchedule.AsNoTracking().Where(m => m.ProgramLCMeetingSchedulePK == meetingSchedulePK.Value).FirstOrDefault();

                        //Fill the add/edit div controls
                        hfAddEditMeetingSchedulePK.Value = currentSchedule.ProgramLCMeetingSchedulePK.ToString();
                        hfAddEditMeetingScheduleType.Value = meetingScheduleType;
                        hfAddEditMeetingScheduleLeadershipCoachUsername.Value = currentSchedule.LeadershipCoachUsername;
                        ddMeetingScheduleType.SelectedItem = ddMeetingScheduleType.Items.FindByValue(meetingScheduleType);
                        ddMeetingScheduleProgram.SelectedItem = ddMeetingScheduleProgram.Items.FindByValue(currentSchedule.ProgramFK);
                        deMeetingScheduleYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.MeetingYear));
                        txtMeetingScheduleTotalMeetings.Value = currentSchedule.TotalMeetings;
                        chkMeetingScheduleMeetingInJan.Checked = currentSchedule.MeetingInJan;
                        chkMeetingScheduleMeetingInFeb.Checked = currentSchedule.MeetingInFeb;
                        chkMeetingScheduleMeetingInMar.Checked = currentSchedule.MeetingInMar;
                        chkMeetingScheduleMeetingInApr.Checked = currentSchedule.MeetingInApr;
                        chkMeetingScheduleMeetingInMay.Checked = currentSchedule.MeetingInMay;
                        chkMeetingScheduleMeetingInJun.Checked = currentSchedule.MeetingInJun;
                        chkMeetingScheduleMeetingInJul.Checked = currentSchedule.MeetingInJul;
                        chkMeetingScheduleMeetingInAug.Checked = currentSchedule.MeetingInAug;
                        chkMeetingScheduleMeetingInSep.Checked = currentSchedule.MeetingInSep;
                        chkMeetingScheduleMeetingInOct.Checked = currentSchedule.MeetingInOct;
                        chkMeetingScheduleMeetingInNov.Checked = currentSchedule.MeetingInNov;
                        chkMeetingScheduleMeetingInDec.Checked = currentSchedule.MeetingInDec;

                        //Set the username label
                        PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == currentSchedule.LeadershipCoachUsername).FirstOrDefault();

                        if (leadershipCoachUser == null)
                        {
                            //Get the user record by username
                            //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                            leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentSchedule.LeadershipCoachUsername);
                        }

                        lblMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);
                    }
                    else
                    {
                        //Hub
                        //Get the schedule
                        HubLCMeetingSchedule currentSchedule = context.HubLCMeetingSchedule.AsNoTracking().Where(m => m.HubLCMeetingSchedulePK == meetingSchedulePK.Value).FirstOrDefault();

                        //Fill the add/edit div controls
                        hfAddEditMeetingSchedulePK.Value = currentSchedule.HubLCMeetingSchedulePK.ToString();
                        hfAddEditMeetingScheduleType.Value = meetingScheduleType;
                        hfAddEditMeetingScheduleLeadershipCoachUsername.Value = currentSchedule.LeadershipCoachUsername;
                        ddMeetingScheduleType.SelectedItem = ddMeetingScheduleType.Items.FindByValue(meetingScheduleType);
                        ddMeetingScheduleHub.SelectedItem = ddMeetingScheduleHub.Items.FindByValue(currentSchedule.HubFK);
                        deMeetingScheduleYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.MeetingYear));
                        txtMeetingScheduleTotalMeetings.Value = currentSchedule.TotalMeetings;
                        chkMeetingScheduleMeetingInJan.Checked = currentSchedule.MeetingInJan;
                        chkMeetingScheduleMeetingInFeb.Checked = currentSchedule.MeetingInFeb;
                        chkMeetingScheduleMeetingInMar.Checked = currentSchedule.MeetingInMar;
                        chkMeetingScheduleMeetingInApr.Checked = currentSchedule.MeetingInApr;
                        chkMeetingScheduleMeetingInMay.Checked = currentSchedule.MeetingInMay;
                        chkMeetingScheduleMeetingInJun.Checked = currentSchedule.MeetingInJun;
                        chkMeetingScheduleMeetingInJul.Checked = currentSchedule.MeetingInJul;
                        chkMeetingScheduleMeetingInAug.Checked = currentSchedule.MeetingInAug;
                        chkMeetingScheduleMeetingInSep.Checked = currentSchedule.MeetingInSep;
                        chkMeetingScheduleMeetingInOct.Checked = currentSchedule.MeetingInOct;
                        chkMeetingScheduleMeetingInNov.Checked = currentSchedule.MeetingInNov;
                        chkMeetingScheduleMeetingInDec.Checked = currentSchedule.MeetingInDec;

                        //Set the username label
                        PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == currentSchedule.LeadershipCoachUsername).FirstOrDefault();

                        if (leadershipCoachUser == null)
                        {
                            //Get the user record by username
                            //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                            leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentSchedule.LeadershipCoachUsername);
                        }

                        lblMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);
                    }
                }

                //Set the title
                lblAddEditMeetingSchedule.Text = "View Meeting Schedule";

                //Show the div
                divAddEditMeetingSchedule.Visible = true;

                //Set focus to the first field
                ddMeetingScheduleType.Focus();

                //Disable the controls
                SetMeetingScheduleControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected schedule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a Meeting Schedule
        /// and it opens the Meeting Schedule edit div so that the user can edit the selected Meeting Schedule
        /// </summary>
        /// <param name="sender">The lbEditMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the label with the PK for editing
            Label lblMeetingSchedulePK = (Label)item.FindControl("lblMeetingSchedulePK");
            Label lblMeetingScheduleType = (Label)item.FindControl("lblMeetingScheduleType");

            //Get the PK from the label
            int? meetingSchedulePK = (string.IsNullOrWhiteSpace(lblMeetingSchedulePK.Text) ? (int?)null : Convert.ToInt32(lblMeetingSchedulePK.Text));

            //Get the schedule type from the label
            string meetingScheduleType = lblMeetingScheduleType.Text;

            if (meetingSchedulePK.HasValue && string.IsNullOrWhiteSpace(meetingScheduleType) == false)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    if(meetingScheduleType.ToLower() == "program")
                    {
                        //Program
                        //Get the schedule
                        ProgramLCMeetingSchedule currentSchedule = context.ProgramLCMeetingSchedule.AsNoTracking().Where(m => m.ProgramLCMeetingSchedulePK == meetingSchedulePK.Value).FirstOrDefault();

                        //Fill the add/edit div controls
                        hfAddEditMeetingSchedulePK.Value = currentSchedule.ProgramLCMeetingSchedulePK.ToString();
                        hfAddEditMeetingScheduleType.Value = meetingScheduleType;
                        hfAddEditMeetingScheduleLeadershipCoachUsername.Value = currentSchedule.LeadershipCoachUsername;
                        ddMeetingScheduleType.SelectedItem = ddMeetingScheduleType.Items.FindByValue(meetingScheduleType);
                        ddMeetingScheduleProgram.SelectedItem = ddMeetingScheduleProgram.Items.FindByValue(currentSchedule.ProgramFK);
                        deMeetingScheduleYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.MeetingYear));
                        txtMeetingScheduleTotalMeetings.Value = currentSchedule.TotalMeetings;
                        chkMeetingScheduleMeetingInJan.Checked = currentSchedule.MeetingInJan;
                        chkMeetingScheduleMeetingInFeb.Checked = currentSchedule.MeetingInFeb;
                        chkMeetingScheduleMeetingInMar.Checked = currentSchedule.MeetingInMar;
                        chkMeetingScheduleMeetingInApr.Checked = currentSchedule.MeetingInApr;
                        chkMeetingScheduleMeetingInMay.Checked = currentSchedule.MeetingInMay;
                        chkMeetingScheduleMeetingInJun.Checked = currentSchedule.MeetingInJun;
                        chkMeetingScheduleMeetingInJul.Checked = currentSchedule.MeetingInJul;
                        chkMeetingScheduleMeetingInAug.Checked = currentSchedule.MeetingInAug;
                        chkMeetingScheduleMeetingInSep.Checked = currentSchedule.MeetingInSep;
                        chkMeetingScheduleMeetingInOct.Checked = currentSchedule.MeetingInOct;
                        chkMeetingScheduleMeetingInNov.Checked = currentSchedule.MeetingInNov;
                        chkMeetingScheduleMeetingInDec.Checked = currentSchedule.MeetingInDec;

                        //Set the username label
                        PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == currentSchedule.LeadershipCoachUsername).FirstOrDefault();

                        if (leadershipCoachUser == null)
                        {
                            //Get the user record by username
                            //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                            leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentSchedule.LeadershipCoachUsername);
                        }

                        lblMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);
                    }
                    else
                    {
                        //Hub
                        //Get the schedule
                        HubLCMeetingSchedule currentSchedule = context.HubLCMeetingSchedule.AsNoTracking().Where(m => m.HubLCMeetingSchedulePK == meetingSchedulePK.Value).FirstOrDefault();

                        //Fill the add/edit div controls
                        hfAddEditMeetingSchedulePK.Value = currentSchedule.HubLCMeetingSchedulePK.ToString();
                        hfAddEditMeetingScheduleType.Value = meetingScheduleType;
                        hfAddEditMeetingScheduleLeadershipCoachUsername.Value = currentSchedule.LeadershipCoachUsername;
                        ddMeetingScheduleType.SelectedItem = ddMeetingScheduleType.Items.FindByValue(meetingScheduleType);
                        ddMeetingScheduleHub.SelectedItem = ddMeetingScheduleHub.Items.FindByValue(currentSchedule.HubFK);
                        deMeetingScheduleYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.MeetingYear));
                        txtMeetingScheduleTotalMeetings.Value = currentSchedule.TotalMeetings;
                        chkMeetingScheduleMeetingInJan.Checked = currentSchedule.MeetingInJan;
                        chkMeetingScheduleMeetingInFeb.Checked = currentSchedule.MeetingInFeb;
                        chkMeetingScheduleMeetingInMar.Checked = currentSchedule.MeetingInMar;
                        chkMeetingScheduleMeetingInApr.Checked = currentSchedule.MeetingInApr;
                        chkMeetingScheduleMeetingInMay.Checked = currentSchedule.MeetingInMay;
                        chkMeetingScheduleMeetingInJun.Checked = currentSchedule.MeetingInJun;
                        chkMeetingScheduleMeetingInJul.Checked = currentSchedule.MeetingInJul;
                        chkMeetingScheduleMeetingInAug.Checked = currentSchedule.MeetingInAug;
                        chkMeetingScheduleMeetingInSep.Checked = currentSchedule.MeetingInSep;
                        chkMeetingScheduleMeetingInOct.Checked = currentSchedule.MeetingInOct;
                        chkMeetingScheduleMeetingInNov.Checked = currentSchedule.MeetingInNov;
                        chkMeetingScheduleMeetingInDec.Checked = currentSchedule.MeetingInDec;

                        //Set the username label
                        PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == currentSchedule.LeadershipCoachUsername).FirstOrDefault();

                        if (leadershipCoachUser == null)
                        {
                            //Get the user record by username
                            //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                            leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentSchedule.LeadershipCoachUsername);
                        }

                        lblMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);
                    }
                }

                //Set the type input based on permission
                if (currentProgramSchedulePermissions.AllowedToAdd && currentHubSchedulePermissions.AllowedToAdd)
                {
                    //Allow both items in the type drop-down
                    ddMeetingScheduleType.ReadOnly = false;
                }
                else if (currentHubSchedulePermissions.AllowedToAdd)
                {
                    //Only allow hub
                    ddMeetingScheduleType.Value = "Hub";
                    ddMeetingScheduleType.ReadOnly = true;
                }
                else
                {
                    //Only allow program
                    ddMeetingScheduleType.Value = "Program";
                    ddMeetingScheduleType.ReadOnly = true;
                }

                //Set the title
                lblAddEditMeetingSchedule.Text = "Edit Meeting Schedule";

                //Show the div
                divAddEditMeetingSchedule.Visible = true;

                //Set focus to the first field
                ddMeetingScheduleType.Focus();

                //Enable the controls
                SetMeetingScheduleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected schedule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the Meeting Schedules
        /// and it closes the Meeting Schedule add/edit div
        /// </summary>
        /// <param name="sender">The submitMeetingSchedule user control</param>
        /// <param name="e">The Click event</param>
        protected void submitMeetingSchedule_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditMeetingSchedulePK.Value = "0";
            hfAddEditMeetingScheduleType.Value = "";
            hfAddEditMeetingScheduleLeadershipCoachUsername.Value = "";
            divAddEditMeetingSchedule.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitMeetingSchedule control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitMeetingSchedule_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the meeting schedules
        /// and it saves the schedule information to the database
        /// </summary>
        /// <param name="sender">The btnSaveMeetingSchedule DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Get the meeting schedule type
            string meetingScheduleType = ddMeetingScheduleType.Value.ToString();
            bool isProgramMeetingSchedule = (meetingScheduleType.ToLower() == "program" ? true : false);
            bool isHubMeetingSchedule = (meetingScheduleType.ToLower() == "hub" ? true : false);

            //Check the permissions
            if ((isProgramMeetingSchedule && currentProgramSchedulePermissions.AllowedToEdit) ||
                (isHubMeetingSchedule && currentHubSchedulePermissions.AllowedToEdit))
            {
                //Get the Meeting Schedule pk
                int meetingSchedulePK = Convert.ToInt32(hfAddEditMeetingSchedulePK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //Check to see if this is an add or an edit
                    if (meetingSchedulePK == 0)
                    {
                        //Add
                        if(isProgramMeetingSchedule)
                        {
                            //Fill the object
                            ProgramLCMeetingSchedule currentSchedule = new ProgramLCMeetingSchedule();
                            currentSchedule.Creator = User.Identity.Name;
                            currentSchedule.CreateDate = DateTime.Now;
                            currentSchedule.MeetingYear = deMeetingScheduleYear.Date.Year;
                            currentSchedule.TotalMeetings = Convert.ToInt32(txtMeetingScheduleTotalMeetings.Value);
                            currentSchedule.MeetingInJan = chkMeetingScheduleMeetingInJan.Checked;
                            currentSchedule.MeetingInFeb = chkMeetingScheduleMeetingInFeb.Checked;
                            currentSchedule.MeetingInMar = chkMeetingScheduleMeetingInMar.Checked;
                            currentSchedule.MeetingInApr = chkMeetingScheduleMeetingInApr.Checked;
                            currentSchedule.MeetingInMay = chkMeetingScheduleMeetingInMay.Checked;
                            currentSchedule.MeetingInJun = chkMeetingScheduleMeetingInJun.Checked;
                            currentSchedule.MeetingInJul = chkMeetingScheduleMeetingInJul.Checked;
                            currentSchedule.MeetingInAug = chkMeetingScheduleMeetingInAug.Checked;
                            currentSchedule.MeetingInSep = chkMeetingScheduleMeetingInSep.Checked;
                            currentSchedule.MeetingInOct = chkMeetingScheduleMeetingInOct.Checked;
                            currentSchedule.MeetingInNov = chkMeetingScheduleMeetingInNov.Checked;
                            currentSchedule.MeetingInDec = chkMeetingScheduleMeetingInDec.Checked;
                            currentSchedule.LeadershipCoachUsername = User.Identity.Name;
                            currentSchedule.ProgramFK = Convert.ToInt32(ddMeetingScheduleProgram.Value);

                            //Add to the database
                            context.ProgramLCMeetingSchedule.Add(currentSchedule);
                        }
                        else if (isHubMeetingSchedule)
                        {
                            //Fill the object
                            HubLCMeetingSchedule currentSchedule = new HubLCMeetingSchedule();
                            currentSchedule.Creator = User.Identity.Name;
                            currentSchedule.CreateDate = DateTime.Now;
                            currentSchedule.MeetingYear = deMeetingScheduleYear.Date.Year;
                            currentSchedule.TotalMeetings = Convert.ToInt32(txtMeetingScheduleTotalMeetings.Value);
                            currentSchedule.MeetingInJan = chkMeetingScheduleMeetingInJan.Checked;
                            currentSchedule.MeetingInFeb = chkMeetingScheduleMeetingInFeb.Checked;
                            currentSchedule.MeetingInMar = chkMeetingScheduleMeetingInMar.Checked;
                            currentSchedule.MeetingInApr = chkMeetingScheduleMeetingInApr.Checked;
                            currentSchedule.MeetingInMay = chkMeetingScheduleMeetingInMay.Checked;
                            currentSchedule.MeetingInJun = chkMeetingScheduleMeetingInJun.Checked;
                            currentSchedule.MeetingInJul = chkMeetingScheduleMeetingInJul.Checked;
                            currentSchedule.MeetingInAug = chkMeetingScheduleMeetingInAug.Checked;
                            currentSchedule.MeetingInSep = chkMeetingScheduleMeetingInSep.Checked;
                            currentSchedule.MeetingInOct = chkMeetingScheduleMeetingInOct.Checked;
                            currentSchedule.MeetingInNov = chkMeetingScheduleMeetingInNov.Checked;
                            currentSchedule.MeetingInDec = chkMeetingScheduleMeetingInDec.Checked;
                            currentSchedule.LeadershipCoachUsername = User.Identity.Name;
                            currentSchedule.HubFK = Convert.ToInt32(ddMeetingScheduleHub.Value);

                            //Add to the database
                            context.HubLCMeetingSchedule.Add(currentSchedule);
                        }

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added the meeting schedule!", 10000);
                    }
                    else
                    {
                        //Edit
                        if (isProgramMeetingSchedule)
                        {
                            //Fill the object
                            ProgramLCMeetingSchedule currentSchedule = context.ProgramLCMeetingSchedule.Find(meetingSchedulePK);
                            currentSchedule.Editor = User.Identity.Name;
                            currentSchedule.EditDate = DateTime.Now;
                            currentSchedule.MeetingYear = deMeetingScheduleYear.Date.Year;
                            currentSchedule.TotalMeetings = Convert.ToInt32(txtMeetingScheduleTotalMeetings.Value);
                            currentSchedule.MeetingInJan = chkMeetingScheduleMeetingInJan.Checked;
                            currentSchedule.MeetingInFeb = chkMeetingScheduleMeetingInFeb.Checked;
                            currentSchedule.MeetingInMar = chkMeetingScheduleMeetingInMar.Checked;
                            currentSchedule.MeetingInApr = chkMeetingScheduleMeetingInApr.Checked;
                            currentSchedule.MeetingInMay = chkMeetingScheduleMeetingInMay.Checked;
                            currentSchedule.MeetingInJun = chkMeetingScheduleMeetingInJun.Checked;
                            currentSchedule.MeetingInJul = chkMeetingScheduleMeetingInJul.Checked;
                            currentSchedule.MeetingInAug = chkMeetingScheduleMeetingInAug.Checked;
                            currentSchedule.MeetingInSep = chkMeetingScheduleMeetingInSep.Checked;
                            currentSchedule.MeetingInOct = chkMeetingScheduleMeetingInOct.Checked;
                            currentSchedule.MeetingInNov = chkMeetingScheduleMeetingInNov.Checked;
                            currentSchedule.MeetingInDec = chkMeetingScheduleMeetingInDec.Checked;
                            currentSchedule.ProgramFK = Convert.ToInt32(ddMeetingScheduleProgram.Value);
                        }
                        else if (isHubMeetingSchedule)
                        {
                            //Fill the object
                            HubLCMeetingSchedule currentSchedule = context.HubLCMeetingSchedule.Find(meetingSchedulePK);
                            currentSchedule.Editor = User.Identity.Name;
                            currentSchedule.EditDate = DateTime.Now;
                            currentSchedule.MeetingYear = deMeetingScheduleYear.Date.Year;
                            currentSchedule.TotalMeetings = Convert.ToInt32(txtMeetingScheduleTotalMeetings.Value);
                            currentSchedule.MeetingInJan = chkMeetingScheduleMeetingInJan.Checked;
                            currentSchedule.MeetingInFeb = chkMeetingScheduleMeetingInFeb.Checked;
                            currentSchedule.MeetingInMar = chkMeetingScheduleMeetingInMar.Checked;
                            currentSchedule.MeetingInApr = chkMeetingScheduleMeetingInApr.Checked;
                            currentSchedule.MeetingInMay = chkMeetingScheduleMeetingInMay.Checked;
                            currentSchedule.MeetingInJun = chkMeetingScheduleMeetingInJun.Checked;
                            currentSchedule.MeetingInJul = chkMeetingScheduleMeetingInJul.Checked;
                            currentSchedule.MeetingInAug = chkMeetingScheduleMeetingInAug.Checked;
                            currentSchedule.MeetingInSep = chkMeetingScheduleMeetingInSep.Checked;
                            currentSchedule.MeetingInOct = chkMeetingScheduleMeetingInOct.Checked;
                            currentSchedule.MeetingInNov = chkMeetingScheduleMeetingInNov.Checked;
                            currentSchedule.MeetingInDec = chkMeetingScheduleMeetingInDec.Checked;
                            currentSchedule.HubFK = Convert.ToInt32(ddMeetingScheduleHub.Value);
                        }

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited the meeting schedule!", 10000);
                    }
                }

                //Reset the values in the hidden field and hide the div
                hfAddEditMeetingSchedulePK.Value = "0";
                hfAddEditMeetingScheduleType.Value = "";
                hfAddEditMeetingScheduleLeadershipCoachUsername.Value = "";
                divAddEditMeetingSchedule.Visible = false;

                //Rebind the MeetingSchedule gridview
                bsGRMeetingSchedules.DataBind();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a meeting schedule
        /// and it deletes the schedule information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Get the meeting schedule type
            string meetingScheduleType = hfDeleteMeetingScheduleType.Value;
            bool isProgramMeetingSchedule = (meetingScheduleType.ToLower() == "program" ? true : false);
            bool isHubMeetingSchedule = (meetingScheduleType.ToLower() == "hub" ? true : false);

            //Check the permissions
            if ((isProgramMeetingSchedule && currentProgramSchedulePermissions.AllowedToDelete) ||
                (isHubMeetingSchedule && currentHubSchedulePermissions.AllowedToDelete))
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteMeetingSchedulePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteMeetingSchedulePK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            if(isProgramMeetingSchedule)
                            {
                                //Get the schedule to remove
                                ProgramLCMeetingSchedule scheduleToRemove = context.ProgramLCMeetingSchedule.Where(m => m.ProgramLCMeetingSchedulePK == rowToRemovePK).FirstOrDefault();

                                //Remove the status
                                context.ProgramLCMeetingSchedule.Remove(scheduleToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.ProgramLCMeetingScheduleChanged
                                        .OrderByDescending(c => c.ProgramLCMeetingSchedulePK)
                                        .Where(c => c.ProgramLCMeetingSchedulePK == scheduleToRemove.ProgramLCMeetingSchedulePK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                            }
                            else if (isHubMeetingSchedule)
                            {
                                //Get the schedule to remove
                                HubLCMeetingSchedule scheduleToRemove = context.HubLCMeetingSchedule.Where(m => m.HubLCMeetingSchedulePK == rowToRemovePK).FirstOrDefault();

                                //Remove the status
                                context.HubLCMeetingSchedule.Remove(scheduleToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.HubLCMeetingScheduleChanged
                                        .OrderByDescending(c => c.HubLCMeetingSchedulePK)
                                        .Where(c => c.HubLCMeetingSchedulePK == scheduleToRemove.HubLCMeetingSchedulePK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();
                            }
                        }

                        //Rebind the meeting schedule gridview
                        bsGRMeetingSchedules.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the meeting schedule!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the meeting schedule, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting schedule!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting schedule!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the meeting schedule to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region Meeting Debriefs

        protected void efMeetingDebriefDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CustomViewPK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            e.QueryableSource = context.LCMeetingDebriefView
                                        .AsNoTracking()
                                        .Where(s => (currentProgramDebriefPermissions.AllowedToViewDashboard && s.ProgramFK.HasValue && currentProgramRole.ProgramFKs.Contains(s.ProgramFK.Value)) ||
                                                         (currentHubDebriefPermissions.AllowedToViewDashboard && s.HubFK.HasValue && currentProgramRole.HubFKs.Contains(s.HubFK.Value)));
        }

        protected void bsGRMeetingDebriefs_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the controls from the row
                Label lblLeadershipCoachUsername = (Label)bsGRMeetingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingDebriefs.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");
                HtmlAnchor lnkViewMeetingDebrief = (HtmlAnchor)bsGRMeetingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingDebriefs.Columns["ActionColumn"], "lnkViewMeetingDebrief");
                HtmlAnchor lnkEditMeetingDebrief = (HtmlAnchor)bsGRMeetingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingDebriefs.Columns["ActionColumn"], "lnkEditMeetingDebrief");
                HtmlButton btnDeleteMeetingDebrief = (HtmlButton)bsGRMeetingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingDebriefs.Columns["ActionColumn"], "btnDeleteMeetingDebrief");
                Label lblMeetingDebriefType = (Label)bsGRMeetingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRMeetingDebriefs.Columns["ActionColumn"], "lblMeetingDebriefType");

                //Get the necessary values
                string currentUsername = (e.GetValue("LeadershipCoachUsername") == null ? null : Convert.ToString(e.GetValue("LeadershipCoachUsername")));
                string scheduleType = (e.GetValue("MeetingDebriefType") == null ? null : Convert.ToString(e.GetValue("MeetingDebriefType")).ToLower());

                //Set the display of the buttons based on type of schedule and permissions
                if (scheduleType == "program")
                {
                    if (lnkViewMeetingDebrief != null)
                        lnkViewMeetingDebrief.Visible = currentProgramDebriefPermissions.AllowedToView;

                    if (lnkEditMeetingDebrief != null)
                        lnkEditMeetingDebrief.Visible = currentProgramDebriefPermissions.AllowedToEdit;

                    if (btnDeleteMeetingDebrief != null)
                        btnDeleteMeetingDebrief.Visible = currentProgramDebriefPermissions.AllowedToDelete;
                }
                else if (scheduleType == "hub")
                {
                    if (lnkViewMeetingDebrief != null)
                        lnkViewMeetingDebrief.Visible = currentHubDebriefPermissions.AllowedToView;

                    if (lnkEditMeetingDebrief != null)
                        lnkEditMeetingDebrief.Visible = currentHubDebriefPermissions.AllowedToEdit;

                    if (btnDeleteMeetingDebrief != null)
                        btnDeleteMeetingDebrief.Visible = currentHubDebriefPermissions.AllowedToDelete;
                }

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                    else
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        currentUser = PyramidUser.GetUserRecordByUsername(currentUsername);

                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a meeting debrief
        /// and it deletes the debrief information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteMeetingDebrief LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteMeetingDebrief_Click(object sender, EventArgs e)
        {
            //Get the meeting debrief type
            string meetingDebriefType = hfDeleteMeetingDebriefType.Value;
            bool isProgramMeetingDebrief = (meetingDebriefType.ToLower() == "program" ? true : false);
            bool isHubMeetingDebrief = (meetingDebriefType.ToLower() == "hub" ? true : false);

            //Check the permissions
            if ((isProgramMeetingDebrief && currentProgramDebriefPermissions.AllowedToDelete) ||
                (isHubMeetingDebrief && currentHubDebriefPermissions.AllowedToDelete))
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteMeetingDebriefPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteMeetingDebriefPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            if (isProgramMeetingDebrief)
                            {
                                //Get the items to remove
                                Models.ProgramLCMeetingDebrief debriefToRemove = context.ProgramLCMeetingDebrief.Where(m => m.ProgramLCMeetingDebriefPK == rowToRemovePK).FirstOrDefault();
                                List<int> sessionPKsToRemove = context.ProgramLCMeetingDebriefSession.Where(s => s.ProgramLCMeetingDebriefFK == rowToRemovePK).Select(s => s.ProgramLCMeetingDebriefSessionPK).ToList();
                                List<int> attendeePKsToRemove = context.ProgramLCMeetingDebriefSessionAttendee.Include(a => a.ProgramLCMeetingDebriefSession).Where(a => a.ProgramLCMeetingDebriefSession.ProgramLCMeetingDebriefFK == rowToRemovePK).Select(a => a.ProgramLCMeetingDebriefSessionAttendeePK).ToList();

                                //Remove the linked items (in order)
                                context.ProgramLCMeetingDebriefSessionAttendee.Where(a => attendeePKsToRemove.Contains(a.ProgramLCMeetingDebriefSessionAttendeePK)).Delete();
                                context.ProgramLCMeetingDebriefSession.Where(s => sessionPKsToRemove.Contains(s.ProgramLCMeetingDebriefSessionPK)).Delete();

                                //Remove the debrief
                                context.ProgramLCMeetingDebrief.Remove(debriefToRemove);

                                //Save the deletions to the database
                                context.SaveChanges();

                                //Update the change rows and set the deleter
                                context.ProgramLCMeetingDebriefSessionAttendeeChanged.Where(c => attendeePKsToRemove.Contains(c.ProgramLCMeetingDebriefSessionAttendeePK)).Update(c => new ProgramLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });
                                context.ProgramLCMeetingDebriefSessionChanged.Where(c => sessionPKsToRemove.Contains(c.ProgramLCMeetingDebriefSessionPK)).Update(c => new ProgramLCMeetingDebriefSessionChanged() { Deleter = User.Identity.Name });

                                //Get the delete change row and set the deleter
                                context.ProgramLCMeetingDebriefChanged.Where(c => c.ProgramLCMeetingDebriefPK == debriefToRemove.ProgramLCMeetingDebriefPK).Update(c => new ProgramLCMeetingDebriefChanged() { Deleter = User.Identity.Name });

                                //Save the delete change row to the database
                                context.SaveChanges();

                            }
                            else if (isHubMeetingDebrief)
                            {
                                //Get the items to remove
                                Models.HubLCMeetingDebrief debriefToRemove = context.HubLCMeetingDebrief.Where(m => m.HubLCMeetingDebriefPK == rowToRemovePK).FirstOrDefault();
                                List<int> sessionPKsToRemove = context.HubLCMeetingDebriefSession.Where(s => s.HubLCMeetingDebriefFK == rowToRemovePK).Select(s => s.HubLCMeetingDebriefSessionPK).ToList();
                                List<int> attendeePKsToRemove = context.HubLCMeetingDebriefSessionAttendee.Include(a => a.HubLCMeetingDebriefSession).Where(a => a.HubLCMeetingDebriefSession.HubLCMeetingDebriefFK == rowToRemovePK).Select(a => a.HubLCMeetingDebriefSessionAttendeePK).ToList();

                                //Remove the linked items (in order)
                                context.HubLCMeetingDebriefSessionAttendee.Where(a => attendeePKsToRemove.Contains(a.HubLCMeetingDebriefSessionAttendeePK)).Delete();
                                context.HubLCMeetingDebriefSession.Where(s => sessionPKsToRemove.Contains(s.HubLCMeetingDebriefSessionPK)).Delete();

                                //Remove the debrief
                                context.HubLCMeetingDebrief.Remove(debriefToRemove);

                                //Save the deletions to the database
                                context.SaveChanges();

                                //Update the change rows and set the deleter
                                context.HubLCMeetingDebriefSessionAttendeeChanged.Where(c => attendeePKsToRemove.Contains(c.HubLCMeetingDebriefSessionAttendeePK)).Update(c => new HubLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });
                                context.HubLCMeetingDebriefSessionChanged.Where(c => sessionPKsToRemove.Contains(c.HubLCMeetingDebriefSessionPK)).Update(c => new HubLCMeetingDebriefSessionChanged() { Deleter = User.Identity.Name });

                                //Get the delete change row and set the deleter
                                context.HubLCMeetingDebriefChanged.Where(c => c.HubLCMeetingDebriefPK == debriefToRemove.HubLCMeetingDebriefPK).Update(c => new HubLCMeetingDebriefChanged() { Deleter = User.Identity.Name });

                                //Save the delete change row to the database
                                context.SaveChanges();
                            }
                        }

                        //Rebind the meeting debrief gridview
                        bsGRMeetingDebriefs.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the meeting debrief!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the meeting debrief, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting debrief!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting debrief!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the meeting debrief to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region Coaching Circle/Community of Practice Meeting Schedules

        protected void efCCMeetingScheduleDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CoachingCircleLCMeetingSchedulePK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                    currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ||
                        currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH)
            {
                //Leadership coaches only see their forms for their state
                e.QueryableSource = context.CoachingCircleLCMeetingSchedule
                                            .Include(c => c.State)
                                            .AsNoTracking()
                                            .Where(s => s.LeadershipCoachUsername == User.Identity.Name &&
                                                        s.StateFK == currentProgramRole.CurrentStateFK.Value);
            }
            else
            {
                //Other users see all their state's forms
                e.QueryableSource = context.CoachingCircleLCMeetingSchedule
                                            .Include(c => c.State)
                                            .AsNoTracking()
                                            .Where(s => currentProgramRole.StateFKs.Contains(s.StateFK));
            }
        }

        protected void bsGRCCMeetingSchedules_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the leadership Coach
                Label lblLeadershipCoachUsername = (Label)bsGRCCMeetingSchedules.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRCCMeetingSchedules.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");

                //Get the current coach username
                string currentUsername = (e.GetValue("LeadershipCoachUsername") == null ? null : Convert.ToString(e.GetValue("LeadershipCoachUsername")));

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                    else
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        currentUser = PyramidUser.GetUserRecordByUsername(currentUsername);

                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtCCMeetingScheduleTotalMeetings DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtCCMeetingScheduleTotalMeetings BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtCCMeetingScheduleTotalMeetings_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //To hold the necessary values
            int totalMeetings;

            //Validate
            if (int.TryParse(txtCCMeetingScheduleTotalMeetings.Text, out totalMeetings))
            {
                if (totalMeetings < 0 || totalMeetings > 999)
                {
                    e.IsValid = false;
                    e.ErrorText = "Total Meetings must be over zero and below 999!";
                }
            }
            else
            {
                e.IsValid = false;
                e.ErrorText = "Total Meetings is required and must be a valid whole number!";
            }
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetCCMeetingScheduleControlUsability(bool enabled)
        {
            //Enable/disable the controls
            txtCCMeetingScheduleCCName.ClientEnabled = enabled;
            txtCCMeetingScheduleTargetAudience.ClientEnabled = enabled;
            deCCMeetingScheduleYear.ClientEnabled = enabled;
            txtCCMeetingScheduleTotalMeetings.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInJan.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInFeb.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInMar.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInApr.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInMay.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInJun.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInJul.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInAug.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInSep.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInOct.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInNov.ClientEnabled = enabled;
            chkCCMeetingScheduleMeetingInDec.ClientEnabled = enabled;

            //Show/hide the submit button
            submitCCMeetingSchedule.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitCCMeetingSchedule.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit control's properties
            submitCCMeetingSchedule.UpdateProperties();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the Meeting Schedules
        /// and it opens a div that allows the user to add a Meeting Schedule
        /// </summary>
        /// <param name="sender">The lbAddCCMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddCCMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Try to get the user record
            PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            //Make sure the record exists
            if (leadershipCoachUser != null &&
                (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                    currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ||
                        currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH))
            {
                //Clear inputs in the Meeting Schedule div
                hfAddEditCCMeetingSchedulePK.Value = "0";
                hfAddEditCCMeetingScheduleLeadershipCoachUsername.Value = "";
                txtCCMeetingScheduleCCName.Value = "";
                txtCCMeetingScheduleTargetAudience.Value = "";
                deCCMeetingScheduleYear.Value = "";
                txtCCMeetingScheduleTotalMeetings.Value = "";
                chkCCMeetingScheduleMeetingInJan.Checked = false;
                chkCCMeetingScheduleMeetingInFeb.Checked = false;
                chkCCMeetingScheduleMeetingInMar.Checked = false;
                chkCCMeetingScheduleMeetingInApr.Checked = false;
                chkCCMeetingScheduleMeetingInMay.Checked = false;
                chkCCMeetingScheduleMeetingInJun.Checked = false;
                chkCCMeetingScheduleMeetingInJul.Checked = false;
                chkCCMeetingScheduleMeetingInAug.Checked = false;
                chkCCMeetingScheduleMeetingInSep.Checked = false;
                chkCCMeetingScheduleMeetingInOct.Checked = false;
                chkCCMeetingScheduleMeetingInNov.Checked = false;
                chkCCMeetingScheduleMeetingInDec.Checked = false;

                //Set the username label
                lblCCMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);

                //Set the title
                lblAddEditCCMeetingSchedule.Text = "Add Meeting Schedule";

                //Show the Meeting Schedule div
                divAddEditCCMeetingSchedule.Visible = true;

                //Set focus to the first field
                txtCCMeetingScheduleCCName.Focus();

                //Enable the controls
                SetCCMeetingScheduleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized because you are not logged in as a Leadership Coach!", 10000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a CC Meeting Schedule
        /// and it opens the CC Meeting Schedule div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewCCMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewCCMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblCCMeetingSchedulePK = (Label)item.FindControl("lblCCMeetingSchedulePK");

            //Get the PK from the label
            int? CCMeetingSchedulePK = (string.IsNullOrWhiteSpace(lblCCMeetingSchedulePK.Text) ? (int?)null : Convert.ToInt32(lblCCMeetingSchedulePK.Text));

            if (CCMeetingSchedulePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedule
                    CoachingCircleLCMeetingSchedule currentSchedule = context.CoachingCircleLCMeetingSchedule.AsNoTracking().Where(s => s.CoachingCircleLCMeetingSchedulePK == CCMeetingSchedulePK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditCCMeetingSchedulePK.Value = currentSchedule.CoachingCircleLCMeetingSchedulePK.ToString();
                    hfAddEditCCMeetingScheduleLeadershipCoachUsername.Value = currentSchedule.LeadershipCoachUsername;
                    txtCCMeetingScheduleCCName.Value = currentSchedule.CoachingCircleName;
                    txtCCMeetingScheduleTargetAudience.Value = currentSchedule.TargetAudience;
                    deCCMeetingScheduleYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.MeetingYear));
                    txtCCMeetingScheduleTotalMeetings.Value = currentSchedule.TotalMeetings;
                    chkCCMeetingScheduleMeetingInJan.Checked = currentSchedule.MeetingInJan;
                    chkCCMeetingScheduleMeetingInFeb.Checked = currentSchedule.MeetingInFeb;
                    chkCCMeetingScheduleMeetingInMar.Checked = currentSchedule.MeetingInMar;
                    chkCCMeetingScheduleMeetingInApr.Checked = currentSchedule.MeetingInApr;
                    chkCCMeetingScheduleMeetingInMay.Checked = currentSchedule.MeetingInMay;
                    chkCCMeetingScheduleMeetingInJun.Checked = currentSchedule.MeetingInJun;
                    chkCCMeetingScheduleMeetingInJul.Checked = currentSchedule.MeetingInJul;
                    chkCCMeetingScheduleMeetingInAug.Checked = currentSchedule.MeetingInAug;
                    chkCCMeetingScheduleMeetingInSep.Checked = currentSchedule.MeetingInSep;
                    chkCCMeetingScheduleMeetingInOct.Checked = currentSchedule.MeetingInOct;
                    chkCCMeetingScheduleMeetingInNov.Checked = currentSchedule.MeetingInNov;
                    chkCCMeetingScheduleMeetingInDec.Checked = currentSchedule.MeetingInDec;

                    //Set the username label
                    PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == currentSchedule.LeadershipCoachUsername).FirstOrDefault();

                    if (leadershipCoachUser == null)
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentSchedule.LeadershipCoachUsername);
                    }

                    lblCCMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);
                }

                //Set the title
                lblAddEditCCMeetingSchedule.Text = "View Meeting Schedule";

                //Show the div
                divAddEditCCMeetingSchedule.Visible = true;

                //Set focus to the first field
                txtCCMeetingScheduleCCName.Focus();

                //Disable the controls
                SetCCMeetingScheduleControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected schedule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a Meeting Schedule
        /// and it opens the Meeting Schedule edit div so that the user can edit the selected Meeting Schedule
        /// </summary>
        /// <param name="sender">The lbEditCCMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditCCMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the label with the PK for editing
            Label lblCCMeetingSchedulePK = (Label)item.FindControl("lblCCMeetingSchedulePK");

            //Get the PK from the label
            int? CCMeetingSchedulePK = (string.IsNullOrWhiteSpace(lblCCMeetingSchedulePK.Text) ? (int?)null : Convert.ToInt32(lblCCMeetingSchedulePK.Text));

            if (CCMeetingSchedulePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedule
                    CoachingCircleLCMeetingSchedule currentSchedule = context.CoachingCircleLCMeetingSchedule.AsNoTracking().Where(s => s.CoachingCircleLCMeetingSchedulePK == CCMeetingSchedulePK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditCCMeetingSchedulePK.Value = currentSchedule.CoachingCircleLCMeetingSchedulePK.ToString();
                    hfAddEditCCMeetingScheduleLeadershipCoachUsername.Value = currentSchedule.LeadershipCoachUsername;
                    txtCCMeetingScheduleCCName.Value = currentSchedule.CoachingCircleName;
                    txtCCMeetingScheduleTargetAudience.Value = currentSchedule.TargetAudience;
                    deCCMeetingScheduleYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.MeetingYear));
                    txtCCMeetingScheduleTotalMeetings.Value = currentSchedule.TotalMeetings;
                    chkCCMeetingScheduleMeetingInJan.Checked = currentSchedule.MeetingInJan;
                    chkCCMeetingScheduleMeetingInFeb.Checked = currentSchedule.MeetingInFeb;
                    chkCCMeetingScheduleMeetingInMar.Checked = currentSchedule.MeetingInMar;
                    chkCCMeetingScheduleMeetingInApr.Checked = currentSchedule.MeetingInApr;
                    chkCCMeetingScheduleMeetingInMay.Checked = currentSchedule.MeetingInMay;
                    chkCCMeetingScheduleMeetingInJun.Checked = currentSchedule.MeetingInJun;
                    chkCCMeetingScheduleMeetingInJul.Checked = currentSchedule.MeetingInJul;
                    chkCCMeetingScheduleMeetingInAug.Checked = currentSchedule.MeetingInAug;
                    chkCCMeetingScheduleMeetingInSep.Checked = currentSchedule.MeetingInSep;
                    chkCCMeetingScheduleMeetingInOct.Checked = currentSchedule.MeetingInOct;
                    chkCCMeetingScheduleMeetingInNov.Checked = currentSchedule.MeetingInNov;
                    chkCCMeetingScheduleMeetingInDec.Checked = currentSchedule.MeetingInDec;

                    //Set the username label
                    PyramidUser leadershipCoachUser = usersForDashboard.Where(u => u.UserName == currentSchedule.LeadershipCoachUsername).FirstOrDefault();

                    if (leadershipCoachUser == null)
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        leadershipCoachUser = PyramidUser.GetUserRecordByUsername(currentSchedule.LeadershipCoachUsername);
                    }

                    lblCCMeetingScheduleLCUsername.Text = string.Format("{0} {1} ({2})", leadershipCoachUser.FirstName, leadershipCoachUser.LastName, leadershipCoachUser.UserName);
                }

                //Set the title
                lblAddEditCCMeetingSchedule.Text = "Edit Meeting Schedule";

                //Show the div
                divAddEditCCMeetingSchedule.Visible = true;

                //Set focus to the first field
                txtCCMeetingScheduleCCName.Focus();

                //Enable the controls
                SetCCMeetingScheduleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected schedule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the Meeting Schedules
        /// and it closes the Meeting Schedule add/edit div
        /// </summary>
        /// <param name="sender">The submitCCMeetingSchedule user control</param>
        /// <param name="e">The Click event</param>
        protected void submitCCMeetingSchedule_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditCCMeetingSchedulePK.Value = "0";
            hfAddEditCCMeetingScheduleLeadershipCoachUsername.Value = "";
            divAddEditCCMeetingSchedule.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCCMeetingSchedule control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCCMeetingSchedule_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the meeting schedules
        /// and it saves the schedule information to the database
        /// </summary>
        /// <param name="sender">The btnSaveCCMeetingSchedule DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitCCMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentCoachingCircleSchedulePermissions.AllowedToEdit)
            {
                //Get the Meeting Schedule pk
                int CCMeetingSchedulePK = Convert.ToInt32(hfAddEditCCMeetingSchedulePK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //The schedule object
                    CoachingCircleLCMeetingSchedule currentSchedule;

                    //Check to see if this is an add or an edit
                    if (CCMeetingSchedulePK == 0)
                    {
                        //Fill the object
                        currentSchedule = new CoachingCircleLCMeetingSchedule();
                        currentSchedule.Creator = User.Identity.Name;
                        currentSchedule.CreateDate = DateTime.Now;
                        currentSchedule.CoachingCircleName = txtCCMeetingScheduleCCName.Text;
                        currentSchedule.TargetAudience = txtCCMeetingScheduleTargetAudience.Text;
                        currentSchedule.MeetingYear = deCCMeetingScheduleYear.Date.Year;
                        currentSchedule.TotalMeetings = Convert.ToInt32(txtCCMeetingScheduleTotalMeetings.Value);
                        currentSchedule.MeetingInJan = chkCCMeetingScheduleMeetingInJan.Checked;
                        currentSchedule.MeetingInFeb = chkCCMeetingScheduleMeetingInFeb.Checked;
                        currentSchedule.MeetingInMar = chkCCMeetingScheduleMeetingInMar.Checked;
                        currentSchedule.MeetingInApr = chkCCMeetingScheduleMeetingInApr.Checked;
                        currentSchedule.MeetingInMay = chkCCMeetingScheduleMeetingInMay.Checked;
                        currentSchedule.MeetingInJun = chkCCMeetingScheduleMeetingInJun.Checked;
                        currentSchedule.MeetingInJul = chkCCMeetingScheduleMeetingInJul.Checked;
                        currentSchedule.MeetingInAug = chkCCMeetingScheduleMeetingInAug.Checked;
                        currentSchedule.MeetingInSep = chkCCMeetingScheduleMeetingInSep.Checked;
                        currentSchedule.MeetingInOct = chkCCMeetingScheduleMeetingInOct.Checked;
                        currentSchedule.MeetingInNov = chkCCMeetingScheduleMeetingInNov.Checked;
                        currentSchedule.MeetingInDec = chkCCMeetingScheduleMeetingInDec.Checked;
                        currentSchedule.LeadershipCoachUsername = User.Identity.Name;
                        currentSchedule.StateFK = currentProgramRole.CurrentStateFK.Value;

                        //Add to the database
                        context.CoachingCircleLCMeetingSchedule.Add(currentSchedule);

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added the meeting schedule!", 10000);
                    }
                    else
                    {
                        //Fill the object
                        currentSchedule = context.CoachingCircleLCMeetingSchedule.Find(CCMeetingSchedulePK);
                        currentSchedule.CoachingCircleName = txtCCMeetingScheduleCCName.Text;
                        currentSchedule.TargetAudience = txtCCMeetingScheduleTargetAudience.Text;
                        currentSchedule.MeetingYear = deCCMeetingScheduleYear.Date.Year;
                        currentSchedule.TotalMeetings = Convert.ToInt32(txtCCMeetingScheduleTotalMeetings.Value);
                        currentSchedule.MeetingInJan = chkCCMeetingScheduleMeetingInJan.Checked;
                        currentSchedule.MeetingInFeb = chkCCMeetingScheduleMeetingInFeb.Checked;
                        currentSchedule.MeetingInMar = chkCCMeetingScheduleMeetingInMar.Checked;
                        currentSchedule.MeetingInApr = chkCCMeetingScheduleMeetingInApr.Checked;
                        currentSchedule.MeetingInMay = chkCCMeetingScheduleMeetingInMay.Checked;
                        currentSchedule.MeetingInJun = chkCCMeetingScheduleMeetingInJun.Checked;
                        currentSchedule.MeetingInJul = chkCCMeetingScheduleMeetingInJul.Checked;
                        currentSchedule.MeetingInAug = chkCCMeetingScheduleMeetingInAug.Checked;
                        currentSchedule.MeetingInSep = chkCCMeetingScheduleMeetingInSep.Checked;
                        currentSchedule.MeetingInOct = chkCCMeetingScheduleMeetingInOct.Checked;
                        currentSchedule.MeetingInNov = chkCCMeetingScheduleMeetingInNov.Checked;
                        currentSchedule.MeetingInDec = chkCCMeetingScheduleMeetingInDec.Checked;
                        currentSchedule.Editor = User.Identity.Name;
                        currentSchedule.EditDate = DateTime.Now;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited the meeting schedule!", 10000);
                    }
                }

                //Reset the values in the hidden field and hide the div
                hfAddEditCCMeetingSchedulePK.Value = "0";
                hfAddEditCCMeetingScheduleLeadershipCoachUsername.Value = "";
                divAddEditCCMeetingSchedule.Visible = false;

                //Rebind the CCMeetingSchedule gridview
                bsGRCCMeetingSchedules.DataBind();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a meeting schedule
        /// and it deletes the schedule information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteCCMeetingSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteCCMeetingSchedule_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentCoachingCircleSchedulePermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteCCMeetingSchedulePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCCMeetingSchedulePK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the schedule to remove
                            CoachingCircleLCMeetingSchedule scheduleToRemove = context.CoachingCircleLCMeetingSchedule.Where(s => s.CoachingCircleLCMeetingSchedulePK == rowToRemovePK).FirstOrDefault();

                            //Remove the status
                            context.CoachingCircleLCMeetingSchedule.Remove(scheduleToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.CoachingCircleLCMeetingScheduleChanged
                                    .OrderByDescending(c => c.CoachingCircleLCMeetingSchedulePK)
                                    .Where(c => c.CoachingCircleLCMeetingSchedulePK == scheduleToRemove.CoachingCircleLCMeetingSchedulePK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();
                        }

                        //Rebind the meeting schedule gridview
                        bsGRCCMeetingSchedules.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the meeting schedule!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the meeting schedule, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting schedule!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the meeting schedule!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the meeting schedule to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region Coaching Circle/Community of Practice Debrief Forms

        protected void efCCMeetingDebriefDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "CoachingCircleLCMeetingDebriefPK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                    currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ||
                        currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH)
            {
                //Leadership coaches only see their forms for their state
                e.QueryableSource = context.CoachingCircleLCMeetingDebrief
                                            .Include(c => c.State)
                                            .AsNoTracking()
                                            .Where(c => c.LeadershipCoachUsername == User.Identity.Name &&
                                                        c.StateFK == currentProgramRole.CurrentStateFK.Value);
            }
            else
            {
                //Other users see all their state's forms
                e.QueryableSource = context.CoachingCircleLCMeetingDebrief
                                            .Include(c => c.State)
                                            .AsNoTracking()
                                            .Where(c => currentProgramRole.StateFKs.Contains(c.StateFK));
            }
        }

        protected void bsGRCCMeetingDebriefs_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the leadership Coach
                Label lblLeadershipCoachUsername = (Label)bsGRCCMeetingDebriefs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRCCMeetingDebriefs.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");

                //Get the current coach username
                string currentUsername = (e.GetValue("LeadershipCoachUsername") == null ? null : Convert.ToString(e.GetValue("LeadershipCoachUsername")));

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                    else
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        currentUser = PyramidUser.GetUserRecordByUsername(currentUsername);

                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a meeting debrief
        /// and it deletes the debrief information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteCCMeetingDebrief LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteCCMeetingDebrief_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentCoachingCircleDebriefPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteCCMeetingDebriefPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCCMeetingDebriefPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the items to remove
                            Models.CoachingCircleLCMeetingDebrief debriefToRemove = context.CoachingCircleLCMeetingDebrief.Where(m => m.CoachingCircleLCMeetingDebriefPK == rowToRemovePK).FirstOrDefault();
                            List<int> sessionPKsToRemove = context.CoachingCircleLCMeetingDebriefSession.Where(s => s.CoachingCircleLCMeetingDebriefFK == rowToRemovePK).Select(s => s.CoachingCircleLCMeetingDebriefSessionPK).ToList();
                            List<int> attendeePKsToRemove = context.CoachingCircleLCMeetingDebriefSessionAttendee.Include(a => a.CoachingCircleLCMeetingDebriefSession).Where(a => a.CoachingCircleLCMeetingDebriefSession.CoachingCircleLCMeetingDebriefFK == rowToRemovePK).Select(a => a.CoachingCircleLCMeetingDebriefSessionAttendeePK).ToList();
                            List<int> teamMemberPKsToRemove = context.CoachingCircleLCMeetingDebriefTeamMember.Where(t => t.CoachingCircleLCMeetingDebriefFK == rowToRemovePK).Select(t => t.CoachingCircleLCMeetingDebriefTeamMemberPK).ToList();

                            //Remove the linked items (in order)
                            context.CoachingCircleLCMeetingDebriefSessionAttendee.Where(a => attendeePKsToRemove.Contains(a.CoachingCircleLCMeetingDebriefSessionAttendeePK)).Delete();
                            context.CoachingCircleLCMeetingDebriefSession.Where(s => sessionPKsToRemove.Contains(s.CoachingCircleLCMeetingDebriefSessionPK)).Delete();
                            context.CoachingCircleLCMeetingDebriefTeamMember.Where(t => teamMemberPKsToRemove.Contains(t.CoachingCircleLCMeetingDebriefTeamMemberPK)).Delete();

                            //Remove the debrief
                            context.CoachingCircleLCMeetingDebrief.Remove(debriefToRemove);

                            //Save the deletions to the database
                            context.SaveChanges();

                            //Update the change rows and set the deleter
                            context.CoachingCircleLCMeetingDebriefSessionAttendeeChanged.Where(c => attendeePKsToRemove.Contains(c.CoachingCircleLCMeetingDebriefSessionAttendeePK)).Update(c => new CoachingCircleLCMeetingDebriefSessionAttendeeChanged() { Deleter = User.Identity.Name });
                            context.CoachingCircleLCMeetingDebriefSessionChanged.Where(c => sessionPKsToRemove.Contains(c.CoachingCircleLCMeetingDebriefSessionPK)).Update(c => new CoachingCircleLCMeetingDebriefSessionChanged() { Deleter = User.Identity.Name });
                            context.CoachingCircleLCMeetingDebriefTeamMemberChanged.Where(c => teamMemberPKsToRemove.Contains(c.CoachingCircleLCMeetingDebriefTeamMemberPK)).Update(c => new CoachingCircleLCMeetingDebriefTeamMemberChanged() { Deleter = User.Identity.Name });

                            //Get the delete change row and set the deleter
                            context.CoachingCircleLCMeetingDebriefChanged.Where(c => c.CoachingCircleLCMeetingDebriefPK == debriefToRemove.CoachingCircleLCMeetingDebriefPK).Update(c => new CoachingCircleLCMeetingDebriefChanged() { Deleter = User.Identity.Name });

                            //Save the delete change row to the database
                            context.SaveChanges();
                        }

                        //Rebind the meeting debrief gridview
                        bsGRCCMeetingDebriefs.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Coaching Circle/Community of Practice Meeting Debrief!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Coaching Circle/Community of Practice Meeting Debrief, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Coaching Circle/Community of Practice Meeting Debrief!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Coaching Circle/Community of Practice Meeting Debrief!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Coaching Circle/Community of Practice Meeting Debrief to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region Leadership Coach Logs

        protected void efLeadershipCoachLogDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "LeadershipCoachLogPK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
                e.QueryableSource = context.LeadershipCoachLog
                                            .Include(c => c.Program)
                                            .Include(c => c.Program.State)
                                            .AsNoTracking()
                                            .Where(c => currentProgramRole.ProgramFKs.Contains(c.ProgramFK));
        }

        protected void bsGRLeadershipCoachLogs_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the label for the leadership Coach
                Label lblLeadershipCoachUsername = (Label)bsGRLeadershipCoachLogs.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRLeadershipCoachLogs.Columns["LeadershipCoachColumn"], "lblLeadershipCoachUsername");

                //Get the current coach username
                string currentUsername = (e.GetValue("LeadershipCoachUsername") == null ? null : Convert.ToString(e.GetValue("LeadershipCoachUsername")));

                //Make sure the username exists
                if (string.IsNullOrWhiteSpace(currentUsername) == false)
                {
                    //Get the user record for the username
                    PyramidUser currentUser = usersForDashboard.Where(u => u.UserName == currentUsername).FirstOrDefault();

                    //Make sure the user record exists
                    if (currentUser != null)
                    {
                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                    else
                    {
                        //Get the user record by username
                        //This is necessary now that the leadership coach roles are program/hub-specific and can be removed
                        currentUser = PyramidUser.GetUserRecordByUsername(currentUsername);

                        //Set the label text (include the username for searching
                        lblLeadershipCoachUsername.Text = string.Format("{0} {1} ({2})", currentUser.FirstName, currentUser.LastName, currentUser.UserName);
                    }
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a coach log
        /// and it deletes the coach log information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteLeadershipCoachLog LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteLeadershipCoachLog_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (currentLeadershipCoachLogPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteLeadershipCoachLogPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteLeadershipCoachLogPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the items to remove
                            Models.LeadershipCoachLog coachLogToRemove = context.LeadershipCoachLog.Where(m => m.LeadershipCoachLogPK == rowToRemovePK).FirstOrDefault();
                            List<int> involvedCoachesToRemove = context.LCLInvolvedCoach.Where(lic => lic.LeadershipCoachLogFK == rowToRemovePK).Select(lic => lic.LCLInvolvedCoachPK).ToList();
                            List<int> responsesToRemove = context.LCLResponse.Where(s => s.LeadershipCoachLogFK == rowToRemovePK).Select(s => s.LCLResponsePK).ToList();
                            List<int> engagementRecordsToRemove = context.LCLTeamMemberEngagement.Where(a => a.LeadershipCoachLogFK == rowToRemovePK).Select(a => a.LCLTeamMemberEngagementPK).ToList();

                            //Remove the linked items (in order)
                            context.LCLInvolvedCoach.Where(lic => involvedCoachesToRemove.Contains(lic.LCLInvolvedCoachPK)).Delete();
                            context.LCLResponse.Where(s => responsesToRemove.Contains(s.LCLResponsePK)).Delete();
                            context.LCLTeamMemberEngagement.Where(a => engagementRecordsToRemove.Contains(a.LCLTeamMemberEngagementPK)).Delete();

                            //Remove the coach log
                            context.LeadershipCoachLog.Remove(coachLogToRemove);

                            //Save the deletions to the database
                            context.SaveChanges();

                            //Update the change rows and set the deleter
                            context.LCLInvolvedCoachChanged.Where(licc => involvedCoachesToRemove.Contains(licc.LCLInvolvedCoachPK)).Update(c => new LCLInvolvedCoachChanged() { Deleter = User.Identity.Name });
                            context.LCLResponseChanged.Where(c => responsesToRemove.Contains(c.LCLResponsePK)).Update(c => new LCLResponseChanged() { Deleter = User.Identity.Name });
                            context.LCLTeamMemberEngagementChanged.Where(c => engagementRecordsToRemove.Contains(c.LCLTeamMemberEngagementPK)).Update(c => new LCLTeamMemberEngagementChanged() { Deleter = User.Identity.Name });

                            //Get the delete change row and set the deleter
                            context.LeadershipCoachLogChanged.Where(c => c.LeadershipCoachLogPK == coachLogToRemove.LeadershipCoachLogPK).Update(c => new LeadershipCoachLogChanged() { Deleter = User.Identity.Name });

                            //Save the delete change row to the database
                            context.SaveChanges();
                        }

                        //Rebind the coach log gridview
                        bsGRLeadershipCoachLogs.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Leadership Coach Log!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Leadership Coach Log, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Leadership Coach Log!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Leadership Coach Log!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Leadership Coach Log to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion
    }
}