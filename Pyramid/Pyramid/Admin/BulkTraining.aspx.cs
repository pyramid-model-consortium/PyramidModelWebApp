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
using DevExpress.Web.Data;
using System.Data.Entity.SqlServer;
using System.Data;

namespace Pyramid.Admin
{
    public partial class BulkTraining : System.Web.UI.Page
    {
        ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow non-admins to use the page
            if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN &&
                    currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN)
            {
                //Kick out any non-admins
                Response.Redirect("/Default.aspx");
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
                                 join jf in context.JobFunction.AsNoTracking().Include(jf => jf.CodeJobType) on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK into jobFunctions
                                 join t in context.Training.AsNoTracking().Include(t => t.CodeTraining) on pe.ProgramEmployeePK equals t.ProgramEmployeeFK into trainings
                                 where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                 select new
                                 {
                                     pe.ProgramEmployeePK,
                                     Name = pe.FirstName + " " + pe.LastName,
                                     pe.EmailAddress,
                                     JobFunctions = from jf in jobFunctions where jf.EndDate.HasValue == false select jf.CodeJobType.Description,
                                     Trainings = from t in trainings where t.TrainingCodeFK == (int)Utilities.TrainingFKs.INTRODUCTION_TO_COACHING ||
                                                                           t.TrainingCodeFK == (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING ||
                                                                           t.TrainingCodeFK == (int)Utilities.TrainingFKs.TPITOS_OBSERVER ||
                                                                           t.TrainingCodeFK == (int)Utilities.TrainingFKs.TPOT_OBSERVER
                                                                           orderby t.TrainingDate ascending
                                                                           select (t.CodeTraining.Abbreviation + ": " + SqlFunctions.DatePart("month", t.TrainingDate) + "/" + SqlFunctions.DatePart("day", t.TrainingDate) + "/" + SqlFunctions.DatePart("year", t.TrainingDate)),
                                     pe.HireDate,
                                     pe.TermDate,
                                     TermReason = pe.CodeTermReason.Description + " " + (pe.TermReasonSpecify == null ? "" : "(" + pe.TermReasonSpecify + ")"),
                                     pe.ProgramFK,
                                     pe.Program.ProgramName
                                 });
        }

        /// <summary>
        /// This method fires when the user clicks the save button for a row and it saves the trainings
        /// for the user to the database
        /// </summary>
        /// <param name="sender">The lbSaveTrainings LinkButton</param>
        /// <param name="e">The click event</param>
        protected void lbSaveTrainings_Click(object sender, EventArgs e)
        {
            try
            {
                //To hold the training dates
                DateTime? PBCDate = null, ICECPDate = null, TPOTDate = null, TPITOSDate = null;

                //Get the calling button
                LinkButton thisButton = (LinkButton)sender;

                //Get the calling button container
                GridViewDataItemTemplateContainer buttonContainer = (GridViewDataItemTemplateContainer)thisButton.NamingContainer;

                //Get the DateEdits from the row in which the button resides
                BootstrapDateEdit dePBC = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "dePBCDate");
                BootstrapDateEdit deICECPDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "deICECPDate");
                BootstrapDateEdit deTPOTTrainingDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["ObserverColumn"], "deTPOTTrainingDate");
                BootstrapDateEdit deTPITOSTrainingDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["ObserverColumn"], "deTPITOSTrainingDate");

                //Get the employee PK and name
                int employeePK = Convert.ToInt32(bsGREmployees.GetRowValues(buttonContainer.VisibleIndex, "ProgramEmployeePK"));
                string employeeName = Convert.ToString(bsGREmployees.GetRowValues(buttonContainer.VisibleIndex, "Name"));

                //Get the training dates from the DateEdits
                PBCDate = (dePBC.Value == null ? (DateTime?)null : Convert.ToDateTime(dePBC.Value));
                ICECPDate = (deICECPDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deICECPDate.Value));
                TPOTDate = (deTPOTTrainingDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTPOTTrainingDate.Value));
                TPITOSDate = (deTPITOSTrainingDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTPITOSTrainingDate.Value));

                //Only continue if the employee PK has a value
                if (employeePK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //To hold the training acronyms of the added trainings
                        List<string> trainingsAdded = new List<string>();

                        //Check each training date to see if it was added
                        if (PBCDate.HasValue)
                        {
                            //Add the PBC training to the database
                            Training PBCTraining = new Training();
                            PBCTraining.CreateDate = DateTime.Now;
                            PBCTraining.Creator = User.Identity.Name;
                            PBCTraining.ProgramEmployeeFK = employeePK;
                            PBCTraining.TrainingCodeFK = (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING;
                            PBCTraining.TrainingDate = PBCDate.Value;
                            context.Training.Add(PBCTraining);

                            //Add the PBC training to the list
                            trainingsAdded.Add("PBC");
                        }
                        if (ICECPDate.HasValue)
                        {
                            //Add the ICECP training to the database
                            Training ICECPTraining = new Training();
                            ICECPTraining.CreateDate = DateTime.Now;
                            ICECPTraining.Creator = User.Identity.Name;
                            ICECPTraining.ProgramEmployeeFK = employeePK;
                            ICECPTraining.TrainingCodeFK = (int)Utilities.TrainingFKs.INTRODUCTION_TO_COACHING;
                            ICECPTraining.TrainingDate = ICECPDate.Value;
                            context.Training.Add(ICECPTraining);

                            //Add the ICECP training to the list
                            trainingsAdded.Add("ICECP");
                        }
                        if (TPOTDate.HasValue)
                        {
                            //Add the TPOT observer training to the database
                            Training TPOTTraining = new Training();
                            TPOTTraining.CreateDate = DateTime.Now;
                            TPOTTraining.Creator = User.Identity.Name;
                            TPOTTraining.ProgramEmployeeFK = employeePK;
                            TPOTTraining.TrainingCodeFK = (int)Utilities.TrainingFKs.TPOT_OBSERVER;
                            TPOTTraining.TrainingDate = TPOTDate.Value;
                            context.Training.Add(TPOTTraining);

                            //Add the TPOT observer training to the list
                            trainingsAdded.Add("TPOT Observer");
                        }
                        if (TPITOSDate.HasValue)
                        {
                            //Add the TPITOS observer training to the database
                            Training TPITOSTraining = new Training();
                            TPITOSTraining.CreateDate = DateTime.Now;
                            TPITOSTraining.Creator = User.Identity.Name;
                            TPITOSTraining.ProgramEmployeeFK = employeePK;
                            TPITOSTraining.TrainingCodeFK = (int)Utilities.TrainingFKs.TPITOS_OBSERVER;
                            TPITOSTraining.TrainingDate = TPITOSDate.Value;
                            context.Training.Add(TPITOSTraining);

                            //Add the TPITOS observer training to the list
                            trainingsAdded.Add("TPITOS Observer");
                        }

                        //Check to see if any changes were made
                        if (trainingsAdded.Count > 0)
                        {
                            //Save the trainings to the database
                            context.SaveChanges();

                            //Refresh the gridview
                            bsGREmployees.DataBind();

                            //Show the user a message
                            msgSys.ShowMessageToUser("success", "Trainings Added", string.Join(",", trainingsAdded) + " trainings were added for " + employeeName, 10000);

                        }
                        else
                        {
                            //Show the user a warning
                            msgSys.ShowMessageToUser("warning", "No Dates Detected", "No training dates were entered, and no changes have been saved.", 5000);
                        }
                    }
                }
                else
                {
                    //Show the user a warning
                    msgSys.ShowMessageToUser("warning", "No Employee Info Found", "The system was unable to determine the employee, please try again.", 5000);
                }
            }
            catch(Exception ex)
            {
                //Log the exception
                Utilities.LogException(ex);

                //Show the user the message
                msgSys.ShowMessageToUser("danger", "Error!", "An error occurred.  Message: " + ex.Message, 5000);
            }
        }
    }
}