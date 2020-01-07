using System;
using System.Data.Entity;
using System.Linq;
using Pyramid.Models;
using Pyramid.Code;

namespace Pyramid.Pages
{
    public partial class BOQFCCDashboard : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow aggregate viewers to see the action column
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (bsGRBOQFCCs.Columns.Count - 1);

                //Hide the action column
                bsGRBOQFCCs.Columns[actionColumnIndex].Visible = false;
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

                //Check for a message in the query string
                string messagetype = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messagetype))
                {
                    switch(messagetype)
                    {
                        case "BOQFCCAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Benchmarks Of Quality FCC form successfully added!", 1000);
                            break;
                        case "BOQFCCEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Benchmarks Of Quality FCC form successfully edited!", 1000);
                            break;
                        case "BOQFCCCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoBOQFCC":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Benchmarks of Quality FCC form could not be found, please try again.", 15000);
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
        /// This method fires when the data source for the Benchmarks of Quality FCC DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efBOQFCCDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efBOQFCCDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "BenchmarkOfQualityFCCPK";
            
            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.BenchmarkOfQualityFCC.AsNoTracking().Include(boqfcc => boqfcc.Program)
                                    .Where(boqfcc => currentProgramRole.ProgramFKs.Contains(boqfcc.ProgramFK));
            
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a Benchmarks of Quality FCC form
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteBOQFCC LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteBOQFCC_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeBOQFCCPK = String.IsNullOrWhiteSpace(hfDeleteBOQFCCPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteBOQFCCPK.Value);

                if (removeBOQFCCPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the BOQ to remove
                        var BOQFCCToRemove = context.BenchmarkOfQualityFCC.Where(x => x.BenchmarkOfQualityFCCPK == removeBOQFCCPK).FirstOrDefault();

                        //Remove the BOQ from the database
                        context.BenchmarkOfQualityFCC.Remove(BOQFCCToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted Benchmarks of Quality FCC form!", 1000);

                        //Bind the gridview
                        bsGRBOQFCCs.DataBind();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Benchmarks of Quality FCC form to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
    }
}