using System;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.EMMA;
using System.Web.UI.HtmlControls;
using System.Configuration;
using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Pyramid
{
    public partial class _Default : System.Web.UI.Page
    {
        private List<CodeProgramRolePermission> currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;
        private StateSettings currentStateSettings;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the user permissions
            currentPermissions = Utilities.GetProgramRolePermissionsFromDatabase(currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the state settings
            currentStateSettings = StateSettings.GetStateSettingsWithDefault(currentProgramRole.CurrentStateFK.Value);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Set the sqldatasource connection string
            sqlFormsDueDataSource.ConnectionString = ConfigurationManager.ConnectionStrings["Pyramid"].ConnectionString;

            //If the due dates are enabled, use them to fill the help message hidden fields
            if (currentStateSettings.DueDatesEnabled)
            {
                hfFormsDueMonthsOverdue.Value = (-currentStateSettings.DueDatesMonthsStart.Value).ToString();
                hfFormsDueMonthsUpcoming.Value = currentStateSettings.DueDatesMonthsEnd.Value.ToString();
                hfFormsDueDaysUntilWarning.Value = currentStateSettings.DueDatesDaysUntilWarning.Value.ToString();
            }

            if (!IsPostBack)
            {
                //Hide the alert div if the state is not NY
                if (currentProgramRole.CurrentStateFK.Value != (int)Utilities.StateFKs.NEW_YORK)
                {
                    divAccountUpdateAlert.Visible = false;
                }
                else
                {
                    //Check to see if date is in session
                    if (Session["UserLastUpdateDate"] == null)
                    {
                        //Store the update time if it's not in session
                        string userName = User.Identity.Name;

                        using (ApplicationDbContext acontext = new ApplicationDbContext())
                        {
                            //Get the last update time
                            var userRecord = acontext.Users.AsNoTracking()
                                .Where(u => u.UserName == userName)
                                .FirstOrDefault();

                            //Set the session variable
                            if (userRecord.CreateTime < DateTime.Now.AddMonths(-6))
                            {
                                //If the user was created over six months ago, use the updatetime
                                Session["UserLastUpdateDate"] = (userRecord.UpdateTime.HasValue ? userRecord.UpdateTime.Value.ToString() : DateTime.MinValue.ToString());
                            }
                            else
                            {
                                //If the user was created less than six months ago, use today's date
                                Session["UserLastUpdateDate"] = DateTime.Now.ToString();
                            }
                        }
                    }

                    //Try to convert the date from session. If TryParse returns false then the div remains shown since the user has no UpdateTime.
                    DateTime updateTime;
                    if (Session["UserLastUpdateDate"] != null && DateTime.TryParse(Session["UserLastUpdateDate"].ToString(), out updateTime) == true)
                    {
                        //If the update time is within 6 months hide the alert
                        if (DateTime.Today.AddMonths(-6) < updateTime)
                        {
                            divAccountUpdateAlert.Visible = false;
                        }
                    }
                }

                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Check to see if there are any messages
                if (Request.QueryString["messageType"] != null)
                {
                    //Get the message type
                    string messageType = Request.QueryString["messageType"].ToString();

                    //Get the message to display
                    switch (messageType)
                    {
                        case "PageNotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to access that part of the system!", 20000);
                            break;
                        case "TwoFactorVerified":
                            msgSys.ShowMessageToUser("success", "Two-Factor Code Verified", "Your Two-Factor code was successfully verified!", 5000);
                            break;
                        case "LinkNotFound":
                            msgSys.ShowMessageToUser("warning", "Link Not Found", "The system could not recognize the link, please try again.  If this continues, please contact support with a support ticket.", 10000);
                            break;
                    }
                }

                //Show or hide the fireworks
                ShowHideFireworks();
            }

            //Get the last month and mind the news
            DateTime lastMonth = DateTime.Now.AddDays(-30);
            BindNews(lastMonth);

            //Set the logos
            bsImgLargeLogo.ImageUrl = string.Format("/Content/images/{0}", (currentProgramRole.StateLogoFileName == null ? "GenericLogo.png" : currentProgramRole.StateLogoFileName));
            bsImgLargeThumbnailLogo.ImageUrl = string.Format("/Content/images/{0}", (currentProgramRole.StateLogoFileName == null ? "CustomPIDSLogoSquare.png" : currentProgramRole.StateThumbnailLogoFileName));

            //Set the display of the logos based on the option in the state row
            switch (currentProgramRole.StateHomePageLogoOption)
            {
                case (int)State.HomePageLogoOptionValues.DISPLAY_BOTH_LOGOS:
                    bsImgLargeLogo.Visible = true;
                    bsImgLargeThumbnailLogo.Visible = true;
                    break;
                case (int)State.HomePageLogoOptionValues.DISPLAY_ONLY_STATE_LOGO:
                    bsImgLargeLogo.Visible = true;
                    bsImgLargeThumbnailLogo.Visible = false;
                    break;
                case (int)State.HomePageLogoOptionValues.DISPLAY_ONLY_THUMBNAIL_LOGO:
                    bsImgLargeLogo.Visible = false;
                    bsImgLargeThumbnailLogo.Visible = true;
                    break;
                default:
                    bsImgLargeLogo.Visible = true;
                    bsImgLargeThumbnailLogo.Visible = true;
                    break;
            }
        }

        #region News

        /// <summary>
        /// This method populates the news div with the news that is after the limit date
        /// passed to this method
        /// </summary>
        /// <param name="limitDate">A limiting DateTime</param>
        private void BindNews(DateTime limitDate)
        {
            //Get the permission object
            CodeProgramRolePermission newsPermissions = currentPermissions.Where(cp => cp.CodeForm.FormAbbreviation == "NEWS").FirstOrDefault();

            //Only display news if the user is authorized
            if (newsPermissions.AllowedToViewDashboard && newsPermissions.AllowedToView)
            {
                //To hold the news
                List<NewsEntry> newsEntries;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the news
                    if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
                    {
                        //Get all the news entries after the limit date
                        newsEntries = context.NewsEntry.AsNoTracking()
                                        .Include(ne => ne.NewsItem)
                                        .Include(ne => ne.CodeNewsEntryType)
                                        .Include(ne => ne.Program)
                                        .Where(ne => ne.EntryDate >= limitDate)
                                        .OrderByDescending(ne => ne.EntryDate)
                                        .ThenByDescending(ne => ne.CreateDate)
                                        .ToList();
                    }
                    else if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                             currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER)
                    {
                        //Get only the application news entries
                        newsEntries = context.NewsEntry.AsNoTracking()
                                        .Include(ne => ne.NewsItem)
                                        .Include(ne => ne.CodeNewsEntryType)
                                        .Where(ne => (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.APPLICATION)
                                                && ne.EntryDate >= limitDate
                                                && ne.EntryDate <= DateTime.Now)
                                        .OrderByDescending(ne => ne.EntryDate)
                                        .ThenByDescending(ne => ne.CreateDate)
                                        .ToList();
                    }
                    else
                    {
                        //Get the news entries that apply to this user's role (app, state, hub, and program)
                        newsEntries = context.NewsEntry.AsNoTracking()
                                        .Include(ne => ne.NewsItem)
                                        .Include(ne => ne.CodeNewsEntryType)
                                        .Where(ne => ((ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.PROGRAM_WIDE
                                                            && currentProgramRole.ProgramFKs.Contains(ne.ProgramFK.Value))
                                                        || ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.APPLICATION
                                                        || (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.STATE_WIDE
                                                            && currentProgramRole.StateFKs.Contains(ne.StateFK.Value))
                                                        || (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.HUB_WIDE
                                                            && currentProgramRole.HubFKs.Contains(ne.HubFK.Value))
                                                        || (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.COHORT_WIDE
                                                            && currentProgramRole.CohortFKs.Contains(ne.CohortFK.Value)))
                                                && ne.EntryDate >= limitDate
                                                && ne.EntryDate <= DateTime.Now)
                                        .OrderByDescending(ne => ne.EntryDate)
                                        .ThenByDescending(ne => ne.CreateDate)
                                        .ToList();
                    }
                }

                //To hold the news HTML
                StringBuilder newsHTML = new StringBuilder();

                //Make sure there is news to show
                if (newsEntries.Count > 0)
                {
                    //There is news to show
                    //Loop through each news entry
                    foreach (NewsEntry entry in newsEntries)
                    {
                        //Add HTML for the entry
                        newsHTML.Append("<div class='news-entry'><label class='news-entry-date'>" + entry.EntryDate.ToString("MM/dd/yyyy") + "</label> (" + entry.CodeNewsEntryType.Description + ")");
                        newsHTML.Append("<ul class='news-list'>");

                        //Loop through each news item in the entry
                        foreach (Models.NewsItem item in entry.NewsItem.OrderBy(ni => ni.ItemNum))
                        {
                            //Add HTML for the item
                            newsHTML.Append("<li>" + Server.HtmlEncode(item.Contents) + "</li>");
                        }

                        //Close the HTML tags
                        newsHTML.Append("</ul></div>");
                    }
                }
                else
                {
                    //There was no news to show
                    newsHTML.Append("No news found...");
                }

                //Display the news
                ltlNews.Text = newsHTML.ToString();
            }
            else
            {
                ltlNews.Text = "Not authorized to view...";
            }
        }

        #endregion

        #region Welcome Section

        /// <summary>
        /// This method sets the welcome message visibility
        /// </summary>
        private void SetWelcomeMessageVisiblity()
        {
            //Check to see if the invalid forms are visible or the dashboard is visible
            if (IsDashboardVisible() || AreInvalidFormsVisible())
            {
                //Get the customization option for the welcome message
                bool? showMessage = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.WELCOME_MESSAGE_OPTION);

                //Check to see if the customization option has value
                if (showMessage.HasValue && showMessage.Value == true)
                {
                    //Show the welcome message
                    divWelcomeSection.Visible = true;
                }
                else
                {
                    //Hide the welcome message
                    divWelcomeSection.Visible = false;
                }
            }
            else
            {
                //Show the welcome section
                divWelcomeSection.Visible = true;
            }
        }

        /// <summary>
        /// This method shows or hides the fireworks based on the user's preferences
        /// </summary>
        private void ShowHideFireworks()
        {
            //Fireworks!
            //Get the customization option for fireworks
            bool? fireworksAllowed = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.FIREWORKS_OPTION);

            //Only allow fireworks if the user has them enabled
            if (fireworksAllowed.HasValue && fireworksAllowed.Value == true)
            {
                //Get the fireworks cookie
                string fireworksCookieValue = Utilities.GetCookieSection("defaultPageFireworks", "enabled");
                bool? fireworksEnabled = (string.IsNullOrWhiteSpace(fireworksCookieValue) ? (bool?)null : Convert.ToBoolean(fireworksCookieValue));

                //Display or hide the fireworks
                if (fireworksEnabled.HasValue)
                {
                    DisplayFireworks(fireworksEnabled.Value);
                }
                else
                {
                    DisplayFireworks(true);
                }
            }
            else
            {
                //Disable the fireworks
                DisplayFireworks(false);

                //Hide the fireworks buttons
                lbEnableFireworks.Visible = false;
                lbDisableFireworks.Visible = false;
            }
        }

        /// <summary>
        /// This method controls the fireworks display
        /// </summary>
        /// <param name="display">True to display fireworks, false otherwise</param>
        private void DisplayFireworks(bool display)
        {
            //Show the right button
            lbEnableFireworks.Visible = !display;
            lbDisableFireworks.Visible = display;

            //Display or hide the fireworks
            if (display)
            {
                divFireworks.Attributes.Add("class", "pyro");
            }
            else
            {
                divFireworks.Attributes.Remove("class");
            }
        }

        /// <summary>
        /// This method fires when the user clicks the enable fireworks button
        /// and displays the fireworks
        /// </summary>
        /// <param name="sender">The lbEnableFireworks LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEnableFireworks_Click(object sender, EventArgs e)
        {
            //Set the fireworks cookie
            Utilities.SetCookieSection("defaultPageFireworks", "enabled", "true", 365);

            //Display the fireworks
            DisplayFireworks(true);
        }

        /// <summary>
        /// This method fires when the user clicks the disable fireworks button
        /// and hides the fireworks
        /// </summary>
        /// <param name="sender">The lbDisableFireworks LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDisableFireworks_Click(object sender, EventArgs e)
        {
            //Set the fireworks cookie
            Utilities.SetCookieSection("defaultPageFireworks", "enabled", "false", 365);

            //Hide the fireworks
            DisplayFireworks(false);
        }

        #endregion

        #region Invalid Forms

        /// <summary>
        /// This method binds the invalid forms repeater and sets its visibility
        /// </summary>
        /// <returns>True if there are invalid forms to show, false otherwise</returns>
        private bool BindInvalidForms()
        {
            //To hold whether or not the invalid forms div is shown
            bool showInvalidFormsDiv = false;

            //Check to see if the user is a program role
            if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.DATA_COLLECTOR ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.AGGREGATE_DATA_VIEWER ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.DETAIL_DATA_VIEWER ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.CLASSROOM_COACH_DATA_COLLECTOR)
            {
                //Set the warning message
                if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.DATA_COLLECTOR)
                {
                    lblInvalidFormsWarning.Text = "Some of the forms entered are invalid, please fix the following validation failures:";
                }
                else
                {
                    lblInvalidFormsWarning.Text = "Some of the forms entered are invalid, please have your program's data collector fix the following validation failures:";
                }

                //Fill the invalid forms repeater if there are invalid forms for the program
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the invalid forms for the program
                    List<rspInvalidForms_Result> invalidForms = context.rspInvalidForms(currentProgramRole.ViewPrivateChildInfo,
                                                                        currentProgramRole.ViewPrivateEmployeeInfo,
                                                                        currentProgramRole.CurrentProgramFK.Value.ToString(),
                                                                        null,
                                                                        null,
                                                                        null).ToList();

                    //Populate the invalid forms repeater
                    repeatInvalidForms.DataSource = invalidForms;
                    repeatInvalidForms.DataBind();

                    //Check to see if there are any invalid forms
                    if (invalidForms.Count > 0)
                    {
                        //Show the invalid forms
                        showInvalidFormsDiv = true;
                    }
                    else
                    {
                        //Hide the invalid forms div
                        showInvalidFormsDiv = false;
                    }
                }
            }
            else
            {
                //Hide the invalid forms div
                showInvalidFormsDiv = false;
            }

            //Return the boolean
            return showInvalidFormsDiv;
        }

        /// <summary>
        /// This method fires when an invalid form item is databound and
        /// it sets the links for editing and viewing
        /// </summary>
        /// <param name="sender">The repeatInvalidForms repeater</param>
        /// <param name="e">The RepeaterItemEventArgs event</param>
        protected void repeatInvalidForms_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            //Make sure this is a data row
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //Get the invalid form values
                rspInvalidForms_Result invalidForm = (rspInvalidForms_Result)e.Item.DataItem;

                //Get the correct permissions object
                CodeProgramRolePermission formPermissions = currentPermissions.Where(cp => cp.CodeForm.FormAbbreviation == invalidForm.ObjectAbbrevation).FirstOrDefault();

                //Get the HyperLinks in the repeater
                HyperLink lnkViewInvalidForm = (HyperLink)e.Item.FindControl("lnkViewInvalidForm");
                HyperLink lnkEditInvalidForm = (HyperLink)e.Item.FindControl("lnkEditInvalidForm");
                HyperLink lnkViewInvalidEmployee = (HyperLink)e.Item.FindControl("lnkViewInvalidEmployee");
                HyperLink lnkEditInvalidEmployee = (HyperLink)e.Item.FindControl("lnkEditInvalidEmployee");

                //Get the dropdown divider and actions dropdown
                HtmlGenericControl divDropdownDivider = (HtmlGenericControl)e.Item.FindControl("divDropdownDivider");
                HtmlGenericControl divActionDropdown = (HtmlGenericControl)e.Item.FindControl("divActionDropdown");

                //Set visibility of the action dropdown
                if (formPermissions.AllowedToView == false)
                {
                    divActionDropdown.Visible = false;
                }
                else
                {
                    //Whether or not show the edit links
                    bool showEditLinks = formPermissions.AllowedToEdit;

                    //Set visibility of the edit form link
                    lnkEditInvalidForm.Visible = showEditLinks;

                    //Get the link to the form page and employee page
                    string formViewLink = Utilities.GetLinkToForm(invalidForm.ObjectAbbrevation, invalidForm.ObjectPK, "View");
                    string formEditLink = Utilities.GetLinkToForm(invalidForm.ObjectAbbrevation, invalidForm.ObjectPK, "Edit");

                    //Get links to the employee page if there is a related invalid employee
                    if (invalidForm.InvalidProgramEmployeePK.HasValue)
                    {
                        //Get the employee links
                        string employeeViewLink = Utilities.GetLinkToForm("PE", invalidForm.InvalidProgramEmployeePK.Value, "View");
                        string employeeEditLink = Utilities.GetLinkToForm("PE", invalidForm.InvalidProgramEmployeePK.Value, "Edit");

                        //Set the employee link URLs
                        lnkViewInvalidEmployee.NavigateUrl = employeeViewLink;
                        lnkEditInvalidEmployee.NavigateUrl = employeeEditLink;

                        //Show the employee links and divider
                        lnkViewInvalidEmployee.Visible = true;
                        lnkEditInvalidEmployee.Visible = showEditLinks;
                        divDropdownDivider.Visible = true;
                    }
                    else
                    {
                        //Hide the employee links and divider
                        lnkViewInvalidEmployee.Visible = false;
                        lnkEditInvalidEmployee.Visible = false;
                        divDropdownDivider.Visible = false;
                    }

                    //Set the HyperLink URLs
                    lnkViewInvalidForm.NavigateUrl = formViewLink;
                    lnkEditInvalidForm.NavigateUrl = formEditLink;
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the btnRefreshInvalidForms
        /// DevExpress button and it rebinds the invalid forms list
        /// </summary>
        /// <param name="sender">The btnRefreshInvalidForms DevExpress button</param>
        /// <param name="e">The Click event arguments</param>
        protected void btnRefreshInvalidForms_Click(object sender, EventArgs e)
        {
            //Rebind the invalid forms
            BindInvalidForms();

            //Show a message
            msgSys.ShowMessageToUser("success", "Forms Refreshed", "Invalid forms have been refreshed!", 5000);
        }

        /// <summary>
        /// This method determines if the invalid forms are visible
        /// </summary>
        /// <returns>True if the invalid forms are visible, false otherwise</returns>
        private bool AreInvalidFormsVisible()
        {
            //Return the invalid form div visibility
            return divInvalidForms.Visible;
        }

        #endregion

        #region Forms Due

        /// <summary>
        /// This method binds the forms due gridview's data source and sets the forms due div visibility
        /// </summary>
        /// <returns>True if the section should display, false otherwise</returns>
        private bool BindFormsDue()
        {
            //Whether or not to show the forms due section
            bool showDueDates;

            //Check to see if the due dates are enabled at a state level and the user is in a program-level role
            if ((currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.DATA_COLLECTOR ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.AGGREGATE_DATA_VIEWER ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.DETAIL_DATA_VIEWER ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.CLASSROOM_COACH_DATA_COLLECTOR ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH) &&
                currentStateSettings.DueDatesEnabled &&
                DateTime.Now >= currentStateSettings.DueDatesBeginDate)
            {
                //Allow select on null parameter (is set to true on .aspx page so that it doesn't select on initial page load)
                sqlFormsDueDataSource.CancelSelectOnNullParameter = false;

                //Get the start and end window
                int startDaysToAdd = Convert.ToInt32(currentStateSettings.DueDatesMonthsStart.Value * 30.417m);
                int endDaysToAdd = Convert.ToInt32(currentStateSettings.DueDatesMonthsEnd.Value * 30.417m);

                //Get the start and end dates
                DateTime startDate = DateTime.Now.AddDays(startDaysToAdd), endDate = DateTime.Now.AddDays(endDaysToAdd);

                //Check the start date against the due date begin date
                if (startDate <= currentStateSettings.DueDatesBeginDate.Value)
                {
                    startDate = currentStateSettings.DueDatesBeginDate.Value;
                }

                //Set the data source parameters
                sqlFormsDueDataSource.SelectParameters["ProgramFKs"].DefaultValue = currentProgramRole.CurrentProgramFK.Value.ToString();
                sqlFormsDueDataSource.SelectParameters["StartDate"].DefaultValue = startDate.ToString();
                sqlFormsDueDataSource.SelectParameters["EndDate"].DefaultValue = endDate.ToString();

                //Set the display boolean
                showDueDates = true;
            }
            else
            {
                //Set the display boolean
                showDueDates = false;
            }

            //Return the display boolean
            return showDueDates;
        }

        /// <summary>
        /// This method fires every time a row in the bsGRFormsDue BootstrapGridView is created and
        /// it set the row class and, if applicable, the completed form link.
        /// </summary>
        /// <param name="sender">The row in the bsGRFormsDue BootstrapGridView</param>
        /// <param name="e">The row event</param>
        protected void bsGRFormsDue_HtmlRowCreated(object sender, DevExpress.Web.ASPxGridViewTableRowEventArgs e)
        {
            //Check to see if the row is a data row
            if (e.RowType == GridViewRowType.Data)
            {
                //Get the necessary values from the row
                bool isCompleted = Convert.ToBoolean(e.GetValue("IsComplete"));
                DateTime dueDate = Convert.ToDateTime(e.GetValue("DueRecommendedDate"));
                string formAbbreviation = Convert.ToString(e.GetValue("FormAbbreviation"));
                int daysUntilWarning = Convert.ToInt32(e.GetValue("DaysUntilWarning"));
                var objFormDate = e.GetValue("FormDate");
                DateTime? formDate = (objFormDate == DBNull.Value || objFormDate == null ? (DateTime?)null : Convert.ToDateTime(objFormDate));
                var objFormFK = e.GetValue("FormFK");
                int? formFK = (objFormFK == DBNull.Value || objFormFK == null ? (int?)null : Convert.ToInt32(objFormFK));

                //Get the form date label and completed form link
                Label lblFormInfo = (Label)bsGRFormsDue.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRFormsDue.Columns["CompletedFormColumn"], "lblFormInfo");
                BootstrapHyperLink lnkCompletedForm = (BootstrapHyperLink)bsGRFormsDue.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRFormsDue.Columns["CompletedFormColumn"], "lnkCompletedForm");

                //Get the correct permissions object
                CodeProgramRolePermission formPermissions = currentPermissions.Where(cp => cp.CodeForm.FormAbbreviation == formAbbreviation).FirstOrDefault();

                //Set the row color
                if (isCompleted)
                {
                    e.Row.CssClass = "alert-success";
                }
                else if ((dueDate - DateTime.Now).TotalDays > daysUntilWarning)
                {
                    e.Row.CssClass = "alert-info";
                }
                else if ((dueDate - DateTime.Now).TotalDays >= 0)
                {
                    e.Row.CssClass = "alert-warning";
                }
                else
                {
                    e.Row.CssClass = "alert-danger";
                }

                //Check to see if the completed form link and form date label exist
                if (lnkCompletedForm != null && lblFormInfo != null)
                {
                    //If the form FK has a value and the user is allowed, set the completed form link URL.  
                    //Otherwise, hide the link and show the date.
                    if (formFK.HasValue && formPermissions.AllowedToView)
                    {
                        //Set the link URL
                        lnkCompletedForm.NavigateUrl = Utilities.GetLinkToForm(formAbbreviation, formFK.Value, "View");

                        //Set the control visibility
                        lnkCompletedForm.Visible = true;
                        lblFormInfo.Visible = false;
                    }
                    else if (formDate.HasValue)
                    {
                        //Set the form info label text
                        lblFormInfo.Text = formDate.Value.ToString("MM/dd/yyyy");

                        //Set the control visibility
                        lnkCompletedForm.Visible = false;
                        lblFormInfo.Visible = true;
                    }
                    else
                    {
                        //Set the form info label text
                        lblFormInfo.Text = "Not Completed";

                        //Set the control visibility
                        lnkCompletedForm.Visible = false;
                        lblFormInfo.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the refresh button for the forms due
        /// table and it refreshes it.
        /// </summary>
        /// <param name="sender">The btnRefreshFormsDue BootstrapButton</param>
        /// <param name="e">The click event</param>
        protected void btnRefreshFormsDue_Click(object sender, EventArgs e)
        {
            //Bind the forms due list
            BindFormsDue();

            //Show a message
            msgSys.ShowMessageToUser("success", "Forms Refreshed", "Forms that are due have been refreshed!", 5000);
        }

        #endregion

        #region Dashboards

        /// <summary>
        /// This method fires when the btnRefreshDashboard button is clicked and
        /// it refreshes the dashboard items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRefreshDashboard_Click(object sender, EventArgs e)
        {
            //Refresh the dashboard
            RefreshDashboard();
        }

        /// <summary>
        /// This method refreshes all the dashboard items
        /// </summary>
        private void RefreshDashboard()
        {
            //Bind the forms due and set the div visibility
            divFormsDue.Visible = BindFormsDue();
            bsGRFormsDue.Visible = divFormsDue.Visible; //Need to set the gridview visibility as well (causes JS error if you don't)

            //Bind the invalid forms and set the div visibility
            divInvalidForms.Visible = BindInvalidForms();

            //Set the welcome message visibility
            SetWelcomeMessageVisiblity();
        }

        /// <summary>
        /// This method determines whether or not there is a dashboard visible
        /// </summary>
        /// <returns>True if there are dashboard items visible, false otherwise</returns>
        private bool IsDashboardVisible()
        {
            //Return the item visibility
            return divFormsDue.Visible;
        }

        #endregion
    }
}