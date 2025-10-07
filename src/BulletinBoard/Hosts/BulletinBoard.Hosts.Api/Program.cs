using BulletinBoard.Infrastructure.ComponentRegistrar;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterAppServices()
                .RegisterRepositories();

// Controllers
builder.Services.AddControllers();

// Swagger + XML docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
    foreach (var xml in xmlFiles)
        o.IncludeXmlComments(xml, includeControllerXmlComments: true);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();