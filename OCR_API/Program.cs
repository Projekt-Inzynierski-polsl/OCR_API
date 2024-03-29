using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using OCR_API;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.Middleware;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.Validators;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);

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
            ValidIssuer = authenticationSettings.JwtIssuer,
            ValidAudience = authenticationSettings.JwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
        };
    });

builder.Services.AddControllers().AddFluentValidation();
builder.Services.AddFluentValidationAutoValidation(); 
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddDbContext<SystemDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Transient);

builder.Services.AddScoped<Seeder>();
builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPasswordHasher<Folder>, PasswordHasher<Folder>>();
builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
builder.Services.AddScoped<IValidator<LoginUserDto>, LoginUserDtoValidator>();

builder.Services.AddScoped<IRepository<BlackListToken>, Repository<BlackListToken>>();
builder.Services.AddScoped<IRepository<BoundingBox>, Repository<BoundingBox>>();
builder.Services.AddScoped<IRepository<Folder>, Repository<Folder>>();
builder.Services.AddScoped<IRepository<NoteCategory>, Repository<NoteCategory>>();
builder.Services.AddScoped<IRepository<NoteFile>, Repository<NoteFile>>();
builder.Services.AddScoped<IRepository<NoteLine>, Repository<NoteLine>>();
builder.Services.AddScoped<IRepository<NoteWorldError>, Repository<NoteWorldError>>();
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Role>, Repository<Role>>();
builder.Services.AddScoped<IRepository<UploadedModel>, Repository<UploadedModel>>();
builder.Services.AddScoped<IRepository<UserAction>, Repository<UserAction>>();
builder.Services.AddScoped<IRepository<UserLog>, Repository<UserLog>>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(FolderMappingProfile));
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

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("FrontEndClient");
var userScope = app.Services.CreateScope();
var userSeeder = userScope.ServiceProvider.GetRequiredService<Seeder>();
userSeeder.Seed();


app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseHttpsRedirection();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
