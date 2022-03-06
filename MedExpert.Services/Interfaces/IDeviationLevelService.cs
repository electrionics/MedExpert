using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IDeviationLevelService
    {
        public Task<List<DeviationLevel>> GetAll();
    }
}