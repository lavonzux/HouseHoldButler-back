using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HouseHoldButler.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultProductsAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AvgConsumptionRate", "Barcode", "CategoryId", "CreatedAt", "LowStockThreshold", "Name", "Unit", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c0000000-0000-0000-0000-000000000001"), 0.05m, null, new Guid("b0000000-0000-0000-0000-000000000007"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0.2m, "白米", "kg", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), 0.15m, null, new Guid("b0000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0.3m, "礦泉水", "瓶", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), 0.03m, null, new Guid("b0000000-0000-0000-0000-000000000021"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0.2m, "洗衣精", "瓶", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), 0.04m, null, new Guid("b0000000-0000-0000-0000-000000000031"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0.2m, "洗髮精", "瓶", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("c0000000-0000-0000-0000-000000000005"), 0.10m, null, new Guid("b0000000-0000-0000-0000-000000000041"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0.25m, "衛生紙", "包", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "常購" },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "有機" },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "進口" }
                });

            migrationBuilder.InsertData(
                table: "ProductTags",
                columns: new[] { "ProductId", "TagId" },
                values: new object[,]
                {
                    { new Guid("c0000000-0000-0000-0000-000000000001"), new Guid("d0000000-0000-0000-0000-000000000001") },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), new Guid("d0000000-0000-0000-0000-000000000001") },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), new Guid("d0000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), new Guid("d0000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000000-0000-0000-0000-000000000005"), new Guid("d0000000-0000-0000-0000-000000000001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductTags",
                keyColumns: new[] { "ProductId", "TagId" },
                keyValues: new object[] { new Guid("c0000000-0000-0000-0000-000000000001"), new Guid("d0000000-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "ProductTags",
                keyColumns: new[] { "ProductId", "TagId" },
                keyValues: new object[] { new Guid("c0000000-0000-0000-0000-000000000002"), new Guid("d0000000-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "ProductTags",
                keyColumns: new[] { "ProductId", "TagId" },
                keyValues: new object[] { new Guid("c0000000-0000-0000-0000-000000000002"), new Guid("d0000000-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "ProductTags",
                keyColumns: new[] { "ProductId", "TagId" },
                keyValues: new object[] { new Guid("c0000000-0000-0000-0000-000000000004"), new Guid("d0000000-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "ProductTags",
                keyColumns: new[] { "ProductId", "TagId" },
                keyValues: new object[] { new Guid("c0000000-0000-0000-0000-000000000005"), new Guid("d0000000-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000003"));
        }
    }
}
