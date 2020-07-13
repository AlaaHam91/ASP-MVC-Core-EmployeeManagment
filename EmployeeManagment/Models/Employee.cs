using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [NotMapped]
        public string EncryptedId{ get; set; }

        [Required]
        [MaxLength(15,ErrorMessage ="Name must not excced 15 char")]
        public string Name { get; set; }
        [Required]
      //  [RegularExpression("^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$")]
      [Display(Name="Mail Address")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }//that make the feild is optional s it is required by default
                                             //or only remove the required and the ?
        public string PhotoPath { get; set; }
    }
}
