using System;
using System.Globalization;
using MedExpert.Excel.Converters.Common.Base;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Common.Default
{
    public class DateTimeConverter:BaseNullableConverter<DateTime, DateTime?>
    {
        private readonly string _format;

        public DateTimeConverter(string format)
        {
            _format = format;
        }
        
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            if (DateTime.TryParseExact(value, _format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateTimeValue))
            {
                return dateTimeValue;
            }

            return propertyType == typeof(decimal?)
                ? null
                : DateTime.MinValue;
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return propertyType == typeof(DateTime?) && string.IsNullOrEmpty(value)
                ? null
                : DateTime.TryParseExact(value, _format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _)
                    ? null
                    : $"Значение должно быть датой в формате {_format}.";
        }
    }
}