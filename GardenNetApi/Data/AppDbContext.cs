﻿using GardenNetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GardenNetApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Plant> Plants { get; set; }
    }
}
