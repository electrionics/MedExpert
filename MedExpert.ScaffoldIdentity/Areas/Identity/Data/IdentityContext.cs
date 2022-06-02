using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedExpert.ScaffoldIdentity.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.ScaffoldIdentity.Data
{
    public class IdentityContext : IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<User>().ToTable("_AspNetUser", "dbo");
            builder.Entity<Role>().ToTable("_AspNetRole", "dbo");
            builder.Entity<RoleClaim>().ToTable("_AspNetRoleClaim", "dbo");
            builder.Entity<UserClaim>().ToTable("_AspNetUserClaim", "dbo");
            builder.Entity<UserRole>().ToTable("_AspNetUserRole", "dbo");
            builder.Entity<UserLogin>().ToTable("_AspNetUserLogin", "dbo");
            builder.Entity<UserToken>().ToTable("_AspNetUserToken", "dbo");
        }
    }
}
