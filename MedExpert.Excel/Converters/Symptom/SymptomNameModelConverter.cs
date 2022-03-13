using System;
using System.Text.RegularExpressions;
using MedExpert.Domain.Enums;
using MedExpert.Excel.Converters.Common;
using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Model.Symptoms;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Converters.Symptom
{
    public class SymptomNameModelConverter:BaseConverter<SymptomNameModel>
    {
        private static readonly Regex SymptomNameRegex = new("^((.+)::)?(.*)$");
        private static readonly SexConverter SexConverter = new();
        
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
                
            var groups = SymptomNameRegex.Match(value).Groups;
            return new SymptomNameModel
            {
                Value = groups[3].Value,
                SexStr = groups[2].Value,
                Sex = (Sex?)SexConverter.ConvertStringValue(typeof(Sex?), groups[2].Value)
            };
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return !string.IsNullOrEmpty(value) && !SymptomNameRegex.IsMatch(value)
                ? "Некорректный формат названия симптома/болезни (пример: 'Ж::Рак шейки матки' или 'Системный васкулит')"
                : null;
        }
    }
}