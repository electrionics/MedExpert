using System;
using System.Collections.Generic;

namespace MedExpert.Excel.Converters.Common.Base
{
    public abstract class BaseNullableConverter<TNotNullable, TNullable>:BaseConverter<TNotNullable>
    {
        public override List<Type> GetTypes()
        {
            var result = base.GetTypes();
            result.Add(typeof(TNullable));

            return result;
        }
    }
}