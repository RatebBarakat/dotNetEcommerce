﻿using ecommerce.Data;
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
using static Org.BouncyCastle.Math.EC.ECCurve;
using ecommerce.Policies;
using System.ComponentModel;
using ecommerce.Services;
using ecommerce.Emails;
using Hangfire;
using ecommerce.Services.Excel;
using ecommerce.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddValidatorsFromAssemblyContaining<LoginUserValidator>();

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<SendEmailVerificationLink>();
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(c => c.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

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

builder.Services.AddSignalR();

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddScoped<IAuthorizationHandler, EmailConfirmedRequirementHandler>();
builder.Services.AddSingleton<IRedis, Redis>();
builder.Services.AddSingleton<ImageHelper>();
builder.Services.AddTransient<PermissionHelper>();
builder.Services.AddTransient<ProductExcelImportService>();
builder.Services.AddTransient<CategoryExcelImportService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmailConfirmedPolicy", policy =>
        policy.Requirements.Add(new EmailConfirmedRequirement()));
});


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, Role>(options =>
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
});


var app = builder.Build();

app.UseRouting();

app.UseCors("all");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notifications");
});

app.MapControllers();

app.Run();
