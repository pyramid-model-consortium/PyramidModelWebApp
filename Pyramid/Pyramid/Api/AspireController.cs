using Newtonsoft.Json.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pyramid.Api
{
    public class AspireController : ApiController
    {
        // POST api/<controller>/<action>
        //Put the ID and Secret in the body
        [HttpPost]
        [ActionName("ImportTrainings")]
        public IHttpActionResult ImportTrainings([FromBody] JObject data)
        {
            //To hold the necessary variables for the method
            JObject responseObject = null;
            bool isAuthorized = false;
            string id = "", secret = "";

            try
            {
                //The start and end dates for the call to ASPIRE (1 year default)
                //NOTE: Always leave the date defaults to prevent potential issues
                DateTime startDate = DateTime.Now.AddMonths(-12);
                DateTime endDate = DateTime.Now;

                //Get the id from the body data
                if (data["id"] != null)
                {
                    id = data["id"].ToString();
                }

                //Get the secret from the body data
                if (data["secret"] != null)
                {
                    secret = data["secret"].ToString();
                }

                //Get the start date from the body data
                if (data["startDate"] != null)
                {
                    //Try to parse the start date.  If it succeeds, the start date will be set by the TryParse.
                    if(!DateTime.TryParse(data["startDate"].ToString(), out startDate))
                    {
                        //Failed to parse, default to 12 months ago
                        startDate = DateTime.Now.AddMonths(-12);
                    }
                }

                //Get the end date from the body data
                if (data["endDate"] != null)
                {
                    //Try to parse the end date.  If it succeeds, the start date will be set by the TryParse.
                    if (!DateTime.TryParse(data["endDate"].ToString(), out endDate))
                    {
                        //Failed to parse, default to today
                        endDate = DateTime.Now;
                    }
                }

                //Get the real ID and secret
                string actualID = ConfigurationManager.AppSettings["PIDSAPIID"];
                string actualSecret = ConfigurationManager.AppSettings["PIDSAPISecret"];

                //Check to see if the real ID and secret
                if (id == actualID && secret == actualSecret)
                {
                    //Set the authorized bool
                    isAuthorized = true;

                    //To hold the client's IP
                    string ipAddress = "NA";

                    //Try to get the client's IP
                    if (Request.Properties.ContainsKey("MS_HttpContext"))
                    {
                        ipAddress = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                    }
                    else if (HttpContext.Current != null)
                    {
                        ipAddress = HttpContext.Current.Request.UserHostAddress;
                    }

                    //Get trainings from ASPIRE
                    Aspire.AspireAPI.FunctionCalls.GetTrainings(string.Format("External API: {0}", ipAddress), null, null, null, null, startDate, endDate, null, null);

                    //Create the JSON response
                    responseObject =
                        new JObject(
                            new JProperty("status", "Success!"),
                            new JProperty("startDate", startDate.ToString("MM/dd/yyyy hh:mm:ss tt")),
                            new JProperty("endDate", endDate.ToString("MM/dd/yyyy hh:mm:ss tt")));
                }
                else
                {
                    //The user is not authorized
                    isAuthorized = false;
                }
            }
            catch (Exception ex)
            {
                //Get the admin's email address
                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                //Send an email to the admin about the issue
                Utilities.SendEmail(adminEmailAddress, "PIDS API Error", "Something went wrong with the ImportTrainings call in the AspireController of the PIDS API.  Please check the error log for details.", "Login", "/Account/Login.aspx", true);

                //Log the real exception
                Code.Utilities.LogException(ex);

                //Throw a HTTP response exception with code 500
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            //Check to see if the user is authorized
            if (isAuthorized)
            {
                //Authorized, check if the response is valid
                if (responseObject != null)
                {
                    //Valid, return the JSON response
                    return Json(responseObject);
                }
                else
                {
                    //Get the admin's email address
                    string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                    //Send an email to the admin about the issue
                    Utilities.SendEmail(adminEmailAddress, "PIDS API Error", "Something went wrong with the ImportTrainings call in the AspireController of the PIDS API.  Please check the error log for details.", "Login", "/Account/Login.aspx", true);

                    //Invalid, create an exception
                    HttpResponseException ex = new HttpResponseException(HttpStatusCode.InternalServerError);

                    //Log the real exception
                    Code.Utilities.LogException(ex);

                    //Throw the exception
                    throw ex;
                }
            }
            else
            {
                //Get the admin's email address
                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                //Send an email to the admin about the issue
                Utilities.SendEmail(adminEmailAddress, "PIDS API Error", "Something went wrong with the ImportTrainings call in the AspireController of the PIDS API.  Please check the error log for details.", "Login", "/Account/Login.aspx", true);

                //Unauthorized, create an exception (need to use a 500 error, using a 401 causes the API to return the login page because of how webforms works)
                HttpResponseException ex = new HttpResponseException(HttpStatusCode.InternalServerError);

                //Log the real exception
                Code.Utilities.LogException(ex);

                //Throw the exception 
                throw ex;
            }
        }

        // POST api/<controller>/<action>
        //Put the ID and Secret in the body
        [HttpPost]
        [ActionName("ImportReliabilityRecords")]
        public IHttpActionResult ImportReliabilityRecords([FromBody] JObject data)
        {
            //To hold the necessary variables for the method
            JObject responseObject = null;
            bool isAuthorized = false;
            string id = "", secret = "";

            try
            {
                //The start and end dates for the call to ASPIRE (1 year default)
                //NOTE: Always leave the date defaults to prevent potential issues
                DateTime startDate = DateTime.Now.AddMonths(-12);
                DateTime endDate = DateTime.Now;

                //Get the id from the body data
                if (data["id"] != null)
                {
                    id = data["id"].ToString();
                }

                //Get the secret from the body data
                if (data["secret"] != null)
                {
                    secret = data["secret"].ToString();
                }

                //Get the start date from the body data
                if (data["startDate"] != null)
                {
                    //Try to parse the start date.  If it succeeds, the start date will be set by the TryParse.
                    if (!DateTime.TryParse(data["startDate"].ToString(), out startDate))
                    {
                        //Failed to parse, default to 12 months ago
                        startDate = DateTime.Now.AddMonths(-12);
                    }
                }

                //Get the end date from the body data
                if (data["endDate"] != null)
                {
                    //Try to parse the end date.  If it succeeds, the start date will be set by the TryParse.
                    if (!DateTime.TryParse(data["endDate"].ToString(), out endDate))
                    {
                        //Failed to parse, default to today
                        endDate = DateTime.Now;
                    }
                }

                //Get the real ID and secret
                string actualID = ConfigurationManager.AppSettings["PIDSAPIID"];
                string actualSecret = ConfigurationManager.AppSettings["PIDSAPISecret"];

                //Check to see if the real ID and secret
                if (id == actualID && secret == actualSecret)
                {
                    //Set the authorized bool
                    isAuthorized = true;

                    //To hold the client's IP
                    string ipAddress = "NA";

                    //Try to get the client's IP
                    if (Request.Properties.ContainsKey("MS_HttpContext"))
                    {
                        ipAddress = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                    }
                    else if (HttpContext.Current != null)
                    {
                        ipAddress = HttpContext.Current.Request.UserHostAddress;
                    }

                    //Get reliability records from ASPIRE
                    Aspire.AspireAPI.FunctionCalls.GetReliabilityRecords(string.Format("External API: {0}", ipAddress), null, startDate, endDate, null, null);

                    //Create the JSON response
                    responseObject =
                        new JObject(
                            new JProperty("status", "Success!"),
                            new JProperty("startDate", startDate.ToString("MM/dd/yyyy hh:mm:ss tt")),
                            new JProperty("endDate", endDate.ToString("MM/dd/yyyy hh:mm:ss tt")));
                }
                else
                {
                    //The user is not authorized
                    isAuthorized = false;
                }
            }
            catch (Exception ex)
            {
                //Get the admin's email address
                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                //Send an email to the admin about the issue
                Utilities.SendEmail(adminEmailAddress, "PIDS API Error", "Something went wrong with the ImportReliabilityRecords call in the AspireController of the PIDS API.  Please check the error log for details.", "Login", "/Account/Login.aspx", true);

                //Log the real exception
                Code.Utilities.LogException(ex);

                //Throw a HTTP response exception with code 500
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            //Check to see if the user is authorized
            if (isAuthorized)
            {
                //Authorized, check if the response is valid
                if (responseObject != null)
                {
                    //Valid, return the JSON response
                    return Json(responseObject);
                }
                else
                {
                    //Get the admin's email address
                    string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                    //Send an email to the admin about the issue
                    Utilities.SendEmail(adminEmailAddress, "PIDS API Error", "Something went wrong with the ImportReliabilityRecords call in the AspireController of the PIDS API.  Please check the error log for details.", "Login", "/Account/Login.aspx", true);

                    //Invalid, create an exception
                    HttpResponseException ex = new HttpResponseException(HttpStatusCode.InternalServerError);

                    //Log the real exception
                    Code.Utilities.LogException(ex);

                    //Throw the exception
                    throw ex;
                }
            }
            else
            {
                //Get the admin's email address
                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];

                //Send an email to the admin about the issue
                Utilities.SendEmail(adminEmailAddress, "PIDS API Error", "Something went wrong with the ImportReliabilityRecords call in the AspireController of the PIDS API.  Please check the error log for details.", "Login", "/Account/Login.aspx", true);

                //Unauthorized, create an exception (need to use a 500 error, using a 401 causes the API to return the login page because of how webforms works)
                HttpResponseException ex = new HttpResponseException(HttpStatusCode.InternalServerError);

                //Log the real exception
                Code.Utilities.LogException(ex);

                //Throw the exception 
                throw ex;
            }
        }
    }
}