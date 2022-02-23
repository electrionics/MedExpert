using Microsoft.AspNetCore.Mvc;

namespace MedExpert.Web
{
    public class ApiRouteAttribute:RouteAttribute
    {
        public ApiRouteAttribute(string template) : base("Api/" + template)
        {
        }
    }
}