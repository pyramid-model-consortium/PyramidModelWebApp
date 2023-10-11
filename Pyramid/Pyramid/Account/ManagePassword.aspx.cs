using System;
using System.Linq;
using System.Web;
using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Models;

namespace Pyramid.Account
{
    public partial class ManagePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                //Set the focus to the current password field
                txtCurrentPassword.Focus();
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtConfirmNewPassword DevExpress
        /// Bootstrap TextBox fires and it validates the txtConfirmNewPassword TextBox
        /// and the txtNewPassword DevExpress TextBox
        /// </summary>
        /// <param name="sender">The txtConfirmPassword TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtConfirmPassword_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            string password, confirmPassword;

            //Get the password and confirmation
            password = txtNewPassword.Value.ToString();
            confirmPassword = txtConfirmNewPassword.Value.ToString();

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

        /// <summary>
        /// This method fires when the user clicks the Save Changes button
        /// and it changes the user's password
        /// </summary>
        /// <param name="sender">The btnChangePassword DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            //Only change the password if the validation succeeds
            if (ASPxEdit.AreEditorsValid(this, btnChangePassword.ValidationGroup))
            {
                //Get the user manager and sign in manager
                var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //Get the user
                PyramidUser currentUser = manager.FindById(User.Identity.GetUserId());

                if (currentUser != null)
                {
                    //Change the password and get the result
                    IdentityResult result = manager.ChangePassword(currentUser.Id, txtCurrentPassword.Text, txtNewPassword.Text);

                    //Set the edit fields
                    currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                    currentUser.UpdateTime = DateTime.Now;
                    manager.Update(currentUser);

                    if (result.Succeeded)
                    {
                        //Redirect the user back to the Manage page on success
                        Response.Redirect("~/Account/Manage?m=ChangePwdSuccess");
                    }
                    else
                    {
                        //Show the user why the change failed
                        msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
                    }
                }
                else
                {
                    //Show an error message
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the user record!", 120000);
                }
            }
        }
    }
}