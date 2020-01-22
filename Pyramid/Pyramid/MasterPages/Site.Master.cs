using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Pyramid.Code;
using Pyramid.Models;
using System.Linq;

namespace Pyramid
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Set the hidden field values for the customization cookie
                hfCustomizationOptionCookieName.Value = Utilities.CustomizationOptionCookieName;
                hfCustomizationOptionCookieSection.Value = Utilities.CustomizationOptionCookieSection;

                try
                {
                    //Get the customization cookie
                    HttpCookie customizationCookie = Request.Cookies[Utilities.CustomizationOptionCookieName];

                    //If the customization cookie is null, refill from the database
                    if (customizationCookie == null)
                    {
                        //Get the user's customization options from the database
                        List<spGetUserCustomizationOptions_Result> userCustomizationOptions = new List<spGetUserCustomizationOptions_Result>();
                        using (PyramidContext databaseContext = new PyramidContext())
                        {
                            //Get the user's customization options
                            userCustomizationOptions = databaseContext.spGetUserCustomizationOptions(Context.User.Identity.Name).ToList();
                        }

                        //Set the customization cookie
                        Utilities.SetCustomizationOptionCookie(userCustomizationOptions);
                    }
                }
                catch (Exception ex)
                {
                    //Log the exception
                    Utilities.LogException(ex);
                }
            }
        }

        /// <summary>
        /// This method fires when an exception inside of an update panel occurs
        /// and it logs that exception
        /// </summary>
        /// <param name="sender">The AllScriptManager ScriptManager</param>
        /// <param name="e">The error event arguments</param>
        protected void AllScriptManager_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
        {
            //Log the error
            Utilities.LogException(e.Exception);
        }
    }

}