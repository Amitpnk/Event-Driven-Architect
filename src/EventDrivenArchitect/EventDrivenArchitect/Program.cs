using EventDrivenArchitect;
using EventDrivenArchitect.Configurations;
using EventDrivenArchitect.Services;
using Polly;
using System.Runtime;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<EventHubConsumerService>();
builder.Services.Configure<EventHubSettings>(builder.Configuration.GetSection("EventHubSettings"));

builder.Services.Configure<ServiceBusSettings>(builder.Configuration.GetSection("ServiceBusSettings"));
builder.Services.AddSingleton<ServiceBusProducerService>();

var host = builder.Build();
host.Run();