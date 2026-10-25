using System.Net;
using Azure.Messaging.ServiceBus;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using ContentHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Messaging;

public sealed class ContentPublishedEmailWorker : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NewsletterSettings _newsletterSettings;
    private readonly ILogger<ContentPublishedEmailWorker> _logger;

    public ContentPublishedEmailWorker(
        ServiceBusClient client,
        IOptions<ServiceBusSettings> serviceBusOptions,
        IOptions<NewsletterSettings> newsletterOptions,
        IServiceScopeFactory scopeFactory,
        ILogger<ContentPublishedEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _newsletterSettings = newsletterOptions.Value;
        _logger = logger;
        _processor = client.CreateProcessor(serviceBusOptions.Value.QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await _processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _processor.DisposeAsync();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        if (!Guid.TryParse(args.Message.Body.ToString(), out var contentId))
        {
            await args.DeadLetterMessageAsync(args.Message, "InvalidContentId", cancellationToken: args.CancellationToken);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var content = await dbContext.ContentItems
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == contentId && x.Status == ContentStatus.Published, args.CancellationToken);

        if (content is null)
        {
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            return;
        }

        var subscribers = await dbContext.NewsletterSubscriptions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => x.Email)
            .ToListAsync(args.CancellationToken);

        var articleUrl = $"{_newsletterSettings.FrontendBaseUrl.TrimEnd('/')}/{content.Id}";
        var encodedTitle = WebUtility.HtmlEncode(content.Title);
        var encodedUrl = WebUtility.HtmlEncode(articleUrl);
        var subject = $"New on TechContentHub: {content.Title}";
        var body = $"""
            <h1>{encodedTitle}</h1>
            <p>A new article has just been published on TechContentHub.</p>
            <p><a href="{encodedUrl}">Read the article</a></p>
            <p style="color:#667085;font-size:12px">You are receiving this because you subscribed to TechContentHub updates.</p>
            """;

        foreach (var email in subscribers)
        {
            try
            {
                await emailSender.SendAsync(email, subject, body);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to send published-content alert. ContentId={ContentId} Email={Email}", contentId, email);
            }
        }

        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        _logger.LogInformation("Processed published-content alert. ContentId={ContentId} Subscribers={SubscriberCount}", contentId, subscribers.Count);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus newsletter worker failed. Entity={EntityPath} Source={ErrorSource}", args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }
}
