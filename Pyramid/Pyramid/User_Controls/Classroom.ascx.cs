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
                Models.Classroom currentClassroom;

                //Determine if the classroom already exists
                if (classroomPK > 0)
                {
                    using (PyramidContext context = new PyramidContext())
                    {
                        currentClassroom = context.Classroom.AsNoTracking().Where(c => c.ClassroomPK == classroomPK).FirstOrDefault();
                    }

                    if (currentClassroom == null)
                    {
                        currentClassroom = new Models.Classroom();
                    }
                }
                else
                {
                    currentClassroom = new Models.Classroom();
                }

                //Set the values from the inputs
                currentClassroom.Name = txtName.Value.ToString();
                currentClassroom.Location = (txtLocation.Value == null ? null : txtLocation.Value.ToString());
                currentClassroom.IsInfantToddler = Convert.ToBoolean(ddInfantToddler.Value);
                currentClassroom.IsPreschool = Convert.ToBoolean(ddPreschool.Value);
                currentClassroom.BeingServedSubstitute = Convert.ToBoolean(ddServedSubstitute.Value);
                currentClassroom.ProgramFK = programFK;

                string programID;
                if (!string.IsNullOrWhiteSpace(txtProgramID.Text))
                {
                    //Use the ID that the user provided
                    //Make sure to trim the input to ensure leading and trailing spaces are removed
                    currentClassroom.ProgramSpecificID = txtProgramID.Text.Trim();
                }
                else
                {
                    //The user didn't specify an ID, generate one
                    //Check to see if this is an edit
                    if (currentClassroom.ClassroomPK > 0)
                    {
                        //This is an edit, use the current PK
                        programID = string.Format("CLID-{0}", currentClassroom.ClassroomPK);
                    }
                    else
                    {
                        //To hold the previous PK
                        int previousPK;

                        using (PyramidContext context = new PyramidContext())
                        {
                            //Get the previous PK
                            Models.Classroom newestClassroom = context.Classroom
                                                                        .AsNoTracking()
                                                                        .OrderByDescending(c => c.ClassroomPK)
                                                                        .FirstOrDefault();
                            previousPK = (newestClassroom != null ? newestClassroom.ClassroomPK : 0);

                            //Set the program specific ID to the previous PK plus one
                            programID = string.Format("CLID-{0}", (previousPK + 1));
                        }
                    }
                    currentClassroom.ProgramSpecificID = programID;
                }

                //Return the object
                return currentClassroom;
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
            string programID = txtProgramID.Text;

            //Perform validation
            if (!string.IsNullOrWhiteSpace(programID))
            {
                //To hold the necessary values
                int programFK, classroomPK;

                if (int.TryParse(hfClassroomPK.Value, out classroomPK) && int.TryParse(hfProgramFK.Value, out programFK))
                {
                    bool isAlreadyUsed;

                    //Check to see if the ID is already used
                    using (PyramidContext context = new PyramidContext())
                    {
                        isAlreadyUsed = context.Classroom
                                        .AsNoTracking()
                                        .Where(cp => cp.ProgramFK == programFK &&
                                                     cp.ClassroomPK != classroomPK &&
                                                     cp.ProgramSpecificID.ToLower().Trim() == programID.ToLower().Trim())
                                        .Count() > 0;
                    }

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
    }
}