using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        List<Employee> empList;
        public MockEmployeeRepository()
        {
            this.empList = new List<Employee>() {
                new Employee() { Id = 1, Name = "Alaa", Email = "alaa@gmail.com", Department = Dept.Hr },
                new Employee() { Id = 2, Name = "Ali", Email = "ali@gmail.com", Department = Dept.IT}

            };
        }

        public Employee Add(Employee emp)
        {
            emp.Id = empList.Max(e => e.Id) + 1;
            empList.Add(emp);
            return emp;
        }

        public Employee Delete(int id)
        {
            Employee e = empList.SingleOrDefault(emp => emp.Id==id);
            if (e != null)
            {
                empList.Remove(e);
            }
            return e;
        }

        public IEnumerable<Employee> GetAllEmplyee()
        {
            return empList ;
        }

        public Employee GetEmployee(int id)
        {
            return empList.SingleOrDefault(e=>e.Id==id);
        }

        public Employee Update(Employee newEmp)
        {
            Employee OldEmp = empList.SingleOrDefault(emp => emp.Id == newEmp.Id);
            if (OldEmp != null)
            {
                OldEmp.Department=newEmp.Department;
                OldEmp.Email = newEmp.Email;
                OldEmp.Name = newEmp.Name;

            }
            return OldEmp;
        }
    }
}
