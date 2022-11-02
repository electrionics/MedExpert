using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedExpert.Services.Interfaces
{
    public interface ISystemHealthCheckService
    {
        Task<Dictionary<string, Tuple<int, int>>> GetHealthCheckData();
    }
}
