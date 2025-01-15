using System;

using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Model.Symptoms;

namespace MedExpert.Excel.Converters.Symptom
{
    public class DeviationLevelModelConverter:BaseConverter<DeviationLevelModel>
    {
        public override object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            return new DeviationLevelModel
            {
                Alias = value,
                Description = additionalValue
            };
        }

        public override string ValidateString(Type propertyType, string value)
        {
            return null;
        }
    }
}