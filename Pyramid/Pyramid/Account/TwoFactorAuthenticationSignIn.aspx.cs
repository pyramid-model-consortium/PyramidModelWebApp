using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Models;
using DevExpress.Web;
using Pyramid.Code;

namespace Pyramid.Account
{
    public partial class TwoFactorAuthenticationSignIn : System.Web.UI.Page
    {
        private ApplicationSignInManager signinManager;
        private ApplicationUserManager manager;

        public TwoFactorAuthenticationSignIn()
        {
            manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            signinManager = Context.GetOwinContext().GetUserManager<ApplicationSignInManager>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                //Get the verified user id
                var userId = signinManager.GetVerifiedUserId<PyramidUser, string>();
                if (userId == null)
                {
                    msgSys.ShowMessageToUser("danger", "User Not Found", "Could not find verified user ID!", 10000);
                    divChooseProvider.Visible = false;
                    divEnterCode.Visible = false;
                }

                //Get the valid two-factor providers and display them
                var userFactors = manager.GetValidTwoFactorProviders(userId);
                ddTwoFactorProviders.DataSource = userFactors.Select(uf => new
                {
                    ProviderName = uf
                })
                .OrderBy(uf => uf.ProviderName)
                .ToList();
                ddTwoFactorProviders.DataBind();

                //Set the focus to the providers dropdown
                ddTwoFactorProviders.Focus();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the verify button in
        /// the verify code section and it attempts to log the user in with
        /// the code they entered
        /// </summary>
        /// <param name="sender">The btnVerifyCode DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnVerifyCode_Click(object sender, EventArgs e)
        {
            //Only continue if the validation is successful
            if (ASPxEdit.AreEditorsValid(this, btnVerifyCode.ValidationGroup))
            {
                //Try to sign the user in
                var result = signinManager.TwoFactorSignIn<PyramidUser, string>(hfSelectedProvider.Value, txtCode.Text, isPersistent: false, rememberBrowser: chkRememberBrowser.Checked);
                switch (result)
                {
                    case SignInStatus.Success:
                        //Get the user ID
                        string userID = signinManager.GetVerifiedUserId<PyramidUser, string>();

                        //Get the user
                        var user = manager.FindById(userID);

                        //The user successfully logged in
                        List<UserProgramRole> userProgramRoles;
                        List<spGetUserCustomizationOptions_Result> userCustomizationOptions;
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the user's program roles
                            userProgramRoles = context.UserProgramRole.AsNoTracking()
                                                .Include(upr => upr.CodeProgramRole)
                                                .Include(upr => upr.Program)
                                                .Where(upr => upr.Username == user.UserName).ToList();

                            //Get the user's customization options
                            userCustomizationOptions = context.spGetUserCustomizationOptions(user.UserName).ToList();

                            //Keep a record of successful logins
                            LoginHistory history = new LoginHistory();
                            history.Username = user.UserName;
                            history.LoginTime = DateTime.Now;

                            //If the user only has one program role, record it in the login history
                            if (userProgramRoles.Count == 1)
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
                            Response.Redirect(String.Format("/Account/SelectRole.aspx?ReturnUrl={0}&message={1}",
                                                        (Request.QueryString["ReturnUrl"] != null ? Request.QueryString["ReturnUrl"].ToString() : "/Default.aspx"),
                                                        "TwoFactorVerified"));
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
                                                    "/Default.aspx?message=TwoFactorVerified");
                        }
                        break;
                    case SignInStatus.LockedOut:
                        Response.Redirect("/Account/Lockout");
                        break;
                    case SignInStatus.Failure:
                    default:
                        msgSys.ShowMessageToUser("danger", "Invalid Code", "The code you entered is invalid or expired!", 25000);
                        break;
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the send code button
        /// </summary>
        /// <param name="sender">The btnSendCode DevEx button</param>
        /// <param name="e">The Click event</param>
        protected void btnSendCode_Click(object sender, EventArgs e)
        {
            //Only continue if the validation is successful
            if (ASPxEdit.AreEditorsValid(this, btnSendCode.ValidationGroup))
            {
                //Try to send the two factor code
                if (!signinManager.SendTwoFactorCode(ddTwoFactorProviders.Value.ToString()))
                {
                    //Show an error message
                    msgSys.ShowMessageToUser("warning", "Transmission Failed", "Unable to send the two-factor code, please try again.", 25000);
                }
                else
                {

                    //Hide the send code section and show the verify code section
                    hfSelectedProvider.Value = ddTwoFactorProviders.Value.ToString();
                    divEnterCode.Visible = true;
                    divChooseProvider.Visible = false;

                    //Tell the user that the code sent
                    msgSys.ShowMessageToUser("success", "Code Sent", "Two-Factor code successfully sent!", 10000);
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the resend code button and it
        /// resends the code to the user
        /// </summary>
        /// <param name="sender">The btnResendCode BootstrapButton</param>
        /// <param name="e">The Click event</param>
        protected void btnResendCode_Click(object sender, EventArgs e)
        {
            //Try to send the two factor code
            if (!signinManager.SendTwoFactorCode(ddTwoFactorProviders.Value.ToString()))
            {
                //Show an error message
                msgSys.ShowMessageToUser("warning", "Transmission Failed", "Unable to send the two-factor code, please try again.", 25000);
            }
            else
            {
                //Show a success message
                msgSys.ShowMessageToUser("success", "Code Sent", "Two-Factor code successfully sent!", 10000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the select other delivery method button and it
        /// allows the user to select a different delivery method
        /// </summary>
        /// <param name="sender">The btnSelectOtherMethod BootstrapButton</param>
        /// <param name="e">The Click event</param>
        protected void btnSelectOtherMethod_Click(object sender, EventArgs e)
        {
            //Hide the enter code section and show the send code section
            hfSelectedProvider.Value = "";
            divEnterCode.Visible = false;
            divChooseProvider.Visible = true;

            //Clear the provider dropdown
            ddTwoFactorProviders.Value = null;
        }
    }
}