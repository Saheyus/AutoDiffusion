﻿using AutoDiffusion.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoDiffusion.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<NameModel>? Names { get; set; }
        public DbSet<ProbabilityModel>? Probabilities { get; set; }
        public DbSet<WordParametersModel>? WordParameters { get; set; }
        public DbSet<GeneratedWordModel>? GeneratedWords { get; set; }
        public DbSet<LanguageModel>? Languages { get; set; }
        public DbSet<FullNameModel>? FullNames { get; set; }
    }
}