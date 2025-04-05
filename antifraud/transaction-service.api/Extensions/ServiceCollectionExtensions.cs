using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using transaction_service.application.Services;
using transaction_service.infrastructure.Kafka;
using transaction_service.infrastructure.Mongo;

namespace transaction_service.api.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ITransactionService, TransactionService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ITransactionRepository, TransactionRepository>();

        return services;
    }

    public static IServiceCollection AddKafkaComponents(this IServiceCollection services)
    {
        services.AddSingleton<ITransactionProducer, TransactionProducer>();
        services.AddSingleton<ITransactionUpdateConsumer, TransactionUpdateConsumer>();

        return services;
    }

    public static IServiceCollection AddMongoConfiguration(this IServiceCollection services, MongoDBSettings mongoDbSettings)
    {
        ConventionRegistry.Register("EnumAsStringConvention", new ConventionPack { new EnumRepresentationConvention(BsonType.String) }, t => true);
        BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoDbSettings.ConnectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));

        return services;
    }


}
