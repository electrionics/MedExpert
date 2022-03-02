using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IIndicatorService:IImportService<Indicator>
    {
        Task<List<Indicator>> GetIndicators(List<string> shortNames);
    }
}