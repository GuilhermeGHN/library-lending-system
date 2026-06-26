using Library.Application.Interfaces.Outbox;
using Library.Application.Interfaces.Persistence;
using Library.Application.Interfaces.ReadStores;
using Library.Application.Interfaces.Repositories;
using Library.Infrastructure.Interfaces.Projections;
using Library.Infrastructure.Outbox;
using Library.Infrastructure.Persistence.Contexts;
using Library.Infrastructure.Persistence.Outbox;
using Library.Infrastructure.Persistence.Repositories;
using Library.Infrastructure.Projections;
using Library.Infrastructure.Projections.Handlers;
using Library.Infrastructure.Projections.ReadStores;
using Library.Infrastructure.Projections.Writers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Library.Infrastructure;

public static class DependencyInjection
{
    #region Public Methods

    public static IServiceCollection AddLibraryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var mysql = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("ConnectionStrings:MySql is required.");
        
        var mongo = configuration.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("ConnectionStrings:MongoDb is required.");

        var mongoDatabase = configuration["MongoDb:Database"]
            ?? "library_read";

        services.AddDbContext<LibraryDbContext>(options => options.UseMySQL(mysql));
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongo));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabase));
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LibraryDbContext>());
        services.AddScoped<IOutbox, EfOutbox>();
        services.AddScoped<IBookReadStore, MongoBookReadStore>();
        services.AddScoped<ILoanReadStore, MongoLoanReadStore>();
        services.AddScoped<IBookProjectionWriter, MongoBookProjectionWriter>();
        services.AddScoped<ILoanProjectionWriter, MongoLoanProjectionWriter>();
        services.AddScoped<IProjectionHandler, BookCreatedProjectionHandler>();
        services.AddScoped<IProjectionHandler, LoanCreatedProjectionHandler>();
        services.AddScoped<IProjectionHandler, LoanReturnedProjectionHandler>();
        services.AddScoped<IProjectionDispatcher, ProjectionDispatcher>();
        services.AddScoped<OutboxProcessor>();
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));

        return services;
    }

    #endregion
}
