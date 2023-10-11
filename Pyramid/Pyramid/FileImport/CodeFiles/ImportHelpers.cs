using Pyramid.Models;

namespace Pyramid.FileImport.CodeFiles
{
    public class ImportHelpers
    {
        /// <summary>
        /// This method returns an object that implements the IImportable interface based
        /// on the passed class abbreviation string.
        /// </summary>
        /// <param name="classAbbreviation">The import class abbreviation.</param>
        /// <returns>An object that implements the IImportable interface if possible, null otherwise.</returns>
        public static IImportable GetImportClassFromString(string classAbbreviation)
        {
            IImportable objectToReturn;

            //Get the object to return
            if (!string.IsNullOrWhiteSpace(classAbbreviation))
            {
                switch (classAbbreviation.ToUpper())
                {
                    case "CHILD":
                        objectToReturn = new ChildProgram.ChildProgramUpload();
                        break;
                    case "PE":
                        objectToReturn = new ProgramEmployee.ProgramEmployeeUpload();
                        break;
                    case "BOQ":
                        objectToReturn = new BenchmarkOfQuality.BOQUpload();
                        break;
                    default:
                        objectToReturn = null;
                        break;
                }
            }
            else
            {
                objectToReturn = null;
            }

            return objectToReturn;
        }
    }
}