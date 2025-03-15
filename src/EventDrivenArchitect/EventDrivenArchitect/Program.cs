using EventDrivenArchitect;
using EventDrivenArchitect.Configurations;
using System.Runtime;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();


builder.Services.Configure<EventHubSettings>(builder.Configuration.GetSection("EventHubSettings"));

var host = builder.Build();
host.Run();