using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DevExpress.Web;
using Pyramid.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Pyramid.FileImport.CodeFiles
{
    public class CsvHelperExtensions
    {
        public class CustomBoolConverter<T> : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (bool.TryParse(text, out bool conversionResult))
                {
                    return conversionResult;
                }
                else if (!string.IsNullOrWhiteSpace(text))
                {
                    bool? valueToReturn = null;

                    switch(text.Trim().ToLower())
                    {
                        case "yes":
                            valueToReturn = true;
                            break;
                        case "no":
                            valueToReturn = false;
                            break;
                        default:
                            valueToReturn = null;
                            break;
                    }

                    return valueToReturn;
                }
                else
                {
                    return null;
                }
            }

            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return base.ConvertToString(value, row, memberMapData);
            }
        }

        public class CustomIntConverter<T> : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (int.TryParse(text, out int conversionResult))
                {
                    return conversionResult;
                }
                else
                {
                    return null;
                }
            }

            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return base.ConvertToString(value, row, memberMapData);
            }
        }

        public class CustomDateTimeConverter<T> : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (DateTime.TryParse(text, out DateTime conversionResult) && conversionResult >= System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                {
                    return conversionResult;
                }
                else
                {
                    return null;
                }
            }

            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return base.ConvertToString(value, row, memberMapData);
            }
        }
    }
}