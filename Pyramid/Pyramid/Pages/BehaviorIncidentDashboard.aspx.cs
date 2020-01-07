using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Collections.Generic;
using DevExpress.Web;

namespace Pyramid.Pages
{
    public partial class BehaviorIncidentDashboard : System.Web.UI.Page
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
                int actionColumnIndex = (bsGRBehaviorIncidents.Columns.Count - 1);

                //Hide the action column
                bsGRBehaviorIncidents.Columns[actionColumnIndex].Visible = false;
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

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!String.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "BehaviorIncidentAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Behavior Incident Report successfully added!", 10000);
                            break;
                        case "BehaviorIncidentEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Behavior Incident Report successfully edited!", 10000);
                            break;
                        case "BehaviorIncidentCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoBehaviorIncident":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Behavior Incident Report could not be found, please try again.", 15000);
                            break;
                        case "NotAuthorized":
                            msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized to view that information!", 10000);
                            break;
                        default:
                            break;
                    }
                }

                //Bind the problem behavior chart
                BindProblemBehaviorChart();

                //Bind the data bound controls
                BindDataBoundControls();
            }
        }

        /// <summary>
        /// This method fills the problem behavior chart with data from the database
        /// </summary>
        private void BindProblemBehaviorChart()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the chart data for the past year
                var behaviorIncidentsByProblemBehavior =
                    context.spGetBehaviorIncidentCountByProblemBehavior(string.Join(",", currentProgramRole.ProgramFKs), 
                    DateTime.Now.AddYears(-1), DateTime.Now)
                    .ToList();

                //Bind the chart to the data
                chartIncidentsByProblemBehavior.DataSource = behaviorIncidentsByProblemBehavior;
                chartIncidentsByProblemBehavior.DataBind();
            }
        }

        /// <summary>
        /// This method populates the data bound controls on the page
        /// </summary>
        private void BindDataBoundControls()
        {
            //Get information from the database
            using (PyramidContext context = new PyramidContext())
            {
                //--------------- Program List Box -----------------
                //Get all the programs for this user
                List<Program> allPrograms = context.Program.AsNoTracking()
                                                .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                                .ToList();

                //Bind the program list box
                lstBxProgram.DataSource = allPrograms;
                lstBxProgram.DataBind();

                //Set the default item for the program list box
                lstBxProgram.SelectedItem = lstBxProgram.Items.FindByValue(currentProgramRole.CurrentProgramFK.Value);
            }

            //---------------- School Year dropdown -----------------
            //To hold the last 10 years (inclusive of this year)
            List<int> schoolYears = new List<int>();

            //Get the current year and add it to the list
            int currentYear = DateTime.Now.Year;
            schoolYears.Add(currentYear);

            //Get the previous 9 years and add them to the list
            for (int i = 1; i <= 10; i++)
            {
                schoolYears.Add(currentYear - i);
            }

            //Bind the school year dropdown
            ddSchoolYear.DataSource = schoolYears;
            ddSchoolYear.DataBind();
        }

        /// <summary>
        /// This method fires when the data source for the Behavior Incident DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efBehaviorIncidentDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efBehaviorIncidentDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the key expression
            e.KeyExpression = "BehaviorIncidentPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = from bi in context.BehaviorIncident.AsNoTracking()
                                    .Include(bi => bi.Child)
                                    .Include(bi => bi.Classroom)
                                    .Include(bi => bi.Classroom.Program)
                                    .Include(bi => bi.CodeProblemBehavior)
                                join cp in context.ChildProgram on bi.ChildFK equals cp.ChildFK
                                where currentProgramRole.ProgramFKs.Contains(bi.Classroom.ProgramFK)
                                  && cp.ProgramFK == bi.Classroom.ProgramFK
                                  && cp.EnrollmentDate <= bi.IncidentDatetime
                                  && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= bi.IncidentDatetime)
                                select new
                                {
                                    bi.BehaviorIncidentPK,
                                    bi.Creator,
                                    bi.CreateDate,
                                    bi.IncidentDatetime,
                                    ChildName = "(" + cp.ProgramSpecificID + ") " + bi.Child.FirstName + " " + bi.Child.LastName,
                                    ClassroomName = "(" + bi.Classroom.ProgramSpecificID + ") " + bi.Classroom.Name,
                                    ProblemBehavior = bi.CodeProblemBehavior.Description,
                                    bi.Classroom.Program.ProgramName
                                };
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a Behavior Incident
        /// and it deletes the Behavior Incident information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteBehaviorIncident LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteBehaviorIncident_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //Get the PK from the hidden field
                int? removeBehaviorIncidentPK = (String.IsNullOrWhiteSpace(hfDeleteBehaviorIncidentPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteBehaviorIncidentPK.Value));

                //Remove the role if the PK is not null
                if (removeBehaviorIncidentPK != null)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the Behavior Incident program row to remove
                            Models.BehaviorIncident behaviorIncidentToRemove = context.BehaviorIncident.Find(removeBehaviorIncidentPK);

                            //Remove the Behavior Incident
                            context.BehaviorIncident.Remove(behaviorIncidentToRemove);
                            context.SaveChanges();

                            //Show a success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted Behavior Incident Report!", 10000);
                        }
                    }
                    catch (DbUpdateException dbUpdateEx)
                    {
                        //Check if it is a foreign key error
                        if (dbUpdateEx.InnerException?.InnerException is SqlException)
                        {
                            //If it is a foreign key error, display a custom message
                            SqlException sqlEx = (SqlException)dbUpdateEx.InnerException.InnerException;
                            if (sqlEx.Number == 547)
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "Could not delete the Behavior Incident Report, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Behavior Incident Report!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Behavior Incident Report!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }

                    //Rebind the Behavior Incident controls
                    bsGRBehaviorIncidents.DataBind();

                    //Bind the chart
                    BindProblemBehaviorChart();
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Behavior Incident Report to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method validates the NCPMI Excel report criteria and sets the visibility and 
        /// navigation url for the download link for that file
        /// </summary>
        /// <param name="sender">The submitGenerateDownloadLink submit control</param>
        /// <param name="e">The Click event args</param>
        protected void submitGenerateDownloadLink_Click(object sender, EventArgs e)
        {
            //To hold the selected program info
            List<int> selectedProgramPKs = new List<int>();

            //Get the selected programs
            foreach (ListEditItem program in lstBxProgram.SelectedItems)
            {
                //Record the selected program info
                selectedProgramPKs.Add(Convert.ToInt32(program.Value));
            }

            //Get the chosen school year integer
            int schoolYear = Convert.ToInt32(ddSchoolYear.Value);

            //Get the school year DateTime
            DateTime schoolYearDate = Convert.ToDateTime("01/01/" + schoolYear.ToString());

            //Create the URL for the download
            string downloadURL = string.Format("/Pages/DownloadFile.aspx?FileName={0}&ProgramFKs={1}&SchoolYear={2}",
                                "NCPMIBIRExcelReport",
                                string.Join(",", selectedProgramPKs),
                                schoolYearDate.ToString("MM/dd/yyyy"));

            //Set the download link url and make it visible
            lnkDownloadExcel.NavigateUrl = downloadURL;
            lnkDownloadExcel.Visible = true;

            //Set the text of the submit control from Generate to Re-generate
            submitGenerateDownloadLink.SubmitButtonText = "Re-generate Download Link";
            submitGenerateDownloadLink.UpdateProperties();
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitGenerateDownloadLink submit control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitGenerateDownloadLink_ValidationFailed(object sender, EventArgs e)
        {
            //Validation failed, hide the download link
            lnkDownloadExcel.Visible = false;

            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }
    }
}