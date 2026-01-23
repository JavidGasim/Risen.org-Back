using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Risen.Business.Integrations.Hipolabs;
using Risen.Business.Options;
using Risen.Business.Services.Abstracts;
using Risen.Business.Services.Concretes;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using Risen.Web.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection("AdminSeed"));


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

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"]!;
var issuer = jwt["Issuer"]!;
var audience = jwt["Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumOnly", p =>
        p.RequireClaim("entitlement", "premium"));
});


builder.Services.Configure<QuestPolicyOptions>(
    builder.Configuration.GetSection("QuestPolicy"));



builder.Services.AddScoped<IEntitlementService, EntitlementService>();
builder.Services.AddScoped<IQuestEntitlementService, QuestEntitlementService>();

builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddScoped<IUniversitySuggestService, UniversitySuggestService>();

builder.Services.AddScoped<IXpService, XpService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IQuestFeedService, QuestFeedService>();

builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IStatsService, StatsService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IQuestAnswerService, QuestAnswerService>();

builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await IdentitySeeder.SeedAdminAsync(app.Services, app.Environment);

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
