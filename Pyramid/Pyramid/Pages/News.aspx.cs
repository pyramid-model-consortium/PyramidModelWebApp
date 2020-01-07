using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Text;

namespace Pyramid.Pages
{
    public partial class News : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            if (!IsPostBack)
            {
                //Set the view only value
                if ((currentProgramRole.AllowedToEdit.Value
                    && currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.DATA_COLLECTOR)
                    || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
                {
                    hfViewOnly.Value = "False";
                }
                else
                {
                    hfViewOnly.Value = "True";
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
                //Get the news entries that apply to this user's role (app, state, hub, and program)
                newsEntries = context.NewsEntry.Include(ne => ne.NewsItem)
                                .Include(ne => ne.CodeNewsEntryType)
                                .Include(ne => ne.Program)
                                .Where(ne => ((ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.PROGRAM_WIDE 
                                                    && currentProgramRole.ProgramFKs.Contains(ne.ProgramFK.Value))
                                                || ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.APPLICATION
                                                || (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.STATE_WIDE
                                                    && currentProgramRole.StateFK.Value == ne.StateFK.Value)
                                                || (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.HUB_WIDE
                                                    && currentProgramRole.HubFK.Value == ne.HubFK.Value)
                                                || (ne.NewsEntryTypeCodeFK == (int)Utilities.NewsTypeFKs.COHORT_WIDE
                                                    && currentProgramRole.CohortFKs.Contains(ne.CohortFK.Value)))
                                        && ne.EntryDate >= limitDate
                                        && (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN || ne.EntryDate <= DateTime.Now || ne.Creator == User.Identity.Name)
                                        && ne.NewsEntryTypeCodeFK == (entryType.Value == 999 ? ne.NewsEntryTypeCodeFK : entryType.Value))
                                .OrderByDescending(ne => ne.EntryDate)
                                .ThenByDescending(ne => ne.CreateDate)
                                .ToList();
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
                    if (entry.CodeNewsEntryType.RolesAuthorizedToModify.Contains(currentProgramRole.RoleFK.ToString() + ","))
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
            if (currentProgramRole.AllowedToEdit.Value || currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
            {
                //Get the PK from the hidden field
                int? removeNewsPK = String.IsNullOrWhiteSpace(hfDeleteNewsPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteNewsPK.Value);

                if (removeNewsPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the News to remove
                        var newsEntryToRemove = context.NewsEntry.Where(ne => ne.NewsEntryPK == removeNewsPK).FirstOrDefault();

                        //Ensure the user is allowed to delete this file
                        if (!newsEntryToRemove.CodeNewsEntryType.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                        {
                            msgSys.ShowMessageToUser("danger", "Delete Failed", "You are not authorized to delete this news entry!", 10000);
                        }
                        else
                        {

                            //Get the item rows to remove
                            var newsItemsToRemove = context.NewsItem.Where(ni => ni.NewsEntryFK == removeNewsPK).ToList();
                            context.NewsItem.RemoveRange(newsItemsToRemove);

                            //Remove the News from the database
                            context.NewsEntry.Remove(newsEntryToRemove);
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the news entry!", 1000);
                        }

                        //Bind the news
                        BindNews();
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