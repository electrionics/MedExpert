using System.Collections.Generic;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class IndicatorValueDependencyModel:IndicatorValueModel
    {
        public List<int> DependencyIndicatorIds { get; set; }
    }
}