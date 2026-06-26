using Library.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("book");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Author).HasColumnName("author").HasMaxLength(200).IsRequired();
        builder.Property(x => x.PublicationYear).HasColumnName("publication_year");
        builder.Property(x => x.AvailableQuantity).HasColumnName("available_quantity");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
    }

    #endregion
}
