using System;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Pyramid.FileImport.CodeFiles;
using CsvHelper.Configuration;
using System.Text;

namespace Pyramid.Models
{
    public partial class ChildProgram
    {
        public class ChildProgramUpload : IImportable
        {
            public bool IsValid { get; set; }
            public bool IsDuplicate { get; set; }
            public string ReasonsInvalid { get; set; }
            public string DisplayName => "Child";
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? DOB { get; set; }
            public string IDNumber { get; set; }
            public DateTime? ProgramEnrollmentDate { get; set; }
            public string Gender { get; set; }
            public int? GenderCodeFK { get; set; }
            public string SpecifyGender { get; set; }
            public string Ethnicity { get; set; }
            public int? EthnicityCodeFK { get; set; }
            public string SpecifyEthnicity { get; set; }
            public string Race { get; set; }
            public int? RaceCodeFK { get; set; }
            public string SpecifyRace { get; set; }
            public bool? DualLanguageLearner { get; set; }
            public bool? IndividualizedEducationProgram { get; set; }
            public bool? ParentOrGuardianPermission { get; set; }
            public DateTime? ProgramDischargeDate { get; set; }
            public string ProgramDischargeReason { get; set; }
            public int? DischargeReasonCodeFK { get; set; }
            public string SpecifyProgramDischargeReason { get; set; }
            public string AssignedClassroomID { get; set; }
            public int? ClassroomFK { get; set; }
            public DateTime? ClassroomAssignDate { get; set; }
            public DateTime? ClassroomLeaveDate { get; set; }
            public string ClassroomLeaveReason { get; set; }
            public int? ClassroomLeaveReasonCodeFK { get; set; }
            public string SpecifyClassroomLeaveReason { get; set; }
            public int ProgramFK { get; set; }

            public class ChildProgramUploadCsvClassMap : ClassMap<ChildProgramUpload>
            {
                public ChildProgramUploadCsvClassMap()
                {
                    Map(m => m.FirstName).Index(0);
                    Map(m => m.LastName).Index(1);
                    Map(m => m.DOB).Index(2).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.IDNumber).Index(3);
                    Map(m => m.ProgramEnrollmentDate).Index(4).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.Gender).Index(5);
                    Map(m => m.SpecifyGender).Index(6);
                    Map(m => m.Ethnicity).Index(7);
                    Map(m => m.SpecifyEthnicity).Index(8);
                    Map(m => m.Race).Index(9);
                    Map(m => m.SpecifyRace).Index(10);
                    Map(m => m.DualLanguageLearner).Index(11).TypeConverter<CsvHelperExtensions.CustomBoolConverter<bool?>>();
                    Map(m => m.IndividualizedEducationProgram).Index(12).TypeConverter<CsvHelperExtensions.CustomBoolConverter<bool?>>();
                    Map(m => m.ParentOrGuardianPermission).Index(13).TypeConverter<CsvHelperExtensions.CustomBoolConverter<bool?>>();
                    Map(m => m.ProgramDischargeDate).Index(14).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.ProgramDischargeReason).Index(15);
                    Map(m => m.SpecifyProgramDischargeReason).Index(16);
                    Map(m => m.AssignedClassroomID).Index(17);
                    Map(m => m.ClassroomAssignDate).Index(18).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.ClassroomLeaveDate).Index(19).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.ClassroomLeaveReason).Index(20);
                    Map(m => m.SpecifyClassroomLeaveReason).Index(21);
                    Map(m => m.ProgramFK).Constant(0);
                }
            }

            /// <summary>
            /// This method returns a list of the fields to hide from the preview.
            /// </summary>
            /// <returns>A string list of field names.</returns>
            public List<string> GetFieldsToHideFromPreview()
            {
                //The fields to hide
                List<string> fieldsToHide = new List<string>()
                {
                    "ProgramFK",
                    "DisplayName",
                    "GenderCodeFK",
                    "EthnicityCodeFK",
                    "RaceCodeFK",
                    "DischargeReasonCodeFK",
                    "ClassroomFK",
                    "ClassroomLeaveReasonCodeFK"
                };

                //Return the list
                return fieldsToHide;
            }

            /// <summary>
            /// This method returns the instructions for importing the child CSV file.
            /// </summary>
            /// <returns>A string that contains the instructions for importing the CSV file.  String is in HTML format.</returns>
            public string GetImportInstructionsHTML()
            {
                //To hold the instructions
                StringBuilder importInstructions = new StringBuilder();

                //Add the instructions
                importInstructions.Append("This page allows you to upload a CSV file that contains information for multiple children in your program.  It also allows you to import a single classroom assignment for each child.  Please follow these instructions for importing children:");
                importInstructions.Append("<ol>");
                importInstructions.Append("<li>Download the template CSV file in this section.</li>");
                importInstructions.Append("<li>(Optional) Download the example CSV file in this section.  This file is the same format as the template file, with a few rows of example information entered.</li>");
                importInstructions.Append("<li>It is recommended that you open the template and example CSV files using <strong>Microsoft Excel</strong> for ease of data-entry.  However, you can use any program capable of handling CSV files.</li>");
                importInstructions.Append("<li>In the downloaded template CSV file, add all the child information and optional classroom assignment information that you would like to import.</li>"); 
                importInstructions.Append("<li>Make sure the information that you added to the file matches the requirements for the fields that are listed in the <strong>CSV File Information</strong> table that is in the <strong>CSV File Information</strong> section below.</li>");
                importInstructions.Append("<li>Save the template CSV file after you have added all the necessary information.</li>");
                importInstructions.Append("<li>Scroll down to the <strong>Import CSV File</strong> section at the bottom of the page.</li>");
                importInstructions.Append("<li>In the <strong>Import CSV File</strong> section, ensure the program selected in the <strong>Program</strong> drop-down list is the one that you wish to add children to.</li>");
                importInstructions.Append("<li>Click the browse button in the <strong>Select File to Import</strong> field and select the template file that you filled out and saved.</li>");
                importInstructions.Append("<li>Click the <strong>Import and Preview Results</strong> button.  This will take you to a new page to preview the import results before the final confirmation to add the children to the system.</li>");
                importInstructions.Append("<li><strong>Children will not be saved to PIDS until you confirm the import on the next page.</strong></li>");
                importInstructions.Append("</ol>");

                //Return the instructions
                return importInstructions.ToString();
            }

            /// <summary>
            /// This method returns the instructions for confirming the child CSV file import.
            /// </summary>
            /// <returns>A string that contains the instructions for confirming the CSV file import.  String is in HTML format.</returns>
            public string GetConfirmationInstructionsHTML()
            {
                //To hold the instructions
                StringBuilder importInstructions = new StringBuilder();

                //Add the instructions
                importInstructions.Append("This page allows you to review the information that you uploaded before saving it to PIDS.  Please follow these instructions to save the child and optional classroom assignment information:");
                importInstructions.Append("<ol>");
                importInstructions.Append("<li>All the child and classroom assignment information in the CSV file you uploaded will appear in the <strong>Import Result Preview</strong> table below.</li>");
                importInstructions.Append("<li>Review all the rosters listed in the table to make sure they are correct.</li>");
                importInstructions.Append("<li>Take note of the <strong>Is Valid</strong> field.  This field indicates whether or not the information is valid and will be saved.  If there is a green checkmark in this field, the information is valid and will be saved.  If there is red X in this field, the information is invalid and will <strong>not</strong> be saved.</li>");
                importInstructions.Append("<li>If the <strong>Is Valid</strong> field has a red X, check the <strong>Reasons Invalid</strong> field to see an explanation of why the field is not valid.  <strong>Note:</strong> Even if some of the rosters are invalid and will not be saved, they will not interfere with saving the valid rosters.  Valid rosters will always be saved, regardless of how many invalid rosters there are.</li>");
                importInstructions.Append("<li>Click the <strong>Save Valid Records</strong> button once you have verified all the information is correct.</li>");
                importInstructions.Append("<li>In the pop-up that appears, read the warning message to make the final confirmation and save the rosters.</li>");
                importInstructions.Append("</ol>");

                //Return the instructions
                return importInstructions.ToString();
            }

            /// <summary>
            /// This method returns information about the fields in the child CSV file.
            /// </summary>
            /// <returns>A list of ImportFileFieldDetail objects that contain descriptions of the fields in the CSV file.</returns>
            public List<ImportFileFieldDetail> GetFileFieldInformation()
            {
                //To hold the options for the code fields
                List<string> genderOptions, ethnicityOptions, raceOptions, dischargeOptions, classroomLeaveReasons;

                using(PyramidContext context = new PyramidContext())
                {
                    //Get the code table information
                    genderOptions = context.CodeGender.AsNoTracking().OrderBy(cg => cg.Description).Select(cg => cg.Description).ToList();
                    ethnicityOptions = context.CodeEthnicity.AsNoTracking().OrderBy(ce => ce.Description).Select(ce => ce.Description).ToList();
                    raceOptions = context.CodeRace.AsNoTracking().OrderBy(cr => cr.Description).Select(cr => cr.Description).ToList();
                    dischargeOptions = context.CodeDischargeReason.AsNoTracking().OrderBy(cdr => cdr.Description).Select(cdr => cdr.Description).ToList();
                    classroomLeaveReasons = context.CodeChildLeaveReason.AsNoTracking().OrderBy(cclr => cclr.Description).Select(cclr => cclr.Description).ToList();
                }

                //Get the list if objects
                List<ImportFileFieldDetail> listToReturn = new List<ImportFileFieldDetail>()
                {
                    new ImportFileFieldDetail("First Name", "Text", true, "This is the child's legal first name.", "Any text."),
                    new ImportFileFieldDetail("Last Name", "Text", true, "This is the child's legal last name.", "Any text."),
                    new ImportFileFieldDetail("DOB", "Date", true, "This is the child's date of birth.", "Any valid date that is not in the future and not after the Program Enrollment Date."),
                    new ImportFileFieldDetail("ID Number", "Text", true, "This is the ID number assigned to the child by the program that is uploading this information.", "Any combination of letters and numbers.  This field must not have any duplication within the file.  If the program doesn't assign ID numbers to their children, please implement an ID number scheme that uses letters and numbers to create a unique ID number for every child that you wish to import.  If you are creating an ID number scheme, we recommend that the ID numbers do not contain the child's initials, DOB, or other personally-identifiable information."),
                    new ImportFileFieldDetail("Program Enrollment Date", "Date", true, "This is the date the child enrolled in the program.", "Any valid date that is not in the future and not after the Program Discharge Date."),
                    new ImportFileFieldDetail("Gender", "Text", true, "This is the child's gender.", string.Format("The text in this field must match one of the following options (not case-sensitive):<br> {0}.", string.Join(",<br>", genderOptions))),
                    new ImportFileFieldDetail("Specify Gender", "Text", false, "A description of the child's gender with a maximum length of 100 characters.", "If 'Other' is entered for the child's gender, then this is required.  Otherwise, it should be left blank."),
                    new ImportFileFieldDetail("Ethnicity", "Text", true, "This is the child's ethnicity.", string.Format("The text in this field must match one of the following options (not case-sensitive):<br> {0}.", string.Join(",<br>", ethnicityOptions))),
                    new ImportFileFieldDetail("Specify Ethnicity", "Text", false, "A description of the child's ethnicity with a maximum length of 100 characters.", "If 'Other' is entered for the child's ethnicity, then this is required.  Otherwise, it should be left blank."),
                    new ImportFileFieldDetail("Race", "Text", true, "This is the child's race.", string.Format("The text in this field must match one of the following options (not case-sensitive):<br> {0}.", string.Join(",<br>", raceOptions))),
                    new ImportFileFieldDetail("Specify Race", "Text", false, "A description of the child's race with a maximum length of 100 characters.", "If 'Other' is entered for the child's race, then this is required.  Otherwise, it should be left blank."),
                    new ImportFileFieldDetail("Dual Language Learner", "Text", true, "This is the child's dual language learner status.", "The text in this field must match one of the following options (not case-sensitive):<br> yes,<br> no,<br> true,<br> false."),
                    new ImportFileFieldDetail("Individualized Education Program", "Text", true, "This is the child's individualized education program status.", "The text in this field must match one of the following options (not case-sensitive):<br> yes,<br> no,<br> true,<br> false."),
                    new ImportFileFieldDetail("Parent Or Guardian Permission", "Text", true, "This indicates whether or not the parent or guardian of the child gave permission for the child's information to be added to PIDS.  The child cannot be added to PIDS without the parent or guardian's permission.", "The text in this field must match one of the following options (not case-sensitive):<br> yes,<br> no,<br> true,<br> false."),
                    new ImportFileFieldDetail("Program Discharge Date", "Date", false, "This is the date the child was discharged from the program.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future and the Program Discharge Reason field is required."),
                    new ImportFileFieldDetail("Program Discharge Reason", "Text", false, "This is the reason that the child was discharged from the program.", string.Format("Since this field is optional, it may be left blank.  If entered, the Discharge Date field is required the text in this field must match one of following options (not case-sensitive):<br> {0}.", string.Join(",<br>", dischargeOptions))),
                    new ImportFileFieldDetail("Specify Program Discharge Reason", "Text", false, "More details about the Program Discharge Reason.", "If 'Other' was used as the Program Discharge Reason for this child, this is required and can be any text.  Otherwise this field will be ignored and can be left blank."),
                    new ImportFileFieldDetail("Assigned Classroom ID", "Text", false, "This is the ID number for the classroom that the child is assigned to.", "Since this field is optional, it may be left blank.  If entered, it must be a combination of letters and numbers that matches the ID Number (not case-sensitive) of one of the existing classrooms in the program."),
                    new ImportFileFieldDetail("Classroom Assign Date", "Date", false, "This is the date the child was assigned to the classroom.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future, not before the Program Enrollment Date, not after the Program Discharge Date, and not after the Classroom Leave Date."),
                    new ImportFileFieldDetail("Classroom Leave Date", "Date", false, "This is the date the child left the classroom.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future, not before the Program Enrollment Date, not after the Program Discharge Date, and the Classroom Leave Reason field is required."),
                    new ImportFileFieldDetail("Classroom Leave Reason", "Text", false, "This is the reason that the child left the classroom.", string.Format("Since this field is optional, it may be left blank.  If entered, the Classroom Leave Date field is required the text in this field must match one of following options (not case-sensitive):<br> {0}.", string.Join(",<br>", classroomLeaveReasons))),
                    new ImportFileFieldDetail("Specify Classroom Leave Reason", "Text", false, "More details about the Classroom Leave Reason.", "If 'Other' was used as the Classroom Leave Reason for this child, this is required and can be any text.  Otherwise this field will be ignored and can be left blank.")
                };

                //Return the list
                return listToReturn;
            }

            /// <summary>
            /// This method returns the path to the CSV template file
            /// </summary>
            /// <returns>The path to the CSV template file</returns>
            public string GetImportTemplateFilePath()
            {
                return "/FileImport/ExcelTemplates/ChildProgramUpload.csv";
            }

            /// <summary>
            /// This method returns the path to the CSV example file
            /// </summary>
            /// <returns>The path to the CSV example file</returns>
            public string GetImportExampleFilePath()
            {
                return "/FileImport/ExcelTemplates/ChildProgramUpload-Example.csv";
            }

            /// <summary>
            /// This method registers this object's class map with the CsvReader object passed to the method.
            /// </summary>
            /// <param name="currentReader">The CsvReader object that represents a CSV file</param>
            public void RegisterClassMapWithReader(CsvReader currentReader)
            {
                currentReader.Context.RegisterClassMap<ChildProgramUploadCsvClassMap>();
            }

            /// <summary>
            /// This methods sets the validation and code fields in the passed list of objects.
            /// </summary>
            /// <param name="objectsToCheck">The objects to check for validity</param>
            /// <param name="genderCodes">The list of available Gender options</param>
            /// <param name="ethnicityCodes">The list of available Ethnicity options</param>
            /// <param name="raceCodes">The list of available race options</param>
            /// <param name="dischargeCodes">The list of available discharge reason options</param>
            /// <param name="clasroomLeaveResons">The list of classroom leave reason options</param>
            /// <param name="existingClassrooms">The list of existing classrooms</param>
            /// <param name="existingChildren">The list of existing children</param>
            private static void SetValidationAndFKFields(ref List<ChildProgramUpload> objectsToCheck, List<CodeGender> genderCodes, List<CodeEthnicity> ethnicityCodes, 
                List<CodeRace> raceCodes, List<CodeDischargeReason> dischargeCodes, List<CodeChildLeaveReason> clasroomLeaveResons,
                List<Classroom> existingClassrooms, List<ChildProgram> existingChildren, int currentProgramFK)
            {
                //Loop through the objects and set the validation fields
                for (int currentIndex = 0; currentIndex < objectsToCheck.Count; currentIndex++)
                {
                    //To hold the reasons the row is invalid
                    List<string> reasonsInvalid = new List<string>();

                    //Get the current object
                    ChildProgramUpload currentObject = objectsToCheck[currentIndex];

                    //Set the program FK
                    currentObject.ProgramFK = currentProgramFK;

                    //Check validity and add reasons to the list
                    //First Name validation
                    if (string.IsNullOrWhiteSpace(currentObject.FirstName))
                    {
                        reasonsInvalid.Add("First Name is missing.");
                    }

                    //Last Name validation
                    if (string.IsNullOrWhiteSpace(currentObject.LastName))
                    {
                        reasonsInvalid.Add("Last Name is missing.");
                    }

                    //DOB validation
                    if (currentObject.DOB.HasValue == false)
                    {
                        reasonsInvalid.Add("DOB is missing or in an invalid format.");
                    }
                    else if (currentObject.DOB.Value > DateTime.Now)
                    {
                        reasonsInvalid.Add("DOB is in the future.");
                    }
                    else if (currentObject.ProgramEnrollmentDate.HasValue && currentObject.DOB.Value > currentObject.ProgramEnrollmentDate.Value)
                    {
                        reasonsInvalid.Add("DOB is after the Program Enrollment Date.");
                    }

                    //ID Number validation
                    if (string.IsNullOrWhiteSpace(currentObject.IDNumber))
                    {
                        reasonsInvalid.Add("ID Number is missing.");
                    }

                    //Program Enrollment Date validation
                    if (currentObject.ProgramEnrollmentDate.HasValue == false)
                    {
                        reasonsInvalid.Add("Program Enrollment Date is missing or in an invalid format.");
                    }
                    else if (currentObject.ProgramEnrollmentDate.Value > DateTime.Now)
                    {
                        reasonsInvalid.Add("Program Enrollment Date is in the future.");
                    }
                    else if (currentObject.ProgramDischargeDate.HasValue && currentObject.ProgramEnrollmentDate.Value > currentObject.ProgramDischargeDate.Value)
                    {
                        reasonsInvalid.Add("Program Enrollment Date is after the Program Discharge Date.");
                    }

                    //Gender validation
                    if (!string.IsNullOrWhiteSpace(currentObject.Gender))
                    {
                        //Get the matching option
                        CodeGender currentGenderOption = genderCodes.Where(gc => gc.Description.Trim().ToUpper() == currentObject.Gender.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching option
                        if(currentGenderOption != null)
                        {
                            //Valid, set the code field
                            currentObject.GenderCodeFK = currentGenderOption.CodeGenderPK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Gender doesn't match the options: {0}.", string.Join(", ", genderCodes.Select(gc => gc.Description).ToList())));
                        }

                        //Validate the specify field
                        if(currentObject.Gender.Trim().ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyGender))
                        {
                            reasonsInvalid.Add("Specify Gender is required when 'Other' is used for the Gender.");
                        }
                    }
                    else
                    {
                        reasonsInvalid.Add("Gender is missing.");
                    }

                    //Ethnicity validation
                    if (!string.IsNullOrWhiteSpace(currentObject.Ethnicity))
                    {
                        //Get the matching option
                        CodeEthnicity currentEthnicityOption = ethnicityCodes.Where(ec => ec.Description.Trim().ToUpper() == currentObject.Ethnicity.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching option
                        if (currentEthnicityOption != null)
                        {
                            //Valid, set the code field
                            currentObject.EthnicityCodeFK = currentEthnicityOption.CodeEthnicityPK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Ethnicity doesn't match the options: {0}.", string.Join(", ", ethnicityCodes.Select(ec => ec.Description).ToList())));
                        }

                        //Validate the specify field
                        if (currentObject.Ethnicity.Trim().ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyEthnicity))
                        {
                            reasonsInvalid.Add("Specify Ethnicity is required when 'Other' is used for the Ethnicity.");
                        }
                    }
                    else
                    {
                        reasonsInvalid.Add("Ethnicity is missing.");
                    }

                    //Race validation
                    if (!string.IsNullOrWhiteSpace(currentObject.Race))
                    {
                        //Get the matching option
                        CodeRace currentRaceOption = raceCodes.Where(rc => rc.Description.Trim().ToUpper() == currentObject.Race.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching option
                        if (currentRaceOption != null)
                        {
                            //Valid, set the code field
                            currentObject.RaceCodeFK = currentRaceOption.CodeRacePK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Race doesn't match the options: {0}.", string.Join(", ", raceCodes.Select(rc => rc.Description).ToList())));
                        }

                        //Validate the specify field
                        if (currentObject.Race.Trim().ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyRace))
                        {
                            reasonsInvalid.Add("Specify Race is required when 'Other' is used for the Race.");
                        }
                    }
                    else
                    {
                        reasonsInvalid.Add("Race is missing.");
                    }

                    //DLL validation
                    if (currentObject.DualLanguageLearner.HasValue == false)
                    {
                        reasonsInvalid.Add("Dual Language Learner is missing.");
                    }

                    //IEP validation
                    if (currentObject.IndividualizedEducationProgram.HasValue == false)
                    {
                        reasonsInvalid.Add("Individualized Education Program is missing.");
                    }

                    //Parent or Guardian Permission validation
                    if (currentObject.ParentOrGuardianPermission.HasValue == false)
                    {
                        reasonsInvalid.Add("Parent Or Guardian Permission is missing.");
                    }
                    else if (currentObject.ParentOrGuardianPermission.Value == false)
                    {
                        reasonsInvalid.Add("Parent Or Guardian Permission must be yes or true in order to continue.");
                    }

                    //Program Discharge Date validation
                    if (currentObject.ProgramDischargeDate.HasValue)
                    {
                        if (currentObject.ProgramDischargeDate.Value > DateTime.Now)
                        {
                            reasonsInvalid.Add("Program Discharge Date is in the future.");
                        }

                        if (string.IsNullOrWhiteSpace(currentObject.ProgramDischargeReason))
                        {
                            reasonsInvalid.Add("Program Discharge Reason is required if a Program Discharge Date is entered.");
                        }
                    }

                    //Program Discharge Reason validation
                    if(!string.IsNullOrWhiteSpace(currentObject.ProgramDischargeReason))
                    {
                        //Get the matching option
                        CodeDischargeReason currentDischargeReason = dischargeCodes.Where(dc => dc.Description.Trim().ToUpper() == currentObject.ProgramDischargeReason.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching option
                        if (currentDischargeReason != null)
                        {
                            //Valid, set the code field
                            currentObject.DischargeReasonCodeFK = currentDischargeReason.CodeDischargeReasonPK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Program Discharge Reason doesn't match the options: {0}.", string.Join(", ", dischargeCodes.Select(dc => dc.Description).ToList())));
                        }

                        if (currentObject.ProgramDischargeDate.HasValue == false)
                        {
                            reasonsInvalid.Add("Program Discharge Date is required if a Program Discharge Reason is entered.");
                        }

                        if (currentObject.ProgramDischargeReason.Trim().ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyProgramDischargeReason))
                        {
                            reasonsInvalid.Add("Specify Program Discharge Reason is required when 'Other' is used for the Program Discharge Reason.");
                        }
                    }

                    //Assigned Classroom ID validation
                    if (!string.IsNullOrWhiteSpace(currentObject.AssignedClassroomID))
                    {
                        //Get the matching classroom
                        Classroom currentClassroom = existingClassrooms.Where(c => c.ProgramSpecificID.Trim().ToUpper() == currentObject.AssignedClassroomID.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching classroom
                        if (currentClassroom != null)
                        {
                            //Valid, set the FK field
                            currentObject.ClassroomFK = currentClassroom.ClassroomPK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Assigned Classroom ID does not match any of the existing classroom IDs in the system: {0}.", string.Join(", ", existingClassrooms.Select(ec => ec.ProgramSpecificID).ToList())));
                        }
                    }
                    else
                    {
                        if (currentObject.ClassroomAssignDate.HasValue || currentObject.ClassroomLeaveDate.HasValue || !string.IsNullOrWhiteSpace(currentObject.ClassroomLeaveReason) || !string.IsNullOrWhiteSpace(currentObject.SpecifyClassroomLeaveReason))
                        {
                            reasonsInvalid.Add("Assigned Classroom ID is required if any classroom assignment information is entered.");
                        }
                    }

                    //Classroom Assign Date validation
                    if (currentObject.ClassroomAssignDate.HasValue)
                    {
                        if (currentObject.ClassroomAssignDate.Value > DateTime.Now)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is in the future.");
                        }
                        else if(currentObject.ClassroomLeaveDate.HasValue && currentObject.ClassroomAssignDate.Value > currentObject.ClassroomLeaveDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is after the Classroom Leave Date.");
                        }
                        else if (currentObject.ProgramEnrollmentDate.HasValue && currentObject.ClassroomAssignDate.Value < currentObject.ProgramEnrollmentDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is before the Program Enrollment Date.");
                        }
                        else if (currentObject.ProgramDischargeDate.HasValue && currentObject.ClassroomAssignDate.Value > currentObject.ProgramDischargeDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is after the Program Discharge Date.");
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrWhiteSpace(currentObject.AssignedClassroomID))
                        {
                            reasonsInvalid.Add("Classroom Assign Date is required if an Assigned Classroom ID is entered.");
                        }
                    }

                    //Classroom Leave Date validation
                    if (currentObject.ClassroomLeaveDate.HasValue)
                    {
                        if (currentObject.ClassroomLeaveDate.Value > DateTime.Now)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is in the future.");
                        }
                        else if (currentObject.ProgramEnrollmentDate.HasValue && currentObject.ClassroomLeaveDate.Value < currentObject.ProgramEnrollmentDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is before the Program Enrollment Date.");
                        }
                        else if (currentObject.ProgramDischargeDate.HasValue && currentObject.ClassroomLeaveDate.Value > currentObject.ProgramDischargeDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is after the Program Discharge Date.");
                        }

                        if (string.IsNullOrWhiteSpace(currentObject.ClassroomLeaveReason))
                        {
                            reasonsInvalid.Add("Classroom Leave Reason is required if a Classroom Leave Date is entered.");
                        }
                    }

                    //Classroom Leave Reason validation
                    if(!string.IsNullOrWhiteSpace(currentObject.ClassroomLeaveReason))
                    {
                        //Get the matching leave reason
                        CodeChildLeaveReason currentLeaveReason = clasroomLeaveResons.Where(clr => clr.Description.Trim().ToUpper() == currentObject.ClassroomLeaveReason.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching leave reason
                        if (currentLeaveReason != null)
                        {
                            //Valid, set the code field
                            currentObject.ClassroomLeaveReasonCodeFK = currentLeaveReason.CodeChildLeaveReasonPK;
                        }
                        else 
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Classroom Leave Reason doesn't match the options: {0}.", string.Join(", ", clasroomLeaveResons.Select(dc => dc.Description).ToList())));
                        }

                        if (currentObject.ClassroomLeaveDate.HasValue == false)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is required if a Classroom Leave Reason is entered.");
                        }

                        if (currentObject.ClassroomLeaveReason.Trim().ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyClassroomLeaveReason))
                        {
                            reasonsInvalid.Add("Specify Classroom Leave Reason is required when 'Other' is used for the Classroom Leave Reason.");
                        }
                    }                    

                    //Duplication validation
                    //Before checking, ensure that the IDNumber, FirstName, LastName, and DOB fields are filled out
                    if(string.IsNullOrWhiteSpace(currentObject.IDNumber) == false && string.IsNullOrWhiteSpace(currentObject.FirstName) == false && string.IsNullOrWhiteSpace(currentObject.LastName) == false && currentObject.DOB.HasValue)
                    {
                        if (existingChildren.Where(c => c.ProgramSpecificID.Trim().ToUpper() == currentObject.IDNumber.Trim().ToUpper()).Count() > 0 )
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is already a child in the system with the same ID Number.");
                        }
                        else if(existingChildren.Where(c => c.Child.FirstName.Trim().ToUpper() == currentObject.FirstName.Trim().ToUpper() &&
                                                            c.Child.LastName.Trim().ToUpper() == currentObject.LastName.Trim().ToUpper() &&
                                                            c.Child.BirthDate == currentObject.DOB.Value).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is already a child in the system with the same first name, last name, and DOB.");
                        }
                        else if(objectsToCheck.Where(o => o != currentObject && 
                                                                    string.IsNullOrWhiteSpace(o.IDNumber) == false && 
                                                                    o.IDNumber.Trim().ToUpper() == currentObject.IDNumber.Trim().ToUpper()).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is another row in this file with the same ID Number.");
                        }
                        else if (objectsToCheck.Where(o => o != currentObject &&
                                                                     string.IsNullOrWhiteSpace(o.FirstName) == false &&
                                                                     string.IsNullOrWhiteSpace(o.LastName) == false &&
                                                                     o.DOB.HasValue &&
                                                                     o.FirstName.Trim().ToUpper() == currentObject.FirstName.Trim().ToUpper() &&
                                                                     o.LastName.Trim().ToUpper() == currentObject.LastName.Trim().ToUpper() &&
                                                                     o.DOB.Value == currentObject.DOB.Value).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is another row in this file with the same first name, last name, and DOB.");
                        }

                    }

                    //Set the IsValid field and ReasonsInvalid field
                    if (reasonsInvalid.Count > 0)
                    {
                        currentObject.IsValid = false;
                        currentObject.ReasonsInvalid = string.Join(Environment.NewLine, reasonsInvalid);
                    }
                    else
                    {
                        currentObject.IsValid = true;
                        currentObject.ReasonsInvalid = null;
                    }
                }
            }

            /// <summary>
            /// This method returns an IEnumerable of objects that represent the rows in a CSV file.
            /// </summary>
            /// <param name="currentReader">The CsvReader object that represents the CSV file</param>
            /// <param name="currentDBContext">The current database context</param>
            /// <param name="currentProgramFK">The program FK to check existing objects</param>
            /// <returns>An IEnumerable of objects that represent the rows in the passed CSV file</returns>
            public IEnumerable<IImportable> GetResultsFromReader(CsvReader currentReader, PyramidContext currentDBContext, int? currentProgramFK)
            {
                //Get the results as a list of objects from the CSV file
                List<ChildProgramUpload> uploadedChildRecords = currentReader.GetRecords<ChildProgramUpload>().ToList();

                //Get the existing objects from the DB
                List<ChildProgram> existingChildren = currentDBContext.ChildProgram.Include(cp => cp.Child).AsNoTracking().Where(cp => cp.ProgramFK == currentProgramFK.Value).ToList();

                //Get the code table information
                List<CodeGender> genders = currentDBContext.CodeGender.AsNoTracking().OrderBy(cg => cg.Description).ToList();
                List<CodeEthnicity> ethnicities = currentDBContext.CodeEthnicity.AsNoTracking().OrderBy(ce => ce.Description).ToList();
                List<CodeRace> races = currentDBContext.CodeRace.AsNoTracking().OrderBy(cr => cr.Description).ToList();
                List<CodeDischargeReason> dischargeCodes = currentDBContext.CodeDischargeReason.AsNoTracking().OrderBy(cdr => cdr.Description).ToList();
                List<Classroom> existingClassrooms = currentDBContext.Classroom.AsNoTracking().Where(c => c.ProgramFK == currentProgramFK.Value).OrderBy(c => c.ProgramSpecificID).ToList();
                List<CodeChildLeaveReason> classroomLeaveReasons = currentDBContext.CodeChildLeaveReason.AsNoTracking().OrderBy(clr => clr.Description).ToList();

                //Set the validation fields in the objects
                SetValidationAndFKFields(ref uploadedChildRecords, genders, ethnicities, races, dischargeCodes, classroomLeaveReasons, existingClassrooms, existingChildren, currentProgramFK.Value);

                //Return the objects
                return uploadedChildRecords;
            }

            /// <summary>
            /// This method saves the passed objects to the database.
            /// </summary>
            /// <param name="currentDBContext">The current database context</param>
            /// <param name="objectsToSave">The objects to save to the database</param>
            /// <param name="creator">The username of the user saving the objects</param>
            public void SaveRangeToDatabase(PyramidContext currentDBContext, IEnumerable<IImportable> objectsToSave, string creator)
            {
                //Cast the parameter to the correct type
                List<ChildProgramUpload> importRows = objectsToSave.Cast<ChildProgramUpload>().ToList();

                //Create the correct EF objects out of the valid import rows
                IEnumerable<ChildProgram> objectsToAdd = importRows.Where(o => o.IsValid == true).Select(cp => new ChildProgram()
                {
                    Child = new Child() {
                        Creator = creator,
                        CreateDate = DateTime.Now,
                        FirstName = cp.FirstName,
                        LastName = cp.LastName,
                        BirthDate = cp.DOB.Value,
                        GenderCodeFK = cp.GenderCodeFK.Value,
                        GenderSpecify = (!string.IsNullOrWhiteSpace(cp.Gender) && cp.Gender.Trim().ToUpper() == "OTHER" ? cp.SpecifyGender : null),
                        EthnicityCodeFK = cp.EthnicityCodeFK.Value,
                        EthnicitySpecify = (!string.IsNullOrWhiteSpace(cp.Ethnicity) && cp.Ethnicity.Trim().ToUpper() == "OTHER" ? cp.SpecifyEthnicity : null),
                        RaceCodeFK = cp.RaceCodeFK.Value,
                        RaceSpecify = (!string.IsNullOrWhiteSpace(cp.Race) && cp.Race.Trim().ToUpper() == "OTHER" ? cp.SpecifyRace : null),
                        ChildClassroom = (cp.ClassroomFK.HasValue == false ? null : new List<ChildClassroom>()
                        {
                            new ChildClassroom()
                            {
                                Creator = creator,
                                CreateDate = DateTime.Now,
                                AssignDate = cp.ClassroomAssignDate.Value,
                                ClassroomFK = cp.ClassroomFK.Value,
                                LeaveDate = cp.ClassroomLeaveDate,
                                LeaveReasonCodeFK = cp.ClassroomLeaveReasonCodeFK,
                                LeaveReasonSpecify = (!string.IsNullOrWhiteSpace(cp.ClassroomLeaveReason) && cp.ClassroomLeaveReason.Trim().ToUpper() == "OTHER" ? cp.SpecifyClassroomLeaveReason : null)
                            }
                        })
                    },
                    Creator = creator,
                    CreateDate = DateTime.Now,
                    EnrollmentDate = cp.ProgramEnrollmentDate.Value,
                    IsDLL = cp.DualLanguageLearner.Value,
                    HasIEP = cp.IndividualizedEducationProgram.Value,
                    HasParentPermission = cp.ParentOrGuardianPermission.Value,
                    ProgramSpecificID = cp.IDNumber,
                    DischargeDate = cp.ProgramDischargeDate,
                    DischargeCodeFK = cp.DischargeReasonCodeFK,
                    DischargeReasonSpecify = (!string.IsNullOrWhiteSpace(cp.ProgramDischargeReason) && cp.ProgramDischargeReason.Trim().ToUpper() == "OTHER" ? cp.SpecifyProgramDischargeReason : null),
                    ProgramFK = cp.ProgramFK
                });

                //Add the objects to the database and save the changes
                currentDBContext.ChildProgram.AddRange(objectsToAdd);
                currentDBContext.SaveChanges();
            }
        }
    }
}