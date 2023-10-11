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
    public partial class ProgramEmployeeDashboard : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "PE";
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
        ProgramAndRoleFromSession currentProgramRole;

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
                int actionColumnIndex = (bsGREmployees.Columns.Count - 1);

                //Hide the action column
                bsGREmployees.Columns[actionColumnIndex].Visible = false;
            }

            //Show/hide the state column based on the number of states accessible to the user
            bsGREmployees.Columns["StateNameColumn"].Visible = (currentProgramRole.StateFKs.Count > 1);

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
                        case "EmployeeAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Professional successfully added!", 10000);
                            break;
                        case "EmployeeEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Professional successfully edited!", 10000);
                            break;
                        case "EmployeeCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoEmployee":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified professional could not be found, please try again.", 15000);
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
            e.QueryableSource = (from pe in context.ProgramEmployee
                                                .Include(pe => pe.Employee)
                                                .Include(pe => pe.CodeTermReason)
                                                .Include(pe => pe.Program)
                                                .Include(pe => pe.Program.State)
                                                .AsNoTracking()
                                join jf in context.JobFunction.AsNoTracking() on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK into jobFunctions
                                where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                select new
                                {
                                    pe.ProgramEmployeePK,
                                    pe.ProgramSpecificID,
                                    Name = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? pe.Employee.FirstName + " " + pe.Employee.LastName : "HIDDEN"),
                                    EmailAddress = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? pe.Employee.EmailAddress : "HIDDEN"),
                                    JobFunctions = from jf in jobFunctions where jf.EndDate.HasValue == false select jf.CodeJobType.Description,
                                    pe.HireDate,
                                    pe.TermDate,
                                    TermReason = pe.CodeTermReason.Description + " " + (pe.TermReasonSpecify == null ? "" : "(" + pe.TermReasonSpecify + ")"),
                                    pe.ProgramFK,
                                    pe.Program.ProgramName,
                                    StateName = pe.Program.State.Name
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
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeProgramEmployeePK = (String.IsNullOrWhiteSpace(hfDeleteEmployeePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteEmployeePK.Value));

                //Remove the role if the PK is not null
                if (removeProgramEmployeePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the employee to remove
                            Models.ProgramEmployee programEmployeeToRemove = context.ProgramEmployee.Include(pe => pe.Employee).Where(pe => pe.ProgramEmployeePK == removeProgramEmployeePK).FirstOrDefault();

                            bool isInOtherProgram = context.ProgramEmployee.AsNoTracking().Where(pe => pe.EmployeeFK == programEmployeeToRemove.EmployeeFK && pe.ProgramFK != programEmployeeToRemove.ProgramFK).Count() > 0;

                            List<Training> trainingsToRemove = new List<Training>();

                            //Only delete trainings if the employee doesn't exist in other programs
                            if (isInOtherProgram == false)
                            {
                                //Get the trainings to remove
                                trainingsToRemove = context.Training
                                                            .Include(t => t.CodeTraining)
                                                            .Where(t => t.EmployeeFK == programEmployeeToRemove.EmployeeFK)
                                                            .ToList();
                                context.Training.RemoveRange(trainingsToRemove);
                            }

                            //Get the job functions to remove and remove them
                            List<JobFunction> jobFunctionsToRemove = context.JobFunction
                                                                        .Include(jf => jf.CodeJobType)
                                                                        .Where(jf => jf.ProgramEmployeeFK == removeProgramEmployeePK)
                                                                        .ToList();
                            context.JobFunction.RemoveRange(jobFunctionsToRemove);
                            
                            //Get the classroom assignments to remove
                            List<EmployeeClassroom> classroomAssignmentsToRemove = context.EmployeeClassroom
                                                                                        .Include(ec => ec.CodeJobType)
                                                                                        .Where(ec => ec.ProgramEmployeeFK == removeProgramEmployeePK)
                                                                                        .ToList();
                            context.EmployeeClassroom.RemoveRange(classroomAssignmentsToRemove);

                            //Remove the Employee record if the employee was only in one program
                            if (isInOtherProgram == false)
                            {
                                context.Employee.Remove(programEmployeeToRemove.Employee);
                            }

                            //Remove the ProgramEmployee record
                            context.ProgramEmployee.Remove(programEmployeeToRemove);

                            //Save the deletions to the database
                            context.SaveChanges();

                            //To hold all the change rows
                            List<TrainingChanged> trainingChangeRows;
                            List<JobFunctionChanged> jobFunctionChangeRows;
                            List<EmployeeClassroomChanged> assignmentChangeRows;

                            //Check the training deletions
                            if (isInOtherProgram == false && trainingsToRemove.Count > 0)
                            {
                                //Get the training deletion rows and set the deleter
                                trainingChangeRows = context.TrainingChanged.Where(tc => tc.EmployeeFK == programEmployeeToRemove.EmployeeFK)
                                                                                        .OrderByDescending(tc => tc.TrainingChangedPK)
                                                                                        .Take(trainingsToRemove.Count).ToList()
                                                                                        .Select(tc => { tc.Deleter = User.Identity.Name; return tc; }).ToList();
                            }

                            //Check the job function deletions
                            if (jobFunctionsToRemove.Count > 0)
                            {
                                //Get the job function deletion rows and set the deleter
                                jobFunctionChangeRows = context.JobFunctionChanged.Where(jfc => jfc.ProgramEmployeeFK == programEmployeeToRemove.ProgramEmployeePK)
                                                                                        .OrderByDescending(jfc => jfc.JobFunctionChangedPK)
                                                                                        .Take(jobFunctionsToRemove.Count).ToList()
                                                                                        .Select(jfc => { jfc.Deleter = User.Identity.Name; return jfc; }).ToList();
                            }

                            //Check the classroom assignment deletions
                            if (classroomAssignmentsToRemove.Count > 0)
                            {
                                //Get the classroom assignment deletion rows and set the deleter
                                assignmentChangeRows = context.EmployeeClassroomChanged.Where(ecc => ecc.ProgramEmployeeFK == programEmployeeToRemove.ProgramEmployeePK)
                                                                                        .OrderByDescending(ecc => ecc.EmployeeClassroomChangedPK)
                                                                                        .Take(classroomAssignmentsToRemove.Count).ToList()
                                                                                        .Select(ecc => { ecc.Deleter = User.Identity.Name; return ecc; }).ToList();
                            }

                            //Get the ProgramEmployee delete row and set the deleter
                            context.ProgramEmployeeChanged
                                    .OrderByDescending(pec => pec.ProgramEmployeeChangedPK)
                                    .Where(pec => pec.ProgramEmployeePK == programEmployeeToRemove.ProgramEmployeePK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //If the employee was only in one program, it was deleted and the delete rows needs to be updated
                            if (isInOtherProgram == false)
                            {
                                //Get the Employee delete row and set the deleter
                                context.EmployeeChanged
                                        .OrderByDescending(ec => ec.EmployeeChangedPK)
                                        .Where(ec => ec.EmployeePK == programEmployeeToRemove.EmployeeFK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;
                            }

                            //Save the delete row changes to the database
                            context.SaveChanges();

                            //Show a success message
                            if (isInOtherProgram)
                            {
                                msgSys.ShowMessageToUser("success", "Success", "The professional has successfully been de-linked from your program!", 10000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("success", "Success", "Successfully deleted professional!", 10000);
                            }
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

                                if (errorMessage.Contains("coachinglog_coach"))
                                {
                                    messageForUser = "there are Classroom Coaching Logs in the system with this professional selected as a coach!";
                                }
                                else if (errorMessage.Contains("coachinglog_coachee"))
                                {
                                    messageForUser = "there are Classroom Coaching Logs in the system with this professional selected as a coachee!";
                                }
                                else if (errorMessage.Contains("tpotparticipant"))
                                {
                                    messageForUser = "there are TPOT observations in the system with this professional selected as a participant!";
                                }
                                else if (errorMessage.Contains("tpitosparticipant"))
                                {
                                    messageForUser = "there are TPITOS observations in the system with this professional selected as a participant!";
                                }
                                else if (errorMessage.Contains("tpot_programemployee"))
                                {
                                    messageForUser = "there are TPOT observations in the system with this professional selected as an observer!";
                                }
                                else if (errorMessage.Contains("tpitos_programemployee"))
                                {
                                    messageForUser = "there are TPITOS observations in the system with this professional selected as an observer!";
                                }
                                else
                                {
                                    messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                                }

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the professional, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the professional!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the professional!", 120000);
                        }

                        //Log the error
                        Utilities.LogException(dbUpdateEx);
                    }

                    //Rebind the employee controls
                    bsGREmployees.DataBind();
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the professional to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
    }
}