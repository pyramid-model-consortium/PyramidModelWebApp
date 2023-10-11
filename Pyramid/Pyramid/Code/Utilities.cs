using System;
using System.Linq;
using System.Data.Entity;
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
using System.Drawing;
using System.ComponentModel;
using PhoneNumbers;

namespace Pyramid.Code
{
    public sealed class Utilities
    {
        #region Session Interaction

        /// <summary>
		/// This class holds session keys
		/// </summary>
		public class SessionKey
        {
            //The session keys
            public static string CONFIDENTIALITY_ACCEPTED
            {
                get
                {
                    return "ConfidentialityAccepted";
                }
            }

            public static string CONFIDENTIALITY_ENABLED
            {
                get
                {
                    return "ConfidentialityEnabled";
                }
            }

            public static string BODY_WIDTH
            {
                get
                {
                    return "ScreenWidth";
                }
            }
        }

        /// <summary>
        /// This method returns a value from session for the specified key
        /// </summary>
        /// <param name="key">The session key</param>
        /// <returns>A string representation of the session value if it exists, null otherwise</returns>
        public static string GetSessionValue(string key)
        {
            if(HttpContext.Current.Session[key] != null)
            {
                return HttpContext.Current.Session[key].ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This method sets a value in session for the specified key
        /// </summary>
        /// <param name="key">The session key</param>
        /// <param name="value">The value for the key</param>
        public static void SetSessionValue(string key, object value)
        {
            //Set the session value
            HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// This method calculates whether the user has accepted the confidentiality agreement or not
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="stateFK">The state FK</param>
        /// <returns>True if the user has not accepted, the false otherwise</returns>
        public static bool IsConfidentialityAccepted(string username, int stateFK)
        {
            //To hold the acceptance value
            bool isAccepted = false;
            
            //Try to get the value from session
            if(bool.TryParse(GetSessionValue(SessionKey.CONFIDENTIALITY_ACCEPTED), out isAccepted))
            {
                //Return the session value
                return isAccepted;
            }
            else
            {
                //Couldn't get the value from session, get it from the database
                using(PyramidContext context = new PyramidContext())
                {
                    //Get the agreement from the database
                    ConfidentialityAgreement agreement = context.ConfidentialityAgreement.AsNoTracking()
                                                                .Where(ca => ca.Username == username && ca.StateFK == stateFK)
                                                                .OrderByDescending(ca => ca.AgreementDate)
                                                                .FirstOrDefault();

                    //Get the state from the database
                    State state = context.State.AsNoTracking()
                                           .Where(s => s.StatePK == stateFK)
                                           .FirstOrDefault();

                    //Make sure the state has confidentiality enabled
                    if (state.ConfidentialityEnabled)
                    {
                        //Check to see if the agreement and state exist
                        if (agreement != null && state != null)
                        {
                            //The agreement and state exist, set the session value based on the necessary dates
                            isAccepted = (agreement.AgreementDate >= state.ConfidentialityChangeDate);
                        }
                        else
                        {
                            //The agreement or state is missing, the acceptance is false
                            isAccepted = false;
                        }
                    }
                    else
                    {
                        //Agreement is not required, set accepted to true
                        isAccepted = true;
                    }
                }

                //Set the session value
                SetSessionValue(SessionKey.CONFIDENTIALITY_ACCEPTED, isAccepted);

                //Return the value from the database
                return isAccepted;
            }
        }

        /// <summary>
        /// This method returns the confidentiality enabled value for the specified state
        /// </summary>
        /// <param name="stateFK">The state FK</param>
        /// <returns>True if the user has not accepted, the false otherwise</returns>
        public static bool IsConfidentialityEnabled(int stateFK)
        {
            //To hold the enabled value
            bool isEnabled = false;

            //Try to get the value from session
            if (bool.TryParse(GetSessionValue(SessionKey.CONFIDENTIALITY_ENABLED), out isEnabled))
            {
                //Return the session value
                return isEnabled;
            }
            else
            {
                //Couldn't get the value from session, get it from the database
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the state from the database
                    State state = context.State.AsNoTracking()
                                           .Where(s => s.StatePK == stateFK)
                                           .FirstOrDefault();

                    //Set the local variable
                    isEnabled = state.ConfidentialityEnabled;
                }

                //Set the session value
                SetSessionValue(SessionKey.CONFIDENTIALITY_ENABLED, isEnabled);

                //Return the value from the database
                return isEnabled;
            }
        }

        #endregion

        #region Roles/Identity

        /// <summary>
        /// This contains the Program Role FKs from the database
        /// </summary>
        public enum CodeProgramRoleFKs
        {
            DATA_COLLECTOR = 1,
            AGGREGATE_DATA_VIEWER = 2,
            DETAIL_DATA_VIEWER = 3,
            HUB_DETAIL_DATA_VIEWER = 4,
            APPLICATION_ADMIN = 5,
            SUPER_ADMIN = 6,
            STATE_AGGREGATE_VIEWER = 7,
            STATE_DETAIL_DATA_VIEWER = 8,
            HUB_AGGREGATE_DATA_VIEWER = 11,
            CLASSROOM_COACH_DATA_COLLECTOR = 12,
            STATE_DATA_COLLECTOR = 13,
            HUB_DATA_COLLECTOR = 14,
            LEADERSHIP_COACH = 15,
            MASTER_CADRE_MEMBER = 16,
            STATE_DATA_ADMIN = 17,
            NATIONAL_DATA_ADMIN = 18,
            NATIONAL_REPORT_VIEWER = 19,
            HUB_LEADERSHIP_COACH = 20,
            PROGRAM_IMPLEMENTATION_COACH = 21
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
            ProgramAndRoleFromSession returnObj;

            try
            {
                //Try to get the program role object from session
                returnObj = (ProgramAndRoleFromSession)session["ProgramRole"];
            }
            catch (Exception ex)
            {
                //It failed, log the exception
                LogException(ex);

                //Set the return obj
                returnObj = new ProgramAndRoleFromSession();
            }

            //If the role from session does not have values, redirect the user
            if (returnObj == null || !returnObj.CodeProgramRoleFK.HasValue || returnObj.RoleName == null
                || !returnObj.CurrentProgramFK.HasValue || returnObj.ProgramName == null || !returnObj.CurrentHubFK.HasValue
                || !returnObj.CurrentStateFK.HasValue)
            {
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    //If the user is logged in, redirect them to the selet role page.
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
                //Set the object in session
                session["ProgramRole"] = programAndRole;
            }
            catch (Exception ex)
            {
                //It failed, log the exception
                LogException(ex);
            }
        }

        /// <summary>
        /// This method fills a ProgramAndRoleFromSession object based on the passed
        /// user role.
        /// </summary>
        /// <param name="userRole">The user's UserProgramRole</param>
        /// <returns>A filled ProgramAndRoleFromSession object</returns>
        public static ProgramAndRoleFromSession GetProgramRoleFromDatabase(UserProgramRole userRole)
        {
            //To hold the role information
            ProgramAndRoleFromSession roleInfo = new ProgramAndRoleFromSession();

            //Set the session variables for the program roles
            roleInfo.CodeProgramRoleFK = userRole.CodeProgramRole.CodeProgramRolePK;
            roleInfo.RoleName = userRole.CodeProgramRole.RoleName;
            roleInfo.ViewPrivateChildInfo = userRole.CodeProgramRole.ViewPrivateChildInfo;
            roleInfo.ViewPrivateEmployeeInfo = userRole.CodeProgramRole.ViewPrivateEmployeeInfo;
            roleInfo.CurrentProgramFK = userRole.ProgramFK;
            roleInfo.ProgramName = userRole.Program.ProgramName;

            //Get the hub and state information
            using (PyramidContext context = new PyramidContext())
            {
                //Get the current program
                Program currentProgram = context.Program.AsNoTracking()
                                            .Include(p => p.Hub)
                                            .Include(p => p.State)
                                            .Include(p => p.ProgramType)
                                            .Where(p => p.ProgramPK == userRole.ProgramFK).FirstOrDefault();

                //Set the state and hub info
                roleInfo.CurrentHubFK = currentProgram.HubFK;
                roleInfo.HubName = currentProgram.Hub.Name;
                roleInfo.CurrentStateFK = currentProgram.StateFK;
                roleInfo.StateName = currentProgram.State.Name;
                roleInfo.StateLogoFileName = currentProgram.State.LogoFilename;
                roleInfo.StateThumbnailLogoFileName = currentProgram.State.ThumbnailLogoFilename;
                roleInfo.StateHomePageLogoOption = currentProgram.State.HomePageLogoOption;
                roleInfo.StateCatchphrase = currentProgram.State.Catchphrase;
                roleInfo.StateDisclaimer = currentProgram.State.Disclaimer;

                //Set the allowed program fks
                if (roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.HUB_DETAIL_DATA_VIEWER
                    || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.HUB_DATA_COLLECTOR
                    || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.HUB_AGGREGATE_DATA_VIEWER
                    || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.HUB_LEADERSHIP_COACH)
                {
                    //Hub role, get the programs in that hub
                    var hubPrograms = context.Program.AsNoTracking()
                                                .Where(p => p.HubFK == roleInfo.CurrentHubFK.Value)
                                                .ToList();

                    //Allow them to see all programs in that hub
                    roleInfo.ProgramFKs = hubPrograms
                                            .Select(hp => hp.ProgramPK)
                                            .ToList();

                    //Allow them to see all cohorts in their hub
                    roleInfo.CohortFKs = hubPrograms
                                                .Select(hp => hp.CohortFK)
                                                .Distinct()
                                                .ToList();

                    //Restrict them to one hub
                    List<int> hubFKs = new List<int>();
                    hubFKs.Add(roleInfo.CurrentHubFK.Value);
                    roleInfo.HubFKs = hubFKs;

                    //Restrict them to one state
                    List<int> stateFKs = new List<int>();
                    stateFKs.Add(roleInfo.CurrentStateFK.Value);
                    roleInfo.StateFKs = stateFKs;

                    //Don't restrict their view of the BOQs
                    roleInfo.ShowBOQ = true;
                    roleInfo.ShowBOQFCC = true;

                    //Set the locked bit field
                    //If ALL of the programs in the hub are ended as of now, then the bit field is set to true
                    if (currentProgram.State.LockEndedPrograms &&
                        hubPrograms.Where(p => p.ProgramEndDate.HasValue == true && p.ProgramEndDate.Value <= DateTime.Now).Count() == hubPrograms.Count)
                    {
                        roleInfo.IsProgramLocked = true;
                    }
                    else
                    {
                        roleInfo.IsProgramLocked = false;
                    }
                }
                else if (roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.APPLICATION_ADMIN
                        || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.STATE_DATA_ADMIN
                        || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.STATE_AGGREGATE_VIEWER
                        || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.STATE_DATA_COLLECTOR
                        || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.STATE_DETAIL_DATA_VIEWER
                        || roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.MASTER_CADRE_MEMBER)
                {
                    //State or master cadre role, allow them to see all programs in a state
                    var allProgramsInState = context.Program.AsNoTracking()
                                                .Where(p => p.StateFK == roleInfo.CurrentStateFK.Value)
                                                .ToList();
                    roleInfo.ProgramFKs = allProgramsInState.Select(p => p.ProgramPK).ToList();

                    //Allow them to see all cohorts in a state
                    roleInfo.CohortFKs = context.Cohort.AsNoTracking()
                                                .Where(c => c.StateFK == roleInfo.CurrentStateFK.Value)
                                                .Select(c => c.CohortPK).ToList();

                    //Allow them to see all hubs in a state
                    roleInfo.HubFKs = context.Hub.AsNoTracking()
                                                .Where(h => h.StateFK == roleInfo.CurrentStateFK.Value)
                                                .Select(h => h.HubPK).ToList();

                    //Restrict them to one state
                    List<int> stateFKs = new List<int>();
                    stateFKs.Add(roleInfo.CurrentStateFK.Value);
                    roleInfo.StateFKs = stateFKs;

                    //Don't restrict their view of the BOQs
                    roleInfo.ShowBOQ = true;
                    roleInfo.ShowBOQFCC = true;

                    //Set the locked bit field
                    //If ALL of the programs in the state are ended as of now, then the bit field is set to true
                    if (currentProgram.State.LockEndedPrograms &&
                        allProgramsInState.Where(p => p.ProgramEndDate.HasValue == true && p.ProgramEndDate.Value <= DateTime.Now).Count() == allProgramsInState.Count)
                    {
                        roleInfo.IsProgramLocked = true;
                    }
                    else
                    {
                        roleInfo.IsProgramLocked = false;
                    }
                }
                else if (roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                         roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER)
                {
                    //National role, allow them to manage programs for states that are not implementing PIDS
                    //(Exclude national state programs since they shouldn't have data)
                    roleInfo.ProgramFKs = context.Program.Include(p => p.State).AsNoTracking()
                                                .Where(p => p.State.UtilizingPIDS == false 
                                                        && p.State.ShareDataNationally == true
                                                        && p.StateFK != (int)StateFKs.NATIONAL)
                                                .Select(p => p.ProgramPK).ToList();

                    //Don't allow access to cohorts
                    List<int> cohortFKs = new List<int>();
                    roleInfo.CohortFKs = cohortFKs;

                    //Allow access to all hubs for states that allow it
                    roleInfo.HubFKs = context.Hub.Include(h => h.State).AsNoTracking()
                                                .Where(h => h.State.ShareDataNationally == true 
                                                            && h.StateFK != (int)StateFKs.NATIONAL)
                                                .Select(h => h.HubPK).ToList();

                    //Allow them to see all states that allow it
                    roleInfo.StateFKs = context.State.AsNoTracking()
                                                .Where(s => s.ShareDataNationally == true
                                                        && s.StatePK != (int)StateFKs.NATIONAL)
                                                .Select(s => s.StatePK).ToList();

                    //Don't restrict their view of the BOQs
                    roleInfo.ShowBOQ = true;
                    roleInfo.ShowBOQFCC = true;

                    //Don't worry about the locked bit field for national.  If all programs are ended, nobody is using the system.
                    roleInfo.IsProgramLocked = false;
                }
                else if (roleInfo.CodeProgramRoleFK == (int)CodeProgramRoleFKs.SUPER_ADMIN)
                {
                    //Super admin, all programs in all states
                    roleInfo.ProgramFKs = context.Program.AsNoTracking()
                                                .Select(p => p.ProgramPK).ToList();

                    //All cohorts
                    roleInfo.CohortFKs = context.Cohort.AsNoTracking()
                                                .Select(c => c.CohortPK).ToList();

                    //All hubs
                    roleInfo.HubFKs = context.Hub.AsNoTracking()
                                                .Select(h => h.HubPK).ToList();

                    //Allow them to see all states
                    roleInfo.StateFKs = context.State.AsNoTracking()
                                                .Select(s => s.StatePK).ToList();

                    //Don't restrict their view of the BOQs
                    roleInfo.ShowBOQ = true;
                    roleInfo.ShowBOQFCC = true;

                    //Super Admin is not prevented from adding data to ended programs
                    roleInfo.IsProgramLocked = false;
                }
                else
                {
                    //Program-level role, limit to the current program fk
                    List<int> programFKs = new List<int>();
                    programFKs.Add(roleInfo.CurrentProgramFK.Value);
                    roleInfo.ProgramFKs = programFKs;

                    //Limit to current cohort fk
                    List<int> cohortFKs = new List<int>();
                    cohortFKs.Add(currentProgram.CohortFK);
                    roleInfo.CohortFKs = cohortFKs;

                    //Restrict them to one hub
                    List<int> hubFKs = new List<int>();
                    hubFKs.Add(roleInfo.CurrentHubFK.Value);
                    roleInfo.HubFKs = hubFKs;

                    //Restrict them to one state
                    List<int> stateFKs = new List<int>();
                    stateFKs.Add(roleInfo.CurrentStateFK.Value);
                    roleInfo.StateFKs = stateFKs;

                    //Determine which BOQs to display
                    bool usesBOQFCC = currentProgram.ProgramType
                        .Where(pt => pt.TypeCodeFK == (int)ProgramTypeFKs.FAMILY_CHILD_CARE ||
                                     pt.TypeCodeFK == (int)ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE)
                                .Count() > 0;

                    bool usesNormalBOQ = currentProgram.ProgramType
                        .Where(pt => pt.TypeCodeFK != (int)ProgramTypeFKs.FAMILY_CHILD_CARE &&
                                     pt.TypeCodeFK != (int)ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE)
                                .Count() > 0;

                    //Limit their view to the right BOQ type(s)
                    roleInfo.ShowBOQ = usesNormalBOQ;
                    roleInfo.ShowBOQFCC = usesBOQFCC;

                    //Set the locked bit field
                    if (currentProgram.State.LockEndedPrograms &&
                        currentProgram.ProgramEndDate.HasValue &&
                        currentProgram.ProgramEndDate.Value <= DateTime.Now)
                    {
                        roleInfo.IsProgramLocked = true;
                    }
                    else
                    {
                        roleInfo.IsProgramLocked = false;
                    }
                }                
            }

            //Return the role information for the session
            return roleInfo;
        }

        /// <summary>
        /// Get the permissions for a specific role.
        /// </summary>
        /// <param name="codeProgramRolePK">The role PK</param>
        /// <param name="isProgramLocked">Whether or not the program is locked</param>
        /// <returns>The permissions object</returns>
        public static List<CodeProgramRolePermission> GetProgramRolePermissionsFromDatabase(int codeProgramRolePK, bool isProgramLocked)
        {
            //To hold the permissions object list
            List<CodeProgramRolePermission> permissions = new List<CodeProgramRolePermission>();

            //Get the permissions object list
            using (PyramidContext context = new PyramidContext())
            {
                permissions = context.CodeProgramRolePermission.AsNoTracking()
                                        .Include(cprp => cprp.CodeForm)
                                        .Include(cprp => cprp.CodeProgramRole)
                                        .Where(cprp => cprp.CodeProgramRoleFK == codeProgramRolePK)
                                        .ToList();
            }

            //Check to see if the program is locked
            if (isProgramLocked)
            {
                //The program is locked, don't allow additions/edits/deletions
                permissions = permissions.Select(cprp => new CodeProgramRolePermission()
                {
                    CodeProgramRolePermissionPK = cprp.CodeProgramRolePermissionPK,
                    AllowedToAdd = false,
                    AllowedToDelete = false,
                    AllowedToEdit = false,
                    AllowedToView = cprp.AllowedToView,
                    AllowedToViewDashboard = cprp.AllowedToViewDashboard,
                    CodeFormFK = cprp.CodeFormFK,
                    CodeProgramRoleFK = cprp.CodeProgramRoleFK,
                    CodeForm = cprp.CodeForm,
                    CodeProgramRole = cprp.CodeProgramRole
                }).ToList();
            }

            //Return the permissions object list
            return permissions;
        }

        /// <summary>
        /// Get the permissions for a specific role and the passed form(s).
        /// </summary>
        /// <param name="formAbbreviations">A list of form abbreviations to get permissions for</param>
        /// <param name="codeProgramRolePK">The role PK</param>
        /// <param name="isProgramLocked">Whether or not the program is locked</param>
        /// <returns>A list that contains the relevant permission objects</returns>
        public static List<CodeProgramRolePermission> GetProgramRolePermissionsFromDatabase(List<string> formAbbreviations, int codeProgramRolePK, bool isProgramLocked)
        {
            //To hold the permissions object list
            List<CodeProgramRolePermission> permissions = new List<CodeProgramRolePermission>();

            //Get the permissions object list
            using (PyramidContext context = new PyramidContext())
            {
                permissions = context.CodeProgramRolePermission.AsNoTracking()
                                        .Include(cprp => cprp.CodeForm)
                                        .Include(cprp => cprp.CodeProgramRole)
                                        .Where(cprp => formAbbreviations.Contains(cprp.CodeForm.FormAbbreviation) && 
                                                       cprp.CodeProgramRoleFK == codeProgramRolePK)
                                        .ToList();
            }

            //Check to see if the program is locked
            if (isProgramLocked)
            {
                //The program is locked, don't allow additions/edits/deletions
                permissions = permissions.Select(cprp => new CodeProgramRolePermission()
                {
                    CodeProgramRolePermissionPK = cprp.CodeProgramRolePermissionPK,
                    AllowedToAdd = false,
                    AllowedToDelete = false,
                    AllowedToEdit = false,
                    AllowedToView = cprp.AllowedToView,
                    AllowedToViewDashboard = cprp.AllowedToViewDashboard,
                    CodeFormFK = cprp.CodeFormFK,
                    CodeProgramRoleFK = cprp.CodeProgramRoleFK,
                    CodeForm = cprp.CodeForm,
                    CodeProgramRole = cprp.CodeProgramRole
                }).ToList();
            }

            //Return the permissions object list
            return permissions;
        }

        /// <summary>
        /// Get the permissions for a specific form and role.
        /// </summary>
        /// <param name="formAbbreviation">The form abbreviation</param>
        /// <param name="codeProgramRolePK">The role PK</param>
        /// <param name="isProgramLocked">Whether or not the program is locked</param>
        /// <returns>The permissions object</returns>
        public static CodeProgramRolePermission GetProgramRolePermissionsFromDatabase(string formAbbreviation, int codeProgramRolePK, bool isProgramLocked)
        {
            //To hold the permissions object
            CodeProgramRolePermission permissions;

            //Get the permissions object
            using (PyramidContext context = new PyramidContext())
            {
                permissions = context.CodeProgramRolePermission.AsNoTracking()
                                        .Include(cprp => cprp.CodeForm)
                                        .Where(cprp => cprp.CodeForm.FormAbbreviation == formAbbreviation &&
                                                        cprp.CodeProgramRoleFK == codeProgramRolePK).FirstOrDefault();
            }

            //Check to see if the program is locked
            if(isProgramLocked)
            {
                //The program is locked, don't allow additions/edits/deletions
                permissions.AllowedToAdd = false;
                permissions.AllowedToEdit = false;
                permissions.AllowedToDelete = false;
            }

            //Return the permissions object
            return permissions;
        }

        #endregion

        #region Email Templates and Methods

        /// <summary>
        /// This holds the paths to the email template files
        /// </summary>
        public static class EmailTemplatePaths
        {
            public const string StandardEmail = "~/EmailTemplates/StandardEmail.html";
        }

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
        public static string GetEmailHTML(string buttonURL, string buttonText, bool buttonVisible, string bodyTitle, string bodyText, string expirationText, HttpRequest request)
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
            urlToGenericLogo.Path = "Content/images/CustomPIDSLogoSquare.png";
            urlToGenericLogo.Port = request.Url.Port;

            //Get the string url
            string strGenericLogoURL = urlToGenericLogo.Uri.AbsoluteUri;

            //Get the file path for the email HTML file
            string emailHTMLFilePath = HttpContext.Current.Server.MapPath(EmailTemplatePaths.StandardEmail);

            //Get the HTML from the email file
            StringBuilder htmlStringBuilder = new StringBuilder(File.ReadAllText(emailHTMLFilePath));

            //If the body text contains curly braces, it is used for string.Format
            if (bodyText.Contains("{") || bodyText.Contains("}"))
            {
                //Escape all current curly braces so that string.Format works as intended
                htmlStringBuilder.Replace("{", "{{").Replace("}", "}}");
            }

            //Replace all the necessary sections of the email HTML with the proper values
            htmlStringBuilder.Replace("*ImageURL*", strGenericLogoURL)
                                        .Replace("*ButtonURL*", buttonURL)
                                        .Replace("*ButtonText*", buttonText)
                                        .Replace("*ButtonVisible*", (buttonVisible ? "" : "display:none;"))
                                        .Replace("*BodyTitle*", bodyTitle)
                                        .Replace("*BodyText*", bodyText)
                                        .Replace("*ExpirationText*", expirationText);

            //Return the email HTML
            return htmlStringBuilder.ToString();
        }

        /// <summary>
        /// This method sends an email using the email service
        /// </summary>
        /// <param name="toAddress">The email address to send an email to</param>
        /// <param name="titleText">The subject of the email and title in the email</param>
        /// <param name="bodyText">The text for the body of the email</param>
        /// <param name="buttonText">The button text</param>
        /// <param name="buttonURLPath">The button URL path</param>
        /// <param name="buttonVisible">Show/hide the button</param>
        public static void SendEmail(string toAddress, string titleText, string bodyText, string buttonText, string buttonURLPath, bool buttonVisible)
        {
            //Get the url for the email
            UriBuilder loginURL = new UriBuilder();
            loginURL.Scheme = HttpContext.Current.Request.Url.Scheme;
            loginURL.Host = HttpContext.Current.Request.Url.Host;
            loginURL.Path = buttonURLPath;
            loginURL.Port = HttpContext.Current.Request.Url.Port;

            //Get the HTML email
            string emailBody = Utilities.GetEmailHTML(loginURL.Uri.AbsoluteUri, buttonText, buttonVisible,
                                            titleText, bodyText, "", HttpContext.Current.Request);

            //Send the email
            EmailService.SendEmail(toAddress, titleText, emailBody);
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
            //Use different logging based on whether context is available or not
            if (HttpContext.Current != null)
            {
                try
                {
                    //Log the error via normal ELMAH handler
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                catch (Exception)
                {
                    //Do nothing
                }
            }
            else
            {
                try
                {
                    //Try to log the exception with the other handler
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
                catch (Exception)
                {
                    //Do nothing
                }
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

            public static readonly ConstantAzureStorageContainerName CHILD_FORM_UPLOADS =
                new ConstantAzureStorageContainerName("CHILD_FORM_UPLOADS", "childformuploads");

            public static readonly ConstantAzureStorageContainerName CONFIDENTIALITY_AGREEMENTS =
                new ConstantAzureStorageContainerName("CONFIDENTIALITY_AGREEMENTS", "confidentialityagreements");

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
		public static string GetFileLinkFromAzureStorage(string fileName, bool isPDF, string containerName, int documentExpirationMinutes)
        {
            //Make sure that the file name and container name are supplied
            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(containerName))
            {
                //Connect to Azure storage and get a reference to the file
                CloudBlockBlob blob = GetBlobFromAzureStorage(fileName, containerName);

                //Make sure the blob exists
                if (blob.Exists())
                {
                    //Set a policy for read-only access that expires in a set number of minutes
                    SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
                    policy.Permissions = SharedAccessBlobPermissions.Read;
                    policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(documentExpirationMinutes);

                    //Only allow access to this file
                    SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders();
                    if (isPDF)
                    {
                        headers.ContentDisposition = string.Format("fileName=\"{0}\"", fileName);
                    }
                    else
                    {
                        headers.ContentDisposition = string.Format("attachment;fileName=\"{0}\"", fileName);
                    }

                    //Get the SAS token
                    string sasToken = blob.GetSharedAccessSignature(policy, headers);

                    //Return the URL and SAS token so the user can view/download the file
                    return blob.Uri.AbsoluteUri + sasToken;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
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
            //Make sure the file and container names are supplied
            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(containerName))
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
                return null;
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
            //Make sure the file and container names are supplied
            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(containerName))
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
                return null;
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
            //Check to make sure the file name and container name are supplied
            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(containerName))
            {
                //Connect to Azure storage and get a reference to the file
                CloudStorageAccount account = CloudStorageAccount.Parse(WebConfigurationManager.ConnectionStrings["PyramidStorage"].ConnectionString);
                CloudBlobClient blobClient = account.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(fileName);

                //Return the blob file
                return blob;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Cookies

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
                //Get the cookie
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
                //Get the user cookie
                HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieName];

                //If the cookie is null, set it to a new cookie
                if (cookie == null)
                {
                    cookie = new HttpCookie(cookieName);
                }

                //Set the section value
                cookie[cookieSection] = cookieSectionValue;

                //Set the cookie properties
                cookie.Expires = DateTime.Now.AddDays(expirationDays);
                cookie.SameSite = SameSiteMode.Lax;
                cookie.Secure = HttpContext.Current.Request.IsSecureConnection;

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

        #endregion

        #region MISC

        ///<summary>
        /// This list is from the bootstrap 4 alert colors
        ///</summary>
        public static class Bootstrap4AlertColors
        {
            public static Color AlertDangerBackground = Color.FromArgb(248, 215, 218);
            public static Color AlertDangerBorder = Color.FromArgb(245, 198, 203);
            public static Color AlertDangerText = Color.FromArgb(114, 28, 36);
            public static Color AlertWarningBackground = Color.FromArgb(255, 243, 205);
            public static Color AlertWarningBorder = Color.FromArgb(255, 238, 186);
            public static Color AlertWarningText = Color.FromArgb(133, 100, 4);
        }

        /// <summary>
        /// This method converts a color object to a hex string.
        /// </summary>
        /// <param name="colorToConvert">The color object to convert</param>
        /// <returns>A hex string representation of the color object</returns>
        public static string ConvertColorToHexString(Color colorToConvert)
        {
            //Get the hex version
            return string.Format("#{0}{1}{2}", colorToConvert.R.ToString("X2"), colorToConvert.G.ToString("X2"), colorToConvert.B.ToString("X2"));
        }

        ///<summary>
        /// This list is from the DevExpress Mixed color palette
        ///</summary>
        public static class DevExChartColors 
        {
            public static Color Blue1 = Color.FromArgb(255, 63, 104, 155);
            public static Color Red1 = Color.FromArgb(255, 157, 64, 61);
            public static Color Green1 = Color.FromArgb(255, 126, 153, 71);
            public static Color Purple1 = Color.FromArgb(255, 104, 80, 132);
            public static Color Blue2 = Color.FromArgb(255, 59, 140, 162);
            public static Color Orange1 = Color.FromArgb(255, 203, 122, 55);
            public static Color Blue3 = Color.FromArgb(255, 78, 128, 188);
            public static Color Red2 = Color.FromArgb(255, 191, 79, 76);
            public static Color Green2 = Color.FromArgb(255, 154, 186, 88);
            public static Color Purple2 = Color.FromArgb(255, 127, 99, 161);
            public static Color Blue4 = Color.FromArgb(255, 74, 171, 197);
            public static Color Orange2 = Color.FromArgb(255, 246, 149, 69);
            public static Color Yellow1 = Color.FromArgb(255, 232, 222, 39);
            public static Color LightBlue1 = Color.FromArgb(255, 221, 235, 247);
            public static Color LightOrange1 = Color.FromArgb(255, 252, 228, 214);
            public static Color LightGray1 = Color.FromArgb(255, 237, 237, 237);
            public static Color LightYellow1 = Color.FromArgb(255, 255, 242, 204);
            public static Color LightBlue2 = Color.FromArgb(255, 217, 225, 242);
            public static Color LightGreen1 = Color.FromArgb(255, 226, 239, 218);
            public static Color LightRed1 = Color.FromArgb(255, 239, 189, 179);
            public static Color LightPurple1 = Color.FromArgb(255, 204, 204, 255);
        }

        /// <summary>
        /// This class allows the creation of items for a dropdown
        /// </summary>
        public sealed class CustomDropDownSourceItem
        {
            //The value
            public string ItemValue
            {
                get;
                set;
            }
            
            //The text
            public string ItemText
            {
                get;
                set;
            }

            /// <summary>
            /// Constructor with set values from parameters
            /// </summary>
            /// <param name="value">The value</param>
            /// <param name="text">The text</param>
            public CustomDropDownSourceItem(string value, string text)
            {
                //Set the properties
                ItemValue = value;
                ItemText = text;
            }

            /// <summary>
            /// Constructor with no parameters
            /// </summary>
            public CustomDropDownSourceItem()
            {
                //Set the properties to null
                ItemValue = null;
                ItemText = null;
            }
        }

        /// <summary>
        /// This class allows the creation of generic code table information
        /// </summary>
        public sealed class CodeTableInfo
        {
            public int CodeTablePK { get; set; }
            public string ItemAbbreviation { get; set; }
            public string ItemDescription { get; set; }
            public int OrderBy { get; set; }

            /// <summary>
            /// Constructor with set values from parameters
            /// </summary>
            /// <param name="value">The value</param>
            /// <param name="text">The text</param>
            public CodeTableInfo(int primaryKey, string abbreviation, string description, int orderBy)
            {
                //Set the properties
                CodeTablePK = primaryKey;
                ItemAbbreviation = abbreviation;
                ItemDescription = description;
                OrderBy = orderBy;
            }

            /// <summary>
            /// Constructor with no parameters
            /// </summary>
            public CodeTableInfo()
            {
                //Set the properties to default values
                CodeTablePK = 0;
                ItemAbbreviation = null;
                ItemDescription = null;
                OrderBy = 0;
            }
        }

        /// <summary>
        /// This method is converts a string property name to the actual value and
        /// allows this report to dynamically group and select
        /// </summary>
        /// <param name="obj">The object that is of the specified type and has the specified property</param>
        /// <param name="objType">The object's type</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>The property value</returns>
        public static object GetPropertyValue(object obj, Type objType, string propertyName)
        {
            return objType.GetProperty(propertyName).GetValue(obj, null);
        }

        /// <summary>
        /// This method checks to see if the passed double is 
        /// infinity or NaN and returns a proper double.
        /// </summary>
        /// <param name="value">The double value to check</param>
        /// <returns>0 if the value is infinity or NaN and the passed value otherwise</returns>
        public static double GetValidatedDoubleValue(double value)
        {
            //Make sure that the value is not infinity or NaN
            if(double.IsInfinity(value) || double.IsNaN(value))
            {
                //It was infinity or NaN, return 0
                return 0.00d;
            }
            else
            {
                //Valid value, return it
                return value;
            }
        }

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
            if(programRole.CodeProgramRoleFK.HasValue && 
                    (programRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_DATA_ADMIN ||
                     programRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.NATIONAL_REPORT_VIEWER))
            {
                //Get the app title values from the program role
                applicationTitle.Append("<span class='app-state-name'>" + programRole.StateName + "</span>");
                applicationTitle.Append("<span class='app-name'>Pyramid Model Implementation Data System</span>");
                applicationTitle.Append((string.IsNullOrWhiteSpace(programRole.StateCatchphrase) ? "" : "<span class='app-state-catchphrase'>" + programRole.StateCatchphrase + "</span>"));
            }
            else if (programRole.CurrentProgramFK.HasValue)
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
        /// This method determines the validity of a phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number in string format.</param>
        /// <param name="phoneRegionAbbreviation">The region abbreviation for the phone. Ex. US.</param>
        /// <returns>True if the number is valid, false otherwise.</returns>
        public static bool IsPhoneNumberValid(string phoneNumber, string phoneRegionAbbreviation)
        {
            //Check for an extension
            if (phoneNumber.Length > 10)
            {
                //Set the format with the extension
                phoneNumber = string.Format("{0} ext. {1}", phoneNumber.Substring(0, 10), phoneNumber.Substring(10, phoneNumber.Length - 10));
            }

            try
            {
                //Parse the phone number
                PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber currentPhone = phoneNumberUtil.Parse(phoneNumber, phoneRegionAbbreviation);

                //Return the validity
                return phoneNumberUtil.IsValidNumber(currentPhone);
            }
            catch(NumberParseException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// This method returns a parsed phone number in the correct format.
        /// </summary>
        /// <param name="phoneNumber">The phone number in string format.</param>
        /// <param name="phoneRegionAbbreviation">The region abbreviation for the phone. Ex. US.</param>
        /// <returns>The formatted phone number, or ERROR if there is a calculation issue.</returns>
        public static string FormatPhoneNumber(string phoneNumber, string phoneRegionAbbreviation)
        {
            //Check for an extension
            if (phoneNumber.Length > 10)
            {
                //Set the format with the extension
                phoneNumber = string.Format("{0} ext. {1}", phoneNumber.Substring(0, 10), phoneNumber.Substring(10, phoneNumber.Length - 10));
            }

            try 
            {
                //Parse the phone number
                PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber currentPhone = phoneNumberUtil.Parse(phoneNumber, phoneRegionAbbreviation);

                //Return the formatted number
                return phoneNumberUtil.Format(currentPhone, PhoneNumberFormat.NATIONAL);
            }
            catch (NumberParseException ex)
            {
                return "ERROR";
            }
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
        /// This method returns a link to the form for the specified abbreviation
        /// </summary>
        /// <param name="formAbbreviation">The form's abbreviation</param>
        /// <param name="formPK">The form PK</param>
        /// <param name="action">View, edit, or add</param>
        /// <returns>A string representation of the link to the form page</returns>
        public static string GetLinkToForm(string formAbbreviation, int formPK, string action)
        {
            //To hold the link to the form page
            string formLink;

            //Get the form link based on the abbreviation
            switch (formAbbreviation.ToUpper())
            {
                case "ASQSE":
                    formLink = string.Format("/Pages/ASQSE.aspx?ASQSEPK={0}&Action={1}", formPK, action);
                    break;
                case "BIR":
                    formLink = string.Format("/Pages/BehaviorIncident.aspx?BehaviorIncidentPK={0}&Action={1}", formPK, action);
                    break;
                case "BOQ":
                    formLink = string.Format("/Pages/BOQ.aspx?BOQPK={0}&Action={1}", formPK, action);
                    break;
                case "BOQFCC":
                    formLink = string.Format("/Pages/BOQFCC.aspx?BOQFCCPK={0}&Action={1}", formPK, action);
                    break;
                case "CCL":
                    formLink = string.Format("/Pages/CoachingLog.aspx?CoachingLogPK={0}&Action={1}", formPK, action);
                    break;
                case "CHILD":
                    formLink = string.Format("/Pages/Child.aspx?ChildProgramPK={0}&Action={1}", formPK, action);
                    break;
                case "CLASS":
                    formLink = string.Format("/Pages/Classroom.aspx?ClassroomPK={0}&Action={1}", formPK, action);
                    break;
                case "OSES":
                    formLink = string.Format("/Pages/OtherSEScreen.aspx?OtherSEScreenPK={0}&Action={1}", formPK, action);
                    break;
                case "PE":
                    formLink = string.Format("/Pages/ProgramEmployee.aspx?ProgramEmployeePK={0}&Action={1}", formPK, action);
                    break;
                case "TPITOS":
                    formLink = string.Format("/Pages/TPITOS.aspx?TPITOSPK={0}&Action={1}", formPK, action);
                    break;
                case "TPOT":
                    formLink = string.Format("/Pages/TPOT.aspx?TPOTPK={0}&Action={1}", formPK, action);
                    break;
                case "ULF":
                    formLink = string.Format("/Pages/UploadedFiles.aspx");
                    break;
                case "NEWS":
                    formLink = string.Format("/Pages/News.aspx");
                    break;
                default:
                    formLink = "/Default.aspx?messageType=LinkNotFound";
                    break;
            }

            //Return the form link
            return formLink;
        }

        /// <summary>
        /// This method returns a link to the confidentiality document for the specified state
        /// if the document exists and null otherwise
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="documentExpiresMinutes">The number of minutes before the document expires</param>
        /// <returns>A link to the confidentiality document for the specified state if the document exists and null otherwise</returns>
        public static string GetConfidentialityDocumentLink(State state, int documentExpiresMinutes)
        {
            //Get a link to the confidentiality document that is valid for a certain number of minutes
            string confidentialityDocumentLink = Utilities.GetFileLinkFromAzureStorage(state.ConfidentialityFilename,
                                true, Utilities.ConstantAzureStorageContainerName.CONFIDENTIALITY_AGREEMENTS.ToString(), documentExpiresMinutes);

            //Make sure the confidentiality document exists
            if (!string.IsNullOrWhiteSpace(confidentialityDocumentLink))
            {
                //Return the link to the confidentiality document
                return confidentialityDocumentLink;
            }
            else
            {
                //The document doesn't exist
                //Create an error message explaining the issue
                string errorMessage = string.Format("{0} is missing its user agreement document named: {1}.<br/><br/>" +
                    "Take immediate action to fix this issue!", state.Name, state.ConfidentialityFilename);

                //Get the url for the email
                UriBuilder loginURL = new UriBuilder();
                loginURL.Scheme = HttpContext.Current.Request.Url.Scheme;
                loginURL.Host = HttpContext.Current.Request.Url.Host;
                loginURL.Path = "/Account/Login.aspx";
                loginURL.Port = HttpContext.Current.Request.Url.Port;

                //Get the HTML email
                string emailBody = Utilities.GetEmailHTML(loginURL.Uri.AbsoluteUri, "Login", true, 
                                                "Confidentiality Document Missing!",
                                                errorMessage, "", HttpContext.Current.Request);

                //Send an email to the admin about the problem
                string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"];
                EmailService.SendEmail(adminEmailAddress, "User Agreement Document Missing", emailBody);

                //Create a null reference exception
                NullReferenceException missingDocException = new NullReferenceException(errorMessage);

                //Log the exception
                LogException(missingDocException);

                //Return null
                return null;
            }
        }

        /// <summary>
        /// This method generates the NCPMI Excel File Report and fills it with values from the database.
        /// </summary>
        /// <param name="programFKs">The program FKs to filter the report</param>
        /// <param name="schoolYear">The school year DateTime to filter the report</param>
        /// <returns>A byte array representation of the Excel file</returns>
        public static byte[] GenerateNCPMIExcelFile(List<int> programFKs, DateTime schoolYear, bool viewPrivateChildInfo, bool viewPrivateEmployeeInfo)
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
                        programInfoWorksheet.Cells[2, 2].Value = ("August " + schoolYear.ToString("yyyy") + " - July " + schoolYear.AddYears(1).ToString("yyyy"));

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
                                childrenToAdd.Append((viewPrivateChildInfo ? childAndBIR.FirstName + " " + childAndBIR.LastName + "," : "HIDDEN,"));
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
            CLASSROOM_COACH = 4,
            TPOT_OBSERVER = 5,
            TPITOS_OBSERVER = 6,
            ADMINISTRATOR = 7,
            BEHAVIOR_SPECIALIST = 8,
            DATA_COLLECTOR = 9,
            SUPPORT_ADMIN_STAFF = 10,
            LEADERSHIP_COACH = 11,
            PHYSICAL_THERAPIST = 12,
            SPEECH_THERAPIST = 13,
            OCCUPATIONAL_THERAPIST = 14,
            SOCIAL_WORKER = 15,
            CLASSROOM_AIDE = 16,
            SPECIAL_ED_ITINERANT_TEACHER = 17,
            DISABILITIES_COORDINATOR = 18,
            EDUCATION_COORDINATOR = 19
        }

        /// <summary>
        /// This contains the Training FKs from the database.
        /// </summary>
        public enum TrainingFKs
        {
            PRACTICE_BASED_COACHING = 1,
            INTRODUCTION_TO_COACHING = 2,
            TPOT_OBSERVER = 3,
            TPITOS_OBSERVER = 4,
            PRESCHOOL_MODULE_1 = 5,
            PRESCHOOL_MODULE_2 = 6,
            PRESCHOOL_MODULE_3 = 7,
            INFANT_TODDLER_MODULE_1 = 8,
            INFANT_TODDLER_MODULE_2 = 9,
            INFANT_TODDLER_MODULE_3 = 10,
            FCC_MODULE_1 = 11,
            FCC_MODULE_2 = 12,
            POSITIVE_SOLUTIONS = 13,
            PARENTS_INTERACTING = 14,
            PRESCHOOL_MODULE_4 = 15,
            PRACTICE_BASED_COACHING_FCC = 16,
            PRESCHOOL_MODULE_3A = 17,
            PRESCHOOL_MODULE_3B = 18,
            ROADMAP_TO_IMPLEMENTATION_1 = 19,
            ROADMAP_TO_IMPLEMENTATION_CB_2 = 20,
            ROADMAP_TO_IMPLEMENTATION_CB_3 = 21,
            ROADMAP_TO_IMPLEMENTATION_CB_4 = 22,
            ROADMAP_TO_IMPLEMENTATION_FCC_2 = 23,
            ROADMAP_TO_IMPLEMENTATION_FCC_3 = 24,
            ROADMAP_TO_IMPLEMENTATION_FCC_4 = 25
        }

        /// <summary>
        /// This contains the State FKs from the database
        /// </summary>
        public enum StateFKs
        {
            NEW_YORK = 1,
            NATIONAL = 2,
            WISCONSIN = 3,
            TENNESSEE = 4,
            MASSACHUSETTS = 5,
            IOWA = 6,
            WASHINGTON = 7,
            ILLINOIS = 8,
            NEW_HAMPSHIRE = 9,
            EXAMPLE = 10,  //This is an example state for states that want to preview PIDS on the test site.  Nobody should be able to log into this state on the live site.
            VERMONT = 11,
            ARIZONA = 12, 
            ARKANSAS = 13, 
            CALIFORNIA = 14, 
            COLORADO = 15, 
            CONNECTICUT = 16, 
            DELAWARE = 17, 
            DISTRICT_OF_COLUMBIA = 18, 
            FLORIDA = 19, 
            GEORGIA = 20, 
            GUAM = 21, 
            HAWAII = 22, 
            IDAHO = 23, 
            INDIANA = 24, 
            KANSAS = 25, 
            KENTUCKY = 26, 
            LOUISIANA = 27, 
            MAINE = 28, 
            MARYLAND = 29, 
            MICHIGAN = 30, 
            MINNESOTA = 31, 
            MISSISSIPPI = 32, 
            MISSOURI = 33, 
            MONTANA = 34, 
            NEBRASKA = 35, 
            NEVADA = 36, 
            NEW_JERSEY = 37, 
            NEW_MEXICO = 38, 
            NORTH_CAROLINA = 39, 
            NORTH_DAKOTA = 40, 
            OHIO = 41, 
            OKLAHOMA = 42, 
            OREGON = 43, 
            PENNSYLVANIA = 44, 
            PUERTO_RICO = 45, 
            RHODE_ISLAND = 46, 
            SOUTH_CAROLINA = 47,
            SOUTH_DAKOTA = 48, 
            TEXAS = 49, 
            UTAH = 50, 
            ALABAMA = 51, 
            VIRGIN_ISLANDS = 52, 
            WEST_VIRGINIA = 53, 
            WYOMING = 54,
            ALASKA = 55,
            WASHINGTON_DCYF = 56
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
        public int? CodeProgramRoleFK { get; set; }
        public string RoleName { get; set; }
        public bool? ViewPrivateChildInfo { get; set; }
        public bool? ViewPrivateEmployeeInfo { get; set; }
        public bool? IsProgramLocked { get; set; }
        public int? CurrentHubFK { get; set; }
        public string HubName { get; set; }
        public int? CurrentStateFK { get; set; }
        public string StateName { get; set; }
        public string StateLogoFileName { get; set; }
        public string StateThumbnailLogoFileName { get; set; }
        public int? StateHomePageLogoOption { get; set; }
        public string StateCatchphrase { get; set; }
        public string StateDisclaimer { get; set; }
        public List<int> StateFKs { get; set; }
        public List<int> CohortFKs { get; set; }
        public List<int> HubFKs { get; set; }

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
            CodeProgramRoleFK = null;
            RoleName = null;
            ViewPrivateChildInfo = null;
            ViewPrivateEmployeeInfo = null;
            IsProgramLocked = null;
            CurrentHubFK = null;
            HubName = null;
            CurrentStateFK = null;
            StateName = null;
            StateLogoFileName = null;
            StateThumbnailLogoFileName = null;
            StateHomePageLogoOption = null;
            StateCatchphrase = null;
            StateDisclaimer = null;
            StateFKs = null;
            CohortFKs = null;
            HubFKs = null;
        }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="programRolePK">The CodeProgramRolePK</param>
        /// <param name="programFKs">A list of the programs that the current user can access data for</param>
        /// <param name="currentProgramFK">The Program's key</param>
        /// <param name="programName">The Program's name</param>
        /// <param name="showBOQ">A boolean that says whether or not to show the BOQ page</param>
        /// <param name="showBOQFCC">A boolean that says whether or not to show the BOQFCC page</param>
        /// <param name="roleFK">The CodeProgramRole PK</param>
        /// <param name="roleName">The ProgramRole's name</param>
        /// <param name="showPrivateChildInfo">The ProgramRole's ShowPrivateChildInfo field</param>
        /// <param name="showPrivateEmployeeInfo">The ProgramRole's showPrivateEmployeeInfo field</param>
        /// <param name="isProgramLocked">A boolean that says whether or not the program is locked</param>
        /// <param name="hubFK">The Program's Hub's key</param>
        /// <param name="hubName">The Program's Hub's name</param>
        /// <param name="stateFK">The Program's State's key</param>
        /// <param name="stateName">The Program's State's name</param>
        /// <param name="stateLogoFileName">The state's logo filename</param>
        /// <param name="stateCatchphrase">The state's catchphrase</param>
        /// <param name="stateDisclaimer">The state's disclaimer</param>
        /// <param name="stateFKs">The state fks for the user</param>
        /// <param name="cohortFKs">The cohort fks for the user</param>
        /// <param name="hubFKs">The hub fks for the user</param>
        public ProgramAndRoleFromSession(
            List<int> programFKs, int? currentProgramFK, string programName,
            bool? showBOQ, bool? showBOQFCC,
            int? roleFK, string roleName, 
            bool? showPrivateChildInfo, bool? showPrivateEmployeeInfo, bool? isProgramLocked,
            int? hubFK, string hubName,
            int? stateFK, string stateName, 
            string stateLogoFileName, string stateThumbnailLogoFileName, int? stateHomePageLogoOption,
            string stateCatchphrase, string stateDisclaimer,
            List<int> stateFKs, List<int> cohortFKs, List<int> hubFKs)
        {
            ProgramFKs = programFKs;
            CurrentProgramFK = currentProgramFK;
            ProgramName = programName;
            ShowBOQ = showBOQ;
            ShowBOQFCC = showBOQFCC;
            CodeProgramRoleFK = roleFK;
            RoleName = roleName;
            ViewPrivateChildInfo = showPrivateChildInfo;
            ViewPrivateEmployeeInfo = showPrivateEmployeeInfo;
            IsProgramLocked = isProgramLocked;
            CurrentHubFK = hubFK;
            HubName = hubName;
            CurrentStateFK = stateFK;
            StateName = stateName;
            StateLogoFileName = stateLogoFileName;
            StateThumbnailLogoFileName = stateThumbnailLogoFileName;
            StateHomePageLogoOption = stateHomePageLogoOption;
            StateCatchphrase = stateCatchphrase;
            StateDisclaimer = stateDisclaimer;
            StateFKs = stateFKs;
            CohortFKs = cohortFKs;
            HubFKs = hubFKs;
        }
    }
}