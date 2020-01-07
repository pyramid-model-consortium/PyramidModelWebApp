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

namespace Pyramid.Pages
{
    public partial class ChildrenDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRChildren.Columns.Count - 1);

                //Hide the action column
                bsGRChildren.Columns[actionColumnIndex].Visible = false;
            }

            if (!IsPostBack)
            {
                //Set the view only value
                if(currentProgramRole.AllowedToEdit.Value)
                {
                    hfViewOnly.Value = "False";
                }
                else
                {
                    hfViewOnly.Value = "True";
                }

                //Populate the chart
                BindRaceChart();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if(!String.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "ChildAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Child successfully added!", 10000);
                            break;
                        case "ChildEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Child successfully edited!", 10000);
                            break;
                        case "ChildCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoChild":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified child could not be found, please try again.", 15000);
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
        /// This method fills the Children by Race chart with data
        /// </summary>
        private void BindRaceChart()
        {
            using(PyramidContext context = new PyramidContext())
            {
                //Get the chart data
                var raceData = context.spGetChildrenCountByRace(string.Join(",", currentProgramRole.ProgramFKs), DateTime.Now).ToList();

                //Bind the chart to the data
                chartChildRace.DataSource = raceData;
                chartChildRace.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the data source for the child DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efChildDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efChildDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "ChildProgramPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = from c in context.Child.Include(c => c.ChildProgram).AsNoTracking()
                                join cp in context.ChildProgram.Include(cp => cp.CodeDischargeReason).Include(cp => cp.Program) on c.ChildPK equals cp.ChildFK
                                where currentProgramRole.ProgramFKs.Contains(cp.ProgramFK)
                                select new {
                                    c.ChildPK,
                                    Name = c.FirstName + " " + c.LastName,
                                    c.BirthDate,
                                    cp.ChildProgramPK,
                                    cp.ProgramSpecificID,
                                    cp.HasIEP,
                                    cp.IsDLL,
                                    cp.EnrollmentDate,
                                    cp.DischargeDate,
                                    DischargeReason = cp.CodeDischargeReason.Description,
                                    cp.DischargeReasonSpecify,
                                    cp.ProgramFK,
                                    cp.Program.ProgramName
                                };
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a child
        /// and it deletes the child information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteChild LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteChild_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeChildProgramPK = (String.IsNullOrWhiteSpace(hfDeleteChildProgramPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteChildProgramPK.Value));

                //Remove the role if the PK is not null
                if (removeChildProgramPK != null)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the child program row to remove
                            ChildProgram childProgramToRemove = context.ChildProgram
                                                                    .Find(removeChildProgramPK);

                            //Get the child to remove
                            Models.Child childToRemove = context.Child
                                                            .Where(c => c.ChildPK == childProgramToRemove.ChildFK).FirstOrDefault();

                            //Get the notes to remove and remove them
                            List<ChildNote> notesToRemove = context.ChildNote.Where(cn => cn.ChildFK == childProgramToRemove.ChildFK).ToList();
                            context.ChildNote.RemoveRange(notesToRemove);

                            //Get the status rows to remove and remove them
                            List<ChildStatus> statusToRemove = context.ChildStatus.Where(cs => cs.ChildFK == childProgramToRemove.ChildFK).ToList();
                            context.ChildStatus.RemoveRange(statusToRemove);

                            //Get the classroom assignments to remove and remove them
                            List<ChildClassroom> classroomAssignmentsToRemove = context.ChildClassroom.Where(cc => cc.ChildFK == childProgramToRemove.ChildFK).ToList();
                            context.ChildClassroom.RemoveRange(classroomAssignmentsToRemove);

                            //Remove the child program row
                            context.ChildProgram.Remove(childProgramToRemove);

                            //Remove the child
                            context.Child.Remove(childToRemove);

                            //Save all the changes to the database
                            context.SaveChanges();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted child!", 10000);
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
                                msgSys.ShowMessageToUser("danger", "Error", "Could not delete the child, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the child!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the child!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }

                    //Rebind the child controls
                    bsGRChildren.DataBind();
                    BindRaceChart();
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the child to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }
    }
}