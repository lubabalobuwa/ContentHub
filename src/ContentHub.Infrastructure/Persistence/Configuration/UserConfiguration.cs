using ContentHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Infrastructure.Persistence.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Role)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.IsDisabled)
                .IsRequired();

            builder.Property(x => x.EmailConfirmed)
                .IsRequired();

            builder.Property(x => x.EmailVerificationTokenHash)
                .HasMaxLength(256);

            builder.Property(x => x.EmailVerificationExpiresAtUtc)
                .IsRequired(false);

            builder.Property(x => x.PasswordResetTokenHash)
                .HasMaxLength(256);

            builder.Property(x => x.PasswordResetExpiresAtUtc)
                .IsRequired(false);

            builder.Property(x => x.PasswordResetUsedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.LastLoginAtUtc)
                .IsRequired(false);
        }
    }
}
