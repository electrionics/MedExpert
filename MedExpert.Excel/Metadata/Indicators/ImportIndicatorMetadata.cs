using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model.Indicators;
using MedExpert.Excel.Validators.Indicators;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata.Indicators
{
    public class ImportIndicatorMetadata : BaseMetadata<ImportIndicatorModel>
    {
        public ImportIndicatorMetadata()
        {
            CellValue(x => x.Name, "Название");
            CellValue(x => x.ShortName, "Сокращенно");
            CellValue(x => x.InAnalysis, "В анализе");
            
            EntityValidator(new ImportIndicatorModelValidator());
        }
    }
}