using DevExpress.DataProcessing;
using DevExpress.XtraCharts;
using DevExpress.XtraReports.UI;
using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptChildDischargeDetails : Pyramid.Reports.PreBuiltReports.MasterReports.RptTableOfContentsMaster
    {
        //To hold all the BIR info
        List<rspChildDischargeDetails_Result> allChildDischarges;

        public RptChildDischargeDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptChildDischargeDetails_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the parameters
            DateTime startDate = Convert.ToDateTime(Parameters["ParamStartDate"].Value);
            DateTime endDate = Convert.ToDateTime(Parameters["ParamEndDate"].Value);
            string programFKs = Convert.ToString(Parameters["ParamProgramFKs"].Value);
            string hubFKs = Convert.ToString(Parameters["ParamHubFKs"].Value);
            string cohortFKs = Convert.ToString(Parameters["ParamCohortFKs"].Value);
            string stateFKs = Convert.ToString(Parameters["ParamStateFKs"].Value);
            string classroomFKs = Convert.ToString(Parameters["ParamClassroomFKs"].Value);
            string raceFKs = Convert.ToString(Parameters["ParamRaceFKs"].Value);
            string ethnicityFKs = Convert.ToString(Parameters["ParamEthnicityFKs"].Value);
            string genderFKs = Convert.ToString(Parameters["ParamGenderFKs"].Value);
            bool? hasIEP = (Parameters["ParamIEP"].Value == null ? (bool?)null : Convert.ToBoolean(Parameters["ParamIEP"].Value));
            bool? isDLL = (Parameters["ParamDLL"].Value == null ? (bool?)null : Convert.ToBoolean(Parameters["ParamDLL"].Value));


            //To hold all the discharge reasons
            List<CodeDischargeReason> allDischargeReasons;
            int totalDischarges;

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the child discharges for the timeframe
                allChildDischarges = context.rspChildDischargeDetails(startDate, endDate,
                                                    classroomFKs, raceFKs, ethnicityFKs,
                                                    genderFKs, hasIEP, isDLL, programFKs, hubFKs, cohortFKs, stateFKs).ToList();

                //Get the total number of discharges
                totalDischarges = allChildDischarges.Count;

                //Get all the discharge reasons
                allDischargeReasons = context.CodeDischargeReason.AsNoTracking()
                                            .OrderBy(cds => cds.OrderBy)
                                            .ToList();
            }


            //Get the discharges grouped by reason
            var groupedDischarges = allChildDischarges.GroupBy(acd => acd.DischargeCodeFK)
                                                        .Select(g => new
                                                        {
                                                            DischargeCodeFK = (int)g.Key,
                                                            NumDischarges = g.Select(acd => acd.ChildProgramPK).Count(),
                                                            PercentOfTotal = (double)g.Select(acd => acd.ChildProgramPK).Count() / totalDischarges
                                                        }).ToList();

            //Left join the code information on the grouped discharges
            var finalDischargeInfo = allDischargeReasons.GroupJoin(groupedDischarges,
                                                        dr => dr.CodeDischargeReasonPK,
                                                        gd => gd.DischargeCodeFK,
                                                        (dr, gd) => new
                                                        {
                                                            DischargeReasonAbbreviation = dr.Abbreviation,
                                                            DischargeReason = dr.Description,
                                                            NumDischarges = (gd.FirstOrDefault() == null ? 0 : gd.First().NumDischarges),
                                                            PercentOfTotal = (gd.FirstOrDefault() == null ? 0 : gd.First().PercentOfTotal)
                                                        }).ToList();

            //Get the chart info (exclude zeros)
            var chartInfo = finalDischargeInfo.Where(fdi => fdi.NumDischarges > 0).ToList();

            //--------------------- Number of Discharges Chart Start -----------------------

            //Set the chart data source
            NumberOfDischargesChart.DataSource = chartInfo;

            //Set the Total # of Discharges chart series
            NumberOfDischargesChart.Series[0].View.Color = Utilities.DevExChartColors.Red1;
            NumberOfDischargesChart.Series[0].ArgumentScaleType = ScaleType.Auto;
            NumberOfDischargesChart.Series[0].ArgumentDataMember = "DischargeReasonAbbreviation";
            NumberOfDischargesChart.Series[0].ValueScaleType = ScaleType.Numerical;
            NumberOfDischargesChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumDischarges" });

            //--------------------- Number of Discharges Chart End -----------------------

            //--------------------- Percent of Total Discharges Chart Start -----------------------

            //Set the chart data source
            PercentOfTotalDischargesChart.DataSource = chartInfo;

            //Set the % of Total Discharges chart series
            PercentOfTotalDischargesChart.Series[0].ArgumentScaleType = ScaleType.Auto;
            PercentOfTotalDischargesChart.Series[0].ArgumentDataMember = "DischargeReasonAbbreviation";
            PercentOfTotalDischargesChart.Series[0].ValueScaleType = ScaleType.Numerical;
            PercentOfTotalDischargesChart.Series[0].ValueDataMembers.AddRange(new string[] { "NumDischarges" });

            //--------------------- Percent of Total Discharges Chart End -----------------------

            //--------------------- Discharge Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.DischargeChartsReport.DataSource = finalDischargeInfo;
            this.DischargeChartsDetail.SortFields.Add(new GroupField("NumDischarges", XRColumnSortOrder.Descending));

            //Set the detail band label expressions
            lblDischargeReason.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "DischargeReason"));
            lblDischargeAbbreviation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "DischargeReasonAbbreviation"));
            lblNumDischarges.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "NumDischarges"));
            lblPercentDischarges.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfTotal"));

            //Set the group footer label expressions
            lblNumDischargesTotal.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumSum([NumDischarges])"));
            lblNumDischargesTotal.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblPercentDischargesTotal.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumSum([PercentOfTotal])"));
            lblPercentDischargesTotal.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //--------------------- Discharge Table End -----------------------

            //--------------------- Child Details Table Start -----------------------

            //Set the data source and sort for the detail of the report
            this.ChildDetailReport.DataSource = allChildDischarges;
            this.ChildDetailsGroupHeader.GroupFields.Add(new GroupField("ProgramName", XRColumnSortOrder.Ascending));
            this.ChildDetailsDetail.SortFields.Add(new GroupField("ClassroomID", XRColumnSortOrder.Ascending));
            this.ChildDetailsDetail.SortFields.Add(new GroupField("ChildID", XRColumnSortOrder.Ascending));

            //Set the detail band label expressions
            lblProgramName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ProgramName"));
            lblClassroomID.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ClassroomID"));
            lblChildID.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ChildID"));
            lblChildName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(?ParamViewPrivateChildInfo == True, ChildName, 'HIDDEN')"));
            lblChildDOB.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(?ParamViewPrivateChildInfo == True, FormatString('{0:MM/dd/yyyy}', BirthDate), 'HIDDEN')"));
            lblEnrollmentDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EnrollmentDate"));
            lblDischargeDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "DischargeDate"));
            lblChildDischargeReason.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "DischargeReason"));

            //--------------------- Child Details Table End -----------------------
        }
    }
}
