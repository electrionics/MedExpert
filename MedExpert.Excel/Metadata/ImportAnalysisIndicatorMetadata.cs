using MedExpert.Excel.Model;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata
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