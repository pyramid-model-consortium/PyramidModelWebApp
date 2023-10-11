using System;
using System.Data;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using DevExpress.DataProcessing;


namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptUserDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptUserDataDump()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptUserDataDump_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary info from the database
            List<string> superAdminUsernames;
            List<string> activeUsernames;
            List<PyramidUser> filteredUsers;
            List<UserProgramRole> filteredUserRoles;
            
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
                                                .Where(upr => upr.ProgramRoleCodeFK == (int)Code.Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
                                                .Select(upr => upr.Username)
                                                .Distinct()
                                                .ToList();

                //Get all the roles for the selected state(s)/hub(s)/cohort(s)/program(s)
                //The program, hub, cohort, and state criteria work in an additive manner, but all other criteria are reductive.
                //For example, if you select a state and a program (even if it is outside the state), you will get all the user roles that
                //are within the state OR within the program.
                filteredUserRoles = context.UserProgramRole
                                            .Include(upr => upr.CodeProgramRole)
                                            .Include(upr => upr.Program)
                                            .Include(upr => upr.Program.Hub)
                                            .Include(upr => upr.Program.State)
                                            .AsNoTracking()
                                            .Where(upr => superAdminUsernames.Contains(upr.Username) == false &&
                                                        (intProgramFKs.Contains(upr.ProgramFK) ||
                                                        intHubFKs.Contains(upr.Program.HubFK) ||
                                                        intCohortFKs.Contains(upr.Program.CohortFK) ||
                                                        intStateFKs.Contains(upr.Program.StateFK)))
                                            .ToList();
            }

            //Get the usernames for active users
            activeUsernames = filteredUserRoles.Select(u => u.Username).Distinct().ToList();

            using (ApplicationDbContext appcontext = new ApplicationDbContext())
            {
                //Retrieve a list of all the users for the selected state(s)/hub(s)/cohort(s)/program(s)
                filteredUsers = appcontext.Users.AsNoTracking()
                                        .Where(u => activeUsernames.Contains(u.UserName))
                                        .ToList();
            }

            //Bind UserDetail data
            this.UsersDetailReport.DataSource = filteredUsers;
            this.UsersDetail.SortFields.Add(new GroupField("UserName", XRColumnSortOrder.Ascending));
            lblUDUsername.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UserName"));
            lblUDFirstName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FirstName"));
            lblUDLastName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "LastName"));
            lblUDAccountEnabled.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif([AccountEnabled] == true, 'Yes', 'No')"));
            lblUDTwoFactor.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif([TwoFactorEnabled] == true, 'Yes', 'No')"));
            lblUDEmail.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Email"));
            lblUDEmailConfirmed.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif([EmailConfirmed] == true, 'Yes', 'No')"));
            lblUDPhoneNumber.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "PhoneNumber"));
            lblUDPhoneNumberConfirmed.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Iif([PhoneNumberConfirmed] == true, 'Yes', 'No')"));
            lblUDWorkPhone.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "WorkPhoneNumber"));
            lblUDAddressStreet.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Street"));
            lblUDAddressCity.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "City"));
            lblUDAddressState.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "State"));
            lblUDAddressZIP.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ZIPCode"));
            lblUDRegion.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "RegionLocation"));
            lblUDCreator.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "CreatedBy"));
            lblUDCreateDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy hh:mm tt}', [CreateTime])"));
            lblUDEditor.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "UpdatedBy"));
            lblUDEditDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy hh:mm tt}', [UpdateTime])"));

            //Bind UserRolesDetail data 
            this.UserRolesDetailReport.DataSource = filteredUserRoles;
            this.UserRolesDetail.SortFields.Add(new GroupField("Username", XRColumnSortOrder.Ascending));
            lblURDUsername.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Username"));
            lblURDRoleName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "CodeProgramRole.RoleName"));
            lblURDProgramKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "ProgramFK"));
            lblURDProgramName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Program.ProgramName"));
            lblURDHubKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Program.HubFK"));
            lblURDHubName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Program.Hub.Name"));
            lblURDStateKey.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Program.StateFK"));
            lblURDStateName.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Program.State.Name"));
            lblURDCreator.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Creator"));
            lblURDCreateDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy hh:mm tt}', [CreateDate])"));
            lblURDEditor.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Editor"));
            lblURDEditDate.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "FormatString('{0:MM/dd/yyyy hh:mm tt}', [EditDate])"));
        }

        public override void PrintingSystem_XlSheetCreated(object sender, DevExpress.XtraPrinting.XlSheetCreatedEventArgs e)
        {
            //To hold the Excel worksheet name
            string sheetName;

            //Set the worksheet name based on the index
            switch (e.Index)
            {
                case 0:
                    sheetName = "Users";
                    break;
                case 1:
                    sheetName = "User Roles";
                    break;
                case 2:
                    sheetName = "Criteria";
                    break;
                default:
                    sheetName = "";
                    break;
            }

            //If the worksheet name has value, set the Excel worksheet name
            if (!string.IsNullOrWhiteSpace(sheetName))
            {
                e.SheetName = sheetName;
            }
        }
    }
}
