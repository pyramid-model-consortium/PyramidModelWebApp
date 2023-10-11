using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using Pyramid.Code;

namespace Pyramid.Models
{
    public partial class PyramidUser
    {
        /// <summary>
        /// This method returns the user record for the related to the
        /// passed username.
        /// </summary>
        /// <param name="username">The username for the user record</param>
        /// <returns>A PyramidUser object representing the user</returns>
        public static PyramidUser GetUserRecordByUsername(string username)
        {
            //To hold the necessary values
            PyramidUser userRecordToReturn;

            //Get the user record to return
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                userRecordToReturn = context.Users.AsNoTracking().Where(u => u.UserName == username).FirstOrDefault();
            }

            //Return the user record
            return userRecordToReturn;
        }

        /// <summary>
        /// This method returns program-level Leadership Coach user records.
        /// </summary>
        /// <param name="programFKs">Only return leadership coach users if they are in one of these programs</param>
        /// <param name="usernameToInclude">(Optional) The username of a user to manually include even if they aren't a coach</param>
        /// <returns>A list of PyramidUser records that have at least one Leadership Coach role.</returns>
        public static List<PyramidUser> GetProgramLeadershipCoachUserRecords(List<int> programFKs, string usernameToInclude = null)
        {
            //To hold the necessary values
            List<string> currentUsernames;
            List<PyramidUser> userRecordsToReturn;

            using (PyramidContext context = new PyramidContext())
            {
                //Get the roles to include
                List<int> rolesToInclude = new List<int>()
                {
                    (int)Utilities.CodeProgramRoleFKs.LEADERSHIP_COACH,
                    (int)Utilities.CodeProgramRoleFKs.PROGRAM_IMPLEMENTATION_COACH
                };

                //Other users that are allowed on this page see all their state's users
                currentUsernames = context.UserProgramRole.AsNoTracking()
                                            .Where(upr => rolesToInclude.Contains(upr.ProgramRoleCodeFK)
                                                && programFKs.Contains(upr.ProgramFK))
                                            .Select(u => u.Username).Distinct().ToList();

                if (!string.IsNullOrWhiteSpace(usernameToInclude) && currentUsernames.Contains(usernameToInclude) == false)
                {
                    currentUsernames.Add(usernameToInclude);
                }
            }

            //Get the user records to return
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                userRecordsToReturn = context.Users.AsNoTracking().Where(u => currentUsernames.Contains(u.UserName)).ToList();
            }

            //Return the user records
            return userRecordsToReturn;
        }

        /// <summary>
        /// This method returns hub-level Leadership Coach user records.
        /// </summary>
        /// <param name="hubFKs">Only return leadership coach users if they are in one of these hubs</param>
        /// <param name="usernameToInclude">(Optional) The username of a user to manually include even if they aren't a coach</param>
        /// <returns>A list of PyramidUser records that have at least one Leadership Coach role.</returns>
        public static List<PyramidUser> GetHubLeadershipCoachUserRecords(List<int> hubFKs, string usernameToInclude = null)
        {
            //To hold the necessary values
            List<string> currentUsernames;
            List<PyramidUser> userRecordsToReturn;

            using (PyramidContext context = new PyramidContext())
            {
                //Get the roles to include
                List<int> rolesToInclude = new List<int>()
                {
                    (int)Utilities.CodeProgramRoleFKs.HUB_LEADERSHIP_COACH
                };

                //Other users that are allowed on this page see all their state's users
                currentUsernames = context.UserProgramRole.Include(upr => upr.Program).AsNoTracking()
                                            .Where(upr => rolesToInclude.Contains(upr.ProgramRoleCodeFK)
                                                && hubFKs.Contains(upr.Program.HubFK))
                                            .Select(u => u.Username).Distinct().ToList();

                if (!string.IsNullOrWhiteSpace(usernameToInclude) && currentUsernames.Contains(usernameToInclude) == false)
                {
                    currentUsernames.Add(usernameToInclude);
                }
            }

            //Get the user records to return
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                userRecordsToReturn = context.Users.AsNoTracking().Where(u => currentUsernames.Contains(u.UserName)).ToList();
            }

            //Return the user records
            return userRecordsToReturn;
        }

        /// <summary>
        /// This method returns Master Cadre user records.
        /// </summary>
        /// <param name="currentProgramRole">The current user's role information.</param>
        /// <param name="currentUsername">The current user's username.</param>
        /// <returns>A list of PyramidUser records that have at least one Master Cadre role.</returns>
        public static List<PyramidUser> GetMasterCadreUserRecords(ProgramAndRoleFromSession currentProgramRole, string currentUsername)
        {
            //To hold the necessary values
            List<string> currentUsernames;
            List<PyramidUser> userRecordsToReturn;

            using (PyramidContext context = new PyramidContext())
            {
                //Check the user's role
                if (currentProgramRole.CodeProgramRoleFK == (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER)
                {
                    //Master cadre members only see their forms
                    currentUsernames = new List<string>() { currentUsername };
                }
                else
                {
                    //Get the roles to include
                    List<int> rolesToInclude = new List<int>()
                    {
                        (int)Utilities.CodeProgramRoleFKs.MASTER_CADRE_MEMBER
                    };

                    //Other users that are allowed on this page see all their state's users
                    currentUsernames = context.UserProgramRole.Include(upr => upr.Program).AsNoTracking()
                                                .Where(upr => rolesToInclude.Contains(upr.ProgramRoleCodeFK)
                                                    && currentProgramRole.StateFKs.Contains(upr.Program.StateFK))
                                                .Select(upr => upr.Username).Distinct().ToList();
                }
            }

            //Get the user records to return
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                userRecordsToReturn = context.Users.AsNoTracking().Where(u => currentUsernames.Contains(u.UserName)).ToList();
            }

            //Return the user records
            return userRecordsToReturn;
        }
    }
}