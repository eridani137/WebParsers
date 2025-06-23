using ActivityChecker.IO;
using MongoDB.Driver;

namespace ActivityChecker.Services;

public class MongoContext
{
    public IMongoCollection<ViewResult> Results { get; }
    public MongoContext(IMongoClient client)
    {
        var db = client.GetDatabase("ActivityChecker");
        Results = db.GetCollection<ViewResult>("Results");
    }
}