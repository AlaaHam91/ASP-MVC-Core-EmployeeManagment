using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
    public class EmployeeCreateViewModel
    {
        [Required]
        [MaxLength(15, ErrorMessage = "Name must not excced 15 char")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Mail Address")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }
          public IFormFile Photo { get; set; }
       // public List<IFormFile> Photos { get; set; }
    }
}
