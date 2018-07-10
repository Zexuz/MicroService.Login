using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using MicroService.Login.Security;

namespace MicroService.Login.WebApi.Filters
{
    public class SameIpOriginRequirementFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.Filters.Any(filter => filter is AllowAnonymousFilter))
                return;
            try
            {
                var userIpClaim = context.HttpContext.User.FindFirst(claim => claim.Type == CustomClaims.Ip).Value;

                var userConnectionId = context.HttpContext.Connection.RemoteIpAddress;

                if (userIpClaim != userConnectionId.ToString())
                {
                    context.Result = new ForbidResult();
                }
            }
            catch (Exception)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}