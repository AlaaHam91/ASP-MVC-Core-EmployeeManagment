using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagment.Models;
using EmployeeManagment.Security;
using EmployeeManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagment.Controllers
{
    //  [Route("home")] 
    //  [Route("{controller}")]
  //  [Route("[controller]/[action]")]
  //  [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository empRepository;

        private  IHostingEnvironment HostingEnvironment { get; }
        private readonly IDataProtector protector;
        public HomeController(IEmployeeRepository empRep,
            IHostingEnvironment hostingEnvironment,IDataProtectionProvider dataProtectionProvider,
            DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            this.empRepository = empRep;
            HostingEnvironment = hostingEnvironment;
            //for incrypt and dycript
            protector = dataProtectionProvider.
                CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }
        //public JsonResult Details()
        //{
        //    Employee e = empRepository.GetEmployee(1);
        //    return Json(e);
        //}
        //public ObjectResult Details()
        //{
        //    Employee e = empRepository.GetEmployee(1);
        //    return new ObjectResult(e);
        //}

        // [Route("details/[id?]")]
        //  [Route("[action]/[id?]")]
       // [Route("{id?}")]
        [AllowAnonymous]
        public ViewResult Details(string id )//encrypt id
        {
            int EmployeeId=Convert.ToInt32(protector.Unprotect(id));
            Employee e= empRepository.GetEmployee(EmployeeId);
            if (e == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", EmployeeId);
            }
            HomeDetailsViewModel model = new HomeDetailsViewModel
            { Employee = e, PageTitle = "Details Page" };
            return View(model);
        }
        //  [Route("")]
        // [Route("index")]
      //  [Route("~/Home")]

       // [Route("~/")]
       [AllowAnonymous]
        public ViewResult Index()
        {
            var model = empRepository.GetAllEmplyee().
                //added for encrypt and decrypt
                Select(e=>
                {
                    e.EncryptedId = protector.Protect(e.Id.ToString());
                    return e;
                });
            return View(model);
        }
        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]

        public IActionResult Create(EmployeeCreateViewModel model)
        {
            //IActionResult instead of RedirectToActionResult
            if (ModelState.IsValid)
            {
                string uniqueFileName = string.Empty;
                uniqueFileName = ProcessFileUpload(model);

                Employee employee = new Employee (){
                    Department=model.Department,
                    Email = model.Email,
                    Name = model.Name,
                    PhotoPath= uniqueFileName


                };

                 empRepository.Add(employee);
                return RedirectToAction("Details", new { id = employee.Id });
            }
            return View();
        }

        private string ProcessFileUpload(EmployeeCreateViewModel model)
        {
            string uniqueFileName=string.Empty;

            if (model.Photo != null)
            {

                string uploadFolder = Path.Combine(HostingEnvironment.WebRootPath, "Images");
                uniqueFileName = Guid.NewGuid().ToString() + '_' + model.Photo.FileName;
                string filepath = Path.Combine(uploadFolder, uniqueFileName);
                using (var FileStream=new FileStream(filepath, FileMode.Create)) 

                {
                    model.Photo.CopyTo(FileStream);
                }
            }
            return uniqueFileName;

        }

        public ViewResult Edit(int id=0)
        {
            Employee e = empRepository.GetEmployee(id);
            EmployeeEditViewModel model = new EmployeeEditViewModel
            {
                Id=e.Id,
                Name=e.Name,
                Email=e.Email,
                Department=e.Department,
                ExistingPhotoPath=e.PhotoPath
            };
            return View(model);
        }
        [HttpPost]

        public IActionResult Edit(EmployeeEditViewModel model)
        {
            //IActionResult instead of RedirectToActionResult
            if (ModelState.IsValid)
            {

                Employee employee = empRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if (model.Photo != null)
                {
                    if (model.ExistingPhotoPath != null)
                        System.IO.File.Delete(Path.Combine(HostingEnvironment.WebRootPath,"Images",model.ExistingPhotoPath));
                 
                    employee.PhotoPath = ProcessFileUpload(model);
                }
                empRepository.Update(employee);
                return RedirectToAction("Details", new { id = employee.Id });
            }
            return View();
        }


    }
}