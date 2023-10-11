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
                        case "DeclinedConfidentiality":
                            msgSys.ShowMessageToUser("warning", "Declined User Agreement", "Until you accept the user agreement, you will not be able to access the system.", 12000);
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
                if (user != null && manager.IsEmailConfirmed(user.Id) && user.AccountEnabled == true)
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
                            UserCustomizationOption.SetCustomizationOptionCookie(userCustomizationOptions);

                            //Redirect the user based on the number of roles they have
                            if (userProgramRoles.Count > 1)
                            {
                                Response.Redirect(String.Format("/Account/SelectRole.aspx?ReturnUrl={0}",
                                                                (Request.QueryString["ReturnUrl"] != null ? Request.QueryString["ReturnUrl"].ToString() : "/Default.aspx")));
                            }
                            else
                            {
                                //Get the UserProgramRole
                                UserProgramRole userRole = userProgramRoles.FirstOrDefault();

                                //Get the role information for the session
                                ProgramAndRoleFromSession roleInfo = Utilities.GetProgramRoleFromDatabase(userRole);

                                //Add the role information to the session
                                Utilities.SetProgramRoleInSession(Session, roleInfo);

                                //Redirect the user to the default page or return URL
                                Response.Redirect(Request.QueryString["ReturnUrl"] != null ? 
                                                        Request.QueryString["ReturnUrl"].ToString() : 
                                                        "/Default.aspx");
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