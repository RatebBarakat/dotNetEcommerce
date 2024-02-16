using ecommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ecommerce.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ecommerce.Interfaces;
using ecommerce.Validators;
using FluentValidation;
using Microsoft.OpenApi.Models;
using ecommerce.Helpers;
using ecommerce.Attributes;
using ecommerce.Handlers;
using Microsoft.AspNetCore.Authorization;
using ecommerce.Hepers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<LoginUserValidator>();

builder.Services.AddTransient<IAuthService, AuthService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "authorize", Version = "1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter Jwt Token",
        Name = "Authorization",
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
                },
            },
            new string[]{}
        }
    });
});

builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    redisOptions.Configuration = config;
});

var services = builder.Services;
var configuration = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", builder =>
    {
        builder.WithOrigins("http://localhost:5173") 
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmailConfirmedPolicy", policy =>
        policy.Requirements.Add(new EmailConfirmedRequirement()));
});

services.AddScoped<IAuthorizationHandler, EmailConfirmedRequirementHandler>();
services.AddSingleton<IRedis, Redis>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; 
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value)),
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["seid"];
            return Task.CompletedTask;
        }
    };
}).AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Google:ClientId"];
    googleOptions.ClientSecret = configuration["Google:ClientSecret"];
    googleOptions.CallbackPath = "/google/callback";
});
;


var app = builder.Build();

app.UseCors("all");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
