using DevExpress.Web;
using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Linq;
using DevExpress.XtraReports.UI;
using Pyramid.Reports.PreBuiltReports.MasterReports;
using System.Collections.Generic;
using System.Data.Entity;

namespace Pyramid.Reports
{
    public partial class Reports : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);
            
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

                //Set the hidden field for the current program FK
                hfCurrentProgramFK.Value = currentProgramRole.CurrentProgramFK.Value.ToString();

                //Set the control visibility based on user role
                SetControlVisibilityAndUsability(currentProgramRole.RoleFK.Value);
            }
        }

        /// <summary>
        /// This method populates the data bound controls on the page
        /// </summary>
        private void FillDataBoundControls()
        {
            //Fill the data bound controls
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
                List<Hub> allHubs;

                //Get the hub list based on role
                if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                {
                    //Get all the hubs
                    allHubs = context.Hub.AsNoTracking()
                                            .OrderBy(h => h.Name)
                                            .ToList();
                }
                else if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN)
                {
                    //Get all the hubs for the state
                    allHubs = context.Hub.AsNoTracking()
                                            .Where(h => h.StateFK == currentProgramRole.StateFK.Value)
                                            .OrderBy(h => h.Name)
                                            .ToList();
                }
                else
                {
                    //Get the hub for this user
                    allHubs = context.Hub.AsNoTracking()
                                            .Where(h => h.HubPK == currentProgramRole.HubFK.Value)
                                            .OrderBy(h => h.Name)
                                            .ToList();
                }

                //Bind the hub list box
                lstBxHub.DataSource = allHubs;
                lstBxHub.DataBind();

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
                                                    ChildName = "(" + cp.ProgramSpecificID + ") " + cp.Child.FirstName + " " + cp.Child.LastName
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
                                                .OrderBy(cr => cr.OrderBy)
                                                .ToList();

                //Bind the race list box
                lstBxGender.DataSource = allGenders;
                lstBxGender.DataBind();

                //--------------- Employee List Box -----------------
                //Get all the employees for this user
                var allEmployees = context.ProgramEmployee.AsNoTracking()
                                                .Where(pe => currentProgramRole.ProgramFKs.Contains(pe.ProgramFK))
                                                .OrderBy(pe => pe.FirstName)
                                                .Select(pe => new
                                                {
                                                    ProgramEmployeePK = pe.ProgramEmployeePK,
                                                    ProgramEmployeeName = pe.FirstName + " " + pe.LastName
                                                })
                                                .ToList();

                //Bind the employee list box
                lstBxEmployee.DataSource = allEmployees;
                lstBxEmployee.DataBind();

                //--------------- Teacher List Box -----------------
                //Get all the teachers for this user
                var allTeachers = (from pe in context.ProgramEmployee.AsNoTracking()
                                                    .Include(pe => pe.JobFunction)
                                  join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK
                                  where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                    && jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.TEACHER
                                  select new
                                  {
                                      ProgramEmployeePK = pe.ProgramEmployeePK,
                                      ProgramEmployeeName = pe.FirstName + " " + pe.LastName
                                  })
                                  .Distinct()
                                  .ToList();

                //Bind the teacher list box
                lstBxTeacher.DataSource = allTeachers;
                lstBxTeacher.DataBind();

                //--------------- Coach List Box -----------------
                //Get all the coaches for this user
                var allCoaches = (from pe in context.ProgramEmployee.AsNoTracking()
                                                    .Include(pe => pe.JobFunction)
                                  join jf in context.JobFunction on pe.ProgramEmployeePK equals jf.ProgramEmployeeFK
                                  join t in context.Training on pe.ProgramEmployeePK equals t.ProgramEmployeeFK
                                  where currentProgramRole.ProgramFKs.Contains(pe.ProgramFK)
                                    && jf.JobTypeCodeFK == (int)Utilities.JobTypeFKs.COACH
                                    && (t.TrainingCodeFK == (int)Utilities.TrainingFKs.INTRODUCTION_TO_COACHING
                                          || t.TrainingCodeFK == (int)Utilities.TrainingFKs.PRACTICE_BASED_COACHING)
                                  select new
                                  {
                                      ProgramEmployeePK = pe.ProgramEmployeePK,
                                      ProgramEmployeeName = pe.FirstName + " " + pe.LastName
                                  })
                                  .Distinct()
                                  .ToList();

                //Bind the coach list box
                lstBxCoach.DataSource = allCoaches;
                lstBxCoach.DataBind();
            }
        }

        /// <summary>
        /// This method shows/hides controls based on the user's role
        /// </summary>
        /// <param name="roleFK">The role FK</param>
        private void SetControlVisibilityAndUsability(int roleFK)
        {
            if(roleFK == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                //Hide nothing
            }
            else if(roleFK == (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN)
            {
                //Hide nothing
            }
            else if (roleFK == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
            {
                //Hide and disable the cohort list box
                lstBxCohort.CssClasses.Control = "hidden";
                lstBxCohort.CssClasses.Caption = "hidden";
                lstBxCohort.ReadOnly = true;
            }
            else if (roleFK == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                //Hide and disable the cohort list box
                lstBxCohort.CssClasses.Control = "hidden";
                lstBxCohort.CssClasses.Caption = "hidden";
                lstBxCohort.ReadOnly = true;

                //Hide and disable the hub list box
                lstBxHub.CssClasses.Control = "hidden";
                lstBxHub.CssClasses.Caption = "hidden";
                lstBxHub.ReadOnly = true;

                //Make the criteria read only
                //Child
                lstBxChild.CssClasses.Control = "hidden";
                lstBxChild.CssClasses.Caption = "hidden";
                lstBxChild.ReadOnly = true;

                //Classroom
                lstBxClassroom.CssClasses.Control = "hidden";
                lstBxClassroom.CssClasses.Caption = "hidden";
                lstBxClassroom.ReadOnly = true;

                //Coach
                lstBxCoach.CssClasses.Control = "hidden";
                lstBxCoach.CssClasses.Caption = "hidden";
                lstBxCoach.ReadOnly = true;

                //Employee
                lstBxEmployee.CssClasses.Control = "hidden";
                lstBxEmployee.CssClasses.Caption = "hidden";
                lstBxEmployee.ReadOnly = true;
                
                //Ethnicity
                lstBxEthnicity.CssClasses.Control = "hidden";
                lstBxEthnicity.CssClasses.Caption = "hidden";
                lstBxEthnicity.ReadOnly = true;

                //Gender
                lstBxGender.CssClasses.Control = "hidden";
                lstBxGender.CssClasses.Caption = "hidden";
                lstBxGender.ReadOnly = true;

                //Race
                lstBxRace.CssClasses.Control = "hidden";
                lstBxRace.CssClasses.Caption = "hidden";
                lstBxRace.ReadOnly = true;

                //IEP
                ddIEP.CssClasses.Control = "hidden";
                ddIEP.CssClasses.Caption = "hidden";
                ddIEP.ReadOnly = true;

                //DLL
                ddDLL.CssClasses.Control = "hidden";
                ddDLL.CssClasses.Caption = "hidden";
                ddDLL.ReadOnly = true;

                //Teacher
                lstBxTeacher.CssClasses.Control = "hidden";
                lstBxTeacher.CssClasses.Caption = "hidden";
                lstBxTeacher.ReadOnly = true;
            }
            else
            {
                //All other program-specific roles
                //Hide and disable the cohort list box
                lstBxCohort.CssClasses.Control = "hidden";
                lstBxCohort.CssClasses.Caption = "hidden";
                lstBxCohort.ReadOnly = true;

                //Hide and disable the hub list box
                lstBxHub.CssClasses.Control = "hidden";
                lstBxHub.CssClasses.Caption = "hidden";
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
                    }
                    catch(Exception ex)
                    {
                        //An error occurred, show the user a message
                        msgSys.ShowMessageToUser("danger", "An Error Occurred", "Error: <br/>" + ex.Message, 10000);

                        //Log the exception in the database
                        Utilities.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the data source for the reports DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efReportDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efReportDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "ReportCatalogPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.ReportCatalog.AsNoTracking()
                                    .Where(rc => rc.RolesAuthorizedToRun.Contains(currentProgramRole.RoleFK.ToString() + ","));
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
            report.lblStateName.Text = currentProgramRole.StateName + " State";
            report.lblStateCatchphrase.Text = currentProgramRole.StateCatchphrase;

            //Set the report name
            report.lblReportTitle.Text = reportName;

            //Set the logo url
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            report.Parameters["ParamLogoPath"].Value = baseUrl + "Content/images/" + currentProgramRole.StateLogoFileName;

            //Don't fill any parameters if the None criteria option exists
            if(!criteriaOptions.Contains("None"))
            {
                //-------------- PROGRAM CRITERIA -------------------
                
                //Check the criteria
                if (criteriaOptions.Contains("PHC,"))
                {
                    //To hold the selected programs/hubs/cohorts program FKs
                    List<int> selectedProgramPKs = new List<int>();

                    //---------------- PROGRAMS -----------------------

                    //To hold the selected program names
                    List<string> selectedProgramNames = new List<string>();

                    //Get the selected programs
                    foreach (ListEditItem program in lstBxProgram.SelectedItems)
                    {
                        //Record the selected program info
                        selectedProgramPKs.Add(Convert.ToInt32(program.Value));
                        selectedProgramNames.Add(program.Text);
                    }

                    if(selectedProgramPKs.Count > 0)
                    {
                        //Add the program names to the criteria
                        criteria.Add("<b>Programs:</b> " + string.Join(", ", selectedProgramNames));
                    }

                    //---------------- HUBS -----------------------

                    //To hold the selected hub info
                    List<int> selectedHubPKs = new List<int>();
                    List<string> selectedHubNames = new List<string>();

                    //Get the selected hubs
                    foreach (ListEditItem hub in lstBxHub.SelectedItems)
                    {
                        //Record the selected hub info
                        selectedHubPKs.Add(Convert.ToInt32(hub.Value));
                        selectedHubNames.Add(hub.Text);
                    }
                    
                    if (selectedHubPKs.Count > 0)
                    {
                        using(PyramidContext context = new PyramidContext())
                        {
                            //Get the program PKs for the programs in the selected hubs
                            List<int> hubProgramPKs = context.Program
                                                        .Where(p => selectedHubPKs.Contains(p.HubFK)
                                                                && currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                                        .Select(p => p.ProgramPK)
                                                        .ToList();

                            //Add the distinct program PKs to the selected program PK list
                            selectedProgramPKs = selectedProgramPKs.Union(hubProgramPKs).ToList();
                        }

                        //Add the hub names to the criteria
                        criteria.Add("<b>Hubs:</b> " + string.Join(", ", selectedHubNames));
                    }

                    //---------------- COHORTS -----------------------

                    //To hold the selected cohorts
                    List<int> selectedCohortPKs = new List<int>();
                    List<string> selectedCohortNames = new List<string>();

                    //Get the selected cohorts
                    foreach (ListEditItem cohort in lstBxCohort.SelectedItems)
                    {
                        //Record the selected cohort info
                        selectedCohortPKs.Add(Convert.ToInt32(cohort.Value));
                        selectedCohortNames.Add(cohort.Text);
                    }

                    if (selectedCohortPKs.Count > 0)
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the program PKs for the programs in the selected cohorts
                            List<int> cohortProgramPKs = context.Program
                                                        .Where(p => selectedCohortPKs.Contains(p.CohortFK)
                                                                && currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                                        .Select(p => p.ProgramPK)
                                                        .ToList();

                            //Add the distinct program PKs to the selected program PK list
                            selectedProgramPKs = selectedProgramPKs.Union(cohortProgramPKs).ToList();
                        }

                        //Add the cohort names to the criteria
                        criteria.Add("<b>Cohorts:</b> " + string.Join(", ", selectedCohortNames));
                    }

                    if(selectedProgramPKs.Count == 0 && optionalCriteriaOptions.Contains("PHC,"))
                    {
                        //Get all the programs
                        foreach (ListEditItem program in lstBxProgram.Items)
                        {
                            //Record the program info
                            selectedProgramPKs.Add(Convert.ToInt32(program.Value));
                        }
                    }

                    //Fill the program FKs parameter
                    report.Parameters["ParamProgramFKs"].Value = string.Join(",", selectedProgramPKs);
                }

                //-------------- NON-PROGRAM CRITERIA -------------------

                if (criteriaOptions.Contains("SED,"))
                {
                    //Get the start and end dates
                    DateTime? startDate = (deStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deStartDate.Value));
                    DateTime? endDate = (deEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEndDate.Value).AddHours(23).AddMinutes(59));

                    //Add start and end date to the criteria
                    criteria.Add("<b>Start Date:</b> " + (startDate.HasValue ? startDate.Value.ToString("MM/dd/yyyy") : "none"));
                    criteria.Add("<b>End Date:</b> " + (endDate.HasValue ? endDate.Value.ToString("MM/dd/yyyy") : "none"));

                    //Fill the start and end date parameters
                    report.Parameters["ParamStartDate"].Value = startDate;
                    report.Parameters["ParamEndDate"].Value = endDate;
                }

                if (criteriaOptions.Contains("PIT,"))
                {
                    //Get the point in time date
                    DateTime? pointInTime = (dePointInTime.Value == null ? (DateTime?)null : Convert.ToDateTime(dePointInTime.Value));

                    //Add start and end date to the criteria
                    criteria.Add("<b>Point In Time:</b> " + (pointInTime.HasValue ? pointInTime.Value.ToString("MM/dd/yyyy") : "none"));

                    //Fill the point in time parameter
                    report.Parameters["ParamPointInTime"].Value = pointInTime;
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
                        criteria.Add("<b>Employeees:</b> " + string.Join(", ", selectedEmployeeNames));

                        //Fill the employee FKs parameter
                        report.Parameters["ParamEmployeeFKs"].Value = string.Join(",", selectedEmployeePKs);
                    }
                    else if (optionalCriteriaOptions.Contains("EMP,"))
                    {
                        //Fill the employee FKs parameter
                        report.Parameters["ParamEmployeeFKs"].Value = "";
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
        /// This method fires when the validation for the lstBxProgram DevExpress
        /// Bootstrap ListBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The lstBxProgram ListBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void lstBxPHC_Validation(object sender, ValidationEventArgs e)
        {
            //Get the criteria options
            string criteriaOptions = hfReportToRunCriteriaOptions.Value;
            string optionalCriteriaOptions = hfReportToRunOptionalCriteriaOptions.Value;

            //Only validate if the criteria options require the program selection
            if (criteriaOptions != null && criteriaOptions.Contains("PHC,"))
            {
                //To hold the selected programs, hubs, and cohorts
                List<int> selectedProgramPKs = new List<int>();
                List<int> selectedHubPKs = new List<int>();
                List<int> selectedCohortPKs = new List<int>();

                //Get the selected programs
                foreach (ListEditItem program in lstBxProgram.SelectedItems)
                {
                    //Record the selected program info
                    selectedProgramPKs.Add(Convert.ToInt32(program.Value));
                }

                //Get the selected hubs
                foreach (ListEditItem hub in lstBxHub.SelectedItems)
                {
                    //Record the selected hub info
                    selectedHubPKs.Add(Convert.ToInt32(hub.Value));
                }

                //Get the selected cohorts
                foreach (ListEditItem cohort in lstBxCohort.SelectedItems)
                {
                    //Record the selected cohort info
                    selectedCohortPKs.Add(Convert.ToInt32(cohort.Value));
                }

                //Perform the validation
                if (selectedProgramPKs.Count == 0 && selectedHubPKs.Count == 0 && selectedCohortPKs.Count == 0 
                    && !optionalCriteriaOptions.Contains("PHC,"))
                {
                    e.IsValid = false;
                    e.ErrorText = "At least one program, hub, or cohort must be selected!";
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
                else if (startDate.HasValue && startDate.Value > DateTime.Now)
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
                else if (endDate.HasValue && endDate.Value > DateTime.Now)
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
                else if (pointInTime.HasValue && pointInTime.Value > DateTime.Now)
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
                    e.ErrorText = "At least one employee must be selected!";
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
        #endregion
    }
}