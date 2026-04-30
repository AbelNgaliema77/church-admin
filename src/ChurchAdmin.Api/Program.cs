using System.Text;
using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Infrastructure;
using ChurchAdmin.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthSettings>(
    builder.Configuration.GetSection("Auth"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

AuthSettings authSettings = builder.Configuration
    .GetSection("Auth")
    .Get<AuthSettings>()
    ?? throw new InvalidOperationException("Auth settings are missing.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = authSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authSettings.JwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "http://localhost:4173",
            "https://localhost:4173",
            "https://church-admin-frontend.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            ValidationProblemDetails problemDetails =
                new ValidationProblemDetails(context.ModelState)
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                };

            return new BadRequestObjectResult(problemDetails);
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Church Admin API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token only. Do not type Bearer manually."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            []
        }
    });
});

WebApplication app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        IExceptionHandlerFeature? exceptionFeature =
            context.Features.Get<IExceptionHandlerFeature>();

        Exception? exception = exceptionFeature?.Error;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problemDetails = new ProblemDetails
        {
            Title = "Unexpected server error",
            Detail = app.Environment.IsDevelopment()
                ? exception?.Message
                : "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("ReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    ChurchAdminDbContext dbContext =
        scope.ServiceProvider.GetRequiredService<ChurchAdminDbContext>();

    dbContext.Database.Migrate();
}

app.Run();