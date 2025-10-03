using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Reliant.Infrastructure.Migrations
{
  /// <inheritdoc />
  public partial class InitialCreate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "Customers",
        columns: table => new
        {
          Id = table
            .Column<int>(type: "integer", nullable: false)
            .Annotation(
              "Npgsql:ValueGenerationStrategy",
              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
            ),
          Name = table.Column<string>(
            type: "character varying(200)",
            maxLength: 200,
            nullable: false
          ),
          Email = table.Column<string>(
            type: "character varying(256)",
            maxLength: 256,
            nullable: true
          ),
          Phone = table.Column<string>(type: "text", nullable: true),
          AddressLine1 = table.Column<string>(type: "text", nullable: true),
          City = table.Column<string>(type: "text", nullable: true),
          Postcode = table.Column<string>(type: "text", nullable: true),
          Segment = table.Column<string>(type: "text", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Customers", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "Products",
        columns: table => new
        {
          Id = table
            .Column<int>(type: "integer", nullable: false)
            .Annotation(
              "Npgsql:ValueGenerationStrategy",
              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
            ),
          Name = table.Column<string>(
            type: "character varying(200)",
            maxLength: 200,
            nullable: false
          ),
          ProductType = table.Column<string>(
            type: "character varying(100)",
            maxLength: 100,
            nullable: false
          ),
          BasePrice = table.Column<decimal>(
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false
          ),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Products", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "Roles",
        columns: table => new
        {
          Id = table
            .Column<int>(type: "integer", nullable: false)
            .Annotation(
              "Npgsql:ValueGenerationStrategy",
              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
            ),
          Name = table.Column<string>(
            type: "character varying(64)",
            maxLength: 64,
            nullable: false
          ),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Roles", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table
            .Column<int>(type: "integer", nullable: false)
            .Annotation(
              "Npgsql:ValueGenerationStrategy",
              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
            ),
          Email = table.Column<string>(
            type: "character varying(256)",
            maxLength: 256,
            nullable: false
          ),
          PasswordHash = table.Column<string>(type: "text", nullable: false),
          FullName = table.Column<string>(type: "text", nullable: true),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          IsActive = table.Column<bool>(type: "boolean", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "Quotes",
        columns: table => new
        {
          Id = table
            .Column<int>(type: "integer", nullable: false)
            .Annotation(
              "Npgsql:ValueGenerationStrategy",
              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
            ),
          CustomerId = table.Column<int>(type: "integer", nullable: false),
          Status = table.Column<string>(
            type: "character varying(50)",
            maxLength: 50,
            nullable: false
          ),
          Subtotal = table.Column<decimal>(
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false
          ),
          Vat = table.Column<decimal>(
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false
          ),
          Total = table.Column<decimal>(
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false
          ),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
          Notes = table.Column<string>(type: "text", nullable: true),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Quotes", x => x.Id);
          table.ForeignKey(
            name: "FK_Quotes_Customers_CustomerId",
            column: x => x.CustomerId,
            principalTable: "Customers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_Quotes_Users_CreatedByUserId",
            column: x => x.CreatedByUserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "UserRoles",
        columns: table => new
        {
          UserId = table.Column<int>(type: "integer", nullable: false),
          RoleId = table.Column<int>(type: "integer", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
          table.ForeignKey(
            name: "FK_UserRoles_Roles_RoleId",
            column: x => x.RoleId,
            principalTable: "Roles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_UserRoles_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "QuoteItems",
        columns: table => new
        {
          Id = table
            .Column<int>(type: "integer", nullable: false)
            .Annotation(
              "Npgsql:ValueGenerationStrategy",
              NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
            ),
          QuoteId = table.Column<int>(type: "integer", nullable: false),
          ProductId = table.Column<int>(type: "integer", nullable: false),
          WidthMm = table.Column<int>(type: "integer", nullable: false),
          HeightMm = table.Column<int>(type: "integer", nullable: false),
          Material = table.Column<string>(
            type: "character varying(50)",
            maxLength: 50,
            nullable: false
          ),
          Glazing = table.Column<string>(
            type: "character varying(50)",
            maxLength: 50,
            nullable: false
          ),
          ColorTier = table.Column<string>(
            type: "character varying(50)",
            maxLength: 50,
            nullable: true
          ),
          HardwareTier = table.Column<string>(
            type: "character varying(50)",
            maxLength: 50,
            nullable: true
          ),
          InstallComplexity = table.Column<string>(
            type: "character varying(50)",
            maxLength: 50,
            nullable: true
          ),
          Qty = table.Column<int>(type: "integer", nullable: false),
          UnitPrice = table.Column<decimal>(
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false
          ),
          LineTotal = table.Column<decimal>(
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false
          ),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_QuoteItems", x => x.Id);
          table.ForeignKey(
            name: "FK_QuoteItems_Products_ProductId",
            column: x => x.ProductId,
            principalTable: "Products",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_QuoteItems_Quotes_QuoteId",
            column: x => x.QuoteId,
            principalTable: "Quotes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(name: "IX_Customers_Email", table: "Customers", column: "Email");

      migrationBuilder.CreateIndex(
        name: "IX_QuoteItems_ProductId",
        table: "QuoteItems",
        column: "ProductId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_QuoteItems_QuoteId",
        table: "QuoteItems",
        column: "QuoteId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Quotes_CreatedByUserId",
        table: "Quotes",
        column: "CreatedByUserId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Quotes_CustomerId",
        table: "Quotes",
        column: "CustomerId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Roles_Name",
        table: "Roles",
        column: "Name",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_UserRoles_RoleId",
        table: "UserRoles",
        column: "RoleId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Users_Email",
        table: "Users",
        column: "Email",
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "QuoteItems");

      migrationBuilder.DropTable(name: "UserRoles");

      migrationBuilder.DropTable(name: "Products");

      migrationBuilder.DropTable(name: "Quotes");

      migrationBuilder.DropTable(name: "Roles");

      migrationBuilder.DropTable(name: "Customers");

      migrationBuilder.DropTable(name: "Users");
    }
  }
}
