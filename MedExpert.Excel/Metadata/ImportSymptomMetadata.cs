using MedExpert.Excel.Model;
using MedExpert.Services.Interfaces;

namespace MedExpert.Excel.Metadata
{
    public class ImportSymptomMetadata : BaseMetadata<ImportSymptomModel>
    {
        public ImportSymptomMetadata(int symptomLevelCount, IIndicatorService indicatorService)
        {
            HeaderValidator(new DynamicHeaderOfIndicatorsValidator(indicatorService));
            
            for (var i = 1; i <= symptomLevelCount; i++)
            {
                CellValue(x => x.SymptomName, "Болезнь" + i, x => x.SymptomLevelStr);
                CellComment(x => x.SymptomDescription, "Болезнь" + i);
            }
            
            CellsDictionary(x => x.DeviationLevels);
            
            EntityValidator(new ImportSymptomModelValidator());
        }
    }
}