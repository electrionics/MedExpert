using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using FluentValidation;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using MedExpert.Domain.Identity;
using MedExpert.Web.Services;
using MedExpert.Web.ViewModels.Account;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class AccountController:ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<LoginModel> _logger;
        private readonly IValidator<LoginFormModel> _loginFormModelValidator;
        private readonly IValidator<RegisterFormModel> _registerFormModelValidator;
        private readonly IJwtGenerator _jwtGenerator;
        
        public AccountController(SignInManager<User> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<User> userManager, IValidator<LoginFormModel> loginFormModelValidator, IValidator<RegisterFormModel> registerFormModelValidator, IEmailSender emailSender, IJwtGenerator jwtGenerator)
        {
            _userManager = userManager;
            _loginFormModelValidator = loginFormModelValidator;
            _registerFormModelValidator = registerFormModelValidator;
            _emailSender = emailSender;
            _jwtGenerator = jwtGenerator;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        [ApiRoute("Account/Login")]
        public async Task<LoginFormModel> Login([FromQuery]string returnUrl = null)
        {
            // if (!string.IsNullOrEmpty(ErrorMessage))
            // {
            //     ModelState.AddModelError(string.Empty, ErrorMessage);
            // }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var model = new LoginFormModel
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnUrl = returnUrl
            };

            return model;
        }

        [HttpPost]
        [ApiRoute("Account/Login")]
        public async Task<LoginResultModel> Login([FromBody]LoginFormModel model)
        {
            var result = new LoginResultModel
            {
                RedirectUrl = model.ReturnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            var validationResult = await _loginFormModelValidator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    result.ErrorMessage = "Пользователь с таким именем не существует.";
                }
                else
                {
                    var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    result.Success = signInResult.Succeeded;

                    if (signInResult.Succeeded)
                    {
                        result.DisplayName = user.Email[..user.Email.IndexOf('@')];
                        result.UserName = user.UserName;
                        result.Token = _jwtGenerator.CreateToken(user);
                        _logger.LogInformation("User logged in.");
                    }
                    else if (signInResult.RequiresTwoFactor)
                    {
                        result.RedirectUrl = $"account/loginWih2fa?ReturnUrl={model.ReturnUrl}&RememberMe={model.RememberMe}";
                    }
                    else if (signInResult.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        result.RedirectUrl = "account/lockout";
                    }
                    else
                    {
                        result.ErrorMessage = "Неудачная попытка входа в систему.";
                    }
                }
                

                return result;
            }

            // If we got this far, something failed, redisplay form
            result.ErrorMessage = "Форма заполнена некорректно.";
            return result;
        }
        
        [HttpPost]
        [ApiRoute("Account/Logout")]
        public async Task<string> Logout([FromQuery]string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return returnUrl ?? "";
        }

        [HttpGet]
        [ApiRoute("Account/Register")]
        public async Task<RegisterFormModel> Register([FromQuery] string returnUrl = null)
        {
            var result = new RegisterFormModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return result;
        }

        [HttpPost]
        [ApiRoute("Account/Register")]
        public async Task<RegisterResultModel> Register([FromBody] RegisterFormModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var resultModel = new RegisterResultModel
            {
                RedirectUrl = model.ReturnUrl,
                Success = false
            };
            
            var validationResult = await _registerFormModelValidator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                var user = new User {UserName = model.Email, Email = model.Email};
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Content($"~/account/confirmEmail?" +
                                                  $"userId={user.Id}&" +
                                                  $"code={code}&" +
                                                  $"returnUrl={model.ReturnUrl}");

                    // protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

                    resultModel.Success = true;
                    
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        resultModel.RedirectUrl = $"account/registerConfirmation?" +
                                                  $"email={model.Email}&" +
                                                  $"returnUrl={model.ReturnUrl}";
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                    }

                    return resultModel;
                }

                resultModel.Errors = result.Errors.Select(x => x.Description).ToList();
            }

            // If we got this far, something failed, redisplay form
            return resultModel;
        }
    }
}