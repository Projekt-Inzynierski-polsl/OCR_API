using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using OCR_API;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Logger;
using OCR_API.MappingProfiles;
using OCR_API.Middleware;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.ModelsDto.Validators;
using OCR_API.Registrars;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;
using System.Text;
using DotNetEnv;
using OCR_API.Enums;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

AuthenticationSettings.JwtKey = Environment.GetEnvironmentVariable("JwtKey");
AuthenticationSettings.JwtExpireDays = int.Parse(Environment.GetEnvironmentVariable("JwtExpireDays"));
AuthenticationSettings.JwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer");
EnvironmentSettings.Environment = Enum.Parse<EEnvironment>(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

builder.Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = "Bearer";
        option.DefaultScheme = "Bearer";
        option.DefaultChallengeScheme = "Bearer";
    }).AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.SaveToken = true;
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = AuthenticationSettings.JwtIssuer,
            ValidAudience = AuthenticationSettings.JwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthenticationSettings.JwtKey))
        };
    });

builder.Services.AddControllers().AddFluentValidation();
builder.Services.AddFluentValidationAutoValidation(); 
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddDbContext<SystemDbContext>(options =>
{
    string connectionString;
    switch (EnvironmentSettings.Environment)
    {
        case EEnvironment.Development:
            connectionString = Environment.GetEnvironmentVariable("DevConnection");
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            break;
        case EEnvironment.Production:
            connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
            options.UseSqlServer(connectionString);
            break;
        default:
            connectionString = Environment.GetEnvironmentVariable("DebugConnection");
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            break;
    };
}, ServiceLifetime.Transient);



Registar registar = new Registar();
registar.ConfigureServices(builder.Services);


builder.Host.UseNLog();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndClient", b =>
        b.AllowAnyMethod()
            .AllowAnyHeader()
             .SetIsOriginAllowed(origin => true)
    //.WithOrigins(builder.Configuration["AllowedOrigins"])
    );
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseCors("FrontEndClient");
var userScope = app.Services.CreateScope();
var userSeeder = userScope.ServiceProvider.GetRequiredService<Seeder>();
userSeeder.Seed();

app.UseAuthentication();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
//}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
