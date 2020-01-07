using System;
using System.Linq;
using System.Web;
using Pyramid.Models;
using Microsoft.AspNet.Identity;
using System.Web.SessionState;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using OfficeOpenXml;
using DevExpress.Web;

namespace Pyramid.Code
{
    public sealed class Utilities
    {
        #region Roles/Identity

        /// <summary>
        /// This contains the Program Role FKs from the database
        /// </summary>
        public enum ProgramRoleFKs
        {
            DATA_COLLECTOR = 1,
            AGGREGATE_DATA_VIEWER = 2,
            DETAIL_DATA_VIEWER = 3,
            HUB_DATA_VIEWER = 4,
            APPLICATION_ADMIN = 5,
            SUPER_ADMIN = 6
        }

        /// <summary>
        /// This method returns a user's Identity role from the database
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="userManager">The ApplicationUserManager</param>
        /// <returns>The identity role name if it can, null if it fails</returns>
        public static string GetIdentityRoleByUsername(string username, ApplicationUserManager userManager)
        {
            string returnVal;

            try
            {
                //Get the user
                PyramidUser user = userManager.FindByName(username);

                //Get the identity role (our system only allows for a user to have one)
                returnVal = userManager.GetRoles(user.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                //If an error occurred, log it and return null
                LogException(ex);
                returnVal = null;
            }

            return returnVal;
        }

        /// <summary>
        /// This method returns a user's program role from the session
        /// </summary>
        /// <param name="session">The current session state</param>
        /// <returns>The program role name if it exists, null otherwise</returns>
        public static ProgramAndRoleFromSession GetProgramRoleFromSession(HttpSessionState session)
        {
            ProgramAndRoleFromSession returnObj = new ProgramAndRoleFromSession();

            try
            {
                //Try to get the program role name and program from session
                returnObj.RoleFK = (session["CodeProgramRoleFK"] != null ? Convert.ToInt32(session["CodeProgramRoleFK"].ToString()) : (int?)null);
                returnObj.RoleName = (session["ProgramRoleName"] != null ? session["ProgramRoleName"].ToString() : null);
                returnObj.AllowedToEdit = (session["ProgramRoleAllowedToEdit"] != null ? Convert.ToBoolean(session["ProgramRoleAllowedToEdit"].ToString()) : (bool?)null);
                returnObj.CurrentProgramFK = (session["CurrentProgramFK"] != null ? Convert.ToInt32(session["CurrentProgramFK"].ToString()) : (int?)null);
                returnObj.ProgramFKs = (session["ProgramFKs"] != null ? (List<int>)session["ProgramFKs"] : null);
                returnObj.ProgramName = (session["ProgramName"] != null ? session["ProgramName"].ToString() : null);
                returnObj.ShowBOQ = (session["BOQOnly"] != null ? Convert.ToBoolean(session["BOQOnly"]) : (bool?)null);
                returnObj.ShowBOQFCC = (session["BOQFCCOnly"] != null ? Convert.ToBoolean(session["BOQFCCOnly"]) : (bool?)null);
                returnObj.HubFK = (session["HubFK"] != null ? Convert.ToInt32(session["HubFK"].ToString()) : (int?)null);
                returnObj.HubName = (session["HubName"] != null ? session["HubName"].ToString() : null);
                returnObj.StateFK = (session["StateFK"] != null ? Convert.ToInt32(session["StateFK"].ToString()) : (int?)null);
                returnObj.StateName = (session["StateName"] != null ? session["StateName"].ToString() : null);
                returnObj.StateLogoFileName = (session["StateLogoFileName"] != null ? session["StateLogoFileName"].ToString() : null);
                returnObj.StateCatchphrase = (session["StateCatchphrase"] != null ? session["StateCatchphrase"].ToString() : null);
                returnObj.StateDisclaimer = (session["StateDisclaimer"] != null ? session["StateDisclaimer"].ToString() : null);
                returnObj.CohortFKs = (session["CohortFKs"] != null ? (List<int>)session["CohortFKs"] : null);
            }
            catch (Exception ex)
            {
                //It failed, log the exception
                LogException(ex);
            }

            //If the role from session does not have values, redirect the user
            if (!returnObj.RoleFK.HasValue || returnObj.RoleName == null || !returnObj.AllowedToEdit.HasValue
                || !returnObj.CurrentProgramFK.HasValue || returnObj.ProgramName == null || !returnObj.HubFK.HasValue
                || !returnObj.StateFK.HasValue)
            {
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    //If the user is logged in, redirect them to the SelectRole page.
                    HttpContext.Current.Response.Redirect(String.Format("/Account/SelectRole.aspx?ReturnUrl={0}&message={1}", HttpContext.Current.Request.Url.PathAndQuery, "LostSession"));
                }
                else
                {
                    //If the user is not logged in, redirect them to the login page
                    HttpContext.Current.Response.Redirect(String.Format("/Account/Login.aspx?ReturnUrl={0}", HttpContext.Current.Request.Url.PathAndQuery));
                }
            }

            //Return the program role
            return returnObj;
        }

        /// <summary>
        /// This method adds a user's selected program role information to the session
        /// </summary>
        /// <param name="session">The current session state</param>
        /// <param name="programAndRole">The program, role, hub and state information to store in session</param>
        /// <returns>The program role name if it exists, null otherwise</returns>
        public static void SetProgramRoleInSession(HttpSessionState session, ProgramAndRoleFromSession programAndRole)
        {
            try
            {
                session["CodeProgramRoleFK"] = programAndRole.RoleFK;
                session["ProgramRoleName"] = programAndRole.RoleName;
                session["ProgramRoleAllowedToEdit"] = programAndRole.AllowedToEdit;
                session["CurrentProgramFK"] = programAndRole.CurrentProgramFK;
                session["ProgramFKs"] = programAndRole.ProgramFKs;
                session["ProgramName"] = programAndRole.ProgramName;
                session["BOQOnly"] = programAndRole.ShowBOQ;
                session["BOQFCCOnly"] = programAndRole.ShowBOQFCC;
                session["HubFK"] = programAndRole.HubFK;
                session["HubName"] = programAndRole.HubName;
                session["StateFK"] = programAndRole.StateFK;
                session["StateName"] = programAndRole.StateName;
                session["StateLogoFileName"] = programAndRole.StateLogoFileName;
                session["StateCatchphrase"] = programAndRole.StateCatchphrase;
                session["StateDisclaimer"] = programAndRole.StateDisclaimer;
                session["CohortFKs"] = programAndRole.CohortFKs;
            }
            catch (Exception ex)
            {
                //It failed, log the exception
                LogException(ex);
            }
        }
        #endregion

        #region EmailTemplates

        /// <summary>
        /// This method returns the Email HTML for an email using the passed parameters and a template
        /// </summary>
        /// <param name="buttonURL">The URL for the button</param>
        /// <param name="buttonText">The button's text</param>
        /// <param name="buttonVisible">A bool that indicates whether or not to show the button</param>
        /// <param name="bodyTitle">The title of the email body</param>
        /// <param name="bodyText">The main email text</param>
        /// <param name="request">The HttpRequest object</param>
        /// <returns>A string that contains the constructed email HTML</returns>
        public static string GetEmailHTML(string buttonURL, string buttonText, bool buttonVisible, string bodyTitle, string bodyText, HttpRequest request)
        {
            /* ---------------------------------NOTE---------------------------------
             * The images will not work when running locally because the email clients require that
             * all images use HTTPS, and won't render tags that use just HTTP.  Localhost only runs
             * using HTTP.
             */

            //Get the url of the generic logo
            UriBuilder urlToGenericLogo = new UriBuilder();
            urlToGenericLogo.Scheme = request.Url.Scheme;
            urlToGenericLogo.Host = request.Url.Host;
            urlToGenericLogo.Path = "Content/images/GenericLogo.png";
            urlToGenericLogo.Port = request.Url.Port;

            //Get the string url
            string strGenericLogoURL = urlToGenericLogo.Uri.AbsoluteUri;

            //Create the HTML for the email
            //============
            //ADD YOUR EMAIL HTML TO THE STRING BELOW
            //============
            string htmlToReturn = "";

            //Return the html in different formats based on whether or not the body text needs string.format
            if(bodyText.Contains("{") || bodyText.Contains("}"))
            {
                return htmlToReturn;
            }
            else
            {
                return htmlToReturn.Replace("{{", "{").Replace("}}", "}");
            }
        }

        #endregion

        #region Errors
        /// <summary>
        /// This method accepts an Exception object and logs that exception
        /// in the ELMAH table in the database through ELMAH's methods
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogException(Exception ex)
        {
            try
            {
                //Log the error via ELMAH
                Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Elmah.Error(ex));
            }
            catch(Exception)
            {
                //Do Nothing
            }
        }
        #endregion

        #region Azure BLOB Storage

        /// <summary>
        /// This class holds constant names for the azure storage containers
        /// </summary>
        public sealed class ConstantAzureStorageContainerName
        {
            private readonly string containerName;
            private readonly string containerValue;

            public static readonly ConstantAzureStorageContainerName UPLOADED_FILES = 
                new ConstantAzureStorageContainerName("UPLOADED_FILES", "uploadedfiles");
            
            public static readonly ConstantAzureStorageContainerName REPORT_DOCUMENTATION =
                new ConstantAzureStorageContainerName("REPORT_DOCUMENTATION", "reportdocs");

            private ConstantAzureStorageContainerName(string name, string value)
            {
                containerName = name;
                containerValue = value;
            }

            public override string ToString()
            {
                return containerValue;
            }
        }

        /// <summary>
		/// This method returns a URL that points to the specified file on Azure storage.
		/// </summary>
		/// <param name="fileName">The name of the file</param>
		/// <param name="isPDF">Whether or not this file is a PDF</param>
		/// <param name="containerName">The name of the container on Azure storage</param>
		/// <returns></returns>
		public static string GetFileLinkFromAzureStorage(string fileName, bool isPDF, string containerName)
        {
            //Connect to Azure storage and get a reference to the file
            CloudBlockBlob blob = GetBlobFromAzureStorage(fileName, containerName);

            //Set a policy for read-only access that expires in one minute
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            policy.Permissions = SharedAccessBlobPermissions.Read;
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(1);

            //Only allow access to this file
            SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders();
            if (isPDF)
                headers.ContentDisposition = string.Format("fileName=\"{0}\"", fileName);
            else
                headers.ContentDisposition = string.Format("attachment;fileName=\"{0}\"", fileName);

            //Get the SAS token
            string sasToken = blob.GetSharedAccessSignature(policy, headers);

            //Return the URL and SAS token so the user can view/download the file
            return blob.Uri.AbsoluteUri + sasToken;
        }

        /// <summary>
        /// This method creates a container in the azure storage account
        /// </summary>
        /// <param name="containerName">The name of the container to create</param>
        /// <returns>True if the container was successfully created, false otherwise</returns>
        public static bool CreateContainerOnAzureStorage(string containerName)
        {
            //Connect to Azure storage and get a reference to the file
            CloudStorageAccount account = CloudStorageAccount.Parse(WebConfigurationManager.ConnectionStrings["PyramidStorage"].ConnectionString);
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(containerName);

            //Check to see if the container already exists
            if(!cloudBlobContainer.Exists())
            {
                //If the container doesn't exist, create it
                cloudBlobContainer.Create();

                //Get a reference to the created container
                CloudBlobContainer createdContainer = blobClient.GetContainerReference(containerName);

                //Return a bool that tells if the container exists
                return createdContainer.Exists();
            }
            else
            {
                //The container exists, return true
                return true;
            }
        }

        /// <summary>
        /// This method uploads a specified file to Azure storage.
        /// </summary>
        /// <param name="file">The HttpPostedFile object for the file</param>
        /// <param name="fileName">The file name</param>
        /// <param name="containerName">The container on Azure storage where the file will be uploaded</param>
        /// <returns>The path to the file on Azure storage</returns>
        public static string UploadFileToAzureStorage(HttpPostedFile file, string fileName, string containerName)
        {
            if (!String.IsNullOrWhiteSpace(containerName))
            {
                //Connect to Azure storage and create a reference to the file
                CloudBlockBlob blob = GetBlobFromAzureStorage(fileName, containerName);

                //Upload the file
                using (Stream fileStream = file.InputStream)
                {
                    blob.UploadFromStream(fileStream);
                }

                //Set the content type of the file
                if (fileName.Contains(".pdf"))
                {
                    blob.Properties.ContentType = "application/pdf";
                    blob.SetProperties();
                }

                //Return the URL for the file
                return HttpUtility.UrlDecode(blob.Uri.AbsoluteUri);
            }
            else
            {
                throw new NullReferenceException("Cannot find container for specified file type!");
            }
        }

        /// <summary>
        /// This method uploads a specified file to Azure storage.
        /// </summary>
        /// <param name="fileByteArray">The byte array for the file</param>
        /// <param name="fileName">The file name</param>
        /// <param name="containerName">The container on Azure storage where the file will be uploaded</param>
        /// <returns>The path to the file on Azure storage</returns>
        public static string UploadFileToAzureStorage(byte[] fileByteArray, string fileName, string containerName)
        {
            if (!String.IsNullOrWhiteSpace(containerName))
            {
                //Connect to Azure storage and create a reference to the file
                CloudBlockBlob blob = GetBlobFromAzureStorage(fileName, containerName);

                //Upload the file
                using (MemoryStream stream = new MemoryStream(fileByteArray, writable: false))
                {
                    blob.UploadFromStream(stream);
                }

                //Set the content type of the file
                if (fileName.Contains(".pdf"))
                {
                    blob.Properties.ContentType = "application/pdf";
                    blob.SetProperties();
                }

                //Return the URL for the file
                return HttpUtility.UrlDecode(blob.Uri.AbsoluteUri);
            }
            else
            {
                throw new NullReferenceException("Cannot find container for specified file type!");
            }
        }

        /// <summary>
        /// This method deletes a file from Azure storage.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted</param>
        /// <param name="containerName">The name of the container on Azure storage where the file is stored</param>
        public static void DeleteFileFromAzureStorage(string fileName, string containerName)
        {
            //Connect to Azure storage and get a reference to the file
            CloudBlockBlob blob = GetBlobFromAzureStorage(fileName, containerName);

            //Delete the file if it exists
            blob.DeleteIfExists();
        }

        /// <summary>
        /// This method retrieves a blob from the storage account
        /// </summary>
        /// <param name="fileName">The file to retrieve</param>
        /// <param name="containerName">The container that holds the file</param>
        /// <returns>The blob from the storage account</returns>
        public static CloudBlockBlob GetBlobFromAzureStorage(string fileName, string containerName)
        {
            //Connect to Azure storage and get a reference to the file
            CloudStorageAccount account = CloudStorageAccount.Parse(WebConfigurationManager.ConnectionStrings["PyramidStorage"].ConnectionString);
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(fileName);

            return blob;
        }
        #endregion

        #region Cookies

        //The customization option cookie name and section
        public const string CustomizationOptionCookieName = "customizationOptions";
        public const string CustomizationOptionCookieSection = "OptionValues";

        /// <summary>
        /// This method retrieves a section of values from a cookie
        /// </summary>
        /// <param name="cookieName">The cookie to retrieve the section from</param>
        /// <param name="cookieSection">The section name</param>
        /// <returns>A string with the values in that section</returns>
        public static string GetCookieSection(string cookieName, string cookieSection)
        {
            try
            {
                //Get the user customization options cookie
                HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieName];

                //Make sure the cookie exists
                if (cookie != null)
                {
                    //Get the section values
                    string sectionValues = Convert.ToString(cookie[cookieSection]);

                    //Return the section values
                    return sectionValues;
                }
                else
                {
                    //Couldn't find the cookie, return null
                    return null;
                }
            }
            catch (Exception ex)
            {
                //Log the exception
                LogException(ex);

                //Return null
                return null;
            }
        }

        /// <summary>
        /// This method sets a cookie's section value
        /// </summary>
        /// <param name="cookieName">The cookie's name</param>
        /// <param name="cookieSection">The cookie's section</param>
        /// <param name="cookieSectionValue">The cookie's section value</param>
        /// <returns>True if the method succeeds, false otherwise</returns>
        public static bool SetCookieSection(string cookieName, string cookieSection, string cookieSectionValue, int expirationDays)
        {
            try
            {
                //Get the user customization options cookie
                HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieName];

                //If the cookie is null, set it to a new cookie
                if (cookie == null)
                {
                    cookie = new HttpCookie(cookieName);
                }

                //Set the section value
                cookie[cookieSection] = cookieSectionValue;

                //Set the cookie expiration
                cookie.Expires = DateTime.Now.AddDays(expirationDays);

                //Add the cookie to the response
                HttpContext.Current.Response.Cookies.Set(cookie);

                //Return true
                return true;
            }
            catch (Exception ex)
            {
                //Log the exception
                LogException(ex);

                //Return false
                return false;
            }
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
                HttpCookie customizationCookie = HttpContext.Current.Request.Cookies["customizationOptions"];

                //If the customization cookie is null, set it to a new cookie
                if (customizationCookie == null)
                {
                    customizationCookie = new HttpCookie("customizationOptions");
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
                customizationCookie[CustomizationOptionCookieSection] = customizationOptions.ToString();
                customizationCookie.Expires = DateTime.Now.AddDays(1);
                HttpContext.Current.Response.Cookies.Set(customizationCookie);

                //Return true
                return true;
            }
            catch (Exception ex)
            {
                //Log the exception
                Utilities.LogException(ex);

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
            try {
                //Get the option values from the customization option cookie
                string optionValues = GetCookieSection(CustomizationOptionCookieName, CustomizationOptionCookieSection);

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
                LogException(ex);

                //Return null
                return null;
            }
        }

        #endregion

        #region MISC

        /// <summary>
        /// This method returns the application title based on the passed program role
        /// </summary>
        /// <param name="programRole">The program role</param>
        /// <returns>The application title string</returns>
        public static string GetApplicationTitle(ProgramAndRoleFromSession programRole)
        {
            //Get the application title
            StringBuilder applicationTitle = new StringBuilder();

            //Check to see if the program role has values
            if (programRole.CurrentProgramFK.HasValue)
            {
                //Get the app title values from the program role
                applicationTitle.Append("<span class='app-state-name'>" + programRole.StateName + " State</span>");
                applicationTitle.Append("<span class='app-name'>Pyramid Model Implementation Data System</span>");
                applicationTitle.Append((string.IsNullOrWhiteSpace(programRole.StateCatchphrase) ? "" : "<span class='app-state-catchphrase'>" + programRole.StateCatchphrase + "</span>"));
            }
            else
            {
                //Set to the default app title
                applicationTitle.Append("<span class='app-name'>Pyramid Model Implementation Data System</span>");
            }

            //Return the title
            return applicationTitle.ToString();
        }

        /// <summary>
        /// This function returns a person's age in days
        /// </summary>
        /// <param name="comparisonDate">The date to compare to the DOB</param>
        /// <param name="DOB">The person's DOB</param>
        /// <returns>The person's age in days</returns>
        public static int CalculateAgeDays(DateTime comparisonDate, DateTime DOB)
        {
            return comparisonDate.Subtract(DOB).Days;
        }

        /// <summary>
        /// This function returns the score type (Above Cutoff, Monitor, Typical, or Error)
        /// based on the passed values
        /// </summary>
        /// <param name="totalScore">The ASQSE total score</param>
        /// <param name="scoreASQSE">The scoreASQSE object</param>
        /// <returns>A string with one of the following values: (Above Cutoff, Monitor, Typical, or Error)</returns>
        public static string GetASQSEScoreType(int totalScore, ScoreASQSE scoreASQSE)
        {
            string scoreType = "";

            //Check to see how the score relates to the cutoff score
            if (totalScore > scoreASQSE.CutoffScore)
            {
                scoreType = "Above Cutoff";
            }
            else if (totalScore >= scoreASQSE.MonitoringScoreStart && totalScore <= scoreASQSE.MonitoringScoreEnd)
            {
                scoreType = "Monitor";
            }
            else if (totalScore >= 0 && totalScore < scoreASQSE.MonitoringScoreStart)
            {
                scoreType = "Well Below";
            }
            else
            {
                scoreType = "Error!";
            }

            //Return the score type
            return scoreType;
        }

        /// <summary>
        /// This function checks to see if this is the test site or a local site
        /// </summary>
        /// <returns>True if this is the test or local site, false otherwise</returns>
        public static bool IsTestSite()
        {
            //Get the lowercase connection string
            string connectionString = ConfigurationManager.ConnectionStrings["Pyramid"].ConnectionString.ToLower();

            //Check to see if the connection string contains the word test or localhost
            if (connectionString.Contains("test"))
            {
                //This is the test site, return true
                return true;
            }
            else if (connectionString.Contains("localhost"))
            {
                //This is a local site, return true
                return true;
            }
            else
            {
                //This is not the test site or a local site, return false
                return false;
            }
        }

        /// <summary>
        /// This method generates the NCPMI Excel File Report and fills it with values from the database.
        /// </summary>
        /// <param name="programFKs">The program FKs to filter the report</param>
        /// <param name="schoolYear">The school year DateTime to filter the report</param>
        /// <returns>A byte array representation of the Excel file</returns>
        public static byte[] GenerateNCPMIExcelFile(List<int> programFKs, DateTime schoolYear)
        {
            try
            {
                //To hold the necessary database values
                List<rspBIRExcel_ProgramInfo_Result> allProgramInfo;
                List<rspBIRExcel_ChildrenAndBIRs_Result> allBIRAndChildInfo;
                List<string> programNames;

                //Get the necessary values from the database
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program names
                    programNames = context.Program.AsNoTracking()
                                        .Where(p => programFKs.Contains(p.ProgramPK))
                                        .Select(p => p.ProgramName)
                                        .ToList();

                    //Get the BIR and Child info for the Excel file
                    allBIRAndChildInfo = context.rspBIRExcel_ChildrenAndBIRs(string.Join(",", programFKs),
                                            schoolYear).ToList();

                    //Get the program info for the Excel file
                    allProgramInfo = context.rspBIRExcel_ProgramInfo(string.Join(",", programFKs), schoolYear).ToList();
                }

                //Get the file info for the master Excel file
                string excelFilePath = HttpContext.Current.Server.MapPath("~/Reports/PreBuiltReports/ExcelReports/BIR_Report.xlsm");
                FileInfo fileInfo = new FileInfo(excelFilePath);

                //Only continue if the master Excel file exists
                if (fileInfo.Exists)
                {
                    using (ExcelPackage excel = new ExcelPackage(fileInfo))
                    {
                        //Make sure that the XLSM file works properly
                        if (excel.Workbook.VbaProject == null)
                        {
                            excel.Workbook.CreateVBAProject();
                        }

                        //Get the necessary worksheets from the excel file
                        var programInfoWorksheet = excel.Workbook.Worksheets.Where(w => w.Name == "Program Information").FirstOrDefault();
                        var childWorksheet = excel.Workbook.Worksheets.Where(w => w.Name == "Child Enrollment").FirstOrDefault();
                        var BIRWorksheet = excel.Workbook.Worksheets.Where(w => w.Name == "BIR Data Entry").FirstOrDefault();

                        //------------------- PROGRAM INFO -----------------------------

                        //Set the basic program info and school year chosen
                        programInfoWorksheet.Cells[1, 2].Value = string.Join(", ", programNames);
                        programInfoWorksheet.Cells[2, 2].Value = schoolYear.ToString("yyyy");

                        //To hold the program info aggregates
                        int[] classroomNumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] ethnicityNumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] race1NumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] race2NumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] race3NumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] race4NumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] race5NumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] race6NumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] genderNumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] IEPNumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int[] DLLNumRow = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                        //Calculate the aggregates by looping through the rows returned from the database
                        foreach (rspBIRExcel_ProgramInfo_Result programInfoRow in allProgramInfo)
                        {
                            //Get the classroom attendance numbers
                            classroomNumRow[programInfoRow.MonthNumOnSpreadsheet]++;

                            /* NOTE: The Excel sheet only asks for 1 row of Ethnicity, IEP, and DLL from 
                             * us and calculates the other row automatically
                             */

                            //Get the ethnicity numbers
                            if (!programInfoRow.Ethnicity.ToLower().Contains("not"))
                                ethnicityNumRow[programInfoRow.MonthNumOnSpreadsheet]++;

                            //Get the current race
                            string race = programInfoRow.Race.ToLower();

                            //Get the race numbers (American Indian and Alaskan native are combined in the stored procedure,
                            //so are Native Hawaiian and Pacific Islander)
                            if (race.Contains("american indian"))
                                race1NumRow[programInfoRow.MonthNumOnSpreadsheet]++;
                            else if (race == "asian")
                                race2NumRow[programInfoRow.MonthNumOnSpreadsheet]++;
                            else if (race.Contains("black"))
                                race3NumRow[programInfoRow.MonthNumOnSpreadsheet]++;
                            else if (race.Contains("native hawaiian"))
                                race4NumRow[programInfoRow.MonthNumOnSpreadsheet]++;
                            else if (race == "two or more races")
                                race5NumRow[programInfoRow.MonthNumOnSpreadsheet]++;
                            else if (race == "white")
                                race6NumRow[programInfoRow.MonthNumOnSpreadsheet]++;

                            //Get the gender numbers
                            if (programInfoRow.Gender.ToLower() == "female")
                                genderNumRow[programInfoRow.MonthNumOnSpreadsheet]++;

                            //Get the IEP numbers
                            if (programInfoRow.HasIEP)
                                IEPNumRow[programInfoRow.MonthNumOnSpreadsheet]++;

                            //Get the DLL numbers
                            if (programInfoRow.IsDLL)
                                DLLNumRow[programInfoRow.MonthNumOnSpreadsheet]++;
                        }

                        //Set the worksheet values
                        programInfoWorksheet.Cells[4, 2].LoadFromText("All Classrooms," + string.Join(",", classroomNumRow));
                        programInfoWorksheet.Cells[37, 3].LoadFromText(string.Join(",", ethnicityNumRow));
                        programInfoWorksheet.Cells[42, 3].LoadFromText(string.Join(",", race1NumRow));
                        programInfoWorksheet.Cells[43, 3].LoadFromText(string.Join(",", race2NumRow));
                        programInfoWorksheet.Cells[44, 3].LoadFromText(string.Join(",", race3NumRow));
                        programInfoWorksheet.Cells[45, 3].LoadFromText(string.Join(",", race4NumRow));
                        programInfoWorksheet.Cells[46, 3].LoadFromText(string.Join(",", race5NumRow));
                        programInfoWorksheet.Cells[47, 3].LoadFromText(string.Join(",", race6NumRow));
                        programInfoWorksheet.Cells[51, 3].LoadFromText(string.Join(",", genderNumRow));
                        programInfoWorksheet.Cells[56, 3].LoadFromText(string.Join(",", IEPNumRow));
                        programInfoWorksheet.Cells[61, 3].LoadFromText(string.Join(",", DLLNumRow));

                        //------------------- END PROGRAM INFO -----------------------------

                        //------------------- CHILDREN AND BIRs -----------------------------

                        //To hold the list of children PKs already added (to prevent duplicates)
                        List<int> childrenAdded = new List<int>();

                        //Start putting the children and BIRs in at the second row
                        int incidentIndex = 2;
                        int childIndex = 2;

                        //Loop through the stored procedure results
                        foreach (rspBIRExcel_ChildrenAndBIRs_Result childAndBIR in allBIRAndChildInfo)
                        {
                            //Don't insert duplicate child information into the worksheet
                            if (!childrenAdded.Contains(childAndBIR.ChildFK))
                            {
                                //Add the child to a stringbuilder
                                StringBuilder childrenToAdd = new StringBuilder();
                                childrenToAdd.Append(childAndBIR.FirstName + " " + childAndBIR.LastName + ",");
                                childrenToAdd.Append(childAndBIR.ProgramSpecificID + "_" + childAndBIR.ProgramFKChild.ToString() + ",");
                                childrenToAdd.Append(childAndBIR.Gender + ",");
                                childrenToAdd.Append((childAndBIR.IsDLL ? "DLL" : "Non-DLL") + ",");
                                childrenToAdd.Append((childAndBIR.HasIEP ? "Yes" : "No") + ",");
                                childrenToAdd.Append(childAndBIR.Ethnicity + ",");
                                childrenToAdd.Append(childAndBIR.Race + ",");
                                childrenToAdd.Append((childAndBIR.DischargeDate.HasValue ? "Disenrolled" : "Enrolled") + ",");
                                childrenToAdd.Append((childAndBIR.DischargeDate.HasValue ? "Disenrolled on " + childAndBIR.DischargeDate.Value.ToString("MM/dd/yyyy") + ".  Reason: " + childAndBIR.DischargeReason + "," : ","));

                                //Add the child to the worksheet via the stringbuilder
                                childWorksheet.Cells[childIndex, 1].LoadFromText(childrenToAdd.ToString());

                                //Record the fact that the child was added
                                childrenAdded.Add(childAndBIR.ChildFK);

                                //Increment the child index
                                childIndex++;
                            }

                            //Add the BIR to a stringbuilder
                            StringBuilder incidentsToAdd = new StringBuilder();
                            incidentsToAdd.Append(childAndBIR.ClassroomID + "_" + childAndBIR.ProgramFKClassroom.ToString() + ",");
                            incidentsToAdd.Append(childAndBIR.ProgramSpecificID + "_" + childAndBIR.ProgramFKChild.ToString() + ",");
                            incidentsToAdd.Append(childAndBIR.IncidentDatetime.ToString("MMMM") + ",");
                            incidentsToAdd.Append(childAndBIR.IncidentDatetime.ToString("MM/dd/yy") + ",");
                            incidentsToAdd.Append(childAndBIR.IncidentDatetime.ToString("hh:mm tt") + ",");
                            incidentsToAdd.Append(childAndBIR.ProblemBehavior + ",");
                            incidentsToAdd.Append(childAndBIR.Activity + ",");
                            incidentsToAdd.Append(childAndBIR.OthersInvolved + ",");
                            incidentsToAdd.Append(childAndBIR.PossibleMotivation + ",");
                            incidentsToAdd.Append(childAndBIR.StrategyResponse + ",");
                            incidentsToAdd.Append(childAndBIR.AdminFollowUp + ",");

                            //Add the BIR to the worksheet via the stringbuilder
                            BIRWorksheet.Cells[incidentIndex, 1].LoadFromText(incidentsToAdd.ToString());

                            //Bug Fix - Excel doesn't like the string version of the time and requires a TimeSpan instead
                            //Set the time of the incident
                            BIRWorksheet.Cells[incidentIndex, 5].Value = childAndBIR.IncidentDatetime.TimeOfDay;

                            //Increment the incident index
                            incidentIndex++;
                        }

                        //------------------- END CHILDREN AND BIRs -----------------------------

                        //Return a byte array representation of the excel file
                        return excel.GetAsByteArray();
                    }
                }
                else
                {
                    //The file doesn't exist, return null
                    return null;
                }
            }
            catch(Exception ex)
            {
                //Log any exceptions and return null
                LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// This contains the File Type FKs from the database
        /// </summary>
        public enum FileTypeFKs
        {
            STATE_WIDE = 1,
            HUB_WIDE = 2,
            PROGRAM_WIDE = 3,
            COHORT_WIDE = 4
        }

        /// <summary>
        /// This contains the News Type FKs from the database
        /// </summary>
        public enum NewsTypeFKs
        {
            APPLICATION = 1,
            STATE_WIDE = 2,
            HUB_WIDE = 3,
            PROGRAM_WIDE = 4,
            COHORT_WIDE = 5
        }

        /// <summary>
        /// This contains the Program Type FKs from the database
        /// </summary>
        public enum ProgramTypeFKs
        {
            FAMILY_CHILD_CARE = 1,
            GROUP_FAMILY_CHILD_CARE = 2
        }

        /// <summary>
        /// This contains the Job Type FKs from the database
        /// </summary>
        public enum JobTypeFKs
        {
            TEACHER = 1,
            TEACHING_ASSISTANT = 2,
            CONSULTANT = 3,
            COACH = 4
        }

        /// <summary>
        /// This contains the Training FKs from the database
        /// </summary>
        public enum TrainingFKs
        {
            PRACTICE_BASED_COACHING = 1,
            INTRODUCTION_TO_COACHING = 2,
            TPOT_OBSERVER = 3,
            TPITOS_OBSERVER = 4
        }

        #endregion
    }

    /// <summary>
    /// This class can hold information about the user's selected
    /// program role
    /// </summary>
    public sealed class ProgramAndRoleFromSession
    {
        public List<int> ProgramFKs { get; set; }
        public int? CurrentProgramFK { get; set; }
        public string ProgramName { get; set; }
        public bool? ShowBOQ { get; set; }
        public bool? ShowBOQFCC { get; set; }
        public int? RoleFK { get; set; }
        public string RoleName { get; set; }
        public bool? AllowedToEdit { get; set; }
        public int? HubFK { get; set; }
        public string HubName { get; set; }
        public int? StateFK { get; set; }
        public string StateName { get; set; }
        public string StateLogoFileName { get; set; }
        public string StateCatchphrase { get; set; }
        public string StateDisclaimer { get; set; }
        public List<int> CohortFKs { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProgramAndRoleFromSession()
        {
            ProgramFKs = null;
            CurrentProgramFK = null;
            ProgramName = null;
            ShowBOQ = null;
            ShowBOQFCC = null;
            RoleFK = null;
            RoleName = null;
            AllowedToEdit = null;
            HubFK = null;
            HubName = null;
            StateFK = null;
            StateName = null;
            StateLogoFileName = null;
            StateCatchphrase = null;
            StateDisclaimer = null;
            CohortFKs = null;
        }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="programFKs">A list of the programs that the current user can access data for</param>
        /// <param name="currentProgramFK">The Program's key</param>
        /// <param name="programName">The Program's name</param>
        /// <param name="showBOQ">A boolean that says whether or not to show the BOQ page</param>
        /// <param name="showBOQFCC">A boolean that says whether or not to show the BOQFCC page</param>
        /// <param name="roleFK">The ProgramRole's key</param>
        /// <param name="roleName">The ProgramRole's name</param>
        /// <param name="allowedToEdit">The ProgramRole's AllowedToEdit field</param>
        /// <param name="hubFK">The Program's Hub's key</param>
        /// <param name="hubName">The Program's Hub's name</param>
        /// <param name="stateFK">The Program's State's key</param>
        /// <param name="stateName">The Program's State's name</param>
        /// <param name="stateLogoFileName">The state's logo filename</param>
        /// <param name="stateCatchphrase">The state's catchphrase</param>
        /// <param name="stateDisclaimer">The state's disclaimer</param>
        /// <param name="cohortFKs">The cohort fks for the programs</param>
        public ProgramAndRoleFromSession(List<int> programFKs, int? currentProgramFK, string programName,
            bool? showBOQ, bool? showBOQFCC,
            int? roleFK, string roleName, bool? allowedToEdit,
            int? hubFK, string hubName,
            int? stateFK, string stateName, string stateLogoFileName, string stateCatchphrase, string stateDisclaimer,
            List<int> cohortFKs)
        {
            ProgramFKs = programFKs;
            CurrentProgramFK = currentProgramFK;
            ProgramName = programName;
            ShowBOQ = showBOQ;
            ShowBOQFCC = showBOQFCC;
            RoleFK = roleFK;
            RoleName = roleName;
            AllowedToEdit = allowedToEdit;
            HubFK = hubFK;
            HubName = hubName;
            StateFK = stateFK;
            StateName = stateName;
            StateLogoFileName = stateLogoFileName;
            StateCatchphrase = stateCatchphrase;
            StateDisclaimer = stateDisclaimer;
            CohortFKs = cohortFKs;
        }
    }
}