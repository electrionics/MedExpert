using System;
using MedExpert.Excel.Converters.Common.Base;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Common.Default
{
    public class BoolConverter:BaseConverter<bool>
    {
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            return value == "1";
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return value is "0" or "1" or "" ? null : "Значение должно быть равно '1', '0' или быть пустым.";
        }
    }
}