using System.Threading.Tasks;

using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface ILookupService
    {
        Task<Lookup> GetByName(string name);
    }
}