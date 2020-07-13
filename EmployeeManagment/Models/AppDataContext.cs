using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
    public class AppDataContext:IdentityDbContext<ApplicationUser>
    {
        public AppDataContext(DbContextOptions<AppDataContext> options):base(options)
        {

        }
       public DbSet<Employee> Employees { get; set; }
        protected override void OnModelCreating(ModelBuilder model)
        {

            base.OnModelCreating(model);
            model.Seed();

            foreach (var foreignKey in
                model.Model.GetEntityTypes().SelectMany(m=>m.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
          

    }
}
