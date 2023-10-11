using System;
using System.Data.Entity;
using System.Linq;
using Pyramid.Models;
using Pyramid.Code;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using DevExpress.Web;
using System.Web.UI.HtmlControls;
using DevExpress.Web.Bootstrap;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Pyramid.Pages
{
    public partial class BOQFCCDashboard : System.Web.UI.Page
    {
        private List<string> FormAbbreviations
        {
            get
            {
                return new List<string>() {
                    "BOQFCC"
                };
            }
        }

        public List<CodeProgramRolePermission> FormPermissions { get; set; }

        public CodeProgramRolePermission BOQFCCPermissions { get; set; }
        public CodeProgramRolePermission FormSchedulePermissions { get; set; }

        private ProgramAndRoleFromSession currentProgramRole;
        private int BOQFCCCodeFormPK;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission objects
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviations, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the specific permission objects
            BOQFCCPermissions = FormPermissions.Where(fp => fp.CodeForm.FormAbbreviation == "BOQFCC").FirstOrDefault();
            FormSchedulePermissions = BOQFCCPermissions;  //Use the BOQ FCC permissions for the schedules for now

            //Get the CodeFormPK for the BOQ FCC CodeForm row
            BOQFCCCodeFormPK = BOQFCCPermissions.CodeFormFK;

            //Check to see if the user can view this page
            if (FormPermissions.Where(p => p.AllowedToViewDashboard == true).Count() == 0)
            {
                Response.Redirect("/Default.aspx?messageType=PageNotAuthorized");
            }

            //Set the permissions for the sections
            bool showStateColumns = currentProgramRole.StateFKs.Count > 1;
            SetPermissions(BOQFCCPermissions, divBOQs, bsGRBOQFCCs, hfBOQViewOnly, showStateColumns);
            SetPermissions(FormSchedulePermissions, divFormSchedules, bsGRFormSchedules, hfScheduleViewOnly, showStateColumns);

            if (!IsPostBack)
            {
                //Bind the dropdowns
                BindDropdowns();

                //Check for a message in the query string
                string messagetype = Request.QueryString["messageType"];

                //Show the message if it exists
                if (!string.IsNullOrWhiteSpace(messagetype))
                {
                    switch(messagetype)
                    {
                        case "BOQFCCAdded":
                            msgSys.ShowMessageToUser("success", "Success", "Benchmarks Of Quality FCC form successfully added!", 1000);
                            break;
                        case "BOQFCCEdited":
                            msgSys.ShowMessageToUser("success", "Success", "Benchmarks Of Quality FCC form successfully edited!", 1000);
                            break;
                        case "BOQFCCCanceled":
                            msgSys.ShowMessageToUser("info", "Canceled", "The action was canceled, no changes were saved.", 10000);
                            break;
                        case "NoBOQFCC":
                            msgSys.ShowMessageToUser("warning", "Warning", "The specified Benchmarks of Quality FCC form could not be found, please try again.", 15000);
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
        /// This method sets the visibility and usability of dashboard sections based on the passed permission object
        /// </summary>
        /// <param name="permissions">The permissions for the section</param>
        /// <param name="sectionDiv">The section div</param>
        /// <param name="gridview">The section gridview</param>
        /// <param name="viewOnlyHiddenField">The view only hidden field</param>
        /// <param name="showStateNameColumns">Whether or not to show state name columns</param>
        private void SetPermissions(CodeProgramRolePermission permissions, HtmlGenericControl sectionDiv, BootstrapGridView gridview, HiddenField viewOnlyHiddenField, bool showStateNameColumns)
        {
            //Check permissions
            if (permissions.AllowedToViewDashboard == false)
            {
                //Not allowed to see the section
                sectionDiv.Visible = false;
            }
            else if (permissions.AllowedToView == false)
            {
                //Get the action column index (the farthest right column)
                int actionColumnIndex = (gridview.Columns.Count - 1);

                //Hide the action column
                gridview.Columns[actionColumnIndex].Visible = false;

                //Hide management options
                viewOnlyHiddenField.Value = "True";
            }
            else if (permissions.AllowedToAdd == false && permissions.AllowedToEdit == false)
            {
                viewOnlyHiddenField.Value = "True";
            }
            else
            {
                viewOnlyHiddenField.Value = "False";
            }

            //Set visibility of the state name column in the gridview
            if (gridview.Columns["StateNameColumn"] != null)
            {
                gridview.Columns["StateNameColumn"].Visible = showStateNameColumns;
            }
        }

        /// <summary>
        /// This method is used to bind the dropdowns on the page
        /// </summary>
        private void BindDropdowns()
        {
            //To hold all the FCC programs
            List<Program> allFCCPrograms = new List<Program>();

            //Get all the FCC programs
            using (PyramidContext context = new PyramidContext())
            {
                allFCCPrograms = context.Program
                                        .Include(p => p.ProgramType)
                                        .AsNoTracking()
                                        .Where(p => currentProgramRole.ProgramFKs.Contains(p.ProgramPK) &&
                                                    p.ProgramType.Where(pt => pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.FAMILY_CHILD_CARE ||
                                                                            pt.TypeCodeFK == (int)Utilities.ProgramTypeFKs.GROUP_FAMILY_CHILD_CARE).Count() > 0)
                                        .OrderBy(p => p.ProgramName)
                                        .ToList();
            }

            //Bind the FCC program dropdowns
            ddFSProgram.DataSource = allFCCPrograms;
            ddFSProgram.DataBind();
        }

        #region BOQs

        /// <summary>
        /// This method fires when the data source for the Benchmarks of Quality FCC DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efBOQFCCDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efBOQFCCDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "BenchmarkOfQualityFCCPK";
            
            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            e.QueryableSource = context.BenchmarkOfQualityFCC.AsNoTracking()
                                            .Include(boqfcc => boqfcc.Program)
                                            .Include(boqfcc => boqfcc.Program.State)
                                            .Where(boqfcc => currentProgramRole.ProgramFKs.Contains(boqfcc.ProgramFK));
            
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a Benchmarks of Quality FCC form
        /// and it deletes the form information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteBOQFCC LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteBOQFCC_Click(object sender, EventArgs e)
        {
            if (BOQFCCPermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? removeBOQFCCPK = string.IsNullOrWhiteSpace(hfDeleteBOQFCCPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteBOQFCCPK.Value);

                if (removeBOQFCCPK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the BOQ to remove
                            Models.BenchmarkOfQualityFCC BOQFCCToRemove = context.BenchmarkOfQualityFCC.Where(boqf => boqf.BenchmarkOfQualityFCCPK == removeBOQFCCPK).FirstOrDefault();

                            //Remove the BOQ
                            context.BenchmarkOfQualityFCC.Remove(BOQFCCToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.BenchmarkOfQualityFCCChanged
                                    .OrderByDescending(boqf => boqf.BenchmarkOfQualityFCCChangedPK)
                                    .Where(boqfc => boqfc.BenchmarkOfQualityFCCPK == BOQFCCToRemove.BenchmarkOfQualityFCCPK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();

                            //Show a delete success message
                            msgSys.ShowMessageToUser("success", "Success", "Successfully deleted Benchmarks of Quality FCC form!", 1000);

                            //Bind the gridview
                            bsGRBOQFCCs.DataBind();
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
                                //Get the SQL error message
                                string errorMessage = sqlEx.Message.ToLower();

                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the Benchmark of Quality FCC form, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Benchmark of Quality FCC form!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the Benchmark of Quality FCC form!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the Benchmarks of Quality FCC form to delete!", 120000);
                }
            }
            else
            {
                //Not allowed to delete, show a message
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion

        #region Form Schedules

        protected void efFormScheduleDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key of the table
            e.KeyExpression = "FormSchedulePK";

            PyramidContext context = new PyramidContext();

            //Set the source to a LINQ query
            e.QueryableSource = context.FormSchedule
                                        .Include(fs => fs.Program)
                                        .Include(fs => fs.Program.State)
                                        .AsNoTracking()
                                        .Where(fs => fs.CodeFormFK == BOQFCCCodeFormPK &&  //Only get the BOQ FCC rows
                                                     currentProgramRole.ProgramFKs.Contains(fs.ProgramFK));
        }

        /// <summary>
        /// This method enables/disables the controls in the add/edit div
        /// </summary>
        /// <param name="enabled">True if the controls should be enabled, false if disabled</param>
        private void SetFormScheduleControlUsability(bool enabled)
        {
            //Enable/disable the controls
            ddFSProgram.ClientEnabled = enabled;
            deFSYear.ClientEnabled = enabled;
            chkFSScheduledForJan.ClientEnabled = enabled;
            chkFSScheduledForFeb.ClientEnabled = enabled;
            chkFSScheduledForMar.ClientEnabled = enabled;
            chkFSScheduledForApr.ClientEnabled = enabled;
            chkFSScheduledForMay.ClientEnabled = enabled;
            chkFSScheduledForJun.ClientEnabled = enabled;
            chkFSScheduledForJul.ClientEnabled = enabled;
            chkFSScheduledForAug.ClientEnabled = enabled;
            chkFSScheduledForSep.ClientEnabled = enabled;
            chkFSScheduledForOct.ClientEnabled = enabled;
            chkFSScheduledForNov.ClientEnabled = enabled;
            chkFSScheduledForDec.ClientEnabled = enabled;

            //Show/hide the submit button
            submitFormSchedule.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitFormSchedule.UseCancelConfirm = enabled && areConfirmationsEnabled;

            //Update the submit control's properties
            submitFormSchedule.UpdateProperties();
        }

        /// <summary>
        /// This method fires when the validation for the ddFSProgram DevExpress
        /// BootstrapComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddFSProgram BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddFSProgram_Validation(object sender, ValidationEventArgs e)
        {
            //To hold the necessary values
            int? selectedProgramFK;
            int currentSchedulePK, selectedYear;

            //Get the necessary values
            int.TryParse(hfAddEditFormSchedulePK.Value, out currentSchedulePK);
            selectedProgramFK = (ddFSProgram.Value == null ? (int?)null : Convert.ToInt32(ddFSProgram.Value));

            //Validate
            if (selectedProgramFK.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Program is required!";
            }
            else
            {
                //Check for duplication
                if (deFSYear.Date != DateTime.MinValue)
                {
                    //Get the selected year
                    selectedYear = deFSYear.Date.Year;

                    using (PyramidContext context = new PyramidContext())
                    {
                        //Check to see if there are any schedules for this year, form, and program
                        List<FormSchedule> duplicateSchedules = context.FormSchedule.AsNoTracking()
                                        .Where(s => s.ProgramFK == selectedProgramFK.Value &&
                                                s.ScheduleYear == selectedYear &&
                                                s.FormSchedulePK != currentSchedulePK &&
                                                s.CodeFormFK == BOQFCCCodeFormPK).ToList();

                        //Check the count of duplicate schedules
                        if (duplicateSchedules.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "A schedule already exists for this program and year!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the Form Schedules
        /// and it opens a div that allows the user to add a Form Schedule
        /// </summary>
        /// <param name="sender">The lbAddFormSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddFormSchedule_Click(object sender, EventArgs e)
        {
            //Make sure the user is allowed to add a schedule
            if (FormSchedulePermissions.AllowedToAdd)
            {
                //Clear inputs in the Form Schedule div
                hfAddEditFormSchedulePK.Value = "0";
                ddFSProgram.SelectedItem = ddFSProgram.Items.FindByValue(currentProgramRole.CurrentProgramFK.Value);
                deFSYear.Value = "";
                chkFSScheduledForJan.Checked = false;
                chkFSScheduledForFeb.Checked = false;
                chkFSScheduledForMar.Checked = false;
                chkFSScheduledForApr.Checked = false;
                chkFSScheduledForMay.Checked = false;
                chkFSScheduledForJun.Checked = false;
                chkFSScheduledForJul.Checked = false;
                chkFSScheduledForAug.Checked = false;
                chkFSScheduledForSep.Checked = false;
                chkFSScheduledForOct.Checked = false;
                chkFSScheduledForNov.Checked = false;
                chkFSScheduledForDec.Checked = false;

                //Set the title
                lblAddEditFormSchedule.Text = "Add Form Schedule";

                //Show the Form Schedule div
                divAddEditFormSchedule.Visible = true;

                //Set focus to the first field
                ddFSProgram.Focus();

                //Enable the controls
                SetFormScheduleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Not Authorized", "You are not authorized because you are not logged in as a Leadership Coach!", 10000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the view button for a CC Form Schedule
        /// and it opens the CC Form Schedule div in read-only mode
        /// </summary>
        /// <param name="sender">The lbViewFormSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbViewFormSchedule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton viewButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)viewButton.Parent;

            //Get the label with the PK for editing
            Label lblFormSchedulePK = (Label)item.FindControl("lblFormSchedulePK");

            //Get the PK from the label
            int? FormSchedulePK = (string.IsNullOrWhiteSpace(lblFormSchedulePK.Text) ? (int?)null : Convert.ToInt32(lblFormSchedulePK.Text));

            if (FormSchedulePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedule
                    FormSchedule currentSchedule = context.FormSchedule.AsNoTracking().Where(s => s.FormSchedulePK == FormSchedulePK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditFormSchedulePK.Value = currentSchedule.FormSchedulePK.ToString();
                    ddFSProgram.SelectedItem = ddFSProgram.Items.FindByValue(currentSchedule.ProgramFK);
                    deFSYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.ScheduleYear));
                    chkFSScheduledForJan.Checked = currentSchedule.ScheduledForJan;
                    chkFSScheduledForFeb.Checked = currentSchedule.ScheduledForFeb;
                    chkFSScheduledForMar.Checked = currentSchedule.ScheduledForMar;
                    chkFSScheduledForApr.Checked = currentSchedule.ScheduledForApr;
                    chkFSScheduledForMay.Checked = currentSchedule.ScheduledForMay;
                    chkFSScheduledForJun.Checked = currentSchedule.ScheduledForJun;
                    chkFSScheduledForJul.Checked = currentSchedule.ScheduledForJul;
                    chkFSScheduledForAug.Checked = currentSchedule.ScheduledForAug;
                    chkFSScheduledForSep.Checked = currentSchedule.ScheduledForSep;
                    chkFSScheduledForOct.Checked = currentSchedule.ScheduledForOct;
                    chkFSScheduledForNov.Checked = currentSchedule.ScheduledForNov;
                    chkFSScheduledForDec.Checked = currentSchedule.ScheduledForDec;
                }

                //Set the title
                lblAddEditFormSchedule.Text = "View Form Schedule";

                //Show the div
                divAddEditFormSchedule.Visible = true;

                //Set focus to the first field
                ddFSProgram.Focus();

                //Disable the controls
                SetFormScheduleControlUsability(false);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected schedule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a Form Schedule
        /// and it opens the Form Schedule edit div so that the user can edit the selected Form Schedule
        /// </summary>
        /// <param name="sender">The lbEditFormSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditFormSchedule_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the label with the PK for editing
            Label lblFormSchedulePK = (Label)item.FindControl("lblFormSchedulePK");

            //Get the PK from the label
            int? FormSchedulePK = (string.IsNullOrWhiteSpace(lblFormSchedulePK.Text) ? (int?)null : Convert.ToInt32(lblFormSchedulePK.Text));

            if (FormSchedulePK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the schedule
                    FormSchedule currentSchedule = context.FormSchedule.AsNoTracking().Where(s => s.FormSchedulePK == FormSchedulePK.Value).FirstOrDefault();

                    //Fill the add/edit div controls
                    hfAddEditFormSchedulePK.Value = currentSchedule.FormSchedulePK.ToString();
                    ddFSProgram.SelectedItem = ddFSProgram.Items.FindByValue(currentSchedule.ProgramFK);
                    deFSYear.Value = Convert.ToDateTime(string.Format("01/01/{0}", currentSchedule.ScheduleYear));
                    chkFSScheduledForJan.Checked = currentSchedule.ScheduledForJan;
                    chkFSScheduledForFeb.Checked = currentSchedule.ScheduledForFeb;
                    chkFSScheduledForMar.Checked = currentSchedule.ScheduledForMar;
                    chkFSScheduledForApr.Checked = currentSchedule.ScheduledForApr;
                    chkFSScheduledForMay.Checked = currentSchedule.ScheduledForMay;
                    chkFSScheduledForJun.Checked = currentSchedule.ScheduledForJun;
                    chkFSScheduledForJul.Checked = currentSchedule.ScheduledForJul;
                    chkFSScheduledForAug.Checked = currentSchedule.ScheduledForAug;
                    chkFSScheduledForSep.Checked = currentSchedule.ScheduledForSep;
                    chkFSScheduledForOct.Checked = currentSchedule.ScheduledForOct;
                    chkFSScheduledForNov.Checked = currentSchedule.ScheduledForNov;
                    chkFSScheduledForDec.Checked = currentSchedule.ScheduledForDec;
                }

                //Set the title
                lblAddEditFormSchedule.Text = "Edit Form Schedule";

                //Show the div
                divAddEditFormSchedule.Visible = true;

                //Set focus to the first field
                ddFSProgram.Focus();

                //Enable the controls
                SetFormScheduleControlUsability(true);
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected schedule!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the Form Schedules
        /// and it closes the Form Schedule add/edit div
        /// </summary>
        /// <param name="sender">The submitFormSchedule user control</param>
        /// <param name="e">The Click event</param>
        protected void submitFormSchedule_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditFormSchedulePK.Value = "0";
            divAddEditFormSchedule.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitFormSchedule control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitFormSchedule_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the form schedules
        /// and it saves the schedule information to the database
        /// </summary>
        /// <param name="sender">The btnSaveFormSchedule DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void submitFormSchedule_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (FormSchedulePermissions.AllowedToEdit)
            {
                //Get the Form Schedule pk
                int formSchedulePK = Convert.ToInt32(hfAddEditFormSchedulePK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    //The schedule object
                    FormSchedule currentSchedule = context.FormSchedule.Where(fs => fs.FormSchedulePK == formSchedulePK).FirstOrDefault();

                    if (currentSchedule == null || currentSchedule.FormSchedulePK == 0)
                    {
                        currentSchedule = new FormSchedule();
                    }

                    //Fill the object
                    currentSchedule.ProgramFK = Convert.ToInt32(ddFSProgram.Value);
                    currentSchedule.ScheduleYear = deFSYear.Date.Year;
                    currentSchedule.ScheduledForJan = chkFSScheduledForJan.Checked;
                    currentSchedule.ScheduledForFeb = chkFSScheduledForFeb.Checked;
                    currentSchedule.ScheduledForMar = chkFSScheduledForMar.Checked;
                    currentSchedule.ScheduledForApr = chkFSScheduledForApr.Checked;
                    currentSchedule.ScheduledForMay = chkFSScheduledForMay.Checked;
                    currentSchedule.ScheduledForJun = chkFSScheduledForJun.Checked;
                    currentSchedule.ScheduledForJul = chkFSScheduledForJul.Checked;
                    currentSchedule.ScheduledForAug = chkFSScheduledForAug.Checked;
                    currentSchedule.ScheduledForSep = chkFSScheduledForSep.Checked;
                    currentSchedule.ScheduledForOct = chkFSScheduledForOct.Checked;
                    currentSchedule.ScheduledForNov = chkFSScheduledForNov.Checked;
                    currentSchedule.ScheduledForDec = chkFSScheduledForDec.Checked;

                    //Check to see if this is an add or an edit
                    if (formSchedulePK == 0)
                    {
                        //Fill the add-only fields
                        currentSchedule.Creator = User.Identity.Name;
                        currentSchedule.CreateDate = DateTime.Now;
                        currentSchedule.CodeFormFK = BOQFCCCodeFormPK;

                        //Add to the database
                        context.FormSchedule.Add(currentSchedule);

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added the form schedule!", 10000);
                    }
                    else
                    {
                        //Fill the edit-only fields
                        currentSchedule.Editor = User.Identity.Name;
                        currentSchedule.EditDate = DateTime.Now;

                        //Get the existing database values
                        Models.FormSchedule existingFormSchedule = context.FormSchedule.Find(formSchedulePK);

                        //Set the object to the new values
                        context.Entry(existingFormSchedule).CurrentValues.SetValues(currentSchedule);

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited the form schedule!", 10000);
                    }
                }

                //Reset the values in the hidden field and hide the div
                hfAddEditFormSchedulePK.Value = "0";
                divAddEditFormSchedule.Visible = false;

                //Rebind the FormSchedule gridview
                bsGRFormSchedules.DataBind();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a form schedule
        /// and it deletes the schedule information from the database
        /// </summary>
        /// <param name="sender">The btnDeleteFormSchedule LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteFormSchedule_Click(object sender, EventArgs e)
        {
            //Check the permissions
            if (FormSchedulePermissions.AllowedToDelete)
            {
                //Get the PK from the hidden field
                int? rowToRemovePK = (string.IsNullOrWhiteSpace(hfDeleteFormSchedulePK.Value) ? (int?)null : Convert.ToInt32(hfDeleteFormSchedulePK.Value));

                //Remove the role if the PK is not null
                if (rowToRemovePK.HasValue)
                {
                    try
                    {
                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the schedule to remove
                            FormSchedule scheduleToRemove = context.FormSchedule.Where(s => s.FormSchedulePK == rowToRemovePK).FirstOrDefault();

                            //Remove the status
                            context.FormSchedule.Remove(scheduleToRemove);

                            //Save the deletion to the database
                            context.SaveChanges();

                            //Get the delete change row and set the deleter
                            context.FormScheduleChanged
                                    .OrderByDescending(c => c.FormScheduleChangedPK)
                                    .Where(c => c.FormSchedulePK == scheduleToRemove.FormSchedulePK)
                                    .FirstOrDefault().Deleter = User.Identity.Name;

                            //Save the delete change row to the database
                            context.SaveChanges();
                        }

                        //Rebind the form schedule gridview
                        bsGRFormSchedules.DataBind();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted the form schedule!", 10000);
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
                                //Create the message for the user based on the error message
                                string messageForUser = "there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.";

                                //Show the error message
                                msgSys.ShowMessageToUser("danger", "Error", string.Format("Could not delete the form schedule, {0}", messageForUser), 120000);
                            }
                            else
                            {
                                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the form schedule!", 120000);
                            }
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the form schedule!", 120000);
                        }

                        //Log the error
                        Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                    }
                }
                else
                {
                    msgSys.ShowMessageToUser("danger", "Error", "Could not find the form schedule to delete!", 120000);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        #endregion
    }
}