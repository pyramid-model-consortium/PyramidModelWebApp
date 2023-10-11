using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace Pyramid.Pages
{
    public partial class OtherSEScreenDashboard : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "OSES";
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
            if (currentPermissions.AllowedToView == false)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (bsGROtherSEScreen.Columns.Count - 1);

                //Hide the action column
                bsGROtherSEScreen.Columns[actionColumnIndex].Visible = false;
            }

            //Show/hide the state column based on the number of states accessible to the user
            bsGROtherSEScreen.Columns["StateNameColumn"].Visible = (currentProgramRole.StateFKs.Count > 1);

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
                        case "OtherSEScreenAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Social emotional assessment successfully added!", 10000);
                            break;
                        case "OtherSEScreenEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Social emotional assessment successfully edited!", 10000);
                            break;
                        case "OtherSEScreenCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOOtherSEScreen":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified social emotional assessment could not be found, please try again.", 15000);
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
        /// This method fills the Other Social-Emotional Screens by Score Type chart with data
        /// </summary>
        private void BindScoreTypeChart()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the score type data
                var scoreTypeDate = context.spGetOtherSEScreensByScoreType(string.Join(",", currentProgramRole.ProgramFKs), 
                                        DateTime.Now.AddYears(-1), DateTime.Now)
                                        .ToList();

                //Bind the chart to the data
                chartScoreType.DataSource = scoreTypeDate;
                chartScoreType.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for an OtherSEScreen
        /// and it deletes the OtherSEScreen screen from the database
        /// </summary>
        /// <param name="sender">The lbDeleteOtherSEScreen LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteOtherSEScreen_Click(object sender, EventArgs e)
        {
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeOtherSEScreenPK = String.IsNullOrWhiteSpace(hfDeleteOtherSEScreenPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteOtherSEScreenPK.Value);

                if (removeOtherSEScreenPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the other SE screen to remove
                            var otherSEScreenToRemove = context.OtherSEScreen.Where(oses => oses.OtherSEScreenPK == removeOtherSEScreenPK).FirstOrDefault();

                            //Remove the other SE screen from the database
                            context.OtherSEScreen.Remove(otherSEScreenToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.OtherSEScreenChanged
                                    .OrderByDescending(osesc => osesc.OtherSEScreenChangedPK)
                                    .Where(osesc => osesc.OtherSEScreenPK == otherSEScreenToRemove.OtherSEScreenPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the social emotional assessment!", 1000);

                            //Bind the gridview
                            bsGROtherSEScreen.DataBind();

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
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the social emotional assessment, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the social emotional assessment!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the social emotional assessment!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the social emotional assessment to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the OtherSEScreen DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efOtherSEScreenDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efOtherSEScreenDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "OtherSEScreenPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = from ose in context.OtherSEScreen.AsNoTracking()
                                    .Include(ose => ose.Program)
                                    .Include(ose => ose.Program.State)
                                    .Include(ose => ose.Child)
                                    .Include(ose => ose.CodeScoreType)
                                    .Include(ose => ose.CodeScreenType)
                                join cp in context.ChildProgram on ose.ChildFK equals cp.ChildFK
                                where currentProgramRole.ProgramFKs.Contains(ose.ProgramFK)
                                  && cp.ProgramFK == ose.ProgramFK
                                  && cp.EnrollmentDate <= ose.ScreenDate
                                  && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= ose.ScreenDate)
                                select new
                                {
                                    ose.OtherSEScreenPK,
                                    ScreenType = ose.CodeScreenType.Abbreviation,
                                    ose.ScreenDate,
                                    ose.Score,
                                    ChildID = cp.ProgramSpecificID,
                                    ChildIDAndName = (currentProgramRole.ViewPrivateChildInfo.Value ? 
                                                        "(" + cp.ProgramSpecificID + ") " + ose.Child.FirstName + " " + ose.Child.LastName :
                                                        cp.ProgramSpecificID),
                                    ose.Program.ProgramName,
                                    StateName = ose.Program.State.Name,
                                    ScoreType = ose.CodeScoreType.Description
                                };
        }
    }
}