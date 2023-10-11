using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pyramid.FileImport.CodeFiles
{
    public class ImportFileFieldDetail
    {
        public bool IsRequired { get; set; }
        public string FieldDescription { get; set; }
        public string FieldName { get; set; }
        public string FieldAcceptableInput { get; set; }
        public string FieldType { get; set; }

        public ImportFileFieldDetail(string currentName, string currentType, bool currentRequired, string currentDescription, string currentAcceptableInput)
        {
            FieldName = currentName;
            FieldType = currentType;
            IsRequired = currentRequired;
            FieldDescription = currentDescription;
            FieldAcceptableInput = currentAcceptableInput;
        }
    }
}