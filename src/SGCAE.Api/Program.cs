using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using SGCAE.Api.Middleware;
using SGCAE.Application;
using SGCAE.Infrastructure;
using SGCAE.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Registro de capas Clean Architecture (Application & Infrastructure)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Controladores REST y serialización JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 3. Manejo global de excepciones (RFC 7807 ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Documentación OpenAPI (.NET 10)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "SGCAE API - Sistema de Gestión y Control de Acceso de Empleados";
        document.Info.Version = "v1";
        document.Info.Description = "Backend empresarial con Clean Architecture, DDD, CQRS y EF Core sobre SQL Server.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// 5. Inicialización automática de Base de Datos y Datos Semilla
await DatabaseInitializer.InitializeDatabaseAsync(app.Services);

// 6. Pipeline HTTP y Documentación Interactiva (Swagger UI + Scalar)
app.UseExceptionHandler();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "SGCAE API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "SGCAE API - Swagger UI";
});

app.MapScalarApiReference(options =>
{
    options.WithTitle("SGCAE API - Documentación Interactiva");
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
