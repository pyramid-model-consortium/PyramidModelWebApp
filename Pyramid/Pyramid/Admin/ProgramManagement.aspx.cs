using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Pyramid.Code;
using DevExpress.Web;
using DevExpress.Web.Bootstrap;

namespace Pyramid.Admin
{
    public partial class ProgramManagement : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's selected role
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Hide the states if the user is not a super admin
            if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                divStates.Visible = false;

            if (!IsPostBack)
            {
                //Bind the cohort, hub, and state data-bound controls
                BindCohorts();
                BindHubs();
                BindPrograms();
                BindStates();

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program types
                    var programTypes = context.CodeProgramType.AsNoTracking().OrderBy(cpt => cpt.OrderBy).ToList();
                    lstBxProgramType.DataSource = programTypes;
                    lstBxProgramType.DataBind();
                }
            }
        }

        /// <summary>
        /// Bind the state repeater and dropdowns
        /// </summary>
        private void BindStates()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all the states
                List<State> allStates;
                if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                {
                    allStates = context.State
                                    .Include(s => s.Program)
                                    .Where(s => s.StatePK == currentProgramRole.StateFK)
                                    .OrderBy(s => s.Name)
                                    .AsNoTracking()
                                    .ToList();
                }
                else
                {
                    allStates = context.State
                                    .Include(s => s.Program)
                                    .OrderBy(s => s.Name)
                                    .AsNoTracking()
                                    .ToList();
                }

                //Bind the state drop-downs
                ddProgramState.DataSource = allStates;
                ddProgramState.DataBind();

                ddHubState.DataSource = allStates;
                ddHubState.DataBind();

                ddCohortState.DataSource = allStates;
                ddCohortState.DataBind();
            }
        }

        /// <summary>
        /// Bind the cohort repeater and dropdowns
        /// </summary>
        private void BindCohorts()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all cohorts
                List<Cohort> allCohorts;
                if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                {
                    allCohorts = context.Cohort
                                    .Include(c => c.Program)
                                    .Where(c => c.StateFK == currentProgramRole.StateFK)
                                    .OrderBy(c => c.CohortName)
                                    .AsNoTracking()
                                    .ToList();
                }
                else
                {
                    allCohorts = context.Cohort
                                    .Include(c => c.Program)
                                    .OrderBy(c => c.CohortName)
                                    .AsNoTracking()
                                    .ToList();
                }

                //Bind the cohort gridview
                bsGRCohort.DataBind();

                //Bind the cohort drop-down
                ddProgramCohort.DataSource = allCohorts;
                ddProgramCohort.DataBind();
            }
        }

        /// <summary>
        /// Bind the hub repeater and dropdowns
        /// </summary>
        private void BindHubs()
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get all hubs
                List<Hub> allHubs;
                if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
                {
                    allHubs = context.Hub
                                .Include(h => h.Program)
                                .Where(h => h.StateFK == currentProgramRole.StateFK)
                                .OrderBy(h => h.Name)
                                .AsNoTracking()
                                .ToList();
                }
                else
                {
                    allHubs = context.Hub
                                .Include(h => h.Program)
                                .OrderBy(h => h.Name)
                                .AsNoTracking()
                                .ToList();
                }

                //Bind the hub gridview
                bsGRHubs.DataBind();

                //Bind the hub drop-down
                ddProgramHub.DataSource = allHubs;
                ddProgramHub.DataBind();
            }
        }

        /// <summary>
        /// Bind the programs repeater
        /// </summary>
        private void BindPrograms()
        {
            bsGRPrograms.DataBind();
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the hubs
        /// and it opens a div that allows the user to add a hub
        /// </summary>
        /// <param name="sender">The lbAddHub LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddHub_Click(object sender, EventArgs e)
        {
            //Clear inputs in the hub div
            hfAddEditHubPK.Value = "0";
            txtHubName.Value = "";
            ddHubState.Value = "";

            //Set the title
            lblAddEditHub.Text = "Add Hub";

            //Show the hub div
            divAddEditHub.Visible = true;

            //Set focus to the hub name field
            txtHubName.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for the hubs
        /// and it opens the hub edit div so that the user can edit the selected hub
        /// </summary>
        /// <param name="sender">The lbEditHub LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditHub_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the container item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfHubPK = (HiddenField)item.FindControl("hfHubPK");

            //Get the PK from the hidden field
            int? hubPK = (String.IsNullOrWhiteSpace(hfHubPK.Value) ? (int?)null : Convert.ToInt32(hfHubPK.Value));

            if (hubPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the hub to edit
                    Hub editHub = context.Hub.Find(hubPK.Value);

                    //Fill the inputs
                    lblAddEditHub.Text = "Edit Hub";
                    txtHubName.Value = editHub.Name;
                    ddHubState.SelectedItem = ddHubState.Items.FindByValue(editHub.StateFK);
                    hfAddEditHubPK.Value = hubPK.Value.ToString();
                }

                //Show the hub div
                divAddEditHub.Visible = true;

                //Set focus to the hub name field
                txtHubName.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected hub!", 20000);

            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the hubs
        /// and it saves the hub information to the database
        /// </summary>
        /// <param name="sender">The submitHub control</param>
        /// <param name="e">The Click event</param>
        protected void submitHub_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitHub.ValidationGroup))
            {
                //Get the hub PK and name
                int hubPK = Convert.ToInt32(hfAddEditHubPK.Value);
                string hubName = txtHubName.Value.ToString();

                using (PyramidContext context = new PyramidContext())
                {
                    Hub currentHub;
                    //Check to see if this is an add or an edit
                    if (hubPK == 0)
                    {
                        //Add
                        currentHub = new Hub();
                        currentHub.Name = hubName;
                        currentHub.StateFK = Convert.ToInt32(ddHubState.Value);
                        currentHub.CreateDate = DateTime.Now;
                        currentHub.Creator = User.Identity.Name;

                        //Save to the database
                        context.Hub.Add(currentHub);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added hub!", 10000);


                    }
                    else
                    {
                        //Edit
                        currentHub = context.Hub.Find(hubPK);
                        currentHub.Name = hubName;
                        currentHub.StateFK = Convert.ToInt32(ddHubState.Value);
                        currentHub.EditDate = DateTime.Now;
                        currentHub.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited hub!", 10000);
                    }

                    //Reset the values
                    hfAddEditHubPK.Value = "0";
                    divAddEditHub.Visible = false;

                    //Bind the hub controls
                    BindHubs();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a hub
        /// and it deletes the hub information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteHub LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteHub_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteHubPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteHubPK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the hub to remove
                        Hub hubToRemove = context.Hub.Where(h => h.HubPK == rowToRemovePK).FirstOrDefault();
                        context.Hub.Remove(hubToRemove);
                        context.SaveChanges();

                        //Re-bind the hub controls
                        BindHubs();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted hub!", 10000);
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
                            msgSys.ShowMessageToUser("danger", "Error", "Could not delete the hub, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the hub!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the hub!", 120000);
                    }

                    //Log the error
                    Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the hub to delete!", 120000);

            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the hubs
        /// and it closes the hub add/edit div
        /// </summary>
        /// <param name="sender">The submitHub control</param>
        /// <param name="e">The Click event</param>
        protected void submitHub_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditHubPK.Value = "0";
            divAddEditHub.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitHub control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitHub_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the cohorts
        /// and it opens a div that allows the user to add a cohort
        /// </summary>
        /// <param name="sender">The lbAddCohort LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddCohort_Click(object sender, EventArgs e)
        {
            //Clear inputs in the cohort div
            hfAddEditCohortPK.Value = "0";
            txtCohortName.Value = "";
            deCohortStartDate.Value = "";
            deCohortEndDate.Value = "";
            ddCohortState.Value = "";

            //Set the title
            lblAddEditCohort.Text = "Add cohort";

            //Show the cohort div
            divAddEditCohort.Visible = true;

            //Set focus to the cohort name field
            txtCohortName.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a cohort
        /// and it opens the cohort edit div so that the user can edit the selected cohort
        /// </summary>
        /// <param name="sender">The lbEditCohort LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditCohort_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfCohortPK = (HiddenField)item.FindControl("hfCohortPK");

            //Get the PK from the hidden field
            int? cohortPK = (String.IsNullOrWhiteSpace(hfCohortPK.Value) ? (int?)null : Convert.ToInt32(hfCohortPK.Value));

            if (cohortPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the cohort to edit
                    Cohort editCohort = context.Cohort.Find(cohortPK.Value);

                    //Fill the inputs
                    lblAddEditCohort.Text = "Edit Cohort";
                    txtCohortName.Value = editCohort.CohortName;
                    deCohortStartDate.Value = editCohort.StartDate.ToString("MM/dd/yyyy");
                    deCohortEndDate.Value = (editCohort.EndDate.HasValue ? editCohort.EndDate.Value.ToString("MM/dd/yyyy") : "");
                    ddCohortState.SelectedItem = ddCohortState.Items.FindByValue(editCohort.StateFK);
                    hfAddEditCohortPK.Value = cohortPK.Value.ToString();
                }

                //Show the cohort div
                divAddEditCohort.Visible = true;

                //Set focus to the cohort name field
                txtCohortName.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected cohort!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the cohorts
        /// and it saves the cohort information to the database
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event</param>
        protected void submitCohort_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitCohort.ValidationGroup))
            {
                //Get the cohort information
                int cohortPK = Convert.ToInt32(hfAddEditCohortPK.Value);
                string cohortName = txtCohortName.Value.ToString();
                DateTime startDate = Convert.ToDateTime(deCohortStartDate.Value);
                DateTime? endDate = (String.IsNullOrWhiteSpace(deCohortEndDate.Value.ToString()) ? (DateTime?)null : Convert.ToDateTime(deCohortEndDate.Value));

                using (PyramidContext context = new PyramidContext())
                {
                    Cohort currentCohort;
                    //Check to see if this is an add or an edit
                    if (cohortPK == 0)
                    {
                        //Add
                        currentCohort = new Cohort();
                        currentCohort.CohortName = cohortName;
                        currentCohort.StartDate = startDate;
                        currentCohort.EndDate = endDate;
                        currentCohort.StateFK = Convert.ToInt32(ddCohortState.Value);
                        currentCohort.CreateDate = DateTime.Now;
                        currentCohort.Creator = User.Identity.Name;

                        //Save to the database
                        context.Cohort.Add(currentCohort);
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added cohort!", 10000);


                    }
                    else
                    {
                        //Edit
                        currentCohort = context.Cohort.Find(cohortPK);
                        currentCohort.CohortName = cohortName;
                        currentCohort.StartDate = startDate;
                        currentCohort.EndDate = endDate;
                        currentCohort.StateFK = Convert.ToInt32(ddCohortState.Value);
                        currentCohort.EditDate = DateTime.Now;
                        currentCohort.Editor = User.Identity.Name;

                        //Save to the database
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited cohort!", 10000);
                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditCohortPK.Value = "0";
                    divAddEditCohort.Visible = false;

                    //Re-bind the cohort controls
                    BindCohorts();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a cohort
        /// and it deletes the cohort information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteCohort LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteCohort_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteCohortPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteCohortPK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the cohort to remove
                        Cohort cohortToRemove = context.Cohort.Where(h => h.CohortPK == rowToRemovePK).FirstOrDefault();
                        context.Cohort.Remove(cohortToRemove);
                        context.SaveChanges();

                        //Rebind the cohort controls
                        BindCohorts();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted cohort!", 10000);
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
                            msgSys.ShowMessageToUser("danger", "Error", "Could not delete the cohort, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the cohort!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the cohort!", 120000);
                    }

                    //Log the error
                    Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the cohort to delete!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the cohorts
        /// and it closes the cohort add/edit div
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event</param>
        protected void submitCohort_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditCohortPK.Value = "0";
            divAddEditCohort.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitCohort control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitCohort_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method executes when the user clicks the add button for the programs
        /// and it opens a div that allows the user to add a program
        /// </summary>
        /// <param name="sender">The lbAddProgram LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbAddProgram_Click(object sender, EventArgs e)
        {
            //Clear inputs in the program div
            hfAddEditProgramPK.Value = "0";
            txtProgramName.Value = "";
            txtProgramLocation.Value = "";
            deProgramStartDate.Value = "";
            deProgramEndDate.Value = "";
            ddProgramState.Value = "";
            ddProgramHub.Value = "";
            ddProgramCohort.Value = "";

            //Clear the program types
            foreach (BootstrapListEditItem item in lstBxProgramType.Items)
            {
                item.Selected = false;
            }


            //Set the title
            lblAddEditProgram.Text = "Add Program";

            //Show the program div
            divAddEditProgram.Visible = true;

            //Set focus to the program name field
            txtProgramName.Focus();
        }

        /// <summary>
        /// This method executes when the user clicks the edit button for a program
        /// and it opens the program edit div so that the user can edit the selected program
        /// </summary>
        /// <param name="sender">The lbEditProgram LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbEditProgram_Click(object sender, EventArgs e)
        {
            //Get the calling button
            LinkButton editButton = (LinkButton)sender;

            //Get the specific repeater item
            GridViewDataItemTemplateContainer item = (GridViewDataItemTemplateContainer)editButton.Parent;

            //Get the hidden field with the PK for deletion
            HiddenField hfProgramPK = (HiddenField)item.FindControl("hfProgramPK");

            //Get the PK from the hidden field
            int? programPK = (String.IsNullOrWhiteSpace(hfProgramPK.Value) ? (int?)null : Convert.ToInt32(hfProgramPK.Value));

            if (programPK.HasValue)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the program to edit
                    Program editProgram = context.Program.Find(programPK.Value);

                    //Fill the inputs
                    lblAddEditProgram.Text = "Edit Program";
                    txtProgramName.Value = editProgram.ProgramName;
                    txtProgramLocation.Value = editProgram.Location;
                    deProgramStartDate.Value = editProgram.ProgramStartDate;
                    deProgramEndDate.Value = editProgram.ProgramEndDate;
                    ddProgramCohort.SelectedItem = ddProgramCohort.Items.FindByValue(editProgram.CohortFK);
                    ddProgramHub.SelectedItem = ddProgramHub.Items.FindByValue(editProgram.HubFK);
                    ddProgramState.SelectedItem = ddProgramState.Items.FindByValue(editProgram.StateFK);
                    hfAddEditProgramPK.Value = programPK.Value.ToString();

                    //Select the program types
                    var editProgramTypes = context.ProgramType.Include(pt => pt.CodeProgramType).Where(pt => pt.ProgramFK == programPK).ToList();
                    foreach (ProgramType pt in editProgramTypes)
                    {
                        lstBxProgramType.Items.FindByValue(pt.CodeProgramType.CodeProgramTypePK).Selected = true;
                    }
                }

                //Show the program div
                divAddEditProgram.Visible = true;

                //Set focus to the program name field
                txtProgramName.Focus();
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "An error occurred while attempting to load the selected program!", 20000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the save button for the programs
        /// and it saves the program information to the database
        /// </summary>
        /// <param name="sender">The submitProgram control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgram_Click(object sender, EventArgs e)
        {
            if (ASPxEdit.AreEditorsValid(this, submitProgram.ValidationGroup))
            {
                //Get the program information
                int programPK = Convert.ToInt32(hfAddEditProgramPK.Value);

                using (PyramidContext context = new PyramidContext())
                {
                    Program currentProgram;
                    //Check to see if this is an add or an edit
                    if (programPK == 0)
                    {
                        //Add
                        currentProgram = new Program();
                        currentProgram.ProgramName = txtProgramName.Value.ToString();
                        currentProgram.Location = txtProgramLocation.Value.ToString();
                        currentProgram.ProgramStartDate = (Convert.ToDateTime(deProgramStartDate.Value));
                        currentProgram.ProgramEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));
                        currentProgram.CohortFK = Convert.ToInt32(ddProgramCohort.Value);
                        currentProgram.HubFK = Convert.ToInt32(ddProgramHub.Value);
                        currentProgram.StateFK = Convert.ToInt32(ddProgramState.Value);
                        currentProgram.CreateDate = DateTime.Now;
                        currentProgram.Creator = User.Identity.Name;

                        //Save to the database
                        context.Program.Add(currentProgram);
                        context.SaveChanges();

                        //Clear the program type rows
                        var currentProgramTypes = context.ProgramType.Where(pt => pt.ProgramFK == currentProgram.ProgramPK).ToList();
                        context.ProgramType.RemoveRange(currentProgramTypes);

                        //Save the program type rows
                        foreach (BootstrapListEditItem item in lstBxProgramType.Items)
                        {
                            if (item.Selected)
                            {
                                ProgramType newProgramType = new ProgramType();
                                newProgramType.CreateDate = DateTime.Now;
                                newProgramType.Creator = User.Identity.Name;
                                newProgramType.ProgramFK = currentProgram.ProgramPK;
                                newProgramType.TypeCodeFK = Convert.ToInt32(item.Value);
                                context.ProgramType.Add(newProgramType);
                            }
                        }
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully added program!", 10000);
                    }
                    else
                    {
                        //Edit
                        currentProgram = context.Program.Find(programPK);
                        currentProgram.ProgramName = txtProgramName.Value.ToString();
                        currentProgram.Location = txtProgramLocation.Value.ToString();
                        currentProgram.ProgramStartDate = (Convert.ToDateTime(deProgramStartDate.Value));
                        currentProgram.ProgramEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));
                        currentProgram.CohortFK = Convert.ToInt32(ddProgramCohort.Value);
                        currentProgram.HubFK = Convert.ToInt32(ddProgramHub.Value);
                        currentProgram.StateFK = Convert.ToInt32(ddProgramState.Value);
                        currentProgram.EditDate = DateTime.Now;
                        currentProgram.Editor = User.Identity.Name;

                        //Clear the program type rows
                        var currentProgramTypes = context.ProgramType.Where(pt => pt.ProgramFK == currentProgram.ProgramPK).ToList();
                        context.ProgramType.RemoveRange(currentProgramTypes);

                        //Save the program type rows
                        foreach (BootstrapListEditItem item in lstBxProgramType.Items)
                        {
                            if (item.Selected)
                            {
                                ProgramType newProgramType = new ProgramType();
                                newProgramType.CreateDate = DateTime.Now;
                                newProgramType.Creator = User.Identity.Name;
                                newProgramType.ProgramFK = currentProgram.ProgramPK;
                                newProgramType.TypeCodeFK = Convert.ToInt32(item.Value);
                                context.ProgramType.Add(newProgramType);
                            }
                        }
                        context.SaveChanges();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully edited program!", 10000);


                    }

                    //Reset the values in the hidden field and hide the div
                    hfAddEditProgramPK.Value = "0";
                    divAddEditProgram.Visible = false;

                    //Re-bind the program controls
                    BindPrograms();
                }
            }
        }

        /// <summary>
        /// This method executes when the user clicks the delete button for a program
        /// and it deletes the program information from the database
        /// </summary>
        /// <param name="sender">The lbDeleteProgram LinkButton</param>
        /// <param name="e">The Click event</param>
        protected void lbDeleteProgram_Click(object sender, EventArgs e)
        {
            //Get the PK from the hidden field
            int? rowToRemovePK = (String.IsNullOrWhiteSpace(hfDeleteProgramPK.Value) ? (int?)null : Convert.ToInt32(hfDeleteProgramPK.Value));

            //Remove the role if the PK is not null
            if (rowToRemovePK != null)
            {
                try
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Get the program to remove
                        Program programToRemove = context.Program.Where(h => h.ProgramPK == rowToRemovePK).FirstOrDefault();

                        //Get the program type rows to remove
                        var programTypesToRemove = context.ProgramType.Where(pt => pt.ProgramFK == rowToRemovePK).ToList();

                        //Remove the program type rows
                        context.ProgramType.RemoveRange(programTypesToRemove);

                        //Remove the program
                        context.Program.Remove(programToRemove);
                        context.SaveChanges();

                        //Rebind the program controls
                        BindPrograms();

                        //Show a success message
                        msgSys.ShowMessageToUser("success", "Success", "Successfully deleted program!", 10000);
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
                            msgSys.ShowMessageToUser("danger", "Error", "Could not delete the program, there are related records in the system!<br/><br/>If you do not know what related records exist, please contact tech support via ticket.", 120000);
                        }
                        else
                        {
                            msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the program!", 120000);
                        }
                    }
                    else
                    {
                        msgSys.ShowMessageToUser("danger", "Error", "An error occurred while deleting the program!", 120000);
                    }

                    //Log the error
                    Elmah.ErrorSignal.FromCurrentContext().Raise(dbUpdateEx);
                }
            }
            else
            {
                msgSys.ShowMessageToUser("danger", "Error", "Could not find the program to delete!", 120000);
            }
        }

        /// <summary>
        /// This method executes when the user clicks the cancel button for the programs
        /// and it closes the program add/edit div
        /// </summary>
        /// <param name="sender">The submitProgram control</param>
        /// <param name="e">The Click event</param>
        protected void submitProgram_CancelClick(object sender, EventArgs e)
        {
            //Clear the necessary values
            hfAddEditProgramPK.Value = "0";
            divAddEditProgram.Visible = false;
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitProgram control</param>
        /// <param name="e">The Click event from the submit click</param>
        protected void submitProgram_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the validation for the deCohortEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deCohortEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deCohortEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end dates
            DateTime? cohortStartDate = (deCohortStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deCohortStartDate.Value));
            DateTime? cohortEndDate = (deCohortEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deCohortEndDate.Value));

            //Validate the end date
            if(cohortEndDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Cohort End Date is required!";
            }
            else if(cohortEndDate.HasValue && cohortStartDate.HasValue 
                        && cohortStartDate.Value >= cohortEndDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Cohort End Date must be after the Cohort Start Date!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the deProgramEndDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deProgramEndDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deProgramEndDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the start and end dates
            DateTime? programStartDate = (deProgramStartDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramStartDate.Value));
            DateTime? programEndDate = (deProgramEndDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deProgramEndDate.Value));

            //Validate the end date
            if (programEndDate.HasValue && programStartDate.HasValue 
                        && programStartDate.Value >= programEndDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Program End Date must be after the Program Start Date!";
            }
        }

        /// <summary>
        /// This method fires when the data source for the state DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efStateDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efStateDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "StatePK";
        }

        /// <summary>
        /// This method fires when the data source for the hub DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efHubDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efHubDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "HubPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                e.QueryableSource = context.Hub.Include(h => h.Program).Where(h => h.StateFK == currentProgramRole.StateFK).AsNoTracking();
            }
            else
            {
                e.QueryableSource = context.Hub.Include(h => h.Program).AsNoTracking();
            }
        }

        /// <summary>
        /// This method fires when the data source for the cohort DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efCohortDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efCohortDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "CohortPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                e.QueryableSource = context.Cohort.Include(h => h.Program).Where(h => h.StateFK == currentProgramRole.StateFK).AsNoTracking();
            }
            else
            {
                e.QueryableSource = context.Cohort.Include(h => h.Program).AsNoTracking();
            }
        }

        /// <summary>
        /// This method fires when the data source for the program DevExpress GridView is selecting
        /// and it handles the select
        /// </summary>
        /// <param name="sender">The efProgramDataSource control</param>
        /// <param name="e">The LinqServerModeDataSourceSelectEventArgs event</param>
        protected void efProgramDataSource_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            //Set the primary key
            e.KeyExpression = "ProgramPK";

            //Set the source to a LINQ query
            PyramidContext context = new PyramidContext();
            if (currentProgramRole.RoleFK.Value != (int)Utilities.ProgramRoleFKs.SUPER_ADMIN)
            {
                e.QueryableSource = from p in context.Program
                                    join pt in context.ProgramType on p.ProgramPK equals pt.ProgramFK into types
                                    where p.StateFK == currentProgramRole.StateFK
                                    select new {
                                        p.ProgramPK,
                                        p.ProgramName,
                                        p.Location,
                                        HubName = p.Hub.Name,
                                        CohortName = p.Cohort.CohortName,
                                        StateName = p.State.Name,
                                        ProgramTypes = from pt in types select pt.CodeProgramType.Description
                                    };
            }
            else
            {
                e.QueryableSource = from p in context.Program
                                    join pt in context.ProgramType on p.ProgramPK equals pt.ProgramFK into types
                                    select new
                                    {
                                        p.ProgramPK,
                                        p.ProgramName,
                                        p.Location,
                                        HubName = p.Hub.Name,
                                        CohortName = p.Cohort.CohortName,
                                        StateName = p.State.Name,
                                        ProgramTypes = from pt in types select pt.CodeProgramType.Description
                                    };
            }
        }
    }
}