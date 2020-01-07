using System;
using System.Data.Entity;
using System.Linq;
using Pyramid.Models;
using Pyramid.Code;

namespace Pyramid.Pages
{
    public partial class BOQDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRBOQs.Columns.Count - 1);

                //Hide the action column
                bsGRBOQs.Columns[actionColumnIndex].Visible = false;
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
                        case "BOQAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Benchmarks Of Quality 2.0 form successfully added!", 1000);
                            break;
                        case "BOQEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Benchmarks Of Quality 2.0 form successfully edited!", 1000);
                            break;
                        case "BOQCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoBOQ":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Benchmarks of Quality 2.0 form could not be found, please try again.", 15000);
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
        /// This method fires when the data source for the Benchmarks of Quality DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efBOQDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efBOQDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "BenchmarkOfQualityPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.BenchmarkOfQuality.AsNoTracking().Include(boq => boq.Program)
                                    .Where(boq => currentProgramRole.ProgramFKs.Contains(boq.ProgramFK));
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a Benchmarks of Quality form
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteBOQ LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteBOQ_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeBOQPK = String.IsNullOrWhiteSpace(hfDeleteBOQPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteBOQPK.Value);

                if (removeBOQPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the BOQ to remove
                        var BOQToRemove = context.BenchmarkOfQuality.Where(x => x.BenchmarkOfQualityPK == removeBOQPK).FirstOrDefault();

                        //Remove the BOQ from the database
                        context.BenchmarkOfQuality.Remove(BOQToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted Benchmarks of Quality 2.0 form!", 1000);

                        //Bind the gridview
                        bsGRBOQs.DataBind();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Benchmarks of Quality 2.0 form to delete!", 120000);
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