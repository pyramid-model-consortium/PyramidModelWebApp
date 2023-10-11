using DevExpress.Web;
using Microsoft.Ajax.Utilities;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Pyramid.Code;
using System.IO;

namespace Pyramid.User_Controls
{
    public partial class Child : System.Web.UI.UserControl
    {
        private string validationMessage;

        public string ValidationGroup
        {
            get
            {
                return "vgChild";
            }
        }

        public bool IsValid
        {
            get
            {
                return ASPxEdit.AreEditorsValid(this, ValidationGroup);
            }
        }

        public string ValidationMessageToDisplay
        {
            get
            {
                return validationMessage;
            }
            private set
            {
                validationMessage = value;
            }
        }

        public DateTime? EnrollmentDate
        {
            get
            {
                return (deEnrollmentDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEnrollmentDate.Value));
            }
        }

        public DateTime? DischargeDate
        {
            get
            {
                return (deDischargeDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDischargeDate.Value));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Set the max dates for the DateEdits (min dates are declared in markup)
                deDOB.MaxDate = DateTime.Now;
                deEnrollmentDate.MaxDate = DateTime.Now;
                deDischargeDate.MaxDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Set focus onto the first name field
        /// </summary>
        public void FocusFirstName()
        {
            txtFirstName.Focus();
        }

        /// <summary>
        /// This method returns the Child object filled with information from
        /// the inputs in this control so that the content page can interact with it
        /// </summary>
        /// <returns>The filled Child object if validation succeeds, null otherwise</returns>
        public Models.Child GetChild()
        {
            if (ASPxEdit.AreEditorsValid(this, ValidationGroup))
            {
                //Get the child pk
                int childPK = Convert.ToInt32(hfChildPK.Value);

                //To hold the child object
                Models.Child child;

                //Determine if the child already exists
                if (childPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        child = context.Child.AsNoTracking().Where(c => c.ChildPK == childPK).FirstOrDefault();
                    }
                }
                else
                {
                    child = new Models.Child();
                }

                //Set the values from the inputs
                child.FirstName = txtFirstName.Value.ToString();
                child.LastName = txtLastName.Value.ToString();
                child.BirthDate = Convert.ToDateTime(deDOB.Value);
                child.GenderCodeFK = Convert.ToInt32(ddGender.Value);
                child.GenderSpecify = (string.IsNullOrWhiteSpace(txtGenderSpecify.Text) ? null : txtGenderSpecify.Text);
                child.EthnicityCodeFK = Convert.ToInt32(ddEthnicity.Value);
                child.EthnicitySpecify = (string.IsNullOrWhiteSpace(txtEthnicitySpecify.Text) ? null : txtEthnicitySpecify.Text);
                child.RaceCodeFK = Convert.ToInt32(ddRace.Value);
                child.RaceSpecify = (string.IsNullOrWhiteSpace(txtRaceSpecify.Text) ? null : txtRaceSpecify.Text);

                //Return the object
                return child;
            }
            else
            {
                if (String.IsNullOrWhiteSpace(ValidationMessageToDisplay))
                {
                    ValidationMessageToDisplay = "Validation failed, see above for details!";
                }
                return null;
            }
        }

        /// <summary>
        /// This method returns the ChildProgram object filled with information from
        /// the inputs in this control so that the content page can interact with it
        /// </summary>
        /// <param name="uploadPermissionFile">A boolean that indicates if the permission file should be uploaded.</param>
        /// <returns>The filled ChildProgram object if validation succeeds, null otherwise</returns>
        public Models.ChildProgram GetChildProgram(bool uploadPermissionFile)
        {
            if (ASPxEdit.AreEditorsValid(this, ValidationGroup))
            {
                //Get the ChildProgram pk
                int childProgramPK = Convert.ToInt32(hfChildProgramPK.Value);

                //To hold the ChildProgram object
                Models.ChildProgram currentChildProgram;

                //Determine if the object already exists
                if (childProgramPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        currentChildProgram = context.ChildProgram.AsNoTracking().Where(cp => cp.ChildProgramPK == childProgramPK).FirstOrDefault();
                    }

                    if (currentChildProgram == null)
                    {
                        currentChildProgram = new Models.ChildProgram();
                    }
                }
                else
                {
                    currentChildProgram = new Models.ChildProgram();
                }

                //Set the values
                currentChildProgram.ProgramFK = Convert.ToInt32(hfProgramFK.Value);
                currentChildProgram.EnrollmentDate = Convert.ToDateTime(deEnrollmentDate.Value);
                currentChildProgram.DischargeDate = (deDischargeDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDischargeDate.Value));
                currentChildProgram.DischargeCodeFK = (ddDischargeReason.Value == null ? (int?)null : Convert.ToInt32(ddDischargeReason.Value));
                currentChildProgram.DischargeReasonSpecify = (txtDischargeReasonSpecify.Value == null ? null : txtDischargeReasonSpecify.Value.ToString());
                currentChildProgram.HasIEP = Convert.ToBoolean(ddIEP.Value);
                currentChildProgram.IsDLL = Convert.ToBoolean(ddDLL.Value);
                currentChildProgram.HasParentPermission = Convert.ToBoolean(ddHasParentPermission.Value);

                string programID;
                if (!string.IsNullOrWhiteSpace(txtProgramID.Text))
                {
                    //Use the ID that the user provided
                    //Make sure to trim the input to ensure leading and trailing spaces are removed
                    currentChildProgram.ProgramSpecificID = txtProgramID.Text.Trim();
                }
                else
                {
                    //The user didn't specify an ID, generate one
                    //Check to see if this is an edit
                    if (currentChildProgram.ChildProgramPK > 0)
                    {
                        //This is an edit, use the current PK
                        programID = string.Format("CID-{0}", currentChildProgram.ChildProgramPK);
                    }
                    else
                    {
                        //To hold the previous PK
                        int previousPK;

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the previous PK
                            Models.ChildProgram newestChildProgram = context.ChildProgram
                                                                                .AsNoTracking()
                                                                                .OrderByDescending(pe => pe.ChildProgramPK)
                                                                                .FirstOrDefault();
                            previousPK = (newestChildProgram != null ? newestChildProgram.ChildProgramPK : 0);

                            //Set the program specific ID to the previous PK plus one
                            programID = string.Format("CID-{0}", (previousPK + 1));
                        }
                    }
                    currentChildProgram.ProgramSpecificID = programID;
                }


                //Check to see whether the parent permission file should be uploaded
                if (uploadPermissionFile)
                {
                    //Get the permission file to upload
                    UploadedFile uploadedPermissionFile = bucParentPermissionDocument.UploadedFiles[0];

                    //Check to see if the file is valid
                    if (uploadedPermissionFile.ContentLength > 0 && uploadedPermissionFile.IsValid)
                    {
                        //If there is an existing file, delete it
                        if (!string.IsNullOrWhiteSpace(currentChildProgram.ParentPermissionDocumentFileName))
                        {
                            Utilities.DeleteFileFromAzureStorage(currentChildProgram.ParentPermissionDocumentFileName,
                                Utilities.ConstantAzureStorageContainerName.CHILD_FORM_UPLOADS.ToString());
                        }

                        //Get the actual file name
                        string fileName = Path.GetFileNameWithoutExtension(uploadedPermissionFile.FileName) + "-" +
                            Path.GetRandomFileName().Substring(0, 6) +
                            Path.GetExtension(uploadedPermissionFile.FileName);

                        //Upload the file to Azure storage
                        string filePath = Utilities.UploadFileToAzureStorage(uploadedPermissionFile.FileBytes, fileName,
                                            Utilities.ConstantAzureStorageContainerName.CHILD_FORM_UPLOADS.ToString());

                        //Set the file name and path
                        currentChildProgram.ParentPermissionDocumentFileName = fileName;
                        currentChildProgram.ParentPermissionDocumentFilePath = filePath;
                    }
                }

                //Return the object
                return currentChildProgram;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ValidationMessageToDisplay))
                {
                    ValidationMessageToDisplay = "Validation failed, see above for details!";
                }
                return null;
            }
        }

        /// <summary>
        /// This method takes a primary key and it fills the inputs in the
        /// control with the information in the Child table for that primary key
        /// </summary>
        /// <param name="childProgramPK">The primary key of the ChildProgram record</param>
        /// <param name="programFK">The primary key for the program which this child will be in</param>
        /// <param name="readOnly">True if the user is only allowed to view the values</param>
        /// <param name="showPrivateInfo">True if the user is allowed to view private info</param>
        public void InitializeWithData(int childProgramPK, int programFK, bool readOnly, bool showPrivateInfo)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Fill the Gender drop-down
                var genders = context.CodeGender.AsNoTracking().OrderBy(cg => cg.OrderBy).ToList();
                ddGender.DataSource = genders;
                ddGender.DataBind();

                //Fill the Ethnicity drop-down
                var ethnicities = context.CodeEthnicity.AsNoTracking().OrderBy(ce => ce.OrderBy).ToList();
                ddEthnicity.DataSource = ethnicities;
                ddEthnicity.DataBind();

                //Fill the Race drop-down
                var races = context.CodeRace.AsNoTracking().OrderBy(cr => cr.OrderBy).ToList();
                ddRace.DataSource = races;
                ddRace.DataBind();

                //Fill the Discharge Reason drop-down
                var dischargeReasons = context.CodeDischargeReason.AsNoTracking().OrderBy(cdr => cdr.OrderBy).ToList();
                ddDischargeReason.DataSource = dischargeReasons;
                ddDischargeReason.DataBind();

                //Set the program fk hidden field
                hfProgramFK.Value = programFK.ToString();

                if (childProgramPK > 0)
                {
                    //If this is an edit, fill the form with the child's information
                    //Get the objects
                    Models.ChildProgram childProgram = context.ChildProgram.AsNoTracking()
                                                            .Include(cp => cp.Child)
                                                            .Where(cp => cp.ChildProgramPK == childProgramPK)
                                                            .FirstOrDefault();
                    Models.Child child = (childProgram == null ? new Models.Child() : childProgram.Child);

                    //Fill the hidden fields
                    hfChildPK.Value = child.ChildPK.ToString();
                    hfChildProgramPK.Value = childProgram.ChildProgramPK.ToString();

                    //Fill the input fields
                    txtFirstName.Value = (showPrivateInfo ? child.FirstName : "HIDDEN");
                    txtLastName.Value = (showPrivateInfo ? child.LastName : "HIDDEN");
                    if (showPrivateInfo)
                    {
                        //Set the DOB
                        deDOB.Value = child.BirthDate;
                    }
                    else
                    {
                        //Hide the DOB control, null out the value, and set to not required
                        deDOB.CssClasses.Control = "hidden";
                        deDOB.Value = null;
                        deDOB.ValidationSettings.RequiredField.IsRequired = false;
                    }
                    txtProgramID.Value = childProgram.ProgramSpecificID;
                    deEnrollmentDate.Value = childProgram.EnrollmentDate;
                    ddGender.SelectedItem = ddGender.Items.FindByValue(child.GenderCodeFK);
                    txtGenderSpecify.Text = child.GenderSpecify;
                    ddEthnicity.SelectedItem = ddEthnicity.Items.FindByValue(child.EthnicityCodeFK);
                    txtEthnicitySpecify.Text = child.EthnicitySpecify;
                    ddRace.SelectedItem = ddRace.Items.FindByValue(child.RaceCodeFK);
                    txtRaceSpecify.Text = child.RaceSpecify;
                    ddDLL.SelectedItem = ddDLL.Items.FindByValue(childProgram.IsDLL);
                    ddIEP.SelectedItem = ddIEP.Items.FindByValue(childProgram.HasIEP);
                    ddHasParentPermission.SelectedItem = ddHasParentPermission.Items.FindByValue(childProgram.HasParentPermission);
                    deDischargeDate.Value = childProgram.DischargeDate;
                    ddDischargeReason.SelectedItem = ddDischargeReason.Items.FindByValue(childProgram.DischargeCodeFK);
                    txtDischargeReasonSpecify.Value = childProgram.DischargeReasonSpecify;

                    if (!string.IsNullOrWhiteSpace(childProgram.ParentPermissionDocumentFileName))
                    {
                        //Get the file URL from Azure storage
                        string fileLink = Utilities.GetFileLinkFromAzureStorage(childProgram.ParentPermissionDocumentFileName,
                            childProgram.ParentPermissionDocumentFileName.Contains(".pdf"),
                            Utilities.ConstantAzureStorageContainerName.CHILD_FORM_UPLOADS.ToString(), 10);

                        //Set the link URL
                        lnkDisplayParentPermissionFile.NavigateUrl = fileLink;

                        //Set div display
                        hfHasParentPermissionDocument.Value = "true";
                    }
                    else
                    {
                        //Set div display
                        hfHasParentPermissionDocument.Value = "false";
                    }
                }
                else
                {
                    hfHasParentPermissionDocument.Value = "false";
                }

                //Set the control usability
                txtFirstName.ReadOnly = readOnly;
                txtLastName.ReadOnly = readOnly;
                deDOB.ReadOnly = readOnly;
                txtProgramID.ReadOnly = readOnly;
                deEnrollmentDate.ReadOnly = readOnly;
                ddGender.ReadOnly = readOnly;
                txtGenderSpecify.ReadOnly = readOnly;
                ddEthnicity.ReadOnly = readOnly;
                txtEthnicitySpecify.ReadOnly = readOnly;
                ddRace.ReadOnly = readOnly;
                txtRaceSpecify.ReadOnly = readOnly;
                ddDLL.ReadOnly = readOnly;
                ddIEP.ReadOnly = readOnly;
                ddHasParentPermission.ReadOnly = readOnly;
                lnkDisplayParentPermissionFile.Visible = showPrivateInfo;
                bucParentPermissionDocument.ClientEnabled = (readOnly == true ? false : true);
                btnDeletePermissionFile.Visible = (readOnly == true ? false : true);
                lbConfirmPermissionFileDelete.Visible = (readOnly == true ? false : true);
                btnUpdateDocument.Visible = (readOnly == true ? false : true);
                deDischargeDate.ReadOnly = readOnly;
                ddDischargeReason.ReadOnly = readOnly;
                txtDischargeReasonSpecify.ReadOnly = readOnly;
            }
        }

        /// <summary>
        /// This method fires when the user clicks the button to confirm deletion of the parent permission document.
        /// </summary>
        /// <param name="sender">The lbConfirmPermissionFileDelete LinkButton</param>
        /// <param name="e">The click event</param>
        protected void lbConfirmPermissionFileDelete_Click(object sender, EventArgs e)
        {
            //Get the ChildProgram pk
            int childProgramPK = Convert.ToInt32(hfChildProgramPK.Value);

            //To hold the ChildProgram object
            Models.ChildProgram currentChildProgram;

            //Determine if the object already exists
            if (childProgramPK > 0)
            {
                using (PyramidContext context = new PyramidContext())
                {
                    //Get the ChildProgram object
                    currentChildProgram = context.ChildProgram.Where(cp => cp.ChildProgramPK == childProgramPK).FirstOrDefault();

                    //Check to see if the file exists
                    if (!string.IsNullOrWhiteSpace(currentChildProgram.ParentPermissionDocumentFileName))
                    {
                        //Delete the file
                        Utilities.DeleteFileFromAzureStorage(currentChildProgram.ParentPermissionDocumentFileName,
                            Utilities.ConstantAzureStorageContainerName.CHILD_FORM_UPLOADS.ToString());

                        //Clear the fields
                        currentChildProgram.ParentPermissionDocumentFileName = null;
                        currentChildProgram.ParentPermissionDocumentFilePath = null;

                        //Save the changes
                        context.SaveChanges();

                        //Set the div display
                        hfHasParentPermissionDocument.Value = "false";

                        //Show a message
                        childControlMsgSys.ShowMessageToUser("success", "Document Deleted", "The Parent/Guardian Permission Document was successfully deleted!", 10000);
                    }
                }
            }
            else
            {
                childControlMsgSys.ShowMessageToUser("danger", "Delete Failed", "The Parent/Guardian Permission Document could not be deleted!  If you continue to experience this error, please contact technical support via a support ticket.", 20000);
            }
        }

        /// <summary>
        /// This method fires when the validation for the deEnrollmentDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deEnrollmentDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deEnrollmentDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the enrollment date and discharge date
            DateTime? enrollmentDate = (deEnrollmentDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEnrollmentDate.Value));
            DateTime? dischargeDate = (deDischargeDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDischargeDate.Value));
            DateTime? DOB = (deDOB.Value == null ? (DateTime?)null : Convert.ToDateTime(deDOB.Value));

            //Perform the validation
            if (enrollmentDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Enrollment Date is required!";
            }
            else if (enrollmentDate > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Enrollment Date cannot be in the future!";
            }
            else if (dischargeDate.HasValue && enrollmentDate >= dischargeDate)
            {
                e.IsValid = false;
                e.ErrorText = "Enrollment Date must be before the discharge date!";
            }
            else if (DOB.HasValue && enrollmentDate.Value < DOB.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Enrollment Date cannot be before the Child's Date of Birth!";
            }
            else
            {
                //Get the child and program pks
                int childPK = Convert.ToInt32(hfChildPK.Value);
                int programPK = Convert.ToInt32(hfProgramFK.Value);

                //Only continue if this is an edit
                if (childPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Validate the enrollment date against other forms' dates
                        var formValidationResults = context.spValidateEnrollmentDischargeDates(childPK,
                                                        programPK, enrollmentDate, (DateTime?)null).ToList();

                        //If there are results, the enrollment date is invalid
                        if (formValidationResults.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Enrollment Date is invalid, see notification message for details!";

                            //Create a message that contains the forms that would be invalidated
                            string message = "The Enrollment Date would invalidate these records if changed to that date:<br/><br/>";
                            foreach (spValidateEnrollmentDischargeDates_Result invalidForm in formValidationResults)
                            {
                                message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                                message += "<br/>";
                            }

                            //Show the message
                            ValidationMessageToDisplay = message;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtGenderSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtGenderSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtGenderSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specify text
            string genderSpecify = txtGenderSpecify.Text;

            //Perform validation
            if (ddGender.SelectedItem != null && ddGender.SelectedItem.Text.ToLower() == "other" && string.IsNullOrWhiteSpace(genderSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Gender is required when the 'Other' gender is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtEthnicitySpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtEthnicitySpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtEthnicitySpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specify text
            string ethnicitySpecify = txtEthnicitySpecify.Text;

            //Perform validation
            if (ddEthnicity.SelectedItem != null && ddEthnicity.SelectedItem.Text.ToLower() == "other" && string.IsNullOrWhiteSpace(ethnicitySpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Ethnicity is required when the 'Other' ethnicity is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtRaceSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtRaceSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtRaceSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specify text
            string raceSpecify = txtRaceSpecify.Text;

            //Perform validation
            if (ddRace.SelectedItem != null && ddRace.SelectedItem.Text.ToLower() == "other" && string.IsNullOrWhiteSpace(raceSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Race is required when the 'Other' race is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the deDischargeDate DevExpress
        /// Bootstrap DateEdit fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The deDischargeDate DateEdit</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void deDischargeDate_Validation(object sender, ValidationEventArgs e)
        {
            //Get the discharge date, enrollment date, and discharge reason
            DateTime? dischargeDate = (deDischargeDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDischargeDate.Value));
            DateTime? enrollmentDate = (deEnrollmentDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deEnrollmentDate.Value));
            string dischargeReason = (ddDischargeReason.Value == null ? null : ddDischargeReason.Value.ToString());

            //Perform the validation
            if (dischargeDate.HasValue == false && dischargeReason != null)
            {
                e.IsValid = false;
                e.ErrorText = "Discharge Date is required if you have a Discharge Reason!";
            }
            else if (dischargeDate.HasValue && enrollmentDate.HasValue == false)
            {
                e.IsValid = false;
                e.ErrorText = "Enrollment Date must be entered before the Discharge Date!";
            }
            else if (dischargeDate.HasValue && dischargeDate.Value < enrollmentDate.Value)
            {
                e.IsValid = false;
                e.ErrorText = "Discharge Date must be after the Enrollment Date!";
            }
            else if (dischargeDate.HasValue && dischargeDate > DateTime.Now)
            {
                e.IsValid = false;
                e.ErrorText = "Discharge Date cannot be in the future!";
            }
            else if (dischargeDate.HasValue)
            {
                //Get the child and program pks
                int childPK = Convert.ToInt32(hfChildPK.Value);
                int programPK = Convert.ToInt32(hfProgramFK.Value);

                //Only continue if this is an edit
                if (childPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        //Validate the discharge date against other forms' dates
                        var formValidationResults = context.spValidateEnrollmentDischargeDates(childPK,
                                                        programPK, (DateTime?)null, dischargeDate).ToList();

                        //If there are results, the discharge date is invalid
                        if (formValidationResults.Count > 0)
                        {
                            e.IsValid = false;
                            e.ErrorText = "Discharge Date is invalid, see notification message for details!";

                            //Create a message that contains the forms that would be invalidated
                            string message = "The Discharge Date would invalidate these records if changed to that date:<br/><br/>";
                            foreach (spValidateEnrollmentDischargeDates_Result invalidForm in formValidationResults)
                            {
                                message += invalidForm.ObjectName + " (" + invalidForm.ObjectDate.Value.ToString("MM/dd/yyyy") + ")";
                                message += "<br/>";
                            }

                            //Show the message
                            ValidationMessageToDisplay = message;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the ddDischargeReason DevExpress
        /// Bootstrap ComboBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddDischargeReason ComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddDischargeReason_Validation(object sender, ValidationEventArgs e)
        {
            //Get the discharge date and reason
            DateTime? dischargeDate = (deDischargeDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDischargeDate.Value));
            string dischargeReason = (ddDischargeReason.Value == null ? null : ddDischargeReason.Value.ToString());

            //Perform validation
            if (dischargeDate.HasValue == false && dischargeReason != null)
            {
                e.IsValid = false;
                e.ErrorText = "Discharge Reason is required if you have a Discharge Date!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtProgramID DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtProgramID TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtProgramID_Validation(object sender, ValidationEventArgs e)
        {
            //Get the program ID
            string programID = txtProgramID.Text;

            if (!string.IsNullOrWhiteSpace(programID))
            {
                //To hold the necessary values
                int programFK, childProgramPK;

                //Parse the necessary values
                if (int.TryParse(hfChildProgramPK.Value, out childProgramPK) && int.TryParse(hfProgramFK.Value, out programFK))
                {
                    bool isAlreadyUsed;

                    //Check to see if the ID is already used
                    using (PyramidContext context = new PyramidContext())
                    {
                        isAlreadyUsed = context.ChildProgram
                                       .AsNoTracking()
                                       .Where(cp => cp.ProgramFK == programFK &&
                                                    cp.ChildProgramPK != childProgramPK &&
                                                    cp.ProgramSpecificID.ToLower().Trim() == programID.ToLower().Trim())
                                       .Count() > 0;
                    }

                    //Set the validation message
                    if (isAlreadyUsed)
                    {
                        e.IsValid = false;
                        e.ErrorText = "That ID Number is already taken!";
                    }
                }
                else
                {
                    e.IsValid = false;
                    e.ErrorText = "Unable to determine ID Number validity, please try again.  If this continues to fail, please contact support via ticket.";
                }
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtDischargeReasonSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The txtDischargeReasonSpecify TextBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void txtDischargeReasonSpecify_Validation(object sender, ValidationEventArgs e)
        {
            //Get the specified discharge reason
            string dischargeReasonSpecify = (txtDischargeReasonSpecify.Value == null ? null : txtDischargeReasonSpecify.Value.ToString());

            //Perform validation
            if (ddDischargeReason.SelectedItem != null && ddDischargeReason.SelectedItem.Text.ToLower() == "other" && String.IsNullOrWhiteSpace(dischargeReasonSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Discharge Reason is required when the 'Other' discharge reason is selected!";
            }
        }

        /// <summary>
        /// This method fires when the validation for the txtDischargeReasonSpecify DevExpress
        /// Bootstrap TextBox fires and it validates that control's value
        /// </summary>
        /// <param name="sender">The ddHasParentPermission BootstrapComboBox</param>
        /// <param name="e">ValidationEventArgs</param>
        protected void ddHasParentPermission_Validation(object sender, ValidationEventArgs e)
        {
            //Validate the combo box
            bool hasParentPermission;

            //Make sure the answer is yes
            if (bool.TryParse(ddHasParentPermission.Text, out hasParentPermission))
            {
                if (hasParentPermission == false)
                {
                    e.IsValid = false;
                    e.ErrorText = "Parent/Guardian Permission must be answered yes in order to continue!";
                }
            }

            //Validate the file
            //Get the parent permission file to upload
            UploadedFile uploadedPermissionFile = bucParentPermissionDocument.UploadedFiles[0];

            //Check if a file was uploaded
            if (uploadedPermissionFile.ContentLength > 0)
            {
                //A file was uploaded, check validity
                if (uploadedPermissionFile.IsValid == false)
                {
                    //Set the validation error
                    e.IsValid = false;
                    e.ErrorText = "The Parent/Guardian Permission document is invalid and could not be uploaded!  It is likely too large or of an incorrect type.";

                    //Show an error message
                    childControlMsgSys.ShowMessageToUser("danger", "Invalid Document", "The Parent/Guardian Permission document is invalid and could not be uploaded!  It is likely too large or of an incorrect type.", 20000);
                }
            }
        }
    }
}