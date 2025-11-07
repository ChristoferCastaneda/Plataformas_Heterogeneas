using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Comprehension.Attributes
{
    public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            var authService = context.HttpContext.RequestServices
                .GetService<Comprehension.Services.IAuthService>();

            if (authService == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var user = await authService.GetUserByTokenAsync(token);

            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.HttpContext.Items["UserId"] = user.Id;
        }
    }
}