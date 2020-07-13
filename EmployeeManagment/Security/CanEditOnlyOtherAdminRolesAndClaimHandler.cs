using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagment.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimHandler :
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirments>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageAdminRolesAndClaimsRequirments requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if (authFilterContext == null)
                return Task.CompletedTask;
            string loggedInAdminId = 
                context.User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier).Value;
            string adminIdBeingEdited =
                authFilterContext.HttpContext.Request.Query["userId"];
            if (context.User.IsInRole("Admin")
                  && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")
                  && adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
                context.Succeed(requirement);
            //the rest handlers still called and the result is accessDenied
            //else
            //    context.Fail();
            return Task.CompletedTask;
        }
    }
}
