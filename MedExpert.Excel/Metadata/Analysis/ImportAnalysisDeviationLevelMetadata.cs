using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model.Analysis;
using MedExpert.Excel.Validators.Analysis;
using MedExpert.Excel.Validators.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata.Analysis
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