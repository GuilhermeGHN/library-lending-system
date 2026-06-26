using Library.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_message");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(200).IsRequired();
        builder.Property(x => x.AggregateId).HasColumnName("aggregate_id");
        builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("json").IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.Property(x => x.Attempts).HasColumnName("attempts");
        builder.Property(x => x.LastAttemptAt).HasColumnName("last_attempt_at");
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
        builder.HasIndex(x => new { x.ProcessedAt, x.Attempts, x.OccurredAt });
    }

    #endregion
}
