using EMS.Models;
using MongoDB.Driver;

namespace EMS.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }
    public IMongoCollection<Employee> Employees =>
    _database.GetCollection<Employee>("Employee");

    public IMongoCollection<Product> Products =>
    _database.GetCollection<Product>("Products");

    public IMongoCollection<Category> Categories =>
    _database.GetCollection<Category>("Categories");

    public IMongoCollection<User> Users =>
    _database.GetCollection<User>("Users");

    public IMongoCollection<Cart> Carts =>
    _database.GetCollection<Cart>("Carts");

    public IMongoCollection<Order> Orders =>
    _database.GetCollection<Order>("Orders");

        public IMongoDatabase Database => _database;
    }
}