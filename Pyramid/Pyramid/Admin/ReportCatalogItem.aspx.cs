using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pyramid.Admin
{
    public partial class ReportCatalogItem : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private Models.ReportCatalog currentReportCatalog;
        private int currentReportCatalogPK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Only allow super admins
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
            {
                Response.Redirect("/Default.aspx");
            }

            //Get the CoachingLog PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["ReportCatalogPK"]))
            {
                int.TryParse(Request.QueryString["ReportCatalogPK"], out currentReportCatalogPK);
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the Report Catalog item from the database
                currentReportCatalog = context.ReportCatalog.AsNoTracking().Where(rc => rc.ReportCatalogPK == currentReportCatalogPK).FirstOrDefault();

                //Check to see if the Report Catalog item from the database exists
                if (currentReportCatalog == null)
                {
                    //The Report Catalog item from the database doesn't exist, set the current Report Catalog item to a default value
                    currentReportCatalog = new Models.ReportCatalog();
                }
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((LoggedIn)this.Master).HideTitle();

                //Bind the databound controls
                BindDataBoundControls();

                //Check to see if this is an edit or view
                if (currentReportCatalogPK > 0)
                {
                    //This is an edit or view
                    //Populate the page
                    PopulatePage(currentReportCatalog);
                }

                //Get the action from the query string
                string action;
                if (Request.QueryString["action"] != null)
                {
                    action = Request.QueryString["action"];
                }
                else
                {
                    action = "View";
                }

                //Allow adding/editing depending on the user's role and the action
                if (currentReportCatalog.ReportCatalogPK == 0)
                {
                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New Report Catalog Item";
                }
                else if (currentReportCatalog.ReportCatalogPK > 0 && action.ToLower() == "edit")
                {
                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit Report Catalog Item";
                }
                else
                {
                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View Report Catalog Item";
                }

                //Set the focus to the report name field
                txtReportName.Focus();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the report catalog item
        /// and it saves the information to the database
        /// </summary>
        /// <param name="sender">The submitReportCatalogItem submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitReportCatalogItem_Click(object sender, EventArgs e)
        {
            //To hold the success message type, file path, and file name
            string successMessageType = null;

            //Ensure user is allowed to edit
            if (currentProgramRole.CodeProgramRoleFK.Value == (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN)
            {
                //Get the relative file path
                string relativePath = "~/Reports/Documentation/" + txtDocumentationFileName.Value.ToString();

                //Set the field values
                currentReportCatalog.CriteriaOptions = tbCriteriaOptions.Text + ",";
                currentReportCatalog.CriteriaDefaults = tbCriteriaDefaults.Text + ",";
                currentReportCatalog.DocumentationLink = relativePath;
                currentReportCatalog.Keywords = tbKeywords.Text + ",";
                currentReportCatalog.OnlyExportAllowed = Convert.ToBoolean(ddOnlyExportAllowed.Value);
                currentReportCatalog.OptionalCriteriaOptions = tbOptionalCriteriaOptions.Text + ",";
                currentReportCatalog.ReportCategory = (ddReportCategory.Value.ToString().ToLower() == "other" ? txtReportCategorySpecify.Value.ToString() : ddReportCategory.Value.ToString());
                currentReportCatalog.ReportClass = txtReportClass.Value.ToString();
                currentReportCatalog.ReportDescription = txtReportDescription.Value.ToString();
                currentReportCatalog.ReportName = txtReportName.Value.ToString();
                currentReportCatalog.RolesAuthorizedToRun = tbRolesAuthorizedToRun.Value.ToString() + ",";

                //Check to see if this is an edit or add
                if (currentReportCatalog.ReportCatalogPK > 0)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "ReportCatalogItemEdited";

                        //Get the existing ReportCatalog record
                        Models.ReportCatalog existingReportCatalog = context.ReportCatalog.Find(currentReportCatalog.ReportCatalogPK);

                        //Overwrite the existing ReportCatalog record with the values from the form
                        context.Entry(existingReportCatalog).CurrentValues.SetValues(currentReportCatalog);
                        context.SaveChanges();
                    }

                    //Redirect the user to the report catalog maintenance page
                    Response.Redirect(string.Format("/Admin/ReportCatalogMaintenance.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "ReportCatalogItemAdded";

                        //Set the edit-only fields
                        currentReportCatalog.Creator = User.Identity.Name;
                        currentReportCatalog.CreateDate = DateTime.Now;

                        //Add the ReportCatalog record to the database
                        context.ReportCatalog.Add(currentReportCatalog);
                        context.SaveChanges();
                    }

                    //Redirect the user to the report catalog maintenance page
                    Response.Redirect(string.Format("/Admin/ReportCatalogMaintenance.aspx?messageType={0}", successMessageType));
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the report
        /// catalog item
        /// </summary>
        /// <param name="sender">The submitReportCatalogItem submit user control</param>
        /// <param name="e">The Click event</param>
        protected void submitReportCatalogItem_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the maintenance page
            Response.Redirect("/Admin/ReportCatalogMaintenance.aspx");
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitReportCatalogItem control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitReportCatalogItem_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        #region MISC Methods
        
        /// <summary>
        /// This method populates the data bound controls on the page
        /// </summary>
        private void BindDataBoundControls()
        {
            //To hold the descriptions of code options
            StringBuilder descriptions = new StringBuilder();

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the report categories from the existing reports
                var allReportCategories = context.ReportCatalog.AsNoTracking().OrderBy(rc => rc.ReportCategory).Select(rc => rc.ReportCategory).Distinct().ToList();

                //Add the other option to the categories
                allReportCategories.Add("Other");

                //Create the category data source with a named string
                var allReportCategoriesDataSource = allReportCategories.Select(c => new {
                    ReportCategory = c
                });

                //Bind the report category dropdown to the data source
                ddReportCategory.DataSource = allReportCategoriesDataSource;
                ddReportCategory.DataBind();

                //Get all the report criteria options
                var allCriteriaOptions = context.CodeReportCriteriaOption.AsNoTracking().OrderBy(crco => crco.OrderBy).ToList();

                //Bind the criteria options tag box
                tbCriteriaOptions.DataSource = allCriteriaOptions;
                tbCriteriaOptions.DataBind();

                //Only include the report criteria options that are allowed to be optional
                //in the optional criteria tag box
                tbOptionalCriteriaOptions.DataSource = allCriteriaOptions.Where(o => o.CanBeOptional == true).ToList();
                tbOptionalCriteriaOptions.DataBind();

                //Set the hidden field for the criteria details
                foreach(CodeReportCriteriaOption criteriaOption in allCriteriaOptions)
                {
                    //Get the descriptions of the criteria options
                    descriptions.Append("*sep*");
                    descriptions.Append(criteriaOption.Abbreviation);
                    descriptions.Append(" - ");
                    descriptions.Append(criteriaOption.Description);

                    //Set the hidden field to the descriptions
                    hfCriteriaDescriptions.Value = descriptions.ToString();
                }

                //Clear the descriptions StringBuilder so that it can be used below
                descriptions.Clear();
                
                //Get all the criteria defaults
                var allCriteriaDefaults = context.CodeReportCriteriaDefault.AsNoTracking().OrderBy(crcd => crcd.OrderBy).ToList();

                //Bind the criteria defaults tag box
                tbCriteriaDefaults.DataSource = allCriteriaDefaults;
                tbCriteriaDefaults.DataBind();

                //Set the hidden field for the criteria defaults
                foreach (CodeReportCriteriaDefault criteriaDefault in allCriteriaDefaults)
                {
                    //Get the descriptions of the criteria defaults
                    descriptions.Append("*sep*");
                    descriptions.Append(criteriaDefault.Abbreviation);
                    descriptions.Append(" - ");
                    descriptions.Append(criteriaDefault.Description);

                    //Set the hidden field to the descriptions
                    hfCriteriaDefaultDescriptions.Value = descriptions.ToString();
                }

                //Clear the descriptions StringBuilder
                descriptions.Clear();

                //Get all the program roles
                var allProgramRoles = context.CodeProgramRole.AsNoTracking().OrderBy(cpr => cpr.RoleName).ToList();

                //Bind the roles authorized tag box
                tbRolesAuthorizedToRun.DataSource = allProgramRoles;
                tbRolesAuthorizedToRun.DataBind();
            }
        }

        /// <summary>
        /// This method populates the page controls from the passed ReportCatalog object
        /// </summary>
        /// <param name="reportCatalog">The ReportCatalog object with values to populate the page controls</param>
        private void PopulatePage(Models.ReportCatalog reportCatalog)
        {
            //Set the page controls to the values from the object
            tbCriteriaOptions.Text = reportCatalog.CriteriaOptions;
            tbCriteriaDefaults.Text = reportCatalog.CriteriaDefaults;
            tbKeywords.Text = reportCatalog.Keywords;
            ddOnlyExportAllowed.SelectedItem = ddOnlyExportAllowed.Items.FindByValue(reportCatalog.OnlyExportAllowed);
            tbOptionalCriteriaOptions.Text = reportCatalog.OptionalCriteriaOptions;
            ddReportCategory.SelectedItem = ddReportCategory.Items.FindByValue(reportCatalog.ReportCategory);
            txtReportClass.Value = reportCatalog.ReportClass;
            txtReportDescription.Value = reportCatalog.ReportDescription;
            txtReportName.Value = reportCatalog.ReportName;
            tbRolesAuthorizedToRun.Value = reportCatalog.RolesAuthorizedToRun;
            txtDocumentationFileName.Value = Path.GetFileName(reportCatalog.DocumentationLink);

            //Check to see if there is existing documentation
            if (!string.IsNullOrWhiteSpace(reportCatalog.DocumentationLink))
            {
                //There is existing documentation, set the link's URL and show it
                lnkExistingDocumentation.NavigateUrl = string.Format("/Pages/ViewFile.aspx?ReportCatalogPK={0}", reportCatalog.ReportCatalogPK.ToString());
                lnkExistingDocumentation.Visible = true;
            }
            else
            {
                //There is no existing documentation, hide the link
                lnkExistingDocumentation.Visible = false;
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            tbCriteriaOptions.ClientEnabled = enabled;
            tbCriteriaDefaults.ClientEnabled = enabled;
            tbKeywords.ClientEnabled = enabled;
            ddOnlyExportAllowed.ClientEnabled = enabled;
            tbOptionalCriteriaOptions.ClientEnabled = enabled;
            ddReportCategory.ClientEnabled = enabled;
            txtReportClass.ClientEnabled = enabled;
            txtReportDescription.ClientEnabled = enabled;
            txtReportName.ClientEnabled = enabled;
            tbRolesAuthorizedToRun.ClientEnabled = enabled;
            txtDocumentationFileName.ClientEnabled = enabled;

            //Show/hide the submit button
            submitReportCatalogItem.ShowSubmitButton = enabled;

            //Use cancel confirmations if the controls are enabled and
            //the customization option for cancel confirmations is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitReportCatalogItem.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        #endregion

        /// <summary>
        /// This method fires when the validation for the txtReportCategorySpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtReportCategorySpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtReportCategorySpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified report category
            string reportCategorySpecify = (txtReportCategorySpecify.Value == null ? null : txtReportCategorySpecify.Value.ToString());

            //Perform validation
            if (ddReportCategory.SelectedItem != null && ddReportCategory.SelectedItem.Text.ToLower() == "other" && String.IsNullOrWhiteSpace(reportCategorySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Report Category is required when the 'Other' report category is selected!";
            }
            else
            {
                e.IsValid = true;
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtDocumentationFileName DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtDocumentationFileName TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtDocumentationFileName_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified file name
            string documentationFileName = (txtDocumentationFileName.Value == null ? null : txtDocumentationFileName.Value.ToString());

            //Make sure the file name exists
            if (string.IsNullOrWhiteSpace(documentationFileName))
            {
                e.IsValid = false;
                e.ErrorText = "Documentation File Name is required!";
            }
            else
            {
                //Get the relative file path
                string relativePath = "~/Reports/Documentation/" + documentationFileName;

                //Get the absolute file path
                string absolutePath = Server.MapPath(relativePath);

                //Make sure the file exists
                if (!File.Exists(absolutePath))
                {
                    e.IsValid = false;
                    e.ErrorText = "Documentation file was not found at: " + absolutePath + "!";
                }
                else
                {
                    e.IsValid = true;
                }
            }
        }
    }
}