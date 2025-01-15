using System.Collections.Generic;

using Microsoft.AspNetCore.Authentication;

namespace MedExpert.Web.ViewModels.Account
{
    public class LoginResultModel
    {
        public bool Success { get; set; }
        
        public string UserName { get; set; }
        
        public string DisplayName { get; set; }
        
        public string Token { get; set; }
        
        public string RedirectUrl { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}