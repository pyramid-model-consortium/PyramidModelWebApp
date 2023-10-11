using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Pyramid.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Text;

namespace Pyramid
{
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// This method sends an email and supports be run asynchronously
        /// </summary>
        /// <param name="message">The IdentityMessage object that represents the message to send</param>
        /// <returns></returns>
        public async Task SendAsync(IdentityMessage message)
        {
            await SendEmailAsync(message.Destination, message.Subject, message.Body);
        }

        /// <summary>
        /// This method sends an email synchronously
        /// </summary>
        /// <param name="message">The IdentityMessage object that represents the message to send</param>
        public static void Send(IdentityMessage message)
        {
            SendEmail(message.Destination, message.Subject, message.Body);
        }

        /// <summary>
        /// This method sends an email and supports running asynchronously
        /// </summary>
        /// <param name="to">The email address you want to send a message to.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="body">The HTML content that will make up the email body.</param>
        /// <returns></returns>
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            //Get the SendGrid info
            string apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            string smtpClientAddress = ConfigurationManager.AppSettings["SMTPClientAddress"];
            //Get the from email address
            string emailFromAddress = ConfigurationManager.AppSettings["EmailFromAddress"];

            //Set the content of the message
            string messageContent = body;

            //Create the SendGridClient object
            SendGridClient client = new SendGridClient(apiKey);

            //Create a new EmailAddress object for the from email address
            EmailAddress fromEmail = new EmailAddress(emailFromAddress, "Pyramid Model Implementation Data System");

            //Create the SendGrid email message
            SendGridMessage message = MailHelper.CreateSingleEmail(fromEmail, new EmailAddress(to), subject, "", messageContent);

            //Send the message
            await client.SendEmailAsync(message);
        }

        /// <summary>
        /// This method sends an email via SendGrid
        /// </summary>
        /// <param name="to">The email address you want to send a message to.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="body">The HTML content that will make up the email body.</param>
        public static void SendEmail(string to, string subject, string body)
        {
            //Get the SendGrid info
            string apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            string smtpClientAddress = ConfigurationManager.AppSettings["SMTPClientAddress"];
            //Get the from email address
            string emailFromAddress = ConfigurationManager.AppSettings["EmailFromAddress"];

            //Set the content of the message
            string messageContent = body;

            //Create the SendGridClient object
            SendGridClient client = new SendGridClient(apiKey);

            //Create a new EmailAddress object for the from email address
            EmailAddress fromEmail = new EmailAddress(emailFromAddress, "Pyramid Model Implementation Data System");

            //Create the SendGrid email message
            SendGridMessage message = MailHelper.CreateSingleEmail(fromEmail, new EmailAddress(to), subject, "", messageContent);

            //Send the message
            client.SendEmailAsync(message).Wait();
        }


        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }

    public class SmsService : IIdentityMessageService
    {
        /// <summary>
        /// This method sends a SMS message and supports running asynchronously
        /// </summary>
        /// <param name="message">The IdentityMessage to send via SMS</param>
        /// <returns></returns>
        public async Task SendAsync(IdentityMessage message)
        {
            //Get the SMS provider information
            string accountSid = ConfigurationManager.AppSettings["SMSAccountIdentification"];
            string authToken = ConfigurationManager.AppSettings["SMSAccountPassword"];
            string phoneNumber = ConfigurationManager.AppSettings["SMSAccountFrom"];

            //Initialize the Twilio client
            TwilioClient.Init(accountSid, authToken);

            //Create the message and send it
            await MessageResource.CreateAsync(
            new PhoneNumber(message.Destination),
            from: new PhoneNumber(phoneNumber),
            body: message.Body
            );
        }

        /// <summary>
        /// This method sends a SMS message and supports running asynchronously
        /// </summary>
        /// <param name="message">The IdentityMessage to send via SMS</param>
        public void Send(IdentityMessage message)
        {
            //Get the SMS provider information
            string accountSid = ConfigurationManager.AppSettings["SMSAccountIdentification"];
            string authToken = ConfigurationManager.AppSettings["SMSAccountPassword"];
            string phoneNumber = ConfigurationManager.AppSettings["SMSAccountFrom"];

            //Initialize the Twilio client
            TwilioClient.Init(accountSid, authToken);

            //Create the message and send it
            MessageResource.Create(
            new PhoneNumber(message.Destination),
            from: new PhoneNumber(phoneNumber),
            body: message.Body
            );
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<PyramidUser>
    {
        public ApplicationUserManager(IUserStore<PyramidUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<PyramidUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<PyramidUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireNonLetterOrDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.

            //Get the email HTML for the two-factor code
            string emailHTML = Pyramid.Code.Utilities.GetEmailHTML("", "", false, "Security Code", 
                            "Your Pyramid Model Implementation Data System security code is: <br/><br/>{0}<br/>", 
                            "This code will expire in 6 minutes.", System.Web.HttpContext.Current.Request);
            
            //Register the email
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<PyramidUser>
            {
                Subject = "Security Code",
                BodyFormat = emailHTML
            });
            
            //Register the phone
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<PyramidUser>
            {
                MessageFormat = "Your Pyramid Model Implementation Data System security code is: {0}.  This code will expire in 6 minutes."
            });

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<PyramidUser>(dataProtectionProvider.Create("ASP.NET Identity")) {
                    TokenLifespan = TimeSpan.FromDays(7)
                };
            }
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<PyramidUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(PyramidUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
