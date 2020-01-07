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
            //Set the max dates for the DateEdits (min dates are declared in markup)
            deDOB.MaxDate = DateTime.Now;
            deEnrollmentDate.MaxDate = DateTime.Now;
            deDischargeDate.MaxDate = DateTime.Now;
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
                child.EthnicityCodeFK = Convert.ToInt32(ddEthnicity.Value);
                child.RaceCodeFK = Convert.ToInt32(ddRace.Value);

                //Return the object
                return child;
            }
            else
            {
                if(String.IsNullOrWhiteSpace(ValidationMessageToDisplay))
                    ValidationMessageToDisplay = "Validation failed, see above for details!";
                return null;
            }
        }

        /// <summary>
        /// This method returns the ChildProgram object filled with information from
        /// the inputs in this control so that the content page can interact with it
        /// </summary>
        /// <returns>The filled ChildProgram object if validation succeeds, null otherwise</returns>
        public Models.ChildProgram GetChildProgram()
        {
            if (ASPxEdit.AreEditorsValid(this, ValidationGroup))
            {
                //Get the ChildProgram pk
                int childProgramPK = Convert.ToInt32(hfChildProgramPK.Value);

                //To hold the ChildProgram object
                Models.ChildProgram childProgram;

                //Determine if the object already exists
                if(childProgramPK > 0)
                {
                    using(PyramidContext context = new PyramidContext())
                    {
                        childProgram = context.ChildProgram.AsNoTracking().Where(cp => cp.ChildProgramPK == childProgramPK).FirstOrDefault();
                    }
                }
                else
                {
                    childProgram = new Models.ChildProgram();
                }

                //Set the values
                childProgram.ProgramFK = Convert.ToInt32(hfProgramFK.Value);
                childProgram.EnrollmentDate = Convert.ToDateTime(deEnrollmentDate.Value);
                childProgram.DischargeDate = (deDischargeDate.Value == null ? (DateTime?)null : Convert.ToDateTime(deDischargeDate.Value));
                childProgram.DischargeCodeFK = (ddDischargeReason.Value == null ? (int?)null : Convert.ToInt32(ddDischargeReason.Value));
                childProgram.DischargeReasonSpecify = (txtDischargeReasonSpecify.Value == null ? null : txtDischargeReasonSpecify.Value.ToString());
                childProgram.ProgramSpecificID = txtProgramID.Value.ToString();
                childProgram.HasIEP = Convert.ToBoolean(ddIEP.Value);
                childProgram.IsDLL = Convert.ToBoolean(ddDLL.Value);

                //Return the object
                return childProgram;
            }
            else
            {
                if (String.IsNullOrWhiteSpace(ValidationMessageToDisplay))
                    ValidationMessageToDisplay = "Validation failed, see above for details!";
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
        public void InitializeWithData(int childProgramPK, int programFK, bool readOnly)
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

                //Fill the used IDs hidden field
                var usedIDs = context.ChildProgram
                                .AsNoTracking()
                                .Where(cp => cp.ProgramFK == programFK && cp.ChildProgramPK != childProgramPK)
                                .OrderBy(cp => cp.ProgramSpecificID)
                                .Select(cp => cp.ProgramSpecificID)
                                .ToList();
                hfUsedIDs.Value = string.Join(",", usedIDs);

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
                    txtFirstName.Value = child.FirstName;
                    txtLastName.Value = child.LastName;
                    deDOB.Value = child.BirthDate.ToString("MM/dd/yyyy");
                    txtProgramID.Value = childProgram.ProgramSpecificID;
                    deEnrollmentDate.Value = childProgram.EnrollmentDate.ToString("MM/dd/yyyy");
                    ddGender.SelectedItem = ddGender.Items.FindByValue(child.GenderCodeFK);
                    ddEthnicity.SelectedItem = ddEthnicity.Items.FindByValue(child.EthnicityCodeFK);
                    ddRace.SelectedItem = ddRace.Items.FindByValue(child.RaceCodeFK);
                    ddDLL.SelectedItem = ddDLL.Items.FindByValue(childProgram.IsDLL);
                    ddIEP.SelectedItem = ddIEP.Items.FindByValue(childProgram.HasIEP);
                    deDischargeDate.Value = (childProgram.DischargeDate.HasValue ? childProgram.DischargeDate.Value.ToString("MM/dd/yyyy") : "");
                    ddDischargeReason.SelectedItem = ddDischargeReason.Items.FindByValue(childProgram.DischargeCodeFK);
                    txtDischargeReasonSpecify.Value = childProgram.DischargeReasonSpecify;
                }

                //Set the controls usability
                txtFirstName.ReadOnly = readOnly;
                txtLastName.ReadOnly = readOnly;
                deDOB.ReadOnly = readOnly;
                txtProgramID.ReadOnly = readOnly;
                deEnrollmentDate.ReadOnly = readOnly;
                ddGender.ReadOnly = readOnly;
                ddEthnicity.ReadOnly = readOnly;
                ddRace.ReadOnly = readOnly;
                ddDLL.ReadOnly = readOnly;
                ddIEP.ReadOnly = readOnly;
                deDischargeDate.ReadOnly = readOnly;
                ddDischargeReason.ReadOnly = readOnly;
                txtDischargeReasonSpecify.ReadOnly = readOnly;
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
            string programID = (txtProgramID.Value == null ? null : txtProgramID.Value.ToString());
            string[] programIDArray = hfUsedIDs.Value.Split(',');

            //Perform validation
            if (String.IsNullOrWhiteSpace(programID))
            {
                e.IsValid = false;
                e.ErrorText = "ID Number is required!";
            }
            else if(programIDArray.Contains(programID))
            {
                e.IsValid = false;
                e.ErrorText = "That ID Number is already taken!";
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
            if(ddDischargeReason.SelectedItem != null && ddDischargeReason.SelectedItem.Text.ToLower().Contains("other") && String.IsNullOrWhiteSpace(dischargeReasonSpecify))
            {
                e.IsValid = false;
                e.ErrorText = "Specify Discharge Reason is required when the 'Other' discharge reason is selected!";
            }
        }
    }
}