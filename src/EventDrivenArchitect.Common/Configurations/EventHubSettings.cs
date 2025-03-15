namespace EventDrivenArchitect.Common.Configurations;

public class EventHubSettings
{
    public string ConnectionString { get; set; }
    public string EventHubName { get; set; }
    public string ConsumerGroup { get; set; }
}