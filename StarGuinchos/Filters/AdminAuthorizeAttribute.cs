using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StarGuinchos.Filters
{
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var adminLogado = context.HttpContext.Session.GetString("AdminLogado");

            if (adminLogado == "true")
            {
                return;
            }

            var request = context.HttpContext.Request;
            var returnUrl = request.Path + request.QueryString;

            context.Result = new RedirectToActionResult(
                "Login",
                "AdminAuth",
                new { returnUrl }
            );
        }
    }
}