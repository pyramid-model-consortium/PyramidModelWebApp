using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;

namespace Pyramid.Pages
{
    public partial class TPITOSDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRTPITOS.Columns.Count - 1);

                //Hide the action column
                bsGRTPITOS.Columns[actionColumnIndex].Visible = false;
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
                        case "TPITOSAdded":
                            msgSys.ShowMessageToUser("success", "Success", "TPITOS observation successfully added!", 10000);
                            break;
                        case "TPITOSEdited":
                            msgSys.ShowMessageToUser("success", "Success", "TPITOS observation successfully edited!", 10000);
                            break;
                        case "TPITOSCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOTPITOS":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified TPITOS observation could not be found, please try again.", 15000);
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
        /// This method executes when the user clicks the delete button for an TPITOS
        /// and it deletes the TPITOS form from the database
        /// </summary>
        /// <param name="sender">The lbDeleteTPITOS LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTPITOS_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeTPITOSPK = String.IsNullOrWhiteSpace(hfDeleteTPITOSPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTPITOSPK.Value);

                if (removeTPITOSPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the TPITOS to remove
                        var TPITOSToRemove = context.TPITOS.Where(x => x.TPITOSPK == removeTPITOSPK).FirstOrDefault();

                        //Get the participant rows to remove and remove them
                        var participantsToRemove = context.TPITOSParticipant.Where(tp => tp.TPITOSFK == removeTPITOSPK).ToList();
                        context.TPITOSParticipant.RemoveRange(participantsToRemove);

                        //Get the red flag rows to remove and remove them
                        var redFlagsToRemove = context.TPITOSRedFlags.Where(trf => trf.TPITOSFK == removeTPITOSPK).ToList();
                        context.TPITOSRedFlags.RemoveRange(redFlagsToRemove);

                        //Remove the TPITOS from the database
                        context.TPITOS.Remove(TPITOSToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the TPITOS observation!", 1000);

                        //Bind the gridview
                        bsGRTPITOS.DataBind();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the TPITOS observation to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the TPITOS DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efTPITOSDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efTPITOSDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "TPITOSPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.TPITOS.AsNoTracking()
                                    .Include(t => t.Classroom)
                                    .Include(t => t.Classroom.Program)
                                    .Where(t => currentProgramRole.ProgramFKs.Contains(t.Classroom.ProgramFK))
                                    .OrderByDescending(t => t.ObservationStartDateTime)
                                    .Select(t => new
                                    {
                                        t.TPITOSPK,
                                        t.ObservationStartDateTime,
                                        t.ObservationEndDateTime,
                                        t.IsValid,
                                        IsValidText = (t.IsValid ? "Valid" : "Invalid"),
                                        ClassroomIdAndName = "(" + t.Classroom.ProgramSpecificID + ") " + t.Classroom.Name,
                                        ProgramName = t.Classroom.Program.ProgramName
                                    });
        }

        /// <summary>
        /// This method fires when the TPITOS GridView prepares a row
        /// and it highlights invalid TPITOS observations
        /// </summary>
        /// <param name="sender">The bsGRTPITOS Bootstrap GridView</param>
        /// <param name="e">The ASPxGridViewTableRowEvent event arguments</param>
        protected void bsGRTPITOS_HtmlRowPrepared(object sender, DevExpress.Web.ASPxGridViewTableRowEventArgs e)
        {
            //Evaluate only data rows
            if(e.RowType == DevExpress.Web.GridViewRowType.Data)
            {
                //Determine if this row represents a valid TPITOS
                bool isTPITOSValid = Convert.ToBoolean(e.GetValue("IsValid"));

                if(!isTPITOSValid)
                {
                    //The TPITOS is not valid, add the CSS class
                    e.Row.CssClass = "invalid-tpitos";
                }
            }
        }
    }
}