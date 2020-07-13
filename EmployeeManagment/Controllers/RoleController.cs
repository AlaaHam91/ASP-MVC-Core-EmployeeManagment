using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagment.Models;
using EmployeeManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeManagment.Controllers
{
    [Authorize(Roles="Admin,Adm")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<RoleController> logger;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RoleController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole { Name = model.RoleName };
                IdentityResult result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("ListRoles", "Role");
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.TryAddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
       // [Authorize(Policy ="EditRolePolicy")]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role with id= {id} can not found";
                return View("NotFound");
            }
            EditRoleViewModel model = new EditRoleViewModel { Id = role.Id, RoleName = role.Name };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                    model.Users.Add(user.UserName);

            }
            return View(model);
        }

        [HttpPost]
       // [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role with id= {model.Id} can not found";
                return View("NotFound");
            }
            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return RedirectToAction("ListRoles");
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role with id= {roleId} can not found";
                return View("NotFound");
            }
            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users)
            {
                var userModel = new UserRoleViewModel {UserName=user.UserName,UserId=user.Id };
                if (await userManager.IsInRoleAsync(user, role.Name))
                    userModel.IsSelected = true;
                else
                    userModel.IsSelected = false;
                model.Add(userModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(IList<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role with id= {roleId} can not found";
                return View("NotFound");
            }
            for (int i = 0; i < model.Count; i++)
            {
               var user =await userManager.FindByIdAsync(model[i].UserId);
                IdentityResult result = null;
                if (!(await userManager.IsInRoleAsync(user, role.Name)) && model[i].IsSelected)
                    result= await userManager.AddToRoleAsync(user,role.Name);
                if (await userManager.IsInRoleAsync(user, role.Name) && !model[i].IsSelected)
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                else
                    continue;
                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole",new {id= roleId });
                }
            }
           return  RedirectToAction("EditRole", new { id = roleId });
        }
        [HttpGet]
        public IActionResult ListUsers()
        {

            return View(userManager.Users);
        }

        [HttpGet]
        public async Task <IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {id} can not found";
                return View("NotFound");
            }
            var claims =await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);

            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                UserName=user.UserName,
                City=user.City,
                Roles=roles,
                Claims=claims.Select(c=>c.Type +" : "+ c.Value).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {model.Id} can not found";
                return View("NotFound");
            }
            else
            {
                user.Id = model.Id;
                user.UserName = model.UserName;
                user.City = model.City;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("ListUsers");
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {id} can not found";
                return View("NotFound");
            }
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
                return RedirectToAction("ListUsers");
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("ListUsers");
        }
        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role with id= {id} can not found";
                return View("NotFound");
            }
            try {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("ListRoles");
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListRoles");
            }
            catch (DbUpdateException e)
            {
                logger.LogError($"Error deleting role {e}");
                @ViewBag.ErrorTitle =$"{role.Name} role in use";
                @ViewBag.ErrorMessage=$"{role.Name} role cannot be deleted as there are users"+
                    "in this role,please try to delete users first.";
                return View("Error");
            }
            }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user =await userManager.FindByIdAsync(userId);
            ViewBag.userId = user.Id;
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {userId} can not found";
                return View("NotFound");
            }
            var model =new List<UserRolesViewModel>();
            UserRolesViewModel userRolesViewModel;
            foreach (var role in roleManager.Roles)
            {
                userRolesViewModel = new UserRolesViewModel {RoleId=role.Id,RoleName=role.Name };
                if (await userManager.IsInRoleAsync(user, role.Name))
                    userRolesViewModel.IsSelected = true;
                else
                    userRolesViewModel.IsSelected = false;
                model.Add(userRolesViewModel);

            }
            return View(model);
        }
        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {userId} can not found";
                return View("NotFound");
            }
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("","cannot rempve user existing roles.");
                return View(model);
            }
            result=await userManager.AddToRolesAsync(user, 
               model.Where(x => x.IsSelected).Select(r => r.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot add user existing roles.");
                return View(model);
            }
            return RedirectToAction("EditUser",new {id=user.Id });
        }
        [HttpGet]
        public async Task<IActionResult> ManageUserClaim(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {userId} can not found";
                return View("NotFound");
            }
            var ExistingClaims =await userManager.GetClaimsAsync(user);
            var model = new UserClaimsViewModel { UserId = user.Id};
            UserClaim userClaim ;
            foreach (var claim in ClaimStore.AllClaims)
            {
                userClaim= new UserClaim { ClaimType = claim.Type };

                if (ExistingClaims.Any(c => c.Type == claim.Type && c.Value=="true"))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ManageUserClaim(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id= {model.UserId} can not found";
                return View("NotFound");
            }
            var ExistingClaims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, ExistingClaims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot remove user existing claims.");
                return View(model);
            }
            result = await userManager.AddClaimsAsync(user,
                      //model.Claims.Where(c=>c.IsSelected==true)
                      //.Select(c=> new Claim ( c.ClaimType, c.ClaimType)));
                      model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" :"false")));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot add selected claims to user.");
                return View(model);
            }
            return RedirectToAction("EditUser", new { id = user.Id });
        }

    }

}