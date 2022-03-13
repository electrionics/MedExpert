using System.Collections.Generic;
using MedExpert.Excel.Converters;
using MedExpert.Excel.Converters.Common;
using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model.Analysis;
using MedExpert.Excel.Validators.Analysis;
using MedExpert.Excel.Validators.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata.Analysis
{
    public class ImportAnalysisMetadata:BaseMetadata<ImportAnalysisModel>
    {
        public ImportAnalysisMetadata()
        {
            HeaderValidator(new DynamicHeaderEmptyValidator());
            
            CellValue(x => x.Sex, "Пол");
            CellValue(x => x.Age, "Возраст");
            
            CellsDictionary(x => x.OtherColumns);
            
            AddConverters(new List<IConverter>{ new SexConverter()});
            
            EntityValidator(new ImportAnalysisModelValidator());
        }
    }
}