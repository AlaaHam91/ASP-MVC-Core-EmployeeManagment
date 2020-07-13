using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Utilities
{
    public class ValidEmailDomainAttribute:ValidationAttribute
    {
        private readonly string AllowDomain;//attribute

        public ValidEmailDomainAttribute(string allowDomain)
        {
            AllowDomain = allowDomain;
        }


        public override bool IsValid(object value)//value is value read from view
        {

            //return base.IsValid(value);
            string[] strings = value.ToString().Split('@');
            return strings[1].ToUpper()==AllowDomain.ToUpper();
        }
    }
}
