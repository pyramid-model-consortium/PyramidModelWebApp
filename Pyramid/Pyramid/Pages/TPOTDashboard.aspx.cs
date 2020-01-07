using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;

namespace Pyramid.Pages
{
    public partial class TPOTDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRTPOT.Columns.Count - 1);

                //Hide the action column
                bsGRTPOT.Columns[actionColumnIndex].Visible = false;
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
                        case "TPOTAdded":
                            msgSys.ShowMessageToUser("success", "Success", "TPOT observation successfully added!", 10000);
                            break;
                        case "TPOTEdited":
                            msgSys.ShowMessageToUser("success", "Success", "TPOT observation successfully edited!", 10000);
                            break;
                        case "TPOTCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOTPOT":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified TPOT observation could not be found, please try again.", 15000);
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
        /// This method executes when the user clicks the delete button for an TPOT
        /// and it deletes the TPOT form from the database
        /// </summary>
        /// <param name="sender">The lbDeleteTPOT LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteTPOT_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeTPOTPK = String.IsNullOrWhiteSpace(hfDeleteTPOTPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteTPOTPK.Value);

                if (removeTPOTPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the TPOT to remove
                        var TPOTToRemove = context.TPOT.Where(x => x.TPOTPK == removeTPOTPK).FirstOrDefault();

                        //Get the participant rows to remove and remove them
                        var participantsToRemove = context.TPOTParticipant.Where(tp => tp.TPOTFK == removeTPOTPK).ToList();
                        context.TPOTParticipant.RemoveRange(participantsToRemove);

                        //Get the red flag rows to remove and remove them
                        var redFlagsToRemove = context.TPOTRedFlags.Where(trf => trf.TPOTFK == removeTPOTPK).ToList();
                        context.TPOTRedFlags.RemoveRange(redFlagsToRemove);

                        //Get the behavior responses to remove and remove them
                        var behaviorResponsesToRemove = context.TPOTBehaviorResponses.Where(tbr => tbr.TPOTFK == removeTPOTPK).ToList();
                        context.TPOTBehaviorResponses.RemoveRange(behaviorResponsesToRemove);

                        //Remove the TPOT from the database
                        context.TPOT.Remove(TPOTToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the TPOT observation!", 1000);

                        //Bind the gridview
                        bsGRTPOT.DataBind();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the TPOT observation to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the TPOT DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efTPOTDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efTPOTDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "TPOTPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.TPOT.AsNoTracking()
                                    .Include(t => t.Classroom)
                                    .Include(t => t.Classroom.Program)
                                    .Where(t => currentProgramRole.ProgramFKs.Contains(t.Classroom.ProgramFK))
                                    .OrderByDescending(t => t.ObservationStartDateTime)
                                    .Select(t => new
                                    {
                                        t.TPOTPK,
                                        t.ObservationStartDateTime,
                                        t.ObservationEndDateTime,
                                        t.IsValid,
                                        IsValidText = (t.IsValid ? "Valid" : "Invalid"),
                                        ClassroomIdAndName = "(" + t.Classroom.ProgramSpecificID + ") " + t.Classroom.Name,
                                        ProgramName = t.Classroom.Program.ProgramName
                                    });
        }

        /// <summary>
        /// This method fires when the TPOT GridView prepares a row
        /// and it highlights invalid TPOT observations
        /// </summary>
        /// <param name="sender">The bsGRTPOT Bootstrap GridView</param>
        /// <param name="e">The ASPxGridViewTableRowEvent event arguments</param>
        protected void bsGRTPOT_HtmlRowPrepared(object sender, DevExpress.Web.ASPxGridViewTableRowEventArgs e)
        {
            //Evaluate only data rows
            if (e.RowType == DevExpress.Web.GridViewRowType.Data)
            {
                //Determine if this row represents a valid TPOT
                bool isTPOTValid = Convert.ToBoolean(e.GetValue("IsValid"));

                if (!isTPOTValid)
                {
                    //The TPOT is not valid, add the CSS class
                    e.Row.CssClass = "invalid-tpot";
                }
            }
        }
    }
}