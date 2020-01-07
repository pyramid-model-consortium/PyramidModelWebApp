using System;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Pyramid
{
    public partial class _Default : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
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
                        case "TwoFactorVerified":
                            msgSys.ShowMessageToUser("success", "Two-Factor Code Verified", "Your Two-Factor code was successfully verified!", 5000);
                            break;
                    }
                }

                //Show or hide the fireworks
                ShowHideFireworks();
            }


            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            DateTime lastMonth = DateTime.Now.AddDays(-30);
            BindNews(lastMonth);

            //Set the logo
            bsImgLargeLogo.ImageUrl = "/Content/images/" + (currentProgramRole.StateLogoFileName == null ? "GenericLogo.png" : currentProgramRole.StateLogoFileName);
        }

        /// <summary>
        /// This method shows or hides the fireworks based on the user's preferences
        /// </summary>
        private void ShowHideFireworks()
        {
            //Fireworks!
            //Get the customization option for fireworks
            string fireworksCustomizationOption = Utilities.GetCustomizationOptionFromCookie("fireworks");
            bool? fireworksAllowed = (string.IsNullOrWhiteSpace(fireworksCustomizationOption) ? (bool?)null : Convert.ToBoolean(fireworksCustomizationOption));

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
        /// This method populates the news div with the news that is after the limit date
        /// passed to this method
        /// </summary>
        /// <param name="limitDate">A limiting DateTime</param>
        private void BindNews(DateTime limitDate)
        {
            //To hold the news
            List<NewsEntry> newsEntries;

            using (PyramidContext context = new PyramidContext())
            {
                //Get the news entries that apply to this user's role (app, state, hub, and program)
                newsEntries = context.NewsEntry.Include(ne => ne.NewsItem)
                                .Include(ne => ne.CodeNewsEntryType)
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
                                        && ne.EntryDate <= DateTime.Now)
                                .OrderByDescending(ne => ne.EntryDate)
                                .ThenByDescending(ne => ne.CreateDate)
                                .ToList();
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
                    foreach (NewsItem item in entry.NewsItem.OrderBy(ni => ni.ItemNum))
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
    }
}