using System;

using MedExpert.Domain.Enums;
using MedExpert.Excel.Converters.Common.Base;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Common
{
    public class SexConverter:BaseNullableConverter<Sex, Sex?>
    {
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            var sexValue = value switch
            {
                "М" => Sex.Male,
                "Ж" => Sex.Female,
                _ => (Sex?)null
            };

            if (propertyType == typeof(Sex) && sexValue == null)
            {
                sexValue = Sex.Male;
            }

            return sexValue;
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return value is "М" or "Ж" || propertyType == typeof(Sex?) && string.IsNullOrEmpty(value)
                ? null
                : "Значение должно быть равно 'М' или 'Ж'.";
        }
    }
}