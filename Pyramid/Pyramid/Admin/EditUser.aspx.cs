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
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace Pyramid.Admin
{
    public partial class EditUser : System.Web.UI.Page
    {
        private ApplicationUserManager manager;
        private PyramidUser currentUser = new PyramidUser();
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow non-admins to use the page
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN &&
                    currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN)
            {
                //Kick out any non-admins
                Response.Redirect("/Default.aspx");
            }

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
                    //Bind the state dropdown list
                    var allStates = context.State.AsNoTracking().Where(s => s.Name != "National" && s.Name != "Example")
                                                                .OrderBy(st => st.Name)
                                                                .ToList();

                    ddState.DataSource = allStates;
                    ddState.DataBind();

                    //Show the user's program roles
                    BindUserProgramRoles(context, currentUser);

                    //Get the programs the user can add roles for
                    List<Models.Program> programs;

                    if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN)
                    {
                        //Only allow national programs for the national role
                        programs = context.Program.AsNoTracking()
                                            .Include(p => p.Hub)
                                            .Where(p => p.StateFK == (int)Utilities.StateFKs.NATIONAL)
                                            .OrderBy(p => p.ProgramName)
                                            .ToList();
                    }
                    else
                    {
                        //Get all allowed programs
                        programs = context.Program.AsNoTracking()
                                            .Include(p => p.Hub)
                                            .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                            .OrderBy(p => p.ProgramName)
                                            .ToList();
                    }

                    //Bind the program dropdown
                    var programDropDownSource = programs.Select(p => new {
                        p.ProgramPK,
                        ProgramName = p.ProgramName + " (" + p.Hub.Name + ")"
                    }).ToList();
                    ddProgram.DataSource = programDropDownSource;
                    ddProgram.DataBind();


                    //Get the program role list, limited to the roles the user is allowed to add
                    List<CodeProgramRole> programRoles = context.CodeProgramRole.AsNoTracking()
                                        .OrderBy(cpr => cpr.RoleName)
                                        .ToList();

                    //Filter the program roles by the user's current role
                    List<CodeProgramRole> filteredProgramRoles = programRoles.Where(pr => pr.RolesAuthorizedToModify.Split(',').ToList().Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString())).ToList();

                    //Bind the program role dropdown
                    ddProgramRole.DataSource = filteredProgramRoles;
                    ddProgramRole.DataBind();
                }

                using (ApplicationDbContext appContext = new ApplicationDbContext())
                {
                    //Get the identity roles (except for guest, which isn't used)
                    var identityRoles = appContext.Roles.AsNoTracking()
                                                    .Where(r => r.Name != "Guest")
                                                    .OrderBy(r => r.Name)
                                                    .ToList();

                    //Only allow super admins and other admins who are editing themselves to see the Admin identity role
                    if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN 
                        || ((currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN ||
                                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN ||
                                currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN)
                                    && User.Identity.Name == currentUser.UserName))
                    {
                        //Do nothing
                    }
                    else
                    {
                        //Remove the Admin identity role
                        IdentityRole adminRole = identityRoles.Where(ir => ir.Name == "Admin").FirstOrDefault();
                        identityRoles.Remove(adminRole);

                        //Hide the password div
                        divChangePassword.Visible = false;
                    }

                    //Bind the identity role dropdown
                    ddIdentityRole.DataSource = identityRoles;
                    ddIdentityRole.DataBind();
                }

                //If the user exists, fill the form
                txtFirstName.Value = currentUser.FirstName;
                txtLastName.Value = currentUser.LastName;
                txtStreet.Value = currentUser.Street;
                ddState.SelectedItem = ddState.Items.FindByValue(currentUser.State);
                txtCity.Value = currentUser.City;
                txtZIPCode.Value = currentUser.ZIPCode;
                txtRegionLocation.Value = currentUser.RegionLocation;
                txtEmail.Value = currentUser.Email;
                txtPhoneNumber.Value = currentUser.PhoneNumber;
                txtWorkPhoneNumber.Value = currentUser.WorkPhoneNumber;
                deLockoutEndDate.Value = (currentUser.LockoutEndDateUtc.HasValue ? currentUser.LockoutEndDateUtc.Value.ToString("MM/dd/yyyy") : "");
                ddAccountEnabled.Value = currentUser.AccountEnabled;
                ddIdentityRole.SelectedItem = ddIdentityRole.Items.FindByValue(currentUser.Roles.FirstOrDefault().RoleId);

                //Use cancel confirmations if the customization option for cancel confirmations is true (default to true)
                bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
                bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
                submitUser.UseCancelConfirm = areConfirmationsEnabled;

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
            if (ASPxEdit.AreEditorsValid(this, btnSavePassword.ValidationGroup)
                && (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN
                    || ((currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN ||
                            currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN ||
                            currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN)
                            && User.Identity.Name == currentUser.UserName)))
            {
                //Change the user's password and get the result
                string resetToken = manager.GeneratePasswordResetToken(currentUser.Id);
                IdentityResult result = manager.ResetPassword(currentUser.Id, resetToken, Convert.ToString(txtPassword.Value));

                //Set the edit info
                currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                currentUser.UpdateTime = DateTime.Now;
                manager.Update(currentUser);

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

                //Get the user's role
                IdentityUserRole currentRole = currentUser.Roles.FirstOrDefault();

                //Update the user's role
                if (currentRole.RoleId != Convert.ToString(ddIdentityRole.Value))
                {
                    //To hold the new old and new role
                    IdentityRole oldRole = null, newRole = null;

                    using (ApplicationDbContext appContext = new ApplicationDbContext())
                    {
                        //Get the old role and new role
                        oldRole = appContext.Roles.AsNoTracking().Where(r => r.Id == currentRole.RoleId).FirstOrDefault();
                        newRole = appContext.Roles.AsNoTracking().Where(r => r.Id == ddIdentityRole.Value.ToString()).FirstOrDefault();
                    }

                    //Change the role
                    manager.RemoveFromRole(currentUser.Id, oldRole.Name);
                    manager.AddToRole(currentUser.Id, newRole.Name);
                }

                //If the email changed, de-confirm the email
                if(currentUser.EmailConfirmed == true && emailChanged == true)
                {
                    currentUser.EmailConfirmed = false;
                }

                //Set the user's information
                currentUser.PhoneNumber = (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text);
                currentUser.WorkPhoneNumber = (string.IsNullOrWhiteSpace(txtWorkPhoneNumber.Text) ? null : txtWorkPhoneNumber.Text);
                currentUser.Email = txtEmail.Value.ToString();
                currentUser.FirstName = txtFirstName.Value.ToString();
                currentUser.LastName = txtLastName.Value.ToString();
                currentUser.Street = (string.IsNullOrWhiteSpace(txtStreet.Text) ? null : txtStreet.Text);
                currentUser.State = (ddState.Value == null ? null : ddState.Value.ToString());
                currentUser.City = (string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text);
                currentUser.ZIPCode = (string.IsNullOrWhiteSpace(txtZIPCode.Text) ? null : txtZIPCode.Text);
                currentUser.RegionLocation = (string.IsNullOrWhiteSpace(txtRegionLocation.Text) ? null : txtRegionLocation.Text);
                currentUser.LockoutEndDateUtc = (String.IsNullOrWhiteSpace(Convert.ToString(deLockoutEndDate.Value)) ? (DateTime?)null : Convert.ToDateTime(deLockoutEndDate.Value));
                currentUser.UpdateTime = DateTime.Now;
                currentUser.UpdatedBy = (string.IsNullOrWhiteSpace(User.Identity.Name) ? "NoLoginName" : User.Identity.Name);
                currentUser.AccountEnabled = Convert.ToBoolean(ddAccountEnabled.Value);

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
                        manager.SendEmail(currentUser.Id, "Confirm your email address change", 
                                Utilities.GetEmailHTML(callbackUrl, "Confirm Email", true, "Email Updated", 
                                "Please confirm your email address change by clicking the Confirm Email link below.",
                                "This link will expire in 7 days.", Request));
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
        /// This method fires when the validation for the txtPhoneNumber DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPhoneNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtPhoneNumber_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtPhoneNumber.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtWorkPhoneNumber DevExpress
        /// BootstrapTextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtPhoneNumber BootstrapTextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtWorkPhoneNumber_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //The phone number is not required
            if (string.IsNullOrWhiteSpace(txtWorkPhoneNumber.Text) == false)
            {
                //The number was entered, validate it
                e.IsValid = Utilities.IsPhoneNumberValid(txtWorkPhoneNumber.Text, "US");
                e.ErrorText = "Must be a valid phone number!";
            }
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
            //To hold the result of the update
            int saveResult = 0;

            using (ApplicationDbContext appContext = new ApplicationDbContext())
            {
                //Get the user
                var founduser = appContext.Users.Where(u => u.Id == user.Id).FirstOrDefault();

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

                //Save the changes
                saveResult = appContext.SaveChanges();
            }

            //Return a bool indicating the save status
            return saveResult > 0;
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

            //Get the label with the PK for deletion
            Label lblUserProgramRolePK = (Label)item.FindControl("lblUserProgramRolePK");

            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(lblUserProgramRolePK.Text) ? (int?)null : Convert.ToInt32(lblUserProgramRolePK.Text));

            //Remove the role if the PK is not null
            if (rowToRemovePK.HasValue)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Only remove the role if the user has another role
                        if (context.UserProgramRole.Where(upr => upr.Username == currentUser.UserName).ToList().Count > 1)
                        {
                            //Get the role to remove
                            UserProgramRole roleToRemove = context.UserProgramRole.Include(upr => upr.CodeProgramRole).Where(upr => upr.UserProgramRolePK == rowToRemovePK).FirstOrDefault();

                            //Get the roles authorized to remove that role
                            List<string> authorizedRoles = roleToRemove.CodeProgramRole.RolesAuthorizedToModify.Split(',').ToList();

                            //Check to see if the user is authorized to remove the role
                            if (authorizedRoles.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()))
                            {
                                //Remove the role
                                context.UserProgramRole.Remove(roleToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.UserProgramRoleChanged
                                        .OrderByDescending(uprc => uprc.UserProgramRoleChangedPK)
                                        .Where(uprc => uprc.UserProgramRolePK == roleToRemove.UserProgramRolePK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Show the changes
                                BindUserProgramRoles(context, currentUser);

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Success", "Successfully removed user's program role!", 10000);
                            }
                            else
                            {
                                //Show an error message
                                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to remove that role!", 120000);
                            }
                        }
                        else
                        {
                            //Show an error message
                            msgSys.ShowMessageToUser("danger", "Error", "You cannot remove a user's only program role!", 120000);
                        }
                    }
                }
                catch (DbUpdateException dbUpdateEx)
                {
                    //Check if it is a foreign key error
                    if (dbUpdateEx.InnerException?.InnerException is SqlException)
                    {
                        //If it is a foreign key error, display a custom message
                        SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                        if (sqlEx.Number == 547)
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "Could not remove the role, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while removing the role!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while removing the role!", 120000);
                    }

                    //Log the error
                    Utilities.LogException(dbUpdateEx);
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
            int? programRoleFK, programFK;

            //Get the program role fk and program fk
            programRoleFK = (ddProgramRole.Value == null ? (int?)null : Convert.ToInt32(ddProgramRole.Value));
            programFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

            if (programRoleFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Role is required!";
            }
            else if(programFK.HasValue == true)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the selected program
                    Program selectedProgram = context.Program.Include(p => p.State).AsNoTracking().Where(p => p.ProgramPK == programFK.Value).FirstOrDefault();

                    //Check to see if this is a national role
                    if (programRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                        programRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER)
                    {

                        //Check to see if the selected program is associated with the national state
                        if (selectedProgram != null && selectedProgram.StateFK != (int)Utilities.StateFKs.NATIONAL)
                        {
                            e.IsValid = false;
                            e.ErrorText = "You can only add a national role if it is associated with a national program!";
                        }
                    }
                    else if(selectedProgram != null && selectedProgram.StateFK == (int)Utilities.StateFKs.NATIONAL)
                    {
                        //Don't allow non-national roles for the national state
                        e.IsValid = false;
                        e.ErrorText = "Only the national roles can be added for national programs!";
                    }
                    else if(selectedProgram.State.UtilizingPIDS == false)
                    {
                        //Don't allow adding users for states that are not using PIDS
                        e.IsValid = false;
                        e.ErrorText = "Invalid! That program is part of a state that is not utilizing PIDS.";
                    }

                    //Only continue if the previous validation succeeded
                    if (e.IsValid)
                    {
                        //The result if false if the role exists in the database for the user, true otherwise
                        if (context.UserProgramRole.Any(upr => upr.Username == currentUser.UserName
                                                                 && upr.ProgramFK == programFK.Value
                                                                 && upr.ProgramRoleCodeFK == programRoleFK.Value))
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
    }
}