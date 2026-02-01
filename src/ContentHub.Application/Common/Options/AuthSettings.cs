namespace ContentHub.Application.Common.Options
{
    public class AuthSettings
    {
        public bool RequireEmailVerification { get; set; } = true;
        public int EmailVerificationTokenHours { get; set; } = 24;
        public int PasswordResetTokenHours { get; set; } = 1;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
