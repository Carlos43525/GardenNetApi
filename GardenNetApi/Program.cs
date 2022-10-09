using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using GardenNetApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GardenNetApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigurationManager configuration = builder.Configuration;

            //var client = new SecretClient(new Uri(configuration["KeyVault:EndPoint"]), new DefaultAzureCredential());
            //var secret = client.GetSecret("ThingSpeak").Value;

            // Access Azure app configuration with managed identity.
            // Must be logged into VS with azure credentials to use locally. 
            builder.Host.ConfigureAppConfiguration(builder =>
            {
                builder.AddAzureAppConfiguration(options =>
                    options.Connect(new Uri(configuration["AppConfig:Endpoint"]), new DefaultAzureCredential()));
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.ConfigureSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Garden Net Api",
                    Version = "v1.0.0"
                });
            });

            // We save the connection string into a variable here because UseNpgsql can't seem
            // to parse the string from builder.Configuration["Connection:String"] unless it's
            // in a variable already. 
            string db = builder.Configuration["Connection:String"]; 

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(db));

            // Identity 
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Authentication and add Jwt Bearer
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidAudience = configuration["JWT:ValidAudience"],
                    //ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:TOKEN"]))
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
                };
            });

            // Auth Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole("Admin"));
            });

            builder.Services.AddHttpClient();

            var app = builder.Build();

            app.UseSwagger(options =>
            {
                // Downgrade Swaggger to V2. Azure does not support Swagger V3 for OpenApi. 
                options.SerializeAsV2 = true;
            });

            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {

            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}