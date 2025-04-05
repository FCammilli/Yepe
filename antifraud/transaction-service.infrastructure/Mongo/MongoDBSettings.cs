namespace transaction_service.infrastructure.Mongo
{
    public class MongoDBSettings
    {
        public required string DatabaseName { get; set; }
        public required string ConnectionString { get; set; }
    }
}

