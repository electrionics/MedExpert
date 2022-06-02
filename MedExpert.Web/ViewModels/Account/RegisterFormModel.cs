using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace MedExpert.Web.ViewModels.Account
{
    public class RegisterFormModel
    {
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
        
        public string ReturnUrl { get; set; }
        
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}