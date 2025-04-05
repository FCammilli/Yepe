using antifraud_service.application.Repositories;
using antifraud_service.application.Services;
using antifraud_service.infrastructure.Kafka;
using antifraud_service.infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace antifraud_service.worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddEnvironmentConfiguration(this HostApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDbSettings"));
        builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

        return builder;
    }
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IAntiFraudService, AntiFraudService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDailyTransactionSummaryRepository, DailyTransactionSummaryRepository>();

        return services;
    }

    public static IServiceCollection AddKafkaComponents(this IServiceCollection services)
    {
        services.AddSingleton<IAntifraudTransactionProducer, TransactionAntiFraudProducer>();
        services.AddScoped<IAntifraudTransactionCreatedConsumer, TransactionCreatedConsumer>();

        return services;
    }

    public static IServiceCollection AddMongoConfiguration(this IServiceCollection services, MongoDBSettings mongoDbSettings)
    {
        ConventionRegistry.Register("EnumAsStringConvention", new ConventionPack { new EnumRepresentationConvention(BsonType.String) }, t => true);
        BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoDbSettings.ConnectionString));
        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));

        return services;
    }


}
