using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Model.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Common
{
    public class IntervalModelConverter:BaseConverter<IntervalModel>
    {
        private static readonly Regex IntervalRegex = new("(\\d+(,?\\d+)?)-(\\d+(,?\\d+)?)");
        
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
                
            var groups = IntervalRegex.Match(value).Groups;
            return new IntervalModel
            {
                ValueMin = decimal.Parse(groups[1].Value
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)),
                ValueMax = decimal.Parse(groups[3].Value
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
            };
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return !string.IsNullOrEmpty(value) && !IntervalRegex.IsMatch(value)
                ? "Некорректный формат интервала (пример: '0,1-0,3')"
                : null;
        }
    }
}