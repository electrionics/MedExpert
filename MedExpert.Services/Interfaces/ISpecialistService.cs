using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface ISpecialistService
    {
        Task<List<Specialist>> GetSpecialists();
    }
}