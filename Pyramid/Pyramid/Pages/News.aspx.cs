using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace Pyramid.Pages
{
    public partial class News : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "NEWS";
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

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Check to see if the user can view this page
            if (FormPermissions.AllowedToViewDashboard == false)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            if (!IsPostBack)
            {
                //Set the view only value
                if (FormPermissions.AllowedToAdd == false && FormPermissions.AllowedToEdit == false)
                {
                    hfViewOnly.Value = "True";
                }
                else
                {
                    hfViewOnly.Value = "False";
                }

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {

                    switch (messageType)
                    {
                        case "NewsEntryAdded":
                            msgSys.ShowMessageToUser("success", "Success", "News entry successfully added!", 10000);
                            break;
                        case "NewsEntryEdited":
                            msgSys.ShowMessageToUser("success", "Success", "News entry successfully edited!", 10000);
                            break;
                        case "NewsEntryCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoNewsEntry":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified news entry could not be found, please try again.", 15000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Bind the databound controls
                BindDataBoundControls();

                //Pre-fill the limit date
                deLimitDate.Value = DateTime.Now.AddDays(-30);

                //Set the max date for the limit date
                deLimitDate.MaxDate = DateTime.Now;

                //Bind the news
                BindNews();
            }
        }

        /// <summary>
        /// This method populates all the databound controls on the page
        /// </summary>
        private void BindDataBoundControls()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the News Entry types
                List<CodeNewsEntryType> allNewsEntryTypes = context.CodeNewsEntryType.AsNoTracking()
                                        .OrderBy(cnet => cnet.OrderBy)
                                        .ToList();

                //Create a default filter for the type list
                CodeNewsEntryType defaultFilter = new CodeNewsEntryType();
                defaultFilter.CodeNewsEntryTypePK = 999;
                defaultFilter.Description = "--All--";
                defaultFilter.OrderBy = 0;

                //Add the default filter to the first point in the list
                allNewsEntryTypes.Insert(0, defaultFilter);

                //Bind the type dropdown
                ddEntryType.DataSource = allNewsEntryTypes;
                ddEntryType.DataBind();

                //Pre-select the default filter
                ddEntryType.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// This method populates the news div with the news that is after the limit date
        /// passed to this method
        /// </summary>
        private void BindNews()
        {
            //Get the limit date and entry type filters
            DateTime? limitDate = (deLimitDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLimitDate.Value));
            int? entryType = (ddEntryType.Value == null ? (int?)null : Convert.ToInt32(ddEntryType.Value));

            if (!limitDate.HasValue)
                limitDate = DateTime.Now.AddDays(-30);
            if (!entryType.HasValue)
                entryType = 999;

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
                                    .Where(ne => ne.EntryDate >= limitDate
                                            && ne.NewsEntryTypeCodeFK == (entryType.Value == 999 ? ne.NewsEntryTypeCodeFK : entryType.Value))
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
                                    .Where(ne => ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.APPLICATION
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

            if (newsEntries.Count > 0)
            {
                //Loop through each news entry
                foreach (NewsEntry entry in newsEntries)
                {
                    //Add HTML for the entry
                    newsHTML.Append("<div class='row news-entry'><div class='col-md-8'><label class='news-entry-date'>" + entry.EntryDate.ToString("MM/dd/yyyy") + "</label> (" + entry.CodeNewsEntryType.Description + ")</div>");

                    //Get the authorized roles
                    List<string> rolesAuthorized = entry.CodeNewsEntryType.RolesAuthorizedToModify.Split(',').ToList();

                    //If the user is authorized, include the actions dropdown
                    if (rolesAuthorized.Contains(currentProgramRole.CodeProgramRoleFK.ToString()))
                    {
                        newsHTML.Append("<div class='col-md-4'><div class='btn-group'><button id='btnActions" + entry.NewsEntryPK + "' type='button' class='btn btn-secondary dropdown-toggle' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>Actions</button>");
                        newsHTML.Append("<div class='dropdown-menu' aria-labelledby='btnActions" + entry.NewsEntryPK + "'>");
                        newsHTML.Append("<a class='dropdown-item' href='/Pages/NewsManagement.aspx?NewsEntryPK=" + entry.NewsEntryPK.ToString() + "&action=Edit'><i class='fas fa-edit'></i>&nbsp;Edit</a>");
                        newsHTML.Append("<button class='dropdown-item delete-gridview' data-pk='" + entry.NewsEntryPK.ToString() + "' data-hf='hfDeleteNewsPK' data-target='#divDeleteNewsModal'><i class='fas fa-trash'></i>&nbsp;Delete</button>");
                        newsHTML.Append("</div></div></div>");
                    }
                    newsHTML.Append("<div class='col-md-12'><ul class='news-list'>");

                    //Loop through each news item in the entry
                    foreach (NewsItem item in entry.NewsItem.OrderBy(ni => ni.ItemNum))
                    {
                        //Add HTML for the item
                        newsHTML.Append("<li>" + Server.HtmlEncode(item.Contents) + "</li>");
                    }

                    //Close the HTML tags
                    newsHTML.Append("</ul><hr></div></div>");
                }
            }
            else
            {
                newsHTML.Append("No news found...");
            }

            //Display the news
            ltlNews.Text = newsHTML.ToString();
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a news entry
        /// and it deletes the news entry from the database
        /// </summary>
        /// <param name="sender">The lbDeleteNews LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteNews_Click(object sender, EventArgs e)
        {
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeNewsPK = String.IsNullOrWhiteSpace(hfDeleteNewsPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteNewsPK.Value);

                if (removeNewsPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the News to remove
                            var newsEntryToRemove = context.NewsEntry.Include(ne => ne.CodeNewsEntryType).Where(ne => ne.NewsEntryPK == removeNewsPK).FirstOrDefault();

                            //Get the roles authorized to delete
                            List<string> rolesAuthorized = newsEntryToRemove.CodeNewsEntryType.RolesAuthorizedToModify.Split(',').ToList();

                            //Ensure the user is allowed to delete this file
                            if (!rolesAuthorized.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()))
                            {
                                msgSys.ShowMessageToUser("danger", "Delete Failed", "You are not authorized to delete this news entry!", 10000);
                            }
                            else
                            {
                                //Get the news item rows to remove and remove them
                                var newsItemsToRemove = context.NewsItem.Where(ni => ni.NewsEntryFK == removeNewsPK).ToList();
                                context.NewsItem.RemoveRange(newsItemsToRemove);

                                //Remove the new entry from the database
                                context.NewsEntry.Remove(newsEntryToRemove);
                                context.SaveChanges();

                                //To hold the news item change rows
                                List<NewsItemChanged> itemChangeRows;

                                //Check the news item deletions
                                if (newsItemsToRemove.Count > 0)
                                {
                                    //Get the news item deletion rows and set the deleter
                                    itemChangeRows = context.NewsItemChanged.Where(nic => nic.NewsEntryFK == newsEntryToRemove.NewsEntryPK)
                                                                                            .OrderByDescending(nic => nic.NewsItemChangedPK)
                                                                                            .Take(newsItemsToRemove.Count).ToList()
                                                                                            .Select(cnc => { cnc.Deleter = User.Identity.Name; return cnc; }).ToList();
                                }

                                //Get the delete change row for the news entry and set the deleter
                                context.NewsEntryChanged
                                        .OrderByDescending(nec => nec.NewsEntryChangedPK)
                                        .Where(nec => nec.NewsEntryPK == newsEntryToRemove.NewsEntryPK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Show a delete success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the news entry!", 1000);
                            }

                            //Bind the news
                            BindNews();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the news entry, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the news entry!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the news entry!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the news entry to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method is called by the filters on the page and it filters the news
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void FilterNews(object sender, EventArgs e)
        {
            //Get the filters
            DateTime? limitDate = (deLimitDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deLimitDate.Value));
            int? entryType = (ddEntryType.Value == null ? (int?)null : Convert.ToInt32(ddEntryType.Value));

            //Only continue if the filters are valid
            if (limitDate.HasValue && entryType.HasValue)
            {
                //Bind the news
                BindNews();
            }
        }
    }
}