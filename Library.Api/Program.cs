using System.Text.Json.Serialization;
using Library.Api.Common;
using Library.Application.Handlers;
using Library.Application.Models.Results;
using Library.Domain.Loans;
using Library.Infrastructure;
using Library.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace Library.Api;

public class Program
{
    #region Public Methods

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Library Lending System API",
                Version = "v1",
                Description = "API for registering books, lending available copies, returning loans, and reading eventually consistent MongoDB projections."
            });

            foreach (var assembly in new[] { typeof(Program).Assembly, typeof(BookResult).Assembly, typeof(LoanStatus).Assembly })
            {
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            }
        });
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddHealthChecks();
        builder.Services.AddLibraryInfrastructure(builder.Configuration);
        builder.Services.AddScoped<CreateBookHandler>();
        builder.Services.AddScoped<CreateLoanHandler>();
        builder.Services.AddScoped<ReturnLoanHandler>();
        builder.Services.AddScoped<GetBookHandler>();
        builder.Services.AddScoped<ListBooksHandler>();
        builder.Services.AddScoped<GetLoanHandler>();
        builder.Services.AddScoped<ListLoansHandler>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseExceptionHandler();

        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier;

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            
            using (app.Logger.BeginScope(new Dictionary<string, object> 
            { 
                ["CorrelationId"] = correlationId 
            })) 
            
            await next();
        });
        app.MapControllers();
        app.MapHealthChecks("/health");

        await ApplyMigrationsAsync(app.Services, app.Logger, app.Lifetime.ApplicationStopping);

        app.Run();
    }

    #endregion

    #region Private Methods

    private static async Task ApplyMigrationsAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<LibraryDbContext>().Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < 10)
            {
                logger.LogWarning(exception, "Database migration attempt {Attempt} failed", attempt);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }

    #endregion
}
