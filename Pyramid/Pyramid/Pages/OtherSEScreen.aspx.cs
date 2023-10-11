using System;
using System.Linq;
using System.Web.UI.WebControls;
using Pyramid.Code;
using Pyramid.Models;
using System.Data.Entity;
using Pyramid.MasterPages;
using DevExpress.Web;

namespace Pyramid.Pages
{
    public partial class OtherSEScreen : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "OSES";
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
        private Models.OtherSEScreen currentOtherSEScreen;
        private int currentOtherSEScreenPK = 0;
        private int currentProgramFK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the OtherSEScreen PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["OtherSEScreenPK"]))
            {
                int.TryParse(Request.QueryString["OtherSEScreenPK"], out currentOtherSEScreenPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (currentOtherSEScreenPK == 0 && !string.IsNullOrWhiteSpace(hfOtherSEScreenPK.Value))
            {
                int.TryParse(hfOtherSEScreenPK.Value, out currentOtherSEScreenPK);
            }

            //Check to see if this is an edit
            isEdit = currentOtherSEScreenPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/OtherSEScreenDashboard.aspx?messageType=NotAuthorized");
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get the OtherSEScreen from the database
                currentOtherSEScreen = context.OtherSEScreen.AsNoTracking()
                                            .Include(ose => ose.Program)
                                            .Where(ose => ose.OtherSEScreenPK == currentOtherSEScreenPK).FirstOrDefault();

                //Check to see if the OtherSEScreen from the database exists
                if (currentOtherSEScreen == null)
                {
                    //The OtherSEScreen from the database doesn't exist, set the current OtherSEScreen to a default value
                    currentOtherSEScreen = new Models.OtherSEScreen();

                    //Set the program label to the current user's program
                    lblProgram.Text = currentProgramRole.ProgramName;
                }
                else
                {
                    //Set the program label to the form's program
                    lblProgram.Text = currentOtherSEScreen.Program.ProgramName;
                }
            }

            //Prevent users from viewing OtherSEScreens from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentOtherSEScreen.ProgramFK))
            {
                Response.Redirect(string.Format("/Pages/OtherSEScreenDashboard.aspx?messageType={0}", "NOOtherSEScreen"));
            }

            //Get the proper program fk
            currentProgramFK = (isEdit ? currentOtherSEScreen.ProgramFK : currentProgramRole.CurrentProgramFK.Value);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //Bind the dropdowns
                BindDropDowns();

                //Check to see if this is an edit
                if (isEdit)
                {
                    //This is an edit
                    //Populate the page
                    PopulatePage(currentOtherSEScreen);

                    //Update the child age
                    UpdateChildAge(currentOtherSEScreen.ChildFK, currentOtherSEScreen.ScreenDate);
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
                if (isEdit == false && action.ToLower() == "add" && FormPermissions.AllowedToAdd)
                {
                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Other Social Emotional Assessment";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Show certain controls
                    hfViewOnly.Value = "False";

                    //Enable page controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Other Social Emotional Assessment";
                }
                else
                {
                    //Hide certain controls
                    hfViewOnly.Value = "True";

                    //Disable page controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Other Social Emotional Assessment";
                }

                //Set the max date for the screen date field
                deScreenDate.MaxDate = DateTime.Now;

                //Set focus on the screen date field
                deScreenDate.Focus();
            }
        }

        #region Submit User Control Methods

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitOtherSEScreen user control 
        /// </summary>
        /// <param name="sender">The submitOtherSEScreen control</param>
        /// <param name="e">The Click event</param>
        protected void submitOtherSEScreen_Click(object sender, EventArgs e)
        {
            //To hold the success message type
            string successMessageType = SaveForm(true);

            //Only redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the OtherSEScreen dashboard
                Response.Redirect(string.Format("/Pages/OtherSEScreenDashboard.aspx?messageType={0}", successMessageType));
            }

        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitOtherSEScreen user control 
        /// </summary>
        /// <param name="sender">The submitOtherSEScreen control</param>
        /// <param name="e">The Click event</param>
        protected void submitOtherSEScreen_CancelClick(object sender, EventArgs e)
        {
            //Redirect the user to the OtherSEScreen dashboard
            Response.Redirect(string.Format("/Pages/OtherSEScreenDashboard.aspx?messageType={0}", "OtherSEScreenCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitOtherSEScreen control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitOtherSEScreen_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        #endregion

        #region Control Value/Index Change Methods

        /// <summary>
        /// This method fires when the value in the deIncidentDatetime DateEdit changes
        /// and it updates the child and classroom dropdowns
        /// </summary>
        /// <param name="sender">The deIncidentDatetime DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deScreenDate_ValueChanged(object sender, EventArgs e)
        {
            //Get the form date and child FK
            DateTime? screenDate = (deScreenDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deScreenDate.Value));
            int? childFK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.Value));

            //Bind the child dropdown
            BindChildDropDown(screenDate, currentProgramFK, childFK);
        }

        /// <summary>
        /// This method fires when the value in the ddChild ComboBox's selected index
        /// changes and it updates the age
        /// </summary>
        /// <param name="sender">The ddChild ComboBox</param>
        /// <param name="e">The SelectedIndexChanged event</param>
        protected void ddChild_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the child pk and form date
            int? childPK = (ddChild.Value == null ? (int?)null : Convert.ToInt32(ddChild.SelectedItem.Value));
            DateTime? screenDate = (deScreenDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deScreenDate.Value));

            if (childPK.HasValue && screenDate.HasValue)
            {
                //Update the child age
                UpdateChildAge(childPK.Value, screenDate.Value);
            }
        }

        #endregion

        #region Dropdown Binding Methods

        /// <summary>
        /// This method populates the dropdowns from the database
        /// </summary>
        private void BindDropDowns()
        {
            //Bind the child and interval dropdowns
            if (isEdit)
            {
                //If this is an edit, use the program fk from the behavior incident's classroom to filter
                BindChildDropDown(currentOtherSEScreen.ScreenDate, currentProgramFK, currentOtherSEScreen.ChildFK);
            }
            else
            {
                //If this is an add, use the program FKs array from the program role to filter
                BindChildDropDown((DateTime?)null, currentProgramFK, (int?)null);
            }

            using (PyramidContext context = new PyramidContext())
            {
                //Get all the screen types and bind them to the screen type dropdown
                var allScreenTypes = context.CodeScreenType.AsNoTracking().OrderBy(cst => cst.OrderBy).ToList();
                ddScreenType.DataSource = allScreenTypes;
                ddScreenType.DataBind();

                //Get all the score types and bind them to the score type dropdown
                var allScoreTypes = context.CodeScoreType.AsNoTracking().OrderBy(cst => cst.OrderBy).ToList();
                ddScoreType.DataSource = allScoreTypes;
                ddScoreType.DataBind();
            }
        }

        /// <summary>
        /// This method binds the child dropdown by getting all the children in
        /// the program that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="screenDate">The date and time to check against</param>
        /// <param name="programFK">The program FK</param>
        /// <param name="childFK">The child's FK to be selected</param>
        private void BindChildDropDown(DateTime? screenDate, int programFK, int? childFK)
        {
            if (screenDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the children in the program that were active as of the form date
                    var allChildren = from c in context.Child.Include(c => c.ChildProgram).AsNoTracking()
                                      join cp in context.ChildProgram on c.ChildPK equals cp.ChildFK
                                      where cp.ProgramFK == programFK
                                         && cp.EnrollmentDate <= screenDate.Value
                                        && (cp.DischargeDate.HasValue == false || cp.DischargeDate >= screenDate.Value)
                                      orderby cp.ProgramSpecificID ascending
                                      select new
                                      {
                                          c.ChildPK,
                                          IdAndName = (currentProgramRole.ViewPrivateChildInfo.Value ?
                                                "(" + cp.ProgramSpecificID + ") " + c.FirstName + " " + c.LastName :
                                                cp.ProgramSpecificID)

                                      };

                    //Bind the child dropdown to the list of children
                    ddChild.DataSource = allChildren.ToList();
                    ddChild.DataBind();
                }

                //Check to see how many children there are
                if (ddChild.Items.Count > 0)
                {
                    //There is at least 1 child, enable the child dropdown
                    ddChild.ReadOnly = false;

                    //Try to select the child passed to this method
                    ddChild.SelectedItem = ddChild.Items.FindByValue(childFK);
                }
                else
                {
                    //There are no kids in the list, disable the child dropdown
                    ddChild.ReadOnly = true;
                }
            }
            else
            {
                //No date was passed, clear and disable the child dropdown
                ddChild.Value = "";
                ddChild.ReadOnly = true;
            }
        }

        #endregion

        #region MISC Methods

        /// <summary>
        /// This method populates the page controls from the passed OtherSEScreen object
        /// </summary>
        /// <param name="OtherSEScreenToPopulate">The OtherSEScreen object with values to populate the page controls</param>
        private void PopulatePage(Models.OtherSEScreen OtherSEScreenToPopulate)
        {
            //Set the page controls to the values from the object
            deScreenDate.Value = OtherSEScreenToPopulate.ScreenDate;
            ddScreenType.SelectedItem = ddScreenType.Items.FindByValue(OtherSEScreenToPopulate.ScreenTypeCodeFK);
            ddChild.SelectedItem = ddChild.Items.FindByValue(OtherSEScreenToPopulate.ChildFK);
            txtScore.Value = OtherSEScreenToPopulate.Score;
            ddScoreType.SelectedItem = ddScoreType.Items.FindByValue(OtherSEScreenToPopulate.ScoreTypeCodeFK);
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            deScreenDate.ClientEnabled = enabled;
            ddScreenType.ClientEnabled = enabled;
            ddChild.ClientEnabled = enabled;
            txtScore.ClientEnabled = enabled;
            ddScoreType.ClientEnabled = enabled;

            //Show/hide the submit button
            submitOtherSEScreen.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitOtherSEScreen.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method updates the child age label
        /// </summary>
        /// <param name="childFK">The FK for the child</param>
        /// <param name="screenDate">The date of the OtherSEScreen</param>
        private void UpdateChildAge(int childFK, DateTime screenDate)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the child's DOB
                DateTime childDOB = context.Child.AsNoTracking()
                                        .Where(c => c.ChildPK == childFK)
                                        .FirstOrDefault()
                                        .BirthDate;

                //Calculate the child's age
                int age = Utilities.CalculateAgeDays(screenDate, childDOB);
                double ageMonths = (age / 30.417);

                //Display the child's age
                lblAge.Text = ageMonths.ToString("0.##") + " months old";
                lblAge.Visible = currentProgramRole.ViewPrivateChildInfo.Value;
            }
        }
        
        /// <summary>
        /// This method fires when the user clicks the print/download button
        /// and it displays the form as a report
        /// </summary>
        /// <param name="sender">The btnPrintPreview LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void btnPrintPreview_Click(object sender, EventArgs e)
        {
            //Make sure the validation succeeds
            if (ASPxEdit.AreEditorsValid(this.Page, submitOtherSEScreen.ValidationGroup))
            {
                //Submit the form
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptOtherSEScreen report = new Reports.PreBuiltReports.FormReports.RptOtherSEScreen();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "Other Social Emotional Assessment", currentOtherSEScreenPK);
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method populates and saves the form
        /// </summary>
        /// <param name="showMessages">Whether to show messages from the save</param>
        /// <returns>The success message type, null if the save failed</returns>
        private string SaveForm(bool showMessages)
        {
            //To hold the success message type
            string successMessageType = null;

            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the OtherSEScreen fields from the form
                currentOtherSEScreen.ScreenDate = deScreenDate.Date;
                currentOtherSEScreen.ScreenTypeCodeFK = Convert.ToInt32(ddScreenType.Value);
                currentOtherSEScreen.ChildFK = Convert.ToInt32(ddChild.Value);
                currentOtherSEScreen.Score = Convert.ToInt32(txtScore.Value);
                currentOtherSEScreen.ScoreTypeCodeFK = Convert.ToInt32(ddScoreType.Value);

                if (isEdit)
                {
                    //This is an edit
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "OtherSEScreenEdited";

                        //Set the edit-only fields
                        currentOtherSEScreen.Editor = User.Identity.Name;
                        currentOtherSEScreen.EditDate = DateTime.Now;

                        //Get the existing OtherSEScreen record
                        Models.OtherSEScreen existingASQ = context.OtherSEScreen.Find(currentOtherSEScreen.OtherSEScreenPK);

                        //Overwrite the existing OtherSEScreen record with the values from the form
                        context.Entry(existingASQ).CurrentValues.SetValues(currentOtherSEScreen);
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfOtherSEScreenPK.Value = currentOtherSEScreen.OtherSEScreenPK.ToString();
                        currentOtherSEScreenPK = currentOtherSEScreen.OtherSEScreenPK;
                    }
                }
                else
                {
                    //This is an add
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Set the success message
                        successMessageType = "OtherSEScreenAdded";

                        //Set the create-only fields
                        currentOtherSEScreen.Creator = User.Identity.Name;
                        currentOtherSEScreen.CreateDate = DateTime.Now;
                        currentOtherSEScreen.ProgramFK = currentProgramRole.CurrentProgramFK.Value;

                        //Add the OtherSEScreen to the database
                        context.OtherSEScreen.Add(currentOtherSEScreen);
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfOtherSEScreenPK.Value = currentOtherSEScreen.OtherSEScreenPK.ToString();
                        currentOtherSEScreenPK = currentOtherSEScreen.OtherSEScreenPK;
                    }
                }
            }
            else if(showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            //Return the success message type
            return successMessageType;
        }

        #endregion

        #region Custom Validation

        #endregion
    }
}