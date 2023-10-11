using System;
using System.Data;
using System.Drawing;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptLCLAnalysis : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptLCLAnalysis()
        {
            InitializeComponent();
        }

        private class PivotedFormSchedule
        {
            public int FormSchedulePK { get; set; }
            public int CodeFormFK { get; set; }
            public int ProgramFK { get; set; }
            public int YearNumber { get; set; }
            public int MonthNumber { get; set; }
            public bool IsScheduled { get; set; }
            public DateTime? MonthYearStartDateTime => new DateTime(YearNumber, MonthNumber, 1);
            public DateTime? MonthYearEndDateTime => new DateTime(YearNumber, MonthNumber, 1).AddMonths(1).AddDays(-1);

            public PivotedFormSchedule(int schedulePK, int formFK, int programFK, int year, int month, bool isScheduled)
            {
                FormSchedulePK = schedulePK;
                CodeFormFK = formFK;
                ProgramFK = programFK;
                YearNumber = year;
                MonthNumber = month;
                IsScheduled = isScheduled;
            }
        }

        private class MostRecentFormForProgram
        {
            public int ProgramFK { get; set; }
            public int CodeFormFK { get; set; }
            public DateTime? MostRecentFormDate { get; set; }
        }

        private class ReportResultObject
        {
            public int ProgramKey { get; set; }
            public string ProgramName { get; set; }
            public int CohortKey { get; set; }
            public string CohortName { get; set; }
            public int HubKey { get; set; }
            public string HubName { get; set; }
            public int StateKey { get; set; }
            public string StateName { get; set; }
            public int? LCLKey { get; set; }
            public string LeadershipCoachName { get; set; }
            public string LeadershipCoachUsername { get; set; }
            public DateTime? LCLCreateDate { get; set; }
            public string LCLMonthYearString { get; set; }
            public string LCLSingleLogDateString { get; set; }
            public string LCLImplementationPhaseString { get; set; }
            public string PBCInPlaceString { get; set; }
            public string ActionPlanInPlaceString { get; set; }
            public string LeadershipTeamInPlaceString { get; set; }
            public DateTime? MostRecentBOQDate { get; set; }
            public DateTime? MostRecentBOQFCCDate { get; set; }
            public DateTime? MostRecentTPOTDate { get; set; }
            public DateTime? MostRecentTPITOSDate { get; set; }
            public DateTime? MostRecentBOQScheduledMonth { get; set; }
            public DateTime? MostRecentBOQFCCScheduledMonth { get; set; }
            public DateTime? MostRecentTPOTScheduledMonth { get; set; }
            public DateTime? MostRecentTPITOSScheduledMonth { get; set; }
        }

        private void RptLCLAnalysis_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the parameters
            DateTime startDate = Convert.ToDateTime(Parameters["ParamStartDate"].Value);
            DateTime endDate = Convert.ToDateTime(Parameters["ParamEndDate"].Value);
            string programFKs = Convert.ToString(Parameters["ParamProgramFKs"].Value);
            string hubFKs = Convert.ToString(Parameters["ParamHubFKs"].Value);
            string cohortFKs = Convert.ToString(Parameters["ParamCohortFKs"].Value);
            string stateFKs = Convert.ToString(Parameters["ParamStateFKs"].Value);

            //To hold the database info
            List<rspLCLAnalysisExport_Result> allProgramAndLCLInfo;
            List<int> allLCLPKs, allProgramPKs;
            List<string> allLeadershipCoachUsernames;
            List<LCLResponse> allLCLResponses;
            List<LCLInvolvedCoach> allLCLInvolvedCoaches;
            List<MostRecentFormForProgram> mostRecentBOQsCompleted, mostRecentBOQFCCsCompleted, mostRecentTPOTsCompleted, mostRecentTPITOSCompleted;
            List<FormSchedule> allFormsScheduledInTimeframe;
            List<PyramidUser> allLeadershipCoachUsers;

            //To hold the report results
            List<ReportResultObject> reportResults = new List<ReportResultObject>();

            //Get the data from the database
            using (PyramidContext context = new PyramidContext())
            {
                //Call the stored procedure for the report
                allProgramAndLCLInfo = context.rspLCLAnalysisExport(startDate, endDate, programFKs, hubFKs, cohortFKs, stateFKs).ToList();

                //Get the distinct LCL PKs from the stored procedure info
                allLCLPKs = allProgramAndLCLInfo.Where(i => i.LeadershipCoachLogPK.HasValue).Select(i => i.LeadershipCoachLogPK.Value).Distinct().ToList();

                //Get the LCL-specific fields we need
                allLCLResponses = context.LCLResponse.AsNoTracking().Where(lr => allLCLPKs.Contains(lr.LeadershipCoachLogFK)).ToList();
                allLCLInvolvedCoaches = context.LCLInvolvedCoach.AsNoTracking().Where(lic => allLCLPKs.Contains(lic.LeadershipCoachLogFK)).ToList();

                //Get the distinct program PKs from the stored procedure info
                allProgramPKs = allProgramAndLCLInfo.Select(i => i.ProgramPK).Distinct().ToList();

                //Get the distinct leadership coach usernames from the stored procedure info
                allLeadershipCoachUsernames = allProgramAndLCLInfo.Where(i => i.LeadershipCoachUsername != null).Select(i => i.LeadershipCoachUsername).Distinct().ToList();

                //Get the forms that were completed prior to the end date of the report
                mostRecentBOQsCompleted = context.BenchmarkOfQuality
                                                    .AsNoTracking()
                                                    .Where(b => allProgramPKs.Contains(b.ProgramFK) && b.FormDate <= endDate)
                                                    .GroupBy(b => b.ProgramFK)
                                                    .Select(g => new MostRecentFormForProgram()
                                                    {
                                                        ProgramFK = g.Key,
                                                        CodeFormFK = CodeForm.CodeFormPKs.BOQ,
                                                        MostRecentFormDate = g.Max(b => b.FormDate)
                                                    })
                                                    .ToList();
                mostRecentBOQFCCsCompleted = context.BenchmarkOfQualityFCC
                                                    .AsNoTracking()
                                                    .Where(b => allProgramPKs.Contains(b.ProgramFK) && b.FormDate <= endDate)
                                                    .GroupBy(b => b.ProgramFK)
                                                    .Select(g => new MostRecentFormForProgram()
                                                    {
                                                        ProgramFK = g.Key,
                                                        CodeFormFK = CodeForm.CodeFormPKs.BOQFCC,
                                                        MostRecentFormDate = g.Max(b => b.FormDate)
                                                    })
                                                    .ToList();
                mostRecentTPOTsCompleted = context.TPOT
                                                    .Include(t => t.Classroom)
                                                    .AsNoTracking()
                                                    .Where(t => allProgramPKs.Contains(t.Classroom.ProgramFK) && t.ObservationStartDateTime <= endDate)
                                                    .GroupBy(t => t.Classroom.ProgramFK)
                                                    .Select(g => new MostRecentFormForProgram()
                                                    {
                                                        ProgramFK = g.Key,
                                                        CodeFormFK = CodeForm.CodeFormPKs.TPOT,
                                                        MostRecentFormDate = g.Max(t => t.ObservationStartDateTime)
                                                    })
                                                    .ToList();
                mostRecentTPITOSCompleted = context.TPITOS
                                                    .Include(t => t.Classroom)
                                                    .AsNoTracking()
                                                    .Where(t => allProgramPKs.Contains(t.Classroom.ProgramFK) && t.ObservationStartDateTime <= endDate)
                                                    .GroupBy(t => t.Classroom.ProgramFK)
                                                    .Select(g => new MostRecentFormForProgram()
                                                    {
                                                        ProgramFK = g.Key,
                                                        CodeFormFK = CodeForm.CodeFormPKs.TPITOS,
                                                        MostRecentFormDate = g.Max(t => t.ObservationStartDateTime)
                                                    })
                                                    .ToList();

                //Get the forms that were scheduled prior to the end date of the reprot
                allFormsScheduledInTimeframe = context.FormSchedule.AsNoTracking().Where(fs => allProgramPKs.Contains(fs.ProgramFK) && fs.ScheduleYear >= startDate.Year && fs.ScheduleYear <= (endDate.Year + 2)).ToList();
            }

            //Pivot the form schedules
            List<PivotedFormSchedule> pivotedFormSchedules = new List<PivotedFormSchedule>();

            foreach (FormSchedule schedule in allFormsScheduledInTimeframe)
            {
                //Add all 12 months
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 1, schedule.ScheduledForJan));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 2, schedule.ScheduledForFeb));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 3, schedule.ScheduledForMar));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 4, schedule.ScheduledForApr));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 5, schedule.ScheduledForMay));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 6, schedule.ScheduledForJun));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 7, schedule.ScheduledForJul));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 8, schedule.ScheduledForAug));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 9, schedule.ScheduledForSep));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 10, schedule.ScheduledForOct));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 11, schedule.ScheduledForNov));
                pivotedFormSchedules.Add(new PivotedFormSchedule(schedule.FormSchedulePK, schedule.CodeFormFK, schedule.ProgramFK, schedule.ScheduleYear, 12, schedule.ScheduledForDec));
            }

            //Get the pivoted schedules that are inside the timeframe and are scheduled
            List<PivotedFormSchedule> validPivotedSchedules = pivotedFormSchedules.Where(pfs => pfs.IsScheduled && 
                                                                                                pfs.MonthYearEndDateTime >= startDate &&
                                                                                                pfs.MonthYearStartDateTime <= endDate.AddYears(2))
                                                                                  .ToList();

            //Get the user records for the leadership coaches
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                allLeadershipCoachUsers = context.Users.AsNoTracking().Where(u => allLeadershipCoachUsernames.Contains(u.UserName)).ToList();
            }

            //Create the list of report result objects
            reportResults = allProgramAndLCLInfo.Select(i => new ReportResultObject()
            {
                ProgramKey = i.ProgramPK,
                ProgramName = i.ProgramName,
                CohortKey = i.CohortPK,
                CohortName = i.CohortName,
                HubKey = i.HubPK,
                HubName = i.HubName,
                StateKey = i.StatePK,
                StateName = i.StateName,
                LCLKey = i.LeadershipCoachLogPK,
                LeadershipCoachName = allLeadershipCoachUsers.Where(u => u.UserName == i.LeadershipCoachUsername).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
                LeadershipCoachUsername = i.LeadershipCoachUsername,
                LCLCreateDate = i.LCLCreateDate,
                LCLMonthYearString = (i.IsMonthly.HasValue ? i.IsMonthly.Value ? i.LCLDateCompleted.Value.ToString("MM/yyyy") : null : null),
                LCLSingleLogDateString = (i.IsMonthly.HasValue ? i.IsMonthly.Value == false ? i.LCLDateCompleted.Value.ToString("MM/dd/yyyy") : null : null),
                LCLImplementationPhaseString = (i.LCLCyclePhase.HasValue ? i.LCLCyclePhase.Value == 99 ? "Not applicable" : i.LCLCyclePhase.Value.ToString() : null),
                PBCInPlaceString = (i.LeadershipCoachLogPK.HasValue ? allLCLInvolvedCoaches.Where(lic => lic.LeadershipCoachLogFK == i.LeadershipCoachLogPK).Count() > 0 ? "Yes" : "No" : null),
                ActionPlanInPlaceString = (i.LCLGoalCompletionLikelihoodCodeFK.HasValue ? i.LCLGoalCompletionLikelihoodCodeFK == Models.CodeLCLResponse.GoalCompletionLikelihoodPKs.NOT_APPLICABLE_NO_ACTION_PLAN ? "Yes" : "No" : null),
                LeadershipTeamInPlaceString = (i.LeadershipCoachLogPK.HasValue ? allLCLResponses.Where(r => r.LeadershipCoachLogFK == i.LeadershipCoachLogPK && r.LCLResponseCodeFK == Models.CodeLCLResponse.ProgramStrengthsPKs.LEADERSHIP_TEAM_ESTABLISHED).Count() > 0 ? "Yes" : "No" : null),
                MostRecentBOQDate = mostRecentBOQsCompleted.Where(b => b.ProgramFK == i.ProgramPK).Select(b => b.MostRecentFormDate).FirstOrDefault(),
                MostRecentBOQFCCDate = mostRecentBOQFCCsCompleted.Where(b => b.ProgramFK == i.ProgramPK).Select(b => b.MostRecentFormDate).FirstOrDefault(),
                MostRecentTPOTDate = mostRecentTPOTsCompleted.Where(t => t.ProgramFK == i.ProgramPK).Select(t => t.MostRecentFormDate).FirstOrDefault(),
                MostRecentTPITOSDate = mostRecentTPITOSCompleted.Where(t => t.ProgramFK == i.ProgramPK).Select(t => t.MostRecentFormDate).FirstOrDefault(),
                MostRecentBOQScheduledMonth = validPivotedSchedules.Where(fs => fs.ProgramFK == i.ProgramPK && fs.CodeFormFK == CodeForm.CodeFormPKs.BOQ).Max(pfs => pfs.MonthYearEndDateTime),
                MostRecentBOQFCCScheduledMonth = validPivotedSchedules.Where(fs => fs.ProgramFK == i.ProgramPK && fs.CodeFormFK == CodeForm.CodeFormPKs.BOQFCC).Max(pfs => pfs.MonthYearEndDateTime),
                MostRecentTPOTScheduledMonth = validPivotedSchedules.Where(fs => fs.ProgramFK == i.ProgramPK && fs.CodeFormFK == CodeForm.CodeFormPKs.TPOT).Max(pfs => pfs.MonthYearEndDateTime),
                MostRecentTPITOSScheduledMonth = validPivotedSchedules.Where(fs => fs.ProgramFK == i.ProgramPK && fs.CodeFormFK == CodeForm.CodeFormPKs.TPITOS).Max(pfs => pfs.MonthYearEndDateTime)
            }).ToList();

            //Bind the report to the results
            this.DataSource = reportResults;

            //Set the expression bindings for all the labels
            lblProgramKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ProgramKey"));
            lblProgramName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ProgramName"));
            lblCohortKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "CohortKey"));
            lblCohortName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "CohortName"));
            lblHubKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "HubKey"));
            lblHubName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "HubName"));
            lblStateKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StateKey"));
            lblStateCompleteName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "StateName"));
            lblLCLKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LCLKey"));
            lblLeadershipCoachName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeadershipCoachName"));
            lblLeadershipCoachUsername.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeadershipCoachUsername"));
            lblLCLDateEntered.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy}', [LCLCreateDate])"));
            lblLCLMonthYear.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LCLMonthYearString"));
            lblLCLSingleDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LCLSingleLogDateString"));
            lblLCLImplementationPhase.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LCLImplementationPhaseString"));
            lblPBCInPlace.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PBCInPlaceString"));
            lblActionPlanInPlace.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ActionPlanInPlaceString"));
            lblTeamInPlace.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LeadershipTeamInPlaceString"));
            lblBOQComplete.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy}', [MostRecentBOQDate])"));
            lblBOQFCCComplete.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy}', [MostRecentBOQFCCDate])"));
            lblTPOTComplete.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy}', [MostRecentTPOTDate])"));
            lblTPITOSComplete.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy}', [MostRecentTPITOSDate])"));
            lblBOQScheduled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MMMM yyyy}', [MostRecentBOQScheduledMonth])"));
            lblBOQFCCScheduled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MMMM yyyy}', [MostRecentBOQFCCScheduledMonth])"));
            lblTPOTScheduled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MMMM yyyy}', [MostRecentTPOTScheduledMonth])"));
            lblTPITOSScheduled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MMMM yyyy}', [MostRecentTPITOSScheduledMonth])"));
        }
    }
}
