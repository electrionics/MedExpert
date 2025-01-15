using System;

using MedExpert.Excel.Converters.Common.Base;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Common.Default
{
    public class IntConverter:BaseNullableConverter<int, int?>
    {
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            if (int.TryParse(value, out var intValue))
            {
                return intValue;
            }

            return propertyType == typeof(int?) ? null : 0;
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return propertyType == typeof(int?) && string.IsNullOrEmpty(value)
                ? null
                : int.TryParse(value, out _)
                    ? null
                    : "Значение должно быть целым.";
        }
    }
}