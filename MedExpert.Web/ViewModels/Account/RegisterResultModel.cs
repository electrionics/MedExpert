using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace MedExpert.Web.ViewModels.Account
{
    public class RegisterResultModel
    {
        public bool Success { get; set; }
        
        public List<string> Errors { get; set; }
        public string RedirectUrl { get; set; }
    }
}