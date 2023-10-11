using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using DevExpress.Web;
using Pyramid.Code;
using Pyramid.MasterPages;
using Pyramid.Models;
using Z.EntityFramework.Plus;

namespace Pyramid.Pages
{
    public partial class BOQCWLT : System.Web.UI.Page, IForm
    {
        public string FormAbbreviation
        {
            get
            {
                return "BOQCWLT";
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
        private BenchmarkOfQualityCWLT currentBOQCWLT;
        private List<BOQCWLTParticipant> currentParticipants;
        private int BOQCWLTPK = 0;
        private int currentHubFK = 0;
        private int currentStateFK = 0;
        private bool isEdit = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's current program role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Get the permission object
            FormPermissions = Utilities.GetProgramRolePermissionsFromDatabase(FormAbbreviation, currentProgramRole.CodeProgramRoleFK.Value, currentProgramRole.IsProgramLocked.Value);

            //Get the BOQCWLT PK from the query string
            if (!string.IsNullOrWhiteSpace(Request.QueryString["BOQCWLTPK"]))
            {
                int.TryParse(Request.QueryString["BOQCWLTPK"], out BOQCWLTPK);
            }

            //If the current PK is 0, try to get the value from the hidden field
            if (BOQCWLTPK == 0 && !string.IsNullOrWhiteSpace(hfBOQCWLTPK.Value))
            {
                int.TryParse(hfBOQCWLTPK.Value, out BOQCWLTPK);
            }

            //Check to see if this is an edit
            isEdit = BOQCWLTPK > 0;

            //Don't allow aggregate viewers into this page
            if (FormPermissions.AllowedToView == false)
            {
                Response.Redirect("/Pages/CWLTDashboard.aspx?messageType=NotAuthorized");
            }

            //Get the BOQCWLT from the database
            using (PyramidContext context = new PyramidContext())
            {
                //To hold the hub information
                Hub currentHub;

                //Get the BOQCWLT
                currentBOQCWLT = context.BenchmarkOfQualityCWLT.Include(boqs => boqs.BOQCWLTParticipant).AsNoTracking().Where(x => x.BenchmarkOfQualityCWLTPK == BOQCWLTPK).FirstOrDefault();

                //If the BOQCWLT is null (this is an add)
                if (currentBOQCWLT == null)
                {
                    //Set the current BOQCWLT to a blank BOQCWLT
                    currentBOQCWLT = new BenchmarkOfQualityCWLT();

                    //Set the list of participants to a blank list
                    currentParticipants = new List<BOQCWLTParticipant>();

                    //Get the hub
                    currentHub = context.Hub.AsNoTracking().Where(p => p.HubPK == currentProgramRole.CurrentHubFK.Value).FirstOrDefault();
                }
                else
                {
                    //Get the list of participants
                    currentParticipants = currentBOQCWLT.BOQCWLTParticipant.ToList();

                    //Get the hub
                    currentHub = context.Hub.AsNoTracking().Where(p => p.HubPK == currentBOQCWLT.HubFK).FirstOrDefault();
                }

                //Set the labels
                lblHubName.Text = currentHub.Name;

                //Set the state FK
                currentStateFK = currentHub.StateFK;
            }

            //Don't allow users to view BOQCWLTs from other hubs
            if (isEdit && !currentProgramRole.HubFKs.Contains(currentBOQCWLT.HubFK))
            {
                //Redirect the user to the dashboard with an error message
                Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", "NoBOQ"));
            }

            //Get the proper hub fk
            currentHubFK = (isEdit ? currentBOQCWLT.HubFK : currentProgramRole.CurrentHubFK.Value);

            if (!IsPostBack)
            {
                //Hide the master page title
                ((Dashboard)this.Master).HideTitle();

                //If this is an edit or view, populate the page with values
                if (isEdit)
                {
                    BindTeamMemberTagBox(currentBOQCWLT.FormDate, currentHubFK, string.Join(",", currentParticipants.Select(p => p.CWLTMemberFK)));
                    PopulatePage(currentBOQCWLT);
                }
                else
                {
                    BindTeamMemberTagBox(null, currentHubFK, null);
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
                    lblPageTitle.Text = "Add New Community-Wide Benchmarks of Quality Form";
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
                    lblPageTitle.Text = "Edit Community-Wide Benchmarks of Quality Form";
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
                    lblPageTitle.Text = "View Community-Wide Benchmarks of Quality Form";
                }

                //Set the max value for the form date field
                deFormDate.MaxDate = DateTime.Now;

                //Set focus on the form date field
                deFormDate.Focus();
            }
        }

        /// <summary>
        /// This method binds the team member tag box by getting all the team members
        /// that were active at the point of time passed to this method.
        /// </summary>
        /// <param name="formDate">The date and time to check against</param>
        /// <param name="hubFK">The Hub FK</param>
        /// <param name="valueToSelect">The value to be selected</param>
        private void BindTeamMemberTagBox(DateTime? formDate, int hubFK, string valueToSelect)
        {
            if (formDate.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get all the community leadership team members
                    var members = context.CWLTMember.AsNoTracking()
                                    .Where(sm => sm.HubFK == hubFK &&
                                            sm.StartDate <= formDate &&
                                            (sm.LeaveDate.HasValue == false || sm.LeaveDate.Value >= formDate))
                        .Select(sm => new
                        {
                            CWLTMemberPK = sm.CWLTMemberPK,
                            MemberIDAndName = "(" + sm.IDNumber + ") " + sm.FirstName + " " + sm.LastName
                        })
                        .OrderBy(sm => sm.MemberIDAndName)
                        .ToList();

                    //Bind the tag box
                    tbTeamMembers.DataSource = members;
                    tbTeamMembers.DataBind();
                }

                //Check to see how many team members there are
                if (tbTeamMembers.Items.Count > 0)
                {
                    //There is at least 1 member, enable the child dropdown
                    tbTeamMembers.ClientEnabled = true;

                    //Try to select the member(s) passed to this method
                    tbTeamMembers.Value = valueToSelect;
                }
                else
                {
                    //There are no members in the list, disable the control
                    tbTeamMembers.ClientEnabled = false;
                }
            }
            else
            {
                //No date was passed, clear and disable the control
                tbTeamMembers.Value = "";
                tbTeamMembers.ClientEnabled = false;
            }
        }

        /// <summary>
        /// This method fires when the user clicks the Save button in the
        /// submitBOQCWLT user control 
        /// </summary>
        /// <param name="sender">The submitBOQCWLT control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQCWLT_Click(object sender, EventArgs e)
        {
            //To hold the success message
            string successMessageType = SaveForm(true);

            //Only redirect if the save succeeded
            if (!string.IsNullOrWhiteSpace(successMessageType))
            {
                //Redirect the user to the dashboard with the success message
                Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", successMessageType));
            }
        }

        /// <summary>
        /// This method fires when the user clicks the cancel button in the
        /// submitBOQCWLT user control 
        /// </summary>
        /// <param name="sender">The submitBOQCWLT control</param>
        /// <param name="e">The Click event</param>
        protected void submitBOQCWLT_CancelClick(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("/Pages/CWLTDashboard.aspx?messageType={0}", "BOQCanceled"));
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitBOQCWLT control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitBOQCWLT_ValidationFailed(object sender, EventArgs e)
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
            if (ASPxEdit.AreEditorsValid(this.Page, submitBOQCWLT.ValidationGroup))
            {
                //Submit the form
                SaveForm(false);

                //Get the master page
                MasterPages.Dashboard masterPage = (MasterPages.Dashboard)Master;

                //Get the report
                Reports.PreBuiltReports.FormReports.RptBOQCWLT report = new Reports.PreBuiltReports.FormReports.RptBOQCWLT();

                //Display the report
                masterPage.DisplayReport(currentProgramRole, report, "Community-Wide Benchmarks of Quality", BOQCWLTPK);
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
            //Enable/disable controls
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
            deFormDate.ClientEnabled = enabled;
            tbTeamMembers.ClientEnabled = enabled;

            //Show/hide the submit button
            submitBOQCWLT.ShowSubmitButton = enabled;

            //Use cancel confirmation if the controls are enabled and
            //the customization option for cancel confirmation is true (default to true)
            bool? confirmationOption = UserCustomizationOption.GetBooleanCustomizationOptionFromCookie(UserCustomizationOption.CustomizationOptionCookie.CANCEL_CONFIRMATION_OPTION);
            bool areConfirmationsEnabled = (confirmationOption.HasValue ? confirmationOption.Value : true); //Default to true
            submitBOQCWLT.UseCancelConfirm = enabled && areConfirmationsEnabled;
        }

        /// <summary>
        /// This method populates the controls on the page with values
        /// from the passed BenchmarkOfQualityCWLT object
        /// </summary>
        /// <param name="boq">The BenchmarkOfQualityCWLT object to fill the page controls</param>
        private void PopulatePage(BenchmarkOfQualityCWLT boq)
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

            //Set the form date
            deFormDate.Date = boq.FormDate;

            //Set the team members
            tbTeamMembers.Value = string.Join(",", currentParticipants.Select(p => p.CWLTMemberFK));
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
                currentBOQCWLT.FormDate = deFormDate.Date;
                currentBOQCWLT.Indicator1 = Convert.ToInt32(ddIndicator1.Value);
                currentBOQCWLT.Indicator2 = Convert.ToInt32(ddIndicator2.Value);
                currentBOQCWLT.Indicator3 = Convert.ToInt32(ddIndicator3.Value);
                currentBOQCWLT.Indicator4 = Convert.ToInt32(ddIndicator4.Value);
                currentBOQCWLT.Indicator5 = Convert.ToInt32(ddIndicator5.Value);
                currentBOQCWLT.Indicator6 = Convert.ToInt32(ddIndicator6.Value);
                currentBOQCWLT.Indicator7 = Convert.ToInt32(ddIndicator7.Value);
                currentBOQCWLT.Indicator8 = Convert.ToInt32(ddIndicator8.Value);
                currentBOQCWLT.Indicator9 = Convert.ToInt32(ddIndicator9.Value);
                currentBOQCWLT.Indicator10 = Convert.ToInt32(ddIndicator10.Value);
                currentBOQCWLT.Indicator11 = Convert.ToInt32(ddIndicator11.Value);
                currentBOQCWLT.Indicator12 = Convert.ToInt32(ddIndicator12.Value);
                currentBOQCWLT.Indicator13 = Convert.ToInt32(ddIndicator13.Value);
                currentBOQCWLT.Indicator14 = Convert.ToInt32(ddIndicator14.Value);
                currentBOQCWLT.Indicator15 = Convert.ToInt32(ddIndicator15.Value);
                currentBOQCWLT.Indicator16 = Convert.ToInt32(ddIndicator16.Value);
                currentBOQCWLT.Indicator17 = Convert.ToInt32(ddIndicator17.Value);
                currentBOQCWLT.Indicator18 = Convert.ToInt32(ddIndicator18.Value);
                currentBOQCWLT.Indicator19 = Convert.ToInt32(ddIndicator19.Value);
                currentBOQCWLT.Indicator20 = Convert.ToInt32(ddIndicator20.Value);
                currentBOQCWLT.Indicator21 = Convert.ToInt32(ddIndicator21.Value);
                currentBOQCWLT.Indicator22 = Convert.ToInt32(ddIndicator22.Value);
                currentBOQCWLT.Indicator23 = Convert.ToInt32(ddIndicator23.Value);
                currentBOQCWLT.Indicator24 = Convert.ToInt32(ddIndicator24.Value);
                currentBOQCWLT.Indicator25 = Convert.ToInt32(ddIndicator25.Value);
                currentBOQCWLT.Indicator26 = Convert.ToInt32(ddIndicator26.Value);
                currentBOQCWLT.Indicator27 = Convert.ToInt32(ddIndicator27.Value);
                currentBOQCWLT.Indicator28 = Convert.ToInt32(ddIndicator28.Value);
                currentBOQCWLT.Indicator29 = Convert.ToInt32(ddIndicator29.Value);
                currentBOQCWLT.Indicator30 = Convert.ToInt32(ddIndicator30.Value);
                currentBOQCWLT.Indicator31 = Convert.ToInt32(ddIndicator31.Value);
                currentBOQCWLT.Indicator32 = Convert.ToInt32(ddIndicator32.Value);
                currentBOQCWLT.Indicator33 = Convert.ToInt32(ddIndicator33.Value);
                currentBOQCWLT.Indicator34 = Convert.ToInt32(ddIndicator34.Value);
                currentBOQCWLT.Indicator35 = Convert.ToInt32(ddIndicator35.Value);
                currentBOQCWLT.Indicator36 = Convert.ToInt32(ddIndicator36.Value);
                currentBOQCWLT.Indicator37 = Convert.ToInt32(ddIndicator37.Value);
                currentBOQCWLT.Indicator38 = Convert.ToInt32(ddIndicator38.Value);
                currentBOQCWLT.Indicator39 = Convert.ToInt32(ddIndicator39.Value);

                //Check to see if this is an add or edit
                if (isEdit)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an edit
                        successMessageType = "BOQEdited";

                        //Fill the edit-specific fields
                        currentBOQCWLT.EditDate = DateTime.Now;
                        currentBOQCWLT.Editor = User.Identity.Name;

                        //Get the existing database values
                        BenchmarkOfQualityCWLT existingBOQCWLT = context.BenchmarkOfQualityCWLT.Where(b => b.BenchmarkOfQualityCWLTPK == currentBOQCWLT.BenchmarkOfQualityCWLTPK).FirstOrDefault();

                        //Set the BOQCWLT object to the new values
                        context.Entry(existingBOQCWLT).CurrentValues.SetValues(currentBOQCWLT);

                        //Get the selected participants
                        List<int> selectedMembers = tbTeamMembers.Value.ToString().Split(',').Select(int.Parse).ToList();

                        //Fill the list of participants
                        foreach (int memberPK in selectedMembers)
                        {
                            BOQCWLTParticipant existingParticipant = currentParticipants.Where(p => p.CWLTMemberFK == memberPK).FirstOrDefault();

                            if (existingParticipant == null || existingParticipant.BOQCWLTParticipantPK == 0)
                            {
                                //Add missing participants
                                existingParticipant = new BOQCWLTParticipant();
                                existingParticipant.CreateDate = DateTime.Now;
                                existingParticipant.Creator = User.Identity.Name;
                                existingParticipant.BenchmarksOfQualityCWLTFK = existingBOQCWLT.BenchmarkOfQualityCWLTPK;
                                existingParticipant.CWLTMemberFK = memberPK;
                                context.BOQCWLTParticipant.Add(existingParticipant);
                            }
                        }

                        //To hold the participant PKs that will be removed
                        List<int> deletedParticipantPKs = new List<int>();

                        //Get all the participants that should no longer be linked
                        foreach (BOQCWLTParticipant participant in currentParticipants)
                        {
                            bool keepParticipant = selectedMembers.Exists(p => p == participant.CWLTMemberFK);

                            if (keepParticipant == false)
                            {
                                deletedParticipantPKs.Add(participant.BOQCWLTParticipantPK);
                            }
                        }

                        //Delete the particpant rows
                        context.BOQCWLTParticipant.Where(p => deletedParticipantPKs.Contains(p.BOQCWLTParticipantPK)).Delete();

                        //Save the changes
                        context.SaveChanges();

                        //Get the change rows
                        context.BOQCWLTParticipantChanged.Where(pc => deletedParticipantPKs.Contains(pc.BOQCWLTParticipantPK)).Update(pc => new BOQCWLTParticipantChanged() { Deleter = User.Identity.Name });

                        //Set the hidden field and local variable
                        hfBOQCWLTPK.Value = currentBOQCWLT.BenchmarkOfQualityCWLTPK.ToString();
                        BOQCWLTPK = currentBOQCWLT.BenchmarkOfQualityCWLTPK;
                    }
                }
                else
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //This is an add
                        successMessageType = "BOQAdded";

                        //Set the create-specific fields
                        currentBOQCWLT.CreateDate = DateTime.Now;
                        currentBOQCWLT.Creator = User.Identity.Name;
                        currentBOQCWLT.HubFK = currentProgramRole.CurrentHubFK.Value;

                        //Add the Benchmark to the database and save
                        context.BenchmarkOfQualityCWLT.Add(currentBOQCWLT);
                        context.SaveChanges();

                        //Get the selected participants
                        List<int> selectedParticipants = tbTeamMembers.Value.ToString().Split(',').Select(int.Parse).ToList();

                        //Fill the list of participants
                        foreach (int memberPK in selectedParticipants)
                        {
                            BOQCWLTParticipant newParticipant = new BOQCWLTParticipant();

                            newParticipant.CreateDate = DateTime.Now;
                            newParticipant.Creator = User.Identity.Name;
                            newParticipant.BenchmarksOfQualityCWLTFK = currentBOQCWLT.BenchmarkOfQualityCWLTPK;
                            newParticipant.CWLTMemberFK = memberPK;
                            context.BOQCWLTParticipant.Add(newParticipant);
                        }

                        //Save the participants
                        context.SaveChanges();

                        //Set the hidden field and local variable
                        hfBOQCWLTPK.Value = currentBOQCWLT.BenchmarkOfQualityCWLTPK.ToString();
                        BOQCWLTPK = currentBOQCWLT.BenchmarkOfQualityCWLTPK;
                    }
                }
            }
            else if (showMessages)
            {
                msgSys.ShowMessageToUser("danger", "Error", "You are not authorized to make changes!", 120000);
            }

            //Return the success message type
            return successMessageType;
        }

        /// <summary>
        /// This method fires when the value in the deFormDate DateEdit changes
        /// and it updates the necessary fields
        /// </summary>
        /// <param name="sender">The deFormDate DateEdit</param>
        /// <param name="e">The ValueChanged event</param>
        protected void deFormDate_ValueChanged(object sender, EventArgs e)
        {
            //Get the form date and child FK
            DateTime? formDate = (deFormDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deFormDate.Value));
            string selectedMembers = (tbTeamMembers.Value == null ? null : tbTeamMembers.Value.ToString());

            //Bind the team member tag box
            BindTeamMemberTagBox(formDate, currentHubFK, selectedMembers);
        }
    }
}