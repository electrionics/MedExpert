using MedExpert.Excel.Model;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata
{
    public class ImportAnalysisMetadata:BaseMetadata<ImportAnalysisModel>
    {
        public ImportAnalysisMetadata()
        {
            HeaderValidator(new DynamicHeaderEmptyValidator());
            
            CellValue(x => x.Sex, "Пол");
            CellValue(x => x.Age, "Возраст");
            
            CellsDictionary(x => x.OtherColumns);
            
            EntityValidator(new ImportAnalysisModelValidator());
        }
    }
}