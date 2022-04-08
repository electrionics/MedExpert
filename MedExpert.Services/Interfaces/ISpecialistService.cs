using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;

namespace MedExpert.Services.Interfaces
{
    public interface ISpecialistService
    {
        Task<List<Specialist>> GetSpecialistsByCriteria(Sex sex);
        Task<List<Specialist>> GetSpecialists();
        Task CreateSpecialist(Specialist specialist);
    }
}