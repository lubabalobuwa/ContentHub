using ContentHub.Domain.Common;

namespace ContentHub.Domain.Newsletter;

public sealed class NewsletterSubscription : Entity
{
    private NewsletterSubscription() { }

    public NewsletterSubscription(string email, DateTime subscribedAtUtc)
    {
        Email = email.Trim();
        NormalizedEmail = Email.ToUpperInvariant();
        SubscribedAtUtc = subscribedAtUtc;
        IsActive = true;
    }

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public DateTime SubscribedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public void Reactivate(string email, DateTime subscribedAtUtc)
    {
        Email = email.Trim();
        NormalizedEmail = Email.ToUpperInvariant();
        SubscribedAtUtc = subscribedAtUtc;
        IsActive = true;
    }
}
