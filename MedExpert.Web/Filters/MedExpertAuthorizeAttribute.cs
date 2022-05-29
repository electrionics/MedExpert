using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedExpert.Web.Filters
{
    public class MedExpertAuthorizeAttribute:Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity is not {IsAuthenticated: true})
            {
                context.Result = new ForbidResult();
            }
        }
    }
}