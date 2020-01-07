using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Pyramid.Account
{
    public partial class CreatePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Set the focus to the username textbox
                txtUsername.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitCreatePassword user control and it changes the user's password
        /// </summary>
        /// <param name="sender">The submitCreatePassword control</param>
        /// <param name="e">The Click event</param>
        protected void submitCreatePassword_Click(object sender, EventArgs e)
        {
            //Get the change code from the request
            string code = IdentityHelper.GetCodeFromRequest(Request);

            //Only continue if the code exists, otherwise show an error message
            if (code != null)
            {
                //Get the user manager
                var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //Get the user by username
                var user = manager.FindByName(txtUsername.Value.ToString());

                //Only continue if the user exists
                if (user != null)
                {
                    //Change the user's password
                    var result = manager.ResetPassword(user.Id, code, txtPassword.Value.ToString());

                    //If the change succeeded, redirect the user, otherwise show an error message
                    if (result.Succeeded)
                    {
                        Response.Redirect("~/Account/CreatePasswordConfirmation");
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "No user found!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error has occurred!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button and
        /// it returns the user to the login page
        /// </summary>
        /// <param name="sender">The submitCreatePassword control</param>
        /// <param name="e">The Click event</param>
        protected void submitCreatePassword_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the login page
            Response.Redirect("/Account/Login.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCreatePassword control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCreatePassword_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the validation for the txtConfirmPassword DevExpress
        /// Bootstrap TextBox fires and it validates the txtConfirmPassword TextBox
        /// </summary>
        /// <param name="sender">The txtConfirmPassword TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtConfirmPassword_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            string password, confirmPassword;

            //Get the password and confirmation
            password = txtPassword.Value.ToString();
            confirmPassword = txtConfirmPassword.Value.ToString();

            //If the passwords don't match, set validation to false, true otherwise
            if (password != confirmPassword)
            {
                e.IsValid = false;
                e.ErrorText = "Password confirmation does not match!";
            }
            else
            {
                e.IsValid = true;
            }
        }
    }
}