using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.UI.WebControls;
using Pyramid.Code;
using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using System.Data.Entity.SqlServer;
using System.Data;

namespace Pyramid.Admin
{
    public partial class BulkTraining : System.Web.UI.Page
    {
        ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow non-admins to use the page
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN)
            {
                //Kick out anyone who isn't an admin or National Data Admin
                Response.Redirect("/Default.aspx");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Set visibility of NY-specific training badges
            if(currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK)
            {
                spanICECPInfo.Visible = true;
                spanPBCFCCInfo.Visible = true;
            }
            else
            {
                spanICECPInfo.Visible = false;
                spanPBCFCCInfo.Visible = false;
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
            e.QueryableSource = (from pe in context.ProgramEmployee.Include(pe => pe.Employee).Include(pe => pe.CodeTermReason).Include(pe => pe.Program).AsNoTracking()
                                 join jf in context.JobFunction.Include(jf => jf.CodeJobType).AsNoTracking() on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK into jobFunctions
                                 join t in context.Training.Include(t => t.CodeTraining).AsNoTracking() on pe.EmployeeFK equals t.EmployeeFK into trainings
                                 where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                 select new
                                 {
                                     pe.ProgramEmployeePK,
                                     pe.Employee.EmployeePK,
                                     EmployeeName = "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName,
                                     pe.Employee.EmailAddress,
                                     JobFunctions = from jf in jobFunctions where jf.EndDate.HasValue == false select jf.CodeJobType.Description,
                                     Trainings = from t in trainings where t.TrainingCodeFK == (int)Utilities.TrainingFKs.INTRODUCTION_TO_COACHING ||
                                                                           t.TrainingCodeFK == (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING ||
                                                                           t.TrainingCodeFK == (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING_FCC ||
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
                DateTime? PBCDate = null, PBCFCCDate = null, ICECPDate = null, TPOTDate = null, TPITOSDate = null;

                //Get the calling button
                LinkButton thisButton = (LinkButton)sender;

                //Get the calling button container
                GridViewDataItemTemplateContainer buttonContainer = (GridViewDataItemTemplateContainer)thisButton.NamingContainer;

                //Get the DateEdits from the row in which the button resides
                BootstrapDateEdit dePBC = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "dePBCDate");
                BootstrapDateEdit dePBCFCC = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "dePBCFCCDate");
                BootstrapDateEdit deICECPDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "deICECPDate");
                BootstrapDateEdit deTPOTTrainingDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["ObserverColumn"], "deTPOTTrainingDate");
                BootstrapDateEdit deTPITOSTrainingDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(buttonContainer.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["ObserverColumn"], "deTPITOSTrainingDate");

                //Get the program employee PK and name
                int programEmployeePK = Convert.ToInt32(bsGREmployees.GetRowValues(buttonContainer.VisibleIndex, "ProgramEmployeePK"));
                string employeeName = Convert.ToString(bsGREmployees.GetRowValues(buttonContainer.VisibleIndex, "EmployeeName"));

                //In order to get the employee PK, it must be a column in the gridview.  The column should be visible = false.
                int employeePK = Convert.ToInt32(bsGREmployees.GetRowValues(buttonContainer.VisibleIndex, "EmployeePK"));

                //Get the training dates from the DateEdits
                PBCDate = (dePBC.Value == null ? (DateTime?)null : Convert.ToDateTime(dePBC.Value));
                PBCFCCDate = (dePBCFCC.Value == null ? (DateTime?)null : Convert.ToDateTime(dePBCFCC.Value));
                ICECPDate = (deICECPDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deICECPDate.Value));
                TPOTDate = (deTPOTTrainingDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTPOTTrainingDate.Value));
                TPITOSDate = (deTPITOSTrainingDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deTPITOSTrainingDate.Value));

                //Only continue if the employee PKs have a value
                if (programEmployeePK > 0 && employeePK > 0)
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
                            PBCTraining.EmployeeFK = employeePK;
                            PBCTraining.TrainingCodeFK = (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING;
                            PBCTraining.TrainingDate = PBCDate.Value;
                            context.Training.Add(PBCTraining);

                            //Add the PBC training to the list
                            trainingsAdded.Add("PBC");
                        }
                        if (PBCFCCDate.HasValue)
                        {
                            //Add the PBCFCC training to the database
                            Training PBCFCCTraining = new Training();
                            PBCFCCTraining.CreateDate = DateTime.Now;
                            PBCFCCTraining.Creator = User.Identity.Name;
                            PBCFCCTraining.EmployeeFK = employeePK;
                            PBCFCCTraining.TrainingCodeFK = (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING_FCC;
                            PBCFCCTraining.TrainingDate = PBCFCCDate.Value;
                            context.Training.Add(PBCFCCTraining);

                            //Add the PBCFCC training to the list
                            trainingsAdded.Add("PBCFCC");
                        }
                        if (ICECPDate.HasValue)
                        {
                            //Add the ICECP training to the database
                            Training ICECPTraining = new Training();
                            ICECPTraining.CreateDate = DateTime.Now;
                            ICECPTraining.Creator = User.Identity.Name;
                            ICECPTraining.EmployeeFK = employeePK;
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
                            TPOTTraining.EmployeeFK = employeePK;
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
                            TPITOSTraining.EmployeeFK = employeePK;
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
                    msgSys.ShowMessageToUser("warning", "No Professional Info Found", "The system was unable to determine the professional, please try again.", 5000);
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

        /// <summary>
        /// This method fires when each row in the bsGREmployees GridView is created
        /// </summary>
        /// <param name="sender">The bsGREmployees GridView</param>
        /// <param name="e">The ASPxGridViewTableRowEventArgs</param>
        protected void bsGREmployees_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            if(e.RowType == GridViewRowType.Data)
            {
                //Get the DateEdits from the row
                BootstrapDateEdit dePBC = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "dePBCDate");
                BootstrapDateEdit dePBCFCC = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "dePBCFCCDate");
                BootstrapDateEdit deICECPDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["CoachColumn"], "deICECPDate");
                BootstrapDateEdit deTPOTTrainingDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["ObserverColumn"], "deTPOTTrainingDate");
                BootstrapDateEdit deTPITOSTrainingDate = (BootstrapDateEdit)bsGREmployees.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGREmployees.Columns["ObserverColumn"], "deTPITOSTrainingDate");

                //Set the max dates to today
                dePBC.MaxDate = DateTime.Now;
                dePBCFCC.MaxDate = DateTime.Now;
                deICECPDate.MaxDate = DateTime.Now;
                deTPOTTrainingDate.MaxDate = DateTime.Now;
                deTPITOSTrainingDate.MaxDate = DateTime.Now;

                //Hide NY-specific trainings if necessary
                if(currentProgramRole.CurrentStateFK.Value != (int)Utilities.StateFKs.NEW_YORK)
                {
                    deICECPDate.Visible = false;
                    dePBCFCC.Visible = false;
                }
            }
        }
    }
}