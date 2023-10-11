using Pyramid.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Pyramid.Pages
{
    public partial class DownloadFile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //Get the current program role
                ProgramAndRoleFromSession currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

                //Get the file name
                string fileName = Request.QueryString["FileName"].ToString();

                //Check to see what file the user wants to download
                if (fileName == "NCPMIBIRExcelReport")
                {
                    //This is the BIR excel file report

                    //Get the parameters from the query string
                    string programFKsString = Request.QueryString["ProgramFKs"].ToString();
                    List<int> programFKs = programFKsString.Split(',').Select(int.Parse).ToList();
                    string schoolYearString = Request.QueryString["SchoolYear"].ToString();
                    DateTime schoolYear;

                    //Only continue if the school year is a valid year and the user is allowed
                    //to run the report for the programs
                    if (DateTime.TryParse(schoolYearString, out schoolYear)
                        && programFKs.Except(currentProgramRole.ProgramFKs).ToList().Count < 1)
                    {
                        //Generate the Excel file to a byte array
                        byte[] excel = Utilities.GenerateNCPMIExcelFile(programFKs, schoolYear, currentProgramRole.ViewPrivateChildInfo.Value, currentProgramRole.ViewPrivateEmployeeInfo.Value);

                        //Only continue if the byte array is not null
                        if (excel != null)
                        {
                            //Download the file to the user
                            Response.Clear();
                            Response.ContentType = "application/vnd.ms-excel.sheet.macroEnabled.12";
                            Response.AddHeader("content-disposition", string.Format("attachment;filename={0};", "BehaviorIncidentReport.xlsm"));
                            Response.BinaryWrite(excel);
                            Response.Flush();
                            Response.SuppressContent = true;
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log any exceptions
                Utilities.LogException(ex);
            }
        }
    }
}