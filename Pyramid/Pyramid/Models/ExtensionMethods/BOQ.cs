using System;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using Pyramid.FileImport.CodeFiles;
using CsvHelper.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Pyramid.Models
{
    public partial class BenchmarkOfQuality
    {
        public class BOQUpload : IImportable
        {
            public bool IsValid { get; set; }
            public bool IsDuplicate { get; set; }
            public string ReasonsInvalid { get; set; }
            public string DisplayName => "BOQ";
            public DateTime? FormDate { get; set; }
            public string TeamMembers { get; set; }
            public int? Indicator1 { get; set; }
            public int? Indicator2 { get; set; }
            public int? Indicator3 { get; set; }
            public int? Indicator4 { get; set; }
            public int? Indicator5 { get; set; }
            public int? Indicator6 { get; set; }
            public int? Indicator7 { get; set; }
            public int? Indicator8 { get; set; }
            public int? Indicator9 { get; set; }
            public int? Indicator10 { get; set; }
            public int? Indicator11 { get; set; }
            public int? Indicator12 { get; set; }
            public int? Indicator13 { get; set; }
            public int? Indicator14 { get; set; }
            public int? Indicator15 { get; set; }
            public int? Indicator16 { get; set; }
            public int? Indicator17 { get; set; }
            public int? Indicator18 { get; set; }
            public int? Indicator19 { get; set; }
            public int? Indicator20 { get; set; }
            public int? Indicator21 { get; set; }
            public int? Indicator22 { get; set; }
            public int? Indicator23 { get; set; }
            public int? Indicator24 { get; set; }
            public int? Indicator25 { get; set; }
            public int? Indicator26 { get; set; }
            public int? Indicator27 { get; set; }
            public int? Indicator28 { get; set; }
            public int? Indicator29 { get; set; }
            public int? Indicator30 { get; set; }
            public int? Indicator31 { get; set; }
            public int? Indicator32 { get; set; }
            public int? Indicator33 { get; set; }
            public int? Indicator34 { get; set; }
            public int? Indicator35 { get; set; }
            public int? Indicator36 { get; set; }
            public int? Indicator37 { get; set; }
            public int? Indicator38 { get; set; }
            public int? Indicator39 { get; set; }
            public int? Indicator40 { get; set; }
            public int? Indicator41 { get; set; }
            public int ProgramFK { get; set; }

            public class BOQUploadCsvClassMap : ClassMap<BOQUpload>
            {
                public BOQUploadCsvClassMap()
                {
                    Map(m => m.FormDate).Index(0).TypeConverter<CsvHelperExtensions.CustomDateTimeConverter<DateTime?>>();
                    Map(m => m.TeamMembers).Index(1);
                    Map(m => m.Indicator1).Index(2).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator2).Index(3).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator3).Index(4).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator4).Index(5).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator5).Index(6).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator6).Index(7).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator7).Index(8).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator8).Index(9).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator9).Index(10).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator10).Index(11).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator11).Index(12).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator12).Index(13).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator13).Index(14).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator14).Index(15).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator15).Index(16).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator16).Index(17).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator17).Index(18).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator18).Index(19).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator19).Index(20).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator20).Index(21).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator21).Index(22).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator22).Index(23).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator23).Index(24).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator24).Index(25).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator25).Index(26).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator26).Index(27).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator27).Index(28).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator28).Index(29).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator29).Index(30).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator30).Index(31).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator31).Index(32).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator32).Index(33).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator33).Index(34).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator34).Index(35).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator35).Index(36).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator36).Index(37).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator37).Index(38).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator38).Index(39).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator39).Index(40).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator40).Index(41).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
                    Map(m => m.Indicator41).Index(42).TypeConverter<CsvHelperExtensions.CustomIntConverter<int?>>();
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
                    "DisplayName"
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
                importInstructions.Append("This page allows you to upload a CSV file that contains information for BOQs in your program.  Please follow these instructions to import BOQ scores:");
                importInstructions.Append("<ol>");
                importInstructions.Append("<li>Download the template CSV file in this section.</li>");
                importInstructions.Append("<li>(Optional) Download the example CSV file in this section.  This file is the same format as the template file, with a few rows of example information entered.</li>");
                importInstructions.Append("<li>It is recommended that you open the template and example CSV files using <strong>Microsoft Excel</strong> for ease of data-entry.  However, you can use any program capable of handling CSV files.</li>");
                importInstructions.Append("<li>In the downloaded template CSV file, add all the BOQ information that you would like to import.</li>");
                importInstructions.Append("<li>Make sure the information that you added to the file matches the requirements for the fields that are listed in the <strong>CSV File Information</strong> table that is in the <strong>CSV File Information</strong> section below.</li>");
                importInstructions.Append("<li>Save the template CSV file after you have added all the necessary information.</li>");
                importInstructions.Append("<li>Scroll down to the <strong>Import CSV File</strong> section at the bottom of the page.</li>");
                importInstructions.Append("<li>In the <strong>Import CSV File</strong> section, ensure the program selected in the <strong>Program</strong> drop-down list is the one that you wish to add BOQs to.</li>");
                importInstructions.Append("<li>Click the browse button in the <strong>Select File to Import</strong> field and select the template file that you filled out and saved.</li>");
                importInstructions.Append("<li>Click the <strong>Import and Preview Results</strong> button.  This will take you to a new page to preview the import results before the final confirmation to add the BOQs to the system.</li>");
                importInstructions.Append("<li><strong>BOQs will not be saved to PIDS until you confirm the import on the next page.</strong></li>");
                importInstructions.Append("</ol>");

                //Return the instructions
                return importInstructions.ToString();
            }

            /// <summary>
            /// This method returns the instructions for confirming the BOQ CSV file import.
            /// </summary>
            /// <returns>A string that contains the instructions for confirming the CSV file import.  String is in HTML format.</returns>
            public string GetConfirmationInstructionsHTML()
            {
                //To hold the instructions
                StringBuilder importInstructions = new StringBuilder();

                //Add the instructions
                importInstructions.Append("This page allows you to review the information that you uploaded before saving it to PIDS.  Please follow these instructions to save the BOQ:");
                importInstructions.Append("<ol>");
                importInstructions.Append("<li>All the BOQ information in the CSV file you uploaded will appear in the <strong>Import Result Preview</strong> table below.</li>");
                importInstructions.Append("<li>Review all the scores listed in the table to make sure they are correct.</li>");
                importInstructions.Append("<li>Take note of the <strong>Is Valid</strong> field.  This field indicates whether or not the information is valid and will be saved.  If there is a green checkmark in this field, the information is valid and will be saved.  If there is red X in this field, the information is invalid and will <strong>not</strong> be saved.</li>");
                importInstructions.Append("<li>If the <strong>Is Valid</strong> field has a red X, check the <strong>Reasons Invalid</strong> field to see an explanation of why the field is not valid.  <strong>Note:</strong> Even if some of the scores are invalid and will not be saved, they will not interfere with saving the valid scores.  Valid scores will always be saved, regardless of how many invalid scores there are.</li>");
                importInstructions.Append("<li>Click the <strong>Save Valid Records</strong> button once you have verified all the information is correct.</li>");
                importInstructions.Append("<li>In the pop-up that appears, read the warning message to make the final confirmation and save the scores.</li>");
                importInstructions.Append("</ol>");

                //Return the instructions
                return importInstructions.ToString();
            }

            /// <summary>
            /// This method returns information about the fields in the BOQ CSV file.
            /// </summary>
            /// <returns>A list of ImportFileFieldDetail objects that contain descriptions of the fields in the CSV file.</returns>
            public List<ImportFileFieldDetail> GetFileFieldInformation()
            {

                //Get the list if objects
                List<ImportFileFieldDetail> listToReturn = new List<ImportFileFieldDetail>()
                {
                    new ImportFileFieldDetail("Form Date", "Date", true,"This is the BOQ's form date.","Any valid date that is not in the future."),
                    new ImportFileFieldDetail("Team Members", "Text", true, "This is the BOQ's team members.", "Any text."),
                    new ImportFileFieldDetail("Indicators 1-41", "Text", true, "These are the BOQ's indicators.","Input 0 for Not In Place, 1 for Partially In Place and 2 for In Place.")
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
                return "/FileImport/ExcelTemplates/BOQUpload.csv";
            }

            /// <summary>
            /// This method returns the path to the CSV example file
            /// </summary>
            /// <returns>The path to the CSV example file</returns>
            public string GetImportExampleFilePath()
            {
                return "/FileImport/ExcelTemplates/BOQUpload-Example.csv";
            }

            /// <summary>
            /// This method registers this object's class map with the CsvReader object passed to the method.
            /// </summary>
            /// <param name="currentReader">The CsvReader object that represents a CSV file</param>
            public void RegisterClassMapWithReader(CsvReader currentReader)
            {
                currentReader.Context.RegisterClassMap<BOQUploadCsvClassMap>();
            }

            private static void SetValidationAndFKFields(ref List<BOQUpload> objectsToCheck, int currentProgramFK, List<BenchmarkOfQuality> existingBOQs )
            {
                //Loop through the objects and set the validation fields
                for (int currentIndex = 0; currentIndex < objectsToCheck.Count; currentIndex++)
                {
                    //To hold the reasons the row is invalid
                    List<string> reasonsInvalid = new List<string>();

                    //Get the current object
                    BOQUpload currentObject = objectsToCheck[currentIndex];

                    //Set the program FK
                    currentObject.ProgramFK = currentProgramFK;

                    //Check validity and add reasons to the list

                    //Form Date validation
                    if (currentObject.FormDate.HasValue == false)
                    {
                        reasonsInvalid.Add("Form Date is missing or in an invalid format.");
                    }
                    else if (currentObject.FormDate.Value > DateTime.Now)
                    {
                        reasonsInvalid.Add("Form Date is in the future.");
                    }

                    //Team members validation
                    if (string.IsNullOrWhiteSpace(currentObject.TeamMembers))
                    {
                        reasonsInvalid.Add("Team members are missing.");
                    }

                    //Indicators validation
                    PropertyInfo[] properties = currentObject.GetType().GetProperties();
                    int count = 0;
                    foreach (PropertyInfo pi in properties)
                    {
                        if (pi.Name.Contains("Indicator") == true)
                        {
                            count++;
                            string indicatorMissing = "Indicator" + count + " is missing or is in an invalid format.";
                            string indicatorOutOfRange = "Indicator" + count + " is not a valid value.  It must be 0, 1 or 2.";
                            int indicatorResponse;

                            //If left blank or not a number
                            if (pi.GetValue(currentObject, null) == null)
                            {
                                reasonsInvalid.Add(indicatorMissing);
                            }
                            else
                            {
                                //Check if it's a number
                                if (int.TryParse(pi.GetValue(currentObject, null).ToString(), out indicatorResponse))
                                {

                                    //then check if it's a number between 0-2
                                    if (indicatorResponse < 0 || indicatorResponse > 2)
                                    {
                                        reasonsInvalid.Add(indicatorOutOfRange);
                                    }
                                }
                            }
                        }
                    }

                    //Duplication validation
                    if (currentObject.FormDate.HasValue)
                    {
                        if (existingBOQs.Where(e => e.FormDate == currentObject.FormDate).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is already a BOQ in the system with the same form date.");
                        }
                        if (objectsToCheck.Where(o => o != currentObject && o.FormDate == currentObject.FormDate).Count() > 0)
                        {
                            currentObject.IsDuplicate = true;
                            reasonsInvalid.Add("Duplicate row. There is another row in the file with the same form date.");
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
                List<BOQUpload> uploadedBOQRecords = currentReader.GetRecords<BOQUpload>().ToList();

                //Get the existing objects from the DB
                List<BenchmarkOfQuality> existingBOQs = currentDBContext.BenchmarkOfQuality.AsNoTracking().Where(pe => pe.ProgramFK == currentProgramFK.Value).ToList();

                //Set the validation fields in the objects
                SetValidationAndFKFields(ref uploadedBOQRecords, currentProgramFK.Value, existingBOQs);

                //Return the objects
                return uploadedBOQRecords;
            }

            public void SaveRangeToDatabase(PyramidContext currentDBContext, IEnumerable<IImportable> objectsToSave, string creator)
            {
                //Cast the parameter to the correct type
                List<BOQUpload> importRows = objectsToSave.Cast<BOQUpload>().ToList();

                //Create the correct EF objects out of the valid import rows
                IEnumerable<BenchmarkOfQuality> objectsToAdd = importRows.Where(o => o.IsValid == true).Select(b => new BenchmarkOfQuality()
                {
                    FormDate = b.FormDate.Value,
                    Creator = creator,
                    CreateDate = DateTime.Now,
                    Indicator1 = b.Indicator1.Value,
                    Indicator2 = b.Indicator2.Value,
                    Indicator3 = b.Indicator3.Value,
                    Indicator4 = b.Indicator4.Value,
                    Indicator5 = b.Indicator5.Value,
                    Indicator6 = b.Indicator6.Value,
                    Indicator7 = b.Indicator7.Value,
                    Indicator8 = b.Indicator8.Value,
                    Indicator9 = b.Indicator9.Value,
                    Indicator10 = b.Indicator10.Value,
                    Indicator11 = b.Indicator11.Value,
                    Indicator12 = b.Indicator12.Value,
                    Indicator13 = b.Indicator13.Value,
                    Indicator14 = b.Indicator14.Value,
                    Indicator15 = b.Indicator15.Value,
                    Indicator16 = b.Indicator16.Value,
                    Indicator17 = b.Indicator17.Value,
                    Indicator18 = b.Indicator18.Value,
                    Indicator19 = b.Indicator19.Value,
                    Indicator20 = b.Indicator20.Value,
                    Indicator21 = b.Indicator21.Value,
                    Indicator22 = b.Indicator22.Value,
                    Indicator23 = b.Indicator23.Value,
                    Indicator24 = b.Indicator24.Value,
                    Indicator25 = b.Indicator25.Value,
                    Indicator26 = b.Indicator26.Value,
                    Indicator27 = b.Indicator27.Value,
                    Indicator28 = b.Indicator28.Value,
                    Indicator29 = b.Indicator29.Value,
                    Indicator30 = b.Indicator30.Value,
                    Indicator31 = b.Indicator31.Value,
                    Indicator32 = b.Indicator32.Value,
                    Indicator33 = b.Indicator33.Value,
                    Indicator34 = b.Indicator34.Value,
                    Indicator35 = b.Indicator35.Value,
                    Indicator36 = b.Indicator36.Value,
                    Indicator37 = b.Indicator37.Value,
                    Indicator38 = b.Indicator38.Value,
                    Indicator39 = b.Indicator39.Value,
                    Indicator40 = b.Indicator40.Value,
                    Indicator41 = b.Indicator41.Value,
                    TeamMembers = b.TeamMembers,
                    ProgramFK = b.ProgramFK
                });

                //Add the objects to the database and save the changes
                currentDBContext.BenchmarkOfQuality.AddRange(objectsToAdd);
                currentDBContext.SaveChanges();
            }
        }
    }
}