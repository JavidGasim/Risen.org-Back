using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Risen.Business.Integrations.Hipolabs;
using Risen.Business.Options;
using Risen.Business.Services.Abstracts;
using Risen.Business.Services.Concretes;
using Risen.Business.Validators;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using Risen.Web.Hubs;
using Risen.Web.Infrastructure;
using Risen.Web.Middlewares;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


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

        // 🔥 THIS IS REQUIRED FOR SIGNALR
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumOnly", p =>
    p.RequireClaim("isPremium", "true"));
});


builder.Services.Configure<QuestPolicyOptions>(
    builder.Configuration.GetSection("QuestPolicy"));

builder.Services.Configure<RetentionOptions>(
    builder.Configuration.GetSection("Retention"));

builder.Services.AddHostedService<Risen.Web.Services.RetentionService>();

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", p =>
    {
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .SetIsOriginAllowed(_ => true)
             .AllowCredentials();
        }
        else
        {
            p.SetIsOriginAllowed(_ => true)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .SetIsOriginAllowed(_ => true)
             .AllowCredentials();
        }
    });
});

builder.Services.AddScoped<IEntitlementService, EntitlementService>();
builder.Services.AddScoped<IQuestEntitlementService, QuestEntitlementService>();

builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddScoped<IUniversitySuggestService, UniversitySuggestService>();

builder.Services.AddScoped<IXpService, XpService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IQuestFeedService, QuestFeedService>();
builder.Services.AddScoped<IQuestQueryService, QuestQueryService>();

builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IStatsService, StatsService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUniversityCandidateService, UniversityCandidateService>();

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikedPostService, LikedPostService>();
builder.Services.AddScoped<ILikedCommentService, LikedCommentService>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();

builder.Services.AddMemoryCache();

// Admin audit service
builder.Services.AddScoped<Risen.Business.Services.Abstracts.IAdminAuditService, Risen.Business.Services.Concretes.AdminAuditService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Subjects
builder.Services.AddScoped<Risen.Business.Services.Abstracts.ISubjectService, Risen.Business.Services.Concretes.SubjectService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

var app = builder.Build();

await IdentitySeeder.SeedAdminAsync(app.Services, app.Environment);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseMiddleware<LastOnlineMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.UseWebSockets();
app.MapHub<CommunityHub>("/communityHub");
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
