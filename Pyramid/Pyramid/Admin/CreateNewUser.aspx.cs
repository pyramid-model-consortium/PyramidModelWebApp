using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DevExpress.Web;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Code;
using Pyramid.Models;

namespace Pyramid.Admin
{
    public partial class CreateNewUser : System.Web.UI.Page
    {
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

            if (!IsPostBack)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Bind the state dropdown
                    var allStates = context.State.AsNoTracking().Where(s => s.Name != "National" && s.Name != "Example")
                                                                .OrderBy(st => st.Name)
                                                                .ToList();

                    ddState.DataSource = allStates;
                    ddState.DataBind();

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
                    var programDropDownSource = programs.Select(p => new
                    {
                        p.ProgramPK,
                        ProgramName = p.ProgramName + " (" + p.Hub.Name + ")"
                    }).ToList();
                    ddProgram.DataSource = programDropDownSource;
                    ddProgram.DataBind();

                    //Show/hide the password section
                    divPasswordSection.Visible = (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN ? true : false);

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

                    //Only allow super admins to add admin identity roles
                    if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
                    {
                        //Remove the Admin identity role from the list
                        IdentityRole adminRole = identityRoles.Where(ir => ir.Name == "Admin").FirstOrDefault();
                        identityRoles.Remove(adminRole);
                    }

                    //Bind the identity role dropdown
                    ddIdentityRole.DataSource = identityRoles;
                    ddIdentityRole.DataBind();
                }

                //Use cancel confirmations if the customization option for cancel confirmations is true (default to true)
                bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
                bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
                submitUser.UseCancelConfirm = areConfirmationsEnabled;

                //Set focus to the username field
                txtUsername.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the save button and 
        /// it attempts to add a new user to the system with the information
        /// provided on the page
        /// </summary>
        /// <param name="sender">The submitUser control</param>
        /// <param name="e">The Click event</param>
        protected void submitUser_Click(object sender, EventArgs e)
        {
            //Get the user manager
            var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            //Create and fill the user object
            PyramidUser newUser = new PyramidUser();
            newUser.FirstName = txtFirstName.Value.ToString();
            newUser.LastName = txtLastName.Value.ToString();
            newUser.Street = (string.IsNullOrWhiteSpace(txtStreet.Text) ? null : txtStreet.Text);
            newUser.State = (ddState.Value == null ? null : ddState.Value.ToString());
            newUser.City = (string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text);
            newUser.ZIPCode = (string.IsNullOrWhiteSpace(txtZIPCode.Text) ? null : txtZIPCode.Text);
            newUser.RegionLocation = (string.IsNullOrWhiteSpace(txtRegionLocation.Text) ? null : txtRegionLocation.Text);
            newUser.UserName = txtUsername.Value.ToString();
            newUser.Email = txtEmail.Value.ToString();
            newUser.EmailConfirmed = false;
            newUser.TwoFactorEnabled = false;
            newUser.WorkPhoneNumber = (string.IsNullOrWhiteSpace(txtWorkPhoneNumber.Text) ? null : txtWorkPhoneNumber.Text);
            newUser.PhoneNumber = (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ? null : txtPhoneNumber.Text);
            newUser.PhoneNumberConfirmed = false;
            newUser.AccountEnabled = true;
            newUser.CreatedBy = User.Identity.Name;
            newUser.CreateTime = DateTime.Now;

            //To hold the result of the user creation
            IdentityResult result;

            //Check to see if the system should create with a password
            if (txtPassword.Value != null)
            {
                //Attempt to create the user with a password
                result = manager.Create(newUser, txtPassword.Value.ToString());
            }
            else
            {
                //Attempt to create the user without a password
                result = manager.Create(newUser);
            }

            //Check to see if the creation succeeded
            if (result.Succeeded)
            {
                //If the user creation succeeded, send the user an email to confirm their account
                string emailcode = manager.GenerateEmailConfirmationToken(newUser.Id);
                string callbackUrl = IdentityHelper.GetAccountConfirmationRedirectUrl(emailcode, newUser.Id, Request);
                manager.SendEmail(newUser.Id, "Confirm your account",
                    Utilities.GetEmailHTML(callbackUrl, "Confirm Account", true,
                        "Welcome " + newUser.FirstName + " " + newUser.LastName + "!",
                        "Your user account for the Pyramid Model Implementation Data System was created by an administrator.<br/><br/>" +
                        "Your username for this system is:<br/><br/>" + newUser.UserName + "<br/><br/>" +
                        "Once you confirm your account and create your password, you will be able to start using the system.<br/><br/>" +
                        "To get started, please click the link below.",
                        "This link will expire in 7 days.", Request));

                //Add the user to their identity role
                manager.AddToRole(newUser.Id, ddIdentityRole.SelectedItem.Text.ToString());

                //Add the user to their program role
                using (PyramidContext context = new PyramidContext())
                {
                    //Create the UserProgramRole object and fill it
                    UserProgramRole userPrgRole = new UserProgramRole();
                    userPrgRole.CreateDate = DateTime.Now;
                    userPrgRole.Creator = User.Identity.Name;
                    userPrgRole.ProgramFK = Convert.ToInt32(ddProgram.Value);
                    userPrgRole.ProgramRoleCodeFK = Convert.ToInt32(ddProgramRole.Value);
                    userPrgRole.Username = newUser.UserName;

                    //Add the UserProgramRole to the database and save
                    context.UserProgramRole.Add(userPrgRole);
                    context.SaveChanges();
                }

                //Redirect the user
                Response.Redirect("/Admin/UserManagement?message=CreateUserSuccess");
            }
            else
            {
                //User creation failed, show the user an error message
                msgSys.ShowMessageToUser("danger", "Error", result.Errors.FirstOrDefault(), 120000);
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
        /// This method fires when the validation for the txtConfirmPassword DevExpress
        /// Bootstrap TextBox fires and it validates the txtConfirmPassword TextBox
        /// </summary>
        /// <param name="sender">The txtConfirmPassword TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtConfirmPassword_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            string password, confirmPassword;

            //Get the password and confirmation
            password = (txtPassword.Value == null ? null : txtPassword.Value.ToString());
            confirmPassword = (txtConfirmPassword.Value == null ? null : txtConfirmPassword.Value.ToString());

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
                else if (selectedProgram != null && selectedProgram.StateFK == (int)Utilities.StateFKs.NATIONAL)
                {
                    //Don't allow non-national roles for the national state
                    e.IsValid = false;
                    e.ErrorText = "Only the national roles can be added for national programs!";
                }
                else if (selectedProgram.State.UtilizingPIDS == false)
                {
                    //Don't allow adding users for states that are not using PIDS
                    e.IsValid = false;
                    e.ErrorText = "Invalid! That program is part of a state that is not utilizing PIDS.";
                }
            }
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
    }
}