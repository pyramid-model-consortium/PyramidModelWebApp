using System;
using System.Linq;
using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;

namespace Pyramid.Pages
{
    public partial class BOQ : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "BOQ";
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
        private BenchmarkOfQuality currentBOQ;
        private int BOQPK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the BOQ PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["BOQPK"]))
            {
                int.TryParse(Request.QueryString["BOQPK"], out BOQPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (BOQPK == 0 && !string.IsNullOrWhiteSpace(hfBOQPK.Value))
            {
                int.TryParse(hfBOQPK.Value, out BOQPK);
            }

            //Check to see if this is an edit
            isEdit = BOQPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/BOQDashboard.aspx?messageType=NotAuthorized");
            }

            //Get the BOQ from the database
            using (PyramidContext context = new PyramidContext())
            {
                //To hold the program information
                Program program;

                //Get the BOQ
                currentBOQ = context.BenchmarkOfQuality.AsNoTracking().Where(x => x.BenchmarkOfQualityPK == BOQPK).FirstOrDefault();

                //If the BOQ is null (this is an add)
                if (currentBOQ == null)
                {
                    //Set the current BOQ to a blank BOQ
                    currentBOQ = new BenchmarkOfQuality();

                    //Get the program
                    program = context.Program.AsNoTracking().Where(p => p.ProgramPK == currentProgramRole.CurrentProgramFK.Value).FirstOrDefault();
                }
                else
                {
                    program = context.Program.AsNoTracking().Where(p => p.ProgramPK == currentBOQ.ProgramFK).FirstOrDefault();
                }

                //Set the labels
                lblProgramName.Text = program.ProgramName;
                lblProgramLocation.Text = program.Location;
            }

            //Don't allow users to view BOQs from other programs
            if (isEdit && !currentProgramRole.ProgramFKs.Contains(currentBOQ.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/BOQDashboard.aspx?messageType={0}", "NoBOQ"));
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //If this is an edit or view, populate the page with values
                if (isEdit)
                {
                    PopulatePage(currentBOQ);
                }

                //Get the action
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
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Add New Benchmarks of Quality 2.0 Form";
                }
                else if (isEdit == true && action.ToLower() == "edit" && FormPermissions.AllowedToEdit)
                {
                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Save and Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "Edit Benchmarks of Quality 2.0 Form";
                }
                else
                {
                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the print preview button text
                    btnPrintPreview.Text = "Download/Print";

                    //Set the page title
                    lblPageTitle.Text = "View Benchmarks of Quality 2.0 Form";
                }

                //Set the max value for the form date field
                deFormDate.MaxDate = DateTime.Now;

                //Set focus on the form date field
                deFormDate.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitBOQ user control 
        /// </summary>
        /// <param name="sender">The submitBOQ control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQ_Click(object sender, EventArgs e)
        {
            //To hold the success message
            string successMessageType = SaveForm(true);

            //Only redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the dashboard with the success message
                Response.Redirect(string.Format("/Pages/BOQDashboard.aspx?messageType={0}", successMessageType));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitBOQ user control 
        /// </summary>
        /// <param name="sender">The submitBOQ control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQ_CancelClick(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("/Pages/BOQDashboard.aspx?messageType={0}", "BOQCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitBOQ control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitBOQ_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitBOQ.ValidationGroup))
            {
                //Submit the form
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptBOQ report = new Reports.PreBuiltReports.FormReports.RptBOQ();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "Benchmarks of Quality 2.0", BOQPK);
            }
            else
            {
                //Tell the user that validation failed
                msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
            }
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
            //Enable/disable the controls
            ddIndicator1.ClientEnabled = enabled;
            ddIndicator2.ClientEnabled = enabled;
            ddIndicator3.ClientEnabled = enabled;
            ddIndicator4.ClientEnabled = enabled;
            ddIndicator5.ClientEnabled = enabled;
            ddIndicator6.ClientEnabled = enabled;
            ddIndicator7.ClientEnabled = enabled;
            ddIndicator8.ClientEnabled = enabled;
            ddIndicator9.ClientEnabled = enabled;
            ddIndicator10.ClientEnabled = enabled;
            ddIndicator11.ClientEnabled = enabled;
            ddIndicator12.ClientEnabled = enabled;
            ddIndicator13.ClientEnabled = enabled;
            ddIndicator14.ClientEnabled = enabled;
            ddIndicator15.ClientEnabled = enabled;
            ddIndicator16.ClientEnabled = enabled;
            ddIndicator17.ClientEnabled = enabled;
            ddIndicator18.ClientEnabled = enabled;
            ddIndicator19.ClientEnabled = enabled;
            ddIndicator20.ClientEnabled = enabled;
            ddIndicator21.ClientEnabled = enabled;
            ddIndicator22.ClientEnabled = enabled;
            ddIndicator23.ClientEnabled = enabled;
            ddIndicator24.ClientEnabled = enabled;
            ddIndicator25.ClientEnabled = enabled;
            ddIndicator26.ClientEnabled = enabled;
            ddIndicator27.ClientEnabled = enabled;
            ddIndicator28.ClientEnabled = enabled;
            ddIndicator29.ClientEnabled = enabled;
            ddIndicator30.ClientEnabled = enabled;
            ddIndicator31.ClientEnabled = enabled;
            ddIndicator32.ClientEnabled = enabled;
            ddIndicator33.ClientEnabled = enabled;
            ddIndicator34.ClientEnabled = enabled;
            ddIndicator35.ClientEnabled = enabled;
            ddIndicator36.ClientEnabled = enabled;
            ddIndicator37.ClientEnabled = enabled;
            ddIndicator38.ClientEnabled = enabled;
            ddIndicator39.ClientEnabled = enabled;
            ddIndicator40.ClientEnabled = enabled;
            ddIndicator41.ClientEnabled = enabled;
            deFormDate.ClientEnabled = enabled;
            txtTeamMembers.ClientEnabled = enabled;

            //Show/hide the submit button
            submitBOQ.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitBOQ.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method populates the controls on the page with values
        /// from the passed BenchmarkOfQuality object
        /// </summary>
        /// <param name="boq">The BenchmarkOfQuality object to fill the page controls</param>
        private void PopulatePage(BenchmarkOfQuality boq)
        {
            //Set the indicator dropdown selected values
            ddIndicator1.SelectedItem = ddIndicator1.Items.FindByValue(boq.Indicator1);
            ddIndicator2.SelectedItem = ddIndicator2.Items.FindByValue(boq.Indicator2);
            ddIndicator3.SelectedItem = ddIndicator3.Items.FindByValue(boq.Indicator3);
            ddIndicator4.SelectedItem = ddIndicator4.Items.FindByValue(boq.Indicator4);
            ddIndicator5.SelectedItem = ddIndicator5.Items.FindByValue(boq.Indicator5);
            ddIndicator6.SelectedItem = ddIndicator6.Items.FindByValue(boq.Indicator6);
            ddIndicator7.SelectedItem = ddIndicator7.Items.FindByValue(boq.Indicator7);
            ddIndicator8.SelectedItem = ddIndicator8.Items.FindByValue(boq.Indicator8);
            ddIndicator9.SelectedItem = ddIndicator9.Items.FindByValue(boq.Indicator9);
            ddIndicator10.SelectedItem = ddIndicator10.Items.FindByValue(boq.Indicator10);
            ddIndicator11.SelectedItem = ddIndicator11.Items.FindByValue(boq.Indicator11);
            ddIndicator12.SelectedItem = ddIndicator12.Items.FindByValue(boq.Indicator12);
            ddIndicator13.SelectedItem = ddIndicator13.Items.FindByValue(boq.Indicator13);
            ddIndicator14.SelectedItem = ddIndicator14.Items.FindByValue(boq.Indicator14);
            ddIndicator15.SelectedItem = ddIndicator15.Items.FindByValue(boq.Indicator15);
            ddIndicator16.SelectedItem = ddIndicator16.Items.FindByValue(boq.Indicator16);
            ddIndicator17.SelectedItem = ddIndicator17.Items.FindByValue(boq.Indicator17);
            ddIndicator18.SelectedItem = ddIndicator18.Items.FindByValue(boq.Indicator18);
            ddIndicator19.SelectedItem = ddIndicator19.Items.FindByValue(boq.Indicator19);
            ddIndicator20.SelectedItem = ddIndicator20.Items.FindByValue(boq.Indicator20);
            ddIndicator21.SelectedItem = ddIndicator21.Items.FindByValue(boq.Indicator21);
            ddIndicator22.SelectedItem = ddIndicator22.Items.FindByValue(boq.Indicator22);
            ddIndicator23.SelectedItem = ddIndicator23.Items.FindByValue(boq.Indicator23);
            ddIndicator24.SelectedItem = ddIndicator24.Items.FindByValue(boq.Indicator24);
            ddIndicator25.SelectedItem = ddIndicator25.Items.FindByValue(boq.Indicator25);
            ddIndicator26.SelectedItem = ddIndicator26.Items.FindByValue(boq.Indicator26);
            ddIndicator27.SelectedItem = ddIndicator27.Items.FindByValue(boq.Indicator27);
            ddIndicator28.SelectedItem = ddIndicator28.Items.FindByValue(boq.Indicator28);
            ddIndicator29.SelectedItem = ddIndicator29.Items.FindByValue(boq.Indicator29);
            ddIndicator30.SelectedItem = ddIndicator30.Items.FindByValue(boq.Indicator30);
            ddIndicator31.SelectedItem = ddIndicator31.Items.FindByValue(boq.Indicator31);
            ddIndicator32.SelectedItem = ddIndicator32.Items.FindByValue(boq.Indicator32);
            ddIndicator33.SelectedItem = ddIndicator33.Items.FindByValue(boq.Indicator33);
            ddIndicator34.SelectedItem = ddIndicator34.Items.FindByValue(boq.Indicator34);
            ddIndicator35.SelectedItem = ddIndicator35.Items.FindByValue(boq.Indicator35);
            ddIndicator36.SelectedItem = ddIndicator36.Items.FindByValue(boq.Indicator36);
            ddIndicator37.SelectedItem = ddIndicator37.Items.FindByValue(boq.Indicator37);
            ddIndicator38.SelectedItem = ddIndicator38.Items.FindByValue(boq.Indicator38);
            ddIndicator39.SelectedItem = ddIndicator39.Items.FindByValue(boq.Indicator39);
            ddIndicator40.SelectedItem = ddIndicator40.Items.FindByValue(boq.Indicator40);
            ddIndicator41.SelectedItem = ddIndicator41.Items.FindByValue(boq.Indicator41);

            //Set the form date
            deFormDate.Date = boq.FormDate;

            //Set the team members
            txtTeamMembers.Text = boq.TeamMembers;
        }

        /// <summary>
        /// This method populates and saves the form
        /// </summary>
        /// <param name="showMessages">Whether to show messages from the save</param>
        /// <returns>The success message type, null if the save failed</returns>
        private string SaveForm(bool showMessages)
        {
            //To hold the success message
            string successMessageType = null;

            //Determine if the user is allowed to save the form
            if ((isEdit && FormPermissions.AllowedToEdit) || (isEdit == false && FormPermissions.AllowedToAdd))
            {
                //Fill the fields of the object from the form
                currentBOQ.FormDate = deFormDate.Date;
                currentBOQ.TeamMembers = txtTeamMembers.Text;
                currentBOQ.Indicator1 = Convert.ToInt32(ddIndicator1.Value);
                currentBOQ.Indicator2 = Convert.ToInt32(ddIndicator2.Value);
                currentBOQ.Indicator3 = Convert.ToInt32(ddIndicator3.Value);
                currentBOQ.Indicator4 = Convert.ToInt32(ddIndicator4.Value);
                currentBOQ.Indicator5 = Convert.ToInt32(ddIndicator5.Value);
                currentBOQ.Indicator6 = Convert.ToInt32(ddIndicator6.Value);
                currentBOQ.Indicator7 = Convert.ToInt32(ddIndicator7.Value);
                currentBOQ.Indicator8 = Convert.ToInt32(ddIndicator8.Value);
                currentBOQ.Indicator9 = Convert.ToInt32(ddIndicator9.Value);
                currentBOQ.Indicator10 = Convert.ToInt32(ddIndicator10.Value);
                currentBOQ.Indicator11 = Convert.ToInt32(ddIndicator11.Value);
                currentBOQ.Indicator12 = Convert.ToInt32(ddIndicator12.Value);
                currentBOQ.Indicator13 = Convert.ToInt32(ddIndicator13.Value);
                currentBOQ.Indicator14 = Convert.ToInt32(ddIndicator14.Value);
                currentBOQ.Indicator15 = Convert.ToInt32(ddIndicator15.Value);
                currentBOQ.Indicator16 = Convert.ToInt32(ddIndicator16.Value);
                currentBOQ.Indicator17 = Convert.ToInt32(ddIndicator17.Value);
                currentBOQ.Indicator18 = Convert.ToInt32(ddIndicator18.Value);
                currentBOQ.Indicator19 = Convert.ToInt32(ddIndicator19.Value);
                currentBOQ.Indicator20 = Convert.ToInt32(ddIndicator20.Value);
                currentBOQ.Indicator21 = Convert.ToInt32(ddIndicator21.Value);
                currentBOQ.Indicator22 = Convert.ToInt32(ddIndicator22.Value);
                currentBOQ.Indicator23 = Convert.ToInt32(ddIndicator23.Value);
                currentBOQ.Indicator24 = Convert.ToInt32(ddIndicator24.Value);
                currentBOQ.Indicator25 = Convert.ToInt32(ddIndicator25.Value);
                currentBOQ.Indicator26 = Convert.ToInt32(ddIndicator26.Value);
                currentBOQ.Indicator27 = Convert.ToInt32(ddIndicator27.Value);
                currentBOQ.Indicator28 = Convert.ToInt32(ddIndicator28.Value);
                currentBOQ.Indicator29 = Convert.ToInt32(ddIndicator29.Value);
                currentBOQ.Indicator30 = Convert.ToInt32(ddIndicator30.Value);
                currentBOQ.Indicator31 = Convert.ToInt32(ddIndicator31.Value);
                currentBOQ.Indicator32 = Convert.ToInt32(ddIndicator32.Value);
                currentBOQ.Indicator33 = Convert.ToInt32(ddIndicator33.Value);
                currentBOQ.Indicator34 = Convert.ToInt32(ddIndicator34.Value);
                currentBOQ.Indicator35 = Convert.ToInt32(ddIndicator35.Value);
                currentBOQ.Indicator36 = Convert.ToInt32(ddIndicator36.Value);
                currentBOQ.Indicator37 = Convert.ToInt32(ddIndicator37.Value);
                currentBOQ.Indicator38 = Convert.ToInt32(ddIndicator38.Value);
                currentBOQ.Indicator39 = Convert.ToInt32(ddIndicator39.Value);
                currentBOQ.Indicator40 = Convert.ToInt32(ddIndicator40.Value);
                currentBOQ.Indicator41 = Convert.ToInt32(ddIndicator41.Value);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an edit
                        successMessageType = "BOQEdited";

                        //Fill the edit-specific fields
                        currentBOQ.EditDate = DateTime.Now;
                        currentBOQ.Editor = User.Identity.Name;

                        //Get the existing database values
                        BenchmarkOfQuality existingBOQ = context.BenchmarkOfQuality.Find(currentBOQ.BenchmarkOfQualityPK);

                        //Set the BOQ object to the new values
                        context.Entry(existingBOQ).CurrentValues.SetValues(currentBOQ);

                        //Save the changes
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfBOQPK.Value = currentBOQ.BenchmarkOfQualityPK.ToString();
                        BOQPK = currentBOQ.BenchmarkOfQualityPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an add
                        successMessageType = "BOQAdded";

                        //Set the create-specific fields
                        currentBOQ.CreateDate = DateTime.Now;
                        currentBOQ.Creator = User.Identity.Name;
                        currentBOQ.ProgramFK = currentProgramRole.CurrentProgramFK.Value;

                        //Add the Benchmark to the database and save
                        context.BenchmarkOfQuality.Add(currentBOQ);
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfBOQPK.Value = currentBOQ.BenchmarkOfQualityPK.ToString();
                        BOQPK = currentBOQ.BenchmarkOfQualityPK;
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
    }
}