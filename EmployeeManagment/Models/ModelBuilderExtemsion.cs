using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
    public static class ModelBuilderExtemsion
    {
        public static void Seed(this ModelBuilder model)
        {
            model.Entity<Employee>().HasData(
               new Employee() { Id = 1, Name = "Alaa", Email = "alaa@gmail.com", Department = Dept.Hr },
               new Employee() { Id = 2, Name = "Ali", Email = "ali@gmail.com", Department = Dept.IT }

              );
        }
    }
}
