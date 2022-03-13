using System.Collections.Generic;
using MedExpert.Excel.Converters.Common;
using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Converters.Symptom;
using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model.Symptoms;
using MedExpert.Excel.Validators.Common;
using MedExpert.Excel.Validators.Symptoms;
using MedExpert.Services.Interfaces;

namespace MedExpert.Excel.Metadata.Symptoms
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
            
            AddConverters(new List<IConverter>{ new SymptomNameModelConverter(), new DeviationLevelModelConverter()});
            
            EntityValidator(new ImportSymptomModelValidator());
        }
    }
}