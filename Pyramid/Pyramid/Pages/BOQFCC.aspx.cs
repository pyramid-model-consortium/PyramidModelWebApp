using Pyramid.Models;
using System;
using System.Linq;
using Pyramid.Code;
using Pyramid.MasterPages;
using DevExpress.Web;

namespace Pyramid.Pages
{
    public partial class BOQFCC : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private BenchmarkOfQualityFCC currentBOQFCC;
        int BOQFCCPK = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the BOQ PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["BOQFCCPK"]))
            {
                int.TryParse(Request.QueryString["BOQFCCPK"], out BOQFCCPK);
            }

            //Don't allow aggregate viewers into this page
            if (currentProgramRole.RoleFK.Value == (int)Utilities.ProgramRoleFKs.AGGREGATE_DATA_VIEWER)
            {
                Response.Redirect("/Pages/BOQFCCDashboard.aspx?messageType=NotAuthorized");
            }

            //Get the BOQ from the database
            using (PyramidContext context = new PyramidContext())
            {
                //To hold the program information
                Program program;

                //Get the BOQ
                currentBOQFCC = context.BenchmarkOfQualityFCC.AsNoTracking().Where(boqfcc => boqfcc.BenchmarkOfQualityFCCPK == BOQFCCPK).FirstOrDefault();

                //If the BOQ is null (this is an add)
                if (currentBOQFCC == null)
                {
                    //Set the current BOQ to a blank BOQ
                    currentBOQFCC = new BenchmarkOfQualityFCC();

                    //Get the program
                    program = context.Program.AsNoTracking().Where(p => p.ProgramPK == currentProgramRole.CurrentProgramFK.Value).FirstOrDefault();
                }
                else
                {
                    program = context.Program.AsNoTracking().Where(p => p.ProgramPK == currentBOQFCC.ProgramFK).FirstOrDefault();
                }

                //Set the labels
                lblProgramName.Text = program.ProgramName;
                lblProgramLocation.Text = program.Location;
            }

            //Don't allow users to view BOQs from other programs
            if (currentBOQFCC.BenchmarkOfQualityFCCPK > 0 && !currentProgramRole.ProgramFKs.Contains(currentBOQFCC.ProgramFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/BOQFCCDashboard.aspx?messageType={0}", "NoBOQFCC"));
            }

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //If this is an edit or view, populate the page with values
                if (BOQFCCPK != 0)
                {
                    PopulatePage(currentBOQFCC);
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
                if (currentBOQFCC.BenchmarkOfQualityFCCPK == 0 && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitBOQFCC.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Add New Benchmarks of Quality FCC Form";
                }
                else if (currentBOQFCC.BenchmarkOfQualityFCCPK > 0 && action.ToLower() == "edit" && currentProgramRole.AllowedToEdit.Value)
                {
                    //Show the submit button
                    submitBOQFCC.ShowSubmitButton = true;

                    //Show other controls
                    hfViewOnly.Value = "False";

                    //Lock the controls
                    EnableControls(true);

                    //Set the page title
                    lblPageTitle.Text = "Edit Benchmarks of Quality FCC Form";
                }
                else
                {
                    //Hide the submit button
                    submitBOQFCC.ShowSubmitButton = false;

                    //Hide other controls
                    hfViewOnly.Value = "True";

                    //Lock the controls
                    EnableControls(false);

                    //Set the page title
                    lblPageTitle.Text = "View Benchmarks of Quality FCC Form";
                }

                //Set focus to the form date field
                deFormDate.Focus();
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitBOQFCC user control 
        /// </summary>
        /// <param name="sender">The submitBOQFCC control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQFCC_Click(object sender, EventArgs e)
        {
            if (currentProgramRole.AllowedToEdit.Value)
            {
                //To hold the success message
                string successMessageType = null;

                //Fill the fields of the object from the form
                currentBOQFCC.FormDate = deFormDate.Date;
                currentBOQFCC.Indicator1 = Convert.ToInt32(ddIndicator1.Value);
                currentBOQFCC.Indicator2 = Convert.ToInt32(ddIndicator2.Value);
                currentBOQFCC.Indicator3 = Convert.ToInt32(ddIndicator3.Value);
                currentBOQFCC.Indicator4 = Convert.ToInt32(ddIndicator4.Value);
                currentBOQFCC.Indicator5 = Convert.ToInt32(ddIndicator5.Value);
                currentBOQFCC.Indicator6 = Convert.ToInt32(ddIndicator6.Value);
                currentBOQFCC.Indicator7 = Convert.ToInt32(ddIndicator7.Value);
                currentBOQFCC.Indicator8 = Convert.ToInt32(ddIndicator8.Value);
                currentBOQFCC.Indicator9 = Convert.ToInt32(ddIndicator9.Value);
                currentBOQFCC.Indicator10 = Convert.ToInt32(ddIndicator10.Value);
                currentBOQFCC.Indicator11 = Convert.ToInt32(ddIndicator11.Value);
                currentBOQFCC.Indicator12 = Convert.ToInt32(ddIndicator12.Value);
                currentBOQFCC.Indicator13 = Convert.ToInt32(ddIndicator13.Value);
                currentBOQFCC.Indicator14 = Convert.ToInt32(ddIndicator14.Value);
                currentBOQFCC.Indicator15 = Convert.ToInt32(ddIndicator15.Value);
                currentBOQFCC.Indicator16 = Convert.ToInt32(ddIndicator16.Value);
                currentBOQFCC.Indicator17 = Convert.ToInt32(ddIndicator17.Value);
                currentBOQFCC.Indicator18 = Convert.ToInt32(ddIndicator18.Value);
                currentBOQFCC.Indicator19 = Convert.ToInt32(ddIndicator19.Value);
                currentBOQFCC.Indicator20 = Convert.ToInt32(ddIndicator20.Value);
                currentBOQFCC.Indicator21 = Convert.ToInt32(ddIndicator21.Value);
                currentBOQFCC.Indicator22 = Convert.ToInt32(ddIndicator22.Value);
                currentBOQFCC.Indicator23 = Convert.ToInt32(ddIndicator23.Value);
                currentBOQFCC.Indicator24 = Convert.ToInt32(ddIndicator24.Value);
                currentBOQFCC.Indicator25 = Convert.ToInt32(ddIndicator25.Value);
                currentBOQFCC.Indicator26 = Convert.ToInt32(ddIndicator26.Value);
                currentBOQFCC.Indicator27 = Convert.ToInt32(ddIndicator27.Value);
                currentBOQFCC.Indicator28 = Convert.ToInt32(ddIndicator28.Value);
                currentBOQFCC.Indicator29 = Convert.ToInt32(ddIndicator29.Value);
                currentBOQFCC.Indicator30 = Convert.ToInt32(ddIndicator30.Value);
                currentBOQFCC.Indicator31 = Convert.ToInt32(ddIndicator31.Value);
                currentBOQFCC.Indicator32 = Convert.ToInt32(ddIndicator32.Value);
                currentBOQFCC.Indicator33 = Convert.ToInt32(ddIndicator33.Value);
                currentBOQFCC.Indicator34 = Convert.ToInt32(ddIndicator34.Value);
                currentBOQFCC.Indicator35 = Convert.ToInt32(ddIndicator35.Value);
                currentBOQFCC.Indicator36 = Convert.ToInt32(ddIndicator36.Value);
                currentBOQFCC.Indicator37 = Convert.ToInt32(ddIndicator37.Value);
                currentBOQFCC.Indicator38 = Convert.ToInt32(ddIndicator38.Value);
                currentBOQFCC.Indicator39 = Convert.ToInt32(ddIndicator39.Value);
                currentBOQFCC.Indicator40 = Convert.ToInt32(ddIndicator40.Value);
                currentBOQFCC.Indicator41 = Convert.ToInt32(ddIndicator41.Value);
                currentBOQFCC.Indicator42 = Convert.ToInt32(ddIndicator42.Value);
                currentBOQFCC.Indicator43 = Convert.ToInt32(ddIndicator43.Value);
                currentBOQFCC.Indicator44 = Convert.ToInt32(ddIndicator44.Value);
                currentBOQFCC.Indicator45 = Convert.ToInt32(ddIndicator45.Value);
                currentBOQFCC.Indicator46 = Convert.ToInt32(ddIndicator46.Value);
                currentBOQFCC.Indicator47 = Convert.ToInt32(ddIndicator47.Value);
                currentBOQFCC.TeamMembers = txtTeamMembers.Text;

                //Check to see if this is an add or edit
                if (BOQFCCPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an edit
                        successMessageType = "BOQFCCEdited";

                        //Fill the edit-specific fields
                        currentBOQFCC.EditDate = DateTime.Now;
                        currentBOQFCC.Editor = User.Identity.Name;

                        //Get the existing database values
                        BenchmarkOfQualityFCC existingBOQFCC = context.BenchmarkOfQualityFCC.Find(currentBOQFCC.BenchmarkOfQualityFCCPK);

                        //Set the BOQ object to the new values
                        context.Entry(existingBOQFCC).CurrentValues.SetValues(currentBOQFCC);

                        //Save the changes
                        context.SaveChanges();
                    }

                    //Redirect the user to the dashboard with the success message
                    Response.Redirect(string.Format("/Pages/BOQFCCDashboard.aspx?messageType={0}", successMessageType));
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an add
                        successMessageType = "BOQFCCAdded";

                        //Set the create-specific fields
                        currentBOQFCC.CreateDate = DateTime.Now;
                        currentBOQFCC.Creator = User.Identity.Name;
                        currentBOQFCC.ProgramFK = currentProgramRole.CurrentProgramFK.Value;

                        //Add the Benchmark to the database and save
                        context.BenchmarkOfQualityFCC.Add(currentBOQFCC);
                        context.SaveChanges();
                    }

                    //Redirect the user to the dashboard with the success message
                    Response.Redirect(string.Format("/Pages/BOQFCCDashboard.aspx?messageType={0}", successMessageType));
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitBOQFCC user control 
        /// </summary>
        /// <param name="sender">The submitBOQFCC control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQFCC_CancelClick(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("/Pages/BOQFCCDashboard.aspx?messageType={0}", "BOQFCCCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitBOQFCC control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitBOQFCC_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method enables/disables the controls based on the passed boolean value
        /// </summary>
        /// <param name="enabled">True if the controls should be read only, false if not</param>
        private void EnableControls(bool enabled)
        {
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
            ddIndicator42.ClientEnabled = enabled;
            ddIndicator43.ClientEnabled = enabled;
            ddIndicator44.ClientEnabled = enabled;
            ddIndicator45.ClientEnabled = enabled;
            ddIndicator46.ClientEnabled = enabled;
            ddIndicator47.ClientEnabled = enabled;
            deFormDate.ClientEnabled = enabled;
            txtTeamMembers.ClientEnabled = enabled;
            submitBOQFCC.ShowSubmitButton = enabled;
        }

        /// <summary>
        /// This method populates the controls on the page with values
        /// from the passed BenchmarkOfQualityFCC object
        /// </summary>
        /// <param name="boq">The BenchmarkOfQualityFCC object to fill the page controls</param>
        private void PopulatePage(BenchmarkOfQualityFCC boq)
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
            ddIndicator42.SelectedItem = ddIndicator42.Items.FindByValue(boq.Indicator42);
            ddIndicator43.SelectedItem = ddIndicator43.Items.FindByValue(boq.Indicator43);
            ddIndicator44.SelectedItem = ddIndicator44.Items.FindByValue(boq.Indicator44);
            ddIndicator45.SelectedItem = ddIndicator45.Items.FindByValue(boq.Indicator45);
            ddIndicator46.SelectedItem = ddIndicator46.Items.FindByValue(boq.Indicator46);
            ddIndicator47.SelectedItem = ddIndicator47.Items.FindByValue(boq.Indicator47);

            //Set the form date
            deFormDate.Date = boq.FormDate;

            //Set the team members
            txtTeamMembers.Text = boq.TeamMembers;
        }
    }
}

