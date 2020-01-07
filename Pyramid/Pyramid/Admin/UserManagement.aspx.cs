using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Pyramid.Admin
{
    public partial class UserManagement : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            if (!IsPostBack)
            {
                //Load the user table
                bsGRUsers.DataBind();

                //Check to see if there are any messages
                if (Request.QueryString["message"] != null)
                {
                    //Get the message type
                    string messageCode = Request.QueryString["message"].ToString();
                    string message = null;
                    string messageType = null;

                    // Strip the query string from action
                    Form.Action = ResolveUrl("~/Admin/UserManagement");

                    //Get the message to display
                    switch(messageCode)
                    {
                        case "CreateUserSuccess":
                            message = "User successfully created!";
                            messageType = "success";
                            break;
                        case "EditUserSuccess":
                            message = "User successfully edited!";
                            messageType = "success";
                            break;
                        case "UserNotFound":
                            message = "User could not be found or an error occurred while retrieving the user!";
                            messageType = "danger";
                            break;
                        default:
                            message = null;
                            messageType = null;
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        //Show the message
                        msgSys.ShowMessageToUser(messageType, "Success", message, 15000);
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the user confirms the disabling of a user
        /// </summary>
        /// <param name="sender">The lbConfirmDisable LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbConfirmDisable_Click(object sender, EventArgs e)
        {
            //Disable the user
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var user = context.Users.Where(x => x.Id == hfUserPK.Value).FirstOrDefault();
                user.LockoutEndDateUtc = DateTime.Now.AddYears(100);
                context.SaveChanges();
            }

            //Rebind the user table
            bsGRUsers.DataBind();

            //Show the user a success message
            msgSys.ShowMessageToUser("success", "Success", "User successfully disabled!", 5000);
        }

        /// <summary>
        /// This method fires when the user confirms the enabling of a user
        /// </summary>
        /// <param name="sender">The lbEnableUser LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEnableUser_Click(object sender, EventArgs e)
        {
            //Enable the user
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var user = context.Users.Where(x => x.Id == hfUserPK.Value).FirstOrDefault();
                user.LockoutEndDateUtc = null;
                context.SaveChanges();
            }

            //Rebind the user table
            bsGRUsers.DataBind();

            //Show the user a success message
            msgSys.ShowMessageToUser("success", "Success", "User successfully enabled!", 5000);
        }

        /// <summary>
        /// This method fires when the user sends a confirmation email
        /// </summary>
        /// <param name="sender">The lbSendConfirmEmail LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbSendConfirmEmail_Click(object sender, EventArgs e)
        {
            //The user object
            PyramidUser user = null;

            //Get the user object
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                user = context.Users.Where(u => u.Id == hfUserPK.Value).FirstOrDefault();
            }

            //Make sure the user exists
            if (user != null && user.Id != null)
            {
                //Get the user manager
                var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

                //If the user exists, send the user an email to confirm their account
                string emailcode = manager.GenerateEmailConfirmationToken(user.Id);
                string callbackUrl = IdentityHelper.GetAccountConfirmationRedirectUrl(emailcode, user.Id, Request);
                manager.SendEmail(user.Id, "Confirm your account", Utilities.GetEmailHTML(callbackUrl, "Confirm Account", true, "Welcome " + user.FirstName + " " + user.LastName + "!", "Your user account for the Pyramid Model Implementation Data System was created by an administrator.<br/>Your username for this system is:<br/><br/>" + user.UserName + "<br/><br/>Once you confirm your account and create your password, you will be able to start using the system.<br/>To get started, please click the link below.", Request));

                //Show the user a success message
                msgSys.ShowMessageToUser("success", "Email Sent", "Confirmation email successfully sent!", 5000);
            }
            else
            {
                //Show an error message
                msgSys.ShowMessageToUser("danger", "Error", "The user could not be found!", 10000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the users DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efUserDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efUserDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "Id";

            //Set the source to a LINQ query
            ApplicationDbContext userContext = new ApplicationDbContext();

            //Get the admin role
            IdentityRole adminRole = userContext.Roles.Where(r => r.Name == "Admin").FirstOrDefault();
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                //Get all users 
                e.QueryableSource = userContext.Users
                                        .OrderBy(u => u.FirstName)
                                        .AsNoTracking().Select(u => new {
                                            u.Id,
                                            Name = u.FirstName + " " + u.LastName,
                                            u.UserName,
                                            u.LockoutEnabled,
                                            u.LockoutEndDateUtc,
                                            u.PhoneNumber,
                                            u.PhoneNumberConfirmed,
                                            u.Email,
                                            u.EmailConfirmed,
                                            u.TwoFactorEnabled
                                        });
            }
            else
            {

                //To hold a list of the usernames in this state
                List<string> allStateUserNames;

                //Get all the state usernames
                using (PyramidContext context = new PyramidContext())
                {
                    allStateUserNames = context.UserProgramRole.AsNoTracking()
                                        .Include(upr => upr.Program)
                                        .Where(upr => upr.Program.StateFK == currentProgramRole.StateFK.Value)
                                        .Select(upr => upr.Username).ToList();
                }

                //Get only non-admin users and the current user's record
                e.QueryableSource = userContext.Users.AsNoTracking()
                                        .Where(u => allStateUserNames.Contains(u.UserName)
                                            && (u.UserName == User.Identity.Name || u.Roles.FirstOrDefault().RoleId != adminRole.Id))
                                        .OrderBy(u => u.FirstName)
                                        .Select(u => new {
                                            u.Id,
                                            Name = u.FirstName + " " + u.LastName,
                                            u.UserName,
                                            u.LockoutEnabled,
                                            u.LockoutEndDateUtc,
                                            u.PhoneNumber,
                                            u.PhoneNumberConfirmed,
                                            u.Email,
                                            u.EmailConfirmed,
                                            u.TwoFactorEnabled
                                        });
            }
        }

        /// <summary>
        /// This method fires when each row in the all users DevExpress GridView is data bound
        /// and it shows/hides buttons and sets popover information
        /// </summary>
        /// <param name="sender">The GridViewRow inside bsGRUsers</param>
        /// <param name="e">The ASPxGridViewTableRowEventArgs</param>
        protected void bsGRUsers_HtmlRowCreated(object sender, DevExpress.Web.ASPxGridViewTableRowEventArgs e)
        {
            //Only work on data rows
            if (e.RowType == DevExpress.Web.GridViewRowType.Data)
            {
                LinkButton disableButton, enableButton;
                Label emailLabel, phoneLabel;
                PyramidUser user;

                //Get the user that the row is for
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    user = context.Users.Find(e.GetValue("Id"));
                }

                //Get the necessary controls
                disableButton = (LinkButton)bsGRUsers.FindRowCellTemplateControl(e.VisibleIndex, null, "lbDisableUser");
                enableButton = (LinkButton)bsGRUsers.FindRowCellTemplateControl(e.VisibleIndex, null, "lbEnableUser");
                emailLabel = (Label)bsGRUsers.FindRowCellTemplateControl(e.VisibleIndex, null, "lblEmail");
                phoneLabel = (Label)bsGRUsers.FindRowCellTemplateControl(e.VisibleIndex, null, "lblPhone");

                //Show/hide the enable and disable buttons
                if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.Now)
                {
                    disableButton.Visible = false;
                    enableButton.Visible = true;
                }
                else
                {
                    disableButton.Visible = true;
                    enableButton.Visible = false;
                }

                //Set the CSS class and popover for the email
                if (user.EmailConfirmed)
                {
                    emailLabel.CssClass = "text-success";
                    emailLabel.Attributes.Add("data-content", "Email is confirmed!");
                }
                else
                {
                    emailLabel.CssClass = "text-custom-warning";
                    emailLabel.Attributes.Add("data-content", "Email is not confirmed!");
                }

                //Set the CSS class and popover for the phone
                if (user.PhoneNumberConfirmed)
                {
                    phoneLabel.CssClass = "text-success";
                    phoneLabel.Attributes.Add("data-content", "Phone is confirmed!");
                }
                else
                {
                    phoneLabel.CssClass = "text-custom-warning";
                    phoneLabel.Attributes.Add("data-content", "Phone is not confirmed!");
                }
            }
        }
    }
}