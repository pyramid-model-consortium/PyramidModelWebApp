using Pyramid.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pyramid.Models;
using System.Data.Entity;

namespace Pyramid.Pages
{
    public partial class Help : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //To hold the state admin name
            string stateAdminName;

            //If the user is in NY, show the proper state admin name
            if(currentProgramRole.CurrentStateFK.Value == (int)Utilities.StateFKs.NEW_YORK)
            {
                stateAdminName = "the Council on Children and Families";

                lblTPOTTrainingFAQ.Text = string.Format("The observer must have the proper TPOT or TPITOS training entered into the observer's professional record by {0} or imported from ASPIRE.", stateAdminName);
                lblCoachingTrainingFAQ.Text = string.Format("The coach needs to have the proper coaching training entered into the coach's professional record by {0} or imported from ASPIRE.", stateAdminName);
            }
            else
            {
                stateAdminName = "state administration";

                lblTPOTTrainingFAQ.Text = string.Format("The observer must have the proper TPOT or TPITOS training entered into the observer's professional record by {0}.", stateAdminName);
                lblCoachingTrainingFAQ.Text = string.Format("The coach needs to have the Practice Based Coaching training entered into the coach's professional record by {0}.", stateAdminName);
            }

            //Set the hidden field
            hfStateAdminName.Value = stateAdminName;

            if(!IsPostBack)
            {
                //To hold the list of role permissions
                List<CodeProgramRolePermission> permissions = new List<CodeProgramRolePermission>();

                //Get the role permissions from the database
                using(PyramidContext context = new PyramidContext())
                {
                    permissions = context.CodeProgramRolePermission.AsNoTracking()
                                            .Include(cprp => cprp.CodeForm)
                                            .Include(cprp => cprp.CodeProgramRole)
                                            .Where(cprp => cprp.CodeForm.DisplayOnHelpPage == true && cprp.CodeProgramRole.DisplayOnHelpPage == true)
                                            .OrderByDescending(cprp => (cprp.CodeProgramRoleFK == currentProgramRole.CodeProgramRoleFK.Value ? 1 : 0))
                                            .ThenBy(cprp => cprp.CodeProgramRole.RoleName)
                                            .ThenBy(cprp => cprp.CodeForm.FormName)
                                            .ToList();
                }

                //Bind the role permission repeater
                repeatRolePermissions.DataSource = permissions;
                repeatRolePermissions.DataBind();
            }
        }
    }
}