using EventDrivenArchitect;
using EventDrivenArchitect.Services;
using Polly;
using System.Runtime;
using EventDrivenArchitect.Common.Configurations;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<EventHubConsumerService>();
builder.Services.Configure<EventHubSettings>(builder.Configuration.GetSection("EventHubSettings"));

builder.Services.Configure<ServiceBusSettings>(builder.Configuration.GetSection("ServiceBusSettings"));
builder.Services.AddSingleton<ServiceBusProducerService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));



var host = builder.Build();
host.Run();