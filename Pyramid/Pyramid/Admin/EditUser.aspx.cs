using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Pyramid.Code;
using Microsoft.AspNet.Identity.EntityFramework;
using DevExpress.Web;

namespace Pyramid.Admin
{
    public partial class EditUser : System.Web.UI.Page
    {
        private ApplicationDbContext appContext = new ApplicationDbContext();
        private ApplicationUserManager manager;
        private PyramidUser currentUser = new PyramidUser();
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the user manager
            manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            //Get the user id from the query string
            string id = Request.QueryString["Id"];

            //Get the user object
            currentUser = manager.FindById(id);

            //Make sure the user exists
            if(currentUser == null)
            {
                Response.Redirect("/Admin/UserManagement.aspx?message=UserNotFound");
            }

            if (!IsPostBack)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Show the user's program roles
                    BindUserProgramRoles(context, currentUser);

                    //Get the program list
                    var programs = context.Program.AsNoTracking().Include(p => p.Hub).OrderBy(p => p.ProgramName).Select(p => new {
                        p.ProgramPK,
                        ProgramName = p.ProgramName + " (" + p.Hub.Name + ")"
                    }).ToList();
                    ddProgram.DataSource = programs;
                    ddProgram.DataBind();

                    //Get the program role list, limited to the roles the user is allowed to add
                    var programRoles = context.CodeProgramRole.AsNoTracking()
                                        .Where(cpr => cpr.RolesAuthorizedToModify.Contains((currentProgramRole.RoleFK.Value.ToString() + ",")))
                                        .OrderBy(cpr => cpr.RoleName)
                                        .ToList();
                    ddProgramRole.DataSource = programRoles;
                    ddProgramRole.DataBind();
                }

                //Get the identity roles
                var identityRoles = appContext.Roles.OrderBy(r => r.Name).ToList();

                //Remove the guest role because it is not implemented in any way
                IdentityRole guestRole = identityRoles.Where(ir => ir.Name == "Guest").FirstOrDefault();
                if (guestRole != null)
                {
                    identityRoles.Remove(guestRole);
                }

                //Only allow super admins and application admins who are editing themselves to see the Admin identity role
                if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN || (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.APPLICATION_ADMIN && User.Identity.Name == currentUser.UserName))
                {
                    //Do not remove the Admin identity role
                }
                else
                {
                    //Remove the Admin identity role
                    IdentityRole adminRole = identityRoles.Where(ir => ir.Name == "Admin").FirstOrDefault();
                    identityRoles.Remove(adminRole);
                }
                //Bind the identity role dropdown
                ddIdentityRole.DataSource = identityRoles;
                ddIdentityRole.DataBind();

                //If the user exists, fill the form
                txtFirstName.Value = currentUser.FirstName;
                txtLastName.Value = currentUser.LastName;
                txtEmail.Value = currentUser.Email;
                txtPhoneNumber.Value = currentUser.PhoneNumber;
                deLockoutEndDate.Value = (currentUser.LockoutEndDateUtc.HasValue ? currentUser.LockoutEndDateUtc.Value.ToString("MM/dd/yyyy") : "");
                ddIdentityRole.SelectedItem = ddIdentityRole.Items.FindByValue(currentUser.Roles.FirstOrDefault().RoleId);

                //Set focus to the first name field
                txtFirstName.Focus();
            }
        }

        /// <summary>
        /// This method gets the UserProgramRoles rows for the user and
        /// it binds those results to the proper Repeater
        /// </summary>
        private void BindUserProgramRoles(PyramidContext currentContext, PyramidUser currentUser)
        {
            //Get the ProgramRoles for the user
            var userProgramRoles = currentContext.UserProgramRole
                .Include(upr => upr.Program)
                .Include(upr => upr.CodeProgramRole)
                .Where(upr => upr.Username == currentUser.UserName)
                .OrderBy(upr => upr.Program.ProgramName)
                .ToList();
            repeatUserRoles.DataSource = userProgramRoles;
            repeatUserRoles.DataBind();
        }

        /// <summary>
        /// When the user clicks the Save button in the Change Password modal, set the user's password
        /// </summary>
        /// <param name="sender">The btnSavePassword DevExpress Button</param>
        /// <param name="e">The Click event</param>
        protected void btnSavePassword_Click(object sender, EventArgs e)
        {
            //Only continue if the validation group is valid
            if (ASPxEdit.AreEditorsValid(this, btnSavePassword.ValidationGroup))
            {
                //Change the user's password and get the result
                string resetToken = manager.GeneratePasswordResetToken(currentUser.Id);
                IdentityResult result = manager.ResetPassword(currentUser.Id, resetToken, Convert.ToString(txtPassword.Value));

                //Show the user a message depending on if the password change succeeded or not
                if (result.Succeeded)
                {
                    msgSys.ShowMessageToUser("success", "Success", "Password successfully updated!", 10000);
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
                }
            }
        }

        /// <summary>
        /// When the user clicks the Save button, save the changes to the user
        /// </summary>
        /// <param name="sender">The submitUser control</param>
        /// <param name="e">The Click event</param>
        protected void submitUser_Click(object sender, EventArgs e)
        {
            //Make sure the current user is correct
            currentUser = manager.FindById(currentUser.Id);

            //Whether or not the email changed
            bool emailChanged = false;

            //Only continue if the page is valid
            if (ASPxEdit.AreEditorsValid(this, submitUser.ValidationGroup))
            {
                //Check to see if the user's email changed
                if (currentUser.Email != Convert.ToString(txtEmail.Value))
                {
                    //The email changed
                    emailChanged = true;
                }

                //Only de-confirm the user's phone if it changed
                if (txtPhoneNumber.Value == null || (currentUser.PhoneNumber != txtPhoneNumber.Value.ToString()))
                {
                    currentUser.PhoneNumberConfirmed = false;
                }

                //Update the user's role
                if (currentUser.Roles.FirstOrDefault().RoleId != Convert.ToString(ddIdentityRole.Value))
                {
                    //Get the old role and new role
                    string oldRole = appContext.Roles.Find(currentUser.Roles.FirstOrDefault().RoleId).Name;
                    string newRole = appContext.Roles.Find(Convert.ToString(ddIdentityRole.Value)).Name;

                    //Change the role
                    manager.RemoveFromRole(currentUser.Id, oldRole);
                    manager.AddToRole(currentUser.Id, newRole);
                }

                //Set the user's information
                currentUser.EmailConfirmed = !emailChanged;
                currentUser.PhoneNumber = (txtPhoneNumber.Value == null ? null : txtPhoneNumber.Value.ToString());
                currentUser.Email = txtEmail.Value.ToString();
                currentUser.FirstName = txtFirstName.Value.ToString();
                currentUser.LastName = txtLastName.Value.ToString();
                currentUser.LockoutEndDateUtc = (String.IsNullOrWhiteSpace(Convert.ToString(deLockoutEndDate.Value)) ? (DateTime?)null : Convert.ToDateTime(deLockoutEndDate.Value));
                currentUser.UpdateTime = DateTime.Now;

                //Update the user in the database
                IdentityResult result = manager.Update(currentUser);

                if (result.Succeeded)
                {
                    //Send an email if the email changed
                    if(emailChanged)
                    {
                        //Generate the confirmation token and url
                        string code = manager.GenerateEmailConfirmationToken(currentUser.Id);
                        string callbackUrl = IdentityHelper.GetEmailConfirmationRedirectUrl(code, currentUser.Id, Request);

                        //Send the confirmation email to the user via email
                        manager.SendEmail(currentUser.Id, "Confirm your email address change", Utilities.GetEmailHTML(callbackUrl, "Confirm Email", true, "Email Updated", "Please confirm your email address change by clicking the Confirm Email link below.", Request));
                    }

                    //Redirect the user to the user management page
                    Response.Redirect("/Admin/UserManagement.aspx?message=EditUserSuccess");
                }
                else
                {
                    //Show the user an error message
                    msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button and
        /// it returns the user to the user management page
        /// </summary>
        /// <param name="sender">The submitUser control</param>
        /// <param name="e">The Click event</param>
        protected void submitUser_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the management page
            Response.Redirect("/Admin/UserManagement.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submitUser validation group
        /// </summary>
        /// <param name="sender">The submitUser control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitUser_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method accepts a user object and updates the user in the database
        /// </summary>
        /// <param name="user">A PyramidUser object</param>
        /// <returns>True if the edit succeeded, false otherwise</returns>
        public bool UpdateUser(PyramidUser user)
        {
            //Get the user
            var founduser = appContext.Users.Where(x => x.Id == user.Id).AsQueryable().FirstOrDefault();

            if (founduser == null)
            {
                //If the user does not exist, add it
                appContext.Users.Add(user);
            }
            else
            {
                //Update the user's values
                appContext.Entry(founduser).CurrentValues.SetValues(user);
            }

            //Return a bool that indicates if the save succeeded
            return appContext.SaveChanges() > 0;
        }

        /// <summary>
        /// This method fires when the user clicks the Remove Role button and
        /// it removes the Program Role from the user
        /// </summary>
        /// <param name="sender">The lbDeleteRole LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteRole_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton deleteButton = (LinkButton)sender;

            //Get the specific repeater item
            RepeaterItem item = (RepeaterItem)deleteButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfUserProgramRolePK = (HiddenField)item.FindControl("hfUserProgramRolePK");

            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfUserProgramRolePK.Value) ? (int?)null : Convert.ToInt32(hfUserProgramRolePK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Only remove the role if the user has another role
                    if(context.UserProgramRole.Where(upr => upr.Username == currentUser.UserName).ToList().Count > 1)
                    {
                        //Get the role to remove
                        UserProgramRole roleToRemove = context.UserProgramRole.Where(upr => upr.UserProgramRolePK == rowToRemovePK).FirstOrDefault();
                        context.UserProgramRole.Remove(roleToRemove);
                        context.SaveChanges();

                        //Show the changes
                        BindUserProgramRoles(context, currentUser);

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully removed user's program role!", 10000);
                    }
                    else
                    {
                        //Show an error message
                        msgSys.ShowMessageToUser("danger", "Error", "You cannot remove a user's only program role!", 120000);
                    }
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to remove the role!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Add Role button and
        /// it adds the selected Program Role to the user
        /// </summary>
        /// <param name="sender">The btnAddRole DevExpress Bootstrap button</param>
        /// <param name="e">The Click event</param>
        protected void btnAddRole_Click(object sender, EventArgs e)
        {
            //Only continue if the page is valid
            if (ASPxEdit.AreEditorsValid(this, btnAddRole.ValidationGroup))
            {
                int programRoleFK, programFK;

                //Get the program role fk and program fk
                programRoleFK = Convert.ToInt32(ddProgramRole.Value);
                programFK = Convert.ToInt32(ddProgram.Value);

                //Create the object and fill it
                UserProgramRole newUpr = new UserProgramRole();
                newUpr.Creator = User.Identity.Name;
                newUpr.CreateDate = DateTime.Now;
                newUpr.ProgramFK = programFK;
                newUpr.Username = currentUser.UserName;
                newUpr.ProgramRoleCodeFK = programRoleFK;

                //Add the object to the database
                using (PyramidContext context = new PyramidContext())
                {
                    //Add the role
                    context.UserProgramRole.Add(newUpr);
                    context.SaveChanges();

                    //Show the changes
                    BindUserProgramRoles(context, currentUser);
                }

                //Clear the inputs
                ddProgramRole.Value = "";
                ddProgram.Value = "";

                //Show a success message
                msgSys.ShowMessageToUser("success", "Success", "Successfully added program role to user!", 10000);
            }
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

        /// <summary>
        /// This method fires when the ddProgramRole validation fires and it
        /// ensures that Program Roles are not duplicated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddProgramRole_Validation(object sender, ValidationEventArgs e)
        {
            int programRoleFK, programFK;

            //Get the program role fk and program fk
            programRoleFK = Convert.ToInt32(ddProgramRole.Value);
            programFK = Convert.ToInt32(ddProgram.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //The result if false if the role exists in the database for the user, true otherwise
                if (context.UserProgramRole.Any(upr => upr.Username == currentUser.UserName
                                                         && upr.ProgramFK == programFK
                                                         && upr.ProgramRoleCodeFK == programRoleFK))
                {
                    e.IsValid = false;
                    e.ErrorText = "Program Role already exists!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
        }
    }
}