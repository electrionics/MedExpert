using System.Collections.Generic;

using MedExpert.Excel.Converters.Common;
using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Model.ReferenceIntervals;
using MedExpert.Excel.Validators.Common;
using MedExpert.Excel.Validators.ReferenceIntervals;
using MedExpert.Services.Interfaces;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata.ReferenceIntervals
{
    public class ImportReferenceIntervalMetadata : BaseMetadata<ImportReferenceIntervalModel>
    {
        public ImportReferenceIntervalMetadata(IIndicatorService indicatorService)
        {
            HeaderValidator(new DynamicHeaderOfIndicatorsValidator(indicatorService));
            
            CellValue(x => x.Sex, "Пол");
            CellValue(x => x.AgeInterval, "Возраст");
            
            CellsDictionary(x => x.Values);
            
            AddConverters(new List<IConverter>{ new SexConverter(), new IntervalModelConverter()});
            
            EntityValidator(new ImportReferenceIntervalModelValidator());
        }
    }
}