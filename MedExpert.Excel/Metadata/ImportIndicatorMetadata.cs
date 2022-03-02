using MedExpert.Excel.Model;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata
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