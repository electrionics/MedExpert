using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IReferenceIntervalService:IImportService<ReferenceIntervalApplyCriteria>
    {
        Task DeleteAllReferenceIntervalValues();
        Task DeleteAllReferenceIntervalApplyCriteria();
    }
}