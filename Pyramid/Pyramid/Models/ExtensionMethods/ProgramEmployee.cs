using System;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using Pyramid.FileImport.CodeFiles;
using CsvHelper.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Entity;

namespace Pyramid.Models
{
    public partial class ProgramEmployee
    {
        public class ProgramEmployeeUpload : IImportable
        {
            public bool IsValid { get; set; }
            public bool IsDuplicate { get; set; }
            public string ReasonsInvalid { get; set; }
            public string DisplayName => "Professional";
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string IDNumber { get; set; }
            public string Gender { get; set; }
            public int? GenderCodeFK { get; set; }
            public string SpecifyGender { get; set; }
            public string Ethnicity { get; set; }
            public int? EthnicityCodeFK { get; set; }
            public string SpecifyEthnicity { get; set; }
            public string Race { get; set; }
            public int? RaceCodeFK { get; set; }
            public string SpecifyRace { get; set; }
            public DateTime? ProfessionalStartDate { get; set; }
            public string EmailAddress { get; set; }
            public bool? IsEmployeeOfProgram { get; set; }
            public DateTime? ProfessionalSeparationDate { get; set; }
            public string ProfessionalSeparationReason { get; set; }
            public int? TerminationReasonCodeFK { get; set; }
            public string SpecifyProfessionalSeparationReason { get; set; }
            public string JobFunction { get; set; }
            public int? JobTypeCodeFK { get; set; }
            public DateTime? JobFunctionStartDate { get; set; }
            public DateTime? JobFunctionEndDate { get; set; }
            public string AssignedClassroomID { get; set; }
            public int? ClassroomFK { get; set; }
            public DateTime? ClassroomAssignDate { get; set; }
            public DateTime? ClassroomLeaveDate { get; set; }
            public string ClassroomLeaveReason { get; set; }
            public int? ClassroomLeaveReasonCodeFK { get; set; }
            public string SpecifyClassroomLeaveReason { get; set; }
            public int ProgramFK { get; set; }

            public class ProgramEmployeeUploadCsvClassMap : ClassMap<ProgramEmployeeUpload>
            {
                public ProgramEmployeeUploadCsvClassMap()
                {
                    Map(m => m.FirstName).Index(0);
                    Map(m => m.LastName).Index(1);
                    Map(m => m.IDNumber).Index(2);
                    Map(m => m.Gender).Index(3);
                    Map(m => m.SpecifyGender).Index(4);
                    Map(m => m.Ethnicity).Index(5);
                    Map(m => m.SpecifyEthnicity).Index(6);
                    Map(m => m.Race).Index(7);
                    Map(m => m.SpecifyRace).Index(8);
                    Map(m => m.ProfessionalStartDate).Index(9).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.EmailAddress).Index(10);
                    Map(m => m.IsEmployeeOfProgram).Index(11).TypeConverter<CsvHelperExtensions.CustomBoolConverter<bool?>>();
                    Map(m => m.ProfessionalSeparationDate).Index(12).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.ProfessionalSeparationReason).Index(13);
                    Map(m => m.SpecifyProfessionalSeparationReason).Index(14);
                    Map(m => m.JobFunction).Index(15);
                    Map(m => m.JobFunctionStartDate).Index(16).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.JobFunctionEndDate).Index(17).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.AssignedClassroomID).Index(18);
                    Map(m => m.ClassroomAssignDate).Index(19).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.ClassroomLeaveDate).Index(20).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.ClassroomLeaveReason).Index(21);
                    Map(m => m.SpecifyClassroomLeaveReason).Index(22);
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
                    "TerminationReasonCodeFK",
                    "JobTypeCodeFK",
                    "ClassroomFK",
                    "ClassroomLeaveReasonCodeFK"
                };

                //Return the list
                return fieldsToHide;
            }

            /// <summary>
            /// This method returns the instructions for importing the CSV file.
            /// </summary>
            /// <returns>A string that contains the instructions for importing the CSV file.  String is in HTML format.</returns>
            public string GetImportInstructionsHTML()
            {
                //To hold the instructions
                StringBuilder importInstructions = new StringBuilder();

                //Add the instructions
                importInstructions.Append("This page allows you to upload a CSV file that contains information for multiple professionals in your program.  It also allows you to import a single job function and/or classroom assignment.  Please follow these instructions to import professional rosters:");
                importInstructions.Append("<ol>");
                importInstructions.Append("<li>Download the template CSV file in this section.</li>");
                importInstructions.Append("<li>(Optional) Download the example CSV file in this section.  This file is the same format as the template file, with a few rows of example information entered.</li>");
                importInstructions.Append("<li>It is recommended that you open the template and example CSV files using <strong>Microsoft Excel</strong> for ease of data-entry.  However, you can use any program capable of handling CSV files.</li>");
                importInstructions.Append("<li>In the downloaded template CSV file, add all the professional information, optional job function information, and optional classroom assignment information that you would like to import.</li>");
                importInstructions.Append("<li>Make sure the information that you added to the file matches the requirements for the fields that are listed in the <strong>CSV File Information</strong> table that is in the <strong>CSV File Information</strong> section below.</li>");
                importInstructions.Append("<li>Save the template CSV file after you have added all the necessary information.</li>");
                importInstructions.Append("<li>Scroll down to the <strong>Import CSV File</strong> section at the bottom of the page.</li>");
                importInstructions.Append("<li>In the <strong>Import CSV File</strong> section, ensure the program selected in the <strong>Program</strong> drop-down list is the one that you wish to add professionals to.</li>");
                importInstructions.Append("<li>Click the browse button in the <strong>Select File to Import</strong> field and select the template file that you filled out and saved.</li>");
                importInstructions.Append("<li>Click the <strong>Import and Preview Results</strong> button.  This will take you to a new page to preview the import results before the final confirmation to add the professionals to the system.</li>");
                importInstructions.Append("<li><strong>Professionals will not be saved to PIDS until you confirm the import on the next page.</strong></li>");
                importInstructions.Append("</ol>");

                //Return the instructions
                return importInstructions.ToString();
            }

            /// <summary>
            /// This method returns the instructions for confirming the employee CSV file import.
            /// </summary>
            /// <returns>A string that contains the instructions for confirming the CSV file import.  String is in HTML format.</returns>
            public string GetConfirmationInstructionsHTML()
            {
                //To hold the instructions
                StringBuilder importInstructions = new StringBuilder();

                //Add the instructions
                importInstructions.Append("This page allows you to review the professional information that you uploaded before saving it to PIDS.  Please follow these instructions to save the professional information:");
                importInstructions.Append("<ol>");
                importInstructions.Append("<li>All the professional information that was in the CSV file you uploaded should appear in the <strong>Import Result Preview</strong> table below.</li>");
                importInstructions.Append("<li>Review all the rosters that are listed in the table to make sure they look correct.</li>");
                importInstructions.Append("<li>Take note of the <strong>Is Valid</strong> field.  If there is a green checkmark in this field, the professional information is valid and will be saved.  If there is a red X in this field, the professional information is invalid and will <strong>not</strong> be saved.</li>");
                importInstructions.Append("<li>If the <strong>Is Valid</strong> field has a red X, look at the <strong>Reasons Invalid</strong> field to see an explanation of why the field is not valid.  <strong>Note:</strong> Even if some of the rosters are invalid and will not be saved, they will not interfere with saving the valid rosters.  Valid rosters will always be saved, regardless of how many invalid rosters there are.</li>");
                importInstructions.Append("<li>Once you have verified that all the information looks correct, click the <strong>Save Valid Records</strong> button.</li>");
                importInstructions.Append("<li>In the pop-up that appears, read the warning make the final confirmation to save the rosters.</li>");
                importInstructions.Append("</ol>");

                //Return the instructions
                return importInstructions.ToString();
            }

            /// <summary>
            /// This method returns information about the fields in the employee CSV file.
            /// </summary>
            /// <returns>A list of ImportFileFieldDetail objects that contain descriptions of the fields in the CSV file.</returns>
            public List<ImportFileFieldDetail> GetFileFieldInformation()
            {
                //To hold the options for the code fields
                List<string> genderOptions, ethnicityOptions, raceOptions, terminationOptions, jobFunctionOptions, classroomLeaveReasons;

                using (PyramidContext context = new PyramidContext())
                {
                    //Get the code table information
                    genderOptions = context.CodeGender.AsNoTracking().OrderBy(cg => cg.Description).Select(cg => cg.Description).ToList();
                    ethnicityOptions = context.CodeEthnicity.AsNoTracking().OrderBy(ce => ce.Description).Select(ce => ce.Description).ToList();
                    raceOptions = context.CodeRace.AsNoTracking().OrderBy(cr => cr.Description).Select(cr => cr.Description).ToList();
                    terminationOptions = context.CodeTermReason.AsNoTracking().OrderBy(ctr => ctr.Description).Select(ctr => ctr.Description).ToList();
                    jobFunctionOptions = context.CodeJobType.AsEnumerable().OrderBy(cjt => cjt.Description).Select(cjt => cjt.Description).ToList();
                    classroomLeaveReasons = context.CodeEmployeeLeaveReason.AsNoTracking().OrderBy(celr => celr.Description).Select(celr => celr.Description).ToList();
                }

                //Get the list if objects
                List<ImportFileFieldDetail> listToReturn = new List<ImportFileFieldDetail>()
                {
                    new ImportFileFieldDetail("First Name", "Text", true, "This is the professional's legal first name.", "Any text."),
                    new ImportFileFieldDetail("Last Name", "Text", true, "This is the professional's legal last name.", "Any text."),
                    new ImportFileFieldDetail("ID Number", "Text", true, "This is the ID number assigned to the professional by the program that is uploading this information.", "Any combination of letters and numbers.  This field must not have any duplication within the file.  If the program doesn't assign ID numbers to their professionals, please implement an ID number scheme that uses letters and numbers to create a unique ID number for every professional that you wish to import.  If you are creating an ID number scheme, we recommend that the ID numbers do not contain the professional's initials, DOB, or other personally-identifiable information."),
                    new ImportFileFieldDetail("Gender", "Text", true, "This is the professional's gender.", string.Format("The text in this field must match one of the following options (not case-sensitive):<br> {0}.", string.Join(",<br>", genderOptions))),
                    new ImportFileFieldDetail("Specify Gender", "Text", false, "A description of the professional's gender with a maximum length of 100 characters.", "If 'Other' is entered for the professional's gender, then this is required.  Otherwise, it should be left blank."),
                    new ImportFileFieldDetail("Ethnicity", "Text", true, "This is the professional's ethnicity.", string.Format("The text in this field must match one of the following options (not case-sensitive):<br> {0}.", string.Join(",<br>", ethnicityOptions))),
                    new ImportFileFieldDetail("Specify Ethnicity", "Text", false, "A description of the professional's ethnicity with a maximum length of 100 characters.", "If 'Other' is entered for the professional's ethnicity, then this is required.  Otherwise, it should be left blank."),
                    new ImportFileFieldDetail("Race", "Text", true, "This is the professional's race.", string.Format("The text in this field must match one of the following options (not case-sensitive):<br> {0}.", string.Join(",<br>", raceOptions))),
                    new ImportFileFieldDetail("Specify Race", "Text", false, "A description of the professional's race with a maximum length of 100 characters.", "If 'Other' is entered for the professional's race, then this is required.  Otherwise, it should be left blank."),
                    new ImportFileFieldDetail("Professional Start Date", "Date", true, "This is the date the professional started working with the program.", "Any valid date that is not in the future and not after the Professional Separation Date (if entered)."),
                    new ImportFileFieldDetail("Email Address", "Text", true, "This is the professional's email address.", "Any valid email address that isn't already taken by a professional in the system or in this import."),
                    new ImportFileFieldDetail("Is this Person an Employee of this Program?", "Text", true, "This is whether or not the professional is an employee of the program.", "The text in this field must match one of the following options (not case-sensitive):<br> yes,<br> no,<br> true,<br> false."),
                    new ImportFileFieldDetail("Professional Separation Date", "Date", false, "This is the date the professional was separated from the program.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future and the Professional Separation Reason field is required."),
                    new ImportFileFieldDetail("Professional Separation Reason", "Text", false, "This is the reason that the professional was separated from the program.", string.Format("Since this field is optional, it may be left blank.  If entered, the Separation Date field is required and the text in this field must match one of following options (not case-sensitive):<br> {0}.", string.Join(",<br>", terminationOptions))),
                    new ImportFileFieldDetail("Specify Professional Separation Reason", "Text", false, "More details about the Professional Separation Reason.", "If 'Other' was used as the Professional Separation Reason for this professional, this is required and can be any text.  Otherwise this field will be ignored and can be left blank."),
                    new ImportFileFieldDetail("Job Function", "Text", false, "This is the professional's job function.", string.Format("Since this field is optional, it may be left blank.  If entered, the Job Function Start Date field is required and the text in this field must match one of following options (not case-sensitive):<br> {0}.", string.Join(",<br>", jobFunctionOptions))),
                    new ImportFileFieldDetail("Job Function Start Date", "Date", false, "This is the date the professional started working in the job function.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future, not before the Professional Start Date, and not after the Professional Separation Date"),
                    new ImportFileFieldDetail("Job Function End Date", "Date", false, "This is the date the professional stopped working in the job function.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future, not before the Professional Start Date, and not after the Professional Separation Date."),
                    new ImportFileFieldDetail("Assigned Classroom ID", "Text", false, "This is the ID number for the classroom that the professional is assigned to.", "Since this field is optional, it may be left blank.  If entered, it must be a combination of letters and numbers that matches the ID Number (not case-sensitive) of one of the existing classrooms in the program.  If entered, job function information must be entered."),
                    new ImportFileFieldDetail("Classroom Assign Date", "Date", false, "This is the date the professional was assigned to the classroom.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future, not before the Job Function Start Date, not after the Job Function End Date, and not after the Classroom Leave Date."),
                    new ImportFileFieldDetail("Classroom Leave Date", "Date", false, "This is the date the professional left the classroom.", "Since this field is optional, it may be left blank.  If entered, it must be a valid date that is not in the future, not before the Job Function Start Date, not after the Job Function End Date, and the Classroom Leave Reason field is required."),
                    new ImportFileFieldDetail("Classroom Leave Reason", "Text", false, "This is the reason that the professional left the classroom.", string.Format("Since this field is optional, it may be left blank.  If entered, the Classroom Leave Date field is required the text in this field must match one of following options (not case-sensitive):<br> {0}.", string.Join(",<br>", classroomLeaveReasons))),
                    new ImportFileFieldDetail("Specify Classroom Leave Reason", "Text", false, "More details about the Classroom Leave Reason.", "If 'Other' was used as the Classroom Leave Reason for this professional, this is required and can be any text.  Otherwise this field will be ignored and can be left blank.")
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
                return "/FileImport/ExcelTemplates/EmployeeProgramUpload.csv";
            }

            /// <summary>
            /// This method returns the path to the CSV example file
            /// </summary>
            /// <returns>The path to the CSV example file</returns>
            public string GetImportExampleFilePath()
            {
                return "/FileImport/ExcelTemplates/EmployeeProgramUpload-Example.csv";
            }

            /// <summary>
            /// This method registers this object's class map with the CsvReader object passed to the method.
            /// </summary>
            /// <param name="currentReader">The CsvReader object that represents a CSV file</param>
            public void RegisterClassMapWithReader(CsvReader currentReader)
            {
                currentReader.Context.RegisterClassMap<ProgramEmployeeUploadCsvClassMap>();
            }

            /// <summary>
            /// This methods sets the validation fields in the passed list of objects.
            /// </summary>
            /// <param name="objectsToCheck">The objects to check for validity</param>
            /// <param name="genderCodes">The list of available Gender options</param>
            /// <param name="ethnicityCodes">The list of available Ethnicity options</param>
            /// <param name="raceCodes">The list of available race options</param>
            /// <param name="terminationCodes">The list of available termination reason options</param>
            /// <param name="clasroomLeaveResons">The list of available classroom leave reasons</param>
            /// <param name="existingClassrooms">The list of existing classrooms</param>
            /// <param name="existingEmployees">The list of existing employees</param>
            private static void SetValidationAndFKFields(ref List<ProgramEmployeeUpload> objectsToCheck, List<CodeGender> genderCodes, List<CodeEthnicity> ethnicityCodes,
                List<CodeRace> raceCodes, List<CodeTermReason> terminationCodes, List<CodeEmployeeLeaveReason> clasroomLeaveResons,
                List<CodeJobType> jobFunctionOptions, List<Classroom> existingClassrooms, List<ProgramEmployee> existingEmployees, int currentProgramFK)
            {
                //Loop through the objects and set the validation fields
                for (int currentIndex = 0; currentIndex < objectsToCheck.Count; currentIndex++)
                {
                    //To hold the reasons the row is invalid
                    List<string> reasonsInvalid = new List<string>();

                    //Get the current object
                    ProgramEmployeeUpload currentObject = objectsToCheck[currentIndex];

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

                    //ID Number validation
                    if (string.IsNullOrWhiteSpace(currentObject.IDNumber))
                    {
                        reasonsInvalid.Add("ID Number is missing.");
                    }

                    //Gender validation
                    if (!string.IsNullOrWhiteSpace(currentObject.Gender))
                    {
                        //Get the matching option
                        CodeGender currentGenderOption = genderCodes.Where(gc => gc.Description.Trim().ToUpper() == currentObject.Gender.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching option
                        if (currentGenderOption != null)
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
                        if (currentObject.Gender.Trim().ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyGender))
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

                    //Professional Start Date validation
                    if (currentObject.ProfessionalStartDate.HasValue == false)
                    {
                        reasonsInvalid.Add("Professional Start Date is missing or in an invalid format.");
                    }
                    else if (currentObject.ProfessionalStartDate.Value > DateTime.Now)
                    {
                        reasonsInvalid.Add("Professional Start Date is in the future.");
                    }
                    else if (currentObject.ProfessionalSeparationDate.HasValue && currentObject.ProfessionalStartDate.Value > currentObject.ProfessionalSeparationDate.Value)
                    {
                        reasonsInvalid.Add("Professional Start Date is after the Professional Separation Date.");
                    }

                    //Email Address validation
                    if (!string.IsNullOrWhiteSpace(currentObject.EmailAddress))
                    {
                        if (!Regex.IsMatch(currentObject.EmailAddress, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        {
                            reasonsInvalid.Add("Email Address is not in a valid format.");
                        }
                        else if (existingEmployees.Where(e => e.Employee.EmailAddress.Trim().ToUpper() == currentObject.EmailAddress.Trim().ToUpper()).Count() > 0)
                        {
                            reasonsInvalid.Add("Email Address is already taken by a professional in the system.");
                        }
                        else if (objectsToCheck.Where(o => o != currentObject &&
                                                                     string.IsNullOrWhiteSpace(o.EmailAddress) == false &&
                                                                     o.EmailAddress.Trim().ToUpper() == currentObject.EmailAddress.Trim().ToUpper()).Count() > 0)
                        {
                            reasonsInvalid.Add("Email Address is the same as the email address for another professional in this file.");
                        }
                    }
                    else
                    {
                        reasonsInvalid.Add("Email Address is missing.");
                    }

                    //Is employee of program? validation
                    if (currentObject.IsEmployeeOfProgram.HasValue == false)
                    {
                        reasonsInvalid.Add("Is this Person an Employee of this Program? is missing.");
                    }

                    //Program Termination Date validation
                    if (currentObject.ProfessionalSeparationDate.HasValue)
                    {
                        if (currentObject.ProfessionalSeparationDate.Value > DateTime.Now)
                        {
                            reasonsInvalid.Add("Professional Separation Date is in the future.");
                        }

                        if (string.IsNullOrWhiteSpace(currentObject.ProfessionalSeparationReason))
                        {
                            reasonsInvalid.Add("Professional Separation Reason is required if a Professional Separation Date is entered.");
                        }
                    }
                    
                    //Program Termination Reason validation
                    if(!string.IsNullOrWhiteSpace(currentObject.ProfessionalSeparationReason))
                    {
                        //Get the current termination reason
                        CodeTermReason currentTerminationReason = terminationCodes.Where(dc => dc.Description.ToUpper() == currentObject.ProfessionalSeparationReason.ToUpper()).FirstOrDefault();

                        //Validate the current termination reason
                        if (currentTerminationReason != null)
                        {
                            //Valid, set the code field
                            currentObject.TerminationReasonCodeFK = currentTerminationReason.CodeTermReasonPK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Professional Separation Reason doesn't match the options: {0}.", string.Join(", ", terminationCodes.Select(dc => dc.Description).ToList())));
                        }

                        if (currentObject.ProfessionalSeparationDate.HasValue == false)
                        {
                            reasonsInvalid.Add("Professional Separation Date is required if a Professional Separation Reason is entered.");
                        }

                        if (currentObject.ProfessionalSeparationReason.ToUpper() == "OTHER" && string.IsNullOrWhiteSpace(currentObject.SpecifyProfessionalSeparationReason))
                        {
                            reasonsInvalid.Add("Specify Professional Separation Reason is required if 'Other' was used for the Professional Separation Reason.");
                        }
                    }

                    //Job Function validation
                    if(!string.IsNullOrWhiteSpace(currentObject.JobFunction))
                    {
                        //Get the matching option
                        CodeJobType currentJobType = jobFunctionOptions.Where(jf => jf.Description.ToUpper() == currentObject.JobFunction.ToUpper()).FirstOrDefault();

                        //Validate the option
                        if(currentJobType != null)
                        {
                            //Valid, set the code field
                            currentObject.JobTypeCodeFK = currentJobType.CodeJobTypePK;
                        }
                        else
                        {
                            //Invalid, add a validation message
                            reasonsInvalid.Add(string.Format("Job Function doesn't match the options: {0}.", string.Join(", ", jobFunctionOptions.Select(jf => jf.Description).ToList())));
                        }
                    }
                    else
                    {
                        if(currentObject.JobFunctionStartDate.HasValue || currentObject.JobFunctionEndDate.HasValue)
                        {
                            reasonsInvalid.Add("Job Function is required if the Job Function Start Date or Job Function End Date information is entered.");
                        }
                    }

                    //Job Function Start Date validation
                    if (currentObject.JobFunctionStartDate.HasValue)
                    {
                        if (currentObject.JobFunctionStartDate.Value > DateTime.Now)
                        {
                            reasonsInvalid.Add("Job Function Start Date is in the future.");
                        }
                        else if (currentObject.JobFunctionEndDate.HasValue && currentObject.JobFunctionStartDate.Value > currentObject.JobFunctionEndDate.Value)
                        {
                            reasonsInvalid.Add("Job Function Start Date is after the Job Function End Date.");
                        }
                        else if (currentObject.ProfessionalStartDate.HasValue && currentObject.JobFunctionStartDate.Value < currentObject.ProfessionalStartDate.Value)
                        {
                            reasonsInvalid.Add("Job Function Start Date is before the Professional Start Date.");
                        }
                        else if (currentObject.ProfessionalSeparationDate.HasValue && currentObject.JobFunctionStartDate.Value > currentObject.ProfessionalSeparationDate.Value)
                        {
                            reasonsInvalid.Add("Job Function Start Date is after the Professional Separation Date.");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(currentObject.JobFunction))
                        {
                            reasonsInvalid.Add("Job Function Start Date is required if a Job Function is entered.");
                        }
                    }

                    //Job Function End Date validation
                    if (currentObject.JobFunctionEndDate.HasValue)
                    {
                        if (currentObject.JobFunctionEndDate.Value > DateTime.Now)
                        {
                            reasonsInvalid.Add("Job Function End Date is in the future.");
                        }
                        else if (currentObject.ProfessionalStartDate.HasValue && currentObject.JobFunctionEndDate.Value < currentObject.ProfessionalStartDate.Value)
                        {
                            reasonsInvalid.Add("Job Function End Date is before the Professional Start Date.");
                        }
                        else if (currentObject.ProfessionalSeparationDate.HasValue && currentObject.JobFunctionEndDate.Value > currentObject.ProfessionalSeparationDate.Value)
                        {
                            reasonsInvalid.Add("Job Function End Date is after the Professional Separation Date.");
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

                        if (string.IsNullOrWhiteSpace(currentObject.JobFunction))
                        {
                            reasonsInvalid.Add("Job Function information is required if entering a classroom assignment.");
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
                        else if (currentObject.ClassroomLeaveDate.HasValue && currentObject.ClassroomAssignDate.Value > currentObject.ClassroomLeaveDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is after the Classroom Leave Date.");
                        }
                        else if (currentObject.ProfessionalStartDate.HasValue && currentObject.ClassroomAssignDate.Value < currentObject.ProfessionalStartDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is before the Professional Start Date.");
                        }
                        else if (currentObject.ProfessionalSeparationDate.HasValue && currentObject.ClassroomAssignDate.Value > currentObject.ProfessionalSeparationDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is after the Professional Separation Date.");
                        }
                        else if (currentObject.JobFunctionStartDate.HasValue && currentObject.ClassroomAssignDate.Value < currentObject.JobFunctionStartDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is before the Job Function Start Date.");
                        }
                        else if (currentObject.JobFunctionEndDate.HasValue && currentObject.ClassroomAssignDate.Value > currentObject.JobFunctionEndDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Assign Date is after the Job Function End Date.");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(currentObject.AssignedClassroomID))
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
                        else if (currentObject.ProfessionalStartDate.HasValue && currentObject.ClassroomLeaveDate.Value < currentObject.ProfessionalStartDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is before the Professional Start Date.");
                        }
                        else if (currentObject.ProfessionalSeparationDate.HasValue && currentObject.ClassroomLeaveDate.Value > currentObject.ProfessionalSeparationDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is after the Professional Separation Date.");
                        }
                        else if (currentObject.JobFunctionStartDate.HasValue && currentObject.ClassroomLeaveDate.Value < currentObject.JobFunctionStartDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is before the Job Function Start Date.");
                        }
                        else if (currentObject.JobFunctionEndDate.HasValue && currentObject.ClassroomLeaveDate.Value > currentObject.JobFunctionEndDate.Value)
                        {
                            reasonsInvalid.Add("Classroom Leave Date is after the Job Function End Date.");
                        }

                        if (string.IsNullOrWhiteSpace(currentObject.ClassroomLeaveReason))
                        {
                            reasonsInvalid.Add("Classroom Leave Reason is required if a Classroom Leave Date is entered.");
                        }
                    }

                    //Classroom Leave Reason validation
                    if (!string.IsNullOrWhiteSpace(currentObject.ClassroomLeaveReason))
                    {
                        //Get the matching leave reason
                        CodeEmployeeLeaveReason currentLeaveReason = clasroomLeaveResons.Where(clr => clr.Description.Trim().ToUpper() == currentObject.ClassroomLeaveReason.Trim().ToUpper()).FirstOrDefault();

                        //Validate the matching leave reason
                        if (currentLeaveReason != null)
                        {
                            //Valid, set the code field
                            currentObject.ClassroomLeaveReasonCodeFK = currentLeaveReason.CodeEmployeeLeaveReasonPK;
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
                            reasonsInvalid.Add("Specify Classroom Leave Reason is required if 'Other' was used for the Classroom Leave Reason.");
                        }
                    }

                    //Duplication validation
                    //Before checking, ensure that the IDNumber, FirstName, LastName, and ProfessionalStartDate fields are filled out
                    if (string.IsNullOrWhiteSpace(currentObject.IDNumber) == false && 
                            string.IsNullOrWhiteSpace(currentObject.FirstName) == false && 
                            string.IsNullOrWhiteSpace(currentObject.LastName) == false && 
                            currentObject.ProfessionalStartDate.HasValue)
                    {
                        if (existingEmployees.Where(e => e.ProgramSpecificID.Trim().ToUpper() == currentObject.IDNumber.Trim().ToUpper()).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is already a professional in the system with the same ID Number.");
                        }
                        else if (existingEmployees.Where(e => e.Employee.FirstName.Trim().ToUpper() == currentObject.FirstName.Trim().ToUpper() &&
                                                             e.Employee.LastName.Trim().ToUpper() == currentObject.LastName.Trim().ToUpper() &&
                                                             e.HireDate == currentObject.ProfessionalStartDate.Value).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is already a professional in the system with the same first name, last name, and professional start date.");
                        }
                        else if (objectsToCheck.Where(o => o != currentObject &&
                                                                     string.IsNullOrWhiteSpace(o.IDNumber) == false &&
                                                                     o.IDNumber.Trim().ToUpper() == currentObject.IDNumber.Trim().ToUpper()).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is another row in this file with the same ID Number.");
                        }
                        else if (objectsToCheck.Where(o => o != currentObject &&
                                                                     string.IsNullOrWhiteSpace(o.FirstName) == false &&
                                                                     string.IsNullOrWhiteSpace(o.LastName) == false &&
                                                                     o.ProfessionalStartDate.HasValue &&
                                                                     o.FirstName.Trim().ToUpper() == currentObject.FirstName.Trim().ToUpper() &&
                                                                     o.LastName.Trim().ToUpper() == currentObject.LastName.Trim().ToUpper() &&
                                                                     o.ProfessionalStartDate.Value == currentObject.ProfessionalStartDate.Value).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is another row in this file with the same first name, last name, and professional start date.");
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
                List<ProgramEmployeeUpload> uploadedEmployeeRecords = currentReader.GetRecords<ProgramEmployeeUpload>().ToList();

                //Get the existing objects from the DB
                List<ProgramEmployee> existingEmployees = currentDBContext.ProgramEmployee.Include(pe => pe.Employee).AsNoTracking().Where(pe => pe.ProgramFK == currentProgramFK.Value).ToList();

                //Get the code table information
                List<CodeGender> genders = currentDBContext.CodeGender.AsNoTracking().OrderBy(cg => cg.Description).ToList();
                List<CodeEthnicity> ethnicities = currentDBContext.CodeEthnicity.AsNoTracking().OrderBy(ce => ce.Description).ToList();
                List<CodeRace> races = currentDBContext.CodeRace.AsNoTracking().OrderBy(cr => cr.Description).ToList();
                List<CodeTermReason> terminationCodes = currentDBContext.CodeTermReason.AsNoTracking().OrderBy(ctr => ctr.Description).ToList();
                List<Classroom> existingClassrooms = currentDBContext.Classroom.AsNoTracking().Where(c => c.ProgramFK == currentProgramFK.Value).OrderBy(c => c.ProgramSpecificID).ToList();
                List<CodeEmployeeLeaveReason> classroomLeaveReasons = currentDBContext.CodeEmployeeLeaveReason.AsNoTracking().OrderBy(celr => celr.Description).ToList();
                List<CodeJobType> jobFunctions = currentDBContext.CodeJobType.AsNoTracking().OrderBy(cjt => cjt.Description).ToList();

                //Set the validation fields in the objects
                SetValidationAndFKFields(ref uploadedEmployeeRecords, genders, ethnicities, races, terminationCodes, classroomLeaveReasons, jobFunctions, existingClassrooms, existingEmployees, currentProgramFK.Value);

                //Return the objects
                return uploadedEmployeeRecords;
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
                List<ProgramEmployeeUpload> importRows = objectsToSave.Cast<ProgramEmployeeUpload>().ToList();

                //Create the correct EF objects out of the valid import rows
                IEnumerable<ProgramEmployee> objectsToAdd = importRows.Where(o => o.IsValid == true).Select(pe => new ProgramEmployee()
                {
                    Employee = new Employee()
                    {
                        Creator = creator,
                        CreateDate = DateTime.Now,
                        AspireEmail = null,
                        AspireID = null,
                        AspireVerified = false,
                        EmailAddress = pe.EmailAddress,
                        EthnicityCodeFK = pe.EthnicityCodeFK.Value,
                        EthnicitySpecify = (!string.IsNullOrWhiteSpace(pe.Ethnicity) && pe.Ethnicity.Trim().ToUpper() == "OTHER" ? pe.SpecifyEthnicity : null),
                        FirstName = pe.FirstName,
                        GenderCodeFK = pe.GenderCodeFK.Value,
                        GenderSpecify = (!string.IsNullOrWhiteSpace(pe.Gender) && pe.Gender.Trim().ToUpper() == "OTHER" ? pe.SpecifyGender : null),
                        LastName = pe.LastName,
                        RaceCodeFK = pe.RaceCodeFK.Value,
                        RaceSpecify = (!string.IsNullOrWhiteSpace(pe.Race) && pe.Race.Trim().ToUpper() == "OTHER" ? pe.SpecifyRace : null)
                    },
                    Creator = creator,
                    CreateDate = DateTime.Now,
                    IsEmployeeOfProgram = pe.IsEmployeeOfProgram.Value,
                    HireDate = pe.ProfessionalStartDate.Value,
                    ProgramSpecificID = pe.IDNumber,
                    TermDate = pe.ProfessionalSeparationDate,
                    TermReasonCodeFK = pe.TerminationReasonCodeFK,
                    TermReasonSpecify = (!string.IsNullOrWhiteSpace(pe.ProfessionalSeparationReason) && pe.ProfessionalSeparationReason.ToUpper() == "OTHER" ? pe.SpecifyProfessionalSeparationReason : null),
                    ProgramFK = pe.ProgramFK,
                    JobFunction = (pe.JobTypeCodeFK.HasValue == false ? null : new List<JobFunction>() { 
                        new JobFunction()
                        {
                            Creator = creator,
                            CreateDate = DateTime.Now,
                            StartDate = pe.JobFunctionStartDate.Value,
                            EndDate = pe.JobFunctionEndDate,
                            JobTypeCodeFK = pe.JobTypeCodeFK.Value
                        }
                    }),
                    EmployeeClassroom = (pe.ClassroomFK.HasValue == false ? null : new List<EmployeeClassroom>()
                    {
                        new EmployeeClassroom()
                        {
                            Creator = creator,
                            CreateDate = DateTime.Now,
                            AssignDate = pe.ClassroomAssignDate.Value,
                            ClassroomFK = pe.ClassroomFK.Value,
                            JobTypeCodeFK = pe.JobTypeCodeFK.Value,
                            LeaveDate = pe.ClassroomLeaveDate,
                            LeaveReasonCodeFK = pe.ClassroomLeaveReasonCodeFK,
                            LeaveReasonSpecify = (!string.IsNullOrWhiteSpace(pe.ClassroomLeaveReason) && pe.ClassroomLeaveReason.Trim().ToUpper() == "OTHER" ? pe.SpecifyClassroomLeaveReason : null)
                        }
                    })
                });

                //Add the objects to the database and save the changes
                currentDBContext.ProgramEmployee.AddRange(objectsToAdd);
                currentDBContext.SaveChanges();
            }
        }
    }
}