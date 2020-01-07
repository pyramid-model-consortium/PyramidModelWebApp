using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;
using System.Data;

namespace Pyramid.Pages
{
    public partial class NewsManagement : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.NewsEntry currentNewsEntry;
        private int currentNewsEntryPK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the News Entry PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["NewsEntryPK"]))
            {
                int.TryParse(Request.QueryString["NewsEntryPK"], out currentNewsEntryPK);
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the News Entry from the database
                currentNewsEntry = context.NewsEntry.AsNoTracking()
                                        .Include(ne => ne.CodeNewsEntryType)
                                        .Where(ne => ne.NewsEntryPK == currentNewsEntryPK).FirstOrDefault();

                //Check to see if the News Entry from the database exists
                if (currentNewsEntry == null)
                {
                    //The NewsEntry from the database doesn't exist, set the current News Entry to a default value
                    currentNewsEntry = new Models.NewsEntry();
                }
            }

            //Prevent users from viewing entries from other programs
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.DATA_COLLECTOR ||
                (currentNewsEntry.NewsEntryPK > 0 
                    && ((currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.PROGRAM_WIDE
                            && !currentProgramRole.ProgramFKs.Contains(currentNewsEntry.ProgramFK.Value))
                        || (currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.STATE_WIDE
                            && currentProgramRole.StateFK.Value != currentNewsEntry.StateFK.Value)
                        || (currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.HUB_WIDE
                            && currentProgramRole.HubFK.Value != currentNewsEntry.HubFK.Value)
                        || (currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.COHORT_WIDE
                            && !currentProgramRole.CohortFKs.Contains(currentNewsEntry.CohortFK.Value))
                        || !currentNewsEntry.CodeNewsEntryType.RolesAuthorizedToModify.Contains(currentProgramRole.RoleFK.Value.ToString() + ","))))
            {
                Response.Redirect(string.Format("/Pages/News.aspx?messageType={0}", "NotAuthorized"));
            }

            //Show certain divs based on whether this is an add or edit
            if (currentNewsEntryPK > 0)
            {
                divAddOnlyMessage.Visible = false;
                divEditOnly.Visible = true;
            }
            else
            {
                divAddOnlyMessage.Visible = true;
                divEditOnly.Visible = false;
            }

            //Show the edit only div if this is an edit
            divEditOnly.Visible = (currentNewsEntryPK > 0 ? true : false);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((LoggedIn)this.Master).HideTitle();

                //Bind the data bound controls
                BindDataBoundControls();

                //Check to see if this is an edit
                if (currentNewsEntryPK > 0)
                {
                    //This is an edit
                    //Populate the page
                    PopulatePage(currentNewsEntry);
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

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {

                    switch (messageType)
                    {
                        case "NewsEntryAdded":
                            msgSys.ShowMessageToUser("success", "Success", "News entry successfully added!<br/><br/>Specific items can now be added.", 10000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Allow adding/editing depending on the user's role and the action
                if (currentNewsEntry.NewsEntryPK == 0 
                        && (currentProgramRole.AllowedToEdit.Value 
                                || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER))
                {
                    //Show the submit button
                    submitNewsEntry.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New News Entry";
                }
                else if (currentNewsEntry.NewsEntryPK > 0 
                            && action.ToLower() == "edit" 
                            && (currentProgramRole.AllowedToEdit.Value
                                || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER))
                {
                    //Show the submit button
                    submitNewsEntry.ShowSubmitButton = true;

                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit News Entry";
                }
                else
                {
                    //Hide the submit button
                    submitNewsEntry.ShowSubmitButton = false;

                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View News Entry";
                }

                //Set the focus to the news entry date field
                deEntryDate.Focus();
            }
        }

        #region Click Methods

        /// <summary>
        /// This method fires when the user clicks the edit button for a news item
        /// and it allows the user to edit that news item.
        /// </summary>
        /// <param name="sender">The lbEditNewsItem LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditNewsItem_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)editButton.Parent;

            //Get the hidden field with the PK for editing
            HiddenField hfNewsItemPK = (HiddenField)item.FindControl("hfNewsItemPK");

            //Get the PK from the hidden field
            int? itemPK = (String.IsNullOrWhiteSpace(hfNewsItemPK.Value) ? (int?)null : Convert.ToInt32(hfNewsItemPK.Value));

            if (itemPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the note to edit
                    NewsItem editNewsItem = context.NewsItem.AsNoTracking().Where(ni => ni.NewsItemPK == itemPK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditNewsItem.Text = "Edit News Item";
                    txtItemNum.Value = editNewsItem.ItemNum;
                    txtNewsItemContents.Value = editNewsItem.Contents;
                    hfAddEditNewsItemPK.Value = itemPK.Value.ToString();
                }

                //Show the note div
                divAddEditNewsItem.Visible = true;

                //Set focus to the order number field
                txtItemNum.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected note!", 20000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the add button for the news items
        /// and it allows the user to enter a new news item
        /// </summary>
        /// <param name="sender">The lbAddNewsItem LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddNewsItem_Click(object sender, EventArgs e)
        {
            //Clear inputs in the item div
            hfAddEditNewsItemPK.Value = "0";
            txtItemNum.Value = "";
            txtNewsItemContents.Value = "";

            //Set the title
            lblAddEditNewsItem.Text = "Add News Item";

            //Show the item div
            divAddEditNewsItem.Visible = true;

            //Set focus to the order number field
            txtItemNum.Focus();
        }

        /// <summary>
        /// This method fires when the user clicks the delete button for a news item
        /// and it removes the news item from the database
        /// </summary>
        /// <param name="sender">The lbDeleteNewsItem LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteNewsItem_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteNewsItemPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteNewsItemPK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK != null)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the note to remove
                        NewsItem newsItemToRemove = context.NewsItem.Where(ni => ni.NewsItemPK == rowToRemovePK).FirstOrDefault();

                        //Remove the note
                        context.NewsItem.Remove(newsItemToRemove);
                        context.SaveChanges();

                        //Bind the news items
                        BindNewsItems();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the News Item!", 10000);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the News Item to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitNewsItem user control 
        /// </summary>
        /// <param name="sender">The submitNewsItem control</param>
        /// <param name="e">The Click event</param>
        protected void submitNewsItem_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
            {
                //Get the item pk
                int itemPK = Convert.ToInt32(hfAddEditNewsItemPK.Value);

                if (itemPK > 0)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //The current news item
                        NewsItem currentNewsItem = context.NewsItem.Find(itemPK);

                        //Fill the NewsItem fields from the form
                        currentNewsItem.ItemNum = Convert.ToInt32(txtItemNum.Value);
                        currentNewsItem.Contents = txtNewsItemContents.Value.ToString();
                        currentNewsItem.NewsEntryFK = currentNewsEntryPK;

                        //Set the edit-only fields
                        currentNewsItem.Editor = User.Identity.Name;
                        currentNewsItem.EditDate = DateTime.Now;

                        //Get the existing NewsItem record
                        Models.NewsItem existingNewsItem = context.NewsItem.Find(currentNewsItem.NewsItemPK);

                        //Overwrite the existing NewsItem record with the values from the form
                        context.Entry(existingNewsItem).CurrentValues.SetValues(currentNewsItem);
                        context.SaveChanges();
                    }

                    //Show the user a success message
                    msgSys.ShowMessageToUser("success", "News Item Added", "News Item successfully edited!", 5000);
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //The current news item
                        NewsItem currentNewsItem = new NewsItem();

                        //Fill the NewsItem fields from the form
                        currentNewsItem.ItemNum = Convert.ToInt32(txtItemNum.Value);
                        currentNewsItem.Contents = txtNewsItemContents.Value.ToString();
                        currentNewsItem.NewsEntryFK = currentNewsEntryPK;

                        //Set the create-only fields
                        currentNewsItem.Creator = User.Identity.Name;
                        currentNewsItem.CreateDate = DateTime.Now;

                        //Add the NewsItem to the database
                        context.NewsItem.Add(currentNewsItem);
                        context.SaveChanges();
                    }

                    //Show the user a success message
                    msgSys.ShowMessageToUser("success", "News Item Added", "News Item successfully added!", 5000);
                }

                //Reset the values in the hidden field and hide the div
                hfAddEditNewsItemPK.Value = "0";
                divAddEditNewsItem.Visible = false;

                //Bind the news items
                BindNewsItems();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitNewsItem user control 
        /// </summary>
        /// <param name="sender">The submitNewsItem control</param>
        /// <param name="e">The Click event</param>
        protected void submitNewsItem_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditNewsItemPK.Value = "0";
            divAddEditNewsItem.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitNewsItem control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitNewsItem_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitNewsEntry user control 
        /// </summary>
        /// <param name="sender">The submitNewsEntry control</param>
        /// <param name="e">The Click event</param>
        protected void submitNewsEntry_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
            {
                //To hold the success message type
                string successMessageType = null;

                //Fill the NewsEntry fields from the form
                currentNewsEntry.EntryDate = Convert.ToDateTime(deEntryDate.Value);
                currentNewsEntry.NewsEntryTypeCodeFK = Convert.ToInt32(ddEntryType.Value);

                //Set the proper FKs
                if(currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.APPLICATION)
                {
                    currentNewsEntry.ProgramFK = null;
                    currentNewsEntry.HubFK = null;
                    currentNewsEntry.StateFK = null;
                    currentNewsEntry.CohortFK = null;
                }
                else if(currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.PROGRAM_WIDE)
                {
                    currentNewsEntry.ProgramFK = Convert.ToInt32(ddProgram.Value);
                    currentNewsEntry.HubFK = null;
                    currentNewsEntry.StateFK = null;
                    currentNewsEntry.CohortFK = null;
                }
                else if(currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.HUB_WIDE)
                {
                    currentNewsEntry.ProgramFK = null;
                    currentNewsEntry.HubFK = Convert.ToInt32(ddHub.Value);
                    currentNewsEntry.StateFK = null;
                    currentNewsEntry.CohortFK = null;
                }
                else if(currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.STATE_WIDE)
                {
                    currentNewsEntry.ProgramFK = null;
                    currentNewsEntry.HubFK = null;
                    currentNewsEntry.StateFK = Convert.ToInt32(ddState.Value);
                    currentNewsEntry.CohortFK = null;
                }
                else if(currentNewsEntry.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.COHORT_WIDE)
                {
                    currentNewsEntry.ProgramFK = null;
                    currentNewsEntry.HubFK = null;
                    currentNewsEntry.StateFK = null;
                    currentNewsEntry.CohortFK = Convert.ToInt32(ddCohort.Value);
                }

                if (currentNewsEntryPK > 0)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "NewsEntryEdited";

                        //Set the edit-only fields
                        currentNewsEntry.Editor = User.Identity.Name;
                        currentNewsEntry.EditDate = DateTime.Now;

                        //Get the existing NewsEntry record
                        Models.NewsEntry existingNewsEntry = context.NewsEntry.Find(currentNewsEntry.NewsEntryPK);

                        //Overwrite the existing NewsEntry record with the values from the form
                        context.Entry(existingNewsEntry).CurrentValues.SetValues(currentNewsEntry);
                        context.SaveChanges();
                    }

                    //Redirect the user to the news page
                    Response.Redirect(string.Format("/Pages/News.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "NewsEntryAdded";

                        //Set the create-only fields
                        currentNewsEntry.Creator = User.Identity.Name;
                        currentNewsEntry.CreateDate = DateTime.Now;

                        //Add the NewsEntry to the database
                        context.NewsEntry.Add(currentNewsEntry);
                        context.SaveChanges();
                    }

                    //Redirect the user to the  this page
                    Response.Redirect(string.Format("/Pages/NewsManagement.aspx?NewsEntryPK={0}&Action=Edit&messageType={1}",
                                                        currentNewsEntry.NewsEntryPK.ToString(), successMessageType));
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitNewsEntry user control 
        /// </summary>
        /// <param name="sender">The submitNewsEntry control</param>
        /// <param name="e">The Click event</param>
        protected void submitNewsEntry_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the NewsEntry dashboard
            Response.Redirect(string.Format("/Pages/News.aspx?messageType={0}", "NewsEntryCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitNewsEntry control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitNewsEntry_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        #endregion

        #region Data Binding Methods

        /// <summary>
        /// This method populates the data bound controls from the database
        /// </summary>
        private void BindDataBoundControls()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the news entry types
                var allNewsEntryTypes = context.CodeNewsEntryType.AsNoTracking()
                                            .Where(cnet => cnet.RolesAuthorizedToModify.Contains(currentProgramRole.RoleFK.Value.ToString() + ","))
                                            .OrderBy(cnet => cnet.OrderBy)
                                            .ToList();

                //Bind the type dropdown
                ddEntryType.DataSource = allNewsEntryTypes;
                ddEntryType.DataBind();

                //Get all the programs
                var allPrograms = context.Program.AsNoTracking()
                                    .Include(p => p.Hub)
                                    .Include(p => p.State)
                                    .Include(p => p.Cohort)
                                    .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                    .OrderBy(p => p.ProgramName)
                                    .ToList();

                //Bind the program dropdown
                ddProgram.DataSource = allPrograms.Select(p => new {
                    p.ProgramPK,
                    p.ProgramName
                });
                ddProgram.DataBind();

                //Bind the hub dropdown
                ddHub.DataSource = allPrograms.Select(p => new {
                    p.Hub.HubPK,
                    p.Hub.Name
                }).Distinct().OrderBy(h => h.Name);
                ddHub.DataBind();

                //Bind the state dropdown
                ddState.DataSource = allPrograms.Select(p => new {
                    p.State.StatePK,
                    p.State.Name
                }).Distinct().OrderBy(s => s.Name);
                ddState.DataBind();

                //Bind the cohort dropdown
                ddCohort.DataSource = allPrograms.Select(p => new {
                    p.Cohort.CohortPK,
                    p.Cohort.CohortName
                }).Distinct().OrderBy(c => c.CohortName);
                ddCohort.DataBind();

                //Bind the news items
                BindNewsItems();
            }
        }

        /// <summary>
        /// This method populates the news item repeater with up-to-date information
        /// </summary>
        private void BindNewsItems()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Bind the repeater
                var allNewsItems = context.NewsItem.AsNoTracking()
                                            .Where(ni => ni.NewsEntryFK == currentNewsEntryPK)
                                            .ToList();
                repeatNewsItems.DataSource = allNewsItems;
                repeatNewsItems.DataBind();
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method populates the page controls from the passed NewsEntry object
        /// </summary>
        /// <param name="newsEntry">The NewsEntry object with values to populate the page controls</param>
        private void PopulatePage(Models.NewsEntry newsEntry)
        {
            //Set the page controls to the values from the object
            deEntryDate.Value = newsEntry.EntryDate;
            ddEntryType.SelectedItem = ddEntryType.Items.FindByValue(newsEntry.NewsEntryTypeCodeFK);
            ddProgram.SelectedItem = ddProgram.Items.FindByValue(newsEntry.ProgramFK);
            ddHub.SelectedItem = ddHub.Items.FindByValue(newsEntry.HubFK);
            ddState.SelectedItem = ddState.Items.FindByValue(newsEntry.StateFK);
            ddCohort.SelectedItem = ddCohort.Items.FindByValue(newsEntry.CohortFK);
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            deEntryDate.ClientEnabled = enabled;
            ddEntryType.ClientEnabled = enabled;
        }

        #endregion

        #region Custom Validation

        /// <summary>
        /// This method validates the news entry type options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void NewsEntryTypeOption_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the entry type
            int? entryType = (ddEntryType.Value == null ? (int?)null : Convert.ToInt32(ddEntryType.Value));

            //Perform validation
            if (entryType.HasValue)
            {
                if (entryType.Value == (int)Utilities.NewsTypeFKs.PROGRAM_WIDE)
                {
                    int? programFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

                    if (programFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
                else if (entryType.Value == (int)Utilities.NewsTypeFKs.HUB_WIDE)
                {
                    int? hubFK = (ddHub.Value == null ? (int?)null : Convert.ToInt32(ddHub.Value));

                    if (hubFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
                else if (entryType.Value == (int)Utilities.NewsTypeFKs.STATE_WIDE)
                {
                    int? stateFK = (ddState.Value == null ? (int?)null : Convert.ToInt32(ddState.Value));

                    if (stateFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
                else if (entryType.Value == (int)Utilities.NewsTypeFKs.COHORT_WIDE)
                {
                    int? cohortFK = (ddCohort.Value == null ? (int?)null : Convert.ToInt32(ddCohort.Value));

                    if (cohortFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
            }
        }

        #endregion
    }
}