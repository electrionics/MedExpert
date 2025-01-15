using System.Collections.Generic;
using System.Threading.Tasks;

using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;

namespace MedExpert.Services.Interfaces
{
    public interface IReferenceIntervalService:IImportService<ReferenceIntervalApplyCriteria>
    {
        Task<List<ReferenceIntervalValues>> GetReferenceIntervalsByCriteria(Sex sex, decimal age);
        Task DeleteAllReferenceIntervalValues();
        Task DeleteAllReferenceIntervalApplyCriteria();
    }
}