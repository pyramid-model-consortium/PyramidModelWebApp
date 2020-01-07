using DevExpress.Web;
using Pyramid.Models;
using System;
using System.Linq;

namespace Pyramid.User_Controls
{
    public partial class Classroom : System.Web.UI.UserControl
    {
        private string validationMessage;

        public string ValidationGroup
        {
            get
            {
                return "vgClassroom";
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

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Focus on the classroom name
        /// </summary>
        public void FocusClassroomName()
        {
            txtName.Focus();
        }

        /// <summary>
        /// This method returns the Classroom object filled with information from
        /// the inputs in this control so that the content page can interact with it
        /// </summary>
        /// <returns>The filled Classroom object if validation succeeds, null otherwise</returns>
        public Models.Classroom GetClassroom()
        {
            if (ASPxEdit.AreEditorsValid(this, ValidationGroup))
            {
                //Get the classroom pk
                int classroomPK = Convert.ToInt32(hfClassroomPK.Value);

                //Get the program fk
                int programFK = Convert.ToInt32(hfProgramFK.Value);

                //To hold the classroom object
                Models.Classroom classroom;

                //Determine if the classroom already exists
                if (classroomPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        classroom = context.Classroom.AsNoTracking().Where(c => c.ClassroomPK == classroomPK).FirstOrDefault();
                    }
                }
                else
                {
                    classroom = new Models.Classroom();
                }

                //Set the values from the inputs
                classroom.Name = txtName.Value.ToString();
                classroom.ProgramSpecificID = txtProgramID.Value.ToString();
                classroom.Location = (txtLocation.Value == null ? null : txtLocation.Value.ToString());
                classroom.IsInfantToddler = Convert.ToBoolean(ddInfantToddler.Value);
                classroom.IsPreschool = Convert.ToBoolean(ddPreschool.Value);
                classroom.BeingServedSubstitute = Convert.ToBoolean(ddServedSubstitute.Value);
                classroom.ProgramFK = programFK;

                //Return the object
                return classroom;
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
        /// control with the information in the Classroom table for that primary key
        /// </summary>
        /// <param name="classroomPK">The primary key of the Classroom record</param>
        /// <param name="programFK">The primary key for the program which this classroom will be in</param>
        /// <param name="readOnly">True if the user is only allowed to view the values</param>
        public void InitializeWithData(int classroomPK, int programFK, bool readOnly)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Set the program fk hidden field
                hfProgramFK.Value = programFK.ToString();

                //Fill the used IDs hidden field
                var usedIDs = context.Classroom
                                .AsNoTracking()
                                .Where(cp => cp.ProgramFK == programFK && cp.ClassroomPK != classroomPK)
                                .OrderBy(cp => cp.ProgramSpecificID)
                                .Select(cp => cp.ProgramSpecificID)
                                .ToList();
                hfUsedIDs.Value = string.Join(",", usedIDs);


                if (classroomPK > 0)
                {
                    //If this is an edit, fill the form with the classroom's information
                    //Get the object
                    Models.Classroom classroom = context.Classroom.AsNoTracking().Where(c => c.ClassroomPK == classroomPK).FirstOrDefault();

                    //Fill the hidden fields
                    hfClassroomPK.Value = classroom.ClassroomPK.ToString();

                    //Fill the input fields
                    txtName.Value = classroom.Name;
                    txtProgramID.Value = classroom.ProgramSpecificID;
                    txtLocation.Value = classroom.Location;
                    ddInfantToddler.SelectedItem = ddInfantToddler.Items.FindByValue(classroom.IsInfantToddler);
                    ddPreschool.SelectedItem = ddPreschool.Items.FindByValue(classroom.IsPreschool);
                    ddServedSubstitute.SelectedItem = ddServedSubstitute.Items.FindByValue(classroom.BeingServedSubstitute);
                }

                //Set the controls usability
                txtName.ReadOnly = readOnly;
                txtProgramID.ReadOnly = readOnly;
                txtLocation.ReadOnly = readOnly;
                ddInfantToddler.ReadOnly = readOnly;
                ddPreschool.ReadOnly = readOnly;
                ddServedSubstitute.ReadOnly = readOnly;
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
            else if (programIDArray.Contains(programID))
            {
                e.IsValid = false;
                e.ErrorText = "That ID Number is already taken!";
            }
        }
    }
}