using System.Security.Claims;
using System.Text;
using BulletinBoard.Application.ComponentRegistrar;
using BulletinBoard.Application.Validators.Advertisements;
using BulletinBoard.Hosts.Api.Validation;
using BulletinBoard.Infrastructure.ComponentRegistrar;
using BulletinBoard.Infrastructure.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting BulletinBoard API...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddApplication()
        .AddInfrastructure(builder.Configuration);
    
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("ActiveUser", policy =>
            policy.RequireAssertion(context =>
            {
                var createdAtClaim = context.User.FindFirstValue("CreatedAt");
                if (string.IsNullOrEmpty(createdAtClaim))
                    return false;

                var createdAt = DateTimeOffset.Parse(createdAtClaim);
                var accountAge = DateTimeOffset.UtcNow - createdAt;

                return accountAge.TotalMinutes>= 1;
            }));

    builder.Services.AddValidatorsFromAssemblyContaining<CreateAdvertisementDtoValidator>();
    builder.Services.AddFluentValidationAutoValidation(configuration =>
    {
        configuration.DisableBuiltInModelValidation = true;
    
        configuration.OverrideDefaultResultFactoryWith<CustomValidationResultFactory>();
    });
    
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
        foreach (var xml in xmlFiles)
            options.IncludeXmlComments(xml, includeControllerXmlComments: true);

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Введите JWT токен в формате: Bearer {ваш токен}"
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
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    
    app.MapControllers();

    Log.Information("BulletinBoard API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}