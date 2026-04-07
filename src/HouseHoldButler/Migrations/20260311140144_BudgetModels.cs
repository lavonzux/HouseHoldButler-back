using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseHoldButler.Migrations
{
    /// <inheritdoc />
    public partial class BudgetModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    YearMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    AlertLevel = table.Column<string>(type: "text", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    LastNotifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetAlerts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BudgetCategoryLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    YearMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategoryLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetCategoryLimits_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetMonthlyTotals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetMonthlyTotals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenditures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExpenditureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    InventoryEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenditures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenditures_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenditures_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenditures_InventoryEvents_InventoryEventId",
                        column: x => x.InventoryEventId,
                        principalTable: "InventoryEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenditures_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAlerts_CategoryId_YearMonth",
                table: "BudgetAlerts",
                columns: new[] { "CategoryId", "YearMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAlerts_YearMonth",
                table: "BudgetAlerts",
                column: "YearMonth");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategoryLimits_CategoryId_YearMonth",
                table: "BudgetCategoryLimits",
                columns: new[] { "CategoryId", "YearMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetMonthlyTotals_YearMonth",
                table: "BudgetMonthlyTotals",
                column: "YearMonth",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenditures_CategoryId",
                table: "Expenditures",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenditures_ExpenditureDate",
                table: "Expenditures",
                column: "ExpenditureDate");

            migrationBuilder.CreateIndex(
                name: "IX_Expenditures_InventoryEventId",
                table: "Expenditures",
                column: "InventoryEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenditures_InventoryId",
                table: "Expenditures",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenditures_ProductId",
                table: "Expenditures",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetAlerts");

            migrationBuilder.DropTable(
                name: "BudgetCategoryLimits");

            migrationBuilder.DropTable(
                name: "BudgetMonthlyTotals");

            migrationBuilder.DropTable(
                name: "Expenditures");
        }
    }
}
