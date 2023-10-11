using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace Pyramid.Pages
{
    public partial class ASQSEDashboard : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "ASQSE";
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
                int actionColumnIndex = (bsGRASQSE.Columns.Count - 1);

                //Hide the action column
                bsGRASQSE.Columns[actionColumnIndex].Visible = false;
            }

            //Show/hide the state column based on the number of states accessible to the user
            bsGRASQSE.Columns["StateNameColumn"].Visible = (currentProgramRole.StateFKs.Count > 1);

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

                //Populate the chart
                BindScoreTypeChart();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {

                    switch (messageType)
                    {
                        case "ASQSEAdded":
                            msgSys.ShowMessageToUser("success", "Success", "ASQ:SE screening successfully added!", 10000);
                            break;
                        case "ASQSEEdited":
                            msgSys.ShowMessageToUser("success", "Success", "ASQ:SE screening successfully edited!", 10000);
                            break;
                        case "ASQSECanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOASQSE":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified ASQ:SE screening could not be found, please try again.", 15000);
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
        /// This method fills the ASQSEs by Score Type chart with data for the past year
        /// </summary>
        private void BindScoreTypeChart()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the score type data for the past year
                var scoreTypeDate = context.spGetASQSEsByScoreType(string.Join(",", currentProgramRole.ProgramFKs), 
                                        DateTime.Now.AddYears(-1), DateTime.Now)
                                        .ToList();

                //Bind the chart to the data
                chartScoreType.DataSource = scoreTypeDate;
                chartScoreType.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for an ASQSE
        /// and it deletes the ASQ:SE screening from the database
        /// </summary>
        /// <param name="sender">The lbDeleteASQSE LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteASQSE_Click(object sender, EventArgs e)
        {
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeASQSEPK = String.IsNullOrWhiteSpace(hfDeleteASQSEPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteASQSEPK.Value);

                if (removeASQSEPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the ASQSE to remove
                            Models.ASQSE ASQSEToRemove = context.ASQSE.Where(a => a.ASQSEPK == removeASQSEPK).FirstOrDefault();

                            //Remove the ASQSE
                            context.ASQSE.Remove(ASQSEToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.ASQSEChanged
                                    .OrderByDescending(ac => ac.ASQSEChangedPK)
                                    .Where(ac => ac.ASQSEPK == ASQSEToRemove.ASQSEPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the ASQ:SE screening!", 1000);

                            //Bind the gridview
                            bsGRASQSE.DataBind();

                            //Bind the chart
                            BindScoreTypeChart();
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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the ASQSE, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the ASQSE!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the ASQSE!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the ASQ:SE screening to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the ASQSE DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efASQSEDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efASQSEDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "ASQSEPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = from a in context.ASQSE.AsNoTracking()
                                    .Include(a => a.Program)
                                    .Include(a => a.Program.State)
                                    .Include(a => a.Child)
                                    .Include(a => a.CodeASQSEInterval)
                                join cp in context.ChildProgram on a.ChildFK equals cp.ChildFK
                                join sa in context.ScoreASQSE on a.IntervalCodeFK equals sa.IntervalCodeFK
                                where currentProgramRole.ProgramFKs.Contains(a.ProgramFK)
                                  && cp.ProgramFK == a.ProgramFK
                                  && cp.EnrollmentDate <= a.FormDate
                                  && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= a.FormDate)
                                  && a.Version == sa.Version
                                select new
                                {
                                    a.ASQSEPK,
                                    a.FormDate,
                                    a.TotalScore,
                                    ChildID = cp.ProgramSpecificID,
                                    ChildIDAndName = (currentProgramRole.ViewPrivateChildInfo.Value ?
                                                        "(" + cp.ProgramSpecificID + ") " + a.Child.FirstName + " " + a.Child.LastName :
                                                        cp.ProgramSpecificID),
                                    a.Program.ProgramName,
                                    StateName = a.Program.State.Name,
                                    Interval = a.CodeASQSEInterval.Description,
                                    ScoreType = (a.TotalScore > sa.CutoffScore ? "Above Cutoff" 
                                                    : a.TotalScore >= sa.MonitoringScoreStart && a.TotalScore <= sa.MonitoringScoreEnd ? "Monitor"
                                                    : a.TotalScore >= 0 && a.TotalScore < sa.MonitoringScoreStart ? "Well Below" 
                                                    : "Error!")
                                };
        }
    }
}