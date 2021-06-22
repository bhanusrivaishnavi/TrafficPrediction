using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCapplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MVCapplication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public ApplicationDbContext()
        { }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
        //protected ApplicationDbContext ApplicationDbContext { get; set; }
       
        protected UserManager<ApplicationUser> UserManager { get; set; }
       
    }
}
