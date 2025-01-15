using System.Collections.Generic;

namespace MedExpert.Web.ViewModels.Account
{
    public class RegisterResultModel
    {
        public bool Success { get; set; }
        
        public List<string> Errors { get; set; }
        public string RedirectUrl { get; set; }
    }
}