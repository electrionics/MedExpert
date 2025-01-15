using System.Collections.Generic;

using Microsoft.AspNetCore.Authentication;

namespace MedExpert.Web.ViewModels.Account
{
    public class LoginFormModel
    {
        public string Email { get; set; }
        
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        
        public string ReturnUrl { get; set; }
    }
}