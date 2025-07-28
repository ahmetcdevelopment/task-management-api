namespace TaskManagement.API.Configurations;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int MaxConnectionPoolSize { get; set; } = 100;
    public int MinConnectionPoolSize { get; set; } = 0;
    public TimeSpan MaxConnectionIdleTime { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan MaxConnectionLifeTime { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan SocketTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool RetryWrites { get; set; } = true;
    public string ReadPreference { get; set; } = "Primary";
    public string WriteConcern { get; set; } = "Majority";
    
    // Collection Names
    public string UsersCollection { get; set; } = "users";
    public string ProjectsCollection { get; set; } = "projects";
    public string WorkItemsCollection { get; set; } = "workitems";
    public string WorkItemLogsCollection { get; set; } = "workitemlogs";
    public string NotificationsCollection { get; set; } = "notifications";
}