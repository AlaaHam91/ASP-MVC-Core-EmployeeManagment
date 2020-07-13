using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//the admin user should not be able to update his roles and claims

namespace EmployeeManagment.Security
{
    public class ManageAdminRolesAndClaimsRequirments:IAuthorizationRequirement
    {
    }
}
