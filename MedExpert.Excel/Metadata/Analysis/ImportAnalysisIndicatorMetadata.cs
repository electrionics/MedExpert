using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model;
using MedExpert.Excel.Model.Analysis;
using MedExpert.Excel.Validators.Analysis;
using MedExpert.Excel.Validators.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata.Analysis
{
    public class ImportAnalysisIndicatorMetadata:BaseMetadata<ImportAnalysisIndicatorModel>
    {
        public ImportAnalysisIndicatorMetadata()
        {
            HeaderValidator(new DynamicHeaderEmptyValidator());
            
            CellValue(x => x.Indicator, "Показатель");
            CellValue(x => x.Value, "Значение");
            CellValue(x => x.ReferenceInterval, "Референсный интервал");
            
            CellsDictionary(x => x.OtherColumns);
            
            EntityValidator(new ImportAnalysisIndicatorModelValidator());
        }
    }
}