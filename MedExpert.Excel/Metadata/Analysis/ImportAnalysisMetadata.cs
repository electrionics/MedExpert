using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model;
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
            
            EntityValidator(new ImportAnalysisModelValidator());
        }
    }
}