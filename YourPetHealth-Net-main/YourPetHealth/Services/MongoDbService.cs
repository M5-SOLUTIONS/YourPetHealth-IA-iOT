using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace YourPetHealth.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
           
            var connectionString = configuration["MongoDbSettings:ConnectionString"] ?? "mongodb://localhost:27017";
            var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "clyvo_vet_db";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}



