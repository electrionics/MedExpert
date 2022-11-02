using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MedExpert.Services.Implementation
{
    // TODO: separate in monitoring service. Do not delete! It uses for performance checks of key tables. Would be refactored and added other tables when necessary. Use on prod carefully!
    public class SystemHealthCheckService : ISystemHealthCheckService
    {
        private readonly MedExpertDataContext _dataContext;

        public SystemHealthCheckService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Dictionary<string, Tuple<int, int>>> GetHealthCheckData()
        {
            var result = new Dictionary<string, Tuple<int, int>>();

            var time = new Stopwatch();
            time.Start();
            result.Add("analysis", Tuple.Create(await _dataContext.Set<Analysis>().CountAsync(), 0));
            time.Stop();
            result["analysis"] = Tuple.Create(result["analysis"].Item1, (int)time.ElapsedMilliseconds);
            time.Reset();

            time.Start();
            result.Add("deviationLevel", Tuple.Create(await _dataContext.Set<DeviationLevel>().SumAsync(x => x.MinPercentFromCenter ?? 0 + x.MaxPercentFromCenter ?? 0), 0));
            time.Stop();
            result["deviationLevel"] = Tuple.Create(result["deviationLevel"].Item1, (int)time.ElapsedMilliseconds);
            time.Reset();

            time.Start();
            result.Add("indicator", Tuple.Create(await _dataContext.Set<Indicator>().CountAsync(), 0));
            time.Stop();
            result["indicator"] = Tuple.Create(result["indicator"].Item1, (int)time.ElapsedMilliseconds);
            time.Reset();

            time.Start();
            result.Add("lookup", Tuple.Create(await _dataContext.Set<Lookup>().CountAsync(), 0));
            time.Stop();
            result["lookup"] = Tuple.Create(result["lookup"].Item1, (int)time.ElapsedMilliseconds);
            time.Reset();

            var categories = await _dataContext.Set<SymptomCategory>().ToListAsync();
            foreach(var category in categories)
            {
                time.Start();
                result.Add($"symptom{category.Name}", Tuple.Create(await _dataContext.Set<Symptom>().CountAsync(x => x.CategoryId == category.Id && !x.IsDeleted), 0));
                time.Stop();
                result[$"symptom{category.Name}"] = Tuple.Create(result[$"symptom{category.Name}"].Item1, (int)time.ElapsedMilliseconds);
                time.Reset();
            }

            time.Start();
            result.Add($"symptom", Tuple.Create(await _dataContext.Set<Symptom>().CountAsync(), 0));
            time.Stop();
            result[$"symptom"] = Tuple.Create(result[$"symptom"].Item1, (int)time.ElapsedMilliseconds);
            time.Reset();

            return result;
        }
    }
}
