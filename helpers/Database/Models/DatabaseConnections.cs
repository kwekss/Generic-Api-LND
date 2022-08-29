namespace helpers.Database.Models
{
    public class DatabaseConnections
    {
        public Connection Default { get; set; }
        public Connection SMS { get; set; }
        public Connection CaaS { get; set; }
        public MongoConnection Mongo { get; set; }
    }

    public class Connection
    {
        public string Schema { get; set; }
        public string ConnectionString { get; set; }
    }

    public class MongoConnection
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }
}
