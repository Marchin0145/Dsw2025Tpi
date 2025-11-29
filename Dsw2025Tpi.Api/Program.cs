
using System.Reflection;
using System.Text;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Middleware;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data.Contexts;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173") // Tu Vite/React
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Si usas JWT/cookies
            });
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>

           options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025Tpi"),
           x=>x.MigrationsAssembly("Dsw2025Tpi.Api")
           )//corregir
           );
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IRepository, EfRepository>();
        builder.Services.AddTransient<ProductServices>();
        builder.Services.AddTransient<OrderServices>();
        builder.Services.AddScoped<CustomerServices>();
        builder.Services.AddSwaggerGen(o => 
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Desarrollo de software",

               Version = "v1",

            });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Ingresar el token",
                Type = SecuritySchemeType.ApiKey


            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });


        }
            
            );
        builder.Services.AddHealthChecks();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(Options =>// configuracion de entity,añadimos el servicio
        {
            Options.Password = new PasswordOptions// parametros para la contraseña
            {
                RequiredLength = 8

            };

        })
            .AddEntityFrameworkStores<AuthenticateContext>()
            .AddDefaultTokenProviders();


        var jwtConfig = builder.Configuration.GetSection("Jwt");// recuperar configuracion de JWT
        var keyText = jwtConfig["key"] ?? throw new ArgumentNullException("JWT Key"); // recuperar key
        var key= Encoding.UTF8.GetBytes(keyText); // convertir a bytes

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;// establecer esquema de autenticación por defecto
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


        })
            .AddJwtBearer(options =>
            {//para la validacion del token JWT
                options.TokenValidationParameters = new TokenValidationParameters// establecer parámetros de validación del token
                {
                    ValidateIssuer = true,// validar el emisor
                    ValidateAudience = true,// validar el público
                    ValidateLifetime = true,// validar la duración del token
                    ValidateIssuerSigningKey = true,// validar la clave de firma
                    ValidIssuer = jwtConfig["issuer"],// establecer el emisor
                    ValidAudience = jwtConfig["audience"],// establecer el público
                    IssuerSigningKey = new SymmetricSecurityKey(key) // establecer la clave de firma

                };
            
            });
        
         builder.Services.AddSingleton<JwtTokenService>();

        
        builder.Services.AddDbContext<AuthenticateContext>(options=>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025Tpi"), b => b.MigrationsAssembly("Dsw2025Tpi.Api"));
        // la base de datos que utilizara entity
        }
        );


        var app = builder.Build();

        Task.Run(async () => await SeedDataAsync(app)).Wait();



        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();
       
        app.UseCors("AllowFrontend");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();
        
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }

    static async Task SeedDataAsync(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            try
            {
                // Crear roles si no existen
                string[] roles = { "Admin", "Employee", "Customer" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Crear usuario admin
                var adminEmail = configuration["AdminUser:Email"] ?? "admin@empresa.com";
                var adminPassword = configuration["AdminUser:Password"] ?? "Admin123!";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"Usuario admin creado: {adminEmail}");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Error al ejecutar seeder");
            }
        }
    }

}
