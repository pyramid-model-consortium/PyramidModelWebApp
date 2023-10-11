using Pyramid.Models;
using System;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Pyramid.Code;
using System.Linq;

namespace Pyramid.Pages
{
    public partial class ClassroomDashboard : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CLASS";
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
                int actionColumnIndex = (bsGRClassrooms.Columns.Count - 1);

                //Hide the action column
                bsGRClassrooms.Columns[actionColumnIndex].Visible = false;
            }

            //Show/hide the state column based on the number of states accessible to the user
            bsGRClassrooms.Columns["StateNameColumn"].Visible = (currentProgramRole.StateFKs.Count > 1);

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
                if (!String.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "ClassroomAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Classroom successfully added!", 10000);
                            break;
                        case "ClassroomEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Classroom successfully edited!", 10000);
                            break;
                        case "ClassroomCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoClassroom":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified classroom could not be found, please try again.", 15000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Bind the chart
                BindClassroomChart();
            }
        }

        /// <summary>
        /// This method fills the Classrooms covered by substitutes chart
        /// </summary>
        private void BindClassroomChart()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the chart data
                var classroomInfo = context.spGetClassroomCountBySubstituteStatus(string.Join(",", currentProgramRole.ProgramFKs)).ToList();

                //Bind the chart to the data
                chartClassroomType.DataSource = classroomInfo;
                chartClassroomType.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the data source for the classroom DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efClassroomDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efClassroomDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "ClassroomPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.Classroom.AsNoTracking()
                                        .Include(c => c.Program)
                                        .Include(c => c.Program.State)
                                        .Where(c => currentProgramRole.ProgramFKs.Contains(c.ProgramFK));
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a classroom
        /// and it deletes the classroom information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteClassroom LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteClassroom_Click(object sender, EventArgs e)
        {
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeClassroomPK = (String.IsNullOrWhiteSpace(hfDeleteClassroomPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteClassroomPK.Value));

                //Remove the role if the PK is not null
                if (removeClassroomPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the classroom program row to remove
                            Models.Classroom classroomToRemove = context.Classroom.Find(removeClassroomPK);

                            //Remove the classroom
                            context.Classroom.Remove(classroomToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.ClassroomChanged
                                    .OrderByDescending(cc => cc.ClassroomChangedPK)
                                    .Where(cc => cc.ClassroomPK == classroomToRemove.ClassroomPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted classroom!", 10000);
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
                                string messageForUser = "";

                                if (errorMessage.Contains("behaviorincident"))
                                {
                                    messageForUser = "there are Behavior Incident Reports entered for this classroom!";
                                }
                                else if (errorMessage.Contains("childclassroom"))
                                {
                                    messageForUser = "there are children assigned to this classroom!";
                                }
                                else if (errorMessage.Contains("employeeclassroom"))
                                {
                                    messageForUser = "there are employees assigned to this classroom!";
                                }
                                else if (errorMessage.Contains("tpitos"))
                                {
                                    messageForUser = "there are TPITOS observations entered for this classroom!";
                                }
                                else if (errorMessage.Contains("tpot"))
                                {
                                    messageForUser = "there are TPOT observations entered for this classroom!";
                                }
                                else
                                {
                                    messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                                }

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the classroom, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the classroom!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the classroom!", 120000);
                        }

                        //Log the error
                        Utilities.LogException(dbUpdateEx);
                    }

                    //Rebind the classroom controls
                    bsGRClassrooms.DataBind();

                    //Bind the chart
                    BindClassroomChart();
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the classroom to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
    }
}