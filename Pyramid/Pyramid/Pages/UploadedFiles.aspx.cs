using Pyramid.Models;
using System;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Pyramid.Code;
using System.Linq;
using System.Configuration;
using DevExpress.Web;
using System.IO;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace Pyramid.Pages
{
    public partial class UserFileUploads : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "ULF";
            }
        }

        public CodeProgramRolePermission FormPermissions
        {
            get
            {
                return currentPermissions;
            }
            set
            {
                currentPermissions = value;
            }
        }

        private CodeProgramRolePermission currentPermissions;
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Init(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            //Check to see if the user can view this page
            if (FormPermissions.AllowedToViewDashboard == false)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Get the necessary values from the program role object
            string programFKs = string.Join(",", currentProgramRole.ProgramFKs);
            string hubFKs = string.Join(",", currentProgramRole.HubFKs);
            string cohortFKs = string.Join(",", currentProgramRole.CohortFKs);
            string stateFKs = string.Join(",", currentProgramRole.StateFKs);

            //-------- This page uses a SqlDataSource configured in both the .aspx file and this file to populate the gridview ---------
            //Set the values for the sql data source
            sqlUserFileUploadDataSource.ConnectionString = ConfigurationManager.ConnectionStrings["Pyramid"].ConnectionString;
            sqlUserFileUploadDataSource.SelectParameters["ProgramFKs"].DefaultValue = programFKs;
            sqlUserFileUploadDataSource.SelectParameters["HubFKs"].DefaultValue = hubFKs;
            sqlUserFileUploadDataSource.SelectParameters["CohortFKs"].DefaultValue = cohortFKs;
            sqlUserFileUploadDataSource.SelectParameters["StateFKs"].DefaultValue = stateFKs;
            sqlUserFileUploadDataSource.SelectParameters["RoleFK"].DefaultValue = currentProgramRole.CodeProgramRoleFK.Value.ToString();
            sqlUserFileUploadDataSource.SelectParameters["Username"].DefaultValue = User.Identity.Name;

            if (!IsPostBack)
            {
                //Set the view only value
                if (FormPermissions.AllowedToAdd == false && FormPermissions.AllowedToEdit == false)
                {
                    hfViewOnly.Value = "True";

                    //Hide the submit button
                    submitFileUpload.ShowSubmitButton = false;

                    //Don't use cancel confirmations
                    submitFileUpload.UseCancelConfirm = false;
                }
                else
                {
                    hfViewOnly.Value = "False";

                    //Show the submit button
                    submitFileUpload.ShowSubmitButton = true;

                    //Use cancel confirmation if the customization option for cancel confirmation is true (default to true)
                    bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
                    bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true

                    submitFileUpload.UseCancelConfirm = areConfirmationsEnabled;
                }

                //Bind the dropdowns
                BindDropDowns();

                //Check for messages in the query string
                string messageType = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!String.IsNullOrWhiteSpace(messageType))
                {
                    switch (messageType)
                    {
                        case "UploadSuccess":
                            msgSys.ShowMessageToUser("success", "Success", "File successfully uploaded!", 10000);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// This method populates the dropdowns on the page from the database
        /// </summary>
        private void BindDropDowns()
        {
            //Bind the dropdowns
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the file types
                List<CodeFileUploadType> allFileTypes = context.CodeFileUploadType.AsNoTracking()
                                    .OrderBy(cfut => cfut.OrderBy)
                                    .ToList();

                //Get all the authorized file types
                List<CodeFileUploadType> filteredFileTypes = allFileTypes.Where(aft => aft.RolesAuthorizedToModify.Split(',').ToList().Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString())).ToList();

                //Bind the file type dropdown
                ddFileType.DataSource = filteredFileTypes;
                ddFileType.DataBind();

                //Get all the programs
                var allPrograms = context.Program.AsNoTracking()
                                    .Include(p => p.Hub)
                                    .Include(p => p.State)
                                    .Include(p => p.Cohort)
                                    .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK))
                                    .OrderBy(p => p.ProgramName)
                                    .ToList();

                //Bind the program dropdown
                ddProgram.DataSource = allPrograms.Select(p => new {
                    p.ProgramPK,
                    p.ProgramName
                });
                ddProgram.DataBind();

                //Bind the hub dropdown
                ddHub.DataSource = allPrograms.Select(p => new {
                    p.Hub.HubPK,
                    p.Hub.Name
                }).Distinct().OrderBy(h => h.Name);
                ddHub.DataBind();

                //Bind the state dropdown
                ddState.DataSource = allPrograms.Select(p => new {
                    p.State.StatePK,
                    p.State.Name
                }).Distinct().OrderBy(s => s.Name);
                ddState.DataBind();

                //Bind the cohort dropdown
                ddCohort.DataSource = allPrograms.Select(p => new {
                    p.Cohort.CohortPK,
                    p.Cohort.CohortName
                }).Distinct().OrderBy(c => c.CohortName);
                ddCohort.DataBind();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Upload File button and it opens
        /// a div that allows the user to upload a file
        /// </summary>
        /// <param name="sender">The lbNewFile LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbNewFile_Click(object sender, EventArgs e)
        {
            //Show the div
            divUploadFile.Visible = true;

            //Clear the inputs
            txtFileDescription.Value = "";
            ddFileType.Value = "";
            ddProgram.Value = "";
            ddHub.Value = "";
            ddState.Value = "";
            ddCohort.Value = "";

            //Set focus to the file type dropdown
            ddFileType.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a UserFileUpload
        /// and it deletes the UserFileUpload information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteUserFileUpload LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteUserFileUpload_Click(object sender, EventArgs e)
        {
            if (FormPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeUserFileUploadPK = (string.IsNullOrWhiteSpace(hfDeleteUserFileUploadPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteUserFileUploadPK.Value));

                //Remove the role if the PK is not null
                if (removeUserFileUploadPK != null)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the UserFileUpload program row to remove
                            UserFileUpload fileToRemove = context.UserFileUpload
                                                                    .Include(ufu => ufu.CodeFileUploadType)
                                                                    .Where(ufu => ufu.UserFileUploadPK == removeUserFileUploadPK)
                                                                    .FirstOrDefault();

                            //Get the roles authorized to delete
                            List<string> rolesAuthorized = fileToRemove.CodeFileUploadType.RolesAuthorizedToModify.Split(',').ToList();

                            //Ensure the user is allowed to delete this file
                            if (!rolesAuthorized.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()))
                            {
                                msgSys.ShowMessageToUser("danger", "Delete Failed", "You are not authorized to delete this file!", 10000);
                            }
                            else
                            {

                                //Remove the file from Azure storage
                                Utilities.DeleteFileFromAzureStorage(fileToRemove.FileName,
                                                Utilities.ConstantAzureStorageContainerName.UPLOADED_FILES.ToString());

                                //Remove the file record in the database
                                context.UserFileUpload.Remove(fileToRemove);

                                //Save the deletion to the database
                                context.SaveChanges();

                                //Get the delete change row and set the deleter
                                context.UserFileUploadChanged
                                        .OrderByDescending(ufuc => ufuc.UserFileUploadChangedPK)
                                        .Where(ufuc => ufuc.UserFileUploadPK == fileToRemove.UserFileUploadPK)
                                        .FirstOrDefault().Deleter = User.Identity.Name;

                                //Save the delete change row to the database
                                context.SaveChanges();

                                //Show a success message
                                msgSys.ShowMessageToUser("success", "Delete Succeeded", "Successfully deleted file!", 10000);
                            }
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
                                msgSys.ShowMessageToUser("danger", "Error", "Could not delete the file, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the file!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the file!", 120000);
                        }

                        //Log the error
                        Utilities.LogException(dbUpdateEx);
                    }

                    //Rebind the UserFileUpload controls
                    bsGRUserFileUploads.DataBind();
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the file to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the  UserFileUploads
        /// and it saves the UserFileUpload information to the database
        /// </summary>
        /// <param name="sender">The submitUserFileUpload submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitFileUpload_Click(object sender, EventArgs e)
        {
            //Allow editors and hub data viewers to add files
            if (FormPermissions.AllowedToAdd)
            {
                //Get the file to upload
                UploadedFile file = bucUploadFile.UploadedFiles[0];

                if (file.ContentLength > 0 && file.IsValid)
                {
                    //Get the actual file name
                    string actualFileName = Path.GetFileNameWithoutExtension(file.FileName) + "-" +
                        Path.GetRandomFileName().Substring(0, 6) +
                        Path.GetExtension(file.FileName);

                    //Get the display file name
                    string displayFileName = Path.GetFileNameWithoutExtension(file.FileName);

                    //Get the file type
                    string fileExtension = Path.GetExtension(file.FileName).ToLower();
                    string fileType;
                    switch (fileExtension)
                    {
                        case ".pdf":
                            fileType = "pdf";
                            break;
                        case ".doc":
                        case ".docx":
                            fileType = "word";
                            break;
                        case ".ppt":
                        case ".pptx":
                            fileType = "powerpoint";
                            break;
                        case ".xls":
                        case ".xlsx":
                        case ".xlsm":
                            fileType = "excel";
                            break;
                        case ".jpeg":
                        case ".jpg":
                        case ".png":
                            fileType = "image";
                            break;
                        default:
                            fileType = "alt";
                            break;
                    }

                    //Upload the file to Azure storage
                    string filePath = Utilities.UploadFileToAzureStorage(file.FileBytes, actualFileName,
                                        Utilities.ConstantAzureStorageContainerName.UPLOADED_FILES.ToString());

                    if (!String.IsNullOrWhiteSpace(filePath))
                    {
                        PyramidUser currentUser = null;
                        // Validate the user password
                        using (var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>())
                        {
                            //Try to get the user
                            currentUser = manager.FindByName(User.Identity.Name);
                        }

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Create the database object for the file
                            UserFileUpload currentUserFileUpload = new UserFileUpload();
                            currentUserFileUpload.CreateDate = DateTime.Now;
                            currentUserFileUpload.Creator = User.Identity.Name;
                            currentUserFileUpload.UploadedBy = currentUser.FirstName + " " + currentUser.LastName;
                            currentUserFileUpload.Description = txtFileDescription.Value.ToString();
                            currentUserFileUpload.FileType = fileType;
                            currentUserFileUpload.DisplayFileName = displayFileName;
                            currentUserFileUpload.FileName = actualFileName;
                            currentUserFileUpload.FilePath = filePath;
                            currentUserFileUpload.TypeCodeFK = Convert.ToInt32(ddFileType.Value);

                            //Set the proper FKs
                            if (currentUserFileUpload.TypeCodeFK == (int)Utilities.FileTypeFKs.PROGRAM_WIDE)
                            {
                                currentUserFileUpload.ProgramFK = Convert.ToInt32(ddProgram.Value);
                                currentUserFileUpload.HubFK = null;
                                currentUserFileUpload.StateFK = null;
                                currentUserFileUpload.CohortFK = null;
                            }
                            else if (currentUserFileUpload.TypeCodeFK == (int)Utilities.FileTypeFKs.HUB_WIDE)
                            {
                                currentUserFileUpload.ProgramFK = null;
                                currentUserFileUpload.HubFK = Convert.ToInt32(ddHub.Value);
                                currentUserFileUpload.StateFK = null;
                                currentUserFileUpload.CohortFK = null;
                            }
                            else if (currentUserFileUpload.TypeCodeFK == (int)Utilities.FileTypeFKs.STATE_WIDE)
                            {
                                currentUserFileUpload.ProgramFK = null;
                                currentUserFileUpload.HubFK = null;
                                currentUserFileUpload.StateFK = Convert.ToInt32(ddState.Value);
                                currentUserFileUpload.CohortFK = null;
                            }
                            else if (currentUserFileUpload.TypeCodeFK == (int)Utilities.FileTypeFKs.COHORT_WIDE)
                            {
                                currentUserFileUpload.ProgramFK = null;
                                currentUserFileUpload.HubFK = null;
                                currentUserFileUpload.StateFK = null;
                                currentUserFileUpload.CohortFK = Convert.ToInt32(ddCohort.Value);
                            }

                            //Save to the database
                            context.UserFileUpload.Add(currentUserFileUpload);
                            context.SaveChanges();

                            //Redirect the user back to this page with a message
                            Response.Redirect("/Pages/UploadedFiles.aspx?messageType=UploadSuccess");
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Upload Failed", "The file failed to upload properly, please try again.", 10000);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "No valid file was selected to be uploaded!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the UserFileUploads
        /// and it closes the UserFileUpload add/edit div
        /// </summary>
        /// <param name="sender">The submitUserFileUpload submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitFileUpload_CancelClick(object sender, EventArgs e)
        {
            divUploadFile.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitFileUpload control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitFileUpload_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes for each row created in the bsGRUserFileUploads BootstrapGridView
        /// and it sets the visibility of the delete button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bsGRUserFileUploads_HtmlRowCreated(object sender, ASPxGridViewTableRowEventArgs e)
        {
            //Only work on data rows
            if (e.RowType == DevExpress.Web.GridViewRowType.Data)
            {
                //Get the necessary controls
                LinkButton lbDeleteFile = (LinkButton)bsGRUserFileUploads.FindRowCellTemplateControl(e.VisibleIndex, (GridViewDataColumn)bsGRUserFileUploads.Columns["ActionColumn"], "lbDeleteFile");
                
                //Get the authorized roles for the file
                string strAuthorizedRoles = Convert.ToString(e.GetValue("RolesAuthorizedToModify"));
                List<string> authorizedRoles = strAuthorizedRoles.Split(',').ToList();

                //Show/hide the delete button
                if (FormPermissions.AllowedToDelete &&
                    authorizedRoles.Contains(currentProgramRole.CodeProgramRoleFK.Value.ToString()))
                {
                    lbDeleteFile.Visible = true;
                }
                else
                {
                    lbDeleteFile.Visible = false;
                }
            }
        }

        #region Custom Validation

        /// <summary>
        /// This method validates the file type options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void FileTypeOption_Validation(object sender, DevExpress.Web.ValidationEventArgs e)
        {
            //Get the entry type
            int? fileType = (ddFileType.Value == null ? (int?)null : Convert.ToInt32(ddFileType.Value));

            //Perform validation
            if (fileType.HasValue)
            {
                if (fileType.Value == (int)Utilities.FileTypeFKs.PROGRAM_WIDE)
                {
                    int? programFK = (ddProgram.Value == null ? (int?)null : Convert.ToInt32(ddProgram.Value));

                    if (programFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
                else if (fileType.Value == (int)Utilities.FileTypeFKs.HUB_WIDE)
                {
                    int? hubFK = (ddHub.Value == null ? (int?)null : Convert.ToInt32(ddHub.Value));

                    if (hubFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
                else if (fileType.Value == (int)Utilities.FileTypeFKs.STATE_WIDE)
                {
                    int? stateFK = (ddState.Value == null ? (int?)null : Convert.ToInt32(ddState.Value));

                    if (stateFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
                else if (fileType.Value == (int)Utilities.FileTypeFKs.COHORT_WIDE)
                {
                    int? cohortFK = (ddCohort.Value == null ? (int?)null : Convert.ToInt32(ddCohort.Value));

                    if (cohortFK.HasValue == false)
                    {
                        e.IsValid = false;
                        e.ErrorText = "Required!";
                    }
                }
            }
        }

        #endregion
    }
}