using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using DevExpress.CodeParser;
using DevExpress.XtraEditors.Filtering.Templates.DateTimeRange;

namespace Pyramid.Models
{
    public partial class StateSettings
    {
        /// <summary>
        /// This method returns the State Settings object for the state FK that is passed
        /// to this method.  If the State Settings row doesn't exist, this method creates
        /// a default row in the database.
        /// </summary>
        /// <param name="StateFK">The state's FK</param>
        /// <returns>The State Settings object</returns>
        public static StateSettings GetStateSettingsWithDefault(int stateFK)
        {
            //To hold the state settings
            StateSettings currentStateSettings;

            using(PyramidContext context = new PyramidContext())
            {
                //Get the state settings from the database
                currentStateSettings = context.StateSettings.AsNoTracking().Where(ss => ss.StateFK == stateFK).FirstOrDefault();

                //Check to see if the state settings row exists
                if(currentStateSettings == null || currentStateSettings.StateSettingsPK == 0)
                {
                    //Create and fill a new state settings object
                    currentStateSettings = new StateSettings();
                    currentStateSettings.Creator = "SYSTEM";
                    currentStateSettings.CreateDate = DateTime.Now;
                    currentStateSettings.Editor = null;
                    currentStateSettings.EditDate = null;
                    currentStateSettings.DueDatesEnabled = false;
                    currentStateSettings.DueDatesBeginDate = null;
                    currentStateSettings.DueDatesDaysUntilWarning = null;
                    currentStateSettings.DueDatesMonthsStart = null;
                    currentStateSettings.DueDatesMonthsEnd = null;
                    currentStateSettings.StateFK = stateFK;

                    //Add the state settings to the database
                    context.StateSettings.Add(currentStateSettings);

                    //Save the changes
                    context.SaveChanges();
                }
            }

            //Return the state settings
            return currentStateSettings;
        }
    }
}