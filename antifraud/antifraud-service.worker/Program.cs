using antifraud_service.infrastructure.Mongo;
using antifraud_service.worker;
using antifraud_service.worker.Extensions;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEnvironmentConfiguration();

builder.Services.AddMongoConfiguration(builder.Services.BuildServiceProvider().GetRequiredService<IOptions<MongoDBSettings>>().Value);

builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddKafkaComponents();
builder.Services.AddHostedService<AntiFraudWorker>();


builder.Services.AddLogging(configure => configure.AddConsole());

var host = builder.Build();




host.Run();
