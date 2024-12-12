using Microsoft.AspNetCore.Authentication.JwtBearer;
using Identity.API.Services.Implementations;
using Identity.API.Services.Abstractions;
using Global.Infrastructure.Middlewares;
using Identity.API.Data.Initialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Identity.API.Services.Mapping;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data.DbContexts;
using Identity.API.Data.Dtos.Email;
using Identity.API.Data.Entities;
using Identity.API.Data.Dtos.Url;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
builder.Services.AddDbContext<AuthDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("BaimIdentity")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.EmailConfirmationTokenProvider = "Default";
}).AddDefaultTokenProviders()
.AddEntityFrameworkStores<AuthDbContext>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(configuration["AllowedOrigins"]!.Split(";"))
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    options.SignIn.RequireConfirmedEmail = true;
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = "Role"
    };
});



builder.Services.AddSwaggerGen(options =>
{
    const string scheme = "Bearer";
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Identity Service",
        Version = "v1"
    });

    options.AddSecurityDefinition(
        name: scheme,
        new OpenApiSecurityScheme()
        {
            Description = "Enter here jwt token without \'Bearer\'. Exapmle: eyJhbGciOiJIUzI1NiIsInR...",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = scheme
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement() {
            {
                new OpenApiSecurityScheme() {
                    Reference = new OpenApiReference() {
                        Id = scheme,
                        Type = ReferenceType.SecurityScheme
                    }
                } ,
                new string[] {}
            }
        }
    );
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
    options.AddPolicy("AdminAndStaffOnly", policy => policy.RequireRole("Admin", "Staff"));
});


var _emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfigurationDto>() ??
                  throw new InvalidOperationException("'Email Configuration' not found."); 

var _urlSettingsConfig = configuration.GetSection("UrlSettings").Get<UrlSettings>() ??
                  throw new InvalidOperationException("'Url Settings Configuration' not found.");

var _userSyncServiceUrl = builder.Configuration["ExternalServices:UserSyncServiceUrl"];


builder.Services.AddSingleton(_urlSettingsConfig);
builder.Services.AddSingleton(_emailConfig);

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>(); 
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPasswordService, PasswordService>(); 
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddScoped<GlobalExceptionsMiddleware>();


builder.Services.AddHttpClient<IExternalUserSyncService, ExternalUserSyncService>(client => client.BaseAddress = new Uri(_userSyncServiceUrl!));
builder.Services.AddHttpClient<IExternalUserPhotoSyncService, ExternalUserPhotoSyncService>(client => client.BaseAddress = new Uri(_userSyncServiceUrl!)); builder.Services.AddHttpClient<IExternalUserSyncService, ExternalUserSyncService>(client => client.BaseAddress = new Uri(_userSyncServiceUrl!));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await DbInitializer.InitializeAsync(serviceProvider);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();


app.UseMiddleware<GlobalExceptionsMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

app.Run();