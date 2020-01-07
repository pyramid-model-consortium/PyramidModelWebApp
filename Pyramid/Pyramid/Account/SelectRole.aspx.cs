using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Pyramid.Code;
using System.Collections.Generic;

namespace Pyramid.Account
{
    public partial class SelectRole : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Get the program roles for the user
                using (PyramidContext context = new PyramidContext())
                {
                    var userProgramRoles = context.UserProgramRole
                    .Include(upr => upr.Program)
                    .Include(upr => upr.CodeProgramRole)
                    .Where(upr => upr.Username == User.Identity.Name).ToList();
                    repeatUserRoles.DataSource = userProgramRoles;
                    repeatUserRoles.DataBind();
                }

                //Check to see if there are any messages
                if (Request.QueryString["message"] != null)
                {
                    //Get the message type
                    string messageType = Request.QueryString["message"].ToString();

                    //Get the message to display
                    switch(messageType)
                    {
                        case "LostSession":
                            msgSys.ShowMessageToUser("warning", "Role Lost", "Your selected role was lost, please choose your role again! <br/> <br/> If this occurs more than occasionally, please contact support via the ticketing system.", 12000);
                            break;
                        case "TwoFactorVerified":
                            msgSys.ShowMessageToUser("success", "Two-Factor Code Verified", "Your Two-Factor code was successfully verified!", 5000);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user selects a role
        /// </summary>
        /// <param name="sender">The lbSelectRole LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbSelectRole_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton deleteButton = (LinkButton)sender;

            //Get the specific repeater item that holds the button
            RepeaterItem item = (RepeaterItem)deleteButton.Parent;

            //Get the hidden fields for this role
            HiddenField hfProgramRoleFK = (HiddenField)item.FindControl("hfProgramRoleFK");
            HiddenField hfProgramRoleName = (HiddenField)item.FindControl("hfProgramRoleName");
            HiddenField hfProgramRoleAllowedToEdit = (HiddenField)item.FindControl("hfProgramRoleAllowedToEdit");
            HiddenField hfProgramFK = (HiddenField)item.FindControl("hfProgramFK");
            HiddenField hfProgramName = (HiddenField)item.FindControl("hfProgramName");

            //To hold the role information
            ProgramAndRoleFromSession roleInfo = new ProgramAndRoleFromSession();

            //Set the session variables for the program roles
            roleInfo.RoleFK = Convert.ToInt32(hfProgramRoleFK.Value);
            roleInfo.RoleName = hfProgramRoleName.Value;
            roleInfo.AllowedToEdit = Convert.ToBoolean(hfProgramRoleAllowedToEdit.Value);
            roleInfo.CurrentProgramFK = Convert.ToInt32(hfProgramFK.Value);
            roleInfo.ProgramName = hfProgramName.Value;

            //Get the hub and state information
            using (PyramidContext context = new PyramidContext())
            {
                Program currentProgram = context.Program
                                            .Include(p => p.Hub)
                                            .Include(p => p.State)
                                            .Where(p => p.ProgramPK == roleInfo.CurrentProgramFK.Value).FirstOrDefault();

                roleInfo.HubFK = currentProgram.HubFK;
                roleInfo.HubName = currentProgram.Hub.Name;
                roleInfo.StateFK = currentProgram.StateFK;
                roleInfo.StateName = currentProgram.State.Name;
                roleInfo.StateLogoFileName = currentProgram.State.LogoFilename;
                roleInfo.StateCatchphrase = currentProgram.State.Catchphrase;
                roleInfo.StateDisclaimer = currentProgram.State.Disclaimer;

                //Set the allowed program fks
                if (roleInfo.RoleFK == (int)Utilities.ProgramRoleFKs.HUB_DATA_VIEWER)
                {
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
                else if (roleInfo.RoleFK == (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN)
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
                else if (roleInfo.RoleFK == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
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

            //Record the role and program in the login history if a record for the login exists
            if (Session["LoginHistoryPK"] != null && !String.IsNullOrWhiteSpace(Session["LoginHistoryPK"].ToString()))
            {
                //Get the login history pk from session
                int historyPK = Convert.ToInt32(Session["LoginHistoryPK"].ToString());

                //Add the record to the database with the logout time
                using (PyramidContext context = new PyramidContext())
                {
                    LoginHistory history = context.LoginHistory.Find(historyPK);
                    history.ProgramFK = Convert.ToInt32(hfProgramFK.Value);
                    history.Role = hfProgramRoleName.Value;
                    context.SaveChanges();
                }
            }

            //Redirect the user after the role selection
            if (String.IsNullOrWhiteSpace(Request.QueryString["ReturnUrl"]))
                Response.Redirect("/Default.aspx");
            else
                IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
        }
    }
}