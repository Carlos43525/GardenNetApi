using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GardenNetApi.Data
{
    public class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider, ModelBuilder modelBuilder)
        {
            using (var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                context.Database.EnsureCreated();

                // Look for prior seed.
                if (context.Users.Any())
                {
                    return;   // DB has been seeded
                }

                //Seeds admin role to AspNetRoles table
                modelBuilder.Entity<IdentityRole>().HasData(
                    new IdentityRole
                    {
                        Id = "168172ee-fcd3-43a2-be0e-9a9a27699bd0",
                        Name = "Admin",
                        NormalizedName = "ADMIN"
                    });

                var hasher = new PasswordHasher<IdentityUser>();

                //Seeding the User to AspNetUsers table
                modelBuilder.Entity<IdentityUser>().HasData(
                            new IdentityUser
                            {
                                Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", // primary key
                                UserName = "admin",
                                NormalizedUserName = "ADMIN",
                                PasswordHash = hasher.HashPassword(null, "admin")
                            }
                        );

                //Seed relation between user and role to AspNetUserRoles table
                modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                    new IdentityUserRole<string>
                    {
                        RoleId = "168172ee-fcd3-43a2-be0e-9a9a27699bd0",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
                    }
                );

            }
        }
    }
}
