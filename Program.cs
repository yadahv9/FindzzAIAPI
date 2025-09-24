using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectAgaman.Core.Helpers;
using ProjectAgaman.Repositories.AffiliateRepositories;
using ProjectAgaman.Repositories.AzureOpenAIRepositories;
using ProjectAgaman.Repositories.DashboardRepositories;
using ProjectAgaman.Repositories.EmailSender;
using ProjectAgaman.Repositories.EmailServices;
using ProjectAgaman.Repositories.ErrorLogsRepositories;
using ProjectAgaman.Repositories.FetchJobsRepositories;
using ProjectAgaman.Repositories.OrderInfRepositories;
using ProjectAgaman.Repositories.PackageRepositories;
using ProjectAgaman.Repositories.PaymentsRepositories;
using ProjectAgaman.Repositories.PromoRepositories;
using ProjectAgaman.Repositories.RecruiterRepositories;
using ProjectAgaman.Repositories.RolesRepositories;
using ProjectAgaman.Repositories.SettingsRepositories;
using ProjectAgaman.Repositories.UserJobsRepositories;
using ProjectAgaman.Repositories.UsersRepositories;
using System.Text;
using IEmailSender = ProjectAgaman.Repositories.EmailSender.IEmailSender;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
// Add services to the container.
// Configure JWT authentication with only the key
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c => {


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddSingleton<DbContext>();
builder.Services.AddScoped<IRoleRepository,RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailSender,EmailSender>();
builder.Services.AddScoped<IEmailServices, EmailServices>();
builder.Services.AddScoped<IUserJobsRepository,UserJobsRepository>();
builder.Services.AddScoped<IOrderInfoRepository, OrderInfoRepository>();
builder.Services.AddScoped<IFetchJobsRepository, FetchJobsRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IAzureOpenAIRepository, AzureOpenAIRepository>();
builder.Services.AddScoped<IRecruiterRepository, RecruiterRepository>();
builder.Services.AddScoped<IPaymentRepository,PaymentsRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddScoped<IAffiliateRepository, AffiliateRepository>();
builder.Services.AddScoped<IPromoRepository, PromoRepository>();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToFile("/index.html");

app.Run();
