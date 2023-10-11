using Pyramid.Code;
using Pyramid.FileImport.CodeFiles;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pyramid.FileImport
{
    public partial class ConfirmImport : System.Web.UI.Page
    {
        //To hold the necessary items for the page
        CodeProgramRolePermission currentPermissions;
        ProgramAndRoleFromSession currentProgramRole;
        string importType, cacheKey, returnURL;
        int currentProgramFK;
        IImportable currentImportObject;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Set the ScriptManager timeout
            ScriptManager currentScriptManager = ScriptManager.GetCurrent(this);
            currentScriptManager.AsyncPostBackTimeout = 360;

            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the import type
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ImportRecordType"]))
            {
                importType = Request.QueryString["ImportRecordType"].ToString();
            }

            //Get the cache key
            if (!string.IsNullOrWhiteSpace(Request.QueryString["CacheKey"]))
            {
                cacheKey = Request.QueryString["CacheKey"].ToString();
            }

            //Get the program FK
            int.TryParse(Request.QueryString["ProgramFK"], out currentProgramFK);

            //Get the return URL for the original page
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ReturnURL"]))
            {
                returnURL = Request.QueryString["ReturnURL"].ToString();
            }
            else
            {
                returnURL = "/Default.aspx";
            }

            //Get the permission object
            currentPermissions = Utilities.GetProgramRolePermissionsFromDatabase(importType, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Make sure the user is authorized
            if(currentPermissions == null || currentPermissions.AllowedToAdd == false)
            {
                //Add a message to the queue to display after redirect
                msgSys.AddMessageToQueue("danger", "Access Denied", "You are not authorized to import those records!", 10000);

                //Redirect
                Response.Redirect(returnURL);
            }

            //Get the import object
            currentImportObject = ImportHelpers.GetImportClassFromString(importType);

            //Get the file results from the cache
            IEnumerable<object> uploadedFileResults = (IEnumerable<object>)Cache.Get(cacheKey);

            //Make sure the file results exist
            if(uploadedFileResults == null || currentImportObject == null)
            {
                //Log an exception
                Utilities.LogException(new NullReferenceException("Either the currentFileUpload or currentImportObject object was null!"));

                //Add a message to the queue to display after redirect
                msgSys.AddMessageToQueue("warning", "Error", "An error occurred while retrieving the imported records, please try again.  If this continues to fail, please submit a support ticket.", 10000);

                //Redirect
                Response.Redirect(returnURL);
            }

            //Display the results
            bsGRUploadPreview.DataSource = uploadedFileResults.ToList();
            bsGRUploadPreview.DataBind();

            //Clear any filter expression that may have held over from exporting invalid records
            bsGRUploadPreview.FilterExpression = null;

            //Hide any gridview columns that are not useful for the user
            HideColumnsByFieldNames(currentImportObject.GetFieldsToHideFromPreview());

            if(!Page.IsPostBack)
            {
                //Hide the master page title
                ((LoggedIn)this.Master).HideTitle();

                //Set the page title
                lblPageTitle.Text = string.Format("Import {0} Records", currentImportObject.DisplayName);

                //Set the instructions
                litConfirmationInstructions.Text = currentImportObject.GetConfirmationInstructionsHTML();

                using(PyramidContext context = new PyramidContext())
                {
                    //Get the program
                    Program currentProgram = context.Program.AsNoTracking().Where(p => p.ProgramPK == currentProgramFK).FirstOrDefault();

                    //Set the label
                    if(currentProgram != null)
                    {
                        lblProgram.Text = currentProgram.ProgramName;
                    }
                    else
                    {
                        divProgram.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// This method hides certain columns that are not useful to the user based
        /// on the passed list of field names
        /// </summary>
        /// <param name="fieldsToHide">A list of the field names to hide.</param>
        private void HideColumnsByFieldNames(List<string> fieldsToHide)
        {
            //Loop through the fields to hide
            foreach(string columnName in fieldsToHide)
            {
                //Get the column
                var columnToHide = bsGRUploadPreview.Columns[columnName];

                //Hide the column if it exists
                if (columnToHide != null)
                {
                    columnToHide.Visible = false;
                }
            }
        }

        /// <summary>
        /// This method fires when the user clicks the submit button in the submitConfirmImport control.
        /// </summary>
        /// <param name="sender">The submitConfirmImport Submit control</param>
        /// <param name="e">The click event</param>
        protected void submitConfirmImport_SubmitClick(object sender, EventArgs e)
        {
            //Get the items to import
            List<IImportable> currentFileUpload = (List<IImportable>)Cache.Get(cacheKey);

            //Get the number of valid items
            int numValidObjects = currentFileUpload.Where(o => o.IsValid).Count();

            //Save the items to the database
            using (PyramidContext context = new PyramidContext())
            {
                currentImportObject.SaveRangeToDatabase(context, currentFileUpload, User.Identity.Name);
            }

            if(numValidObjects > 0)
            {
                //Add a message to the queue to display after redirect
                msgSys.AddMessageToQueue("success", "Records Saved", string.Format("Successfully saved {0} valid record(s) to the database!", numValidObjects), 10000);
            }
            else
            {
                //Add a message to the queue to display after redirect
                msgSys.AddMessageToQueue("warning", "Nothing was Saved", "There were no valid records, so nothing was saved.", 10000);
            }

            //Removed the cached data
            Cache.Remove(cacheKey);

            //Redirect
            Response.Redirect(returnURL);
        }

        /// <summary>
        /// This method fires when the user clicks the Export Invalid Records button.  It exports the invalid records
        /// to Excel.
        /// </summary>
        /// <param name="sender">The btnExportInvalidRecords Bootstrap button</param>
        /// <param name="e">The click event</param>
        protected void btnExportInvalidRecords_Click(object sender, EventArgs e)
        {
            try
            {
                //Filter the gridview to only include invalid cases
                bsGRUploadPreview.FilterExpression = "[IsValid] = false";

                //Set the export file name
                bsGRUploadPreview.SettingsExport.FileName = string.Format("Invalid {0} Records", currentImportObject.DisplayName);

                //Export the file
                bsGRUploadPreview.ExportXlsxToResponse();
            }
            catch(Exception ex)
            {
                //Log the exception
                Utilities.LogException(ex);

                //Set the success message now since export call ends the response
                msgSys.ShowMessageToUser("danger", "Export Failed", "Something went wrong while attempting to export the invalid records!  Please try again, and if it continues to fail, please submit a support ticket.", 20000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the submitConfirmImport Submit control.
        /// </summary>
        /// <param name="sender">The submitConfirmImport Submit control</param>
        /// <param name="e">The click event</param>
        protected void submitConfirmImport_CancelClick(object sender, EventArgs e)
        {
            //Removed the cached data
            Cache.Remove(cacheKey);

            //Redirect back to the import page.
            Response.Redirect(string.Format("/FileImport/ImportRecords.aspx?ImportRecordType={0}&ReturnURL={1}", importType, returnURL));
        }

        /// <summary>
        /// This method fires when validation fails for the submitConfirmImport Submit control.
        /// </summary>
        /// <param name="sender">The submitConfirmImport Submit control</param>
        /// <param name="e"></param>
        protected void submitConfirmImport_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }
    }
}