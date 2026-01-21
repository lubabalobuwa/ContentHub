using ContentHub.Api.Endpoints;
using ContentHub.Api.Middleware;
using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateContentHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHealthChecks("/health");
app.MapContentEndpoints();

app.Run();
