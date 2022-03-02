﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IImportService<TImportEntity>
    {
        Task UpdateBulk(List<TImportEntity> indicators);
        Task InsertBulk(List<TImportEntity> indicators);
    }
}