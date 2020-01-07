using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using Pyramid.Models;
using Pyramid.Code;
using DevExpress.Web;
using System.Text;

namespace Pyramid.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Hide the master page title and footer
            NotLoggedIn masterPage = (NotLoggedIn)this.Master;
            masterPage.HideTitle();
            masterPage.HideFooter();

            if (!IsPostBack)
            {
                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "LogOutSuccess":
                            msgSys.ShowMessageToUser("primary", "Logged Out", "You have successfully logged out of PIDS!", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Set the focus to the username text box
                txtUsername.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Login button and it attempts to log
        /// the user in
        /// </summary>
        /// <param name="sender">The btnLogin DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, btnLogin.ValidationGroup))
            {
                // Validate the user password
                var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var signinManager = Context.GetOwinContext().GetUserManager<ApplicationSignInManager>();

                //Try to get the user
                PyramidUser user = manager.FindByName(txtUsername.Text);

                //Make sure that the user is confirmed
                if (user != null && manager.IsEmailConfirmed(user.Id))
                {
                    //Try to sign the user in
                    var result = signinManager.PasswordSignIn(txtUsername.Text, txtPassword.Text, false, user.LockoutEnabled);

                    switch (result)
                    {
                        case SignInStatus.Success:
                            //The user successfully logged in

                            List<UserProgramRole> userProgramRoles;
                            List<spGetUserCustomizationOptions_Result> userCustomizationOptions;
                            using (PyramidContext context = new PyramidContext())
                            {
                                //Get the user's program roles
                                userProgramRoles = context.UserProgramRole.AsNoTracking()
                                                    .Include(upr => upr.CodeProgramRole)
                                                    .Include(upr => upr.Program)
                                                    .Where(upr => upr.Username == txtUsername.Text).ToList();

                                //Get the user's customization options
                                userCustomizationOptions = context.spGetUserCustomizationOptions(txtUsername.Text).ToList();

                                //Keep a record of successful logins
                                LoginHistory history = new LoginHistory();
                                history.Username = txtUsername.Text;
                                history.LoginTime = DateTime.Now;

                                //If the user only has one program role, record it in the login history
                                if(userProgramRoles.Count == 1)
                                {
                                    history.ProgramFK = userProgramRoles.First().ProgramFK;
                                    history.Role = userProgramRoles.First().CodeProgramRole.RoleName;
                                }

                                //Save the login history
                                context.LoginHistory.Add(history);
                                context.SaveChanges();

                                //Save the LoginHistory primary key to the session for later access
                                Session["LoginHistoryPK"] = history.LoginHistoryPK;
                            }

                            //Set the user customization options cookie
                            Utilities.SetCustomizationOptionCookie(userCustomizationOptions);

                            //Redirect the user based on the number of roles they have
                            if (userProgramRoles.Count > 1)
                            {
                                Response.Redirect(String.Format("/Account/SelectRole.aspx?ReturnUrl={0}",
                                                                (Request.QueryString["ReturnUrl"] != null ? Request.QueryString["ReturnUrl"].ToString() : "/Default.aspx")));
                            }
                            else
                            {
                                //To hold the role information
                                ProgramAndRoleFromSession roleInfo = new ProgramAndRoleFromSession();

                                //Get the UserProgramRole
                                UserProgramRole userRole = userProgramRoles.FirstOrDefault();

                                //Set the session variables for the program roles
                                roleInfo.RoleFK = userRole.CodeProgramRole.CodeProgramRolePK;
                                roleInfo.RoleName = userRole.CodeProgramRole.RoleName;
                                roleInfo.AllowedToEdit = userRole.CodeProgramRole.AllowedToEdit;
                                roleInfo.CurrentProgramFK = userRole.ProgramFK;
                                roleInfo.ProgramName = userRole.Program.ProgramName;

                                //Get the hub and state information
                                using(PyramidContext context = new PyramidContext())
                                {
                                    Program currentProgram = context.Program.AsNoTracking()
                                                                .Include(p => p.Hub)
                                                                .Include(p => p.State)
                                                                .Include(p => p.ProgramType)
                                                                .Where(p => p.ProgramPK == userRole.ProgramFK).FirstOrDefault();

                                    roleInfo.HubFK = currentProgram.HubFK;
                                    roleInfo.HubName = currentProgram.Hub.Name;
                                    roleInfo.StateFK = currentProgram.StateFK;
                                    roleInfo.StateName = currentProgram.State.Name;
                                    roleInfo.StateLogoFileName = currentProgram.State.LogoFilename;
                                    roleInfo.StateCatchphrase = currentProgram.State.Catchphrase;
                                    roleInfo.StateDisclaimer = currentProgram.State.Disclaimer;

                                    //Set the allowed program fks
                                    if (roleInfo.RoleFK == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER) {
                                        //Hub viewer, allow them to see the programs in that hub
                                        var hubPrograms = context.Program.AsNoTracking()
                                                                    .Where(p => p.HubFK == roleInfo.HubFK.Value)
                                                                    .ToList();
                                        roleInfo.ProgramFKs = hubPrograms
                                                                .Select(hp => hp.ProgramPK)
                                                                .ToList();

                                        //Allow them to see all cohorts in their hub
                                        roleInfo.CohortFKs = hubPrograms
                                                                    .Select(hp => hp.CohortFK)
                                                                    .Distinct()
                                                                    .ToList();

                                        //Don't restrict their view of the BOQs
                                        roleInfo.ShowBOQ = true;
                                        roleInfo.ShowBOQFCC = true;
                                    }
                                    else if(roleInfo.RoleFK == (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN)
                                    {
                                        //App admin, allow them to see all programs in a state
                                        roleInfo.ProgramFKs = context.Program.AsNoTracking()
                                                                    .Where(p => p.StateFK == roleInfo.StateFK.Value)
                                                                    .Select(p => p.ProgramPK).ToList();

                                        //Allow them to see all cohorts in a state
                                        roleInfo.CohortFKs = context.Cohort.AsNoTracking()
                                                                    .Where(c => c.StateFK == roleInfo.StateFK.Value)
                                                                    .Select(c => c.CohortPK).ToList();

                                        //Don't restrict their view of the BOQs
                                        roleInfo.ShowBOQ = true;
                                        roleInfo.ShowBOQFCC = true;
                                    }
                                    else if(roleInfo.RoleFK == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                                    {
                                        //Super admin, all programs in all states
                                        roleInfo.ProgramFKs = context.Program.AsNoTracking()
                                                                    .Select(p => p.ProgramPK).ToList();

                                        //All cohorts
                                        roleInfo.CohortFKs = context.Cohort.AsNoTracking()
                                                                    .Select(c => c.CohortPK).ToList();

                                        //Don't restrict their view of the BOQs
                                        roleInfo.ShowBOQ = true;
                                        roleInfo.ShowBOQFCC = true;
                                    }
                                    else
                                    {
                                        //Something else, limit to the current program fk
                                        List<int> programFKs = new List<int>();
                                        programFKs.Add(roleInfo.CurrentProgramFK.Value);
                                        roleInfo.ProgramFKs = programFKs;

                                        //Limit to current cohort fk
                                        List<int> cohortFKs = new List<int>();
                                        cohortFKs.Add(currentProgram.CohortFK);
                                        roleInfo.CohortFKs = cohortFKs;

                                        //Determine if this program is a FCC program
                                        var fccProgramTypes = currentProgram.ProgramType
                                            .Where(pt => pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.FAMILY_CHILD_CARE 
                                                    || pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE)
                                                    .ToList();

                                        //Limit their view to the right BOQ type
                                        if (fccProgramTypes.Count > 0)
                                        {
                                            roleInfo.ShowBOQ = false;
                                            roleInfo.ShowBOQFCC = true;
                                        }
                                        else
                                        {
                                            roleInfo.ShowBOQ = true;
                                            roleInfo.ShowBOQFCC = false;
                                        }
                                    }
                                }

                                //Add the role information to the session
                                Utilities.SetProgramRoleInSession(Session, roleInfo);

                                //Redirect the user
                                Response.Redirect(Request.QueryString["ReturnUrl"] != null ? Request.QueryString["ReturnUrl"].ToString() : "/Default.aspx");
                            }
                            break;
                        case SignInStatus.LockedOut:
                            Response.Redirect("/Account/Lockout");
                            break;
                        case SignInStatus.RequiresVerification:
                            Response.Redirect(String.Format("/Account/TwoFactorAuthenticationSignIn?ReturnUrl={0}",
                                                            Request.QueryString["ReturnUrl"]), true);
                            break;
                        case SignInStatus.Failure:
                        default:
                            //Show the user an error message
                            msgSys.ShowMessageToUser("danger", "Error", "Invalid login attempt", 120000);

                            //Focus the password text box
                            txtPassword.Focus();
                            break;
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Invalid login attempt", 120000);
                }
            }
        }
    }
}