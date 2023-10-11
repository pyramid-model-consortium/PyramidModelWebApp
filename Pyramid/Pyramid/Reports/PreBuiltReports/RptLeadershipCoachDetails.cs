using System;
using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using DevExpress.XtraCharts;
using DevExpress.DataProcessing;
using Pyramid.Code;
using DevExpress.XtraReports.UI.CrossTab;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptLeadershipCoachDetails : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        //List for all the leadership coaches and master cadre rows in the UserProgramRole table.
        List<string> superAdminUsernames;
        List<string> activeLCUsernames;
        List<PyramidUser> activeLCUsers;
        List<UserProgramRole> MCRoles;
        List<UserProgramRole> LCRoles;

        public RptLeadershipCoachDetails()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptLeadershipCoachDetails_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Get the report parameters
            string programFKs = Convert.ToString(Parameters["ParamProgramFKs"].Value);
            string hubFKs = Convert.ToString(Parameters["ParamHubFKs"].Value);
            string cohortFKs = Convert.ToString(Parameters["ParamCohortFKs"].Value);
            string stateFKs = Convert.ToString(Parameters["ParamStateFKs"].Value);
            List<int> intProgramFKs = (string.IsNullOrWhiteSpace(programFKs) ? new List<int>() : programFKs.Split(',').Select(int.Parse).ToList());
            List<int> intHubFKs = (string.IsNullOrWhiteSpace(hubFKs) ? new List<int>() : hubFKs.Split(',').Select(int.Parse).ToList());
            List<int> intCohortFKs = (string.IsNullOrWhiteSpace(cohortFKs) ? new List<int>() : cohortFKs.Split(',').Select(int.Parse).ToList());
            List<int> intStateFKs = (string.IsNullOrWhiteSpace(stateFKs) ? new List<int>() : stateFKs.Split(',').Select(int.Parse).ToList());

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the super admin usernames
                superAdminUsernames = context.UserProgramRole
                                                .AsNoTracking()
                                                .Where(upr => upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
                                                .Select(upr => upr.Username)
                                                .Distinct()
                                                .ToList();

                //Get all the leadership coach roles (exclude super admins)
                LCRoles = context.UserProgramRole
                                            .Include(upr => upr.CodeProgramRole)
                                            .Include(upr => upr.Program)
                                            .Include(upr => upr.Program.Hub)
                                            .Include(upr => upr.Program.State)
                                            .AsNoTracking()
                                            .Where(upr => superAdminUsernames.Contains(upr.Username) == false &&
                                                        (intProgramFKs.Contains(upr.ProgramFK) ||
                                                        intHubFKs.Contains(upr.Program.HubFK) ||
                                                        intCohortFKs.Contains(upr.Program.CohortFK) ||
                                                        intStateFKs.Contains(upr.Program.StateFK)) &&
                                                            (upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH ||
                                                             upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ||
                                                             upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH))
                                            .ToList();

                //Get the usernames for Leadership Coach users
                activeLCUsernames = LCRoles.Select(upr => upr.Username).Distinct().ToList();

                //Get all the Master Cadre roles for Leadership Coach users
                MCRoles = context.UserProgramRole
                                            .AsNoTracking()
                                            .Where(upr => (activeLCUsernames.Contains(upr.Username) &&
                                                             upr.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER))
                                            .ToList();
            }


            using (ApplicationDbContext appcontext = new ApplicationDbContext())
            {
                //Get the active LC users
                activeLCUsers = appcontext.Users.AsNoTracking().Where(u => u.AccountEnabled == true && activeLCUsernames.Contains(u.UserName)).ToList();
            }

            //Create the report data source
            var reportDataSource = activeLCUsers.Select(au => new { 
                UserInfo = au, 
                RoleInfo = LCRoles
                                .Where(r => r.Username == au.UserName)
                                .Select(r => new
                                {
                                    RoleDescription = string.Format("{0}{1} for {2}",
                                                    (r.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ? "" : "Program "),
                                                    r.CodeProgramRole.RoleName,
                                                    (r.ProgramRoleCodeFK == (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH ? r.Program.Hub.Name : r.Program.ProgramName))
                                })
                                .ToList(), 
                IsMasterCadreMember = MCRoles.Exists(upr => upr.Username == au.UserName) 
            }).ToList();

            //Bind the primary detail report
            this.LeadershipCoachDetailReport.DataSource = reportDataSource;
            this.LeadershipCoachGroupHeader.GroupFields.Add(new GroupField("UserInfo.UserName", XRColumnSortOrder.Ascending));

            lblLCDUsername.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UserInfo.UserName"));
            lblLCDName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0} {1}', [UserInfo.FirstName], [UserInfo.LastName])"));
            lblLCDIsMCMember.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif([IsMasterCadreMember] == true, 'Yes', 'No')"));
            lblLCDEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UserInfo.Email"));
            lblLCDPersonalPhone.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UserInfo.PhoneNumber"));
            lblLCDWorkPhone.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UserInfo.WorkPhoneNumber"));
            lblLCDAddress.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif(IsNullOrEmpty([UserInfo.Street]), '', FormatString('{0} {1}, {2} {3}', [UserInfo.Street], [UserInfo.City], [UserInfo.State], [UserInfo.ZIPCode]))"));
            lblLCDRegion.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UserInfo.RegionLocation"));


            //Bind the role detail report
            this.RolesDetailReport.DataSource = reportDataSource;
            this.RolesDetailReport.DataMember = "RoleInfo";
            this.RolesDetail.SortFields.Add(new GroupField("RoleDescription", XRColumnSortOrder.Ascending));

            lblRDRoleInfo.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "RoleDescription"));

        }
    }
}
