using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;

namespace Pyramid.Pages
{
    public partial class OtherSEScreenDashboard : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Don't allow aggregate viewers to see the action column
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (bsGROtherSEScreen.Columns.Count - 1);

                //Hide the action column
                bsGROtherSEScreen.Columns[actionColumnIndex].Visible = false;
            }

            if (!IsPostBack)
            {
                //Set the view only value
                if (currentProgramRole.AllowedToEdit.Value)
                {
                    hfViewOnly.Value = "False";
                }
                else
                {
                    hfViewOnly.Value = "True";
                }

                //Populate the chart
                BindScoreTypeChart();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messageType))
                {

                    switch (messageType)
                    {
                        case "OtherSEScreenAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Social emotional screening successfully added!", 10000);
                            break;
                        case "OtherSEScreenEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Social emotional screening successfully edited!", 10000);
                            break;
                        case "OtherSEScreenCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOOtherSEScreen":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified social emotional screening could not be found, please try again.", 15000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// This method fills the Other Social-Emotional Screens by Score Type chart with data
        /// </summary>
        private void BindScoreTypeChart()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the score type data
                var scoreTypeDate = context.spGetOtherSEScreensByScoreType(string.Join(",", currentProgramRole.ProgramFKs), 
                                        DateTime.Now.AddYears(-1), DateTime.Now)
                                        .ToList();

                //Bind the chart to the data
                chartScoreType.DataSource = scoreTypeDate;
                chartScoreType.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for an OtherSEScreen
        /// and it deletes the OtherSEScreen screen from the database
        /// </summary>
        /// <param name="sender">The lbDeleteOtherSEScreen LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteOtherSEScreen_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeOtherSEScreenPK = String.IsNullOrWhiteSpace(hfDeleteOtherSEScreenPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteOtherSEScreenPK.Value);

                if (removeOtherSEScreenPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the OtherSEScreen to remove
                        var OtherSEScreenToRemove = context.OtherSEScreen.Where(x => x.OtherSEScreenPK == removeOtherSEScreenPK).FirstOrDefault();

                        //Remove the OtherSEScreen from the database
                        context.OtherSEScreen.Remove(OtherSEScreenToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the social emotional screening!", 1000);

                        //Bind the gridview
                        bsGROtherSEScreen.DataBind();

                        //Bind the chart
                        BindScoreTypeChart();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the social emotional screening to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the OtherSEScreen DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efOtherSEScreenDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efOtherSEScreenDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "OtherSEScreenPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = from ose in context.OtherSEScreen.AsNoTracking()
                                    .Include(ose => ose.Program)
                                    .Include(ose => ose.Child)
                                    .Include(ose => ose.CodeScoreType)
                                    .Include(ose => ose.CodeScreenType)
                                join cp in context.ChildProgram on ose.ChildFK equals cp.ChildFK
                                where currentProgramRole.ProgramFKs.Contains(ose.ProgramFK)
                                  && cp.ProgramFK == ose.ProgramFK
                                  && cp.EnrollmentDate <= ose.ScreenDate
                                  && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= ose.ScreenDate)
                                select new
                                {
                                    ose.OtherSEScreenPK,
                                    ScreenType = ose.CodeScreenType.Description,
                                    ose.ScreenDate,
                                    ose.Score,
                                    ChildIdAndName = "(" + cp.ProgramSpecificID + ") " + ose.Child.FirstName + " " + ose.Child.LastName,
                                    ose.Program.ProgramName,
                                    ScoreType = ose.CodeScoreType.Description
                                };
        }
    }
}