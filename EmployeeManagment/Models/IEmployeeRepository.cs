using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
   public interface IEmployeeRepository
    {
        Employee GetEmployee(int id);
        IEnumerable<Employee> GetAllEmplyee();
        Employee Add(Employee e);
        Employee Update(Employee e);
        Employee Delete(int id);
    }
}
