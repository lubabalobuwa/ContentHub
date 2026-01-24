using ContentHub.Worker;
using ContentHub.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ContentPublishedConsumer>();

var host = builder.Build();
host.Run();
