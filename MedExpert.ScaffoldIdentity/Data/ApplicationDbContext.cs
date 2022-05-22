using System;
using System.Collections.Generic;
using System.Text;
using MedExpert.ScaffoldIdentity.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.ScaffoldIdentity.Data
{
    public class ApplicationDbContext : IdentityContext
    {
        public ApplicationDbContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }
    }
}