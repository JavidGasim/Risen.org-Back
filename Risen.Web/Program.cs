using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Risen.Business.Integrations.Hipolabs;
using Risen.Business.Services.Abstracts;
using Risen.Business.Services.Concretes;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services
    .AddIdentityCore<CustomIdentityUser>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;
        opt.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<CustomIdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.Services.AddHttpClient<IHipolabsClient, HipolabsClient>(c =>
{
    c.BaseAddress = new Uri("http://universities.hipolabs.com");
    c.Timeout = TimeSpan.FromSeconds(10);
});

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
var issuer = builder.Configuration["Jwt:Issuer"]!;
var audience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumOnly", p =>
        p.RequireClaim("entitlement", "premium"));
});

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEntitlementService, EntitlementService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
