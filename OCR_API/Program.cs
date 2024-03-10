using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.Middleware;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Host.UseNLog();

builder.Services.AddFluentValidationAutoValidation(); 
builder.Services.AddFluentValidationClientsideAdapters(); 
builder.Services.AddDbContext<SystemDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
}, ServiceLifetime.Transient);
builder.Services.AddScoped<Seeder>();
builder.Services.AddDbContext<SystemDbContext>(options =>
{

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IRepository<User>, Repository<User>>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddAutoMapper(typeof(UserMappingProfile));


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndClient", b =>
        b.AllowAnyMethod()
            .AllowAnyHeader()
             //.SetIsOriginAllowed(origin => true)
    .WithOrigins(builder.Configuration["AllowedOrigins"])
    );
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("FrontEndClient");
var userScope = app.Services.CreateScope();
var userSeeder = userScope.ServiceProvider.GetRequiredService<Seeder>();
userSeeder.Seed();


app.UseMiddleware<ErrorHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
