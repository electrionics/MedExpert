using System;
using System.Collections.Generic;

namespace MedExpert.Excel.Converters.Common.Base
{
    public interface IConverter
    {
        List<Type> GetTypes();
        
        bool ValidateType(Type propertyType);
        
        public abstract object ConvertStringValue(Type propertyType, string value, string additionalValue = null);

        public abstract string ValidateString(Type propertyType, string value);
    }
}