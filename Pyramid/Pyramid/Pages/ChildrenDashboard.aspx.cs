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
    public partial class ChildrenDashboard : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "CHILD";
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
                int actionColumnIndex = (bsGRChildren.Columns.Count - 1);

                //Hide the action column
                bsGRChildren.Columns[actionColumnIndex].Visible = false;
            }

            //Don't allow certain users to see the child's DOB
            if(currentProgramRole.ViewPrivateChildInfo == false)
            {
                //Hide the birth date column
                bsGRChildren.Columns["BirthDateColumn"].Visible = false;
            }

            //Show/hide the state column based on the number of states accessible to the user
            bsGRChildren.Columns["StateNameColumn"].Visible = (currentProgramRole.StateFKs.Count > 1);

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
            e.QueryableSource = from c in context.Child.AsNoTracking()
                                                .Include(c => c.ChildProgram)
                                join cp in context.ChildProgram.AsNoTracking()
                                                .Include(cp => cp.CodeDischargeReason)
                                                .Include(cp => cp.Program) 
                                                .Include(cp => cp.Program.State)
                                    on c.ChildPK equals cp.ChildFK
                                where currentProgramRole.ProgramFKs.Contains(cp.ProgramFK)
                                select new {
                                    c.ChildPK,
                                    Name = (currentProgramRole.ViewPrivateChildInfo.Value ? 
                                                c.FirstName + " " + c.LastName :
                                                "HIDDEN"),
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
                                    cp.Program.ProgramName,
                                    StateName = cp.Program.State.Name
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
            if (FormPermissions.AllowedToDelete)
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

                            //Save all the deletions to the database
                            context.SaveChanges();

                            //To hold lists of change rows
                            List<ChildNoteChanged> noteChangeRows;
                            List<ChildStatusChanged> statusChangeRows;
                            List<ChildClassroomChanged> assignmentChangeRows;

                            //Check the note deletions
                            if (notesToRemove.Count > 0) 
                            {
                                //Get the note deletion rows and set the deleter
                                noteChangeRows = context.ChildNoteChanged.Where(cnc => cnc.ChildFK == childToRemove.ChildPK)
                                                                                        .OrderByDescending(cnc => cnc.ChildNoteChangedPK)
                                                                                        .Take(notesToRemove.Count).ToList()
                                                                                        .Select(cnc => { cnc.Deleter = User.Identity.Name; return cnc; }).ToList();
                            }

                            //Check the status deletions
                            if (statusToRemove.Count > 0) 
                            {
                                //Get the status deletion rows and set the deleter
                                statusChangeRows = context.ChildStatusChanged.Where(csc => csc.ChildFK == childToRemove.ChildPK)
                                                                                        .OrderByDescending(csc => csc.ChildStatusChangedPK)
                                                                                        .Take(statusToRemove.Count).ToList()
                                                                                        .Select(csc => { csc.Deleter = User.Identity.Name; return csc; }).ToList();
                            }

                            //Check the classroom assignment deletions
                            if (classroomAssignmentsToRemove.Count > 0) 
                            {
                                //Get the classroom assignment deletion rows and set the deleter
                                assignmentChangeRows = context.ChildClassroomChanged.Where(ccc => ccc.ChildFK == childToRemove.ChildPK)
                                                                                        .OrderByDescending(ccc => ccc.ChildClassroomChangedPK)
                                                                                        .Take(classroomAssignmentsToRemove.Count).ToList()
                                                                                        .Select(ccc => { ccc.Deleter = User.Identity.Name; return ccc; }).ToList();
                            }

                            //Get the child program delete row and set the deleter
                            context.ChildProgramChanged
                                    .OrderByDescending(cpc => cpc.ChildProgramChangedPK)
                                    .Where(cpc => cpc.ChildProgramPK == childProgramToRemove.ChildProgramPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Get the child delete row and set the deleter
                            context.ChildChanged
                                    .OrderByDescending(cc => cc.ChildChangedPK)
                                    .Where(cc => cc.ChildPK == childToRemove.ChildPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete row changes to the database
                            context.SaveChanges();

                            //Check to see if the parent permission file exists
                            if (!string.IsNullOrWhiteSpace(childProgramToRemove.ParentPermissionDocumentFileName))
                            {
                                //Delete the parent permission file
                                Utilities.DeleteFileFromAzureStorage(childProgramToRemove.ParentPermissionDocumentFileName,
                                    Utilities.ConstantAzureStorageContainerName.CHILD_FORM_UPLOADS.ToString());
                            }

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
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "";

                                if (errorMessage.Contains("asqse"))
                                {
                                    messageForUser = "there are ASQSEs entered for this child!";
                                }
                                else if (errorMessage.Contains("behaviorincident"))
                                {
                                    messageForUser = "there are Behavior Incident Reports entered for this child!";
                                }
                                else if (errorMessage.Contains("othersescreen"))
                                {
                                    messageForUser = "there are social emotional assessments entered for this child!";
                                }
                                else
                                {
                                    messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";
                                }

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the child, {0}", messageForUser), 120000);
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
                        Utilities.LogException(dbUpdateEx);
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