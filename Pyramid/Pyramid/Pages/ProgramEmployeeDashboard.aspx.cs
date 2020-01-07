using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Pyramid.Code;
using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using System.Text.RegularExpressions;

namespace Pyramid.Pages
{
    public partial class ProgramEmployeeDashboard : System.Web.UI.Page
    {
        ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow aggregate viewers to see the action column
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (bsGREmployees.Columns.Count - 1);

                //Hide the action column
                bsGREmployees.Columns[actionColumnIndex].Visible = false;
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
                if (!String.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "EmployeeAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Employee successfully added!", 10000);
                            break;
                        case "EmployeeEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Employee successfully edited!", 10000);
                            break;
                        case "EmployeeCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoEmployee":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified employee could not be found, please try again.", 15000);
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
        /// This method fires when the data source for the employee DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efEmployeeDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efEmployeeDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "ProgramEmployeePK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = (from pe in context.ProgramEmployee.AsNoTracking().Include(pe => pe.CodeTermReason).Include(pe => pe.Program)
                                join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK into jobFunctions
                                where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                select new
                                {
                                    pe.ProgramEmployeePK,
                                    Name = pe.FirstName + " " + pe.LastName,
                                    pe.EmailAddress,
                                    JobFunctions = from jf in jobFunctions where jf.EndDate.HasValue == false select jf.CodeJobType.Description,
                                    pe.HireDate,
                                    pe.TermDate,
                                    TermReason = pe.CodeTermReason.Description + " " + (pe.TermReasonSpecify == null ? "" : "(" + pe.TermReasonSpecify + ")"),
                                    pe.ProgramFK,
                                    pe.Program.ProgramName
                                });
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a employee
        /// and it deletes the employee information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteEmployee LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteEmployee_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeEmployeePK = (String.IsNullOrWhiteSpace(hfDeleteEmployeePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteEmployeePK.Value));

                //Remove the role if the PK is not null
                if (removeEmployeePK != null)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the employee to remove
                            Models.ProgramEmployee employeeToRemove = context.ProgramEmployee.Find(removeEmployeePK);

                            //Get the trainings to remove
                            List<Training> trainingsToRemove = context.Training
                                                                    .Include(t => t.CodeTraining)
                                                                    .Where(t => t.ProgramEmployeeFK == removeEmployeePK
                                                                                && t.CodeTraining.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                                                                    .ToList();
                            context.Training.RemoveRange(trainingsToRemove);

                            //Get the job functions to remove and remove them
                            List<JobFunction> jobFunctionsToRemove = context.JobFunction
                                                                        .Include(jf => jf.CodeJobType)
                                                                        .Where(jf => jf.ProgramEmployeeFK == removeEmployeePK
                                                                                && jf.CodeJobType.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                                                                        .ToList();
                            context.JobFunction.RemoveRange(jobFunctionsToRemove);
                            
                            //Get the classroom assignments to remove
                            List<EmployeeClassroom> classroomAssignmentsToRemove = context.EmployeeClassroom
                                                                                        .Include(ec => ec.CodeJobType)
                                                                                        .Where(ec => ec.EmployeeFK == removeEmployeePK
                                                                                                && ec.CodeJobType.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                                                                                        .ToList();
                            context.EmployeeClassroom.RemoveRange(classroomAssignmentsToRemove);
                            
                            //Remove the employee
                            context.ProgramEmployee.Remove(employeeToRemove);
                            context.SaveChanges();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted employee!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", "Could not delete the employee, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the employee!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the employee!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }

                    //Rebind the employee controls
                    bsGREmployees.DataBind();
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the employee to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
    }
}