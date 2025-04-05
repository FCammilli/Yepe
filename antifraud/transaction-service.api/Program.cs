using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json.Serialization;
using transaction_service.api.Extensions;
using transaction_service.application.Services;
using transaction_service.infrastructure.Kafka;
using transaction_service.infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

builder.Services.AddMongoConfiguration(builder.Services.BuildServiceProvider().GetRequiredService<IOptions<MongoDBSettings>>().Value);

builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddKafkaComponents();

//builder.Services.AddHostedService<TransactionWorker>();

builder.Services.AddLogging(configure => configure.AddConsole());

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
Task.Run(async () =>
{
    var cancellationToken = new CancellationTokenSource();
    var transactionUpdateConsumer = app.Services.GetRequiredService<ITransactionUpdateConsumer>();
    await transactionUpdateConsumer.StartListeningAsync(cancellationToken.Token);
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
