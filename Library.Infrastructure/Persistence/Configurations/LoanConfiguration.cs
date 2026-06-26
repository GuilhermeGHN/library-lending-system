using Library.Domain.Books;
using Library.Domain.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loan");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.BookId).HasColumnName("book_id");
        builder.Property(x => x.LoanDate).HasColumnName("loan_date");
        builder.Property(x => x.ReturnDate).HasColumnName("return_date");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(x => x.BookId);
        builder.HasOne<Book>().WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
    }

    #endregion
}
