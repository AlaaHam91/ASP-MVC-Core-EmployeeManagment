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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console.Internal;

namespace EmployeeManagment.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly SignInManager<ApplicationUser> SiginManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> siginManager,
                                 ILogger<AccountController> logger)
        {
            UserManager = userManager;
            SiginManager = siginManager;
            this.logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await SiginManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Login(LoginViewModel model, string ReturnUrl)
        {
            model.ExternalLogins = (await SiginManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed &&
                    (await UserManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed");
                    return View(model);
                }

                var result = await SiginManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);//true for lokout,lockout column will increment by 1
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                        return Redirect(ReturnUrl);
                    // return LocalRedirect(ReturnUrl);//prevent any external url
                    else
                        return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                    return View("AccountLocked");
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login..");

                }
            }
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string returnUrl, string provider)
        {
            ///redirect url after success login
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Account", new { ReturnUrl = returnUrl });
            var properites = SiginManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            //go to sigin google
            return new ChallengeResult(provider, properites);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallBack(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await SiginManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error for external provider {remoteError}");
                return View("Login", model);
            }
            var info = await SiginManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error load external login information.");
                return View("Login", model);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;
            if (email != null)
            {
                user = await UserManager.FindByEmailAsync(email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View("Login", model);
                }
            }

            var sigInResult = await SiginManager.ExternalLoginSignInAsync
                (info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (sigInResult.Succeeded)
            {
                //there is record in asp user login table
                return LocalRedirect(returnUrl);
            }
            else
            {
                if (email != null)
                {
                    if (user == null)
                    {
                        //does not have local account

                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                        //create user
                        await UserManager.CreateAsync(user);

                        var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                            new { userId = user.Id, token = token }, Request.Scheme);
                        logger.Log(LogLevel.Warning, confirmationLink);

                    }
                    //local user existing
                    //add row to asp user login table
                    await UserManager.AddLoginAsync(user, info);

                    await SiginManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);

                }
                ViewBag.ErrorTitle = $"Email not recived from {info.LoginProvider}";
                ViewBag.ErrorMessage = "please ccontact us to fix the problem";
                return View("Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, City = model.City };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //confirm the mail
                    //note that taken provider must be added
                    var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);
                    logger.Log(LogLevel.Warning, confirmationLink);

                    if (SiginManager.IsSignedIn(User) && User.IsInRole("Admin"))
                        return RedirectToAction("ListUsers", "Role");
                    //prevent before confirmation
                    //await SiginManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("Index", "Home");

                    ViewBag.ErrorTitle = "Regesteration successful";
                    ViewBag.ErrorMessage = "Before login,please confirm your email.";
                    return View("Error");


                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)//parametr send from the confiramtion link
        {
            if (userId == null || token == null)
                return RedirectToAction("Index", "Home");
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, $"The user Id {userId} invalid");
                return View("NotFound");
            }
            var result = await UserManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return View();
            ViewBag.ErrorTitle = "Email could not be confirmed.";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await SiginManager.SignOutAsync();
            return RedirectToAction("Index", "home");
        }
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]

        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)

                return Json(true);
            else
                return Json($"Email {email} already in use..");
        }

        [HttpGet]

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null && await UserManager.IsEmailConfirmedAsync(user))
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                    var confirmationLink = Url.Action("ResetPassword", "Account",
                        new { email = model.Email, token = token }, Request.Scheme);
                    //logger.Log(LogLevel.Warning,passwordReset);
                    return View("ForgotPasswordConfirmation");
                }
                return View("ForgotPasswordConfirmation");

            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                ModelState.AddModelError("", "invalid password reset token");
            return View();

        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await UserManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        //if the user is locked,free it
                        if (await UserManager.IsLockedOutAsync(user))
                            await UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        return View("ResetPasswordConfirmation");

                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                return View("ResetPasswordConfirmation");
            }
            return View(model);


        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await UserManager.GetUserAsync(User);
            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (!hasPassword)
                return RedirectToAction("AddPassword");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login");

                var result = await UserManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View();

                }
                //redresh cookie
                await SiginManager.RefreshSignInAsync(user);
                return View("ConfirmChangePassword");
            }
            return View(model);


        }

        //for external account if they nned to login with login page
        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await UserManager.GetUserAsync(User);
            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (hasPassword)
                return RedirectToAction("ChangePassword");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.GetUserAsync(User);
                var result = await UserManager.AddPasswordAsync(user,model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View();

                }
                //refresh cookie
                await SiginManager.RefreshSignInAsync(user);
                return View("ConfirmAddPassword");
            }
            return View(model);


        }
    }
}
    