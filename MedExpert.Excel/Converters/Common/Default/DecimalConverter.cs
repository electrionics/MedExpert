using System;
using System.Globalization;

using MedExpert.Excel.Converters.Common.Base;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Common.Default
{
    public class DecimalConverter:BaseNullableConverter<decimal, decimal?>
    {
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            if (decimal.TryParse(value, out var decimalValue))
            {
                return decimalValue;
            }

            return propertyType == typeof(decimal?) ? null : 0;
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return propertyType == typeof(decimal?) && string.IsNullOrEmpty(value)
                ? null
                : decimal.TryParse(value?.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out _)
                    ? null
                    : "Значение должно быть числом с плавающей точкой.";
        }
    }
}