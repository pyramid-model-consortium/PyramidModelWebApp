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
                    .Include(upr => upr.Program.Hub)
                    .Include(upr => upr.Program.State)
                    .Include(upr => upr.CodeProgramRole)
                    .Where(upr => upr.Username == User.Identity.Name).ToList();

                    //Bind the repeater
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

            //Get the label for the role PK
            Label lblUserProgramRolePK = (Label)item.FindControl("lblUserProgramRolePK");

            //Conver the PK into an int
            int userProgramRolePK = Convert.ToInt32(lblUserProgramRolePK.Text);

            //Get the program role from the database with the PK and username in
            //order to ensure that the user has the role with that PK
            UserProgramRole userRole;
            using(PyramidContext context = new PyramidContext())
            {
                userRole = context.UserProgramRole
                                        .Include(upr => upr.CodeProgramRole)
                                        .Include(upr => upr.Program)
                                        .AsNoTracking()
                                        .Where(upr => upr.UserProgramRolePK == userProgramRolePK 
                                                    && upr.Username == User.Identity.Name).FirstOrDefault();
            }

            //Make sure the program role exists
            if (userRole != null && userRole.UserProgramRolePK > 0)
            {
                //To hold the role information
                ProgramAndRoleFromSession roleInfo = Utilities.GetProgramRoleFromDatabase(userRole);

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
                        history.ProgramFK = userRole.ProgramFK;
                        history.Role = userRole.CodeProgramRole.RoleName;
                        context.SaveChanges();
                    }
                }

                //Remove the confidentiality accepted session object so that it
                //will refresh from the database once the user loads a page
                Session.Remove(Utilities.SessionKey.CONFIDENTIALITY_ACCEPTED);

                //Redirect the user after the role selection
                if (String.IsNullOrWhiteSpace(Request.QueryString["ReturnUrl"]))
                    Response.Redirect("/Default.aspx");
                else
                    IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
            }
            else
            {
                //Show an error message
                msgSys.ShowMessageToUser("danger", "Invalid Role", "Unable to select the role, please try again!", 10000);
            }
        }
    }
}