using MedExpert.Excel.Model;
using MedExpert.Services.Interfaces;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata
{
    public class ImportReferenceIntervalMetadata : BaseMetadata<ImportReferenceIntervalModel>
    {
        public ImportReferenceIntervalMetadata(IIndicatorService indicatorService)
        {
            HeaderValidator(new DynamicHeaderOfIndicatorsValidator(indicatorService));
            
            CellValue(x => x.Sex, "Пол");
            CellValue(x => x.AgeInterval, "Возраст");

            CellsDictionary(x => x.Values);
            
            EntityValidator(new ImportReferenceIntervalModelValidator());
        }
    }
}