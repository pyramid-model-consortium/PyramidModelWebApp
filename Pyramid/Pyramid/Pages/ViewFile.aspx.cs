using Pyramid.Code;
using Pyramid.Models;
using System;
using System.IO;
using System.Linq;

namespace Pyramid.Pages
{
    public partial class ViewFile : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private UserFileUpload currentFile;
        private ReportCatalog currentReportCatalog;
        private int filePK = 0;
        private int reportCatalogPK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the file PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["UserFileUploadPK"]))
            {
                int.TryParse(Request.QueryString["UserFileUploadPK"], out filePK);
            }
            else if (!string.IsNullOrWhiteSpace(Request.QueryString["ReportCatalogPK"]))
            {
                int.TryParse(Request.QueryString["ReportCatalogPK"], out reportCatalogPK);
            }

            //Get the file information from the database
            if (filePK > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the file record
                    currentFile = context.UserFileUpload.AsNoTracking()
                                    .Where(ufu => ufu.UserFileUploadPK == filePK)
                                    .FirstOrDefault();

                    //Check to see if the file record exists
                    if (currentFile == null)
                    {
                        //The file record doesn't exist, set to a default
                        currentFile = new UserFileUpload();
                    }

                }
            }
            else
            {
                currentFile = new UserFileUpload();
            }

            if(reportCatalogPK > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the report catalog record
                    currentReportCatalog = context.ReportCatalog.AsNoTracking()
                                    .Where(rc => rc.ReportCatalogPK == reportCatalogPK)
                                    .FirstOrDefault();

                    //Check to see if the report catalog record exists
                    if (currentReportCatalog == null)
                    {
                        //The report catalog record doesn't exist, set to a default
                        currentReportCatalog = new ReportCatalog();
                    }
                }
            }
            else
            {
                currentReportCatalog = new ReportCatalog();
            }

            //Don't allow users to view files from other programs
            if (currentFile.UserFileUploadPK > 0)
            {
                if (currentFile.TypeCodeFK == (int)Utilities.FileTypeFKs.STATE_WIDE 
                        && currentProgramRole.StateFK.Value != currentFile.StateFK.Value)
                {
                    //This is a state-wide file and the user is not logged in under that state
                    lblMessage.Text = "No file found...";
                }
                else if (currentFile.TypeCodeFK == (int)Utilities.FileTypeFKs.HUB_WIDE && 
                            currentProgramRole.HubFK.Value != currentFile.HubFK.Value)
                {
                    //This is a hub-wide file and the user is not logged in under that hub
                    lblMessage.Text = "No file found...";
                }
                else if (currentFile.TypeCodeFK == (int)Utilities.FileTypeFKs.PROGRAM_WIDE 
                            && !currentProgramRole.ProgramFKs.Contains(currentFile.ProgramFK.Value))
                {
                    //This is a program-wide file and the user is not allowed to see that cohort
                    lblMessage.Text = "No file found...";
                }
                else if (currentFile.TypeCodeFK == (int)Utilities.FileTypeFKs.COHORT_WIDE
                            && !currentProgramRole.CohortFKs.Contains(currentFile.CohortFK.Value))
                {
                    //This is a cohort-wide file and the user is not allowed to see that cohort
                    lblMessage.Text = "No file found...";
                }
                else
                {
                    //Get the file URL from Azure storage
                    string fileLink = Utilities.GetFileLinkFromAzureStorage(currentFile.FileName,
                        currentFile.FileName.Contains(".pdf"),
                        Utilities.ConstantAzureStorageContainerName.UPLOADED_FILES.ToString());

                    //Redirect the user to the file link
                    Response.Redirect(fileLink);
                }
            }
            else if(!string.IsNullOrWhiteSpace(currentReportCatalog.DocumentationLink))
            {
                //Get the file path
                string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
                string filePath = currentReportCatalog.DocumentationLink.Replace("~", baseUrl);

                //Redirect the user to the file link
                Response.Redirect(filePath);
            }
            else
            {
                lblMessage.Text = "No file found...";
            }
        }
    }
}