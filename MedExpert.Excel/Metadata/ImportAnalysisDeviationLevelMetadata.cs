using MedExpert.Excel.Model;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata
{
    public class ImportAnalysisDeviationLevelMetadata:BaseMetadata<ImportAnalysisDeviationLevelModel>
    {
        public ImportAnalysisDeviationLevelMetadata()
        {
            HeaderValidator(new DynamicHeaderEmptyValidator());
            
            CellValue(x => x.Alias, "Уровень отклонения");
            CellValue(x => x.MinPercentFromCenter, "Вниз от центра%");
            CellValue(x => x.MaxPercentFromCenter, "Вверх от центра%");
            
            CellsDictionary(x => x.OtherColumns);
            
            EntityValidator(new ImportAnalysisDeviationLevelModelValidator());
        }
    }
}