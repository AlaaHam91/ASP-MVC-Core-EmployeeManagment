using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
    public class SqlEmployeeContext : IEmployeeRepository
    {
        public AppDataContext Context { get; }

        public SqlEmployeeContext(AppDataContext context)
        {

            Context = context;
        }


        public Employee Add(Employee e)
        {
           Context.Employees.Add(e);
            Context.SaveChanges();
            return e;
        }

        public Employee Delete(int id)
        {
            Employee e =Context.Employees.Find(id);
            if (e != null)
            {
                Context.Employees.Remove(e);
                Context.SaveChanges();
            }
            return e;
        }

        public IEnumerable<Employee> GetAllEmplyee()
        {
            return Context.Employees;
        }

        public Employee GetEmployee(int id)
        {
            return Context.Employees.SingleOrDefault(e=>e.Id==id);
        }

        public Employee Update(Employee e)
        {
            var emp= Context.Employees.Attach(e);
            emp.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            Context.SaveChanges();
            return e;
        }
    }
}
