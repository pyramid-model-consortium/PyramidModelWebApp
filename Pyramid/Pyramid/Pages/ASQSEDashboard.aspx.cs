using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;

namespace Pyramid.Pages
{
    public partial class ASQSEDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRASQSE.Columns.Count - 1);

                //Hide the action column
                bsGRASQSE.Columns[actionColumnIndex].Visible = false;
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
                        case "ASQSEAdded":
                            msgSys.ShowMessageToUser("success", "Success", "ASQ:SE screening successfully added!", 10000);
                            break;
                        case "ASQSEEdited":
                            msgSys.ShowMessageToUser("success", "Success", "ASQ:SE screening successfully edited!", 10000);
                            break;
                        case "ASQSECanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NOASQSE":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified ASQ:SE screening could not be found, please try again.", 15000);
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
        /// This method fills the ASQSEs by Score Type chart with data for the past year
        /// </summary>
        private void BindScoreTypeChart()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the score type data for the past year
                var scoreTypeDate = context.spGetASQSEsByScoreType(string.Join(",", currentProgramRole.ProgramFKs), 
                                        DateTime.Now.AddYears(-1), DateTime.Now)
                                        .ToList();

                //Bind the chart to the data
                chartScoreType.DataSource = scoreTypeDate;
                chartScoreType.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for an ASQSE
        /// and it deletes the ASQ:SE screening from the database
        /// </summary>
        /// <param name="sender">The lbDeleteASQSE LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteASQSE_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeASQSEPK = String.IsNullOrWhiteSpace(hfDeleteASQSEPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteASQSEPK.Value);

                if (removeASQSEPK.HasValue)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the ASQSE to remove
                        var ASQSEToRemove = context.ASQSE.Where(x => x.ASQSEPK == removeASQSEPK).FirstOrDefault();

                        //Remove the ASQSE from the database
                        context.ASQSE.Remove(ASQSEToRemove);
                        context.SaveChanges();

                        //Show a delete success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the ASQ:SE screening!", 1000);

                        //Bind the gridview
                        bsGRASQSE.DataBind();

                        //Bind the chart
                        BindScoreTypeChart();
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the ASQ:SE screening to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the data source for the ASQSE DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efASQSEDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efASQSEDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "ASQSEPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = from a in context.ASQSE.AsNoTracking()
                                    .Include(a => a.Program)
                                    .Include(a => a.Child)
                                    .Include(a => a.CodeASQSEInterval)
                                join cp in context.ChildProgram on a.ChildFK equals cp.ChildFK
                                join sa in context.ScoreASQSE on a.IntervalCodeFK equals sa.IntervalCodeFK
                                where currentProgramRole.ProgramFKs.Contains(a.ProgramFK)
                                  && cp.ProgramFK == a.ProgramFK
                                  && cp.EnrollmentDate <= a.FormDate
                                  && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= a.FormDate)
                                  && a.Version == sa.Version
                                select new
                                {
                                    a.ASQSEPK,
                                    a.FormDate,
                                    a.TotalScore,
                                    ChildIdAndName = "(" + cp.ProgramSpecificID + ") " + a.Child.FirstName + " " + a.Child.LastName,
                                    a.Program.ProgramName,
                                    Interval = a.CodeASQSEInterval.Description,
                                    ScoreType = (a.TotalScore > sa.CutoffScore ? "Above Cutoff" 
                                                    : a.TotalScore >= sa.MonitoringScoreStart && a.TotalScore <= sa.MonitoringScoreEnd ? "Monitor"
                                                    : a.TotalScore >= 0 && a.TotalScore < sa.MonitoringScoreStart ? "Well Below" 
                                                    : "Error!")
                                };
        }
    }
}