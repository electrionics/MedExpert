using System;
using System.Collections.Generic;

namespace MedExpert.Excel.Converters.Common.Base
{
    public abstract class BaseConverter<TNotNullable>:IConverter
    {
        public virtual List<Type> GetTypes()
        {
            return new(2) {typeof(TNotNullable)};
        }

        public bool ValidateType(Type propertyType)
        {
            return GetTypes().Contains(propertyType);
        }
        
        public abstract object ConvertStringValue(Type propertyType, string value, string additionalValue = null);

        public abstract string ValidateString(Type propertyType, string value);
    }
}