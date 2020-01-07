using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Models;
using DevExpress.Web;

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
                ddTwoFactorProviders.DataSource = userFactors.Select(uf => new {
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

                        //Get the user's program roles
                        List<UserProgramRole> userProgramRoles;
                        using (PyramidContext context = new PyramidContext())
                        {
                            userProgramRoles = context.UserProgramRole.Where(upr => upr.Username == user.UserName).ToList();
                        }

                        //Redirect the user based on the number of roles they have
                        if (userProgramRoles.Count > 1)
                        {
                            //Redirect the user to the select role page
                            Response.Redirect(String.Format("/Account/SelectRole.aspx?ReturnUrl={0}&message={1}",
                                                        (Request.QueryString["ReturnUrl"] != null ? Request.QueryString["ReturnUrl"].ToString() : "/Default.aspx"),
                                                        "TwoFactorVerified"));
                        }
                        else
                        {
                            //Get the UserProgramRole
                            UserProgramRole programRole = userProgramRoles.FirstOrDefault();

                            //Set the session variables
                            Session["CodeProgramRoleFK"] = programRole.CodeProgramRole.CodeProgramRolePK;
                            Session["ProgramRoleName"] = programRole.CodeProgramRole.RoleName;
                            Session["ProgramFK"] = programRole.ProgramFK;
                            Session["ProgramName"] = programRole.Program.ProgramName;

                            //Redirect the user
                            Response.Redirect(Request.QueryString["ReturnUrl"] != null ? Request.QueryString["ReturnUrl"].ToString() : "/Default.aspx?message=TwoFactorVerified");
                        }
                        break;
                    case SignInStatus.LockedOut:
                        Response.Redirect("/Account/Lockout");
                        break;
                    case SignInStatus.Failure:
                    default:
                        msgSys.ShowMessageToUser("danger", "Invalid Code", "The code you entered is invalid!", 25000);
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

                //Get the user's verified ID
                string userID = signinManager.GetVerifiedUserId<PyramidUser, string>();

                //Get the user
                var user = manager.FindById(userID);
                if (user != null)
                {
                    //Generate the two factor token
                    var code = manager.GenerateTwoFactorToken(user.Id, ddTwoFactorProviders.Value.ToString());
                }

                //Hide the send code section and show the verify code section
                hfSelectedProvider.Value = ddTwoFactorProviders.Value.ToString();
                divEnterCode.Visible = true;
                divChooseProvider.Visible = false;

                //Tell the user that the code sent
                msgSys.ShowMessageToUser("success", "Code Sent", "Two-Factor code successfully sent!", 5000);
            }
        }
    }
}