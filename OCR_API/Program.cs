using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using OCR_API;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Middleware;
using OCR_API.Repositories;
using OCR_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddFluentValidationAutoValidation(); 
builder.Services.AddFluentValidationClientsideAdapters(); 
builder.Services.AddDbContext<SystemDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Transient);

builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IRepository<User>, Repository<User>>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();

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
