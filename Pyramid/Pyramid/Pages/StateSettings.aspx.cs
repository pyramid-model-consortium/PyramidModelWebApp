using Pyramid.Code;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using DevExpress.CodeParser;
using DevExpress.XtraRichEdit.Import.Html;

namespace Pyramid.Pages
{
    public partial class StateSettings : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private string strDueDateYear = "/2019";  //No need to change this, it is irrelevant since we only want month/day

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Check the user's role
            if (currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.SUPER_ADMIN &&
                currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.APPLICATION_ADMIN &&
                currentProgramRole.CodeProgramRoleFK.Value != (int)Utilities.CodeProgramRoleFKs.STATE_DATA_ADMIN)
            {
                //Kick out any non-admins
                Response.Redirect("/Default.aspx");
            }

            if (!IsPostBack)
            {
                //Bind the data bound controls
                BindDataBoundControls();

                //Bind the due date settings
                BindDueDateSettings();
            }
        }

        /// <summary>
        /// This method binds all the data bound controls on the page
        /// </summary>
        private void BindDataBoundControls()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the forms
                List<CodeForm> forms = context.CodeForm.AsNoTracking()
                                                .Where(cf => cf.AllowDueDate == true)
                                                .OrderBy(cf => cf.OrderBy)
                                                .ToList();

                //Bind the form combo box
                ddDueDateForm.DataSource = forms;
                ddDueDateForm.DataBind();
            }
        }

        #region Due Date Settings

        /// <summary>
        /// This method fills the due date settings fields
        /// </summary>
        private void BindDueDateSettings()
        {
            //Get the state settings
            Models.StateSettings stateSettings = Models.StateSettings.GetStateSettingsWithDefault(currentProgramRole.CurrentStateFK.Value);

            //Fill the controls
            ddDueDatesEnabled.Value = stateSettings.DueDatesEnabled;
            deDueDatesBeginDate.Value = stateSettings.DueDatesBeginDate;
            txtDueDatesMonthsStart.Value = (stateSettings.DueDatesMonthsStart.HasValue ? -stateSettings.DueDatesMonthsStart.Value : (decimal?)null); //Change back to a positive number
            txtDueDatesMonthsEnd.Value = stateSettings.DueDatesMonthsEnd;
            txtDueDatesDaysUntilWarning.Value = stateSettings.DueDatesDaysUntilWarning;
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the due date
        /// and it saves the due date information to the database
        /// </summary>
        /// <param name="sender">The submitDueDate control</param>
        /// <param name="e">The Click event</param>
        protected void submitDueDateSettings_SubmitClick(object sender, EventArgs e)
        {
            //Get the due date settings values
            bool dueDatesEnabled = Convert.ToBoolean(ddDueDatesEnabled.Value);
            DateTime? dueDatesBeginDate = (deDueDatesBeginDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDueDatesBeginDate.Value));
            decimal? dueDatesMonthsStart = (txtDueDatesMonthsStart.Value == null ? (decimal?)null : Convert.ToDecimal(txtDueDatesMonthsStart.Value));
            decimal? dueDatesMonthsEnd = (txtDueDatesMonthsEnd.Value == null ? (decimal?)null : Convert.ToDecimal(txtDueDatesMonthsEnd.Value));
            int? dueDatesDaysUntilWarning = (txtDueDatesDaysUntilWarning.Value == null ? (int?)null : Convert.ToInt32(txtDueDatesDaysUntilWarning.Value));

            using (PyramidContext context = new PyramidContext())
            {
                //Get the state settings row
                Models.StateSettings currentStateSettings = context.StateSettings.Where(ss => ss.StateFK == currentProgramRole.CurrentStateFK.Value).FirstOrDefault();

                //Fill the row
                currentStateSettings.DueDatesEnabled = dueDatesEnabled;
                currentStateSettings.DueDatesBeginDate = dueDatesBeginDate;
                currentStateSettings.DueDatesMonthsStart = -dueDatesMonthsStart; //Needs to be negative, but show as positive
                currentStateSettings.DueDatesMonthsEnd = dueDatesMonthsEnd;
                currentStateSettings.DueDatesDaysUntilWarning = dueDatesDaysUntilWarning;

                //Set the creator and editor
                if(currentStateSettings.StateSettingsPK > 0)
                {
                    //This is an edit, set the edit fields
                    currentStateSettings.Editor = User.Identity.Name;
                    currentStateSettings.EditDate = DateTime.Now;
                }
                else
                {
                    //This is an add, set the create fields
                    currentStateSettings.Creator = User.Identity.Name;
                    currentStateSettings.CreateDate = DateTime.Now;
                }

                //Save the changes
                context.SaveChanges();
            }

            //Show a success message
            msgSys.ShowMessageToUser("success", "Settings Saved", "Due date settings have been saved and are now in effect!", 5000);
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitDueDateSettings control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitDueDateSettings_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when validation fires for the deDueDatesBeginDate date edit
        /// </summary>
        /// <param name="sender">The deDueDatesBeginDate BootstrapDateEdit</param>
        /// <param name="e">The validation event</param>
        protected void deDueDatesBeginDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the due dates enabled value and begin date
            bool? dueDatesEnabled = (ddDueDatesEnabled.Value == null ? (bool?)null : Convert.ToBoolean(ddDueDatesEnabled.Value));
            DateTime? dueDateBeginDate = (deDueDatesBeginDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDueDatesBeginDate.Value));

            //Check to see if due dates are enabled
            if(dueDatesEnabled.HasValue && dueDatesEnabled.Value)
            {
                //Check to see if the begin date has value
                if(dueDateBeginDate.HasValue == false)
                {
                    //Set the validation status and message
                    e.IsValid = false;
                    e.ErrorText = "Required when due dates are shown!";
                }
            }
        }

        /// <summary>
        /// This method executes when validation fires for the txtDueDatesMonthsStart text box
        /// </summary>
        /// <param name="sender">The txtDueDatesMonthsStart TextBox</param>
        /// <param name="e">The validation event</param>
        protected void txtDueDatesMonthsStart_Validation(object sender, ValidationEventArgs e)
        {
            //Get the due dates enabled value and months start
            bool? dueDatesEnabled = (ddDueDatesEnabled.Value == null ? (bool?)null : Convert.ToBoolean(ddDueDatesEnabled.Value));
            decimal dueDateMonthsStart;

            //Check to see if due dates are enabled
            if (dueDatesEnabled.HasValue && dueDatesEnabled.Value)
            {
                //Check to see if the months start is valid
                if (!decimal.TryParse(txtDueDatesMonthsStart.Text, out dueDateMonthsStart))
                {
                    //Set the validation status and message
                    e.IsValid = false;
                    e.ErrorText = "Required when due dates are shown!";
                }
            }
        }

        /// <summary>
        /// This method executes when validation fires for the txtDueDatesMonthsEnd text box
        /// </summary>
        /// <param name="sender">The txtDueDatesMonthsEnd TextBox</param>
        /// <param name="e">The validation event</param>
        protected void txtDueDatesMonthsEnd_Validation(object sender, ValidationEventArgs e)
        {
            //Get the due dates enabled value and months start
            bool? dueDatesEnabled = (ddDueDatesEnabled.Value == null ? (bool?)null : Convert.ToBoolean(ddDueDatesEnabled.Value));
            decimal dueDatesMonthsEnd;

            //Check to see if due dates are enabled
            if (dueDatesEnabled.HasValue && dueDatesEnabled.Value)
            {
                //Check to see if the months start is valid
                if (!decimal.TryParse(txtDueDatesMonthsEnd.Text, out dueDatesMonthsEnd))
                {
                    //Set the validation status and message
                    e.IsValid = false;
                    e.ErrorText = "Required when due dates are shown!";
                }
            }
        }

        /// <summary>
        /// This method executes when validation fires for the txtDueDatesDaysUntilWarning text box
        /// </summary>
        /// <param name="sender">The txtDueDatesDaysUntilWarning TextBox</param>
        /// <param name="e">The validation event</param>
        protected void txtDueDatesDaysUntilWarning_Validation(object sender, ValidationEventArgs e)
        {
            //Get the due dates enabled value and months start
            bool? dueDatesEnabled = (ddDueDatesEnabled.Value == null ? (bool?)null : Convert.ToBoolean(ddDueDatesEnabled.Value));
            int dueDatesDaysUntilWarning;

            //Check to see if due dates are enabled
            if (dueDatesEnabled.HasValue && dueDatesEnabled.Value)
            {
                //Check to see if the months start is valid
                if (!int.TryParse(txtDueDatesDaysUntilWarning.Text, out dueDatesDaysUntilWarning))
                {
                    //Set the validation status and message
                    e.IsValid = false;
                    e.ErrorText = "Required when due dates are shown!";
                }
            }
        }

        #endregion

        #region Due Dates

        /// <summary>
        /// This method fires when the linq data source for the form due dates 
        /// gridview is selecting and it sets the results to a EF query
        /// </summary>
        /// <param name="sender">The linqDueDateDataSource LinqDataSource</param>
        /// <param name="e">The LinqDataSourceSelectEventArgs event</param>
        protected void linqDueDateDataSource_Selecting(object sender, LinqDataSourceSelectEventArgs e)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the due dates from the database
                var formDueDates = context.FormDueDate.AsNoTracking()
                            .Include(fdd => fdd.CodeForm)
                            .Include(fdd => fdd.State)
                            .Where(fdd => currentProgramRole.StateFKs.Contains(fdd.StateFK))
                            .Select(fdd => new
                            {
                                fdd.FormDueDatePK,
                                DueEndDate = DbFunctions.AddDays(fdd.DueRecommendedDate, fdd.DueEndWindow),
                                fdd.DueRecommendedDate,
                                DueStartDate = DbFunctions.AddDays(fdd.DueRecommendedDate, fdd.DueStartWindow),
                                fdd.HelpText,
                                fdd.CodeFormFK,
                                fdd.CodeForm.FormAbbreviation,
                                fdd.CodeForm.FormName,
                                fdd.StateFK,
                                StateName = fdd.State.Name,
                                StateAbbreviation = fdd.State.Abbreviation
                            })
                            .ToList();

                //Set the data source result to the due dates
                e.Result = formDueDates;
            }
        }

        /// <summary>
        /// This method fires when the user clicks the lbAddDueDate LinkButton and
        /// it opens the add/edit section for due dates so that the user can add a
        /// new form due date.
        /// </summary>
        /// <param name="sender">The lbAddDueDate LinkButton</param>
        /// <param name="e">The click event</param>
        protected void lbAddDueDate_Click(object sender, EventArgs e)
        {
            //Clear inputs in the due date div
            hfAddEditDueDatePK.Value = "0";
            ddDueDateForm.Value = null;
            txtDueStartDate.Value = null;
            txtDueRecommendedDate.Value = null;
            txtDueEndDate.Value = null;
            txtDueDateHelpText.Value = null;

            //Set the title
            lblAddEditDueDate.Text = "Add Due Date";

            //Show the due date div
            divAddEditDueDate.Visible = true;

            //Set focus to the first field
            ddDueDateForm.Focus();
        }

        /// <summary>
        /// This method fires when the user clicks the lbEditDueDate LinkButton and
        /// it opens the add/edit section for due dates so that the user can edit the
        /// selected form due date.
        /// </summary>
        /// <param name="sender">The lbEditDueDate LinkButton</param>
        /// <param name="e">The click event</param>
        protected void lbEditDueDate_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the container item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfDueDatePK = (HiddenField)item.FindControl("hfDueDatePK");

            //Get the PK from the hidden field
            int? dueDatePK = (String.IsNullOrWhiteSpace(hfDueDatePK.Value) ? (int?)null : Convert.ToInt32(hfDueDatePK.Value));

            if (dueDatePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the due date to edit
                    FormDueDate editDueDate = context.FormDueDate.AsNoTracking()
                                                        .Where(fdd => fdd.FormDueDatePK == dueDatePK.Value).FirstOrDefault();

                    //Fill the inputs
                    lblAddEditDueDate.Text = "Edit Due Date";
                    ddDueDateForm.SelectedItem = ddDueDateForm.Items.FindByValue(editDueDate.CodeFormFK);
                    txtDueStartDate.Value = editDueDate.DueRecommendedDate.AddDays(editDueDate.DueStartWindow).ToString("MM/dd");
                    txtDueRecommendedDate.Value = editDueDate.DueRecommendedDate.ToString("MM/dd");
                    txtDueEndDate.Value = editDueDate.DueRecommendedDate.AddDays(editDueDate.DueEndWindow).ToString("MM/dd");
                    txtDueDateHelpText.Value = editDueDate.HelpText;
                    hfAddEditDueDatePK.Value = dueDatePK.Value.ToString();
                }

                //Show the due date div
                divAddEditDueDate.Visible = true;

                //Set focus to the first field
                ddDueDateForm.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected due date!", 20000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the lbDeleteDueDate LinkButton and
        /// it deletes the selected form due date.
        /// </summary>
        /// <param name="sender">The lbDeleteDueDate LinkButton</param>
        /// <param name="e">The click event</param>
        protected void lbDeleteDueDate_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteDueDatePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteDueDatePK.Value));

            //Remove the due date if the PK is not null
            if (rowToRemovePK.HasValue)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the due date to remove and remove it
                        FormDueDate dueDateToRemove = context.FormDueDate
                                                                .Where(fdd => fdd.FormDueDatePK == rowToRemovePK).FirstOrDefault();
                        context.FormDueDate.Remove(dueDateToRemove);

                        //Save the deletion to the database
                        context.SaveChanges();

                        //Get the delete change row and set the deleter
                        context.FormDueDateChanged
                                .OrderByDescending(fddc => fddc.FormDueDateChangedPK)
                                .Where(fddc => fddc.FormDueDatePK == dueDateToRemove.FormDueDatePK)
                                .FirstOrDefault().Deleter = User.Identity.Name;

                        //Save the delete change row to the database
                        context.SaveChanges();

                        //Re-bind the due date gridview
                        bsGRDueDates.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the due date!", 10000);
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
                            msgSys.ShowMessageToUser("danger", "Error", "Could not delete the due date, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the due date!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the due date!", 120000);
                    }

                    //Log the error
                    Utilities.LogException(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the due date to delete!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the due date
        /// and it saves the due date information to the database
        /// </summary>
        /// <param name="sender">The submitDueDate control</param>
        /// <param name="e">The Click event</param>
        protected void submitDueDate_SubmitClick(object sender, EventArgs e)
        {
            //Get the due date values
            int dueDatePK = Convert.ToInt32(hfAddEditDueDatePK.Value);
            int codeFormFK = Convert.ToInt32(ddDueDateForm.Value);
            DateTime startWindow = Convert.ToDateTime(txtDueStartDate.Text + strDueDateYear);
            DateTime dueDate = Convert.ToDateTime(txtDueRecommendedDate.Text + strDueDateYear);
            DateTime endWindow = Convert.ToDateTime(txtDueEndDate.Text + strDueDateYear);
            string helpText = (txtDueDateHelpText.Value == null ? null : txtDueDateHelpText.Value.ToString());
            int stateFK = currentProgramRole.CurrentStateFK.Value;

            using (PyramidContext context = new PyramidContext())
            {
                FormDueDate currentDueDate;

                //Check to see if this is an add or an edit
                if (dueDatePK == 0)
                {
                    //Add
                    currentDueDate = new FormDueDate();
                    currentDueDate.CodeFormFK = codeFormFK;
                    currentDueDate.DueStartWindow = (startWindow - dueDate).Days;
                    currentDueDate.DueRecommendedDate = dueDate;
                    currentDueDate.DueEndWindow = (endWindow - dueDate).Days;
                    currentDueDate.HelpText = helpText;
                    currentDueDate.StateFK = stateFK;
                    currentDueDate.CreateDate = DateTime.Now;
                    currentDueDate.Creator = User.Identity.Name;

                    //Save to the database
                    context.FormDueDate.Add(currentDueDate);
                    context.SaveChanges();

                    //Show a success message
                    msgSys.ShowMessageToUser("success", "Success", "Successfully added due date!", 10000);
                }
                else
                {
                    //Edit
                    currentDueDate = context.FormDueDate.Find(dueDatePK);
                    currentDueDate.CodeFormFK = codeFormFK;
                    currentDueDate.DueStartWindow = (startWindow - dueDate).Days;
                    currentDueDate.DueRecommendedDate = dueDate;
                    currentDueDate.DueEndWindow = (endWindow - dueDate).Days;
                    currentDueDate.HelpText = helpText;
                    currentDueDate.StateFK = stateFK;
                    currentDueDate.EditDate = DateTime.Now;
                    currentDueDate.Editor = User.Identity.Name;

                    //Save to the database
                    context.SaveChanges();

                    //Show a success message
                    msgSys.ShowMessageToUser("success", "Success", "Successfully edited due date!", 10000);
                }

                //Reset the values
                hfAddEditDueDatePK.Value = "0";
                divAddEditDueDate.Visible = false;

                //Rebind the gridview
                bsGRDueDates.DataBind();
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the due date
        /// and it closes the due date add/edit div
        /// </summary>
        /// <param name="sender">The submitDueDate control</param>
        /// <param name="e">The Click event</param>
        protected void submitDueDate_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditDueDatePK.Value = "0";
            divAddEditDueDate.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitDueDate control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitDueDate_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method validates the due start date text box for the due dates
        /// </summary>
        /// <param name="sender">The txtDueStartDate BootstrapTextBox</param>
        /// <param name="e">The validation event</param>
        protected void txtDueStartDate_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary dates
            DateTime startDate, dueDate, endDate;

            //To hold the FKs and PK
            int formDueDatePK, codeFormFK;

            //Validate
            if (string.IsNullOrWhiteSpace(txtDueStartDate.Text))
            {
                //The start date is required
                e.IsValid = false;
                e.ErrorText = "Window Start Date is required!";
            }
            else if (DateTime.TryParse(txtDueStartDate.Text + strDueDateYear, out startDate))
            {
                //The start date is valid, validate it against the other dates
                if (DateTime.TryParse(txtDueRecommendedDate.Text + strDueDateYear, out dueDate)
                    && DateTime.TryParse(txtDueEndDate.Text + strDueDateYear, out endDate))
                {
                    //Check if the start date is valid in relation to the other dates
                    if (startDate > dueDate)
                    {
                        //The start date must be on or before the due date
                        e.IsValid = false;
                        e.ErrorText = "Window Start Date must be on or before the Due Date!";
                    }
                    else if (startDate > endDate)
                    {
                        //The start date must be on or before the end date
                        e.IsValid = false;
                        e.ErrorText = "Window Start Date must be on or before the Window End Date!";
                    }
                    else if (ddDueDateForm.Value != null &&
                                int.TryParse(hfAddEditDueDatePK.Value, out formDueDatePK) &&
                                int.TryParse(ddDueDateForm.Value.ToString(), out codeFormFK))
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get all OTHER existing due date rows
                            var otherDueDates = context.FormDueDate.AsNoTracking()
                                                        .Include(fdd => fdd.CodeForm)
                                                        .Where(fdd => fdd.CodeFormFK == codeFormFK &&
                                                                fdd.StateFK == currentProgramRole.CurrentStateFK.Value &&
                                                                fdd.FormDueDatePK != formDueDatePK)
                                                        .Select(fdd => new
                                                        {
                                                            fdd.FormDueDatePK,
                                                            DueEndDate = DbFunctions.AddDays(fdd.DueRecommendedDate, fdd.DueEndWindow),
                                                            fdd.DueRecommendedDate,
                                                            DueStartDate = DbFunctions.AddDays(fdd.DueRecommendedDate, fdd.DueStartWindow),
                                                            fdd.CodeFormFK,
                                                            fdd.CodeForm.FormAbbreviation,
                                                            fdd.CodeForm.FormName,
                                                            fdd.StateFK
                                                        })
                                                        .ToList();

                            //Loop through all the form due date rows
                            foreach (var formDueDate in otherDueDates)
                            {
                                //Check to see if the start date is within another due date range
                                if (startDate >= formDueDate.DueStartDate.Value && startDate <= formDueDate.DueEndDate.Value)
                                {
                                    //The start date is within another due date range
                                    e.IsValid = false;
                                    e.ErrorText = string.Format("Window Start Date cannot be inside of the other {0} due date range of {1:MM/dd} - {2:MM/dd}!",
                                                                formDueDate.FormAbbreviation, formDueDate.DueStartDate.Value, formDueDate.DueEndDate.Value);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //The start date is invalid
                e.IsValid = false;
                e.ErrorText = "Invalid month/day value!";
            }
        }

        /// <summary>
        /// This method validates the due date text box for the due dates
        /// </summary>
        /// <param name="sender">The txtDueRecommendedDate BootstrapTextBox</param>
        /// <param name="e">The validation event</param>
        protected void txtDueRecommendedDate_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary dates
            DateTime startDate, dueDate, endDate;

            //Validate
            if (string.IsNullOrWhiteSpace(txtDueRecommendedDate.Text))
            {
                //The due date is required
                e.IsValid = false;
                e.ErrorText = "Due Date is required!";
            }
            else if (DateTime.TryParse(txtDueRecommendedDate.Text + strDueDateYear, out dueDate))
            {
                //The due date is valid, validate it against the other dates
                if (DateTime.TryParse(txtDueStartDate.Text + strDueDateYear, out startDate)
                    && DateTime.TryParse(txtDueEndDate.Text + strDueDateYear, out endDate))
                {
                    //Check if the due date is valid in relation to the other dates
                    if (dueDate < startDate)
                    {
                        //The due date must be on or after the start date
                        e.IsValid = false;
                        e.ErrorText = "Due Date must be on or after the Window Start Date!";
                    }
                    else if (dueDate > endDate)
                    {
                        //The due date must be on or before the end date
                        e.IsValid = false;
                        e.ErrorText = "Due Date must be on or before the Window End Date!";
                    }
                }
            }
            else
            {
                //The due date is invalid
                e.IsValid = false;
                e.ErrorText = "Invalid month/day value!";
            }
        }

        /// <summary>
        /// This method validates the due end date text box for the due dates
        /// </summary>
        /// <param name="sender">The txtDueEndDate BootstrapTextBox</param>
        /// <param name="e">The validation event</param>
        protected void txtDueEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary dates
            DateTime startDate, dueDate, endDate;

            //To hold the FKs and PK
            int formDueDatePK, codeFormFK;

            //Validate
            if (string.IsNullOrWhiteSpace(txtDueEndDate.Text))
            {
                //The end date is required
                e.IsValid = false;
                e.ErrorText = "Window End Date is required!";
            }
            else if (DateTime.TryParse(txtDueEndDate.Text + strDueDateYear, out endDate))
            {
                //The end date is valid, validate it against the other dates
                if (DateTime.TryParse(txtDueStartDate.Text + strDueDateYear, out startDate)
                    && DateTime.TryParse(txtDueRecommendedDate.Text + strDueDateYear, out dueDate))
                {
                    //Check if the end date is valid in relation to the other dates
                    if (endDate < startDate)
                    {
                        //The end date must be on or after the start date
                        e.IsValid = false;
                        e.ErrorText = "Window End Date must be on or after the Window Start Date!";
                    }
                    else if (endDate < dueDate)
                    {
                        //The end date must be on or after the due date
                        e.IsValid = false;
                        e.ErrorText = "Window End Date must be on or after the Due Date!";
                    }
                    else if (ddDueDateForm.Value != null &&
                                int.TryParse(hfAddEditDueDatePK.Value, out formDueDatePK) &&
                                int.TryParse(ddDueDateForm.Value.ToString(), out codeFormFK))
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get all OTHER existing due date rows
                            var otherDueDates = context.FormDueDate.AsNoTracking()
                                                        .Include(fdd => fdd.CodeForm)
                                                        .Where(fdd => fdd.CodeFormFK == codeFormFK &&
                                                                fdd.StateFK == currentProgramRole.CurrentStateFK.Value &&
                                                                fdd.FormDueDatePK != formDueDatePK)
                                                        .Select(fdd => new
                                                        {
                                                            fdd.FormDueDatePK,
                                                            DueEndDate = DbFunctions.AddDays(fdd.DueRecommendedDate, fdd.DueEndWindow),
                                                            fdd.DueRecommendedDate,
                                                            DueStartDate = DbFunctions.AddDays(fdd.DueRecommendedDate, fdd.DueStartWindow),
                                                            fdd.CodeFormFK,
                                                            fdd.CodeForm.FormAbbreviation,
                                                            fdd.CodeForm.FormName,
                                                            fdd.StateFK
                                                        })
                                                        .ToList();

                            //Loop through all the form due date rows
                            foreach (var formDueDate in otherDueDates)
                            {
                                //Check to see if the end date is within another due date range
                                if (endDate >= formDueDate.DueStartDate.Value && endDate <= formDueDate.DueEndDate.Value)
                                {
                                    //The end date is within another form due date range
                                    e.IsValid = false;
                                    e.ErrorText = string.Format("Window End Date cannot be inside of the other {0} due date range of {1:MM/dd} - {2:MM/dd}!", 
                                                                formDueDate.FormAbbreviation, formDueDate.DueStartDate.Value, formDueDate.DueEndDate.Value);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //The end date is invalid
                e.IsValid = false;
                e.ErrorText = "Invalid month/day value!";
            }
        }

        #endregion
    }
}