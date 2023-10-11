using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using Z.EntityFramework.Plus;

namespace Pyramid.Pages
{
    public partial class LeadershipCoachLog : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "LCL";
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
        private Models.LeadershipCoachLog currentLeadershipCoachLog;
        private List<PyramidUser> allLeadershipCoaches;
        private int currentLCLPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //To hold the action the user is performing on this page
            string action;

            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the pk from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["LeadershipCoachLogPK"]))
            {
                //Parse the pk
                int.TryParse(Request.QueryString["LeadershipCoachLogPK"], out currentLCLPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentLCLPK == 0 && !string.IsNullOrWhiteSpace(hfLeadershipCoachLogPK.Value))
            {
                int.TryParse(hfLeadershipCoachLogPK.Value, out currentLCLPK);
            }

            //Check to see if this is an edit
            isEdit = currentLCLPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                //Show a message after redirect
                msgSys.AddMessageToQueue("danger", "Not Authorized", "You are not authorized to view that information!", 10000);

                //Redirect to the dashboard
                Response.Redirect("/Pages/LeadershipCoachDashboard.aspx");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the leadership coach log object
                currentLeadershipCoachLog = context.LeadershipCoachLog
                                        .Include(lcl => lcl.LCLResponse)
                                        .Include(lcl => lcl.LCLResponse.Select(lr => lr.CodeLCLResponse))
                                        .Include(lcl => lcl.LCLTeamMemberEngagement)
                                        .Include(lcl => lcl.LCLInvolvedCoach)
                                        .AsNoTracking()
                                        .Where(lcl => lcl.LeadershipCoachLogPK == currentLCLPK).FirstOrDefault();

                //Check to see if the leadership coach log exists
                if (currentLeadershipCoachLog == null)
                {
                    //The leadership coach log doesn't exist, set it to a new object
                    currentLeadershipCoachLog = new Models.LeadershipCoachLog();
                }
            }

            //Prevent users from viewing action plans from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentLeadershipCoachLog.ProgramFK))
            {
                //Add a message that will show after redirect
                msgSys.AddMessageToQueue("danger", "Not Found", "The leadership coach log you are attempting to access does not exist.", 10000);

                //Redirect the user back to the dashboard
                Response.Redirect("/Pages/LeadershipCoachDashboard.aspx");
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Try to get the action type
                if (!string.IsNullOrWhiteSpace(Request.QueryString["Action"]))
                {
                    action = Request.QueryString["Action"].ToString();
                }
                else
                {
                    action = "View";
                }

                if (isEdit)
                {
                    //Get the leadership coaches for the leadership coach log's program
                    BindLeadershipCoachList(currentLeadershipCoachLog.ProgramFK);
                    BindLeadershipCoachDropDown(currentLeadershipCoachLog.ProgramFK, currentLeadershipCoachLog.LeadershipCoachUsername);

                    //Get the team members and coaches engaged
                    List<int> teamMemberPKs = currentLeadershipCoachLog.LCLTeamMemberEngagement.Select(tme => tme.PLTMemberFK).ToList();
                    List<int> coachPKs = currentLeadershipCoachLog.LCLInvolvedCoach.Select(lic => lic.ProgramEmployeeFK).ToList();

                    //Bind the team member and coach list boxes
                    BindTeamMemberListBox(currentLeadershipCoachLog.DateCompleted, currentLeadershipCoachLog.IsMonthly, currentLeadershipCoachLog.ProgramFK, teamMemberPKs);
                    BindInvolvedCoachListBox(currentLeadershipCoachLog.DateCompleted, currentLeadershipCoachLog.IsMonthly, currentLeadershipCoachLog.ProgramFK, coachPKs);

                    //Bind the BOQ info
                    SetMostRecentBOQInfo(currentLeadershipCoachLog.DateCompleted, currentLeadershipCoachLog.ProgramFK);
                }
                else
                {
                    //Get the leadership coaches for the user's current program
                    BindLeadershipCoachList(currentProgramRole.CurrentProgramFK.Value);
                    BindLeadershipCoachDropDown(currentProgramRole.CurrentProgramFK.Value, null);

                    //Bind the team member and coach list boxes to empty
                    BindTeamMemberListBox(null, null, null, new List<int>());
                    BindInvolvedCoachListBox(null, null, null, new List<int>());

                    //Bind the BOQ info
                    SetMostRecentBOQInfo(null, null);
                }

                //Bind the drop-downs
                BindInputControls();

                //Fill the form with data
                FillFormWithDataFromObject();

                //Set the other specify code labels
                lblDomain2SpecifyCode.Text = Models.CodeLCLResponse.OtherSpecifyPKs.DOMAIN_2_SPECIFY.ToString();
                lblSiteResourcesSpecifyCode.Text = Models.CodeLCLResponse.OtherSpecifyPKs.SITE_RESOURCES_SPECIFY.ToString();
                lblTopicsDiscussedSpecifyCode.Text = Models.CodeLCLResponse.OtherSpecifyPKs.TOPICS_DISCUSSED_SPECIFY.ToString();
                lblTrainingsCoveredSpecifyCode.Text = Models.CodeLCLResponse.OtherSpecifyPKs.TRAININGS_COVERED_SPECIFY.ToString();

                //Allow adding/editing depending on the user's role and the action
                if (isEdit == false && action.ToLower() == "add" && FormPermissions.AllowedToAdd)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Leadership Coach Log";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Leadership Coach Log";
                }
                else
                {
                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Leadership Coach Log";
                }

                //Set focus to the first field
                ddProgram.Focus();

                //Check for the printing item in the query string
                string strIsPrinting = Request.QueryString["Print"];

                //Check to see if printing
                if (!string.IsNullOrWhiteSpace(strIsPrinting))
                {
                    //To hold the printing value
                    bool isPrinting = false;

                    //Print the form if the query string value is true
                    if (bool.TryParse(strIsPrinting, out isPrinting) && isPrinting == true)
                    {
                        //Print the form
                        PrintForm();
                    }
                }
            }
            else
            {
                //Get the selected values
                int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);
                bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);
                DateTime? logDate = GetLogDateFromForm();
                List<int> teamMembersSelected = GetSelectedTeamMemberPKs();
                List<int> involvedCoachesSelected = GetSelectedCoachPKs();

                //Bind the list boxes
                BindTeamMemberListBox(logDate, isMonthly, selectedProgramFK, teamMembersSelected);
                BindInvolvedCoachListBox(logDate, isMonthly, selectedProgramFK, involvedCoachesSelected);

                //Get the most recent BOQ info
                SetMostRecentBOQInfo(logDate, selectedProgramFK);
            }
        }

        /// <summary>
        /// This method fills the input fields with data from the relevant object
        /// </summary>
        private void FillFormWithDataFromObject()
        {
            //Check if this is an edit or not
            if (isEdit)
            {
                //Fill the program dropdown
                ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentLeadershipCoachLog.ProgramFK);

                //Set the program labels
                SetProgramLabels(currentLeadershipCoachLog.ProgramFK);

                //Fill the other input fields
                ddLeadershipCoach.Value = currentLeadershipCoachLog.LeadershipCoachUsername;
                ddIsMonthly.SelectedItem = ddIsMonthly.Items.FindByValue(currentLeadershipCoachLog.IsMonthly);
                
                if (currentLeadershipCoachLog.IsMonthly.HasValue)
                {
                    if (currentLeadershipCoachLog.IsMonthly.Value)
                    {
                        deLCLMonth.Value = (currentLeadershipCoachLog.DateCompleted.HasValue ? currentLeadershipCoachLog.DateCompleted.Value : (DateTime?)null);
                        txtMonthlyNumEngagements.Value = currentLeadershipCoachLog.NumberOfEngagements;
                        txtMonthlyNumAttemptedEngagements.Value = currentLeadershipCoachLog.NumberOfAttemptedEngagements;
                    }
                    else
                    {
                        deDateCompleted.Value = (currentLeadershipCoachLog.DateCompleted.HasValue ? currentLeadershipCoachLog.DateCompleted.Value : (DateTime?)null);
                        txtTotalDurationHours.Value = currentLeadershipCoachLog.TotalDurationHours;
                        txtTotalDurationMinutes.Value = currentLeadershipCoachLog.TotalDurationMinutes;
                    }
                }

                ddCyclePhase.SelectedItem = ddCyclePhase.Items.FindByValue(currentLeadershipCoachLog.CyclePhase);
                ddTimelyProgression.SelectedItem = ddTimelyProgression.Items.FindByValue(currentLeadershipCoachLog.TimelyProgressionLikelihoodCodeFK);
                ddGoalCompletionLikelihood.SelectedItem = ddGoalCompletionLikelihood.Items.FindByValue(currentLeadershipCoachLog.GoalCompletionLikelihoodCodeFK);

                //Team member engagment is handled separately

                txtOtherEngagementSpecify.Text = currentLeadershipCoachLog.OtherEngagementSpecify;
                txtOtherDomainTwoSpecify.Text = currentLeadershipCoachLog.OtherDomainTwoSpecify;
                txtOtherSiteResourcesSpecify.Text = currentLeadershipCoachLog.OtherSiteResourcesSpecify;
                txtOtherTopicsDiscussedSpecify.Text = currentLeadershipCoachLog.OtherTopicsDiscussedSpecify;
                txtOtherTrainingsCoveredSpecify.Text = currentLeadershipCoachLog.OtherTrainingsCoveredSpecify;

                List<LCLResponse> responseItemList = currentLeadershipCoachLog.LCLResponse.ToList();
                SetListBoxSelectedItems(lbDomain1, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_1);
                SetListBoxSelectedItems(lbDomain2, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_2);
                SetListBoxSelectedItems(lbDomain3, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_3);
                SetListBoxSelectedItems(lbDomain4, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_4);
                SetListBoxSelectedItems(lbDomain5, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_5);
                SetListBoxSelectedItems(lbDomain6, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_6);

                SetListBoxSelectedItems(lbIdentifiedProgramBarriers, responseItemList, CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_BARRIERS);
                SetListBoxSelectedItems(lbIdentifiedProgramStrengths, responseItemList, CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_STRENGTHS);
                SetListBoxSelectedItems(lbSiteResources, responseItemList, CodeLCLResponse.ResponseGroups.SITE_RESOURCES);
                SetListBoxSelectedItems(lbTopicsDiscussed, responseItemList, CodeLCLResponse.ResponseGroups.TOPICS_DISCUSSED);
                SetListBoxSelectedItems(lbTrainingsCovered, responseItemList, CodeLCLResponse.ResponseGroups.TRAININGS_COVERED);

                txtTargetedTrainingHours.Value = (currentLeadershipCoachLog.TargetedTrainingHours.HasValue ? currentLeadershipCoachLog.TargetedTrainingHours.Value.ToString() : "");
                txtTargetedTrainingMinutes.Value = (currentLeadershipCoachLog.TargetedTrainingMinutes.HasValue ? currentLeadershipCoachLog.TargetedTrainingMinutes.Value.ToString() : "");
                txtThinkNarrative.Text = currentLeadershipCoachLog.ThinkNarrative;
                txtActNarrative.Text = currentLeadershipCoachLog.ActNarrative;
                txtHighlightsNarrative.Text = currentLeadershipCoachLog.HighlightsNarrative;

                chkIsComplete.Checked = currentLeadershipCoachLog.IsComplete;

                //Disable the checkbox and hide the section
                chkIsComplete.ClientReadOnly = currentLeadershipCoachLog.IsComplete;
                divIsComplete.Visible = (currentLeadershipCoachLog.IsComplete ? false : true);

                //Set the required fields based on whether the form is complete or not
                HandleIsComplete(currentLeadershipCoachLog.IsComplete);
            }
            else
            {
                //On add, set the program to the user's program
                ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentProgramRole.CurrentProgramFK.Value);

                //Set the program labels
                SetProgramLabels(currentProgramRole.CurrentProgramFK.Value);

                //Set the required fields to least restrictive
                HandleIsComplete(false);
            }
        }

        /// <summary>
        /// This method sets the selected listbox items for the passed listbox
        /// </summary>
        /// <param name="currentListBox">The listbox to set</param>
        /// <param name="currentResponseList">The responses</param>
        /// <param name="currentResponseGroup">The response group for the listbox</param>
        private void SetListBoxSelectedItems(BootstrapListBox currentListBox, List<LCLResponse> currentResponseList, string currentResponseGroup)
        {
            //Get the items in the group that should be selected
            List<LCLResponse> groupItemsSelected = currentResponseList.Where(r => r.CodeLCLResponse.Group == currentResponseGroup).ToList();

            //Only continue if there are items to select
            if (groupItemsSelected != null && groupItemsSelected.Count > 0)
            {
                //Select the items
                foreach (LCLResponse responseRow in groupItemsSelected)
                {
                    var listBoxItem = currentListBox.Items.FindByValue(responseRow.LCLResponseCodeFK);

                    if (listBoxItem != null)
                    {
                        listBoxItem.Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// This method sets the selected listbox items for the passed listbox
        /// </summary>
        /// <param name="currentListBox">The listbox to set</param>
        /// <param name="currentResponseList">The responses</param>
        /// <param name="currentResponseGroup">The response group for the listbox</param>
        /// <returns>A list of LCLResponse objects that represent the values selected in the list box</returns>
        private List<LCLResponse> GetResponseListFromListBox(BootstrapListBox currentListBox, List<LCLResponse> currentResponseList, string currentResponseGroup, int currentFormPK)
        {
            //Get the items in the group that should be selected
            List<LCLResponse> existingResponseItems = currentResponseList.Where(r => r.CodeLCLResponse.Group == currentResponseGroup).ToList();
            List<LCLResponse> validResponseItems = new List<LCLResponse>();

            //Only continue if there are items in the listbox that were selected
            if (currentListBox != null && currentListBox.SelectedItems.Count > 0)
            {
                //Loop through all the selected list box items
                foreach (ListEditItem item in currentListBox.SelectedItems)
                {
                    //Ensure that the item is valid
                    if (item != null && item.Value != null && int.TryParse(item.Value.ToString(), out int responseCodeFK))
                    {
                        //Check to see if the item already exists in the list of response items
                        LCLResponse existingRecord = existingResponseItems.Where(r => r.LCLResponseCodeFK == responseCodeFK).FirstOrDefault();

                        if (existingRecord == null)
                        {
                            existingRecord = new LCLResponse()
                            {
                                Creator = User.Identity.Name,
                                CreateDate = DateTime.Now,
                                LCLResponseCodeFK = responseCodeFK,
                                LeadershipCoachLogFK = currentFormPK
                            };
                        }

                        //Add the record to the list of valid records
                        validResponseItems.Add(existingRecord);
                    }
                }
            }

            //Return the valid records
            return validResponseItems;
        }

        /// <summary>
        /// This method returns the specify text for a specific response item
        /// </summary>
        /// <param name="currentSpecifyTextBox">The BootstrapMemo specify field</param>
        /// <param name="currentResponseList">The list of response items</param>
        /// <param name="specifyResponseCodePK">The PK of the CodeLCLResponse row that indicates that the specify field is required</param>
        /// <returns>A string that contains the specify text</returns>
        private string GetResponseSpecifyText(BootstrapMemo currentSpecifyTextBox, List<LCLResponse> currentResponseList, int specifyResponseCodePK)
        {
            string returnValue = null;

            //If the specify response was selected, return the value in the text box
            if (currentResponseList.Where(r => r.LCLResponseCodeFK == specifyResponseCodePK).Count() > 0)
            {
                returnValue = (string.IsNullOrWhiteSpace(currentSpecifyTextBox.Text) ? null : currentSpecifyTextBox.Text);
            }

            return returnValue;
        }

        /// <summary>
        /// This method sets the selected team members that were engaged
        /// </summary>
        /// <param name="currentList">The current list of team member  records</param>
        /// <param name="currentFormPK">The current LeadershipCoachLog PK</param>
        /// <returns>A list of LCLInvolvedCoach objects that represent the values selected in the list box</returns>
        private List<LCLInvolvedCoach> GetInvolvedCoachesList(List<LCLInvolvedCoach> currentList, int currentFormPK)
        {
            List<LCLInvolvedCoach> validRecords = new List<LCLInvolvedCoach>();

            //Get the members selected from the hidden field.
            //Can't use the ListBox.SelectedItems since that is always empty due to a quirk of
            //how DevEx handles multi column list boxes.
            List<int> selectedCoaches = GetSelectedCoachPKs();

            //Only continue if there are members selected
            if (selectedCoaches != null && selectedCoaches.Count > 0)
            {
                //Loop through all the selected list box items
                foreach (int involvedCoachFK in selectedCoaches)
                {
                    //Check to see if the item already exists
                    LCLInvolvedCoach existingRecord = currentList.Where(r => r.ProgramEmployeeFK == involvedCoachFK).FirstOrDefault();

                    if (existingRecord == null)
                    {
                        existingRecord = new LCLInvolvedCoach()
                        {
                            Creator = User.Identity.Name,
                            CreateDate = DateTime.Now,
                            ProgramEmployeeFK = involvedCoachFK,
                            LeadershipCoachLogFK = currentFormPK
                        };
                    }

                    //Add the record to the list of valid records
                    validRecords.Add(existingRecord);
                }
            }

            //Return the valid records
            return validRecords;
        }

        /// <summary>
        /// This method sets the selected team members that were engaged
        /// </summary>
        /// <param name="currentEngagementList">The current list of team member engagement records</param>
        /// <param name="currentFormPK">The current LeadershipCoachLog PK</param>
        /// <returns>A list of LCLTeamMemberEngagement objects that represent the values selected in the list box</returns>
        private List<LCLTeamMemberEngagement> GetTeamMembersEngagementList(List<LCLTeamMemberEngagement> currentEngagementList, int currentFormPK)
        {
            List<LCLTeamMemberEngagement> validEngagementRecords = new List<LCLTeamMemberEngagement>();

            //Get the members selected from the hidden field.
            //Can't use the ListBox.SelectedItems since that is always empty due to a quirk of
            //how DevEx handles multi column list boxes.
            List<int> selectedMembers = GetSelectedTeamMemberPKs();

            //Only continue if there are members selected
            if (selectedMembers != null && selectedMembers.Count > 0)
            {
                //Loop through all the selected list box items
                foreach (int engagedMemberFK in selectedMembers)
                {
                    //Check to see if the item already exists
                    LCLTeamMemberEngagement existingRecord = currentEngagementList.Where(r => r.PLTMemberFK == engagedMemberFK).FirstOrDefault();

                    if (existingRecord == null)
                    {
                        existingRecord = new LCLTeamMemberEngagement()
                        {
                            Creator = User.Identity.Name,
                            CreateDate = DateTime.Now,
                            PLTMemberFK = engagedMemberFK,
                            LeadershipCoachLogFK = currentFormPK
                        };
                    }

                    //Add the record to the list of valid records
                    validEngagementRecords.Add(existingRecord);
                }
            }

            //Return the valid records
            return validEngagementRecords;
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            ddProgram.ClientEnabled = enabled;
            ddLeadershipCoach.ClientEnabled = enabled;
            lbInvolvedCoaches.ClientEnabled = enabled;
            ddIsMonthly.ClientEnabled = enabled;
            deDateCompleted.ClientEnabled = enabled;
            txtTotalDurationHours.ClientEnabled = enabled;
            txtTotalDurationMinutes.ClientEnabled = enabled;
            deLCLMonth.ClientEnabled = enabled;
            txtMonthlyNumEngagements.ClientEnabled = enabled;
            txtMonthlyNumAttemptedEngagements.ClientEnabled = enabled;
            ddCyclePhase.ClientEnabled = enabled;
            ddTimelyProgression.ClientEnabled = enabled;
            ddGoalCompletionLikelihood.ClientEnabled = enabled;
            lbTeamMemberEngagement.ClientEnabled = enabled;
            txtOtherEngagementSpecify.ClientEnabled = enabled;
            lbDomain1.ClientEnabled = enabled;
            lbDomain2.ClientEnabled = enabled;
            txtOtherDomainTwoSpecify.ClientEnabled = enabled;
            lbDomain3.ClientEnabled = enabled;
            lbDomain4.ClientEnabled = enabled;
            lbDomain5.ClientEnabled = enabled;
            lbDomain6.ClientEnabled = enabled;
            lbIdentifiedProgramBarriers.ClientEnabled = enabled;
            lbIdentifiedProgramStrengths.ClientEnabled = enabled;
            lbSiteResources.ClientEnabled = enabled;
            txtOtherSiteResourcesSpecify.ClientEnabled = enabled;
            lbTopicsDiscussed.ClientEnabled = enabled;
            txtOtherTopicsDiscussedSpecify.ClientEnabled = enabled;
            txtTargetedTrainingHours.ClientEnabled = enabled;
            txtTargetedTrainingMinutes.ClientEnabled = enabled;
            lbTrainingsCovered.ClientEnabled = enabled;
            txtOtherTrainingsCoveredSpecify.ClientEnabled = enabled;
            txtThinkNarrative.ClientEnabled = enabled;
            txtActNarrative.ClientEnabled = enabled;
            txtHighlightsNarrative.ClientEnabled = enabled;
            chkIsComplete.ClientEnabled = enabled;

            //Show/hide the submit button
            submitLeadershipCoachLog.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitLeadershipCoachLog.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method binds the drop-downs for this page
        /// </summary>
        private void BindInputControls()
        {
            //To hold all the necessary items
            List<Program> allPrograms = new List<Program>();
            List<CodeLCLResponse> allResponseOptions = new List<CodeLCLResponse>();

            //Get all the items
            using (PyramidContext context = new PyramidContext())
            {
                allPrograms = context.Program.AsNoTracking()
                                             .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                             .OrderBy(p => p.ProgramName)
                                             .ToList();

                allResponseOptions = context.CodeLCLResponse.AsNoTracking()
                                                .OrderBy(r => r.Group)
                                                .ThenBy(r => r.OrderBy)
                                                .ToList();
            }

            //Bind the program dropdown
            ddProgram.DataSource = allPrograms;
            ddProgram.DataBind();

            //Bind the response controls
            ddGoalCompletionLikelihood.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.GOAL_COMPLETION.ToLower()).ToList();
            ddGoalCompletionLikelihood.DataBind();

            ddTimelyProgression.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.TIMELY_PROGRESSION.ToLower()).ToList();
            ddTimelyProgression.DataBind();

            //Bind the list boxes and set the "Rows" property to the count of
            //items so that all items display without requiring scrolling
            lbDomain1.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.DOMAIN_1.ToLower()).ToList();
            lbDomain1.DataBind();
            lbDomain1.Rows = lbDomain1.Items.Count;

            lbDomain2.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.DOMAIN_2.ToLower()).ToList();
            lbDomain2.DataBind();
            lbDomain2.Rows = lbDomain2.Items.Count;

            lbDomain3.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.DOMAIN_3.ToLower()).ToList();
            lbDomain3.DataBind();
            lbDomain3.Rows = lbDomain3.Items.Count;

            lbDomain4.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.DOMAIN_4.ToLower()).ToList();
            lbDomain4.DataBind();
            lbDomain4.Rows = lbDomain4.Items.Count;

            lbDomain5.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.DOMAIN_5.ToLower()).ToList();
            lbDomain5.DataBind();
            lbDomain5.Rows = lbDomain5.Items.Count;

            lbDomain6.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.DOMAIN_6.ToLower()).ToList();
            lbDomain6.DataBind();
            lbDomain6.Rows = lbDomain6.Items.Count;

            lbIdentifiedProgramBarriers.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_BARRIERS.ToLower()).ToList();
            lbIdentifiedProgramBarriers.DataBind();
            lbIdentifiedProgramBarriers.Rows = lbIdentifiedProgramBarriers.Items.Count;

            lbIdentifiedProgramStrengths.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_STRENGTHS.ToLower()).ToList();
            lbIdentifiedProgramStrengths.DataBind();
            lbIdentifiedProgramStrengths.Rows = lbIdentifiedProgramStrengths.Items.Count;

            lbSiteResources.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.SITE_RESOURCES.ToLower()).ToList();
            lbSiteResources.DataBind();
            lbSiteResources.Rows = lbSiteResources.Items.Count;

            lbTopicsDiscussed.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.TOPICS_DISCUSSED.ToLower()).ToList();
            lbTopicsDiscussed.DataBind();
            lbTopicsDiscussed.Rows = lbTopicsDiscussed.Items.Count;

            lbTrainingsCovered.DataSource = allResponseOptions.Where(r => r.Group.ToLower() == Models.CodeLCLResponse.ResponseGroups.TRAININGS_COVERED.ToLower()).ToList();
            lbTrainingsCovered.DataBind();
            lbTrainingsCovered.Rows = lbTrainingsCovered.Items.Count;
        }

        /// <summary>
        /// This method sets the program labels based on the passed parameter
        /// </summary>
        /// <param name="currentProgramFK">The program FK</param>
        private void SetProgramLabels(int? currentProgramFK)
        {
            //Make sure the selected program is valid before continuing
            if (currentProgramFK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program info
                    Program selectedProgram = context.Program.Include(p => p.ProgramType)
                                                        .Include(p => p.ProgramType.Select(pt => pt.CodeProgramType))
                                                        .AsNoTracking()
                                                        .Where(p => p.ProgramPK == currentProgramFK.Value)
                                                        .FirstOrDefault();

                    //Make sure the program exists
                    if (selectedProgram != null)
                    {
                        //Set the program-specific labels
                        lblProgramIDNumber.Text = (string.IsNullOrWhiteSpace(selectedProgram.IDNumber) ? "No ID Number Registered" : selectedProgram.IDNumber);

                        if (selectedProgram.ProgramType.Count > 0)
                        {
                            lblProgramTypes.Text = string.Join(", ", selectedProgram.ProgramType.OrderBy(pt => pt.CodeProgramType.Description).Select(pt => pt.CodeProgramType.Description));
                        }
                    }
                }
            }
        }

        private void SetMostRecentBOQInfo(DateTime? logDate, int? currentProgramFK)
        {
            //Make sure the necessary values are there
            if (currentProgramFK.HasValue && logDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the most recent BOQ info
                    var mostRecentBOQDate = context.BenchmarkOfQuality.AsNoTracking()
                                                .Where(boq => boq.ProgramFK == currentProgramFK.Value &&
                                                              boq.FormDate <= logDate.Value)
                                                .OrderByDescending(boq => boq.FormDate)
                                                .Select(boq => boq.FormDate)
                                                .FirstOrDefault();

                    var mostRecentBOQFCCDate = context.BenchmarkOfQualityFCC.AsNoTracking()
                                                .Where(boqf => boqf.ProgramFK == currentProgramFK.Value &&
                                                              boqf.FormDate <= logDate.Value)
                                                .OrderByDescending(boqf => boqf.FormDate)
                                                .Select(boqf => boqf.FormDate)
                                                .FirstOrDefault();

                    //Get the most recent date
                    if (mostRecentBOQDate != DateTime.MinValue && mostRecentBOQDate >= mostRecentBOQFCCDate)
                    {
                        lblMostRecentBOQDate.Text = mostRecentBOQDate.ToString("MM/dd/yyyy");
                    }
                    else if (mostRecentBOQFCCDate != DateTime.MinValue)
                    {
                        lblMostRecentBOQDate.Text = mostRecentBOQFCCDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        lblMostRecentBOQDate.Text = "No BOQs found...";
                    }
                }
            }
            else
            {
                lblMostRecentBOQDate.Text = "Program and Date/Month must be entered...";
            }
        }

        /// <summary>
        /// This method binds the list of leadership coaches
        /// </summary>
        /// <param name="currentProgramFK">The program FK used to filter the dropdown</param>
        private void BindLeadershipCoachList(int? currentProgramFK)
        {
            //Make sure the program is valid
            if (currentProgramFK.HasValue && currentProgramFK.Value > 0)
            {
                if (currentProgramFK.Value == currentLeadershipCoachLog.ProgramFK)
                {
                    //Get the leadership coaches for the program and
                    //always include the coach saved to the form in case
                    //the coach's role was removed.
                    allLeadershipCoaches = PyramidUser.GetProgramLeadershipCoachUserRecords(new List<int> { currentProgramFK.Value }, currentLeadershipCoachLog.LeadershipCoachUsername);
                }
                else
                {
                    //Get the leadership coaches for the program
                    allLeadershipCoaches = PyramidUser.GetProgramLeadershipCoachUserRecords(new List<int> { currentProgramFK.Value });
                }
            }
            else
            {
                //Set to a blank list
                allLeadershipCoaches = new List<PyramidUser>();
            }
        }

        /// <summary>
        /// This method populates the leadership coach dropdown based on the passed parameters
        /// </summary>
        /// <param name="currentProgramFK">The program FK used to filter the dropdown</param>
        /// <param name="currentLCUsername">The current selected leadership coach username (if any)</param>
        private void BindLeadershipCoachDropDown(int? currentProgramFK, string currentLCUsername)
        {
            //Make sure the program is valid
            if (currentProgramFK.HasValue && currentProgramFK.Value > 0)
            {
                //Bind the leadership coach dropdown
                var leadershipCoaches = allLeadershipCoaches.Select(lc => new { lc.UserName, FullName = string.Format("{0} {1}", lc.FirstName, lc.LastName) }).OrderBy(lc => lc.FullName).ToList();
                ddLeadershipCoach.DataSource = leadershipCoaches;
                ddLeadershipCoach.DataBind();

                //Check to see how many coaches there are
                if (ddLeadershipCoach.Items.Count > 0)
                {
                    //There is at least 1 coach, enable the child dropdown
                    ddLeadershipCoach.ReadOnly = false;

                    //Try to select the coach passed to this method
                    ddLeadershipCoach.SelectedItem = ddLeadershipCoach.Items.FindByValue(currentLCUsername);
                }
                else
                {
                    //There are no coaches in the list, disable the coach dropdown
                    ddLeadershipCoach.Value = "";
                    ddLeadershipCoach.ReadOnly = true;
                }
            }
            else
            {
                //Clear the leadership coach dropdown and disable it
                ddLeadershipCoach.Value = "";
                ddLeadershipCoach.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method binds the involved coach list box by getting all the coaches in
        /// the program that were active.
        /// </summary>
        /// <param name="startWindow">The start of the date window for checking if active</param>
        /// <param name="endWindow">The end of the date window for checking if active</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="selectedInvolvedCoaches">The coaches selected in the box</param>
        private void BindInvolvedCoachListBox(DateTime? logDate, bool? isMonthly, int? programFK, List<int> selectedInvolvedCoaches)
        {
            if (logDate.HasValue && isMonthly.HasValue && programFK.HasValue)
            {
                DateTime startOfWindow, endOfWindow;

                if (isMonthly.Value)
                {
                    startOfWindow = new DateTime(logDate.Value.Year, logDate.Value.Month, 1);
                    endOfWindow = startOfWindow.AddMonths(1).AddDays(-1);
                }
                else
                {
                    startOfWindow = logDate.Value;
                    endOfWindow = logDate.Value;
                }

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the active coaches
                    var activeCoaches = context.ProgramEmployee
                                                .Include(pe => pe.Employee)
                                                .Include(pe => pe.JobFunction)
                                                .AsNoTracking()
                                                .Where(pe => pe.ProgramFK == programFK.Value &&
                                                             (selectedInvolvedCoaches.Contains(pe.ProgramEmployeePK) ||
                                                                 (pe.HireDate <= endOfWindow &&
                                                                 (pe.TermDate.HasValue == false ||
                                                                    pe.TermDate >= startOfWindow) &&
                                                                 pe.JobFunction.Where(jf => jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.CLASSROOM_COACH && 
                                                                                            jf.StartDate <= endOfWindow && 
                                                                                            (jf.EndDate.HasValue == false ||
                                                                                                jf.EndDate.Value >= startOfWindow)).Count() > 0)))
                                                .ToList();

                    //Get the coaches
                    //Display the names if the user is allowed to see private employee info
                    //OR if the user is a Leadership Coach (as per Summer Edwards on 03/28/2023)
                    var listBoxDataSource = activeCoaches.Select(ac =>
                                                   new
                                                   {
                                                       ac.ProgramEmployeePK,
                                                       EmployeeIDAndName = (currentProgramRole.ViewPrivateEmployeeInfo.Value || 
                                                                            currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ? 
                                                                                "(" + ac.ProgramSpecificID + ") " + ac.Employee.FirstName + " " + ac.Employee.LastName : ac.ProgramSpecificID)
                                                   })
                                                .Distinct()
                                                .OrderBy(ac => ac.EmployeeIDAndName)
                                                .ToList();

                    //Bind the list box to the list of active members and set the "Rows"
                    //property so that the user doesn't need to scroll
                    lbInvolvedCoaches.DataSource = listBoxDataSource;
                    lbInvolvedCoaches.DataBind();

                    if (lbInvolvedCoaches.Items.Count > 0)
                    {
                        if (lbInvolvedCoaches.Items.Count > 5)
                        {
                            lbInvolvedCoaches.Rows = 5;
                        }
                        else
                        {
                            lbInvolvedCoaches.Rows = lbInvolvedCoaches.Items.Count;
                        }
                    }

                    if (activeCoaches.Count > 0)
                    {
                        //Enable the list box
                        lbInvolvedCoaches.ReadOnly = false;

                        //Re-select team members after binding
                        if (selectedInvolvedCoaches != null && selectedInvolvedCoaches.Count > 0)
                        {
                            foreach (int involvedCoachPK in selectedInvolvedCoaches)
                            {
                                var listBoxItem = lbInvolvedCoaches.Items.FindByValue(involvedCoachPK);

                                if (listBoxItem != null)
                                {
                                    listBoxItem.Selected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Disable the list box
                        lbInvolvedCoaches.ReadOnly = true;
                    }
                }
            }
            else
            {
                //Disable the list box
                lbInvolvedCoaches.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method binds the team member tag box by getting all the leadership team members in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="startWindow">The start of the date window for checking if active</param>
        /// <param name="endWindow">The end of the date window for checking if active</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="selectedTeamMembers">The coachees selected in the box</param>
        private void BindTeamMemberListBox(DateTime? logDate, bool? isMonthly, int? programFK, List<int> selectedTeamMembers)
        {
            if (logDate.HasValue && isMonthly.HasValue && programFK.HasValue)
            {
                DateTime startOfWindow, endOfWindow;

                if (isMonthly.Value)
                {
                    startOfWindow = new DateTime(logDate.Value.Year, logDate.Value.Month, 1);
                    endOfWindow = startOfWindow.AddMonths(1).AddDays(-1);
                }
                else
                {
                    startOfWindow = logDate.Value;
                    endOfWindow = logDate.Value;
                }

                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the active team members
                    var activePLTMembers = context.PLTMember
                                                .Include(tm => tm.PLTMemberRole)
                                                .Include(tm => tm.PLTMemberRole.Select(r => r.CodeTeamPosition))
                                                .AsNoTracking()
                                                .Where(tm => tm.ProgramFK == programFK.Value &&
                                                             (selectedTeamMembers.Contains(tm.PLTMemberPK) ||
                                                                 (tm.StartDate <= endOfWindow &&
                                                                 (tm.LeaveDate.HasValue == false ||
                                                                    tm.LeaveDate >= startOfWindow))))
                                                .ToList();

                    var listBoxDataSource = activePLTMembers.Select(tm =>
                                                   new
                                                   {
                                                       tm.PLTMemberPK,
                                                       MemberIDAndName = "(" + tm.IDNumber + ") " + tm.FirstName + " " + tm.LastName,
                                                       tm.EmailAddress,
                                                       Roles = string.Join(", ", tm.PLTMemberRole.Select(r => r.CodeTeamPosition.Description).ToList()),
                                                       StartDate = tm.StartDate.ToString("MM/dd/yyyy")
                                                   })
                                                .Distinct()
                                                .OrderBy(tm => tm.MemberIDAndName)
                                                .ToList();

                    //Bind the list box to the list of active members and set the "Rows"
                    //property so that the user doesn't need to scroll
                    lbTeamMemberEngagement.DataSource = listBoxDataSource;
                    lbTeamMemberEngagement.DataBind();

                    if (lbTeamMemberEngagement.Items.Count > 0)
                    {
                        if (lbTeamMemberEngagement.Items.Count > 10)
                        {
                            lbTeamMemberEngagement.Rows = 10;
                        }
                        else
                        {
                            lbTeamMemberEngagement.Rows = lbTeamMemberEngagement.Items.Count;
                        }
                    }

                    if (activePLTMembers.Count > 0)
                    {
                        //Enable the list box
                        lbTeamMemberEngagement.ReadOnly = false;

                        //Re-select team members after binding
                        if (selectedTeamMembers != null && selectedTeamMembers.Count > 0)
                        {
                            foreach (int teamMemberPK in selectedTeamMembers)
                            {
                                var listBoxItem = lbTeamMemberEngagement.Items.FindByValue(teamMemberPK);

                                if (listBoxItem != null)
                                {
                                    listBoxItem.Selected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Disable the list box
                        lbTeamMemberEngagement.ReadOnly = true;
                    }
                }
            }
            else
            {
                //Disable the list box
                lbTeamMemberEngagement.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitLeadershipCoachLog user control 
        /// </summary>
        /// <param name="sender">The submitLeadershipCoachLog control</param>
        /// <param name="e">The Click event</param>
        protected void submitLeadershipCoachLog_Click(object sender, EventArgs e)
        {
            //Try to save the form to the database
            bool formSaved = SaveForm(true);

            //Only allow redirect if the save succeeded
            if (formSaved)
            {
                //Show a message after redirect
                msgSys.AddMessageToQueue("success", "Success", "Leadership Coach Log successfully edited!", 1000);

                //Redirect the user to the dashboard
                Response.Redirect(string.Format("/Pages/LeadershipCoachDashboard.aspx"));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitLeadershipCoachLog user control 
        /// </summary>
        /// <param name="sender">The submitLeadershipCoachLog control</param>
        /// <param name="e">The Click event</param>
        protected void submitLeadershipCoachLog_CancelClick(object sender, EventArgs e)
        {
            //Show a message after redirect
            msgSys.AddMessageToQueue("info", "Canceled", "The action was canceled, no changes were saved.", 10000);

            //Redirect the user to the dashboard
            Response.Redirect("/Pages/LeadershipCoachDashboard.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitLeadershipCoachLog control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitLeadershipCoachLog_ValidationFailed(object sender, EventArgs e)
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
        /// This method prints the form
        /// </summary>
        private void PrintForm()
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitLeadershipCoachLog.ValidationGroup))
            {
                //Submit the form
                bool formSaved = SaveForm(false);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    //Get the master page
                    MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                    //Get the report
                    Reports.PreBuiltReports.FormReports.RptLeadershipCoachLog report = new Reports.PreBuiltReports.FormReports.RptLeadershipCoachLog();

                    //Set the role FK parameter
                    report.ParamUserRoleFK.Value = currentProgramRole.CodeProgramRoleFK.Value;

                    //Display the report
                    masterPage.DisplayReport(currentProgramRole, report, "Leadership Coach Log", currentLCLPK);
                }
                else
                {
                    //Get the action
                    string action = "View";
                    if (formSaved)
                    {
                        //The save was successful, the user will be editing
                        action = "Edit";
                    }

                    //Show a message after redirect
                    msgSys.AddMessageToQueue("success", "Success", "The Leadership Coach Log was successfully saved!", 12000);

                    //Redirect the user back to this page with the PK
                    Response.Redirect(string.Format("/Pages/LeadershipCoachLog.aspx?LeadershipCoachLogPK={0}&Action={1}&Print=True",
                                                        currentLCLPK, action));
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
        private bool SaveForm(bool showMessages)
        {
            //Whether or not the save succeeded
            bool didSaveSucceed = false;

            //Check the user permissions
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the field values from the form
                currentLeadershipCoachLog.LeadershipCoachUsername = (ddLeadershipCoach.Value != null ? ddLeadershipCoach.Value.ToString() : null);
                currentLeadershipCoachLog.ProgramFK = Convert.ToInt32(ddProgram.Value);
                bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);
                currentLeadershipCoachLog.IsMonthly = isMonthly;
                
                if (isMonthly.HasValue)
                {
                    if (isMonthly.Value)
                    {
                        //Set the monthly log values
                        currentLeadershipCoachLog.DateCompleted = GetLogDateFromForm();
                        currentLeadershipCoachLog.NumberOfEngagements = (string.IsNullOrWhiteSpace(txtMonthlyNumEngagements.Text) ? (int?)null : Convert.ToInt32(txtMonthlyNumEngagements.Text)); ;
                        currentLeadershipCoachLog.NumberOfAttemptedEngagements = (string.IsNullOrWhiteSpace(txtMonthlyNumAttemptedEngagements.Text) ? (int?)null : Convert.ToInt32(txtMonthlyNumAttemptedEngagements.Text));

                        //Clear the single encounter values
                        currentLeadershipCoachLog.TotalDurationHours = null;
                        currentLeadershipCoachLog.TotalDurationMinutes = null;
                    }
                    else
                    {
                        //Clear the monthly log values
                        currentLeadershipCoachLog.NumberOfEngagements = null;
                        currentLeadershipCoachLog.NumberOfAttemptedEngagements = null;

                        //Set the single encounter values
                        currentLeadershipCoachLog.DateCompleted = GetLogDateFromForm();
                        currentLeadershipCoachLog.TotalDurationHours = (string.IsNullOrWhiteSpace(txtTotalDurationHours.Text) ? (int?)null : Convert.ToInt32(txtTotalDurationHours.Text));
                        currentLeadershipCoachLog.TotalDurationMinutes = (string.IsNullOrWhiteSpace(txtTotalDurationMinutes.Text) ? (int?)null : Convert.ToInt32(txtTotalDurationMinutes.Text));
                    }
                }

                currentLeadershipCoachLog.CyclePhase = (ddCyclePhase.Value != null ? Convert.ToInt32(ddCyclePhase.Value) : (int?)null);
                currentLeadershipCoachLog.TimelyProgressionLikelihoodCodeFK = (ddTimelyProgression.Value != null ? Convert.ToInt32(ddTimelyProgression.Value) : (int?)null);
                currentLeadershipCoachLog.GoalCompletionLikelihoodCodeFK = (ddGoalCompletionLikelihood.Value != null ? Convert.ToInt32(ddGoalCompletionLikelihood.Value) : (int?)null);

                //Involved coaches list box
                List<LCLInvolvedCoach> involvedCoachRecords = currentLeadershipCoachLog.LCLInvolvedCoach.ToList();
                List<LCLInvolvedCoach> validInvolvedCoachRecords = GetInvolvedCoachesList(involvedCoachRecords, currentLCLPK);

                //Team member engagement list box
                List<LCLTeamMemberEngagement> memberEngagementRecords = currentLeadershipCoachLog.LCLTeamMemberEngagement.ToList();
                List<LCLTeamMemberEngagement> validMemberEngagementRecords = GetTeamMembersEngagementList(memberEngagementRecords, currentLCLPK);

                //Multi-select list boxes
                List<LCLResponse> responseItemList = currentLeadershipCoachLog.LCLResponse.ToList();
                List<LCLResponse> validResponseItems = new List<LCLResponse>();
                validResponseItems.AddRange(GetResponseListFromListBox(lbDomain1, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_1, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbDomain2, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_2, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbDomain3, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_3, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbDomain4, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_4, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbDomain5, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_5, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbDomain6, responseItemList, CodeLCLResponse.ResponseGroups.DOMAIN_6, currentLCLPK));

                validResponseItems.AddRange(GetResponseListFromListBox(lbIdentifiedProgramBarriers, responseItemList, CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_BARRIERS, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbIdentifiedProgramStrengths, responseItemList, CodeLCLResponse.ResponseGroups.IDENTIFIED_PROGRAM_STRENGTHS, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbSiteResources, responseItemList, CodeLCLResponse.ResponseGroups.SITE_RESOURCES, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbTopicsDiscussed, responseItemList, CodeLCLResponse.ResponseGroups.TOPICS_DISCUSSED, currentLCLPK));
                validResponseItems.AddRange(GetResponseListFromListBox(lbTrainingsCovered, responseItemList, CodeLCLResponse.ResponseGroups.TRAININGS_COVERED, currentLCLPK));

                //Other specify fields
                currentLeadershipCoachLog.OtherEngagementSpecify = (string.IsNullOrWhiteSpace(txtOtherEngagementSpecify.Text) ? null : txtOtherEngagementSpecify.Text);
                currentLeadershipCoachLog.OtherDomainTwoSpecify = GetResponseSpecifyText(txtOtherDomainTwoSpecify, validResponseItems, CodeLCLResponse.OtherSpecifyPKs.DOMAIN_2_SPECIFY);
                currentLeadershipCoachLog.OtherSiteResourcesSpecify = GetResponseSpecifyText(txtOtherSiteResourcesSpecify, validResponseItems, CodeLCLResponse.OtherSpecifyPKs.SITE_RESOURCES_SPECIFY);
                currentLeadershipCoachLog.OtherTopicsDiscussedSpecify = GetResponseSpecifyText(txtOtherTopicsDiscussedSpecify, validResponseItems, CodeLCLResponse.OtherSpecifyPKs.TOPICS_DISCUSSED_SPECIFY);
                currentLeadershipCoachLog.OtherTrainingsCoveredSpecify = GetResponseSpecifyText(txtOtherTrainingsCoveredSpecify, validResponseItems, CodeLCLResponse.OtherSpecifyPKs.TRAININGS_COVERED_SPECIFY);

                //Targeted training
                currentLeadershipCoachLog.TargetedTrainingHours = (string.IsNullOrWhiteSpace(txtTargetedTrainingHours.Text) ? (int?)null : Convert.ToInt32(txtTargetedTrainingHours.Text));
                currentLeadershipCoachLog.TargetedTrainingMinutes = (string.IsNullOrWhiteSpace(txtTargetedTrainingMinutes.Text) ? (int?)null : Convert.ToInt32(txtTargetedTrainingMinutes.Text));

                //Narrative fields
                currentLeadershipCoachLog.ThinkNarrative = (string.IsNullOrWhiteSpace(txtThinkNarrative.Text) ? null : txtThinkNarrative.Text);
                currentLeadershipCoachLog.ActNarrative = (string.IsNullOrWhiteSpace(txtActNarrative.Text) ? null : txtActNarrative.Text);
                currentLeadershipCoachLog.HighlightsNarrative = (string.IsNullOrWhiteSpace(txtHighlightsNarrative.Text) ? null : txtHighlightsNarrative.Text);

                currentLeadershipCoachLog.IsComplete = chkIsComplete.Checked;

                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the edit-only fields
                        currentLeadershipCoachLog.EditDate = DateTime.Now;
                        currentLeadershipCoachLog.Editor = User.Identity.Name;

                        //Get the existing database values
                        Models.LeadershipCoachLog existingLeadershipCoachLog = context.LeadershipCoachLog.Find(currentLeadershipCoachLog.LeadershipCoachLogPK);

                        //Set the object to the new values
                        context.Entry(existingLeadershipCoachLog).CurrentValues.SetValues(currentLeadershipCoachLog);

                        //Fill the list of involved coaches
                        foreach (LCLInvolvedCoach coachRecord in validInvolvedCoachRecords)
                        {
                            if (coachRecord.LCLInvolvedCoachPK == 0)
                            {
                                //Add new rows
                                context.LCLInvolvedCoach.Add(coachRecord);
                            }
                        }

                        //Get the valid coach FKs
                        List<int> validCoachFKs = validInvolvedCoachRecords.Select(er => er.ProgramEmployeeFK).ToList();

                        //To hold the involved coach PKs that will be removed
                        List<int> deletedInvolvedCoachRecordPKs = involvedCoachRecords.Where(er => validCoachFKs.Contains(er.ProgramEmployeeFK) == false).Select(er => er.LCLInvolvedCoachPK).ToList();

                        //Delete the involved coach records
                        context.LCLInvolvedCoach.Where(ltme => deletedInvolvedCoachRecordPKs.Contains(ltme.LCLInvolvedCoachPK)).Delete();

                        //Fill the list of team member engagement
                        foreach (LCLTeamMemberEngagement engagementRecord in validMemberEngagementRecords)
                        {
                            if (engagementRecord.LCLTeamMemberEngagementPK == 0)
                            {
                                //Add new rows
                                context.LCLTeamMemberEngagement.Add(engagementRecord);
                            }
                        }

                        //Get the valid member FKs
                        List<int> validPLTMemberFKs = validMemberEngagementRecords.Select(er => er.PLTMemberFK).ToList();

                        //To hold the engagement record PKs that will be removed
                        List<int> deletedEngagementRecordPKs = memberEngagementRecords.Where(er => validPLTMemberFKs.Contains(er.PLTMemberFK) == false).Select(er => er.LCLTeamMemberEngagementPK).ToList();

                        //Delete the engagment records
                        context.LCLTeamMemberEngagement.Where(ltme => deletedEngagementRecordPKs.Contains(ltme.LCLTeamMemberEngagementPK)).Delete();

                        //Fill the list of responses
                        foreach (LCLResponse responseItem in validResponseItems)
                        {
                            if (responseItem.LCLResponsePK == 0)
                            {
                                //Add new rows
                                context.LCLResponse.Add(responseItem);
                            }
                        }

                        //Get the valid response code FKs
                        List<int> validResponseCodeFKs = validResponseItems.Select(r => r.LCLResponseCodeFK).ToList();

                        //To hold the response PKs that will be removed
                        List<int> deletedResponsePKs = responseItemList.Where(r => validResponseCodeFKs.Contains(r.LCLResponseCodeFK) == false).Select(r => r.LCLResponsePK).ToList();

                        //Delete the response rows
                        context.LCLResponse.Where(lr => deletedResponsePKs.Contains(lr.LCLResponsePK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows for the deletes, and update the deleter field
                        context.LCLInvolvedCoachChanged.Where(licc => deletedInvolvedCoachRecordPKs.Contains(licc.LCLInvolvedCoachPK)).Update(pc => new LCLInvolvedCoachChanged() { Deleter = User.Identity.Name });
                        context.LCLTeamMemberEngagementChanged.Where(ltmec => deletedEngagementRecordPKs.Contains(ltmec.LCLTeamMemberEngagementPK)).Update(pc => new LCLTeamMemberEngagementChanged() { Deleter = User.Identity.Name });
                        context.LCLResponseChanged.Where(lrc => deletedResponsePKs.Contains(lrc.LCLResponsePK)).Update(pc => new LCLResponseChanged() { Deleter = User.Identity.Name });

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfLeadershipCoachLogPK.Value = currentLeadershipCoachLog.LeadershipCoachLogPK.ToString();
                        currentLCLPK = currentLeadershipCoachLog.LeadershipCoachLogPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the create-only fields
                        currentLeadershipCoachLog.CreateDate = DateTime.Now;
                        currentLeadershipCoachLog.Creator = User.Identity.Name;

                        //Add it to the context
                        context.LeadershipCoachLog.Add(currentLeadershipCoachLog);

                        //Save the changes
                        context.SaveChanges();

                        //Fill the list of involved coach records
                        foreach (LCLInvolvedCoach coachRecord in validInvolvedCoachRecords)
                        {
                            if (coachRecord.LCLInvolvedCoachPK == 0)
                            {
                                //Add new rows
                                coachRecord.LeadershipCoachLogFK = currentLeadershipCoachLog.LeadershipCoachLogPK;
                                context.LCLInvolvedCoach.Add(coachRecord);
                            }
                        }

                        //Fill the list of team member engagment records
                        foreach (LCLTeamMemberEngagement engagementRecord in validMemberEngagementRecords)
                        {
                            if (engagementRecord.LCLTeamMemberEngagementPK == 0)
                            {
                                //Add new rows
                                engagementRecord.LeadershipCoachLogFK = currentLeadershipCoachLog.LeadershipCoachLogPK;
                                context.LCLTeamMemberEngagement.Add(engagementRecord);
                            }
                        }

                        //Fill the list of responses
                        foreach (LCLResponse responseItem in validResponseItems)
                        {
                            if (responseItem.LCLResponsePK == 0)
                            {
                                //Add new rows
                                responseItem.LeadershipCoachLogFK = currentLeadershipCoachLog.LeadershipCoachLogPK;
                                context.LCLResponse.Add(responseItem);
                            }
                        }

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfLeadershipCoachLogPK.Value = currentLeadershipCoachLog.LeadershipCoachLogPK.ToString();
                        currentLCLPK = currentLeadershipCoachLog.LeadershipCoachLogPK;

                        //Save success
                        didSaveSucceed = true;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            return didSaveSucceed;
        }

        /// <summary>
        /// This method fires when the validation for the ddLeadershipCoach DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddLeadershipCoach BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddLeadershipCoach_Validation(object sender, ValidationEventArgs e)
        {
            //Get the selected leadership coach's username
            string leadershipCoachUsername = (ddLeadershipCoach.Value == null ? null : Convert.ToString(ddLeadershipCoach.Value));

            //Get the selected program FK
            int? selectedProgramFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

            //Only validate if the form is complete
            if (chkIsComplete.Checked)
            {
                if (string.IsNullOrWhiteSpace(leadershipCoachUsername))
                {
                    e.IsValid = false;
                    e.ErrorText = "Leadership Coach is required!";
                }
                else if (selectedProgramFK.HasValue)
                {
                    //Only do this validation if a program was selected and a coach was selected
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the matching leadership coach roles for the selected leadership coach and the selected program
                        List<UserProgramRole> matchingLeadershipCoachRoles = context.UserProgramRole.AsNoTracking()
                                .Include(upr => upr.Program)
                                .Where(upr => upr.Username == leadershipCoachUsername &&
                                                (upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                                                    upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH) &&
                                                upr.ProgramFK == selectedProgramFK)
                                .ToList();

                        //Validate
                        if (currentLeadershipCoachLog.ProgramFK == selectedProgramFK &&
                                currentLeadershipCoachLog.LeadershipCoachUsername == leadershipCoachUsername)
                        {
                            //Allow saving the leadership coach log if the program FK and leadership coach have not changed since the leadership coach
                            //may lose the role in their account, but we don't have a date for that.
                            e.IsValid = true;
                        }
                        else if (matchingLeadershipCoachRoles == null || matchingLeadershipCoachRoles.Count <= 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "That combination of Program and Leadership Coach is not valid!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user selects a program from the ddProgram control.
        /// </summary>
        /// <param name="sender">The ddProgram BootstrapComboBox</param>
        /// <param name="e">The ValueChanged event</param>
        protected void ddProgram_ValueChanged(object sender, EventArgs e)
        {
            //Get the selected values
            int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);
            string selectedLCUsername = (ddLeadershipCoach.Value != null ? ddLeadershipCoach.Value.ToString() : null);

            //Set the labels 
            SetProgramLabels(selectedProgramFK);

            //Bind the leadership coach list
            BindLeadershipCoachList(selectedProgramFK);

            //Bind the leadership coach dropdown
            BindLeadershipCoachDropDown(selectedProgramFK, selectedLCUsername);

            //Re-focus on the control
            ddProgram.Focus();
        }

        /// <summary>
        /// This method fires when the user changes the dates for the log.
        /// </summary>
        /// <param name="sender">A BootstrapDateEdit</param>
        /// <param name="e">EventArgs</param>
        protected void LogDate_ValueChanged(object sender, EventArgs e)
        {
            var currentDateEdit = (BootstrapDateEdit)sender;

            //We need to keep this since it need to re-bind the team members
            //which is done in the Page_Load method

            //Re-focus on the control
            currentDateEdit.Focus();
        }

        protected void chkIsComplete_CheckedChanged(object sender, EventArgs e)
        {
            //Set the required field values for the controls
            HandleIsComplete(chkIsComplete.Checked);
        }

        private void HandleIsComplete(bool isComplete)
        {
            //Set certain fields to required or not
            ddLeadershipCoach.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddIsMonthly.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddCyclePhase.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddTimelyProgression.ValidationSettings.RequiredField.IsRequired = isComplete;
            ddGoalCompletionLikelihood.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbDomain1.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbDomain2.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbDomain3.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbDomain4.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbDomain5.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbDomain6.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbIdentifiedProgramBarriers.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbIdentifiedProgramStrengths.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbSiteResources.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbTopicsDiscussed.ValidationSettings.RequiredField.IsRequired = isComplete;
            lbTrainingsCovered.ValidationSettings.RequiredField.IsRequired = isComplete;
            txtThinkNarrative.ValidationSettings.RequiredField.IsRequired = isComplete;
            txtActNarrative.ValidationSettings.RequiredField.IsRequired = isComplete;
        }

        /// <summary>
        /// This method returns the coach PKs selected on the form
        /// </summary>
        /// <returns>The selected coach PKs, or an empty list if none selected</returns>
        private List<int> GetSelectedCoachPKs()
        {
            //To hold the selected coach PKs
            List<int> selectedCoaches = new List<int>();

            //Loop through all the selected list box items
            foreach (ListEditItem item in lbInvolvedCoaches.SelectedItems)
            {
                //Ensure that the item is valid
                if (item != null && item.Value != null && int.TryParse(item.Value.ToString(), out int programEmployeePK))
                {
                    selectedCoaches.Add(programEmployeePK);
                }
            }

            return selectedCoaches;
        }

        /// <summary>
        /// This method returns the team member PKs selected on the form
        /// </summary>
        /// <returns>The selected team member PKs, or an empty list if none selected</returns>
        private List<int> GetSelectedTeamMemberPKs()
        {
            //Get the engaged team members
            List<int> selectedTeamMembers = new List<int>();
            if (!string.IsNullOrWhiteSpace(hfTeamMemberEngagement.Value))
            {
                selectedTeamMembers = hfTeamMemberEngagement.Value.Split(',').Select(int.Parse).ToList();
            }

            return selectedTeamMembers;
        }

        /// <summary>
        /// This method returns the log date from the form
        /// </summary>
        /// <returns>The log date</returns>
        private DateTime? GetLogDateFromForm()
        {
            DateTime? valueToReturn;

            //Determine if this is a monthly log
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            //Get the value to return
            if (isMonthly.HasValue && isMonthly.Value && deLCLMonth.Date != DateTime.MinValue)
            {
                valueToReturn = new DateTime(deLCLMonth.Date.Year, deLCLMonth.Date.Month, 1);
            }
            else if (isMonthly.HasValue && isMonthly.Value == false && deDateCompleted.Date != DateTime.MinValue)
            {
                valueToReturn = deDateCompleted.Date;
            }
            else
            {
                valueToReturn = null;
            }

            //Return the value
            return valueToReturn;
        }

        /// <summary>
        /// This method fires when the validation for the lbInvolvedCoaches DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbInvolvedCoaches ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbInvolvedCoaches_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary form information
            int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);
            List<int> selectedInvolvedCoachs = GetSelectedCoachPKs();
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);
            DateTime? logDate = GetLogDateFromForm();
            DateTime startOfWindow, endOfWindow;

            //Validate
            if (selectedInvolvedCoachs != null &&
                selectedInvolvedCoachs.Count > 0 &&
                selectedProgramFK.HasValue &&
                isMonthly.HasValue &&
                logDate.HasValue)
            {
                if (isMonthly.Value)
                {
                    startOfWindow = new DateTime(logDate.Value.Year, logDate.Value.Month, 1);
                    endOfWindow = startOfWindow.AddMonths(1).AddDays(-1);
                }
                else
                {
                    startOfWindow = logDate.Value;
                    endOfWindow = logDate.Value;
                }

                //Of the selected coaches, determine if any are invalid
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the records
                    var invalidInvolvedCoaches = context.ProgramEmployee
                                            .Include(pe => pe.Employee)
                                            .Include(pe => pe.JobFunction)
                                            .AsNoTracking()
                                            .Where(pe => selectedInvolvedCoachs.Contains(pe.ProgramEmployeePK) &&
                                                    (pe.ProgramFK != selectedProgramFK.Value ||
                                                      pe.HireDate > endOfWindow ||
                                                     (pe.TermDate.HasValue && pe.TermDate.Value < startOfWindow) ||
                                                     pe.JobFunction.Where(jf => jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.CLASSROOM_COACH &&
                                                                                            jf.StartDate <= endOfWindow &&
                                                                                            (jf.EndDate.HasValue == false ||
                                                                                                jf.EndDate.Value >= startOfWindow)).Count() == 0))
                                            .ToList();

                    //Check if any coaches are invalid
                    if (invalidInvolvedCoaches.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "At least one coach that was selected is invalid.  See the notification message for details.";

                        //Convert the invalid coach objects to a string to display to the user in a notification message
                        string invalidInvolvedCoachString = string.Join("<br/>", invalidInvolvedCoaches.Select(c => "(" + c.ProgramSpecificID + ") " + c.Employee.FirstName + " " + c.Employee.LastName).ToList());

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Involved Coach Validation Error", string.Format("The following coaches are invalid because they are either not active, or don't have an active classroom coach job function:<br/><br/> {0}", invalidInvolvedCoachString), 200000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbTeamMemberEngagement DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbTeamMemberEngagement ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbTeamMemberEngagement_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary form information
            int? selectedProgramFK = (ddProgram.Value != null ? Convert.ToInt32(ddProgram.Value) : (int?)null);
            List<int> selectedTeamMembers = GetSelectedTeamMemberPKs();
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);
            DateTime? logDate = GetLogDateFromForm();
            DateTime startOfWindow, endOfWindow;

            //Validate
            if (selectedTeamMembers != null && 
                selectedTeamMembers.Count > 0 && 
                selectedProgramFK.HasValue &&
                isMonthly.HasValue && 
                logDate.HasValue)
            {
                if (isMonthly.Value)
                {
                    startOfWindow = new DateTime(logDate.Value.Year, logDate.Value.Month, 1);
                    endOfWindow = startOfWindow.AddMonths(1).AddDays(-1);
                }
                else
                {
                    startOfWindow = logDate.Value;
                    endOfWindow = logDate.Value;
                }

                //Of the selected team members, determine if any are invalid
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the records
                    var invalidTeamMembers = context.PLTMember.AsNoTracking()
                                            .Where(pm => selectedTeamMembers.Contains(pm.PLTMemberPK) &&
                                                    (pm.ProgramFK != selectedProgramFK.Value ||
                                                      pm.StartDate > endOfWindow ||
                                                     (pm.LeaveDate.HasValue && pm.LeaveDate.Value < startOfWindow)))
                                            .ToList();

                    //Check if any coachees are invalid
                    if (invalidTeamMembers.Count > 0)
                    {
                        e.IsValid = false;
                        e.ErrorText = "At least one team member that was selected is invalid.  See the notification message for details.";

                        //Convert the invalid team member objects to a string to display to the user in a notification message
                        string invalidTeamMemberString = string.Join("<br/>", invalidTeamMembers.Select(c => "(" + c.IDNumber + ") " + c.FirstName + " " + c.LastName).ToList());

                        //Show the message
                        msgSys.ShowMessageToUser("danger", "Team Member Validation Error", string.Format("The following team members are invalid because they are not active during this log's timeframe:<br/><br/> {0}", invalidTeamMemberString), 200000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtMonthlyNumEngagements DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtMonthlyNumEngagements TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtMonthlyNumEngagements_Validation(object sender, ValidationEventArgs e)
        {
            //Determine if this is a monthly log
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            //Only validate if the form is complete and this is a monthly log
            if(chkIsComplete.Checked && isMonthly.HasValue && isMonthly.Value)
            {
                //Make sure it's a valid positive number
                if (int.TryParse(txtMonthlyNumEngagements.Text, out int numEngagements) == false || numEngagements < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "This must be a valid number and not less than zero!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtMonthlyNumAttemptedEngagements DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtMonthlyNumAttemptedEngagements TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtMonthlyNumAttemptedEngagements_Validation(object sender, ValidationEventArgs e)
        {
            //Determine if this is a monthly log
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            //Only validate if the form is complete and this is a monthly log
            if (chkIsComplete.Checked && isMonthly.HasValue && isMonthly.Value)
            {
                //Make sure it's a valid positive number
                if (int.TryParse(txtMonthlyNumAttemptedEngagements.Text, out int numAttemptedEngagements) == false || numAttemptedEngagements < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "This must be a valid number and not less than zero!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtTotalDurationHours DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTotalDurationHours TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTotalDurationHours_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary values
            int? durationHours = null, durationMinutes = null;
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            if (int.TryParse(txtTotalDurationHours.Text, out int parsedHours))
            {
                durationHours = parsedHours;
            }

            if (int.TryParse(txtTotalDurationMinutes.Text, out int parsedMinutes))
            {
                durationMinutes = parsedMinutes;
            }

            //Only validate if this is not a monthly log
            if (isMonthly.HasValue && isMonthly.Value == false)
            {
                //Validate
                if (durationHours.HasValue == false && !string.IsNullOrWhiteSpace(txtTotalDurationHours.Text))
                {
                    e.IsValid = false;
                    e.ErrorText = "If hours are entered, they must be a valid number!";
                }
                else if (durationHours.HasValue && durationHours.Value < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "Hours cannot be less than zero!";
                }
                else if (durationHours.HasValue && durationHours.Value == 0 && durationMinutes.HasValue && durationMinutes.Value == 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "Total duration cannot be zero hours and zero minutes!";
                }
                else if (chkIsComplete.Checked && string.IsNullOrWhiteSpace(txtTotalDurationHours.Text) && string.IsNullOrWhiteSpace(txtTotalDurationMinutes.Text))
                {
                    e.IsValid = false;
                    e.ErrorText = "A total duration must be entered!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtTotalDurationMinutes DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTotalDurationMinutes TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTotalDurationMinutes_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary values
            int? durationHours = null, durationMinutes = null;
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            if (int.TryParse(txtTotalDurationHours.Text, out int parsedHours))
            {
                durationHours = parsedHours;
            }

            if (int.TryParse(txtTotalDurationMinutes.Text, out int parsedMinutes))
            {
                durationMinutes = parsedMinutes;
            }

            //Only validate if this is not a monthly log
            if (isMonthly.HasValue && isMonthly.Value == false)
            {
                //Validate
                if (durationMinutes.HasValue == false && !string.IsNullOrWhiteSpace(txtTotalDurationMinutes.Text))
                {
                    e.IsValid = false;
                    e.ErrorText = "If minutes are entered, they must be a valid number!";
                }
                else if (durationMinutes.HasValue && durationMinutes.Value < 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "Minutes cannot be less than zero!";
                }
                else if (durationHours.HasValue && durationHours.Value == 0 && durationMinutes.HasValue && durationMinutes.Value == 0)
                {
                    e.IsValid = false;
                    e.ErrorText = "Total duration cannot be zero hours and zero minutes!";
                }
                else if (chkIsComplete.Checked && string.IsNullOrWhiteSpace(txtTotalDurationHours.Text) && string.IsNullOrWhiteSpace(txtTotalDurationMinutes.Text))
                {
                    e.IsValid = false;
                    e.ErrorText = "A total duration must be entered!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deDateCompleted DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deDateCompleted DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deDateCompleted_Validation(object sender, ValidationEventArgs e)
        {
            //Determine if this is a monthly log
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            //Only validate if the form is complete and this is NOT a monthly log
            if (chkIsComplete.Checked && isMonthly.HasValue && isMonthly.Value == false)
            {
                if (deDateCompleted.Date == DateTime.MinValue)
                {
                    e.IsValid = false;
                    e.ErrorText = "Date Completed is required!";
                }
                else if (deDateCompleted.Date > DateTime.Now)
                {
                    e.IsValid = false;
                    e.ErrorText = "Date Completed cannot be in the future!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the deLCLMonth DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deLCLMonth DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deLCLMonth_Validation(object sender, ValidationEventArgs e)
        {
            //Determine if this is a monthly log
            bool? isMonthly = (ddIsMonthly.Value != null ? Convert.ToBoolean(ddIsMonthly.Value) : (bool?)null);

            //Only validate if the form is complete and this is a monthly log
            if (chkIsComplete.Checked &&  isMonthly.HasValue && isMonthly.Value)
            {
                if (deLCLMonth.Date == DateTime.MinValue)
                {
                    e.IsValid = false;
                    e.ErrorText = "Month of log is required!";
                }
                else if (new DateTime(deLCLMonth.Date.Year, deLCLMonth.Date.Month, 1) > DateTime.Now)
                {
                    e.IsValid = false;
                    e.ErrorText = "Month of log cannot be in the future!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtTargetedTrainingHours DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTargetedTrainingHours TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTargetedTrainingHours_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary values
            int? trainingHours = null, trainingMinutes = null;

            if (int.TryParse(txtTargetedTrainingHours.Text, out int parsedHours))
            {
                trainingHours = parsedHours;
            }

            if (int.TryParse(txtTargetedTrainingMinutes.Text, out int parsedMinutes))
            {
                trainingMinutes = parsedMinutes;
            }

            //Validate (take form completion into account)
            if (trainingHours.HasValue == false && !string.IsNullOrWhiteSpace(txtTargetedTrainingHours.Text))
            {
                e.IsValid = false;
                e.ErrorText = "If hours are entered, they must be a valid number!";
            }
            else if (trainingHours.HasValue && trainingHours.Value < 0)
            {
                e.IsValid = false;
                e.ErrorText = "If hours are entered, they cannot be less than zero!";
            }
            else if (chkIsComplete.Checked)
            {
                //Get the 'no trainings' option for this list box
                var noTrainingsItem = lbTrainingsCovered.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.TRAININGS_COVERED_NONE);

                //Check to see if the 'no trainings' option was selected
                if (noTrainingsItem != null && noTrainingsItem.Selected)
                {
                    //If the 'no trainings' option was selected, require the training time to be blank or zero
                    if ((trainingHours.HasValue && trainingHours.Value > 0) || (trainingMinutes.HasValue && trainingMinutes.Value > 0))
                    {
                        e.IsValid = false;
                        e.ErrorText = "Because the 'No trainings covered' option was selected, the training time cannot be over zero hours and zero minutes!";
                    }
                }
                else
                {
                    //If the 'no trainings' option was not selected, require the training time
                    if ((trainingHours.HasValue == false || trainingHours.Value == 0) && (trainingMinutes.HasValue == false || trainingMinutes.Value == 0))
                    {
                        e.IsValid = false;
                        e.ErrorText = "Training time must be over zero hours and zero minutes!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtTargetedTrainingMinutes DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtTargetedTrainingMinutes TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtTargetedTrainingMinutes_Validation(object sender, ValidationEventArgs e)
        {
            //Get the necessary values
            int? trainingHours = null, trainingMinutes = null;

            if (int.TryParse(txtTargetedTrainingHours.Text, out int parsedHours))
            {
                trainingHours = parsedHours;
            }

            if (int.TryParse(txtTargetedTrainingMinutes.Text, out int parsedMinutes))
            {
                trainingMinutes = parsedMinutes;
            }

            //Validate
            if (trainingMinutes.HasValue == false && !string.IsNullOrWhiteSpace(txtTargetedTrainingMinutes.Text))
            {
                e.IsValid = false;
                e.ErrorText = "If minutes are entered, they must be a valid number!";
            }
            else if (trainingMinutes.HasValue && trainingMinutes.Value < 0)
            {
                e.IsValid = false;
                e.ErrorText = "If minutes are entered, they cannot be less than zero!";
            }
            else if (chkIsComplete.Checked)
            {
                //Get the 'no trainings' option for this list box
                var noTrainingsItem = lbTrainingsCovered.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.TRAININGS_COVERED_NONE);

                //Check to see if the 'no trainings' option was selected
                if (noTrainingsItem != null && noTrainingsItem.Selected)
                {
                    //If the 'no trainings' option was selected, require the training time to be blank or zero
                    if ((trainingHours.HasValue && trainingHours.Value > 0) || (trainingMinutes.HasValue && trainingMinutes.Value > 0))
                    {
                        e.IsValid = false;
                        e.ErrorText = "Because the 'No trainings covered' option was selected, the training time cannot be over zero hours and zero minutes!";
                    }
                }
                else
                {
                    //If the 'no trainings' option was not selected, require the training time
                    if ((trainingHours.HasValue == false || trainingHours.Value == 0) && (trainingMinutes.HasValue == false || trainingMinutes.Value == 0))
                    {
                        e.IsValid = false;
                        e.ErrorText = "Training time must be over zero hours and zero minutes!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtOtherDomainTwoSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtOtherDomainTwoSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtOtherDomainTwoSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if the form is complete
            if (chkIsComplete.Checked)
            {
                //Get the 'other' option for this list box
                var otherOptionItem = lbDomain2.Items.FindByValue(CodeLCLResponse.OtherSpecifyPKs.DOMAIN_2_SPECIFY);

                //Check to see if the 'other' option was selected
                if (otherOptionItem != null && otherOptionItem.Selected)
                {
                    if (string.IsNullOrWhiteSpace(txtOtherDomainTwoSpecify.Text))
                    {
                        e.IsValid = false;
                        e.ErrorText = "This is required because you selected an option above that requires more details!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtOtherSiteResourcesSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtOtherSiteResourcesSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtOtherSiteResourcesSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if the form is complete
            if (chkIsComplete.Checked)
            {
                //Get the 'other' option for this list box
                var otherOptionItem = lbSiteResources.Items.FindByValue(CodeLCLResponse.OtherSpecifyPKs.SITE_RESOURCES_SPECIFY);

                //Check to see if the 'other' option was selected
                if (otherOptionItem != null && otherOptionItem.Selected)
                {
                    if (string.IsNullOrWhiteSpace(txtOtherSiteResourcesSpecify.Text))
                    {
                        e.IsValid = false;
                        e.ErrorText = "This is required because you selected an option above that requires more details!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtOtherTrainingsCoveredSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtOtherTrainingsCoveredSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtOtherTrainingsCoveredSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if the form is complete
            if (chkIsComplete.Checked)
            {
                //Get the 'other' option for this list box
                var otherOptionItem = lbTrainingsCovered.Items.FindByValue(CodeLCLResponse.OtherSpecifyPKs.TRAININGS_COVERED_SPECIFY);

                //Check to see if the 'other' option was selected
                if (otherOptionItem != null && otherOptionItem.Selected)
                {
                    if (string.IsNullOrWhiteSpace(txtOtherTrainingsCoveredSpecify.Text))
                    {
                        e.IsValid = false;
                        e.ErrorText = "This is required because you selected an option above that requires more details!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtOtherTopicsDiscussedSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtOtherTopicsDiscussedSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtOtherTopicsDiscussedSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Only validate if the form is complete
            if (chkIsComplete.Checked)
            {
                //Get the 'other' option for this list box
                var otherOptionItem = lbTopicsDiscussed.Items.FindByValue(CodeLCLResponse.OtherSpecifyPKs.TOPICS_DISCUSSED_SPECIFY);

                //Check to see if the 'other' option was selected
                if (otherOptionItem != null && otherOptionItem.Selected)
                {
                    if (string.IsNullOrWhiteSpace(txtOtherTopicsDiscussedSpecify.Text))
                    {
                        e.IsValid = false;
                        e.ErrorText = "This is required because you selected an option above that requires more details!";
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbDomain1 DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbDomain1 ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbDomain1_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbDomain1.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.DOMAIN_1_NONE_PROVIDED);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbDomain1.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None provided' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbDomain2 DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbDomain2 ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbDomain2_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbDomain2.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.DOMAIN_2_NONE_PROVIDED);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbDomain2.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None provided' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbDomain3 DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbDomain3 ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbDomain3_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbDomain3.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.DOMAIN_3_NONE_PROVIDED);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbDomain3.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None provided' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbDomain4 DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbDomain4 ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbDomain4_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbDomain4.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.DOMAIN_4_NONE_PROVIDED);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbDomain4.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None provided' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbDomain5 DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbDomain5 ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbDomain5_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbDomain5.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.DOMAIN_5_NONE_PROVIDED);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbDomain5.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None provided' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbDomain6 DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbDomain6 ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbDomain6_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbDomain6.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.DOMAIN_6_NONE_PROVIDED);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbDomain6.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None provided' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbTopicsDiscussed DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbTopicsDiscussed ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbTopicsDiscussed_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbTopicsDiscussed.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.TOPICS_DISCUSSED_NONE);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbTopicsDiscussed.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None of the above' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbTrainingsCovered DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbTrainingsCovered ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbTrainingsCovered_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbTrainingsCovered.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.TRAININGS_COVERED_NONE);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbTrainingsCovered.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'No trainings covered' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbSiteResources DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbSiteResources ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbSiteResources_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem1 = lbSiteResources.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.SITE_RESOURCES_NONE_NO_PLAN);
            var noneOptionItem2 = lbSiteResources.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.SITE_RESOURCES_NONE_EFFORTS_UNDERWAY);

            //Check to see if the 'none' option was selected
            if ((noneOptionItem1 != null && noneOptionItem1.Selected) ||
                (noneOptionItem2 != null && noneOptionItem2.Selected))
            {
                //Don't allow other items to be selected
                if (lbSiteResources.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "One of the 'No resources in place' options was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbIdentifiedProgramStrengths DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbIdentifiedProgramStrengths ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbIdentifiedProgramStrengths_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbIdentifiedProgramStrengths.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.PROGRAM_STRENGTHS_NONE);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbIdentifiedProgramStrengths.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'No strengths identified' option was selected, no other options are allowed to be selected!";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the lbIdentifiedProgramBarriers DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lbIdentifiedProgramBarriers ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lbIdentifiedProgramBarriers_Validation(object sender, ValidationEventArgs e)
        {
            //Get the 'none' option for this list box
            var noneOptionItem = lbIdentifiedProgramBarriers.Items.FindByValue(CodeLCLResponse.NoOtherItemsPKs.PROGRAM_BARRIERS_NONE);

            //Check to see if the 'none' option was selected
            if (noneOptionItem != null && noneOptionItem.Selected)
            {
                //Don't allow other items to be selected
                if (lbIdentifiedProgramBarriers.SelectedItems.Count > 1)
                {
                    e.IsValid = false;
                    e.ErrorText = "The 'None barriers identified' option was selected, no other options are allowed to be selected!";
                }
            }
        }
    }
}