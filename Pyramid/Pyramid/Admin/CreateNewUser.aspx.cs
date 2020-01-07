using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DevExpress.Web;
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

            if (!IsPostBack)
            {
                ApplicationDbContext appContext = new ApplicationDbContext();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program list
                    var programs = context.Program.AsNoTracking().Include(p => p.Hub).OrderBy(p => p.ProgramName).Select(p => new {
                        p.ProgramPK,
                        ProgramName = p.ProgramName + " (" + p.Hub.Name + ")"
                    })
                    .ToList();
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
                if(guestRole != null)
                {
                    identityRoles.Remove(guestRole);
                }

                //Only allow super admins to add admin identity roles
                if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                {
                    //Remove the Admin identity role from the list
                    IdentityRole adminRole = identityRoles.Where(ir => ir.Name == "Admin").FirstOrDefault();
                    identityRoles.Remove(adminRole);
                }

                //Bind the identity role dropdown
                ddIdentityRole.DataSource = identityRoles;
                ddIdentityRole.DataBind();

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
            newUser.UserName = txtUsername.Value.ToString();
            newUser.Email = txtEmail.Value.ToString();
            newUser.EmailConfirmed = false;
            newUser.TwoFactorEnabled = false;
            newUser.PhoneNumber = (txtPhoneNumber.Value == null ? null : txtPhoneNumber.Value.ToString());
            newUser.PhoneNumberConfirmed = false;

            //Attempt to create the user
            IdentityResult result = manager.Create(newUser, txtPassword.Value.ToString());

            if (result.Succeeded)
            {
                //If the user creation succeeded, send the user an email to confirm their account
                string emailcode = manager.GenerateEmailConfirmationToken(newUser.Id);
                string callbackUrl = IdentityHelper.GetAccountConfirmationRedirectUrl(emailcode, newUser.Id, Request);
                manager.SendEmail(newUser.Id, "Confirm your account", Utilities.GetEmailHTML(callbackUrl, "Confirm Account", true, "Welcome " + newUser.FirstName + " " + newUser.LastName + "!", "Your user account for the Pyramid Model Implementation Data System was created by an administrator.<br/>Your username for this system is:<br/><br/>" + newUser.UserName + "<br/><br/>Once you confirm your account and create your password, you will be able to start using the system.<br/>To get started, please click the link below.", Request));

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