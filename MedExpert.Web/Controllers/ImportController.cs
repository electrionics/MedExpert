using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MedExpert.Web.ViewModels;
using MedExpert.Services.Interfaces;

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class ImportController:ControllerBase
    {
        private readonly IReferenceIntervalService _referenceIntervalService;


        public ImportController(IReferenceIntervalService referenceIntervalService)
        {
            _referenceIntervalService = referenceIntervalService;
        }

        #region ReferenceInterval
        
        [HttpGet]
        [ApiRoute("Import/ReferenceInterval")]
        public async Task<ImportResult> ImportReferenceInterval()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}