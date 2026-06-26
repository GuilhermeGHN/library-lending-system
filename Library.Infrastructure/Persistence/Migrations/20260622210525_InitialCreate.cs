using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "book",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    author = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    publication_year = table.Column<int>(type: "int", nullable: false),
                    available_quantity = table.Column<int>(type: "int", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    event_type = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    payload = table.Column<string>(type: "json", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    attempts = table.Column<int>(type: "int", nullable: false),
                    last_attempt_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    last_error = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "loan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    book_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    loan_date = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    return_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan", x => x.id);
                    table.ForeignKey(
                        name: "FK_loan_book_book_id",
                        column: x => x.book_id,
                        principalTable: "book",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_loan_book_id",
                table: "loan",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_processed_at_attempts_occurred_at",
                table: "outbox_message",
                columns: ["processed_at", "attempts", "occurred_at"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "loan");
            migrationBuilder.DropTable(name: "outbox_message");
            migrationBuilder.DropTable(name: "book");
        }
    }
}
