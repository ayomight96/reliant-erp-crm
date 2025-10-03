using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reliant.Infrastructure.Migrations
{
  /// <inheritdoc />
  public partial class AddAiFlagsToQuoteItem : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<double>(
        name: "AiConfidence",
        table: "QuoteItems",
        type: "double precision",
        nullable: true
      );

      migrationBuilder.AddColumn<bool>(
        name: "IsAiPriced",
        table: "QuoteItems",
        type: "boolean",
        nullable: false,
        defaultValue: false
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "AiConfidence", table: "QuoteItems");

      migrationBuilder.DropColumn(name: "IsAiPriced", table: "QuoteItems");
    }
  }
}
