using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Pyramid.Models
{
    public class AspNetUserChange
    {
        [Required]
        [Key]
        public int AspNetUserChangePK { get; set; }
        [Required]
        public DateTime ChangeDatetime { get; set; }
        [Required]
        [MaxLength(100)]
        public string ChangeType { get; set; }
        [Required]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(512)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(512)]
        public string LastName { get; set; }
        public Nullable<DateTime> UpdateTime { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        [Required]
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        [MaxLength(40)]
        public string WorkPhoneNumber { get; set; }
        [Required]
        public bool PhoneNumberConfirmed { get; set; }
        [Required]
        public bool TwoFactorEnabled { get; set; }
        public Nullable<DateTime> LockoutEndDateUtc { get; set; }
        [Required]
        public bool LockoutEnabled { get; set; }
        [Required]
        public int AccessFailedCount { get; set; }
        [Required]
        [MaxLength(256)]
        public string UserName { get; set; }
        [Required]
        public bool AccountEnabled { get; set; }
        [Required]
        [MaxLength(256)]
        public string CreatedBy { get; set; }
        [Required]
        public DateTime CreateTime { get; set; }
        [MaxLength(256)]
        public string UpdatedBy { get; set; }
        [MaxLength(50)]
        public string ZIPCode { get; set; }

        [MaxLength(100)]
        public string City { get;  set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(256)]
        public string RegionLocation { get; set; }

        [MaxLength(300)]
        public string Street { get; set; }
    }

    // You can add User data for the user by adding more properties to your User class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public partial class PyramidUser : IdentityUser
    {
        [Required]
        [MaxLength(512)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(512)]
        public string LastName { get; set; }
        [Required]
        public bool AccountEnabled { get; set; }
        [MaxLength(40)]
        public string WorkPhoneNumber { get; set; }
        [Required]
        [MaxLength(256)]
        public string CreatedBy { get; set; }
        [Required]
        public DateTime CreateTime { get; set; }
        [MaxLength(256)]
        public string UpdatedBy { get; set; }
        public Nullable<DateTime> UpdateTime { get; set; }
        [MaxLength(50)]
        public string ZIPCode { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(50)]
        public string State { get; set; }
        [MaxLength(256)]
        public string RegionLocation { get; set; }

        [MaxLength(300)]
        public string Street { get; set; }


        public ClaimsIdentity GenerateUserIdentity(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = manager.CreateIdentity(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            return Task.FromResult(GenerateUserIdentity(manager));
        }
    }

    public class ApplicationDbContext : IdentityDbContext<PyramidUser>
    {
        public ApplicationDbContext()
            : base("PyramidIdentity", throwIfV1Schema: false)
        {
        }

        public DbSet<AspNetUserChange> UserChanges { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}

#region Helpers
namespace Pyramid
{
    public static class IdentityHelper
    {
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";

        public const string ProviderNameKey = "providerName";
        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request.QueryString[ProviderNameKey];
        }

        public const string CodeKey = "code";
        public static string GetCodeFromRequest(HttpRequest request)
        {
            return request.QueryString[CodeKey];
        }

        public const string UserIdKey = "userId";
        public static string GetUserIdFromRequest(HttpRequest request)
        {
            return HttpUtility.UrlDecode(request.QueryString[UserIdKey]);
        }

        public const string ConfirmModeKey = "confirmMode";
        public static string GetConfirmModeFromRequest(HttpRequest request)
        {
            return request.QueryString[ConfirmModeKey];
        }

        public static string GetCreatePasswordRedirectUrl(string code, HttpRequest request)
        {
            var absoluteUri = "/Account/CreatePassword?" + CodeKey + "=" + HttpUtility.UrlEncode(code);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        public static string GetResetPasswordRedirectUrl(string code, HttpRequest request)
        {
            var absoluteUri = "/Account/ResetPassword?" + CodeKey + "=" + HttpUtility.UrlEncode(code);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        public static string GetAccountConfirmationRedirectUrl(string code, string userId, HttpRequest request)
        {
            var absoluteUri = "/Account/Confirm?" + CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(userId) + "&" + ConfirmModeKey + "=AccountConfirm";
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        public static string GetEmailConfirmationRedirectUrl(string code, string userId, HttpRequest request)
        {
            var absoluteUri = "/Account/Confirm?" + CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(userId) + "&" + ConfirmModeKey + "=EmailConfirm";
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                response.Redirect(returnUrl);
            }
            else
            {
                response.Redirect("~/");
            }
        }
    }
}
#endregion
