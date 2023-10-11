using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System;
using Pyramid.Code;

namespace Pyramid.Models
{
    public partial class UserCustomizationOption
    {
        /// <summary>
        /// This contains the CodeCustomizationOptionType FKs from the database
        /// </summary>
        public enum CustomizationOptionTypeFKs
        {
            FIREWORKS = 1,
            WELCOME_MESSAGE = 2,
            CANCEL_CONFIRMATION = 3
        }

        /// <summary>
        /// This contains static strings for the customization option cookie
        /// </summary>
        public static class CustomizationOptionCookie
        {
            //The customization option cookie static strings
            public const string COOKIE_NAME = "customizationOptions";
            public const string COOKIE_SECTION = "OptionValues";
            public const string FIREWORKS_OPTION = "fireworks";
            public const string WELCOME_MESSAGE_OPTION = "welcome-message";
            public const string CANCEL_CONFIRMATION_OPTION = "cancel-confirmation";
        }

        /// <summary>
        /// This method sets the customization option cookie based on the passed parameter and returns true
        /// if the method succeeds and false otherwise.
        /// </summary>
        /// <param name="userCustomizationOptions">The customization options to put into the cookie</param>
        /// <returns>True if the method succeeds, false otherwise</returns>
        public static bool SetCustomizationOptionCookie(List<spGetUserCustomizationOptions_Result> userCustomizationOptions)
        {
            try
            {
                //Get the customization cookie
                HttpCookie customizationCookie = HttpContext.Current.Request.Cookies[CustomizationOptionCookie.COOKIE_NAME];

                //If the customization cookie is null, set it to a new cookie
                if (customizationCookie == null)
                {
                    customizationCookie = new HttpCookie(CustomizationOptionCookie.COOKIE_NAME);
                }

                //Get the customization options
                StringBuilder customizationOptions = new StringBuilder();
                foreach (spGetUserCustomizationOptions_Result option in userCustomizationOptions)
                {
                    customizationOptions.Append(option.OptionTypeDescription);
                    customizationOptions.Append("|");
                    customizationOptions.Append(option.OptionValue);
                    customizationOptions.Append("|");
                }

                //Set the customization cookie
                customizationCookie[CustomizationOptionCookie.COOKIE_SECTION] = customizationOptions.ToString();
                customizationCookie.Expires = DateTime.Now.AddDays(1);
                customizationCookie.SameSite = SameSiteMode.Lax;
                customizationCookie.Secure = HttpContext.Current.Request.IsSecureConnection;
                HttpContext.Current.Response.Cookies.Set(customizationCookie);

                //Return true
                return true;
            }
            catch (Exception ex)
            {
                //Log the exception
                Code.Utilities.LogException(ex);

                //Return false
                return false;
            }
        }

        /// <summary>
        /// This method returns a value from the customization option cookie for the passed paramater
        /// </summary>
        /// <param name="optionToRetrieve">The customization option to retrieve</param>
        /// <returns>The option value or null if it can't be found</returns>
        public static string GetCustomizationOptionFromCookie(string optionToRetrieve)
        {
            try
            {
                //Get the option values from the customization option cookie
                string optionValues = Utilities.GetCookieSection(CustomizationOptionCookie.COOKIE_NAME, CustomizationOptionCookie.COOKIE_SECTION);

                //Split the options into an array
                string[] optionList = optionValues.ToLower().Split('|');

                //Get the index of the desired option 
                int optionIndex = Array.IndexOf(optionList, optionToRetrieve.ToLower());

                //Get the option value
                string optionValue;
                if (optionIndex != -1)
                {
                    optionValue = (string.IsNullOrWhiteSpace(optionList[optionIndex + 1]) ? null : optionList[optionIndex + 1]);
                }
                else
                {
                    optionValue = null;
                }

                //Return the option value
                return optionValue;
            }
            catch (Exception ex)
            {
                //Log the exception
                Utilities.LogException(ex);

                //Return null
                return null;
            }
        }

        /// <summary>
        /// This method returns a boolean value from the customization option cookie for the passed paramater
        /// </summary>
        /// <param name="optionToRetrieve">The customization option to retrieve</param>
        /// <returns>The option value or null if it can't be found</returns>
        public static bool? GetBooleanCustomizationOptionFromCookie(string optionToRetrieve)
        {
            //The value to return
            bool returnValue;

            try
            {
                //Get the option values from the customization option cookie
                string optionValues = Utilities.GetCookieSection(CustomizationOptionCookie.COOKIE_NAME, CustomizationOptionCookie.COOKIE_SECTION);

                //Split the options into an array
                string[] optionList = optionValues.ToLower().Split('|');

                //Get the index of the desired option 
                int optionIndex = Array.IndexOf(optionList, optionToRetrieve.ToLower());

                //Get the option value
                string optionValue;
                if (optionIndex != -1)
                {
                    optionValue = (string.IsNullOrWhiteSpace(optionList[optionIndex + 1]) ? null : optionList[optionIndex + 1]);
                }
                else
                {
                    optionValue = null;
                }

                //Parse the option into the correct type to return
                if(bool.TryParse(optionValue, out returnValue))
                {
                    return returnValue;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //Log the exception
                Utilities.LogException(ex);

                //Return null
                return null;
            }
        }
    }
}