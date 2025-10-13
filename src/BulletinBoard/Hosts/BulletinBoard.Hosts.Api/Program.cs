using BulletinBoard.Application.ComponentRegistrar;
using BulletinBoard.Application.Validators.Advertisements;
using BulletinBoard.Hosts.Api.Validation;
using BulletinBoard.Infrastructure.ComponentRegistrar;
using BulletinBoard.Infrastructure.Middlewares;
using FluentValidation;
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

    builder.Services.AddValidatorsFromAssemblyContaining<CreateAdvertisementDtoValidator>();
    builder.Services.AddFluentValidationAutoValidation(configuration =>
    {
        configuration.DisableBuiltInModelValidation = true;
    
        configuration.OverrideDefaultResultFactoryWith<CustomValidationResultFactory>();
    });
    
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(o =>
    {
        var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
        foreach (var xml in xmlFiles)
            o.IncludeXmlComments(xml, includeControllerXmlComments: true);
    });

    var app = builder.Build();
    
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
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