using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Roles = new List<string>();
            Claims= new List<string>();
        }
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string City { get; set; }
        public List<string> Claims { get; set; }

        public IList<string> Roles { get; set; }



    }
}
