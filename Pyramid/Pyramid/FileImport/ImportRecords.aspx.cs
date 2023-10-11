using CsvHelper;
using CsvHelper.Configuration;
using DevExpress.Web;
using Pyramid.Code;
using Pyramid.FileImport.CodeFiles;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pyramid.FileImport
{
    public partial class ImportRecords : System.Web.UI.Page
    {
        //To hold the necessary items for the page
        CodeProgramRolePermission currentPermissions;
        ProgramAndRoleFromSession currentProgramRole;
        string cacheKey, importType, returnURL;
        IImportable currentImportObject;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the import type
            if(!string.IsNullOrWhiteSpace(Request.QueryString["ImportRecordType"]))
            {
                importType = Request.QueryString["ImportRecordType"].ToString();
            }

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
            if (currentPermissions == null || currentPermissions.AllowedToAdd == false)
            {
                //Add a message to the queue to display after redirect
                msgSys.AddMessageToQueue("danger", "Access Denied", "You are not authorized to import those records!", 10000);

                //Redirect
                Response.Redirect(returnURL);
            }

            //Set the cache key
            cacheKey = string.Format("{0}-Upload{1}", User.Identity.Name, importType);

            //Get the import object
            currentImportObject = ImportHelpers.GetImportClassFromString(importType);

            //Make sure the import object exists
            if(currentImportObject == null)
            {
                //Log an exception
                Utilities.LogException(new NullReferenceException("The currentImportObject object was null!"));

                //Add a message to the queue to display after redirect
                msgSys.AddMessageToQueue("warning", "Error", "An error occurred while loading the page. Please try again and if this continues to fail, submit a support ticket.", 20000);

                //Redirect
                Response.Redirect(returnURL);
            }

            if (!Page.IsPostBack)
            {
                //Hide the master page title
                ((LoggedIn)this.Master).HideTitle();

                //Set the page title
                lblPageTitle.Text = string.Format("Import {0} Records", currentImportObject.DisplayName);

                //Bind the dropdowns
                BindDropDowns();

                //Set the hyperlink hrefs
                lnkImportFileTemplate.NavigateUrl = currentImportObject.GetImportTemplateFilePath();
                lnkImportFileExample.NavigateUrl = currentImportObject.GetImportExampleFilePath();

                //Set the instructions
                litImportInstructions.Text = currentImportObject.GetImportInstructionsHTML();

                //Set the field information table
                repeatFileDetails.DataSource = currentImportObject.GetFileFieldInformation();
                repeatFileDetails.DataBind();
            }
        }

        /// <summary>
        /// This method binds the dropdowns on the page to their data sources.
        /// </summary>
        private void BindDropDowns()
        {
            using(PyramidContext context = new PyramidContext())
            {
                //Bind the program dropdown
                List<Program> authorizedPrograms = context.Program.AsNoTracking().Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK)).ToList();
                ddProgram.DataSource = authorizedPrograms;
                ddProgram.DataBind();

                //Try to pre-select the currently logged in program
                ddProgram.SelectedItem = ddProgram.Items.FindByValue(currentProgramRole.CurrentProgramFK.Value);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the submit button in the submitFileImport Submit control.
        /// </summary>
        /// <param name="sender">The submitFileImport Submit control</param>
        /// <param name="e">The click event</param>
        protected void submitFileImport_SubmitClick(object sender, EventArgs e)
        {
            //Get the file to upload
            UploadedFile file = bucUploadFile.UploadedFiles[0];

            //Ensure the file is valid
            if (file.ContentLength > 0 && file.IsValid)
            {
                bool importSucceeded = true;

                try
                {
                    //Open the file (make sure to use the Trim option)
                    using (var reader = new StreamReader(file.FileContent))
                    {
                        using (var csvFile = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            TrimOptions = TrimOptions.Trim
                        }))
                        {
                            //Map the file to the object
                            currentImportObject.RegisterClassMapWithReader(csvFile);

                            using (PyramidContext context = new PyramidContext())
                            {
                                //Get the file results
                                var records = currentImportObject.GetResultsFromReader(csvFile, context, Convert.ToInt32(ddProgram.Value));

                                //Call the ToList function to ensure that all results are in memory
                                var parsedRecords = records.ToList();

                                //Add the records to the cache
                                Cache.Insert(cacheKey, parsedRecords, null, DateTime.Now.AddHours(4), TimeSpan.Zero);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log the exception
                    Utilities.LogException(ex);

                    //Import failed
                    importSucceeded = false;

                    //Remove any cached items
                    Cache.Remove(cacheKey);

                    //Show an error message
                    msgSys.ShowMessageToUser("danger", "Invalid file", "Something went wrong while importing the file, please ensure that your file is valid and try again!", 20000);
                }

                //Only redirect if success
                if (importSucceeded)
                {
                    //Add a message to the queue to display after redirect
                    msgSys.AddMessageToQueue("success", "Import Succeeded", "Records successfully imported!  A preview of the import results should display on this page.", 10000);

                    //Redirect to the confirmation page
                    string redirectURL = string.Format("/FileImport/ConfirmImport.aspx?ImportRecordType={0}&CacheKey={1}&ProgramFK={2}&ReturnURL={3}", importType, cacheKey, Convert.ToInt32(ddProgram.Value), returnURL);
                    Response.Redirect(redirectURL);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Invalid file", "The file was missing, too large, or was an incorrect type!", 20000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the submitFileImport Submit control.
        /// </summary>
        /// <param name="sender">The submitFileImport Submit control</param>
        /// <param name="e">The click event</param>
        protected void submitFileImport_CancelClick(object sender, EventArgs e)
        {
            //Check the return URL and redirect
            if (!string.IsNullOrWhiteSpace(returnURL))
            {
                Response.Redirect(returnURL);
            }
            else
            {
                Response.Redirect("/Default.aspx");
            }
        }

        /// <summary>
        /// This method fires when validation fails for the submitFileImport Submit control.
        /// </summary>
        /// <param name="sender">The submitFileImport Submit control</param>
        /// <param name="e"></param>
        protected void submitFileImport_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }
    }
}