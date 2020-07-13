using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagment.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/errors/{statusCode}")]
        public IActionResult httpStatusCodeHandeler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "sorry, the resource not be found";
                    break;
            }
            return View("NotFound");
        }
        [Route("error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
           var exceptionDetails= HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.ExceptionPath = exceptionDetails.Path;
            ViewBag.ExceptionMessage = exceptionDetails.Error.Message;
            ViewBag.Stacktrace = exceptionDetails.Error.StackTrace;
            return View("Error");




        }
    }
}