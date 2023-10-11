using System;
using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBIRProfiles : MasterReports.RptTableOfContentsMaster
    {
        //To hold all the BIR info
        List<rspBIRAllInfo_Result> allBIRInfo;
        List<rspEnrolledChildren_Result> allEnrolledChildren;
        int totalBIRs;
        int totalChildren;
        int totalChildrenWithBIRs;

        public RptBIRProfiles()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptBIRProfiles_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the parameters
            DateTime startDate = Convert.ToDateTime(Parameters["ParamStartDate"].Value);
            DateTime endDate = Convert.ToDateTime(Parameters["ParamEndDate"].Value);
            string programFKs = Convert.ToString(Parameters["ParamProgramFKs"].Value);
            string hubFKs = Convert.ToString(Parameters["ParamHubFKs"].Value);
            string cohortFKs = Convert.ToString(Parameters["ParamCohortFKs"].Value);
            string stateFKs = Convert.ToString(Parameters["ParamStateFKs"].Value);
            string childFKs = Convert.ToString(Parameters["ParamChildFKs"].Value);
            string classroomFKs = Convert.ToString(Parameters["ParamClassroomFKs"].Value);
            string raceFKs = Convert.ToString(Parameters["ParamRaceFKs"].Value);
            string ethnicityFKs = Convert.ToString(Parameters["ParamEthnicityFKs"].Value);
            string genderFKs = Convert.ToString(Parameters["ParamGenderFKs"].Value);
            string problemBehaviorFKs = Convert.ToString(Parameters["ParamProblemBehaviorFKs"].Value);
            string activityFKs = Convert.ToString(Parameters["ParamActivityFKs"].Value);
            string othersInvolvedFKs = Convert.ToString(Parameters["ParamOthersInvolvedFKs"].Value);
            string possibleMotivationFKs = Convert.ToString(Parameters["ParamPossibleMotivationFKs"].Value);
            string strategyResponseFKs = Convert.ToString(Parameters["ParamStrategyResponseFKs"].Value);
            string adminFollowUpFKs = Convert.ToString(Parameters["ParamAdminFollowUpFKs"].Value);
            bool? hasIEP = (Parameters["ParamIEP"].Value == null ? (bool?)null : Convert.ToBoolean(Parameters["ParamIEP"].Value));
            bool? isDLL = (Parameters["ParamDLL"].Value == null ? (bool?)null : Convert.ToBoolean(Parameters["ParamDLL"].Value));
            string grouping = Convert.ToString(Parameters["ParamBIRProfileGroup"].Value);
            string item = Convert.ToString(Parameters["ParamBIRItem"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the BIR info for the timeframe
                allBIRInfo = context.rspBIRAllInfo(startDate, endDate, childFKs,
                                                    classroomFKs, raceFKs, ethnicityFKs,
                                                    genderFKs, problemBehaviorFKs, activityFKs,
                                                    othersInvolvedFKs, possibleMotivationFKs, strategyResponseFKs,
                                                    adminFollowUpFKs, hasIEP, isDLL, programFKs, hubFKs, cohortFKs, stateFKs).ToList();

                //Get all the enrolled children for the timeframe
                allEnrolledChildren = context.rspEnrolledChildren(startDate, endDate, childFKs,
                                                    classroomFKs, raceFKs, ethnicityFKs,
                                                    genderFKs, hasIEP, isDLL, programFKs, hubFKs, cohortFKs, stateFKs).ToList();
            }

            //Get the totals
            totalBIRs = allBIRInfo.Count;
            totalChildren = allEnrolledChildren.Count;
            totalChildrenWithBIRs = allBIRInfo.Select(abi => abi.ChildFK).Distinct().Count();

            //Set the total labels
            lblTotalBIRs.Text = totalBIRs.ToString();
            lblTotalChildren.Text = totalChildren.ToString();
            lblTotalChildrenWithBIRs.Text = totalChildrenWithBIRs.ToString();

            //Fill the profiles
            FillProfile(grouping, item);
        }

        #region Report-Specific Classes

        /// <summary>
        /// This class holds code table information
        /// </summary>
        private class CodeTableInformation
        {
            public int CodePK { get; set; }
            public string Abbreviation { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// This class holds necessary information for filling the profile report
        /// </summary>
        private class GroupProfileInformation
        {
            public string GroupAbbreviation { get; set; }
            public string GroupDescription { get; set; }
            public string ItemAbbreviation { get; set; }
            public string ItemDescription { get; set; }
            public int GroupChildrenEnrolled { get; set; }
            public int GroupChildrenWithItems { get; set; }
            public int GroupItems { get; set; }
            public int TotalItems { get; set; }
            public int TotalChildrenEnrolled { get; set; }
            public int TotalChildrenWithItems { get; set; }
            public double PercentOfEnrollment => Utilities.GetValidatedDoubleValue((double)GroupChildrenEnrolled / TotalChildrenEnrolled);
            public double ItemRate => Utilities.GetValidatedDoubleValue((double)GroupItems / GroupChildrenEnrolled);
            public string ItemRateExplanation => string.Format("Children identified as {0} receive an average of {1:0.00} {2}s per child.", 
                                                            GroupDescription, ItemRate, ItemDescription);
            public double ItemRatio => Utilities.GetValidatedDoubleValue(ItemRate / ((double)(TotalItems - GroupItems) / (TotalChildrenEnrolled - GroupChildrenEnrolled)));
            public string ItemRatioExplanation => string.Format("The average number of {0}s per child for {1} children is {2:0.00} times the {0} rate for all other children.", 
                                                            ItemDescription, GroupDescription, ItemRatio);
            public double Risk => Utilities.GetValidatedDoubleValue((double)GroupChildrenWithItems / GroupChildrenEnrolled);
            public string RiskExplanation => string.Format("Of the {0} {1} children, {2:0.0%} have at least one {3}.",
                                                            GroupChildrenEnrolled, GroupDescription,
                                                            Risk, ItemDescription);
            public double RiskRatio => Utilities.GetValidatedDoubleValue(Risk / ((double)(TotalChildrenWithItems - GroupChildrenWithItems) / (TotalChildrenEnrolled - GroupChildrenEnrolled)));
            public string RiskRatioExplanation => string.Format("{0} children are {1:0.00} times more likely to have at least one {2} than all other children.", 
                                                            GroupDescription, RiskRatio, ItemDescription);
            public double ChildComp => Utilities.GetValidatedDoubleValue((double)GroupChildrenWithItems / TotalChildrenWithItems);
            public string ChildCompExplanation => string.Format("Of the {0} children who received at least one {1}, {2:0.0%} are {3}; this group comprises {4:0.0%} of the total child enrollment.", 
                                                            TotalChildrenWithItems, ItemDescription, ChildComp, 
                                                            GroupDescription, PercentOfEnrollment);
            public double ChildCompDifference => Utilities.GetValidatedDoubleValue((ChildComp - PercentOfEnrollment) * 100);
            public string ChildCompDifferenceExplanation => string.Format("{0} children's representation among children who receive {1}s is {2:0.00} percentage points {3} than expected given {0} children's percentage of the child enrollment.", 
                                                                    GroupDescription, ItemDescription, 
                                                                    Math.Abs(ChildCompDifference), 
                                                                    ChildCompDifference > 0 ? "higher" : "lower");
            public double ItemComp => Utilities.GetValidatedDoubleValue((double)GroupItems / TotalItems);
            public string ItemCompExplanation => string.Format("Of the {0} {1}s generated, {2:0.0%} were attributed to {3} children.", 
                                                        TotalItems, ItemDescription, ItemComp, GroupDescription);
            public double ItemCompDifference => Utilities.GetValidatedDoubleValue((ItemComp - PercentOfEnrollment) * 100);
            public string ItemCompDifferenceExplanation => string.Format("The percentage of {0}s attributed to {1} children is {2:0.00} percentage points {3} than expected given {1} children's percentage of the child enrollment.", 
                                                                    ItemDescription, GroupDescription, 
                                                                    Math.Abs(ItemCompDifference), 
                                                                    ItemCompDifference > 0 ? "higher" : "lower");
            public double EFormulaComp => Utilities.GetValidatedDoubleValue(PercentOfEnrollment + Math.Sqrt(PercentOfEnrollment * ((1.0 - PercentOfEnrollment) / TotalChildrenWithItems)));
        }

        #endregion

        #region Filling Report

        /// <summary>
        /// This method fills the report with the profile information for the specified group
        /// and item
        /// </summary>
        /// <param name="group">The group (race, ethnicity, etc.)</param>
        /// <param name="item">The item (BIR, OSS, ISS, Dismissal)</param>
        private void FillProfile(string group, string item)
        {
            //These lists hold the profile information and code table information
            List<GroupProfileInformation> profileInfo = new List<GroupProfileInformation>();
            List<CodeTableInformation> codeTableInfo = new List<CodeTableInformation>();

            //These strings hold information about the grouping and item
            string groupDescription = "", groupingField = "", 
                itemDescription = "", itemAbbreviation = "";

            //----- Calculate the code information -----
            if (group.ToLower() == "race")
            {
                //Set the group description string
                groupDescription = "Race";
                groupingField = "RaceCodeFK";

                //Get the code table information
                using (PyramidContext context = new PyramidContext())
                {
                    codeTableInfo = context.CodeRace.AsNoTracking()
                        .OrderBy(cr => cr.Abbreviation)
                        .Select(cr => new CodeTableInformation() { 
                            CodePK = cr.CodeRacePK,
                            Abbreviation = cr.Abbreviation,
                            Description = cr.Description
                        }).ToList();
                }
            }
            else if (group.ToLower() == "ethnicity")
            {
                //Set the group description string
                groupDescription = "Ethnicity";
                groupingField = "EthnicityCodeFK";

                //Get the code table information
                using (PyramidContext context = new PyramidContext())
                {
                    codeTableInfo = context.CodeEthnicity.AsNoTracking()
                        .OrderBy(ce => ce.Abbreviation)
                        .Select(ce => new CodeTableInformation()
                        {
                            CodePK = ce.CodeEthnicityPK,
                            Abbreviation = ce.Abbreviation,
                            Description = ce.Description
                        }).ToList();
                }
            }
            else if (group.ToLower() == "gender")
            {
                //Set the group description string
                groupDescription = "Gender";
                groupingField = "GenderCodeFK";

                //Get the code table information
                using (PyramidContext context = new PyramidContext())
                {
                    codeTableInfo = context.CodeGender.AsNoTracking()
                        .OrderBy(cg => cg.Abbreviation)
                        .Select(cg => new CodeTableInformation()
                        {
                            CodePK = cg.CodeGenderPK,
                            Abbreviation = cg.Abbreviation,
                            Description = cg.Description
                        }).ToList();
                }
            }
            else if (group.ToLower() == "iep")
            {
                //Set the group description string
                groupDescription = "IEP";
                groupingField = "IEPInt";

                //Add the IEP information to the code table info
                codeTableInfo.Add(new CodeTableInformation()
                {
                    CodePK = 1,
                    Abbreviation = "IEP",
                    Description = "Individualized Education Program"
                });

                codeTableInfo.Add(new CodeTableInformation()
                {
                    CodePK = 0,
                    Abbreviation = "GenEd",
                    Description = "General Education"
                });
            }
            else if (group.ToLower() == "dll")
            {
                //Set the group description string
                groupDescription = "DLL";
                groupingField = "DLLInt";

                //Add the IEP information to the code table info
                codeTableInfo.Add(new CodeTableInformation()
                {
                    CodePK = 1,
                    Abbreviation = "DLL",
                    Description = "Dual Language Learner"
                });

                codeTableInfo.Add(new CodeTableInformation()
                {
                    CodePK = 0,
                    Abbreviation = "Non-DLL",
                    Description = "Non Dual Language Learner"
                });
            }

            //----- Calculate the item numbers -----
            if (item.ToLower() == "bir")
            {
                //Set the item description
                itemDescription = "Incident Frequency";
                itemAbbreviation = "BIR";

                //Get the initial grouping
                var initialGroup = allBIRInfo.GroupBy(abi => Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField))
                                         .Select(g => new
                                         {
                                             CodeFK = (int)g.Key,
                                             ChildrenWithBIRs = g.Select(abi => abi.ChildFK).Distinct().Count(),
                                             NumBIRs = g.Select(abi => abi.BehaviorIncidentPK).Count()
                                         }).ToList();

                //Left join the code table info on the initial grouping
                var finalGroup = codeTableInfo.GroupJoin(initialGroup,
                                                         cti => cti.CodePK,
                                                         ri => ri.CodeFK,
                                                         (cti, ri) => new
                                                         {
                                                             CodeFK = cti.CodePK,
                                                             GroupAbbreviation = cti.Abbreviation,
                                                             GroupDescription = cti.Description,
                                                             GroupCounts = ri.FirstOrDefault()
                                                         }).ToList();

                //Convert the final grouping into GroupProfileInformation objects
                profileInfo = finalGroup.Select(gri => new GroupProfileInformation()
                {
                    GroupAbbreviation = gri.GroupAbbreviation,
                    GroupDescription = gri.GroupDescription,
                    ItemAbbreviation = "BIR",
                    ItemDescription = "BIR",
                    GroupChildrenEnrolled = allEnrolledChildren.Where(aec => (int)Utilities.GetPropertyValue(aec, typeof(rspEnrolledChildren_Result), groupingField) == gri.CodeFK).Count(),
                    GroupChildrenWithItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.ChildrenWithBIRs),
                    GroupItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.NumBIRs),
                    TotalChildrenEnrolled = totalChildren,
                    TotalChildrenWithItems = totalChildrenWithBIRs,
                    TotalItems = totalBIRs
                }).ToList();
            }
            else if (item.ToLower() == "iss")
            {
                //Set the item description
                itemDescription = "In-School Suspension Events";
                itemAbbreviation = "ISS";

                //Get the initial grouping
                var initialGroup = allBIRInfo.GroupBy(abi => Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField))
                                         .Select(g => new
                                         {
                                             CodeFK = (int)g.Key,
                                             ChildrenWithISSs = g.Where(abi => abi.StrategyResponseCodeFK == 12 || abi.AdminFollowUpCodeFK == 7).Select(abi => abi.ChildFK).Distinct().Count(),
                                             NumISSs = g.Where(abi => abi.StrategyResponseCodeFK == 12 || abi.AdminFollowUpCodeFK == 7).Select(abi => abi.BehaviorIncidentPK).Count()
                                         }).ToList();

                //Left join the code table info on the initial grouping
                var finalGroup = codeTableInfo.GroupJoin(initialGroup,
                                                         cti => cti.CodePK,
                                                         ri => ri.CodeFK,
                                                         (cti, ri) => new
                                                         {
                                                             CodeFK = cti.CodePK,
                                                             GroupAbbreviation = cti.Abbreviation,
                                                             GroupDescription = cti.Description,
                                                             GroupCounts = ri.FirstOrDefault()
                                                         }).ToList();

                //Get totals across groups
                int totalChildrenWithISSs = allBIRInfo.Where(abi => abi.StrategyResponseCodeFK == 12 || abi.AdminFollowUpCodeFK == 7).Select(abi => abi.ChildFK).Distinct().Count();
                int totalISSs = allBIRInfo.Where(abi => abi.StrategyResponseCodeFK == 12 || abi.AdminFollowUpCodeFK == 7).Select(abi => abi.BehaviorIncidentPK).Count();

                //Convert the final grouping into GroupProfileInformation objects
                profileInfo = finalGroup.Select(gri => new GroupProfileInformation()
                {
                    GroupAbbreviation = gri.GroupAbbreviation,
                    GroupDescription = gri.GroupDescription,
                    ItemAbbreviation = "ISS",
                    ItemDescription = "In-School Suspension",
                    GroupChildrenEnrolled = allEnrolledChildren.Where(aec => (int)Utilities.GetPropertyValue(aec, typeof(rspEnrolledChildren_Result), groupingField) == gri.CodeFK).Count(),
                    GroupChildrenWithItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.ChildrenWithISSs),
                    GroupItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.NumISSs),
                    TotalChildrenEnrolled = totalChildren,
                    TotalChildrenWithItems = totalChildrenWithISSs,
                    TotalItems = totalISSs
                }).ToList();
            }
            else if (item.ToLower() == "oss")
            {
                //Set the item description
                itemDescription = "Out-of-School Suspension Events";
                itemAbbreviation = "OSS";

                //Get the initial grouping
                var initialGroup = allBIRInfo.GroupBy(abi => Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField))
                                         .Select(g => new
                                         {
                                             CodeFK = (int)g.Key,
                                             ChildrenWithOSSs = g.Where(abi => abi.AdminFollowUpCodeFK == 8 || abi.AdminFollowUpCodeFK == 9 || abi.AdminFollowUpCodeFK == 12).Select(abi => abi.ChildFK).Distinct().Count(),
                                             NumOSSs = g.Where(abi => abi.AdminFollowUpCodeFK == 8 || abi.AdminFollowUpCodeFK == 9 || abi.AdminFollowUpCodeFK == 12).Select(abi => abi.BehaviorIncidentPK).Count()
                                         }).ToList();

                //Left join the code table info on the initial grouping
                var finalGroup = codeTableInfo.GroupJoin(initialGroup,
                                                         cti => cti.CodePK,
                                                         ri => ri.CodeFK,
                                                         (cti, ri) => new
                                                         {
                                                             CodeFK = cti.CodePK,
                                                             GroupAbbreviation = cti.Abbreviation,
                                                             GroupDescription = cti.Description,
                                                             GroupCounts = ri.FirstOrDefault()
                                                         }).ToList();

                //Get totals across groups
                int totalChildrenWithOSSs = allBIRInfo.Where(abi => abi.AdminFollowUpCodeFK == 8 || abi.AdminFollowUpCodeFK == 9 || abi.AdminFollowUpCodeFK == 12).Select(abi => abi.ChildFK).Distinct().Count();
                int totalOSSs = allBIRInfo.Where(abi => abi.AdminFollowUpCodeFK == 8 || abi.AdminFollowUpCodeFK == 9 || abi.AdminFollowUpCodeFK == 12).Select(abi => abi.BehaviorIncidentPK).Count();

                //Convert the final grouping into GroupProfileInformation objects
                profileInfo = finalGroup.Select(gri => new GroupProfileInformation()
                {
                    GroupAbbreviation = gri.GroupAbbreviation,
                    GroupDescription = gri.GroupDescription,
                    ItemAbbreviation = "OSS",
                    ItemDescription = "Out-of-School Suspension",
                    GroupChildrenEnrolled = allEnrolledChildren.Where(aec => (int)Utilities.GetPropertyValue(aec, typeof(rspEnrolledChildren_Result), groupingField) == gri.CodeFK).Count(),
                    GroupChildrenWithItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.ChildrenWithOSSs),
                    GroupItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.NumOSSs),
                    TotalChildrenEnrolled = totalChildren,
                    TotalChildrenWithItems = totalChildrenWithOSSs,
                    TotalItems = totalOSSs
                }).ToList();
            }
            else if (item.ToLower() == "dismissal")
            {
                //Set the item description
                itemDescription = "Dismissal Events";
                itemAbbreviation = "Dismissal";

                //Get the initial grouping
                var initialGroup = allBIRInfo.GroupBy(abi => Utilities.GetPropertyValue(abi, typeof(rspBIRAllInfo_Result), groupingField))
                                         .Select(g => new
                                         {
                                             CodeFK = (int)g.Key,
                                             ChildrenWithDismissals = g.Where(abi => abi.AdminFollowUpCodeFK == 11 || abi.AdminFollowUpCodeFK == 13).Select(abi => abi.ChildFK).Distinct().Count(),
                                             NumDismissals = g.Where(abi => abi.AdminFollowUpCodeFK == 11 || abi.AdminFollowUpCodeFK == 13).Select(abi => abi.BehaviorIncidentPK).Count()
                                         }).ToList();

                //Left join the code table info on the initial grouping
                var finalGroup = codeTableInfo.GroupJoin(initialGroup,
                                                         cti => cti.CodePK,
                                                         ri => ri.CodeFK,
                                                         (cti, ri) => new
                                                         {
                                                             CodeFK = cti.CodePK,
                                                             GroupAbbreviation = cti.Abbreviation,
                                                             GroupDescription = cti.Description,
                                                             GroupCounts = ri.FirstOrDefault()
                                                         }).ToList();

                //Get totals across groups
                int totalChildrenWithDismissals = allBIRInfo.Where(abi => abi.AdminFollowUpCodeFK == 11 || abi.AdminFollowUpCodeFK == 13).Select(abi => abi.ChildFK).Distinct().Count();
                int totalDismissals = allBIRInfo.Where(abi => abi.AdminFollowUpCodeFK == 11 || abi.AdminFollowUpCodeFK == 13).Select(abi => abi.ChildFK).Count();

                //Convert the final grouping into GroupProfileInformation objects
                profileInfo = finalGroup.Select(gri => new GroupProfileInformation()
                {
                    GroupAbbreviation = gri.GroupAbbreviation,
                    GroupDescription = gri.GroupDescription,
                    ItemAbbreviation = "Dismissal",
                    ItemDescription = "Dismissal",
                    GroupChildrenEnrolled = allEnrolledChildren.Where(aec => (int)Utilities.GetPropertyValue(aec, typeof(rspEnrolledChildren_Result), groupingField) == gri.CodeFK).Count(),
                    GroupChildrenWithItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.ChildrenWithDismissals),
                    GroupItems = (gri.GroupCounts == null ? 0 : gri.GroupCounts.NumDismissals),
                    TotalChildrenEnrolled = totalChildren,
                    TotalChildrenWithItems = totalChildrenWithDismissals,
                    TotalItems = totalDismissals
                }).ToList();
            }

            //Set the cover page information
            lblChildCompCoverPage.Text = string.Format("The % of children who belong to a target group and also have at least one {0}.", itemAbbreviation);
            lblRatioCoverPageTitle.Text = string.Format("{0} Ratio:", itemAbbreviation);
            lblRatioCoverPage.Text = string.Format("{0} Rate for a target group divided by the {0} rate for all other children; 1.00 is equal.", itemAbbreviation);
            lblRateCoverPageTitle.Text = string.Format("{0} Rate:", itemAbbreviation);
            lblRateCoverPage.Text = string.Format("Total # of {0}s from Group divided by the # of Children Enrolled for that group.", itemAbbreviation);
            lblRiskRatioCoverPage.Text = string.Format("A target group's risk of receiving a {0} compared to all other children; 1.00 is equal.", itemAbbreviation);
            lblChildCompDiffCoverPage.Text = "Child Comp minus the Group's % of Enrollment; positive values suggest disproportionality.";
            lblBIRCompDiffCoverPageTitle.Text = string.Format("Difference in {0} Comp:", itemAbbreviation);
            lblBIRCompDiffCoverPage.Text = string.Format("% of {0}s accounted for by children in a target group.", itemAbbreviation);
            lblBIRCompCoverPageTitle.Text = string.Format("{0} Comp:", itemAbbreviation);
            lblBIRCompCoverPage.Text = string.Format("Total # of {0}s from Group divided by the Total # of {0}s for all other children.", itemAbbreviation);
            lblRiskCoverPage.Text = string.Format("% of children in a target group who have at least one {0}.", itemAbbreviation);
            lblEFormulaCoverPage.Text = "The upper bound of what would be expected given the size of the population.";

            //Set the title labels based on the selected group and item
            lblIncidentProfileTitle.Text = string.Format("{0} Equity Profile for {1}", groupDescription, itemDescription);
            lblIncidentProfileTitle.Bookmark = string.Format("{0} Equity Profile for {1}", groupDescription, itemDescription);
            lblChildrenWithItem.Text = string.Format("# of Children in Group with a(n) {0}", itemAbbreviation);
            lblTotalItems.Text = string.Format("Total # of {0}s from Group", itemAbbreviation);
            lblRatioTableTitle.Text = string.Format("{0} Ratio", itemAbbreviation);
            lblRatioExplTitle.Text = string.Format("{0} Ratio", itemAbbreviation);
            lblRateTableTitle.Text = string.Format("{0} Rate", itemAbbreviation);
            lblRateExplTitle.Text = string.Format("{0} Rate", itemAbbreviation);
            lblBIRCompDiffTableTitle.Text = string.Format("Difference in {0} Comp", itemAbbreviation);
            lblBIRCompDiffExplTitle.Text = string.Format("Difference in {0} Comp", itemAbbreviation);
            lblBIRCompTableTitle.Text = string.Format("{0} Comp", itemAbbreviation);
            lblBIRCompExplTitle.Text = string.Format("{0} Comp", itemAbbreviation);
            lblIncidentProfileOverviewChart.Text = string.Format("{0} Information by {1}", itemDescription, groupDescription);
            lblIncidentProfileOverviewChart.Bookmark = string.Format("{0} Information by {1}", itemDescription, groupDescription);
            lblIncidentProfileRiskRatioChart.Text = string.Format("{0} Risk Ratio by {1}", itemDescription, groupDescription);
            lblIncidentProfileRiskRatioChart.Bookmark = string.Format("{0} Risk Ratio by {1}", itemDescription, groupDescription);

            //Set the title label colors
            lblChildCompTableTitle.BackColor = Utilities.DevExChartColors.LightBlue1;
            lblChildCompExplTitle.BackColor = Utilities.DevExChartColors.LightBlue1;
            lblRatioTableTitle.BackColor = Utilities.DevExChartColors.LightOrange1;
            lblRatioExplTitle.BackColor = Utilities.DevExChartColors.LightOrange1;
            lblRateTableTitle.BackColor = Utilities.DevExChartColors.LightGray1;
            lblRateExplTitle.BackColor = Utilities.DevExChartColors.LightGray1;
            lblRiskRatioTableTitle.BackColor = Utilities.DevExChartColors.LightYellow1;
            lblRiskRatioExplTitle.BackColor = Utilities.DevExChartColors.LightYellow1;
            lblChildCompDiffTableTitle.BackColor = Utilities.DevExChartColors.LightBlue2;
            lblChildCompDiffExplTitle.BackColor = Utilities.DevExChartColors.LightBlue2;
            lblBIRCompDiffTableTitle.BackColor = Utilities.DevExChartColors.LightGreen1;
            lblBIRCompDiffExplTitle.BackColor = Utilities.DevExChartColors.LightGreen1;
            lblBIRCompTableTitle.BackColor = Utilities.DevExChartColors.LightRed1;
            lblBIRCompExplTitle.BackColor = Utilities.DevExChartColors.LightRed1;
            lblRiskTableTitle.BackColor = Utilities.DevExChartColors.LightPurple1;
            lblRiskExplTitle.BackColor = Utilities.DevExChartColors.LightPurple1;

            //Set the detail report data source and sort
            IncidentProfileTableDetailReport.DataSource = profileInfo;
            IncidentProfileTableDetail.SortFields.Add(new GroupField("GroupAbbreviation", XRColumnSortOrder.Ascending));

            //Set the label expressions
            lblIncGroupAbbreviation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroupAbbreviation"));
            lblIncChildrenEnrolled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroupChildrenEnrolled"));
            lblIncChildrenWithItem.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroupChildrenWithItems"));
            lblIncItems.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "GroupItems"));
            lblIncPercentEnrollment.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PercentOfEnrollment"));
            lblIncBIRRate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemRate"));
            lblIncBIRRatio.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemRatio"));
            lblIncRisk.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Risk"));
            lblIncBIRRiskRatio.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "RiskRatio"));
            lblIncChildComp.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ChildComp"));
            lblIncChildCompDiff.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ChildCompDifference"));
            lblIncBIRComp.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemComp"));
            lblIncBIRCompDiff.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemCompDifference"));
            lblIncEFormula.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "EFormulaComp"));

            //Set the group footer label expressions
            lblIncTotalChildrenEnrolled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumSum([GroupChildrenEnrolled])"));
            lblIncTotalChildrenEnrolled.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblIncTotalChildrenWithItem.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumSum([GroupChildrenWithItems])"));
            lblIncTotalChildrenWithItem.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblIncTotalItems.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumSum([GroupItems])"));
            lblIncTotalItems.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };
            lblIncTotalPercentEnrollment.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "sumSum([PercentOfEnrollment])"));
            lblIncTotalPercentEnrollment.Summary = new XRSummary()
            {
                Running = SummaryRunning.Report
            };

            //Set the detail report data source and sort
            IncidentProfileExplanationDetailReport.DataSource = profileInfo;
            IncidentProfileExplanationDetail.SortFields.Add(new GroupField("GroupAbbreviation", XRColumnSortOrder.Ascending));

            //Set the label expressions
            lblIncGroupExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('({0}) {1}', GroupAbbreviation, GroupDescription)"));
            lblIncGroupExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Bookmark", "FormatString('Details for {0} Children', GroupDescription)"));
            lblIncChildCompExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ChildCompExplanation"));
            lblIncBIRRatioExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemRatioExplanation"));
            lblIncBIRRateExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemRateExplanation"));
            lblIncRiskRatioExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "RiskRatioExplanation"));
            lblIncChildCompDiffExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ChildCompDifferenceExplanation"));
            lblIncBIRCompDiffExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemCompDifferenceExplanation"));
            lblIncBIRCompExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ItemCompExplanation"));
            lblIncRiskExplanation.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "RiskExplanation"));

            //Set the overview chart data source
            IncidentProfileOverviewChart.DataSource = profileInfo;

            //Set the # of Children Enrolled chart series
            IncidentProfileOverviewChart.Series[0].View.Color = Utilities.DevExChartColors.Blue4;
            IncidentProfileOverviewChart.Series[0].ArgumentDataMember = "GroupAbbreviation";
            IncidentProfileOverviewChart.Series[0].ValueScaleType = ScaleType.Numerical;
            IncidentProfileOverviewChart.Series[0].ValueDataMembers.AddRange(new string[] { "GroupChildrenEnrolled" });

            //Set the # of Children with a(n) item chart series
            IncidentProfileOverviewChart.Series[1].Name = string.Format("Total # of Children with a(n) {0}", itemAbbreviation);
            IncidentProfileOverviewChart.Series[1].View.Color = Utilities.DevExChartColors.Red2;
            IncidentProfileOverviewChart.Series[1].ArgumentDataMember = "GroupAbbreviation";
            IncidentProfileOverviewChart.Series[1].ValueScaleType = ScaleType.Numerical;
            IncidentProfileOverviewChart.Series[1].ValueDataMembers.AddRange(new string[] { "GroupChildrenWithItems" });

            //Set the # of Items chart series
            IncidentProfileOverviewChart.Series[2].Name = string.Format("Total # of {0}s", itemAbbreviation);
            IncidentProfileOverviewChart.Series[2].View.Color = Utilities.DevExChartColors.Purple2;
            IncidentProfileOverviewChart.Series[2].ArgumentDataMember = "GroupAbbreviation";
            IncidentProfileOverviewChart.Series[2].ValueScaleType = ScaleType.Numerical;
            IncidentProfileOverviewChart.Series[2].ValueDataMembers.AddRange(new string[] { "GroupItems" });

            //Set the risk ratio chart data source
            IncidentProfileRiskRatioChart.DataSource = profileInfo;

            //Set the risk ratio chart series
            IncidentProfileRiskRatioChart.Series[0].View.Color = Utilities.DevExChartColors.Orange2;
            IncidentProfileRiskRatioChart.Series[0].ArgumentDataMember = "GroupAbbreviation";
            IncidentProfileRiskRatioChart.Series[0].ValueScaleType = ScaleType.Numerical;
            IncidentProfileRiskRatioChart.Series[0].ValueDataMembers.AddRange(new string[] { "RiskRatio" });
        }

        #endregion
    }
}
