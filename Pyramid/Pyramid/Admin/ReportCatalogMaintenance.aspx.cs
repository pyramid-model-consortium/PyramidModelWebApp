using Pyramid.Code;
using Pyramid.Models;
using System;
using System.IO;
using System.Linq;

namespace Pyramid.Admin
{
    public partial class ReportCatalogMaintenance : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Only allow super admins
            if(currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                Response.Redirect("/Default.aspx");
            }

            if (!IsPostBack)
            {
                //Set the view only value
                if (currentProgramRole.AllowedToEdit.Value)
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
                        case "ReportCatalogItemAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Report Catalog Item successfully added!", 10000);
                            break;
                        case "ReportCatalogItemEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Report Catalog Item successfully edited!", 10000);
                            break;
                        case "ReportCatalogItemCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOReportCatalogItem":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Report Catalog Item could not be found, please try again.", 15000);
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
        /// This method executes when the user clicks the delete button for an Report Catalog Item
        /// and it deletes the Report Catalog Item from the database
        /// </summary>
        /// <param name="sender">The lbDeleteReportCatalogItem LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteReportCatalogItem_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeReportCatalogItemPK = String.IsNullOrWhiteSpace(hfDeleteReportCatalogItemPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteReportCatalogItemPK.Value);

                if (removeReportCatalogItemPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the ReportCatalogItem to remove
                        ReportCatalog ReportCatalogItemToRemove = context.ReportCatalog.Where(rc => rc.ReportCatalogPK == removeReportCatalogItemPK).FirstOrDefault();
                        
                        //Remove the ReportCatalogItem from the database
                        context.ReportCatalog.Remove(ReportCatalogItemToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Report Catalog Item!", 1000);

                        //Bind the gridview
                        bsGRReports.DataBind();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Report Catalog Item to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the reports DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efReportDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efReportDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "ReportCatalogPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.ReportCatalog.AsNoTracking();
        }
    }
}