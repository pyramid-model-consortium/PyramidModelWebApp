using CsvHelper;
using Pyramid.Models;
using System.Collections.Generic;

namespace Pyramid.FileImport.CodeFiles
{
    public interface IImportable
    {
        //Required properties
        bool IsValid { get; set; }
        bool IsDuplicate { get; set; }
        string ReasonsInvalid { get; set; }
        string DisplayName { get; }

        //Required methods
        List<string> GetFieldsToHideFromPreview();
        string GetImportInstructionsHTML();
        string GetConfirmationInstructionsHTML();
        List<ImportFileFieldDetail> GetFileFieldInformation();
        string GetImportTemplateFilePath();
        string GetImportExampleFilePath();
        void RegisterClassMapWithReader(CsvReader currentReader);
        IEnumerable<IImportable> GetResultsFromReader(CsvReader currentReader, PyramidContext currentDBContext, int? currentProgramFK);
        void SaveRangeToDatabase(PyramidContext currentDBContext, IEnumerable<IImportable> objectsToSave, string creator);
    }
}
