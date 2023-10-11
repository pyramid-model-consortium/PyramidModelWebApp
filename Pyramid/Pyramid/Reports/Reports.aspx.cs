using DevExpress.Web;
using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Web.UI;
using System.Linq;
using Pyramid.Reports.PreBuiltReports.MasterReports;
using System.Collections.Generic;
using System.Data.Entity;
using DevExpress.Web.Bootstrap;
using System.IO;
using System.Web;

namespace Pyramid.Reports
{
    public partial class Reports : System.Web.UI.Page
    {
        private List<CodeProgramRolePermission> currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            /* NOTE: I'm adding the scripts this way so that ASP.NET 
             * auto-versions them and users don't need to hard refresh
             * when the files change.
             */
            //Get the script manager
            ScriptManager scriptManager = ScriptManager.GetCurrent(Page);

            //Get the script reference for the report page script bundle
            ScriptReference reportPageScripts = new ScriptReference("~/bundles/ReportsPage");

            //Check to see if there is a reference to the report page script bundle
            if (scriptManager.Scripts.Where(s => s.Path == reportPageScripts.Path).FirstOrDefault() == null)
            {
                //There isn't a reference, add the bundle to the script manager's scripts
                scriptManager.Scripts.Add(reportPageScripts);
            }

            //Get the user's program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permissions for the user
            currentPermissions = Utilities.GetProgramRolePermissionsFromDatabase(currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Try to get the report div width supplied by JS
            double? reportDivWidth = (string.IsNullOrWhiteSpace(hfReportDivWidth.Value) ? (double?)null : Convert.ToDouble(hfReportDivWidth.Value));

            //Check to see if the width was supplied
            if(reportDivWidth.HasValue)
            {
                //Use mobile mode for the report viewer if the report div is under 1000 pixels wide
                if(reportDivWidth.Value < 1000)
                {
                    //Get the new height for the report viewer (sized for an 8x11 paper)
                    double newHeight = reportDivWidth.Value * 1.28;

                    //Use mobile mode
                    reportViewer.MobileMode = true;

                    //Set the report viewer height
                    reportViewer.Height = Convert.ToInt32(newHeight);
                }
                else
                {
                    reportViewer.MobileMode = false;
                }
            }

            if(!IsPostBack)
            {
                //Fill all the data bound controls
                FillDataBoundControls();

                //Set the hidden fields for the current program, hub and state FKs
                hfCurrentProgramFK.Value = (currentProgramRole.CurrentProgramFK.HasValue ? currentProgramRole.CurrentProgramFK.Value.ToString() : "");
                hfCurrentHubFK.Value = (currentProgramRole.CurrentHubFK.HasValue ? currentProgramRole.CurrentHubFK.Value.ToString() : "");
                hfCurrentStateFK.Value = (currentProgramRole.CurrentStateFK.HasValue ? currentProgramRole.CurrentStateFK.Value.ToString() : "");

                //Set the control visibility based on user role
                SetControlVisibilityAndUsability(currentProgramRole.CodeProgramRoleFK.Value, currentPermissions);
            }
        }

        /// <summary>
        /// This method populates the data bound controls on the page
        /// </summary>
        private void FillDataBoundControls()
        {
            //Fill the data bound controls

            //-------------- Year Combo Box --------------------
            //To hold the last 10 years
            List<Utilities.CustomDropDownSourceItem> years = new List<Utilities.CustomDropDownSourceItem>();

            //Get the current year
            int currentYear = DateTime.Now.Year;

            //Add 20 years to the list
            for (int i = 0; i <= 20; i++)
            {
                //Get the year value
                int yearValue = currentYear - i;

                //Get the year text
                string yearText = "January " + yearValue.ToString() + " - December " + yearValue.ToString();

                //Create an object that hold the year and text
                Utilities.CustomDropDownSourceItem yearItem =
                    new Utilities.CustomDropDownSourceItem(yearValue.ToString(), yearText);

                //Add the object to the list
                years.Add(yearItem);
            }

            //Bind the combo box
            ddYear.DataSource = years;
            ddYear.DataBind();

            using (PyramidContext context = new PyramidContext())
            {
                //--------------- Program List Box -----------------
                //Get all the programs for this user
                List<Program> allPrograms = context.Program.AsNoTracking()
                                                .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                                .OrderBy(p => p.ProgramName)
                                                .ToList();

                //Bind the program list box
                lstBxProgram.DataSource = allPrograms;
                lstBxProgram.DataBind();

                //--------------- Hub List Box -----------------
                //Get all the allowed hubs
                List<Hub> allHubs = context.Hub.AsNoTracking()
                                            .Where(h => currentProgramRole.HubFKs.Contains(h.HubPK))
                                            .OrderBy(h => h.Name)
                                            .ToList();

                //Bind the hub list box
                lstBxHub.DataSource = allHubs;
                lstBxHub.DataBind();

                //--------------- State List Box -----------------
                //Get all the states for this user
                var allStates = context.State.AsNoTracking()
                                                .Where(s => currentProgramRole.StateFKs.Contains(s.StatePK))
                                                .OrderBy(s => s.Name)
                                                .Select(s => new
                                                {
                                                    StatePK = s.StatePK,
                                                    StateName = s.Name
                                                })
                                                .ToList();

                //Bind the state list box
                lstBxState.DataSource = allStates;
                lstBxState.DataBind();

                //--------------- Cohort List Box -----------------
                //Get all the cohorts for this user
                List<Cohort> allCohorts = context.Cohort.AsNoTracking()
                                                .Where(c => currentProgramRole.CohortFKs.Contains(c.CohortPK))
                                                .OrderBy(c => c.CohortName)
                                                .ToList();

                //Bind the cohort list box
                lstBxCohort.DataSource = allCohorts;
                lstBxCohort.DataBind();

                //--------------- Classroom List Box -----------------
                //Get all the classrooms for this user
                var allClassrooms = context.Classroom.AsNoTracking()
                                                .Where(c => currentProgramRole.ProgramFKs.Contains(c.ProgramFK))
                                                .OrderBy(c => c.ProgramSpecificID)
                                                .Select(c => new
                                                {
                                                    ClassroomPK = c.ClassroomPK,
                                                    ClassroomName = "(" + c.ProgramSpecificID + ") " + c.Name
                                                })
                                                .ToList();

                //Bind the classroom list box
                lstBxClassroom.DataSource = allClassrooms;
                lstBxClassroom.DataBind();

                //--------------- Children List Box -----------------
                //Get all the children for this user
                var allChildren = context.ChildProgram.AsNoTracking()
                                                .Include(cp => cp.Child)
                                                .Where(cp => currentProgramRole.ProgramFKs.Contains(cp.ProgramFK))
                                                .OrderBy(cp => cp.ProgramSpecificID)
                                                .Select(cp => new
                                                {
                                                    ChildPK = cp.Child.ChildPK,
                                                    ChildName = (currentProgramRole.ViewPrivateChildInfo.Value ?
                                                                    "(" + cp.ProgramSpecificID + ") " + cp.Child.FirstName + " " + cp.Child.LastName :
                                                                    cp.ProgramSpecificID)
                                                })
                                                .ToList();

                //Bind the child list box
                lstBxChild.DataSource = allChildren;
                lstBxChild.DataBind();

                //--------------- Race List Box -----------------
                //Get all the races
                var allRaces = context.CodeRace.AsNoTracking()
                                                .OrderBy(cr => cr.OrderBy)
                                                .ToList();

                //Bind the race list box
                lstBxRace.DataSource = allRaces;
                lstBxRace.DataBind();

                //--------------- Ethnicity List Box -----------------
                //Get all the ethnicities
                var allEthnicities = context.CodeEthnicity.AsNoTracking()
                                                .OrderBy(cr => cr.OrderBy)
                                                .ToList();

                //Bind the race list box
                lstBxEthnicity.DataSource = allEthnicities;
                lstBxEthnicity.DataBind();

                //--------------- Gender List Box -----------------

                //Get all the genders
                var allGenders = context.CodeGender.AsNoTracking()
                                                .OrderBy(cg => cg.OrderBy)
                                                .ToList();

                //Bind the race list box
                lstBxGender.DataSource = allGenders;
                lstBxGender.DataBind();

                //--------------- Employee List Box -----------------
                //Get all the employees for this user
                var allEmployees = context.ProgramEmployee
                                                .Include(pe => pe.Employee)
                                                .AsNoTracking()
                                                .Where(pe => currentProgramRole.ProgramFKs.Contains(pe.ProgramFK))
                                                .OrderBy(pe => pe.Employee.FirstName)
                                                .ThenBy(pe => pe.Employee.LastName)
                                                .Select(pe => new
                                                {
                                                    ProgramEmployeePK = pe.ProgramEmployeePK,
                                                    ProgramEmployeeName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName : pe.ProgramSpecificID)
                                                })
                                                .ToList();

                //Bind the employee list box
                lstBxEmployee.DataSource = allEmployees;
                lstBxEmployee.DataBind();

                //--------------- Teacher List Box -----------------
                //Get all the teachers for this user
                var allTeachers = (from pe in context.ProgramEmployee
                                                    .Include(pe => pe.Employee)
                                                    .Include(pe => pe.JobFunction)
                                                    .AsNoTracking()
                                   join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK
                                   where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                     && jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHER
                                   orderby pe.Employee.FirstName ascending, pe.Employee.LastName ascending
                                   select new
                                   {
                                       ProgramEmployeePK = pe.ProgramEmployeePK,
                                       ProgramEmployeeName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName : pe.ProgramSpecificID)
                                   })
                                  .Distinct()
                                  .ToList();

                //Bind the teacher list box
                lstBxTeacher.DataSource = allTeachers;
                lstBxTeacher.DataBind();

                //--------------- Coach List Box -----------------
                //Get all the coaches for this user
                var allCoaches = (from pe in context.ProgramEmployee
                                                    .Include(pe => pe.Employee)
                                                    .Include(pe => pe.JobFunction)
                                                    .AsNoTracking()
                                  join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK
                                  join t in context.Training on pe.EmployeeFK equals t.EmployeeFK
                                  where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                    && jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.CLASSROOM_COACH
                                    && (t.TrainingCodeFK == (int)Utilities.TrainingFKs.INTRODUCTION_TO_COACHING
                                          || t.TrainingCodeFK == (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING
                                          || t.TrainingCodeFK == (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING_FCC)
                                  orderby pe.Employee.FirstName ascending, pe.Employee.LastName ascending
                                  select new
                                  {
                                      ProgramEmployeePK = pe.ProgramEmployeePK,
                                      ProgramEmployeeName = (currentProgramRole.ViewPrivateEmployeeInfo.Value ? "(" + pe.ProgramSpecificID + ") " + pe.Employee.FirstName + " " + pe.Employee.LastName : pe.ProgramSpecificID)
                                  })
                                  .Distinct()
                                  .ToList();

                //Bind the coach list box
                lstBxCoach.DataSource = allCoaches;
                lstBxCoach.DataBind();

                //--------------- Problem Behavior List Box -----------------
                //Get all the problem behaviors
                var allProblemBehaviors = context.CodeProblemBehavior.AsNoTracking()
                                                        .OrderBy(cpb => cpb.Abbreviation)
                                                        .ToList();

                //Bind the problem behavior list box
                lstBxProblemBehavior.DataSource = allProblemBehaviors;
                lstBxProblemBehavior.DataBind();

                //--------------- Activity List Box -----------------
                //Get all the activity
                var allActivities = context.CodeActivity.AsNoTracking()
                                                        .OrderBy(cpb => cpb.Abbreviation)
                                                        .ToList();

                //Bind the activity list box
                lstBxActivity.DataSource = allActivities;
                lstBxActivity.DataBind();

                //--------------- Others Involved List Box -----------------
                //Get all the Others Involved
                var allOthersInvolved = context.CodeOthersInvolved.AsNoTracking()
                                                        .OrderBy(cpb => cpb.Abbreviation)
                                                        .ToList();

                //Bind the Others Involved list box
                lstBxOthersInvolved.DataSource = allOthersInvolved;
                lstBxOthersInvolved.DataBind();

                //--------------- Possible Motivation List Box -----------------
                //Get all the Possible Motivations
                var allPossibleMotivations = context.CodePossibleMotivation.AsNoTracking()
                                                        .OrderBy(cpb => cpb.Abbreviation)
                                                        .ToList();

                //Bind the Possible Motivation list box
                lstBxPossibleMotivation.DataSource = allPossibleMotivations;
                lstBxPossibleMotivation.DataBind();

                //--------------- Strategy Response List Box -----------------
                //Get all the Strategy Responses
                var allStrategyResponses = context.CodeStrategyResponse.AsNoTracking()
                                                        .OrderBy(cpb => cpb.Abbreviation)
                                                        .ToList();

                //Bind the Strategy Response list box
                lstBxStrategyResponse.DataSource = allStrategyResponses;
                lstBxStrategyResponse.DataBind();

                //--------------- Admin Follow-up List Box -----------------
                //Get all the Admin Follow-ups
                var allAdminFollowUps = context.CodeAdminFollowUp.AsNoTracking()
                                                        .OrderBy(cpb => cpb.Abbreviation)
                                                        .ToList();

                //Bind the Admin Follow-up list box
                lstBxAdminFollowUp.DataSource = allAdminFollowUps;
                lstBxAdminFollowUp.DataBind();
            }
        }

        /// <summary>
        /// This method shows/hides controls based on the user's role
        /// </summary>
        /// <param name="roleFK">The role FK</param>
        private void SetControlVisibilityAndUsability(int roleFK, List<CodeProgramRolePermission> permissions)
        {
            //Hide detailed criteria if this is an aggregate role
            if (permissions.Where(p => p.CodeForm.FormAbbreviation == "CHILD").FirstOrDefault().AllowedToView == false)
            {
                //Child
                lstBxChild.ClientVisible = false;
                lstBxChild.ClientReadOnly = true;
                lstBxChild.ReadOnly = true;

                //Ethnicity
                lstBxEthnicity.ClientVisible = false;
                lstBxEthnicity.ClientReadOnly = true;
                lstBxEthnicity.ReadOnly = true;

                //Gender
                lstBxGender.ClientVisible = false;
                lstBxGender.ClientReadOnly = true;
                lstBxGender.ReadOnly = true;

                //Race
                lstBxRace.ClientVisible = false;
                lstBxRace.ClientReadOnly = true;
                lstBxRace.ReadOnly = true;

                //IEP
                ddIEP.ClientVisible = false;
                ddIEP.ClientReadOnly = true;
                ddIEP.ReadOnly = true;

                //DLL
                ddDLL.ClientVisible = false;
                ddDLL.ClientReadOnly = true;
                ddDLL.ReadOnly = true;
            }

            if (permissions.Where(p => p.CodeForm.FormAbbreviation == "CLASS").FirstOrDefault().AllowedToView == false)
            {
                //Classroom
                lstBxClassroom.ClientVisible = false;
                lstBxClassroom.ClientReadOnly = true;
                lstBxClassroom.ReadOnly = true;
            }

            if (permissions.Where(p => p.CodeForm.FormAbbreviation == "PE").FirstOrDefault().AllowedToView == false)
            {
                //Coach
                lstBxCoach.ClientVisible = false;
                lstBxCoach.ClientReadOnly = true;
                lstBxCoach.ReadOnly = true;

                //Teacher
                lstBxTeacher.ClientVisible = false;
                lstBxTeacher.ClientReadOnly = true;
                lstBxTeacher.ReadOnly = true;

                //Employee
                lstBxEmployee.ClientVisible = false;
                lstBxEmployee.ClientReadOnly = true;
                lstBxEmployee.ReadOnly = true;
            }

            if(roleFK == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
            {
                //Hide nothing
            }
            else if(roleFK == (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.STATE_AGGREGATE_VIEWER
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.STATE_DATA_COLLECTOR
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.STATE_DETAIL_DATA_VIEWER)
            {
                //Hide nothing
            }
            else if (roleFK == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                     roleFK == (int)Utilities.CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER)
            {
                //National role

                //Hide and disable the program list box
                lstBxProgram.ClientVisible = false;
                lstBxProgram.ClientReadOnly = true;
                lstBxProgram.ReadOnly = true;

                //Hide and disable the cohort list box
                lstBxCohort.ClientVisible = false;
                lstBxCohort.ClientReadOnly = true;
                lstBxCohort.ReadOnly = true;

            }
            else if (roleFK == (int)Utilities.CodeProgramRoleFKs.HUB_DETAIL_DATA_VIEWER
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.HUB_DATA_COLLECTOR
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.HUB_AGGREGATE_DATA_VIEWER
                        || roleFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH)
            {
                //Hide and disable the state list box
                lstBxState.ClientVisible = false;
                lstBxState.ClientReadOnly = true;
                lstBxState.ReadOnly = true;

                //Hide and disable the cohort list box
                lstBxCohort.ClientVisible = false;
                lstBxCohort.ClientReadOnly = true;
                lstBxCohort.ReadOnly = true;
            }
            else
            {
                //All other program-specific roles

                //Hide and disable the state list box
                lstBxState.ClientVisible = false;
                lstBxState.ClientReadOnly = true;
                lstBxState.ReadOnly = true;

                //Hide and disable the cohort list box
                lstBxCohort.ClientVisible = false;
                lstBxCohort.ClientReadOnly = true;
                lstBxCohort.ReadOnly = true;

                //Hide and disable the hub list box
                lstBxHub.ClientVisible = false;
                lstBxHub.ClientReadOnly = true;
                lstBxHub.ReadOnly = true;
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Run Report button and it
        /// runs the report, hides the report gridview, and shows the report viewer
        /// </summary>
        /// <param name="sender">The btnRunReport DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnRunReport_Click(object sender, EventArgs e)
        {
            //Get the report to run class, criteria options, and name
            string reportToRunClass = hfReportToRunClass.Value;
            string reportCriteriaOptions = hfReportToRunCriteriaOptions.Value;
            string reportOptionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;
            string reportName = hfReportToRunName.Value;
            int reportPK = (string.IsNullOrWhiteSpace(hfReportToRunPK.Value) ? 0 : Convert.ToInt32(hfReportToRunPK.Value));

            //Set the running report hidden field
            hfRunningReport.Value = "true";

            //Only continue if a report was selected
            if (!string.IsNullOrWhiteSpace(reportToRunClass))
            {
                //Only continue if the page is valid
                if (ASPxEdit.AreEditorsValid(this, btnRunReport.ValidationGroup))
                {
                    try
                    {
                        //Get the report type
                        Type reportType = Type.GetType("Pyramid.Reports.PreBuiltReports.Rpt" + reportToRunClass);

                        //Get the constructor for the report
                        System.Reflection.ConstructorInfo CtorInfo = reportType.GetConstructor(Type.EmptyTypes);

                        //Get the report from the constructor
                        RptLogoMaster report = (RptLogoMaster)(CtorInfo.Invoke(new object[] { }));

                        //Set parameters
                        FillReport(ref report, reportCriteriaOptions, reportOptionalCriteriaOptions, reportName);

                        //Open the report
                        reportViewer.OpenReport(report);

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Create a user report history object for this user and report
                            UserReportHistory reportHistory = new UserReportHistory()
                            {
                                Username = User.Identity.Name,
                                RunDate = DateTime.Now,
                                ReportCatalogFK = reportPK
                            };

                            //Get the other report history rows
                            List<UserReportHistory> existingReportHistory = context.UserReportHistory
                                                                                        .Where(urh => urh.Username == reportHistory.Username && 
                                                                                               urh.ReportCatalogFK == reportHistory.ReportCatalogFK).ToList();

                            //Check to see if there are other report history rows
                            if (existingReportHistory.Count > 0)
                            {
                                //Delete the other rows from the database (we only want one)
                                context.UserReportHistory.RemoveRange(existingReportHistory);
                            }

                            //Add it to the DB and save the changes
                            context.UserReportHistory.Add(reportHistory);
                            context.SaveChanges();
                        }

                        //Refresh the table
                        bsGVReports.DataBind();
                    }
                    catch(Exception ex)
                    {
                        //An error occurred, show the user a message
                        msgSys.ShowMessageToUser("danger", "An Error Occurred", "Error: <br/>" + ex.Message, 10000);

                        //Log the exception in the database
                        Utilities.LogException(ex);
                    }
                }
                else
                {
                    //Validation failed, show a message
                    msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Export Report button and it
        /// runs the report and downloads it to the user
        /// </summary>
        /// <param name="sender">The btnExportReport DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnExportReport_Click(object sender, EventArgs e)
        {
            //Get the report to run class, criteria options, and name
            string reportToRunClass = hfReportToRunClass.Value;
            string reportCriteriaOptions = hfReportToRunCriteriaOptions.Value;
            string reportOptionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;
            string reportName = hfReportToRunName.Value;
            int reportPK = (string.IsNullOrWhiteSpace(hfReportToRunPK.Value) ? 0 : Convert.ToInt32(hfReportToRunPK.Value));

            //Only continue if a report was selected
            if (!string.IsNullOrWhiteSpace(reportToRunClass))
            {
                //Only continue if the page is valid
                if (ASPxEdit.AreEditorsValid(this, btnExportReport.ValidationGroup))
                {
                    try
                    {
                        //Get the report type
                        Type reportType = Type.GetType("Pyramid.Reports.PreBuiltReports.Rpt" + reportToRunClass);

                        //Get the constructor for the report
                        System.Reflection.ConstructorInfo CtorInfo = reportType.GetConstructor(Type.EmptyTypes);

                        //Get the report from the constructor
                        RptLogoMaster report = (RptLogoMaster)(CtorInfo.Invoke(new object[] { }));

                        //Set parameters
                        FillReport(ref report, reportCriteriaOptions, reportOptionalCriteriaOptions, reportName);

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Create a user report history object for this user and report
                            UserReportHistory reportHistory = new UserReportHistory()
                            {
                                Username = User.Identity.Name,
                                RunDate = DateTime.Now,
                                ReportCatalogFK = reportPK
                            };

                            //Get the other report history rows
                            List<UserReportHistory> existingReportHistory = context.UserReportHistory
                                                                                        .Where(urh => urh.Username == reportHistory.Username &&
                                                                                               urh.ReportCatalogFK == reportHistory.ReportCatalogFK).ToList();

                            //Check to see if there are other report history rows
                            if (existingReportHistory.Count > 0)
                            {
                                //Delete the other rows from the database (we only want one)
                                context.UserReportHistory.RemoveRange(existingReportHistory);
                            }

                            //Add it to the DB and save the changes
                            context.UserReportHistory.Add(reportHistory);
                            context.SaveChanges();
                        }

                        //Export the report in XLSX format to a MemoryStream
                        MemoryStream stream = new MemoryStream();
                        report.ExportToXlsx(stream);

                        //Download the MemoryStream to the user's browser as a XLSX file
                        byte[] data = stream.ToArray();
                        string headerText = string.Format("attachment; filename={0}.xlsx", (string.IsNullOrWhiteSpace(report.DisplayName) ? reportToRunClass : report.DisplayName));
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AppendHeader("content-disposition", headerText);
                        Response.AddHeader("content-length", data.Length.ToString());
                        Response.BinaryWrite(data);
                        Response.Flush();
                        Response.End();
                    }
                    catch (Exception ex)
                    {
                        //An error occurred, show the user a message
                        msgSys.ShowMessageToUser("danger", "An Error Occurred", "Error: <br/>" + ex.Message, 10000);

                        //Log the exception in the database
                        Utilities.LogException(ex);
                    }
                }
                else
                {
                    //Validation failed, show a message
                    msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
                }
            }
        }

        /// <summary>
        /// This method fires when the data source for the reports DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The linqReportDataSource control</param>
        /// <param name="e">The LinqDataSourceSelectEventArgs event</param>
        protected void linqReportDataSource_Selecting(object sender, System.Web.UI.WebControls.LinqDataSourceSelectEventArgs e)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Use LINQ to get the reports
                var reports = (from report in context.ReportCatalog.AsNoTracking()
                                     join urh in context.UserReportHistory.AsNoTracking().Where(urh => urh.Username == User.Identity.Name) on report.ReportCatalogPK equals urh.ReportCatalogFK into userReportHistory
                                     select new
                                     {
                                         report.ReportCatalogPK,
                                         report.CriteriaOptions,
                                         report.CriteriaDefaults,
                                         report.Creator,
                                         report.CreateDate,
                                         report.DocumentationLink,
                                         report.Keywords,
                                         report.OnlyExportAllowed,
                                         report.OptionalCriteriaOptions,
                                         report.ReportCategory,
                                         report.ReportClass,
                                         report.ReportDescription,
                                         report.ReportName,
                                         report.RolesAuthorizedToRun,
                                         LastRun = userReportHistory.OrderByDescending(urh => urh.RunDate).Take(1).Select(urh => (DateTime?)urh.RunDate).FirstOrDefault()
                                     }).ToList();

                //Limit the reports by the user's current role
                var filteredReports = reports.Where(r => r.RolesAuthorizedToRun.Split(',').ToList().Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString())).ToList();

                //Set the results to the filtered report list
                e.Result = filteredReports;
            }
        }

        /// <summary>
        /// This method fills the reports parameters from the page's controls
        /// </summary>
        /// <param name="report">The XtraReport to fill with parameter values</param>
        private void FillReport(ref RptLogoMaster report, string criteriaOptions, string optionalCriteriaOptions, string reportName)
        {
            //To hold the criteria
            List<string> criteria = new List<string>();

            //Set the state name and catchphrase
            if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER)
            {
                report.lblStateName.Text = currentProgramRole.StateName;
            }
            else
            {
                report.lblStateName.Text = currentProgramRole.StateName + " State";
            }
            report.lblStateCatchphrase.Text = currentProgramRole.StateCatchphrase;

            //Set the report name
            report.lblReportTitle.Text = reportName;

            //Set the logo url
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            report.ParamLogoPath.Value = baseUrl + "Content/images/" + currentProgramRole.StateThumbnailLogoFileName;

            //Set the visibility of information
            report.ParamViewPrivateChildInfo.Value = currentProgramRole.ViewPrivateChildInfo.Value;
            report.ParamViewPrivateEmployeeInfo.Value = currentProgramRole.ViewPrivateEmployeeInfo.Value;

            //Don't fill any parameters if the None criteria option exists
            if(!criteriaOptions.Contains("None"))
            {
                //---------------------- Programs -----------------------
                if (criteriaOptions.Contains("PRG,"))
                {
                    //To hold the selected program info
                    List<int> selectedProgramPKs = new List<int>();
                    List<string> selectedProgramNames = new List<string>();

                    if (lstBxProgram.SelectedItems.Count > 0)
                    {
                        //Get the selected programs
                        foreach (ListEditItem program in lstBxProgram.SelectedItems)
                        {
                            //Record the selected program info
                            selectedProgramPKs.Add(Convert.ToInt32(program.Value));
                            selectedProgramNames.Add(program.Text);
                        }

                        //Add the selected programs to the criteria
                        criteria.Add("<b>Programs:</b> " + string.Join(", ", selectedProgramNames));

                        //Fill the program parameter
                        report.Parameters["ParamProgramFKs"].Value = string.Join(",", selectedProgramPKs);
                    }
                    else
                    {
                        //Fill the program parameter
                        report.Parameters["ParamProgramFKs"].Value = "";
                    }
                }

                //---------------------- Hubs -----------------------
                if (criteriaOptions.Contains("HUB,"))
                {
                    //To hold the selected hub info
                    List<int> selectedHubPKs = new List<int>();
                    List<string> selectedHubNames = new List<string>();

                    if (lstBxHub.SelectedItems.Count > 0)
                    {
                        //Get the selected hubs
                        foreach (ListEditItem hub in lstBxHub.SelectedItems)
                        {
                            //Record the selected hub info
                            selectedHubPKs.Add(Convert.ToInt32(hub.Value));
                            selectedHubNames.Add(hub.Text);
                        }

                        //Add the selected hubs to the criteria
                        criteria.Add("<b>Hubs:</b> " + string.Join(", ", selectedHubNames));

                        //Fill the hub parameter
                        report.Parameters["ParamHubFKs"].Value = string.Join(",", selectedHubPKs);
                    }
                    else
                    {
                        //Fill the hub parameter
                        report.Parameters["ParamHubFKs"].Value = "";
                    }
                }

                //---------------------- Cohorts -----------------------
                if (criteriaOptions.Contains("COH,"))
                {
                    //To hold the selected cohort info
                    List<int> selectedCohortPKs = new List<int>();
                    List<string> selectedCohortNames = new List<string>();

                    if (lstBxCohort.SelectedItems.Count > 0)
                    {
                        //Get the selected cohorts
                        foreach (ListEditItem cohort in lstBxCohort.SelectedItems)
                        {
                            //Record the selected cohort info
                            selectedCohortPKs.Add(Convert.ToInt32(cohort.Value));
                            selectedCohortNames.Add(cohort.Text);
                        }

                        //Add the selected cohorts to the criteria
                        criteria.Add("<b>Cohorts:</b> " + string.Join(", ", selectedCohortNames));

                        //Fill the cohort parameter
                        report.Parameters["ParamCohortFKs"].Value = string.Join(",", selectedCohortPKs);
                    }
                    else
                    {
                        //Fill the cohort parameter
                        report.Parameters["ParamCohortFKs"].Value = "";
                    }
                }

                //---------------------- States -----------------------
                if (criteriaOptions.Contains("ST,"))
                {
                    //To hold the selected state info
                    List<int> selectedStatePKs = new List<int>();
                    List<string> selectedStateNames = new List<string>();

                    if (lstBxState.SelectedItems.Count > 0)
                    {
                        //Get the selected states
                        foreach (ListEditItem state in lstBxState.SelectedItems)
                        {
                            //Record the selected state info
                            selectedStatePKs.Add(Convert.ToInt32(state.Value));
                            selectedStateNames.Add(state.Text);
                        }

                        //Add the selected states to the criteria
                        criteria.Add("<b>States:</b> " + string.Join(", ", selectedStateNames));

                        //Fill the state parameter
                        report.Parameters["ParamStateFKs"].Value = string.Join(",", selectedStatePKs);
                    }
                    else
                    {
                        //Fill the state parameter
                        report.Parameters["ParamStateFKs"].Value = "";
                    }
                }

                //-------------- NON-PROGRAM CRITERIA -------------------

                if (criteriaOptions.Contains("SED,"))
                {
                    //Get the start and end dates
                    DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
                    DateTime? endDate = (deEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEndDate.Value));
                    DateTime? endDateEndDay = (endDate.HasValue == false ? (DateTime?)null : new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59));

                    //Add start and end date to the criteria
                    criteria.Add("<b>Start Date:</b> " + (startDate.HasValue ? startDate.Value.ToString("MM/dd/yyyy") : "none"));
                    criteria.Add("<b>End Date:</b> " + (endDateEndDay.HasValue ? endDateEndDay.Value.ToString("MM/dd/yyyy") : "none"));

                    //Fill the start and end date parameters
                    report.Parameters["ParamStartDate"].Value = startDate;
                    report.Parameters["ParamEndDate"].Value = endDateEndDay;
                }

                if (criteriaOptions.Contains("PIT,"))
                {
                    //Get the point in time date
                    DateTime? pointInTime = (dePointInTime.Value == null ? (DateTime?)null : Convert.ToDateTime(dePointInTime.Value));
                    DateTime? pointInTimeEndDay = (pointInTime.HasValue == false ? (DateTime?)null : new DateTime(pointInTime.Value.Year, pointInTime.Value.Month, pointInTime.Value.Day, 23, 59, 59));

                    //Add the point in time date to the criteria
                    criteria.Add("<b>Point In Time:</b> " + (pointInTimeEndDay.HasValue ? pointInTimeEndDay.Value.ToString("MM/dd/yyyy") : "none"));

                    //Fill the point in time parameter
                    report.Parameters["ParamPointInTime"].Value = pointInTimeEndDay;
                }

                if (criteriaOptions.Contains("YRS,"))
                {
                    //Get the year
                    int? year = (ddYear.Value == null ? (int?)null : Convert.ToInt32(ddYear.Value));

                    //Add year to the criteria
                    criteria.Add("<b>Year:</b> " + (year.HasValue ? year.Value.ToString() : "none"));

                    //Fill the year parameter
                    report.Parameters["ParamYear"].Value = year;
                }

                if (criteriaOptions.Contains("BPG,"))
                {
                    //Get the BIR profile group information
                    string groupValue = (ddBIRProfileGroup.Value == null ? null : ddBIRProfileGroup.Value.ToString());
                    string groupText = (ddBIRProfileGroup.Text == null ? null : ddBIRProfileGroup.Text.ToString());

                    //Add the group to the criteria
                    criteria.Add("<b>BIR Profile Group:</b> " + (!string.IsNullOrWhiteSpace(groupText) ? groupText.ToString() : "none"));

                    //Fill the parameter
                    report.Parameters["ParamBIRProfileGroup"].Value = groupValue;
                }

                if (criteriaOptions.Contains("BPI,"))
                {
                    //Get the BIR profile item information
                    string itemValue = (ddBIRProfileItem.Value == null ? null : ddBIRProfileItem.Value.ToString());
                    string itemText = (ddBIRProfileItem.Text == null ? null : ddBIRProfileItem.Text.ToString());

                    //Add the item to the criteria
                    criteria.Add("<b>BIR Profile Item:</b> " + (!string.IsNullOrWhiteSpace(itemText) ? itemText.ToString() : "none"));

                    //Fill the parameter
                    report.Parameters["ParamBIRItem"].Value = itemValue;
                }

                if (criteriaOptions.Contains("RF,"))
                {
                    //Get the report focus information
                    string focusValue = (ddReportFocus.Value == null ? null : ddReportFocus.Value.ToString());
                    string focusText = (ddReportFocus.Text == null ? null : ddReportFocus.Text.ToString());

                    //Add the information to the criteria
                    criteria.Add("<b>Report Focus:</b> " + (!string.IsNullOrWhiteSpace(focusText) ? focusText.ToString() : "none"));

                    //Fill the parameter
                    report.Parameters["ParamReportFocus"].Value = focusValue;
                }

                if (criteriaOptions.Contains("CR,"))
                {
                    //To hold the selected classroom info
                    List<int> selectedClassroomPKs = new List<int>();
                    List<string> selectedClassroomNames = new List<string>();

                    if (lstBxClassroom.SelectedItems.Count > 0)
                    {
                        //Get the selected classrooms
                        foreach (ListEditItem classroom in lstBxClassroom.SelectedItems)
                        {
                            //Record the selected classroom info
                            selectedClassroomPKs.Add(Convert.ToInt32(classroom.Value));
                            selectedClassroomNames.Add(classroom.Text);
                        }

                        //Add the selected classrooms to the criteria
                        criteria.Add("<b>Classrooms:</b> " + string.Join(", ", selectedClassroomNames));

                        //Fill the classroom parameter
                        report.Parameters["ParamClassroomFKs"].Value = string.Join(",", selectedClassroomPKs);
                    }
                    else if(optionalCriteriaOptions.Contains("CR,"))
                    {
                        //Fill the classroom parameter
                        report.Parameters["ParamClassroomFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("CHI,"))
                {
                    //To hold the selected child info
                    List<int> selectedChildPKs = new List<int>();
                    List<string> selectedChildNames = new List<string>();

                    if (lstBxChild.SelectedItems.Count > 0)
                    {
                        //Get the selected children
                        foreach (ListEditItem child in lstBxChild.SelectedItems)
                        {
                            //Record the selected child info
                            selectedChildPKs.Add(Convert.ToInt32(child.Value));
                            selectedChildNames.Add(child.Text);
                        }

                        //Add the selected children to the criteria
                        criteria.Add("<b>Children:</b> " + string.Join(", ", selectedChildNames));

                        //Fill the child FKs parameter
                        report.Parameters["ParamChildFKs"].Value = string.Join(",", selectedChildPKs);
                    }
                    else if(optionalCriteriaOptions.Contains("CHI,"))
                    {
                        //Fill the child FKs parameter
                        report.Parameters["ParamChildFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("CD,"))
                {
                    //To hold the selected child demographic info
                    List<int> selectedRacePKs = new List<int>();
                    List<string> selectedRaceNames = new List<string>();
                    List<int> selectedEthnicityPKs = new List<int>();
                    List<string> selectedEthnicityNames = new List<string>();
                    List<int> selectedGenderPKs = new List<int>();
                    List<string> selectedGenderNames = new List<string>();
                    bool? selectedIEP = (bool?)null;
                    List<string> selectedIEPText = new List<string>();
                    bool? selectedDLL = (bool?)null;
                    List<string> selectedDLLText = new List<string>();

                    if (lstBxRace.SelectedItems.Count > 0)
                    {
                        //Get the selected races
                        foreach (ListEditItem race in lstBxRace.SelectedItems)
                        {
                            //Record the selected race info
                            selectedRacePKs.Add(Convert.ToInt32(race.Value));
                            selectedRaceNames.Add(race.Text);
                        }

                        //Add the selected races to the criteria
                        criteria.Add("<b>Race:</b> " + string.Join(", ", selectedRaceNames));

                        //Set the race parameter
                        report.Parameters["ParamRaceFKs"].Value = string.Join(",", selectedRacePKs);
                    }
                    else if (optionalCriteriaOptions.Contains("CD,"))
                    {
                        //Set the race parameter
                        report.Parameters["ParamRaceFKs"].Value = "";
                    }

                    if (lstBxEthnicity.SelectedItems.Count > 0)
                    {
                        //Get the selected ethnicities
                        foreach (ListEditItem ethnicity in lstBxEthnicity.SelectedItems)
                        {
                            //Record the selected ethnicity info
                            selectedEthnicityPKs.Add(Convert.ToInt32(ethnicity.Value));
                            selectedEthnicityNames.Add(ethnicity.Text);
                        }

                        //Add the selected ethnicities to the criteria
                        criteria.Add("<b>Ethnicity:</b> " + string.Join(", ", selectedEthnicityNames));

                        //Set the ethnicity parameter
                        report.Parameters["ParamEthnicityFKs"].Value = string.Join(",", selectedEthnicityPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("CD,"))
                    {
                        //Set the ethnicity parameter
                        report.Parameters["ParamEthnicityFKs"].Value = "";
                    }

                    if (lstBxGender.SelectedItems.Count > 0)
                    {
                        //Get the selected genders
                        foreach (ListEditItem gender in lstBxGender.SelectedItems)
                        {
                            //Record the selected gender info
                            selectedGenderPKs.Add(Convert.ToInt32(gender.Value));
                            selectedGenderNames.Add(gender.Text);
                        }

                        //Add the selected children to the criteria
                        criteria.Add("<b>Gender:</b> " + string.Join(", ", selectedGenderNames));

                        //Fill the gender parameter
                        report.Parameters["ParamGenderFKs"].Value = string.Join(",", selectedGenderPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("CD,"))
                    {
                        //Fill the gender parameter
                        report.Parameters["ParamGenderFKs"].Value = "";
                    }

                    if (ddIEP.SelectedIndex > -1)
                    {
                        //Get the selected item
                        ListEditItem IEPItem = ddIEP.SelectedItem;

                        //Get an int that represents the selected value
                        int intIEPValue = Convert.ToInt32(IEPItem.Value);

                        //Convert the int to a nullable boolean
                        switch(intIEPValue)
                        {
                            case 0:
                                selectedIEP = false;
                                break;
                            case 1:
                                selectedIEP = true;
                                break;
                            case 2:
                                selectedIEP = (bool?)null;
                                break;
                            default:
                                selectedIEP = (bool?)null;
                                break;
                        }

                        //Add the selected item to the criteria
                        criteria.Add("<b>IEP:</b> " + IEPItem.Text);
                    }
                    else if (optionalCriteriaOptions.Contains("CD,"))
                    {
                        //Don't filter
                        selectedIEP = (bool?)null;
                    }

                    if (ddDLL.SelectedIndex > -1)
                    {
                        //Get the selected item
                        ListEditItem DLLItem = ddDLL.SelectedItem;

                        //Get an int that represents the selected value
                        int intDLLValue = Convert.ToInt32(DLLItem.Value);

                        //Convert the int to a nullable boolean
                        switch (intDLLValue)
                        {
                            case 0:
                                selectedDLL = false;
                                break;
                            case 1:
                                selectedDLL = true;
                                break;
                            case 2:
                                selectedDLL = (bool?)null;
                                break;
                            default:
                                selectedDLL = (bool?)null;
                                break;
                        }

                        //Add the selected item to the criteria
                        criteria.Add("<b>DLL:</b> " + DLLItem.Text);
                    }
                    else if (optionalCriteriaOptions.Contains("CD,"))
                    {
                        //Don't filter
                        selectedDLL = (bool?)null;
                    }

                    //Fill the IEP and DLL parameters
                    if(selectedIEP.HasValue)
                    {
                        report.Parameters["ParamIEP"].Value = selectedIEP;
                    }
                    else
                    {
                        report.Parameters["ParamIEP"].Value = null;
                    }

                    if (selectedDLL.HasValue)
                    {
                        report.Parameters["ParamDLL"].Value = selectedDLL;
                    }
                    else
                    {
                        report.Parameters["ParamDLL"].Value = null;
                    }
                }

                if (criteriaOptions.Contains("EMP,"))
                {
                    //To hold the selected employee info
                    List<int> selectedEmployeePKs = new List<int>();
                    List<string> selectedEmployeeNames = new List<string>();

                    if (lstBxEmployee.SelectedItems.Count > 0)
                    {
                        //Get the selected employees
                        foreach (ListEditItem employee in lstBxEmployee.SelectedItems)
                        {
                            //Record the selected employee info
                            selectedEmployeePKs.Add(Convert.ToInt32(employee.Value));
                            selectedEmployeeNames.Add(employee.Text);
                        }

                        //Add the selected employees to the criteria
                        criteria.Add("<b>Employees:</b> " + string.Join(", ", selectedEmployeeNames));

                        //Fill the employee FKs parameter
                        report.Parameters["ParamEmployeeFKs"].Value = string.Join(",", selectedEmployeePKs);
                    }
                    else if (optionalCriteriaOptions.Contains("EMP,"))
                    {
                        //Fill the employee FKs parameter
                        report.Parameters["ParamEmployeeFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("EMR,"))
                {
                    //Get the BIR profile item information
                    string itemValue = (ddEmployeeRole.Value == null ? null : ddEmployeeRole.Value.ToString());
                    string itemText = (ddEmployeeRole.Text == null ? null : ddEmployeeRole.Text.ToString());

                    if (itemValue != null)
                    {
                        //Add the item to the criteria
                        criteria.Add("<b>Employee Role:</b> " + (!string.IsNullOrWhiteSpace(itemText) ? itemText.ToString() : "none"));

                        //Fill the parameter
                        report.Parameters["ParamEmployeeRole"].Value = itemValue;
                    }
                    else if (optionalCriteriaOptions.Contains("EMR,"))
                    {
                        //Fill the parameter
                        report.Parameters["ParamEmployeeRole"].Value = null;
                    }
                }

                if (criteriaOptions.Contains("TCH,"))
                {
                    //To hold the selected teacher info
                    List<int> selectedTeacherPKs = new List<int>();
                    List<string> selectedTeacherNames = new List<string>();

                    if (lstBxTeacher.SelectedItems.Count > 0)
                    {
                        //Get the selected teachers
                        foreach (ListEditItem teacher in lstBxTeacher.SelectedItems)
                        {
                            //Record the selected teacher info
                            selectedTeacherPKs.Add(Convert.ToInt32(teacher.Value));
                            selectedTeacherNames.Add(teacher.Text);
                        }

                        //Add the selected teachers to the criteria
                        criteria.Add("<b>Teachers:</b> " + string.Join(", ", selectedTeacherNames));

                        //Fill the teacher FKs parameter
                        report.Parameters["ParamTeacherFKs"].Value = string.Join(",", selectedTeacherPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("TCH,"))
                    {
                        //Fill the teacher FKs parameter
                        report.Parameters["ParamTeacherFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("CCH,"))
                {
                    //To hold the selected coach info
                    List<int> selectedCoachPKs = new List<int>();
                    List<string> selectedCoachNames = new List<string>();

                    if (lstBxCoach.SelectedItems.Count > 0)
                    {
                        //Get the selected coaches
                        foreach (ListEditItem coach in lstBxCoach.SelectedItems)
                        {
                            //Record the selected coach info
                            selectedCoachPKs.Add(Convert.ToInt32(coach.Value));
                            selectedCoachNames.Add(coach.Text);
                        }

                        //Add the selected coaches to the criteria
                        criteria.Add("<b>Coaches:</b> " + string.Join(", ", selectedCoachNames));

                        //Fill the coach FKs parameter
                        report.Parameters["ParamCoachFKs"].Value = string.Join(",", selectedCoachPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("CCH,"))
                    {
                        //Fill the coach FKs parameter
                        report.Parameters["ParamCoachFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("PB,"))
                {
                    //To hold the selected Problem Behavior info
                    List<int> selectedProblemBehaviorPKs = new List<int>();
                    List<string> selectedProblemBehaviorDescriptions = new List<string>();

                    if (lstBxProblemBehavior.SelectedItems.Count > 0)
                    {
                        //Get the selected Problem Behaviors
                        foreach (ListEditItem problemBehavior in lstBxProblemBehavior.SelectedItems)
                        {
                            //Record the selected Problem Behavior info
                            selectedProblemBehaviorPKs.Add(Convert.ToInt32(problemBehavior.Value));
                            selectedProblemBehaviorDescriptions.Add(problemBehavior.Text);
                        }

                        //Add the selected Problem Behaviors to the criteria
                        criteria.Add("<b>Problem Behaviors:</b> " + string.Join(", ", selectedProblemBehaviorDescriptions));

                        //Fill the Problem Behavior FKs parameter
                        report.Parameters["ParamProblemBehaviorFKs"].Value = string.Join(",", selectedProblemBehaviorPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("PB,"))
                    {
                        //Fill the Problem Behavior FKs parameter
                        report.Parameters["ParamProblemBehaviorFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("ACT,"))
                {
                    //To hold the selected Activity info
                    List<int> selectedActivityPKs = new List<int>();
                    List<string> selectedActivityDescriptions = new List<string>();

                    if (lstBxActivity.SelectedItems.Count > 0)
                    {
                        //Get the selected Activities
                        foreach (ListEditItem activity in lstBxActivity.SelectedItems)
                        {
                            //Record the selected Activity info
                            selectedActivityPKs.Add(Convert.ToInt32(activity.Value));
                            selectedActivityDescriptions.Add(activity.Text);
                        }

                        //Add the selected Activities to the criteria
                        criteria.Add("<b>Activities:</b> " + string.Join(", ", selectedActivityDescriptions));

                        //Fill the Activity FKs parameter
                        report.Parameters["ParamActivityFKs"].Value = string.Join(",", selectedActivityPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("ACT,"))
                    {
                        //Fill the Activity FKs parameter
                        report.Parameters["ParamActivityFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("OI,"))
                {
                    //To hold the selected Others Involved info
                    List<int> selectedOthersInvolvedPKs = new List<int>();
                    List<string> selectedOthersInvolvedDescriptions = new List<string>();

                    if (lstBxOthersInvolved.SelectedItems.Count > 0)
                    {
                        //Get the selected Others Involved
                        foreach (ListEditItem otherInvolved in lstBxOthersInvolved.SelectedItems)
                        {
                            //Record the selected Others Involved info
                            selectedOthersInvolvedPKs.Add(Convert.ToInt32(otherInvolved.Value));
                            selectedOthersInvolvedDescriptions.Add(otherInvolved.Text);
                        }

                        //Add the selected Others Involved to the criteria
                        criteria.Add("<b>Others Involved:</b> " + string.Join(", ", selectedOthersInvolvedDescriptions));

                        //Fill the Others Involved FKs parameter
                        report.Parameters["ParamOthersInvolvedFKs"].Value = string.Join(",", selectedOthersInvolvedPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("OI,"))
                    {
                        //Fill the Others Involved FKs parameter
                        report.Parameters["ParamOthersInvolvedFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("PM,"))
                {
                    //To hold the selected Possible Motivation info
                    List<int> selectedPossibleMotivationPKs = new List<int>();
                    List<string> selectedPossibleMotivationDescriptions = new List<string>();

                    if (lstBxPossibleMotivation.SelectedItems.Count > 0)
                    {
                        //Get the selected Possible Motivations
                        foreach (ListEditItem possibleMotivation in lstBxPossibleMotivation.SelectedItems)
                        {
                            //Record the selected Possible Motivation info
                            selectedPossibleMotivationPKs.Add(Convert.ToInt32(possibleMotivation.Value));
                            selectedPossibleMotivationDescriptions.Add(possibleMotivation.Text);
                        }

                        //Add the selected Possible Motivations to the criteria
                        criteria.Add("<b>Possible Motivations:</b> " + string.Join(", ", selectedPossibleMotivationDescriptions));

                        //Fill the Possible Motivation FKs parameter
                        report.Parameters["ParamPossibleMotivationFKs"].Value = string.Join(",", selectedPossibleMotivationPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("PM,"))
                    {
                        //Fill the Possible Motivation FKs parameter
                        report.Parameters["ParamPossibleMotivationFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("SR,"))
                {
                    //To hold the selected Strategy Response info
                    List<int> selectedStrategyResponsePKs = new List<int>();
                    List<string> selectedStrategyResponseDescriptions = new List<string>();

                    if (lstBxStrategyResponse.SelectedItems.Count > 0)
                    {
                        //Get the selected Strategy Responses
                        foreach (ListEditItem strategyResponse in lstBxStrategyResponse.SelectedItems)
                        {
                            //Record the selected Strategy Response info
                            selectedStrategyResponsePKs.Add(Convert.ToInt32(strategyResponse.Value));
                            selectedStrategyResponseDescriptions.Add(strategyResponse.Text);
                        }

                        //Add the selected Strategy Responses to the criteria
                        criteria.Add("<b>Strategy Responses:</b> " + string.Join(", ", selectedStrategyResponseDescriptions));

                        //Fill the Strategy Response FKs parameter
                        report.Parameters["ParamStrategyResponseFKs"].Value = string.Join(",", selectedStrategyResponsePKs);
                    }
                    else if (optionalCriteriaOptions.Contains("SR,"))
                    {
                        //Fill the Strategy Response FKs parameter
                        report.Parameters["ParamStrategyResponseFKs"].Value = "";
                    }
                }

                if (criteriaOptions.Contains("AFU,"))
                {
                    //To hold the selected Admin Follow-up info
                    List<int> selectedAdminFollowUpPKs = new List<int>();
                    List<string> selectedAdminFollowUpDescriptions = new List<string>();

                    if (lstBxAdminFollowUp.SelectedItems.Count > 0)
                    {
                        //Get the selected Admin Follow-ups
                        foreach (ListEditItem adminFollowUp in lstBxAdminFollowUp.SelectedItems)
                        {
                            //Record the selected Admin Follow-up info
                            selectedAdminFollowUpPKs.Add(Convert.ToInt32(adminFollowUp.Value));
                            selectedAdminFollowUpDescriptions.Add(adminFollowUp.Text);
                        }

                        //Add the selected Admin Follow-ups to the criteria
                        criteria.Add("<b>Admin Follow-ups:</b> " + string.Join(", ", selectedAdminFollowUpDescriptions));

                        //Fill the Admin Follow-up FKs parameter
                        report.Parameters["ParamAdminFollowUpFKs"].Value = string.Join(",", selectedAdminFollowUpPKs);
                    }
                    else if (optionalCriteriaOptions.Contains("AFU,"))
                    {
                        //Fill the Admin Follow-up FKs parameter
                        report.Parameters["ParamAdminFollowUpFKs"].Value = "";
                    }
                }
            }
            else
            {
                //No criteria
                criteria.Add("None");
            }

            //Set the criteria label
            report.lblCriteriaValues.Text = string.Join("<br><br>", criteria);
        }

        #region Custom Validation

        /// <summary>
        /// This method fires when the validation for the assigned DevExpress
        /// Bootstrap ListBox fires and it validates the program, hub, cohort, and
        /// state ListBoxes
        /// </summary>
        /// <param name="sender">The Devexpress Bootstrap ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxPHCS_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;

            //Only validate if the criteria options require the program selection
            if (criteriaOptions != null)
            {
                //To hold the invalid items
                List<string> unselectedItems = new List<string>();

                //Get the number of available items
                int numAvailableItems = 0;

                //Check all the items for validity
                //Program
                if(criteriaOptions.Contains("PRG,") && lstBxProgram.ReadOnly == false)
                {
                    //Increment the number of available items
                    numAvailableItems++;

                    //If there are no programs selected, add program to the list of unselected items
                    if (lstBxProgram.SelectedItems.Count == 0)
                    {
                        unselectedItems.Add("program");
                    }
                }

                //Hub
                if (criteriaOptions.Contains("HUB,") && lstBxHub.ReadOnly == false)
                {
                    //Increment the number of available items
                    numAvailableItems++;

                    //If there are no hubs selected, add hub to the list of unselected items
                    if (lstBxHub.SelectedItems.Count == 0)
                    {
                        unselectedItems.Add("hub");
                    }
                }

                //Cohort
                if (criteriaOptions.Contains("COH,") && lstBxCohort.ReadOnly == false)
                {
                    //Increment the number of available items
                    numAvailableItems++;

                    //If there are no cohorts selected, add cohort to the list of unselected items
                    if (lstBxCohort.SelectedItems.Count == 0)
                    {
                        unselectedItems.Add("cohort");
                    }
                }

                //State
                if (criteriaOptions.Contains("ST,") && lstBxState.ReadOnly == false)
                {
                    //Increment the number of available items
                    numAvailableItems++;

                    //If there are no states selected, add state to the list of unselected items
                    if (lstBxState.SelectedItems.Count == 0)
                    {
                        unselectedItems.Add("state");
                    }
                }

                //If there are available items, and they are all unselected, this is invalid
                if (numAvailableItems > 0 && unselectedItems.Count == numAvailableItems)
                {
                    e.IsValid = false;
                    e.ErrorText = string.Format("At least one {0} must be selected!", string.Join(" or ", unselectedItems));
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the deStartDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deStartDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the start and end date
            if (criteriaOptions != null && criteriaOptions.Contains("SED,"))
            {
                //Get the start and end dates
                DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
                DateTime? endDate = (deEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEndDate.Value));

                //Perform the validation
                if (startDate.HasValue == false && !optionalCriteriaOptions.Contains("SED,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "Start Date is required!";
                }
                else if (startDate.HasValue == false && endDate.HasValue)
                {
                    e.IsValid = false;
                    e.ErrorText = "Start Date is required if the End Date is selected!";
                }
                else if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
                {
                    e.IsValid = false;
                    e.ErrorText = "Start Date must be before the End Date!";
                }
                else if (startDate.HasValue && startDate.Value > DateTime.Today.AddDays(1).AddTicks(-1))
                {
                    e.IsValid = false;
                    e.ErrorText = "Start Date cannot be in the future!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the deEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the start and end date
            if (criteriaOptions != null && criteriaOptions.Contains("SED,"))
            {
                //Get the start and end dates
                DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
                DateTime? endDate = (deEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEndDate.Value));

                //Perform the validation
                if (endDate.HasValue == false && !optionalCriteriaOptions.Contains("SED,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "End Date is required!";
                }
                else if (endDate.HasValue == false && startDate.HasValue)
                {
                    e.IsValid = false;
                    e.ErrorText = "End Date is required if the Start Date is selected!";
                }
                else if (endDate.HasValue && startDate.HasValue && startDate.Value >= endDate.Value)
                {
                    e.IsValid = false;
                    e.ErrorText = "End Date must be after the Start Date!";
                }
                else if (endDate.HasValue && endDate.Value > DateTime.Today.AddDays(1).AddTicks(-1))
                {
                    e.IsValid = false;
                    e.ErrorText = "End Date cannot be in the future!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the dePointInTime DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The dePointInTime DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void dePointInTime_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the point in time
            if (criteriaOptions != null && criteriaOptions.Contains("PIT,"))
            {
                //Get the point in time date
                DateTime? pointInTime = (dePointInTime.Value == null ? (DateTime?)null : Convert.ToDateTime(dePointInTime.Value));

                //Perform the validation
                if (pointInTime.HasValue == false && !optionalCriteriaOptions.Contains("PIT,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "Point In Time is required!";
                }
                else if (pointInTime.HasValue && pointInTime.Value > DateTime.Today.AddDays(1).AddTicks(-1))
                {
                    e.IsValid = false;
                    e.ErrorText = "Point In Time cannot be in the future!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddYear DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddYear ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddYear_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the year
            if (criteriaOptions != null && criteriaOptions.Contains("YRS,"))
            {
                //Get the year
                int? year = (ddYear.Value == null ? (int?)null : Convert.ToInt32(ddYear.Value));

                //Perform the validation
                if (year.HasValue == false && !optionalCriteriaOptions.Contains("YRS,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "A year must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddBIRProfileGroup DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddBIRProfileGroup ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddBIRProfileGroup_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the group
            if (criteriaOptions != null && criteriaOptions.Contains("BPG,"))
            {
                //Get the group
                string group = (ddBIRProfileGroup.Value == null ? null : ddBIRProfileGroup.Value.ToString());

                //Perform the validation
                if (string.IsNullOrWhiteSpace(group) && !optionalCriteriaOptions.Contains("BPG,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "A BIR Profile Group must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddBIRProfileItem DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddBIRProfileItem ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddBIRProfileItem_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the point in time
            if (criteriaOptions != null && criteriaOptions.Contains("BPI,"))
            {
                //Get the item
                string item = (ddBIRProfileItem.Value == null ? null : ddBIRProfileItem.Value.ToString());

                //Perform the validation
                if (string.IsNullOrWhiteSpace(item) && !optionalCriteriaOptions.Contains("BPI,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "A BIR Profile Item must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddReportFocus DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddReportFocus ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddReportFocus_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the focus information
            if (criteriaOptions != null && criteriaOptions.Contains("RF,"))
            {
                //Get the focus
                string focus = (ddReportFocus.Value == null ? null : ddReportFocus.Value.ToString());

                //Perform the validation
                if (string.IsNullOrWhiteSpace(focus) && !optionalCriteriaOptions.Contains("RF,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "A Report Focus must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxClassroom DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxClassroom ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxClassroom_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the classroom selection
            if (criteriaOptions != null && criteriaOptions.Contains("CR,"))
            {
                //To hold the selected classroom info
                List<int> selectedClassroomPKs = new List<int>();

                //Get the selected classrooms
                foreach (ListEditItem classroom in lstBxClassroom.SelectedItems)
                {
                    //Record the selected classroom info
                    selectedClassroomPKs.Add(Convert.ToInt32(classroom.Value));
                }

                //Perform the validation
                if (selectedClassroomPKs.Count == 0 && !optionalCriteriaOptions.Contains("CR,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one classroom must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxChild DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxChild ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxChild_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the children selection
            if (criteriaOptions != null && criteriaOptions.Contains("CHI,"))
            {
                //To hold the selected child info
                List<int> selectedChildPKs = new List<int>();

                //Get the selected children
                foreach (ListEditItem child in lstBxChild.SelectedItems)
                {
                    //Record the selected child info
                    selectedChildPKs.Add(Convert.ToInt32(child.Value));
                }

                //Perform the validation
                if (selectedChildPKs.Count == 0 && !optionalCriteriaOptions.Contains("CHI,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one child must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the child demographics
        /// fires and it validates those controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lstBxCD_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options include the child demographics
            if (criteriaOptions != null && criteriaOptions.Contains("CD,"))
            {
                //To hold the selected demographic info
                List<int> selectedRacePKs = new List<int>();
                List<int> selectedEthnicityPKs = new List<int>();
                List<int> selectedGenderPKs = new List<int>();
                bool? selectedIEP = false;
                bool? selectedDLL = false;

                //Get the selected races
                foreach (ListEditItem race in lstBxRace.SelectedItems)
                {
                    //Record the selected race info
                    selectedRacePKs.Add(Convert.ToInt32(race.Value));
                }

                //Get the selected ethnicities
                foreach (ListEditItem ethnicity in lstBxEthnicity.SelectedItems)
                {
                    //Record the selected ethnicity info
                    selectedEthnicityPKs.Add(Convert.ToInt32(ethnicity.Value));
                }

                //Get the selected genders
                foreach (ListEditItem gender in lstBxGender.SelectedItems)
                {
                    //Record the selected gender info
                    selectedGenderPKs.Add(Convert.ToInt32(gender.Value));
                }

                //Check to see if an IEP option was selected
                if(ddIEP.SelectedIndex > -1)
                {
                    selectedIEP = true;
                }

                //Check to see if a DLL option was selected
                if (ddDLL.SelectedIndex > -1)
                {
                    selectedDLL = true;
                }

                //Perform the validation
                if (selectedRacePKs.Count == 0 && selectedEthnicityPKs.Count == 0 && selectedGenderPKs.Count == 0
                    && selectedIEP == false && selectedDLL == false
                    && !optionalCriteriaOptions.Contains("CD,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one child demographic (Race, Ethnicity, Gender, IEP, or DLL) must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxEmployee DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxEmployee ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxEmployee_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the employee selection
            if (criteriaOptions != null && criteriaOptions.Contains("EMP,"))
            {
                //To hold the selected employee info
                List<int> selectedEmployeePKs = new List<int>();

                //Get the selected employees
                foreach (ListEditItem employee in lstBxEmployee.SelectedItems)
                {
                    //Record the selected employee info
                    selectedEmployeePKs.Add(Convert.ToInt32(employee.Value));
                }

                //Perform the validation
                if (selectedEmployeePKs.Count == 0 && !optionalCriteriaOptions.Contains("EMP,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one professional must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddEmployeeRole DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddEmployeeRole ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddEmployeeRole_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the point in time
            if (criteriaOptions != null && criteriaOptions.Contains("EMR,"))
            {
                //Get the item
                string item = (ddEmployeeRole.Value == null ? null : ddEmployeeRole.Value.ToString());

                //Perform the validation
                if (string.IsNullOrWhiteSpace(item) && !optionalCriteriaOptions.Contains("EMR,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "A professional role must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxTeacher DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxTeacher ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxTeacher_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the teacher selection
            if (criteriaOptions != null && criteriaOptions.Contains("TCH,"))
            {
                //To hold the selected teacher info
                List<int> selectedTeacherPKs = new List<int>();

                //Get the selected teachers
                foreach (ListEditItem teacher in lstBxTeacher.SelectedItems)
                {
                    //Record the selected teacher info
                    selectedTeacherPKs.Add(Convert.ToInt32(teacher.Value));
                }

                //Perform the validation
                if (selectedTeacherPKs.Count == 0 && !optionalCriteriaOptions.Contains("TCH,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one teacher must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxCoach DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxCoach ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxCoach_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the coach selection
            if (criteriaOptions != null && criteriaOptions.Contains("CCH,"))
            {
                //To hold the selected coach info
                List<int> selectedCoachPKs = new List<int>();

                //Get the selected coaches
                foreach (ListEditItem coach in lstBxCoach.SelectedItems)
                {
                    //Record the selected coach info
                    selectedCoachPKs.Add(Convert.ToInt32(coach.Value));
                }

                //Perform the validation
                if (selectedCoachPKs.Count == 0 && !optionalCriteriaOptions.Contains("CCH,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one coach must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxProblemBehavior DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxProblemBehavior ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxProblemBehavior_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the Problem Behavior selection
            if (criteriaOptions != null && criteriaOptions.Contains("PB,"))
            {
                //To hold the selected Problem Behavior info
                List<int> selectedProblemBehaviorPKs = new List<int>();

                //Get the selected Problem Behaviors
                foreach (ListEditItem problemBehavior in lstBxProblemBehavior.SelectedItems)
                {
                    //Record the selected Problem Behavior info
                    selectedProblemBehaviorPKs.Add(Convert.ToInt32(problemBehavior.Value));
                }

                //Perform the validation
                if (selectedProblemBehaviorPKs.Count == 0 && !optionalCriteriaOptions.Contains("PB,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one Problem Behavior must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxActivity DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxActivity ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxActivity_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the Activity selection
            if (criteriaOptions != null && criteriaOptions.Contains("ACT,"))
            {
                //To hold the selected Activity info
                List<int> selectedActivityPKs = new List<int>();

                //Get the selected Activities
                foreach (ListEditItem activity in lstBxActivity.SelectedItems)
                {
                    //Record the selected Activity info
                    selectedActivityPKs.Add(Convert.ToInt32(activity.Value));
                }

                //Perform the validation
                if (selectedActivityPKs.Count == 0 && !optionalCriteriaOptions.Contains("ACT,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one Activity must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxOthersInvolved DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxOthersInvolved ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxOthersInvolved_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the Others Involved selection
            if (criteriaOptions != null && criteriaOptions.Contains("OI,"))
            {
                //To hold the selected Others Involved info
                List<int> selectedOthersInvolvedPKs = new List<int>();

                //Get the selected Others Involved
                foreach (ListEditItem otherInvolved in lstBxOthersInvolved.SelectedItems)
                {
                    //Record the selected Others Involved info
                    selectedOthersInvolvedPKs.Add(Convert.ToInt32(otherInvolved.Value));
                }

                //Perform the validation
                if (selectedOthersInvolvedPKs.Count == 0 && !optionalCriteriaOptions.Contains("OI,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one Others Involved must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxPossibleMotivation DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxPossibleMotivation ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxPossibleMotivation_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the Possible Motivation selection
            if (criteriaOptions != null && criteriaOptions.Contains("PM,"))
            {
                //To hold the selected Possible Motivation info
                List<int> selectedPossibleMotivationPKs = new List<int>();

                //Get the selected Possible Motivations
                foreach (ListEditItem possibleMotivation in lstBxPossibleMotivation.SelectedItems)
                {
                    //Record the selected Possible Motivation info
                    selectedPossibleMotivationPKs.Add(Convert.ToInt32(possibleMotivation.Value));
                }

                //Perform the validation
                if (selectedPossibleMotivationPKs.Count == 0 && !optionalCriteriaOptions.Contains("PM,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one Possible Motivation must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxStrategyResponse DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxStrategyResponse ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxStrategyResponse_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the Strategy Response selection
            if (criteriaOptions != null && criteriaOptions.Contains("SR,"))
            {
                //To hold the selected Strategy Response info
                List<int> selectedStrategyResponsePKs = new List<int>();

                //Get the selected Strategy Responses
                foreach (ListEditItem strategyResponse in lstBxStrategyResponse.SelectedItems)
                {
                    //Record the selected Strategy Response info
                    selectedStrategyResponsePKs.Add(Convert.ToInt32(strategyResponse.Value));
                }

                //Perform the validation
                if (selectedStrategyResponsePKs.Count == 0 && !optionalCriteriaOptions.Contains("SR,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one Strategy Response must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the lstBxAdminFollowUp DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxAdminFollowUp ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxAdminFollowUp_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the Admin Follow-up selection
            if (criteriaOptions != null && criteriaOptions.Contains("AFU,"))
            {
                //To hold the selected Admin Follow-up info
                List<int> selectedAdminFollowUpPKs = new List<int>();

                //Get the selected Admin Follow-ups
                foreach (ListEditItem adminFollowUp in lstBxAdminFollowUp.SelectedItems)
                {
                    //Record the selected Admin Follow-up info
                    selectedAdminFollowUpPKs.Add(Convert.ToInt32(adminFollowUp.Value));
                }

                //Perform the validation
                if (selectedAdminFollowUpPKs.Count == 0 && !optionalCriteriaOptions.Contains("AFU,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one Admin Follow-up must be selected!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        #endregion

    }
}