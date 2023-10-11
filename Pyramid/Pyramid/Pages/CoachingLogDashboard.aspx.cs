using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Pyramid.Pages
{
    public partial class CoachingLogDashboard : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CCL";
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

            //Don't allow aggregate viewers to see the action column
            if (FormPermissions.AllowedToView == false)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (bsGRCoachingLog.Columns.Count - 1);

                //Hide the action column
                bsGRCoachingLog.Columns[actionColumnIndex].Visible = false;
            }

            //Show/hide the state column based on the number of states accessible to the user
            bsGRCoachingLog.Columns["StateNameColumn"].Visible = (currentProgramRole.StateFKs.Count > 1);

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
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeCoachingLogPK = String.IsNullOrWhiteSpace(hfDeleteCoachingLogPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCoachingLogPK.Value);

                if (removeCoachingLogPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the coaching log to remove
                            var coachingLogToRemove = context.CoachingLog.Where(cl => cl.CoachingLogPK == removeCoachingLogPK).FirstOrDefault();

                            //Get the coachees to remove
                            var coacheesToRemove = context.CoachingLogCoachees.Where(clc => clc.CoachingLogFK == removeCoachingLogPK).ToList();

                            //Remove coachees
                            context.CoachingLogCoachees.RemoveRange(coacheesToRemove);

                            //Remove the coaching log from the database
                            context.CoachingLog.Remove(coachingLogToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Hold the change rows
                            List<CoachingLogCoacheesChanged> coacheeChangeRows;

                            //Check the coachee deletions
                            if (coacheesToRemove.Count > 0)
                            {
                                //Get the coachee change rows and set the deleter
                                coacheeChangeRows = context.CoachingLogCoacheesChanged.Where(tpc => tpc.CoachingLogFK == coachingLogToRemove.CoachingLogPK)
                                                                .OrderByDescending(tpc => tpc.CoachingLogCoacheesChangedPK)
                                                                .Take(coacheesToRemove.Count).ToList()
                                                                .Select(tpc => { tpc.Deleter = User.Identity.Name; return tpc; }).ToList();
                            }

                            //Get the delete change row and set the deleter
                            context.CoachingLogChanged
                                    .OrderByDescending(clc => clc.CoachingLogChangedPK)
                                    .Where(clc => clc.CoachingLogPK == coachingLogToRemove.CoachingLogPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the Classroom Coaching Log!", 1000);

                            //Bind the gridview
                            bsGRCoachingLog.DataBind();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Classroom Coaching Log, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Classroom Coaching Log!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Classroom Coaching Log!", 120000);
                        }

                        //Log the error
                        Utilities.LogException(dbUpdateEx);
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
            e.QueryableSource = context.CoachingLog
                                .Include(cl => cl.Program)
                                .Include(cl => cl.Program.State)
                                .Include(cl => cl.ProgramEmployee)
                                .Include(cl => cl.ProgramEmployee.Employee)
                                .AsNoTracking()
                                .Where(cl => currentProgramRole.ProgramFKs.Contains(cl.ProgramFK))
                                .Select(cl => new
                                {
                                    cl.CoachingLogPK,
                                    cl.LogDate,
                                    cl.DurationMinutes,
                                    CoachName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + cl.ProgramEmployee.ProgramSpecificID + ") " + cl.ProgramEmployee.Employee.FirstName + " " + cl.ProgramEmployee.Employee.LastName : cl.ProgramEmployee.ProgramSpecificID),
                                    ProgramName = cl.Program.ProgramName,
                                    StateName = cl.Program.State.Name
                                });
        }
    }
}