using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;

namespace Pyramid.Pages
{
    public partial class CoachingLogDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRCoachingLog.Columns.Count - 1);

                //Hide the action column
                bsGRCoachingLog.Columns[actionColumnIndex].Visible = false;
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
                        case "CoachingLogAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Classroom Coaching Log successfully added!", 10000);
                            break;
                        case "CoachingLogEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Classroom Coaching Log successfully edited!", 10000);
                            break;
                        case "CoachingLogCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOCoachingLog":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Classroom Coaching Log could not be found, please try again.", 15000);
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
        /// This method executes when the user clicks the delete button for an CoachingLog
        /// and it deletes the CoachingLog form from the database
        /// </summary>
        /// <param name="sender">The lbDeleteCoachingLog LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteCoachingLog_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeCoachingLogPK = String.IsNullOrWhiteSpace(hfDeleteCoachingLogPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCoachingLogPK.Value);

                if (removeCoachingLogPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the CoachingLog to remove
                        var CoachingLogToRemove = context.CoachingLog.Where(x => x.CoachingLogPK == removeCoachingLogPK).FirstOrDefault();

                        //Remove the CoachingLog from the database
                        context.CoachingLog.Remove(CoachingLogToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Classroom Coaching Log!", 1000);

                        //Bind the gridview
                        bsGRCoachingLog.DataBind();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Classroom Coaching Log to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the CoachingLog DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efCoachingLogDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efCoachingLogDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "CoachingLogPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.CoachingLog.AsNoTracking()
                                .Include(cl => cl.Program)
                                .Include(cl => cl.Coach)
                                .Include(cl => cl.Teacher)
                                .Where(cl => currentProgramRole.ProgramFKs.Contains(cl.ProgramFK))
                                .Select(cl => new
                                {
                                    cl.CoachingLogPK,
                                    cl.LogDate,
                                    cl.DurationMinutes,
                                    CoachName = cl.Coach.FirstName + " " + cl.Coach.LastName,
                                    TeacherName = cl.Teacher.FirstName + " " + cl.Teacher.LastName,
                                    ProgramName = cl.Program.ProgramName
                                });
        }
    }
}