using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace FileUploadAPI.Data
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;
        public GridFSBucket GridFS { get; }

        public MongoDBContext()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("FileUploadDB");
            GridFS = new GridFSBucket(_database);
        }

        public IMongoCollection<BsonDocument> GetCollection(string name)
        {
            return _database.GetCollection<BsonDocument>(name);
        }
    }
}
